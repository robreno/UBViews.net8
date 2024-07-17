namespace UBViews.Controls.Custom;
public class CustomBorder : Border
{
    public static BindableProperty CornerRadiusProperty =
             BindableProperty.Create(nameof(CornerRadius), typeof(int), typeof(CustomEntry), 0);

    public static BindableProperty BorderThicknessProperty =
        BindableProperty.Create(nameof(BorderThickness), typeof(int), typeof(CustomEntry), 0);

    //public static BindableProperty PaddingProperty =
    //    BindableProperty.Create(nameof(Padding), typeof(Thickness), typeof(CustomEntry), new Thickness(5));

    public static BindableProperty BorderColorProperty =
        BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(CustomEntry), Colors.Transparent);

    public static BindableProperty CustomHeightProperty =
        BindableProperty.Create(nameof(CustomHeight), typeof(int), typeof(CustomEntry), 0);

    public static BindableProperty CustomNameProperty =
        BindableProperty.Create(nameof(CustomName), typeof(int), typeof(CustomEntry), string.Empty);

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
    public new Thickness Padding
    {
        get => (Thickness)GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }
    public int CustomHeight
    {
        get => (int)GetValue(CustomHeightProperty);
        set => SetValue(CustomHeightProperty, value);
    }

    public string CustomName
    {
        get => (string)GetValue(CustomNameProperty);
        set => SetValue(CustomNameProperty, value);
    }
}
