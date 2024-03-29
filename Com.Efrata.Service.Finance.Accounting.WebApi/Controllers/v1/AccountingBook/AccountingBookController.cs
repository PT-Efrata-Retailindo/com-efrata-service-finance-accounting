﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Com.Efrata.Service.Finance.Accounting.Lib.BusinessLogic.Services.AccountingBook;
using Com.Efrata.Service.Finance.Accounting.Lib.Services.IdentityService;
using Com.Efrata.Service.Finance.Accounting.Lib.Services.ValidateService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Com.Efrata.Service.Finance.Accounting.WebApi.Utilities;
using Com.Efrata.Service.Finance.Accounting.Lib.ViewModels.AccountingBook;
using Com.Efrata.Service.Finance.Accounting.Lib.Models.AccountingBook;
using Com.Efrata.Service.Finance.Accounting.Lib.Utilities;

namespace Com.Efrata.Service.Finance.Accounting.WebApi.Controllers.v1.AccountingBook
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/accounting-book")]
    [Authorize]

    public class AccountingBookController : Controller
    {
        private readonly IIdentityService _identityService;
        private readonly IValidateService _validateService;
        private readonly IAccountingBookService _service;
        private readonly IMapper _mapper;
        private const string ApiVersion = "1.0";

        public AccountingBookController(IServiceProvider serviceProvider)
        {
            _identityService = serviceProvider.GetService<IIdentityService>();
            _validateService = serviceProvider.GetService<IValidateService>();
            _service = serviceProvider.GetService<IAccountingBookService>();
            _mapper = serviceProvider.GetService<IMapper>();
        }

        protected void VerifyUser()
        {
            _identityService.Username = User.Claims.ToArray().SingleOrDefault(p => p.Type.Equals("username")).Value;
            _identityService.Token = Request.Headers["Authorization"].FirstOrDefault().Replace("Bearer ", "");
            _identityService.TimezoneOffset = Convert.ToInt32(Request.Headers["x-timezone-offset"]);
        }

        [HttpGet]
        public IActionResult Get(int page = 1, int size = 25, string order = "{}", [Bind(Prefix = "Select[]")] List<string> select = null, string keyword = null, string filter = "{}")
        {
            try
            {
                var queryResult = _service.Read(page, size, order, select, keyword, filter);

                var result =
                    new ResultFormatter(ApiVersion, General.OK_STATUS_CODE, General.OK_MESSAGE)
                    .Ok(_mapper, queryResult.Data, page, size, queryResult.Count, queryResult.Data.Count, queryResult.Order, select);
                return Ok(result);
            }
            catch (Exception ex)
            {
                var result = new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, ex.Message).Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, result);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody]AccountingBookViewModel viewModel)
        {
            try
            {
                VerifyUser();
                _validateService.Validate(viewModel);

                var model = _mapper.Map<AccountingBookModel>(viewModel);
                await _service.CreateAsync(model);

                var result = new ResultFormatter(ApiVersion, General.CREATED_STATUS_CODE, General.OK_MESSAGE).Ok();

                return Created(String.Concat(Request.Path, "/", 0), result);
            }
            catch (ServiceValidationException e)
            {
                var result = new ResultFormatter(ApiVersion, General.BAD_REQUEST_STATUS_CODE, General.BAD_REQUEST_MESSAGE).Fail(e);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                var result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, ex.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, result);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            try
            {
                var model = await _service.ReadByIdAsync(id);

                if (model == null)
                {
                    var result = new ResultFormatter(ApiVersion, General.NOT_FOUND_STATUS_CODE, General.NOT_FOUND_MESSAGE).Fail();
                    return NotFound(result);
                }
                else
                {
                    var viewModel = _mapper.Map<AccountingBookViewModel>(model);
                    var result = new ResultFormatter(ApiVersion, General.OK_STATUS_CODE, General.OK_MESSAGE).Ok<AccountingBookViewModel>(_mapper, viewModel);
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                var result = new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message).Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, result);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                VerifyUser();

                await _service.DeleteAsync(id);

                return NoContent();
            }
            catch (Exception e)
            {
                var result = new ResultFormatter(ApiVersion, General.BAD_REQUEST_STATUS_CODE, e.Message).Fail();
                return StatusCode(General.BAD_REQUEST_STATUS_CODE, result);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] AccountingBookViewModel viewModel)
        {
            try
            {
                VerifyUser();
                _validateService.Validate(viewModel);

                if (id != viewModel.Id)
                {
                    var result = new ResultFormatter(ApiVersion, General.BAD_REQUEST_STATUS_CODE, General.BAD_REQUEST_MESSAGE).Fail();
                    return BadRequest(result);
                }

                var model = _mapper.Map<AccountingBookModel>(viewModel);

                await _service.UpdateAsync(id, model);

                return NoContent();
            }
            catch (ServiceValidationException e)
            {
                var result = new ResultFormatter(ApiVersion, General.BAD_REQUEST_STATUS_CODE, General.BAD_REQUEST_MESSAGE).Fail(e);
                return BadRequest(result);
            }
            catch (Exception e)
            {
                var result = new ResultFormatter(ApiVersion, General.BAD_REQUEST_STATUS_CODE, e.Message).Fail();
                return StatusCode(General.BAD_REQUEST_STATUS_CODE, result);
            }
        }


    }
}
