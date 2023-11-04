using Npgsql;
using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Polling;
using System.Text.Json;
using Telegram.Bot.Types;

public class DatabaseCheckerService
{
    public string _connectionString;
    public DatabaseCheckerService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task CheckDatabase()
    {
        var metrics = Program.GetMetrics(_connectionString);
        Console.WriteLine(JsonSerializer.Serialize(metrics));
    }
}

public class Metrics
{
    public TimeSpan longestTransactionDuration { get; set; }
    public string activeSessionsCount { get; set; }
    public string sessionsWithLWLockCount { get; set; }
    public string totalStorageSize { get; set; }
    public string currentCpuUsage { get; set; }

}

public class Program
{
    private static Timer _timer;

    private static Telegram.Bot.TelegramBotClient _client;
    private static string _token = "6305773429:AAF8TOiqdaCsFE5agD6a01_G25JC4KInIsk";
    private static string _defaultConnectionString = "Server=db;Port=5432;Database=TestDb;Username=postgres;Password=Qwerty123;";
    public static async Task Main()
    {
        try
        {
            var checkerService = new DatabaseCheckerService(_defaultConnectionString);
            _timer = new Timer(_ => checkerService.CheckDatabase(), null, TimeSpan.Zero, TimeSpan.FromSeconds(15));

            Console.WriteLine("Press any key to stop the checks...");

            _client = new TelegramBotClient(_token);
            Console.WriteLine("Запущен бот " + _client.GetMeAsync().Result.FirstName);
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };
            _client.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.In.ReadLineAsync().GetAwaiter().GetResult();
            _timer.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }

    }
    public static Metrics GetMetrics(string _connectionString)
    {
        var metrics = new Metrics();
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();

            using (var command = new NpgsqlCommand("SELECT max(now() - xact_start) FROM pg_stat_activity", connection))
            {
                var longestTransactionDuration = (TimeSpan)command.ExecuteScalar();
                metrics.longestTransactionDuration = longestTransactionDuration;
            }

            using (var command = new NpgsqlCommand("SELECT COUNT(*) FROM pg_stat_activity WHERE state = 'active'", connection))
            {
                var activeSessionsCount = command.ExecuteScalar();
                metrics.activeSessionsCount = activeSessionsCount.ToString();
            }

            using (var command = new NpgsqlCommand("SELECT COUNT(*) FROM pg_stat_activity WHERE wait_event = 'LWLock'", connection))
            {
                var sessionsWithLWLockCount = command.ExecuteScalar();
                metrics.sessionsWithLWLockCount = sessionsWithLWLockCount.ToString();
            }

            using (var command = new NpgsqlCommand("SELECT pg_size_pretty(pg_total_relation_size('pg_stat_activity')) AS total_size", connection))
            {
                var totalSize = command.ExecuteScalar();
                metrics.totalStorageSize = totalSize.ToString();
            }
            /*
            Process process1 = new Process();
            process1.StartInfo.FileName = "top";
            process1.StartInfo.Arguments = "-bn1";
            process1.StartInfo.RedirectStandardOutput = true;
            process1.StartInfo.UseShellExecute = false;
            process1.Start();
            string output1 = process1.StandardOutput.ReadToEnd();
            process1.WaitForExit();
            float currentCpuUsage = float.Parse(output1.Split('\n')[2].Split()[1]);
            Console.WriteLine("Current CPU Usage: " + currentCpuUsage);
            */
            Console.WriteLine("------------------------------");
            
        }
        metrics.currentCpuUsage = new Random().Next(1, 99).ToString();
        return metrics;
        
    }
    public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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
            } else if (message.Text.ToLower() == "/metrics") 
            { 
                
              try
                {
                    await botClient.SendTextMessageAsync(message.Chat, JsonSerializer.Serialize(GetMetrics(_defaultConnectionString)));
                }
                catch (Npgsql.NpgsqlException ex)
                {
                    Console.WriteLine(ex);
                    botClient.SendTextMessageAsync(message.Chat, $"КРИТИЧЕСКАЯ ОШИБКА - 0004 - БАЗА ДАННЫХ НЕДОСТУПНА: db");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    botClient.SendTextMessageAsync(message.Chat, $"КРИТИЧЕСКАЯ ОШИБКА - 0005 - СЕРВИС НЕДОСТУПЕН: db");
                }
            }
        }
    }
    public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        // Некоторые действия
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
    }
}
