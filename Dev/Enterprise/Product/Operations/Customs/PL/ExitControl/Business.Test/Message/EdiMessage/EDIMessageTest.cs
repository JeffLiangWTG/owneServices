using Enterprise.Customs.PL.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

[TestedType(typeof(EDIMessage))]
sealed class EDIMessageTest : BaseEdiMessageTest<EDIMessage>
{
	protected override string ApplicationCode => Messaging.Business.EDIInterchange.ApplicationCodes.PLCustomsExitControl;
}
