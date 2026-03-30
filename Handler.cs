using System.Data;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualBasic;
using SQLitePCL;
using Telegram.Bot;
using Telegram.Bot.Extensions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public class Handler
{
    private readonly TelegramBotClient _bot;
    private readonly DatabaseService _db;
    private readonly LogService _log;
    private readonly List<DialogueService> _dialogue;
    private readonly List<SlashCommand> _slashCommands;
    private readonly List<InlineCommand> _inlineCommands;
    

    public Handler(TelegramBotClient bot, DatabaseService db, LogService log)
    {
        _bot = bot;
        _db = db;
        _log = log;
        
        
        _slashCommands = new List<SlashCommand>
        {
            new Start(bot, db),
            new Tag(bot, db),
            new Greeting(bot, db),
            new Farewell(bot, db),
            new ChatInfo(bot, db),
            new Ban(bot, db),
        };

        _inlineCommands = new List<InlineCommand>
        {
            new DeleteTopic(bot, db),
            new ConfirmDeleteTopic(bot, db),
            new CancelDeleteTopic(bot, db),
            new SendGreeting(bot, db, log),
            new SendFarewell(bot, db, log),
            new CheckSubscription(bot, db),
            new RenameTopic(bot, db),
        };

        _dialogue = new List<DialogueService>
        {
            new CreateDialogue(bot,db, log),
            new ForwardToAdmin(bot,db, log),
            new ForwardToUser(bot,db, log),
        };

        /* _replyCommands = new List<ReplyCommand>
        {
        }; */
    }
    
    public async Task OnError(Exception exception, HandleErrorSource source)
    {
        Console.WriteLine(exception); 
        await Task.CompletedTask;
    }

    public async Task OnMessage(Message msg, UpdateType type)
    {
        if (msg.From == null) return;
        _db.UpsertUser(msg.From.Id, msg.From.Username, $"{msg.From.FirstName} {msg.From.LastName}".Trim());
        
        if (msg.Chat.Type == ChatType.Private && !await IsSubscribedToChannel(msg.From.Id, _bot))
        {
            await SendSubscriptionRequired(msg.Chat.Id);
            return;
        }

        foreach (SlashCommand command in _slashCommands)
        {
            if (msg.Text != null && msg.Text.StartsWith(command.Name))
            {
                await command.Execute(msg);
                return;
            }
        }   

        foreach (DialogueService dialogue in _dialogue)
        {
            if (dialogue.Сondition(msg))
            {
                await dialogue.Execute(msg);
                return;
            }
        }
    }

    public async Task OnUpdate(Update update)
    {
        if (update is { CallbackQuery: { } query }) 
        {
            BotUser? botUser = _db.GetBotUser(query.From.Id);
            if (botUser == null) return;

            // Проверка подписки для inline-команд (пропускаем checkSubscription, чтобы он мог обработаться)
            if (query.Data != "checkSubscription"
                && query.Message?.Chat.Type == ChatType.Private
                && !await IsSubscribedToChannel(query.From.Id, _bot))
            {
                await SendSubscriptionRequired(query.Message.Chat.Id);
                await _bot.AnswerCallbackQuery(query.Id);
                return;
            }

            foreach (InlineCommand command in _inlineCommands)
            {
                if (query.Data != null && query.Data.StartsWith(command.CallbackData))
                {
                    await command.Execute(query);
                    return;
                }
            }
        }
    }

    public static async Task<bool> IsSubscribedToChannel(long userId, TelegramBotClient bot)
    {
        try
        {
            ChatMember member = await bot.GetChatMember(Settings.RequiredChannelId, userId);
            return member.Status is not (ChatMemberStatus.Left or ChatMemberStatus.Kicked);
        }
        catch
        {
            // Если бот не администратор канала или канал не найден — пропускаем проверку
            return true;
        }
    }

    private async Task SendSubscriptionRequired(long chatId)
    {
        await _bot.SendMessage(chatId,"⚠️ Для использования бота необходимо подписаться на наш канал.",
            replyMarkup: new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithUrl("📢 Подписаться на канал", Settings.RequiredChannelUrl) },
                new[] { InlineKeyboardButton.WithCallbackData("✅ Я подписался", "checkSubscription") }
            }));
    }

    public static InlineKeyboardMarkup createTopicMarkup = new InlineKeyboardMarkup(new[] 
    { 
        new InlineKeyboardButton[] { new InlineKeyboardButton("👋 Отправить приветствие", $"sendGreeting") },
        new InlineKeyboardButton[] { new InlineKeyboardButton("🫂 Отправить прощание", $"sendFarewell")  },
        new InlineKeyboardButton[] { new InlineKeyboardButton("❌ Удалить чат", $"deleteTopic") },
        new InlineKeyboardButton[] { new InlineKeyboardButton("✏️ Переименовать чат", $"renameTopic") },
    });

    private async Task MainMenu(long id)
    {
        await _bot.SendPhoto(id, "", "<b>👋 Добро пожаловать в службу поддержки!</b>\n\nВыберите действие:", parseMode: ParseMode.Html, 
        replyMarkup: new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("💬 Начать общение", $"sendGreeting") },
            new[] { InlineKeyboardButton.WithCallbackData("📕 Правилами", $"sendFarewell")  },
        }));
    }

}