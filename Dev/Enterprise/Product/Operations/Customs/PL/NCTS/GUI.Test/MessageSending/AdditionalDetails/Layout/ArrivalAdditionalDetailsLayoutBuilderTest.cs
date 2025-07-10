using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.GUI.Testing;

[TestedType(typeof(ArrivalAdditionalDetailsLayoutBuilder<MessageSendingObject>))]
sealed class ArrivalAdditionalDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ArrivalAdditionalDetailsLayoutBuilder<MessageSendingObject>, MessageSendingObject, ArrivalAdditionalDetailsControlBag>
{
	public void TestControlsVisibility()
	{
		CombineAssertions(() =>
		{
			AssertControlVisibilityDependantOnMessageType(ArrivalAdditionalDetailsControlBag.Instance.TirPageNumberDropEdit);

			AssertControlVisibilityDependantOnMessageType(ArrivalAdditionalDetailsControlBag.Instance.TirUnloadingNumberDropEdit);
		});
	}

	void AssertControlVisibilityDependantOnMessageType(ControlReference controlReference)
	{
		var nctsHeader = Factory.New<Business.NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		var messageSendingObject = new MessageSendingObjectParent(nctsHeader).SendingObjectsCollection[0];

		foreach (var messageType in MessageTypes)
		{
			messageSendingObject.MessageType = messageType;

			messageSendingObject.NctsHeader.ArrivalMovementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
			AssertEquals($"{messageType} -TIR- {controlReference.Name}", IsControlVisible(messageSendingObject), Layout.IsVisible(controlReference, messageSendingObject));

			messageSendingObject.NctsHeader.ArrivalMovementHeader.BM_InBondEntryType = string.Empty;
			AssertEquals($"{messageType} -Empty- {controlReference.Name}", IsControlVisible(messageSendingObject), Layout.IsVisible(controlReference, messageSendingObject));

			messageSendingObject.NctsHeader.ArrivalMovementHeader.BM_InBondEntryType = "ABC";
			AssertEquals($"{messageType} -ABC- {controlReference.Name}", IsControlVisible(messageSendingObject), Layout.IsVisible(controlReference, messageSendingObject));
		}
	}

	bool IsControlVisible(MessageSendingObject messageSendingObject)
	{
		return messageSendingObject.MessageType == ArrivalMessageSendingObjectTypeList.Codes.URM &&
			(string.IsNullOrEmpty(messageSendingObject.NctsHeader.ArrivalMovementHeader.BM_InBondEntryType) || messageSendingObject.NctsHeader.ArrivalMovementHeader.BM_InBondEntryType == NctsPhase5DeclarationTypeList.Codes.TIR);
	}

	protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

	protected override int ExpectedMaxColumns => 1;

	protected override ArrivalAdditionalDetailsLayoutBuilder<MessageSendingObject> GetColumnLayoutBuilderForTesting()
	{
		var builder = new ArrivalAdditionalDetailsLayoutBuilder<MessageSendingObject>();
		builder.AddControlBag(ArrivalAdditionalDetailsControlBag.Instance);
		return builder;
	}

	string[] MessageTypes => new[]
	{
		ArrivalMessageSendingObjectTypeList.Codes.ARN,
		ArrivalMessageSendingObjectTypeList.Codes.URM,
		ArrivalMessageSendingObjectTypeList.Codes.RNM,
		string.Empty
	};

	PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new ArrivalAdditionalDetailsLayout()).Layout);
	PanelLayout layout;
}
