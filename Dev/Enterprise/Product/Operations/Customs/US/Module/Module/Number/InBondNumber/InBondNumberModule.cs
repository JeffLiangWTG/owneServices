using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class InBondNumberModule : NumberModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.US.InBondNumber;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.InBondNumber;

		protected override ZPopupController GetNewController() => new InBondNumberController();
	}
}
