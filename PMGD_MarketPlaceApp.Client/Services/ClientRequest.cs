using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PMGD_MarketPlaceApp.Client.Services {
    internal class ClientRequest {
        public static void JoinSession() {
            using var package = new Package((int)ClientProtocol.Connected);
            package.Write(Client.Instance.Uid);
            package.Write("User MAUI");
            SendTCPData(package);
        }

        public static void LeftSession() {
            using var package = new Package((int)ClientProtocol.Disconnect);
        }

        public static void GetDataFromServer(ClientProtocol protocol) {
            using var package = new Package((int)protocol);
            package.Write((int)protocol);
            SendTCPData(package);
        }

        public static void PostDataToServer(ClientProtocol protocol, object model) {
            using var package = new Package((int)protocol);
            var bytedata = JsonSerializer.SerializeToUtf8Bytes(model);
            package.Write((int)protocol);
            package.Write(bytedata.Length);
            package.Write(bytedata);
            SendTCPData(package);
        }

        private static void SendTCPData(Package package) {
            package.WriteLength();
            Client.Instance.Tcp.SendData(package);
        }
    }
}
