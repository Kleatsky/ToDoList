using ConsoleTelegramServer.Model;
using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTelegramServer.DB
{
    internal class DeleteFromDB
    {
        public static async Task DeleteTaskAsync(long taskId, CancellationToken cancellationToken)
        {
            var query = @"DELETE FROM tasks WHERE id = @TaskId;";

            using (var connection = new NpgsqlConnection(ConnectionString.connectionString))
            {
                await connection.QueryAsync(query, new { TaskId = taskId });

                //Log
                ServerConsoleWrite.SimpleWrite($"Задача сохранена в БД: ID={taskId}");
            }
        }
    }
}
