﻿using Com.Efrata.Service.Finance.Accounting.Lib.BusinessLogic.Services.CreditorAccount;
using Com.Efrata.Service.Finance.Accounting.Lib.Models.CreditorAccount;
using Com.Efrata.Service.Finance.Accounting.Lib.ViewModels.CreditorAccount;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.Efrata.Service.Finance.Accounting.Test.DataUtils.CreditorAccount
{
    public class CreditorAccountDataUtil
    {
        private readonly CreditorAccountService Service;

        public CreditorAccountDataUtil(CreditorAccountService service)
        {
            Service = service;
        }

        public async Task<CreditorAccountModel> GetTestData_CreditorAccountModel()
        {
            var data = GetNewData_CreditorAccountModel();
            await Service.CreateAsync(data);
            var result = Service.ReadModelById(data.Id);
            return await result;
        }

        public  CreditorAccountModel GetNewData_CreditorAccountModel()
        {
            return new CreditorAccountModel()
            {
                SupplierName = "SupplierName"
            };


        }
        public CreditorAccountUnitReceiptNotePostedViewModel GetNewData_UnitReceiptNotePostedViewModel()
        {
            return new CreditorAccountUnitReceiptNotePostedViewModel()
            {
                Code = "Code",
                CreditorAccountId = 1,
                Date = DateTimeOffset.UtcNow,
                DPP = 1000,
                PPN = 100,
                InvoiceNo = "InvoiceNo",
                SupplierCode = "SupplierCode",
                SupplierName = "SupplierName",
                Products = "- Product 1\n- Product 2",
                Currency = "IDR",
                CurrencyRate = 1,
                SupplierIsImport = true,
                DPPCurrency = 1,
                PaymentDuration = "1",
                UseIncomeTax = true,
                MemoDPP = 1,
                MemoMutation = 1,
                MemoPPN = 0,
                DivisionId=1
            };
        }


        public CreditorAccountBankExpenditureNotePostedViewModel GetBankExpenditureNotePostedViewModel()
        {
            return new CreditorAccountBankExpenditureNotePostedViewModel()
            {
                Code = "Code",
                CreditorAccountId = 1,
                Date = DateTimeOffset.UtcNow,
                Id = 1,
                Mutation = 5500,
                InvoiceNo = "InvoiceNo",
                SupplierCode = "SupplierCode",
                SupplierName = "SupplierName",
                CurrencyRate=1,
                DPPCurrency=1,
                MemoDPP=1,
                MemoMutation=1,
                MemoPPN=1,
                PaymentDuration= "PaymentDuration",
                

            };
        }

        public CreditorAccountMemoPostedViewModel GetMemoPostedViewModel()
        {
            return new CreditorAccountMemoPostedViewModel()
            {
                Code = "Code",
                CreditorAccountId = 1,
                Date = DateTimeOffset.UtcNow,
                DPP = 1000,
                PPN = 100,
                InvoiceNo = "InvoiceNo",
                SupplierCode = "SupplierCode",
                SupplierName = "SupplierName"
            };
        }
    }
}
