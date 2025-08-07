using Com.Efrata.Service.Finance.Accounting.Lib.BusinessLogic.AzureStorage;
using Com.Efrata.Service.Finance.Accounting.Lib.BusinessLogic.Services.JournalTransaction;
using Com.Efrata.Service.Finance.Accounting.Lib.BusinessLogic.VBRequestDocument;
using Com.Efrata.Service.Finance.Accounting.Lib.Models.VBRealizationDocument;
using Com.Efrata.Service.Finance.Accounting.Lib.Services.HttpClientService;
using Com.Efrata.Service.Finance.Accounting.Lib.Services.IdentityService;
using Com.Efrata.Service.Finance.Accounting.Lib.Utilities;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.Efrata.Service.Finance.Accounting.Lib.BusinessLogic.VBRealizationDocument
{
    public class VBRealizationService : IVBRealizationService
    {
        private const string UserAgent = "finance-service";
        public readonly FinanceDbContext _dbContext;

        private readonly IIdentityService _identityService;
        private readonly IServiceProvider _serviceProvider;

        public VBRealizationService(FinanceDbContext dbContext, IServiceProvider serviceProvider)
        {
            _dbContext = dbContext;
            _identityService = serviceProvider.GetService<IIdentityService>();
            _serviceProvider = serviceProvider;
        }

        private IAzureDocumentService AzureDocumentService
        {
            get { return this._serviceProvider.GetService<IAzureDocumentService>(); }
        }


        public ReadResponse<VBRealizationDocumentModel> Read(int page, int size, string order, List<string> select, string keyword, string filter)
        {
            var query = _dbContext.Set<VBRealizationDocumentModel>().AsQueryable();

            List<string> searchAttributes = new List<string>()
            {
                "DocumentNo", "SuppliantUnitName","VBRequestDocumentNo","VBRequestDocumentCreatedBy"
            };

            query = QueryHelper<VBRealizationDocumentModel>.Search(query, searchAttributes, keyword);

            var filterDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(filter);
            query = QueryHelper<VBRealizationDocumentModel>.Filter(query, filterDictionary);

            var orderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(order);
            query = QueryHelper<VBRealizationDocumentModel>.Order(query, orderDictionary);

            var pageable = new Pageable<VBRealizationDocumentModel>(query, page - 1, size);
            var data = pageable.Data.ToList();

            int TotalData = pageable.TotalCount;

            return new ReadResponse<VBRealizationDocumentModel>(data, TotalData, orderDictionary, new List<string>());
        }

        public async Task<Tuple<VBRealizationDocumentModel, List<VBRealizationDocumentExpenditureItemModel>, List<VBRealizationDocumentUnitCostsItemModel>, List<VBRealizationDocumentFileModel>, List<string>>> ReadByIdAsync(int id)
        {
            var data = await _dbContext.VBRealizationDocuments.FirstOrDefaultAsync(s => s.Id == id);

            if (data == null)
                return new Tuple<VBRealizationDocumentModel, List<VBRealizationDocumentExpenditureItemModel>, List<VBRealizationDocumentUnitCostsItemModel>, List<VBRealizationDocumentFileModel>, List<string>>(null, new List<VBRealizationDocumentExpenditureItemModel>(), new List<VBRealizationDocumentUnitCostsItemModel>(), new List<VBRealizationDocumentFileModel>(), null);

            var items = _dbContext.VBRealizationDocumentExpenditureItems.Where(s => s.VBRealizationDocumentId == id).ToList();
            var expenditureIds = items.Select(s => s.Id);
            var unitCostItems = new List<VBRealizationDocumentUnitCostsItemModel>();
            var documentFile = new List<VBRealizationDocumentFileModel>();
            var documentPath = new List<string>();
            if (data.Type == VBType.WithPO)
            {

                unitCostItems = _dbContext.VBRealizationDocumentUnitCostsItems.Where(s => expenditureIds.Contains(s.VBRealizationDocumentExpenditureItemId)).ToList();
            }
            else
            {
                unitCostItems = _dbContext.VBRealizationDocumentUnitCostsItems.Where(s => s.VBRealizationDocumentId == id).ToList();

            }

            documentFile = _dbContext.VBRealizationDocumentFiles.Where(s => s.VBRealizationDocumentId == id).ToList();

            if (documentFile.Count() > 0)
            {


                var documentPathList = documentFile.Select(s => s.DocumentsPath).ToList();

                documentPath = await AzureDocumentService.DownloadMultipleFiles2("vb-finance", documentPathList);

            }

            return new Tuple<VBRealizationDocumentModel, List<VBRealizationDocumentExpenditureItemModel>, List<VBRealizationDocumentUnitCostsItemModel>, List<VBRealizationDocumentFileModel>, List<string>>(data, items, unitCostItems, documentFile, documentPath);
        }

        public PostingJournalDto ReadByReferenceNo(string referenceNo)
        {
            var result = _dbContext.VBRealizationDocuments.Where(entity => entity.ReferenceNo == referenceNo).Select(entity => new PostingJournalDto(entity.VBRequestDocumentNo, entity.DocumentNo)).FirstOrDefault();
            return result;
        }
    }
}
