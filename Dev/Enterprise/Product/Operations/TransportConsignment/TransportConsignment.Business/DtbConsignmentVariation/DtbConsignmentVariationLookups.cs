//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDtbConsignmentVariationLookups
//
//    This class should be used for overriding collections in AutoDtbConsignmentVariationLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentVariationLookups : AutoDtbConsignmentVariationLookups
	{
		public DtbConsignmentVariationLookups(AutoDtbConsignmentVariation parent) : base(parent)
		{
		}

		public DtbConsignmentVariationStatuses Statuses => new();

		public DtbConsignmentVariationTypes Types => new();
	}
}
