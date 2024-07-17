namespace UBViews.Services;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;

using UBViews.Models;
using UBViews.Models.Audio;
using UBViews.Models.Ubml;

public interface IAudioService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="contentPage"></param>
    /// <param name="mediaElement"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    Task InitializeDataAsync(ContentPage contentPage, IMediaElement mediaElement, PaperDto dto);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<bool> IsInitializedAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="contentPage"></param>
    /// <returns></returns>
    Task SetContentPageAsync(ContentPage contentPage);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mediaElement"></param>
    /// <returns></returns>
    Task SetMediaElementAsync(IMediaElement mediaElement);

    /// <summary>
    /// Get MediaMarker at index position.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    Task<AudioMarker> GetAtAsync(int index);

    /// <summary>
    /// Removes all elements from the sequence.
    /// </summary>
    Task ClearAync();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="audioMarker"></param>
    Task InsertAsync(AudioMarker audioMarker);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<IList<AudioMarker>> ValuesAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<IList<AudioMarker>> GetAudioMarkersListAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="paperId"></param>
    /// <returns></returns>
    Task<AudioMarkerSequence> LoadAudioMarkersAsync(int paperId);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task DisconnectMediaElementAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="markers"></param>
    /// <returns></returns>
    Task SetMarkersAsync(AudioMarkerSequence markers);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Task SetAudioStatusAsync(bool value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Task SetAudioDownloadStatusAsync(bool value);

   /// <summary>
   /// 
   /// </summary>
   /// <param name="value"></param>
   /// <returns></returns>
    Task SetAudioStreamingStatusAsync(bool value);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<bool> GetAudioStatusAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Task SetMediaPlaybackControlsAsync(bool value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="audioMarker"></param>
    /// <returns></returns>
    Task SetPlaybackControlsStartTimeAsync(AudioMarker audioMarker);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Task SetShowMediaPlaybackControlsAsync(bool value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mediaStatePair"></param>
    /// <returns></returns>
    Task SetMediaStateAsync(MediaStatePair mediaStatePair);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<MediaStatePair> GetMediaStateAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    Task SetMediaSourceAsync(string uri);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="action"></param>
    /// <param name="uri"></param>
    /// <returns></returns>
    Task SetMediaSourceAsync(string action, string uri);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    Task SetDurationAsync(TimeSpan duration);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="platformName"></param>
    /// <returns></returns>
    Task SetPlatformAsync(string platformName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="paperDto"></param>
    /// <returns></returns>
    Task SetPaperDtoAsync(PaperDto paperDto);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="paragraphs"></param>
    /// <returns></returns>
    Task SetParagraphsAsync(List<Paragraph> paragraphs);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Task SetSendToastAsync(bool value);

    Task TappedGestureForPaperAsync(string value);
    Task DoubleTappedGestureForPaperAsync(string value);
    Task TappedGestureAsync(string value);
    Task TappedGestureAsync(string value, bool sendToast);
    Task DoubleTappedGestureAsync(string id);

    Task<bool> PlayPauseAsync(string value);
    Task PlayAudioAsync();
    Task PauseAudioAsync();
    Task StopAudioAsync();
    Task PlayAudioRangeAsync(string timeSpanRange);
    Task PlayAudioRangeExAsync(AudioMarker audioMarker);
    Task PositionChangedAsync(TimeSpan timeSpan);
    Task StateChangedAsync(string state);
    Task DownloadAudioFileAsync(string fileName, string audioDir);
    Task DownloadAudioFileAsync(Uri uri, string audioDir);
    Task SendToastAsync(string message);
}
