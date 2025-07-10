using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	sealed class DeleteMessageSendingObjectTest : TestCaseWithFactory
	{
		public void TestDeleteMessageSendingObjectMembers()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;

			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "HB1105031520";
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
			var attachee = (IACEBillManifestMessageAttachee)sendingBill;
			var creatingMessage = new ACEAMSMessageBuilder(sendingBill, ActionCode.Creating).PopulateMessage();
			creatingMessage.EM_Status = EDIMessage.Status.Sent;
			Factory.Save();

			var creatingResponseMessage = Factory.New<AMSEDIMessage>();
			creatingResponseMessage.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			creatingResponseMessage.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			creatingResponseMessage.EM_MessageNum = creatingMessage.EM_MessageNum;
			creatingResponseMessage.EM_SystemCreateTimeUtc = creatingMessage.EM_SystemCreateTimeUtc;
			creatingResponseMessage.EM_ApplicationCode = creatingMessage.EM_ApplicationCode;
			creatingResponseMessage.EM_MessageText =
"ACR8CWS      MR11050303221800085                                                " +
"M01OTT110AUAPL EMERALD            K34L 00001" + moveHeader.BM_ManifestSequenceNumber + " 7819369                      " +
"M02HB1105031520_EDIEDIDAT4000086                                                " +
"P01270405131100001    1518                                                      " +
"W02OTT11105030322150100100001000000000000000000000000100021                     " +
"ZCR8CWS      MI                   00004";
			moveHeader.Messages.Add(creatingResponseMessage);
			Factory.Save();

			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			header.BH_CarrierSCAC = "BBB";
			header.BH_ImportConveyanceCountry = "US";
			header.BH_ImportConveyanceName = "AAAAAA";
			header.BH_VoyageNumber = "11111";
			header.BH_ETA = new ZDateTime(2012, 02, 07, 18, 20, 10);
			header.BH_FTZMove = ZBool.False;
			header.BH_PortUnladingDCode = "5555";
			header.BH_FIRMS = "FOD5";
			header.BH_LloydsNumber = "1111113";
			moveHeader.BM_ManifestSequenceNumber = "000003";
			Factory.Save();

			sendingBill = new MessageSendingObject(moveDetail, ActionCode.Creating);
			attachee = sendingBill;
			creatingMessage = new ACEAMSMessageBuilder(sendingBill, ActionCode.Creating).PopulateMessage();
			creatingMessage.EM_Status = EDIMessage.Status.Sent;
			Factory.Save();

			creatingResponseMessage = Factory.New<AMSEDIMessage>();
			creatingResponseMessage.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			creatingResponseMessage.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			creatingResponseMessage.EM_MessageNum = creatingMessage.EM_MessageNum;
			creatingResponseMessage.EM_SystemCreateTimeUtc = creatingMessage.EM_SystemCreateTimeUtc;
			creatingResponseMessage.EM_ApplicationCode = creatingMessage.EM_ApplicationCode;
			creatingResponseMessage.EM_MessageText =
"ACR8CWS      MR11050303221800085                                                " +
"W01                              000001148 INCORRECT CONV CODE                  " +
"M01OTT110AUAPL EMERALD            K34L 00001" + moveHeader.BM_ManifestSequenceNumber + " 7819369                      " +
"M02HB1105031520_EDIEDIDAT4000086                                                " +
"P01270405131100001    1518                                                      " +
"W02OTT11105030322150100100001000000000000000000000000100021                     " +
"ZCR8CWS      MI                   00004";
			moveHeader.Messages.Add(creatingResponseMessage);
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

			sendingBill.UpdateAction(ActionCode.AmendingDelete);
			var deleteMessageSendingObject = new DeleteMessageSendingObject(sendingBill);
			var deleteMessageAttachee = (IACEBillManifestMessageAttachee)deleteMessageSendingObject;
			AssertNotNull(deleteMessageAttachee);

			var carrierCode = deleteMessageAttachee.CarrierCode;
			var modeOfTransportationCode = deleteMessageAttachee.ModeOfTransportationCode;
			var conveyanceCountryCode = deleteMessageAttachee.ConveyanceCountryCode;
			var conveyanceName = deleteMessageAttachee.ConveyanceName;
			var voyageNumber = deleteMessageAttachee.VoyageNumber;
			var manifestSequenceNumber = deleteMessageAttachee.ManifestSequenceNumber;
			var conveyanceCode = deleteMessageAttachee.ConveyanceCode;
			var portOfUnladingCode = deleteMessageAttachee.PortDetails.DistrictPortOfUnladingCode;
			var originalEstimatedDate = deleteMessageAttachee.PortDetails.OriginalEstimatedDate;

			AssertEquals("Not equal 'ABCD', it is header's CarrierCode", "XXXX", carrierCode);
			AssertEquals("Not equal '11', it is header's ModeOfTransportationCode", "10", modeOfTransportationCode);
			AssertEquals("Not equal 'AU', it is header's ConveyanceCountryCode", "NZ", conveyanceCountryCode);
			AssertEquals("Not equal '1234A', it is header's VoyageNumber", "1111", voyageNumber);
			AssertEquals("Not equal '000001', it is header's ManifestSequenceNumber", "000002", manifestSequenceNumber);
			AssertEquals("Not equal '1111111', it is header's ConveyanceCode", "1111112", conveyanceCode);
			AssertEquals("Not equal '5687', it is header's PortOfUnladingCode", "2222", portOfUnladingCode);
			AssertEquals("Not equal '02-Oct-12', it is header's OriginalEstimatedDate", "01-Jan-15", originalEstimatedDate.ToShortDateString());
			AssertEquals("ConveyanceName should be empty when ConveyanceCode is reported", ZString.Empty, conveyanceName);

			sendingBill.MB_VesselOverride = true;
			AssertEquals("It should return header's ConveyanceName because we set MB_VesselOverride as true.", "APL VESSEL", deleteMessageAttachee.ConveyanceName);
		}

		public void TestDeleteMessageSendingObjectMembers2()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;

			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "HB1105031520";
			var moveDetail = bill.MovementDetail;

			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselNonContainer;
			header.BH_CarrierSCAC = "XXXX";
			header.BH_ImportConveyanceCountry = "NZ";
			header.BH_ImportConveyanceName = " TEST VESS 2";
			header.BH_VoyageNumber = " 111";
			header.BH_ETA = new ZDateTime(2015, 01, 01, 01, 01, 01);
			header.BH_FTZMove = ZBool.False;
			header.BH_PortUnladingDCode = "2222";
			header.BH_FIRMS = "FOD3";
			header.BH_LloydsNumber = "1111112";
			moveHeader.BM_ManifestSequenceNumber = "000002";
			Factory.Save();

			var sendingBill = new MessageSendingObject(moveDetail, ActionCode.Creating);
			var attachee = (IACEBillManifestMessageAttachee)sendingBill;
			var creatingMessage = new ACEAMSMessageBuilder(sendingBill, ActionCode.Creating).PopulateMessage();
			creatingMessage.EM_Status = EDIMessage.Status.Sent;
			Factory.Save();

			var creatingResponseMessage = Factory.New<AMSEDIMessage>();
			creatingResponseMessage.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			creatingResponseMessage.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			creatingResponseMessage.EM_MessageNum = creatingMessage.EM_MessageNum;
			creatingResponseMessage.EM_SystemCreateTimeUtc = creatingMessage.EM_SystemCreateTimeUtc;
			creatingResponseMessage.EM_ApplicationCode = creatingMessage.EM_ApplicationCode;
			creatingResponseMessage.EM_MessageText =
"ACR8CWS      MR11050303221800085                                                " +
"M01OTT110AUAPL EMERALD            K34L 00001" + moveHeader.BM_ManifestSequenceNumber + " 7819369                      " +
"M02HB1105031520_EDIEDIDAT4000086                                                " +
"P01270405131100001    1518                                                      " +
"W02OTT11105030322150100100001000000000000000000000000100021                     " +
"ZCR8CWS      MI                   00004";
			moveHeader.Messages.Add(creatingResponseMessage);
			Factory.Save();

			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			header.BH_CarrierSCAC = "BBB";
			header.BH_ImportConveyanceCountry = "US";
			header.BH_ImportConveyanceName = " AAAAA";
			header.BH_VoyageNumber = " 1111";
			header.BH_ETA = new ZDateTime(2012, 02, 07, 18, 20, 10);
			header.BH_FTZMove = ZBool.False;
			header.BH_PortUnladingDCode = "5555";
			header.BH_FIRMS = "FOD5";
			header.BH_LloydsNumber = "1111113";
			moveHeader.BM_ManifestSequenceNumber = "000003";
			Factory.Save();

			sendingBill = new MessageSendingObject(moveDetail, ActionCode.Creating);
			attachee = sendingBill;
			creatingMessage = new ACEAMSMessageBuilder(sendingBill, ActionCode.Creating).PopulateMessage();
			creatingMessage.EM_Status = EDIMessage.Status.Sent;
			Factory.Save();

			creatingResponseMessage = Factory.New<AMSEDIMessage>();
			creatingResponseMessage.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			creatingResponseMessage.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			creatingResponseMessage.EM_MessageNum = creatingMessage.EM_MessageNum;
			creatingResponseMessage.EM_SystemCreateTimeUtc = creatingMessage.EM_SystemCreateTimeUtc;
			creatingResponseMessage.EM_ApplicationCode = creatingMessage.EM_ApplicationCode;
			creatingResponseMessage.EM_MessageText =
"ACR8CWS      MR11050303221800085                                                " +
"W01                              000001148 INCORRECT CONV CODE                  " +
"M01OTT110AUAPL EMERALD            K34L 00001" + moveHeader.BM_ManifestSequenceNumber + " 7819369                      " +
"M02HB1105031520_EDIEDIDAT4000086                                                " +
"P01270405131100001    1518                                                      " +
"W02OTT11105030322150100100001000000000000000000000000100021                     " +
"ZCR8CWS      MI                   00004";
			moveHeader.Messages.Add(creatingResponseMessage);
			Factory.Save();

			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			header.BH_CarrierSCAC = "ABCD";
			header.BH_ImportConveyanceCountry = "AU";
			header.BH_ImportConveyanceName = " APL VESSEL";
			header.BH_VoyageNumber = " 234A";
			header.BH_ETA = new ZDateTime(2012, 10, 02, 18, 20, 10);
			header.BH_FTZMove = ZBool.False;
			header.BH_PortUnladingDCode = "5687";
			header.BH_FIRMS = "FOD3";
			header.BH_LloydsNumber = "1111111";
			moveHeader.BM_ManifestSequenceNumber = "000001";

			Factory.Save();

			sendingBill.UpdateAction(ActionCode.AmendingAdd);
			sendingBill.MB_BillActionCode = AMSBillSendingActionCodeList.Codes.DeleteBill;
			sendingBill.MB_PortOfUnladingOverride = "3333";
			var deleteMessageSendingObject = new DeleteMessageSendingObject(sendingBill);
			var deleteMessageAttachee = (IACEBillManifestMessageAttachee)deleteMessageSendingObject;
			AssertNotNull(deleteMessageAttachee);

			var carrierCode = deleteMessageAttachee.CarrierCode;
			var modeOfTransportationCode = deleteMessageAttachee.ModeOfTransportationCode;
			var conveyanceCountryCode = deleteMessageAttachee.ConveyanceCountryCode;
			var conveyanceName = deleteMessageAttachee.ConveyanceName;
			var voyageNumber = deleteMessageAttachee.VoyageNumber;
			var manifestSequenceNumber = deleteMessageAttachee.ManifestSequenceNumber;
			var conveyanceCode = deleteMessageAttachee.ConveyanceCode;
			var portOfUnladingCode = deleteMessageAttachee.PortDetails.DistrictPortOfUnladingCode;
			var originalEstimatedDate = deleteMessageAttachee.PortDetails.OriginalEstimatedDate;

			AssertEquals("Not equal 'ABCD', it is header's CarrierCode", "XXXX", carrierCode);
			AssertEquals("Not equal '11', it is header's ModeOfTransportationCode", "10", modeOfTransportationCode);
			AssertEquals("Not equal 'AU', it is header's ConveyanceCountryCode", "NZ", conveyanceCountryCode);
			AssertEquals("Not equal '1234A', it is header's VoyageNumber", " 111", voyageNumber);
			AssertEquals("Not equal '000001', it is header's ManifestSequenceNumber", "000002", manifestSequenceNumber);
			AssertEquals("Not equal '1111111', it is header's ConveyanceCode", "1111112", conveyanceCode);
			AssertEquals("Not equal '5687', it is header's PortOfUnladingCode", "3333", portOfUnladingCode);
			AssertEquals("Not equal '02-Oct-12', it is header's OriginalEstimatedDate", "01-Jan-15", originalEstimatedDate.ToShortDateString());
			AssertEquals("ConveyanceName should be empty when ConveyanceCode is reported", ZString.Empty, conveyanceName);
		}

		public void TestIACEBillManifestMessageAttacheeMembers()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;

			var sendingObject = new MessageSendingObject(moveDetail, ActionCode.Creating);
			sendingObject.MB_Date = new ZDateTime(2020, 02, 02);
			sendingObject.MB_ForeignDeparturePort = "Test";
			var deleteMessageSendingObject = new DeleteMessageSendingObject(sendingObject);
			IACEBillManifestMessageAttachee attachee = deleteMessageSendingObject;
			AssertEquals(deleteMessageSendingObject, attachee.BillOfLadingDetails);
			AssertEquals(bill.PK.ToString(), attachee.BillPKAsString);
			AssertEquals("Test", attachee.ForeignDeparturePort);
			AssertEquals(new ZDateTime(2020, 02, 02), attachee.EventDateTime);
			AssertEquals(bill.Messages, attachee.BillMessages);
		}
	}
}
