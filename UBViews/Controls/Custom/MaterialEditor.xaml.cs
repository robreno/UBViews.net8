namespace UBViews.Controls;

using UBViews.ViewModels;

public partial class MaterialEditor : ContentView
{
    private int _xOffset;
    private int _yOffset;
    private readonly Color _primary;

    private double _xOffsetDelta;
    private double _yOffsetDelta;

    public MaterialEditor()
	{
		InitializeComponent();

        if (DeviceInfo.Current.Platform == DevicePlatform.Android)
        {
            _xOffsetDelta = 4.0;
            _yOffsetDelta = 32.5;
        }
        else if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
        {
            _xOffsetDelta = 4.0;
            _yOffsetDelta = 7.5;
        }
        else if (DeviceInfo.Current.Platform == DevicePlatform.iOS ||
                 DeviceInfo.Current.Platform == DevicePlatform.MacCatalyst)
        {
            _xOffsetDelta = 4.0;
            _yOffsetDelta = 7.5;
        }

        var rd = App.Current!.Resources.MergedDictionaries.First();
        _primary = (Color)rd["Primary"];

        meEditor.ZIndex = 2;
        meBorder.ZIndex = 2;
        meLabel.ZIndex = 3;

        BindingContext = this;
    }

    // NewGet Package auto bindable checkout
    public static BindableProperty LabelProperty =
             BindableProperty.Create(nameof(Label), typeof(string), typeof(MaterialEntry), null);
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static BindableProperty TextProperty =
             BindableProperty.Create(nameof(Text), typeof(string), typeof(MaterialEntry), null);
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static BindableProperty NameProperty =
             BindableProperty.Create(nameof(Name), typeof(string), typeof(MaterialEntry), null);
    public string Name
    {
        get => (string)GetValue(NameProperty);
        set => SetValue(NameProperty, value);
    }

    public static BindableProperty SubjectProperty =
             BindableProperty.Create(nameof(Subject), typeof(string), typeof(MaterialEntry), null);
    public string Subject
    {
        get => (string)GetValue(SubjectProperty);
        set => SetValue(SubjectProperty, value);
    }

    public static BindableProperty DateCreatedProperty =
             BindableProperty.Create(nameof(DateCreated), typeof(string), typeof(MaterialEntry), null);
    public string DateCreated
    {
        get => (string)GetValue(DateCreatedProperty);
        set => SetValue(DateCreatedProperty, value);
    }

    public static BindableProperty DateEditedProperty =
             BindableProperty.Create(nameof(DateCreated), typeof(string), typeof(MaterialEntry), null);
    public string DateEdited
    {
        get => (string)GetValue(DateEditedProperty);
        set => SetValue(DateEditedProperty, value);
    }

    public static BindableProperty AuthorProperty =
             BindableProperty.Create(nameof(Author), typeof(string), typeof(MaterialEntry), null);
    public string Author
    {
        get => (string)GetValue(AuthorProperty);
        set => SetValue(AuthorProperty, value);
    }

    private void meEditor_Focused(object sender, FocusEventArgs e)
    {
        meBorder.Stroke = _primary;
        meLabel.TextColor = _primary;

        (_xOffset, _yOffset) = GetOffsets(new Point(meEditor.Bounds.Size),
                                          new Point(meEditor.Bounds.Size),
                                          new Point(meEditor.Bounds.Center.X, meEditor.Bounds.Center.Y),
                                          new Point(meEditor.Bounds.Center.X, meEditor.Bounds.Center.Y));

        ScaleLabelDown();
    }

    private void meEditor_Unfocused(object sender, FocusEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(meEditor.Text))
        {
            ScaleLabelUp();
        }
    }

    private void ScaleLabelDown()
    {
        // OnFocus ScaleLabelDown()
        // TranslationX and TranslationY just indicate
        // the Offset of View, not the coordinate .
        meLabel.ZIndex = 3;
        meLabel.ScaleTo(0.8, 250, Easing.Linear);
        meLabel.TranslateTo(_xOffset, _yOffset, 250, Easing.Linear);
    }

    private void ScaleLabelUp()
    {
        // OnUnfocus ScaleLabeUp()
        meLabel.ZIndex = 1;
        meLabel.ScaleTo(1, 250, Easing.Linear);
        meLabel.TranslateTo(0, 0, 250, Easing.Linear);
    }

    private (int, int) GetOffsets(Point entrySizePoint, Point labelSizePoint,
                                  Point entryCenterPoint, Point labelCenterPoint)
    {
        var meEntryPoint = entrySizePoint;
        var meLabelPoint = labelSizePoint;
        var d1 = meEntryPoint.X - meLabelPoint.X;
        var d2 = meEntryPoint.Y - meLabelPoint.Y;
        var d1Pow = Math.Pow(d1, 2);
        var d2Pow = Math.Pow(d2, 2);
        var sqrtPoint = Math.Sqrt(d1Pow + d2Pow) / 2 + _xOffsetDelta;
        _xOffset = Convert.ToInt32(sqrtPoint * -1);

        meEntryPoint = entryCenterPoint;
        meLabelPoint = labelCenterPoint;
        d1 = meEntryPoint.X - meLabelPoint.X;
        d2 = meEntryPoint.Y - meLabelPoint.Y;
        d1Pow = Math.Pow(d1, 2);
        d2Pow = Math.Pow(d2, 2);
        sqrtPoint = Math.Sqrt(d1Pow + d2Pow) + _yOffsetDelta;
        _yOffset = Convert.ToInt32(sqrtPoint * -1) / 2;
        return (_xOffset, _yOffset);
    }
}