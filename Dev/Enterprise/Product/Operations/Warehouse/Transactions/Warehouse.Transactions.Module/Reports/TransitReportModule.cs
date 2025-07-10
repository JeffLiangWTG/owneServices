using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class TransitReportModule : ZReportModule
	{
		public TransitReportModule()
		{
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.TransitReports;

		public override ModuleIdentifier ID => ModuleIDs.WhsTransitReport;
	}
}
