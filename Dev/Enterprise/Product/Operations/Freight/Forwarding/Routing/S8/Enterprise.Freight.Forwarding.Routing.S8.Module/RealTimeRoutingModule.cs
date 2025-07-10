using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Routing.S8.Module
{
	public class RealTimeRoutingModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.RoutingLookups; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.RoutingRealTimeLookup; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.RoutingRealTimeLookup; }
		}

		protected override ZPopupController GetNewController()
		{
			return new RealTimeRoutingController();
		}
	}
}
