namespace UBViews;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        Task.Run(async () => { await AppInitData(true); });

        MainPage = new AppShell();
    }
}
