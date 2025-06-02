using ConsoleTelegramServer.Model;
using Dapper;
using Microsoft.VisualBasic;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;



namespace ConsoleTelegramServer
{
    public enum TaskType
    {
        UrgentImportant = 1,
        UrgentNotImportant = 2,
        NotUrgentImportant = 3,
        NotUrgentNotImportant = 4
    }

    internal class UpdateHandler : IUpdateHandler
    {

        public delegate void MessageHandler(object sender, string message);
        public enum Step
        {
            WaitingForType,
            WaitingForText,
            WaitingForDate
        }
        private readonly Dictionary<long, (Step currentStep, TodoListTask task)> UserAdding = new();
        public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update?.Message?.Type == MessageType.Text)
            {
                if (update.Message?.From == null)
                {
                    Console.WriteLine("Message is empty");
                    return;
                }

                long chatId = update.Message.Chat.Id;
                string? messageText = update.Message.Text;
                long userId = update.Message.From.Id;

                // Проверяем, находится ли пользователь в процессе добавления задачи
                if (await IsUserAdding(bot, update, cancellationToken, chatId))
                {
                    return;
                }


                switch (messageText)
                {
                    case "/start":
                        {
                            await UserInput.Start(bot, update, cancellationToken, chatId);
                        }
                        break;
                    case "/addtask":
                        {
                            await UserInput.AddTask(bot, update, cancellationToken, chatId, UserAdding);
                        }
                        break;
                    case "/showtasks":
                        {
                            await UserInput.ShowTasks(bot, update, cancellationToken, userId, chatId);
                        }
                        break;
                    case "/showtasksbytype":
                        {
                            await DrowKeyboard.ShowTasksByTypeKeyBoard(bot, chatId, userId, cancellationToken);
                        }
                        break;
                    case  "/deletetask":
                        {
                            await UserInput.DeleteTask(bot, update, cancellationToken, userId, chatId);
                        }
                        break;
                    case "/deletetaskbytype":
                        {
                            await DrowKeyboard.DeleteTaskByTypeKeyBoard(bot, chatId, userId, cancellationToken);
                        }
                        break;
                    default:
                        {
                            await bot.SendMessage(
                                chatId: chatId,
                                text: $"Команды: {messageText} не найдено.",
                                cancellationToken: cancellationToken
                            );
                        }
                        Console.WriteLine("There is no such command like \"" + messageText + '\"');
                        break;
                }

                Console.WriteLine($"Получено сообщение: {messageText}");
            }
            else if (update?.Type == UpdateType.CallbackQuery)
            {
                if (System.String.IsNullOrEmpty(update.CallbackQuery?.Message?.Text) && System.String.IsNullOrEmpty(update.CallbackQuery?.Data))
                {
                    Console.WriteLine("Message is empty.");
                    return;
                }
                var callbackQuery = update.CallbackQuery;
                var chatId = callbackQuery.Message!.Chat.Id;
                var messageId = callbackQuery.Message.MessageId; // ID сообщения для удаления
                var data = callbackQuery.Data;  // Данные, которые были переданы в кнопке

                switch (data)
                {
                    case "Hide keyboard":
                        {
                            await bot.DeleteMessage(chatId: chatId,
                                messageId: messageId,
                                cancellationToken: cancellationToken);
                            break;
                        }
                    case string tempstring when tempstring.StartsWith("CompliteTaskId "):
                        {
                            //Обработка клика по задаче
                            await CallBackHandler.CompliteTaskHandler(bot, update, cancellationToken, data, chatId, messageId);
                        }
                        break;
                    case string tempstring when tempstring.StartsWith("TaskTypeShow "):
                        {
                            //Обработка клика по клавиатуре с типом задач 
                            await CallBackHandler.MenuTypeHandler(bot, update, cancellationToken, data, chatId, messageId);
                        }
                        break;
                    case string tempstring when tempstring.StartsWith("DeleteTaskId "):
                        {
                            //Обработка нажатия на кнопку удаления
                            await CallBackHandler.DeleteHandler(bot, update, cancellationToken, data, chatId, messageId);
                        }
                        break;
                    case string tempstring when tempstring.StartsWith("PrevPage "):
                        {
                            //Обработка клика по пагенатору
                            await CallBackHandler.PrevPageHandler(bot, update, cancellationToken, data, chatId, messageId);
                        }
                        break;
                    case string tempstring when tempstring.StartsWith("NextPage "):
                        {
                            //Обработка клика по пагенатору
                            await CallBackHandler.NextPageHandler(bot, update, cancellationToken, data, chatId, messageId);
                        }
                        break;
                    default:
                        break;
                }
                ;
                return;
            }
        }

        private async Task<bool> IsUserAdding(ITelegramBotClient bot, Update update, CancellationToken cancellationToken,
                    long chatId)
        {
            if (UserAdding.TryGetValue(chatId, out var stepTask))
            {
                var returnedAdd = await Add.AddTask(bot, update, cancellationToken, stepTask.currentStep, stepTask.task);
                UserAdding[chatId] = returnedAdd.stepTask;//изменение шага добавления и задачи

                

                await bot.SendMessage(
                           chatId: chatId,
                           text: returnedAdd.message,
                           cancellationToken: cancellationToken
                       );

                if (returnedAdd.isComplite)
                {
                    //Удаляем добавление задачи, если ошибка или завершино добавление
                    UserAdding.Remove(chatId);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            Console.WriteLine(exception.Message);
            await Task.Delay(2000, cancellationToken);
        }
    }
}
