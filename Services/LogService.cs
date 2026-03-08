using System.Web;
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
        var safeName = HttpUtility.HtmlEncode(adminName);
        var safeUserName = HttpUtility.HtmlEncode(_db.GetBotUser(targetId)?.Name);
        var safeText = HttpUtility.HtmlEncode(text);
        await _bot.SendMessage(Settings.GroupId, $"From: <b>{safeName}</b>\nTo: <b>{safeUserName}</b>\n\n{safeText}", 
        messageThreadId: Settings.LogThreadId, parseMode: ParseMode.Html);
    }

    public async Task MessageFromUser(Message msg)
    {
        await _bot.ForwardMessage(Settings.GroupId, msg.Chat.Id, msg.MessageId, messageThreadId: Settings.LogThreadId);
    }
}