using CargoWise.EntityFramework.Testing;
using Newtonsoft.Json;

namespace Enterprise.eTail.Business.Testing
{
	public class HVLVConsignmentPreScreeningResultTest : TestCaseWithFactory
	{
		public void TestPreScreeningStatus()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var response = new HVLVConsignmentPreScreeningResult(consignment);
			AssertEquals("Should default to passed", HVLVConsignmentPreScreeningStatusCodes.Codes.Passed, response.PreScreeningStatus);

			response.AddErrorMessage("some error message");
			AssertEquals("Should be failed", HVLVConsignmentPreScreeningStatusCodes.Codes.Failed, response.PreScreeningStatus);
		}

		public void TestConsignment()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var response1 = new HVLVConsignmentPreScreeningResult(consignment);
			AssertEquals("Should be the same Guid", consignment.PK.ToGuid(), response1.Consignment.PK);
		}

		public void TestWarningMessage()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var response = new HVLVConsignmentPreScreeningResult(consignment);

			response.AddWarningMessage("Some warning message");
			AssertEquals("Should have warning message", "Some warning message", response.FormattedWarningMessage);

			response.AddWarningMessage("Some warning message #2");
			AssertEquals("Should have 2 warning messages", @"Some warning message
Some warning message #2", response.FormattedWarningMessage);
		}

		public void TestErrorMessage()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var response = new HVLVConsignmentPreScreeningResult(consignment);

			response.AddErrorMessage("Some error message");
			AssertEquals("Should have error message", "Some error message", response.FormattedErrorMessage);

			response.AddErrorMessage("Some error message #2");
			AssertEquals("Should have 2 error messages", @"Some error message
Some error message #2", response.FormattedErrorMessage);
		}

		public void TestNotifyOnlyWarningMessage()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var response = new HVLVConsignmentPreScreeningResult(consignment);

			response.AddNotifyOnlyWarningMessage("Some notify only warning message");
			AssertEquals("Should have warning message", "Some notify only warning message", response.FormattedNotifyOnlyWarningMessage);

			response.AddNotifyOnlyWarningMessage("Some notify only warning message #2");
			AssertEquals("Should have 2 warning messages", @"Some notify only warning message
Some notify only warning message #2", response.FormattedNotifyOnlyWarningMessage);
		}

		public void TestCombineResponse()
		{
			var consignment = Factory.New<HVLVConsignment>();

			var response1 = new HVLVConsignmentPreScreeningResult(consignment);
			response1.AddWarningMessage("Some warning message");

			var response2 = new HVLVConsignmentPreScreeningResult(consignment);
			response2.AddErrorMessage("Some error message");
			response2.AddNotifyOnlyWarningMessage("Some notify only warning message");

			AssertEquals("Pre-condition: Should have 1 warning message", "Some warning message", response1.FormattedWarningMessage);
			AssertEquals("Pre-condition: Should have no error message", string.Empty, response1.FormattedErrorMessage);
			AssertEquals("Pre-condition: Should have no notify only warning message", string.Empty, response1.FormattedNotifyOnlyWarningMessage);

			response1.CombineResponse(response2);
			AssertEquals("Should have 1 warning message", "Some warning message", response1.FormattedWarningMessage);
			AssertEquals("Should have 1 error message now", "Some error message", response1.FormattedErrorMessage);
			AssertEquals("Should have 1 notify only warning message now", "Some notify only warning message", response1.FormattedNotifyOnlyWarningMessage);
		}

		public void TestSerializeHVLVPreScreeningResult_ShouldNotPromptSelfReferencingLoopError()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var result = new HVLVConsignmentPreScreeningResult(consignment);

			AssertNoExceptionThrown("Self referencing loop not detected", () => JsonConvert.SerializeObject(result));
		}

		public void TestSerializePreScreenNotificationDetail_ShouldNotPromptSelfReferencingLoopError()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var message = "Some Prescreening Warning";
			var validationRule = "WRN";
			var detail = new PreScreenNotificationDetail(message, null, false, consignment, validationRule);

			AssertNoExceptionThrown("Self referencing loop not detected", () => JsonConvert.SerializeObject(detail));
		}
	}
}
