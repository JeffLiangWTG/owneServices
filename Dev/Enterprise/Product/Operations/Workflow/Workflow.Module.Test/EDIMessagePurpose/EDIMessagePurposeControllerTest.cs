using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Module.Test
{
	[TestedType(typeof(EDIMessagePurposeController))]
	class EDIMessagePurposeControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Messaging.EDIMessagePurpose;
	}
}
