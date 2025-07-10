//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCImportEstablishmentAlternateNameValidation
//
//    This class should be used for overriding validation in AutoUSCImportEstablishmentAlternateNameValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business.RefDbEntUS
{
	public class USCImportEstablishmentAlternateNameValidation : AutoUSCImportEstablishmentAlternateNameValidation
	{
		public USCImportEstablishmentAlternateNameValidation(AutoUSCImportEstablishmentAlternateName parent)
			: base(parent)
		{
		}
	}
}
