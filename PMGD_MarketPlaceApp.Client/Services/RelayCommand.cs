using System.Windows.Input;

namespace PMGD_MarketPlaceApp.Client.Services {
    public class RelayCommand : ICommand {
        public RelayCommand(Action cmdactionnone) {
            this.cmdactionnone = cmdactionnone;
        }

        public RelayCommand(Action<object> cmdaction) {
            this.cmdaction = cmdaction;
        }

        public bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            if (parameter != null) {
                cmdaction(parameter);
            } else {
                cmdactionnone();
            }
        }

        public event EventHandler CanExecuteChanged;

        private readonly Action cmdactionnone;
        private readonly Action<object> cmdaction;
    }
}
