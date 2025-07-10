using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.TransportConsignment.Business.Testing
{
	class PackageTotalsTest : DtbBookingConsignmentTestCaseWithFactory
	{
		#region TestConstructor

		public void TestConstructor()
		{
			var bizOCollection = new DynamicBusinessObjectCollection(Factory);
			bizOCollection.Load("SELECT 1 as Num");
			var bizO = bizOCollection[0];
			AssertExceptionThrown(typeof(ArgumentNullException), () => new PackageTotals(bizO, null, GroupedPackTypeCounts.Empty));
			AssertExceptionThrown(typeof(ArgumentNullException), () => new PackageTotals(null, Enumerable.Empty<PackTypeCount>(), GroupedPackTypeCounts.Empty));
			AssertExceptionThrown(typeof(ArgumentNullException), () => new PackageTotals(bizO, Enumerable.Empty<PackTypeCount>(), null));
		}

		#endregion

		#region TestCacheIndexer

		public void TestCacheIndexer()
		{
			var bizOCollection = new DynamicBusinessObjectCollection(Factory);
			bizOCollection.Load("SELECT 1 as Num, 'X' as Word");
			var bizO = bizOCollection[0];
			var packTotals = new PackageTotals(bizO, Enumerable.Empty<PackTypeCount>(), GroupedPackTypeCounts.Empty);
			AssertEquals("packTotals['Num']", 1, packTotals["Num"]);
			AssertEquals("packTotals['Word']", "X", packTotals["Word"]);
		}

		#endregion

		#region TestGetCompletePackageSummary

		public void TestGetCompletePackageSummary()
		{
			AssertEquals("", PackageTotals.GetCompletePackageSummary((IEnumerable<PackTypeCount>)null));
			AssertEquals("", PackageTotals.GetCompletePackageSummary((IEnumerable<GroupedPackTypeCounts>)null));
			AssertEquals("", PackageTotals.GetCompletePackageSummary(Enumerable.Empty<PackTypeCount>()));
			AssertEquals("", PackageTotals.GetCompletePackageSummary(Enumerable.Empty<GroupedPackTypeCounts>()));

			AssertEquals("3x BOX, 7x CTN, 5x PLT",
				PackageTotals.GetCompletePackageSummary(new[] { new PackTypeCount("PLT", 5), new PackTypeCount("BOX", 3), new PackTypeCount("CTN", 7) }));

			AssertEquals("5x PLT and 7x PLT should have been consolidated.", "3x BOX, 12x PLT",
				PackageTotals.GetCompletePackageSummary(new[] { new PackTypeCount("PLT", 5), new PackTypeCount("BOX", 3), new PackTypeCount("PLT", 7) }));

			var key1 = ZGuid.NewZGuid();
			var key2 = ZGuid.NewZGuid();
			AssertEquals("1x BOX and 3x BOX should have been consolidated.", "4x BOX, 7x CTN, 5x PLT",
				PackageTotals.GetCompletePackageSummary(new[]
				{
					new GroupedPackTypeCounts(key1, new[] { new PackTypeCount("PLT", 5), new PackTypeCount("BOX", 3) }),
					new GroupedPackTypeCounts(key2, new[] { new PackTypeCount("CTN", 7), new PackTypeCount("BOX", 1) }),
				}));
			AssertEquals("Should not consolidate a unique group more than once.", "3x BOX, 7x CTN, 1x KEG, 5x PLT",
				PackageTotals.GetCompletePackageSummary(new[]
				{
					new GroupedPackTypeCounts(key1, new[] { new PackTypeCount("PLT", 5), new PackTypeCount("BOX", 3) }),
					new GroupedPackTypeCounts(key1, new[] { new PackTypeCount("PLT", 5), new PackTypeCount("BOX", 3) }),
					new GroupedPackTypeCounts(key2, new[] { new PackTypeCount("CTN", 7), new PackTypeCount("KEG", 1) }),
				}));
		}

		#endregion

		#region TestBookedPickupPackageList

		public void TestBookedPickupPackageList()
		{
			var bizOCollection = new DynamicBusinessObjectCollection(Factory);
			bizOCollection.Load("SELECT 1 as Num");
			var bizO = bizOCollection[0];

			var bookedPackTypes = new[] { new PackTypeCount("PLT", 5), new PackTypeCount("BOX", 3), new PackTypeCount("CTN", 7) };
			var groupedPackTypes = new GroupedPackTypeCounts(ZGuid.NewZGuid(), bookedPackTypes);
			var packTotals = new PackageTotals(bizO, Enumerable.Empty<PackTypeCount>(), groupedPackTypes);
			int i = 0;
			AssertEquals(3, packTotals.BookedPickupPackageList.PackTypeCounts.Count());

			foreach (var bookedPickup in packTotals.BookedPickupPackageList.PackTypeCounts)
			{
				var packTypeFromOtherArray = bookedPackTypes[i++];
				AssertEquals(packTypeFromOtherArray.PackType + "-" + packTypeFromOtherArray.Quantity, bookedPickup.PackType + "-" + bookedPickup.Quantity);
			}
		}

		#endregion

		#region TestPackageSummary

		public void TestPackageSummary()
		{
			var bizOCollection = new DynamicBusinessObjectCollection(Factory);
			bizOCollection.Load("SELECT 1 as Num");
			var bizO = bizOCollection[0];

			var packTypes = new[] { new PackTypeCount("BOX", 3), new PackTypeCount("CTN", 7), new PackTypeCount("PLT", 5) };
			var packTotals = new PackageTotals(bizO, packTypes, GroupedPackTypeCounts.Empty);
			AssertEquals("3x BOX, 7x CTN, 5x PLT", packTotals.PackageSummary);
		}

		#endregion

		#region TestPackageSummaryList

		public void TestPackageSummaryList()
		{
			var bizOCollection = new DynamicBusinessObjectCollection(Factory);
			bizOCollection.Load("SELECT 1 as Num");
			var bizO = bizOCollection[0];

			var packTypes = new[] { new PackTypeCount("PLT", 5), new PackTypeCount("BOX", 3), new PackTypeCount("CTN", 7) };
			var packTotals = new PackageTotals(bizO, packTypes, GroupedPackTypeCounts.Empty);
			int i = 0;
			AssertEquals(3, packTotals.PackageSummaryList.Count());

			foreach (var packTypeCount in packTotals.PackageSummaryList)
			{
				var packTypeFromOtherArray = packTypes[i++];
				AssertEquals(packTypeFromOtherArray.PackType + "-" + packTypeFromOtherArray.Quantity, packTypeCount.PackType + "-" + packTypeCount.Quantity);
			}
		}

		#endregion
	}
}
