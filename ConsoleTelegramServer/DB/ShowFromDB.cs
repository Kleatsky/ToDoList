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
            using (var connection = new NpgsqlConnection(ConnectionString.connectionString))
            {
                var tasks = await connection.QueryAsync<TodoListTask>(query, new { telegramUserId });

                todoListTasks = tasks.ToList();
                foreach (var item in todoListTasks)
                {
                    ServerConsoleWrite.SimpleWrite($"Получена задача:ID={item.Id} user_id " +
                        $"{item.TelegramUserId} {item.TextTask}, Тип: {item.Priority}, До: {item.DateTask}");
                }
            }
            return todoListTasks;
        }
        public static async Task<List<TodoListTask>> GetTaskWithTypeAsync(long telegramUserId,
                    TaskType taskType, CancellationToken cancellationToken)
        {
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
            using (var connection = new NpgsqlConnection(ConnectionString.connectionString))
            {
                var tasks = await connection.QueryAsync<TodoListTask>(query, new { telegramUserId, priority });

                todoListTasks = tasks.ToList();
                foreach (var item in todoListTasks)
                {
                    ServerConsoleWrite.SimpleWrite($"Получена задача:ID={item.Id} user_id " +
                        $"{item.TelegramUserId} {item.TextTask}, Тип: {item.Priority}, До: {item.DateTask}");
                }
            }
            return todoListTasks;
        }
    }
}
