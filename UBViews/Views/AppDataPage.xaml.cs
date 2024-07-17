namespace UBViews.Views;

using UBViews.ViewModels;

public partial class AppDataPage : ContentPage
{
	public AppDataPage(AppDataViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}