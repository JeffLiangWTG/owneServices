using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using OcmPoc.Infrastructure.MessageInterfaces.Entities;
using OcmPoc.Infrastructure.MessageInterfaces.Enums;
using OcmPoc.Utils.Config;
using Xunit;
using Xunit.Abstractions;

namespace OcmPoc.Infrastructure.MessageTracking.Kafka.Tests.Integration
{
	public class EventListener_Events_Should : IDisposable
    {
		readonly EventLogger logger;
		readonly EventListener listener;
		readonly ITestOutputHelper output;

		public EventListener_Events_Should(ITestOutputHelper output)
		{
			var config = new KafkaConfig
			{
				Brokers = "10.61.163.188:9092",
				Topic = { Event = "foo" },
				Group = "hello"
			};
			listener = new EventListener(config);
			logger = new EventLogger(config);
			this.output = output;
			listener.OnLog += LogToOutput;
		}

		void LogToOutput(object sender, string message)
		{
			output.WriteLine(message);
		}

		public void Dispose()
		{
			output.WriteLine("Disposing");
			listener.OnLog -= LogToOutput;
			logger.Dispose();
			listener.Dispose();
		}

		[Fact]
		[Trait("TestType", "Integration")]
		public async Task BeSequenceOfPublishedEvents()
		{
			var messageFlowId = 42;
			var messageId = Guid.NewGuid();
			var messageEvent = MessageEvent.New(messageFlowId, messageId, MessageEventType.Sent);
			var events = new List<MessageEvent>();

			using (listener.Events.Subscribe(e => events.Add(e)))
			{
				await Task.Delay(TimeSpan.FromSeconds(3));
				var offset = await logger.LogEventAsync(messageEvent);
				output.WriteLine($"Logged {offset}");
				await Task.Delay(TimeSpan.FromSeconds(5));
				output.WriteLine("Finished listening");

				offset.Should().BeGreaterOrEqualTo(0);
			}

			events.Last().Should().BeEquivalentTo(messageEvent);
		}
	}
}
