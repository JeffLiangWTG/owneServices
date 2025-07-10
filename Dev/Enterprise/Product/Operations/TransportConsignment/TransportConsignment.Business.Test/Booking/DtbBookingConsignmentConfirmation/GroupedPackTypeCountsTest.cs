using System;
using System.Linq;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	class GroupedPackTypeCountsTest : TestCase
	{
		#region TestConstructor

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new GroupedPackTypeCounts(ZGuid.NewZGuid(), null));
			AssertExceptionThrown<InvalidOperationException>("Use 'Empty' if you want to create an Empty GroupedPackTypeCounts.",
				() => new GroupedPackTypeCounts(ZGuid.Empty, Enumerable.Empty<PackTypeCount>()));
		}

		#endregion

		#region TestEmpty

		public void TestEmpty()
		{
			AssertEquals(ZGuid.Empty, GroupedPackTypeCounts.Empty.Key);
			AssertEquals(0, GroupedPackTypeCounts.Empty.PackTypeCounts.Count());
			AssertEquals(true, GroupedPackTypeCounts.Empty.IsEmpty);
		}

		#endregion

		#region TestIsEmpty

		public void TestIsEmpty()
		{
			AssertEquals(false, new GroupedPackTypeCounts(ZGuid.NewZGuid(), Enumerable.Empty<PackTypeCount>()).IsEmpty);
			AssertEquals(true, GroupedPackTypeCounts.Empty.IsEmpty);
		}

		#endregion

		#region TestKey

		public void TestKey()
		{
			var key = ZGuid.NewZGuid();
			AssertEquals(key, new GroupedPackTypeCounts(key, Enumerable.Empty<PackTypeCount>()).Key);
		}

		#endregion

		#region TestPackTypeCounts

		public void TestPackTypeCounts()
		{
			var packTypeCounts = new[] { new PackTypeCount("PLT", 2), new PackTypeCount("CTN", 3) };
			AssertContainsExactElementsInAnyOrder(packTypeCounts, new GroupedPackTypeCounts(ZGuid.NewZGuid(), packTypeCounts).PackTypeCounts);
		}

		#endregion
	}
}
