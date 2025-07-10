using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class InBondDeleteMessageSendingObjectTest : TestCaseWithFactory
	{
		public void TestInBondDeleteMessageSendingObjectMembers()
		{
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			header.BH_CarrierSCAC = "ABC";
			header.BH_ImportConveyanceName = "APL VESSEL";
			header.BH_VoyageNumber = "1234A";
			header.BH_ETA = new ZDateTime(2012, 10, 02, 18, 20, 10);
			header.BH_FTZMove = ZBool.False;
			header.BH_PortUnladingDCode = "5687";
			header.BH_FIRMS = "FOD3";
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			moveHeader.InBondNumber = "777888125";
			moveHeader.BM_InBondCarrierSCAC = "NFCT";
			moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			var outgoingMessage = Factory.NewWithValidTestData<MQEDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = moveHeader;
			outgoingMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureOriginal;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondTransaction;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_MessageText = @"B018888XJ5QP                                               <<MSGNO PLACEHOLDER>>10A62123456789   OTT112341234500000323123456789012 Y                            20ABC 11  APL VESSEL             1234A       5687100212                         30A 0001ABC 12345678901                                             0000000124  Y  8888XJ5QP00014";
			Factory.Save();
			moveHeader.Reload();
			moveHeader.Messages.Load();
			var deleteMessageSendingObject = new InBondDeleteMessageSendingObject(moveHeader);
			var inbondQPHeader = (IInBondQPHeader)deleteMessageSendingObject;
			AssertNotNull(inbondQPHeader);
			var inbondNumber = inbondQPHeader.InBondNumber;
			var inbondEntryType = inbondQPHeader.EntryType;
			var inbondCarrierSCAC = inbondQPHeader.InbondCarrierSCAC;
			AssertEquals("Not equal '777888125', it is moveHeader's InBondNumber", "123456789", inbondNumber);
			AssertEquals("Not equal '61', it is moveHeader's InBondEntryType", "62", inbondEntryType);
			AssertEquals("Not equal 'NFCT', it is moveHeader's InBondCarrierSCAC", "OTT1", inbondCarrierSCAC);
			AssertEquals(TransportModeCodes.Codes.VesselContainer, inbondQPHeader.TransportMode);
		}
	}
}
