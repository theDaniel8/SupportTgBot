using System.Linq.Expressions;
using System.Runtime.InteropServices.Marshalling;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Web;
public abstract class DialogueService
{
    protected readonly TelegramBotClient _bot;
    protected readonly DatabaseService _db;
    protected readonly LogService _log;
    public DialogueService (TelegramBotClient bot, DatabaseService db, LogService log)
    {
        _bot = bot;
        _db = db;
        _log = log;
    }
    
    public abstract bool Сondition(Message msg);
    public abstract Task Execute(Message msg);
    
    protected async Task CreateTopic(Message msg)
    {
        ForumTopic topic = await _bot.CreateForumTopic(Settings.GroupId, "⌛ Ожидание");
        if (!_db.IsInDialogue(msg.From!.Id))
            _db.InsertDialogue(topic.MessageThreadId, msg.From.Id);
        else
            _db.UpdateTopicId(topic.MessageThreadId, msg.From.Id);

        string username = msg.From.Username == null ? "Не установлен" : "@" + HttpUtility.HtmlEncode(msg.From.Username);
        string caption = $"💬 Пользователь создал чат\n\n<b>Юзернейм:</b> {username}\n<b>Ник:</b> {msg.From.FirstName} {msg.From.LastName}\n<b>ID:</b> {msg.From.Id}";

        UserProfilePhotos photos = await _bot.GetUserProfilePhotos(msg.From.Id, limit: 1);

        if (photos.Photos.Length > 0)
        {
            PhotoSize photo = photos.Photos[0].First();
            await _bot.SendPhoto(Settings.GroupId,
                photo.FileId,
                messageThreadId: topic.MessageThreadId,
                caption: caption,
                parseMode: ParseMode.Html,
                replyMarkup: Handler.createTopicMarkup);
        }
        else
        {
            await _bot.SendMessage(Settings.GroupId,
                caption,
                messageThreadId: topic.MessageThreadId,
                parseMode: ParseMode.Html,
                replyMarkup: Handler.createTopicMarkup);
        }
    }
    
}