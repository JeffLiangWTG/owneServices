//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusMapTypeLookups
//
//    This class should be used for overriding collections in AutoRefCusMapTypeLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusMapTypeLookups : AutoRefCusMapTypeLookups
	{
		public RefCusMapTypeLookups(AutoRefCusMapType parent) : base(parent)
		{
		}

		public MapDirectionList MapDirectionList
		{
			get { return Factory.GetCachedValue<MapDirectionList>(); }
		}
	}
}
