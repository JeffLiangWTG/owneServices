using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(UCC6ExportCusAuthorizationUsageValidationDecider))]
sealed class UCC6ExportCusAuthorizationUsageValidationDeciderTest : CusAuthorizationUsageValidationDeciderTestBase<UCC6ExportCusAuthorizationUsageValidationDecider>
{
	protected override bool ExpectedIsRuleR0010Active => false;

	protected override bool ExpectedIsRuleR0675Active => true;
}
