using Npgsql;
using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Polling;
using System.Text.Json;
using Telegram.Bot.Types;
using PostgreSqlMonitoringBot;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Hangfire;
using Quartz.Impl;
using Quartz;

string _defaultConnectionString = "Server=db;Port=5432;Database=TestDb;Username=postgres;Password=Qwerty123;";
string _token = "6684432976:AAHHpDpa8cCnAc-zmytoWcNVEmSHymJyYTA";

List<string> _connStrings = new List<string>()
{
    "Server=db;Port=5432;Database=TestDb;Username=postgres;Password=Qwerty123;",
    "Server=smoldb;Port=5432;Database=TestDb;Username=postgres;Password=Qwerty123;"
};

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(async (context, services) =>
    {
        var telegramBot = new TelegramBotClient(_token);
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { },
        };
        telegramBot.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken
        );
        services.AddSingleton(telegramBot);

        ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
        IScheduler scheduler = await schedulerFactory.GetScheduler();

        await scheduler.Start();

        foreach (var connString in _connStrings)
        {
            NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder(connString);
            var dbName = builder.Host;

            IJobDetail job = JobBuilder.Create<CheckDbJob>()
                .WithIdentity(dbName + "Job", "metrics")
                .UsingJobData("connString", connString)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(dbName + "Trigger", "metrics")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(15)
                    .RepeatForever())
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    })
    .Build();

    await host.RunAsync();

    async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Некоторые действия
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
        if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
        {
            var message = update.Message;

            if (message.Text.ToLower() == "/start")
            {
                await botClient.SendTextMessageAsync(message.Chat, "Сервис мониторинга Rezetka готов к работе");
                return;
            }
            else if (message.Text.ToLower() == "/metrics")
            {
                await botClient.SendTextMessageAsync(message.Chat, JsonSerializer.Serialize(DbExtensions.GetMetrics(_defaultConnectionString)));
            }
        }
    }
    async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        // Некоторые действия
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
    }

