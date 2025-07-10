//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusRefApplicabilityValidation
//
//    This class should be used for overriding validation in AutoCusRefApplicabilityValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusRefApplicabilityValidation : AutoCusRefApplicabilityValidation
	{
		public CusRefApplicabilityValidation(AutoCusRefApplicability parent) : base(parent)
		{
		}
	}
}
