using Telegram.Bot;

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(Settings.BotToken, cancellationToken: cts.Token);
var me = await bot.GetMe();

DatabaseService db = new DatabaseService(Settings.ConnectionString);
LogService log = new LogService(bot, db);

Handler handler = new Handler(bot, db, log);

bot.OnError += handler.OnError;
bot.OnMessage += handler.OnMessage;
bot.OnUpdate += handler.OnUpdate;

Console.WriteLine($"@{me.Username} запущен... Нажмите Enter для завершения");
Console.ReadLine();
cts.Cancel();