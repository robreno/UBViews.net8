namespace UBViews.Services;

using UBViews.Models.AppData;
using System.Xml.Linq;

public interface IXmlContactsService
{
    Task SaveContactsAsync();
    Task<List<ContactDto>> GetContactsAsync();
    Task<ContactDto> GetContactByIdAsync(string id);
    Task<ContactDto> GetContactByDisplayNameAsync(string displayName);
    Task<int> SaveContactAsync(ContactDto contact);
    Task<int> UpdateContactAsync(ContactDto contact);
    Task<int> DeleteContactAsync(ContactDto contact);
    Task<bool> DisplayNameExistsAsync(string displayName);
}
