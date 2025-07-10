using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	public class AsycudaBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPaymentType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var list = bill.Lookups.PaymentTypeList;
			AssertEquals(true, list.ContainsCode("A"));
			AssertEquals(true, list.ContainsCode("B"));
			AssertEquals(true, list.ContainsCode("C"));
			AssertEquals(true, list.ContainsCode("H"));
			AssertEquals(true, list.ContainsCode("Z"));
		}

		public void TestTransshipmentType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var list = bill.Lookups.TransshipmentTypeList;
			AssertEquals(true, list.ContainsCode("1"));
			AssertEquals(true, list.ContainsCode("2"));
			AssertEquals(true, list.ContainsCode("3"));
			AssertEquals(true, list.ContainsCode("4"));
		}

		public void TestTRLocations()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey, "Turkey", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes, "TR Warehouses");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes, "WAR1", "Warehouse1", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes, "WAR2", "Warehouse2", yesterday, tomorrow);
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var list = bill.Lookups.Locations as BusinessObjectCollection;
			list.Load();
			AssertEquals(2, list.Count);
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "WAR1"));
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "WAR2"));
		}

		public void TestYesNoList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var list = bill.Lookups.YesNoList;
			AssertEquals(2, list.Count);
			AssertEquals("N, Y", list.CodesAsString);
		}

		public void TestGoodsLocationList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey, "Turkey", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes, "TR Warehouses");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes, "WAR1", "Warehouse1", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes, "WAR2", "Warehouse2", yesterday, tomorrow);
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var list = bill.Lookups.GoodsLocationList;
			CombineAssertions(() =>
			{
				AssertEquals(2, list.Count);
				AssertEquals("WAR1 Description", "Warehouse1", list.GetDescriptionFromCode("WAR1"));
				AssertEquals("WAR2 Description", "Warehouse2", list.GetDescriptionFromCode("WAR2"));
			});
		}
	}
}
