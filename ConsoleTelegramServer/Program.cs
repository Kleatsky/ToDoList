
namespace ConsoleTelegramServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await TG_Bot.Run();
            Console.WriteLine("Programm closed.");
        }
    }
}
