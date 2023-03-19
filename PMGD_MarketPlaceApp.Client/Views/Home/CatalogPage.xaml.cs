using PMGD_MarketPlaceApp.Client.Views.Forms;

namespace PMGD_MarketPlaceApp.Client.Views.Home;

public partial class CatalogPage : ContentPage
{
	public CatalogPage()
	{
		InitializeComponent();
	}

    private async void CounterBtn_Clicked(object sender, EventArgs e) {
		await Navigation.PushAsync(new LogPage());
    }
}