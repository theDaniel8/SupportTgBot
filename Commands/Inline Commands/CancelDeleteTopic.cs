using System.Runtime.CompilerServices;
using Microsoft.Data.Sqlite;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
public class CancelDeleteTopic : InlineCommand
{
    public CancelDeleteTopic(TelegramBotClient bot, DatabaseService db) : base(bot, db)
    {
    }

    public override string CallbackData => "cancelDeleteTopic";

    public override async Task Execute(CallbackQuery query)
    {
        await _bot.AnswerCallbackQuery(query.Id);
        if (query.Message == null) return;
        await _bot.EditMessageReplyMarkup(query.Message.Chat.Id, query.Message.MessageId, replyMarkup: Handler.createTopicMarkup);
    }
}