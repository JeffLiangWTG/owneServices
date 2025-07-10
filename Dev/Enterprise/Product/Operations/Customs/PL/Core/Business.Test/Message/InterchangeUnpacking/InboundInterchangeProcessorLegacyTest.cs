using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class InboundInterchangeProcessorLegacyTest : TestCaseWithFactory
{
	public void TestApplicationCodes() => AssertContainsExactElementsInAnyOrder(
		expected: new[] { EDIInterchange.ApplicationCodes.PLCustoms, EDIInterchange.ApplicationCodes.PLCustomsNCTS, EDIInterchange.ApplicationCodes.PLCustomsExitControl },
		actual: processor.GetSecurePrivateGrades());

	protected override void SetUp()
	{
		base.SetUp();
		processor = new InboundInterchangeProcessorLegacy();
	}
	InboundInterchangeProcessorLegacy processor;
}
