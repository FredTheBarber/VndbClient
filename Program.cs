using System;
using System.Threading.Tasks;

namespace VndbClient
{
    class Program
    {
        static void Main(string[] args)
        {
            switch (args.Length)
            {
                case 0:
                    Run().Wait();
                    break;
                case 1:
                    Run(args[0]).Wait();
                    break;
                default:
                    Console.WriteLine("Usage:");
                    Console.WriteLine("    VndbClient [username]");
                    Console.WriteLine();
                    Console.WriteLine("    If username is not specified, anonymous login will be used and features like updating votes will not be available");
                    Console.WriteLine("    If username is specified, type password at password prompt and press enter when done");
                    Console.WriteLine();
                    break;
            }

        }

        private static async Task Run(string username = null)
        {
            Connection conn = new Connection();
            await conn.Open();
            Console.WriteLine("Connection opened");

            string password = null;
            if (username != null)
            {
                Console.WriteLine("Enter password:");
                password = ReadPasswordFromConsoleQuietly();
            }

            await conn.Login(username, password);
            if (username == null)
            {
                Console.WriteLine("Logged in anonymously. Type a command and press enter, or just press enter to terminate session.");
            }
            else
            {
                Console.WriteLine("Logged in as {0}. Type a command and press enter, or just press enter to terminate session.", username);
            }

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

        private static string ReadPasswordFromConsoleQuietly()
        {
            string password = string.Empty;
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else
                {
                    password += key.KeyChar;
                }
            }

            return password;
        }
    }
}
