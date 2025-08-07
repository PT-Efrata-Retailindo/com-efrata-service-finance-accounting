using Com.Efrata.Service.Finance.Accounting.Lib.BusinessLogic.AzureStorage;
using Com.Efrata.Service.Finance.Accounting.Lib.BusinessLogic.Interfaces.VBRealizationDocumentNonPO;
using Com.Efrata.Service.Finance.Accounting.Lib.BusinessLogic.VBRealizationDocumentExpedition;
using Com.Efrata.Service.Finance.Accounting.Lib.BusinessLogic.VBRequestDocument;
using Com.Efrata.Service.Finance.Accounting.Lib.Models.VBRealizationDocument;
using Com.Efrata.Service.Finance.Accounting.Lib.Services.IdentityService;
using Com.Efrata.Service.Finance.Accounting.Lib.Utilities;
using Com.Efrata.Service.Finance.Accounting.Lib.ViewModels.VBRealizationDocumentNonPO;
using Com.Efrata.Service.Finance.Accounting.Lib.ViewModels.VBRealizationPaymentNonVB;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using iTextSharp.text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Com.Efrata.Service.Finance.Accounting.Lib.ViewModels.VBRealizationPaymentNonVB.VBRealizationPaymentNonVBViewModel;

namespace Com.Efrata.Service.Finance.Accounting.Lib.BusinessLogic.Services.VBRealizationDocumentNonPO
{
    public class VBRealizationPaymentNonVBService : IVBRealizationPaymentNonVBService
    {
        private const string UserAgent = "finance-service";
        protected DbSet<VBRealizationDocumentModel> _dbSet;
        protected IIdentityService _identityService;
        public FinanceDbContext _dbContext;
        //private readonly IAzureDocument _azureDocument;
        public IServiceProvider ServiceProvider;

        public VBRealizationPaymentNonVBService(IServiceProvider serviceProvider, FinanceDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<VBRealizationDocumentModel>();
            _identityService = serviceProvider.GetService<IIdentityService>();
            //_azureDocument = serviceProvider.GetService<IAzureDocument>();
            ServiceProvider = serviceProvider;
        }

        private IAzureDocumentService AzureDocumentService
        {
            get { return this.ServiceProvider.GetService<IAzureDocumentService>(); }
        }

        private Tuple<string, int> GetDocumentNo(VBRealizationPaymentNonVBViewModel form, VBRealizationDocumentModel existingData)
        {
            var now = form.Date.GetValueOrDefault().AddHours(_identityService.TimezoneOffset);
            var year = now.ToString("yy");
            var month = now.ToString("MM");

            var unitCode = "T";
            if (form.Unit.Division.Name.ToUpper() == "GARMENT")
                unitCode = "G";

            if (form.IsInklaring) unitCode += "I";

            if (form.DocumentType == RealizationDocumentType.PaymentNonVB) unitCode += "D";

            var documentNo = $"R-{unitCode}-{month}{year}-";

            var index = 1;

            if (existingData != null)
            {
                index = existingData.Index + 1;
            }

            documentNo += string.Format("{0:000}", index);


            return new Tuple<string, int>(documentNo, index);
        }



        private string GetDocumentUnitCode(string division, bool isInklaring, RealizationDocumentType documentType)
        {
            var unitCode = "T";
            if (division.ToUpper() == "GARMENT")
                unitCode = "G";

            unitCode += (isInklaring) ? "I" : null;

            if (documentType == RealizationDocumentType.PaymentNonVB) unitCode += "D";

            return $"R-{unitCode}-";
        }

        private List<VBRealizationDocumentExpenditureItemModel> AddItems(int id, IEnumerable<VBRealizationDocumentNonPOExpenditureItemViewModel> items)
        {
            var models = items.Select(element =>
            {
                var result = new VBRealizationDocumentExpenditureItemModel(id, element);

                result.FlagForCreate(_identityService.Username, UserAgent);
                return result;
            }).ToList();

            return models;
        }

        private List<VBRealizationDocumentUnitCostsItemModel> AddUnitCosts(int id, IEnumerable<VBRealizationDocumentNonPOUnitCostViewModel> unitCosts)
        {
            var models = unitCosts.Select(element =>
            {
                var result = new VBRealizationDocumentUnitCostsItemModel(id, element);

                result.FlagForCreate(_identityService.Username, UserAgent);
                return result;
            }).ToList();

            return models;
        }

        private List<VBRealizationDocumentFileModel> AddDocumentFiles(int id, List<DocumentFileViewModel> DocumentFile)
        {
            var models = DocumentFile.Select(element =>
            {
                var result = new VBRealizationDocumentFileModel(id, element);

                result.FlagForCreate(_identityService.Username, UserAgent);
                return result;
            }).ToList();

            return models;
        }

        private async Task<string> UploadFile( int id, DateTime createUtc, List<string> DocumentFile, string DocumentFileName, string DocumentPath)
        {
            string var = await AzureDocumentService.UploadMultipleFile("vb-finance", id, createUtc, DocumentFile, DocumentFileName, DocumentPath);
            return var;
        }

        public async Task<int> CreateAsync(VBRealizationPaymentNonVBViewModel vm)
        {
            var internalTransaction = _dbContext.Database.CurrentTransaction == null;
            var transaction = !internalTransaction ? _dbContext.Database.CurrentTransaction : _dbContext.Database.BeginTransaction();

            try
            {
                vm.DocumentType = RealizationDocumentType.PaymentNonVB;
                string unitCode = GetDocumentUnitCode(vm.Unit.Division.Name.ToUpper(), vm.IsInklaring, vm.DocumentType);
                var existingData = _dbContext.VBRealizationDocuments
                    .Where(a => a.Date.AddHours(_identityService.TimezoneOffset).Month == vm.Date.GetValueOrDefault().AddHours(_identityService.TimezoneOffset).Month
                    && a.Date.AddHours(_identityService.TimezoneOffset).Year == vm.Date.GetValueOrDefault().AddHours(_identityService.TimezoneOffset).Year
                    && a.DocumentNo.StartsWith(unitCode))
                    .OrderByDescending(s => s.Index)
                    .FirstOrDefault();

                var documentNo = GetDocumentNo(vm, existingData);
                vm.DocumentNo = documentNo.Item1;
                vm.Index = documentNo.Item2;
                vm.Amount = vm.Items.Sum(s => s.Total);

                var model = new VBRealizationDocumentModel(vm);

                model.FlagForCreate(_identityService.Username, UserAgent);
                _dbContext.VBRealizationDocuments.Add(model);
                await _dbContext.SaveChangesAsync();


                var items = AddItems(model.Id, vm.Items);

                _dbContext.VBRealizationDocumentExpenditureItems.AddRange(items);
                await _dbContext.SaveChangesAsync();

                var unitCosts = AddUnitCosts(model.Id, vm.UnitCosts);

                _dbContext.VBRealizationDocumentUnitCostsItems.AddRange(unitCosts);
                await _dbContext.SaveChangesAsync();


                //var upload = await UploadFile(model.Id, model.CreatedUtc, vm.DocumentsFile, vm.DocumentsFileName.Select( s => s.documentName).ToList(), vm.DocumentsPath);
                if (vm.DocumentsFileName.Count() > 0)
                {


                    var upload = await UploadFile(model.Id, model.CreatedUtc, vm.DocumentsFile, vm.DocumentsFileName.First().documentName, null);
                    //string jsonString = upload;
                    List<string> list = JsonConvert.DeserializeObject<List<string>>(upload);

                    var listC = vm.DocumentsFileName.Zip(list, (file, detail) =>
                    {
                        return new DocumentFileViewModel
                        {
                            documentName = file.documentName,
                            amount = file.amount,
                            documentsPath = detail
                        };
                    }).ToList();

                    var documentFile = AddDocumentFiles(model.Id, listC);

                    _dbContext.VBRealizationDocumentFiles.AddRange(documentFile);
                    await _dbContext.SaveChangesAsync();
                }
                //if (vm.VBNonPOType == "Dengan Nomor VB")
                //{
                //    var vbRequest = _dbContext.VBRequestDocuments.FirstOrDefault(s => s.Id == vm.VBDocument.Id);
                //    if (vbRequest != null)
                //    {
                //        vbRequest.SetIsRealized(true, _identityService.Username, UserAgent);
                //    }
                //}
                //await _dbContext.SaveChangesAsync();
                if (internalTransaction)
                    transaction.Commit();

                return model.Id;
            }
            catch (Exception ex)
            {

                if (internalTransaction)
                    transaction.Rollback();
                throw ex;
            }
        }

        private void DeleteItems(int id)
        {
            var items = _dbContext.VBRealizationDocumentExpenditureItems.Where(s => s.VBRealizationDocumentId == id).ToList();

            foreach (var item in items)
            {
                item.FlagForDelete(_identityService.Username, UserAgent);
            }
        }

        private void DeleteUnitCosts(int id)
        {
            var items = _dbContext.VBRealizationDocumentUnitCostsItems.Where(s => s.VBRealizationDocumentId == id).ToList();

            foreach (var item in items)
            {
                item.FlagForDelete(_identityService.Username, UserAgent);
            }
        }

        public Task<int> DeleteAsync(int id)
        {
            var data = _dbContext.VBRealizationDocuments.FirstOrDefault(s => s.Id == id);
            data.FlagForDelete(_identityService.Username, UserAgent);

            DeleteItems(id);

            DeleteUnitCosts(id);

            var vbRequest = _dbContext.VBRequestDocuments.FirstOrDefault(s => s.Id == data.VBRequestDocumentId);
            if (vbRequest != null)
            {
                vbRequest.SetIsRealized(false, _identityService.Username, UserAgent);
            }

            return _dbContext.SaveChangesAsync();
        }

        public ReadResponse<VBRealizationDocumentModel> Read(int page, int size, string order, List<string> select, string keyword, string filter)
        {
            var query = _dbContext.VBRealizationDocuments.Where(entity => entity.DocumentType == RealizationDocumentType.PaymentNonVB).AsQueryable();

            var searchAttributes = new List<string>()
            {
                "DocumentNo",
                "VBRequestDocumentNo",
                "VBRequestDocumentCreatedBy"
            };

            query = QueryHelper<VBRealizationDocumentModel>.Search(query, searchAttributes, keyword);

            var filterDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(filter);
            query = QueryHelper<VBRealizationDocumentModel>.Filter(query, filterDictionary);

            var orderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(order);
            query = QueryHelper<VBRealizationDocumentModel>.Order(query, orderDictionary);

            var pageable = new Pageable<VBRealizationDocumentModel>(query, page - 1, size);
            var data = pageable.Data.ToList();

            int totalData = pageable.TotalCount;

            return new ReadResponse<VBRealizationDocumentModel>(data, totalData, orderDictionary, new List<string>());
        }
        
        public ReadResponse<VBRealizationDocumentModel> ReadByUser(int page, int size, string order, List<string> select, string keyword, string filter)
        {
            var query = _dbContext.VBRealizationDocuments.Where(entity => entity.DocumentType == RealizationDocumentType.PaymentNonVB).AsQueryable();

            query = query.Where(entity => entity.CreatedBy == _identityService.Username);

            var searchAttributes = new List<string>()
            {
                "DocumentNo",
                //"VBRequestDocumentNo",
                //"VBRequestDocumentCreatedBy"
            };

            query = QueryHelper<VBRealizationDocumentModel>.Search(query, searchAttributes, keyword);

            var filterDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(filter);
            query = QueryHelper<VBRealizationDocumentModel>.Filter(query, filterDictionary);

            var orderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(order);
            query = QueryHelper<VBRealizationDocumentModel>.Order(query, orderDictionary);

            var pageable = new Pageable<VBRealizationDocumentModel>(query, page - 1, size);
            var data = pageable.Data.ToList();

            int totalData = pageable.TotalCount;

            return new ReadResponse<VBRealizationDocumentModel>(data, totalData, orderDictionary, new List<string>());
        }

        private VBRealizationPaymentNonVBViewModel MaptoVM(VBRealizationDocumentModel model, List<VBRealizationDocumentExpenditureItemModel> items, List<VBRealizationDocumentUnitCostsItemModel> unitCosts, List<VBRealizationDocumentFileModel> documentFile)
        {
            return new VBRealizationPaymentNonVBViewModel()
            {
                Active = model.Active,
                Amount = model.Amount,
                CreatedAgent = model.CreatedAgent,
                CreatedBy = model.CreatedBy,
                CreatedUtc = model.CreatedUtc,
                Currency = new CurrencyViewModel()
                {
                    Code = model.CurrencyCode,
                    Description = model.CurrencyDescription,
                    Id = model.CurrencyId,
                    Rate = model.CurrencyRate,
                    Symbol = model.CurrencySymbol
                },
                Id = model.Id,
                Date = model.Date,
                DocumentNo = model.DocumentNo,
                Index = model.Index,
                IsDeleted = model.IsDeleted,
                LastModifiedAgent = model.LastModifiedAgent,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedUtc = model.LastModifiedUtc,
                Type = model.Type,
                Position = model.Position,
                BLAWBNumber = model.BLAWBNumber,
                ContractPONumber = model.ContractPONumber,
                IsInklaring = model.IsInklaring,
                ClaimType = model.ClaimType,
                DispositionType = model.DispositionType,
                Unit = new UnitViewModel()
                {
                    Code = model.SuppliantUnitCode,
                    Division = new DivisionViewModel()
                    {
                        Code = model.SuppliantDivisionCode,
                        Id = model.SuppliantDivisionId,
                        Name = model.SuppliantDivisionName
                    },
                    Name = model.SuppliantUnitName,
                    Id = model.SuppliantUnitId
                },
                VBDocument = model.VBRequestDocumentId == 0 ? null : new VBRequestDocumentNonPODto()
                {
                    Id = model.VBRequestDocumentId,
                    DocumentNo = model.VBRequestDocumentNo,
                    Amount = model.VBRequestDocumentAmount,
                    RealizationEstimationDate = model.VBRequestDocumentRealizationEstimationDate,
                    CreatedBy = model.VBRequestDocumentCreatedBy,
                    Date = model.VBRequestDocumentDate,
                    Purpose = model.VBRequestDocumentPurpose
                },
                VBNonPOType = model.VBNonPoType,
                Remark = model.Remark,
                Items = items.Select(s => new VBRealizationDocumentNonPOExpenditureItemViewModel()
                {
                    Active = s.Active,
                    Amount = s.Amount,
                    CreatedAgent = s.CreatedAgent,
                    CreatedBy = s.CreatedBy,
                    CreatedUtc = s.CreatedUtc,
                    DateDetail = s.Date,
                    Id = s.Id,
                    IncomeTax = new IncomeTaxViewModel()
                    {
                        Id = s.IncomeTaxId,
                        Name = s.IncomeTaxName,
                        Rate = s.IncomeTaxRate
                    },
                    IncomeTaxBy = s.IncomeTaxBy,
                    IsDeleted = s.IsDeleted,
                    IsGetPPh = s.UseIncomeTax,
                    IsGetPPn = s.UseVat,
                    PPhAmount = s.PPhAmount,
                    PPnAmount = s.PPnAmount,
                    BLAWBNumber = s.BLAWBNumber,
                    LastModifiedAgent = s.LastModifiedAgent,
                    LastModifiedBy = s.LastModifiedBy,
                    LastModifiedUtc = s.LastModifiedUtc,
                    VatTax = new VatTaxViewModel()
                    {
                        Id = s.VatId,
                        Rate = s.VatRate,
                    },
                    Remark = s.Remark,
                    Invoice = s.InvoiceNo
                }).ToList(),
                UnitCosts = unitCosts.Select(s => new VBRealizationDocumentNonPOUnitCostViewModel()
                {
                    Amount = s.Amount,
                    IsSelected = s.IsSelected,
                    Active = s.Active,
                    CreatedAgent = s.CreatedAgent,
                    CreatedBy = s.CreatedBy,
                    CreatedUtc = s.CreatedUtc,
                    Id = s.Id,
                    IsDeleted = s.IsDeleted,
                    LastModifiedAgent = s.LastModifiedAgent,
                    LastModifiedBy = s.LastModifiedBy,
                    LastModifiedUtc = s.LastModifiedUtc,
                    Unit = new UnitViewModel()
                    {
                        Id = s.UnitId,
                        Code = s.UnitCode,
                        Division = new DivisionViewModel()
                        {
                            Code = s.DivisionCode,
                            Id = s.DivisionId,
                            Name = s.DivisionName
                        },
                        Name = s.UnitName,
                        VBDocumentLayoutOrder = s.VBDocumentLayoutOrder
                    }
                }).ToList(),

                DocumentsFileName = documentFile.Select( s => new DocumentFileViewModel()
                { 
                    Id= s.Id,
                  amount  = s.DocumentAmount,
                  documentName = s.DocumentsFileName,
                  documentsPath = s.DocumentsPath,
                
                }).ToList(),
                DocumentsPath = documentFile.Select( s=> s.DocumentsPath).ToList(),
            };
        }

        public async Task<VBRealizationPaymentNonVBViewModel> ReadByIdAsync(int id)
        {
            var data = await _dbContext.VBRealizationDocuments.FirstOrDefaultAsync(s => s.Id == id);

            if (data == null)
                return null;

            var items = _dbContext.VBRealizationDocumentExpenditureItems.Where(s => s.VBRealizationDocumentId == id).ToList();
            var unitCosts = _dbContext.VBRealizationDocumentUnitCostsItems.Where(s => s.VBRealizationDocumentId == id).OrderBy(s => s.VBDocumentLayoutOrder).ToList();
            var documentFile = _dbContext.VBRealizationDocumentFiles.Where(s => s.VBRealizationDocumentId == id).ToList();

            var result = MaptoVM(data, items, unitCosts, documentFile);

            result.DocumentsFile = await AzureDocumentService.DownloadMultipleFiles2("vb-finance", result.DocumentsPath);

                //vm.DocumentsFile = await _azureDocument.DownloadMultipleFiles("GarmentSampleRequest", vm.DocumentsPath);

            return result;
        }

        public Task<int> UpdateAsync(int id, VBRealizationPaymentNonVBViewModel model)
        {
            var data = _dbContext.VBRealizationDocuments.FirstOrDefault(s => s.Id == id);
            

            model.Amount = model.Items.Sum(s => s.Total);
            data.SetRemark(model.Remark);
            data.SetAmount(model.Amount, _identityService.Username, UserAgent);
            data.SetClaimType(model.ClaimType, _identityService.Username, UserAgent);
            data.SetDispositionType(model.DispositionType, _identityService.Username, UserAgent);
            data.SetCurrency(model.Currency.Id, model.Currency.Code, model.Currency.Symbol, model.Currency.Rate, model.Currency.Description, _identityService.Username, UserAgent);
            data.SetUnit(model.Unit.Id, model.Unit.Code, model.Unit.Name, _identityService.Username, UserAgent);
            //if(data.VBRequestDocumentId != model.VBDocument.Id)
            //{
            //    var newVBRequest = _dbContext.VBRequestDocuments.FirstOrDefault(s => s.Id == model.VBDocument.Id);
            //    var oldVBRequest = _dbContext.VBRequestDocuments.FirstOrDefault(s => s.Id == data.VBRequestDocumentId);
            //    if (newVBRequest != null)
            //    {
            //        newVBRequest.SetIsRealized(true, _identityService.Username, UserAgent);
            //    }

            //    if (oldVBRequest != null)
            //    {
            //        oldVBRequest.SetIsRealized(false, _identityService.Username, UserAgent);
            //    }
            //}



            //if (data.VBNonPoType == "Tanpa Nomor VB")
            //{
            //    data.SetCurrency(model.Currency.Id, model.Currency.Code, model.Currency.Symbol, model.Currency.Rate, model.Currency.Description, _identityService.Username, UserAgent);
            //    data.SetUnit(model.Unit.Id, model.Unit.Code, model.Unit.Name, _identityService.Username, UserAgent);

            //    if (model.Unit != null)
            //    {

            //        data.SetDivision(model.Unit.Division.Id, model.Unit.Division.Code, model.Unit.Division.Name, _identityService.Username, UserAgent);
            //    }
            //    data.SetVBRequest(model.VBDocument.Id, model.VBDocument.DocumentNo, model.VBDocument.Date, model.VBDocument.RealizationEstimationDate, model.VBDocument.CreatedBy, model.VBDocument.Amount.GetValueOrDefault(), model.VBDocument.Purpose, _identityService.Username, UserAgent);
            //}


            EditItems(id, model.Items);
            EditUnitCosts(id, model.UnitCosts);
            EditDocumentFile(id, model.DocumentsFileName);

            data.UpdatePosition(VBRealizationPosition.Purchasing, _identityService.Username, UserAgent);
            data.FlagForUpdate(_identityService.Username, UserAgent);
            return _dbContext.SaveChangesAsync();
        }

        private void EditItems(int id, IEnumerable<VBRealizationDocumentNonPOExpenditureItemViewModel> formItems)
        {
            var items = _dbContext.VBRealizationDocumentExpenditureItems.Where(s => s.VBRealizationDocumentId == id).ToList();
            foreach (var item in items)
            {
                var formItem = formItems.FirstOrDefault(s => s.Id == item.Id);
                if (formItem == null)
                {
                    item.FlagForDelete(_identityService.Username, UserAgent);
                }
                else
                {
                    item.SetAmount(formItem.Amount, _identityService.Username, UserAgent);
                    item.SetDate(formItem.DateDetail.GetValueOrDefault(), _identityService.Username, UserAgent);
                    item.SetIncomeTax(formItem.IncomeTax.Id, formItem.IncomeTax.Rate.GetValueOrDefault(), formItem.IncomeTax.Name, _identityService.Username, UserAgent);
                    item.SetVatTax(formItem.VatTax.Id, formItem.VatTax.Rate, _identityService.Username, UserAgent);
                    item.SetIncomeTaxBy(formItem.IncomeTaxBy, _identityService.Username, UserAgent);
                    item.SetRemark(formItem.Remark, _identityService.Username, UserAgent);
                    item.SetUseIncomeTax(formItem.IsGetPPh, _identityService.Username, UserAgent);
                    item.SetUseVat(formItem.IsGetPPn, _identityService.Username, UserAgent);
                    item.SetBLAWBNumber(formItem.BLAWBNumber, _identityService.Username, UserAgent);
                    item.SetPPnAmount(formItem.PPnAmount, _identityService.Username, UserAgent);
                    item.SetPPhAmount(formItem.PPhAmount, _identityService.Username, UserAgent);

                }
            }

            var newItems = formItems.Where(s => s.Id == 0).Select(element =>
            {
                var result = new VBRealizationDocumentExpenditureItemModel(id, element);

                result.FlagForCreate(_identityService.Username, UserAgent);
                return result;
            }).ToList();

            _dbContext.VBRealizationDocumentExpenditureItems.AddRange(newItems);

            //return _dbContext.SaveChangesAsync();
        }

        private void EditUnitCosts(int id, IEnumerable<VBRealizationDocumentNonPOUnitCostViewModel> formItems)
        {
            var items = _dbContext.VBRealizationDocumentUnitCostsItems.Where(s => s.VBRealizationDocumentId == id).ToList();
            foreach (var item in items)
            {
                var formItem = formItems.FirstOrDefault(s => s.Id == item.Id);
                if (formItem == null)
                {
                    item.FlagForDelete(_identityService.Username, UserAgent);
                }
                else
                {
                    item.SetAmount(formItem.Amount, _identityService.Username, UserAgent);
                    item.SetIsSelected(formItem.IsSelected, _identityService.Username, UserAgent);
                    item.SetUnit(formItem.Unit.Id, formItem.Unit.Name, formItem.Unit.Code, _identityService.Username, UserAgent);

                    if (formItem.Unit.Division == null)
                    {
                        item.SetDivision(0, null, null, _identityService.Username, UserAgent);

                    }
                    else
                    {
                        item.SetDivision(formItem.Unit.Division.Id, formItem.Unit.Division.Name, formItem.Unit.Division.Code, _identityService.Username, UserAgent);
                    }

                }
            }

            var models = formItems.Where(s => s.Id == 0).Select(element =>
            {


                var result = new VBRealizationDocumentUnitCostsItemModel(id, element);

                result.FlagForCreate(_identityService.Username, UserAgent);
                return result;
            }).ToList();

            _dbContext.VBRealizationDocumentUnitCostsItems.AddRange(models);

            //return _dbContext.SaveChangesAsync();
        }

        private void EditDocumentFile(int id, List<DocumentFileViewModel> formItems)
        {
            var items = _dbContext.VBRealizationDocumentFiles.Where(s => s.VBRealizationDocumentId == id).ToList();
            foreach (var item in items)
            {
                var formItem = formItems.FirstOrDefault(s => s.Id == item.Id);
                if (formItem == null)
                {
                    //string docPath = string.Join(",", model.DocumentsPath);
                    AzureDocumentService.RemoveMultipleFile("vb-finance", item.DocumentsPath);

                    item.FlagForDelete(_identityService.Username, UserAgent);
                }
                else
                {
                    item.SetAmount(formItem.amount, _identityService.Username, UserAgent);
                    

                }
            }

            var models = formItems.Where(s => s.Id == 0);

            var documentFile = new List<VBRealizationDocumentFileModel>();

            if(models.Count() > 0)
            {
                var upload = UploadFile(id, DateTime.Now, models.Select(s => s.documentFile).ToList(), models.First().documentName, null);


                //string jsonString = upload;
                List<string> list = JsonConvert.DeserializeObject<List<string>>(upload.Result);

                var listC = models.Zip(list, (file, detail) => {
                    return new DocumentFileViewModel
                    {
                        documentName = file.documentName,
                        amount = file.amount,
                        documentsPath = detail
                    };
                }).ToList();

                documentFile = AddDocumentFiles(id, listC);

            }

            
            //var models = formItems.Where(s => s.Id == 0).Select(element =>
            //{
            //    var upload = UploadFile(id, element.CreatedUtc, documentFile, element.documentName, null);

            //    var result = new VBRealizationDocumentFileModel(id, element);

            //    result.FlagForCreate(_identityService.Username, UserAgent);
            //    return result;
            //}).ToList();

            _dbContext.VBRealizationDocumentFiles.AddRange(documentFile);

            //return _dbContext.SaveChangesAsync();
        }

    }
}
