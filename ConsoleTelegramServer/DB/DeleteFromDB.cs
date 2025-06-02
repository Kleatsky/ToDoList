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
            string connectionString = "Host=localhost;Username=postgres;Password=12345;Database=ToDoList;Port=5432";

            var query = @"DELETE FROM tasks WHERE id = @TaskId;";

            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.QueryAsync(query, new { TaskId = taskId });
                Console.WriteLine($"Задача сохранена в БД: ID={taskId}");
            }
        }
    }
}
