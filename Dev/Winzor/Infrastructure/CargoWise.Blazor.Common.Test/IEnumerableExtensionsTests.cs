using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CargoWise.Blazor.Common.Test
{
	public class IEnumerableExtensionsTests
	{
		class A
		{
			public IEnumerable<A> Children { get; set; }
		}

		[Test]
		public void EmptyList_EmptyReturned()
		{
			var listA = new List<A>();

			var result = listA.Flatten((a) => a.Children);

			Assert.That(result, Is.Empty);
		}

		[Test]
		public void FlatList_NullChild_EquivalentListReturned()
		{
			var listA = new List<A>() { new A(), new A(), new A(), new A() };

			var result = listA.Flatten((a) => a.Children);

			Assert.That(result, Is.EquivalentTo(listA));
		}

		[Test]
		public void FlatList_EmptyChildren_EquivalentListReturned()
		{
			var listA = new List<A>
			{
				new A() { Children = Array.Empty<A>() },
				new A() { Children = Array.Empty<A>() },
				new A() { Children = Array.Empty<A>() },
				new A() { Children = Array.Empty<A>() },
			};

			var result = listA.Flatten((a) => a.Children);

			Assert.That(result, Is.EquivalentTo(listA));
		}

		[Test]
		public void FlatList_N1NestedChildren_EquivalentListReturned()
		{
			var child1 = new A();
			var child2 = new A();
			var child3 = new A();
			var child4 = new A();
			var parent1 = new A() { Children = new A[] { child1, child2 } };
			var parent2 = new A() { Children = new A[] { child3 } };
			var parent3 = new A() { Children = Array.Empty<A>() };
			var parent4 = new A() { Children = new A[] { child4 } };

			var listA = new A[]
			{
				parent1,
				parent2,
				parent3,
				parent4,
			};

			var expectation = new A[]
			{
				child1,
				child2,
				child3,
				child4,
				parent1,
				parent2,
				parent3,
				parent4,
			};

			var result = listA.Flatten((a) => a.Children);

			Assert.That(result, Is.EquivalentTo(expectation));
		}

		[Test]
		public void FlatList_XNNestedChildren_EquivalentListReturned()
		{
			var childChildChildChild1 = new A();

			var childChildChild1 = new A() { Children = new A[] { childChildChildChild1 } };

			var childChild1 = new A() { Children = new A[] { childChildChild1 } };
			var childchild2 = new A();

			var child1 = new A();
			var child2 = new A();
			var child3 = new A() { Children = new A[] { childChild1 } };
			var child4 = new A();
			var parent1 = new A() { Children = new A[] { child1, child2 } };
			var parent2 = new A() { Children = new A[] { child3 } };
			var parent3 = new A() { Children = Array.Empty<A>() };
			var parent4 = new A() { Children = new A[] { child4 } };

			var listA = new A[]
			{
				parent1,
				parent2,
				parent3,
				parent4,
			};

			var expectation = new A[]
			{
				childChildChildChild1,
				childChildChild1,
				childChild1,
				child1,
				child2,
				child3,
				child4,
				parent1,
				parent2,
				parent3,
				parent4,
			};

			var result = listA.Flatten((a) => a.Children);

			Assert.That(result, Is.EquivalentTo(expectation));
		}

		[Test]
		public void FlatList_DuplicateChildren_MultipleInstancesOfRepeatedChildrenReturned()
		{
			var childChildChildChild1 = new A();

			var childChildChild1 = new A() { Children = new A[] { childChildChildChild1 } };

			var childChild1 = new A() { Children = new A[] { childChildChild1 } };
			var childchild2 = new A();

			var child1 = new A();
			var child2 = new A() { Children = new A[] { child1 } };
			var child3 = new A() { Children = new A[] { childChild1, childChild1 } };
			var child4 = new A();
			var parent1 = new A() { Children = new A[] { child1, child2 } };
			var parent2 = new A() { Children = new A[] { child3 } };
			var parent3 = new A() { Children = Array.Empty<A>() };
			var parent4 = new A() { Children = new A[] { child4 } };

			var listA = new A[]
			{
				parent1,
				parent2,
				parent3,
				parent4,
			};

			var expectation = new A[]
			{
				parent4,
				child4,
				parent3,
				parent2,
				child3,
				childChild1,
				childChildChild1,
				childChildChildChild1,
				childChild1,
				childChildChild1,
				childChildChildChild1,
				parent1,
				child1,
				child2,
				child1,
			};

			var result = listA.Flatten((a) => a.Children);

			Assert.That(result, Is.EquivalentTo(expectation));
		}
	}
}
