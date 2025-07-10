using System;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(AMSBrokerDownloadController))]
	sealed class AMSBrokerDownloadControllerTest : ZControllerBasherTest
	{
		public void TestCheckPoints()
		{
			var message = Factory.New<MQEDIMessage>();
			var controller = new AMSBrokerDownloadController();
			AssertEquals(Env.Security.AMSBrokerDownloadMessagesView, controller.GetCheckPointForView(message));
		}

		public override Type ControllerToBashType => typeof(AMSBrokerDownloadController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.AMSBrokerDownloadMessages;
	}
}
