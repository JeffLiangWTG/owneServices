namespace Enterprise.Customs.SG.V4.Business
{
	public interface IMessageManager
	{
		bool SendAmendmentMessages(Customs.Business.ISendsMessagesToCustoms sender);
		bool SendCancellationMessages(Customs.Business.ISendsMessagesToCustoms sender);
		bool SendOriginalMessages(Customs.Business.ISendsMessagesToCustoms sender);
		bool SendRefundMessage(Customs.Business.ISendsMessagesToCustoms sender);
	}
}
