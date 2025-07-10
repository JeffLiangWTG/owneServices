//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZCConcessionValidation
//
//    This class should be used for overriding validation in AutoNZCConcessionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCConcessionValidation : AutoNZCConcessionValidation
	{
		public NZCConcessionValidation(AutoNZCConcession parent)
			: base(parent)
		{
		}
	}
}
