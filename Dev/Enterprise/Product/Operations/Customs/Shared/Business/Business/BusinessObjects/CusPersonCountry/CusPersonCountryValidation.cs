//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusPersonCountryValidation
//
//    This class should be used for overriding validation in AutoCusPersonCountryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusPersonCountryValidation : AutoCusPersonCountryValidation
	{
		public CusPersonCountryValidation(AutoCusPersonCountry parent) : base(parent)
		{
		}
	}
}
