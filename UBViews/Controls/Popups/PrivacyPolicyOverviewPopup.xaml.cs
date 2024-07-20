namespace UBViews.Controls.Help;

using CommunityToolkit.Maui.Views;
using UBViews.ViewModels;

public partial class PrivacyPolicyOverviewPopup : Popup
{
	public PrivacyPolicyOverviewPopup(PopupViewModel vm)
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