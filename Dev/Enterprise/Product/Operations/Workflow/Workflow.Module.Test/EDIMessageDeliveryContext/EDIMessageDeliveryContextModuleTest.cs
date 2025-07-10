using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Module.Test
{
	[TestedType(typeof(EDIMessageDeliveryContextModule))]
	public class EDIMessageDeliveryContextModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Messaging.EDIMessageDeliveryContext;
	}
}
