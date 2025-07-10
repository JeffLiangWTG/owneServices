//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDtbBookingConsolidationLookups
//
//    This class should be used for overriding collections in AutoDtbBookingConsolidationLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.TransportCommon.Business.Common
{
	public class DtbBookingConsolidationLookups : AutoDtbBookingConsolidationLookups
	{
		internal DtbBookingConsolidationLookups(AutoDtbBookingConsolidation parent)
			: base(parent)
		{
		}
	}
}

