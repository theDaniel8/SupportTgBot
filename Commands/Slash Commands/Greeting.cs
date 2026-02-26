using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
public class Greeting : SlashCommand
{
    public Greeting(TelegramBotClient bot, DatabaseService db) : base(bot, db)
    {
    }

    public override string Name => "/greeting";

    public override async Task Execute(Message msg)
    {
        if (msg.Chat.Id != Settings.GroupId || msg.From == null || msg.Text == null) return;

        _db.InsertOrIgnoreAdmin(msg.From.Id);
        Admin? admin = _db.GetAdmin(msg.From.Id);
        if (admin == null) return;

        
        string greeting = msg.Text.Replace("/greeting", "").Trim();
        if (string.IsNullOrEmpty(greeting))
        {
            string text = admin.Greeting == null
                ? "Сейчас у вас нет приветствия.\n\nЧтобы установить приветствие, отправьте команду в формате:\n/greeting [ваше приветствие]. Например, /greeting Добро пожаловать!"
                : $"Сейчас ваше приветствие выглядит вот так:\n\n{admin.Greeting}\n\nЧтобы изменить его, отправьте команду в формате:\n/greeting [ваше новое приветствие], например, /greeting Добро пожаловать!";
            
            await _bot.SendMessage(msg.Chat.Id, text, messageThreadId: msg.MessageThreadId);
            return;
        }
        else
        {
            admin.Greeting = greeting;
            _db.UpdateAdmin(admin);
            await _bot.SendMessage(Settings.GroupId, $"✔️ Ваше приветствие успешно установлено.", messageThreadId: msg.MessageThreadId);
        }
        
    }
}