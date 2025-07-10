using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class SupportingDocumentsProviderTest : TestCaseWithFactory
	{
		public void TestDocumentsMembers()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
				var documents = declaration.Documents.ToArray();

				AssertEquals("Count", 3, documents.Length);

				CombineAssertions("Documents Test 1", () =>
				{
					AssertEquals("LineNo", 1, documents[0].LineNo);
					AssertEquals("Code", "0100", documents[0].Code);
					AssertEquals("Verification", "V", documents[0].Verification);
					AssertEquals("DocumentDate", "24/02/2021", documents[0].DocumentDate);
					AssertEquals("Reference", "544554", documents[0].Reference);
					AssertEquals("VisaDate", string.Empty, documents[0].VisaDate);
				});

				CombineAssertions("Documents Test 2", () =>
				{
					AssertEquals("LineNo", 1, documents[1].LineNo);
					AssertEquals("Code", "0200", documents[1].Code);
					AssertEquals("Verification", "Y", documents[1].Verification);
					AssertEquals("DocumentDate", "25/02/2021", documents[1].DocumentDate);
					AssertEquals("Reference", "666666", documents[1].Reference);
					AssertEquals("VisaDate", string.Empty, documents[1].VisaDate);
				});

				CombineAssertions("Documents Test 3", () =>
				{
					AssertEquals("LineNo", 1, documents[2].LineNo);
					AssertEquals("Code", "0300", documents[2].Code);
					AssertEquals("Verification", "L", documents[2].Verification);
					AssertEquals("DocumentDate", "26/02/2021", documents[2].DocumentDate);
					AssertEquals("Reference", "777777", documents[2].Reference);
					AssertEquals("VisaDate", string.Empty, documents[2].VisaDate);
				});
			}
		}
	}
}
