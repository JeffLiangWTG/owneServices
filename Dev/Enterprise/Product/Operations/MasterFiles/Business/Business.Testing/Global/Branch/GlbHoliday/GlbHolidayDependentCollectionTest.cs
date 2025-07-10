using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbHolidayDependentCollection))]
	sealed class GlbHolidayDependentCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbHolidayDependentCollection>
	{
		public void TestContainsDate()
		{
			GlbBranch branch = Factory.New<GlbBranch>();
			GlbHoliday holiday1 = branch.GlbHolidays.AddNew();
			GlbHoliday holiday2 = branch.GlbHolidays.AddNew();

			holiday1.GH_Date = new ZDateTime(2006, 5, 28);

			holiday2.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.First;
			holiday2.GH_RecurrDay = DayOfWeekCodeList.Codes.Monday;
			holiday2.GH_RecurrMonth = "MAR";

			AssertEquals("When doesn't match", false, branch.GlbHolidays.Contains(new ZDate(2006, 5, 27)));
			AssertEquals("First date match", true, branch.GlbHolidays.Contains(new ZDate(2006, 5, 28)));
			AssertEquals("First monday of month", true, branch.GlbHolidays.Contains(new ZDate(2006, 3, 6)));
		}

		protected override GlbHolidayDependentCollection GetCollectionToTest()
		{
			GlbBranch branch = Factory.New<GlbBranch>();
			return new GlbHolidayDependentCollection(branch, Factory);
		}
	}
}
