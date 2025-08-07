using Com.Efrata.Service.Finance.Accounting.Lib.BusinessLogic.VBRealizationDocument;
using Com.Efrata.Service.Finance.Accounting.Lib.Models.VBRealizationDocument;
using Com.Efrata.Service.Finance.Accounting.Lib.Services.IdentityService;
using Com.Efrata.Service.Finance.Accounting.Lib.Services.ValidateService;
using Com.Efrata.Service.Finance.Accounting.Lib.Utilities;
using Com.Efrata.Service.Finance.Accounting.WebApi.Controllers.v1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Com.Efrata.Service.Finance.Accounting.Test.Controllers.VBRealizationDocument
{
  public  class VBRealizationDocumentControllerTest
    {
        protected VBRealizationDocumentController GetController(Mock<IServiceProvider> serviceProvider)
        {
            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "unittestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);

            VBRealizationDocumentController controller = new VBRealizationDocumentController(serviceProvider.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = user.Object
                }
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "Bearer unittesttoken";
            controller.ControllerContext.HttpContext.Request.Path = new PathString("/v1/unit-test");
            return controller;
        }

        Mock<IServiceProvider> GetServiceProvider()
        {
            Mock<IServiceProvider> serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
              .Setup(s => s.GetService(typeof(IIdentityService)))
              .Returns(new IdentityService() { TimezoneOffset = 1, Token = "token", Username = "username" });

            var validateService = new Mock<IValidateService>();
            serviceProvider
              .Setup(s => s.GetService(typeof(IValidateService)))
              .Returns(validateService.Object);
            return serviceProvider;
        }

        protected ServiceValidationException GetServiceValidationException()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            var validationResults = new List<ValidationResult>();
            System.ComponentModel.DataAnnotations.ValidationContext validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(null, serviceProvider.Object, null);
            return new ServiceValidationException(validationContext, validationResults);
        }

        protected int GetStatusCode(IActionResult response)
        {
            return (int)response.GetType().GetProperty("StatusCode").GetValue(response, null);
        }

        [Fact]
        public void Get_Return_OK()
        {
            //Setup
            var serviceProviderMock = GetServiceProvider();
            var service = new Mock<IVBRealizationService>();

            Dictionary<string, string> order = new Dictionary<string, string>();
            service.Setup(s => s.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new ReadResponse<VBRealizationDocumentModel>(new List<VBRealizationDocumentModel>(), 1, order, new List<string>()));

            serviceProviderMock
               .Setup(serviceProvider => serviceProvider.GetService(typeof(IVBRealizationService)))
               .Returns(service.Object);

            //Act
            IActionResult response = GetController(serviceProviderMock).Get(0, 0, null, null, null,null);
            
            //Result
            int statusCode = this.GetStatusCode(response);
            Assert.Equal((int)HttpStatusCode.OK, statusCode);
        }


        [Fact]
        public void Get_Return_InternalServerError()
        {
            //Setup
            var serviceProviderMock = GetServiceProvider();
            var service = new Mock<IVBRealizationService>();

            Dictionary<string, string> order = new Dictionary<string, string>();
            service
                .Setup(s => s.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception());

            serviceProviderMock
               .Setup(serviceProvider => serviceProvider.GetService(typeof(IVBRealizationService)))
               .Returns(service.Object);

            //Act
            IActionResult response = GetController(serviceProviderMock).Get(0, 0, null, null, null, null);

            //Result
            int statusCode = this.GetStatusCode(response);
            Assert.Equal((int)HttpStatusCode.InternalServerError, statusCode);
        }

        //[Fact]
        //public async Task GetNonPOById_Return_Ok()
        //{
        //    //Setup
        //    var serviceProviderMock = GetServiceProvider();
        //    var service = new Mock<IVBRealizationService>();

        //    service
        //        .Setup(s => s.ReadByIdAsync(It.IsAny<int>()))
        //        .ReturnsAsync(new Tuple<VBRealizationDocumentModel,List<VBRealizationDocumentExpenditureItemModel>,List<VBRealizationDocumentUnitCostsItemModel>, List<VBRealizationDocumentFileModel>>(new VBRealizationDocumentModel(),new List<VBRealizationDocumentExpenditureItemModel>(),new List<VBRealizationDocumentUnitCostsItemModel>(), new List<VBRealizationDocumentFileModel>()));

        //    serviceProviderMock
        //       .Setup(serviceProvider => serviceProvider.GetService(typeof(IVBRealizationService)))
        //       .Returns(service.Object);

        //    //Act
        //    IActionResult response =await  GetController(serviceProviderMock).GetNonPOById(It.IsAny<int>());

        //    //Result
        //    int statusCode = this.GetStatusCode(response);
        //    Assert.Equal((int)HttpStatusCode.OK, statusCode);
        //}

        [Fact]
        public async Task GetNonPOById_Return_InternalServerError()
        {
            //Setup
            var serviceProviderMock = GetServiceProvider();
            var service = new Mock<IVBRealizationService>();

            service
                .Setup(s => s.ReadByIdAsync(It.IsAny<int>()))
                .Throws(new Exception());

            serviceProviderMock
               .Setup(serviceProvider => serviceProvider.GetService(typeof(IVBRealizationService)))
               .Returns(service.Object);

            //Act
            IActionResult response =await GetController(serviceProviderMock).GetNonPOById(It.IsAny<int>());

            //Result
            int statusCode = this.GetStatusCode(response);
            Assert.Equal((int)HttpStatusCode.InternalServerError, statusCode);
        }

        [Fact]
        public async Task GetNonPOById_Return_NotFound()
        {
            //Setup
            var serviceProviderMock = GetServiceProvider();
            var service = new Mock<IVBRealizationService>();

            service
                .Setup(s => s.ReadByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(()=>null);

            serviceProviderMock
               .Setup(serviceProvider => serviceProvider.GetService(typeof(IVBRealizationService)))
               .Returns(service.Object);

            //Act
            IActionResult response =await GetController(serviceProviderMock).GetNonPOById(It.IsAny<int>());

            //Result
            int statusCode = this.GetStatusCode(response);
            Assert.Equal((int)HttpStatusCode.NotFound, statusCode);
        }

    }
}
