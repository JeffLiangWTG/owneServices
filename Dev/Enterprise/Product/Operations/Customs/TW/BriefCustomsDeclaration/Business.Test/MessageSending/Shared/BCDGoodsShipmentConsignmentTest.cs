using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(BCDGoodsShipmentConsignment))]
	sealed class BCDGoodsShipmentConsignmentTest : TestCaseWithFactory
	{
		public void TestGoodsShipmentConsignmentData()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_CustomsValue = 101m;
			bill.ABL_BillNumber = "A12345";
			bill.ABL_ManifestQty = 999;
			bill.ABL_Remarks = "remarks test";
			bill.ABL_RL_NKPortOfLoading = "USLAX";
			bill.ABL_Procedure = "C";
			bill.ABL_ManifestUQ = "KG";
			bill.ABL_RL_NKPortOfDischarge = "CNSHA";

			IBCDConsignment consignment = new BCDGoodsShipmentConsignment(bill);
			CombineAssertions(() =>
			{
				AssertEquals("TotalPackageQuantity", 999, consignment.TotalPackageQuantity);
				AssertEquals("InvoiceAmount", 101m, consignment.InvoiceAmount);
				AssertEquals("AssociatedGovernmentProcedureCode", "C", ((IConsignmentItem)consignment).AssociatedGovernmentProcedureCode);
				AssertEquals("GovernmentProcedure Description", "remarks test", consignment.GovernmentProcedures.Single().Description);
				AssertEquals("LoadingLocation", "USLAX", consignment.LoadingLocation.ID);
				AssertEquals("UnloadingLocation", "CNSHA", consignment.UnloadingLocation.ID);
				AssertEquals("IPackaging TypeCode", "KG", ((IPackaging)consignment).TypeCode);
				AssertEquals("TransportContractDocumentId", "A12345", consignment.TransportContractDocumentId);
			});
		}
	}
}
