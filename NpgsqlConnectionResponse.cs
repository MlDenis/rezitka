using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostgreSqlMonitoringBot
{
    public class NpgsqlConnectionResponse
    {
        public NpgsqlConnection Connection { get; set; }  
        public bool isAvailable { get; set; }
        public string Message { get; set; }
    }
}
