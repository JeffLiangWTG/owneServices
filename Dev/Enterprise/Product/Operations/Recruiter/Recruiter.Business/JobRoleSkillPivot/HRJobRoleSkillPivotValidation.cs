using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public class HRJobRoleSkillPivotValidation : AutoHRJobRoleSkillPivotValidation
	{
		public HRJobRoleSkillPivotValidation(AutoHRJobRoleSkillPivot parent) : base(parent)
		{
		}

		public new HRJobRoleSkillPivot Parent
		{
			get { return (HRJobRoleSkillPivot)base.Parent; }
		}

		#region CheckH1_SkillsWeighting

		protected override void CheckH1_SkillsWeighting()
		{
			base.CheckH1_SkillsWeighting();
			MandatoryValidation.CheckNotNegative(Parent.H1_SkillsWeightingInfo);
			if (Parent.JobRole != null && Parent.JobRole.JobRoleSkills.SkillWeightingsAddTo != 100)
			{
				Parent.H1_SkillsWeightingInfo.AddError(Res.GetString("de4dc960-5cd1-4a46-b509-7dc1b8cdaac3", "Skill weightings must add to 100%"));
			}
		}

		#endregion

		#region Duplicate Skill Pivot Exists

		protected override void CheckH1_HS()
		{
			base.CheckH1_HS();
			if (DuplicateRoleSkillPivotExists)
			{
				Parent.H1_HSInfo.AddError(Res.GetString("0779ae5b-738a-40ab-bab6-258eed3323b7", "This skill already exists on this Job Role"));
			}
		}

		bool DuplicateRoleSkillPivotExists
		{
			get
			{
				if (Parent.H1_HJ.IsValid && Parent.H1_HS.IsValid)
				{
					ZQuery duplicateRoleSkillFilter = new ZQuery(HRJobRoleSkillPivotSchema.H1_HJ, Parent.H1_HJ);
					duplicateRoleSkillFilter.AddToFilter(HRJobRoleSkillPivotSchema.H1_HS, Parent.H1_HS);
					duplicateRoleSkillFilter.AddToFilter(HRJobRoleSkillPivotSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
					var duplicateRoleSkills = Parent.Factory.Load<HRJobRoleSkillPivot>(duplicateRoleSkillFilter);

					return duplicateRoleSkills.Length != 0;
				}

				return false;
			}
		}

		#endregion

	}
}

