//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZDocsMAFCSCommodityLookups
//
//    This class should be used for overriding collections in AutoNZDocsMAFCSCommodityLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet.Testing
{
	internal class NZDocsMAFCSCommodityLookupsTest : BusinessObjectLookupsTestCase
	{
	}
}
