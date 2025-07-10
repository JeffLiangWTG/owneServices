using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal.Testing
{
	sealed class TranslationHelperTest : TestCaseWithFactory
	{
		public void TestGetCurrentLanguageCode()
		{
			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.ChineseSimplified;
			Factory.Save();
			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Chinese Simplified code", "ZHS", TranslationHelper.GetCurrentLanguageCode());
			}

			currentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.ChineseTraditional;
			Factory.Save();
			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Chinese Traditional code", "ZHT", TranslationHelper.GetCurrentLanguageCode());
			}

			currentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.French;
			Factory.Save();
			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("French code", "FR", TranslationHelper.GetCurrentLanguageCode());
			}

			currentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.Japanese;
			Factory.Save();
			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Japanese code", "JP", TranslationHelper.GetCurrentLanguageCode());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			using (Env.SetTemporaryUserContext(string.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Null user reverts to company country", "DE", TranslationHelper.GetCurrentLanguageCode());
			}
		}

		public void TestGetCurrentCountryLanguageCode()
		{
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.Spain, "ES");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.Germany, "DE");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.France, "FR");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.China, "ZHS");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.Taiwan, "ZHT");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.Italy, "IT");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.Turkey, "TR");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.Netherlands, "NL");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.Belgium, "NL");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.Denmark, "DA");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.Estonia, "ET");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.Finland, "FI");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.CzechRepublic, "CS");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.Sweden, "SV");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.Hungary, "HU");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.Latvia, "LV");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.Greece, "EL");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.Slovenia, "SL");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.Lithuania, "LT");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.Romania, "RO");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.Bulgaria, "BG");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.Croatia, "HR");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.Poland, "PL");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.Slovakia, "SK");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.Portugal, "PT");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.Brazil, "PT");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.KoreaSouth, "KO");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.Japan, "JP");
			AssertCurrentCountryLanguageCode(Core.Constants.CountryCodes.Israel, "HE");
		}

		public void TestGetTranslatedValue()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("IT", "Italian");
			helper.CreateOrGetLanguage("DE", "German");
			const string dataGrouping = Core.Constants.CountryCodes.China;
			var codeType = helper.CreateCusCodeType("TP1", "Ref Code Type 1", dataGrouping);
			helper.CreateNewOrGetExistingDataGrouping(dataGrouping);
			var attributeName = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT1", "Attribute Name 1", "TP1", dataGrouping, isValueMandatory: true);
			var attributeNameLanguage1 = helper.CreateCusCodeListAttributeNameLanguage(attributeName, "IT", "IT ATT1", "IT Ref Code Type 1 Description", "IT Caption ATT1");
			var attributeNameLanguage2 = helper.CreateCusCodeListAttributeNameLanguage(attributeName, "DE", "DE ATT1", "DE Ref Code Type 1 Description", ZString.Empty);
			Factory.Save();
			Factory.ClearCachedValue<ZString>($"RefCusCodeListAttributeName_{attributeName.PK}_ZXH_ColumnCaption_IT");
			Factory.ClearCachedValue<ZString>($"RefCusCodeListAttributeName_{attributeName.PK}_ZXH_ColumnCaption_DE");
			Factory.ClearCachedValue<ZString>($"RefCusCodeListAttributeName_{attributeName.PK}_ZXH_ColumnCaption_CH");

			CombineAssertions(() =>
			{
				AssertEquals("IT", "IT Caption ATT1", TranslationHelper.GetTranslatedValue(attributeName, "HELLO", RefCusCodeListAttributeNameLanguageSchema.ZXH_ColumnCaption, "IT"));
				AssertEquals("DE", "HELLO", TranslationHelper.GetTranslatedValue(attributeName, "HELLO", RefCusCodeListAttributeNameLanguageSchema.ZXH_ColumnCaption, "DE"));
				AssertEquals("CH", "HELLO", TranslationHelper.GetTranslatedValue(attributeName, "HELLO", RefCusCodeListAttributeNameLanguageSchema.ZXH_ColumnCaption, "CH"));
			});
		}

		public void TestGetAlternateLanguageDescriptionPreferredLanguage()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var s1p1TariffType = helper.CreateTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1", nomenclatureGroupType: "ZA", ensureDataGroupingExists: false);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF01", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			helper.CreateOrGetLanguage("IT", "Italian");
			helper.CreateOrGetLanguage("DE", "German");
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("No IT description", "Not Available", TranslationHelper.GetAlternateLanguageDescription(tariff, CusRefTariffLanguageViewSchema.ZX7_Description, "IT"));
				AssertEquals("No DE description", "Not Available", TranslationHelper.GetAlternateLanguageDescription(tariff, CusRefTariffLanguageViewSchema.ZX7_Description, "DE"));

				helper.LoadOrCreateNewCusRefTariffLanguageView(Factory, tariff.PK, "IT", "Test Description IT");
				Factory.Save();
				ClearFullTariffDescriptionCache(tariff);

				AssertEquals("IT Description Added", "Test Description IT", TranslationHelper.GetAlternateLanguageDescription(tariff, CusRefTariffLanguageViewSchema.ZX7_Description, "IT"));
				AssertEquals("DE Description record missing", "Not Available", TranslationHelper.GetAlternateLanguageDescription(tariff, CusRefTariffLanguageViewSchema.ZX7_Description, "DE"));

				helper.LoadOrCreateNewCusRefTariffLanguageView(Factory, tariff.PK, "DE", "Test Description DE");
				Factory.Save();
				ClearFullTariffDescriptionCache(tariff);

				AssertEquals("IT Description still exits", "Test Description IT", TranslationHelper.GetAlternateLanguageDescription(tariff, CusRefTariffLanguageViewSchema.ZX7_Description, "IT"));
				AssertEquals("DE Description Added", "Test Description DE", TranslationHelper.GetAlternateLanguageDescription(tariff, CusRefTariffLanguageViewSchema.ZX7_Description, "DE"));
			});
		}

		public void TestGetAlternateLanguageDescriptionPreferredLanguage_UseCountryLanguage()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var s1p1TariffType = helper.CreateTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1", nomenclatureGroupType: "ZA", ensureDataGroupingExists: false);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF01", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			helper.CreateOrGetLanguage("DE", "German");
			Factory.Save();
			helper.LoadOrCreateNewCusRefTariffLanguageView(Factory, tariff.PK, "DE", "Test Description DE");
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("No description for test company country ER", "Not Available", TranslationHelper.GetAlternateLanguageDescription(tariff, CusRefTariffLanguageViewSchema.ZX7_Description, ZString.Empty, true));

				ClearFullTariffDescriptionCache(tariff);
				var branch = CreateCompanyWithBranch("DE");
				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					AssertEquals("Test Company changed to DE", "Test Description DE", TranslationHelper.GetAlternateLanguageDescription(tariff, CusRefTariffLanguageViewSchema.ZX7_Description, ZString.Empty, true));
				}
			});
		}

		void ClearFullTariffDescriptionCache(TariffView tariff)
		{
			var tariffDescriptionCacheKey = "TariffView_{0}_ZX7_Description_{1}_{2}_Alternate";
			var alternateLanguageCacheKey = "TariffViewAlternateLanguage_{0}_{1}_{2}";

			tariff.Factory.ClearCachedValue<ZString>(ZString.Format(tariffDescriptionCacheKey, tariff.PK, "IT", false));
			tariff.Factory.ClearCachedValue<BusinessObject>(ZString.Format(alternateLanguageCacheKey, tariff.PK, "IT", false));
			tariff.Factory.ClearCachedValue<ZString>(ZString.Format(tariffDescriptionCacheKey, tariff.PK, "IT", true));
			tariff.Factory.ClearCachedValue<BusinessObject>(ZString.Format(alternateLanguageCacheKey, tariff.PK, "IT", true));

			tariff.Factory.ClearCachedValue<ZString>(ZString.Format(tariffDescriptionCacheKey, tariff.PK, "DE", false));
			tariff.Factory.ClearCachedValue<BusinessObject>(ZString.Format(alternateLanguageCacheKey, tariff.PK, "DE", false));
			tariff.Factory.ClearCachedValue<ZString>(ZString.Format(tariffDescriptionCacheKey, tariff.PK, "DE", true));
			tariff.Factory.ClearCachedValue<BusinessObject>(ZString.Format(alternateLanguageCacheKey, tariff.PK, "DE", true));

			tariff.Factory.ClearCachedValue<ZString>(ZString.Format(tariffDescriptionCacheKey, tariff.PK, ZString.Empty, false));
			tariff.Factory.ClearCachedValue<BusinessObject>(ZString.Format(alternateLanguageCacheKey, tariff.PK, ZString.Empty, false));
			tariff.Factory.ClearCachedValue<ZString>(ZString.Format(tariffDescriptionCacheKey, tariff.PK, ZString.Empty, true));
			tariff.Factory.ClearCachedValue<BusinessObject>(ZString.Format(alternateLanguageCacheKey, tariff.PK, ZString.Empty, true));
		}

		void AssertCurrentCountryLanguageCode(string countryCode, string expectedCode)
		{
			var branch = CreateCompanyWithBranch(countryCode);
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("Country Language Code ", expectedCode, TranslationHelper.GetCurrentCountryLanguageCode());
			}
		}

		GlbBranch CreateCompanyWithBranch(ZString companyCountryCode)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = companyCountryCode;
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			Factory.Save();
			return branch;
		}
	}
}
