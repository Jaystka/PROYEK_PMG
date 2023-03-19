using PMGD_MarketPlaceApp.Client.Services;
using PMGD_MarketPlaceApp.Client.ViewModels;
using PMGD_MarketPlaceApp.Client.Models;
using PMGD_MarketPlaceApp.Client.Views.Forms;

namespace PMGD_MarketPlaceApp.Client.Views.Home;

public partial class UniversityPage : ContentPage {
	public UniversityPage() {
        vm = new();
        InitializeComponent();
        BindingContext = vm;
        vm.OnCallBack += OnCallBack_Handler;
        vm.SelectCommand.Execute(null);
    }

    private void OnCallBack_Handler() {
        vm.SelectCommand.Execute(null);
    }

    private readonly UniversityViewModel vm;

    private async void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e) {
        if (e.SelectedItem is not University model) {
            return;
        }
        await Navigation.PushAsync(new RegisterPage(vm));
    }

    private async void BtnNew_Clicked(object sender, EventArgs e) {
        vm.Model = new University();
        await Navigation.PushAsync(new RegisterPage(vm));
    }

    private async void BtnExit_Clicked(object sender, EventArgs e) {
        await Shell.Current.GoToAsync("//HomeTab/Dashboard");
    }
}
