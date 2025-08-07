using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Com.Efrata.Service.Finance.Accounting.Lib.BusinessLogic.AzureStorage.AzureDocumentService;

namespace Com.Efrata.Service.Finance.Accounting.Lib.BusinessLogic.AzureStorage
{
    public interface IAzureDocumentService
    {

        Task<string> UploadMultipleFile(string moduleName, int id, DateTime _createdUtc, List<string> filesBase64, string filesNameString, string beforeFilePaths);
        Task<DocumentFileResult> DownloadDocument(string documentPath);
        Task RemoveMultipleFile(string moduleName, string filesPath);
        Task<string> DownloadFile(string moduleName, string filePath);
        Task<List<string>> DownloadMultipleFiles(string moduleName, string filesPath);

        Task<List<string>> DownloadMultipleFiles2(string moduleName, List<string> filesPath);
    }
}
