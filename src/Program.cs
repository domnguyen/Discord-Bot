namespace WhalesFargo
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                new DiscordBot().RunAsync().GetAwaiter().GetResult();
            }
            catch
            {
                System.Console.WriteLine("Failed to run.");
            }
        }
    }
}
