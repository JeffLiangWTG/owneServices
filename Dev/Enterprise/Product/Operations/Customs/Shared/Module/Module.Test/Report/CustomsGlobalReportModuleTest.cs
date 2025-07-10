using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CustomsGlobalReportModule))]
	sealed class CustomsGlobalReportModuleTest : ZEmbeddedModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.CustomsGlobalReport;
	}
}
