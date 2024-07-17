namespace UBViews.Controls;

using Microsoft.Maui.Controls;

public class BorderlessEntry : Entry
{
    // Decare BindableProperty
    public static BindableProperty CornerRadiusProperty =
             BindableProperty.Create(nameof(CornerRadius), typeof(int), typeof(CustomEntry), 0);

    // Getter and Setter for BindableProperty
    public int CornerRadius
    {
        get => (int)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
}
