using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(ReportModule))]
	public class ReportModuleTest : ZEmbeddedModuleBasherTest
	{
		public void TestReportModule()
		{
			using (var bookingReportModule = new ReportModule())
			{
				AssertEquals(Env.Licence.TransportBookings, bookingReportModule.LicenceCheckPoint);
				AssertEquals(Env.Security.DtbBookingReports, bookingReportModule.SecurityCheckpoint);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.DtbBookingReports;
		}
	}
}
