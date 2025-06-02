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
    internal class ShowFromDB
    {
        public static async Task<List<TodoListTask>> GetTaskAsync(long telegramUserId, CancellationToken cancellationToken)
        {
            string connectionString = "Host=localhost;Username=postgres;Password=12345;Database=ToDoList;Port=5432";

            //var query = INSERT INTO tasks (telegram_user_id, text, ""typeTask"", is_completed, datetask) VALUES (@TelegramUserId, @TextTask, @TypeTask, @IsCompleted, @DateTask)";
            var query = @"SELECT 
                id, 
                telegram_user_id AS TelegramUserId, 
                text_task AS TextTask, 
                priority AS Priority, 
                is_completed AS IsCompleted, 
                datetask AS DateTask 
              FROM tasks 
              WHERE telegram_user_id = @telegramUserId 
              ORDER BY datetask;";
            List<TodoListTask> todoListTasks = null!;
            using (var connection = new NpgsqlConnection(connectionString))
            {
                var tasks = await connection.QueryAsync<TodoListTask>(query, new { telegramUserId });

                todoListTasks = tasks.ToList();
                foreach (var item in todoListTasks)
                {
                    Console.WriteLine($"Задача сохранена (заглушка): {item.TextTask}, Тип: {item.Priority}, До: {item.DateTask}");
                    Console.WriteLine($"Задача сохранена в БД: {item.TextTask}, ID={item.Id}");
                }
            }

            return todoListTasks;
        }
        public static async Task<List<TodoListTask>> GetTaskWithTypeAsync(long telegramUserId, 
                    TaskType taskType, CancellationToken cancellationToken)
        {
            string connectionString = "Host=localhost;Username=postgres;Password=12345;Database=ToDoList;Port=5432";

            //var query = INSERT INTO tasks (telegram_user_id, text, ""typeTask"", is_completed, datetask) VALUES (@TelegramUserId, @TextTask, @TypeTask, @IsCompleted, @DateTask)";
            var query = @"SELECT 
                id, 
                telegram_user_id AS TelegramUserId, 
                text_task AS TextTask, 
                priority AS Priority, 
                is_completed AS IsCompleted, 
                datetask AS DateTask 
              FROM tasks 
              WHERE telegram_user_id = @telegramUserId 
              AND priority = @priority
              ORDER BY datetask;";
            List<TodoListTask> todoListTasks = null!;
            int priority = (int)taskType;
            using (var connection = new NpgsqlConnection(connectionString))
            {
                var tasks = await connection.QueryAsync<TodoListTask>(query, new { telegramUserId, priority });

                todoListTasks = tasks.ToList();
                foreach (var item in todoListTasks)
                {
                    Console.WriteLine($"Задача сохранена (заглушка): {item.TextTask}, Тип: {item.Priority}, До: {item.DateTask}");
                    Console.WriteLine($"Задача сохранена в БД: {item.TextTask}, ID={item.Id}");
                }
            }

            return todoListTasks;
        }
    }
}
