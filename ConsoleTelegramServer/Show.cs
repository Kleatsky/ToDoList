using ConsoleTelegramServer.DB;
using ConsoleTelegramServer.Model;
using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTelegramServer
{
    internal class Show
    {
        public static async Task<List<TodoListTask>> ShowTasks(long userId, CancellationToken cancellationToken)
        {
            List<TodoListTask> todoListTasks = new List<TodoListTask>();

            todoListTasks = await ShowFromDB.GetTaskAsync(userId, cancellationToken);

            return todoListTasks;
        }
        public static async Task<List<TodoListTask>> ShowTasksWithType(long userId, TaskType taskType, CancellationToken cancellationToken)
        {
            List<TodoListTask> todoListTasks = new List<TodoListTask>();

            todoListTasks = await ShowFromDB.GetTaskWithTypeAsync(userId, taskType, cancellationToken);

            return todoListTasks;
        }
    }
}
