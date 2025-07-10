//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefLanguageTypeValidation
//
//    This class should be used for overriding validation in AutoRefLanguageTypeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefLanguageTypeValidation : AutoRefLanguageTypeValidation
	{
		public RefLanguageTypeValidation(AutoRefLanguageType parent) : base(parent)
		{
		}
	}
}
