using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Module.Testing
{
	[TestedType(typeof(DtbReportsModule))]
	public class DtbReportsModuleTest : ZEmbeddedModuleBasherTest
	{
		public void TestReportModule()
		{
			using (var reportModule = new DtbReportsModule())
			{
				AssertEquals(Env.Licence.LandTransport, reportModule.LicenceCheckPoint);
				AssertEquals(Env.Security.DtbReports, reportModule.SecurityCheckpoint);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.DtbReports;
		}
	}
}
