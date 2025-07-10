using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Routing.S8.Module.Test
{
	[TestedType(typeof(RealTimeRoutingModule))]
	public class RealTimeRoutingModuleTest : ZPopupModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RoutingLookups;
		}
	}
}
