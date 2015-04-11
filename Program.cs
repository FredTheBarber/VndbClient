using System;
using System.Threading.Tasks;

namespace VndbClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Run().Wait();
        }

        private static async Task Run()
        {
            Connection conn = new Connection();
            await conn.Open();
            Console.WriteLine("Connection opened");
            await conn.Login();
            Console.WriteLine("Logged in. Type a command and press enter, or just press enter to terminate session.");

            do
            {
                string line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    break;
                }

                await conn.Query(line);
            } while (true);
        }
    }
}
