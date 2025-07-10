//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZCCountryGroupValidation
//
//    This class should be used for overriding validation in AutoNZCCountryGroupValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCCountryGroupValidation : AutoNZCCountryGroupValidation
	{
		public NZCCountryGroupValidation(AutoNZCCountryGroup parent)
			: base(parent)
		{
		}
	}
}
