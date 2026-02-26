using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
public class Farewell : SlashCommand
{
    public Farewell(TelegramBotClient bot, DatabaseService db) : base(bot, db)
    {
    }

    public override string Name => "/farewell";

    public override async Task Execute(Message msg)
    {
        if (msg.Chat.Id != Settings.GroupId || msg.From == null || msg.Text == null) return;
        _db.InsertOrIgnoreAdmin(msg.From.Id);

        Admin? admin = _db.GetAdmin(msg.From.Id);
        if (admin == null) return;
        string farewell = msg.Text.Replace("/farewell", "").Trim();
        if (string.IsNullOrEmpty(farewell))
        {
            string text = admin.Farewell == null
                ? "Сейчас у вас нет прощального сообщения.\n\nЧтобы установить прощальное сообщение, отправьте команду в формате /farewell [ваше прощальное сообщение]. Например, /farewell До свидания!"
                : $"Сейчас ваше прощальное сообщение выглядит вот так:\n\n{admin.Farewell}\n\nЧтобы изменить его, отправьте команду в формате /farewell [ваше новое прощальное сообщение], например, /farewell До свидания!";
            
            await _bot.SendMessage(msg.Chat.Id, text, messageThreadId: msg.MessageThreadId);
            return;
        }
        else
        {
            admin.Farewell = farewell;
            _db.UpdateAdmin(admin);
            await _bot.SendMessage(Settings.GroupId, $"✔️ Ваше прощальное сообщение успешно установлено.", messageThreadId: msg.MessageThreadId);
        }
        
    }
}