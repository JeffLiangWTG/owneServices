using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Orders.Module
{
	public class OrdersReportModule : ZReportModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.OrdersReport; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.OrderReports; }
		}
	}
}
