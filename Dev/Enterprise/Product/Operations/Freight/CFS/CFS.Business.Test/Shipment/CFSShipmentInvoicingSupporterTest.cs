using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSShipment.CFSShipmentInvoicingSupporter))]
	public class CFSShipmentInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public override void TestCustomsEntryNumberType()
		{
			var shipment = Factory.New<CFSShipment>();
			shipment.CustomsEntryNumberType = "TF";
			var supporter = new CFSShipment.CFSShipmentInvoicingSupporter(shipment);

			AssertEquals("CustomsEntryNumberType", "TF", supporter.CustomsEntryNumberType);
		}

		public override void TestCommunityTransitStatus()
		{
			var shipment = Factory.New<CFSShipment>();
			shipment.JS_CommunityTransitStatus = "TF";
			var supporter = new CFSShipment.CFSShipmentInvoicingSupporter(shipment);

			AssertEquals("CommunityTransitStatus", "TF", supporter.CommunityTransitStatus);
		}

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			CFSShipment cfsShipment = Factory.NewWithValidTestData<CFSShipment>();
			return cfsShipment;
		}
	}
}
