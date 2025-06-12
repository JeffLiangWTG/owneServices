using System;
using FluentAssertions;
using OcmPoc.Infrastructure.MessageInterfaces.Entities;
using OcmPoc.Infrastructure.MessageInterfaces.Enums;
using Xunit;

namespace OcmPoc.Infrastructure.MessageTracking.Kafka.Tests.Unit
{
	public class JsonSerialiserOfMessageEvent_Should : IClassFixture<JsonSerialiser<MessageEvent>>
	{
		readonly JsonSerialiser<MessageEvent> serialiser;
		readonly MessageEvent testValue;

		public JsonSerialiserOfMessageEvent_Should(JsonSerialiser<MessageEvent> serialiser)
		{
			this.serialiser = serialiser;

			testValue = new MessageEvent
			{
				MessageId = new Guid("00000001-0002-0003-0004-000000000005"),
				Timestamp = DateTime.UtcNow,
				Type = MessageEventType.MappingFrom,
				Component = "foo"
			};
		}

		[Fact]
		public void RoundTripAMessageEvent()
		{
			var result = serialiser.Deserialize("", serialiser.Serialize("", testValue));

			result.Should().BeEquivalentTo(testValue);
		}
	}
}
