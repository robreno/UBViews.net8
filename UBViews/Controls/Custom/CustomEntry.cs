namespace UBViews.Controls;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Custom Entry Control in .NET Maui #2 - Now working on iOS as well
// https://youtu.be/GnZOYgG5Ibg?si=M4-1kEXem_aPjr3d
// https://github.com/gzadro/.NET-MAUI-custom-Entry-control/tree/main

public sealed class CustomEntry : Entry
{
    public static BindableProperty CornerRadiusProperty =
             BindableProperty.Create(nameof(CornerRadius), typeof(int), typeof(CustomEntry), 0);

    public static BindableProperty BorderThicknessProperty =
        BindableProperty.Create(nameof(BorderThickness), typeof(int), typeof(CustomEntry), 0);

    public static BindableProperty PaddingProperty =
        BindableProperty.Create(nameof(Padding), typeof(Thickness), typeof(CustomEntry), new Thickness(5));

    public static BindableProperty BorderColorProperty =
        BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(CustomEntry), Colors.Transparent);

    public static BindableProperty CustomHeightProperty =
        BindableProperty.Create(nameof(CustomHeight), typeof(int), typeof(CustomEntry), 0);

    public int CornerRadius
    {
        get => (int)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public int BorderThickness
    {
        get => (int)GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }
    public Color BorderColor
    {
        get => (Color)GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }
    /// <summary>
    /// This property cannot be changed at runtime in iOS.
    /// </summary>
    public Thickness Padding
    {
        get => (Thickness)GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    public int CustomHeight
    {
        get => (int)GetValue(CustomHeightProperty);
        set => SetValue(CustomHeightProperty, value);
    }
}
