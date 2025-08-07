using Com.Efrata.Service.Finance.Accounting.Lib.ViewModels.Memo;
using Com.Efrata.Service.Finance.Accounting.Lib.ViewModels.VBRealizationDocumentNonPO;
using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Com.Efrata.Service.Finance.Accounting.Lib.ViewModels.VBRealizationPaymentNonVB.VBRealizationPaymentNonVBViewModel;

namespace Com.Efrata.Service.Finance.Accounting.Lib.Models.VBRealizationDocument
{
    public class VBRealizationDocumentFileModel : StandardEntity
    {
        public string DocumentsFileName { get; set; }
        public string DocumentsPath { get; set; }
        public decimal DocumentAmount { get; set; }
        public int VBRealizationDocumentId { get; private set; }

        public VBRealizationDocumentFileModel()
        { 
        
        }

        public VBRealizationDocumentFileModel(int vbRealizationDocumentId, DocumentFileViewModel viewModel)
        {
            DocumentsFileName = viewModel.documentName;
            DocumentsPath = viewModel.documentsPath;
            DocumentAmount = viewModel.amount;
            VBRealizationDocumentId = vbRealizationDocumentId;
        }

        public void SetAmount(decimal newAmount, string user, string userAgent)
        {
            if (newAmount != DocumentAmount)
            {
                DocumentAmount = newAmount;
                this.FlagForUpdate(user, userAgent);
            }
        }
    }


}
