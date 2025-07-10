//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZCConcessionDutyRateLookups
//
//    This class should be used for overriding collections in AutoNZCConcessionDutyRateLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCConcessionDutyRateLookups : AutoNZCConcessionDutyRateLookups
	{
		public NZCConcessionDutyRateLookups(AutoNZCConcessionDutyRate parent)
			: base(parent)
		{
		}
	}
}
