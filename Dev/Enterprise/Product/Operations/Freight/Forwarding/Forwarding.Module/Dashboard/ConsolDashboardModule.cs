using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Module
{
	public class ConsolDashboardModule : ZPopupModule
	{
		public override ModuleIdentifier ID => ModuleIDs.ConsolPlanningBoard;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.ConsolPlanningBoard;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;

		protected override ZPopupController GetNewController()
		{
			return new ConsolDashboardController();
		}
	}
}
