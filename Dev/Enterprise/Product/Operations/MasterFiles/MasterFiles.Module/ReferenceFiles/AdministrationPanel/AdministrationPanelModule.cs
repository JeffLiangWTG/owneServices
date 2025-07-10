using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class AdministrationPanelModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AdministrationPanel; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.MasterDataManagement; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.MdmAdministrationPanel; }
		}

		protected override ZPopupController GetNewController()
		{
			return new AdministrationPanelController();
		}
	}
}
