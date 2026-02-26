using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;

class Start : SlashCommand
{
    
    public Start(TelegramBotClient bot, DatabaseService db) : base(bot, db)
    {
    }

    public override string Name => "/start";

    public override async Task Execute(Message msg)
    {
        if (msg.Chat.Type != ChatType.Private) return;
        await _bot.SendMessage(msg.Chat.Id, $"Приветствуем вас в боте поддржки. Напишите ваше сообщения и служба поддержки ответит вам в ближайшее время.");
    }
}