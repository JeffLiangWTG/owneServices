using System.Linq;
using FluentAssertions;
using Xunit;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit.Tests.Unit
{
	public class QueueSpecCollection_Add_Should
    {
		[Theory]
		[InlineData("foo1>bar1", "bar1", "foo1")]
		[InlineData("foo2>", "foo2.dl", "foo2")]
		[InlineData("foo3", "foo3")]
		public void PopulateListAsExpected(string spec, params string[] expectedQueues)
		{
			var specs = new QueueSpecCollection();

			specs.Add(QueueSpec.Parse(spec));

			specs.Select(s => s.Name).Should().BeEquivalentTo(expectedQueues, opts => opts.WithStrictOrdering());
		}
    }
}
