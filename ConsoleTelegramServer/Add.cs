using ConsoleTelegramServer.DB;
using ConsoleTelegramServer.Model;
using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using static ConsoleTelegramServer.UpdateHandler;

namespace ConsoleTelegramServer
{
    internal class Add
    {
        public static async Task<(bool isComplite, (Step, TodoListTask) stepTask, string message)> AddTask(ITelegramBotClient bot,
                            Update update, CancellationToken cancellationToken, Step step, TodoListTask task)
        {

            var message = update.Message;
            var chatId = message!.Chat.Id;
            string returnMessage = string.Empty;
            bool isComplite = false;

            switch (step)
            {
                case Step.WaitingForType:
                    {
                        int type;
                        if (int.TryParse(message.Text, out type) && type > 0 && type < 5)
                        {
                            task.Priority = type;
                            returnMessage = "Напишите задачу:";//Ответ пользователю
                            isComplite = false;//Продолжаем добавлять
                            step = Step.WaitingForText;//Следующий шаг
                        }
                        else
                        {
                            returnMessage = "Неверный тип задачи!";
                            isComplite = true;
                            ServerConsoleWrite.ErrorWrite(returnMessage);
                        }
                    }
                    break;

                case Step.WaitingForText:
                    if (!string.IsNullOrWhiteSpace(message.Text))
                    {
                        task.TextTask = message.Text;
                        returnMessage = "Введите срок выполнения задачи (dd-MM-yyyy):";
                        isComplite = false;
                        step = Step.WaitingForDate;
                    }
                    else
                    {
                        returnMessage = "Ошибка ввода текста задачи!";
                        isComplite = true;
                        ServerConsoleWrite.ErrorWrite(returnMessage);
                    }
                    break;

                case Step.WaitingForDate:
                    if (DateTime.TryParseExact(message.Text, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime date))
                    {
                        task.DateTask = date;
                        task.TelegramUserId = update!.Message!.From!.Id;

                        //Добавления в БД
                        await AddToDB.SaveTaskAsync(task);

                        returnMessage = $"Задача добавлена:{Environment.NewLine}" +
                            $"Тип: {task.Priority}{Environment.NewLine}" +
                            $"Текст: {task.TextTask}{Environment.NewLine}" +
                            $"Срок: {task.DateTask:dd-MM-yyyy}";
                        isComplite = true;
                    }
                    else
                    {
                        returnMessage = "Неверный формат даты. Используйте dd-MM-yyyy.";
                        isComplite = true;
                        ServerConsoleWrite.ErrorWrite(returnMessage);
                    }
                    break;
            }
            (Step, TodoListTask) stepTask = (step, task);
            return (isComplite, stepTask, returnMessage);
        }
    }
}
