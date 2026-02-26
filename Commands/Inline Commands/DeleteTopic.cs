using System.Runtime.CompilerServices;
using Microsoft.Data.Sqlite;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
public class DeleteTopic : InlineCommand
{
    public DeleteTopic(TelegramBotClient bot, DatabaseService db) : base(bot, db)
    {
    }

    public override string CallbackData => "deleteTopic";

    public override async Task Execute(CallbackQuery query)
    {
        await _bot.AnswerCallbackQuery(query.Id);
        if (query.Message == null) return;
        await _bot.EditMessageReplyMarkup(query.Message.Chat.Id, query.Message.MessageId, replyMarkup: new InlineKeyboardMarkup(new[]
        {
            new InlineKeyboardButton[] { new InlineKeyboardButton("✅ Да, удалить", $"confirmDeleteTopic") },
            new InlineKeyboardButton[] { new InlineKeyboardButton("❌ Нет, оставить", $"cancelDeleteTopic") }
        }));

        
    }
}