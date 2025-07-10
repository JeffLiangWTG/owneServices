namespace Enterprise.Freight.Forwarding.AWB.Messaging
{
	interface IMessageSendingManager
	{
		bool ReSendAll(out string resultMessage);
		bool ReSendThisOnly(out string resultMessage);
		bool SendAll(out string resultMessage);
	}
}
