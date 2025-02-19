﻿namespace UBViews.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Input;

using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics.Text;

using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Platform;

using UBViews.Models;
using UBViews.Models.Ubml;
using UBViews.Models.Query;
using UBViews.Services;
using UBViews.Views;

[QueryProperty(nameof(QueryLocations), nameof(QueryLocations))]
public partial class QueryResultViewModel : BaseViewModel
{
    #region Private Data Members
    public ContentPage contentPage;

    public ObservableCollection<QueryLocationDto> QueryLocationsDto { get; } = new();
    public ObservableCollection<QueryHit> Hits { get; set; } = new();
    public ObservableCollection<Paragraph> Paragraphs { get; set; } = new();
    public ObservableCollection<Border> Borders { get; set; } = new();

    IFileService fileService;
    IEmailService emailService;
    IAppSettingsService settingsService;
    IQueryProcessingService queryProcessingService;

    readonly string _class = "QueryResultViewModel";
    #endregion

    #region Constructor

    public QueryResultViewModel(IFileService fileService,
                                IEmailService emailService,
                                IAppSettingsService settingsService,
                                IQueryProcessingService queryProcessingService)
    {
        this.fileService = fileService;
        this.emailService = emailService;
        this.settingsService = settingsService;
        this.queryProcessingService = queryProcessingService;
    }
    #endregion

    #region Observable Properties
    [ObservableProperty]
    string audioStatus = "off";

    [ObservableProperty]
    string audioDownloadStatus = "off";

    [ObservableProperty]
    bool audioStreaming = false;

    [ObservableProperty]
    bool isInitialized = false;

    [ObservableProperty]
    string pageTitle;

    [ObservableProperty]
    bool isRefreshing;

    [ObservableProperty]
    QueryResultLocationsDto queryLocations = new();

    [ObservableProperty]
    string queryString;

    [ObservableProperty]
    string queryInputString;

    [ObservableProperty]
    string previousQueryInputString;

    [ObservableProperty]
    bool queryResultExists = false;

    [ObservableProperty]
    string queryExpression = string.Empty;

    [ObservableProperty]
    List<string> termList = new();

    [ObservableProperty]
    int queryHits;

    [ObservableProperty]
    int maxQueryResults;

    [ObservableProperty]
    bool showReferencePids;

    [ObservableProperty]
    bool isScrollToLabel;

    [ObservableProperty]
    string scrollToLabelName;

    [ObservableProperty]
    bool hideUnselected;

    [ObservableProperty]
    Color defaultColorForMainVSL = Color.Parse("White");

    [ObservableProperty]
    Color defaultColorForSelectionHSL = Color.Parse("White");

    [ObservableProperty]
    Color defaultColorForScrollView = Color.Parse("White");

    [ObservableProperty]
    Color defaulColorForContentVSL = Color.Parse("White");

    [ObservableProperty]
    Color defaultColorForBorder = Color.Parse("LightBlue");

    [ObservableProperty]
    Color defaultColorForEditor = Color.Parse("White");

    [ObservableProperty]
    Color defaultColorForHSL = Color.Parse("LightBlue");
    #endregion

    #region Relay Commands
    [RelayCommand]
    async Task QueryResultPageAppearing(QueryResultLocationsDto dto)
    {
        string _method = "QueryResultAppearing";
        try
        {
            if (contentPage == null)
            {
                throw new NullReferenceException("ContentPage null.");
            }

            if (dto == null)
            {
                throw new NullReferenceException("QueryResultLocationsDto null.");
            }

            await queryProcessingService.SetContentPageAsync(contentPage);

            if (dto.DefaultQueryString != null)
            {
                QueryInputString = PreviousQueryInputString = dto.DefaultQueryString;
            }
            else
            {
                QueryInputString = PreviousQueryInputString = dto.QueryString;
            }
                
            QueryHits = dto.Hits;
            MaxQueryResults = await settingsService.Get("max_query_results", 50);

            string titleMessage = $"Query Result {QueryHits} hits ...";
            Title = titleMessage;

            var locations = dto.QueryLocations.Take(MaxQueryResults).ToList();
            foreach (var location in locations)
            {
                QueryLocationsDto.Add(location);
            }
            IsInitialized = true;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }

    [RelayCommand]
    async Task QueryResultPageLoaded()
    {
        string _method = "QueryResultLoaded";
        try
        {
            IsBusy = true; IsInitialized = true;

            var locations = QueryLocationsDto;
            if (locations == null)
            {
                return;
            }
            else
            {
                List<QueryLocationDto> dtos = new();
                foreach (var location in QueryLocationsDto)
                {
                    dtos.Add(location);
                }
                await LoadXamlAsync(dtos, IsInitialized);
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised => {_class}.{_method}.", ex.Message, "Cancel");
            return;
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    async Task QueryResultDisappearing()
    {
        string _method = "QueryResultDisappearing";
        try
        {
            QueryLocationsDto.Clear();

        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }

    [RelayCommand]
    async Task SubmitQuery(string queryString)
    {
        string _method = "SubmitQuery";
        try
        {
            IsBusy = true;
            string message = string.Empty;
            bool parsingSuccessful = false;
            bool runPreCheckSilent = await settingsService.Get("run_precheck_silent", true);

            Preferences.Default.Set("CurrentQueryString", queryString);

            QueryInputString = queryString.Trim();
            (bool result, message) = await queryProcessingService.PreCheckQueryAsync(QueryInputString,
                                                                                     runPreCheckSilent);
            if (message.Contains("="))
            {
                return;
            }

            if (QueryInputString == PreviousQueryInputString)
            {
                message = $"The query \"{QueryInputString}\" was same. Try another query?";
                await App.Current.MainPage.DisplayAlert("Query Results", message, "Cancel");
                return;
            }
            else
            {
                PreviousQueryInputString = QueryInputString;
            }

            (parsingSuccessful, message) = await queryProcessingService.ParseQueryAsync(QueryInputString);
            if (parsingSuccessful)
            {
                QueryInputString = await queryProcessingService.GetQueryInputStringAsync();
                QueryExpression = await queryProcessingService.GetQueryExpressionAsync();
                TermList = await queryProcessingService.GetTermListAsync();

                (bool isSuccess, QueryResultExists, QueryLocations) = await queryProcessingService.RunQueryAsync(QueryInputString);
                if (isSuccess)
                {
                    QueryLocationsDto.Clear();
                    foreach (var location in QueryLocations.QueryLocations)
                    {
                        QueryLocationsDto.Add(location);
                    }
                    
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
                    await LoadXamlAsync(QueryLocationsDto.ToList(), true);
                }
            }
            else // Parsing failure
            {
                string _msg = $"{message}";
                QueryInputString = await App.Current.MainPage.DisplayPromptAsync("Query Parsing Error",
                    _msg, "Ok", "Cancel", "Retry Query here ..", -1, null, "");
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_method}.{_class} => ", ex.Message, "Ok");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    async Task TappedGesture(string lableName)
    {
        string _method = "TappedGesture";
        try
        {
            var arry = lableName.Split('_', StringSplitOptions.RemoveEmptyEntries);
            var paperId = Int32.Parse(arry[0]);
            var seqId = Int32.Parse(arry[1]);
            Paragraph paragraph = await fileService.GetParagraphAsync(paperId, seqId);
            PaperDto paperDto = await fileService.GetPaperDtoAsync(paperId);
            var uid = paragraph.Uid;
            var pid = paragraph.Pid;

            paperDto.ScrollTo = true;
            paperDto.SeqId = seqId;
            paperDto.Pid = pid;
            paperDto.Uid = uid;

            string msg = $"Navigating to {pid}";

            await GoToDetails(paperDto);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    [RelayCommand]
    async Task SwipeLeftGesture(string value)
    {
        try 
        { 
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return;
        }
    }

    [RelayCommand]
    async Task SelectedCheckboxChanged(bool value)
    {
        string _method = "SelectedCheckboxChanged";
        try
        {
            var contentVSL = contentPage.FindByName("contentVerticalStackLayout") as VerticalStackLayout;
            var checkedLabel = contentPage.FindByName("hideUncheckedLabel") as Label;
            if (contentVSL == null)
            {
                return;
            }
            var children = contentVSL.Children;
            foreach (var child in children)
            {
                Border border = (Border)child;
                var content = border.Content;
                var visualTree = content.GetVisualTreeDescendants();
                var vsl = (VerticalStackLayout)visualTree[0];
                var lbl = (Label)visualTree[1];
                var chk = (CheckBox)visualTree[2];
                var isChecked = chk.IsChecked;
                var borderIsVisible = border.IsVisible;
                if (!isChecked && borderIsVisible)
                {
                    border.IsVisible = false;
                    checkedLabel.Text = "Show Unchecked";
                }
                else if (!isChecked && !borderIsVisible)
                {
                    border.IsVisible = true;
                    checkedLabel.Text = "Hide Unchecked";
                }
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    [RelayCommand]
    async Task ParagraphSelected(CheckBox checkbox)
    {
        string _method = "ParagraphSelected";
        try
        {
            string id = checkbox.ClassId;
            bool isSelected = checkbox.IsChecked;
            QueryHit hit = Hits.Where(h => h.Id == id).FirstOrDefault();
            hit.Selected = isSelected;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    [RelayCommand]
    async Task ShareSelected(string value)
    {
        string _method = "ShareSelected";
        // TODO: unfined code
        try
        {
            var selectedHits = Hits.Where(p => p.Selected == true).ToList();
            List<Paragraph> paragraphs = new();
            foreach (var hit in selectedHits)
            {
                var paragraph = hit.Paragraph;
                paragraphs.Add(paragraph);
            }
#if WINDOWS
            await emailService.EmailParagraphsAsync(paragraphs, IEmailService.EmailType.PlainText, IEmailService.SendMode.AutoSend);
#elif ANDROID
            await emailService.ShareParagraphsAsync(paragraphs);
#endif
        }
        catch (FormatException ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
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
    async Task GoToDetails(PaperDto dto)
    {
        string _method = "GoToDetails";
        try
        {
            IsBusy = true;

            if (dto == null)
                return;

            string className = "_" + dto.Id.ToString("000");

            await Shell.Current.GoToAsync(className, true, new Dictionary<string, object>
            {
                {"PaperDto", dto }
            });
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
    #endregion

    #region Helper Methods
    private async Task LoadXamlAsync(List<QueryLocationDto> dtos, bool clear = false)
    {
        string _method = "LoadXamlAsync";
        try
        {
            var contentScrollView = contentPage.FindByName("contentScrollView") as ScrollView;
            var contentVSL = contentPage.FindByName("contentVerticalStackLayout") as VerticalStackLayout;
            var locations = dtos;
            int hit = 0;

            if (clear) 
            {
                contentScrollView.Content = null;
                contentVSL = new VerticalStackLayout()
                {
                    HorizontalOptions = LayoutOptions.Center
                };
            }

            foreach (var location in locations)
            {
                hit++;
                var id = location.Id;
                var pid = location.Pid;
                var arry = id.Split('.');
                var paperId = Int32.Parse(arry[0]);
                var seqId = Int32.Parse(arry[1]);
                var paragraph  = await fileService.GetParagraphAsync(paperId, seqId);
                var paraStyle = paragraph.ParaStyle;
                var labelName = "_" + paperId.ToString("000") + "_" + seqId.ToString("000");

                Paragraphs.Add(paragraph);

                QueryHit queryHit = new QueryHit
                {
                    Id = paperId + "." + seqId,
                    Query = QueryInputString,
                    Hit = hit,
                    PaperId = paperId,
                    SequenceId = seqId,
                    Pid = pid,
                    Selected = false,
                    Paragraph = paragraph,
                };
                Hits.Add(queryHit);

                // Create Span List
                var spans = await CreateHighlightedSpansAsync(location, paragraph);
                // Create FormattedString
                FormattedString fs = await CreateFormattedStringAsync(paragraph, spans, hit);
                // Create Label
                Label label = await CreateLabelAsync(fs, labelName, paperId, seqId, pid);
                // Create Border
                Border newBorder = await CreateBorderAsync(paperId, seqId, label);

                Borders.Add(newBorder);
                contentVSL.Add(newBorder);
            }
            contentScrollView.Content = contentVSL;

            string titleMessage = string.Empty;
            if (hit == QueryHits)
            {
                titleMessage = $"Query Result {hit} hit(s) ...";
            }
            else
            {
                titleMessage = $"Query Result {hit} hit(s) out of {QueryHits} total ...";
            }        
            Title = titleMessage;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }
    private async Task<List<Span>> CreateHighlightedSpansAsync(QueryLocationDto location, Paragraph paragraph)
    {
        string _method = "CreateHighlightedSpansAsync";
        try
        {
            string text = paragraph.Text;
            List<string> termList = new List<string>();
            foreach (var termOcc in location.TermOccurrences)
            {
                var position = termOcc.TextPosition;
                var length = termOcc.TextLength;
                var term = paragraph.Text.Substring(position, length);
                var termReplacement = "_{" + term + "}_";
                if (termList.Contains(term))
                    continue;
                var rgx = new Regex(term);
                text = Regex.Replace(text, "\\b" + term + "\\b", termReplacement);
                termList.Add(term);
            }
            var spanArray = text.Split('_');

            var txt = string.Empty;
            Span newSpan = null;
            List<Span> spans = new();
            foreach (var item in spanArray)
            {
                txt = item;
                if (item.Contains('{'))
                {
                    txt = txt.Replace('{', ' ');
                    txt = txt.Replace('}', ' ');
                    txt = txt.Trim();
                    newSpan = new Span() { Style = (Style)App.Current.Resources["HighlightSpan"], Text = txt };
                    spans.Add(newSpan);
                }
                else
                {
                    newSpan = new Span() { Style = (Style)App.Current.Resources["RegularSpan"], Text = item };
                    spans.Add(newSpan);
                }
            }
            return spans;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class} . {_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task<FormattedString> CreateFormattedStringAsync(Paragraph paragraph, List<Span> spans, int hit)
    {
        string _method = "CreateFormattedStringAsync";
        try
        {
            FormattedString formattedString = new FormattedString();

            var paperId = paragraph.PaperId;
            var seqId = paragraph.SeqId;
            var pid = paragraph.Pid;
            var labelName = "_" + paperId.ToString("000") + "_" + seqId.ToString("000");

            Span tabSpan = new Span() { Style = (Style)App.Current.Resources["TabsSpan"] };
            Span pidSpan = new Span()
            {
                Style = (Style)App.Current.Resources["PID"],
                StyleId = labelName,
                Text = pid
            };
            Span spaceSpan = new Span() { Style = (Style)App.Current.Resources["RegularSpaceSpan"] };
            Span hitsSpan = new Span()
            {
                Style = (Style)App.Current.Resources["HID"],
                StyleId = labelName,
                Text = $"[hit {hit}]"
            };

            formattedString.Spans.Add(hitsSpan);
            formattedString.Spans.Add(tabSpan);
            formattedString.Spans.Add(pidSpan);
            formattedString.Spans.Add(spaceSpan);

            foreach (var span in spans)
            {
                formattedString.Spans.Add(span);
            }

            return formattedString;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task<Label> CreateLabelAsync(FormattedString fs, string labelName, int paperId, int seqId, string pid)
    {
        string _method = "CreateLabelAsync";
        try
        {
            Label label = new Label { FormattedText = fs };
            //label.Style = (Style)App.Current.Resources["HighlightSpan"];
            label.ClassId = paperId + "." + seqId;
            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.SetBinding(TapGestureRecognizer.CommandProperty, "TappedGestureCommand");
            tapGestureRecognizer.CommandParameter = $"{labelName}";
            tapGestureRecognizer.NumberOfTapsRequired = 1;
            label.GestureRecognizers.Add(tapGestureRecognizer);
            label.SetValue(ToolTipProperties.TextProperty, $"Tap to go to paragraph {pid} ...");
            return label;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class} . {_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task<Border> CreateBorderAsync(int paperId, int seqId, Label label)
    {
        string _method = "CreateBorderAsync";
        try
        {
            var id = paperId + "." + seqId;
            VerticalStackLayout vsl = new VerticalStackLayout() { ClassId = id };

            CheckBox checkBox = await CreateCheckBoxAsync(paperId, seqId);
            
            vsl.Add(label);
            vsl.Add(checkBox);

            Border newBorder = new Border()
            {
                ClassId = id,
                Style = (Style)App.Current.Resources["QueryResultBorderStyle"],
                Content = vsl
            };
            return newBorder;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}  .  {_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task<CheckBox> CreateCheckBoxAsync(int paperId, int seqId)
    {
        string _method = "CreateCheckBoxAsync";
        try
        {
            CheckBox checkBox = new CheckBox();
            checkBox.ClassId = paperId + "." + seqId;
            checkBox.HorizontalOptions = LayoutOptions.End;

            var binding = new Binding();
            binding.Source = nameof(QueryResultViewModel);
            binding.Path = "IsChecked";

            var behavior = new EventToCommandBehavior
            {
                EventName = "CheckedChanged",
                Command = ParagraphSelectedCommand,
                CommandParameter = checkBox
            };
            checkBox.Behaviors.Add(behavior);
            return checkBox;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class} . {_method} => ", ex.Message, "Ok");
            return null;
        }
    } 
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
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class} . {_method} => ", ex.Message, "Ok");
            return;
        }
    }
    #endregion
}
