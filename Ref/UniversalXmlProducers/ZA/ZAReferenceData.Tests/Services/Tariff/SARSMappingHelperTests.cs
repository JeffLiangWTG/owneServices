using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Helpers;
using NUnit.Framework;
using static CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Constants;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff
{
	[TestFixture]
	public class SARSMappingHelperTests
	{
		[TestCase("Standard 1P1", Schedules.S1P1, RateTypes.Standard, Preferences.None)]
		[TestCase("Standard Non-1P1", "XXX", RateTypes.Standard, null)]
		[TestCase("SADC", Schedules.S1P1, RateTypes.SADC, Preferences.PreferentialRate)]
		[TestCase("EFTA", Schedules.S1P1, RateTypes.EFTA, Preferences.PreferentialRate)]
		[TestCase("EU", Schedules.S1P1, RateTypes.EU, Preferences.PreferentialRate)]
		[TestCase("MERCORSUR", Schedules.S1P1, RateTypes.MERCOSUR, Preferences.PreferentialRate)]
		[TestCase("AFCFTA", Schedules.S1P1, RateTypes.AFCFTA, Preferences.PreferentialRate)]
		[TestCase("EUQUOTA - From Rules", Schedules.S1P1, RateTypes.EUQuota, Preferences.PreferentialQuota)]
		[TestCase("EFTAQUOTA - From Rules", Schedules.S1P1, RateTypes.EFTAQuota, Preferences.PreferentialQuota)]
		public void GetPreferenceFromScheduleAndRate(string testName, string scheduleType, string rateType, string expectedPreference)
		{
			var preference = SARSMappingHelper.GetPreferenceFromScheduleAndRate(scheduleType, rateType);
			Assert.That(preference, Is.EqualTo(expectedPreference), testName);
		}

		[TestCase(RateTypes.Standard, TradeGroups.Standard)]
		[TestCase(RateTypes.AFCFTA, TradeGroups.AFCFTA)]
		[TestCase(RateTypes.EFTA, TradeGroups.EFTA)]
		[TestCase(RateTypes.EU, TradeGroups.EU)]
		[TestCase(RateTypes.MERCOSUR, TradeGroups.MERCOSUR)]
		[TestCase(RateTypes.SADC, TradeGroups.SADC)]
		[TestCase(RateTypes.EUQuota, TradeGroups.EUQuota)]
		[TestCase(RateTypes.EFTAQuota, TradeGroups.EFTAQuota)]
		[TestCase("XXX", "XXX")]
		public void TradeGroupsFromRateType(string rateType, string expected)
		{
			var tradeGroup = SARSMappingHelper.GetTradeGroupFromRateType(rateType);
			Assert.That(tradeGroup, Is.EqualTo(expected));
		}

		[TestCase(TradeGroups.Standard, RateTypes.Standard)]
		[TestCase(TradeGroups.AFCFTA, RateTypes.AFCFTA)]
		[TestCase(TradeGroups.EFTA, RateTypes.EFTA)]
		[TestCase(TradeGroups.EU, RateTypes.EU)]
		[TestCase(TradeGroups.MERCOSUR, RateTypes.MERCOSUR)]
		[TestCase(TradeGroups.SADC, RateTypes.SADC)]
		[TestCase(TradeGroups.EUQuota, RateTypes.EUQuota)]
		[TestCase(TradeGroups.EFTAQuota, RateTypes.EFTAQuota)]
		[TestCase("UY", "")]
		public void RateTypeFromTradeGroup(string tradeGroup, string expected)
		{
			var rateType = SARSMappingHelper.GetRateTypeFromTradeGroup(tradeGroup);
			Assert.That(rateType, Is.EqualTo(expected));
		}

		[TestCase(null, "")]
		[TestCase("NO-MAPPING-EXISTS", "NO-MAPPING-EXISTS")]
		[TestCase("1000 KILOWATT HOUR", "MW")]
		[TestCase("1000 KW.H", "MW")]
		[TestCase("1000 U", "KU")]
		[TestCase("1000 UNITS", "KU")]
		[TestCase("10CIGARETTES", "NO")]
		[TestCase("10STICKS", "NO")]
		[TestCase("2U", "PR")]
		[TestCase("BAG", "NO")]
		[TestCase("BG", "NO")]
		[TestCase("CARAT", "CT")]
		[TestCase("CIGARETTES", "NO")]
		[TestCase("CIGARS", "NO")]
		[TestCase("CM", "CM")]
		[TestCase("CT", "CT")]
		[TestCase("CUBIC METRE", "MC")]
		[TestCase("G/KM", "GK")]
		[TestCase("G/M²", "SM")]
		[TestCase("GJ", "GJ")]
		[TestCase("GK", "GK")]
		[TestCase("GRAM OF THE SUGAR CONTENT THAT EXCEEDS /100ML", "GJ")]
		[TestCase("KG NET", "KN")]
		[TestCase("KG", "KG")]
		[TestCase("KILOGRAM", "KG")]
		[TestCase("KILOWATT HOUR", "KW")]
		[TestCase("KK", "KK")]
		[TestCase("KN", "KN")]
		[TestCase("KU", "KU")]
		[TestCase("KW", "KW")]
		[TestCase("KW.H", "KW")]
		[TestCase("KWH", "KW")]
		[TestCase("LA", "LA")]
		[TestCase("LAMP", "NO")]
		[TestCase("LI AA", "LA")]
		[TestCase("LI", "LI")]
		[TestCase("LITRE", "LI")]
		[TestCase("M", "ME")]
		[TestCase("M?", "SM")]
		[TestCase("M²", "SM")]
		[TestCase("M3 AT A PRESSURE OF 101,3 KPA AT 15C", "MC")]
		[TestCase("M³", "MC")]
		[TestCase("M³/101.3KP", "MC")]
		[TestCase("MC", "MC")]
		[TestCase("ME", "ME")]
		[TestCase("METRE", "ME")]
		[TestCase("MM", "MM")]
		[TestCase("MW", "MW")]
		[TestCase("NO", "NO")]
		[TestCase("PACKS", "NO")]
		[TestCase("PR", "PR")]
		[TestCase("SM", "SM")]
		[TestCase("SQUARE METRE", "SM")]
		[TestCase("TON", "KK")]
		[TestCase("TWO UNITS", "PR")]
		[TestCase("U (JUE/PACK)", "NO")]
		[TestCase("U", "NO")]
		[TestCase("UNIT", "NO")]
		public void UnitsOfMeasure(string sarsUOM, string expectedUOM)
		{
			Assert.That(SARSMappingHelper.GetUnitOfMeasure(sarsUOM), Is.EqualTo(expectedUOM));
		}
	}
}
