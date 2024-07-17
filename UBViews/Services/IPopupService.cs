namespace UBViews.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UBViews.Controls.Help;
using UBViews.ViewModels;

public interface IPopupService
{
    Task ShowPopup(string popupName);
    Task ClosePopup();
}
