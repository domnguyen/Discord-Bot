namespace WhalesFargo
{
    public class Program
    {
        // Create a mutex to only allow one instance of the app at a time.
        private static System.Threading.Mutex INSTANCE_MUTEX = new System.Threading.Mutex(true, "WhalesFargo_DiscordBot");
        private static DiscordBot BOT = new DiscordBot();
        public static UI.Window UI = new UI.Window(BOT);
        static void Main(string[] args)
        {
            // Check if an instance is already running.
            if (!INSTANCE_MUTEX.WaitOne(System.TimeSpan.Zero, false))
            {
                System.Windows.Forms.MessageBox.Show("The applicaton is already running.");
                return;
            }
            try { System.Windows.Forms.Application.Run(UI as System.Windows.Forms.Form); }
            catch { System.Console.WriteLine("Failed to run."); }
        }
        public static void Run() => System.Threading.Tasks.Task.Run(() => BOT.RunAsync());
        public static void Cancel() => System.Threading.Tasks.Task.Run(() => BOT.CancelAsync());
    }
}
