using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public abstract class SlashCommand
{
    protected readonly TelegramBotClient _bot;
    protected readonly DatabaseService _db;

    public SlashCommand(TelegramBotClient bot, DatabaseService db)
    {
        _bot = bot;
        _db = db;
    }
    public abstract string Name { get; }
    public abstract Task Execute(Message msg);
}
