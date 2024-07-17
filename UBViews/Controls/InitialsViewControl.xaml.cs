namespace UBViews.Controls;



[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class InitialsViewControl : ContentView
{
    public static readonly BindableProperty DefaultBackgroundColorProperty
            = BindableProperty.Create(nameof(DefaultBackgroundColor), typeof(Color),
                typeof(InitialsViewControl), Color.Parse("LightGray"));

    public Color DefaultBackgroundColor
    {
        get => (Color)GetValue(DefaultBackgroundColorProperty);
        set => SetValue(DefaultBackgroundColorProperty, value);
    }

    public static readonly BindableProperty TextColorLightProperty
            = BindableProperty.Create(nameof(TextColorLight), typeof(Color),
                typeof(InitialsViewControl), Color.Parse("White"));

    public Color TextColorLight
    {
        get => (Color)GetValue(TextColorLightProperty);
        set => SetValue(TextColorLightProperty, value);
    }

    public static readonly BindableProperty TextColorDarkProperty
            = BindableProperty.Create(nameof(TextColorDark), typeof(Color),
                typeof(InitialsViewControl), Color.Parse("Black"));

    public Color TextColorDark
    {
        get => (Color)GetValue(TextColorDarkProperty);
        set => SetValue(TextColorDarkProperty, value);
    }

    public static readonly BindableProperty NameProperty
           = BindableProperty.Create(nameof(Name), typeof(string),
               typeof(InitialsViewControl), string.Empty,
               propertyChanged: OnNamePropertyChanged);

    public string Name
    {
        get => (string)GetValue(NameProperty);
        set => SetValue(NameProperty, value);
    }

    private static void OnNamePropertyChanged(BindableObject bindable,
            object oldValue, object newValue)
    {
        if (!(bindable is InitialsViewControl initialsView))
            return;

        initialsView.SetName((string)newValue);
    }

    public InitialsViewControl()
    {
        InitializeComponent();
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (WidthRequest == -1 || HeightRequest == -1)
        {
            InitControl(50);
        }
        else
        {
            InitControl(Math.Min(WidthRequest, HeightRequest));
        }
    }

    private void InitControl(double size)
    {
        // set width and height of contentboxview
        ContentBoxView.HeightRequest = size;
        ContentBoxView.WidthRequest = size;

        // calculate corner radius of contentboxview
        ContentBoxView.CornerRadius = size / 2;

        // set default background
        ContentBoxView.BackgroundColor = DefaultBackgroundColor;

        // set fontsize
        ContentLabel.FontSize = (size / 2) - 5;

        // check if name is already present
        if (!string.IsNullOrEmpty(Name))
            SetName(Name);
    }

    private void SetName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            ContentLabel.Text = string.Empty;
            ContentBoxView.BackgroundColor = DefaultBackgroundColor;
            return;
        }

        var words = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 1)
        {
            ContentLabel.Text = words[0][0].ToString();
        }
        else if (words.Length > 1)
        {
            var initialsString = words[0][0].ToString() + words[words.Length - 1][0].ToString();
            ContentLabel.Text = initialsString;
        }
        else
        {
            ContentLabel.Text = string.Empty;
        }

        SetColors(name);
    }

    private void SetColors(string name)
    {
        // get color for the provided text
        var hexColor = "#FF" + Convert.ToString(name.GetHashCode(), 16).Substring(0, 6);

        // fix issue if value is too short
        if (hexColor.Length == 8)
            hexColor += "5";

        // create color from hex value
        var color = Color.FromArgb(hexColor);

        // set backgroundcolor of contentboxview
        ContentBoxView.BackgroundColor = color;

        // get brightness and set textcolor
        var brightness = color.Red * .3 + color.Green * .59 + color.Blue * .11;
        ContentLabel.TextColor = brightness < 0.5 ? TextColorLight : TextColorDark;
    }
}