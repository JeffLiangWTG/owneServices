namespace OcmPoc.Infrastructure.MessageInterfaces.Queueing
{
	public interface IQueueClientFactory
    {
		IQueueClient CreateQueueClient();
    }
}
