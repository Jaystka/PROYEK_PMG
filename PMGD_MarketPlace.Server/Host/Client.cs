using System.Net;
using System.Net.Sockets;

namespace PMGD_MarketPlace.Server {
    public class Client {
        public static int DataBufferSize = 4096;

        public int Id;
        public TCP Tcp;
        public UDP Udp;

        public Client(int clientId) {
            Id = clientId;
            Tcp = new TCP(Id);
            Udp = new UDP(Id);
        }

        public class TCP {
            public TcpClient Socket;

            public TCP(int id) {
                this.id = id;
            }

            public void Connect(TcpClient socket) {
                Socket = socket;
                Socket.ReceiveBufferSize = DataBufferSize;
                Socket.SendBufferSize = DataBufferSize;
                stream = Socket.GetStream();

                receivedData = new Package();
                receiveBuffer = new byte[DataBufferSize];
                stream.BeginRead(receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);

                ServerRequest.UserConnecting(id, "Welcome to the server!");
            }

            public void SendData(Package package) {
                try {
                    if (Socket != null) {
                        stream.BeginWrite(package.ToArray(), 0, package.Length(), null, null);
                    }
                } catch (Exception ex) {
                    Console.WriteLine($"[FAIL] Error sending data to player {id} via TCP: {ex}");
                }
            }

            public void Disconnect() {
                Socket.Close();
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                Socket = null;
            }

            private readonly int id;
            private NetworkStream stream;
            private Package receivedData;
            private byte[] receiveBuffer;

            private void ReceiveCallback(IAsyncResult result) {
                try {
                    var byteLength = stream.EndRead(result);
                    if (byteLength <= 0) {
                        Server.Clients[id].Disconnect();
                        return;
                    }
                    var data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);
                    receivedData.Reset(HandleData(data));
                    stream.BeginRead(receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
                } catch (Exception ex) {
                    //Console.WriteLine($"[FAIL] Error receiving TCP data: {ex}");
                    Server.Clients[id].Disconnect();
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
                        Server.PackageList[packageId](id, package);
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
        }

        public class UDP {
            public IPEndPoint EndPoint;

            public UDP(int id) {
                this.id = id;
            }

            public void Connect(IPEndPoint endPoint) {
                EndPoint = endPoint;
            }

            public void SendData(Package package) {
                Server.SendUDPData(EndPoint, package);
            }

            public void HandleData(Package packageData) {
                var packageLength = packageData.ReadInt();
                var packageBytes = packageData.ReadBytes(packageLength);

                ThreadManager.ExecuteOnMainThread(() => {
                    using var package = new Package(packageBytes);
                    try {
                        var packageId = package.ReadInt();
                        Server.PackageList[packageId](id, package);
                    } catch (Exception) {
                        return;
                    }
                });
            }

            public void Disconnect() {
                EndPoint = null;
            }

            private readonly int id;
        }

        private void Disconnect() {
            Console.WriteLine($"[WARN] {Tcp.Socket.Client.RemoteEndPoint} has disconnected.");
            Tcp.Disconnect();
            Udp.Disconnect();
        }
    }
}
