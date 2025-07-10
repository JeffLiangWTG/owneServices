using System.Linq;

namespace Enterprise.Rating.Business.Testing
{
	public class NamedAccountComparerTest : RatingTestCase
	{
		#region JobNamedAccountIsEmpty

		public void TestJobNamedAccountIsEmpty_BothLinesHaveNamedAccountsNotEmpty_ShouldBeEqual()
		{
			var comparer = new NamedAccountComparer(string.Empty);

			entry1.NamedAccounts = new[] { "AAA", "BBB" };
			entry2.NamedAccounts = new[] { "BBB", "CCC" };
			CompareTwoEntriesAndAssert("2 lines should be equal", 0, comparer);
		}

		public void TestJobNamedAccountIsEmpty_OneLineWithoutNamedAccount_ShouldBePreferred()
		{
			var comparer = new NamedAccountComparer(string.Empty);

			entry1.NamedAccounts = Enumerable.Empty<string>();
			entry2.NamedAccounts = new[] { "BBB", "CCC" };
			CompareTwoEntriesAndAssert("Line 1 should be preferred", 1, comparer);

			entry1.NamedAccounts = new[] { "BBB", "CCC" };
			entry2.NamedAccounts = Enumerable.Empty<string>();
			CompareTwoEntriesAndAssert("Line 2 should be preferred", -1, comparer);
		}

		public void TestJobNamedAccountIsEmpty_OneLineHasEmptyNamedAccount_ShouldBePreferred()
		{
			var comparer = new NamedAccountComparer(string.Empty);

			entry1.NamedAccounts = new[] { string.Empty, "AAA" };
			entry2.NamedAccounts = new[] { "BBB" };
			CompareTwoEntriesAndAssert("Line 1 should be preferred", 1, comparer);

			entry1.NamedAccounts = new[] { "BBB", "CCC" };
			entry2.NamedAccounts = new[] { "AAA", string.Empty };
			CompareTwoEntriesAndAssert("Line 2 should be preferred", -1, comparer);
		}

		public void TestJobNamedAccountIsEmpty_BothLinesHaveEmptyOrWithoutNamedAccount_ShouldBeEqual()
		{
			var comparer = new NamedAccountComparer(string.Empty);

			entry1.NamedAccounts = new[] { string.Empty };
			entry2.NamedAccounts = new[] { string.Empty };
			CompareTwoEntriesAndAssert("2 lines should be equal", 0, comparer);

			entry1.NamedAccounts = Enumerable.Empty<string>();
			entry2.NamedAccounts = new[] { string.Empty };
			CompareTwoEntriesAndAssert("2 lines should be equal", 0, comparer);

			entry1.NamedAccounts = new[] { "AAA", string.Empty };
			entry2.NamedAccounts = new[] { "BBB", string.Empty };
			CompareTwoEntriesAndAssert("2 lines should be equal", 0, comparer);

			entry1.NamedAccounts = Enumerable.Empty<string>();
			entry2.NamedAccounts = new[] { "CCC", string.Empty };
			CompareTwoEntriesAndAssert("2 lines should be equal", 0, comparer);
		}

		#endregion

		#region JobNamedAccountIsNotEmpty

		public void TestJobNamedAccountIsNotEmpty_BothLinesHaveNamedAccounts_DoNotMatchCriteria_ShouldBeEqual()
		{
			var comparer = new NamedAccountComparer("aaa");

			entry1.NamedAccounts = new[] { "BBB", "CCC" };
			entry2.NamedAccounts = new[] { "CCC", "DDD" };
			CompareTwoEntriesAndAssert("2 lines should be equal", 0, comparer);
		}

		public void TestJobNamedAccountIsNotEmpty_OneLineHasEmptyOrWithoutNamedAccounts_DoNotMatchCriteria_ShouldBePreferred()
		{
			var comparer = new NamedAccountComparer("aaa");

			entry1.NamedAccounts = Enumerable.Empty<string>();
			entry2.NamedAccounts = new[] { "CCC", "DDD" };
			CompareTwoEntriesAndAssert("Line 1 should be preferred", 1, comparer);

			entry1.NamedAccounts = new[] { "CCC", "DDD" };
			entry2.NamedAccounts = new[] { string.Empty };
			CompareTwoEntriesAndAssert("Line 2 should be preferred", -1, comparer);
		}

		public void TestJobNamedAccountIsNotEmpty_OneLineMatchesNamedAccount_ShouldBePreferred()
		{
			var comparer = new NamedAccountComparer("aaa");

			entry1.NamedAccounts = new[] { "AAA", "BBB" };
			entry2.NamedAccounts = new[] { "CCC", string.Empty };
			CompareTwoEntriesAndAssert("Line 1 should be preferred", 1, comparer);

			entry1.NamedAccounts = Enumerable.Empty<string>();
			entry2.NamedAccounts = new[] { "AAA", "BBB" };
			CompareTwoEntriesAndAssert("Line 2 should be preferred", -1, comparer);
		}

		public void TestJobNamedAccountIsNotEmpty_BothLinesMatchNamedAccount_ShouldBeEqual()
		{
			var comparer = new NamedAccountComparer("aaa");

			entry1.NamedAccounts = new[] { "AAA", "BBB" };
			entry2.NamedAccounts = new[] { "AAA", string.Empty };
			CompareTwoEntriesAndAssert("2 lines should be equal", 0, comparer);
		}

		#endregion

		void CompareTwoEntriesAndAssert(string message, int expectedResult, NamedAccountComparer comparer)
		{
			var line1 = entry1.RateLines[0];
			var line2 = entry2.RateLines[0];

			AssertEquals(message, expectedResult, comparer.Compare(new FastLine(line1), new FastLine(line2)));
		}

		protected override void SetUp()
		{
			base.SetUp();

			var costing = Helper.NewCosting(null);
			entry1 = costing.AddRateEntry("FCL", "SEA", "AUSYS", "USLAX", string.Empty, "20GP");
			entry2 = costing.AddRateEntry("FCL", "SEA", "AUSYS", "USLAX", string.Empty, "20GP");
		}

		RateEntry entry1;
		RateEntry entry2;
	}
}
