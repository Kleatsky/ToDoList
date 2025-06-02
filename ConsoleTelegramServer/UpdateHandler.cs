using ConsoleTelegramServer.Model;
using Dapper;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
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
        private static readonly Dictionary<TaskType, string> TypeEmojiMap = new()
        {
            [TaskType.UrgentImportant] = "🔴",
            [TaskType.UrgentNotImportant] = "🟠",
            [TaskType.NotUrgentImportant] = "🟡",
            [TaskType.NotUrgentNotImportant] = "🔵"
        };
        public delegate void MessageHandler(object sender, string message);
        //public event MessageHandler OnHandleUpdateStarted;
        //public event MessageHandler OnHandleUpdateCompleted;
        public enum Step
        {
            WaitingForType,
            WaitingForText,
            WaitingForDate
        }
        private static readonly Dictionary<long, (Step currentStep, TodoListTask task)> UserAdding = new();
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
                        //Удаляем добавление, если ошибка или завершино добавление
                        UserAdding.Remove(chatId);
                    }
                    return;
                }

                switch (messageText)
                {
                    case "/start":
                        {
                            string answerStart = $"Это ToDoList бот, вот список команд:{Environment.NewLine}" +
                            $"/addtask для добавления задачи.{Environment.NewLine}" +
                            $"/showtasks для отображения списка ваших задач.";
                            await bot.SendMessage(
                               chatId: chatId,
                               text: answerStart,
                               cancellationToken: cancellationToken
                           );
                        }
                        Console.WriteLine("/start get name from user.");//Сделать внятное описание
                        break;
                    case "/addtask":
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
                        Console.WriteLine("/addtask ask you to add new task to ToDoList.");
                        break;
                    case "/showtasks":
                        List<TodoListTask> todoListTasks = await Show.ShowTasks(userId, cancellationToken);//Добавить from to
                        if (todoListTasks != null && todoListTasks.Count > 0)
                        {
                            await DrowTasks(bot, update, cancellationToken, todoListTasks, chatId, 0);
                        }
                        else
                        {
                            await bot.SendMessage(
                                chatId: chatId,
                                text: $"Списка задач не найдено.",
                                cancellationToken: cancellationToken
                            );
                        }

                        Console.WriteLine("/showtasks ask you to add new task to ToDoList.");
                        break;
                    case "/showtasksByType":
                        {
                            await ShowTasksByTypeKeyBoard(bot, chatId, userId, cancellationToken);
                            Console.WriteLine("/showtasksByType — показываем меню выбора типа задач.");
                        }
                        break;
                    default:
                        {
                            // Ответ на сообщение
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
                if (String.IsNullOrEmpty(update.CallbackQuery?.Message?.Text) && String.IsNullOrEmpty(update.CallbackQuery?.Data))
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

                            try
                            {
                                long taskId = long.Parse(data.Split(' ')[1]);
                                int preorityTask = int.Parse(data.Split(' ')[2]);
                                long userId = await UpdateTask.IsComplited(taskId);

                                if (userId == 0)
                                {
                                    Console.WriteLine("Id пользователя или id задачи в бд не найден.");
                                    return;
                                }
                                {
                                    await RedrowTasksWithChangeIsComplited(bot, update, chatId, taskId, userId,
                                            messageId, cancellationToken, preorityTask);
                                }

                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                                break;
                            }
                        }
                        break;
                    case string tempstring when tempstring.StartsWith("TaskTypeShow "):
                        {
                            //Обработка клика по клавиатуре с типом задач 
                            try
                            {
                                if (!Enum.TryParse<TaskType>(data.Split(' ')[1], out TaskType taskType))
                                {
                                    break;
                                }
                                long userId = long.Parse(data.Split(' ')[2]);

                                //Удаляем текущую клавиатуру
                                await bot.DeleteMessage(chatId: chatId,
                                    messageId: messageId,
                                    cancellationToken: cancellationToken);

                                //Получение данных по типу и отрисовка
                                List<TodoListTask> todoListTasks = await Show.ShowTasksWithType(userId, taskType, cancellationToken);//Добавить from to
                                if (todoListTasks != null && todoListTasks.Count > 0)
                                {
                                    await DrowTasks(bot, update, cancellationToken, todoListTasks, chatId, (int)taskType);
                                }
                                else
                                {
                                    await bot.SendMessage(
                                        chatId: chatId,
                                        text: $"Списка задач не найдено.",
                                        cancellationToken: cancellationToken
                                    );
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"Ошибка вызова меню: {e.Message}");
                                break;
                            }
                        }
                        break;
                    case string tempstring when tempstring.StartsWith("DeleteTaskId "):
                        {
                            //Обработка нажатия на кнопку удаления

                            try
                            {
                                long taskId = long.Parse(data.Split(' ')[1]);
                                int preorityTask = int.Parse(data.Split(' ')[2]);
                                long userId = long.Parse(data.Split(' ')[3]);

                                await Delete.DeleteTask(taskId, cancellationToken);

                                await RedrowTasksWithChangeIsComplited(bot, update, chatId, taskId, userId,
                                            messageId, cancellationToken, preorityTask);

                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"Ошибка при удалении задачи: {e.Message}");
                                break;
                            }
                        }
                        break;
                    default:
                        break;
                }
                ;

                return;
            }
        }

        private async Task ShowTasksByTypeKeyBoard(ITelegramBotClient bot, long chatId, long userId, CancellationToken cancellationToken)
        {
            string emojiDefault;
            string UrgentImportantEmoji = TypeEmojiMap.TryGetValue(TaskType.UrgentImportant, out emojiDefault!) ? emojiDefault : "⚪";
            string UrgentNotImportantEmoji = TypeEmojiMap.TryGetValue(TaskType.UrgentNotImportant, out emojiDefault!) ? emojiDefault : "⚪";
            string NotUrgentImportantEmoji = TypeEmojiMap.TryGetValue(TaskType.NotUrgentImportant, out emojiDefault!) ? emojiDefault : "⚪";
            string NotUrgentNotImportantEmoji = TypeEmojiMap.TryGetValue(TaskType.NotUrgentNotImportant, out emojiDefault!) ? emojiDefault : "⚪";

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(UrgentImportantEmoji + "Срочные и важные",$"TaskTypeShow {1} {userId}"),
                        InlineKeyboardButton.WithCallbackData(UrgentNotImportantEmoji + "Срочные и не важные",$"TaskTypeShow {2} {userId}")
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(NotUrgentImportantEmoji + "Не срочные и важные",$"TaskTypeShow {3} {userId}"),
                        InlineKeyboardButton.WithCallbackData(NotUrgentNotImportantEmoji + "Не срочные и не важные",$"TaskTypeShow {4} {userId}")
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Скрыть клаву","Hide keyboard")
                    }
                });

            await bot.SendMessage(
                chatId: chatId,
                text: "Выберите тип задачи:",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken
            );
        }

        private async Task RedrowTasksWithChangeIsComplited(ITelegramBotClient bot, Update update,
                    long chatId, long taskId, long userId, int messageIdPreviousKeyboard, CancellationToken cancellationToken, int preorityTask)
        {
            List<TodoListTask> todoListTasks;
            //Получаем список задач
            if (preorityTask == 0)
            {
                todoListTasks = await Show.ShowTasks(userId, cancellationToken);//Добавить from to
            }
            else
            {
                todoListTasks = await Show.ShowTasksWithType(userId, (TaskType)preorityTask, cancellationToken);
            }

            //Удаляем прежнюю клавиатуру
            await bot.DeleteMessage(chatId: chatId,
                            messageId: messageIdPreviousKeyboard,
                            cancellationToken: cancellationToken);

            //Перерисовывка клавиатуры
            await DrowTasks(bot, update, cancellationToken, todoListTasks, chatId, preorityTask);
        }
        private static async Task DrowTasks(ITelegramBotClient bot, Update update, CancellationToken cancellationToken,
            List<TodoListTask> todoListTasks, long chatId, long userId, int prioretyTask = 0)
        {
            var buttons = new List<InlineKeyboardButton>();
            List<InlineKeyboardButton[]> keys = new List<InlineKeyboardButton[]>();

            //Заполняем кнопки текстом
            foreach (TodoListTask task in todoListTasks)
            {
                string status = task.IsCompleted ? "✅" : "❏";
                string typeEmoji = TypeEmojiMap.TryGetValue((TaskType)task.Priority, out var emoji) ? emoji : "⚪";

                string keyText = $"{status} {typeEmoji}: {task.TextTask} : {task.DateTask:dd-MM-yyyy}";

                //Кнопка удаления задачи
                var deleteKey = InlineKeyboardButton.WithCallbackData("🗑️", $"DeleteTaskId {task.Id} {prioretyTask} {userId}");

                var taskKey = InlineKeyboardButton.WithCallbackData(keyText, $"CompliteTaskId {task.Id} {prioretyTask}");

                keys.Add(new[] { deleteKey, taskKey });
            }

            // Добавляем кнопку Скрыть клаву
            keys.Add(new[] { InlineKeyboardButton.WithCallbackData("Скрыть клаву", "Hide keyboard") });

            var inlineKeyboard = new InlineKeyboardMarkup(keys);

            await bot.SendMessage(
                chatId: chatId,
                text: "Список задач:",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }
        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            Console.WriteLine(exception.Message);
            await Task.Delay(2000, cancellationToken);
        }
    }
}
