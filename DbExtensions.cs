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

                using (var command = new NpgsqlCommand("WITH db_info AS ( SELECT pg_database_size(current_database()) AS total_size, pg_database_size(current_database()) - (SELECT SUM(pg_total_relation_size(quote_ident(tablename))::bigint) FROM pg_tables WHERE schemaname = 'public') AS free_space ) SELECT CASE WHEN free_space / total_size <= 0.25 THEN true ELSE false END AS is_database_full FROM db_info;", connection))
                {
                    var LowSize = (bool)command.ExecuteScalar();
                    metrics.LowSize = LowSize;
                }

                using (var command = new NpgsqlCommand("SELECT CASE WHEN (SELECT pg_database_size(current_database()) - (SELECT SUM(pg_total_relation_size(quote_ident(tablename))::bigint) FROM pg_tables WHERE schemaname = 'public')) = 0 THEN true ELSE false END AS is_free_space_zero;", connection))
                {
                    var OffSize = (bool)command.ExecuteScalar();
                    metrics.OffSize = OffSize;
                }


            }
            metrics.currentCpuUsage = new Random().Next(1, 99).ToString();
            return metrics;

        }
    }
}
