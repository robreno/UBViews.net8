﻿namespace UBViews.Helpers;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
using Microsoft.FSharp.Collections;

using System.Xml.Linq;
using System.Linq;
using Microsoft.FSharp.Core;

using System.Globalization;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Core.Primitives;

// Needed from GlobalUsings file
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using UBViews.Extensions;
using UBViews.Models;
using UBViews.Models.Ubml;
using UBViews.Models.Audio;
using UBViews.Services;
using UBViews.Helpers;

public partial class AudioService : IAudioService
{
    #region  Data Members
    private Dictionary<int, List<int>> _astriskDic = new Dictionary<int, List<int>>()
    {
        { 31, new List<int> { 92 } },
        { 56, new List<int> { 92 } },
        { 120, new List<int> { 41 } },
        { 134, new List<int> { 70 } },
        { 196, new List<int> { 78 } },
        { 144, new List<int> { 70, 84, 97, 113, 132, 146 } }
    };

    /// <summary>
    /// ContentPage
    /// </summary>
    private ContentPage contentPage;

    /// <summary>
    /// MediaElement
    /// </summary>
    private IMediaElement mediaElement;

    /// <summary>
    /// 
    /// </summary>
    public HttpClient httpClient;

    /// <summary>
    /// ObservableCollection
    /// </summary>
    public ObservableCollection<AudioMarker> AudioMarkers { get; private set; } = new();

    /// <summary>
    /// 
    /// </summary>
    public ObservableCollection<Paragraph> Paragraphs { get; private set; } = new();

    readonly string _class = "AudioService";
    #endregion

    #region   Services
    IFileService fileService;
    IAppSettingsService settingsService;
    IDownloadService downloadService;
    #endregion

    #region   Constructors
    public AudioService(IFileService fileService, IAppSettingsService settingsService, IDownloadService downloadService)
    {
        this.fileService = fileService;
        this.settingsService = settingsService;
        this.downloadService = downloadService;
    }
    #endregion

    #region  Public Properties
    public bool Initialized { get; set; } = false;
    public bool ContentPageInitialized { get; set; } = false;
    public bool MediaElementInitialized { get; set; } = false;
    public bool ShowPlaybackControls { get; set; } = false;
    public bool SendToastState { get; set; } = false;
    

    public MediaStatePair MediaState { get; set; } = new();
    public MediaStatePair MediaElementMediaState { get; set; } = new();
    public AudioFlag AudioStatusFlag { get; set; } = new();
    public AudioMarkerSequence Markers { get; set; } = new();
    public PaperDto PaperDto { get; set; } = null;

    public int PaperId { get; set; }
    public string Plattform { get; set; }
    public string PaperTitle { get; set; }
    public string PaperAuthor { get; set; }
    public string PaperNumber { get; set; }
    public string TimeSpanString { get; set; }
    public TimeSpan Position { get; set; }
    public TimeSpan Duration { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string PreviousState { get; set; }
    public string CurrentState { get; set; }

    public bool AudioDownloadStatus { get; set; } = false;
    public bool AudioStreamingStatus { get; set; } = false;
    public bool ValidAudioDownloadPath { get; set; } = false;
    public bool ValidAudioUriPath { get; set; } = false;
    public bool AudioFolderExists { get; set; } = false;
    public string AudioUriString { get; set; } = null;
    public Uri AudioUri { get; set; } = null;
    public string AudioFileName { get; set; } = null;
    public bool AudioFileDownloaded { get; set; } = false;
    public string AudioStatus { get; set; }
    public string AudioDownloadPath { get; set; } = null;
    public string AudioDownloadFullPathName { get; set; } = null;
    public string LocalStatePath { get; set; } = FileSystem.AppDataDirectory;
    #endregion

    #region  Interface Implementations
    /// <summary>
    /// 
    /// </summary>
    /// <param name="contentPage"></param>
    /// <param name="mediaElement"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task InitializeDataAsync(ContentPage contentPage, IMediaElement mediaElement, PaperDto dto, Uri uri)
    {
        string _method = "InitializeDataAsync";
        try
        {
            this.contentPage = contentPage;
            this.mediaElement = mediaElement;
            await SetPaperDtoAsync(dto);


            AudioFileName = PaperDto.Id.ToString("000") + ".mp3";

            if (uri != null)
            {
                AudioUriString = uri.OriginalString;
                AudioUri = uri;
                ValidAudioUriPath = true;
            }

            AudioStatus = await settingsService.Get("audio_status", "off");
            AudioStreamingStatus = await settingsService.Get("stream_audio", false);
            string audioFolderPath = await settingsService.Get("audio_folder_path", @"LocalState\AudioFiles");
            string audioFolderName = await settingsService.Get("audio_folder_name", "AudioFiles");

            if (!string.IsNullOrEmpty(audioFolderPath) && !string.IsNullOrEmpty(audioFolderName))
            {
                if (audioFolderPath == @"LocalState\AudioFiles")
                {
                    AudioDownloadPath = Path.Combine(LocalStatePath, audioFolderName);
                    AudioFolderExists = Directory.Exists(AudioDownloadPath);
                    if (!AudioFolderExists)
                    {
                        System.IO.Directory.CreateDirectory(AudioDownloadPath);
                    }
                    ValidAudioDownloadPath = true;
                }
                else
                {
                    AudioDownloadPath = audioFolderPath;
                    ValidAudioDownloadPath = true;
                }

                if (ValidAudioDownloadPath)
                {
                    AudioDownloadFullPathName = Path.Combine(AudioDownloadPath, AudioFileName);
                    AudioFileDownloaded = File.Exists(AudioDownloadFullPathName);
                }
            }

            Initialized = (ValidAudioUriPath && ValidAudioDownloadPath);

            if (AudioStatus.Equals("on"))
            {
                AudioStatusFlag.SetAudioStatus(AudioFlag.AudioStatus.On);
            }
            else
            {
                AudioStatusFlag.SetAudioStatus(AudioFlag.AudioStatus.Off);
            }

            CurrentState = "None";
            PreviousState = "None";
            MediaState = new MediaStatePair() { CurrentState = "None", PreviousState = "None" };
            SendToastState = false;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<bool> IsInitializedAsync()
    {
        string _method = "SetContentPageAsync";
        try
        {
            bool isInitialized = false;
            if (ContentPageInitialized && MediaElementInitialized)
            {
                isInitialized = true;
            }
            return isInitialized;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="contentPage"></param>
    /// <returns></returns>
    public async Task SetContentPageAsync(ContentPage contentPage)
    {
        string _method = "SetContentPageAsync";
        try
        {
            this.contentPage = contentPage;
            ContentPageInitialized = true;
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    public async Task SetMediaElementAsync(IMediaElement mediaElement)
    {
        string _method = "SetMediaElementAsync";
        try
        {
            this.mediaElement = mediaElement;
            MediaElementInitialized = true;
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public async Task<AudioMarker> GetAtAsync(int index)
    {
        string _method = "GetAtAsync";
        try
        {
            var marker = new AudioMarker();
            marker = AudioMarkers.ElementAt(index);
            return marker;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task ClearAync()
    {
        string _method = "ClearAsync";
        try 
        {
            this.AudioMarkers.Clear();
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="audioMarker"></param>
    /// <returns></returns>
    public async Task InsertAsync(AudioMarker audioMarker)
    {
        string _method = "InsertAsync";
        try 
        {
            this.AudioMarkers.Add(audioMarker);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<IList<AudioMarker>> ValuesAsync()
    {
        string _method = "ValuesAsync";
        try
        {
            List<AudioMarker> markers = new List<AudioMarker>();
            await Task.Run(() => markers = AudioMarkers.ToList());
            return markers;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="paperId"></param>
    /// <returns></returns>
    public async Task<AudioMarkerSequence> LoadAudioMarkersExAsync(int paperId)
    {
        string _method = "LoadAudioMarkersAsync";
        try
        {
            var fileName = paperId.ToString("000") + ".audio.xml";
            var content = await fileService.LoadAsset("AudioMarkers", fileName);
            var xDoc = XDocument.Parse(content);
            var root = xDoc.Root;
            var markers = root.Descendants("Marker");

            List<int> astriskSeqIds = new List<int>();
            bool isAstriskPaper = _astriskDic.TryGetValue(paperId, out astriskSeqIds);
            foreach (var marker in markers)
            {
                int seqId = Int32.Parse(marker.Attribute("seqId").Value);
                if (isAstriskPaper)
                {
                    if (astriskSeqIds.Contains(seqId))
                    {
                        continue;
                    }
                }
                var newMarker = new AudioMarker(marker);
                Markers.Insert(newMarker);
                AudioMarkers.Add(newMarker);
            }
            return Markers;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    public async Task<AudioMarkerSequence> LoadAudioMarkersAsync(int paperId)
    {
        string _method = "LoadAudioMarkersAsync";
        try
        {
            var filePathName = @"AudioMarkers\" + paperId.ToString("000") + ".audio.xml";
            string targetFile = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, filePathName);

            using FileStream inputStream = System.IO.File.OpenRead(targetFile);
            using StreamReader reader = new StreamReader(inputStream);
            string _content = reader.ReadToEnd();

            var xDoc = XDocument.Parse(_content);
            var root = xDoc.Root;
            var markers = root.Descendants("Marker");

            List<int> astriskSeqIds = new List<int>();
            bool isAstriskPaper = _astriskDic.TryGetValue(paperId, out astriskSeqIds);
            foreach (var marker in markers)
            {
                int seqId = Int32.Parse(marker.Attribute("seqId").Value);
                if (isAstriskPaper)
                {
                    if (astriskSeqIds.Contains(seqId))
                    {
                        continue;
                    }
                }
                var newMarker = new AudioMarker(marker);
                Markers.Insert(newMarker);
                AudioMarkers.Add(newMarker);
            }
            return Markers;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<IList<AudioMarker>> GetAudioMarkersListAsync()
    {
        string _method = "GetAudioMarkersListAsync";

        try
        {
            List<AudioMarker> values = new();
            if (Markers.Size != 0)
            {
                List<AudioMarker> markers = Markers.Values().ToList();
                foreach (var value in markers)
                {
                    values.Add(value);
                }
            }
            return values;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task DisconnectMediaElementAsync()
    {
        string _method = "DisconnectMediaElementAsync";
        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                // Stop and cleanup MediaElement when we navigate away
                mediaElement.Stop();
                mediaElement.Handler?.DisconnectHandler();
            });
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="markers"></param>
    /// <returns></returns>
    public async Task SetMarkersAsync(AudioMarkerSequence markers)
    {
        string _method = "SetMarkersAsync";
        try
        {
            this.Markers = markers;
            var audioMarkers = Markers.Values();
            foreach (var marker in audioMarkers)
            {
                this.AudioMarkers.Add(marker);
            }
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task SetAudioStatusAsync(bool value)
    {
        string _method = "SetAudioStatusAsync";
        try
        {
            if (value)
            {
                AudioStatusFlag.SetAudioStatus(AudioFlag.AudioStatus.On);
                Preferences.Default.Set("audio_status", "on");
                await settingsService.Set("audio_status", "on");
            }
            else
            {
                AudioStatusFlag.SetAudioStatus(AudioFlag.AudioStatus.Off);
                Preferences.Default.Set("audio_status", "off");
                await settingsService.Set("audio_status", "off");
            }
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task SetAudioDownloadStatusAsync(bool value)
    {
        string _method = "SetAudioDownloadStatusAsync";
        try
        {
            if (value)
            {
                AudioDownloadStatus = true;
                Preferences.Default.Set("audio_download_status", "on");
                await settingsService.Set("audio_download_status", "on");
            }
            else
            {
                AudioDownloadStatus = false;
                Preferences.Default.Set("audio_download_status", "off");
                await settingsService.Set("audio_download_status", "off");
            }
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task SetAudioStreamingStatusAsync(bool value)
    {
        string _method = "SetAudioStreamingStatus";
        try
        {
            if (value)
            {
                AudioStreamingStatus = true;
                Preferences.Default.Set("stream_audio", true);
                await settingsService.Set("stream_audio", true);
            }
            else
            {
                AudioStreamingStatus = false;
                Preferences.Default.Set("stream_audio", false);
                await settingsService.Set("stream_audio", false);
            }
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<bool> GetAudioStatusAsync()
    {
        string _method = "GetAudioStatusAsync";
        try
        {
            bool _state = false;
            var state = AudioStatusFlag.State;
            switch (state)
            {
                case AudioFlag.AudioStatus.Off:
                    _state = false;
                    break;
                case AudioFlag.AudioStatus.On:
                    _state = true;
                    break;
            }
            return _state;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task SetMediaPlaybackControlsAsync(bool value)
    {
        string _method = "SetMediaPlaybackControlsAsync";
        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                // Stop and cleanup MediaElement when we navigate away
                mediaElement.ShouldShowPlaybackControls = value;
            });
            this.ShowPlaybackControls = value;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="audioMarker"></param>
    /// <returns></returns>
    public async Task SetPlaybackControlsStartTimeAsync(AudioMarker audioMarker)
    {
        string _method = "SetPlaybackControlsStartTimeAsync";
        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                mediaElement.SeekTo(audioMarker.StartTime);
            });
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task SetShowMediaPlaybackControlsAsync(bool value)
    {
        string _method = "SetShowMediaPlaybackControlsAsync";
        try
        {
            this.ShowPlaybackControls = value;
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="mediaStatePair"></param>
    /// <returns></returns>
    public async Task SetMediaStateAsync(MediaStatePair mediaStatePair)
    {
        string _method = "SetMediaStateAsync";
        try
        {
            this.MediaState = mediaStatePair;
            CurrentState = MediaState.CurrentState;
            PreviousState = MediaState.PreviousState;
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<MediaStatePair> GetMediaStateAsync()
    {
        string _method = "GetMediaStateAsync";
        try
        {
            MediaStatePair value = this.MediaState;
            return value;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    public async Task SetMediaSourceAsync(string uri)
    {
        string _method = "SetMediaSourceAsync";
        try
        {
            mediaElement.Source = MediaSource.FromUri(uri);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="action"></param>
    /// <param name="uri"></param>
    /// <returns></returns>
    public async Task SetMediaSourceAsync(string action, string uri)
    {
        string _method = "SetMediaSourceAsync";
        try
        {
            switch (action)
            {
                case "loadStreamingMp3":
                    mediaElement.Source = MediaSource.FromUri(uri);
                    return;
                case "resetSource":
                    mediaElement.Source = null;
                    return;
                case "loadFromLocalFile":
                    mediaElement.Source = MediaSource.FromFile(uri);
                    break;
                case "loadLocalResource":
                    if (DeviceInfo.Platform == DevicePlatform.MacCatalyst
                        || DeviceInfo.Platform == DevicePlatform.iOS)
                    {
                        mediaElement.Source = MediaSource.FromResource(uri);
                    }
                    else if (DeviceInfo.Platform == DevicePlatform.Android)
                    {
                        mediaElement.Source = MediaSource.FromResource(uri);
                    }
                    else if (DeviceInfo.Platform == DevicePlatform.WinUI)
                    {
                        mediaElement.Source = MediaSource.FromResource(uri);
                    }
                    return;
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    public async Task SetDurationAsync(TimeSpan duration)
    {
        string _method = "SetDurationAsync";
        try
        {
            this.Duration = duration;
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="platformName"></param>
    /// <returns></returns>
    public async Task SetPlatformAsync(string platformName)
    {
        string _method = "SetPlatformAsync";
        try
        {
            this.Plattform = platformName;
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="paperDto"></param>
    /// <returns></returns>
    public async Task SetPaperDtoAsync(PaperDto paperDto)
    {
        string _method = "SetPaperDtoAsync";
        try
        {
            this.PaperDto = paperDto;
            this.PaperId = paperDto.Id;
            this.PaperNumber = paperDto.Id.ToString("0");
            this.PaperTitle = paperDto.Title;
            this.TimeSpanString = paperDto.TimeSpan;
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="paragraphs"></param>
    /// <returns></returns>
    public async Task SetParagraphsAsync(List<Paragraph> paragraphs)
    {
        string _method = "SetParagraphsAsync";
        try
        {
            foreach (var paragraph in paragraphs)
            {
                this.Paragraphs.Add(paragraph);
            }
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task SetSendToastAsync(bool value)
    {
        string _method = "SetSendToastAsync";
        try
        {
            this.SendToastState = value;
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    public async Task DownloadAudioFile(string fileName)
    {
        string _method = "DownloadAudioFile";
        try
        {
            // Setup Uri and File Path
            //string uriBasePath = "https://s3.amazonaws.com/urantia/media/en/";
            //string uriFullPath = uriBasePath + fileName;
            //Uri uri = new Uri(uriFullPath);
            //string fileNamePath = Path.Combine(audioDir, "000.mp3");

            // This works, but WebClient is obsolete and need to do with HttpClient
            //if (false)
            //{
            //    WebClient wc = new WebClient();
            //    wc.DownloadFileAsync(uri, fileNamePath);
            //}

            // https://stackoverflow.com/questions/45711428/download-file-with-webclient-or-httpclient
            //if (false)
            //{
            //    HttpClient client = new();

            //    HttpResponseMessage response = await client.GetAsync(uri);
            //    if (response.IsSuccessStatusCode)
            //    {
            //        using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
            //        {

            //        }
            //    }
            //}

            // https://learn.microsoft.com/en-us/archive/blogs/henrikn/httpclient-downloading-to-a-local-file
            //if (true)
            //{
            //    HttpClient client = new();
            //    HttpResponseMessage response = await client.GetAsync(uri);
            //    if (response.IsSuccessStatusCode)
            //    {
            //        await response.Content.ReadAsFileAsync(fileNamePath, true);
            //    }
            //}

            //if (true)
            //{
            //    using (var client = new HttpClient())
            //    {
            //        var response = await client.GetAsync(uri);
            //        var result = response.EnsureSuccessStatusCode();
            //        if (result.IsSuccessStatusCode)
            //        {
            //            var stream = await response.Content.ReadAsStreamAsync();
            //            var fileInfo = new FileInfo(fileNamePath);
            //            using (var fileStream = fileInfo.OpenWrite())
            //            {
            //                await stream.CopyToAsync(fileStream);
            //            }
            //        }
            //        else
            //        {
            //            throw new Exception("File not found");
            //        }
            //    }
            //}

            // https://stackoverflow.com/questions/73403608/net-maui-c-sharp-background-task-continuewith-and-notification-event

            //await DownloadAudioFileAsync(fileName, audioDir);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
        }
    }
    #endregion

    #region  Audio Gestures
    public async Task TappedGestureForPaperAsync(string value)
    {
        string _method = "TappedGestureForPaper";
        try
        {
            if (contentPage == null)
                return;

            string paperTitle = PaperTitle;
            string message = $"Playing {paperTitle}";

            // bool currentState = await audioService.PlayPauseAsync(value)

            //if (CurrentState == "None")
            //{
            //    CurrentState = PreviousState;
            //    CurrentState = "Playing";
            //    message = $"Plyaing {paperTitle}";
            //    await PlayAudio();
            //}
            //else if (CurrentState == "Playing")
            //{
            //    CurrentState = PreviousState;
            //    CurrentState = "Paused";
            //    await PauseAudio();
            //    message = $"Pausing {paperTitle}";
            //}
            //else if (CurrentState == "Paused" || CurrentState == "Stopped")
            //{
            //    CurrentState = PreviousState;
            //    CurrentState = "Playing";
            //    await PlayAudio();
            //    message = $"Resume Playing {paperTitle}";
            //}
            //else
            //{
            //    string errorMsg = $"Current State = {CurrentState} Previous State = {PreviousState}";
            //    throw new Exception("Uknown State: " + errorMsg);
            //}

            //using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            //{
            //    ToastDuration duration = ToastDuration.Short;
            //    double fontSize = 14;
            //    var toast = Toast.Make(message, duration, fontSize);
            //    await toast.Show(cancellationTokenSource.Token);
            //}
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }
    public async Task DoubleTappedGestureForPaperAsync(string value)
    {
        string _method = "DoubleTappedGestureForPaperAsync";
        try
        {
            if (contentPage == null)
                return;

            string paperTitle = PaperTitle;
            string message = $"Stopping {paperTitle}";

            //if (CurrentState == "None")
            //{
            //    CurrentState = PreviousState;
            //    CurrentState = "Stopped";
            //    message = $"Setting Status Stopped for {paperTitle}";
            //    await StopAudio();
            //}
            //else if (CurrentState == "Playing" || CurrentState == "Paused")
            //{
            //    CurrentState = PreviousState;
            //    CurrentState = "Stopped";
            //    await StopAudio();
            //}
            //else
            //{
            //    string errorMsg = $"Current State = {CurrentState} Previous State = {PreviousState}";
            //    throw new Exception("Uknown State: " + errorMsg);
            //}

            //using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            //{
            //    ToastDuration duration = ToastDuration.Short;
            //    double fontSize = 14;
            //    var toast = Toast.Make(message, duration, fontSize);
            //    await toast.Show(cancellationTokenSource.Token);
            //}
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }
    public async Task TappedGestureAsync(string id) 
    {
        string _method = "TappedGestureAsync";
        try
        {
            if (contentPage == null)
                return;

            var audioSource = mediaElement.Source;

            if (!AudioFileDownloaded && !AudioStreamingStatus)
            {
                // Download Audio File and set MediaElement Source
                if (AudioDownloadFullPathName != null && AudioFolderExists)
                {
                    bool result = await downloadService.DownloadAudioFileAsync(AudioUri, AudioDownloadFullPathName);
                    if (result == true)
                    {
                        AudioFileDownloaded = true;
                        await SetMediaSourceAsync(AudioDownloadFullPathName);
                        audioSource = mediaElement.Source;
                    }
                }
            }

            var mediaElementCurrentState = mediaElement.CurrentState;
            CurrentState = MediaState.CurrentState;
            PreviousState = MediaState.PreviousState;

            int paperId = Int32.Parse(id.Substring(1, 3));
            int sequenceId = Int32.Parse(id.Substring(5, 3));
            var audioMarker = AudioMarkers.Where(m => m.SequenceId == sequenceId).FirstOrDefault();

            Label currentLabel = (Label)contentPage.FindByName(id);
            string uid = currentLabel.GetValue(AttachedProperties.Ubml.UniqueIdProperty) as string;
            // 001.000.000.000
            string[] arr = uid.Split('.');
            string pid = Int32.Parse(arr[1]).ToString("0")
                         + ":" +
                         Int32.Parse(arr[2]).ToString("0")
                         + "." +
                         Int32.Parse(arr[3]).ToString("0");

            string timeRange = audioMarker.MarkerRange;
            string message = $"Playing {pid} Timespan {timeRange}";

            // Initial State -> Trigger Play
            if (CurrentState == "None" &&
                PreviousState == "None")
            {
                PreviousState = "Paused";
                CurrentState = "Playing";
                await StateChangedAsync("Playing");
                await PlayAudioRangeExAsync(audioMarker);
            }
            // Play State -> Tappeed Event -> 
            // || PreviousState = "Paused" */
            else if (CurrentState == "Playing" &&
                     PreviousState == "Paused" ||
                     PreviousState == "None")
            {
                PreviousState = CurrentState;
                CurrentState = "Paused";
                await StateChangedAsync("Paused");
                await PauseAudioAsync();
                message = $"Pausing {pid} Timespan {timeRange}";
            }
            // Playing State -> Play Trigger
            else if (CurrentState == "Paused" &&
                     PreviousState == "Playing")
            {
                PreviousState = CurrentState;
                CurrentState = "Playing";
                await StateChangedAsync("Playing");
                await PlayAudioAsync();
                message = $"Resuming {pid} Timespan {timeRange}";
            }
            // Play State -> Reach Marker Event -> 
            // || PreviousState = "Playing" */
            else if (CurrentState == "Stopped" &&
                     PreviousState == "Playing")
            {
                PreviousState = "Paused";
                CurrentState = "Playing";
                await StateChangedAsync("Playing");
                await PlayAudioRangeExAsync(audioMarker);
                message = $"Playing {pid} Timespan {timeRange}";
            }
            else
            {
                string errorMsg = $"Current State = {CurrentState} Previous State = {PreviousState}";
                throw new Exception("Uknown audio State: " + errorMsg);
            }

            if (SendToastState)
            {
                await SendToastAsync(message);
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }
    public async Task TappedGestureAsync(string id, bool value)
    {
        string _method = "TappedGestureAsync";
        try
        {
            if (contentPage == null)
                return;

            var mediaElementCurrentState = mediaElement.CurrentState;
            CurrentState = MediaState.CurrentState;
            PreviousState = MediaState.PreviousState;
            SendToastState = value;

            int paperId = Int32.Parse(id.Substring(1, 3));
            int sequenceId = Int32.Parse(id.Substring(5, 3));
            var audioMarker = AudioMarkers.Where(m => m.SequenceId == sequenceId).FirstOrDefault();

            Label currentLabel = (Label)contentPage.FindByName(id);
            string uid = currentLabel.GetValue(AttachedProperties.Ubml.UniqueIdProperty) as string;
            // 001.000.000.000
            string[] arr = uid.Split('.');
            string pid = Int32.Parse(arr[1]).ToString("0")
                         + ":" +
                         Int32.Parse(arr[2]).ToString("0")
                         + "." +
                         Int32.Parse(arr[3]).ToString("0");

            string timeRange = audioMarker.MarkerRange;
            string message = $"Playing {pid} Timespan {timeRange}";

            // Initial State -> Trigger Play
            if (CurrentState == "None" &&
                PreviousState == "None")
            {
                PreviousState = CurrentState;
                CurrentState = "Playing";
                await StateChangedAsync("Playing");
                await PlayAudioRangeExAsync(audioMarker);
            }
            // Play State -> Tappeed Event -> 
            /* || PreviousState = "Paused" */
            else if (CurrentState == "Playing" &&
                     PreviousState == "None" ||
                     PreviousState == "Paused")
            {
                PreviousState = CurrentState;
                CurrentState = "Paused";
                await StateChangedAsync("Paused");
                await PauseAudioAsync();
                message = $"Pausing {pid} Timespan {timeRange}";
            }
            //// Playing State -> Play Trigger
            else if (CurrentState == "Paused" &&
                     PreviousState == "Playing")
            {
                PreviousState = CurrentState;
                CurrentState = "Playing";
                await StateChangedAsync("Playing");
                await PlayAudioAsync();
                message = $"Resuming {pid} Timespan {timeRange}";
            }
            // Play State -> Reach Marker Event -> 
            /* || PreviousState = "Playing" */
            else if (CurrentState == "Stopped" &&
                     PreviousState == "Playing")
            {
                PreviousState = CurrentState;
                CurrentState = "Playing";
                await StateChangedAsync("Playing");
                await PlayAudioRangeExAsync(audioMarker);
                message = $"Playing {pid} Timespan {timeRange}";
            }
            else
            {
                string errorMsg = $"Current State = {CurrentState} Previous State = {PreviousState}";
                throw new Exception("Uknown State: " + errorMsg);
            }

            if (SendToastState)
            {
                await SendToastAsync(message);
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }
    public async Task DoubleTappedGestureAsync(string id)
    {
        string _method = "DoubleTappedGesture";
        try
        {
            if (contentPage == null)
                return;

            var mediaElementCurrentState = mediaElement.CurrentState;
            CurrentState = MediaState.CurrentState;
            PreviousState = MediaState.PreviousState;

            Label currentLabel = (Label)contentPage.FindByName(id);
            string timeSpanRange = currentLabel.GetValue(AttachedProperties.Audio.TimeSpanProperty) as string;
            string timeSpanRangeMsg = timeSpanRange.Replace("_", " - ");
            string uid = currentLabel.GetValue(AttachedProperties.Ubml.UniqueIdProperty) as string;
            // 001.000.000.000
            string[] arr = uid.Split('.');
            string pid = Int32.Parse(arr[1]).ToString("0")
                         + ":" +
                         Int32.Parse(arr[2]).ToString("0")
                         + "." +
                         Int32.Parse(arr[3]).ToString("0");

            //string format = @"dd\:hh\:mm\:ss\.fffffff";
            // Console.WriteLine("The time difference is: {0}", ts.ToString(format));
            //string format = @"dd\:hh\:mm\:ss\.fffffff";

            //string format = @"hh\:mm\:ss\.ff";
            //var hrs = timeSpan.TotalHours;
            //var min = timeSpan.TotalMinutes;
            //var sec = timeSpan.TotalSeconds;
            //var str1 = timeSpan.ToShortTimeString();
            //var str2 = timeSpan.ToString(format);

            string message = $"Stopping {pid} Timespan {timeSpanRangeMsg}";

            await StopAudioAsync();
            await StateChangedAsync("Stopped");
            if (SendToastState)
            {
                await SendToastAsync(message);
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }
    public async Task DoubleTappedGestureAsync(string id, bool value)
    {
        string _method = "DoubleTappedGestureAsync";
        try
        {
            if (contentPage == null)
                return;

            var mediaElementCurrentState = mediaElement.CurrentState;
            CurrentState = MediaState.CurrentState;
            PreviousState = MediaState.PreviousState;
            SendToastState = value;

            Label currentLabel = (Label)contentPage.FindByName(id);
            string timeSpanRange = currentLabel.GetValue(AttachedProperties.Audio.TimeSpanProperty) as string;
            string timeSpanRangeMsg = timeSpanRange.Replace("_", " - ");
            string uid = currentLabel.GetValue(AttachedProperties.Ubml.UniqueIdProperty) as string;
            // 001.000.000.000
            string[] arr = uid.Split('.');
            string pid = Int32.Parse(arr[1]).ToString("0")
                         + ":" +
                         Int32.Parse(arr[2]).ToString("0")
                         + "." +
                         Int32.Parse(arr[3]).ToString("0");

            //string format = @"dd\:hh\:mm\:ss\.fffffff";
            // Console.WriteLine("The time difference is: {0}", ts.ToString(format));
            //string format = @"dd\:hh\:mm\:ss\.fffffff";

            //string format = @"hh\:mm\:ss\.ff";
            //var hrs = timeSpan.TotalHours;
            //var min = timeSpan.TotalMinutes;
            //var sec = timeSpan.TotalSeconds;
            //var str1 = timeSpan.ToShortTimeString();
            //var str2 = timeSpan.ToString(format);

            string message = $"Stopping {pid} Timespan {timeSpanRangeMsg}";

            await StopAudioAsync();

            if (SendToastState)
            {
                await SendToastAsync(message);
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }
    #endregion

    #region   MediaElement Audio Methods
    public async Task<bool> PlayPauseAsync(string value)
    {
        string _method = "PlayPauseAsync";
        try
        {
            bool retval = false;

            string _state = MediaState.CurrentState;
            PreviousState = CurrentState;
            CurrentState = _state;
            MediaState.SetState(_state);
            if (_state == "Playing")
            {

            }
            if (_state == "Paused")
            {

            }
            return retval;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return false;
        }
    }
    public async Task PlayAudioAsync()
    {
        string _method = "PlayAudioAsync";
        try
        {
            string _state = "Playing";
            PreviousState = CurrentState;
            CurrentState = _state;
            MediaState.SetState(_state);

            //string message = $"Playing {pid} Timespan {timeRange}";
            var paperId = PaperDto.Id;
            var paperTitle = PaperDto.Title;
            var pid = PaperDto.Pid;
            var timeSpan = PaperDto.TimeSpan;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (mediaElement != null)
                {
                    mediaElement.Play();
                }
            });
      
            string message = $"Playing Audio";
            if (SendToastState)
            {
                await SendToastAsync(message);
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }
    public async Task PauseAudioAsync()
    {
        string _method = "PauseAudioAsync";
        try
        {
            string _state = "Paused";
            PreviousState = CurrentState;
            CurrentState = _state;
            MediaState.SetState(_state);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (mediaElement != null)
                {
                    mediaElement.Pause();
                }
            });
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }
    public async Task StopAudioAsync()
    {
        string _method = "StopAudioAsync";
        try
        {
            string _state = "Stopped";
            PreviousState = CurrentState;
            CurrentState = _state;
            MediaState.SetState(_state);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (mediaElement != null)
                {
                    mediaElement.Stop();
                    mediaElement.MediaEnded();
                }
            });
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }
    public async Task PlayAudioRangeAsync(string timeSpanRange)
    {
        string _method = "PlayAudioRangeAsync";
        try
        {
            string _state = "Playing";
            PreviousState = CurrentState;
            CurrentState = _state;
            MediaState.SetState(_state);

            char[] separators = { ':', '.' };
            string[] arry = timeSpanRange.Split('_');
            string[] sa = arry[0].Split(separators, StringSplitOptions.RemoveEmptyEntries);
            string[] ea = arry[1].Split(separators, StringSplitOptions.RemoveEmptyEntries);
            TimeSpan start = new TimeSpan(0, Int32.Parse(sa[0]), Int32.Parse(sa[1]), Int32.Parse(sa[2]), Int32.Parse(sa[3]));
            TimeSpan end = new TimeSpan(0, Int32.Parse(ea[0]), Int32.Parse(ea[1]), Int32.Parse(ea[2]), Int32.Parse(ea[3]));

            StartTime = start;
            EndTime = end;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                mediaElement.SeekTo(start);
                mediaElement.Play();
            });
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }
    public async Task PlayAudioRangeExAsync(AudioMarker audioMarker)
    {
        string _method = "PlayAudioRangeExAsync";
        try
        {
            string _state = "Playing";
            PreviousState = CurrentState;
            CurrentState = _state;
            MediaState.SetState(_state);

            TimeSpan start = audioMarker.StartTime;
            TimeSpan end = audioMarker.EndTime;

            StartTime = start;
            EndTime = end;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                mediaElement.SeekTo(start);
                mediaElement.Play();
            });
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }
    public async Task PositionChangedAsync(TimeSpan timeSpan)
    {
        string _method = "PositionChangedAsync";
        try
        {
            Position = timeSpan;
            if (EndTime.ToShortTimeString() == timeSpan.ToShortTimeString())
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (mediaElement != null)
                    {
                        mediaElement.Stop();
                        mediaElement.MediaEnded();
                    }
                });
                var currentState = mediaElement.CurrentState.ToString();
                MediaElementMediaState.SetState(currentState);
                MediaState.SetState("Stopped");
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }
    public async Task StateChangedAsync(string state)
    {
        string _method = "StateChangedAsync";
        try
        {
            var currentState = state;
            MediaElementMediaState.SetState(state);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }
    public async Task DownloadAudioFileAsync(string fileName, string audioDir)
    {
        string _method = "DownloadAudioFileAsync";
        try
        {
            // Setup Uri and File Path
            string uriBasePath = "https://s3.amazonaws.com/urantia/media/en/";
            string uriFullPath = uriBasePath + fileName;
            Uri uri = new Uri(uriFullPath);
            string fileNamePath = Path.Combine(audioDir, "000.mp3");

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(uri);
                var result = response.EnsureSuccessStatusCode();
                if (result.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    var fileInfo = new FileInfo(fileNamePath);
                    using (var fileStream = fileInfo.OpenWrite())
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                }
                else
                {
                    throw new Exception("File not found");
                }
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
        }
    }
    public async Task DownloadAudioFileAsync(Uri uri, string audioDir)
    {
        string _method = "DownloadAudioFileAsync";
        try
        {
            // Setup Uri and File Path
            string fileNamePath = Path.Combine(audioDir, "000.mp3");

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(uri);
                var result = response.EnsureSuccessStatusCode();
                if (result.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    var fileInfo = new FileInfo(fileNamePath);
                    using (var fileStream = fileInfo.OpenWrite())
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                }
                else
                {
                    throw new Exception("File not found");
                }
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
        }
    }
    public async Task SendToastAsync(string message)
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
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }
    #endregion
}
