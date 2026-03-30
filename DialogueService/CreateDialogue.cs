using Microsoft.Extensions.Configuration;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;

class CreateDialogue : DialogueService
{
    public CreateDialogue(TelegramBotClient bot, DatabaseService db, LogService log) : base(bot, db, log)
    {
    }
    
    public override bool Сondition(Message msg)
    {
        
        if (msg.Chat.Type != ChatType.Private || msg.From == null || _db.IsInDialogue(msg.From.Id))
            return false;

        BotUser? botUser = _db.GetBotUser(msg.From.Id);
        if (botUser is null || botUser.Ban)
            return false;

        return true;
    }
    
    public override async Task Execute(Message msg)
    {   
        await CreateTopic(msg);
        await _bot.ForwardMessage(Settings.GroupId, msg.From!.Id, msg.Id, messageThreadId: _db.GetTopicIdByUserId(msg.From.Id));
        await _log.MessageFromUser(msg);
    }
}

