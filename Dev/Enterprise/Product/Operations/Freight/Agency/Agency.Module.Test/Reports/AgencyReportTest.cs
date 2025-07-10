using Enterprise.Environment;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Module.Testing
{
	internal class AgencyReportTest : BaseAgencyTest
	{
		public void TestModuleID()
		{
			using (AgencyReports module = new AgencyReports())
			{
				AssertEquals(ModuleIDs.AgencyReports, module.ID);
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (AgencyReports module = new AgencyReports())
			{
				AssertEquals(Env.Security.AgencyReports, module.SecurityCheckpoint);
			}
		}

		public void TestLoad()
		{
			using (ZModule module = ZModuleFactory.Instance.Create(ModuleIDs.AgencyReports))
			{
				AssertEquals(typeof(AgencyReports), module.GetType());
			}
		}
	}
}
