//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDtbConsignmentLodgementLookups
//
//    This class should be used for overriding collections in AutoDtbConsignmentLodgementLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentLodgementLookups : AutoDtbConsignmentLodgementLookups
	{
		public DtbConsignmentLodgementLookups(AutoDtbConsignmentLodgement parent) : base(parent)
		{
		}
	}
}
