using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CommonWorkSheetCostSupporterTest : TestCaseWithFactory
	{
		public void TestIGenericJobCostSupporter_Precondition()
		{
			var workSheet = Factory.New<CommonWorkSheet>();
			var costSupporter = (IGenericJobCostSupporter)new CommonWorkSheetCostSupporter(workSheet);
			AssertEquals("Get Creditor PK", ZGuid.Empty, costSupporter.GetCreditorPK("TRN", ZGuid.Empty));
			AssertEquals("PK", workSheet.PK, costSupporter.PK);
			AssertEquals("Type", "EY", costSupporter.Type);
			AssertEquals("Shipments List PKs", 0, costSupporter.ShipmentsListPKs.Length);
			AssertEquals("Shipments List", 0, costSupporter.ShipmentsList.Length);
			AssertEquals("Has Changes", false, costSupporter.HasChanges);
			AssertEquals("Is in Database", false, costSupporter.IsInDatabase);
			AssertNull("Document Supporter", costSupporter.DocumentSupporter);
			AssertEquals("Master Bill Num", ZString.Empty, costSupporter.MasterBillNum);
			AssertEquals("Transport Mode", ZString.Empty, costSupporter.TransportMode);
			AssertEquals("Total Chargeable Unit", ZString.Empty, costSupporter.TotalChargeableUnit);
			AssertEquals("Direction", Directions.Unknown, costSupporter.Direction);
			AssertEquals("Is Buyers Consol", false, costSupporter.IsBuyersConsol);
			AssertEquals("Port of Loading", ZString.Empty, costSupporter.PortOfLoading);
			AssertEquals("Port of Discharge", ZString.Empty, costSupporter.PortOfDischarge);
			AssertEquals("Consol Mode", ZString.Empty, costSupporter.ConsolMode);
			AssertNull("Sending Forwarder", costSupporter.SendingForwarder);
			AssertNull("Receiving Forwarder", costSupporter.ReceivingForwarder);
			AssertNull("Shipments", costSupporter.Shipments);
			AssertEquals("ETD", ZDateTime.Empty, costSupporter.ETD);
			AssertEquals("ETA", ZDateTime.Empty, costSupporter.ETA);
			AssertContainsExactElementsInAnyOrder(new ZString[] { AllocationMethod.Revenue, AllocationMethod.CapacityPerContainer, AllocationMethod.FreeSpaceContribution }, costSupporter.ExcludedApportionmentMethods);
			AssertEquals("Is Apportionment Filter Enabled", false, costSupporter.IsApportionmentFilterEnabled);
			AssertEquals("JobConsolCostingCheckPoint", Env.Security.None, costSupporter.JobConsolCostingCheckPoint);
		}

		public void TestShipmentsList()
		{
			// Arrange
			var workSheet = Factory.New<CommonWorkSheet>();
			var cartage1 = Factory.New<CommonCartage>();
			var cartage2 = Factory.New<CommonCartage>();
			workSheet.CartageLegs.AddRange(Helper.CreateCartageLegs(cartage1, 2));
			workSheet.CartageLegs.AddRange(Helper.CreateCartageLegs(cartage2, 3));
			// Act
			var costSupporter = (IGenericJobCostSupporter)new CommonWorkSheetCostSupporter(workSheet);
			// Assert
			AssertEquals("Expecting 2 shipments", 2, costSupporter.ShipmentsList.Length);
			var plugins = costSupporter.ShipmentsList.OfType<CommonWorkSheetInvoicingPlugIn>().ToArray();
			AssertEquals("Expecting all shipments with correct plugin type", 2, plugins.Length);
			AssertContainsExactElementsInAnyOrder(new[] { cartage1.PK, cartage2.PK }, plugins.Cast<IJobHeaderParentCore>().Select(plugin => plugin.PK));
			AssertContainsExactElementsInAnyOrder(new[] { cartage1.PK, cartage2.PK }, costSupporter.ShipmentsListPKs);
		}

		public void TestGetCreditorPK()
		{
			var transportOrganisation = Helper.CreateOrganisation("TO42");
			var workSheet = Factory.New<CommonWorkSheet>();
			var costSupporter = (IGenericJobCostSupporter)new CommonWorkSheetCostSupporter(workSheet);
			// Case 1: When Creditor is not set
			AssertEquals("Expecting empty GUID when creditor and charge code are not valid", ZGuid.Empty, costSupporter.GetCreditorPK("XYZ", ZGuid.Empty));
			AssertEquals("Expecting empty GUID when charge code is valid but creditor is not set", ZGuid.Empty, costSupporter.GetCreditorPK("TRN", ZGuid.Empty));
			// Case 2: When Creditor is set
			workSheet.EY_OH_TransportCo = transportOrganisation.PK;
			AssertEquals("Expecting empty GUID when creditor is set but charge code is not valid", ZGuid.Empty, costSupporter.GetCreditorPK("XYZ", ZGuid.Empty));
			AssertEquals("Expecting creditor PK when creditor and charge code are valid", transportOrganisation.PK, costSupporter.GetCreditorPK("TRN", ZGuid.Empty));
		}

		readonly TestCommonWorkSheetHelper Helper;
		public CommonWorkSheetCostSupporterTest()
		{
			Helper = new TestCommonWorkSheetHelper(Factory);
		}
	}
}
