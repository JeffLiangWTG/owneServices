using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class N5101HConsignmentTest : TestCaseWithFactory
	{
		public void TestBoardedQuantity()
		{
			AssertEquals(5m, consignment.BoardedQuantity);

			bill.BagNumber = ZString.Empty;
			consignment = new N5101HConsignment(bill, 5m);
			AssertEquals(0m, consignment.BoardedQuantity);
		}

		public void TestTotalPackageQuantity()
		{
			bill.ABL_ManifestQty = 100;
			AssertEquals(100m, consignment.TotalPackageQuantity);
		}

		public void TestEscortMark()
		{
			AssertEquals("A12", consignment.EscortMark);
		}

		public void TestTotalGrossMassMeasure()
		{
			bill.ABL_GrossWeight = 10000m;
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Grams;
			AssertEquals(10m, consignment.TotalGrossMassMeasure);
		}

		public void TestTypeCode()
		{
			AssertEquals("1", consignment.TypeCode);
		}

		public void TestAssociatedTransportDocumentId()
		{
			bill.BagNumber = "B1";
			AssertEquals("B1", consignment.AssociatedTransportDocumentId);
		}

		public void TestConsignee()
		{
			AssertType<N5101HConsignee>(consignment.Consignee);
		}

		public void TestConsignmentItem()
		{
			AssertType<N5101HConsignmentItem>(consignment.ConsignmentItem);
		}

		public void TestConsignor()
		{
			AssertType<N5101HConsignor>(consignment.Consignor);
		}

		public void TestGoodsLocationId()
		{
			bill.ABL_GoodsLocation = "123";
			AssertEquals("123", consignment.GoodsLocationId);
		}

		public void TestLoadingLocation()
		{
			bill.ABL_RL_NKPortOfLoading = "TWKEL";
			bill.ABL_LocationInformation = "DESC Test";
			var loadingLocation = consignment.LoadingLocation;
			CombineAssertions(() =>
			{
				AssertType<N5101HLoadingLocation>(loadingLocation);
				AssertEquals("TWKEL", loadingLocation.ID);
				AssertEquals("DESC Test", loadingLocation.Name);
			});
		}

		public void TestNotifyParties()
		{
			AssertType<N5101HNotifyParty[]>(consignment.NotifyParties);
		}

		public void TestTransportContractDocument()
		{
			AssertType<N5101HTransportContractDocument>(consignment.TransportContractDocument);
		}

		public void TestTransportEquipments()
		{
			var transportEquipment = consignment.TransportEquipments.FirstOrDefault();

			CombineAssertions(() =>
			{
				AssertEquals(1, consignment.TransportEquipments.Count());
				AssertNotNull(transportEquipment);
				AssertEquals("40GP", transportEquipment.CharacteristicCode);
				AssertEquals("1", transportEquipment.UsedCapacityCode);
				AssertEquals("1234567890", transportEquipment.ID);
				AssertContainsExactElementsInExactOrder(new string[] { "Seal1", "Seal2", "Seal3" }, transportEquipment.Seals);
			});
		}

		public void TestUnloadingLocationId()
		{
			bill.ABL_RL_NKFinalDestination = "TWTPE";
			AssertEquals("TWTPE", consignment.UnloadingLocationId);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var refContainer1 = Factory.NewWithValidTestData<RefContainer>();
			refContainer1.RC_Code = "50GP";
			var refContainer2 = Factory.NewWithValidTestData<RefContainer>();
			refContainer2.RC_Code = "40GP";
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var headerContainer = header.Containers.AddNew();
			headerContainer.ACN_RC_ContainerType = refContainer1.PK;
			headerContainer.ACN_ContainerNumber = "hcn";
			headerContainer.ACN_EmptyFullIndicator = "2";

			var billContainer = header.Containers.AddNew();
			billContainer.ACN_RC_ContainerType = refContainer2.PK;
			billContainer.ACN_ContainerNumber = "1234567890";
			billContainer.ACN_EmptyFullIndicator = "1";
			billContainer.ACN_Seal1 = "Seal1";
			billContainer.ACN_Seal2 = "Seal2";
			billContainer.ACN_Seal3 = "Seal3";

			bill = header.Bills.AddNew();
			bill.ABL_SpecialCargoCode = "A12";
			bill.ABL_ShipmentType = TWManifestShipmentTypes.Codes.Import;
			bill.BagNumber = "A";
			bill.LinkContainer(billContainer.PK);

			consignment = new N5101HConsignment(bill, 5m);
		}

		AsycudaBill bill;
		N5101HConsignment consignment;
	}
}
