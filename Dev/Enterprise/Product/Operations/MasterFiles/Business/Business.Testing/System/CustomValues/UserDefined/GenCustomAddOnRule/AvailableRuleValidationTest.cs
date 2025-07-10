using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	sealed class AvailableRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckIsEnabled()
		{
			GenCustomAddOnRule addOnRule = Factory.New<GenCustomAddOnRule>();
			var dateTimeFormat = addOnRule.AllRules.Cast<AvailableRule>().First(rule => rule.Rule is DateTimeFormatRule);
			var invalidCode = addOnRule.AllRules.Cast<AvailableRule>().First(rule => rule.Rule is InvalidCodeRule);
			dateTimeFormat.IsEnabled = true;
			AssertNoErrors(dateTimeFormat.IsEnabledInfo);
			AssertNoErrors(invalidCode.IsEnabledInfo);
			invalidCode.IsEnabled = true;
			AssertHasError(dateTimeFormat.IsEnabledInfo, "'Code and description list' and 'Date time format' behaviors are mutually exclusive.");
			AssertHasError(invalidCode.IsEnabledInfo, "'Code and description list' and 'Date time format' behaviors are mutually exclusive.");
			dateTimeFormat.IsEnabled = false;
			AssertNoErrors(dateTimeFormat.IsEnabledInfo);
			AssertNoErrors(invalidCode.IsEnabledInfo);
		}
	}
}
