//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoVATApplicabilityViewValidation
//
//    This class should be used for overriding validation in AutoVATApplicabilityViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class VATApplicabilityViewValidation : AutoVATApplicabilityViewValidation
	{
		public VATApplicabilityViewValidation(AutoVATApplicabilityView parent) : base(parent)
		{
		}
	}
}
