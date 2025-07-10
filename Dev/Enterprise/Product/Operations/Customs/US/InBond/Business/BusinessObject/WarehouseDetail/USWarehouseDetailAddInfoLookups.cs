//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSWarehouseDetailAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSWarehouseDetailAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.InBond.Business
{
	public class USWarehouseDetailAddInfoLookups : AutoUSWarehouseDetailAddInfoLookups
	{
		public USWarehouseDetailAddInfoLookups(AutoUSWarehouseDetailAddInfo parent) : base(parent)
		{
		}
	}
}
