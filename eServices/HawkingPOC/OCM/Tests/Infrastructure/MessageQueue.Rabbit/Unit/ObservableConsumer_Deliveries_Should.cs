using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using RabbitMQ.Client;
using Xunit;
using Xunit.Abstractions;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit.Tests.Unit
{
	public class ObservableConsumer_Deliveries_Should
	{
		Mock<IModel> mockChannel;

		ObservableConsumer<TestQueueItem> consumer;

		public ObservableConsumer_Deliveries_Should()
		{
			mockChannel = new Mock<IModel>();

			var mockConnection = new Mock<IConnection>();
			mockConnection.Setup(m => m.CreateModel()).Returns(mockChannel.Object);

			consumer = new ObservableConsumer<TestQueueItem>(mockChannel.Object);
		}

		[Fact]
		public void BeObservableStreamOfDeliveryEventArgs()
		{
			var indices = new[] { 1, 5, 9 };
			var receivedItems = new List<TestQueueItem>();

			using (var subscription =
				consumer.Deliveries.Subscribe(onNext: item => receivedItems.Add(item),
											  onCompleted: () => { }))
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
