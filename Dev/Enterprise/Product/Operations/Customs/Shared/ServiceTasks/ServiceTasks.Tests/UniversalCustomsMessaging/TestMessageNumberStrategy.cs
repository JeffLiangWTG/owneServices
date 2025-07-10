using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	class TestMessageNumberStrategy : IMessageNumberStrategy
	{
		public TestMessageNumberStrategy(string number)
		{
			this.number = number;
		}

		readonly string number;

		public string GetMessageReferenceNumber() => number;
	}
}
