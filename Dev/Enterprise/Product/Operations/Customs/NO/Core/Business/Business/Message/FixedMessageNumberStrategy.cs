using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NO.Business;

public sealed class FixedMessageNumberStrategy : IMessageNumberStrategy
{
	//This is a temporary implementation. In WI00753510 - NO - NCTS - Message Number Format, we will implement the message number strategy.
	public string GetMessageReferenceNumber() => "1";
}
