namespace UBViews.ViewModels;

using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

//using System;
//using System.Collections.ObjectModel;
//using Microsoft.FSharp.Collections;
//using Microsoft.FSharp.Core;

using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Maui.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
//using CommunityToolkit.Maui.Views;

using UBViews.Services;
using UBViews.Helpers;
using UBViews.Models.Query;
using UBViews.Models.Audio;
using UBViews.Views;
using UBViews.Models;

//using UBViews.Models.Ubml;
//using UBViews.Models.Notes;

using UBViews.Controls.Help;
using UBViews.ViewModels;

using QueryEngine;
using QueryFilter;
using static DataTypesEx;

//using static QueryEngine.SimpleParse;

// See: https://stackoverflow.com/questions/61811825/stop-vs-from-automatically-adding-using-directives


[QueryProperty(nameof(QueryInput), nameof(QueryInput))]
public partial class MainViewModel : BaseViewModel
{
    #region   Private Data Members
    /// <summary>
    /// 
    /// </summary>
    public ContentPage contentPage;

    /// <summary>
    /// Services
    /// </summary>
    IFileService fileService;
    IAppSettingsService settingsService;
    IFSRepositoryService repositoryService;
    IQueryProcessingService queryProcessingService;
    IConnectivity connectivityService;
    IAudioService audioService;

    /// <summary>
    /// 
    /// </summary>
    public IMediaElement mediaElement;

    ParserService parserService;

    ConnectivityViewModel connectivityViewModel;

    readonly string _class = "MainViewModel";
    #endregion

    #region   Constructor
    public MainViewModel(IFileService fileService, 
                         IAppSettingsService settingsService, 
                         IFSRepositoryService repositoryService, 
                         IQueryProcessingService queryProcessingService,
                         IConnectivity connectivityService,
                         IAudioService audioService)
    {
        this.fileService = fileService;
        this.settingsService = settingsService;
        this.repositoryService = repositoryService;
        this.queryProcessingService = queryProcessingService;
        this.connectivityService = connectivityService;
        this.audioService = audioService;
        
        parserService = new ParserService();
        connectivityViewModel = new ConnectivityViewModel(connectivityService);
    }
    #endregion

    #region  Observable Properties
    [ObservableProperty]
    string audioStatus = "off";

    [ObservableProperty]
    string audioDownloadStatus = "off";

    [ObservableProperty]
    bool audioStreaming = false;

    [ObservableProperty]
    bool isRefreshing;

    [ObservableProperty]
    bool isInitialized;

    [ObservableProperty]
    QueryResultLocationsDto queryLocations;

    [ObservableProperty]
    QueryInputDto queryInputDto;

    [ObservableProperty]
    string queryInput;

    [ObservableProperty]
    string queryInputString;

    [ObservableProperty]
    string queryExpression;

    [ObservableProperty]
    XElement queryResult;

    [ObservableProperty]
    bool queryResultExists;

    [ObservableProperty]
    int maxQueryResults;

    [ObservableProperty]
    string partId;

    [ObservableProperty]
    List<string> termList = new();

    [ObservableProperty]
    string stem;
    #endregion

    #region  Relay Commands

    [RelayCommand]
    async Task MainPageAppearing()
    {
        string _method = "MainPageAppearing";
        try
        {
            if (contentPage == null)
            {
                return;
            }

            QueryInputDto = new QueryInputDto() { Text = "[Empty]", TokenCount = 0 };

            AudioStatus = await settingsService.Get("audio_status", "off");
            AudioStreaming = await settingsService.Get("stream_audio", false);
            AudioDownloadStatus = await settingsService.Get("audio_download_status", "off");

            await audioService.SetContentPageAsync(contentPage);

            bool hasInternet = await CheckInternetAsync();
            Preferences.Default.Set("has_internet", hasInternet);

            if (!IsInitialized)
            {
                await queryProcessingService.SetContentPageAsync(contentPage);
                MaxQueryResults = await settingsService.Get("max_query_results", 50);
                await queryProcessingService.SetMaxQueryResultsAsync(MaxQueryResults);
                this.IsInitialized = true;
            }

            string titleMessage = $"UBViews";
            Title = titleMessage;
        }
        catch (NullReferenceException ex)
        {
            await App.Current.MainPage.DisplayAlert($"Null reference raised in {_class}.{_method} => ", ex.Message, "Cancel");
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
        }
    }

    [RelayCommand]
    async Task MainPageDisappearing()
    {
        string _method = "MainPageDisappearing";
        try
        {
            
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
        }
    }

    [RelayCommand]
    async Task MainPageUnloaded()
    {
        string _method = "MainPageUnloaded";
        try
        {
            
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
        }
    }

    [RelayCommand]
    async Task SubmitQuery(string queryString)
    {
        string _method = "SubmitQuery";
        try
        {
            // temple and prostitutes
            // prostitutes and temple
            // foreword and orvonton

            IsBusy = true;
            string message = string.Empty;
            bool parsingSuccessful = false;
            bool runPreCheckSilent = await settingsService.Get("run_precheck_silent", true);

            QueryInputString = queryString;
            (bool isValid, message) = await queryProcessingService.PreCheckQueryAsync(QueryInputString,
                                                                                      runPreCheckSilent);

            if (!isValid)
            {
                await App.Current.MainPage.DisplayAlert($"Invalid Query String => ", message, "Ok");
            }
            else
            {
                if (message.Contains("="))
                {
                    if (message.Contains("Audio status"))
                    {
                        var _audioStatus = await settingsService.Get("audio_status", "off");
                        AudioStatus = _audioStatus;
                    }
                    else if (message.Contains("Audio streaming"))
                    {
                        var _streamAudio = await settingsService.Get("stream_audio", false);
                        AudioStreaming = _streamAudio;
                    }
                    else if (message.Contains("Audio download"))
                    {
                        var _downloadStatus = await settingsService.Get("audio_download_status", "off");
                        AudioDownloadStatus = _downloadStatus;
                    }
                    return;
                }

                (parsingSuccessful, message) = await queryProcessingService.ParseQueryAsync(QueryInputString);
                if (parsingSuccessful)
                {
                    QueryInputString = await queryProcessingService.GetQueryInputStringAsync();
                    QueryExpression = await queryProcessingService.GetQueryExpressionAsync();
                    TermList = await queryProcessingService.GetTermListAsync();

                    (bool isSuccess, QueryResultExists, QueryLocations) = await queryProcessingService.RunQueryAsync(queryString);
                    if (isSuccess)
                    {
                        if (QueryResultExists)
                        {
                            // Query result from history successfully
                            // Navigate to results page
                        }
                        else
                        {
                            // New query run successfully
                            // Navigate to results page
                        }
                        await NavigateTo("QueryResults");
                    }
                }
                else // Query parsing error
                {
                    string _msg = $"{message}";
                    QueryInputString = await App.Current.MainPage.DisplayPromptAsync("Query Parsing Error",
                        _msg, "Ok", "Cancel", "Retry Query here ..", -1, null, "");
                }
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    async Task NavigateTo(string target)
    {
        string _method = "NavigateTo";
        try
        {
            IsBusy = true;

            string targetName = string.Empty;
            if (target == "QueryResults")
            {
                targetName = nameof(QueryResultPage);
                QueryResultLocationsDto dto = QueryLocations;
                await Shell.Current.GoToAsync(targetName, new Dictionary<string, object>()
                {
                    {"QueryLocations", dto }
                });
            }
            else if (target == "PartsPage")
            {
                targetName = nameof(PartsPage);
                await Shell.Current.GoToAsync(targetName);
            }
            else if (target == "PaperTitles")
            {
                targetName = nameof(PaperTitlesPage);
                await Shell.Current.GoToAsync(targetName);
            }
            else if (target == "AppSettings")
            {
                targetName = nameof(AppSettingsPage);
                await Shell.Current.GoToAsync(targetName);
            }
            else if (target == "AppContacts")
            {
                targetName = nameof(ContactsPage);
                await Shell.Current.GoToAsync(targetName);
            }
            else if (target == "PopupHelp")
            {
                targetName = nameof(HelpPage);
                await Shell.Current.GoToAsync(targetName);
            }
            else
            {
                targetName = nameof(MainPage);
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Helper Methods
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private async Task<bool> CheckInternetAsync()
    {
        string _method = "CheckInternetAsync";
        try
        {
            bool isInternet = false;
            NetworkAccess accessType = connectivityService.NetworkAccess;
            if (accessType == NetworkAccess.Internet)
            {
                isInternet = true;
            }
            return isInternet;

        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private async Task SendToastAsync(string message)
    {
        string _method = "SendToastAsync";
        try
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                ToastDuration duration = ToastDuration.Short;
                double fontSize = 14;
                var toast = Toast.Make(message, duration, fontSize);
                await toast.Show(cancellationTokenSource.Token);
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }
    #endregion
}
