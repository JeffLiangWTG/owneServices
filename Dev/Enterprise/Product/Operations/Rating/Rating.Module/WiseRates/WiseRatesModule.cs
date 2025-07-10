using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Module
{
	public class WiseRatesModule : ZPopupModule
	{
		public override ModuleIdentifier ID => ModuleIDs.WiseRates;
		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WiseRatesSearch;
		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;
		protected override ZPopupController GetNewController()
		{
			return new WiseRatesController();
		}
	}
}

