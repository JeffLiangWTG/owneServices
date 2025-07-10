using System;
using System.Linq;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class FindPickResultTest : WhsTestCaseWithFactory
	{
		#region TestConstructor

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new FindPickResult((WhsPick)null));
			AssertExceptionThrown<ArgumentNullException>(() => new FindPickResult(Factory.New<WhsPick>(), (WhsPickProcessTask)null));
			AssertExceptionThrown<ArgumentNullException>(() => new FindPickResult(Factory.New<WhsPick>(), (WhsPickLine[])null));
			AssertExceptionThrown<ArgumentNullException>(() => new FindPickResult(null, new[] { Factory.New<WhsPickLine>() }));
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => new FindPickResult(Factory.New<WhsPick>(), Array.Empty<WhsPickLine>()));
			AssertExceptionThrown<ArgumentNullException>(() => new FindPickResult(Factory.New<WhsPick>(), new[] { Factory.New<WhsPickLine>() }, null));
			AssertExceptionThrown<ArgumentException>(() => new FindPickResult(""));
		}

		#endregion

		#region TestIsConcurrencyError

		public void TestIsConcurrencyError()
		{
			AssertEquals(false, new FindPickResult(Factory.New<WhsPick>()).IsConcurrencyError);
			AssertEquals(false, new FindPickResult(Factory.New<WhsPick>(), new[] { Factory.New<WhsPickLine>() }).IsConcurrencyError);
			AssertEquals(false, new FindPickResult("Error").IsConcurrencyError);
			AssertEquals(false, FindPickResult.NoPick.IsConcurrencyError);
			AssertEquals(true, FindPickResult.ConcurrencyError.IsConcurrencyError);
		}

		#endregion

		#region TestIsPutawayOnlyPickResult

		public void TestIsPutawayOnlyPickResult()
		{
			AssertEquals(false, new FindPickResult(Factory.New<WhsPick>(), new[] { Factory.New<WhsPickLine>() }).IsPutawayOnlyPickResult);
			AssertEquals(false, new FindPickResult("Error").IsPutawayOnlyPickResult);
			AssertEquals(false, FindPickResult.NoPick.IsPutawayOnlyPickResult);
			AssertEquals(false, FindPickResult.ConcurrencyError.IsPutawayOnlyPickResult);
			AssertEquals(true, new FindPickResult(Factory.New<WhsPick>()).IsPutawayOnlyPickResult);
		}

		#endregion

		#region TestErrorMessage

		public void TestErrorMessage()
		{
			AssertEquals("", new FindPickResult(Factory.New<WhsPick>(), new[] { Factory.New<WhsPickLine>() }).ErrorMessage);
			AssertEquals("Error", new FindPickResult("Error").ErrorMessage);
			AssertEquals("", FindPickResult.NoPick.ErrorMessage);
			AssertEquals("Another user has taken the next Pick.", FindPickResult.ConcurrencyError.ErrorMessage);
		}

		#endregion

		#region TestPick

		public void TestPick()
		{
			var pick = Factory.New<WhsPick>();
			AssertEquals(pick, new FindPickResult(pick, new[] { Factory.New<WhsPickLine>() }).Pick);
			AssertNull(new FindPickResult("Error").Pick);
			AssertNull(FindPickResult.NoPick.Pick);
			AssertNull(FindPickResult.ConcurrencyError.Pick);
		}

		#endregion

		#region TestPickLines

		public void TestPickLines()
		{
			var pickLines = new[] { Factory.New<WhsPickLine>() };
			AssertContainsExactElementsInAnyOrder(pickLines, new FindPickResult(Factory.New<WhsPick>(), pickLines).PickLines);
			AssertEquals(0, new FindPickResult("Error").PickLines.Count());
			AssertEquals(0, FindPickResult.NoPick.PickLines.Count());
			AssertEquals(0, FindPickResult.ConcurrencyError.PickLines.Count());
		}

		#endregion

		#region TestTask

		public void TestTask()
		{
			var pick = Factory.New<WhsPick>();
			var pickLines = new[] { Factory.New<WhsPickLine>() };
			var task = Factory.New<WhsPickProcessTask>();

			var result1 = new FindPickResult(pick, task);
			AssertEquals(task, result1.Task);

			var result2 = new FindPickResult(pick, pickLines, task);
			AssertEquals(task, result2.Task);
		}

		#endregion
	}
}
