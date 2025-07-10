//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZDocsMAFCoverSheetLookups
//
//    This class should be used for overriding collections in AutoNZDocsMAFCoverSheetLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet
{
	public class NZDocsMAFCoverSheetLookups : AutoNZDocsMAFCoverSheetLookups
	{
		public NZDocsMAFCoverSheetLookups(AutoNZDocsMAFCoverSheet parent)
			: base(parent)
		{
		}

		public MAFOfficesList_DescriptionsOnly MAFOfficesList
		{
			get { return new MAFOfficesList_DescriptionsOnly(); }
		}
	}
}
