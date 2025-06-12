using System;
using System.Threading.Tasks;
using FluentAssertions;
using OcmPoc.Infrastructure.MessageInterfaces.Entities;
using OcmPoc.Infrastructure.MessageInterfaces.Enums;
using OcmPoc.Utils.Config;
using Xunit;
using Xunit.Abstractions;

namespace OcmPoc.Infrastructure.MessageTracking.Kafka.Tests.Integration
{
	public class EventLogger_LogEventAsync_Should : IDisposable
	{
		readonly EventLogger logger;
		readonly ITestOutputHelper output;

		public EventLogger_LogEventAsync_Should(ITestOutputHelper output)
		{
			logger = new EventLogger(new KafkaConfig
			{
				Brokers = "10.61.163.188:9092",
				Topic = { Event = nameof(EventLogger_LogEventAsync_Should) },
				Group = "hello"
			});
			this.output = output;
		}

		public void Dispose()
		{
			logger.Dispose();
		}

		[Fact]
		[Trait("TestType", "Integration")]
		public async Task SuccessfullyPublishEvent()
		{
			var messageEvent = MessageEvent.New(42, Guid.NewGuid(), MessageEventType.Sent);

			var offset = await logger.LogEventAsync(messageEvent);

			offset.Should().BeGreaterOrEqualTo(0);
			output.WriteLine($"Offset {offset}");
		}
    }
}
