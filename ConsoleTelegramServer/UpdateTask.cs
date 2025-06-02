using ConsoleTelegramServer.DB;
using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTelegramServer
{
    internal class UpdateTask
    {
        public static async Task<long> IsComplited(long taskId)
        {
            long userId = await UpdateToDB.ChangeIsComplitedAsync(taskId);
            return userId;
        }
    }
}
