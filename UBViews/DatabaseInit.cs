using System.Reflection;
using System.Xml.Linq;

//using UBViews.Helpers;
//using UBViews.Services;

// See: https://learn.microsoft.com/en-us/dotnet/maui/data-cloud/database-sqlite

namespace UBViews
{
    public partial class AppShell
    {
        private readonly string _class = "AppShell";
        private string innerCause = string.Empty;
        private string innerMessage = string.Empty;

        #region Database Paths and Data Members 
        private string _appLocalState = FileSystem.Current.AppDataDirectory;
        private string _appLocalCache = FileSystem.Current.CacheDirectory;
        private string _plDatabaseName = "postingLists.db3";
        private string _postingsPathName = Path.Combine(FileSystem.Current.AppDataDirectory, "postingLists.db3");
        private string _qrDatabaseName = "queryResults.db3";
        private string _queriesPathName = Path.Combine(FileSystem.Current.AppDataDirectory, "queryResults.db3");
        #endregion

        #region Database Initialization and Table Generation Methods
        public async Task InitializeData()
        {
            string _method = "InitializeData";
            try
            {
                Preferences.Default.Set("CurrentAppDataDirectory", _appLocalState);
                Preferences.Default.Set("CurrentCacheDirectory", _appLocalCache);
                Preferences.Default.Set("PostingDBName", _plDatabaseName);
                Preferences.Default.Set("PostingDBPath", _postingsPathName);
                Preferences.Default.Set("QueryDBName", _qrDatabaseName);
                Preferences.Default.Set("QueryDBPath", _queriesPathName);

                if (!File.Exists(_postingsPathName))
                    await CopyDatabase(_plDatabaseName, _postingsPathName);

                if (!File.Exists(_queriesPathName))
                    await CopyDatabase(_qrDatabaseName, _queriesPathName);

                return;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in " +
                    $"{_class}.{_method} => ", ex.Message, "Ok");
                return;
            }
        }
        public async Task CopyDatabase(string databaseName, string targetPath)
        {
            string _method = "CopyDatabase";
            try
            {
                var assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
                if (assembly != null)
                {
                    var manfests = assembly.GetManifestResourceNames();
                    if (manfests != null)
                    {
                        string rootPath = "UBViews.Resources.Raw.Database.";
                        using (Stream stream = assembly.GetManifestResourceStream(rootPath + databaseName))
                        {
                            if (stream != null)
                            {
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    stream.CopyTo(ms);
                                    File.WriteAllBytes(targetPath, ms.ToArray());
                                }
                            }
                            else
                            {
                                innerCause = "assembly.GetManifestResourceStream(rootPath + databaseName)" +
                                    " returned null result.";
                                innerMessage = $"CopyDatabase error: {innerCause}";
                                throw new NullReferenceException(innerMessage);
                            }
                        }
                    }
                    else
                    {
                        innerCause = "assembly.GetManifestResourceNames() returned null result.";
                        innerMessage = $"CopyDatabase error: {innerCause}";
                        throw new NullReferenceException(innerMessage);
                    }
                }
                else
                {
                    innerCause = "assembly.GetManifestResourceNames() returned null result.";
                    innerMessage = $"CopyDatabase error: {innerCause}";
                    throw new NullReferenceException(innerMessage);
                }
                return;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in " +
                    $"{_class}.{_method} => ", ex.Message, "Ok");
                return;
            }
        }
        #endregion
    }
}
