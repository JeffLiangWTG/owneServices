using System;
using System.Net;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Moq;

namespace Enterprise.Services.ServiceHost.Tests
{
	class ETailConvertToStandAloneDeclarationControllerTest : TestCaseWithFactory
	{
		public void TestConvertToImportStandAloneDeclaration_SuccessfulResponse()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

			Factory.Save();

			var mockService = new Mock<IConvertToStandAloneDeclarationService>();
			mockService.Setup(service => service.ConvertToStandAloneDeclaration(consignment.PK.ToGuid(), It.IsAny<BusinessObjectFactory>()))
				.Callback<Guid, BusinessObjectFactory>((pk, factory) =>
				{
					var consignmentInControllerFactory = factory.Load<HVLVConsignment>(pk);
					consignmentInControllerFactory.HVC_JE_ImportDeclaration = declaration.PK;
				})
				.Returns(new ConvertToStandAloneDeclarationSuccessfulResponse(declaration.PK.ToGuid()));

			using (ObjectFactory.Substitute(mockService.Object))
			{
				var response = controller.ConvertToStandAloneDeclaration(consignment.PK.ToGuid());

				response.AssertResultContains(HttpStatusCode.OK, declaration.PK.ToString());

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var consignmentInNewFactory = newFactory.Load<HVLVConsignment>(consignment.PK);
				AssertEquals("Consignment's newly linked declaration should be saved", declaration.PK, consignmentInNewFactory.HVC_JE_ImportDeclaration);
			}
		}

		public void TestConvertToExportStandAloneDeclaration_SuccessfulResponse()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

			Factory.Save();

			var mockService = new Mock<IConvertToStandAloneDeclarationService>();
			mockService.Setup(service => service.ConvertToStandAloneDeclaration(consignment.PK.ToGuid(), It.IsAny<BusinessObjectFactory>()))
				.Callback<Guid, BusinessObjectFactory>((pk, factory) =>
				{
					var consignmentInControllerFactory = factory.Load<HVLVConsignment>(pk);
					consignmentInControllerFactory.HVC_JE_ExportDeclaration = declaration.PK;
				})
				.Returns(new ConvertToStandAloneDeclarationSuccessfulResponse(declaration.PK.ToGuid()));

			using (ObjectFactory.Substitute(mockService.Object))
			{
				var response = controller.ConvertToStandAloneDeclaration(consignment.PK.ToGuid());

				response.AssertResultContains(HttpStatusCode.OK, declaration.PK.ToString());

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var consignmentInNewFactory = newFactory.Load<HVLVConsignment>(consignment.PK);
				AssertEquals("Consignment's newly linked declaration should be saved", declaration.PK, consignmentInNewFactory.HVC_JE_ExportDeclaration);
			}
		}

		public void TestConvertToStandAloneDeclaration_ConsignmentNotAttachedToShipment()
		{
			var consignmentGuid = new Guid();

			var mockService = new Mock<IConvertToStandAloneDeclarationService>();
			mockService.Setup(service => service.ConvertToStandAloneDeclaration(consignmentGuid, It.IsAny<BusinessObjectFactory>()))
				.Returns(new ConvertToStandAloneDeclarationConsignmentNotAttachedToShipmentResponse());

			using (ObjectFactory.Substitute(mockService.Object))
			{
				var controller = ControllerHelper.GetController<ETailConvertToStandAloneDeclarationController>();
				var response = controller.ConvertToStandAloneDeclaration(consignmentGuid);

				response.AssertResultContains(HttpStatusCode.BadRequest, ConvertToStandAloneDeclarationResponse.ErrorMessages.ConsignmentNotAttachedToShipment);
			}
		}

		public void TestConvertToStandAloneDeclaration_ConsignmentMissingTransportDetails()
		{
			var consignmentGuid = new Guid();

			var mockService = new Mock<IConvertToStandAloneDeclarationService>();
			mockService.Setup(service => service.ConvertToStandAloneDeclaration(consignmentGuid, It.IsAny<BusinessObjectFactory>()))
				.Returns(new ConvertToStandAloneDeclarationConsignmentMissingTransportDetailsResponse());

			using (ObjectFactory.Substitute(mockService.Object))
			{
				var response = controller.ConvertToStandAloneDeclaration(consignmentGuid);

				response.AssertResultContains(HttpStatusCode.BadRequest, ConvertToStandAloneDeclarationResponse.ErrorMessages.ConsignmentMissingTransportDetails);
			}
		}

		public void TestConvertToStandAloneDeclaration_ConsignmentNotFound()
		{
			var consignmentGuid = new Guid();

			var mockService = new Mock<IConvertToStandAloneDeclarationService>();
			mockService.Setup(service => service.ConvertToStandAloneDeclaration(consignmentGuid, It.IsAny<BusinessObjectFactory>()))
				.Returns(new ConvertToStandAloneDeclarationConsignmentNotFoundResponse());

			using (ObjectFactory.Substitute(mockService.Object))
			{
				var response = controller.ConvertToStandAloneDeclaration(consignmentGuid);

				response.AssertResultContains(HttpStatusCode.BadRequest, ConvertToStandAloneDeclarationResponse.ErrorMessages.ConsignmentNotFound);
			}
		}

		public void TestConvertToStandAloneDeclaration_ConsignmentHasClearedImportCustomsStatus()
		{
			var consignmentGuid = new Guid();

			var mockService = new Mock<IConvertToStandAloneDeclarationService>();
			mockService.Setup(service => service.ConvertToStandAloneDeclaration(consignmentGuid, It.IsAny<BusinessObjectFactory>()))
				.Returns(new ConvertToStandAloneDeclarationConsignmentHasClearedImportCustomsStatusResponse());

			using (ObjectFactory.Substitute(mockService.Object))
			{
				var response = controller.ConvertToStandAloneDeclaration(consignmentGuid);

				response.AssertResultContains(HttpStatusCode.BadRequest, ConvertToStandAloneDeclarationResponse.ErrorMessages.ConsignmentHasClearedImportCustomsStatus);
			}
		}

		public void TestConvertToStandAloneDeclaration_ShipmentDestinationNotSupported()
		{
			var consignmentGuid = new Guid();

			var mockService = new Mock<IConvertToStandAloneDeclarationService>();
			mockService.Setup(service => service.ConvertToStandAloneDeclaration(consignmentGuid, It.IsAny<BusinessObjectFactory>()))
				.Returns(new ConvertToStandAloneDeclarationShipmentDestinationNotSupportedResponse());

			using (ObjectFactory.Substitute(mockService.Object))
			{
				var response = controller.ConvertToStandAloneDeclaration(consignmentGuid);

				response.AssertResultContains(HttpStatusCode.BadRequest, ConvertToStandAloneDeclarationResponse.ErrorMessages.ShipmentDestinationNotSupported);
			}
		}

		public void TestConvertToStandAloneDeclaration_ConsignmentHasExistingDeclaration()
		{
			var consignmentGuid = new Guid();

			var mockService = new Mock<IConvertToStandAloneDeclarationService>();
			mockService.Setup(service => service.ConvertToStandAloneDeclaration(consignmentGuid, It.IsAny<BusinessObjectFactory>()))
				.Returns(new ConvertToStandAloneDeclarationConsignmentHasExistingDeclarationResponse());

			using (ObjectFactory.Substitute(mockService.Object))
			{
				var response = controller.ConvertToStandAloneDeclaration(consignmentGuid);

				response.AssertResultContains(HttpStatusCode.BadRequest, ConvertToStandAloneDeclarationResponse.ErrorMessages.ConsignmentHasExistingDeclaration);
			}
		}

		public void TestConvertToStandAloneDeclaration_UnexpectedError()
		{
			var consignmentGuid = new Guid();

			var mockService = new Mock<IConvertToStandAloneDeclarationService>();
			mockService.Setup(service => service.ConvertToStandAloneDeclaration(consignmentGuid, It.IsAny<BusinessObjectFactory>()))
				.Returns(new ConvertToStandAloneDeclarationUnexpectedErrorResponse());

			using (ObjectFactory.Substitute(mockService.Object))
			{
				var response = controller.ConvertToStandAloneDeclaration(consignmentGuid);

				response.AssertResultContains(HttpStatusCode.BadRequest, ConvertToStandAloneDeclarationResponse.ErrorMessages.UnexpectedError);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var controllerBuilder = new ControllerBuilder<ETailConvertToStandAloneDeclarationController>();
			controller = controllerBuilder
				.BuildController();
		}

		ETailConvertToStandAloneDeclarationController controller;
	}
}
