namespace UBViews.Controls.Help;

using System.Collections.ObjectModel;

using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;

using UBViews.Models.Notes;
using UBViews.Services;
using UBViews.ViewModels;


public partial class NotesPopup : Popup
{
    private string _uniqueId = null;
    //private NoteLocationsDto _locationsDto;

    public ObservableCollection<NoteEntry> Notes { get; private set; } = new();
    public List<NoteEntry> CurrentNotes { get; private set; } = new();

    INoteService notesService;
	public NotesPopup(PopupViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
		vm.popupPage = this;
		vm.vslPopupContent = this.mainSCVSL;
	}

    public NotesPopup(PopupViewModel vm, INoteService notesService) : this(vm) 
    {
        _uniqueId = vm.UniqueId;
        this.notesService = notesService;
        Task.Run(async () => 
        {
            await vm.LoadNotesAsync(_uniqueId);
            await vm.CreateNoteContentAsync();
        });
    }

    private void closePopup_Clicked(object sender, EventArgs e)
    {
		this.Close();
    }
}