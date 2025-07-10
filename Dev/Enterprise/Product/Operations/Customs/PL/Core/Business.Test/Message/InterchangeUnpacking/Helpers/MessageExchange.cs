using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.PL.Business.Testing;

public readonly record struct MessageExchange(
	EnterpriseEDIMessage RequestMessage,
	EDIInterchange ResponseInterchange)
{
	public EDIInterchange RequestInterchange => RequestMessage.Interchange;

	public CusPollingTransaction CusPollingTransaction { get; init; }
}
