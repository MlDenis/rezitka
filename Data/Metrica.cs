﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostgreSqlMonitoringBot
{
    public class Metrica
    {
        public Guid id { get; set; }
        public DateTime dt { get; set; } = DateTime.UtcNow;
        public TimeSpan longestTransactionDuration { get; set; }
        public string activeSessionsCount { get; set; }
        public string sessionsWithLWLockCount { get; set; }
        public string totalStorageSize { get; set; }
        public string currentCpuUsage { get; set; }
    }
}