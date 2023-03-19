using PMGD_MarketPlaceApp.Client.Models;
using PMGD_MarketPlaceApp.Client.ViewModels;

namespace PMGD_MarketPlaceApp.Client.Views.Forms;

public partial class RegisterPage : ContentPage {
    public RegisterPage(UniversityViewModel viewmodel) {
        InitializeComponent();
        BindingContext = viewmodel;
        if (viewmodel.Model.Uid <= 0) {
            BtnSave.IsVisible = true;
            BtnUpdate.IsVisible = false;
            BtnDelete.IsVisible = false;
        } else {
            BtnSave.IsVisible = false;
            BtnUpdate.IsVisible = true;
            BtnDelete.IsVisible = true;
        }
        viewmodel.OnCallBack += OnCallBack_Handler;
    }

    private async void OnCallBack_Handler() {
        await Navigation.PopAsync();
    }
}