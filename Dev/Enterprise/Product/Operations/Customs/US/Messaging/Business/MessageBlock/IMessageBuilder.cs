namespace Enterprise.Customs.US.Messaging.Business
{
	public interface IMessageBuilder<TEDIMessage>
	{
		TEDIMessage PopulateMessage();
	}
}
