using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.eTail.Business.Testing
{
	class ConvertToStandAloneDeclarationResponseTest : TestCaseWithFactory
	{
		public void TestConvertToStandAloneDeclarationSuccessfulResponse()
		{
			var standAloneDeclarationGuid = new Guid();
			var response = new ConvertToStandAloneDeclarationSuccessfulResponse(standAloneDeclarationGuid);
			AssertSuccessResponse(response, standAloneDeclarationGuid);
		}

		public void TestConvertToStandAloneDeclarationConsignmentNotAttachedToShipmentResponse()
		{
			var response = new ConvertToStandAloneDeclarationConsignmentNotAttachedToShipmentResponse();
			AssertFailureResponse(response, ConvertToStandAloneDeclarationResponse.ErrorMessages.ConsignmentNotAttachedToShipment);
		}

		public void TestConvertToStandAloneDeclarationConsignmentMissingTransportDetailsResponse()
		{
			var response = new ConvertToStandAloneDeclarationConsignmentMissingTransportDetailsResponse();
			AssertFailureResponse(response, ConvertToStandAloneDeclarationResponse.ErrorMessages.ConsignmentMissingTransportDetails);
		}

		public void TestConvertToStandAloneDeclarationConsignmentHasUnsavedChangesResponse()
		{
			var response = new ConvertToStandAloneDeclarationConsignmentNotFoundResponse();
			AssertFailureResponse(response, ConvertToStandAloneDeclarationResponse.ErrorMessages.ConsignmentNotFound);
		}

		public void TestConvertToStandAloneDeclarationConsignmentHasClearedImportCustomsStatusResponse()
		{
			var response = new ConvertToStandAloneDeclarationConsignmentHasClearedImportCustomsStatusResponse();
			AssertFailureResponse(response, ConvertToStandAloneDeclarationResponse.ErrorMessages.ConsignmentHasClearedImportCustomsStatus);
		}

		public void TestConvertToStandAloneDeclarationConsignmentHasClearedExportCustomsStatusResponse()
		{
			var response = new ConvertToStandAloneDeclarationConsignmentHasClearedExportCustomsStatusResponse();
			AssertFailureResponse(response, ConvertToStandAloneDeclarationResponse.ErrorMessages.ConsignmentHasClearedExportCustomsStatus);
		}

		public void TestConvertToStandAloneDeclarationShipmentDestinationNotSupportedResponse()
		{
			var response = new ConvertToStandAloneDeclarationShipmentDestinationNotSupportedResponse();
			AssertFailureResponse(response, ConvertToStandAloneDeclarationResponse.ErrorMessages.ShipmentDestinationNotSupported);
		}

		public void TestConvertToStandAloneDeclarationConsignmentHasExistingDeclarationResponse()
		{
			var response = new ConvertToStandAloneDeclarationConsignmentHasExistingDeclarationResponse();
			AssertFailureResponse(response, ConvertToStandAloneDeclarationResponse.ErrorMessages.ConsignmentHasExistingDeclaration);
		}

		public void TestConvertToStandAloneDeclarationUnexpectedErrorResponse()
		{
			var response = new ConvertToStandAloneDeclarationUnexpectedErrorResponse();
			AssertFailureResponse(response, ConvertToStandAloneDeclarationResponse.ErrorMessages.UnexpectedError);
		}

		void AssertSuccessResponse(ConvertToStandAloneDeclarationResponse response, Guid expectedDeclaration)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Conversion Succeeded:", true, response.ConversionSucceeded);
				AssertEquals("Empty Failure Reason:", true, string.IsNullOrEmpty(response.ConversionFailureReason));
				AssertEquals("Stand Alone Declaration:", expectedDeclaration, response.ConvertedStandAloneDeclaration);
			});
		}

		void AssertFailureResponse(ConvertToStandAloneDeclarationResponse response, string expectedFailureReason)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Conversion Failed:", false, response.ConversionSucceeded);
				AssertEquals("Failure Reason:", expectedFailureReason, response.ConversionFailureReason);
				AssertEquals("No Stand Alone Declaration Result:", Guid.Empty, response.ConvertedStandAloneDeclaration);
			});
		}
	}
}
