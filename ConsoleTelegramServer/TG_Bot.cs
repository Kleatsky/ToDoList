using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using static Telegram.Bot.TelegramBotClient;

namespace ConsoleTelegramServer
{
    internal class TG_Bot
    {
        private static string _filePath = @"Token.txt";//файл с токеном
        private static string? _token;
        public static async Task Run()
        {
            if (File.Exists(_filePath))
            {
                try
                {
                    var lines = File.ReadAllLines(_filePath);
                    _token = lines[0];
                }
                catch (Exception e)
                {
                    Console.WriteLine("В дериктории проекта должен быть файл Token.txt, " +
                        "в котором хранится токен телеграмм бота, ошибка: " + e.Message);
                    return;
                }
            }
            else//Если нет токена
            {
                return;
            }

            var cts = new CancellationTokenSource();
            var bot = new TelegramBotClient(_token);
            UpdateHandler updateHandler = new UpdateHandler();

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
