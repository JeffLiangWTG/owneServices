using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Module.Test
{
	[TestedType(typeof(ProcessFieldChangeRuleModule))]
	class ProcessFieldChangeRuleModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.ProcessFieldChangeRule;
	}
}
