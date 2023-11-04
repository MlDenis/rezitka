using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PostgreSqlMonitoringBot
{
    public class DatabaseCheckerService
    {
        public string _connectionString;
        public DatabaseCheckerService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task CheckDatabase()
        {
            var metrics = DbExtensions.GetMetrics(_connectionString);
            Console.WriteLine(JsonSerializer.Serialize(metrics));
        }
    }
}
