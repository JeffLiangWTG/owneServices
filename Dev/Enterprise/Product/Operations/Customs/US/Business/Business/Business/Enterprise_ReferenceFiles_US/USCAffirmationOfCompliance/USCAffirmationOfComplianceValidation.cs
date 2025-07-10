//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCAffirmationOfComplianceValidation
//
//    This class should be used for overriding validation in AutoUSCAffirmationOfComplianceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCAffirmationOfComplianceValidation : AutoUSCAffirmationOfComplianceValidation
	{
		public USCAffirmationOfComplianceValidation(AutoUSCAffirmationOfCompliance parent)
			: base(parent)
		{
		}
	}
}
