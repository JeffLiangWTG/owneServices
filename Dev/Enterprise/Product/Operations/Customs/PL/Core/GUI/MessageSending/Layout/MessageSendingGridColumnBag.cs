using System;
using Enterprise.Customs.PL.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public sealed class MessageSendingGridColumnBag
{
	public static MessageSendingGridColumnBag Instance => instance ?? (instance = new MessageSendingGridColumnBag());

	[ThreadStatic]
	static MessageSendingGridColumnBag instance;

	MessageSendingGridColumnBag()
	{
		ShouldSendCheckBoxColumn = new GridColumnReference<ZCheckBoxColumnStyleInfo>(BaseMessageSendingObject.PLSchema.ShouldSend, 40);
		ActionDropEditColumnStyle = new GridColumnReference<ZDropEditColumnStyleInfo>(BaseMessageSendingObject.PLSchema.Action, 40, c => c.IsMandatory = true);
		DeclarationDateDateEditColumn = new GridColumnReference<ZDateEditColumnStyleInfo>(BaseMessageSendingObject.PLSchema.DeclarationDate, 70);
		ReferenceNumberTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(BaseMessageSendingObject.Schema.LocalReferenceNumber, 150);
		EntryNumberTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(BaseMessageSendingObject.PLSchema.EntryNumber, 150, c => c.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper);
		EntryStatusTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(BaseMessageSendingObject.Schema.EntryStatus, 80);
		StatusTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(BaseMessageSendingObject.PLSchema.MessageStatus, 80);
		EntryDescriptionTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(BaseMessageSendingObject.PLSchema.EntryDescription, 120);
		ResponseMessageDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(BaseMessageSendingObject.PLSchema.ResponseMessage, 150);
	}

	public IGridColumnReference ShouldSendCheckBoxColumn { get; }

	public IGridColumnReference DeclarationDateDateEditColumn { get; }

	public IGridColumnReference ActionDropEditColumnStyle { get; }

	public IGridColumnReference EntryNumberTextBoxColumn { get; }

	public IGridColumnReference EntryStatusTextBoxColumn { get; }

	public IGridColumnReference StatusTextBoxColumn { get; }

	public IGridColumnReference ReferenceNumberTextBoxColumn { get; }

	public IGridColumnReference EntryDescriptionTextBoxColumn { get; }

	public IGridColumnReference ResponseMessageDropEditColumn { get; }
}
