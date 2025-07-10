//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsItemReceiveConsignmentLookups
//
//    This class should be used for overriding collections in AutoWhsItemReceiveConsignmentLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemReceiveConsignmentLookups : AutoWhsItemReceiveConsignmentLookups
	{
		public WhsItemReceiveConsignmentLookups(AutoWhsItemReceiveConsignment parent) : base(parent)
		{
		}

		public CodeDescriptionPairList Directions()
		{
			return new ConsignmentDirections();
		}
	}
}

