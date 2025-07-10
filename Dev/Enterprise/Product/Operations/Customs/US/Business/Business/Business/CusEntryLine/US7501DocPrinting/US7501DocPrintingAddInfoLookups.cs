//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUS7501DocPrintingAddInfoLookups
//
//    This class should be used for overriding collections in AutoUS7501DocPrintingAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class US7501DocPrintingAddInfoLookups : AutoUS7501DocPrintingAddInfoLookups
	{
		public US7501DocPrintingAddInfoLookups(AutoUS7501DocPrintingAddInfo parent) : base(parent)
		{
		}
	}
}
