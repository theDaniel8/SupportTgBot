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

    private readonly List<SlashCommand> _slashCommands;
    private readonly List<InlineCommand> _inlineCommands;
    private readonly List<ReplyCommand> _replyCommands;


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
        };

        _replyCommands = new List<ReplyCommand>
        {
        };
    }
    
    public async Task OnError(Exception exception, HandleErrorSource source)
    {
        Console.WriteLine(exception); 
    }

    public async Task OnMessage(Message msg, UpdateType type)
    {
        if (msg.From == null) return;
        _db.UpsertUser(msg.From.Id, msg.From.Username, $"{msg.From.FirstName} {msg.From.LastName}".Trim());
        BotUser? botUser = _db.GetBotUser(msg.From.Id);
        if (botUser == null) return;

        foreach (SlashCommand command in _slashCommands)
        {
            if (msg.Text != null && msg.Text.StartsWith(command.Name))
            {
                await command.Execute(msg);
                return;
            }
        }

        // Создание чата с пользователем
        if (msg.Chat.Type == ChatType.Private && !_db.IsInDialogue(msg.From.Id) && !botUser.Ban)
        {   
            ForumTopic topic = await _bot.CreateForumTopic(Settings.GroupId, msg.From.FirstName);
            _db.InsertDialogue(topic.MessageThreadId, msg.From.Id);

            string username = msg.From.Username == null ? "Не установлен" : "@" + msg.From.Username;
            UserProfilePhotos photos = await _bot.GetUserProfilePhotos(msg.From.Id, limit: 1);
            
            if (photos.Photos.Length > 0)
            {
                // photos.Photos[0] — массив PhotoSize[] (разные размеры одного фото), последний элемент — наибольшее разрешение
                PhotoSize photo = photos.Photos[0].Last();

                await _bot.SendPhoto(Settings.GroupId,
                    photo.FileId,
                    messageThreadId: topic.MessageThreadId,
                    caption: $"💬 Пользователь создал чат\n\n<b>Юзернейм:</b> {username}\n<b>Ник:</b> {msg.From.FirstName} {msg.From.LastName}\n<b>ID:</b> {msg.From.Id}",
                    parseMode: ParseMode.Html,
                    replyMarkup: createTopicMarkup);
            }
            else
            {
                await _bot.SendMessage(Settings.GroupId,
                    $"💬 Пользователь создал чат\n\n<b>Юзернейм:</b> {username}\n<b>Ник:</b> {msg.From.FirstName} {msg.From.LastName}\n<b>ID:</b> {msg.From.Id}",
                    messageThreadId: topic.MessageThreadId, parseMode: ParseMode.Html, replyMarkup: createTopicMarkup);
            }
        }

        // Пересылка сообщений пользователя админу
        if (msg.Chat.Type == ChatType.Private && _db.IsInDialogue(msg.From.Id) && !botUser.Ban)
        {
            await _bot.ForwardMessage(Settings.GroupId, msg.Chat.Id, msg.MessageId, messageThreadId: _db.GetTopicIdByUserId(msg.From.Id));
            await _log.MessageFromUser(msg);
            return;
        }

        // Пересылка сообщений админа пользователю
        if (_db.GetUserIdByTopicId(msg.MessageThreadId) is long userId && msg.Chat.Id == Settings.GroupId)
        {
            if (string.IsNullOrEmpty(msg.Text) || msg.Text.StartsWith("//")) return;
            Admin? admin = _db.GetAdmin(msg.From.Id);
            string text = admin?.Tag != null ? $"{msg.Text} {admin.Tag}" : msg.Text;
            await _bot.SendMessage(userId, text);
            await _log.MessageFromAdmin(userId, text, msg.From.FirstName);
        }
    }

    public async Task OnUpdate(Update update)
    {
        if (update is { CallbackQuery: { } query }) 
        {
            BotUser? botUser = _db.GetBotUser(query.From.Id);
            if (botUser == null) return;
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


    public static InlineKeyboardMarkup createTopicMarkup = new InlineKeyboardMarkup(new[] 
    { 
        new InlineKeyboardButton[] { new InlineKeyboardButton("👋 Отправить приветствие", $"sendGreeting") },
        new InlineKeyboardButton[] { new InlineKeyboardButton("🫂 Отправить прощание", $"sendFarewell")  },
        new InlineKeyboardButton[] { new InlineKeyboardButton("❌ Удалить чат", $"deleteTopic") },
    });

}