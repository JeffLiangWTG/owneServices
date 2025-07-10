using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.GUI.OnlineSailingSchedules.Testing
{
	public class ModuleFilterOnlineSchedulesDateValidationTest : TestCaseWithFactory
	{
		public void TestModuleDateFilter_PastDates_ValidationError()
		{
			var filter = new OnlineSchedulesModuleDateFilter("moo", DummyBizoSchema.Z0_Date);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertEquals(true, GlbStaff.CurrentUser.IsSupportUser);
			filter.Property1 = ZDateTime.Today.AddDays(-1);
			AssertNoErrors(filter.Property1Info);

			var loginName = GlbStaff.CurrentUser.GS_LoginName;

			try
			{
				GlbStaff.CurrentUser.GS_LoginName = "BOB";
				AssertEquals(false, GlbStaff.CurrentUser.IsSupportUser);

				filter.Property1 = ZDateTime.Today.AddDays(-2);
				AssertHasError(filter.Property1Info, "Schedules can be searched by current or future dates only.");

				filter.Property2 = ZDateTime.Today.AddDays(-1);
				AssertHasError(filter.Property2Info, "Schedules can be searched by current or future dates only.");
			}
			finally
			{
				GlbStaff.CurrentUser.GS_LoginName = loginName;
			}
		}

		public void TestModuleDateFilter_CurrentOrEmptyDates_NoValidationError()
		{
			var filter = new OnlineSchedulesModuleDateFilter("moo", DummyBizoSchema.Z0_Date);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertEquals(true, GlbStaff.CurrentUser.IsSupportUser);

			var loginName = GlbStaff.CurrentUser.GS_LoginName;

			try
			{
				GlbStaff.CurrentUser.GS_LoginName = "BOB";
				AssertEquals(false, GlbStaff.CurrentUser.IsSupportUser);

				filter.Property1 = ZDateTime.Today;
				AssertNoErrors(filter.Property1Info);

				filter.Property2 = ZDateTime.Today;
				AssertNoErrors(filter.Property2Info);

				filter.Property1 = ZDateTime.Empty;
				AssertNoErrors(filter.Property1Info);

				filter.Property2 = ZDateTime.Empty;
				AssertNoErrors(filter.Property2Info);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_LoginName = loginName;
			}
		}
	}
}
