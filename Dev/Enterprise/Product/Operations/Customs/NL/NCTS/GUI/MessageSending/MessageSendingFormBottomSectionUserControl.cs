using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.NL.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.NCTS.GUI;

public partial class MessageSendingFormBottomSectionUserControl : ZUserControl
{
	public MessageSendingFormBottomSectionUserControl()
	{
		InitializeComponent();
		AfterFirstBinding += MessageSendingFormBottomSectionUserControl_AfterFirstBinding;
	}

	void MessageSendingFormBottomSectionUserControl_AfterFirstBinding(object sender, System.EventArgs e)
	{
		dataSource = (MessageSendingActionParent)DataSource;
		foreach (MessageSendingAction action in dataSource.SendingObjectsCollection)
		{
			action.MessageTypeInfo.ValueChanged += MessageTypeInfo_ValueChanged;
			MessageTypeChanged(action);
		}
	}

	MessageSendingActionParent dataSource;
	void MessageTypeInfo_ValueChanged(object sender, System.EventArgs e)
	{
		MessageTypeChanged((MessageSendingAction)sender);
	}

	internal void MessageTypeChanged(MessageSendingAction action)
	{
		SetControlsVisibility(action);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			components?.Dispose();
			if (dataSource != null)
			{
				foreach (MessageSendingAction action in dataSource.SendingObjectsCollection)
				{
					action.MessageTypeInfo.ValueChanged -= MessageTypeInfo_ValueChanged;
				}
			}
		}
		base.Dispose(disposing);
	}

	void SetControlsVisibility(MessageSendingAction action)
	{
		var isMessageTypeInv = false;
		var isMessageTypeRNM = false;
		var showPresentationDateTime = false;
		bool showAgreeWithMinorDiscrepancies = false;
		if (action != null)
		{
			var actionMessageType = action.MessageType;
			var actionMessageStatus = action.MessageStatus;
			isMessageTypeInv = actionMessageType == NctsMessageTypeListNL.Codes.InvalidationCancellation;
			isMessageTypeRNM = actionMessageType == NctsMessageTypeListNL.Codes.ResponseOnRequestForNonArrivedMovement;
			showPresentationDateTime = actionMessageType == NctsMessageTypeListNL.Codes.Amendment || actionMessageStatus == NctsTransitStatusList.Codes.DeclarationRejected || actionMessageStatus == NctsTransitStatusList.Codes.DeclarationAccepted || !action.PresentationDateTimeReadOnly;
			showAgreeWithMinorDiscrepancies = actionMessageType == NctsMessageTypeListNL.Codes.RequestARelease;
		}
		PresentationDateAndTimeOffsetEdit.Visible = showPresentationDateTime;
		JustificationTextBox.Visible = isMessageTypeInv;
		TCI11DateEdit.Visible = isMessageTypeRNM;
		QueryInformationTextBox.Visible = isMessageTypeRNM;
		ActualConsigneeDocAddressControl.Visible = isMessageTypeRNM;
		ActualOfficeOfDestinationFindBox.Visible = isMessageTypeRNM;
		AgreeWithMinorDiscrepanciesCheckBox.Visible = showAgreeWithMinorDiscrepancies;
	}
}
