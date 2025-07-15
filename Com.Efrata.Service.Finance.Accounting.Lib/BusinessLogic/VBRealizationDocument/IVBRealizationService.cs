using Com.Efrata.Service.Finance.Accounting.Lib.Models.VBRealizationDocument;
using Com.Efrata.Service.Finance.Accounting.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.Efrata.Service.Finance.Accounting.Lib.BusinessLogic.VBRealizationDocument
{
    public interface IVBRealizationService
    {
        PostingJournalDto ReadByReferenceNo(string referenceNo);
        ReadResponse<VBRealizationDocumentModel> Read(int page, int size, string order, List<string> select, string keyword, string filter);
        Task<Tuple<VBRealizationDocumentModel, List<VBRealizationDocumentExpenditureItemModel>, List<VBRealizationDocumentUnitCostsItemModel>, List<VBRealizationDocumentFileModel>, List<string>>> ReadByIdAsync(int id);
    }
}
