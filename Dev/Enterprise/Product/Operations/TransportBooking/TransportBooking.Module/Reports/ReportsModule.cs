using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportBookings.Module
{
	public class ReportModule : ZReportModule
	{
		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.TransportBookings; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.DtbBookingReports; }
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.DtbBookingReports; }
		}
	}
}
