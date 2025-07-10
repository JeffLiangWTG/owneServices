using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class LinkedCusAuthorisationRuleRangeTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<Exception>("RuleType Null", () => new LinkedCusAuthorisationRuleRange(null, 1, 1));
				AssertExceptionThrown<Exception>("RuleType Empty String", () => new LinkedCusAuthorisationRuleRange(string.Empty, 1, 1));
			});
		}
	}
}
