//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusProcedureLanguageValidation
//
//    This class should be used for overriding validation in AutoRefCusProcedureLanguageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusProcedureLanguageValidation : AutoRefCusProcedureLanguageValidation
	{
		public RefCusProcedureLanguageValidation(AutoRefCusProcedureLanguage parent) : base(parent)
		{
		}
	}
}
