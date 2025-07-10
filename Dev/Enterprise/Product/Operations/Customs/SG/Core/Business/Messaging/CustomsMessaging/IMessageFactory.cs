namespace Enterprise.Customs.SG.Business.CustomsMessaging
{
	using Enterprise.Customs.SG.V4.Business;

	public enum MessageType
	{
		EDIFact, XML
	}

	interface IMessageFactory
	{
		MessageType MessageType { get; }
		ICusMessage GetOriginalMessage(ICustomsDec cusEntryHeader);
		ICusMessage GetAmendmentMessage(ICustomsDec cusEntryHeader);
		ICusMessage GetRefundMessage(ICustomsDec cusEntryHeader);
		ICusMessage GetCancellationMessage(ICustomsDec cusEntryHeader);
	}
}
