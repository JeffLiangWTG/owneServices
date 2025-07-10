using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(CusInBondBillSynchroniser))]
	sealed class CusInBondBillSynchroniserInheritedTest : Customs.Business.Testing.ManifestBillSynchroniserTest
	{
		protected override IManifestBillForSynchroniser GetManifestBill(ForwardingConsol consol)
		{
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = "JK";
			var bill = header.Bills.AddNew();
			return bill;
		}

		protected override BusinessObjectSynchroniser GetManifestBillSynchroniser(IManifestBillForSynchroniser bill, ForwardingShipment shipment)
		{
			return new CusInBondBillSynchroniser((CusInBondBill)bill, shipment);
		}

		protected override ZString GetPlaceOfReceipt(ForwardingShipment shipment)
		{
			return shipment.JS_RL_NKDestination;
		}

		protected override ZString GetPortOfLading(ForwardingShipment shipment)
		{
			return shipment.JS_RL_NKOrigin;
		}

		protected override ZString GetLastForeignPort(ForwardingShipment shipment)
		{
			return shipment.JS_RL_NKOrigin;
		}
	}
}
