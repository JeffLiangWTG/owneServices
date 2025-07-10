//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbAccreditationRequirementPivotValidation
//
//    This class should be used for overriding validation in AutoGlbAccreditationRequirementPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Recruiter.Business
{
	public class GlbAccreditationRequirementPivotValidation : AutoGlbAccreditationRequirementPivotValidation
	{
		public GlbAccreditationRequirementPivotValidation(AutoGlbAccreditationRequirementPivot parent) : base(parent)
		{
		}

		protected override void CheckHAR_HAC()
		{
			base.CheckHAR_HAC();
			if (Parent.HAR_HAC == Parent.HAR_HAC_Parent)
			{
				Parent.HAR_HACInfo.AddError(ErrorItselfAsParent);
			}
		}

		static string ErrorItselfAsParent
		{
			get
			{
				return Res.GetString("6EA33439-BA96-4BAA-AB72-DA9F0A8DB77C", "Cannot have itself as a requirement");
			}
		}

		protected override void CheckHAR_HAC_Parent()
		{
			base.CheckHAR_HAC_Parent();
			if (Parent.HAR_HAC == Parent.HAR_HAC_Parent)
			{
				Parent.HAR_HAC_ParentInfo.AddError(ErrorItselfAsParent);
			}
		}
	}
}
