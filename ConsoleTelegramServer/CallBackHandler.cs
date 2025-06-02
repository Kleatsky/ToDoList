using ConsoleTelegramServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ConsoleTelegramServer
{
    internal class CallBackHandler
    {
        public static async Task CompliteTaskHandler(ITelegramBotClient bot, Update update, CancellationToken cancellationToken,
            string data, long chatId, int messageId)
        {
            try
            {
                long taskId = long.Parse(data.Split(' ')[1]);
                int preorityTask = int.Parse(data.Split(' ')[2]);
                long userId = await UpdateTask.IsComplited(taskId);
                int page = int.Parse(data.Split(' ')[4]);
                bool isForDelete = bool.Parse(data.Split(" ")[5]);

                if (userId == 0)
                {
                    Console.WriteLine("Id пользователя или id задачи в бд не найден.");
                    return;
                }
                {
                    await DrowKeyboard.RedrowTasks(bot, update, chatId, userId,
                            messageId, cancellationToken, page, isForDelete, preorityTask);
                }

            }
            catch (Exception e)
            {
                ServerConsoleWrite.ErrorWrite($"Ошибка смены выполнения задачи: {e.Message}");
                return;
            }
        }
        public static async Task MenuTypeHandler(ITelegramBotClient bot, Update update, CancellationToken cancellationToken,
            string data, long chatId, int messageId)
        {
            try
            {
                if (!Enum.TryParse<TaskType>(data.Split(' ')[1], out TaskType taskType))
                {
                    ServerConsoleWrite.ErrorWrite($"Ошибка при парсинге:ChatId: {chatId} data: {data}");
                    return;
                }
                long userId = long.Parse(data.Split(' ')[2]);
                bool isForDelete = bool.Parse(data.Split(" ")[3]);

                //Удаляем текущую клавиатуру
                await bot.DeleteMessage(chatId: chatId,
                    messageId: messageId,
                    cancellationToken: cancellationToken);

                //Получение данных по типу и отрисовка
                List<TodoListTask> todoListTasks = await Show.ShowTasksWithType(userId, taskType, cancellationToken);
                if (todoListTasks != null && todoListTasks.Count > 0)
                {
                    int startingPage = 1;
                    await DrowKeyboard.DrowTasks(bot, update, cancellationToken, todoListTasks,
                        chatId, userId, startingPage, isForDelete, (int)taskType);
                }
                else
                {
                    await bot.SendMessage(
                        chatId: chatId,
                        text: $"Списка задач не найдено.",
                        cancellationToken: cancellationToken
                    );
                    ServerConsoleWrite.ErrorWrite($"Списка задач не найдено.");
                }
            }
            catch (Exception e)
            {
                ServerConsoleWrite.ErrorWrite($"Ошибка при парсинге:ChatId: {chatId} data: {data} ошибка{e.Message}");
                return;
            }
        }
        public static async Task PrevPageHandler(ITelegramBotClient bot, Update update, CancellationToken cancellationToken,
            string data, long chatId, int messageId)
        {

            try
            {
                int taskType = int.Parse(data.Split(' ')[1]);
                long userId = long.Parse(data.Split(' ')[2]);
                int page = int.Parse(data.Split(' ')[3]);
                bool isForDelete = bool.Parse(data.Split(" ")[4]);

                await DrowKeyboard.RedrowTasks(bot, update, chatId, userId,
                            messageId, cancellationToken, page - 1, isForDelete, taskType);
            }
            catch (Exception e)
            {
                ServerConsoleWrite.ErrorWrite($"Ошибка пагенации: {e.Message}");
                return;
            }
        }

        public static async Task NextPageHandler(ITelegramBotClient bot, Update update, CancellationToken cancellationToken,
            string data, long chatId, int messageId)
        {
            try
            {
                int taskType = int.Parse(data.Split(' ')[1]);
                long userId = long.Parse(data.Split(' ')[2]);
                int page = int.Parse(data.Split(' ')[3]);
                bool isForDelete = bool.Parse(data.Split(" ")[4]);

                await DrowKeyboard.RedrowTasks(bot, update, chatId, userId,
                            messageId, cancellationToken, page + 1, isForDelete, taskType);
            }
            catch (Exception e)
            {
                ServerConsoleWrite.ErrorWrite($"Ошибка пагенации: {e.Message}");
                return;
            }
        }
        public static async Task DeleteHandler(ITelegramBotClient bot, Update update, CancellationToken cancellationToken,
            string data, long chatId, int messageId)
        {
            try
            {
                long taskId = long.Parse(data.Split(' ')[1]);
                int preorityTask = int.Parse(data.Split(' ')[2]);
                long userId = long.Parse(data.Split(' ')[3]);
                int page = int.Parse(data.Split(' ')[4]);
                bool isForDelete = bool.Parse(data.Split(" ")[5]);

                await Delete.DeleteTask(taskId, cancellationToken);

                await DrowKeyboard.RedrowTasks(bot, update, chatId, userId,
                            messageId, cancellationToken, page, isForDelete, preorityTask);

            }
            catch (Exception e)
            {
                ServerConsoleWrite.ErrorWrite($"Ошибка при удалении задачи: {e.Message}");
                return;
            }
        }
    }
}
