using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public class DialogueService
{
    private readonly TelegramBotClient _bot;
    private readonly DatabaseService _db;
    private readonly LogService _log;

    public DialogueService(TelegramBotClient bot, DatabaseService db, LogService log)
    {
        _bot = bot;
        _db = db;
        _log = log;
    }

    /// Создаёт новый диалог (топик) для пользователя, если он ещё не в диалоге.
    /// Возвращает true, если диалог был создан.
    public async Task<bool> TryCreateDialogue(Message msg, BotUser botUser)
    {
        if (msg.Chat.Type != ChatType.Private || msg.From == null || _db.IsInDialogue(msg.From.Id) || botUser.Ban)
            return false;

        ForumTopic topic = await _bot.CreateForumTopic(Settings.GroupId, "⌛ Ожидание");
        _db.InsertDialogue(topic.MessageThreadId, msg.From.Id);

        string username = msg.From.Username == null ? "Не установлен" : "@" + msg.From.Username;
        string caption = $"💬 Пользователь создал чат\n\n<b>Юзернейм:</b> {username}\n<b>Ник:</b> {msg.From.FirstName} {msg.From.LastName}\n<b>ID:</b> {msg.From.Id}";

        UserProfilePhotos photos = await _bot.GetUserProfilePhotos(msg.From.Id, limit: 1);
        
        if (photos.Photos.Length > 0)
        {
            PhotoSize photo = photos.Photos[0].Last();
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

        return true;
    }

    /// Пересылает сообщение пользователя в топик админу.
    /// Возвращает true, если сообщение было переслано.    
    public async Task<bool> TryForwardToAdmin(Message msg, BotUser botUser)
    {
        if (msg.Chat.Type != ChatType.Private || !_db.IsInDialogue(msg.From!.Id) || botUser.Ban)
            return false;

        int? topicId = _db.GetTopicIdByUserId(msg.From.Id);
        if (topicId == null) return false;

        await _bot.ForwardMessage(Settings.GroupId, msg.Chat.Id, msg.MessageId, messageThreadId: topicId.Value);
        await _log.MessageFromUser(msg);
        return true;
    }

    /// Пересылает ответ админа пользователю из топика.
    /// Возвращает true, если сообщение было переслано.
    public async Task<bool> TryForwardToUser(Message msg)
    {
        if (_db.GetUserIdByTopicId(msg.MessageThreadId) is not long userId || msg.Chat.Id != Settings.GroupId)
            return false;

        if (msg.Type == MessageType.Text && (string.IsNullOrEmpty(msg.Text) || msg.Text.StartsWith("//")))
            return true; // Комментарий — считаем обработанным, но не пересылаем

        string text;
        Admin? admin = _db.GetAdmin(msg.From!.Id);
        try
        {
            switch (msg.Type)
            {
                case MessageType.Text:
                    text = admin?.Tag != null ? $"{msg.Text} {admin.Tag}" : msg.Text! ;
                    await _bot.SendMessage(userId, text);
                    await _log.MessageFromAdmin(userId, text, msg.From.FirstName);
                    break;

                case MessageType.Sticker:
                    await _bot.SendSticker(userId, msg.Sticker!.FileId);
                    await _bot.ForwardMessage(Settings.GroupId, msg.Chat.Id, // Логироваине стикера от админа
                        msg.MessageId, messageThreadId: Settings.LogThreadId);
                    break;

                case MessageType.Photo:
                    text = admin?.Tag != null ? $"{msg.Text} {admin.Tag}" : msg.Caption ?? "";
                    await _bot.SendPhoto(userId, msg.Photo!.Last().FileId, caption: text );
                    await _bot.ForwardMessage(Settings.GroupId, msg.Chat.Id, // Логироваине фото от админа
                        msg.MessageId, messageThreadId: Settings.LogThreadId);
                    break;
                
                case MessageType.Video:
                    text = admin?.Tag != null ? $"{msg.Text} {admin.Tag}" : msg.Caption ?? "";
                    await _bot.SendVideo(userId, msg.Video!.FileId, caption: text);
                    await _bot.ForwardMessage(Settings.GroupId, msg.Chat.Id, // Логироваине видео от админа
                        msg.MessageId, messageThreadId: Settings.LogThreadId);
                    break;

                case MessageType.Document:
                    text = admin?.Tag != null ? $"{msg.Text} {admin.Tag}" : msg.Caption ?? "";
                    await _bot.SendDocument(userId, msg.Document!.FileId, caption: text);
                    await _bot.ForwardMessage(Settings.GroupId, msg.Chat.Id, // Логироваине документа от админа
                        msg.MessageId, messageThreadId: Settings.LogThreadId);
                    break;

                case MessageType.Voice:
                    text = admin?.Tag != null ? $"{msg.Text} {admin.Tag}" : msg.Caption ?? "";
                    await _bot.SendVoice(userId, msg.Voice!.FileId, caption: text);
                    await _bot.ForwardMessage(Settings.GroupId, msg.Chat.Id, // Логироваине голосового от админа
                        msg.MessageId, messageThreadId: Settings.LogThreadId);
                    break;

                case MessageType.Audio:
                    text = admin?.Tag != null ? $"{msg.Text} {admin.Tag}" : msg.Caption ?? "";
                    await _bot.SendAudio(userId, msg.Audio!.FileId, caption: text);
                    await _bot.ForwardMessage(Settings.GroupId, msg.Chat.Id, // Логироваине аудио от админа
                        msg.MessageId, messageThreadId: Settings.LogThreadId);
                    break;

                case MessageType.VideoNote:
                    await _bot.SendVideoNote(userId, msg.VideoNote!.FileId);
                    await _bot.ForwardMessage(Settings.GroupId, msg.Chat.Id, // Логироваине видеозаметки от админа
                        msg.MessageId, messageThreadId: Settings.LogThreadId);
                    break;

                case MessageType.Animation:
                    text = admin?.Tag != null ? $"{msg.Text} {admin.Tag}" : msg.Caption ?? "";
                    await _bot.SendAnimation(userId, msg.Animation!.FileId, caption: text);
                    await _bot.ForwardMessage(Settings.GroupId, msg.Chat.Id, // Логирование GIF от админа
                        msg.MessageId, messageThreadId: Settings.LogThreadId);
                    break;
                
                default:
                    await _bot.SendMessage(msg.Chat.Id, "❌ Данный вид сообщений не поддерживается.", 
                        messageThreadId: msg.MessageThreadId);
                    await _bot.ForwardMessage(Settings.GroupId, msg.Chat.Id, // Логирование неподдерживаемого сообщения от админа
                        msg.MessageId, messageThreadId: Settings.LogThreadId);
                    return false; // Не поддерживаем другие типы сообщений
            }
        }
        catch (Telegram.Bot.Exceptions.ApiRequestException ex) when (ex.Message.Contains("bot was blocked by the user"))
        {
            await _bot.SendMessage(msg.Chat.Id, "❌ Пользователь заблокировал бота.", 
                messageThreadId: msg.MessageThreadId);
            return false;
        }
        return true;
    }

    
}