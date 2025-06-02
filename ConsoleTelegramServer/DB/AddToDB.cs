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
            string connectionString = "Host=localhost;Username=postgres;Password=12345;Database=ToDoList;Port=5432";

            var query = @"INSERT INTO tasks (telegram_user_id, text_task, priority, is_completed, datetask) VALUES (@TelegramUserId, @TextTask, @Priority, @IsCompleted, @DateTask)";

            using (var connection = new NpgsqlConnection(connectionString))
            {
                var id = await connection.ExecuteAsync(query, task);
                Console.WriteLine($"Задача сохранена (заглушка): {task.TextTask}, Тип: {task.Priority}, До: {task.DateTask}");


                //Console.WriteLine($"Задача сохранена в БД: {task.Text}, ID={id}");
            }
        }
    }
}
