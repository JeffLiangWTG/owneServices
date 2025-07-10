using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(JobSeaVoyageModule))]
	sealed class JobSeaVoyageModuleBasherTest : ZModuleBasherTest
	{
		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.JobSeaVoyage;
		}

		#endregion
	}
}
