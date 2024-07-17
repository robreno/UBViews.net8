namespace UBViews.Helpers;

using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Linq;

using UBViews.Services;
using UBViews.Models.AppData;
using UBViews.Models.Contacts;

// Performing CRUD Operation In .NET MAUI (SQLite)
// Multi Page
// https://youtu.be/JRNwjsywrWM?si=t1n5weZn6y7O7ErA

// CRUD Operations with SQLite for Xamarin.Forms and .NET MAUI (Gerald)
// Single Page
// https://youtu.be/8_cqUvriwM8?si=OuzK-JnJ2EdduwdT

// Playlist Builder - Part 1 , Building a .NET MAUI app from scratch (Daniel Hindrikes)
// https://youtu.be/s8sQHZ4845k?si=rNbtWtkym2k1s22s

// STEP 6. How to Create CRUD Employees APP in NET MAUI .NET 7.0|| CRUD .NET MAUI APP Using SQLITE (Macro Code)
// FlyoutMenu and Top Tool Bar Menu on Android
// https://youtu.be/II9Klgr8gPg?si=MVTqRUyi6jxnH4ve

// .NET MAUI Full CRUD With SQLite | MVVM | Ep:10
// https://youtu.be/F31e4siUvV4?si=7daJOkAYFqloj-Xv


public class XmlContactsService : IContactsService
{
    /// <summary>
    /// Private Data Members
    /// </summary>
    private const string _contactsFileName = "Contacts.xml";
    private XDocument _contactsXDoc;
    private XElement _contactsRoot;
    private string _content;
    private string _appDir;
    private int _contactCount;
    private bool _cacheDirty;
    //private int _cacheCount;

    private bool _initialized = false;
    private bool _dataInitialized = false;

    private readonly string _class = "XmlContactsService";

    ObservableCollection<Contact> Contacts = new();

    /// <summary>
    /// 
    /// </summary>
    IFileService fileService;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileService"></param>
    public XmlContactsService(IFileService fileService)
    {
        this.fileService = fileService;

        // TODO: Fix to use patten used other services
        // remove from cstor
        Task.Run( async () => await InitializeData());
    }

    #region Private Initialization Methods
    private async Task InitializeDataAsync()
    {
        string _method = "InitializeDataAsync";
        try
        {
            //// Sets _initialzed to true if successful
            //var loadContentResult = await LoadContentAsync();
            //if (!loadContentResult)
            //{
            //    throw new Exception("Initialization Exception: Settings file failed to load.");
            //}
            //// Sets _dataInitialized to true if successful
            //var loadDataResult = await InitializeSettingsAsync();
            //if (!loadDataResult)
            //{
            //    throw new Exception("Initialization Exception: Loading date failed.");
            //}
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }
    private async Task<bool> InitializeSettingsAsync()
    {
        string _method = "InitializeSettingsAsync";
        try
        {
            if (!_initialized)
            {
                throw new Exception("Initialization Exception.");
            }

            var contacts = _contactsRoot.Descendants("Contact");
            foreach (var contact in contacts)
            {
                Contact newContact = new Contact();
                var id = contact.Attribute("id").Value;
                var autoSendEmail = contact.Attribute("autoSendEmail").Value;

                var firstName = contact.Descendants("FirstName").FirstOrDefault();
                var lastName = contact.Descendants("LastName").FirstOrDefault();
                var displayName = contact.Descendants("DisplayName").FirstOrDefault();
                var email = contact.Descendants("Email").FirstOrDefault();

                newContact.Id = Int32.Parse(id);
                newContact.AutoSendEmail = autoSendEmail == "false" ? false : true;
                newContact.FirstName = firstName == null ? "Unknown" : firstName.Value;
                newContact.LastName = lastName == null ? "Unknown" : lastName.Value;
                newContact.DisplayName = displayName == null ? "Unknown" : displayName.Value;
                newContact.Email = email == null ? "Unknown" : email.Value;
                Contacts.Add(newContact);
            }
            _dataInitialized = true;
            return _dataInitialized;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return false;
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// InitializeData
    /// </summary>
    /// <returns></returns>
    private async Task InitializeData()
    {
        string _method = "Initialize";
        try
        {
            // TODO: Update and move
            _appDir = FileSystem.Current.AppDataDirectory;
            _content = await LoadContactsAsync(_contactsFileName);
            _contactsXDoc = XDocument.Parse(_content, LoadOptions.None);
            _contactsRoot = _contactsXDoc.Root;
            _contactCount = Int32.Parse(_contactsRoot.Attribute("count").Value);
            _cacheDirty = false;
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    // Count Methods
    private async Task<int> GetCurrentCountAsync()
    {
        string _method = "GetCurrentCount";

        try
        {
            var _count = Int32.Parse(_contactsRoot.Attribute("count").Value);
            return _count;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return 0;
        }
    }
    private async Task<int> IncrementCountAsync()
    {
        string _method = "IncrementCount";

        try
        {
            var _count = Int32.Parse(_contactsRoot.Attribute("count").Value);
            _contactsRoot.SetAttributeValue("count", ++_count);
            _cacheDirty = true;
            return _count;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return 0;
        }
    }
    private async Task<int> DecrementCountAsync()
    {
        string _method = "DecrementCount";

        try
        {
            var _count = Int32.Parse(_contactsRoot.Attribute("count").Value);
            _contactsRoot.SetAttributeValue("count", --_count);
            _cacheDirty = true;
            return _count;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return 0;
        }
    }

    // ContactId Methods
    private async Task<int> GetLastContactId()
    {
        string _method = "GetLastContactId";

        try
        {
            var _lastId = Int32.Parse(_contactsRoot.Attribute("lastId").Value);
            return _lastId;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return 0;
        }
    }
    private async Task<int> ResetLastContactId()
    {
        string _method = "ResetContactId";

        try
        {
            _contactsRoot.SetAttributeValue("lastId", 0);
            _cacheDirty = true;
            return 0;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return 0;
        }
    }
    private async Task<int> IncrementContactId()
    {
        string _method = "GetLastContactId";

        try
        {
            var _lastId = Int32.Parse(_contactsRoot.Attribute("lastId").Value);
            _contactsRoot.SetAttributeValue("lastId", ++_lastId);
            _cacheDirty = true;
            return _lastId;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return 0;
        }
    }

    /// <summary>
    /// LoadContactsAsync
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns>Contacts Xml content as string</returns>
    private async Task<string> LoadContactsAsync(string fileName)
    {
        string _method = "LoadContactsAsync";

        try
        {
            string targetFile = System.IO.Path.Combine(_appDir, fileName);
            using FileStream inputStream = System.IO.File.OpenRead(targetFile);
            using StreamReader reader = new StreamReader(inputStream);
            string _content = reader.ReadToEnd();
            return _content;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// SaveContactsAsync
    /// </summary>
    /// <returns></returns>
    public async Task SaveContactsAsync()
    {
        string _method = "SaveContactsAsync";

        try
        {
            if (_cacheDirty)
            {
                string targetFile = System.IO.Path.Combine(_appDir, _contactsFileName);
                _contactsRoot.Save(targetFile, SaveOptions.None);
                _cacheDirty = false;
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    /// <summary>
    /// SaveContactAsyn
    /// </summary>
    /// <param name="contact"></param>
    /// <returns></returns>
    public async Task<int> SaveContactAsync(ContactDto contact)
    {
        string _method = "SaveContactAsync";

        try
        {
            string message = string.Empty;

            var _displayNameTaken = await DisplayNameExistsAsync(contact.DisplayName);
            if (_displayNameTaken)
            {
                message = "Display name taken! Please enter another Dissplay Name.";
                await App.Current.MainPage.DisplayAlert($"Save Contact", message, "Ok");
                return 0;
            }

            int _nextId = 0;
            contact.Id = _nextId = await IncrementContactId();

            XElement newContact = new XElement("Contact",
                                    new XAttribute("id", contact.Id),
                                    new XAttribute("autoSendEmail", contact.AutoSendEmail),
                                    new XElement("FirstName", contact.FirstName),
                                    new XElement("LastName", contact.LastName),
                                    new XElement("DisplayName", contact.DisplayName),
                                    new XElement("Email", contact.Email));

            _contactsRoot.Add(newContact);
            await IncrementCountAsync();
            await SaveContactsAsync();

            message = "Successfully Saved!";
            await App.Current.MainPage.DisplayAlert($"Save Contact", message, "Ok");

            return _nextId;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return 0;
        }
    }

    /// <summary>
    /// GetContactsAsync
    /// </summary>
    /// <returns>List of ContactDto</returns>
    public async Task<List<ContactDto>> GetContactsAsync()
    {
        string _method = "GetContactsAsync";

        try
        {
            List<ContactDto> contacts = new();
            var contactElms = _contactsRoot.Descendants("Contact");
            foreach (var elm in contactElms) 
            {
                var id = Int32.Parse(elm.Attribute("id").Value);
                var autoSendEmail = "false" == elm.Attribute("autoSendEmail").Value ? false : true;
                var firstName = elm.Element("FirstName").Value;
                var lastName = elm.Element("LastName").Value;
                var displayName = elm.Element("DisplayName").Value;
                var email = elm.Element("Email").Value;
                var newContact = new ContactDto()
                {
                    Id = id,
                    AutoSendEmail = autoSendEmail,
                    FirstName = firstName,
                    LastName = lastName,
                    DisplayName = displayName,
                    Email = email
                };
                contacts.Add(newContact);
            }
            return contacts;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }

    /// <summary>
    /// GetAutoSendContactsAsync
    /// </summary>
    /// <returns></returns>
    public async Task<List<ContactDto>> GetAutoSendContactsAsync()
    {
        string _method = "GetAutoSendContactsAsync";

        try
        {
            List<ContactDto> autoSendContacts = new();
            var contactElms = _contactsRoot.Descendants("Contact");
            foreach (var elm in contactElms)
            {
                var id = Int32.Parse(elm.Attribute("id").Value);
                var autoSendEmail = "false" == elm.Attribute("autoSendEmail").Value ? false : true;
                var firstName = elm.Element("FirstName").Value;
                var lastName = elm.Element("LastName").Value;
                var displayName = elm.Element("DisplayName").Value;
                var email = elm.Element("Email").Value;
                if (autoSendEmail)
                {
                    var newContact = new ContactDto()
                    {
                        Id = id,
                        AutoSendEmail = autoSendEmail,
                        FirstName = firstName,
                        LastName = lastName,
                        DisplayName = displayName,
                        Email = email
                    };
                    autoSendContacts.Add(newContact);
                }
            }
            return autoSendContacts;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }

    /// <summary>
    ///  GetAutoSendEmailList
    /// </summary>
    /// <returns></returns>
    public async Task<List<string>> GetAutoSendEmailListAsync()
    {
        string _method = "GetAutoSendEmailList";

        try
        {
            List<string> autoSendList = new();

            List<ContactDto> contacts = new();
            var contactElms = _contactsRoot.Descendants("Contact");
            foreach (var elm in contactElms)
            {
                var autoSendEmail = "false" == elm.Attribute("autoSendEmail").Value ? false : true;
                var email = elm.Element("Email").Value;
                if (autoSendEmail)
                {
                    autoSendList.Add(email);
                }
            }
            return autoSendList;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }

    /// <summary>
    /// GetContactByIdAsyn
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ContactDto> GetContactByIdAsync(string id)
    {
        string _method = "GetContactByIdAsync";

        try
        {
            ContactDto _contact = null;

            var contactElm = _contactsRoot.Descendants("Contact")
                                          .Where(c => c.Attribute("id").Value == id)
                                          .FirstOrDefault();
            if (contactElm != null)
            {
                var contactId = Int32.Parse(contactElm.Attribute("id").Value);
                var autoSendEmail = contactElm.Attribute("autoSendEmail").Value;
                var firstNameElm = contactElm.Descendants("FirstName").FirstOrDefault();
                var lastNameElm = contactElm.Descendants("LastName").FirstOrDefault();
                var displayNameElm = contactElm.Descendants("DisplayName").FirstOrDefault();
                var emailElm = contactElm.Descendants("Email").FirstOrDefault();

                _contact = new ContactDto
                {
                    Id = contactId,
                    AutoSendEmail = autoSendEmail == "true" ? true : false,
                    FirstName = firstNameElm.Value,
                    LastName = lastNameElm.Value,
                    DisplayName = displayNameElm.Value,
                    Email = emailElm.Value
                };

            }
            return _contact;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }

    /// <summary>
    /// GetContactByDisplayNameAsync
    /// </summary>
    /// <param name="displayName"></param>
    /// <returns></returns>
    public async Task<ContactDto> GetContactByDisplayNameAsync(string displayName)
    {
        string _method = "GetContactByIdAsync";

        try
        {
            ContactDto _contact = null;

            var contacts = _contactsRoot.Descendants("Contact");

            foreach (var contact in contacts)
            {
                var _displayName = contact.Element("DisplayName").Value;
                if (_displayName == displayName)
                {
                    var id = contact.Attribute("id").Value;
                    _contact = await GetContactByIdAsync(id);
                }
            }
            return _contact;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }

    /// <summary>
    /// GetContactElementByIdAsync
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<XElement> GetContactElementByIdAsync(string id)
    {
        string _method
            = "GetContactElmByIdAsync";

        try
        {
            var _contactElm = _contactsRoot.Descendants("Contact")
                                           .Where(c => c.Attribute("id").Value == id)
                                           .FirstOrDefault();
            return _contactElm;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }

    /// <summary>
    /// EqualToAsync
    /// </summary>
    /// <param name="oldContact"></param>
    /// <param name="newContact"></param>
    /// <returns></returns>
    public async Task<bool> EqualToAsync(XElement oldContact, ContactDto newContact)
    {
        string _method = "EqualToAsync";

        try
        {
            var isEqual = false;

            if (oldContact != null)
            {
                var contactId = Int32.Parse(oldContact.Attribute("id").Value);
                var autoSendEmail = oldContact.Attribute("autoSendEmail").Value;
                var firstNameElm = oldContact.Descendants("FirstName").FirstOrDefault();
                var lastNameElm = oldContact.Descendants("LastName").FirstOrDefault();
                var displayNameElm = oldContact.Descendants("DisplayName").FirstOrDefault();
                var emailElm = oldContact.Descendants("Email").FirstOrDefault();

                var _oldContact = new ContactDto
                {
                    Id = contactId,
                    AutoSendEmail = autoSendEmail == "true" ? true : false,
                    FirstName = firstNameElm.Value,
                    LastName = lastNameElm.Value,
                    DisplayName = displayNameElm.Value,
                    Email = emailElm.Value
                };

                isEqual = _oldContact.EqualTo(newContact);
            }
            return isEqual;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return false;
        }
    }

    /// <summary>
    /// UpdateContactAsync
    /// </summary>
    /// <param name="newContact"></param>
    /// <returns></returns>
    public async Task<int> UpdateContactAsync(ContactDto newContact)
    {
        string _method = "UpdateContactAsync";

        try
        {
            string message = string.Empty;

            var contactId = newContact.Id;
            var id = contactId.ToString("0");

            var oldContact = await GetContactElementByIdAsync(id);
            var isEqual = await EqualToAsync(oldContact, newContact);

            if (!isEqual)
            {
                XElement _contact = new XElement("Contact",
                                        new XAttribute("id", newContact.Id),
                                        new XAttribute("autoSendEmail", newContact.AutoSendEmail),
                                        new XElement("FirstName", newContact.FirstName),
                                        new XElement("LastName", newContact.LastName),
                                        new XElement("DisplayName", newContact.DisplayName),
                                        new XElement("Email", newContact.Email));

                oldContact.ReplaceWith(_contact);

                _cacheDirty = true;
                await SaveContactsAsync();

                message = "Successfully Updated!";
                await App.Current.MainPage.DisplayAlert($"Update Contact", message, "Ok");
            }
            return contactId;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return 0;
        }
    }

    /// <summary>
    /// DeleteContactAsync
    /// </summary>
    /// <param name="contact"></param>
    /// <returns></returns>
    public async Task<int> DeleteContactAsync(ContactDto contact)
    {
        string _method = "DeleteContactAsync";

        try
        {
            string message = string.Empty;

            int contactId = contact.Id;
            var id = contact.Id.ToString("0");

            var oldContact = _contactsRoot.Descendants("Contact")
                                          .Where(c => c.Attribute("id").Value == id)
                                          .FirstOrDefault();

            oldContact.Remove();

            int _count = await DecrementCountAsync();
            if (_count == 0) 
            {
                var _lastId = await ResetLastContactId();
                await SaveContactsAsync();
            }
            else
            {
                await SaveContactsAsync();
            }

            message = "Successfully Deleted!";
            await App.Current.MainPage.DisplayAlert($"Update Contact", message, "Ok");

            return contactId;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return 0;
        }
    }

    /// <summary>
    /// DisplayNameExistsAsync
    /// </summary>
    /// <param name="displayName"></param>
    /// <returns></returns>
    public async Task<bool> DisplayNameExistsAsync(string displayName)
    {
        string _method = "DisplayNameExistsAsync";

        try
        {
            var _displayNameExists = false;

            var _contacts = await GetContactsAsync();
            var result = _contacts.Where(c => c.DisplayName == displayName).FirstOrDefault();
            if (result != null)
            {
                _displayNameExists = true;
            }
            return _displayNameExists;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return false;
        }
    }
    #endregion

    #region Private Helper Methods
    private async Task<DateTime> CreateDateTimeAsync(string date, string time)
    {
        string _method = "CreateDateTimeAsync";
        try
        {
            char[] delimiterChars = { '/', ':', '.' };
            var dateArr = date.Split(delimiterChars);
            var timeArr = time.ToString().Split(delimiterChars);
            var hour = Int32.Parse(timeArr[0]);
            var minute = Int32.Parse(timeArr[1]);
            var second = Int32.Parse(timeArr[2]);
            var day = Int32.Parse(dateArr[0]);
            var month = Int32.Parse(dateArr[1]);
            var year = Int32.Parse(dateArr[2]);
            DateTime dateTime = new DateTime(year, month, day, hour, minute, second);
            return dateTime;
        }
        catch (Exception ex)
        {
            var exceptionMessage = $"Exception raised in {_class}.{_method} => " + ex.Message;
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            throw new Exception(exceptionMessage);
        }
    }
    #endregion
}
