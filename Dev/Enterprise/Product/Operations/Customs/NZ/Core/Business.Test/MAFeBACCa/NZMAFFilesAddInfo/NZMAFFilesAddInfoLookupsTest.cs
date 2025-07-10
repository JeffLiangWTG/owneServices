//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZMAFFilesAddInfoLookups
//
//    This class should be used for overriding collections in AutoNZMAFFilesAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.Testing
{
	using System.Linq;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Integration;
	using CargoWise.Types;
	using Enterprise.Customs.Business.Testing;
	using Enterprise.Customs.NZ.Business.Declaration;

	internal class NZMAFFilesAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDocumentTypes()
		{
			var dummyBO = Factory.New<DummyBusinessObjectWithIAddInfoManager>();
			var file = new MAFFile(dummyBO.Z0_VarCharMaxInfo);
			AssertNotNull(file.Lookups.DocumentTypes);
		}

		public void TestNoExceptionWhenDeclarationNotFound()
		{
			var dummyBO = Factory.New<DummyBusinessObjectWithIAddInfoManager>();
			var file = new MAFFile(dummyBO.Z0_VarCharMaxInfo);
			var list = file.Lookups.AvailableEDocs;

			AssertNotNull("Lookups.AvailableEDocs", list);
			AssertEquals("Lookups.AvailableEDocs.Count", 0, list.Count);
		}

		public void TestAvailableEDocsWithDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.DocManagerInfo.AddFileOrDocument(new byte[] { 23, 42, 37 }, "Bingo.pdf", "XXX");

			var file = TestDataBuilder.GetMAFMessaging(declaration).Files.AddNew().Data;

			var list = file.Lookups.AvailableEDocs;
			AssertNotNull("Lookups.AvailableEDocs", list);
			AssertEquals("Lookups.AvailableEDocs.Count", 1, list.Count);
			AssertEquals("Lookups.AvailableEDocs[0].Code", "XXX-Bingo.pdf", list[0].Code);
		}

		public void TestAvailableEDocsUpdatesWhenAnotherEDocAdded()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.DocManagerInfo.AddFileOrDocument(new byte[] { 23, 42, 37 }, "Bingo.pdf", "XXX");

			var file = TestDataBuilder.GetMAFMessaging(declaration).Files.AddNew().Data;

			var list = file.Lookups.AvailableEDocs;
			AssertNotNull("Precondition: Lookups.AvailableEDocs", list);
			AssertEquals("Precondition: Lookups.AvailableEDocs.Count", 1, list.Count);

			declaration.DocManagerInfo.AddFileOrDocument(new byte[] { 23, 42, 37 }, "Bongo.pdf", "YYY");

			list = file.Lookups.AvailableEDocs;
			AssertNotNull("Lookups.AvailableEDocs", list);
			var codes = from ICodeDescription element in list orderby element.Code select element.Code;
			AssertEquals("All codes in List", "XXX-Bingo.pdf, YYY-Bongo.pdf", new ZStringBuilder(codes).ToStringWithDelimiterBetweenAppends(", "));
		}
	}
}
