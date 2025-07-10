using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBillSynchroniser))]
	sealed class AsycudaBillSynchroniserTest : ManifestBillSynchroniserTest
	{
		public void TestBillSync()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_Code = "org1";
			header.OH_FullName = "Company Name";
			var address = header.Addresses.AddNew();
			address.OA_Address1 = "Test Address1";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKLoadPort = "TRIST";
			shipment.JS_RL_NKDischargePort = "TRIZT";
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_OA_ReceivingForwarderAddress = address.PK;

			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "TRIST";
			transport.JW_RL_NKDiscPort = "TRIZT";
			
			Factory.Save();

			shipment.Consols.Add(consol);
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			var manifestBill = manifestHeader.Bills.AddNew();
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			AssertEquals("ABL_OA_ContainerAgent", "Company Name", manifestBill.ContainerAgent.CompanyName);
		}

		protected override IManifestBillForSynchroniser GetManifestBill(ForwardingConsol consol)
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ParentId = Consol.PK;
			header.AMA_ParentTableCode = "JK";
			var bill = header.Bills.AddNew();
			return bill;
		}

		protected override BusinessObjectSynchroniser GetManifestBillSynchroniser(IManifestBillForSynchroniser bill, ForwardingShipment shipment) => new AsycudaBillSynchroniser((AsycudaBill)bill, shipment);

		protected override ZString GetPortOfLading(ForwardingShipment shipment) => shipment.JS_RL_NKOrigin;

		protected override ZString GetPlaceOfReceipt(ForwardingShipment shipment) => shipment.JS_RL_NKDestination;

		protected override ZString GetLastForeignPort(ForwardingShipment shipment) => shipment.JS_RL_NKOrigin;
	}
}
