//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusVATApplicabilityValidation
//
//    This class should be used for overriding validation in AutoRefCusVATApplicabilityValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal.Internal
{
	public class RefCusVATApplicabilityValidation : AutoRefCusVATApplicabilityValidation
	{
		public RefCusVATApplicabilityValidation(AutoRefCusVATApplicability parent)
			: base(parent)
		{
		}
	}
}
