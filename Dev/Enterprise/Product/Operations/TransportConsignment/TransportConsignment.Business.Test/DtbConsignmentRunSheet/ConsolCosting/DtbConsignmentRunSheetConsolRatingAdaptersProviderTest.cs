using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbConsignmentRunSheetConsolRatingAdaptersProviderTest : DtbBookingConsignmentTestCaseWithFactory
	{
		#region TestIJobInvoicingHostWithAdditionalJobs_AdditionalJobs

		public void TestIJobInvoicingHostWithAdditionalJobs_AdditionalJobs()
		{
			var runSheet = Helper.CreateRunSheet();
			var provider = ((IRatingSupporter)runSheet).AdaptersProvider;
			AssertEquals(0, provider.GetAdditionalJobs().Count);

			var consignment1 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var consignment2 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			runSheet.AddNewRunSheetInstructions(new[] { consignment1.PickupInstruction.Confirmations.First(c => c.IsPickUp) });
			runSheet.AddNewRunSheetInstructions(new[] { consignment2.PickupInstruction.Confirmations.First(c => c.IsPickUp) });
			AssertContainsExactElementsInAnyOrder(new[] { consignment1, consignment2 }, provider.GetAdditionalJobs());
		}

		#endregion

		#region TestRunSheetWithNewConsignments_AdditionalJobs

		public void TestRunSheetWithNewConsignments_AdditionalJobs()
		{
			var runSheet = Helper.CreateRunSheet();
			var provider = ((IRatingSupporter)runSheet).AdaptersProvider;
			AssertEquals(0, provider.GetAdditionalJobs().Count);

			// New consignments and run sheets are only created in Glow.
			// provider.GetAdditionalJobs() accesses HasNewConsignments property and caches the value therefore, we have to clear this before testing it with new consignments.
			Factory.ClearCachedValue<bool>($"DtbConsignmentRunSheet|HasNewConsignments|{runSheet.PK}");
			var consignment1 = ConsignmentTestHelper.CreateConsignment("C1");
			var consignment2 = ConsignmentTestHelper.CreateConsignment("C2");
			var addressInConsignment1 = ConsignmentTestHelper.CreateConsignmentAddressWithAction(consignment1, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp);
			var addressInConsignment2 = ConsignmentTestHelper.CreateConsignmentAddressWithAction(consignment2, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp);
			ConsignmentTestHelper.CreateRunSheetInstruction(runSheet, addressInConsignment1.PickupAction);
			ConsignmentTestHelper.CreateRunSheetInstruction(runSheet, addressInConsignment2.PickupAction);
			AssertContainsExactElementsInAnyOrder(new[] { consignment1, consignment2 }, provider.GetAdditionalJobs());
		}

		TransportConsignmentTestHelper ConsignmentTestHelper
		{
			get { return consignmentTestHelper ?? (consignmentTestHelper = new TransportConsignmentTestHelper(Factory)); }
		}

		TransportConsignmentTestHelper consignmentTestHelper;

		#endregion
	}
}
