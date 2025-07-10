using System;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(BorderLineReleaseMessageController))]
	sealed class BorderLineReleaseMessageControllerTest : ZControllerBasherTest
	{
		public void TestCheckPoints()
		{
			var message = Factory.New<MQEDIMessage>();
			var controller = new BorderLineReleaseMessageController();
			AssertEquals(Env.Security.BorderLineReleaseMessageView, controller.GetCheckPointForView(message));
		}

		public override Type ControllerToBashType => typeof(BorderLineReleaseMessageController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.BorderLineReleaseMessage;
	}
}
