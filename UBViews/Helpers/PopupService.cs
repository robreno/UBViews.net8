namespace UBViews.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Views;
using UBViews.Controls.Help;
using UBViews.ViewModels;

public class PopupService
{
    public Popup popupPage;

    readonly string _class = nameof(PopupService);

    public PopupService() 
    { 
        
    }

    public PopupService(Popup popup)
    {
        this.popupPage = popup;
    }

    public async Task ShowPopup(string popupName)
    {
        string _method = "ShowPopuup";
        try
        {
            Popup popup;
            string target = popupName;
            if (popupPage != null)
            {
                if (popupName == "DownloadFolderPopup")
                {
                    popup = new AudioOverviewPopup(new PopupViewModel());
                    Shell.Current.CurrentPage.ShowPopup(popup);
                }
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    public async Task ClosePopup()
    {
        string _method = "ClosePopup";
        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (popupPage != null)
                {
                    popupPage.Close();
                }
            });
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }
}
