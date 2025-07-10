//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCCountryValidation
//
//    This class should be used for overriding validation in AutoUSCCountryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCCountryValidation : AutoUSCCountryValidation
	{
		public USCCountryValidation(AutoUSCCountry parent)
			: base(parent)
		{
		}
	}
}
