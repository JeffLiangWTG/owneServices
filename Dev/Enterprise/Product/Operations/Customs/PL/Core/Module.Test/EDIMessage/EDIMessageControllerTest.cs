using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Module.Test;

[TestedType(typeof(EDIMessageController))]
sealed class EDIMessageControllerBasherTest : ZControllerBasherTest
{
	protected override ControllerID GetControllerID() => ControllerIDs.Messaging.EDIMessage;
}
