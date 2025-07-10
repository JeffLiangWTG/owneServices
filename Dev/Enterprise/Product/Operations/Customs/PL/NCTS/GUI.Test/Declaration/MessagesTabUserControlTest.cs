using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.NCTS.GUI.Testing;

sealed class MessagesTabUserControlTest : TestCaseWithFactory
{
	public void TestInterpretationTabPageCaption()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;

		var interpretationCaption = "Interpretation";
		var messageTextCaption = "Message Text";

		using (var form = new ZForm(nctsHeader))
		using (var messageUserControl = new MessagesTabUserControl())
		{
			messageUserControl.SetDataBinding(nctsHeader, "Messages");
			form.Controls.Add(messageUserControl);
			form.Show();

			var messagesGrid = messageUserControl.FindSingle<ZGrid>("MessageGrid");
			var interpretationTabPage = messageUserControl.FindSingleOrDefault<ZTabPage>("InterpretationTabPage");

			CombineAssertions(() =>
			{
				AssertEquals("no messages on grid", interpretationCaption, interpretationTabPage.CaptionResourceString.Caption);

				var outgoingMessage = nctsHeader.Messages.AddNew();
				outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
				var incomingMessage = nctsHeader.Messages.AddNew();
				incomingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;

				messagesGrid.PerformMouseDownForTest(0, 1);
				AssertEquals("Outgoing message should be selected", 0, messagesGrid.CurrentRowIndex);
				AssertEquals("Outgoing Message type caption", messageTextCaption, interpretationTabPage.CaptionResourceString.Caption);

				messagesGrid.PerformMouseDownForTest(1, 1);
				AssertEquals("Other message should be selected", 1, messagesGrid.CurrentRowIndex);
				AssertEquals("Other Message type caption", interpretationCaption, interpretationTabPage.CaptionResourceString.Caption);
			});
		}
	}
}
