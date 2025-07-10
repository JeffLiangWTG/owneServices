//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCZipCodeValidation
//
//    This class should be used for overriding validation in AutoUSCZipCodeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCZipCodeValidation : AutoUSCZipCodeValidation
	{
		public USCZipCodeValidation(AutoUSCZipCode parent)
			: base(parent)
		{
		}
	}
}
