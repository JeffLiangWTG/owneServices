using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class OtherReconIssueListCreatorTest : TestCaseWithFactory
	{
		public void TestCreateOtherReconIssueList()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals("OtherReconIssueList", typeof(ReconIssueCodeList), declaration.AddInfoLookups.OtherReconIssueList.GetType());
			AssertEquals("not existing ReconIssueCodeList.Codes.FTA", false, declaration.AddInfoLookups.OtherReconIssueList.ContainsCode(ReconIssueCodeList.Codes.FTA));
		}
	}
}
