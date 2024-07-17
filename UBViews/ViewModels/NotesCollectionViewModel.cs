namespace UBViews.ViewModels;

using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.Input;

using UBViews.Services;
using UBViews.Models.Notes;
using CommunityToolkit.Mvvm.ComponentModel;

public partial class NotesCollectionViewModel : BaseViewModel
{
    #region Private Data Members
    public ContentPage contentPage;

    public ObservableCollection<NoteEntry> Notes { get; } = new();

    INoteService notesService;

    string _class = "NotesCollectionViewModel";
    #endregion

    #region Constructor
    public NotesCollectionViewModel(INoteService notesService)
    {
        this.notesService = notesService;
    }
    #endregion

    #region Observable Properties
    [ObservableProperty]
    int noteCount = 0;
    #endregion

    #region Relay Commands
    [RelayCommand]
    async Task NotesCollectionPageAppearing()
    {
        string _method = "NotesCollectionPageAppearing";
        try
        {
            if (contentPage == null)
            {
                return;
            }

            var notes = await notesService.GetNotesAsync();

            foreach (var note in notes)
            {
                var paperId = note.PaperId;
                var seqId = note.SequenceId;
                var locationId = note.LocationId;
                Notes.Add(note);
            }

            NoteCount = Notes.Count;
            string titleMessage = $"Notes ({NoteCount})";
            Title = titleMessage;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in " +
                $"{_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }

    [RelayCommand]
    async Task SelectionChanged(NoteEntry selectedItem)
    {
        string _method = "SelectionChanged";

        try
        {
            if (selectedItem == null)
            {
                return;
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in " +
                $"{_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task SelectedCheckboxChanged(bool value)
    {
        string _method = "SelectedCheckboxChanged";
        try
        {
           
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }
    #endregion

    #region Helper Methods
    #endregion
}
