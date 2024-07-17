namespace UBViews.Views;

using UBViews.ViewModels;

public partial class NotesCollectionPage : ContentPage
{
    public NotesCollectionPage(NotesCollectionViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        vm.contentPage = this;
    }
}