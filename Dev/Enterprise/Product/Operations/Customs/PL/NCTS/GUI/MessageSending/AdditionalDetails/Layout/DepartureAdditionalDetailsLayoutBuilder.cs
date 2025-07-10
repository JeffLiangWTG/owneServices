using CargoWise.Types;
using Enterprise.Customs.PL.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.NCTS.GUI;

public class DepartureAdditionalDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, DepartureAdditionalDetailsControlBag> where T : MessageSendingObject
{
	public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

	public override DepartureAdditionalDetailsControlBag CommonBag => DepartureAdditionalDetailsControlBag.Instance;

	protected override int MaxColumns => 2;

	protected override void SetDefaultVisibilities()
	{
		base.SetDefaultVisibilities();
		SetVisibility(CommonBag.AmendmentTypeDropEdit, AmendmentTypeDropEditVisible, x => x.MessageTypeInfo);
		SetVisibility(CommonBag.JustificationTextBox, JustificationTextBoxVisible, x => x.MessageTypeInfo);
		SetVisibility(CommonBag.TC11DeliveryDateTimeOffsetEdit, TC11DeliveryDateTimeOffsetEditVisible, x => x.MessageTypeInfo);
		SetVisibility(CommonBag.AdditionalTextBox, AdditionalTextBoxVisible, x => x.MessageTypeInfo);
		SetVisibility(CommonBag.ActualConsigneeDocAddressControl, ActualConsigneeDocAddressControlVisible, x => x.MessageTypeInfo);
		SetVisibility(CommonBag.ActualOfficeOfDestinationCodeFindBox, ActualOfficeOfDestinationCodeFindBoxVisible, x => x.MessageTypeInfo);
		SetVisibility(CommonBag.DepartureOfficeOfEnquiryCodeFindBox, DepartureOfficeOfEnquiryCodeFindBoxVisible, x => x.MessageTypeInfo);
	}

	static bool AmendmentTypeDropEditVisible(T messageSendingObject) => IsMessageType(messageSendingObject, DepartureMessageSendingObjectTypeList.Codes.AMD);
	static bool JustificationTextBoxVisible(T messageSendingObject) => IsMessageType(messageSendingObject, DepartureMessageSendingObjectTypeList.Codes.INV);
	static bool TC11DeliveryDateTimeOffsetEditVisible(T messageSendingObject) => IsMessageType(messageSendingObject, ArrivalMessageSendingObjectTypeList.Codes.RNM);
	static bool AdditionalTextBoxVisible(T messageSendingObject) => IsMessageType(messageSendingObject, ArrivalMessageSendingObjectTypeList.Codes.RNM);
	static bool ActualConsigneeDocAddressControlVisible(T messageSendingObject) => IsMessageType(messageSendingObject, ArrivalMessageSendingObjectTypeList.Codes.RNM);
	static bool ActualOfficeOfDestinationCodeFindBoxVisible(T messageSendingObject) => IsMessageType(messageSendingObject, ArrivalMessageSendingObjectTypeList.Codes.RNM);
	static bool DepartureOfficeOfEnquiryCodeFindBoxVisible(T messageSendingObject) => IsMessageType(messageSendingObject, ArrivalMessageSendingObjectTypeList.Codes.RNM);

	static bool IsMessageType(T messageSendingObject, ZString messageType) => messageSendingObject.MessageType == messageType;
}
