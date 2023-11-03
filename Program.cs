using Newtonsoft.Json.Linq;
using Npgsql;
using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public class DatabaseCheckerService
{
    public DatabaseCheckerService()
    {
    }

    public async Task CheckDatabase(string _connectionString)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            try
            {
                connection.Open();
            }
            catch (Exception)
            {
                await Console.Out.WriteLineAsync("Connection Error");
            }


            using (var command = new NpgsqlCommand("SELECT max(now() - xact_start) FROM pg_stat_activity", connection))
            {
                var longestTransactionDuration = (TimeSpan)command.ExecuteScalar();
                Console.WriteLine("Longest Transaction Duration: " + longestTransactionDuration);
            }

            using (var command = new NpgsqlCommand("SELECT COUNT(*) FROM pg_stat_activity WHERE state = 'active'", connection))
            {
                var activeSessionsCount = command.ExecuteScalar();
                Console.WriteLine("Active Session Count: " + activeSessionsCount);
            }

            using (var command = new NpgsqlCommand("SELECT COUNT(*) FROM pg_stat_activity WHERE wait_event = 'LWLock'", connection))
            {
                var sessionsWithLWLockCount = command.ExecuteScalar();
                Console.WriteLine("Sessions with LWLockCount: " + sessionsWithLWLockCount);
            }

            using (var command = new NpgsqlCommand("SELECT pg_size_pretty(pg_total_relation_size('pg_stat_activity')) AS total_size", connection))
            {
                var totalSize = command.ExecuteScalar();

                Console.WriteLine("Total Size: " + totalSize);
            }

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

            Console.WriteLine("------------------------------");
        }
    }
}

public class Program
{
    private static Timer _timer;

    private static Telegram.Bot.TelegramBotClient _client;
    public static async Task Main()
    {
        try
        {
            var defaultConnectionString = "Server=db;Port=5432;Database=TestDb;Username=postgres;Password=Qwerty123;"; // Replace with your PostgreSQL connection string

            var checkerService = new DatabaseCheckerService();
            _timer = new Timer(_ => checkerService.CheckDatabase(defaultConnectionString), null, TimeSpan.Zero, TimeSpan.FromSeconds(15));

            Console.WriteLine("Press any key to stop the checks...");
            Console.In.ReadLineAsync().GetAwaiter().GetResult();

            _timer.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }

    }
}
