using System.ComponentModel;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(CusInBondContainerCollection))]
	class CusInBondContainerCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInBondContainerCollection>
	{
		public void TestShouldNotAccessDeletedMaster()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var containers = bill.MovementDetail.Containers;
			containers.AddNew();
			bill.Delete();
			AssertEquals(((IBindingList)containers).AllowNew, ((IBindingList)containers).AllowNew); // just to call AllowNew
			AssertNotContains(" Should not be accessing a property on a deleted business object", ErrorReporter.LastMessageReported);
		}

		public void TestIndexer_ContainerNum()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.B0_WeightUQ = Core.Constants.Weight.Grams;
			var moveDetail = bill.MovementDetail;
			var containers = moveDetail.Containers;
			var container1 = containers.AddNew();
			container1.BC_ContainerNum = "CONT1";
			var container2 = containers.AddNew();
			container2.BC_ContainerNum = "CONT2";
			var container3 = containers.AddNew();
			container3.BC_ContainerNum = "CONT3";
			AssertEquals(container2, containers["CONT2"]);
			AssertEquals(container1, containers["CONT1"]);
			AssertEquals(container3, containers["CONT3"]);
			AssertNull(containers["CONT4"]);
			AssertNull(containers[""]);
			container1.Delete();
			AssertNull(containers["CONT1"]);
		}

		public void TestTotalManifestQty()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.B0_WeightUQ = Core.Constants.Weight.Grams;
			var moveDetail = bill.MovementDetail;
			var containers = moveDetail.Containers;
			var container1 = containers.AddNew();
			var commodities1 = container1.Commodities;
			var commodity1 = commodities1.AddNew();
			commodity1.BY_PieceCount = 10;
			AssertEquals(10, containers.TotalManifestQty);

			var commodity2 = commodities1.AddNew();
			commodity2.BY_PieceCount = 25;
			AssertEquals(35, containers.TotalManifestQty);

			var container2 = containers.AddNew();
			var commodities2 = container2.Commodities;
			var commodity3 = commodities2.AddNew();
			commodity3.BY_PieceCount = 35;
			AssertEquals(70, containers.TotalManifestQty);

			var commodity4 = commodities2.AddNew();
			commodity4.BY_PieceCount = 30;
			AssertEquals(100, containers.TotalManifestQty);
		}

		public void TestTotalMonetaryValue()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.B0_WeightUQ = Core.Constants.Weight.Grams;
			var moveDetail = bill.MovementDetail;
			var containers = moveDetail.Containers;
			var container1 = containers.AddNew();
			var commodities1 = container1.Commodities;
			var commodity1 = commodities1.AddNew();
			commodity1.BY_MonetaryValue = 10m;
			AssertEquals(10m, containers.TotalMonetaryValue);

			var commodity2 = commodities1.AddNew();
			commodity2.BY_MonetaryValue = 25m;
			AssertEquals(35m, containers.TotalMonetaryValue);

			var container2 = containers.AddNew();
			var commodities2 = container2.Commodities;
			var commodity3 = commodities2.AddNew();
			commodity3.BY_MonetaryValue = 35m;
			AssertEquals(70m, containers.TotalMonetaryValue);

			var commodity4 = commodities2.AddNew();
			commodity4.BY_MonetaryValue = 30m;
			AssertEquals(100m, containers.TotalMonetaryValue);
		}

		public void TestTotalCargoWeight()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.B0_WeightUQ = Core.Constants.Weight.Grams;
			var moveDetail = bill.MovementDetail;
			var containers = moveDetail.Containers;
			var container1 = containers.AddNew();
			var commodities1 = container1.Commodities;
			var commodity1 = commodities1.AddNew();
			commodity1.BY_GrossWeight = 0.125m;
			commodity1.BY_GrossWeightUnit = Core.Constants.Weight.Tonnes;
			AssertEquals(new ZWeight(0.125m, Core.Constants.Weight.Tonnes), containers.TotalCargoWeight);

			var commodity2 = commodities1.AddNew();
			commodity2.BY_GrossWeight = 0.125m;
			commodity2.BY_GrossWeightUnit = Core.Constants.Weight.Tonnes;
			AssertEquals(new ZWeight(0.25m, Core.Constants.Weight.Tonnes), containers.TotalCargoWeight);

			var container2 = containers.AddNew();
			var commodities2 = container2.Commodities;
			var commodity3 = commodities2.AddNew();
			commodity3.BY_GrossWeight = 0.1m;
			commodity3.BY_GrossWeightUnit = Core.Constants.Weight.Tonnes;
			AssertEquals(new ZWeight(0.35m, Core.Constants.Weight.Tonnes), containers.TotalCargoWeight);

			var commodity4 = commodities2.AddNew();
			commodity4.BY_GrossWeight = 5m;
			commodity4.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals(new ZWeight(355000m, Core.Constants.Weight.Grams), containers.TotalCargoWeight);
		}

		protected override CusInBondContainerCollection GetCollectionToTest()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var moveDetail = moveHeader.MovementDetails.AddNew();
			return new CusInBondContainerCollection(moveDetail);
		}
	}
}
