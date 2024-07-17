namespace UBViews.Views;

using UBViews.ViewModels;

public partial class PartsPage : ContentPage
{
	public PartsPage(PartsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
		vm.contentPage = this;
	}
}