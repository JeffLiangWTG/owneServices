//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusRateCodeLanguageValidation
//
//    This class should be used for overriding validation in AutoRefCusRateCodeLanguageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	//using CargoWise.ComponentModel;
	//using Enterprise.ZArchitecture.Business;

	public class RefCusRateCodeLanguageValidation : AutoRefCusRateCodeLanguageValidation
	{
		public RefCusRateCodeLanguageValidation(AutoRefCusRateCodeLanguage parent) : base(parent)
		{
		}
	}
}
