using Telegram.Bot;
using Telegram.Bot.Types;


public class RenameTopic : InlineCommand
{
    public RenameTopic(TelegramBotClient bot, DatabaseService db) : base(bot, db)
    {
    }

    public override string CallbackData => "renameTopic";

    public override async Task Execute(CallbackQuery query)
    {
        await _bot.AnswerCallbackQuery(query.Id);
        if (query.Message?.MessageThreadId == null || query.From == null) return;

        BotUser? botUser = _db.GetBotUser(query.From.Id);

        try
        {
            await _bot.EditForumTopic(Settings.GroupId, query.Message.MessageThreadId.Value, $"{query.From.FirstName} | {botUser?.Name}");
        }
        catch (Telegram.Bot.Exceptions.ApiRequestException ex) when (ex.Message.Contains("TOPIC_NOT_MODIFIED"))
        {
            return;
        }
    }
}