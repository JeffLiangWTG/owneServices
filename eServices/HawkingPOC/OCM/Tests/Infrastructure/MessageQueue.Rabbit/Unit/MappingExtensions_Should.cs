using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using Xunit;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit.Tests.Unit
{
	public class MappingExtensions_Should
	{
		TestQueueItem initialItem;

		public MappingExtensions_Should()
		{
			initialItem = new TestQueueItem
			{
				MessageId = Guid.NewGuid(),
				Sender = "someone",
				Recipient = "someone else",
				AdditionalHeader = "foo"
			};
			initialItem.Metadata["Int32Header"] = 42;
			initialItem.Metadata["DoubleHeader"] = 42.0;
		}

		[Fact]
		public void RoundTripQueueItemCorrectly()
		{
			var result = new TestProperties().MapFrom(initialItem).MapTo<TestQueueItem>();

			result.Should().BeEquivalentTo(initialItem);
		}

		[Fact]
		public void HandleRoundTrippingADerivedItemViaALessDerivedItem()
		{
			var lessDerivedItem = new TestProperties().MapFrom(initialItem).MapTo<QueueItem>();
			var result = new TestProperties().MapFrom(lessDerivedItem).MapTo<TestQueueItem>();

			result.Should().BeEquivalentTo(initialItem);
		}
	}
}
