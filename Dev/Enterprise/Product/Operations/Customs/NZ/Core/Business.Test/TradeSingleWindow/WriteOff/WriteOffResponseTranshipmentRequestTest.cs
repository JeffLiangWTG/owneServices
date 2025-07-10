using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.Business.MessageProcessors;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	sealed class WriteOffResponseTranshipmentRequestTest : TestCaseWithFactory
	{
		// Adjust output &/or email details if required for all test cases in this class that include the Movement status in the response.
		public void TestMasterDTRErrorResponseLinksToCorrectManifestUnderbond()
		{
			var transitDestOrg = Factory.New<OrgHeader>();
			transitDestOrg.OH_Code = "TDCODE";

			var transitDestOrgAddr = transitDestOrg.Addresses.AddNew();
			transitDestOrgAddr.OA_Address1 = "TD WareHouse 1 Address";
			transitDestOrgAddr.OA_Code = "TD1";
			transitDestOrg.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "52638P");
			Factory.Save();

			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			mawb.CM_MessageReference = "X00002256";
			mawb.CM_MAWB = "08600024861";
			mawb.CM_RL_NKLoadPort = "AUSYD";
			mawb.CM_RL_NKDischargePort = "NZAKL";
			mawb.CM_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;
			mawb.CM_CustomsStatus = LowValueConsignmentStatusList.Codes.Cleared;

			var hawb1 = mawb.ChildBills.AddNew();
			hawb1.CS_HAWB = "SYD1145";
			hawb1.CS_ConsignmentNum = 1;

			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_HAWB = "GAZ0592882";
			hawb2.CS_ConsignmentNum = 2;

			var mawbUnderbondDTR = TranshipmentRequest.Create(mawb);
			mawbUnderbondDTR.C4_MovementReason = MovementReason.Codes.DomesticTranshipmentRequest;
			mawbUnderbondDTR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			mawbUnderbondDTR.C4_OA_DestinationAddress = transitDestOrgAddr.PK;
			Assert("Pre-condition: CusEntryNum record does not exist for Underbond", mawbUnderbondDTR.CusEntryNumbers.Length == 0);

			var outgoingUnderbondMessage = Factory.New<TSWMessage>();
			outgoingUnderbondMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingUnderbondMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingUnderbondMessage.EM_ApplicationReference = "U00000344";
			outgoingUnderbondMessage.EM_LinkedObject = mawbUnderbondDTR;

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = "RES";
			nzcsMessage.EM_MessageText = U00000344NZCSResponse;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsMessage);
			Factory.Save();

			#region expectedOutput

			ZString expectedOutput = @"[ICR/CRE Rejected] Response for AirCargo ICR/CRE: X00002256

Error report
---------------------------------------------------------------------
Master DTR     : U00000344
Entry Number   : 
Master Bill    : 086-00024861
Message No     : 10778

Message Status : (841) ICR / CRE / OCR / ANA / AND rejected
               : error report attached.

Message Errors
---------------------------------------------------------------------
**Error** in {Declaration/Consignment[1]/TransitDestination}:-
  Transit Destination for Sea Transport must be either a New Zealand Port or a Premises having CCA and TF/CF client types
**Error** in {Declaration/Consignment[1]/Consignor}:-
  There must be exactly one Consignor for a Consignment
**Error** in {Declaration/Consignment[1]/Consignee/Unknown}:-
  Either the Consignee Name or Code must be specified
**Error** in {Declaration/Consignment[1]/Consignee}:-
  Either the Consignee Name or Code must be specified
";

			#endregion

			AssertEquals("EM_MessageSubType - Error response message comes from TSW Front end", TSWMessage.ResponseMessage.MessageSubTypes.TradeSingleWindow, nzcsMessage.EM_MessageSubType);
			AssertEquals("Error response '0000000' should display blank", "", mawbUnderbondDTR.TSWNumber);
			Assert("CusEntryNum record created for Underbond", mawbUnderbondDTR.CusEntryNumbers.Length > 0);

			var underbondEntryNum = mawbUnderbondDTR.CusEntryNumbers[0];
			AssertEquals("CE_Category", "CUS", underbondEntryNum.CE_Category);
			AssertEquals("CE_EntryType", "DTR", underbondEntryNum.CE_EntryType);
			AssertEquals("CE_RN_NKCountryCode", "NZ", underbondEntryNum.CE_RN_NKCountryCode);
			AssertEquals("CE_ParentTable", "CusUnderbond", underbondEntryNum.CE_ParentTable);
			AssertEquals("CE_ParentID", mawbUnderbondDTR.PK, underbondEntryNum.CE_ParentID);

			AssertEquals("Transhipment/Underbond status", "REJ", mawbUnderbondDTR.C4_Status);

			AssertSame(nzcsMessage.EM_LinkedObject, mawbUnderbondDTR);
			AssertEquals("Message rejection errors", expectedOutput, nzcsMessage.EM_MessageInterpretation);
		}

		public void TestAcceptedMasterDTRResponse()
		{
			var transitDestOrg = Factory.New<OrgHeader>();
			transitDestOrg.OH_Code = "TDCODE";

			var transitDestOrgAddr = transitDestOrg.Addresses.AddNew();
			transitDestOrgAddr.OA_Address1 = "TD WareHouse 1 Address";
			transitDestOrgAddr.OA_Code = "TD1";
			transitDestOrg.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "52638P");
			Factory.Save();

			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			mawb.CM_MessageReference = "X00002257";
			mawb.CM_MAWB = "08163635246";
			mawb.CM_RL_NKLoadPort = "AUSYD";
			mawb.CM_RL_NKDischargePort = "NZAKL";
			mawb.CM_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;
			mawb.CM_CustomsStatus = LowValueConsignmentStatusList.Codes.Cleared;

			var hawb1 = mawb.ChildBills.AddNew();
			hawb1.CS_HAWB = "BKG230718HB1";
			hawb1.CS_ConsignmentNum = 1;

			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_HAWB = "BKG230718HB2";
			hawb2.CS_ConsignmentNum = 2;

			var mawbUnderbondDTR = TranshipmentRequest.Create(mawb);
			mawbUnderbondDTR.C4_MovementReason = MovementReason.Codes.DomesticTranshipmentRequest;
			mawbUnderbondDTR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			mawbUnderbondDTR.C4_OA_DestinationAddress = transitDestOrgAddr.PK;
			Assert("Pre-condition: CusEntryNum record does not exist for Underbond", mawbUnderbondDTR.CusEntryNumbers.Length == 0);

			var outgoingUnderbondMessage = Factory.New<TSWMessage>();
			outgoingUnderbondMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingUnderbondMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingUnderbondMessage.EM_ApplicationReference = "U00000347";
			outgoingUnderbondMessage.EM_LinkedObject = mawbUnderbondDTR;

			#region Bio Response

			var icrBioResponse = Factory.New<TSWMessage>();
			icrBioResponse.EM_MessageType = "RES";
			icrBioResponse.EM_MessageText = U00000347BioResponse;
			icrBioResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrBioResponse);
			Factory.Save();

			AssertEquals("Transhipment/Underbond status after Bio message processed", "93", mawbUnderbondDTR.C4_Status);

			#endregion

			#region Customs Response

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = "RES";
			nzcsMessage.EM_MessageText = U00000347NZCSResponse;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			#endregion

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsMessage);
			Factory.Save();

			#region expectedOutput

			ZString expectedOutput = @"[ICR/CRE in Error, Check Consignments for Status] Response for AirCargo ICR/CRE: X00002257

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Master DTR     : U00000347
Entry Number   : 6965837
Master Bill    : 081-63635246
Message No     : 10788

Message Status : (C06) Customs Cargo Report Notification - consignment status as specified

Message Errors
---------------------------------------------------------------------
**Error** in {Declaration/Consignment[1]/Consignee/Unknown}:-
  Either the Consignee Name or Code must be specified
**Error** in {Declaration/Consignment[1]/Consignor/Unknown}:-
  Either the Consignor Name or Code must be specified

Job Responses
---------------------------------------------------------------------
--- Movement Status: DTA-Domestic Transhipment Approved ---
";

			#endregion

			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, nzcsMessage.EM_MessageSubType);
			AssertEquals("Underbond TSWNumber is updated", "6965837", mawbUnderbondDTR.TSWNumber);
			Assert("CusEntryNum record created for Underbond", mawbUnderbondDTR.CusEntryNumbers.Length > 0);

			var underbondEntryNum = mawbUnderbondDTR.CusEntryNumbers[0];
			AssertEquals("CE_Category", "CUS", underbondEntryNum.CE_Category);
			AssertEquals("CE_EntryType", "DTR", underbondEntryNum.CE_EntryType);
			AssertEquals("CE_RN_NKCountryCode", "NZ", underbondEntryNum.CE_RN_NKCountryCode);
			AssertEquals("CE_ParentTable", "CusUnderbond", underbondEntryNum.CE_ParentTable);
			AssertEquals("CE_ParentID", mawbUnderbondDTR.PK, underbondEntryNum.CE_ParentID);

			AssertEquals("Transhipment/Underbond status", "33", mawbUnderbondDTR.C4_Status);

			AssertSame(nzcsMessage.EM_LinkedObject, mawbUnderbondDTR);
			AssertContains("Message rejection errors", expectedOutput, nzcsMessage.EM_MessageInterpretation);
		}

		public void TestClearedMasterDTRResponse()
		{
			var transitDestOrg = Factory.New<OrgHeader>();
			transitDestOrg.OH_Code = "TDCODE";

			var transitDestOrgAddr = transitDestOrg.Addresses.AddNew();
			transitDestOrgAddr.OA_Address1 = "TD WareHouse 1 Address";
			transitDestOrgAddr.OA_Code = "TD1";
			transitDestOrg.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "52638P");
			Factory.Save();

			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			mawb.CM_MessageReference = "X00002259";
			mawb.CM_MAWB = "08163635261";
			mawb.CM_RL_NKLoadPort = "AUSYD";
			mawb.CM_RL_NKDischargePort = "NZAKL";
			mawb.CM_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;
			mawb.CM_CustomsStatus = LowValueConsignmentStatusList.Codes.Cleared;

			var hawb1 = mawb.ChildBills.AddNew();
			hawb1.CS_HAWB = "GAZ442824";
			hawb1.CS_ConsignmentNum = 1;

			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_HAWB = "GAZ905238";
			hawb2.CS_ConsignmentNum = 2;

			var mawbUnderbondDTR = TranshipmentRequest.Create(mawb);
			mawbUnderbondDTR.C4_MovementReason = MovementReason.Codes.DomesticTranshipmentRequest;
			mawbUnderbondDTR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			mawbUnderbondDTR.C4_OA_DestinationAddress = transitDestOrgAddr.PK;
			Assert("Pre-condition: CusEntryNum record does not exist for Underbond", mawbUnderbondDTR.CusEntryNumbers.Length == 0);

			var outgoingUnderbondMessage = Factory.New<TSWMessage>();
			outgoingUnderbondMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingUnderbondMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingUnderbondMessage.EM_ApplicationReference = "U00000348";
			outgoingUnderbondMessage.EM_LinkedObject = mawbUnderbondDTR;

			#region Acknowledgement Response

			var icrAckResponse = Factory.New<TSWMessage>();
			icrAckResponse.EM_ApplicationCode = "NZC";
			icrAckResponse.EM_MessageType = "TWR";
			icrAckResponse.EM_MessageText = U00000348_Acknowledgment;
			icrAckResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrAckResponse);
			Factory.Save();

			AssertEquals("Transhipment/Underbond status after ACK message processed", "ACK", mawbUnderbondDTR.C4_Status);
			AssertEquals("Transhipment/Underbond status description", "Transhipment Request Acknowledged", mawbUnderbondDTR.MovementStatusDesc);

			#endregion

			#region Bio Response

			var icrBioResponse = Factory.New<TSWMessage>();
			icrBioResponse.EM_ApplicationCode = "NZC";
			icrBioResponse.EM_MessageType = "TWR";
			icrBioResponse.EM_MessageText = U00000348_Bio;
			icrBioResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrBioResponse);
			Factory.Save();

			AssertEquals("Transhipment/Underbond status after Bio message processed", "93", mawbUnderbondDTR.C4_Status);

			#endregion

			#region Customs Response

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_ApplicationCode = "NZC";
			nzcsMessage.EM_MessageType = "TWR";
			nzcsMessage.EM_MessageText = U00000348_Customs;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			#endregion

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsMessage);
			Factory.Save();

			#region expectedOutput

			ZString expectedOutput = @"[ICR/CRE Accepted, Check Consignments for Status] Response for AirCargo ICR/CRE: X00002259

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Master DTR     : U00000348
Entry Number   : 41745165
Master Bill    : 081-63635261
Message No     : 10801

Message Status : (C06) Customs Cargo Report Notification - consignment status as specified

Job Responses
---------------------------------------------------------------------
--- Movement Status: DTA-Domestic Transhipment Approved ---
";

			#endregion

			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, nzcsMessage.EM_MessageSubType);
			AssertEquals("Underbond TSWNumber is updated", "41745165", mawbUnderbondDTR.TSWNumber);
			Assert("CusEntryNum record created for Underbond", mawbUnderbondDTR.CusEntryNumbers.Length > 0);

			var underbondEntryNum = mawbUnderbondDTR.CusEntryNumbers[0];
			AssertEquals("CE_Category", "CUS", underbondEntryNum.CE_Category);
			AssertEquals("CE_EntryType", "DTR", underbondEntryNum.CE_EntryType);
			AssertEquals("CE_RN_NKCountryCode", "NZ", underbondEntryNum.CE_RN_NKCountryCode);
			AssertEquals("CE_ParentTable", "CusUnderbond", underbondEntryNum.CE_ParentTable);
			AssertEquals("CE_ParentID", mawbUnderbondDTR.PK, underbondEntryNum.CE_ParentID);

			AssertEquals("Transhipment/Underbond status", "33", mawbUnderbondDTR.C4_Status);
			AssertEquals("Transhipment/Underbond status description", "DTR Approved", mawbUnderbondDTR.MovementStatusDesc);

			AssertSame(nzcsMessage.EM_LinkedObject, mawbUnderbondDTR);
			AssertContains("Message rejection errors", expectedOutput, nzcsMessage.EM_MessageInterpretation);
		}

		public void TestCancelledMasterDTRResponse()
		{
			var transitDestOrg = Factory.New<OrgHeader>();
			transitDestOrg.OH_Code = "TDCODE";

			var transitDestOrgAddr = transitDestOrg.Addresses.AddNew();
			transitDestOrgAddr.OA_Address1 = "TD WareHouse 1 Address";
			transitDestOrgAddr.OA_Code = "TD1";
			transitDestOrg.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "52638P");
			Factory.Save();

			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			mawb.CM_MessageReference = "X00002265";
			mawb.CM_MAWB = "08145454441";
			mawb.CM_RL_NKLoadPort = "AUSYD";
			mawb.CM_RL_NKDischargePort = "NZAKL";
			mawb.CM_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;
			mawb.CM_CustomsStatus = LowValueConsignmentStatusList.Codes.Cleared;

			var hawb1 = mawb.ChildBills.AddNew();
			hawb1.CS_HAWB = "BKG230727HBL4";
			hawb1.CS_ConsignmentNum = 1;

			var mawbUnderbondDTR = TranshipmentRequest.Create(mawb);
			mawbUnderbondDTR.C4_MovementReason = MovementReason.Codes.DomesticTranshipmentRequest;
			mawbUnderbondDTR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			mawbUnderbondDTR.C4_OA_DestinationAddress = transitDestOrgAddr.PK;
			Assert("Pre-condition: CusEntryNum record does not exist for Underbond", mawbUnderbondDTR.CusEntryNumbers.Length == 0);

			var outgoingUnderbondMessage = Factory.New<TSWMessage>();
			outgoingUnderbondMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingUnderbondMessage.EM_MessageSubType = MessageSubTypeList.Codes.Cancellation;
			outgoingUnderbondMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingUnderbondMessage.EM_ApplicationReference = "U00000358";
			outgoingUnderbondMessage.EM_LinkedObject = mawbUnderbondDTR;

			#region Cancel DTR Processing

			#region Acknowledgement Response

			var icrAckResponse = Factory.New<TSWMessage>();
			icrAckResponse.EM_ApplicationCode = "NZC";
			icrAckResponse.EM_MessageType = "TWR";
			icrAckResponse.EM_MessageText = U00000358_AcknowledgmentCancelResponse;
			icrAckResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrAckResponse);
			Factory.Save();

			AssertEquals("Transhipment/Underbond status after ACK message processed", "ACK", mawbUnderbondDTR.C4_Status);
			AssertEquals("Transhipment/Underbond status description", "Transhipment Request Acknowledged", mawbUnderbondDTR.MovementStatusDesc);

			#endregion

			#region Customs Response

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_ApplicationCode = "NZC";
			nzcsMessage.EM_MessageType = "TWR";
			nzcsMessage.EM_MessageText = U00000358_CustomsCancelResponse;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			#endregion

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsMessage);
			Factory.Save();

			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, nzcsMessage.EM_MessageSubType);
			AssertEquals("Underbond TSWNumber is unchanged", "25232936", mawbUnderbondDTR.TSWNumber);
			AssertEquals("Transhipment/Underbond status", "CAN", mawbUnderbondDTR.C4_Status);
			AssertEquals("Transhipment/Underbond status description should be set to Cancelled", "Transhipment Request Cancelled", mawbUnderbondDTR.MovementStatusDesc);

			AssertSame(nzcsMessage.EM_LinkedObject, mawbUnderbondDTR);

			#endregion
		}

		public void TestMasterDTRResponseMessageInterpretation()
		{
			var transitDestOrg = Factory.New<OrgHeader>();
			transitDestOrg.OH_Code = "TDCODE";

			var transitDestOrgAddr = transitDestOrg.Addresses.AddNew();
			transitDestOrgAddr.OA_Address1 = "TD WareHouse 1 Address";
			transitDestOrgAddr.OA_Code = "TD1";
			transitDestOrg.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "52638P");
			Factory.Save();

			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			mawb.CM_MessageReference = "X00002259";
			mawb.CM_MAWB = "08163635261";
			mawb.CM_RL_NKLoadPort = "AUSYD";
			mawb.CM_RL_NKDischargePort = "NZAKL";
			mawb.CM_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;

			var mawbUnderbondDTR = TranshipmentRequest.Create(mawb);
			mawbUnderbondDTR.C4_MovementReason = MovementReason.Codes.DomesticTranshipmentRequest;
			mawbUnderbondDTR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			mawbUnderbondDTR.C4_OA_DestinationAddress = transitDestOrgAddr.PK;

			var outgoingUnderbondMessage = Factory.New<TSWMessage>();
			outgoingUnderbondMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingUnderbondMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingUnderbondMessage.EM_ApplicationReference = "U00000348";
			outgoingUnderbondMessage.EM_LinkedObject = mawbUnderbondDTR;

			#region Acknowledgement Response Interpretation

			var icrAckResponse = Factory.New<TSWMessage>();
			icrAckResponse.EM_ApplicationCode = "NZC";
			icrAckResponse.EM_MessageType = "TWR";
			icrAckResponse.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.Acknowledgement;
			icrAckResponse.EM_MessageText = U00000348_Acknowledgment;
			icrAckResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrAckResponse);
			Factory.Save();
			var expectedAckResponseInterpretation = "[Acknowledgement] Response for Master DTR: U00000348";
			AssertEquals("ICR Acknowledgement Response Interpretation", expectedAckResponseInterpretation, icrAckResponse.EM_MessageInterpretation);

			#endregion

			#region Bio Response Interpretation

			var icrBioResponse = Factory.New<TSWMessage>();
			icrBioResponse.EM_ApplicationCode = "NZC";
			icrBioResponse.EM_MessageType = "TWR";
			icrAckResponse.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO;
			icrBioResponse.EM_MessageText = U00000348_Bio;
			icrBioResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrBioResponse);
			Factory.Save();
			ZString expectedBIOResponseInterpretation = @"[ICR/CRE Accepted, Check Consignments for Status] Response for AirCargo ICR/CRE: X00002259

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Master DTR     : U00000348
Entry Number   : 41745165
Master Bill    : 081-63635261
Message No     : 10800

Message Status : (B07) MPI Biosecurity Cargo Report Notification - consignment status as specified

Job Responses
---------------------------------------------------------------------
--- Movement Status: DTA-Domestic Transhipment Approved ---
";
			AssertEquals("ICR BIO Response Interpretation", expectedBIOResponseInterpretation, icrBioResponse.EM_MessageInterpretation);

			#endregion

			#region NZC Response Interpretation

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_ApplicationCode = "NZC";
			nzcsMessage.EM_MessageType = "TWR";
			nzcsMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms;
			nzcsMessage.EM_MessageText = U00000348_Customs;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsMessage);
			Factory.Save();

			ZString expectedNZCResponseInterpretation = @"[ICR/CRE Accepted, Check Consignments for Status] Response for AirCargo ICR/CRE: X00002259

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Master DTR     : U00000348
Entry Number   : 41745165
Master Bill    : 081-63635261
Message No     : 10801

Message Status : (C06) Customs Cargo Report Notification - consignment status as specified

Job Responses
---------------------------------------------------------------------
--- Movement Status: DTA-Domestic Transhipment Approved ---
";
			AssertContains("ICR NZC Response Interpretation", expectedNZCResponseInterpretation, nzcsMessage.EM_MessageInterpretation);
			#endregion
		}

		public void TestContainerDTRErrorResponseLinksToCorrectTranshipmentRequest()
		{
			var transitDestOrg = Factory.New<OrgHeader>();
			transitDestOrg.OH_Code = "TDCODE";

			var transitDestOrgAddr = transitDestOrg.Addresses.AddNew();
			transitDestOrgAddr.OA_Address1 = "TD WareHouse 1 Address";
			transitDestOrgAddr.OA_Code = "TD1";
			transitDestOrg.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "52638P");
			Factory.Save();

			var oceanBill = Factory.NewWithValidTestData<CusSCAOceanBill>();
			oceanBill.CB_MessageReference = "X00002256";
			oceanBill.CB_OceanBill = "08600024861";
			oceanBill.CB_RL_NKPortOfLoading = "AUSYD";
			oceanBill.CB_RL_NKPortOfDischarge = "NZAKL";
			oceanBill.CB_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;
			oceanBill.CB_CustomsStatus = LowValueConsignmentStatusList.Codes.Cleared;
			var container = oceanBill.Containers.AddNew();

			var house1 = oceanBill.HouseBills.AddNew();
			house1.CA_HouseBill = "SYD1145";
			var packingLine1 = container.PackingLines.AddNew();
			packingLine1.CV_CA = house1.PK;
			packingLine1.CV_LineNo = 1;

			var house2 = oceanBill.HouseBills.AddNew();
			house2.CA_HouseBill = "GAZ0592882";
			var packingLine2 = container.PackingLines.AddNew();
			packingLine2.CV_CA = house2.PK;
			packingLine2.CV_LineNo = 2;

			var containerDTR = TranshipmentRequest.Create(container);
			containerDTR.C4_MovementReason = MovementReason.Codes.DomesticTranshipmentRequest;
			containerDTR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			containerDTR.C4_OA_DestinationAddress = transitDestOrgAddr.PK;
			containerDTR.C4_SendersMessageReference = "";
			Assert("Pre-condition: CusEntryNum record does not exist for Underbond", containerDTR.CusEntryNumbers.Length == 0);

			var outgoingUnderbondMessage = Factory.New<TSWMessage>();
			outgoingUnderbondMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingUnderbondMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingUnderbondMessage.EM_ApplicationReference = "U00000328";
			outgoingUnderbondMessage.EM_LinkedObject = containerDTR;

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = "RES";
			nzcsMessage.EM_MessageText = U00000328NZCSResponse;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsMessage);
			Factory.Save();

				var expectedInterpretation = @"[ICR/CRE Rejected] Response for SeaCargo ICR/CRE: X00002256

Error report
---------------------------------------------------------------------
Master DTR     : U00000328
Entry Number   : 
Master Bill    : 08600024861
Message No     : 10755

Message Status : (858) Customs Processing Error

Message Errors
---------------------------------------------------------------------
**Error** in {Declaration/ID}:-
  ECI Number : Not specified or invalid
";

				AssertEquals("EM_MessageSubType - Error response message comes from TSW Front end", TSWMessage.ResponseMessage.MessageSubTypes.TradeSingleWindow, nzcsMessage.EM_MessageSubType);
				AssertEquals("Error response '0000000' should display blank", "", containerDTR.TSWNumber);
				Assert("CusEntryNum record created for Underbond", containerDTR.CusEntryNumbers.Length > 0);

			var underbondEntryNum = containerDTR.CusEntryNumbers[0];
			AssertEquals("CE_Category", "CUS", underbondEntryNum.CE_Category);
			AssertEquals("CE_EntryType", "DTR", underbondEntryNum.CE_EntryType);
			AssertEquals("CE_RN_NKCountryCode", "NZ", underbondEntryNum.CE_RN_NKCountryCode);
			AssertEquals("CE_ParentTable", "CusUnderbond", underbondEntryNum.CE_ParentTable);
			AssertEquals("CE_ParentID", containerDTR.PK, underbondEntryNum.CE_ParentID);

			AssertEquals("Transhipment/Underbond status", "REJ", containerDTR.C4_Status);

				AssertSame(nzcsMessage.EM_LinkedObject, containerDTR);
				AssertEquals("Message rejection errors", expectedInterpretation, nzcsMessage.EM_MessageInterpretation);
			}

		public void TestAcceptedContainerDTRResponse()
		{
			var transitDestOrg = Factory.New<OrgHeader>();
			transitDestOrg.OH_Code = "TDCODE";

			var transitDestOrgAddr = transitDestOrg.Addresses.AddNew();
			transitDestOrgAddr.OA_Address1 = "TD WareHouse 1 Address";
			transitDestOrgAddr.OA_Code = "TD1";
			transitDestOrg.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "52638P");
			Factory.Save();

			var oceanBill = Factory.NewWithValidTestData<CusSCAOceanBill>();
			oceanBill.CB_MessageReference = "X00002256";
			oceanBill.CB_OceanBill = "08600024861";
			oceanBill.CB_RL_NKPortOfLoading = "AUSYD";
			oceanBill.CB_RL_NKPortOfDischarge = "NZAKL";
			oceanBill.CB_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;
			oceanBill.CB_CustomsStatus = LowValueConsignmentStatusList.Codes.Cleared;
			var container = oceanBill.Containers.AddNew();

			var house1 = oceanBill.HouseBills.AddNew();
			house1.CA_HouseBill = "SYD1145";
			var packingLine1 = container.PackingLines.AddNew();
			packingLine1.CV_CA = house1.PK;
			packingLine1.CV_LineNo = 1;

			var house2 = oceanBill.HouseBills.AddNew();
			house2.CA_HouseBill = "GAZ0592882";
			var packingLine2 = container.PackingLines.AddNew();
			packingLine2.CV_CA = house2.PK;
			packingLine2.CV_LineNo = 2;

			var containerDTR = TranshipmentRequest.Create(container);
			containerDTR.C4_MovementReason = MovementReason.Codes.DomesticTranshipmentRequest;
			containerDTR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			containerDTR.C4_OA_DestinationAddress = transitDestOrgAddr.PK;
			Assert("Pre-condition: CusEntryNum record does not exist for Underbond", containerDTR.CusEntryNumbers.Length == 0);

			var outgoingUnderbondMessage = Factory.New<TSWMessage>();
			outgoingUnderbondMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingUnderbondMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingUnderbondMessage.EM_ApplicationReference = "U00000329";
			outgoingUnderbondMessage.EM_LinkedObject = containerDTR;

			#region Bio Response

			var icrBioResponse = Factory.New<TSWMessage>();
			icrBioResponse.EM_MessageType = "RES";
			icrBioResponse.EM_MessageText = U00000329BioResponse;
			icrBioResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrBioResponse);
			Factory.Save();

				var expectedInterpretation = @"[ICR/CRE Accepted, Check Consignments for Status] Response for SeaCargo ICR/CRE: X00002256

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Master DTR     : U00000329
Entry Number   : 6965837
Master Bill    : 08600024861
Message No     : 10789

Message Status : (B07) MPI Biosecurity Cargo Report Notification - consignment status as specified

Job Responses
---------------------------------------------------------------------
--- Movement Status: DTA-Domestic Transhipment Approved ---
";

				AssertEquals("Transhipment/Underbond status after Bio message processed", "93", containerDTR.C4_Status);
				AssertEquals("Message interpretation", expectedInterpretation, icrBioResponse.EM_MessageInterpretation);

			#endregion

			#region Customs Response

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = "RES";
			nzcsMessage.EM_MessageText = U00000329NZCSResponse;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			#endregion

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsMessage);
			Factory.Save();

				expectedInterpretation = @"[ICR/CRE in Error, Check Consignments for Status] Response for SeaCargo ICR/CRE: X00002256

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Master DTR     : U00000329
Entry Number   : 6965837
Master Bill    : 08600024861
Message No     : 10788

Message Status : (C06) Customs Cargo Report Notification - consignment status as specified

Message Errors
---------------------------------------------------------------------
**Error** in {Declaration/Consignment[1]/Consignee/Unknown}:-
  Either the Consignee Name or Code must be specified
**Error** in {Declaration/Consignment[1]/Consignor/Unknown}:-
  Either the Consignor Name or Code must be specified

Job Responses
---------------------------------------------------------------------
--- Movement Status: DTA-Domestic Transhipment Approved ---
";

			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, nzcsMessage.EM_MessageSubType);
			AssertEquals("Underbond TSWNumber is updated", "6965837", containerDTR.TSWNumber);
			Assert("CusEntryNum record created for Underbond", containerDTR.CusEntryNumbers.Length > 0);

			var underbondEntryNum = containerDTR.CusEntryNumbers[0];
			AssertEquals("CE_Category", "CUS", underbondEntryNum.CE_Category);
			AssertEquals("CE_EntryType", "DTR", underbondEntryNum.CE_EntryType);
			AssertEquals("CE_RN_NKCountryCode", "NZ", underbondEntryNum.CE_RN_NKCountryCode);
			AssertEquals("CE_ParentTable", "CusUnderbond", underbondEntryNum.CE_ParentTable);
			AssertEquals("CE_ParentID", containerDTR.PK, underbondEntryNum.CE_ParentID);

			AssertEquals("Transhipment/Underbond status", "33", containerDTR.C4_Status);

			AssertSame(nzcsMessage.EM_LinkedObject, containerDTR);
			AssertEquals("Message rejection errors", expectedInterpretation, nzcsMessage.EM_MessageInterpretation);
		}

		public void TestClearedContainerDTRResponse()
		{
			var transitDestOrg = Factory.New<OrgHeader>();
			transitDestOrg.OH_Code = "TDCODE";

			var transitDestOrgAddr = transitDestOrg.Addresses.AddNew();
			transitDestOrgAddr.OA_Address1 = "TD WareHouse 1 Address";
			transitDestOrgAddr.OA_Code = "TD1";
			transitDestOrg.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "52638P");
			Factory.Save();

			var oceanBill = Factory.NewWithValidTestData<CusSCAOceanBill>();
			oceanBill.CB_MessageReference = "X00002256";
			oceanBill.CB_OceanBill = "08600024861";
			oceanBill.CB_RL_NKPortOfLoading = "AUSYD";
			oceanBill.CB_RL_NKPortOfDischarge = "NZAKL";
			oceanBill.CB_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;
			oceanBill.CB_CustomsStatus = LowValueConsignmentStatusList.Codes.Cleared;
			var container = oceanBill.Containers.AddNew();

			var house1 = oceanBill.HouseBills.AddNew();
			house1.CA_HouseBill = "SYD1145";
			var packingLine1 = container.PackingLines.AddNew();
			packingLine1.CV_CA = house1.PK;
			packingLine1.CV_LineNo = 1;

			var house2 = oceanBill.HouseBills.AddNew();
			house2.CA_HouseBill = "GAZ0592882";
			var packingLine2 = container.PackingLines.AddNew();
			packingLine2.CV_CA = house2.PK;
			packingLine2.CV_LineNo = 2;

			var containerDTR = TranshipmentRequest.Create(container);
			containerDTR.C4_MovementReason = MovementReason.Codes.DomesticTranshipmentRequest;
			containerDTR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			containerDTR.C4_OA_DestinationAddress = transitDestOrgAddr.PK;
			Assert("Pre-condition: CusEntryNum record does not exist for Underbond", containerDTR.CusEntryNumbers.Length == 0);

			var outgoingUnderbondMessage = Factory.New<TSWMessage>();
			outgoingUnderbondMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingUnderbondMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingUnderbondMessage.EM_ApplicationReference = "U00000348";
			outgoingUnderbondMessage.EM_LinkedObject = containerDTR;

			#region Acknowledgement Response

				var icrAckResponse = Factory.New<TSWMessage>();
				icrAckResponse.EM_ApplicationCode = "NZC";
				icrAckResponse.EM_MessageType = "TWR";
				icrAckResponse.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.Acknowledgement;
				icrAckResponse.EM_MessageText = U00000348_Acknowledgment;
				icrAckResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrAckResponse);
			Factory.Save();

				var expectedInterpretation = "[Acknowledgement] Response for Master DTR: U00000348";

				AssertEquals("Transhipment/Underbond status after ACK message processed", "ACK", containerDTR.C4_Status);
				AssertEquals("Transhipment/Underbond status description", "Transhipment Request Acknowledged", containerDTR.MovementStatusDesc);
				AssertEquals("Message interpretation", expectedInterpretation, icrAckResponse.EM_MessageInterpretation);

			#endregion

			#region Bio Response

				var icrBioResponse = Factory.New<TSWMessage>();
				icrBioResponse.EM_ApplicationCode = "NZC";
				icrBioResponse.EM_MessageType = "TWR";
				icrBioResponse.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO;
				icrBioResponse.EM_MessageText = U00000348_Bio;
				icrBioResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrBioResponse);
			Factory.Save();

				expectedInterpretation = @"[ICR/CRE Accepted, Check Consignments for Status] Response for SeaCargo ICR/CRE: X00002256

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Master DTR     : U00000348
Entry Number   : 41745165
Master Bill    : 08600024861
Message No     : 10800

Message Status : (B07) MPI Biosecurity Cargo Report Notification - consignment status as specified

Job Responses
---------------------------------------------------------------------
--- Movement Status: DTA-Domestic Transhipment Approved ---
";

				AssertEquals("Transhipment/Underbond status after Bio message processed", "93", containerDTR.C4_Status);
				AssertEquals("Message interpretation", expectedInterpretation, icrBioResponse.EM_MessageInterpretation);

			#endregion

			#region Customs Response

				var nzcsMessage = Factory.New<TSWMessage>();
				nzcsMessage.EM_ApplicationCode = "NZC";
				nzcsMessage.EM_MessageType = "TWR";
				nzcsMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms;
				nzcsMessage.EM_MessageText = U00000348_Customs;
				nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				Factory.Save();

			#endregion

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsMessage);
			Factory.Save();

				expectedInterpretation = @"[ICR/CRE Accepted, Check Consignments for Status] Response for SeaCargo ICR/CRE: X00002256

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Master DTR     : U00000348
Entry Number   : 41745165
Master Bill    : 08600024861
Message No     : 10801

Message Status : (C06) Customs Cargo Report Notification - consignment status as specified

Job Responses
---------------------------------------------------------------------
--- Movement Status: DTA-Domestic Transhipment Approved ---
";

			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, nzcsMessage.EM_MessageSubType);
			AssertEquals("Underbond TSWNumber is updated", "41745165", containerDTR.TSWNumber);
			Assert("CusEntryNum record created for Underbond", containerDTR.CusEntryNumbers.Length > 0);

			var underbondEntryNum = containerDTR.CusEntryNumbers[0];
			AssertEquals("CE_Category", "CUS", underbondEntryNum.CE_Category);
			AssertEquals("CE_EntryType", "DTR", underbondEntryNum.CE_EntryType);
			AssertEquals("CE_RN_NKCountryCode", "NZ", underbondEntryNum.CE_RN_NKCountryCode);
			AssertEquals("CE_ParentTable", "CusUnderbond", underbondEntryNum.CE_ParentTable);
			AssertEquals("CE_ParentID", containerDTR.PK, underbondEntryNum.CE_ParentID);

			AssertEquals("Transhipment/Underbond status", "33", containerDTR.C4_Status);
			AssertEquals("Transhipment/Underbond status description", "DTR Approved", containerDTR.MovementStatusDesc);

			AssertSame(nzcsMessage.EM_LinkedObject, containerDTR);
			AssertEquals("Message rejection errors", expectedInterpretation, nzcsMessage.EM_MessageInterpretation);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
		}

		#endregion

		#region Messages

		#region U00000328

		public const string U00000328NZCSResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20230717114415</IssueDateTime>
    <FunctionalReferenceID>10755</FunctionalReferenceID>
    <FunctionCode>48</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID />
        <FunctionalReferenceID>U00000328</FunctionalReferenceID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Error>
      <ValidationCode>625</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>D014</DocumentSectionCode>
      </Pointer>
    </Error>
    <Status>
      <EffectiveDateTime formatCode=""204"">20230717114415</EffectiveDateTime>
      <NameCode>858</NameCode>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		#endregion

		#region U00000329

		public const string U00000329BioResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20230719154847</IssueDateTime>
    <FunctionalReferenceID>10789</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>6965837</ID>
        <AcceptanceDateTime formatCode=""204"">20230719154847</AcceptanceDateTime>
        <FunctionalReferenceID>U00000329</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">CIR</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">DTA</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <TransportContractDocument>
            <ID>08163635246</ID>
            <TypeCode>MB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20230719154847</EffectiveDateTime>
      <NameCode>B07</NameCode>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		public const string U00000329NZCSResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20230719154827</IssueDateTime>
    <FunctionalReferenceID>10788</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>6965837</ID>
        <AcceptanceDateTime formatCode=""204"">20230719154827</AcceptanceDateTime>
        <FunctionalReferenceID>U00000329</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">ERR</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">DTA</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <TransportContractDocument>
            <ID>08163635246</ID>
            <TypeCode>MB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Error>
      <ValidationCode>632</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>27A</DocumentSectionCode>
        <TagID>246</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>633</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>30A</DocumentSectionCode>
        <TagID>246</TagID>
      </Pointer>
    </Error>
    <Status>
      <EffectiveDateTime formatCode=""204"">20230719154827</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		#endregion

		#region U00000344

		public const string U00000344NZCSResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20230717194556</IssueDateTime>
    <FunctionalReferenceID>10778</FunctionalReferenceID>
    <FunctionCode>48</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>00000000</ID>
        <FunctionalReferenceID>U00000344</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Error>
      <ValidationCode>5027</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>26B</DocumentSectionCode>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>1046</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
        <TagID>30A</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>632</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>27A</DocumentSectionCode>
        <TagID>246</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>632</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>27A</DocumentSectionCode>
      </Pointer>
    </Error>
    <Status>
      <EffectiveDateTime formatCode=""204"">20230717194556</EffectiveDateTime>
      <NameCode>841</NameCode>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		#endregion

		#region U00000345

		public const string U00000345NZCSResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20230718123746</IssueDateTime>
    <FunctionalReferenceID>10782</FunctionalReferenceID>
    <FunctionCode>48</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID />
        <FunctionalReferenceID>U00000345</FunctionalReferenceID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Error>
      <ValidationCode>625</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>D014</DocumentSectionCode>
      </Pointer>
    </Error>
    <Status>
      <EffectiveDateTime formatCode=""204"">20230718123746</EffectiveDateTime>
      <NameCode>858</NameCode>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		#endregion

		#region U00000347

		public const string U00000347BioResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20230719154847</IssueDateTime>
    <FunctionalReferenceID>10789</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>6965837</ID>
        <AcceptanceDateTime formatCode=""204"">20230719154847</AcceptanceDateTime>
        <FunctionalReferenceID>U00000347</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">CIR</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">DTA</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <TransportContractDocument>
            <ID>08163635246</ID>
            <TypeCode>MB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20230719154847</EffectiveDateTime>
      <NameCode>B07</NameCode>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		public const string U00000347NZCSResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20230719154827</IssueDateTime>
    <FunctionalReferenceID>10788</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>6965837</ID>
        <AcceptanceDateTime formatCode=""204"">20230719154827</AcceptanceDateTime>
        <FunctionalReferenceID>U00000347</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">ERR</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">DTA</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <TransportContractDocument>
            <ID>08163635246</ID>
            <TypeCode>MB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Error>
      <ValidationCode>632</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>27A</DocumentSectionCode>
        <TagID>246</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>633</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>30A</DocumentSectionCode>
        <TagID>246</TagID>
      </Pointer>
    </Error>
    <Status>
      <EffectiveDateTime formatCode=""204"">20230719154827</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		#endregion

		#region U00000348

		public const string U00000348_Acknowledgment = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20230721130613</IssueDateTime>
    <FunctionalReferenceID>10799</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>41745165</ID>
        <FunctionalReferenceID>U00000348</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20230721130613</EffectiveDateTime>
      <NameCode>ACK</NameCode>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		public const string U00000348_Bio = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20230721130633</IssueDateTime>
    <FunctionalReferenceID>10800</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>41745165</ID>
        <AcceptanceDateTime formatCode=""204"">20230721130633</AcceptanceDateTime>
        <FunctionalReferenceID>U00000348</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">CIR</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">DTA</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <TransportContractDocument>
            <ID>08163635261</ID>
            <TypeCode>MB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20230721130633</EffectiveDateTime>
      <NameCode>B07</NameCode>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		public const string U00000348_Customs = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20230721130653</IssueDateTime>
    <FunctionalReferenceID>10801</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>41745165</ID>
        <AcceptanceDateTime formatCode=""204"">20230721130653</AcceptanceDateTime>
        <FunctionalReferenceID>U00000348</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">CIR</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">DTA</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <TransportContractDocument>
            <ID>08163635261</ID>
            <TypeCode>MB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20230721130653</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		#endregion

		#region U00000358

		public const string U00000358_AcknowledgmentCancelResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20230802123749</IssueDateTime>
    <FunctionalReferenceID>10861</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>25232936</ID>
        <FunctionalReferenceID>U00000358</FunctionalReferenceID>
        <VersionID>2</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20230802123749</EffectiveDateTime>
      <NameCode>ACK</NameCode>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		public const string U00000358_CustomsCancelResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20230802123759</IssueDateTime>
    <FunctionalReferenceID>10862</FunctionalReferenceID>
    <FunctionCode>34</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>25232936</ID>
        <FunctionalReferenceID>U00000358</FunctionalReferenceID>
        <VersionID>2</VersionID>
        <CancellationDateTime formatCode=""204"">20230802123759</CancellationDateTime>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20230802123759</EffectiveDateTime>
      <NameCode>814</NameCode>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		#endregion

		#endregion
	}
}
