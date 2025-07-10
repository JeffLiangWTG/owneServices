//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusNomenclatureLanguageValidation
//
//    This class should be used for overriding validation in AutoRefCusNomenclatureLanguageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusNomenclatureLanguageValidation : AutoRefCusNomenclatureLanguageValidation
	{
		public RefCusNomenclatureLanguageValidation(AutoRefCusNomenclatureLanguage parent) : base(parent)
		{
		}
	}
}
