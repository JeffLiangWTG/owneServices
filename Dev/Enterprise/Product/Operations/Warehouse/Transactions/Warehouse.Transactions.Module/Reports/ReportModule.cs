using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class ReportModule : ZReportModule
	{
		public ReportModule()
		{
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.WarehouseManagerCoreAnd4PL;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsReports;

		public override ModuleIdentifier ID => ModuleIDs.WhsReport;
	}
}
