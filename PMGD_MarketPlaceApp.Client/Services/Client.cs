using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PMGD_MarketPlaceApp.Client.Services {
    internal class Client {
        public static int DataBufferSize = 4096;
        public int Uid;
        public string IP = "127.0.0.1";
        public int Port = 26951;
        public TCP Tcp;
        public UDP Udp;

        public Client() {
            Instance = this;
            Tcp = new();
            Udp = new();
        }

        public static Client Instance { get; set; }

        public class TCP {
            public TcpClient Socket;

            public void Connect() {
                Socket = new() {
                    ReceiveBufferSize = DataBufferSize,
                    SendBufferSize = DataBufferSize,
                };

                receiveBuffer = new byte[DataBufferSize];
                Socket.BeginConnect(Instance.IP, Instance.Port, ConnectCallback, Socket);
            }

            public void SendData(Package package) {
                try {
                    if (Socket != null) {
                        stream.BeginWrite(package.ToArray(), 0, package.Length(), null, null); // Send data to server
                    }
                } catch (Exception ex) {
                    Console.WriteLine($"[FAIL] Error sending data to server via TCP: {ex}");
                }
            }

            private NetworkStream stream;
            private Package receivedData;
            private byte[] receiveBuffer;

            private void ConnectCallback(IAsyncResult result) {
                Socket.EndConnect(result);
                if (!Socket.Connected) {
                    return;
                }
                stream = Socket.GetStream();
                receivedData = new();
                stream.BeginRead(receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
            }

            private void ReceiveCallback(IAsyncResult result) {
                try {
                    var byteLength = stream.EndRead(result);
                    if (byteLength <= 0) {
                        Instance.Disconnect();
                        return;
                    }
                    var data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);
                    receivedData.Reset(HandleData(data));
                    stream.BeginRead(receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
                } catch (Exception ex) {
                    Console.WriteLine($"[FAIL] Error receiving TCP data: {ex}");
                    Disconnect();
                }
            }

            private bool HandleData(byte[] data) {
                var packageLength = 0;
                
                receivedData.SetBytes(data);
                if (receivedData.UnreadLength() >= 4) {
                    packageLength = receivedData.ReadInt();
                    if (packageLength <= 0) {
                        return true;
                    }
                }

                while (packageLength > 0 && packageLength <= receivedData.UnreadLength()) {
                    var packageBytes = receivedData.ReadBytes(packageLength);
                    ThreadManager.ExecuteOnMainThread(() => {
                        using var package = new Package(packageBytes);
                        var packageId = package.ReadInt();
                        packageHandler[packageId](package);
                    });
                    packageLength = 0;
                    if (receivedData.UnreadLength() >= 4) {
                        packageLength = receivedData.ReadInt();
                        if (packageLength <= 0) {
                            return true;
                        }
                    }
                }

                if (packageLength <= 1) {
                    return true;
                }
                return false;
            }

            private void Disconnect() {
                Instance.Disconnect();
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                Socket = null;
            }
        }

        public class UDP {
            public UdpClient Socket;
            public IPEndPoint EndPoint;

            public UDP() {
                EndPoint = new IPEndPoint(IPAddress.Parse(Instance.IP), Instance.Port);
            }

            public void Connect(int localPort) {
                Socket = new UdpClient(localPort);

                Socket.Connect(EndPoint);
                Socket.BeginReceive(ReceiveCallback, null);

                using var package = new Package();
                SendData(package);
            }

            public void SendData(Package package) {
                try {
                    package.InsertInt(Instance.Uid); // Insert the client's ID at the start of the packet
                    Socket?.BeginSend(package.ToArray(), package.Length(), null, null);
                } catch (Exception ex) {
                    Console.WriteLine($"[FAIL] Error sending data to server via UDP: {ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult result) {
                try {
                    var data = Socket.EndReceive(result, ref EndPoint);
                    Socket.BeginReceive(ReceiveCallback, null);

                    if (data.Length < 4) {
                        Instance.Disconnect();
                        return;
                    }
                    HandleData(data);
                } catch (Exception ex) {
                    Console.WriteLine($"[FAIL] Error receiving UDP data : {ex}");
                    Disconnect();
                }
            }

            private void HandleData(byte[] data) {
                using (var package = new Package(data)) {
                    var packetLength = package.ReadInt();
                    data = package.ReadBytes(packetLength);
                }

                ThreadManager.ExecuteOnMainThread(() => {
                    using var package = new Package(data);
                    var packetId = package.ReadInt();
                    packageHandler[packetId](package);
                });
            }

            private void Disconnect() {
                Instance.Disconnect();
                EndPoint = null;
                Socket = null;
            }
        }

        public void ConnectToServer() {
            InitializeClientData();
            IsConnected = true;
            Tcp.Connect();
        }

        private delegate void PackageHandler(Package package);
        private static Dictionary<int, PackageHandler> packageHandler;
        public bool IsConnected;

        private void InitializeClientData() {
            packageHandler = new() {
                { (int)ServerProtocol.Connected, ClientResponse.JoinToHost },
                { (int)ServerProtocol.Disconnect, ClientResponse.LeftFromHost },
                { (int)ServerProtocol.UniversityGet, ClientResponse.GetUniversity },
            };
            Console.WriteLine("[INFO] Initialized package");
        }

        private void Disconnect() {
            if (IsConnected) {
                ClientRequest.LeftSession();
                try {
                    IsConnected = false;
                    Tcp.Socket.Close();
                    Udp.Socket.Close();
                    Console.WriteLine($"[WARN] Disconnected from server.");
                } catch (Exception) {
                    Console.WriteLine($"[FAIL] Error Disconnect");
                    return;
                }
            }
        }
    }
}
