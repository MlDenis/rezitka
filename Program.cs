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

internal class Program
{
    public static string _defaultConnectionString = "Server=db;Port=5432;Database=TestDb;Username=postgres;Password=Qwerty123;";
    public static string _token = "6684432976:AAHHpDpa8cCnAc-zmytoWcNVEmSHymJyYTA";

    public static List<string> _connStrings = new List<string>()
    {
        "Server=db;Port=5432;Database=TestDb;Username=postgres;Password=Qwerty123;",
        "Server=smoldb;Port=5433;Database=TestDb;Username=postgres;Password=Qwerty123;"
    };

    private static async Task Main(string[] args)
    {
        IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(async (context, services) =>
            {
                services.AddDbContext<AppDbContext>(options => options.UseNpgsql(_defaultConnectionString));

                var telegramBot = new TelegramBot(_token).StartAsync();
                services.AddSingleton(telegramBot);

                ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
                IScheduler scheduler = await schedulerFactory.GetScheduler();

                await scheduler.Start();

                var provider = services.BuildServiceProvider();
                var dbContext = provider.GetService<AppDbContext>();
                await dbContext.Database.MigrateAsync();
                scheduler.Context.Put("dbContext", dbContext);
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
                            .WithIntervalInSeconds(15)
                            .RepeatForever())
                        .Build();

                    await scheduler.ScheduleJob(job, trigger);
                }
            })
            .Build();

        await host.RunAsync();
    }
}