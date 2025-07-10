using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportConsignment.Module
{
	public class DtbRoutePlannerModule : ZPopupModule
	{
		protected override ZPopupController GetNewController()
		{
			return (ZPopupController)ZControllerFactory.Create(ControllerIDs.DtbRoutePlanner);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.DtbRoutePlanner; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.LandTransport; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.DtbRoutePlanner; }
		}
	}
}
