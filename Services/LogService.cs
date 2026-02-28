using System.Runtime;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
public class LogService
{
    private readonly TelegramBotClient _bot;
    private readonly DatabaseService _db;

    public LogService(TelegramBotClient bot, DatabaseService db)
    {
        _bot = bot;
        _db = db;
    }

    public async Task MessageFromAdmin(long targetId, string text, string adminName)
    {
        await _bot.SendMessage(Settings.GroupId, $"From: <b>{adminName}</b>\nTo: <b>{_db.GetBotUser(targetId)?.Name}</b>\n\n{text}", 
        messageThreadId: Settings.LogThreadId, parseMode: ParseMode.Html);
    }

    public async Task MessageFromUser(Message msg)
    {
        await _bot.ForwardMessage(Settings.GroupId, msg.Chat.Id, msg.MessageId, messageThreadId: Settings.LogThreadId);
    }
}