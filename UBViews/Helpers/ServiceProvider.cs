namespace UBViews.Helpers;

using Microsoft.Maui.Controls.Internals;

public static class ServiceHelper
{
    public static TService GetService<TService>()
        => Current.GetService<TService>();

    /* TODO:
        1>C:\Archive\GitHub\UBViews_2024\UBViews\UBViews\Helpers\ServiceProvider.cs(14,4,14,46): warning CS0618: 
        'MauiUIApplicationDelegate.Services' is obsolete: 'Use the IPlatformApplication.Current.Services instead.'
     * 
     *  https://stackoverflow.com/questions/77484020/net-7-net-8-mauiuiapplicationdelegate-could-not-be-found
     */
    public static IServiceProvider Current =>
#if WINDOWS10_0_17763_0_OR_GREATER
			MauiWinUIApplication.Current.Services;
#elif ANDROID
            MauiApplication.Current.Services;
#elif IOS || MACCATALYST
            //MauiUIApplicationDelegate.Current.Services;
            null;
#else
			null;
#endif
}