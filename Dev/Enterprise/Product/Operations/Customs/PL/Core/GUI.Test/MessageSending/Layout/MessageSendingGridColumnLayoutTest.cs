using System;
using System.Collections.Generic;
using CargoWise.Windows.UI;
using Enterprise.Customs.PL.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class MessageSendingGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<MessageSendingGridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		(BaseMessageSendingObject.PLSchema.ShouldSend, typeof(ZCheckBoxColumnStyleInfo), ControlDpiScalingHelper.ScaleToCurrentDpiX(40)),
		(BaseMessageSendingObject.PLSchema.Action, typeof(ZDropEditColumnStyleInfo), ControlDpiScalingHelper.ScaleToCurrentDpiX(40)),
		(BaseMessageSendingObject.PLSchema.DeclarationDate, typeof(ZDateEditColumnStyleInfo), ControlDpiScalingHelper.ScaleToCurrentDpiX(70)),
		(BaseMessageSendingObject.Schema.LocalReferenceNumber, typeof(ZTextBoxColumnStyleInfo), ControlDpiScalingHelper.ScaleToCurrentDpiX(150)),
		(BaseMessageSendingObject.PLSchema.EntryNumber, typeof(ZTextBoxColumnStyleInfo), ControlDpiScalingHelper.ScaleToCurrentDpiX(150)),
		(BaseMessageSendingObject.Schema.EntryStatus, typeof(ZTextBoxColumnStyleInfo), ControlDpiScalingHelper.ScaleToCurrentDpiX(80)),
		(BaseMessageSendingObject.PLSchema.MessageStatus, typeof(ZTextBoxColumnStyleInfo), ControlDpiScalingHelper.ScaleToCurrentDpiX(80)),
		(BaseMessageSendingObject.PLSchema.EntryDescription, typeof(ZTextBoxColumnStyleInfo), ControlDpiScalingHelper.ScaleToCurrentDpiX(120)),
		(BaseMessageSendingObject.PLSchema.ResponseMessage, typeof(ZDropEditColumnStyleInfo), ControlDpiScalingHelper.ScaleToCurrentDpiX(150)),
	};

	protected override Type GridBoundEntityType => typeof(BaseMessageSendingObject);
}
