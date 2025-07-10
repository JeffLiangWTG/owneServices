using System.ComponentModel;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(CusInBondCargoDescCollection))]
	sealed class CusInBondCargoDescCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInBondCargoDescCollection>
	{
		public void TestShouldNotAccessDeletedMaster()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var container = bill.MovementDetail.Containers.AddNew();
			var commodities = container.Commodities;
			commodities.AddNew();
			container.Delete();
			AssertEquals(((IBindingList)commodities).AllowNew, ((IBindingList)commodities).AllowNew); // just to call AllowNew
			AssertNotContains(" Should not be accessing a property on a deleted business object", ErrorReporter.LastMessageReported);
		}

		public void TestIOverrideDefaultValuesCollectionMembers()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var container = bill.MovementDetail.Containers.AddNew();
			IOverrideDefaultValuesCollection collection = container.Commodities;
			AssertEquals(false, collection.IsOverrideDefaultValuesEnabled);
			var info = collection.OverrideDefaultValuesInfo;
			AssertEquals("BH_OverrideFreightDefaults", info.Name);
			var consol = Factory.New<ForwardingConsol>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			AssertEquals(true, collection.IsOverrideDefaultValuesEnabled);
		}

		public void TestIndexer_Tariff()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.B0_WeightUQ = Core.Constants.Weight.Grams;
			var moveDetail = bill.MovementDetail;
			var container = moveDetail.Containers.AddNew();
			var commodities = container.Commodities;
			var commodity1 = commodities.AddNew();
			commodity1.BY_HarmonisedTariff = "10 10 .1101";
			var commodity2 = commodities.AddNew();
			commodity2.BY_HarmonisedTariff = "20122030";
			var commodity3 = commodities.AddNew();
			commodity3.BY_HarmonisedTariff = "3 0 350.355";
			AssertEquals(commodity2, commodities["2012.20 30"]);
			AssertEquals(commodity1, commodities["1010 .1101"]);
			AssertEquals(commodity3, commodities["30350355"]);
			AssertNull(commodities["1040"]);
			AssertNull(commodities[""]);
			commodity1.Delete();
			AssertNull(commodities["1010 .1101"]);
		}

		public void TestDefaultingNewCommodity()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.B0_RL_NKPortOfLading = "AUSYD";
			bill.B0_Weight = 355000m;
			bill.B0_WeightUQ = Core.Constants.Weight.Grams;
			bill.B0_ManifestQty = 100;
			bill.B0_ManifestUQ = Core.Constants.PkgUnit.Package;
			var moveDetail = bill.MovementDetail;
			var containers = moveDetail.Containers;
			var container1 = containers.AddNew();
			var commodities1 = container1.Commodities;
			var commodity1 = commodities1.AddNew();
			AssertEquals(355000m, commodity1.BY_GrossWeight);
			AssertEquals(Core.Constants.Weight.Grams, commodity1.BY_GrossWeightUnit);
			AssertEquals(100, commodity1.BY_PieceCount);
			AssertEquals(Core.Constants.PkgUnit.Package, commodity1.BY_ManifestUnitCode);
			AssertEquals(Core.Constants.CountryCodes.Australia, commodity1.BY_RN_NKCountryOfOrigin);

			commodity1.BY_GrossWeight = 0.125m;
			commodity1.BY_GrossWeightUnit = Core.Constants.Weight.Tonnes;
			commodity1.BY_PieceCount = 25;
			commodity1.BY_ManifestUnitCode = Core.Constants.PkgUnit.Box;
			commodity1.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.NewZealand;
			var commodity2 = commodities1.AddNew();
			AssertEquals(0.230m, commodity2.BY_GrossWeight);
			AssertEquals(Core.Constants.Weight.Tonnes, commodity2.BY_GrossWeightUnit);
			AssertEquals(75, commodity2.BY_PieceCount);
			AssertEquals(Core.Constants.PkgUnit.Box, commodity2.BY_ManifestUnitCode);
			AssertEquals(Core.Constants.CountryCodes.NewZealand, commodity2.BY_RN_NKCountryOfOrigin);

			commodity2.BY_GrossWeight = 0.1m;
			commodity2.BY_PieceCount = 35;

			var container2 = containers.AddNew();
			var commodities2 = container2.Commodities;
			var commodity3 = commodities2.AddNew();
			AssertEquals(130000m, commodity3.BY_GrossWeight);
			AssertEquals(Core.Constants.Weight.Grams, commodity3.BY_GrossWeightUnit);
			AssertEquals(40, commodity3.BY_PieceCount);
			AssertEquals(Core.Constants.PkgUnit.Package, commodity3.BY_ManifestUnitCode);
			AssertEquals(Core.Constants.CountryCodes.Australia, commodity3.BY_RN_NKCountryOfOrigin);

			commodity3.BY_GrossWeight = 30m;
			commodity3.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			commodity3.BY_PieceCount = 15;
			commodity3.BY_ManifestUnitCode = Core.Constants.PkgUnit.Sheet;
			commodity3.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.PapuaNewGuinea;
			var commodity4 = commodities2.AddNew();
			AssertEquals(100m, commodity4.BY_GrossWeight);
			AssertEquals(Core.Constants.Weight.Kilograms, commodity4.BY_GrossWeightUnit);
			AssertEquals(25, commodity4.BY_PieceCount);
			AssertEquals(Core.Constants.PkgUnit.Sheet, commodity4.BY_ManifestUnitCode);
			AssertEquals(Core.Constants.CountryCodes.PapuaNewGuinea, commodity4.BY_RN_NKCountryOfOrigin);

			commodity4.BY_GrossWeightUnit = "UW";
			CusInBondCargoDesc commodity5 = null;
			AssertNoExceptionThrown(
				"No exception should be thrown even the previous has an invalid BY_GrossWeightUnit",
				() => commodity5 = commodities2.AddNew()
			);
			AssertEquals("Should not default unit even it is invalid.", "UW", commodity5.BY_GrossWeightUnit);
			AssertEquals("Should not default quantity for invalid unit.", ZDecimal.Zero, commodity5.BY_GrossWeight);
		}

		public void TestTotalManifestQty()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var container = moveDetail.Containers.AddNew();
			var commodities = container.Commodities;
			var commodity1 = commodities.AddNew();
			commodity1.BY_PieceCount = 10;
			AssertEquals(10, commodities.TotalManifestQty);

			var commodity2 = commodities.AddNew();
			commodity2.BY_PieceCount = 25;
			AssertEquals(35, commodities.TotalManifestQty);

			var commodity3 = commodities.AddNew();
			commodity3.BY_PieceCount = 35;
			AssertEquals(70, commodities.TotalManifestQty);
		}

		public void TestTotalMonetaryValue()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var container = moveDetail.Containers.AddNew();
			var commodities = container.Commodities;
			var commodity1 = commodities.AddNew();
			commodity1.BY_MonetaryValue = 10m;
			AssertEquals(10m, commodities.TotalMonetaryValue);

			var commodity2 = commodities.AddNew();
			commodity2.BY_MonetaryValue = 25m;
			AssertEquals(35m, commodities.TotalMonetaryValue);

			var commodity3 = commodities.AddNew();
			commodity3.BY_MonetaryValue = 35m;
			AssertEquals(70m, commodities.TotalMonetaryValue);
		}

		public void TestTotalCargoWeight()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.B0_WeightUQ = Core.Constants.Weight.Grams;
			var moveDetail = bill.MovementDetail;
			var container = moveDetail.Containers.AddNew();
			var commodities = container.Commodities;
			var commodity1 = commodities.AddNew();
			commodity1.BY_GrossWeight = 0.125m;
			commodity1.BY_GrossWeightUnit = Core.Constants.Weight.Tonnes;
			AssertEquals(new ZWeight(0.125m, Core.Constants.Weight.Tonnes), commodities.TotalCargoWeight);

			var commodity2 = commodities.AddNew();
			commodity2.BY_GrossWeight = 0.125m;
			commodity2.BY_GrossWeightUnit = Core.Constants.Weight.Tonnes;
			AssertEquals(new ZWeight(0.25m, Core.Constants.Weight.Tonnes), commodities.TotalCargoWeight);

			var commodity3 = commodities.AddNew();
			commodity3.BY_GrossWeight = 5m;
			commodity3.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals(new ZWeight(255000m, Core.Constants.Weight.Grams), commodities.TotalCargoWeight);
		}

		public void TestTotalPieceCount()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var container = moveDetail.Containers.AddNew();
			var commodity1 = container.Commodities.AddNew();
			commodity1.BY_PieceCount = 10;
			AssertEquals(10, container.Commodities.TotalPieceCount);
			var commodity2 = container.Commodities.AddNew();
			commodity2.BY_PieceCount = 35;
			AssertEquals(45, container.Commodities.TotalPieceCount);
		}

		protected override CusInBondCargoDescCollection GetCollectionToTest()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var container = moveDetail.Containers.AddNew();
			return new CusInBondCargoDescCollection(container);
		}
	}
}
