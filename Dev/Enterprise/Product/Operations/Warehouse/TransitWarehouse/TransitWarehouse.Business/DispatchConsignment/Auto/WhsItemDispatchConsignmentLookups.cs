//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsTransitDispatchConsignmentLookups
//
//    This class should be used for overriding collections in AutoWhsTransitDispatchConsignmentLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemDispatchConsignmentLookups : AutoWhsItemDispatchConsignmentLookups
	{
		public WhsItemDispatchConsignmentLookups(AutoWhsItemDispatchConsignment parent) : base(parent)
		{
		}

		protected CodeDescriptionPairList Directions()
		{
			return new ConsignmentDirections();
		}
	}
}
