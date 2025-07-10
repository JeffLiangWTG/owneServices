//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUniversalValidationRuleValidation
//
//    This class should be used for overriding validation in AutoUniversalValidationRuleValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Workflow.Business
{
	public class UniversalValidationRuleValidation : AutoUniversalValidationRuleValidation
	{
		public UniversalValidationRuleValidation(AutoUniversalValidationRule parent) : base(parent)
		{
		}

		protected override void CheckVR_Status()
		{
			var info = Parent.VR_StatusInfo;
			MandatoryValidation.CheckEntered(info);
			ListValidation.ErrorIfInvalidCode(info);
		}

		protected override void CheckVR_BusinessRule()
		{
			var info = Parent.VR_BusinessRuleInfo;
			MandatoryValidation.CheckEntered(info);
		}
	}
}
