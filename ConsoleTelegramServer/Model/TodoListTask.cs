using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTelegramServer.Model
{
    internal class TodoListTask
    {
        public long Id { get; set; }
        public long TelegramUserId { get; set; }
        public string TextTask { get; set; }
        public int Priority { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? DateTask { get; set; }
    }
}
