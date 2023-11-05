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
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using MediatR;

internal class Program
{
    public static string _defaultConnectionString = "Server=db;Port=5432;Database=TestDb;Username=postgres;Password=Qwerty123;";
    public static string _token = "6684432976:AAHHpDpa8cCnAc-zmytoWcNVEmSHymJyYTA";
    public static AppDbContext dbContext;

    public static List<string> _connStrings = new List<string>()
    {
        "Server=db;Port=5432;Database=TestDb;Username=postgres;Password=Qwerty123;",
        //"Server=smoldb;Port=5433;Database=TestDb;Username=postgres;Password=Qwerty123;"
    };

    private static async Task Main(string[] args)
    {
        IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(async (context, services) =>
            {
                services.AddDbContext<AppDbContext>(options => options.UseNpgsql(_defaultConnectionString));

                ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
                IScheduler scheduler = await schedulerFactory.GetScheduler();

                await scheduler.Start();

                var provider = services.BuildServiceProvider();
                dbContext = provider.GetService<AppDbContext>();
                //var telegramBot = await new TelegramBot(_token, dbContext).StartAsync();

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

                await dbContext.Database.MigrateAsync();
                scheduler.Context.Put("dbContext", dbContext);
                scheduler.Context.Put("bot", telegramBot);

                foreach (var connString in _connStrings)
                {
                    NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder(connString);
                    var hostName = builder.Host;

                    IJobDetail job = JobBuilder.Create<CheckDbJob>()
                        .WithIdentity(hostName + "Job", "metrics")
                        .UsingJobData("connString", connString)
                        .Build();

                    ITrigger trigger = TriggerBuilder.Create()
                        .WithIdentity(hostName + "Trigger", "metrics")
                        .StartNow()
                        .WithSimpleSchedule(x => x
                            .WithIntervalInSeconds(30)
                            .RepeatForever())
                        .Build();

                    await scheduler.ScheduleJob(job, trigger);
                }
            })
            .Build();

        await host.RunAsync();
    }
    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
        if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
        {
            var message = update.Message;
            if (message.Text.ToLower() == "/start")
            {
                await botClient.SendTextMessageAsync(message.Chat, "Сервис мониторинга Rezetka готов к работе");

                var telegramUser = new TelegramUser();
                telegramUser.Name = message.Chat.Username;
                telegramUser.ChatId = message.Chat.Id.ToString();
                dbContext.TelegramUsers.Add(telegramUser);
                await dbContext.SaveChangesAsync();

                return;
            }
            else if (message.Text.ToLower() == "/metrics")
            {
                try
                {
                    var lastMetrics = await dbContext.Metrics.FirstOrDefaultAsync();
                    await botClient.SendTextMessageAsync(message.Chat, JsonSerializer.Serialize(lastMetrics));

                }
                catch (Exception)
                {
                    Console.WriteLine("Exception");
                }
            }
            else if (message.Text.ToLower() == "/currentConnections")
            {
                var connections = "";
                foreach (var connString in Program._connStrings)
                {
                    connections += $"{connString},\n";
                }
                await botClient.SendTextMessageAsync(message.Chat, JsonSerializer.Serialize(connections));
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat, "Я пока не придумал, что на это ответить ):");
            }
        }
    }
    private static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        // Некоторые действия
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
    }
}