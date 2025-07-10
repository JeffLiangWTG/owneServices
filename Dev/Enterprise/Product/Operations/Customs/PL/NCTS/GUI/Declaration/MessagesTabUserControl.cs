using System;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.NCTS.GUI;

public partial class MessagesTabUserControl : EU.NCTS.GUI.MessagesTabUserControl
{
	public MessagesTabUserControl()
	{
		InitializeComponent();
	}

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);

		MessageGrid_SelectedRowsChangedInMouseDown(null, null);
	}

	void MessageGrid_SelectedRowsChangedInMouseDown(object sender, EventArgs e)
	{
		var messageGrid = MessageGrid;
		if (messageGrid.GetCurrent() is Messaging.Business.EDIMessage message)
		{
			InterpretationTabPage.CaptionResourceString = message.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit
				? Enterprise.Customs.PL.NCTS.GUI.Res.GetData("419B33BB-BD89-44C8-A8FA-FD56292BC9EB", "Message Text")
				: Enterprise.Customs.PL.NCTS.GUI.Res.GetData("FD814950-F482-40C7-B1EA-A4DC1A72D8C4", "Interpretation");
			InterpretationTabPage.UpdateCaption();
		}
	}
}
