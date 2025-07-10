using System;
using Enterprise.Customs.NL.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.NCTS.GUI;

public partial class MessageSendingForm : EU.NCTS.GUI.MessageSendingForm
{
	public MessageSendingForm(MessageSendingActionParent sendingObjectWrapper) : base(sendingObjectWrapper)
	{
		InitializeComponent();
	}

	public new MessageSendingActionParent MessageSendingObjectParent => (MessageSendingActionParent)base.MessageSendingObjectParent;

	protected override ZUserControl GetBottomSectionUserControl()
	{
		if (bottomSectionUserControl == null)
		{
			bottomSectionUserControl = new MessageSendingFormBottomSectionUserControl();
		}
		return bottomSectionUserControl;
	}
	MessageSendingFormBottomSectionUserControl bottomSectionUserControl;

	public override void SetDataBinding(object dataSource, string dataMember)
	{
		base.SetDataBinding(dataSource, dataMember);

		var parent = MessageSendingObjectParent;
		if (parent != null)
		{
			foreach (MessageSendingAction action in parent.SendingObjectsCollection)
			{
				action.MessageTypeInfo.ValueChanged -= MessageTypeInfo_ValueChanged;
				action.MessageTypeInfo.ValueChanged += MessageTypeInfo_ValueChanged;
			}
		}
		SurpressValidationsByMessageType();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && (components != null))
		{
			components.Dispose();
			var parent = MessageSendingObjectParent;
			if (parent != null)
			{
				foreach (MessageSendingAction action in parent.SendingObjectsCollection)
				{
					action.MessageTypeInfo.ValueChanged -= MessageTypeInfo_ValueChanged;
				}
			}
		}
		base.Dispose(disposing);
	}

	void MessageTypeInfo_ValueChanged(object sender, EventArgs e)
	{
		SurpressValidationsByMessageType();
	}

	void SurpressValidationsByMessageType()
	{
		ChangeSendWithValidationErrorsCheckBoxAvailability();
		ChangeValidationErrorsTextBoxAvailability();
		ChangeSendButtonAvailability();
	}

	protected override bool SendWithValidationErrorsTextBoxVisible => MessageSendingObjectParent?.ShowValidationErrors ?? false;
}
