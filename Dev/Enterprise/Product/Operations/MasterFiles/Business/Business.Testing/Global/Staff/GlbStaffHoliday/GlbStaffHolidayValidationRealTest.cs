using System;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbStaffHolidayValidationRealTest : GlbStaffHolidayValidationTest
	{
		public void TestApprovalStatus()
		{
			Holiday.GA_ApprovalStatus = "XXX";
			AssertHasErrors(Holiday.GA_ApprovalStatusInfo);

			Holiday.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;
			AssertNoErrors(Holiday.GA_ApprovalStatusInfo);

			Holiday.GA_ApprovalStatus = "";
			AssertHasErrors(Holiday.GA_ApprovalStatusInfo);
		}

		public void TestDaysLeaveTaken()
		{
			Holiday.GA_DaysLeaveTaken = 3.5m;
			AssertNoErrors(Holiday.GA_DaysLeaveTakenInfo);
		}

		public void TestDaysLeaveTaken_IfEnableValidateLeaveOnStaffEditIsEnabled()
		{
			using (SystemDataRegistry.Instance.EnableValidateLeaveOnStaffEdit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("EnableValidateLeaveOnStaffEdit is Enabled", true, SystemDataRegistry.Instance.EnableValidateLeaveOnStaffEdit.Value);

				Holiday.GA_DaysLeaveTaken = 0m;
				AssertHasErrors(Holiday.GA_DaysLeaveTakenInfo);
			}
		}

		public void TestDaysLeaveTaken_IfEnableValidateLeaveOnStaffEditIsDisabled()
		{
			using (SystemDataRegistry.Instance.EnableValidateLeaveOnStaffEdit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("EnableValidateLeaveOnStaffEdit is Disabled", false, SystemDataRegistry.Instance.EnableValidateLeaveOnStaffEdit.Value);

				Holiday.GA_DaysLeaveTaken = 0m;
				AssertNoErrors(Holiday.GA_DaysLeaveTakenInfo);
			}
		}

		public void TestAvailabilityPercentage()
		{
			Holiday.GA_AvailabilityPercentage = 101;
			AssertHasErrors(Holiday.GA_AvailabilityPercentageInfo);

			Holiday.GA_AvailabilityPercentage = 100;
			AssertNoErrors(Holiday.GA_AvailabilityPercentageInfo);
		}
	}
}
