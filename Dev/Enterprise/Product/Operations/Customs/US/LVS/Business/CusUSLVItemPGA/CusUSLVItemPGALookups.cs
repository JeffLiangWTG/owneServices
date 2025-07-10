//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusUSLVItemPGALookups
//
//    This class should be used for overriding collections in AutoCusUSLVItemPGALookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.LVS.Business
{
	public class CusUSLVItemPGALookups : AutoCusUSLVItemPGALookups
	{
		public CusUSLVItemPGALookups(AutoCusUSLVItemPGA parent) : base(parent)
		{
		}
	}
}
