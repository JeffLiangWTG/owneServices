using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Module
{
	public class AgencyReports : ZReportModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AgencyReports; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AgencyReports; }
		}
	}
}
