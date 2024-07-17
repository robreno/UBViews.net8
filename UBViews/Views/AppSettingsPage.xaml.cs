namespace UBViews.Views;

using UBViews.Services;
using UBViews.ViewModels;

public partial class AppSettingsPage : ContentPage
{
	public AppSettingsPage(XmlAppSettingsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
		vm.contentPage = this;
	}
}