using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.NZ.Module
{
	public class NZTariffBulkChangeModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.TariffBulkChange; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Broker; }
			//get { return Env.Licence.TariffBulkUpdater; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.TariffBulkChange; }
		}

		protected override ZPopupController GetNewController()
		{
			return new NZTariffBulkChangeController();
		}
	}
}
