using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostgreSqlMonitoringBot
{
    public static class DbExtensions
    {
        public static Metrica GetMetrics(string _connectionString)
        {
            var metrics = new Metrica();

            var response = CreateConnection(_connectionString);
            var connection = response.Connection;
            connection.Open();
            var isAvailable = response.isAvailable;
            if (!isAvailable)
            {
                
            }
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

            using (var command = new NpgsqlCommand("SELECT pg_size_pretty(pg_database_size(current_database()))", connection))
            {
                var totalSize = command.ExecuteScalar();
                metrics.totalStorageSize = totalSize.ToString();
            }

            metrics.currentCpuUsage = new Random().Next(1, 99).ToString();
            return metrics;
        }
        public static NpgsqlConnectionResponse CreateConnection(string connString)
        {
            var response = new NpgsqlConnectionResponse();  
            try
            {
                var connection = new NpgsqlConnection(connString);
                connection.Open();
                response.Connection = connection;
                response.Message = "Connection established";
                response.isAvailable = true;
                return response;
            }
            catch (NpgsqlException ex)
            {
                if (ex.SqlState == "28P01")
                {
                    response.Message = "Authentication failed: Invalid username or password.";
                    response.isAvailable = false;
                    return response;
                }
                else
                {
                    response.Message = "Connection refused.";
                    response.isAvailable = false;
                    return response;
                }
            }
        }
    }
}
