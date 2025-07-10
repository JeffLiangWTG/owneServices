using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class UNDGDataItemSynchroniserTest : SynchroniserTestCase
	{
		public void TestSynchroniseDI_DG()
		{
			source.DI_DG = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, destination.DI_DG);
			var subs = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First();
			source.DI_DG = subs.PK;
			AssertEquals(subs.PK, destination.DI_DG);
		}

		public void TestSynchroniseDI_TechnicalName()
		{
			source.DI_TechnicalName = "";
			AssertEquals("", destination.DI_TechnicalName);

			source.DI_TechnicalName = "ABCD2";
			AssertEquals("ABCD2", destination.DI_TechnicalName);

			source.DI_TechnicalName = "ABCD4";
			AssertEquals("ABCD4", destination.DI_TechnicalName);
		}

		public void TestSynchroniseDI_DGFlashPoint()
		{
			source.DI_DGFlashPoint = 0m;
			AssertEquals(0m, destination.DI_DGFlashPoint);

			source.DI_DGFlashPoint = 10m;
			AssertEquals(10m, destination.DI_DGFlashPoint);

			source.DI_DGFlashPoint = 15.2m;
			AssertEquals(15.2m, destination.DI_DGFlashPoint);
		}

		public void TestSynchroniseDI_OC_DGContact()
		{
			source.DI_OC_DGContact = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, destination.DI_OC_DGContact);

			var pk1 = ZGuid.NewZGuid();
			source.DI_OC_DGContact = pk1;
			AssertEquals(pk1, destination.DI_OC_DGContact);

			var pk2 = ZGuid.NewZGuid();
			source.DI_OC_DGContact = pk2;
			AssertEquals(pk2, destination.DI_OC_DGContact);
		}

		UNDGDataItem source;
		UNDGDataItem destination;
		UNDGDataItemSynchroniser synchroniser;

		protected override void SetUp()
		{
			base.SetUp();
			var shipment = Factory.New<ForwardingShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			var header = (CusInBondHeader)Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();

			var bill = (CusInBondBill)header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var container = (CusInBondContainer)moveDetail.Containers.AddNew();
			source = packLine.UNDGs.AddNew();
			var collectionSynchroniser = new UNDGDataItemCollectionSynchroniser(shipment, container);
			synchroniser = collectionSynchroniser.AddUNDGSynchroniserIfNotExists(source);
			destination = synchroniser.destination;
			synchroniser.Synchronise(true);
		}
	}
}
