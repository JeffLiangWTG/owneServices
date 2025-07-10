//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbAccreditationJobSkillPivotValidation
//
//    This class should be used for overriding validation in AutoGlbAccreditationJobSkillPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;

namespace Enterprise.Recruiter.Business
{
	public class GlbAccreditationJobSkillPivotValidation : AutoGlbAccreditationJobSkillPivotValidation
	{
		public GlbAccreditationJobSkillPivotValidation(AutoGlbAccreditationJobSkillPivot parent) : base(parent)
		{
		}

		protected new GlbAccreditationJobSkillPivot Parent
		{
			get { return (GlbAccreditationJobSkillPivot)base.Parent; }
		}

		static string SameSkillError
		{
			get { return Res.GetString("B4175638-903A-490C-B4FC-7E401BFDEB7B", "Cannot add same Skill twice."); }
		}

		protected override void CheckHAJ_HJG()
		{
			base.CheckHAJ_HJG();
			if (Parent == null || Parent.JobSkillGroup == null)
			{
				return;
			}

			if (Parent.JobSkillGroup.SkillPivots.Cast<GlbAccreditationJobSkillPivot>().Any(p => p.PK != Parent.PK && p.HAJ_HS == Parent.HAJ_HS))
			{
				Parent.HAJ_HJGInfo.AddError(SameSkillError);
			}
		}

		protected override void CheckHAJ_HS()
		{
			base.CheckHAJ_HS();
			if (Parent == null || Parent.JobSkillGroup == null)
			{
				return;
			}

			if (Parent.JobSkillGroup.SkillPivots.Cast<GlbAccreditationJobSkillPivot>().Any(p => p.PK != Parent.PK && p.HAJ_HS == Parent.HAJ_HS))
			{
				Parent.HAJ_HSInfo.AddError(SameSkillError);
			}
		}
	}
}
