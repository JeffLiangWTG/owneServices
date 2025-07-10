using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	class NMFSDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCY_CodeList()
		{
			var nmfsLine = Factory.New<NMFSLine>();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			var documentDetail = nmfsLine.DocumentDetails.AddNew();
			AssertEquals(typeof(NMFSHMSDocumentIdentifierList), documentDetail.Lookups.CY_CodeList.GetType());

			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			AssertEquals(typeof(NMFSAMRDocumentIdentifierList), documentDetail.Lookups.CY_CodeList.GetType());
		}
	}
}
