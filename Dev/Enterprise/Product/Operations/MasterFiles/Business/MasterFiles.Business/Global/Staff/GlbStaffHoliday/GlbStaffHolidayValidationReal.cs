using CargoWise.EntityFramework;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffHolidayValidationReal : GlbStaffHolidayValidation
	{
		public GlbStaffHolidayValidationReal(AutoGlbStaffHoliday parent)
			: base(parent)
		{
		}

		public new GlbStaffHoliday Parent
		{
			get { return (GlbStaffHoliday)base.Parent; }
		}

		#region GA_DaysLeaveTaken

		protected override void CheckGA_DaysLeaveTaken()
		{
			base.CheckGA_DaysLeaveTaken();

			if (SystemDataRegistry.Instance.EnableValidateLeaveOnStaffEdit.Value)
			{
				MandatoryValidation.CheckEntered(Parent.GA_DaysLeaveTakenInfo);
			}
		}

		#endregion

		#region GA_ApprovalStatus

		protected override void CheckGA_ApprovalStatus()
		{
			base.CheckGA_ApprovalStatus();
			MandatoryValidation.CheckEntered(Parent.GA_ApprovalStatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.GA_ApprovalStatusInfo);
		}

		#endregion

		#region GA_AvailabilityPercentage

		protected override void CheckGA_AvailabilityPercentage()
		{
			base.CheckGA_AvailabilityPercentage();
			CompareValidation.CheckWithinRange(Parent.GA_AvailabilityPercentageInfo, 0, 100);
		}

		#endregion
	}
}
