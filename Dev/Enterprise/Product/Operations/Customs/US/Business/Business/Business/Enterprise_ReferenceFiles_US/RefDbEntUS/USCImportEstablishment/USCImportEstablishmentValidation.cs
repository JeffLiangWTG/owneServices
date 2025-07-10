//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCImportEstablishmentValidation
//
//    This class should be used for overriding validation in AutoUSCImportEstablishmentValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business.RefDbEntUS
{
	public class USCImportEstablishmentValidation : AutoUSCImportEstablishmentValidation
	{
		public USCImportEstablishmentValidation(AutoUSCImportEstablishment parent)
			: base(parent)
		{
		}
	}
}
