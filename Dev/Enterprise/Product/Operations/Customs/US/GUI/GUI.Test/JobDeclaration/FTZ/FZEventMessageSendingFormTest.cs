using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(FZEventMessageSendingForm))]
	sealed class FZEventMessageSendingFormTest : ZFormBasherTest
	{
		public void TestSendButton_Click()
		{
			var query = new ZQuery(EDIMessageSchema.EM_MessageType, ACEApplicationIdentifierCodeList.Codes.FTZEventReporting);
			var messagesGenerated = Factory.Load<MQEDIMessage>(query);
			AssertEquals("Precondition: No FZ EventReporting message is generated", 0, messagesGenerated.Length);
			var declaration = Factory.New<JobDeclaration>();
			var messageData = new FZEventAction(declaration, FZEventType.Concur);
			using (var form = new FZEventMessageSendingForm(messageData))
			{
				form.Visible = true;
				messageData.US_ActionCode = "";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.SendButton.PerformClick();
				AssertEquals("Message cannot be sent", true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("Please fix the errors first"));
				messagesGenerated = Factory.Load<MQEDIMessage>(query);
				AssertEquals("No FZ EventReporting message is generated", 0, messagesGenerated.Length);
				messageData.US_ActionCode = FTZActionCodeList.Codes.B;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.SendButton.PerformClick();
				AssertEquals("Message can be sent", null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			var action = new FZEventAction(declaration, FZEventType.Concur);
			using (var form = new FZEventMessageSendingForm(action))
			{
				form.Show();
				var actionPanel = form.FindSingle<ZPanel>("FTZActionPanel");
				var unconcurPanel = form.FindSingle<ZPanel>("FTZUnconcurrencePanel");
				AssertEquals(true, actionPanel.Visible);
				AssertEquals(false, unconcurPanel.Visible);
			}

			action = new FZEventAction(declaration, FZEventType.Unconcur);
			using (var form = new FZEventMessageSendingForm(action))
			{
				form.Show();
				var actionPanel = form.FindSingle<ZPanel>("FTZActionPanel");
				var unconcurPanel = form.FindSingle<ZPanel>("FTZUnconcurrencePanel");
				AssertEquals(false, actionPanel.Visible);
				AssertEquals(true, unconcurPanel.Visible);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var messageData = new FZEventAction(declaration, FZEventType.Unconcur);
			messageData.US_ReasonCode = FTZUnconcurrenceReasonCodeList.Codes._03;
			messageData.US_Reasons = "1234567";
			messageData.US_FTZContactName = "Dan Brown";
			messageData.US_FTZContactPhone = "7382945000";
			Factory.Save();
			return new FZEventMessageSendingForm(messageData);
		}
	}
}
