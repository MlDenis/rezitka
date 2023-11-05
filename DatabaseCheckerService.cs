using MediatR;
using Npgsql;
using PostgreSqlMonitoringBot.Queries;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PostgreSqlMonitoringBot
{
    public class DatabaseCheckerServiceJob : IJob 
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var connString = context.JobDetail.JobDataMap.GetString("connString");

            var schedulerContext = context.Scheduler.Context;
            var mediatr = (IMediator)schedulerContext.Get("mediatr");

            var result = DbExtensions.CreateConnection(connString);

            if(!result.isAvailable)
            {
                await mediatr.Publish(new ConnectionToDatabaseFailedQuery(result));
            }
            return;
        }
    }
}
