using PMGD_MarketPlaceApp.Client.Models;

namespace PMGD_MarketPlaceApp.Client {
    public partial class App : Application {
        public const int TICKS_PER_SEC = 30;
        public const float MS_PER_TICK = 1000f / TICKS_PER_SEC;

        public App() {
            InitializeComponent();
            MainPage = new AppShell();

            isRunning = true;
            var mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            Services.Client.Instance = new();
            Services.Client.Instance.ConnectToServer();
        }

        private static bool isRunning = false;

        private static void MainThread() {
            Console.WriteLine($"Main thread started. Running at {TICKS_PER_SEC} ticks per second.");
            var nextLoop = DateTime.Now;

            while (isRunning) {
                while (nextLoop < DateTime.Now) {
                    Update();
                    nextLoop = nextLoop.AddMilliseconds(MS_PER_TICK);
                    if (nextLoop > DateTime.Now) {
                        Thread.Sleep(nextLoop - DateTime.Now);
                    }
                }
            }
        }

        private static void Update() {
            ThreadManager.UpdateMain();
        }
    }
}