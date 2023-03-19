using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text.Json;
using System.Text;
using System.Reflection;
using PMGD_MarketPlace.Server.Models;

namespace PMGD_MarketPlace.Server.Data{
    public class Repository {
        public SqlConnection SqlConnect;
        public string DbServer;
        public string DbSource;
        public string SqlNotice;

        public Repository() {
            SqlConnect = new SqlConnection();
            DbServer = @"DESKTOP-USP5OHI\BATTLECRYSQL"; // local
            //DbServer = @"WIN-L5PHR2FRN4D\SQLEXPRESS";
            DbSource = "PMGD_MarketPlaceDatabase";
            isbusy = false;
        }

        public Repository(string dbserver, string dbsource) : this() {
            this.DbServer = dbserver;
            this.DbSource = dbsource;
        }

        public async Task<bool> OpenConnectionAsync() {
            return await Task.FromResult(OpenConnection());
        }

        public async Task<bool> CloseConnectionAsync() {
            return await Task.FromResult(CloseConnection());
        }

        public async Task<string> RetrivingEntityAsync<T>(string query) where T : new() {
            var json = JsonSerializer.Serialize(RetrivingEntity<T>(query));
            return await Task.FromResult(json.Normalize());
        }

        public async Task<T> ModelEntityAsync<T>(string query) where T : new() {
            var list = await Task.FromResult(RetrivingEntity<T>(query));
            return list[0];
        }

        public async Task InsertEntityAsync(object model) {
            await Task.Factory.StartNew(InsertEntity, model);
        }

        public async Task UpdateEntityAsync(object model, string? atrb = null) {
            await Task.Factory.StartNew(() => {
                UpdateEntity(model, atrb);
            });
        }

        public async Task DeleteEntityAsync(object model) {
            await Task.Factory.StartNew(DeleteEntity, model);
        }

        private bool isbusy;

        private List<T> RetrivingEntity<T>(string query) where T : new() {
            var list = new List<T>();
            while (!isbusy) {
                OpenConnection();
                isbusy = true;
                var result = new SqlCommand(query, SqlConnect).ExecuteReader();
                while (result.Read() && result.HasRows) {
                    var item = Activator.CreateInstance<T>();
                    foreach (var info in typeof(T).GetProperties()) {
                        Type t = Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType;
                        if (!t.Namespace.Equals("System")) {
                            var sub = OrdinalRecursive(result, info);
                            info.SetValue(item, Convert.ChangeType(sub, info.PropertyType), null);
                        } else {
                            if (!result.IsDBNull(result.GetOrdinal(info.Name))) {
                                info.SetValue(item, Convert.ChangeType(result[info.Name], t), null);
                            }
                        }
                    }
                    list.Add(item);
                }
                isbusy = false;
                CloseConnection();
                break;
            }
            return list;
        }

        private void InsertEntity(object model) {
            while (!isbusy) {
                var head = string.Empty;
                var body = string.Empty;
                for (int i = 1; i < model.GetType().GetProperties().Length; i++) {
                    var info = model.GetType().GetProperties()[i];
                    var t = Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType;
                    if (t.Namespace != "System") {
                        object sub = info.GetValue(model);
                        head += $", id_{info.Name.ToLower()}";
                        body += $", '{sub.GetType().GetProperties()[0].GetValue(sub)}'";
                    } else {
                        head += $", {info.Name.ToLower()}";
                        if (info.PropertyType.ToString().Equals("System.DateTime")) {
                            body += $", '{Convert.ToDateTime(info.GetValue(model)).ToString("yyyy/MM/dd")}'";
                        } else {
                            body += $", '{info.GetValue(model)}'";
                        }
                    }
                }
                var table = model.GetType().Name;
                var query = $"INSERT INTO {table}({head.Substring(2)}) VALUES ({body.Substring(2)})";

                OpenConnection();
                isbusy = true;
                if (SqlConnect != null) {
                    Console.WriteLine($"[INFO] {query}");
                    var command = new SqlCommand(query, SqlConnect);
                    command.ExecuteNonQuery();
                }
                isbusy = false;
                CloseConnection();
                break;
            }
        }

        private void UpdateEntity(object model, string? atrb = null) {
            while (!isbusy) {
                var body = string.Empty;

                for (int i = 1; i < model.GetType().GetProperties().Length; i++) {
                    var info = model.GetType().GetProperties()[i];
                    if (info.GetValue(model) != null) {
                        var t = Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType;
                        var head = info.Name.ToLower();
                        if (t.Namespace != "System") {
                            object sub = info.GetValue(model);
                            body += $", id_{head} = '{sub.GetType().GetProperties()[0].GetValue(sub)}'";
                        } else {
                            if (info.PropertyType.ToString().Equals("System.DateTime")) {
                                body += $", {head} = '{Convert.ToDateTime(info.GetValue(model)).ToString("yyyy/MM/dd")}'";
                            } else {
                                body += $", {head} = '{info.GetValue(model)}'";
                            }
                        }
                    }
                }
                var index = (atrb != null) ? FindAttributeIndex(model, atrb) : 0;
                var extend = model.GetType().GetProperties()[index];

                var table = model.GetType().Name;
                var query = $"UPDATE {table} SET {body.Substring(2)} WHERE {extend.Name.ToLower()} = '{extend.GetValue(model)}'";

                OpenConnection();
                isbusy = true;
                if (SqlConnect != null) {
                    Console.WriteLine($"[INFO] {query}");
                    var command = new SqlCommand(query, SqlConnect);
                    command.ExecuteNonQuery();
                }
                isbusy = false;
                CloseConnection();
                break;
            }
        }

        private void DeleteEntity(object model) {
            while (!isbusy) {
                var table = model.GetType().Name;
                var query = $"DELETE FROM {table} WHERE {model.GetType().GetProperties()[0].Name} = '{model.GetType().GetProperties()[0].GetValue(model)}'";

                OpenConnection();
                isbusy = true;
                if (SqlConnect != null) {
                    Console.WriteLine($"[INFO] {query}");
                    var command = new SqlCommand(query, SqlConnect);
                    command.ExecuteNonQuery();
                }
                isbusy = false;
                CloseConnection();
                break;
            }
        }

        private object OrdinalRecursive(SqlDataReader result, PropertyInfo info) {
            var sub = Activator.CreateInstance(info.PropertyType);
            foreach (var subinfo in sub.GetType().GetProperties()) {
                Type t = Nullable.GetUnderlyingType(subinfo.PropertyType) ?? subinfo.PropertyType;
                var column = (subinfo.Name.ToLower() == "uid") ? $"id_{sub.GetType().Name.ToLower()}" : subinfo.Name;
                if (t.Namespace != "System") {
                    var subx = OrdinalRecursive(result, subinfo);
                    subinfo.SetValue(sub, Convert.ChangeType(subx, subinfo.PropertyType), null);
                } else {
                    if (!result.IsDBNull(result.GetOrdinal(column))) {
                        subinfo.SetValue(sub, Convert.ChangeType(result[column], subinfo.PropertyType), null);
                    }
                }
            }
            return sub;
        }

        private int FindAttributeIndex(object model, string keyword) {
            int index = -1;
            for (int i = 0; i < model.GetType().GetProperties().Length; i++) {
                var name = model.GetType().GetProperties()[i].Name;
                if (name == keyword) {
                    index = i;
                    break;
                }
            }
            return index;
        }

        private bool OpenConnection() {
            SqlConnect.ConnectionString = $"Server={DbServer};Database={DbSource};Trusted_Connection=yes;";
            SqlConnect.InfoMessage += Notice_Handler;
            SqlConnect.FireInfoMessageEventOnUserErrors = true;
            SqlConnect.Open();
            return true;
        }

        private bool CloseConnection() {
            SqlConnect.InfoMessage -= Notice_Handler;
            SqlConnect.Close();
            return true;
        }

        private void Notice_Handler(object sender, SqlInfoMessageEventArgs e) {
            SqlNotice = e.Message;
        }
    }
}
