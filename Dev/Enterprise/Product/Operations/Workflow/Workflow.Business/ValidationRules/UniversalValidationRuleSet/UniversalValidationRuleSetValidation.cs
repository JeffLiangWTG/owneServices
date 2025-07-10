//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUniversalValidationRuleSetValidation
//
//    This class should be used for overriding validation in AutoUniversalValidationRuleSetValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Workflow.Business
{
	public class UniversalValidationRuleSetValidation : AutoUniversalValidationRuleSetValidation
	{
		public UniversalValidationRuleSetValidation(AutoUniversalValidationRuleSet parent) : base(parent)
		{
		}

		protected override void CheckVRS_Name()
		{
			MandatoryValidation.CheckEntered(Parent.VRS_NameInfo);
		}

		protected override void CheckVRS_Description()
		{
			MandatoryValidation.CheckEntered(Parent.VRS_DescriptionInfo);
		}

		protected override void CheckVRS_DataContext()
		{
			var info = Parent.VRS_DataContextInfo;
			MandatoryValidation.CheckEntered(info);
			ListValidation.ErrorIfInvalidCode(info);
		}
	}
}
