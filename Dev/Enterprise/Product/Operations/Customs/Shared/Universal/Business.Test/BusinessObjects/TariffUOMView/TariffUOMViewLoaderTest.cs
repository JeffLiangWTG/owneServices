using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;
using UHelper = Enterprise.Customs.Universal.Testing.UniversalReferenceTestDataHelper;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(TariffUOMView.Loader))]
	class TariffUOMViewLoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest() => new TariffUOMView.Loader(Factory);

		public void TestLoadTariffUom()
		{
			var helper = new UHelper(Factory);
			var s1p1TariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			Factory.Save();
			var cusTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", isSystem: false, versionCode: "001");
			var uom = helper.CreateTariffUOM(cusTariff, "CU1", "VWG");
			Factory.Save();
			var loader = new TariffUOMView.Loader(Factory);
			var uom1 = loader.LoadTariffUomView(cusTariff.PK, "CU1", false);
			AssertEquals(uom.PK, uom1.PK);
			AssertEquals(null, loader.LoadTariffUomView(cusTariff.PK, "CU1", true));
			AssertEquals(null, loader.LoadTariffUomView(cusTariff.PK, "CU2", false));
			AssertEquals(null, loader.LoadTariffUomView(ZGuid.NewZGuid(), "CU1", false));
			AssertEquals(null, loader.LoadTariffUomView(cusTariff.PK, "", true));
			AssertEquals(null, loader.LoadTariffUomView(ZGuid.Empty, "CU1", true));
		}
	}
}
