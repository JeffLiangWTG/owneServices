using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbConsignmentRunSheetCostSupporterTest : DtbBookingConsignmentTestCaseWithFactory
	{
		public void TestConsignments()
		{
			var runSheet = Helper.CreateRunSheet();
			IGenericJobCostSupporter costSupporter = new DtbConsignmentRunSheetCostSupporter(runSheet);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<IJobInvoicingPlugIn>(), costSupporter.ShipmentsList);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ZGuid>(), costSupporter.ShipmentsListPKs);

			var consignment1 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var consignment2 = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			runSheet.AddNewRunSheetInstructions(new[] { consignment1.PickupInstruction.Confirmations.First(c => c.IsPickUp) });
			runSheet.AddNewRunSheetInstructions(new[] { consignment2.PickupInstruction.Confirmations.First(c => c.IsPickUp) });
			costSupporter = new DtbConsignmentRunSheetCostSupporter(runSheet);
			AssertContainsExactElementsInAnyOrder(new IJobInvoicingPlugIn[] { consignment1, consignment2 }, costSupporter.ShipmentsList);
			AssertContainsExactElementsInAnyOrder(new[] { consignment1.PK, consignment2.PK }, costSupporter.ShipmentsListPKs);
		}

		public void TestCreditorPK()
		{
			var runSheet = Helper.CreateRunSheet();
			IGenericJobCostSupporter costSupporter = new DtbConsignmentRunSheetCostSupporter(runSheet);

			var chargeCodeGroupCodes = ((ChargeCodeGroupList)System.Activator.CreateInstance(typeof(ChargeCodeGroupList), null));
			var validCodeGroup = new[] { ChargeCodeGroupList.Codes.Transport, ChargeCodeGroupList.Codes.TransportBooking };
			var org = Helper.CreateOrganisation("RS");
			var runSheetWithOrg = Helper.CreateRunSheet(org);
			IGenericJobCostSupporter costSupporterWithOrg = new DtbConsignmentRunSheetCostSupporter(runSheetWithOrg);
			foreach (var chargeCodeGroup in chargeCodeGroupCodes)
			{
				var code = chargeCodeGroup.ToString();
				var expectCreditor = validCodeGroup.Contains(code);
				var expectedCreditor = expectCreditor ? runSheetWithOrg.TransportCo.PK : ZGuid.Empty;
				AssertEquals(string.Format("Get Creditor {0} have a creditor for ChargeGroupCode {1}", expectCreditor ? "should" : "should not", code), expectedCreditor, costSupporterWithOrg.GetCreditorPK(code, ZGuid.Empty));
			}
		}
	}
}
