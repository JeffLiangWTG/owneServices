using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class NMFS370DocumentTest : TestCaseWithFactory
	{
		public void TestINMFSDocumentMembers()
		{
			INMFSDocument document = new NMFS370Document(NMFS370DocumentIdentifierList.Codes.NOAAForm370, "B1");
			AssertEquals("DocumentIdentifier", NMFS370DocumentIdentifierList.Codes.NOAAForm370, document.DocumentIdentifier);
			AssertEquals("DocumentNumber", "B1", document.DocumentNumber);
		}
	}
}
