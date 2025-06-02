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
            string connectionString = "Host=localhost;Username=postgres;Password=12345;Database=ToDoList;Port=5432";

            string query = @"UPDATE tasks SET is_completed = NOT is_completed WHERE id = @TaskId RETURNING telegram_user_id;";

            long userId;
            using (var connection = new NpgsqlConnection(connectionString))
            {
                userId = await connection.QueryFirstOrDefaultAsync<long>(query, new { TaskId = taskId });
            }
            return userId;
        }
    }
}
