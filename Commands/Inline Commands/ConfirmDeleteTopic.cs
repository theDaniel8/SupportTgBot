using System.Runtime.CompilerServices;
using Microsoft.Data.Sqlite;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
public class ConfirmDeleteTopic : InlineCommand
{
    public ConfirmDeleteTopic(TelegramBotClient bot, DatabaseService db) : base(bot, db)
    {
    }

    public override string CallbackData => "confirmDeleteTopic";

    public override async Task Execute(CallbackQuery query)
    {
        await _bot.AnswerCallbackQuery(query.Id);
        if (query.Message == null || query.Message.MessageThreadId == null) return;

        int threadId = query.Message.MessageThreadId.Value;
        await _bot.DeleteForumTopic(query.Message.Chat.Id, threadId);
        _db.DeleteDialogueByTopicId(threadId);
    }
}