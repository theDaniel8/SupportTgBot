using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
public class ChatInfo : SlashCommand
{
    public ChatInfo(TelegramBotClient bot, DatabaseService db) : base(bot, db)
    {
    }

    public override string Name => "/chatinfo";

    public override async Task Execute(Message msg)
    {
        await _bot.SendMessage(msg.Chat.Id, $"Chat ID: <code>{msg.Chat.Id}</code>\nTopic ID: <code>{msg.MessageThreadId}</code>", 
            messageThreadId: msg.MessageThreadId, 
            parseMode: ParseMode.Html);
    }
}