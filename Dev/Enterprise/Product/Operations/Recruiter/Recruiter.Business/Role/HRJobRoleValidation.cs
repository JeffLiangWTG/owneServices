using CargoWise.EntityFramework;

namespace Enterprise.Recruiter.Business
{
	public class HRJobRoleValidation : AutoHRJobRoleValidation
	{
		public HRJobRoleValidation(AutoHRJobRole parent) : base(parent)
		{
		}

		#region Description

		protected override void CheckHJ_JobRoleDescription()
		{
			base.CheckHJ_JobRoleDescription();
			MandatoryValidation.CheckEntered(Parent.HJ_JobRoleDescriptionInfo);
		}

		#endregion

		#region Full Description

		public void ValidateFullJobRoleDescription()
		{
			ValidateCalculatedProperty(Parent.FullJobRoleDescriptionInfo);
		}

		protected void CheckFullJobRoleDescription()
		{
			MandatoryValidation.CheckEntered(Parent.FullJobRoleDescriptionInfo, Res.GetString("49ae76c3-6435-4b8f-9b2b-47c3a4f190ee", "Full Job Role Description"));
		}

		#endregion

		#region Title

		protected override void CheckHJ_JobTitle()
		{
			base.CheckHJ_JobTitle();
			MandatoryValidation.CheckEntered(Parent.HJ_JobTitleInfo);
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateFullJobRoleDescription();
		}

		protected new HRJobRole Parent
		{
			get { return (HRJobRole)base.Parent; }
		}
	}
}
