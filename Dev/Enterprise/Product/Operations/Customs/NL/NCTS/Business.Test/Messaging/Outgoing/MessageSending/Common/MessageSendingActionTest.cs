using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(MessageSendingAction))]
sealed class MessageSendingActionTest : EU.NCTS.Business.Testing.NctsHeaderMessageSendingObjectTest
{
	public void TestGetDefaultMessageType()
	{
		commonMovement.BM_SubApplicationCode = "D";
		commonMovement.BM_AdditionalDeclarationType = "A";
		commonMovement.BM_CustomsStatus = string.Empty;
		commonMovement.BM_Phase = string.Empty;
		var goodsLocation = ((NctsDepartureMovementHeader)commonMovement).GoodsLocation;
		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
		goodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.ApprovedPlace;

		var messageSendingAction1 = new MessageSendingAction(commonMovement);

		CombineAssertions(() =>
		{
			AssertEquals("MessageTypeList has only one code", 1, messageSendingAction1.Lookups.MessageTypeList.Count);
			AssertEquals("MessageType uses the only code", NctsMessageTypeListNL.Codes.Declaration, messageSendingAction1.MessageType);
			commonMovement.BM_CustomsStatus = "MRN";
			commonMovement.BM_Phase = "015";
			var messageSendingAction2 = new MessageSendingAction(commonMovement);
			AssertEquals("MessageTypeList has multiple codes", 2, messageSendingAction2.Lookups.MessageTypeList.Count);
			AssertNullOrEmpty("MessageType is empty", messageSendingAction2.MessageType);
		});
	}

	public new void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new MessageSendingAction(null));
	}

	public void TestMessageSendingActionLookups()
	{
		AssertType<MessageSendingActionLookups>(messageSendingAction.Lookups);
	}

	public void TestPresentationDateTime_Arrival_ReadOnly()
	{
		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		var arrivalMovement = arrivalHeader.ArrivalMovementHeader;
		var arrivalMessageSendingAction = new MessageSendingAction(arrivalMovement);

		var presentationDateTimeInfo = arrivalMessageSendingAction.PresentationDateTimeInfo;
		CombineAssertions(() =>
		{
			arrivalMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			AssertEquals("PresentationDateTime should be ReadOnly", true, presentationDateTimeInfo.ReadOnly);
			arrivalMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			AssertEquals("PresentationDateTime should not be ReadOnly", false, presentationDateTimeInfo.ReadOnly);
		});
	}

	public void TestPresentationDateTime_Departure_ReadOnly()
	{
		var presentationDateTimeInfo = messageSendingAction.PresentationDateTimeInfo;
		CombineAssertions(() =>
		{
			commonMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationRejected;
			commonMovement.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.MrnAllocated;
			AssertEquals("PresentationDateTime should not be ReadOnly for DRJ, DMA", false, presentationDateTimeInfo.ReadOnly);

			commonMovement.BM_CustomsStatus = NctsMovementHeaderTransactionStatusList.Codes.AmendmentSent;
			AssertEquals("PresentationDateTime should be ReadOnly for MAM", true, presentationDateTimeInfo.ReadOnly);

			commonMovement.BM_CustomsStatus = NctsMovementHeaderTransactionStatusList.Codes.AmendmentSent;
			messageSendingAction.MessageType = NctsMessageTypeListNL.Codes.Amendment;
			AssertEquals("PresentationDateTime should not be ReadOnly for AMD, unless log with GIV reference exists", false, presentationDateTimeInfo.ReadOnly);

			nctsHeader.Logs.CreateRecreateOrUpdateEventLog(new EventValue(Events.CustomsEntryStatus, eventTime: ZDateTimeOffset.Now, reference: NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid));
			messageSendingAction = new MessageSendingAction(commonMovement);
			messageSendingAction.MessageType = NctsMessageTypeListNL.Codes.Amendment;
			AssertEquals("PresentationDateTime should be ReadOnly for AMD and log with GIV reference exists", true, messageSendingAction.PresentationDateTimeInfo.ReadOnly);
		});
	}

	public void TestArrivalDate()
	{
		commonMovement.BM_ArrivalDate = ZDateTime.Empty;
		messageSendingAction.PresentationDateTime = ZDateTime.BrettsBirthday;
		AssertEquals(ZDateTime.BrettsBirthday, commonMovement.BM_ArrivalDate);
	}

	public void TestPresentationDateTime_Caption()
	{
		AssertEquals("Presentation Date And Time", DataBoundResourceStrings.GetDataForProperty(messageSendingAction.PresentationDateTimeInfo).Caption);
	}

	public void TestTCI11_Caption()
	{
		AssertEquals("TCI11 Date", DataBoundResourceStrings.GetDataForProperty(messageSendingAction.TCI11Info).Caption);
	}

	public void TestAgreeWithMinorDiscrepancies_Caption()
	{
		AssertEquals("Agree with minor discrepancies?", DataBoundResourceStrings.GetDataForProperty(messageSendingAction.AgreeWithMinorDiscrepanciesInfo).Caption);
	}

	public void TestIsPresentationDateTimeEnabledForCustomsStatusAndPhase()
	{
		var variations = GetPresentationDateTimeEnabledVariations();

		CombineAssertions(() =>
		{
			foreach (var (customsStatus, phase, enabled) in variations)
			{
				commonMovement.BM_CustomsStatus = customsStatus;
				commonMovement.BM_Phase = phase;
				AssertEquals($"CustomsStatus: {customsStatus}, Phase: {phase}, Expected Enabled: {enabled}", enabled, !messageSendingAction.PresentationDateTimeInfo.ReadOnly);
			}
		});
	}

	public void TestShowValidationErrors()
	{
		var action = new MessageSendingAction(commonMovement);

		CombineAssertions(() =>
		{
			AssertEquals("default is true", true, action.ShowValidationErrors);

			action.MessageType = NctsMessageTypeListNL.Codes.InvalidationCancellation;
			AssertEquals("MessageType is INV, ShowValidationErrors is false", false, action.ShowValidationErrors);

			action.MessageType = NctsMessageTypeListNL.Codes.ResponseOnRequestForNonArrivedMovement;
			AssertEquals("MessageType is RNM, ShowValidationErrors is false", false, action.ShowValidationErrors);

			action.MessageType = NctsMessageTypeListNL.Codes.RequestARelease;
			AssertEquals("MessageType is RRL, ShowValidationErrors is false", false, action.ShowValidationErrors);
		});
	}

	List<(string customsStatus, string phase, bool enabled)> GetPresentationDateTimeEnabledVariations()
	{
		var variations = new List<(string customsStatus, string phase, bool enabled)>();
		var customsStatuses = new NctsTransitStatusList().GetAllCodes().ToList<string>();
		customsStatuses.Add(NctsMessageTypeListNL.Codes.Amendment);
		foreach (var customsStatus in customsStatuses)
		{
			foreach (var phase in new NctsMovementHeaderTransactionStatusList().GetAllCodes())
			{
				bool enabled = ExpectPresentationDateTimeEnabled(customsStatus, phase);
				variations.Add((customsStatus, phase, enabled));
			}
		}
		return variations;
	}

	bool ExpectPresentationDateTimeEnabled(string customsStatus, string phase)
	{
		var enabledForCustomsStatus = false;
		var enabledForPhase = true;

		switch (customsStatus)
		{
			case NctsTransitStatusList.Codes.DeclarationAccepted:
			case NctsTransitStatusList.Codes.DeclarationRejected:
			case NctsTransitStatusList.Codes.RequestForAmendment:
			case NctsMessageTypeListNL.Codes.Amendment:
			case NctsTransitStatusList.Codes.Unknown:
				enabledForCustomsStatus = true;
				break;
		}

		if (enabledForCustomsStatus)
		{
			switch (phase)
			{
				case NctsMovementHeaderTransactionStatusList.Codes.AmendmentSent:
				case NctsMovementHeaderTransactionStatusList.Codes.InvalidationSent:
				case NctsMovementHeaderTransactionStatusList.Codes.DeclarationSent:
				case NctsMovementHeaderTransactionStatusList.Codes.RequestForReleaseSent:
				case NctsMovementHeaderTransactionStatusList.Codes.InformationNonArrivedMovementSent:
				case NctsMovementHeaderTransactionStatusList.Codes.PresentationNotificationSent:
				case NctsMovementHeaderTransactionStatusList.Codes.ArrivalNotificationSent:
				case NctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarksSent:
					enabledForPhase = false;
					break;
			}
		}

		return enabledForCustomsStatus && enabledForPhase;
	}

	protected override BusinessObject GetNewBusinessObject() => messageSendingAction;

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		commonMovement = nctsHeader.MovementHeader;
		messageSendingAction = new MessageSendingAction(commonMovement);
	}

	NctsHeader nctsHeader;
	NctsCommonMovementHeader commonMovement;
	MessageSendingAction messageSendingAction;
}
