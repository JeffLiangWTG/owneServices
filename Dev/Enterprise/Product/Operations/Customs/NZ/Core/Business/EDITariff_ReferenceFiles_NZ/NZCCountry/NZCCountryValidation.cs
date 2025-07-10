//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZCCountryValidation
//
//    This class should be used for overriding validation in AutoNZCCountryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCCountryValidation : AutoNZCCountryValidation
	{
		public NZCCountryValidation(AutoNZCCountry parent)
			: base(parent)
		{
		}
	}
}
