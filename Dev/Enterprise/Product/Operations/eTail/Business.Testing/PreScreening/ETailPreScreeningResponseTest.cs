using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.eTail.Business.Testing
{
	public class ETailPreScreeningResponseTest : TestCaseWithFactory
	{
		public void TestPreScreeningFinished()
		{
			var response = new ETailPreScreeningResponse();
			AssertNull("Abort reason is null by default", response.ErrorMessage);
			Assert(response.Finished);
		}

		public void TestPreScreeningNotFinished()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var response = new ETailPreScreeningConsignmentNotFoundResponse(consignment.PK.ToGuid());

			Assert("Should not finished when has error message", !response.Finished);
			AssertNull("Should return null result when Pre-Screening aborted", response.Results);
		}

		public void TestPreScreeningErrorMessage()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var response1 = new ETailPreScreeningConsignmentNotFoundResponse(consignment.PK.ToGuid());
			AssertEquals("Match abort reason", string.Format("Request failed: Consignment {0} not found", consignment.PK), response1.ErrorMessage);

			var response2 = new ETailPreScreeningRegistryDisabledResponse();
			AssertEquals("Match abort reason", "Request failed: HVLV Pre-Screening is disabled", response2.ErrorMessage);

			var response3 = new ETailPreScreeningUnsupportedConsignmentParentTypeResponse("XX");
			AssertEquals("Match abort reason", "Request failed: Unsupported consignment parent type XX", response3.ErrorMessage);

			var response4 = new ETailPreScreeningConsignmentParentNotFoundResponse("XX", consignment.PK.ToGuid());
			AssertEquals("Match abort reason", string.Format("Request failed: Consignment parent {0} not found with table code XX", consignment.PK), response4.ErrorMessage);

			var response5 = new ETailPreScreeningEmptyConsignmentCollectionResponse("XX", consignment.PK.ToGuid());
			AssertEquals("Match abort reason", string.Format("Request failed: Consignment parent {0} with table code XX has no consignment", consignment.PK), response5.ErrorMessage);
		}

		public void TestPreScreeningResults()
		{
			var response = new ETailPreScreeningResponse();
			AssertNotNull(response.Results);
			AssertEquals("precondition", 0, response.Results.Count);

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			response.AddPreScreeningResult(new HVLVConsignmentPreScreeningResult(consignment));
			AssertEquals("Have 1 result", 1, response.Results.Count);
		}

		public void TestIsPreScreeningPass()
		{
			var response = new ETailPreScreeningResponse();
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			var preScreeningResult1 = new HVLVConsignmentPreScreeningResult(consignment);
			response.AddPreScreeningResult(preScreeningResult1);

			Assert("precondiction : all consignments passed pre-screening ", !response.Results.Any(n => n.PreScreeningStatus == "FAL"));
			Assert(response.Passed);

			var preScreeningResult2 = new HVLVConsignmentPreScreeningResult(consignment);
			preScreeningResult2.AddErrorMessage("error");
			response.AddPreScreeningResult(preScreeningResult2);

			Assert("precondiction : not all consignments passed pre-screening ", response.Results.Any(n => n.PreScreeningStatus == "FAL"));
			Assert(!response.Passed);
		}
	}
}
