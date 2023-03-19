using System.Collections.ObjectModel;
using System.ComponentModel;
using PMGD_MarketPlaceApp.Client.Services;
using PMGD_MarketPlaceApp.Client.Models;

namespace PMGD_MarketPlaceApp.Client.ViewModels {
    public class UniversityViewModel : INotifyPropertyChanged {
        public UniversityViewModel() {
            collection = new();
            model = new();

            InsertCommand = new RelayCommand(async () => await CreateDataAsync());
            UpdateCommand = new RelayCommand(async () => await UpdateDataAsync());
            DeleteCommand = new RelayCommand(async () => await DeleteDataAsync());
            SelectCommand = new RelayCommand(async () => await ReadDataAsync());
        }

        public RelayCommand InsertCommand { get; set; }
        public RelayCommand UpdateCommand { get; set; }
        public RelayCommand DeleteCommand { get; set; }
        public RelayCommand SelectCommand { get; set; }

        public ObservableCollection<University> Collection {
            get {
                return collection;
            }
            set {
                collection = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            }
        }

        public University Model {
            get {
                return model;
            }
            set {
                model = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action OnCallBack;

        private ObservableCollection<University> collection;
        private University model;

        private async Task ReadDataAsync() {
            try {
                ClientResponse.Universities.Clear();
                ClientRequest.GetDataFromServer(ClientProtocol.UniversityGet);
                while (ClientResponse.Universities.Count <= 0) {
                    continue;
                }
            } catch (Exception) {
                ClientResponse.Universities.Clear();
                Collection.Clear();
            } finally {
                Collection.Clear();
                foreach (var item in ClientResponse.Universities) {
                    Collection.Add(item);
                }
            }
            await Task.Delay(0);
        }

        private async Task CreateDataAsync() {
            ClientRequest.PostDataToServer(ClientProtocol.UniversityPost, Model);
            await Application.Current.MainPage.DisplayAlert("Info", "Updated", "Ok");
            OnCallBack?.Invoke();
        }

        private async Task UpdateDataAsync() {
            ClientRequest.PostDataToServer(ClientProtocol.UniversityPut, Model);
            await Application.Current.MainPage.DisplayAlert("Info", "Updated", "Ok");
            OnCallBack?.Invoke();
        }

        private async Task DeleteDataAsync() {
            var confirm = await Application.Current.MainPage.DisplayAlert("Warning", "Are you sure?", "Yes", "No");
            if (confirm) {
                ClientRequest.PostDataToServer(ClientProtocol.UniversityDel, Model);
                OnCallBack?.Invoke();
            }
        }
    }
}
