using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PMGD_MarketPlace.Server {
    internal class ServerRequest {
        public static void UserConnecting(int idClient, string msg) {
            using var package = new Package((int)ServerProtocol.Connected);
            package.Write(msg);
            package.Write(idClient);
            SendTCPData(idClient, package);
        }

        public static void UserLeaving(int idClient) {
            using var package = new Package((int)ServerProtocol.Disconnect);
            package.Write(idClient);
            SendUDPDataToAll(idClient, package);
        }

        public static void GetDataReturn(int idClient, int protocol, string data) {
            using var package = new Package(protocol);

            Console.WriteLine($"[INFO] Data returned : {data}");
            Console.WriteLine($"[INFO] Data sent...");

            package.Write(data);
            SendTCPData(idClient, package);
        }

        #region Communicator Sender
        private static void SendTCPData(int idClient, Package package) {
            package.WriteLength();
            Server.Clients[idClient].Tcp.SendData(package);
        }

        private static void SendUDPData(int idClient, Package package) {
            package.WriteLength();
            Server.Clients[idClient].Udp.SendData(package);
        }

        private static void SendTCPDataToAll(Package package) {
            package.WriteLength();
            for (int i = 1; i <= Server.MAX_USER; i++) {
                Server.Clients[i].Tcp.SendData(package);
            }
        }

        private static void SendTCPDataToAll(int exceptClient, Package package) {
            package.WriteLength();
            for (int i = 1; i <= Server.MAX_USER; i++) {
                if (i != exceptClient) {
                    Server.Clients[i].Tcp.SendData(package);
                }
            }
        }

        private static void SendUDPDataToAll(Package package) {
            package.WriteLength();
            for (int i = 1; i <= Server.MAX_USER; i++) {
                Server.Clients[i].Udp.SendData(package);
            }
        }

        private static void SendUDPDataToAll(int exceptClient, Package package) {
            package.WriteLength();
            for (int i = 1; i <= Server.MAX_USER; i++) {
                if (i != exceptClient) {
                    Server.Clients[i].Udp.SendData(package);
                }
            }
        }
        #endregion
    }
}
