using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ExceptionValidation : ProcessTaskValidationBase
	{
		public ExceptionValidation(ProcessTask parent)
			: base(parent)
		{
		}

		protected override void CheckP9_GS_NKAssignedStaffMember()
		{
			base.CheckP9_GS_NKAssignedStaffMember();
			ListValidation.ErrorIfInvalidCode(Parent.P9_GS_NKAssignedStaffMemberInfo);
		}

		static string DescriptionWarningMessage => Res.GetString("F7D82A28-EBF1-48EB-BAB3-27308646AC6D", "Modifying the description will break the link between the Milestone and the Exception. This may prevent the Exception from being actioned automatically and could lead to incorrect information appearing in Exception reports.");

		protected override void CheckP9_Description()
		{
			base.CheckP9_Description();
			var info = Parent.P9_DescriptionInfo;

			if (info == null || !info.HasChanges)
			{
				return;
			}

			var milestone = Parent.GetExceptionMilestone(info.OriginalValue);

			if (milestone != null)
			{
				Parent.P9_DescriptionInfo.AddWarning(DescriptionWarningMessage);
			}
		}
	}
}
