using ConsoleTelegramServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ConsoleTelegramServer
{
    internal class DrowKeyboard
    {
        public static readonly Dictionary<TaskType, string> TypeEmojiMap = new()
        {
            [TaskType.UrgentImportant] = "🔴",
            [TaskType.UrgentNotImportant] = "🟠",
            [TaskType.NotUrgentImportant] = "🟡",
            [TaskType.NotUrgentNotImportant] = "🔵"
        };
        public static async Task DrowTasks(ITelegramBotClient bot, Update update, CancellationToken cancellationToken,
            List<TodoListTask> todoListTasks, long chatId, long userId, int page, bool isForDelete, int prioretyTask = 0)
        {
            List<InlineKeyboardButton[]> keys = new List<InlineKeyboardButton[]>();

            // Вычисляем диапазон задач для текущей страницы
            int tasksPerPage = 10;

            int tasksCount = todoListTasks.Count;
            int totalPages = (int)Math.Ceiling((double)tasksCount / tasksPerPage);

            int startIdx = (page - 1) * tasksPerPage;
            int endIdx = Math.Min(startIdx + tasksPerPage, tasksCount);

            //Заполняем кнопки текстом
            for (int i = startIdx; i < endIdx; i++)
            {
                var task = todoListTasks[i];

                string status = task.IsCompleted ? "✅" : "❏";
                string typeEmoji = TypeEmojiMap.TryGetValue((TaskType)task.Priority, out var emoji) ? emoji : "⚪";

                string keyText = $"{status} {typeEmoji}: {task.TextTask} : {task.DateTask:dd-MM-yy}";

                InlineKeyboardButton key;

                //Кнопка для выполнения или удаления задачи
                if (isForDelete)
                {
                    key = InlineKeyboardButton.WithCallbackData("🗑️ "+ keyText, $"DeleteTaskId {task.Id} " +
                        $"{prioretyTask} {userId} {page} {isForDelete}");
                }
                else
                {
                    key = InlineKeyboardButton.WithCallbackData(keyText, $"CompliteTaskId {task.Id} " +
                        $"{prioretyTask} {userId} {page} {isForDelete}");
                }

                keys.Add(new[] { key });
            }

            // Кнопки пагинации
            var paginationButtons = new List<InlineKeyboardButton>();

            if (page > 1)
            {
                paginationButtons.Add(InlineKeyboardButton.WithCallbackData("⬅️ Назад", $"PrevPage " +
                    $"{prioretyTask} {userId} {page} {isForDelete}"));
            }
            if (endIdx < tasksCount)
            {
                paginationButtons.Add(InlineKeyboardButton.WithCallbackData("➡️ Вперёд", $"NextPage " +
                    $"{prioretyTask} {userId} {page} {isForDelete}"));
            }
            if (paginationButtons.Count > 0)
            {
                keys.Add(paginationButtons.ToArray());
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

        public static async Task ShowTasksByTypeKeyBoard(ITelegramBotClient bot, long chatId, long userId, CancellationToken cancellationToken)
        {
            string emojiDefault;
            string UrgentImportantEmoji = TypeEmojiMap.TryGetValue(TaskType.UrgentImportant, out emojiDefault!) ? emojiDefault : "⚪";
            string UrgentNotImportantEmoji = TypeEmojiMap.TryGetValue(TaskType.UrgentNotImportant, out emojiDefault!) ? emojiDefault : "⚪";
            string NotUrgentImportantEmoji = TypeEmojiMap.TryGetValue(TaskType.NotUrgentImportant, out emojiDefault!) ? emojiDefault : "⚪";
            string NotUrgentNotImportantEmoji = TypeEmojiMap.TryGetValue(TaskType.NotUrgentNotImportant, out emojiDefault!) ? emojiDefault : "⚪";

            bool isForDelete = false;

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(UrgentImportantEmoji + "Срочные и важные",$"TaskTypeShow " +
                        $"{1} {userId} {isForDelete}"),
                        InlineKeyboardButton.WithCallbackData(UrgentNotImportantEmoji + "Срочные и не важные",$"TaskTypeShow " +
                        $"{2} {userId} {isForDelete}")
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(NotUrgentImportantEmoji + "Не срочные и важные",$"TaskTypeShow " +
                        $"{3} {userId} {isForDelete}"),
                        InlineKeyboardButton.WithCallbackData(NotUrgentNotImportantEmoji + "Не срочные и не важные",$"TaskTypeShow " +
                        $"{4} {userId} {isForDelete}")
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

            //LOG
            ServerConsoleWrite.SimpleWrite("/showtasksByType — показываем меню выбора типа задач.");
        }
        public static async Task DeleteTaskByTypeKeyBoard(ITelegramBotClient bot, long chatId, long userId, CancellationToken cancellationToken)
        {
            string emojiDefault;
            string UrgentImportantEmoji = TypeEmojiMap.TryGetValue(TaskType.UrgentImportant, out emojiDefault!) ? emojiDefault : "⚪";
            string UrgentNotImportantEmoji = TypeEmojiMap.TryGetValue(TaskType.UrgentNotImportant, out emojiDefault!) ? emojiDefault : "⚪";
            string NotUrgentImportantEmoji = TypeEmojiMap.TryGetValue(TaskType.NotUrgentImportant, out emojiDefault!) ? emojiDefault : "⚪";
            string NotUrgentNotImportantEmoji = TypeEmojiMap.TryGetValue(TaskType.NotUrgentNotImportant, out emojiDefault!) ? emojiDefault : "⚪";

            bool isForDelete = true;

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(UrgentImportantEmoji + "Срочные и важные",$"TaskTypeShow " +
                        $"{1} {userId} {isForDelete}"),
                        InlineKeyboardButton.WithCallbackData(UrgentNotImportantEmoji + "Срочные и не важные",$"TaskTypeShow " +
                        $"{2} {userId} {isForDelete}")
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(NotUrgentImportantEmoji + "Не срочные и важные",$"TaskTypeShow " +
                        $"{3} {userId} {isForDelete}"),
                        InlineKeyboardButton.WithCallbackData(NotUrgentNotImportantEmoji + "Не срочные и не важные",$"TaskTypeShow " +
                        $"{4} {userId} {isForDelete}")
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

            //LOG
            ServerConsoleWrite.SimpleWrite("/showtasksByType — показываем меню выбора типа задач.");
        }
        public static async Task RedrowTasks(ITelegramBotClient bot, Update update,
                    long chatId, long userId, int messageIdPreviousKeyboard, CancellationToken cancellationToken,
                    int page, bool isForDelete, int preorityTask)
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
            await DrowKeyboard.DrowTasks(bot, update, cancellationToken, todoListTasks, chatId, userId, page, isForDelete, preorityTask);
        }
    }
}
