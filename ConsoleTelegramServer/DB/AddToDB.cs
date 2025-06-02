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
    internal class AddToDB
    {
        public static async Task SaveTaskAsync(TodoListTask task)
        {
            var query = @"INSERT INTO tasks (telegram_user_id, text_task, priority, is_completed, datetask) VALUES (@TelegramUserId, @TextTask, @Priority, @IsCompleted, @DateTask)";

            using (var connection = new NpgsqlConnection(ConnectionString.connectionString))
            {
                var id = await connection.ExecuteAsync(query, task);

                //Log
                ServerConsoleWrite.SimpleWrite($"Задача сохранена (заглушка): usert_id:{task.TelegramUserId} {task.TextTask}," +
                    $" Тип: {task.Priority}, До: {task.DateTask}");
            }
        }
    }
}
