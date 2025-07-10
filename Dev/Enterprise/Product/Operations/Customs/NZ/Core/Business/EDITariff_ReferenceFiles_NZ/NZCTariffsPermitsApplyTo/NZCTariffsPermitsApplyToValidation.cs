//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZCTariffsPermitsApplyToValidation
//
//    This class should be used for overriding validation in AutoNZCTariffsPermitsApplyToValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCTariffsPermitsApplyToValidation : AutoNZCTariffsPermitsApplyToValidation
	{
		public NZCTariffsPermitsApplyToValidation(AutoNZCTariffsPermitsApplyTo parent)
			: base(parent)
		{
		}
	}
}
