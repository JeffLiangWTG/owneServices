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

using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business.Declaration;

namespace Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet.Testing
{
	using Enterprise.Customs.NZ.Business.MAFeBACCa.Testing;

	internal class NZDocsMAFCoverSheetLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMAFOfficesList()
		{
			AssertEquals("CoverSheet.Lookups.MAFOfficesList", typeof(MAFOfficesList_DescriptionsOnly), CoverSheet.Lookups.MAFOfficesList.GetType());
		}

		NZDocsMAFCoverSheet CoverSheet
		{
			get { return fCoverSheet ?? (fCoverSheet = new NZDocsMAFCoverSheet(TestDataBuilder.GetMAFMessaging(Declaration))); }
		}
		NZDocsMAFCoverSheet fCoverSheet;

		JobDeclaration Declaration
		{
			get { return fDeclaration ?? (fDeclaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration fDeclaration;
	}
}
