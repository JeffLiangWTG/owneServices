namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	public class EDIMessageQueueStateQueueProvider : UCMHostedServiceQueue
	{
		public EDIMessageQueueStateQueueProvider(string serviceTaskCode, string name, string applicationCode, string status)
			: base(serviceTaskCode, name, applicationCode, (x) => EDIMessageQueueStateFactory.GetQueueResultObject(x, status, true))
		{
		}
	}
}
