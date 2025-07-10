using CargoWise.Common;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UniqueListWithPriorityTest : TestCase
	{
		class A
		{
			public int i;
		}

		public void TestPriorities()
		{
			var a = new A { i = 1 };
			var b = new A { i = 2 };
			A c = null;
			var d = new A { i = 2 };
			var e = new A { i = 3 };
			var f = e;
			var g = new A { i = 4 };

			var list = new UniqueListWithPriority<A>(null)
							{
								{ 1, new[] { b } },
								{ 0, new[] { c, } },
								{ 5, new[] { d, e } },
								{ 5, new[] { e } },
							};

			list.Merge(new UniqueListWithPriority<A>(null)
							{
								{ 1, new[] { a, b } },
								{ 0, new[] { c, d } },
								{ 5, new[] { c, d, e } },
								{ 6, new[] { f } },
								{ 3, new[] { g, d, null } },
								{ 5, new[] { e } },
							});

			CombineAssertions("Testing using ref equals for comparisons...",
							  () =>
							  {
								  AssertContainsExactElementsInAnyOrder("a", new[] { 1 }, list.GetPriorities(a));
								  AssertContainsExactElementsInAnyOrder("b", new[] { 1 }, list.GetPriorities(b));
								  AssertContainsExactElementsInAnyOrder("c", System.Array.Empty<int>(), list.GetPriorities(c));
								  AssertContainsExactElementsInAnyOrder("d", new[] { 5, 3 }, list.GetPriorities(d));
								  AssertContainsExactElementsInAnyOrder("e", new[] { 5, 6 }, list.GetPriorities(e));
								  AssertContainsExactElementsInAnyOrder("f", new[] { 5, 6 }, list.GetPriorities(f));
								  AssertContainsExactElementsInAnyOrder("g", new[] { 3 }, list.GetPriorities(g));
							  });

			list = new UniqueListWithPriority<A>(new LambdaComparer<A>((x, y) => x.i.Equals(y.i), x => x.i.GetHashCode()))
						{
							{ 1, new[] { a, b } },
							{ 0, new[] { c, d } },
							{ 5, new[] { c, d, e } },
							{ 6, new[] { f } },
							{ 3, new[] { g, d, null } },
							{ 5, new[] { e } },
						};

			CombineAssertions("Testing using custom comparer...",
							  () =>
							  {
								  AssertContainsExactElementsInAnyOrder("a 1", new[] { 1 }, list.GetPriorities(a));
								  AssertContainsExactElementsInAnyOrder("b 2", new[] { 1, 5, 3 }, list.GetPriorities(b));
								  AssertContainsExactElementsInAnyOrder("c null", System.Array.Empty<int>(), list.GetPriorities(c));
								  AssertContainsExactElementsInAnyOrder("d 2", new[] { 1, 5, 3 }, list.GetPriorities(d));
								  AssertContainsExactElementsInAnyOrder("e 3", new[] { 5, 6 }, list.GetPriorities(e));
								  AssertContainsExactElementsInAnyOrder("f e", new[] { 5, 6 }, list.GetPriorities(f));
								  AssertContainsExactElementsInAnyOrder("g 4", new[] { 3 }, list.GetPriorities(g));
							  });
		}
	}
}
