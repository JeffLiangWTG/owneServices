using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(MessageSendingObject))]
	sealed class MessageSendingBillTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSettingMB_Send_DoesntTriggerValidation()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var sendingBill = new MessageSendingObject(moveDetail, ActionCode.Creating);

			bill.RunPreSaveValidation();
			Assert("precondition", bill.Notifications.Any());

			bill.ClearAllNotifications();
			Assert("precondition", !bill.Notifications.Any());

			sendingBill.MB_Send = true;
			Assert("Should not have any notifications as Validation didn't run", !bill.Notifications.Any());

			ErrorReporter.Clear();
		}

		public void TestICommonBillManifestMessageAttachee()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;

			var sendingBill = new MessageSendingObject(moveDetail, ActionCode.Creating);
			ICommonBillManifestMessageAttachee manifest = sendingBill;
			header.BH_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK), manifest.Branch);
			AssertEquals(Factory, manifest.Factory);
			AssertEquals(moveHeader.Messages, manifest.Messages);
			moveHeader.BM_CustomsStatus = "BDD";
			AssertEquals("BDD", manifest.MessageStatus);
			manifest.MessageStatus = "KDS";
			AssertEquals("KDS", moveHeader.BM_CustomsStatus);
			AssertEquals(consol, manifest.TopLevelBusinessObject);
			AssertEquals(ControllerIDs.JobConsol, manifest.ControllerID);
			AssertEquals(consol.PK.ToGuid(), manifest.BusinessObjectPK);
			AssertEquals(consol.Logs, manifest.TopLevelBusinessObjectLogs);
			header.BH_CarrierSCAC = "SD43";
			AssertEquals("SD43", manifest.CarrierCode);
			header.BH_ImportTransportMode = TransportTypeList.Codes.Truck;
			AssertEquals(TransportTypeList.Codes.Truck, manifest.ModeOfTransportationCode);
			header.BH_ImportConveyanceCountry = "DS";
			AssertEquals("DS", manifest.ConveyanceCountryCode);
			header.BH_ImportConveyanceName = "IMP DUMMY VESSEL";
			AssertEquals("IMP DUMMY VESSEL", manifest.ConveyanceName);
			header.BH_VoyageNumber = "E343";
			AssertEquals("E343", manifest.VoyageNumber);
			AssertEquals("", manifest.ManifestSequenceNumber);
			moveHeader.BM_ManifestSequenceNumber = "000002";
			AssertEquals("000002", manifest.ManifestSequenceNumber);
			AssertEquals(ZBool.False, manifest.IsPaperlessMIBParticipant);
			header.BH_LloydsNumber = "9323423";
			AssertEquals("9323423", manifest.ConveyanceCode);
			header.BH_ExportFlag = ExportTypeList.Codes.OutboundCargo;
			AssertEquals(ZBool.True, manifest.IsOutboundCargo);

			AssertEquals(manifest, manifest.PortDetails);
		}

		public void TestIACEBillManifestMessageAttacheeMembers()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;

			var sendingObject = new MessageSendingObject(moveDetail, ActionCode.Creating);
			sendingObject.MB_Date = new ZDateTime(2020, 02, 02);
			sendingObject.MB_ForeignDeparturePort = "Test";
			IACEBillManifestMessageAttachee attachee = sendingObject;
			AssertEquals(sendingObject, attachee.BillOfLadingDetails);
			AssertEquals(bill.PK.ToString(), attachee.BillPKAsString);
			AssertEquals("Test", attachee.ForeignDeparturePort);
			AssertEquals(new ZDateTime(2020, 02, 02), attachee.EventDateTime);
			AssertEquals(bill.Messages, attachee.BillMessages);
		}

		public void TestIBillManifestMessageAttachee_UpdateOutgoingBillStatus()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;

			foreach (var pair in new[]
				{
					new KeyValuePair<ActionCode, string>(ActionCode.InBondArrival, AMSBillMessageStatusList.Codes.AwaitingArrival),
					new KeyValuePair<ActionCode, string>(ActionCode.InBondDiversion, AMSBillMessageStatusList.Codes.AwaitingDiversion),
					new KeyValuePair<ActionCode, string>(ActionCode.InBondExportation, AMSBillMessageStatusList.Codes.AwaitingExportation),
					new KeyValuePair<ActionCode, string>(ActionCode.InBondTransferOfLiability, AMSBillMessageStatusList.Codes.AwaitingTransferOfLiability),
					new KeyValuePair<ActionCode, string>(ActionCode.SubsequentInBondAmendment, AMSBillMessageStatusList.Codes.AwaitingDeparture),
					new KeyValuePair<ActionCode, string>(ActionCode.SubsequentInBondOriginal, AMSBillMessageStatusList.Codes.AwaitingDeparture),
					new KeyValuePair<ActionCode, string>(ActionCode.SubsequentInBondDelete, AMSBillMessageStatusList.Codes.Deleting),
					new KeyValuePair<ActionCode, string>(ActionCode.CancelPermitToTransfer, AMSBillMessageStatusList.Codes.Deleting),
					new KeyValuePair<ActionCode, string>(ActionCode.Creating, AMSBillMessageStatusList.Codes.Adding),
					new KeyValuePair<ActionCode, string>(ActionCode.AmendingUpdate, AMSBillMessageStatusList.Codes.Updating)
				})
			{
				var sendingObject = new MessageSendingObject(moveDetail, pair.Key);
				sendingObject.MB_BillActionCode = AMSBillSendingActionCodeList.Codes.ReplaceManifestQuantity;
				IACEBillManifestMessageAttachee manifest = sendingObject;
				manifest.UpdateOutgoingBillStatus();
				AssertEquals(pair.Value, moveDetail.B9_MessageStatus);
			}
		}

		public void TestIPortMembers_NVOCC()
		{
			var helper = new MasterFilesTestHelper(Factory);
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var sendingBill = new MessageSendingObject(moveDetail, ActionCode.Creating);
			ICommonBillManifestMessageAttachee manifest = sendingBill;
			IPort port = sendingBill;
			header.BH_RL_NKPortUnlading = helper.USLAX.RL_Code;
			AssertEquals(MasterFilesTestHelper.USLAXScheduleDOrK, port.DistrictPortOfUnladingCode);
			header.BH_ETA = ZDateTime.BrettsBirthday;
			AssertEquals(ZDate.BrettsBirthday, port.OriginalEstimatedDate);
			AssertEquals(1, port.NumberOfBillsOfLadingForPort);
			moveHeader.MovementDetails.AddNew();
			AssertEquals(1, port.NumberOfBillsOfLadingForPort);
			moveHeader.MovementDetails.AddNew();
			AssertEquals(1, port.NumberOfBillsOfLadingForPort);
			header.BH_FIRMS = "K23K";
			AssertEquals("K23K", port.FIRMSCode);
			header.BH_ETA = new ZDateTime(2011, 1, 23, 15, 25, 53);
			AssertEquals("1525", port.Time);
		}

		public void TestIPortMembers_VOCC()
		{
			var helper = new MasterFilesTestHelper(Factory);
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.BH_TransitDirection = DirectionTypeList.Codes.MVOCC;
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var sendingBill = new MessageSendingObject(moveDetail, ActionCode.Creating);
			ICommonBillManifestMessageAttachee manifest = sendingBill;
			IPort port = sendingBill;
			bill.B0_RL_NKInBondPortOfDest = helper.USLAX.RL_Code;
			bill.B0_DateOfDischarge = ZDate.BrettsBirthday;
			bill.B0_Firms = "K23K";
			AssertEquals(MasterFilesTestHelper.USLAXScheduleDOrK, port.DistrictPortOfUnladingCode);
			AssertEquals(ZDate.BrettsBirthday, port.OriginalEstimatedDate);
			AssertEquals(1, port.NumberOfBillsOfLadingForPort);
			AssertEquals("K23K", port.FIRMSCode);
			AssertEquals("0000", port.Time);
		}

		public void TestRegisterAndUnRegisterBillAsEditableChildObject()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var sendingBill = new MessageSendingObject(moveDetail, ActionCode.Creating);
			sendingBill.MB_Send = true;
			AssertEquals(true, sendingBill.IsRegisteredEditableChildObject(bill));
			sendingBill.MB_Send = false;
			AssertEquals(false, sendingBill.IsRegisteredEditableChildObject(bill));
		}

		public void TestUnRegisterBillAsEditableChildObject()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = ZString.Empty;
			AssertHasMessageErrorContaining(bill.B0_MasterBillNumberInfo, MandatoryValidation.YouHaveNotEntered);
			var notification = bill.B0_MasterBillNumberInfo.Notifications.ToUniqueMessageListString();
			var moveDetail = bill.MovementDetail;
			var sendingBill = new MessageSendingObject(moveDetail, ActionCode.Creating);
			sendingBill.MB_Send = false;
			AssertEquals(false, sendingBill.IsRegisteredEditableChildObject(bill));
			AssertEquals(ValidationModes.None, bill.ValidationModes);
			AssertEquals(false, sendingBill.NotificationsIncludingChildren.ContainsNotificationContaining(notification));

			sendingBill.MB_Send = true;
			AssertEquals(true, sendingBill.IsRegisteredEditableChildObject(bill));
			AssertEquals(ValidationModes.InventoryRecord, bill.ValidationModes);
			AssertEquals(true, sendingBill.NotificationsIncludingChildren.ContainsNotificationContaining(notification));

			sendingBill.UnRegisterBillAsEditableChildObject();
			AssertEquals(false, sendingBill.IsRegisteredEditableChildObject(bill));
			AssertEquals(ValidationModes.InventoryRecord, bill.ValidationModes);
			AssertEquals(false, sendingBill.NotificationsIncludingChildren.ContainsNotificationContaining(notification));

			bill.B0_MasterBillNumber = "MB224332";
			AssertNoMessageErrorContaining(bill.B0_MasterBillNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestDefaults()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.ValidationModes = ValidationModes.UseParentValidateMode;
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_IssuerCode = "OTT1";
			bill.B0_MasterBillNumber = "MB1023- 2398";
			var moveDetail = bill.MovementDetail;
			moveDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.OnFile;
			moveDetail.B9_MessageStatus = AMSBillMessageStatusList.Codes.Added;
			var sendingBill = new MessageSendingObject(moveDetail, ActionCode.AmendingUpdate);
			AssertEquals(ActionCode.AmendingUpdate, sendingBill.ActionCode);
			AssertEquals(AMSBillCustomsStatusList.Codes.OnFile, sendingBill.MB_CustomsStatus);
			AssertEquals(AMSBillMessageStatusList.Codes.Added, sendingBill.MB_MessageStatus);
			AssertEquals(AMSBillSendingActionCodeList.Codes.ReplaceEntireBillDeleteAndAdd, sendingBill.MB_BillActionCode);
			AssertEquals("OTT1", sendingBill.MB_IssuerCode);
			AssertEquals("MB10232398", sendingBill.MB_BillOfLadingSequenceNumber);
			AssertEquals(true, sendingBill.MB_Send);
			AssertEquals(false, sendingBill.MB_VesselOverride);
		}

		public void TestUpdateAction()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;

			var sendingBill = new MessageSendingObject(moveDetail, ActionCode.AmendingAdd);
			AssertEquals(AMSBillSendingActionCodeList.Codes.AddBill, sendingBill.MB_BillActionCode);
			moveDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.OnFile;
			sendingBill.UpdateAction(ActionCode.AmendingAdd);
			AssertEquals(AMSBillSendingActionCodeList.Codes.ReplaceEntireBillDeleteAndAdd, sendingBill.MB_BillActionCode);
			sendingBill.UpdateAction(ActionCode.AmendingDelete);
			AssertEquals(AMSBillSendingActionCodeList.Codes.DeleteBill, sendingBill.MB_BillActionCode);
			sendingBill.UpdateAction(ActionCode.InBondArrival);
			AssertEquals(InBondAndVesselEventMessageCodeList.Codes.ArriveInBond, sendingBill.MB_BillActionCode);
			sendingBill.UpdateAction(ActionCode.InBondDiversion);
			AssertEquals(InBondAndVesselEventMessageCodeList.Codes.RequestForInBondDiversion, sendingBill.MB_BillActionCode);
			sendingBill.UpdateAction(ActionCode.InBondExportation);
			AssertEquals(InBondAndVesselEventMessageCodeList.Codes.ExportInBond, sendingBill.MB_BillActionCode);
			sendingBill.UpdateAction(ActionCode.InBondTransferOfLiability);
			AssertEquals(InBondAndVesselEventMessageCodeList.Codes.TransferOfInBondLiability, sendingBill.MB_BillActionCode);
			sendingBill.UpdateAction(ActionCode.CancelPermitToTransfer);
			AssertEquals(InBondAndVesselEventMessageCodeList.Codes.CancelPermitsToTransferArrivalByBillOfLading, sendingBill.MB_BillActionCode);
			sendingBill.UpdateAction(ActionCode.VesselArrival);
			AssertEquals(InBondAndVesselEventMessageCodeList.Codes.VesselArrival, sendingBill.MB_BillActionCode);
			sendingBill.UpdateAction(ActionCode.VesselDeparture);
			AssertEquals(InBondAndVesselEventMessageCodeList.Codes.VesselDeparture, sendingBill.MB_BillActionCode);
			sendingBill.UpdateAction(ActionCode.ChangeEstDateOfArrival);
			AssertEquals(InBondAndVesselEventMessageCodeList.Codes.ChangeInTheEstimatedDateOfArrival, sendingBill.MB_BillActionCode);
		}

		public void TestPopulateActionCodeAndAmendmentCode()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;

			var sendingBill = new MessageSendingObject(moveDetail, ActionCode.AmendingAdd);
			AssertEquals(AMSBillSendingActionCodeList.Codes.AddBill, sendingBill.MB_BillActionCode);
			AssertEquals(ZString.Empty, sendingBill.MB_AmendmentCode);

			bill.B0_BillActionCode = AMSBillSendingActionCodeList.Codes.ReplaceManifestQuantity;
			bill.B0_BillAmendmentCode = AMSAmendmentCodeList.Codes._05;
			sendingBill = new MessageSendingObject(moveDetail, ActionCode.AmendingAdd);
			AssertEquals(AMSBillSendingActionCodeList.Codes.ReplaceManifestQuantity, sendingBill.MB_BillActionCode);
			AssertEquals(AMSAmendmentCodeList.Codes._05, sendingBill.MB_AmendmentCode);
		}

		public void TestPurgeActionCodeAndAmendmentCode()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;

			bill.B0_BillActionCode = AMSBillSendingActionCodeList.Codes.ReplaceManifestQuantity;
			bill.B0_BillAmendmentCode = AMSAmendmentCodeList.Codes._05;

			var sendingBill = new MessageSendingObject(moveDetail, ActionCode.AmendingAdd);
			AssertEquals(AMSBillSendingActionCodeList.Codes.ReplaceManifestQuantity, sendingBill.MB_BillActionCode);
			AssertEquals(AMSAmendmentCodeList.Codes._05, sendingBill.MB_AmendmentCode);

			sendingBill.PurgeActionCodeAndAmendmentCode();
			AssertEquals(ZString.Empty, bill.B0_BillActionCode);
			AssertEquals(ZString.Empty, bill.B0_BillAmendmentCode);
		}

		public void TestGeneratePendingOriginalAdd()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;

			var sendingBill = new MessageSendingObject(moveDetail, ActionCode.AmendingAdd);
			sendingBill.GeneratePendingOriginalAdd();

			AssertEquals(1, moveHeader.Messages.Count);
			AssertNotNull(moveHeader.Messages.Where(x => x.EM_MessageSubType == AMSMessageSubTypeList.Codes.Creating && x.EM_SendWithMessageErrors && x.EM_Status == EDIMessage.Status.Pending).FirstOrDefault());
		}

		public void TestGetActualBillActionCode()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;

			var sendingBill = new MessageSendingObject(moveDetail, ActionCode.AmendingAdd);
			sendingBill.MB_BillActionCode = AMSBillSendingActionCodeList.Codes.ReplaceManifestQuantity;
			AssertEquals(ActionCode.AmendingUpdate, sendingBill.GetActualActionCode());

			sendingBill.MB_BillActionCode = AMSBillSendingActionCodeList.Codes.DeleteBill;
			AssertEquals(ActionCode.AmendingDelete, sendingBill.GetActualActionCode());

			sendingBill.MB_BillActionCode = AMSBillSendingActionCodeList.Codes.ReplaceEntireBillDeleteAndAdd;
			AssertEquals(ActionCode.AmendingDelete, sendingBill.GetActualActionCode());

			sendingBill = new MessageSendingObject(moveDetail, ActionCode.PermitToTransfer);
			AssertEquals(ActionCode.PermitToTransfer, sendingBill.GetActualActionCode());

			sendingBill = new MessageSendingObject(moveDetail, ActionCode.CancelPermitToTransfer);
			AssertEquals(ActionCode.CancelPermitToTransfer, sendingBill.GetActualActionCode());

			sendingBill = new MessageSendingObject(moveDetail, ActionCode.VesselArrival);
			AssertEquals(ActionCode.VesselArrival, sendingBill.GetActualActionCode());
		}

		public void TestMessageData_CarrierAssignedBatchNumberShouldHasBillOfLadingWhenItLongerThan12()
		{
			var header = GetHeader();
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "123456789123456789";
			var moveDetail = bill.MovementDetail;

			var sendingBill = new MessageSendingObject(moveDetail, ActionCode.Creating);
			AssertEquals(@"-----------------APLACR-----------------

-----------------INPM01-----------------
 Carrier Code (4-7)                :SD43
 Mode Of Transportation Code (8-9) :30
 Vessel Country Code (10-11)       :DS
 Voyage Number (35-39)             :E343
 Vessel Code (52-58)               :9323423

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :789123456789_<MSG PLACEHOLDER>

-----------------INPP01-----------------

-----------------INPJ01-----------------

-----------------INPB01-----------------
 Bill Of Lading Sequence Number (4-15)   :789123456789
 Manifest Quantity (21-30)               :0
 Weight (36-45)                          :0
 Bill Of Lading Status Indicator (48-48) :N

-----------------INPB02-----------------
 Second Notify Party1 (45-48) :SD43

-----------------APLZCR-----------------
 Number Of Transaction Detail Records (35-39) :0", sendingBill.MB_MessageContents.Trim());
		}

		public void TestMessageContentForVoyageNumberLongerThan5_ShouldTruncateRatherThan5Asterisks()
		{
			var header = GetHeader();
			header.BH_VoyageNumber = "123456789";
			header.MovementHeader.BM_CustomsStatus = "BDD";
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;

			var sendingBill = new MessageSendingObject(moveDetail, ActionCode.Creating);
			AssertEquals(@"-----------------APLACR-----------------

-----------------INPM01-----------------
 Carrier Code (4-7)                :SD43
 Mode Of Transportation Code (8-9) :30
 Vessel Country Code (10-11)       :DS
 Voyage Number (35-39)             :12345
 Vessel Code (52-58)               :9323423

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :_<MSG PLACEHOLDER>

-----------------INPP01-----------------

-----------------INPJ01-----------------

-----------------INPB01-----------------
 Manifest Quantity (21-30)               :0
 Weight (36-45)                          :0
 Bill Of Lading Status Indicator (48-48) :N

-----------------INPB02-----------------
 Second Notify Party1 (45-48) :SD43

-----------------APLZCR-----------------
 Number Of Transaction Detail Records (35-39) :0", sendingBill.MB_MessageContents.Trim());
		}

		[TestDate(2012, 04, 05)]
		public void TestMessageContentForACE()
		{
			var header = GetHeader();
			var moveHeader = header.MovementHeader;
			moveHeader.BM_CustomsStatus = "BDD";
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;

			var sendingBill = new MessageSendingObject(moveDetail, ActionCode.Creating);
			AssertEquals(@"-----------------APLACR-----------------

-----------------INPM01-----------------
 Carrier Code (4-7)                :SD43
 Mode Of Transportation Code (8-9) :30
 Vessel Country Code (10-11)       :DS
 Voyage Number (35-39)             :E343
 Vessel Code (52-58)               :9323423

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :_<MSG PLACEHOLDER>

-----------------INPP01-----------------

-----------------INPJ01-----------------

-----------------INPB01-----------------
 Manifest Quantity (21-30)               :0
 Weight (36-45)                          :0
 Bill Of Lading Status Indicator (48-48) :N

-----------------INPB02-----------------
 Second Notify Party1 (45-48) :SD43

-----------------APLZCR-----------------
 Number Of Transaction Detail Records (35-39) :0", sendingBill.MB_MessageContents.Trim());

			header.BH_LloydsNumber = ZString.Empty;
			sendingBill = new MessageSendingObject(moveDetail, ActionCode.Creating);
			AssertEquals(@"-----------------APLACR-----------------

-----------------INPM01-----------------
 Carrier Code (4-7)                :SD43
 Mode Of Transportation Code (8-9) :30
 Vessel Country Code (10-11)       :DS
 Vessel Name (12-34)               :IMP DUMMY VESSEL
 Voyage Number (35-39)             :E343

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :_<MSG PLACEHOLDER>

-----------------INPP01-----------------

-----------------INPJ01-----------------

-----------------INPB01-----------------
 Manifest Quantity (21-30)               :0
 Weight (36-45)                          :0
 Bill Of Lading Status Indicator (48-48) :N

-----------------INPB02-----------------
 Second Notify Party1 (45-48) :SD43

-----------------APLZCR-----------------
 Number Of Transaction Detail Records (35-39) :0", sendingBill.MB_MessageContents.Trim());

			header.BH_ImportConveyanceName = "THIS IS A VERY LONG IMP VESSEL NAME";
			header.BH_LloydsNumber = ZString.Empty;
			sendingBill = new MessageSendingObject(moveDetail, ActionCode.Creating);
			AssertEquals(@"-----------------APLACR-----------------

-----------------INPM01-----------------
 Carrier Code (4-7)                :SD43
 Mode Of Transportation Code (8-9) :30
 Vessel Country Code (10-11)       :DS
 Vessel Name (12-34)               :THIS IS A VERY LONG IMP
 Voyage Number (35-39)             :E343

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :_<MSG PLACEHOLDER>

-----------------INPP01-----------------

-----------------INPJ01-----------------

-----------------INPB01-----------------
 Manifest Quantity (21-30)               :0
 Weight (36-45)                          :0
 Bill Of Lading Status Indicator (48-48) :N

-----------------INPB02-----------------
 Second Notify Party1 (45-48) :SD43

-----------------APLZCR-----------------
 Number Of Transaction Detail Records (35-39) :0", sendingBill.MB_MessageContents.Trim());
		}

		public void TestDeleteMessageSendingObjectMembers()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;

			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "B0000000001";
			var moveDetail = bill.MovementDetail;

			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselNonContainer;
			header.BH_CarrierSCAC = "XXXX";
			header.BH_ImportConveyanceCountry = "NZ";
			header.BH_ImportConveyanceName = "TEST VESS 2";
			header.BH_VoyageNumber = "1111";
			header.BH_ETA = new ZDateTime(2015, 01, 01, 01, 01, 01);
			header.BH_FTZMove = ZBool.False;
			header.BH_PortUnladingDCode = "2222";
			header.BH_FIRMS = "FOD3";
			header.BH_LloydsNumber = "1111112";
			moveHeader.BM_ManifestSequenceNumber = "000002";
			Factory.Save();

			var sendingBill = new MessageSendingObject(moveDetail, ActionCode.Creating);
			var creatingMessage = new ACEAMSMessageBuilder(sendingBill, ActionCode.Creating).PopulateMessage();
			creatingMessage.EM_Status = "SNT";
			Factory.Save();

			var creatingResponseMessage1 = Factory.New<AMSEDIMessage>();
			creatingResponseMessage1.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			creatingResponseMessage1.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			creatingResponseMessage1.EM_MessageNum = creatingMessage.EM_MessageNum;
			creatingResponseMessage1.EM_SystemCreateTimeUtc = creatingMessage.EM_SystemCreateTimeUtc;
			creatingResponseMessage1.EM_ApplicationCode = creatingMessage.EM_ApplicationCode;
			creatingResponseMessage1.EM_MessageText =
"ACR8CWS      MR11050303221800085                                                " +
"M01OTT110AUAPL EMERALD            K34L 00001" + moveHeader.BM_ManifestSequenceNumber + " 7819369                      " +
"M02B0000000001_OTT14000086                                                      " +
"P01270405131100001    1518                                                      " +
"W02OTT11105030322150100100001000000000000000000000000100021                     " +
"ZCR8CWS      MI                   00004";
			moveHeader.Messages.Add(creatingResponseMessage1);

			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			header.BH_CarrierSCAC = "ABCD";
			header.BH_ImportConveyanceCountry = "AU";
			header.BH_ImportConveyanceName = "APL VESSEL";
			header.BH_VoyageNumber = "1234A";
			header.BH_ETA = new ZDateTime(2012, 10, 02, 18, 20, 10);
			header.BH_FTZMove = ZBool.False;
			header.BH_PortUnladingDCode = "5687";
			header.BH_FIRMS = "FOD3";
			header.BH_LloydsNumber = "1111111";
			moveHeader.BM_ManifestSequenceNumber = "000001";
			Factory.Save();
			sendingBill.UpdateAction(ActionCode.AmendingDelete);

			AssertContains(@"Carrier Code (4-7)                :XXXX", sendingBill.MB_MessageContents.Trim());
			AssertContains(@"Mode Of Transportation Code (8-9) :10", sendingBill.MB_MessageContents.Trim());
			AssertContains(@"Vessel Country Code (10-11)       :NZ", sendingBill.MB_MessageContents.Trim());
			AssertContains(@"Voyage Number (35-39)             :1111", sendingBill.MB_MessageContents.Trim());
			AssertContains(@"Vessel Code (52-58)               :1111112", sendingBill.MB_MessageContents.Trim());
		}

		public void TestDeleteMessageWithMultipleBillOfLanding()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;

			var moveHeader = header.MovementHeader;
			var bill1 = header.Bills.AddNew();
			bill1.B0_IssuerCode = "SHCR";
			bill1.B0_MasterBillNumber = "B0000000001";
			var moveDetail1 = bill1.MovementDetail;

			var bill2 = header.Bills.AddNew();
			bill2.B0_IssuerCode = "SHCR";
			bill2.B0_MasterBillNumber = "B0000000002";
			var moveDetail2 = bill2.MovementDetail;

			var bill3 = header.Bills.AddNew();
			bill3.B0_IssuerCode = "SHCR";
			bill3.B0_MasterBillNumber = "B0000000003";
			var moveDetail3 = bill3.MovementDetail;

			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselNonContainer;
			header.BH_CarrierSCAC = "XXXX";
			header.BH_ImportConveyanceCountry = "NZ";
			header.BH_ImportConveyanceName = "TEST VESS 2";
			header.BH_VoyageNumber = "1111";
			header.BH_ETA = new ZDateTime(2015, 01, 01, 01, 01, 01);
			header.BH_FTZMove = ZBool.False;
			header.BH_PortUnladingDCode = "2222";
			header.BH_FIRMS = "FOD3";
			header.BH_LloydsNumber = "1111112";
			moveHeader.BM_ManifestSequenceNumber = "000002";
			moveDetail1.
			Factory.Save();
			var sendingBill1 = new MessageSendingObject(moveDetail1, ActionCode.AmendingAdd);
			var message1 = new ACEAMSMessageBuilder(sendingBill1, ActionCode.AmendingAdd).PopulateMessage();
			message1.EM_Status = "SNT";
			message1.EM_SystemCreateTimeUtc = System.DateTime.Today.AddDays(-3);
			message1.EM_SystemLastEditTimeUtc = System.DateTime.Today.AddDays(-3);

			var sendingBill2 = new MessageSendingObject(moveDetail2, ActionCode.AmendingAdd);
			var message2 = new ACEAMSMessageBuilder(sendingBill2, ActionCode.AmendingAdd).PopulateMessage();
			message2.EM_Status = "SNT";
			message2.EM_SystemCreateTimeUtc = System.DateTime.Today.AddDays(-3);
			message2.EM_SystemLastEditTimeUtc = System.DateTime.Today.AddDays(-2);

			var sendingBill3 = new MessageSendingObject(moveDetail3, ActionCode.AmendingAdd);
			var message3 = new ACEAMSMessageBuilder(sendingBill3, ActionCode.AmendingAdd).PopulateMessage();
			message3.EM_Status = "SNT";
			message3.EM_SystemCreateTimeUtc = ZDateTime.Now;
			message3.EM_SystemLastEditTimeUtc = ZDateTime.Now;
			Factory.Save();

			var creatingResponseMessage1 = Factory.New<AMSEDIMessage>();
			creatingResponseMessage1.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			creatingResponseMessage1.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			creatingResponseMessage1.EM_MessageNum = message1.EM_MessageNum;
			creatingResponseMessage1.EM_SystemCreateTimeUtc = message1.EM_SystemCreateTimeUtc;
			creatingResponseMessage1.EM_ApplicationCode = message1.EM_ApplicationCode;
			creatingResponseMessage1.EM_MessageText =
"ACR8CWS      MR11050303221800085                                                " +
"M01OTT110AUAPL EMERALD            K34L 00001" + moveHeader.BM_ManifestSequenceNumber + " 7819369                      " +
"M02B0000000001_OTT14000086                                                      " +
"P01270405131100001    1518                                                      " +
"W02OTT11105030322150100100001000000000000000000000000100021                     " +
"ZCR8CWS      MI                   00004";
			moveHeader.Messages.Add(creatingResponseMessage1);

			var creatingResponseMessage2 = Factory.New<AMSEDIMessage>();
			creatingResponseMessage2.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			creatingResponseMessage2.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			creatingResponseMessage2.EM_MessageNum = message2.EM_MessageNum;
			creatingResponseMessage2.EM_SystemCreateTimeUtc = message2.EM_SystemCreateTimeUtc;
			creatingResponseMessage2.EM_ApplicationCode = message2.EM_ApplicationCode;
			creatingResponseMessage2.EM_MessageText =
"ACR8CWS      MR11050303221800085                                                " +
"M01OTT110AUAPL EMERALD            K34L 00001" + moveHeader.BM_ManifestSequenceNumber + " 7819369                      " +
"M02B0000000002_OTT14000086                                                      " +
"P01270405131100001    1518                                                      " +
"W02OTT11105030322150100100001000000000000000000000000100021                     " +
"ZCR8CWS      MI                   00004";
			moveHeader.Messages.Add(creatingResponseMessage2);

			var creatingResponseMessage3 = Factory.New<AMSEDIMessage>();
			creatingResponseMessage3.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			creatingResponseMessage3.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			creatingResponseMessage3.EM_MessageNum = message3.EM_MessageNum;
			creatingResponseMessage3.EM_SystemCreateTimeUtc = message3.EM_SystemCreateTimeUtc;
			creatingResponseMessage3.EM_ApplicationCode = message3.EM_ApplicationCode;
			creatingResponseMessage3.EM_MessageText =
"ACR8CWS      MR11050303221800085                                                " +
"M01OTT110AUAPL EMERALD            K34L 00001" + moveHeader.BM_ManifestSequenceNumber + " 7819369                      " +
"M02B0000000003_OTT14000086                                                      " +
"P01270405131100001    1518                                                      " +
"W02OTT11105030322150100100001000000000000000000000000100021                     " +
"ZCR8CWS      MI                   00004";
			moveHeader.Messages.Add(creatingResponseMessage3);
			Factory.Save();

			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			header.BH_CarrierSCAC = "ABCD";
			header.BH_ImportConveyanceCountry = "AU";
			header.BH_ImportConveyanceName = "APL VESSEL";
			header.BH_VoyageNumber = "1234A";
			header.BH_ETA = new ZDateTime(2012, 10, 02, 18, 20, 10);
			header.BH_FTZMove = ZBool.False;
			header.BH_PortUnladingDCode = "5687";
			header.BH_FIRMS = "FOD3";
			header.BH_LloydsNumber = "1111111";
			moveHeader.BM_ManifestSequenceNumber = "000001";
			Factory.Save();

			sendingBill1.UpdateAction(ActionCode.AmendingDelete);
			AssertContains(@"Carrier Code (4-7)                :XXXX", sendingBill1.MB_MessageContents.Trim());
			AssertContains(@"Mode Of Transportation Code (8-9) :10", sendingBill1.MB_MessageContents.Trim());
			AssertContains(@"Vessel Country Code (10-11)       :NZ", sendingBill1.MB_MessageContents.Trim());
			AssertContains(@"Voyage Number (35-39)             :1111", sendingBill1.MB_MessageContents.Trim());
			AssertContains(@"Vessel Code (52-58)               :1111112", sendingBill1.MB_MessageContents.Trim());
			AssertContains(@"Carrier Assigned Batch Number (4-33) :B0000000001", sendingBill1.MB_MessageContents.Trim());

			sendingBill2.UpdateAction(ActionCode.AmendingDelete);
			AssertContains(@"Carrier Code (4-7)                :XXXX", sendingBill2.MB_MessageContents.Trim());
			AssertContains(@"Mode Of Transportation Code (8-9) :10", sendingBill2.MB_MessageContents.Trim());
			AssertContains(@"Vessel Country Code (10-11)       :NZ", sendingBill2.MB_MessageContents.Trim());
			AssertContains(@"Voyage Number (35-39)             :1111", sendingBill2.MB_MessageContents.Trim());
			AssertContains(@"Vessel Code (52-58)               :1111112", sendingBill2.MB_MessageContents.Trim());
			AssertContains(@"Carrier Assigned Batch Number (4-33) :B0000000002", sendingBill2.MB_MessageContents.Trim());

			sendingBill3.UpdateAction(ActionCode.AmendingDelete);
			AssertContains(@"Carrier Code (4-7)                :XXXX", sendingBill3.MB_MessageContents.Trim());
			AssertContains(@"Mode Of Transportation Code (8-9) :10", sendingBill3.MB_MessageContents.Trim());
			AssertContains(@"Vessel Country Code (10-11)       :NZ", sendingBill3.MB_MessageContents.Trim());
			AssertContains(@"Voyage Number (35-39)             :1111", sendingBill3.MB_MessageContents.Trim());
			AssertContains(@"Vessel Code (52-58)               :1111112", sendingBill3.MB_MessageContents.Trim());
			AssertContains(@"Carrier Assigned Batch Number (4-33) :B0000000003", sendingBill3.MB_MessageContents.Trim());
		}

		[TestDate(2012, 04, 05)]
		public void TestMessageContentForVesselArrival()
		{
			var header = GetHeader();
			var moveHeader = header.MovementHeader;
			moveHeader.BM_CustomsStatus = "BDD";
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;

			var sendingBill = new MessageSendingObject(moveDetail, ActionCode.VesselArrival);
			AssertMultilineASCIIEquals("Content", @"-----------------APLACR-----------------

-----------------INPM01-----------------
 Carrier Code (4-7)                :SD43
 Mode Of Transportation Code (8-9) :30
 Vessel Country Code (10-11)       :DS
 Voyage Number (35-39)             :E343
 Vessel Code (52-58)               :9323423

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :_<MSG PLACEHOLDER>

-----------------INPP01-----------------

-----------------ICMH01-----------------
 Message Code (4-4) :4

-----------------APLZCR-----------------
 Number Of Transaction Detail Records (35-39) :0", sendingBill.MB_MessageContents.Trim());
		}

		[TestDate(2012, 04, 05)]
		public void TestMessageContentForVesselDeparture()
		{
			var header = GetHeader();
			header.BH_SailingDate = new ZDateTime(2015, 4, 8);
			var moveHeader = header.MovementHeader;
			moveHeader.BM_CustomsStatus = "BDD";
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;

			var sendingBill = new MessageSendingObject(moveDetail, ActionCode.VesselDeparture);
			AssertMultilineASCIIEquals("Content", @"-----------------APLACR-----------------

-----------------INPM01-----------------
 Carrier Code (4-7)                :SD43
 Mode Of Transportation Code (8-9) :30
 Vessel Country Code (10-11)       :DS
 Voyage Number (35-39)             :E343
 Vessel Code (52-58)               :9323423

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :_<MSG PLACEHOLDER>

-----------------INPP01-----------------

-----------------ICMH01-----------------
 Message Code (4-4) :9
 Date (19-24)       :08-Apr-15
 Time (33-38)       :0000

-----------------ICMH02-----------------

-----------------APLZCR-----------------
 Number Of Transaction Detail Records (35-39) :0", sendingBill.MB_MessageContents.Trim());
		}

		[TestDate(2012, 04, 05)]
		public void TestMessageContentForInBondArrival()
		{
			var header = GetHeader();
			var inbondmoveHeader = header.InBondMovementHeaders.AddNew();
			inbondmoveHeader.InBondNumber = "INB123231";
			inbondmoveHeader.BM_ArrivalDate = new ZDateTime(2012, 04, 03, 17, 34, 54);
			inbondmoveHeader.BM_DestinationPortCode = "2705";
			var bill = header.Bills.AddNew();
			var moveDetail = inbondmoveHeader.MovementDetails.AddNew(bill.PK);

			var sendingBill = new MessageSendingObject(moveDetail, ActionCode.InBondArrival);
			sendingBill.MB_Send = true;
			sendingBill.MB_BillActionCode = InBondAndVesselEventMessageCodeList.Codes.ArriveInBond;
			AssertMultilineASCIIEquals("Message Contents", @"-----------------APLACR-----------------

-----------------INPM01-----------------
 Carrier Code (4-7)                :SD43
 Mode Of Transportation Code (8-9) :30
 Vessel Country Code (10-11)       :DS
 Voyage Number (35-39)             :E343
 Vessel Code (52-58)               :9323423

-----------------INPM02-----------------
 Carrier Assigned Batch Number (4-33) :_<MSG PLACEHOLDER>

-----------------INPP01-----------------

-----------------ICMH01-----------------
 Message Code (4-4)                            :1
 Inbond Entity (5-18)                          :INB123231
 Date (19-24)                                  :03-Apr-12
 C B P Port (25-28)                            :2705
 Time (33-38)                                  :1734
 F I R M S Location On In Bond Arrival (76-79) :IV34

-----------------APLZCR-----------------
 Number Of Transaction Detail Records (35-39) :0", sendingBill.MB_MessageContents.Trim());
		}

		public void TestMB_BillActionCodeReadOnly()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var sendingObject = new MessageSendingObject(moveDetail, ActionCode.Creating);
			AssertEquals(true, sendingObject.MB_BillActionCodeInfo.ReadOnly);
			sendingObject = new MessageSendingObject(moveDetail, ActionCode.AmendingAdd);
			AssertEquals(false, sendingObject.MB_BillActionCodeInfo.ReadOnly);
			sendingObject = new MessageSendingObject(moveDetail, ActionCode.InBondArrival);
			AssertEquals(false, sendingObject.MB_BillActionCodeInfo.ReadOnly);
		}

		public void TestBillActionCodeList()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var sendingObject = new MessageSendingObject(moveDetail, ActionCode.Creating);
			AssertEquals(AMSBillSendingActionCodeList.GetBillActionCodeList(Factory, ActionCode.Creating), sendingObject.BillActionCodeList);

			foreach (var actionCode in new[] { ActionCode.InBondArrival,
				ActionCode.InBondExportation,
				ActionCode.InBondTransferOfLiability })
			{
				sendingObject = new MessageSendingObject(moveDetail, actionCode);
				AssertEquals(InBondAndVesselEventMessageCodeList.GetBillActionCodeList(Factory, actionCode), sendingObject.BillActionCodeList);
			}
		}

		public void TestBOLGreaterThan12Characters()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "MB1023- 2398";
			var moveDetail = bill.MovementDetail;
			var sendingObject = new MessageSendingObject(moveDetail, ActionCode.Creating);
			ICommonBillOfLading commonBillOfLading = sendingObject;
			AssertEquals("MB10232398", sendingObject.MB_BillOfLadingSequenceNumber);
			AssertEquals("MB10232398", commonBillOfLading.BillOfLadingSequenceNumber);

			bill.B0_MasterBillNumber = "MB1023- 2398123";
			sendingObject = new MessageSendingObject(moveDetail, ActionCode.Creating);
			commonBillOfLading = sendingObject;
			AssertEquals("B10232398123", sendingObject.MB_BillOfLadingSequenceNumber);
			AssertEquals("B10232398123", commonBillOfLading.BillOfLadingSequenceNumber);

			bill.B0_MasterBillNumber = "MB1023- 239812";
			sendingObject = new MessageSendingObject(moveDetail, ActionCode.Creating);
			commonBillOfLading = sendingObject;
			AssertEquals("MB1023239812", sendingObject.MB_BillOfLadingSequenceNumber);
			AssertEquals("MB1023239812", commonBillOfLading.BillOfLadingSequenceNumber);
		}

		public void TestMB_PortOfUnlading()
		{
			var header = GetHeader();
			header.BH_TransitDirection = DirectionTypeList.Codes.MVOCC;
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			bill.B0_InBondPortOfDestDCode = "1101";
			var moveDetail = bill.MovementDetail;

			var sendingBill = new MessageSendingObject(moveDetail, ActionCode.VesselArrival);
			AssertEquals("1101", sendingBill.MB_PortOfUnlading);

			bill.B0_InBondPortOfDestDCode = "1001";
			sendingBill = new MessageSendingObject(moveDetail, ActionCode.ChangeEstDateOfArrival);
			AssertEquals("1001", sendingBill.MB_PortOfUnlading);

			sendingBill = new MessageSendingObject(moveDetail, ActionCode.VesselDeparture);
			AssertEquals(ZString.Empty, sendingBill.MB_PortOfUnlading);
		}

		public void TestMB_DateForChangeOfEstimate()
		{
			var header = GetHeader();
			header.BH_TransitDirection = DirectionTypeList.Codes.MVOCC;
			var moveHeader = header.MovementHeader;
			var bill0 = header.Bills.AddNew();
			bill0.B0_InBondPortOfDestDCode = "1101";
			bill0.B0_DateOfDischarge = ZDate.Today.AddDays(-1);
			var moveDetail0 = bill0.MovementDetail;
			var bill1 = header.Bills.AddNew();
			bill1.B0_InBondPortOfDestDCode = "1102";
			bill1.B0_DateOfDischarge = ZDate.Today.AddDays(1);
			var moveDetail1 = bill1.MovementDetail;

			var sendingBill0 = new MessageSendingObject(moveDetail0, ActionCode.ChangeEstDateOfArrival);
			AssertEquals("1101", sendingBill0.MB_PortOfUnlading);
			AssertEquals(ZDate.Today.AddDays(-1), sendingBill0.MB_Date);
			var sendingBill1 = new MessageSendingObject(moveDetail1, ActionCode.ChangeEstDateOfArrival);
			AssertEquals("1102", sendingBill1.MB_PortOfUnlading);
			AssertEquals(ZDate.Today.AddDays(1), sendingBill1.MB_Date);
		}

		#region Implementation

		CusInBondHeader GetHeader()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.BH_CarrierSCAC = "SD43";
			header.BH_ImportTransportMode = TransportTypeList.Codes.Truck;
			header.BH_ImportConveyanceCountry = "DS";
			header.BH_ImportConveyanceName = "IMP DUMMY VESSEL";
			header.BH_VoyageNumber = "E343";
			header.BH_LloydsNumber = "9323423";
			header.BH_ExportFlag = ExportTypeList.Codes.OutboundCargo;
			header.BH_FIRMS = "IV34";
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			return header;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_B0 = bill.PK;
			return new MessageSendingObject(moveDetail, ActionCode.Creating);
		}
		#endregion
	}
}
