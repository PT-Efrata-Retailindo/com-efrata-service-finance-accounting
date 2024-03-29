﻿using Com.Efrata.Service.Finance.Accounting.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.Efrata.Service.Finance.Accounting.Lib.ViewModels.PurchasingDispositionReport
{
    public class PurchasingDispositionBaseResponseViewModel
    {
        public List<PurchasingDispositionReportViewModel> data { get; set; }
        public APIInfo info { get; set; }
    }
}
