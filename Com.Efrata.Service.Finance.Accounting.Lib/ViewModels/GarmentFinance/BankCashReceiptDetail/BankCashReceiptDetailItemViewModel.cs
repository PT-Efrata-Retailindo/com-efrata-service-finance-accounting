﻿using Com.Efrata.Service.Finance.Accounting.Lib.Utilities.BaseClass;
using Com.Efrata.Service.Finance.Accounting.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.Efrata.Service.Finance.Accounting.Lib.ViewModels.GarmentFinance.BankCashReceiptDetail
{
    public class BankCashReceiptDetailItemViewModel : BaseViewModel
    {
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; }
        public BuyerViewModel BuyerAgent { get; set; }
        public CurrencyViewModel Currency { get; set; }
        public decimal Amount { get; set; }
    }
}
