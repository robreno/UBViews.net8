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
    public string PaperName { get; set; } = null;
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
            PaperName = PaperDto.Id.ToString("000") + ".mp3";

            var hasValue = contentPage.Resources.TryGetValue("audioUri", out object uri);
            if (hasValue)
            {
                AudioUriString = (string)uri;
                AudioUri = new Uri(AudioUriString);
                ValidAudioUriPath = true;
            }

            // LocalState/AudioFiles
            // C:\Users\robre\AppData\Local\Packages\879ca98e-d45e-44b3-9be6-e6d900695058_9zz4h110yvjzm\LocalState
            string audioFolderPath = await settingsService.Get("audio_folder_path", "");
            string audioFolderName = await settingsService.Get("audio_folder_name", "");
            if (!string.IsNullOrEmpty(audioFolderPath) && !string.IsNullOrEmpty(audioFolderName))
            {
                if (audioFolderName.Equals("[Empty]"))
                {
                    ValidAudioDownloadPath = false;
                }
                else if (audioFolderPath == "LocalState\\AudioFiles")
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
                    AudioDownloadFullPathName = Path.Combine(AudioDownloadPath, PaperName);
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
    public async Task<bool> DownloadAudioFileAsync()
    {
        string _method = "DownloadAudioFileAsync";
        try
        {
            bool isSuccess = false;
            if (File.Exists(AudioDownloadFullPathName))
            {
                isSuccess = true;
            }
            else
            {
                if (ValidAudioDownloadPath)
                {
                    using (var client = new HttpClient())
                    {
                        var response = await client.GetAsync(AudioUri);
                        var result = response.EnsureSuccessStatusCode();
                        if (result.IsSuccessStatusCode)
                        {
                            var stream = await response.Content.ReadAsStreamAsync();
                            var fileInfo = new FileInfo(AudioDownloadFullPathName);
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
                else
                {
                    throw new Exception("Invalid Audio Path.");
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
    public async Task<bool> DownloadAudioFileExAsync()
    {
        string _method = "DownloadAudioFileAsync";
        try
        {
            bool isSuccess = false;

            if (ValidAudioDownloadPath)
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(AudioUri);
                    var result = response.EnsureSuccessStatusCode();
                    if (result.IsSuccessStatusCode)
                    {
                        var stream = await response.Content.ReadAsStreamAsync();
                        var fileInfo = new FileInfo(AudioDownloadFullPathName);
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
            else
            {
                throw new Exception("Invalid Audio Path.");
            }

        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return false;
        }
    }
    public async Task<bool> DownloadAudioFileAsync(string fileName, string audioDir)
    {
        string _method = "DownloadAudioFileAsync";
        try
        {
            bool isSuccess = false;

            // Setup Uri and File Path

            string uriBasePath = AudioUriString;
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
    public async Task<bool> DownloadAudioFileAsync(Uri uri, string audioDir)
    {
        string _method = "DownloadAudioFileAsync";
        try
        {
            bool isSuccess = false;
            // Setup Uri and File Path
            string fileName = PaperDto.Id.ToString("000") + ".mp3";
            string fileNamePath = Path.Combine(audioDir, fileName);

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

    // See: https://www.mauicestclair.fr/en/posts/tutos/my-first-app/13-download-audio-file/
    //public async Task DownloadCurrentAudioFile(CancellationToken cancellation)
    //{
    //    // We raise an exception when cancellation is requested
    //    cancellationToken.ThrowIfCancellationRequested();

    //    try
    //    {
    //        // We need an HTTP client to send our request through the network
    //        HttpClient client = new HttpClient();
    //        client.MaxResponseContentBufferSize = 100000000; // We can download up to ~100MB of data per file!

    //        // We send an HTTP request to the link for downloading audio
    //        using var httpResponse =
    //            await client.GetAsync(
    //                new Uri(CurrentTrack.AudioDownloadURL), cancellationToken);

    //        httpResponse.EnsureSuccessStatusCode();

    //        var downloadedImage = await httpResponse.Content.ReadAsStreamAsync(cancellationToken);

    //        try
    //        {
    //            string fileName = $"{CurrentTrack.Title} - {CurrentTrack.Author}.mp3";

    //            // The retrieved data is then transferred to a file
    //            // Note: we need CommunityToolkit.Maui to be updated to 5.1.0 at least
    //            var fileSaveResult = await FileSaver.SaveAsync(fileName, downloadedImage, cancellationToken);

    //            fileSaveResult.EnsureSuccess();

    //            await Toast.Make($"File saved at: {fileSaveResult.FilePath}").Show(cancellationToken);
    //        }
    //        catch (Exception ex)
    //        {
    //            await Toast.Make($"Cannot save file because: {ex.Message}").Show(cancellationToken);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        await Toast.Make($"Cannot download file because: {ex.Message}").Show(cancellationToken);
    //    }
    //}

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
