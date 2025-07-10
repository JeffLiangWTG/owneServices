using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportConsignment.Module
{
	public class DtbReportsModule : ZReportModule
	{
		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.LandTransport; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.DtbReports; }
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.DtbReports; }
		}
	}
}
