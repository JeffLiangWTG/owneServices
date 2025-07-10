using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(OrgCusCode.EuropeanUnionSharedCodeTypes))]
sealed class OrgCusCodeEUCodeTypesTest : TestCaseWithFactory
{
	public void TestDefermentApprovalNumber()
	{
		AssertEquals("Ensure EU constant used matches the Norwegian code", "DAN", OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber);
	}
}
