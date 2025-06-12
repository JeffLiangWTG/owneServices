using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit.Tests.Unit
{
	public class QueueListener_QueueItems_Should
	{
		Mock<IModel> mockChannel;
		QueueListener<TestQueueItem> listener;

		IBasicConsumer consumer;

		public QueueListener_QueueItems_Should()
		{
			mockChannel = new Mock<IModel>();
			mockChannel.Setup(m => m.BasicConsume(It.IsAny<string>(), // queue
												  It.IsAny<bool>(),   // autoAck
												  It.IsAny<string>(), // consumerTag
												  It.IsAny<bool>(),   // noLocal
												  It.IsAny<bool>(),   // exclusive
												  It.IsAny<IDictionary<string, object>>(), // arguments
												  It.IsAny<IBasicConsumer>()))            // consumer
					   .Callback<string, bool, string, bool, bool, IDictionary<string, object>, IBasicConsumer>((q, aa, ct, nl, e, a, c) => consumer = c )
					   .Returns(() => "");

			var mockConnection = new Mock<IConnection>();
			mockConnection.Setup(m => m.CreateModel()).Returns(mockChannel.Object);

			listener = new QueueListener<TestQueueItem>(mockConnection.Object, "foo");
		}

		[Fact]
		public void BeObservableStreamOfQueueItems()
		{
			var indices = new[] { 1, 5, 9 };
			var receivedItems = new List<TestQueueItem>();

			using (var subscription = listener.QueueItems.Subscribe(onNext: item => receivedItems.Add(item)))
			{
				DeliverQueueItems(indices);
				consumer.HandleBasicCancel("");
			}

			using (new AssertionScope())
			{
				receivedItems.Should().HaveCount(3);
				receivedItems
					.Zip(indices, (item, index) => new { Item = item, ExpectedIndex = index })
					.ToList()
					.ForEach(x => 
					{
						x.Item.Delivery.DeliveryTag.Should().Be((ulong)x.ExpectedIndex);
						x.Item.Delivery.Exchange.Should().Be($"exch{x.ExpectedIndex}");
						x.Item.Delivery.RoutingKey.Should().Be($"key{x.ExpectedIndex}");
						x.Item.AdditionalHeader.Should().Be($"foo{x.ExpectedIndex}");
					});
			}
		}

		private void DeliverQueueItems(params int[] indices)
		{
			foreach (var index in indices)
			{
				var properties = new TestProperties
				{
					Headers = new Dictionary<string, object>
					{
						[nameof(TestQueueItem.AdditionalHeader)] = $"foo{index}"
					}
				};
				consumer.HandleBasicDeliver("", (ulong)index, false, $"exch{index}", $"key{index}", properties, null);
			}
		}
	}
}
