using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMGD_MarketPlace.Server {
    internal class ThreadManager {
        public static void ExecuteOnMainThread(Action action) {
            if (action == null) {
                Console.WriteLine("[WARN] No action to execute on main thread!");
                return;
            }

            lock (executeOnMainThread) {
                executeOnMainThread.Add(action);
                actionToExecuteOnMainThread = true;
            }
        }

        public static void UpdateMain() {
            if (actionToExecuteOnMainThread) {
                executeCopiedOnMainThread.Clear();
                lock (executeOnMainThread) {
                    executeCopiedOnMainThread.AddRange(executeOnMainThread);
                    executeOnMainThread.Clear();
                    actionToExecuteOnMainThread = false;
                }

                for (int i = 0; i < executeCopiedOnMainThread.Count; i++) {
                    executeCopiedOnMainThread[i]();
                }
            }
        }

        private static readonly List<Action> executeOnMainThread = new();
        private static readonly List<Action> executeCopiedOnMainThread = new();
        private static bool actionToExecuteOnMainThread = false;
    }
}
