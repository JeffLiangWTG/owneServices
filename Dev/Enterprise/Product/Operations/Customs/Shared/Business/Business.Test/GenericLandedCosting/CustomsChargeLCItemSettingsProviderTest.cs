using System.Linq;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CustomsChargeLCItemSettingsProviderTest : TestCase
	{
		public void TestGetCustomsChargeLCItemSettings_NullIsIntegrated()
		{
			CombineAssertions(() =>
			{
				var provider = new CustomsChargeLCItemSettingsProvider();
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.SouthAfrica, null, "TDT");
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.Canada, null, "TDT", "OTH", "EXC");
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.China, null, "TDT", "EXC");
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.NewZealand, null, "TDT", "ENT", "OTH", "ST1", "ST2", "ST3");
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.Australia, null, "TDT", "ENT", "OTH", "QUA", "ST1", "ST2", "ST3");
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.UnitedStates, null, "TDT", "EXC", "ENT", "OTH", "ST1", "ST2");
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.UnitedKingdom, null, "TDT", "OTH", "ST1", "ST2", "ST3");
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.France, null, "TDT");
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.Latvia, null, "TDT", "EXC", "ENT", "OTH", "ST1", "ST2", "ST3");
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.Singapore, null, "TDT", "EXC", "ENT", "OTH", "ST1", "ST2", "ST3");
			});
		}

		public void TestGetCustomsChargeLCItemSettings_IsIntegrated()
		{
			CombineAssertions(() =>
			{
				var provider = new CustomsChargeLCItemSettingsProvider();
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.SouthAfrica, true, "TDT");
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.Canada, true, "TDT");
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.China, true, "TDT");
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.NewZealand, true, "TDT");
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.Australia, true, "TDT");
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.UnitedStates, true, "TDT");
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.UnitedKingdom, true, "TDT");
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.France, true, "TDT");
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.Latvia, true, "TDT");
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.Singapore, true, "TDT");
			});
		}

		public void TestGetCustomsChargeLCItemSettings_NotIsIntegrated()
		{
			CombineAssertions(() =>
			{
				var provider = new CustomsChargeLCItemSettingsProvider();
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.SouthAfrica, false, "TDT");
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.Canada, false, "TDT", "OTH", "EXC");
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.China, false, "TDT", "EXC");
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.NewZealand, false, "TDT", "ENT", "OTH", "ST1", "ST2", "ST3");
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.Australia, false, "TDT", "ENT", "OTH", "QUA", "ST1", "ST2", "ST3");
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.UnitedStates, false, "TDT", "EXC", "ENT", "OTH", "ST1", "ST2");
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.UnitedKingdom, false, "TDT", "OTH", "ST1", "ST2", "ST3");
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.France, false, "TDT");
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.Latvia, false, "TDT", "EXC", "ENT", "OTH", "ST1", "ST2", "ST3");
				AssertCustomsChargeLCItemSettings(provider, Core.Constants.CountryCodes.Singapore, false, "TDT", "EXC", "ENT", "OTH", "ST1", "ST2", "ST3");
			});
		}

		void AssertCustomsChargeLCItemSettings(CustomsChargeLCItemSettingsProvider provider, string countryCode, bool? isIntegrated, params string[] costTypes)
		{
			var itemSettings = provider.GetCustomsChargeLCItemSettings(countryCode, isIntegrated, GlbCompany.CurrentCompany.PK);
			AssertContainsExactElementsInAnyOrder("CustomsChargeLCItemSettings for " + countryCode, costTypes, itemSettings.Select(x => x.CostType.ToString()));
		}
	}
}
