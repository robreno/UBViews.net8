namespace UBViews.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Alerts;

using UBViews.Models;
using UBViews.Services;
using UBViews.Extensions;

using System.Net.Http;

public class DownloadService : IDownloadService
{
    #region  Data Members
    /// <summary>
    /// ContentPage
    /// </summary>
    private ContentPage contentPage;

    readonly string _class = "DownloadService";
    #endregion

    #region   Services
    IAppSettingsService settingsService;
    #endregion

    public DownloadService(IAppSettingsService settingsService)
    {
        this.settingsService = settingsService;
    }

    #region  Public Properties
    public bool ValidAudioDownloadPath { get; set; } = false;
    public bool ValidAudioUriPath { get; set; } = false;
    public bool AudioFolderExists { get; set; } = false;
    public bool AudioFileExists { get; set; } = false;
    public bool Initialized { get; set; } = false;
    public PaperDto PaperDto { get; set; } = null;
    public string AudioUriString { get; set; } = null;
    public Uri AudioUri { get; set; } = null;
    public string AudioFileName { get; set; } = null;
    public bool StreamAudio { get; set; } = false;
    public string AudioStatus { get; set; }
    public string AudioDownloadPath { get; set; } = null;
    public string AudioDownloadFullPathName { get; set; } = null;
    public string LocalStatePath { get; set; } = FileSystem.AppDataDirectory;
    #endregion

    #region  Interface Implementations
    public async Task  InitializeDataAsync(ContentPage contentPage, PaperDto dto)
    {
        string _method = "InitializeDataAsync";
        try
        {
            this.contentPage = contentPage;
            PaperDto = dto;
            AudioFileName = PaperDto.Id.ToString("000") + ".mp3";

            var hasValue = contentPage.Resources.TryGetValue("audioUri", out object uri);
            if (hasValue)
            {
                AudioUriString = (string)uri;
                AudioUri = new Uri(AudioUriString);
                ValidAudioUriPath = true;
            }

            AudioStatus = await settingsService.Get("audio_status", "off");
            StreamAudio = await settingsService.Get("stream_audio", false);
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
                    AudioFileExists = File.Exists(AudioDownloadFullPathName);
                }
            }
            Initialized = (ValidAudioUriPath && ValidAudioDownloadPath);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }
    public async Task<bool> DownloadAudioFileAsync(Uri sourceUri, string targetFullPathName)
    {
        string _method = "DownloadAudioFileAsync";
        try
        {
            bool isSuccess = false;
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(sourceUri);
                var result = response.EnsureSuccessStatusCode();
                if (result.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    var fileInfo = new FileInfo(targetFullPathName);
                    using (var fileStream = fileInfo.OpenWrite())
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                    isSuccess = true;
                }
                else
                {
                    throw new Exception("File not found");
                }
            }
            return isSuccess;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return false;
        }
    }
    public async Task<bool> DownloadAudioFileExAsync(Uri sourceUri, string targetFullPathName)
    {
        // See: https://github.com/erossini/HttpClientMultipart/blob/main/ClientSideApp/DemoHttpClient.cs
        string _method = "DownloadAudioFileExAsync";
        try
        {
            if (sourceUri == null)
                throw new ArgumentNullException(nameof(sourceUri.OriginalString), "URL is empty.");

            var sourceUriString = sourceUri.OriginalString;

            bool isSuccess = true;
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(sourceUriString);
                response.EnsureSuccessStatusCode();

                await using var ms = await response.Content.ReadAsStreamAsync();
                await using var fs = File.Create(targetFullPathName);
                ms.Seek(0, SeekOrigin.Begin);
                ms.CopyTo(fs);
            }
            return isSuccess;
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
