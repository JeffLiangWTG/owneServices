using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Xunit;

namespace OcmPoc.Infrastructure.MessageTracking.Kafka.Tests.Unit
{
	public class JsonSerialiserOfString_Should : IClassFixture<JsonSerialiser<string>>
	{
		readonly JsonSerialiser<string> serialiser;
		readonly string testValueString;
		readonly byte[] testValueBytes; 

		public JsonSerialiserOfString_Should(JsonSerialiser<string> serialiser)
		{
			this.serialiser = serialiser;
			testValueString = "Hello, World!";
			testValueBytes = Encoding.UTF8.GetBytes($"\"{testValueString}\"");
		}

		[Fact]
		public void PassThroughConfiguration()
		{
			var config = new Dictionary<string, object>();

			var result = serialiser.Configure(config, false);

			result.Should().BeSameAs(config);
		}

		[Fact]
		public void SerialiseAStringCorrectly()
		{
			var result = serialiser.Serialize("", testValueString);

			result.Should().BeEquivalentTo(testValueBytes, opts => opts.WithStrictOrdering());
		}

		[Fact]
		public void DeserialiseAStringCorrectly()
		{
			var result = serialiser.Deserialize("", testValueBytes);

			result.Should().Be(testValueString);
		}

		[Fact]
		public void RoundTripAString()
		{
			var result = serialiser.Deserialize("", serialiser.Serialize("", testValueString));

			result.Should().Be(testValueString);
		}
	}
}
