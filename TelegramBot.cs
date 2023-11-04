using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace PostgreSqlMonitoringBot
{
    public class TelegramBot
    {
        private readonly AppDbContext _dbContext;
        public TelegramBot(string token)
        {
            _token = token; 
        }

        public string _token {  get; set; }

        public async Task<TelegramBotClient> StartAsync()
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

            return telegramBot;
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
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
                    var lastMetrics = _dbContext.Metrics.LastOrDefaultAsync();
                    await botClient.SendTextMessageAsync(message.Chat, JsonSerializer.Serialize(lastMetrics));
                }
            }
        }
        private async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }
    }
}
