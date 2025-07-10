using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbCapabilityModule))]
	sealed class GlbCapabilityModuleTest : ZArchitecture.Modules.Testing.ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GlbCapability;
		}
	}
}
