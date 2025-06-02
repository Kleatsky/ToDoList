using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTelegramServer.DB
{
    internal class UpdateToDB
    {
        public static async Task<long> ChangeIsComplitedAsync(long taskId)
        {
            string query = @"UPDATE tasks SET is_completed = NOT is_completed WHERE id = @TaskId RETURNING telegram_user_id;";

            long userId;
            using (var connection = new NpgsqlConnection(ConnectionString.connectionString))
            {
                userId = await connection.QueryFirstOrDefaultAsync<long>(query, new { TaskId = taskId });
            }

            ServerConsoleWrite.SimpleWrite($"Задача обнавлена: ID {taskId}");
            return userId;
        }
    }
}
