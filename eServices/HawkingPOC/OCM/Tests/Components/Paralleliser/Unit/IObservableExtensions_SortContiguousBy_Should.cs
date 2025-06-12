using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace OcmPoc.Components.Paralleliser.Tests.Unit
{
    public class IObservableExtensions_SortContiguousBy_Should
    {
		[Fact]
		public async Task ReorderASequenceBasedOnTheKey()
		{
			var items = new long[] { 2, 0, 3, 1, 5, 4 };
			var unordered = items.ToObservable();

			var ordered = await unordered.SortContiguousBy(x => x).ToList().FirstAsync();

			ordered.Should().Contain(items).And.BeInAscendingOrder();
		}

		[Fact]
		public async Task ReorderAnIndexedSequenceBasedOnTheKey()
		{
			var items = new long[] { 1, 2, 0, 4, 3, 5 }.Select(x => new { Item = "foo", Index = x }).ToList();
			var unordered = items.ToObservable();

			var ordered = await unordered.SortContiguousBy(x => x.Index).ToList().FirstAsync();

			ordered.Should().Contain(items).And.BeInAscendingOrder(x => x.Index);
		}
	}
}
