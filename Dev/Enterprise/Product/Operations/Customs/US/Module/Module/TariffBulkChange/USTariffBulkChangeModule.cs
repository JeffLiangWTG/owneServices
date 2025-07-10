using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class USTariffBulkChangeModule : ZPopupModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.US.USTariffBulkChange;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.TariffBulkChange;

		protected override ZPopupController GetNewController() => new USTariffBulkChangeController();

		internal ZPopupController GetNewControllerInternal() => GetNewController();
	}
}
