namespace UBViews.Services;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UBViews.Models;

public interface IDownloadService
{
    Task InitializeDataAsync(ContentPage contentPage, PaperDto dto);
    Task<bool> DownloadAudioFileAsync(Uri sourceUri, string targetFullPathName);
    Task<bool> DownloadAudioFileExAsync(Uri sourceUri, string targetFullPathName);
}
