//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusTRPreviousDocumentItemLookups
//
//    This class should be used for overriding collections in AutoCusTRPreviousDocumentItemLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class CusTRPreviousDocumentItemLookups : AutoCusTRPreviousDocumentItemLookups
	{
		public CusTRPreviousDocumentItemLookups(AutoCusTRPreviousDocumentItem parent) : base(parent)
		{
		}
	}
}

