using System.Runtime.CompilerServices;
using Microsoft.Data.Sqlite;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
public class CheckSubscription : InlineCommand
{
    public CheckSubscription (TelegramBotClient bot, DatabaseService db) : base(bot, db)
    {
    }

    public override string CallbackData => "checkSubscription";

    public override async Task Execute(CallbackQuery query)
    {
        if (await Handler.IsSubscribedToChannel(query.From.Id, _bot))
        {
            await _bot.AnswerCallbackQuery(query.Id, "✅ Подписка подтверждена! Можете пользоваться ботом.");
            if (query.Message != null)
                await _bot.DeleteMessage(query.Message.Chat.Id, query.Message.MessageId);
        }
        else
        {
            await _bot.AnswerCallbackQuery(query.Id, "❌ Вы ещё не подписаны на канал.", showAlert: true);
        }
        return;
    }
}