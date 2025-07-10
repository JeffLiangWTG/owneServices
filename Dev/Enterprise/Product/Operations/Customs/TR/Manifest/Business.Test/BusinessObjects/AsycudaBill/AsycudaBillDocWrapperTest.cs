using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	sealed class AsycudaBillDocWrapperTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestWrapperAtBillLevel()
		{
			SetData();
			var billWrapper = new AsycudaBillDocWrapper(header.Bills[0]);

			CombineAssertions(() =>
			{
				AssertEquals(11, billWrapper.TotalBillPackQuantity);
				AssertEquals("20.60", billWrapper.TotalBillPackedItemsGrossWeight);
				AssertEquals("Blue Car", billWrapper.BillPackGoodsDescription);
			});
		}

		public void TestShortGoodsLocationDescription()
		{
			SetData();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey, "Turkey", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes, "TR Warehouses");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes, "WAR1", "Warehouse1", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes, "WAR2", "Warehouse2Long", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes, "WAR3", "War3Short", yesterday, tomorrow);
			Factory.Save();

			var billWrapper = new AsycudaBillDocWrapper(header.Bills[0]);

			CombineAssertions(() =>
			{
				AssertEquals("ABL_GoodsLocation is empty", ZString.Empty, billWrapper.ShortGoodsLocationDescription);

				header.Bills[0].ABL_GoodsLocation = "WAR1";
				AssertEquals("WAR1 description is in 10 characters", "Warehouse1", billWrapper.ShortGoodsLocationDescription);

				header.Bills[0].ABL_GoodsLocation = "WAR2";
				AssertEquals("WAR2 description is longer", "Warehouse2", billWrapper.ShortGoodsLocationDescription);

				header.Bills[0].ABL_GoodsLocation = "WAR3";
				AssertEquals("WAR3 description is shorther", "War3Short", billWrapper.ShortGoodsLocationDescription);

				header.Bills[0].ABL_GoodsLocation = "WAR4";
				AssertEquals("WAR4 isn't in list", ZString.Empty, billWrapper.ShortGoodsLocationDescription);
			});
		}

		void SetData()
		{
			if (header == null)
			{
				var testWeight = new ZDecimal(5.15);
				header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
				company.CompanyName = "TestName";
				company.Address1 = "Test adress 1";
				company.Address2 = "Test adress 2";
				company.Branches.Add(branch);

				var bill = header.Bills.AddNew();
				var pack1 = bill.Packs.AddNew();
				pack1.APA_PackQty = 6;
				pack1.APA_PackUQ = "BAG";
				pack1.APA_GoodsDescription = "Red Car";
				var item1 = pack1.PackedItems.AddNewPackedItem();
				var item2 = pack1.PackedItems.AddNewPackedItem();
				item1.API_GrossWeight = testWeight;
				item1.API_GrossWeightUQ = "KG";
				item1.API_GoodsDescription = "Blue Car";
				item2.API_GrossWeight = testWeight;
				item2.API_GrossWeightUQ = "KG";

				var pack2 = bill.Packs.AddNew();
				pack2.APA_PackQty = 5;
				pack2.APA_PackUQ = "BAG";
				pack2.APA_GoodsDescription = "Yellow Car";
				item1 = pack2.PackedItems.AddNewPackedItem();
				item2 = pack2.PackedItems.AddNewPackedItem();
				item1.API_GrossWeight = testWeight;
				item1.API_GrossWeightUQ = "KG";
				item1.API_GoodsDescription = "Green Car";
				item2.API_GrossWeight = testWeight;
				item2.API_GrossWeightUQ = "KG";
			}
		}
	}
}
