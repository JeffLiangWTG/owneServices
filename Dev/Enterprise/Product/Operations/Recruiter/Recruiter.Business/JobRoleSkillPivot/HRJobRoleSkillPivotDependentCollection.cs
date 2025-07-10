using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Recruiter.Business
{
	public class HRJobRoleSkillPivotDependentCollection : DependentBusinessObjectCollection<HRJobRoleSkillPivot, BusinessObject>
	{
		public HRJobRoleSkillPivotDependentCollection(HRJobRole parent) : base(parent)
		{
		}

		#region SetDefaultsForNewChild

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			int total = 100 - SkillWeightingsAddTo;
			using (child.GetValidationSuspender())
			{
				((HRJobRoleSkillPivot)child).H1_SkillsWeighting = (ZByte)((total < 0 || total > 255) ? 0 : total);
			}
		}

		#endregion

		#region Skill Weightings

		public int SkillWeightingsAddTo
		{
			get
			{
				int total = 0;
				foreach (HRJobRoleSkillPivot pivot in this)
				{
					total += pivot.H1_SkillsWeighting;
				}

				return total;
			}
		}

		#endregion
	}
}

