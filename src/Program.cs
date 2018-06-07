namespace WhalesFargo
{
    public class Program
    {
        // Create a mutex for a single instance.
        private static System.Threading.Mutex INSTANCE_MUTEX = new System.Threading.Mutex(true, "WhalesFargo_DiscordBot");
        private static DiscordBot BOT = new DiscordBot();
        public static UI.Window UI = new UI.Window(BOT);
        static void Main(string[] args)
        {
            // Check if an instance is already running. Remove this block if you want to run multiple instances.
            if (!INSTANCE_MUTEX.WaitOne(System.TimeSpan.Zero, false))
            {
                System.Windows.Forms.MessageBox.Show("The applicaton is already running.");
                return;
            }
            // Start the UI.
            try { System.Windows.Forms.Application.Run(UI as System.Windows.Forms.Form); }
            catch { System.Console.WriteLine("Failed to run."); }
        }
        // Connect to the bot, or cancel before the connection happens.
        public static void Run() => System.Threading.Tasks.Task.Run(() => BOT.RunAsync());
        public static void Cancel() => System.Threading.Tasks.Task.Run(() => BOT.CancelAsync());
    }
}
