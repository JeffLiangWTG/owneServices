using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class UniversalReferenceDataHelperTest : TestCaseWithFactory
	{
		public void TestGetTaxTypeList()
		{
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var today = ZDateTime.Today;
				refDataHelper.CreateTaxOrFee("ABS", 0, Core.Constants.CountryCodes.Turkey, 0, 0, "OTH", today.AddDays(-1), today.AddDays(1), "ZERO");
				refDataHelper.CreateTaxOrFee("GMS", 0.01, Core.Constants.CountryCodes.Turkey, 0, 0, "OTH", today.AddDays(-1), today.AddDays(1), "1");
				refDataHelper.CreateTaxOrFee("KD8", 0.08, Core.Constants.CountryCodes.Turkey, 0, 0, "VAT", today.AddDays(-1), today.AddDays(1), "8");
				refDataHelper.CreateTaxOrFee("KD18", 0.18, Core.Constants.CountryCodes.Turkey, 0, 0, "VAT", today.AddDays(-1), today.AddDays(1), "18");
				Factory.Save();

				var statementLine = Factory.New<CusStatementLine>();
				statementLine.B3_EntryType = StatementLineEntryTypeList.Codes.MAN;

				var charge = Factory.New<CusStatementLineCharge>();
				statementLine.Charges.Add(charge);

				var cusStatementLineChargeLookups = charge.Lookups;

				AssertEquals("ABS, GMS", cusStatementLineChargeLookups.TaxOrFeeCodeList.CodesAsString);
			}
		}

		public void TestCheckRefCusCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			helper.CreateCusCodeType("TRCUQ", "TurkeyCustomsQuestion", Core.Constants.CountryCodes.Turkey);
			helper.CreateCusCodeType("TRCUW", "TurkeyCustomsWarning", Core.Constants.CountryCodes.Turkey);
			Factory.Save();

			CombineAssertions(() =>
			{
				var question = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "111", Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyCustomsQuestion, ZDate.Today);
				AssertNull("Quesion not found", question);

				var warning = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "222", Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyCustomsWarning, ZDate.Today);
				AssertNull("Warning not found", warning);

				UniversalReferenceDataHelper.GatherRefCusCodeList(Factory, "111", "Question 111", "Q");
				Factory.Save();
				var factory2 = new BusinessObjectFactory();
				question = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory2, "111", Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyCustomsQuestion, ZDate.Today);
				AssertNotNull("Quesion found", question);
				AssertEquals("ZZD_CodeType", "TRCUQ", question.ZZD_CodeType);
				AssertEquals("ZZD_Code", "111", question.ZZD_Code);
				AssertEquals("ZZD_Description", "Question 111", question.ZZD_Description);
				AssertEquals("ZZD_IsSystem", false, question.ZZD_IsSystem);

				UniversalReferenceDataHelper.GatherRefCusCodeList(Factory, "222", "Warning 222", "W");
				Factory.Save();
				var factory3 = new BusinessObjectFactory();
				warning = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory3, "222", Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyCustomsWarning, ZDate.Today);
				AssertNotNull("Warning found", warning);
				AssertEquals("ZZD_CodeType", "TRCUW", warning.ZZD_CodeType);
				AssertEquals("ZZD_Code", "222", warning.ZZD_Code);
				AssertEquals("ZZD_Description", "Warning 222", warning.ZZD_Description);
				AssertEquals("ZZD_IsSystem", false, warning.ZZD_IsSystem);
			});
		}

		public void TestGetErrorDescByCode()
		{
			var errorCode = "000";
			var errorDesc = "Success";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("TR", "Turkish");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingCusCodeType("NCTER", "NCTS Errors", Core.Constants.CountryCodes.Turkey);
			var addedCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, "NCTER", errorCode, errorDesc, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeListLanguage(addedCode, "TR", "Başarılı");
			Factory.Save();
			GlbStaff.CurrentUser.GS_WorkingLanguage = "EN";
			AssertEquals("English", "Success", UniversalReferenceDataHelper.GetErrorDescByCode(Factory, errorCode));

			GlbStaff.CurrentUser.GS_WorkingLanguage = "TR";
			AssertEquals("Turkish", "Başarılı", UniversalReferenceDataHelper.GetErrorDescByCode(Factory, errorCode));
		}
	}
}
