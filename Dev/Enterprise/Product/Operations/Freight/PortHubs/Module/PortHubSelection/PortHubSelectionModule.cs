using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.PortHubs.Module
{
	public class PortHubSelectionModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.PortHubSelection; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.PortDepotSelection; }
		}

		protected override ZPopupController GetNewController()
		{
			return (ZPopupController)ZControllerFactory.Create(ControllerIDs.PortHubSelection);
		}
	}
}
