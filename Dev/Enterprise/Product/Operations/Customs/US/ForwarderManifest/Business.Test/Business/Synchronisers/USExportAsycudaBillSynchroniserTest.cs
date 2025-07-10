using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	[TestedType(typeof(USExportAsycudaBillSynchroniser))]
	public sealed class USExportAsycudaBillSynchroniserTest : ManifestBillSynchroniserTest
	{
		public void TestAESITNSynchroniser()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.CustomsEntryNumberType = "ITN";
			shipment.CustomsEntryNumber = "0123456789";
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();
			var manifestHeader = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();
			var manifestBill = manifestHeader.Bills[0];
			manifestBill.Header.AMA_RN_NKCountry = "US";
			AssertEquals("0123456789", manifestBill.AESITNNumbers);
		}

		public void TestInBondNumbers()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var header = Factory.New<InBond.Business.CusInBondHeader>();
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.InBondNumber = "INB12365";
			var moveHeader2 = header.MovementHeaders.AddNew();
			moveHeader2.InBondNumber = "";
			var moveHeader3 = header.MovementHeaders.AddNew();
			moveHeader3.InBondNumber = "INB12366";
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();
			var manifestHeader = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();
			var manifestBill = manifestHeader.Bills[0];
			manifestBill.Header.AMA_RN_NKCountry = "US";
			AssertEquals("INB12365,INB12366", manifestBill.InBondNumbers);
		}

		protected override IManifestBillForSynchroniser GetManifestBill(ForwardingConsol consol)
		{
			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			header.AMA_ParentId = consol.PK;
			header.AMA_ParentTableCode = "JK";
			var bill = header.Bills.AddNew();
			return bill;
		}

		protected override BusinessObjectSynchroniser GetManifestBillSynchroniser(IManifestBillForSynchroniser bill, ForwardingShipment shipment) => new USExportAsycudaBillSynchroniser((USExportAsycudaBill)bill, shipment);

		protected override ZString GetPortOfLading(ForwardingShipment shipment) => shipment.JS_RL_NKOrigin;

		protected override ZString GetPlaceOfReceipt(ForwardingShipment shipment) => shipment.JS_RL_NKDestination;

		protected override ZString GetLastForeignPort(ForwardingShipment shipment) => shipment.JS_RL_NKOrigin;
	}
}
