using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class StmUpgradeModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.StmUpgrade; }
		}

		protected override ZPopupController GetNewController()
		{
			return new StmUpgradeController();
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.Upgrades; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}
	}
}
