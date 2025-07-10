using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public class CustomsRuleValidationTest : CusPermitHeaderValidationTest
	{
		public void TestCheckStartAndEndDate()
		{
			var todaty = ZDate.Today;
			var customsRule = Factory.New<CustomsRule>();
			customsRule.CPH_StartDate = todaty;
			customsRule.CPH_EndDate = todaty.AddDays(-1);
			var validation = customsRule.Validation as CustomsRuleValidation;
			validation.ValidateAll();
			AssertHasError(customsRule.CPH_StartDateInfo, validation.StartDateEarlierThanOrEqualToEndDate);
			AssertHasError(customsRule.CPH_EndDateInfo, validation.StartDateEarlierThanOrEqualToEndDate);

			customsRule.CPH_EndDate = todaty;
			validation.ValidateAll();
			AssertNoError(customsRule.CPH_StartDateInfo, validation.StartDateEarlierThanOrEqualToEndDate);
			AssertNoError(customsRule.CPH_EndDateInfo, validation.StartDateEarlierThanOrEqualToEndDate);
		}

		public void TestExistsCustomsRuleUniqueIndex()
		{
			var permitHolder = Factory.New<OrgHeader>();
			permitHolder.OH_Code = "TST001";

			var startDate = ZDateTime.Today.Date;
			var customsRule1 = Factory.New<CustomsRule>();
			customsRule1.CPH_StartDate = startDate;
			Factory.Save();

			var errorText = ((CustomsRuleValidation)customsRule1.Validation).CustomsRuleUniqueIndexExists;
			var customsRule2 = Factory.New<CustomsRule>();
			customsRule2.CPH_StartDate = startDate;
			AssertHasError(customsRule2.CPH_StartDateInfo, errorText);
			AssertHasError(customsRule2.CPH_OH_PermitHolderInfo, errorText);

			customsRule2.CPH_OH_PermitHolder = permitHolder.PK;
			AssertNoError(customsRule2.CPH_StartDateInfo, errorText);
			AssertNoError(customsRule2.CPH_OH_PermitHolderInfo, errorText);

			customsRule2.CPH_StartDate = startDate.AddDays(1);
			AssertNoError(customsRule2.CPH_StartDateInfo, errorText);
			AssertNoError(customsRule2.CPH_OH_PermitHolderInfo, errorText);

			customsRule2.CPH_OH_PermitHolder = ZGuid.Empty;
			AssertNoError(customsRule2.CPH_StartDateInfo, errorText);
			AssertNoError(customsRule2.CPH_OH_PermitHolderInfo, errorText);

			customsRule2.CPH_StartDate = startDate;
			AssertHasError(customsRule2.CPH_StartDateInfo, errorText);
			AssertHasError(customsRule2.CPH_OH_PermitHolderInfo, errorText);

			customsRule1.Validation.ValidateCPH_StartDate();
			customsRule1.Validation.ValidateCPH_OH_PermitHolder();
			AssertNoError(customsRule1.CPH_StartDateInfo, errorText);
			AssertNoError(customsRule1.CPH_OH_PermitHolderInfo, errorText);
		}

		public void TestExistsCustomsRuleUniqueIndexBetweenTwoInstances()
		{
			var newFactory1 = new BusinessObjectFactory();
			var newFactory2 = new BusinessObjectFactory();

			var customsRule1 = newFactory1.New<CustomsRule>();
			var customsRule2 = newFactory2.New<CustomsRule>();

			var startDate = ZDateTime.Today.Date;
			var errorText = ((CustomsRuleValidation)customsRule1.Validation).CustomsRuleUniqueIndexExists;

			customsRule1.CPH_StartDate = startDate;
			customsRule2.CPH_StartDate = startDate;

			AssertNoError(customsRule1.CPH_StartDateInfo, errorText);
			AssertNoError(customsRule2.CPH_StartDateInfo, errorText);

			newFactory1.Save();

			customsRule2.Validation.ValidateCPH_StartDate();
			AssertHasError(customsRule2.CPH_StartDateInfo, errorText);
		}

		public void TestGetOverlappedCustomsRules()
		{
			var newFactory1 = new BusinessObjectFactory();
			var newFactory2 = new BusinessObjectFactory();

			var customsRule1 = newFactory1.New<CustomsRule>();
			var customsRule2 = newFactory2.New<CustomsRule>();

			var today = ZDateTime.Today.Date;
			var errorText = string.Format(((CustomsRuleValidation)customsRule1.Validation).OverlappedCustomsRules, customsRule1.HumanReadableName);

			customsRule1.CPH_StartDate = today;
			newFactory1.Save();

			customsRule2.CPH_StartDate = today;
			AssertHasErrorContaining(customsRule2.CPH_StartDateInfo, errorText);

			customsRule2.CPH_StartDate = today.AddDays(-10);
			AssertHasErrorContaining(customsRule2.CPH_StartDateInfo, errorText);

			customsRule2.CPH_EndDate = today;
			AssertHasErrorContaining(customsRule2.CPH_StartDateInfo, errorText);

			customsRule2.CPH_EndDate = today.AddDays(-1);
			AssertNoErrorContaining(customsRule2.CPH_StartDateInfo, errorText);

			customsRule2.CPH_EndDate = today.AddDays(10);
			AssertHasErrorContaining(customsRule2.CPH_StartDateInfo, errorText);

			customsRule1.CPH_EndDate = today.AddDays(5);
			newFactory1.Save();

			customsRule2.Validation.ValidateCPH_StartDate();
			AssertHasErrorContaining(customsRule2.CPH_StartDateInfo, errorText);

			customsRule2.CPH_EndDate = today.AddDays(2);
			AssertHasErrorContaining(customsRule2.CPH_StartDateInfo, errorText);

			customsRule2.CPH_EndDate = ZDate.Empty;
			AssertHasErrorContaining(customsRule2.CPH_StartDateInfo, errorText);

			customsRule2.CPH_StartDate = today.AddDays(1);
			AssertHasErrorContaining(customsRule2.CPH_StartDateInfo, errorText);

			customsRule2.CPH_EndDate = today.AddDays(2);
			AssertHasErrorContaining(customsRule2.CPH_StartDateInfo, errorText);

			customsRule2.CPH_EndDate = today.AddDays(10);
			AssertHasErrorContaining(customsRule2.CPH_StartDateInfo, errorText);

			customsRule2.CPH_StartDate = today.AddDays(6);
			AssertNoErrorContaining(customsRule2.CPH_StartDateInfo, errorText);

			customsRule2.CPH_EndDate = ZDate.Empty;
			AssertNoErrorContaining(customsRule2.CPH_StartDateInfo, errorText);

			customsRule1.CPH_EndDate = ZDate.Empty;
			newFactory1.Save();

			customsRule2.Validation.ValidateCPH_StartDate();
			AssertHasErrorContaining(customsRule2.CPH_StartDateInfo, errorText);

			customsRule2.CPH_EndDate = today.AddDays(10);
			AssertHasError(customsRule2.CPH_StartDateInfo, string.Format("Overlapping Customs Rules exist. Please adjust the start and end dates. Overlapping Customs Rules are:\nRule  - : start from {0} to ", today.ToISO8601ShortDateString()));
		}
	}
}
