using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(CensusWarningQueryForm))]
	sealed class CensusWarningQueryFormTest : ZFormBasherTest
	{
		public void TestSendButton_Click()
		{
			var query = new ZQuery(EDIMessageSchema.EM_MessageType, ACEApplicationIdentifierCodeList.Codes.CensusWarningQuery);
			var messagesGenerated = Factory.Load<MQEDIMessage>(query);
			AssertEquals("Precondition: No Census Warning Query message is generated", 0, messagesGenerated.Length);
			var messageData = new CensusWarningQuery(Factory);
			messageData.DateFrom = ZDateTime.Today.AddDays(2);
			messageData.DateTo = ZDateTime.Today;
			using (var form = new CensusWarningQueryForm(messageData))
			{
				form.Visible = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.SendButton.PerformClick();
				AssertEquals("Message cannot be sent", true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("Please fix the errors first"));
				messagesGenerated = Factory.Load<MQEDIMessage>(query);
				AssertEquals("No Census Warning Query message is generated", 0, messagesGenerated.Length);
				messageData.EntryFilerCode = "SV9";
				messageData.DateFrom = ZDateTime.Today.AddDays(-1);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.SendButton.PerformClick();
				messagesGenerated = Factory.Load<MQEDIMessage>(query);
				AssertEquals("Census Warning Query message should be sent", 1, messagesGenerated.Length);
				AssertEquals(ACEApplicationIdentifierCodeList.Codes.CensusWarningQuery, messagesGenerated[0].EM_MessageType);
			}
		}

		public void TestGiveUpButton_Click()
		{
			using (var form = (CensusWarningQueryForm)GetFormToBash())
			{
				form.Visible = true;
				form.GiveUpButton.PerformClick();
				AssertEquals("Action should be None", CensusWarningQueryForm.Action.None, form.ActionChosenByUsers);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var messageData = new CensusWarningQuery(Factory);
			Factory.Save();
			return new CensusWarningQueryForm(messageData);
		}
	}
}
