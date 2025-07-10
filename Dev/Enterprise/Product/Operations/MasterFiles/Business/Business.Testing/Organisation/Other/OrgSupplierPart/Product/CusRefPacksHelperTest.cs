using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CusRefPacksHelperTest : TestCaseWithFactory
	{
		public void TestLoadRefPack()
		{
			var pack1 = Factory.New<CusRefPacks>();
			pack1.RP_CustomsPack = "XX";
			pack1.RP_Type = "";
			pack1.RP_CustomsCountry = "US";
			pack1.RP_ConversionFactor = 1;
			pack1.RP_CommercialPack = "XXX";

			var pack2 = Factory.New<CusRefPacks>();
			pack2.RP_CustomsPack = "XX";
			pack2.RP_Type = "CIP";
			pack2.RP_CustomsCountry = "US";
			pack2.RP_ConversionFactor = 1;
			pack2.RP_CommercialPack = "XXX";
			Factory.Save();

			var loadedPack = CusRefPacksHelper.LoadRefPack(Factory, "XXX", "CIP", "US");
			AssertNotNull(loadedPack);
			AssertEquals("CIP", loadedPack.RP_Type);

			var pack3 = Factory.New<CusRefPacks>();
			pack3.RP_CustomsPack = "YY";
			pack3.RP_Type = "";
			pack3.RP_CustomsCountry = "US";
			pack3.RP_ConversionFactor = 3;
			pack3.RP_CommercialPack = "XXX";
			Factory.Save();

			loadedPack = CusRefPacksHelper.LoadRefPack(Factory, "XXX", "CIP", "US");
			AssertNull(loadedPack);

			loadedPack = CusRefPacksHelper.LoadRefPack(Factory, "XXX", "CIP", "US", false);
			AssertNotNull(loadedPack);
			AssertEquals("CIP", loadedPack.RP_Type);

			loadedPack = CusRefPacksHelper.LoadRefPack(Factory, "XXX", "CIP", "US", false, "XX");
			AssertNotNull(loadedPack);
			AssertEquals("CIP", loadedPack.RP_Type);

			loadedPack = CusRefPacksHelper.LoadRefPack(Factory, "XXX", "CIP", "US", false, "YY");
			AssertNull(loadedPack);
		}

		public void TestLoadFilteredRefPacks()
		{
			var dbPacks = Factory.Load<CusRefPacks>(new ZQuery(RefPacksSchema.RP_CustomsCountry, "US"));
			dbPacks.DeleteAll();

			Factory.Save();

			var pack1 = Factory.New<CusRefPacks>();
			pack1.RP_CustomsPack = "XX";
			pack1.RP_Type = "CIP";
			pack1.RP_CustomsCountry = "US";
			pack1.RP_ConversionFactor = 1;
			pack1.RP_CommercialPack = "YYY";

			var pack2 = Factory.New<CusRefPacks>();
			pack2.RP_CustomsPack = "XX";
			pack2.RP_Type = "CIP";
			pack2.RP_CustomsCountry = "US";
			pack2.RP_ConversionFactor = 2;
			pack2.RP_CommercialPack = "XXX";

			var pack3 = Factory.New<CusRefPacks>();
			pack3.RP_CustomsPack = "XX";
			pack3.RP_Type = "CIP";
			pack3.RP_CustomsCountry = "US";
			pack3.RP_ConversionFactor = 3;
			pack3.RP_CommercialPack = "ZZZ";

			var pack4 = Factory.New<CusRefPacks>();
			pack4.RP_CustomsPack = "XX";
			pack4.RP_Type = "";
			pack4.RP_CustomsCountry = "US";
			pack4.RP_ConversionFactor = 1;
			pack4.RP_CommercialPack = "YYY";

			var pack5 = Factory.New<CusRefPacks>();
			pack5.RP_CustomsPack = "XX";
			pack5.RP_Type = "";
			pack5.RP_CustomsCountry = "US";
			pack5.RP_ConversionFactor = 5;
			pack5.RP_CommercialPack = "XXX";

			var pack6 = Factory.New<CusRefPacks>();
			pack6.RP_CustomsPack = "YY";
			pack6.RP_Type = "";
			pack6.RP_CustomsCountry = "US";
			pack6.RP_ConversionFactor = 6;
			pack6.RP_CommercialPack = "ZZZ";

			var pack7 = Factory.New<CusRefPacks>();
			pack7.RP_CustomsPack = "KK";
			pack7.RP_Type = "";
			pack7.RP_CustomsCountry = "US";
			pack7.RP_ConversionFactor = 1;
			pack7.RP_CommercialPack = "YYY";
			Factory.Save();

			var packs = CusRefPacksHelper.LoadFilteredRefPacks(Factory, "US", "CIP", "");
			AssertEquals(5, packs.Count);

			packs = CusRefPacksHelper.LoadFilteredRefPacks(Factory, "US", "", "YYY");
			AssertEquals(2, packs.Count);

			packs = CusRefPacksHelper.LoadFilteredRefPacks(Factory, "US", "CIP", "ZZZ");
			AssertEquals(2, packs.Count);

			packs = CusRefPacksHelper.LoadFilteredRefPacks(Factory, "US", "CIP", "", true, 1);
			AssertEquals(2, packs.Count);

			packs = CusRefPacksHelper.LoadFilteredRefPacks(Factory, "US", "CIP", "", true, 1, "XX");
			AssertEquals(1, packs.Count);

			packs = CusRefPacksHelper.LoadFilteredRefPacks(Factory, "US", "CIP", "", false);
			AssertEquals(3, packs.Count);

			packs = CusRefPacksHelper.LoadFilteredRefPacks(Factory, "US", "", "YYY", false);
			AssertEquals(2, packs.Count);

			packs = CusRefPacksHelper.LoadFilteredRefPacks(Factory, "US", "CIP", "ZZZ", false);
			AssertEquals(1, packs.Count);

			packs = CusRefPacksHelper.LoadFilteredRefPacks(Factory, "US", "CIP", "", false, 1);
			AssertEquals(1, packs.Count);
		}
	}
}
