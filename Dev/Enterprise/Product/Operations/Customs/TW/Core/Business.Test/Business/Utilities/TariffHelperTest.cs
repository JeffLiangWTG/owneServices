using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TariffHelper))]
	sealed class TariffHelperTest : TestCaseWithFactory
	{
		public void TestGetValidRefCusTariffSortedDictionary()
		{
			SetUpTariff();
			var tariffView = new TariffView.Loader(Factory).LoadMostRecentCachedTariff("TW", "HSN", "87031000002", new ZDateTime(2024, 5, 30));
			var dictionary = TariffHelper.GetValidRefCusTariffSortedDictionary(Factory, tariffView, tariffView.ZZ1_TariffCode, new ZDateTime(2024, 5, 30));
			var ssTariffs = dictionary.FirstOrDefault(c => c.Key.ZZI_TariffType == "SS").Value;
			AssertContainsExactElementsInAnyOrder(new string[] { "PASSENGERCAR", "SEDAN" }, ssTariffs.Select(t => t.ZZ1_TariffCode));
			var ctTariffs = dictionary.FirstOrDefault(c => c.Key.ZZI_TariffType == "CT").Value;
			AssertContainsExactElementsInAnyOrder(new string[] { "TESTSEDAN" }, ctTariffs.Select(t => t.ZZ1_TariffCode));
		}

		[ExpectNoExceptions]
		public void TestHasTariffCustomsRequirementsAttribute()
		{
			SetUpTariff();
			var tariffView = new TariffView.Loader(Factory).LoadMostRecentCachedTariff("TW", "HSN", ZString.Empty, new ZDateTime(2024, 5, 30));
			NUnit.Framework.Assert.That(tariffView.HasTariffCustomsRequirementsAttribute("T"), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
			tariffView = new TariffView.Loader(Factory).LoadMostRecentCachedTariff("TW", "HSN", "2713200000", new ZDateTime(2024, 5, 30));
			NUnit.Framework.Assert.That(tariffView.HasTariffCustomsRequirementsAttribute("T"), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
			tariffView = new TariffView.Loader(Factory).LoadMostRecentCachedTariff("TW", "HSN", "87031000002", new ZDateTime(2024, 5, 30));
			NUnit.Framework.Assert.That(tariffView.HasTariffCustomsRequirementsAttribute("T"), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		void SetUpTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			var ssTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "SS");
			var ctTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "CT");
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "87031000002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "L*", tariff);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "T", tariff);
			Factory.Save();

			helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "2713200000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var passengerCarChildtariff = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, ssTariffType.PK, "PASSENGERCAR", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTariffRelationship(passengerCarChildtariff.PK, hsnTariffType.PK, "87031000002");
			var sedanChildTariff = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, ssTariffType.PK, "SEDAN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTariffRelationship(sedanChildTariff.PK, hsnTariffType.PK, "87031000002");
			var testSedanChildTariff = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, ctTariffType.PK, "TESTSEDAN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTariffRelationship(testSedanChildTariff.PK, hsnTariffType.PK, "87031000002");
		}
	}
}
