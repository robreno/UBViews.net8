namespace UBViews.Controls.Help;

using CommunityToolkit.Maui.Views;
using UBViews.ViewModels;

public partial class AboutOverviewPopup : Popup
{
	public AboutOverviewPopup(PopupViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
		vm.popupPage = this;
	}

    private void closePopup_Clicked(object sender, EventArgs e)
    {
		this.Close();
    }
}