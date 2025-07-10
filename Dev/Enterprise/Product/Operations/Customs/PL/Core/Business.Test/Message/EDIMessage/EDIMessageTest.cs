using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(EDIMessage))]
sealed class PLEDIMessageTest : BaseEdiMessageTest<EDIMessage>
{
	protected override string ApplicationCode => Messaging.Business.EDIInterchange.ApplicationCodes.PLCustoms;
}
