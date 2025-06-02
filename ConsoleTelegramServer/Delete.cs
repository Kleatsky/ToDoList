using ConsoleTelegramServer.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTelegramServer
{
    internal class Delete
    {
        public static async Task DeleteTask(long taskId, CancellationToken cancellationToken)
        {
            await DeleteFromDB.DeleteTaskAsync(taskId, cancellationToken);
        }
    }
}
