using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
public class Tag : SlashCommand
{
    public Tag(TelegramBotClient bot, DatabaseService db) : base(bot, db)
    {
    }

    public override string Name => "/tag";

    public override async Task Execute(Message msg)
    {
        if (msg.Chat.Id != Settings.GroupId || msg.From == null || msg.Text == null) return;

        _db.InsertOrIgnoreAdmin(msg.From.Id);
        Admin? admin = _db.GetAdmin(msg.From.Id);
        if (admin == null) return;
        string tag = msg.Text.Replace("/tag", "").Trim();
        if (string.IsNullOrEmpty(tag))
        {
            string text = admin.Tag == null
                ? "Сейчас у вас нет тега. \n\nЧтобы установить тег, отправьте команду в формате:\n/tag [ваш тег]. Например, /tag АгентГабена"
                : $"Сейчас ваш тег выглядит вот так: {admin.Tag} \n\nЧтобы изменить его, отправьте команду в формате:\n/tag [ваш новый тег], например, /tag АгентГабена";
            
            await _bot.SendMessage(msg.Chat.Id, text, messageThreadId: msg.MessageThreadId);
            return;
        }
        else if (tag == "remove")
        {
            admin.Tag = null;
            _db.UpdateAdmin(admin);
            await _bot.SendMessage(Settings.GroupId, "✔️ Ваш тег успешно удален.", messageThreadId: msg.MessageThreadId);
        }
        else
        {
            tag = tag.Replace("#", "");
            admin.Tag = "#" + tag;
            _db.UpdateAdmin(admin);
            await _bot.SendMessage(Settings.GroupId, $"✔️ Ваш тег успешно установлен: {admin.Tag}", messageThreadId: msg.MessageThreadId);
        }
        
    }
}