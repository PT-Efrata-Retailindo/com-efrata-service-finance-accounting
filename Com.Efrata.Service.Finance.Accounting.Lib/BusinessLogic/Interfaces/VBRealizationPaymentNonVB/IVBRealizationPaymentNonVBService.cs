using Com.Efrata.Service.Finance.Accounting.Lib.Models.VBRealizationDocument;
using Com.Efrata.Service.Finance.Accounting.Lib.Utilities;
using Com.Efrata.Service.Finance.Accounting.Lib.Utilities.BaseInterface;
using Com.Efrata.Service.Finance.Accounting.Lib.ViewModels.VBRealizationDocumentNonPO;
using Com.Efrata.Service.Finance.Accounting.Lib.ViewModels.VBRealizationPaymentNonVB;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.Efrata.Service.Finance.Accounting.Lib.BusinessLogic.Interfaces.VBRealizationDocumentNonPO
{
    public interface IVBRealizationPaymentNonVBService
    {
        ReadResponse<VBRealizationDocumentModel> Read(int page, int size, string order, List<string> select, string keyword, string filter);
        ReadResponse<VBRealizationDocumentModel> ReadByUser(int page, int size, string order, List<string> select, string keyword, string filter);
        Task<int> CreateAsync(VBRealizationPaymentNonVBViewModel model);
        Task<VBRealizationPaymentNonVBViewModel> ReadByIdAsync(int id);
        Task<int> UpdateAsync(int id, VBRealizationPaymentNonVBViewModel model);
        Task<int> DeleteAsync(int id);
    }
}
