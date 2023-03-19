using System.Net.Sockets;
using System.Net;

namespace PMGD_MarketPlace.Server {
    public class Server : BackgroundService{ 
        public const int PORT = 26951;
        public const int MAX_USER = 50;
        public const int TICKS_PER_SEC = 30;
        public const float MS_PER_TICK = 1000f / TICKS_PER_SEC;

        public delegate void PackageHandler(int fromClient, Package package);

        public static ILogger<Server> Logger;
        public static Dictionary<int, Client> Clients = new Dictionary<int, Client>();
        public static Dictionary<int, PackageHandler> PackageList;

        public Server(ILogger<Server> logger) {
            Server.Logger = logger;
        }

        public static void Start() {
            Console.WriteLine($"[INFO] Starting server...");
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, PORT);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            udpListener = new UdpClient(PORT);
            udpListener.BeginReceive(UDPReceiveCallback, null);
            Console.WriteLine($"[INFO] Server started on port {PORT}.");
        }

        public static void SendUDPData(IPEndPoint clientEndPoint, Package package) {
            try {
                if (clientEndPoint != null) {
                    udpListener.BeginSend(package.ToArray(), package.Length(), clientEndPoint, null, null);
                }
            } catch (Exception ex) {
                Console.WriteLine($"[FAIL] Error sending data to {clientEndPoint} via UDP: {ex}");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            Console.Title = "Game Server";
            isRunning = true;

            var mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();
            Server.Start();
            //while (!stoppingToken.IsCancellationRequested) {
            //    Logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //    await Task.Delay(1000, stoppingToken);
            //}
        }

        private static TcpListener tcpListener;
        private static UdpClient udpListener;
        private static bool isRunning = false;

        private static void MainThread() {
            Console.WriteLine($"[INFO] Main thread started. Running at {TICKS_PER_SEC} ticks per second.");
            var nextLoop = DateTime.Now;

            while (isRunning) {
                while (nextLoop < DateTime.Now) {
                    ThreadManager.UpdateMain();
                    nextLoop = nextLoop.AddMilliseconds(Server.MS_PER_TICK);
                    if (nextLoop > DateTime.Now) {
                        Thread.Sleep(nextLoop - DateTime.Now);
                    }
                }
            }
        }

        private static void TCPConnectCallback(IAsyncResult result) {
            var client = tcpListener.EndAcceptTcpClient(result);
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            Console.WriteLine($"[WARN] Incoming connection from {client.Client.RemoteEndPoint}...");

            for (int i = 1; i <= MAX_USER; i++) {
                if (Clients[i].Tcp.Socket == null) {
                    Clients[i].Tcp.Connect(client);
                    return;
                }
            }
            Console.WriteLine($"[FAIL] {client.Client.RemoteEndPoint} failed to connect: Server full!");
        }

        private static void UDPReceiveCallback(IAsyncResult result) {
            try {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpListener.EndReceive(result, ref clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (data.Length < 4) {
                    return;
                }

                using var package = new Package(data);
                var clientId = package.ReadInt();

                if (clientId == 0) {
                    return;
                }

                if (Clients[clientId].Udp.EndPoint == null) {
                    Clients[clientId].Udp.Connect(clientEndPoint);
                    return;
                }

                if (Clients[clientId].Udp.EndPoint.ToString() == clientEndPoint.ToString()) {
                    Clients[clientId].Udp.HandleData(package);
                }
            } catch (Exception ex) {
                Console.WriteLine($"[FAIL] Error receiving UDP data: {ex}");
            }
        }

        private static void InitializeServerData() {
            for (int i = 1; i <= MAX_USER; i++) {
                Clients.Add(i, new Client(i));
            }

            PackageList = new Dictionary<int, PackageHandler>() {
                { (int)ClientProtocol.Connected, ServerResponse.UserHasJoined },
                { (int)ClientProtocol.Disconnect, ServerResponse.UserHasLeft },
                { (int)ClientProtocol.UniversityPost, ServerResponse.PostUniversity },
                { (int)ClientProtocol.UniversityPut, ServerResponse.PostUniversity },
                { (int)ClientProtocol.UniversityDel, ServerResponse.PostUniversity },
                { (int)ClientProtocol.UniversityGet, ServerResponse.GetUniversity },
            };
            Console.WriteLine($"[INFO] Initialized packets.");
        }
    }
}
