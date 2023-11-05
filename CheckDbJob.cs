using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;

namespace PostgreSqlMonitoringBot
{
    [DisallowConcurrentExecution]
    public class CheckDbJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var connString = context.JobDetail.JobDataMap.GetString("connString");
            NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder(connString);
            var host = builder.Host;

            var schedulerContext = context.Scheduler.Context;
            var dbContext = (AppDbContext)schedulerContext.Get("dbContext");
            var bot = (TelegramBotClient)schedulerContext.Get("bot");

            var result = DbExtensions.CreateConnection(connString);

            if (!result.isAvailable)
            {
                var users = await dbContext.TelegramUsers.ToListAsync();
                var uniqueUsers = users.Distinct();
                foreach (var user in uniqueUsers)
                {
                    await bot.SendTextMessageAsync(user.ChatId,host + " " + result.Message);
                }
            }

            var metrics = DbExtensions.GetMetrics(connString);
            dbContext.Metrics.Add(metrics);
            await dbContext.SaveChangesAsync();

            Console.WriteLine("Host: "+ host + "\nData: " + JsonSerializer.Serialize(metrics));
            Console.WriteLine("____________________________________________________________________");
            return;
        }
    }
}
