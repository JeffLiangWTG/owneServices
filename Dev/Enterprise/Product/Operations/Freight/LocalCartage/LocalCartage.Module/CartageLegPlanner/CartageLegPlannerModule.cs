using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.Module
{
	public class CartageLegPlannerModule : ZPopupModule
	{
		public CartageLegPlannerModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.CartageLegPlanner; }
		}

		protected override ZPopupController GetNewController()
		{
			return (ZPopupController)ZControllerFactory.Create(ControllerIDs.CartageLegPlanner);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.LocalTransport; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.LocalTransportLegPlanner; }
		}
	}
}
