using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostgreSqlMonitoringBot
{
    public class TelegramUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key]
        public Guid Id { get; set; }
        public string ChatId { get; set; }
        public string Name { get; set; }
    }
}
