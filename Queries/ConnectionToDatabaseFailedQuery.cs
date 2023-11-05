using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace PostgreSqlMonitoringBot.Queries
{
    public class ConnectionToDatabaseFailedQuery : INotification
    {
        public ConnectionToDatabaseFailedQuery(NpgsqlConnectionResponse npgsqlConnectionResponse)
        {
            connectionResponse = npgsqlConnectionResponse;
        }

        public NpgsqlConnectionResponse connectionResponse { get; set; }
    }
    public class ConnectionToDatabaseFailedQueryHandler : INotificationHandler<ConnectionToDatabaseFailedQuery>
    {
        public ConnectionToDatabaseFailedQueryHandler(AppDbContext appDbContext, TelegramBotClient telegramBotClient)
        {
            _dbContext = appDbContext;
            _bot = telegramBotClient;
        }

        private AppDbContext _dbContext {  get; set; }
        public TelegramBotClient _bot { get; set; }
        public async Task Handle(ConnectionToDatabaseFailedQuery notification, CancellationToken cancellationToken)
        {
            var users = await _dbContext.TelegramUsers.ToListAsync();
            foreach (var user in users)
            {
                await _bot.SendTextMessageAsync(user.ChatId, notification.connectionResponse.Message);
            }
        }
    }
}
