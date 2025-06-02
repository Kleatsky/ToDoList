using ConsoleTelegramServer.DB;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static Telegram.Bot.TelegramBotClient;

namespace ConsoleTelegramServer
{
    internal class TG_Bot
    {
        private static string _filePath = @"Token.txt";//файл с токеном
        private static string? _token;
        private static string _dbFilePathPassword = @"db.password";//файл с паролем от бд
        private static string? _dbpassword;
        public static async Task Run()
        {
            //Чтение токена из файла
            if (File.Exists(_filePath))
            {
                try
                {
                    var lines = File.ReadAllLines(_filePath);
                    _token = lines[0];
                }
                catch (Exception e)
                {
                    ServerConsoleWrite.ErrorWrite("В дериктории проекта должен быть файл Token.txt, " +
                        "в котором хранится токен телеграмм бота, ошибка: " + e.Message);
                    return;
                }
            }
            else//Если нет токена
            {
                ServerConsoleWrite.ErrorWrite("В дериктории проекта должен быть файл Token.txt, " +
                        "в котором хранится токен телеграмм бота.");
                return;
            }

            //Добавление конфигурации БД
            if (File.Exists(_filePath))
            {
                try
                {
                    var lines = File.ReadAllLines(_dbFilePathPassword);
                    _dbpassword = lines[0];
                }
                catch (Exception e)
                {
                    ServerConsoleWrite.ErrorWrite("В дериктории проекта должен быть файл db.password, " +
                        "в котором хранится пороль к базе данных." + e.Message);
                    return;
                }
            }
            else
            {
                ServerConsoleWrite.ErrorWrite("В дериктории проекта должен быть файл Tdb.password, " +
                        "в котором хранится пороль к базе данных.");
                return;
            }

            ConnectionString.connectionString = $"Host=localhost;Username=postgres;Password={_dbpassword};Database=ToDoList;Port=5432";

            var cts = new CancellationTokenSource();
            var bot = new TelegramBotClient(_token);
            UpdateHandler updateHandler = new UpdateHandler();


            //Добавление списка команд при начале ввода
            var commands = new List<BotCommand>(10);
            commands.Add(new BotCommand("/start", "Начать работу с ботом"));
            commands.Add(new BotCommand("/addtask", "Добавить новую задачу"));
            commands.Add(new BotCommand("/showtasks", "Показать список задач"));
            commands.Add(new BotCommand("/showtasksbytype", "Выбрать тип вывода задач (важно/срочно)"));
            commands.Add(new BotCommand("/deletetask", "Вывести список задачь на удаление"));
            commands.Add(new BotCommand("/deletetaskbytype", "Выбрать тип вывода задач на удаление (важно/срочно)"));

            await bot.SetMyCommands(
                commands, cancellationToken: cts.Token);

            //Добавление меню справа от ввода в ТГ
            await bot.SetChatMenuButton(
                menuButton: new MenuButtonCommands()
            );



            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()//любой тип данных на получение
            };

            bot.StartReceiving(
                updateHandler: updateHandler.HandleUpdateAsync,
                errorHandler: updateHandler.HandleErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            Console.ReadKey();
            await cts.CancelAsync();
        }
    }
}
