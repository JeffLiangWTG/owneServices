using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Universal.Testing
{
	internal class ZZRefCusRulingCombinedValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZZX_RulingType()
		{
			var ruling = Factory.NewWithValidTestData<ZZRefCusRulingCombined>();
			ruling.ZZX_RulingType = "";
			AssertHasError(ruling.ZZX_RulingTypeInfo, "Please enter a Ruling Type.");
			ruling.ZZX_RulingType = "A";
			AssertHasError(ruling.ZZX_RulingTypeInfo, "Enter a valid Ruling Type.");
			ruling.ZZX_RulingType = RefCusRulingTypeList.Codes._2;
			AssertNoErrors(ruling.ZZX_RulingTypeInfo);
		}

		public void TestCheckZZX_RulingNumber()
		{
			var ruling = Factory.NewWithValidTestData<ZZRefCusRulingCombined>();
			ruling.ZZX_RulingNumber = "";
			AssertHasError(ruling.ZZX_RulingNumberInfo, "Please enter a Ruling Number.");
			ruling.ZZX_RulingNumber = "A";
			AssertNoErrors(ruling.ZZX_RulingNumberInfo);
		}

		public void TestCheckZZX_Description()
		{
			var ruling = Factory.NewWithValidTestData<ZZRefCusRulingCombined>();
			ruling.ZZX_Description = "";
			AssertHasError(ruling.ZZX_DescriptionInfo, "Please enter a value.");
			ruling.ZZX_Description = "A";
			AssertNoErrors(ruling.ZZX_DescriptionInfo);
		}

		public void TestCheckZZX_OA_AppliesTo()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusRuling = helper.CreateOrGetRefCusRuling(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "111111", RefCusRulingTypeList.Codes._2, ZDate.Today, ZDate.Today.AddYears(1));
			var cusRuling2 = (ZZRefCusRulingCombined)cusRuling.TemplateCopy();
			cusRuling2.Validation.ValidateZZX_OA_AppliesTo();
			AssertNoErrors(cusRuling2.ZZX_OA_AppliesToInfo);
			var cusRuling3 = (ZZRefCusRulingCombined)cusRuling.TemplateCopy();
			cusRuling3.Validation.ValidateZZX_OA_AppliesTo();
			AssertHasError(cusRuling3.ZZX_OA_AppliesToInfo, "Duplicate Ruling Number, Ruling Type, Applies To and Start Date is not allowed.");
			var address = Factory.NewWithValidTestData<OrgAddress>();
			cusRuling2.ZZX_OA_AppliesTo = address.PK;
			AssertNoErrors(cusRuling2.ZZX_OA_AppliesToInfo);
			cusRuling3.ZZX_RulingNumber = "";
			cusRuling3.ZZX_OA_AppliesTo = address.PK;
			AssertNoErrors(cusRuling3.ZZX_OA_AppliesToInfo);
			cusRuling3.ZZX_RulingNumber = cusRuling2.ZZX_RulingNumber;
			cusRuling3.ZZX_RulingType = "";
			cusRuling3.Validation.ValidateZZX_OA_AppliesTo();
			AssertNoErrors(cusRuling3.ZZX_OA_AppliesToInfo);
			cusRuling3.ZZX_RulingType = cusRuling2.ZZX_RulingType;
			cusRuling3.ZZX_StartDate = ZDate.Empty;
			cusRuling3.Validation.ValidateZZX_OA_AppliesTo();
			AssertNoErrors(cusRuling3.ZZX_OA_AppliesToInfo);
			cusRuling3.ZZX_StartDate = cusRuling2.ZZX_StartDate;
			cusRuling3.Validation.ValidateZZX_OA_AppliesTo();
			AssertHasError(cusRuling3.ZZX_OA_AppliesToInfo, "Duplicate Ruling Number, Ruling Type, Applies To and Start Date is not allowed.");
		}

		public void TestCheckZZX_StartDate()
		{
			var ruling = Factory.NewWithValidTestData<ZZRefCusRulingCombined>();
			ruling.Validation.ValidateZZX_StartDate();
			AssertNoErrors(ruling.ZZX_StartDateInfo);
			ruling.ZZX_StartDate = ZDate.Empty;
			AssertHasError(ruling.ZZX_StartDateInfo, "Please enter a value.");
			ruling.ZZX_StartDate = ZDateTime.MinSmallDateTimeValue.Date.AddDays(-1);
			AssertHasErrorContaining(ruling.ZZX_StartDateInfo, "the limit for this field.");
			ruling.ZZX_StartDate = ZDate.Today;
			AssertNoErrors(ruling.ZZX_StartDateInfo);
			ruling.ZZX_EndDate = ZDate.Today.AddDays(-1);
			AssertHasError(ruling.ZZX_StartDateInfo, "Start Date cannot be after End Date.");
		}

		public void TestCheckZZX_EndDate()
		{
			var ruling = Factory.NewWithValidTestData<ZZRefCusRulingCombined>();
			ruling.Validation.ValidateZZX_EndDate();
			AssertNoErrors(ruling.ZZX_EndDateInfo);
			ruling.ZZX_EndDate = ZDate.Empty;
			AssertHasError(ruling.ZZX_EndDateInfo, "Please enter a value.");
			ruling.ZZX_EndDate = ZDateTime.MaxSmallDateTimeValue.Date.AddDays(1);
			ruling.ZZX_StartDate = ZDate.Today;
			ruling.ZZX_EndDate = ZDate.Today.AddDays(-1);
			AssertHasError(ruling.ZZX_StartDateInfo, "Start Date cannot be after End Date.");
			ruling.ZZX_EndDate = ZDateTime.MaxSmallDateTimeValue.Date;
			AssertNoErrors(ruling.ZZX_StartDateInfo);
			AssertNoErrors(ruling.ZZX_EndDateInfo);
		}
	}
}
