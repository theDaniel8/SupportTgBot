using Microsoft.Extensions.Configuration;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;

class ForwardToAdmin : DialogueService
{
    public ForwardToAdmin(TelegramBotClient bot, DatabaseService db, LogService log) : base(bot, db, log)
    {
    }
    int? topicId;
    public override bool Сondition(Message msg)
    {
        if (msg.Chat.Type != ChatType.Private || !_db.IsInDialogue(msg.From!.Id))
            return false;

        BotUser? botUser = _db.GetBotUser(msg.From.Id);
        if (botUser == null  ||  botUser.Ban) return false;
        
        int? topicId = _db.GetTopicIdByUserId(msg.From.Id);
        if (topicId == null) return false;

        this.topicId = topicId;
    
        return true;   
    }
    
    public override async Task Execute(Message msg)
    {   
        try
        {
            Message sentMessage = await _bot.ForwardMessage(Settings.GroupId, msg.Chat.Id, msg.MessageId, messageThreadId: topicId!.Value);
            
            // Telegram молча отправляет в General, если топик был закрыт/удален. 
            // Проверяем, совпадает ли вернувшийся MessageThreadId с тем, куда мы просили отправить.
            if (sentMessage.MessageThreadId != topicId.Value)
            {
                Console.WriteLine($"[TryForwardToAdmin] Топик {topicId.Value} удален! Telegram перенаправил в General. Пересоздаем...");
                
                try { await _bot.DeleteMessage(Settings.GroupId, sentMessage.MessageId); } catch { }

                await CreateTopic(msg); // Создаем новый топик
                
                int? newTopicId = _db.GetTopicIdByUserId(msg.From!.Id);
                if (newTopicId != null)
                {
                    await _bot.ForwardMessage(Settings.GroupId, msg.Chat.Id, msg.MessageId, messageThreadId: newTopicId.Value);
                }
            }
        }
        catch (Telegram.Bot.Exceptions.ApiRequestException ex)
        {
            Console.WriteLine($"[TryForwardToAdmin Error] {ex.ErrorCode}: {ex.Message}");
            if (ex.Message.Contains("message thread not found") || ex.Message.Contains("thread not found"))
            {
                await CreateTopic(msg);

                int? newTopicId = _db.GetTopicIdByUserId(msg.From!.Id);
                if (newTopicId != null)
                {
                    try
                    {
                        await _bot.ForwardMessage(Settings.GroupId, msg.Chat.Id, msg.MessageId, messageThreadId: newTopicId.Value);
                    }
                    catch (Exception ex2) { Console.WriteLine($"[Retry Forward Error]: {ex2.Message}"); }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Unknown Error in Forwarding]: {ex.Message}");
        }

        await _log.MessageFromUser(msg);
        return;
    }
}



