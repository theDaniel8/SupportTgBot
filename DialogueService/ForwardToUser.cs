using Microsoft.Extensions.Configuration;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;

class ForwardToUser : DialogueService
{
    public ForwardToUser(TelegramBotClient bot, DatabaseService db, LogService log) : base(bot, db, log)
    {
    }
    long _userId;
    public override bool Сondition(Message msg)
    {
        // 1. Сначала базовые фильтры (комментарии и служебные сообщения)
        if (msg.Text?.StartsWith("//") == true || msg.Caption?.StartsWith("//") == true) return false;

        if (msg.Type is MessageType.ForumTopicCreated or MessageType.ForumTopicEdited
            or MessageType.ForumTopicClosed or MessageType.ForumTopicReopened) return false;

        // 2. Проверяем, что сообщение пришло из нужной группы
        if (msg.Chat.Id != Settings.GroupId) return false;

        // 3. Пытаемся получить userId из базы по ID топика
        var userIdFromDb = _db.GetUserIdByTopicId(msg.MessageThreadId);

        if (userIdFromDb is long userId)
        {
            _userId = userId; // Сохраняем в поле класса для метода Execute
            return true;
        }

        return false;
    }
    
    public override async Task Execute(Message msg)
    {   
        
        
        Admin? admin = _db.GetAdmin(msg.From!.Id);

        string? text = msg.Text ?? msg.Caption;
        if (text != null)
        {
            if (text.StartsWith("##")) 
            {
                text = text.Split("##")[1];
                admin!.Tag = null;
            }
            text = admin?.Tag != null ? $"{text} {admin.Tag}" : text;
        }
        else text = admin!.Tag;
        
        try
        {
            switch (msg.Type)
            {
                case MessageType.Text:
                    await _bot.SendMessage(_userId, text!);
                    await _log.MessageFromAdmin(_userId, text!, msg.From.FirstName);
                    break;

                case MessageType.Sticker:
                    await _bot.SendSticker(_userId, msg.Sticker!.FileId);
                    await _bot.ForwardMessage(Settings.GroupId, msg.Chat.Id, // Логироваине стикера от админа
                        msg.MessageId, messageThreadId: Settings.LogThreadId);
                    break;

                case MessageType.Photo:
                    await _bot.SendPhoto(_userId, msg.Photo!.Last().FileId, caption: text);
                    await _bot.ForwardMessage(Settings.GroupId, msg.Chat.Id, // Логироваине фото от админа
                        msg.MessageId, messageThreadId: Settings.LogThreadId);
                    break;

                case MessageType.Video:
                    await _bot.SendVideo(_userId, msg.Video!.FileId, caption: text);
                    await _bot.ForwardMessage(Settings.GroupId, msg.Chat.Id, // Логироваине видео от админа
                        msg.MessageId, messageThreadId: Settings.LogThreadId);
                    break;

                case MessageType.Document:
                    await _bot.SendDocument(_userId, msg.Document!.FileId, caption: text);
                    await _bot.ForwardMessage(Settings.GroupId, msg.Chat.Id, // Логироваине документа от админа
                        msg.MessageId, messageThreadId: Settings.LogThreadId);
                    break;

                case MessageType.Voice:
                    await _bot.SendVoice(_userId, msg.Voice!.FileId, caption: text);
                    await _bot.ForwardMessage(Settings.GroupId, msg.Chat.Id, // Логироваине голосового от админа
                        msg.MessageId, messageThreadId: Settings.LogThreadId);
                    break;

                case MessageType.Audio:
                    await _bot.SendAudio(_userId, msg.Audio!.FileId, caption: text);
                    await _bot.ForwardMessage(Settings.GroupId, msg.Chat.Id, // Логироваине аудио от админа
                        msg.MessageId, messageThreadId: Settings.LogThreadId);
                    break;

                case MessageType.VideoNote:
                    await _bot.SendVideoNote(_userId, msg.VideoNote!.FileId);
                    await _bot.ForwardMessage(Settings.GroupId, msg.Chat.Id, // Логироваине видеозаметки от админа
                        msg.MessageId, messageThreadId: Settings.LogThreadId);
                    break;

                case MessageType.Animation:
                    await _bot.SendAnimation(_userId, msg.Animation!.FileId, caption: text);
                    await _bot.ForwardMessage(Settings.GroupId, msg.Chat.Id, // Логирование GIF от админа
                        msg.MessageId, messageThreadId: Settings.LogThreadId);
                    break;

                default:
                    await _bot.SendMessage(msg.Chat.Id, "❌ Данный вид сообщений не поддерживается.",
                        messageThreadId: msg.MessageThreadId);
                    await _bot.ForwardMessage(Settings.GroupId, msg.Chat.Id, // Логирование неподдерживаемого сообщения от админа
                        msg.MessageId, messageThreadId: Settings.LogThreadId);
                    break; // Не поддерживаем другие типы сообщений
            }
        }
        catch (Telegram.Bot.Exceptions.ApiRequestException ex) when (ex.Message.Contains("bot was blocked by the user"))
        {
            await _bot.SendMessage(msg.Chat.Id, "❌ Пользователь заблокировал бота.",
                messageThreadId: msg.MessageThreadId);
            
        }
        
    
    }

}