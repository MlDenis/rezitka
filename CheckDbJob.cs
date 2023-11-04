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
    public class CheckDbJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var connString = context.JobDetail.JobDataMap.GetString("connString");
            var metrics = DbExtensions.GetMetrics(connString);
            NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder(connString);
            var host = builder.Host;
            Console.WriteLine("Host: "+ host + "\nData: " + JsonSerializer.Serialize(metrics));
            Console.WriteLine("____________________________________________________________________");
            return Task.CompletedTask;
        }
    }
}
