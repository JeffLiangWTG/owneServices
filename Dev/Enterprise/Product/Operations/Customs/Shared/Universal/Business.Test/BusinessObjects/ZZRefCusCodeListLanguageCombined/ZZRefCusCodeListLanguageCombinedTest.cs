using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(ZZRefCusCodeListLanguageCombined))]
	class ZZRefCusCodeListLanguageCombinedTest : EnterpriseBusinessObjectTestCase
	{
		public void TestReadOnly()
		{
			var codeList = Factory.New<ZZRefCusCodeListCombined>();
			var language = codeList.Languages.AddNew();
			codeList.ZZD_IsSystem = false;
			AssertEquals(false, language.ReadOnly);
			language.ReadOnly = true;
			AssertEquals(true, language.ReadOnly);
			codeList.ZZD_IsSystem = true;
			AssertEquals(true, language.ReadOnly);
			language.ReadOnly = false;
			AssertEquals(true, language.ReadOnly);
		}

		public void TestClone()
		{
			var codeList = Factory.New<ZZRefCusCodeListCombined>();
			var language = codeList.Languages.AddNew("DE", "DE Description");
			var clonedlanguage = (ZZRefCusCodeListLanguageCombined)language.Clone();
			CombineAssertions(() =>
			{
				AssertEquals("clonedlanguage.ZXA_ZZD_CodeList", codeList.PK, clonedlanguage.ZXA_ZZD_CodeList);
				AssertEquals("clonedlanguage.ZXA_ZX6_NKLanguage", "DE", clonedlanguage.ZXA_ZX6_NKLanguage);
				AssertEquals("clonedlanguage.ZXA_Description", "DE Description", clonedlanguage.ZXA_Description);
			});
		}

		public void TestICanDeleteMembers()
		{
			var codeList = Factory.New<ZZRefCusCodeListCombined>();
			var language = codeList.Languages.AddNew();
			codeList.ZZD_IsSystem = false;
			ICanDelete languageCanDelete = language;
			AssertEquals(true, languageCanDelete.CanDelete);
			codeList.ZZD_IsSystem = true;
			CombineAssertions(() =>
			{
				AssertEquals(false, languageCanDelete.CanDelete);
				AssertEquals(ZZRefCusCodeListCombined.CannotDeleteSystemGenerated, languageCanDelete.ReasonForNotAbleToDelete);
			});
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var cusCodeList = factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_CodeType = "SD";
			cusCodeList.ZZD_Code = "B0B";
			cusCodeList.ZZD_Description = "BOB THE BUILDER";
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Cambodia;
			cusCodeList.ZZD_StartDate = ZDateTime.Today;
			cusCodeList.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			return cusCodeList.Languages.AddNew("DE", "DE Desc.");
		}
	}
}
