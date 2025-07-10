using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.GUI.Testing;

[TestedType(typeof(DepartureAdditionalDetailsLayoutBuilder<MessageSendingObject>))]
sealed class DepartureAdditionalDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<DepartureAdditionalDetailsLayoutBuilder<MessageSendingObject>, MessageSendingObject, DepartureAdditionalDetailsControlBag>
{
	public void TestControlsVisibility()
	{
		CombineAssertions(() =>
		{
			AssertControlVisibilityDependantOnMessageType(DepartureAdditionalDetailsControlBag.Instance.JustificationTextBox, x => x.MessageType == DepartureMessageSendingObjectTypeList.Codes.INV);
			AssertControlVisibilityDependantOnMessageType(DepartureAdditionalDetailsControlBag.Instance.AmendmentTypeDropEdit, x => x.MessageType == DepartureMessageSendingObjectTypeList.Codes.AMD);
			AssertControlVisibilityDependantOnMessageType(DepartureAdditionalDetailsControlBag.Instance.TC11DeliveryDateTimeOffsetEdit, x => x.MessageType == DepartureMessageSendingObjectTypeList.Codes.RNM);
			AssertControlVisibilityDependantOnMessageType(DepartureAdditionalDetailsControlBag.Instance.AdditionalTextBox, x => x.MessageType == DepartureMessageSendingObjectTypeList.Codes.RNM);
			AssertControlVisibilityDependantOnMessageType(DepartureAdditionalDetailsControlBag.Instance.DepartureOfficeOfEnquiryCodeFindBox, x => x.MessageType == DepartureMessageSendingObjectTypeList.Codes.RNM);
			AssertControlVisibilityDependantOnMessageType(DepartureAdditionalDetailsControlBag.Instance.ActualConsigneeDocAddressControl, x => x.MessageType == DepartureMessageSendingObjectTypeList.Codes.RNM);
			AssertControlVisibilityDependantOnMessageType(DepartureAdditionalDetailsControlBag.Instance.ActualOfficeOfDestinationCodeFindBox, x => x.MessageType == DepartureMessageSendingObjectTypeList.Codes.RNM);
		});
	}

	void AssertControlVisibilityDependantOnMessageType(ControlReference controlReference, Func<MessageSendingObject, bool> visible)
	{
		var nctsHeader = Factory.New<Business.NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var messageSendingObject = new MessageSendingObjectParent(nctsHeader).SendingObjectsCollection[0];

		foreach (var messageType in MessageTypes)
		{
			messageSendingObject.MessageType = messageType;
			AssertEquals($"{messageType} - {controlReference.Name}", visible(messageSendingObject), Layout.IsVisible(controlReference, messageSendingObject));
		}
	}

	protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

	protected override int ExpectedMaxColumns => 2;

	protected override DepartureAdditionalDetailsLayoutBuilder<MessageSendingObject> GetColumnLayoutBuilderForTesting()
	{
		var builder = new DepartureAdditionalDetailsLayoutBuilder<MessageSendingObject>();
		builder.AddControlBag(DepartureAdditionalDetailsControlBag.Instance);
		return builder;
	}

	string[] MessageTypes => new[]
	{
		DepartureMessageSendingObjectTypeList.Codes.AMD,
		DepartureMessageSendingObjectTypeList.Codes.DEC,
		DepartureMessageSendingObjectTypeList.Codes.INV,
		DepartureMessageSendingObjectTypeList.Codes.PRN,
		DepartureMessageSendingObjectTypeList.Codes.RRL,
		DepartureMessageSendingObjectTypeList.Codes.RNM,
		string.Empty
	};

	PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new DepartureAdditionalDetailsLayout()).Layout);
	PanelLayout layout;
}
