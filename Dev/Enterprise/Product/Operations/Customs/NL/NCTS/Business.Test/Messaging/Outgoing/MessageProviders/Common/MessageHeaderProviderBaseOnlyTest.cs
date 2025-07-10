using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(MessageHeaderProvider))]
sealed class MessageHeaderProviderBaseOnlyTest : MessageHeaderProviderAbstractTest<MessageHeaderProvider>
{
	protected override string MessageType => ZString.Empty;

	protected override string MovementType => NctsMovementType.Codes.Departure;
}
