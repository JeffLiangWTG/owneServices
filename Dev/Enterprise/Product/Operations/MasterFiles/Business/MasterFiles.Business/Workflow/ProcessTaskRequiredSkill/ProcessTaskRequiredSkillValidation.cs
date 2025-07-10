//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoProcessTaskRequiredSkillValidation
//
//    This class should be used for overriding validation in AutoProcessTaskRequiredSkillValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskRequiredSkillValidation : AutoProcessTaskRequiredSkillValidation
	{
		public ProcessTaskRequiredSkillValidation(AutoProcessTaskRequiredSkill parent) : base(parent)
		{
		}
	}
}
