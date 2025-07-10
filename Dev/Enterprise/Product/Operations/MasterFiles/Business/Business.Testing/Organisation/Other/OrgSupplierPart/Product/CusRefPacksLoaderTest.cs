using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Customs;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CusRefPacks.Loader))]
	sealed class CusRefPacksLoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new CusRefPacks.Loader(Factory);
		}

		public void TestHumanReadableName()
		{
			var refPacks = Factory.New<CusRefPacks>();
			refPacks.RP_CustomsPack = "EN";
			refPacks.RP_CommercialPack = "ENV";

			AssertEquals("Packs Conversion EN-ENV", refPacks.HumanReadableName);
		}

		public void TestAll()
		{
			var rp1 = Factory.New<CusRefPacks>();
			rp1.RP_CommercialPack = "SRC";
			rp1.RP_CustomsPack = "DST";
			rp1.RP_CustomsCountry = "ER";
			rp1.RP_Type = RPTypeList.Codes.DeclarationTotal;

			Factory.Save();

			var loader = (CusRefPacks.Loader)GetNewLoaderToTest();
			var result = loader.Load("SRC", "ER", RPTypeList.Codes.AllAreas, null);
			AssertNull(result);
			result = loader.Load("SRC", "ER", RPTypeList.Codes.DeclarationTotal, null);
			AssertEquals("DST", result.RP_CustomsPack);
			var rp2 = Factory.New<CusRefPacks>();
			rp2.RP_CommercialPack = "SRC";
			rp2.RP_CustomsPack = "TSD";
			rp2.RP_CustomsCountry = "ER";
			rp2.RP_Type = RPTypeList.Codes.DeclarationTotal;

			Factory.Save();

			result = loader.Load("SRC", "ER", RPTypeList.Codes.DeclarationTotal, new[] { "XXX", "YYY", "DST" });
			AssertEquals("DST", result.RP_CustomsPack);
			result = loader.Load("SRC", "ER", RPTypeList.Codes.DeclarationTotal, new[] { "XXX", "YYY", "TSD" });
			AssertEquals("TSD", result.RP_CustomsPack);
			result = loader.Load("SRC", "ER", RPTypeList.Codes.DeclarationTotal, new[] { "XXX", "YYY", "ZZZ" });
			AssertEquals(null, result);
		}
	}
}
