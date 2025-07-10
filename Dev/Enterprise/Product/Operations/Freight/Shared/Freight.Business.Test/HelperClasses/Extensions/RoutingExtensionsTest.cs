using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Business.Testing
{
	sealed class RoutingExtensionsTest : TestCaseWithFactory
	{
		public void TestIsValidToTryMatch_InvalidETD()
		{
			var scheduleToMatch = new ScheduleInfo("QF", 1234, "SYD", ZDate.Invalid, "JFK", ZDate.Today.AddDays(1));
			AssertEquals("IsValidToTryMatch should return false when ETD is invalid", false, scheduleToMatch.IsValidToTryMatch());
		}

		public void TestIsValidToTryMatch_InvalidETA()
		{
			var scheduleToMatch = new ScheduleInfo("QF", 1234, "SYD", ZDate.Today, "JFK", ZDate.Invalid);
			AssertEquals("IsValidToTryMatch should return false when ETA is invalid", false, scheduleToMatch.IsValidToTryMatch());
		}

		public void TestIsValidToTryMatch_ValidDates()
		{
			var scheduleToMatch = new ScheduleInfo("QF", 1234, "SYD", ZDate.Today, "JFK", ZDate.Today.AddDays(1));
			AssertEquals("IsValidToTryMatch should return true when ETD and ETA are valid", true, scheduleToMatch.IsValidToTryMatch());
		}

		public void TestIsValidToTryMatch_PastETD()
		{
			var scheduleToMatch = new ScheduleInfo("QF", 1234, "SYD", ZDate.Today.AddDays(-1), "JFK", ZDate.Invalid);
			AssertEquals("IsValidToTryMatch should return false when ETD and ETA are past date", false, scheduleToMatch.IsValidToTryMatch());
		}

		public void TestIsValidToTryMatch_PastETA()
		{
			var scheduleToMatch = new ScheduleInfo("QF", 1234, "SYD", ZDate.Invalid, "JFK", ZDate.Today.AddDays(-1));
			AssertEquals("IsValidToTryMatch should return false when ETD and ETA are past date", false, scheduleToMatch.IsValidToTryMatch());
		}
	}
}
