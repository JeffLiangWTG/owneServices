using System;
using CargoWise.Types;
using Enterprise.HRM.Common;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffHoliday))]
	public class GlbStaffHolidayTest : GlbStaffResourceTimeTest
	{
		public void TestValidation()
		{
			GlbStaffHoliday holiday = Factory.New<GlbStaffHoliday>();
			AssertEquals(typeof(GlbStaffHolidayValidationReal), holiday.Validation.GetType());
		}

		public void TestLookups()
		{
			GlbStaffHoliday holiday = Factory.New<GlbStaffHoliday>();
			AssertEquals(typeof(GlbStaffHolidayLookupsReal), holiday.Lookups.GetType());
		}

		public void TestDefaultValues()
		{
			GlbStaffHoliday holiday = GlbStaff.CurrentUser.Holidays.AddNew();
			AssertEquals(GlbStaffHolidayLookupsReal.Requested, holiday.GA_ApprovalStatus);
			AssertEquals("Requested", holiday.ApprovalStatusDescription);
		}

		public void TestIsApproved()
		{
			GlbStaffHoliday holiday = GlbStaff.CurrentUser.Holidays.AddNew();
			holiday.GA_ApprovalStatus = "AAA";
			Assert(!holiday.IsApproved);

			GlbStaffHoliday holiday2 = GlbStaff.CurrentUser.Holidays.AddNew();
			holiday2.GA_ApprovalStatus = "APP";
			Assert(holiday2.IsApproved);

			GlbStaffHoliday holiday3 = GlbStaff.CurrentUser.Holidays.AddNew();
			holiday3.GA_ApprovalStatus = "APC";
			Assert(holiday3.IsApproved);

			GlbStaffHoliday holiday4 = GlbStaff.CurrentUser.Holidays.AddNew();
			holiday4.GA_ApprovalStatus = "ZZZ";
			Assert(!holiday4.IsApproved);
		}

		public void TestIsWorkingAway()
		{
			var registryItem = SystemDataRegistry.Instance.StaffLeaveTypes.Value;
			registryItem.Add("BEN", (NoResString)"International benedictday", true);

			SystemDataRegistry.Instance.StaffLeaveTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryItem);

			var holiday = GlbStaff.CurrentUser.Holidays.AddNew();
			holiday.GA_WorkHolidayType = "ANN";
			AssertEquals(false, holiday.GA_IsWorkingAway);
			holiday.GA_WorkHolidayType = "BEN";
			AssertEquals(true, holiday.GA_IsWorkingAway);
			holiday.GA_WorkHolidayType = "ANN";
			AssertEquals(false, holiday.GA_IsWorkingAway);
			holiday.GA_IsWorkingAway = true;
			AssertEquals(true, holiday.GA_IsWorkingAway);
		}

		public void TestAuditColumns()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var holiday = staff.Holidays.AddNew();
			holiday.FillWithValidTestData();
			AssertEquals("", holiday.GA_SystemCreateUser);
			AssertEquals(ZDateTime.Empty, holiday.GA_SystemCreateTimeUtc);
			AssertEquals("", holiday.GA_SystemLastEditUser);
			AssertEquals(ZDateTime.Empty, holiday.GA_SystemLastEditTimeUtc);

			Factory.Save();

			AssertEquals(GlbStaff.CurrentUser.GS_Code, holiday.GA_SystemCreateUser);
			Assert(!holiday.GA_SystemCreateTimeUtc.IsEmpty);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, holiday.GA_SystemLastEditUser);
			Assert(!holiday.GA_SystemLastEditTimeUtc.IsEmpty);
		}

		public void TestReadOnly_True()
		{
			AssertReadOnlyCase(true, true);
		}

		public void TestReadOnly_False()
		{
			AssertReadOnlyCase(false, false);
		}

		public void AssertReadOnlyCase(bool hasPolicy, bool readOnly)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var holiday = staff.Holidays.AddNew();
			holiday.FillWithValidTestData();
			if (hasPolicy)
			{
				var policy = Factory.NewWithValidTestData<HrlStaffPolicy>();
				policy.LLS_GS_Staff = staff.PK;
			}
			Factory.Save();

			var factory2 = NewFactory();
			var holiday2 = factory2.Load<GlbStaffHoliday>(holiday.PK);

			AssertEquals(readOnly, holiday2.ReadOnly);
		}
	}
}
