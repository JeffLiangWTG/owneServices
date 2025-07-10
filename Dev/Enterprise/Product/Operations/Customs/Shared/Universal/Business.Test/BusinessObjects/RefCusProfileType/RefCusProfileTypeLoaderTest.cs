using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusProfileType.Loader))]
	class RefCusProfileTypeLoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new RefCusProfileType.Loader(Factory);
		}

		public void TestLoad()
		{
			var today = ZDateTime.Today;
			Helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Brazil);
			Helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Angola);
			var tariffType1 = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, "HSN", "NCM");
			var tariffType2 = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Angola, "HSN", "NCM");
			Factory.Save();

			var tariff1 = Helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Brazil, tariffType1.PK, "12345678", today.AddDays(-5), today.AddDays(5));
			var tariff2 = Helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Brazil, tariffType1.PK, "87654321", today.AddDays(-5), today.AddDays(5));
			var tariff3 = Helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Angola, tariffType2.PK, "12345678", today.AddDays(-5), today.AddDays(5));
			var tariff4 = Helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Angola, tariffType2.PK, "87654321", today.AddDays(-5), today.AddDays(5));

			var profileType1 = Helper.CreateRefCusProfileType(tariffType1.PK, Core.Constants.CountryCodes.Brazil, "NCM", "ZA NCM");
			var profileType2 = Helper.CreateRefCusProfileType(tariffType1.PK, Core.Constants.CountryCodes.Brazil, "NVE", "ZA NVE");
			var profileType3 = Helper.CreateRefCusProfileType(tariffType2.PK, Core.Constants.CountryCodes.Angola, "NCM", "AO NCM");
			Factory.Save();

			var loader = new RefCusProfileType.Loader(Factory);
			AssertEquals(profileType1, loader.Load("NCM", tariffType1.PK, Core.Constants.CountryCodes.Brazil));
			AssertEquals(profileType2, loader.Load("NVE", tariffType1.PK, Core.Constants.CountryCodes.Brazil));
			AssertEquals(profileType3, loader.Load("NCM", tariffType2.PK, Core.Constants.CountryCodes.Angola));
			AssertContainsExactElementsInAnyOrder(new[] { profileType1, profileType2 }, loader.Load(new ZString[] { "NCM", "NVE" }, tariffType1.PK, Core.Constants.CountryCodes.Brazil));
			AssertContainsExactElementsInAnyOrder(new[] { profileType3 }, loader.Load(new ZString[] { "NCM", "NVE" }, tariffType2.PK, Core.Constants.CountryCodes.Angola));
		}

		public void TestLoadByTariffTypeAndDataGrouping()
		{
			var today = ZDateTime.Today;
			Helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Mexico);
			Helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Angola);
			var tariffType1 = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Mexico, "IDL");
			var tariffType2 = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Angola, "HSN", "NCM");
			Factory.Save();

			var profileType1 = Helper.CreateRefCusProfileType(tariffType1.PK, Core.Constants.CountryCodes.Mexico, "AI", "OPERACIONES DE COMERCIO EXTERIOR CON AMPARO");
			var profileType2 = Helper.CreateRefCusProfileType(tariffType1.PK, Core.Constants.CountryCodes.Mexico, "AC", "ALMACÉN GENERAL DE DEPÓSITO CERTIFICADO");
			var profileType3 = Helper.CreateRefCusProfileType(tariffType1.PK, Core.Constants.CountryCodes.Mexico, "B2", "BIENES DEL ARTÍCULO 2 DE LA LEY DEL IEPS.");
			var profileType4 = Helper.CreateRefCusProfileType(tariffType2.PK, Core.Constants.CountryCodes.Angola, "NCM", "AO NCM");
			Factory.Save();

			var loader = new RefCusProfileType.Loader(Factory);
			AssertEquals(3, loader.Load(tariffType1.ZZI_TariffType, Core.Constants.CountryCodes.Mexico).Length);
			AssertEquals(1, loader.Load(tariffType2.ZZI_TariffType, Core.Constants.CountryCodes.Angola).Length);
			AssertEquals(0, loader.Load(ZString.Empty, Core.Constants.CountryCodes.Mexico).Length);
			AssertEquals(0, loader.Load(tariffType1.ZZI_TariffType, ZString.Empty).Length);
			AssertContainsExactElementsInAnyOrder(new[] { profileType1, profileType2, profileType3 }, loader.Load(tariffType1.ZZI_TariffType, Core.Constants.CountryCodes.Mexico));
			AssertContainsExactElementsInAnyOrder(new[] { profileType4 }, loader.Load(tariffType2.ZZI_TariffType, Core.Constants.CountryCodes.Angola));
		}

		UniversalReferenceTestDataHelper Helper => helper ??= new UniversalReferenceTestDataHelper(Factory);
		UniversalReferenceTestDataHelper helper;
	}
}
