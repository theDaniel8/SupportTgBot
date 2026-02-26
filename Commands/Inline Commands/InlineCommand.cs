using Telegram.Bot;
using Telegram.Bot.Types;
public abstract class InlineCommand
{
    protected readonly TelegramBotClient _bot;
    protected readonly DatabaseService _db;

    public InlineCommand(TelegramBotClient bot, DatabaseService db)
    {
        _bot = bot;
        _db = db;
    }
    public abstract string CallbackData { get; }
    public abstract Task Execute(CallbackQuery query);
}
