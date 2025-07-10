using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Recruiter.Module
{
	public class GlbAccreditationLatestAttemptForGroupFilterValidation : ModuleTextFilterValidation
	{
		public GlbAccreditationLatestAttemptForGroupFilterValidation(GlbAccreditationHighestLevelByProgramFilter parent) : base(parent)
		{
		}

		protected new GlbAccreditationHighestLevelByProgramFilter Parent => (GlbAccreditationHighestLevelByProgramFilter)base.Parent;

		#region Validate AccreditationGroupDescription

		public void ValidateAccreditationGroupDescription()
		{
			ValidateCalculatedProperty(Parent.AccreditationGroupDescriptionInfo);
		}

		protected virtual void CheckAccreditationGroupDescription()
		{
			ListValidation.ErrorIfInvalidCode(Parent.AccreditationGroupDescriptionInfo);
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAccreditationGroupDescription();
		}
	}
}
