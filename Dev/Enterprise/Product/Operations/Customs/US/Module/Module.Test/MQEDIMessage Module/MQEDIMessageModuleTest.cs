using Enterprise.Customs.US.Business;
using Enterprise.Messaging.Module;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestsSubclassesOf(typeof(MQEDIMessageModule), ExcludeClientDlls = true)]
	abstract class MQEDIMessageModuleTest : ZModuleBasherTest
	{
		public virtual void TestRequeuingMenu()
		{
			AssertMenuItemIsNull(EDIMessageModule.ResetStatusToQueuedMenuName);
		}

		public abstract void TestSetToComplete();

		protected void AssertMenuItemIsNull(string menuName)
		{
			using (var module = (MQEDIMessageModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertNull("Menu name '" + menuName + "' should have been removed.", module.ContextMenuExposedForTesting.FindByText(menuName));
			}
		}

		protected void AssertSetToCompleteMenuItem(string messageType)
		{
			SetupTwoIncompleteMessages(messageType);
			using (var module = (MQEDIMessageModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var actionsMenuItem = module.ContextMenuExposedForTesting.FindByText("Actions");
				var item = actionsMenuItem.MenuItems.FindByText(MQEDIMessageModule.SetToCompleteMenuName);
				AssertEquals("Menu action '" + MQEDIMessageModule.SetToCompleteMenuName + "' should enabled.", true, item.Enabled);
				using (var form = new ZChildForm(module.GridCollection))
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();
					((ZFilterStripControl)module.EmbeddedControl).FirePerformSearch();
					var messages = module.GridCollection.ToArray();
					AssertEquals(2, messages.Length);
					module.GridExposedForTesting.UnSelectAll();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					item.PerformClick();
					AssertEquals(EDIMessageModule.SelectAtLeastOneMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(EM_ActionStatusList.Codes.Incomplete, ((MQEDIMessage)messages[0]).EM_ActionStatus);
					AssertEquals(EM_ActionStatusList.Codes.Incomplete, ((MQEDIMessage)messages[1]).EM_ActionStatus);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					module.GridExposedForTesting.Select(1);
					item.PerformClick();
					AssertEquals(EM_ActionStatusList.Codes.Incomplete, ((MQEDIMessage)messages[0]).EM_ActionStatus);
					AssertEquals(EM_ActionStatusList.Codes.Complete, ((MQEDIMessage)messages[1]).EM_ActionStatus);
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					module.GridExposedForTesting.Select(0);
					item.PerformClick();
					AssertEquals(EM_ActionStatusList.Codes.Complete, ((MQEDIMessage)messages[0]).EM_ActionStatus);
					AssertEquals(EM_ActionStatusList.Codes.Complete, ((MQEDIMessage)messages[1]).EM_ActionStatus);
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		protected virtual void SetupTwoIncompleteMessages(string messageType)
		{
			var message1 = Factory.New<MQEDIMessage>();
			message1.EM_MessageType = messageType;
			message1.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message1.EM_MessageNum = "1";
			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_MessageType = messageType;
			message2.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message2.EM_MessageNum = "1";
			Factory.Save();
		}
	}
}
