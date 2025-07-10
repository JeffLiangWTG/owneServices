using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LandedCosting.Business.Testing
{
	public sealed class DistributeeComparerTest : TestCaseWithFactory
	{
		public void TestDistributeeLineCompare()
		{
			DummyLandedCostHeader dummyHost = Factory.New<DummyLandedCostHeader>();
			dummyHost.LineComparerExposed = new DummyLineComparer();

			DummyIUltimateDistributee dummyDistributee1 = Factory.New<DummyIUltimateDistributee>();
			dummyDistributee1.HumanReadableCodeExposed = "ZZZ";
			dummyDistributee1.CostInLocalCurrencyExposed = 400m;

			DummyIUltimateDistributee dummyDistributee2 = Factory.New<DummyIUltimateDistributee>();
			dummyDistributee2.HumanReadableCodeExposed = "AAA";
			dummyDistributee2.CostInLocalCurrencyExposed = 300m;

			DummyIUltimateDistributee dummyDistributee3 = Factory.New<DummyIUltimateDistributee>();
			dummyDistributee3.HumanReadableCodeExposed = "CCC";
			dummyDistributee3.CostInLocalCurrencyExposed = 500m;

			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			lCHeader.LT_ParentID = dummyHost.PK;
			lCHeader.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			LandedCostHistory lCLine1 = lCHeader.Histories.AddNew();
			lCLine1.UltimateDistributee = dummyDistributee1;

			LandedCostHistory lCLine2 = lCHeader.Histories.AddNew();
			lCLine2.UltimateDistributee = dummyDistributee2;

			LandedCostHistory lCLine3 = lCHeader.Histories.AddNew();
			lCLine3.UltimateDistributee = dummyDistributee3;
			DummyLineComparer comparer = new DummyLineComparer();
			int result = comparer.Compare(lCLine1.UltimateDistributee, lCLine2.UltimateDistributee);
			AssertEquals("ZZZ > AAA", true, result > 0);

			result = comparer.Compare(lCLine2.UltimateDistributee, lCLine3.UltimateDistributee);
			AssertEquals("AAA < CCC", true, result < 0);

			result = comparer.Compare(lCLine3.UltimateDistributee, lCLine1.UltimateDistributee);
			AssertEquals("CCC < ZZZ", true, result < 0);
		}

		public void TestCompare()
		{
			DummyLandedCostHeader dummyHost = Factory.New<DummyLandedCostHeader>();

			DummyIUltimateDistributee dummyDistributee1 = Factory.New<DummyIUltimateDistributee>();
			dummyDistributee1.CostInLocalCurrencyExposed = 400m;

			DummyIUltimateDistributee dummyDistributee2 = Factory.New<DummyIUltimateDistributee>();
			dummyDistributee2.CostInLocalCurrencyExposed = 300m;

			DummyIUltimateDistributee dummyDistributee3 = Factory.New<DummyIUltimateDistributee>();
			dummyDistributee3.CostInLocalCurrencyExposed = 500m;

			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			lCHeader.LT_ParentID = dummyHost.PK;
			lCHeader.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			LandedCostHistory lCLine1 = lCHeader.Histories.AddNew();
			lCLine1.UltimateDistributee = dummyDistributee1;

			LandedCostHistory lCLine2 = lCHeader.Histories.AddNew();
			lCLine2.UltimateDistributee = dummyDistributee2;

			LandedCostHistory lCLine3 = lCHeader.Histories.AddNew();
			lCLine3.UltimateDistributee = dummyDistributee3;

			//ascending
			var list = new List<LandedCostHistory>(lCHeader.Histories);
			list.Sort(new LCHistoryCostAscendingComparer());
			AssertEquals("First element after sort", lCLine2, list[0]);
			AssertEquals("Second element after sort", lCLine1, list[1]);
			AssertEquals("Third element after sort", lCLine3, list[2]);

			//descending
			list.Sort(new LCHistoryCostDescendingComparer());
			AssertEquals("First element after sort", lCLine3, list[0]);
			AssertEquals("Second element after sort", lCLine1, list[1]);
			AssertEquals("Third element after sort", lCLine2, list[2]);
		}

		public sealed class DummyLineComparer : IComparer
		{
			public override bool Equals(object obj) => obj.GetType() == typeof(DummyLineComparer);

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required because Equals is overridden")]
			public override int GetHashCode() => base.GetHashCode();

			public int Compare(object x, object y)
			{
				var x1 = x as IUltimateDistributee;
				var y1 = y as IUltimateDistributee;

				int result = 0;
				if (x1 != null && y1 != null)
				{
					result = x1.HumanReadableCode.CompareTo(y1.HumanReadableCode);
				}
				return result;
			}
		}

		sealed class LCHistoryCostAscendingComparer : IComparer<LandedCostHistory>
		{
			public override bool Equals(object obj) => obj.GetType() == typeof(LCHistoryCostAscendingComparer);

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required because Equals is overridden")]
			public override int GetHashCode() => base.GetHashCode();

			public int Compare(LandedCostHistory lCHistoryX, LandedCostHistory lCHistoryY)
			{
				var result = 0;
				if (lCHistoryX.UltimateDistributee != null && lCHistoryY.UltimateDistributee != null)
				{
					result = lCHistoryX.UltimateDistributee.CostInLocalCurrency.CompareTo(lCHistoryY.UltimateDistributee.CostInLocalCurrency);
				}
				return result;
			}
		}
	}
}
