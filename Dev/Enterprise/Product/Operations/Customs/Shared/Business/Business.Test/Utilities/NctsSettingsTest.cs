using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.Business.Testing
{
	class NctsSettingsTest : TestCaseWithFactory
	{
		public void TestIsNctsEnabled()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals($"IsNctsEnabled, country not in {Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EUCommonTransitProcedure}", expected: false, nctsSettings.IsNctsEnabled);
			}
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				AssertEquals($"IsNctsEnabled, country is in {Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EUCommonTransitProcedure}", expected: true, nctsSettings.IsNctsEnabled);
			}
		}

		public void TestIsUsingPhase5_Default()
		{
			AssertEquals(expected: true, nctsSettings.IsUsingPhase5(GlbCompany.CurrentCompany.Country.Code));
		}

		public void TestIsUsingPhase5_Phase4()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSPhase4, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: true))
			{
				AssertEquals(expected: false, nctsSettings.IsUsingPhase5(GlbCompany.CurrentCompany.Country.Code));
			}
		}

		public void TestIsUsingPhase5_Phase5Override()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSPhase4, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSPhase5Override, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: true))
			{
				AssertEquals(expected: true, nctsSettings.IsUsingPhase5(GlbCompany.CurrentCompany.Country.Code));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsSettings = new NctsSettings();
		}

		NctsSettings nctsSettings;
	}
}
