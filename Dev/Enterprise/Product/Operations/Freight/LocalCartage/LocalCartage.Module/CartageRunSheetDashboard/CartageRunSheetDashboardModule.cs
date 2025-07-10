using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.Module
{
	public class CartageRunSheetDashboardModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.CartageRunSheetDashboard; }
		}

		protected override ZPopupController GetNewController()
		{
			return (ZPopupController)ZControllerFactory.Create(ControllerIDs.CartageRunSheetDashboard);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.LocalTransport; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.LocalTransportRunSheetDashboard; }
		}
	}
}
