﻿namespace UBViews.Services;

using UBViews.Models.AppData;
using System.Xml.Linq;

public interface IXmlAppDataService
{
    Task<string> LoadAppDataAsync(string filename);
    Task SaveAppDataAsync(string filename, string content);
    Task SaveAppDataExAsync(string fileName);
    Task AddQueryResult(XElement queryResult);
    Task<List<AppFileDto>> GetAppFilesAsync();
    Task<(bool, int)> QueryResultExistsAsync(string query);
    Task<QueryResultLocationsDto> GetQueryResultByIdAsync(int queryId);
    Task<QueryResultLocationsDto> GetQueryResultAsync(string query);
    Task<List<QueryCommandDto>> GetQueryCommandsAsync();
}
