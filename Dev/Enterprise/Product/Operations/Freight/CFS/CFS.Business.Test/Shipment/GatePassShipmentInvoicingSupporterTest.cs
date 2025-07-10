using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(GatePassShipment.GatePassShipmentInvoicingSupporter))]
	public class GatePassShipmentInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public override void TestCustomsEntryNumberType()
		{
			var shipment = Factory.New<GatePassShipment>();
			shipment.CustomsEntryNumberType = "TF";
			var supporter = new GatePassShipment.GatePassShipmentInvoicingSupporter(shipment);

			AssertEquals("CustomsEntryNumberType", "TF", supporter.CustomsEntryNumberType);
		}

		public override void TestCommunityTransitStatus()
		{
			var shipment = Factory.New<GatePassShipment>();
			shipment.JS_CommunityTransitStatus = "TF";
			var supporter = new GatePassShipment.GatePassShipmentInvoicingSupporter(shipment);

			AssertEquals("CommunityTransitStatus", "TF", supporter.CommunityTransitStatus);
		}

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			GatePassShipment gatePassShipment = Factory.NewWithValidTestData<GatePassShipment>();
			return gatePassShipment;
		}
	}
}
