using Telegram.Bot;
using Telegram.Bot.Types;

public abstract class ReplyCommand
{
    protected readonly TelegramBotClient _bot;
    protected readonly DatabaseService _db;

    public ReplyCommand(TelegramBotClient bot, DatabaseService db)
    {
        _bot = bot;
        _db = db;
    }
    public abstract string Name { get; }
    public abstract Task Execute(Message msg);
}
