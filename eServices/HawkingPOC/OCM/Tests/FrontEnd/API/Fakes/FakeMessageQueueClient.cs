using System;
using System.Reactive.Disposables;
using Moq;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;

namespace OcmPoc.FrontEnd.Api.Tests.Fakes
{
	public class FakeMessageQueueClient : IQueueClient
	{
		public IDisposable CreateTemporaryQueues(IQueueSpecCollection specs)
		{
			return Disposable.Empty;
		}

		public IExchangeWriter GetExchangeWriter(string exchangeName)
		{
			return new Mock<IExchangeWriter>().Object;
		}

		public IQueueListener<TQueueItem> GetQueueListener<TQueueItem>(string queueName)
			where TQueueItem : QueueItem, new()
		{
			return new Mock<IQueueListener<TQueueItem>>().Object;
		}

		public IQueueReader<TQueueItem> GetQueueReader<TQueueItem>(string queueName = null) 
			where TQueueItem : QueueItem, new()
		{
			return new Mock<IQueueReader<TQueueItem>>().Object;
		}

		public IQueueWriter GetQueueWriter(string queueName)
		{
			return new Mock<IQueueWriter>().Object;
		}

		public void Dispose()
		{
			// nothing to do;
		}

		public void CreateDurableQueues(IQueueSpecCollection specs)
		{
			
		}

		public void CreateExchange(IExchangeSpec spec)
		{
		}

		public void BindQueues(IQueueSpecCollection specs)
		{
		}

		public void CreateExchanges(IExchangeSpecCollection specs)
		{
		}
	}
}
