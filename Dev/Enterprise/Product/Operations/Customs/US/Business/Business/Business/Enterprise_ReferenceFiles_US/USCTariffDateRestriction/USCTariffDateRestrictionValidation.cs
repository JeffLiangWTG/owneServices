//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCTariffDateRestrictionValidation
//
//    This class should be used for overriding validation in AutoUSCTariffDateRestrictionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCTariffDateRestrictionValidation : AutoUSCTariffDateRestrictionValidation
	{
		public USCTariffDateRestrictionValidation(AutoUSCTariffDateRestriction parent)
			: base(parent)
		{
		}
	}
}
