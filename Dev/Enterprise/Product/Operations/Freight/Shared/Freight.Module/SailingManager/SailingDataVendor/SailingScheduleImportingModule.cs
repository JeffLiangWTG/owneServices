using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.SailingDataVendor.Module
{
	public class SailingScheduleImportingModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.SailingDataVendorImporting; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			// no license will be required during testing
			// when enabling the license, update OneStopSailingScheduleDataVendor.IsEnabledCore to examine if the license is enabled
			get { return Env.Licence.AlwaysAllow; }//.OneStopVesselIntegration; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.SailingScheduleImporting; }
		}

		protected override ZPopupController GetNewController()
		{
			return new SailingScheduleImportingController();
		}
	}
}
