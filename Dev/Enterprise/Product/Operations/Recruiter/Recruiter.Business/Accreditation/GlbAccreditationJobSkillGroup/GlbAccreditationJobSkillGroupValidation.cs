//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbAccreditationJobSkillGroupValidation
//
//    This class should be used for overriding validation in AutoGlbAccreditationJobSkillGroupValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Recruiter.Business
{
	public class GlbAccreditationJobSkillGroupValidation : AutoGlbAccreditationJobSkillGroupValidation
	{
		public GlbAccreditationJobSkillGroupValidation(AutoGlbAccreditationJobSkillGroup parent) : base(parent)
		{
		}

		public new GlbAccreditationJobSkillGroup Parent
		{
			get { return (GlbAccreditationJobSkillGroup)base.Parent; }
		}

		protected override void CheckHJG_Description()
		{
			base.CheckHJG_Description();
			MandatoryValidation.CheckEntered(Parent.HJG_DescriptionInfo);
		}

		protected override void CheckHJG_Threshold()
		{
			base.CheckHJG_Threshold();

			if (Parent.SkillPivots.Count + Parent.Groups.Count < Parent.HJG_Threshold)
			{
				Parent.HJG_ThresholdInfo.AddWarning(Res.GetString("1A26158C-3F5A-4DD4-B247-F6BDAAFB10E6", "The Completion Minimum is greater than the number of Job Skills."));
			}

			if (Parent.SkillPivots.Count > 0 && Parent.HJG_Threshold <= 0)
			{
				MandatoryValidation.CheckNotZero(Parent.HJG_ThresholdInfo);
				MandatoryValidation.CheckNotNegative(Parent.HJG_ThresholdInfo);
			}
		}
	}
}
