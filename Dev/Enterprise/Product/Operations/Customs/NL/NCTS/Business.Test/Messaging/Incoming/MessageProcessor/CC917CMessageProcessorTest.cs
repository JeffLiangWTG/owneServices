using System;
using System.Collections.Generic;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.NL.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC917CMessageProcessorTest : NCTSResponseMessageProcessorAbstractTest<CC917CMessageProcessor, ICC917CDataProvider>
{
	public void TestFindDeclarationOnMRN()
	{
		var dataProviderMock = Mock.Of<ICC917CDataProvider>(p =>
					p.LRN == "LRN321" &&
					p.MRN == "22NL000000000012J1" &&
					p.XmlErrors == new List<INCTSFunctionalError>
					{
						Mock.Of<INCTSFunctionalError>(x =>
							x.SequenceNumeric == 1 &&
							x.ErrorColumnNumber == 2 &&
							x.ErrorCode == "52" &&
							x.ErrorPointer == "CustomsOfficeOfDeclaration" &&
							x.ErrorReason == "Invalid character > detected. Please remove character" &&
							x.OriginalAttributeValue == "BENL>01010001")
					}
				);
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, dataProviderMock, SetupNctsHeader);
		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		CombineAssertions(() =>
		{
			AssertEquals("EDIMessage - Status should be set to 'PRS - Processed'", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("NctsHeader - Message Status should be set to 'ERR - Error'", LogicalStatusList.Codes.Error, nctsHeader.EffectiveMessageStatus);
		});
	}

	public void TestFindDeclarationOnCorrelationId()
	{
		Action<NctsHeader> setupNctsHeader = (nctsHeader) =>
		{
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRN123";

			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Netherlands);
			mrnEntryNumber.CE_EntryNum = "22NL000000000012J1";

			var outgoingEdiInterchange = Factory.New<EDIInterchange>();
			outgoingEdiInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.NLCustoms;
			outgoingEdiInterchange.EI_InterchangeNum = "71";
			outgoingEdiInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outgoingEdiInterchange.EI_Status = EDIInterchangeStatusList.Codes.Sent;

			var outgoingEdiMessage = Factory.New<NCTSMessage>();
			outgoingEdiMessage.EM_ApplicationCode = ApplicationCodeList.Codes.NLCustoms;
			outgoingEdiMessage.EM_EI = outgoingEdiInterchange.PK;
			outgoingEdiMessage.EM_LinkedObject = nctsHeader.MovementHeader;
			outgoingEdiMessage.EM_MessageNum = "00000000000001";
			outgoingEdiMessage.EM_MessageSubType = "015";
			outgoingEdiMessage.EM_MessageType = NLEDIMessageTypes.Codes.NCT;
			outgoingEdiMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			outgoingEdiMessage.EM_Status = EDIMessageStatusList.Codes.Sent;

			var arrivalNcts = Factory.New<NctsHeader>();
			arrivalNcts.SetMovementType(NctsMovementType.Codes.Arrival);

			var mrnEntryNumberArrival = CusEntryNumber.LoadOrCreate(arrivalNcts, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Netherlands);
			mrnEntryNumberArrival.CE_EntryNum = "22NL000000000012J1";

			var outgoingEdiInterchangeArrival = Factory.New<EDIInterchange>();
			outgoingEdiInterchangeArrival.EI_ApplicationCode = ApplicationCodeList.Codes.NLCustoms;
			outgoingEdiInterchangeArrival.EI_InterchangeNum = "72";
			outgoingEdiInterchangeArrival.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outgoingEdiInterchangeArrival.EI_Status = EDIInterchangeStatusList.Codes.Sent;
			outgoingEdiInterchangeArrival.EI_SessionGUID = ZGuid.BrettsGuid;

			var outgoingEdiMessageArrival = Factory.New<NCTSMessage>();
			outgoingEdiMessageArrival.EM_ApplicationCode = ApplicationCodeList.Codes.NLCustoms;
			outgoingEdiMessageArrival.EM_EI = outgoingEdiInterchangeArrival.PK;
			outgoingEdiMessageArrival.EM_LinkedObject = arrivalNcts;
			outgoingEdiMessageArrival.EM_MessageNum = "00000000000002";
			outgoingEdiMessageArrival.EM_MessageSubType = "013";
			outgoingEdiMessageArrival.EM_MessageType = NLEDIMessageTypes.Codes.NCT;
			outgoingEdiMessageArrival.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			outgoingEdiMessageArrival.EM_Status = EDIMessageStatusList.Codes.Sent;
		};
		var dataProviderMock = Mock.Of<ICC917CDataProvider>(p =>
					p.MRN == "22NL000000000012J1" &&
					p.CorrelationIdentifier == "00000000000002" &&
					p.XmlErrors == new List<INCTSFunctionalError>
					{
						Mock.Of<INCTSFunctionalError>(x =>
							x.SequenceNumeric == 1 &&
							x.ErrorColumnNumber == 2 &&
							x.ErrorCode == "52" &&
							x.ErrorPointer == "CustomsOfficeOfDeclaration" &&
							x.ErrorReason == "Invalid character > detected. Please remove character" &&
							x.OriginalAttributeValue == "BENL>01010001")
					}
				);
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, dataProviderMock, setupNctsHeader);
		var linkedObject = incomingMessage.EM_LinkedObject;

		CombineAssertions(() =>
		{
			AssertEquals("EDIMessage - Status should be set to 'PRS - Processed'", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("linked object is NCTS Header", true, linkedObject is NctsHeader);
			AssertEquals("linked Header is Arrival", true, linkedObject is NctsHeader nctsHeader && nctsHeader.BH_HeaderType == NctsMovementType.Codes.Arrival);
		});
	}

	protected override ICC917CDataProvider GetMessageDataProviderMock()
	{
		return Mock.Of<ICC917CDataProvider>(p =>
					p.LRN == "LRN123" &&
					p.XmlErrors == new List<INCTSFunctionalError>
					{
						Mock.Of<INCTSFunctionalError>(x =>
							x.SequenceNumeric == 1 &&
							x.ErrorColumnNumber == 2 &&
							x.ErrorCode == "52" &&
							x.ErrorPointer == "CustomsOfficeOfDeclaration" &&
							x.ErrorReason == "Invalid character > detected. Please remove character" &&
							x.OriginalAttributeValue == "BENL>01010001")
					}
		);
	}

	protected override Action<NctsHeader> SetupNctsHeader => (nctsHeader) =>
	{
		nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRN123";
		var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Netherlands);
		mrnEntryNumber.CE_EntryNum = "22NL000000000012J1";
	};

	protected override bool SetNewMessageStatusExpected => true;

	protected override string ExpectedMessageStatus => LogicalStatusList.Codes.Error;
}
