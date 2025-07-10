using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	sealed class MessageSendingObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckMB_Date()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var messageSending = new MessageSendingObject(moveDetail, ActionCode.VesselDeparture);
			messageSending.ShouldValidateDate = () => true;
			messageSending.MB_Send = true;
			messageSending.MB_Date = ZDateTime.Empty;
			AssertHasMessageErrorContaining(messageSending.MB_DateInfo, MandatoryValidation.YouHaveNotEntered);

			messageSending.MB_Date = ZDateTime.Today;
			AssertNoMessageErrorContaining(messageSending.MB_DateInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(messageSending.MB_DateInfo, ValidationConstants.MessageSending.ExportDateForVesselDeparture.ToString());

			messageSending.MB_Date = ZDateTime.Today.AddDays(-1);
			AssertNoMessageErrorContaining(messageSending.MB_DateInfo, ValidationConstants.MessageSending.ExportDateForVesselDeparture.ToString());

			messageSending.MB_Date = ZDateTime.Today.AddDays(1);
			AssertHasMessageErrorContaining(messageSending.MB_DateInfo, ValidationConstants.MessageSending.ExportDateForVesselDeparture.ToString());

			messageSending = new MessageSendingObject(moveDetail, ActionCode.VesselArrival);
			messageSending.ShouldValidateDate = () => true;
			messageSending.MB_Send = true;
			messageSending.MB_Date = ZDateTime.Empty;
			AssertHasMessageErrorContaining(messageSending.MB_DateInfo, MandatoryValidation.YouHaveNotEntered);

			messageSending = new MessageSendingObject(moveDetail, ActionCode.ChangeEstDateOfArrival);
			messageSending.ShouldValidateDate = () => true;
			messageSending.MB_Send = true;
			messageSending.MB_Date = ZDateTime.Empty;
			AssertHasMessageErrorContaining(messageSending.MB_DateInfo, MandatoryValidation.YouHaveNotEntered);

			messageSending = new MessageSendingObject(moveDetail, ActionCode.VesselArrival);
			messageSending.ShouldValidateDate = () => true;
			messageSending.MB_Send = true;
			messageSending.MB_Date = ZDateTime.Today.AddDays(2);
			AssertHasMessageErrorContaining(messageSending.MB_DateInfo, ValidationConstants.MessageSending.ArrivalDateForVesselArrivalOrChangeDateEvent.ToString());

			messageSending.MB_Date = ZDateTime.Today.AddDays(-2);
			AssertNoMessageErrorContaining(messageSending.MB_DateInfo, ValidationConstants.MessageSending.ArrivalDateForVesselArrivalOrChangeDateEvent.ToString());
		}

		public void TestCheckMB_SendWhenNotOnFile()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var messageSending = new MessageSendingObject(moveDetail, ActionCode.Creating);
			messageSending.MB_Send = true;
			var actionCodeList = new ActionCode[] { ActionCode.AmendingAdd };
			foreach (var actionCode in actionCodeList)
			{
				moveDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.NotOnFile;
				messageSending = new MessageSendingObject(moveDetail, actionCode);
				messageSending.MB_Send = true;
				AssertNoNotifications(messageSending.MB_BillActionCodeInfo);
				AssertHasWarningContaining(messageSending.MB_SendInfo, ValidationConstants.MessageSending.NotUseAmendmentManifestIsNotOnFile.ToString());
				AssertEquals(ValidationConstants.MessageSending.NotUseAmendmentManifestIsNotOnFile.ToString(), "Manifest is not on file. If the vessel has not been arrived yet, you should send an Original Manifest, not an amendment.");
			}

			foreach (var actionCode in actionCodeList)
			{
				moveDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.OnFile;
				messageSending = new MessageSendingObject(moveDetail, actionCode);
				messageSending.MB_Send = true;
				AssertNoNotifications(messageSending.MB_BillActionCodeInfo);
				AssertNoWarningContaining(messageSending.MB_SendInfo, ValidationConstants.MessageSending.NotUseAmendmentManifestIsNotOnFile.ToString());
			}
		}

		public void TestCheckMB_PortOfUnladingOverride()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4117", "Test Name", startDate, endDate);
			newFactory.Save();

			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var messageSending = new MessageSendingObject(moveDetail, ActionCode.AmendingAdd);

			messageSending.MB_PortOfUnladingOverride = "ASDF";
			AssertEquals(true, messageSending.MB_PortOfUnladingOverrideInfo.HasWarning("You have not entered a valid code."));

			messageSending.MB_PortOfUnladingOverride = "4117";
			AssertEquals(false, messageSending.MB_PortOfUnladingOverrideInfo.HasWarning("You have not entered a valid code."));
		}

		public void TestCheckMB_BillActionCode()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var messageSending = new MessageSendingObject(moveDetail, ActionCode.Creating);
			messageSending.MB_Send = true;
			AssertEquals(true, messageSending.MB_BillActionCodeInfo.ReadOnly);
			messageSending.MB_BillActionCode = ZString.Empty;
			AssertNoNotifications(messageSending.MB_BillActionCodeInfo);

			var actionCodeList = new ActionCode[] { ActionCode.AmendingAdd, ActionCode.AmendingDelete, ActionCode.AmendingUpdate };
			foreach (var actionCode in actionCodeList)
			{
				moveDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.OnFile;
				messageSending = new MessageSendingObject(moveDetail, actionCode);
				messageSending.MB_Send = true;
				AssertEquals(false, messageSending.MB_BillActionCodeInfo.ReadOnly);
				messageSending.MB_BillActionCode = ZString.Empty;
				AssertHasMessageErrorContaining(messageSending.MB_BillActionCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(messageSending.MB_BillActionCodeInfo, ListValidation.InvalidCodeMessageError);
				messageSending.MB_BillActionCode = "!";
				AssertNoMessageErrorContaining(messageSending.MB_BillActionCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining(messageSending.MB_BillActionCodeInfo, ListValidation.InvalidCodeMessageError);
				messageSending.MB_BillActionCode = AMSBillSendingActionCodeList.Codes.AddBill;
				AssertNoMessageErrorContaining(messageSending.MB_BillActionCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(messageSending.MB_BillActionCodeInfo, ListValidation.InvalidCodeMessageError);

				if (actionCode == ActionCode.AmendingAdd)
				{
					AssertHasMessageError(messageSending.MB_BillActionCodeInfo, ValidationConstants.MessageSending.BillIsAlreadyOnFileUseUpdateInstead.ToString());
				}
				else
				{
					AssertNoMessageError(messageSending.MB_BillActionCodeInfo, ValidationConstants.MessageSending.BillIsAlreadyOnFileUseUpdateInstead.ToString());
				}
				if (actionCode == ActionCode.AmendingAdd)
				{
					messageSending.MB_BillActionCode = AMSBillSendingActionCodeList.Codes.ReplaceManifestQuantity;
					AssertHasWarning(messageSending.MB_BillActionCodeInfo, ValidationConstants.MessageSending.ReplaceMessageWillOnlyReplaceManifestQuantity.ToString());
				}
				moveDetail.B9_CustomsStatus = ZString.Empty;
				messageSending.MB_BillActionCode = AMSBillSendingActionCodeList.Codes.AddBill;
				AssertNoMessageError(messageSending.MB_BillActionCodeInfo, ValidationConstants.MessageSending.BillIsAlreadyOnFileUseUpdateInstead.ToString());

				messageSending.MB_Send = false;
				messageSending.MB_BillActionCode = ZString.Empty;
				AssertNoMessageErrorContaining(messageSending.MB_BillActionCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(messageSending.MB_BillActionCodeInfo, ListValidation.InvalidCodeMessageError);

				messageSending.MB_BillActionCode = "!";
				AssertNoMessageErrorContaining(messageSending.MB_BillActionCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(messageSending.MB_BillActionCodeInfo, ListValidation.InvalidCodeMessageError);
				if (actionCode == ActionCode.AmendingAdd)
				{
					messageSending.MB_BillActionCode = AMSBillSendingActionCodeList.Codes.AddBill;
					AssertNoMessageError(messageSending.MB_BillActionCodeInfo, ValidationConstants.MessageSending.BillIsAlreadyOnFileUseUpdateInstead.ToString());
				}
				if (actionCode == ActionCode.AmendingAdd)
				{
					messageSending.MB_BillActionCode = AMSBillSendingActionCodeList.Codes.ReplaceManifestQuantity;
					AssertNoWarning(messageSending.MB_BillActionCodeInfo, ValidationConstants.MessageSending.ReplaceMessageWillOnlyReplaceManifestQuantity.ToString());
				}
			}

			var inBondMoveHeader = header.InBondMovementHeaders.AddNew();
			var inBondMoveDetail = inBondMoveHeader.MovementDetails.AddNew(bill.PK);
			messageSending = new MessageSendingObject(moveDetail, ActionCode.InBondArrival);
			messageSending.MB_Send = true;
			messageSending.MB_BillActionCode = ZString.Empty;
			AssertEquals(false, messageSending.MB_BillActionCodeInfo.ReadOnly);
			AssertHasErrorContaining(messageSending.MB_BillActionCodeInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(messageSending.MB_BillActionCodeInfo, ListValidation.InvalidCodeError);
			messageSending.MB_BillActionCode = "!";
			AssertNoErrorContaining(messageSending.MB_BillActionCodeInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(messageSending.MB_BillActionCodeInfo, ListValidation.InvalidCodeError);
			messageSending.MB_BillActionCode = InBondAndVesselEventMessageCodeList.Codes.ArriveInBond;
			AssertNoErrorContaining(messageSending.MB_BillActionCodeInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(messageSending.MB_BillActionCodeInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckMB_CustomsStatus()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_B0 = bill.PK;
			var messageSending = new MessageSendingObject(moveDetail, ActionCode.Creating);
			messageSending.MB_Send = true;
			moveDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.OnFile;
			messageSending.Validation.ValidateMB_CustomsStatus();
			AssertHasMessageError(messageSending.MB_CustomsStatusInfo, ValidationConstants.MessageSending.BillIsAlreadyOnFileUseUpdateInsteadForCreatingAction.ToString());

			moveDetail.B9_CustomsStatus = ZString.Empty;
			messageSending.Validation.ValidateMB_CustomsStatus();
			AssertNoMessageError(messageSending.MB_CustomsStatusInfo, ValidationConstants.MessageSending.BillIsAlreadyOnFileUseUpdateInsteadForCreatingAction.ToString());

			messageSending.MB_Send = false;
			messageSending.Validation.ValidateMB_CustomsStatus();
			AssertNoMessageError(messageSending.MB_CustomsStatusInfo, ValidationConstants.MessageSending.BillIsAlreadyOnFileUseUpdateInsteadForCreatingAction.ToString());

			moveDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.OnFile;
			messageSending.Validation.ValidateMB_CustomsStatus();
			AssertNoMessageError(messageSending.MB_CustomsStatusInfo, ValidationConstants.MessageSending.BillIsAlreadyOnFileUseUpdateInsteadForCreatingAction.ToString());

			messageSending = new MessageSendingObject(moveDetail, ActionCode.Creating);
			moveDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.NotOnFile;
			messageSending.Validation.ValidateMB_CustomsStatus();
			AssertNoMessageError(messageSending.MB_CustomsStatusInfo, ValidationConstants.MessageSending.BillIsAlreadyOnFileUseUpdateInsteadForCreatingAction.ToString());
		}

		public void TestCheckMB_AmendmentCode()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_B0 = bill.PK;
			var messageSending = new MessageSendingObject(moveDetail, ActionCode.Creating);
			messageSending.MB_Send = true;
			AssertEquals(true, messageSending.MB_AmendmentCodeInfo.ReadOnly);
			messageSending.MB_AmendmentCode = ZString.Empty;
			AssertNoMessageErrorContaining(messageSending.MB_AmendmentCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(messageSending.MB_AmendmentCodeInfo, ListValidation.InvalidCodeMessageError);

			var actionCodeList = new ActionCode[] { ActionCode.AmendingAdd, ActionCode.AmendingDelete, ActionCode.AmendingUpdate };
			foreach (var actionCode in actionCodeList)
			{
				messageSending = new MessageSendingObject(moveDetail, actionCode);
				messageSending.MB_Send = true;
				AssertEquals(false, messageSending.MB_AmendmentCodeInfo.ReadOnly);
				messageSending.MB_AmendmentCode = ZString.Empty;
				AssertHasMessageErrorContaining(messageSending.MB_AmendmentCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(messageSending.MB_AmendmentCodeInfo, ListValidation.InvalidCodeMessageError);

				messageSending.MB_AmendmentCode = "Z1";
				AssertNoMessageErrorContaining(messageSending.MB_AmendmentCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining(messageSending.MB_AmendmentCodeInfo, ListValidation.InvalidCodeMessageError);

				messageSending.MB_AmendmentCode = AMSAmendmentCodeList.Codes._04;
				AssertNoMessageErrorContaining(messageSending.MB_AmendmentCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(messageSending.MB_AmendmentCodeInfo, ListValidation.InvalidCodeMessageError);

				messageSending.MB_Send = false;
				messageSending.MB_AmendmentCode = ZString.Empty;
				AssertNoMessageErrorContaining(messageSending.MB_AmendmentCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(messageSending.MB_AmendmentCodeInfo, ListValidation.InvalidCodeMessageError);

				messageSending.MB_AmendmentCode = "Z1";
				AssertNoMessageErrorContaining(messageSending.MB_AmendmentCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(messageSending.MB_AmendmentCodeInfo, ListValidation.InvalidCodeMessageError);
			}

			messageSending.MB_Send = false;
			messageSending.MB_AmendmentCode = "Z1";
			AssertNoMessageErrorContaining(messageSending.MB_AmendmentCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(messageSending.MB_AmendmentCodeInfo, ListValidation.InvalidCodeMessageError);
			messageSending.MB_AmendmentCode = ZString.Empty;
			AssertNoMessageErrorContaining(messageSending.MB_AmendmentCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(messageSending.MB_AmendmentCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestValidateShouldSendManifestAmendmentMessage()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_B0 = bill.PK;

			var messageSending = new MessageSendingObject(moveDetail, ActionCode.Creating);
			AssertNoRowMessageErrorContaining(messageSending, ValidationConstants.MessageSending.AmendmentMessageShouldBeFiled);

			moveHeader = (CusInBondMoveHeader)header.MovementHeaders.AddNew();
			moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_MessageStatus = AMSBillMessageStatusList.Codes.Adding;

			var transmitMessage = moveHeader.Messages.AddNew(typeof(AMSEDIMessage));
			transmitMessage.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			transmitMessage.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			transmitMessage.EM_MessageNum = "~10001";

			messageSending = new MessageSendingObject(moveDetail, ActionCode.Creating);
			AssertNoRowMessageErrorContaining(messageSending, ValidationConstants.MessageSending.AmendmentMessageShouldBeFiled);

			moveHeader = (CusInBondMoveHeader)header.MovementHeaders.AddNew();
			moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_MessageStatus = AMSBillMessageStatusList.Codes.Adding;

			transmitMessage = moveHeader.Messages.AddNew(typeof(AMSEDIMessage));
			transmitMessage.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			transmitMessage.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			transmitMessage.EM_MessageNum = "~10001";

			var responseMessage = moveHeader.Messages.AddNew(typeof(AMSEDIMessage));
			responseMessage.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			responseMessage.EM_MessageNum = "~10001";

			moveDetail.B9_MessageStatus = AMSBillMessageStatusList.Codes.Added;
			messageSending = new MessageSendingObject(moveDetail, ActionCode.Creating);
			AssertNoRowMessageErrorContaining(messageSending, ValidationConstants.MessageSending.AmendmentMessageShouldBeFiled);

			moveHeader = (CusInBondMoveHeader)header.MovementHeaders.AddNew();
			moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_MessageStatus = AMSBillMessageStatusList.Codes.Adding;

			transmitMessage = moveHeader.Messages.AddNew(typeof(AMSEDIMessage));
			transmitMessage.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			transmitMessage.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			transmitMessage.EM_MessageNum = "~10001";

			responseMessage = moveHeader.Messages.AddNew(typeof(AMSEDIMessage));
			responseMessage.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			responseMessage.EM_MessageNum = "~10001";

			responseMessage.EM_MessageText = "ACR          MR15012114503073208                                                M01CCLL11DEHOUSTON EXPRESS        FR345     000096 9204790                      M02VEJS14006824_CCLL4482                                                        P011401011515                                                                   W01                          1401000001059 A01/M13 REQ AS OF ACT/ARR            W02OTT11203262105430100000000000000000000000000000000000017000000000000000      ZCR          MR                   00006                                         ";
			moveDetail.B9_MessageStatus = AMSBillMessageStatusList.Codes.Error;
			messageSending = new MessageSendingObject(moveDetail, ActionCode.Creating);
			AssertHasRowMessageErrorContaining(messageSending, ValidationConstants.MessageSending.AmendmentMessageShouldBeFiled);

			moveHeader = (CusInBondMoveHeader)header.MovementHeaders.AddNew();
			moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_MessageStatus = AMSBillMessageStatusList.Codes.Adding;

			transmitMessage = moveHeader.Messages.AddNew(typeof(AMSEDIMessage));
			transmitMessage.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			transmitMessage.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			transmitMessage.EM_MessageNum = "~10001";

			responseMessage = moveHeader.Messages.AddNew(typeof(AMSEDIMessage));
			responseMessage.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			responseMessage.EM_MessageNum = "~10001";

			responseMessage.EM_MessageText = "ACR          MR15020817410550296                                                M01OTT110GBNVO STEP2              389       000001 2111112                      W01                              000001151 INCORRECT VESSEL NAME                M02342890890342_OTT18230                                                        P012704020915                                                                   W01                          2704000001102 NO BILLS PROCESSD FOR PORT           W02OTT11502081741040100000000000000000000000000000000000018000000000000000      ZCR          MR                   00006";
			messageSending = new MessageSendingObject(moveDetail, ActionCode.Creating);
			AssertNoRowMessageErrorContaining(messageSending, ValidationConstants.MessageSending.AmendmentMessageShouldBeFiled);
		}
	}
}
