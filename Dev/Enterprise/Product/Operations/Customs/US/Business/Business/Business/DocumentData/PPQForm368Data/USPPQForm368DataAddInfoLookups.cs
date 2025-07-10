//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSPPQForm368DataAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSPPQForm368DataAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USPPQForm368DataAddInfoLookups : AutoUSPPQForm368DataAddInfoLookups
	{
		public USPPQForm368DataAddInfoLookups(AutoUSPPQForm368DataAddInfo parent)
			: base(parent)
		{
		}
	}
}
