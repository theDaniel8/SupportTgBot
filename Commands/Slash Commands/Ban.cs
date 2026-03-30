using System.Runtime.CompilerServices;
using Microsoft.Data.Sqlite;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
public class Ban : SlashCommand
{
    public Ban(TelegramBotClient bot, DatabaseService db) : base(bot, db)
    {
    }

    public override string Name => "/ban";

    public override async Task Execute(Message msg)
    {
        if (!(_db.GetUserIdByTopicId(msg.MessageThreadId) is long targetId) || msg.Chat.Id != Settings.GroupId)
            return;

        BotUser? targetUser = _db.GetBotUser(targetId);
        if (targetUser == null) return;

        targetUser.Ban = true;
        _db.UpdateInfoUser(targetUser);
        await _bot.SendMessage(msg.Chat.Id, $"🚫 Пользователь {targetUser.Username} был заблокирован.", messageThreadId: msg.MessageThreadId);
    }
}