using PMGD_MarketPlace.Server.Data;
using PMGD_MarketPlace.Server.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PMGD_MarketPlace.Server {
    internal class ServerResponse {
        public static void UserHasJoined(int sender, Package package) {
            var senderIdCheck = package.ReadInt();
            var username = package.ReadString();

            Console.WriteLine($"[INFO] {Server.Clients[sender].Tcp.Socket.Client.RemoteEndPoint} {username} has join #{senderIdCheck}");

            //var repo = new Repository();
            //var model = new University() {
            //    Uid = 0,
            //    Name = "UII",
            //};
            //Task.Run(async () => await repo.InsertEntityAsync(model
            //var query = "SELECT * FROM University";
            //var data = Task.Run(async () => await repo.RetrivingEntityAsync<University>(query)).Result;
            //Console.WriteLine($"[INFO] Database connected...");
            //Console.WriteLine($"[INFO] Json : " + data);
        }

        public static void UserHasLeft(int fromClient, Package package) {
            ServerRequest.UserLeaving(fromClient);
        }

        public static void PostUniversity(int fromClient, Package package) {
            var protocol = (ServerProtocol)package.ReadInt();
            var len = package.ReadInt();
            var data = package.ReadBytes(len);

            var model = JsonSerializer.Deserialize<University>(data);
            Console.WriteLine($"[INFO] Request post University data from client {fromClient}");
            var repo = new Repository();
            //var state = false;
            Task? task = null;
            switch (protocol) {
                case ServerProtocol.UniversityPost:
                    task = repo.InsertEntityAsync(model);
                    break;
                case ServerProtocol.UniversityPut:
                    task = repo.UpdateEntityAsync(model);
                    break;
                case ServerProtocol.UniversityDel:
                    task = repo.DeleteEntityAsync(model);
                    break;
            }
            while (!task.IsCompleted) continue;
            //state = task.IsCompleted;

            //var query = "SELECT * FROM University";
            //var result = Task.Run(async () => await repo.RetrivingEntityAsync<University>(query)).Result;
            //Console.WriteLine($"[INFO] Data processing.........2");
            //ServerRequest.GetDataReturn(fromClient, (int)ServerProtocol.UniversityGet, result);
        }

        public static void GetUniversity(int fromClient, Package package) {
            var protocol = package.ReadInt();

            var repo = new Repository();
            var query = "SELECT * FROM University";
            var result = Task.Run(async () => await repo.RetrivingEntityAsync<University>(query)).Result;

            Console.WriteLine($"[INFO] Request University data from client {fromClient}");
            Console.WriteLine($"[INFO] Data processing...");

            ServerRequest.GetDataReturn(fromClient, protocol, result);
        }
    }
}
