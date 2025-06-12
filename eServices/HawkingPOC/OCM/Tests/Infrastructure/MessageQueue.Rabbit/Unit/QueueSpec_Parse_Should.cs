using FluentAssertions;
using RabbitMQ.Client;
using Xunit;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit.Tests.Unit
{
	public class QueueSpec_Parse_Should
    {
        [Theory]
        [InlineData("foo1>bar1", "foo1", "bar1")]
        [InlineData("foo2>", "foo2", "foo2.dl")]
        [InlineData("foo3", "foo3", null)]
        public void GivenValidInput_ProduceExpectedQueueSpec(string input, string expectedQueue, string expectedDlQueue)
        {
            var spec = QueueSpec.Parse(input);

			spec.Name.Should().Be(expectedQueue);

			if (expectedDlQueue == null)
			{
				spec.DeadLetterQueue.Should().BeNull();
				spec.Configuration.Should().BeEmpty();
			}
			else
			{
				spec.DeadLetterQueue.Name.Should().Be(expectedDlQueue);
				spec.DeadLetterQueue.Configuration.Should().BeEmpty();
				spec.Configuration.Should().ContainKey(Headers.XDeadLetterExchange).WhichValue.Should().Be(string.Empty);
				spec.Configuration.Should().ContainKey(Headers.XDeadLetterRoutingKey).WhichValue.Should().Be(expectedDlQueue);
			}
		}
	}
}
