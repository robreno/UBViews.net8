using UBViews.ViewModels;
using UBViews.Models.AppData;

namespace UBViews.Views;

public partial class ContactsPage : ContentPage
{
	public ContactsPage(ContactsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
		vm.contentPage = this;
	}
}