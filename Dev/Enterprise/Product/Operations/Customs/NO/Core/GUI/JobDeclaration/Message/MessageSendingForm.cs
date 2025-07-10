using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NO.Business;

namespace Enterprise.Customs.NO.GUI;

public partial class MessageSendingForm : MessageSendingFormWithValidationDetails
{
	public MessageSendingForm(MessageSendingObjectParent messageSendingObjectParent)
		: base(messageSendingObjectParent)
	{
		InitializeComponent();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && (components != null))
		{
			var parent = MessageSendingObjectParent;
			if (parent != null)
			{
				foreach (MessageSendingObject sendingObject in parent.SendingObjectsCollection)
				{
					sendingObject.MessageTypeInfo.ValueChanged -= MessageTypeInfo_ValueChanged;
				}
			}
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	public override void SetDataBinding(object dataSource, string dataMember)
	{
		base.SetDataBinding(dataSource, dataMember);
		if (dataSource != null)
		{
			foreach (MessageSendingObject sendingObject in MessageSendingObjectParent.SendingObjectsCollection)
			{
				sendingObject.MessageTypeInfo.ValueChanged += MessageTypeInfo_ValueChanged;
			}
		}
	}

	void MessageTypeInfo_ValueChanged(object sender, System.EventArgs e)
	{
		ChangeSendButtonAvailability();
	}

	protected override bool CheckIsOKToSend()
	{
		return base.CheckIsOKToSend() && IsMessageTypePresentOnAllLines(BusinessEntity);
	}

	static bool IsMessageTypePresentOnAllLines(BaseMessageSendingObjectParent businessEntity)
		=> businessEntity.SendingObjectsCollection
			.Cast<MessageSendingObject>()
			.Where(m => m.ShouldSend)
			.All(m => !m.MessageType.IsEmpty);

	protected override void ChangeSendButtonAvailability()
	{
		base.ChangeSendButtonAvailability();
		if (MessageSendingObjectParent != null)
		{
			var effectiveSendButton = GetEffectiveSendButton();
			if (effectiveSendButton.Enabled)
			{
				effectiveSendButton.Enabled = IsMessageTypePresentOnAllLines(BusinessEntity);
			}
		}
	}
}
