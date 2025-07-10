using System;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(QueryMessageController))]
	sealed class QueryMessageControllerTest : ZControllerBasherTest
	{
		public void TestCheckPoints()
		{
			var message = Factory.New<MQEDIMessage>();
			var controller = new QueryMessageController();
			AssertEquals(Env.Security.QueryMessagesView, controller.GetCheckPointForView(message));
		}

		public void TestGetForm()
		{
			var message = Factory.New<MQEDIMessage>();
			message.FillWithValidTestData();
			message.EM_MessageText = "";
			Factory.Save();
			var controller = new QueryMessageController();
			using (var form = controller.ShowViewForm(message))
			{
				AssertEquals(typeof(MQEDIMessageWithRelatedMessageDetailsForm), form.GetType());
			}
		}

		public override Type ControllerToBashType => typeof(QueryMessageController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.QueryMessages;
	}
}
