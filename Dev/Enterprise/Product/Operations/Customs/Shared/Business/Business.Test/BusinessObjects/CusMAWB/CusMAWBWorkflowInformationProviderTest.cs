using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusMAWBWorkflowInformationProviderTest : TestCaseWithFactory
	{
		public void TestAll()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_RL_NKLoadPort = "LOAD1";
			mawb.CM_RL_NKDischargePort = "DEST1";
			var workflowInfo = ((IWorkflowProvider)mawb).GetWorkflowInformationProvider();
			AssertNotNull(workflowInfo);
			AssertEquals("DEST1", workflowInfo.Destination);
			AssertEquals("LOAD1", workflowInfo.Origin);
			var workflowInfo2 = ((IWorkflowProvider)mawb).GetWorkflowInformationProvider();
			AssertEquals("Cached", workflowInfo2, workflowInfo);
			AssertEquals(mawb.Branch.Company.PK, workflowInfo.Companies.First());
		}
	}
}
