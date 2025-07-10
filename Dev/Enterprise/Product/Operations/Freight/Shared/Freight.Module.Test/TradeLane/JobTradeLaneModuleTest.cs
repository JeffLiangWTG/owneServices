using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(JobTradeLaneModule))]
	sealed class JobTradeLaneModuleTest : ZModuleBasherTest
	{
		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.TradeLane;
		}

		#endregion
	}
}
