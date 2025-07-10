using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusHAWBWorkflowInformationProviderTest : TestCaseWithFactory
	{
		public void TestAll()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_RL_NKLoadPort = "LOAD1";
			hawb.CS_RL_NKDischargePort = "DEST1";
			var workflowInfo = ((IWorkflowProvider)hawb).GetWorkflowInformationProvider();
			AssertNotNull(workflowInfo);
			AssertEquals("DEST1", workflowInfo.Destination);
			AssertEquals("LOAD1", workflowInfo.Origin);
			var workflowInfo2 = ((IWorkflowProvider)hawb).GetWorkflowInformationProvider();
			AssertEquals("Cached", workflowInfo2, workflowInfo);
			AssertEquals(mawb.Branch.Company.PK, workflowInfo.Companies.First());
			hawb.CS_CM = ZGuid.Empty;
			AssertEquals(0, workflowInfo.Companies.Count());
		}
	}
}
