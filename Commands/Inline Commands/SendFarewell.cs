using System.Runtime.CompilerServices;
using Microsoft.Data.Sqlite;
using SQLitePCL;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
public class SendFarewell : InlineCommand
{
    private readonly LogService _log;
    public SendFarewell(TelegramBotClient bot, DatabaseService db, LogService log) : base(bot, db)
    {
        _log = log;
    }

    public override string CallbackData => "sendFarewell";
    
    public override async Task Execute(CallbackQuery query) 
    {
        await _bot.AnswerCallbackQuery(query.Id);
        Admin? admin = _db.GetAdmin(query.From.Id);
        
        if (query.Message == null || query.Message.MessageThreadId == null || admin == null) return;
        
        long? targetId = _db.GetUserIdByTopicId(query.Message.MessageThreadId);
        if (targetId == null) return;

        if (string.IsNullOrEmpty(admin.Farewell))
        {
            await _bot.SendMessage(query.Message.Chat.Id, $"❌ Чтобы отправлять прощание, вы должны сначала его установить.", messageThreadId: query.Message.MessageThreadId);
            return;
        }
        else
        {
            await _bot.SendMessage(targetId.Value, admin.Farewell);
            await _bot.SendMessage(query.Message.Chat.Id, $"🫂 Ваше прощание было отправлено пользователю.", messageThreadId: query.Message.MessageThreadId);   
            await _log.MessageFromAdmin(targetId.Value, admin.Farewell, query.From.FirstName);
        }
    }
}