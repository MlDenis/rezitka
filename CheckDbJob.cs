using Npgsql;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PostgreSqlMonitoringBot
{
    [DisallowConcurrentExecution]
    public class CheckDbJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var connString = context.JobDetail.JobDataMap.GetString("connString");

            var schedulerContext = context.Scheduler.Context;
            var dbContext = (AppDbContext)schedulerContext.Get("dbContext");

            var metrics = DbExtensions.GetMetrics(connString);
            dbContext.Metrics.Add(metrics);
            await dbContext.SaveChangesAsync();

            NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder(connString);
            var host = builder.Host;
            Console.WriteLine("Host: "+ host + "\nData: " + JsonSerializer.Serialize(metrics));
            Console.WriteLine("____________________________________________________________________");
            return;
        }
    }
}
