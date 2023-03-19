using PMGD_MarketPlaceApp.Client.Models;
using PMGD_MarketPlaceApp.Client.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace PMGD_MarketPlaceApp.Client.Services {
    public class ClientResponse {
        public static List<University> Universities { get; set; } = new();

        public static void JoinToHost(Package package) {
            var msg = package.ReadString();
            var uid = package.ReadInt();

            Client.Instance.Uid = uid;
            Console.WriteLine($"Message from server : {msg}");
            ClientRequest.JoinSession();
            Client.Instance.Udp.Connect(((IPEndPoint)Client.Instance.Tcp.Socket.Client.LocalEndPoint).Port);
        }

        public static void LeftFromHost(Package package) {

        }

        public static void PostUniversity(Package package) {

        }

        public static void GetUniversity(Package package) {
            var data = package.ReadString();
            Universities = JsonSerializer.Deserialize<List<University>>(data);
        }
    }
}
