using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusProfile.Loader))]
	class RefCusProfileLoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new RefCusProfile.Loader(Factory);
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

			var profile1 = Helper.CreateRefCusProfile(profileType1, "12345678", "ATT1", today.AddDays(-5), today.AddDays(5), new[] { ("A1", "1"), ("A2", "2") });
			var profile2 = Helper.CreateRefCusProfile(profileType1, "123456", "ATT2", today.AddDays(-5), today.AddDays(5), new[] { ("A1", "1"), ("A2", "X") });
			var profile3 = Helper.CreateRefCusProfile(profileType1, "", "ATT3", today.AddDays(-2), today.AddDays(5), new[] { ("A1", "X"), ("A2", "2") });
			var profile4 = Helper.CreateRefCusProfile(profileType1, "12345678", "ATT4", today.AddDays(-5), today.AddDays(-2));
			var profile5 = Helper.CreateRefCusProfile(profileType1, "87654321", "ATT5", today.AddDays(-5), today.AddDays(5));
			var profile6 = Helper.CreateRefCusProfile(profileType2, "12345678", "ATT6", today.AddDays(-5), today.AddDays(5));
			var profile7 = Helper.CreateRefCusProfile(profileType3, "12345678", "ATT7", today.AddDays(-5), today.AddDays(5));
			Factory.Save();

			var loader = new RefCusProfile.Loader(Factory);
			AssertEquals(0, loader.Load(ZString.Empty, tariffType1.PK, "12345678", Core.Constants.CountryCodes.Brazil, today).Length);
			AssertEquals(0, loader.Load("NCM", ZGuid.Empty, "12345678", Core.Constants.CountryCodes.Brazil, today).Length);
			AssertEquals(0, loader.Load("NCM", tariffType1.PK, ZString.Empty, Core.Constants.CountryCodes.Brazil, today).Length);
			AssertEquals(0, loader.Load("NCM", tariffType1.PK, "12345678", ZString.Empty, today).Length);

			AssertContainsExactElementsInAnyOrder(new[] { profile1, profile2, profile3 }, loader.Load("NCM", tariffType1.PK, "12345678", Core.Constants.CountryCodes.Brazil, ZDate.Empty));
			AssertContainsExactElementsInAnyOrder(new[] { profile1, profile2, profile3 }, loader.Load("NCM", tariffType1.PK, "12345678", Core.Constants.CountryCodes.Brazil, today));
			AssertContainsExactElementsInAnyOrder(new[] { profile1, profile2, profile3, profile6 }, loader.Load(new ZString[] { "NCM", "NVE" }, tariffType1.PK, "12345678", Core.Constants.CountryCodes.Brazil, today));
			AssertContainsExactElementsInAnyOrder(new[] { profile1, profile2, profile4 }, loader.Load(new ZString[] { "NCM" }, tariffType1.PK, "12345678", Core.Constants.CountryCodes.Brazil, today.AddDays(-3)));
			AssertContainsExactElementsInAnyOrder(new[] { profile1 }, loader.Load(new ZString[] { "NCM" }, tariffType1.PK, "12345678", Core.Constants.CountryCodes.Brazil, today, new[] { ("A1", new[] { "1" }), ("A2", new[] { "2" }) }));
			AssertContainsExactElementsInAnyOrder(new[] { profile1, profile2 }, loader.Load(new ZString[] { "NCM" }, tariffType1.PK, "12345678", Core.Constants.CountryCodes.Brazil, today, new[] { ("A1", new[] { "1" }), ("A2", new[] { "2", "X" }) }));
			AssertContainsExactElementsInAnyOrder(new[] { profile1, profile2 }, loader.Load(new ZString[] { "NCM" }, tariffType1.PK, "12345678", Core.Constants.CountryCodes.Brazil, today, new[] { ("A1", new[] { "1" }) }));
			AssertContainsExactElementsInAnyOrder(new[] { profile1, profile3 }, loader.Load(new ZString[] { "NCM" }, tariffType1.PK, "12345678", Core.Constants.CountryCodes.Brazil, today, new[] { ("A2", new[] { "2" }) }));
		}

		UniversalReferenceTestDataHelper Helper => helper ??= new UniversalReferenceTestDataHelper(Factory);
		UniversalReferenceTestDataHelper helper;
	}
}
