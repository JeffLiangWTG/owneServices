using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Module
{
	public class YardReportModule : ZReportModule
	{
		public YardReportModule()
		{
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CYDYardReports;

		public override ModuleIdentifier ID => ModuleIDs.CYDYardReport;
	}
}
