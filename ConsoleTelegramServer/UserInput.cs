using ConsoleTelegramServer.Model;
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
    internal class UserInput
    {
        public static async Task Start(ITelegramBotClient bot, Update update, CancellationToken cancellationToken, long chatId)
        {
            string answerStart = $"Это ToDoList бот, вот список команд:{Environment.NewLine}" +
                            $"/addtask для добавления задачи.{Environment.NewLine}" +
                            $"/showtasks для отображения списка ваших задач.{Environment.NewLine}" +
                            $"/showtasksbytype для выбора типа вывода задач (важно/срочно){Environment.NewLine}" +
                            $"/deletetask для вывода списока задачь на удаление{Environment.NewLine}" +
                            $"/deletetaskbytype для выбора типа вывода задач на удаление (важно/срочно)";
            await bot.SendMessage(
               chatId: chatId,
               text: answerStart,
               cancellationToken: cancellationToken
           );

            ServerConsoleWrite.SimpleWrite($"/start input from {update?.Message?.From?.Id}");
        }
        public static async Task ShowTasks(ITelegramBotClient bot, Update update, CancellationToken cancellationToken,
            long userId, long chatId)
        {
            List<TodoListTask> todoListTasks = await Show.ShowTasks(userId, cancellationToken);
            if (todoListTasks != null && todoListTasks.Count > 0)
            {
                int startingPage = 1;
                int preority = 0;
                bool isForDelete = false;
                await DrowKeyboard.DrowTasks(bot, update, cancellationToken, todoListTasks, chatId, userId, 
                    startingPage, isForDelete, preority);
            }
            else
            {
                await bot.SendMessage(
                    chatId: chatId,
                    text: $"Списка задач не найдено.",
                    cancellationToken: cancellationToken
                );
            }

            ServerConsoleWrite.SimpleWrite($"/showtasks input from {update?.Message?.From?.Id}");
        }
        public static async Task AddTask(ITelegramBotClient bot, Update update, CancellationToken cancellationToken,
            long chatId, Dictionary<long, (Step currentStep, TodoListTask task)> UserAdding)
        {
            UserAdding.Add(chatId, (Step.WaitingForType, new TodoListTask()));
            UserAdding.TryGetValue(chatId, out var tempStepTask);
            string answer = $"Введите тип задачи:{Environment.NewLine}" +
                $"1 - срочная/важная{Environment.NewLine}" +
                $"2 - срочная/не важная{Environment.NewLine}" +
                $"3 - не срочная/важная{Environment.NewLine}" +
                $"4 - не срочная/не важная";
            await bot.SendMessage(
                   chatId: chatId,
                   text: answer,
                   cancellationToken: cancellationToken
               );

             ServerConsoleWrite.SimpleWrite($"/addtask input from {update?.Message?.From?.Id}");
        }

        internal static async Task DeleteTask(ITelegramBotClient bot, Update update, CancellationToken cancellationToken,
            long userId, long chatId)
        {
            List<TodoListTask> todoListTasks = await Show.ShowTasks(userId, cancellationToken);
            if (todoListTasks != null && todoListTasks.Count > 0)
            {
                int startingPage = 1;
                int preority = 0;
                bool isForDelete = true;
                await DrowKeyboard.DrowTasks(bot, update, cancellationToken, todoListTasks, chatId, userId, startingPage, isForDelete, preority);
            }
            else
            {
                await bot.SendMessage(
                    chatId: chatId,
                    text: $"Списка задач не найдено.",
                    cancellationToken: cancellationToken
                );
            }

            ServerConsoleWrite.SimpleWrite($"/showtasks input from {update?.Message?.From?.Id}");
        }
    }
}
