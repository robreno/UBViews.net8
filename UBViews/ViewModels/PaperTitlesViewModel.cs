using System.Text;
using System.Globalization;
using System.Collections.ObjectModel;

// Needed from GlobalUsings file
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Primitives;


using UBViews.Models;
using UBViews.Services;
using UBViews.Views;
using UBViews.Models.Audio;
using UBViews.Models.Ubml;

namespace UBViews.ViewModels
{
    public partial class PaperTitlesViewModel : BaseViewModel
    {
        /// <summary>
        /// CultureInfo
        /// </summary>
        public CultureInfo cultureInfo;

        /// <summary>
        /// ContentPage
        /// </summary>
        public ContentPage contentPage;

        /// <summary>
        /// 
        /// </summary>
        IFileService fileService;

        /// <summary>
        /// 
        /// </summary>
        IAppSettingsService settingsService;
        
        /// <summary>
        /// 
        /// </summary>
        IAudioService audioService;

        /// <summary>
        /// 
        /// </summary>
        public IMediaElement mediaElement;

        /// <summary>
        /// 
        /// </summary>
        IDownloadService downloadService;

        /// <summary>
        /// MediaStatePair
        /// </summary>
        public MediaStatePair MediaState = new();

        /// <summary>
        /// 
        /// </summary>
        protected AudioMarkerSequence Markers { get; set; } = new();

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<Paragraph> Paragraphs { get; private set; } = new();

        /// <summary>
        /// ObservableCollection
        /// </summary>
        public ObservableCollection<AudioMarker> AudioMarkers { get; private set; } = new();

        readonly string _class = "PaperTitlesViewModel";

        public PaperTitlesViewModel(IFileService fileService, IAppSettingsService settingsService, IAudioService audioService, IDownloadService downloadService)
        {
            this.fileService = fileService;
            this.settingsService = settingsService;
            this.audioService = audioService;
            this.downloadService = downloadService;
        }

        [ObservableProperty]
        bool isRefreshing;

        [ObservableProperty]
        bool pathInitialized = false;

        [ObservableProperty]
        bool showPaperContents;

        [ObservableProperty]
        string audioBaseUriString;

        [ObservableProperty]
        string audioUriString;

        [ObservableProperty]
        Uri audioUri;

        [ObservableProperty]
        string audioStatus = null;

        [ObservableProperty]
        bool streamAudio = false;

        [ObservableProperty]
        string audioDownloadStatus = null;

        [ObservableProperty]
        bool useDefaultDownloadPath = false;

        [ObservableProperty]
        string audioFolderName = null;

        [ObservableProperty]
        string audioFolderPath = null;

        [ObservableProperty]
        string localAudioFilePathName;

        [RelayCommand]
        async Task PaperTitlesPageAppearing()
        {
            string _method = "PaperTitlesPageAppearing";
            try
            {
                IsBusy = true;
                IsRefreshing = true;

                var hasValue = contentPage.Resources.TryGetValue("audioBaseUri", out object uri);
                if (hasValue)
                {
                    AudioBaseUriString = (string)uri;
                }

                Title = "Titles of the Papers";
                ShowPaperContents = await settingsService.Get("show_paper_contents", false);

            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        async Task TappedGesture(string id)
        {
            string _method = "TappedGesture";
            try
            {
                IsBusy = true;

                string className = id;
                string[] arr = id.Split('_', StringSplitOptions.RemoveEmptyEntries);
                int paperId = Int32.Parse(arr.ElementAt(0));
                PaperDto paperDto = await fileService.GetPaperDtoAsync(paperId);

                await GoToDetails(paperDto);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
                return;
            }
            finally 
            { 
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        async Task FlyoutMenu(string actionId)
        {
            string _method = "FlyoutMenu";
            try
            {
                if (contentPage == null)
                {
                    return;
                }

                if (mediaElement == null)
                {
                    return;
                }

                var actionArray = actionId.Split('_');
                var action = actionArray[0];
                var labelName = "_" + actionArray[1];
                var paperId = Int32.Parse(actionArray[1]);

                // Create Markers and PaperDto
                Markers = await LoadAudioMarkers(paperId);
                foreach (var marker in Markers.Values())
                {
                    AudioMarkers.Add(marker);
                }
                var dto = await fileService.GetPaperDtoAsync(paperId);
                var paragraphs = await fileService.GetParagraphsAsync(paperId);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
                return;
            }
        }

        [RelayCommand]
        async Task<AudioMarkerSequence> LoadAudioMarkers(int paperId)
        {
            string _method = "LoadAudioMarkers";
            try
            {
                this.Markers = await audioService.LoadAudioMarkersAsync(paperId);
                return this.Markers;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
                return null;
            }
        }

        [RelayCommand]
        async Task PositionChanged(TimeSpan timeSpan)
        {
            string _method = "PositionChanged";
            try
            {
                await audioService.PositionChangedAsync(timeSpan);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
                return;
            }
        }

        [RelayCommand]
        async Task StateChanged(string state)
        {
            string _method = "StateChanged";
            try
            {
                await audioService.StateChangedAsync(state);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
                return;
            }
        }

        [RelayCommand]
        async Task GoToDetails(PaperDto dto)
        {
            string _method = "GoToDetails";
            try
            {
                if (dto == null)
                    return;

                string className;
                if (ShowPaperContents)
                {
                    //className = nameof(MainPage);
                    className = nameof(ContentTitlesPage);
                }
                else
                {
                    className = "_" + dto.Id.ToString("000");
                }

                await Shell.Current.GoToAsync(className, true, new Dictionary<string, object>
                {
                    {"PaperDto", dto }
                });
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
                return;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
