using System;
using System.Collections.Generic;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.NL.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC906CMessageProcessorTest : NCTSResponseMessageProcessorAbstractTest<CC906CMessageProcessor, ICC906CDataProvider>
{
	public void TestMessageStatusWhenMessagesStatusACK()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, LogicalStatusList.Codes.Acknowledged, GetMessageDataProviderMock(), SetupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		CombineAssertions(() =>
		{
			AssertEquals("EDIMessage - Status should be 'PRS - Processed'", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("NCTSHeader - Message Status should be set to 'ERR - Error'", LogicalStatusList.Codes.Error, nctsHeader.EffectiveMessageStatus);
		});
	}

	public void TestMessageStatus_MRN()
	{
		var dataProviderMock = Mock.Of<ICC906CDataProvider>(p =>
			p.MRN == "TestMRN" &&
			p.FunctionalErrors == new List<INCTSFunctionalError>()
			{
				Mock.Of<INCTSFunctionalError>(e =>
					e.ErrorPointer == "EP1" &&
					e.ErrorCode == "12" &&
					e.ErrorReason == "bad type one" &&
					e.OriginalAttributeValue == "11"),
				Mock.Of<INCTSFunctionalError>(e =>
					e.ErrorPointer == "EP2" &&
					e.ErrorCode == "15" &&
					e.ErrorReason == "bad type two" &&
					e.OriginalAttributeValue == "12"),
			}
		);

		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, dataProviderMock, SetupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		CombineAssertions(() =>
		{
			AssertEquals("EDIMessage - Status should be 'PRS - Processed'", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("NCTSHeader - Message Status should be set to 'ERR - Error'", LogicalStatusList.Codes.Error, nctsHeader.EffectiveMessageStatus);
		});
	}

	public void TestMessageStatusWhenMessageStatusACK_MRN()
	{
		var dataProviderMock = Mock.Of<ICC906CDataProvider>(p =>
			p.MRN == "TestMRN" &&
			p.FunctionalErrors == new List<INCTSFunctionalError>()
			{
				Mock.Of<INCTSFunctionalError>(e =>
					e.ErrorPointer == "EP1" &&
					e.ErrorCode == "12" &&
					e.ErrorReason == "bad type one" &&
					e.OriginalAttributeValue == "11"),
				Mock.Of<INCTSFunctionalError>(e =>
					e.ErrorPointer == "EP2" &&
					e.ErrorCode == "15" &&
					e.ErrorReason == "bad type two" &&
					e.OriginalAttributeValue == "12"),
			}
		);

		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, LogicalStatusList.Codes.Acknowledged, dataProviderMock, SetupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		CombineAssertions(() =>
		{
			AssertEquals("EDIMessage - Status should be 'PRS - Processed'", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("NCTSHeader - Message Status should be set to 'ERR - Error'", LogicalStatusList.Codes.Error, nctsHeader.EffectiveMessageStatus);
		});
	}
	public void TestMessageStatus_CorrelationID()
	{
		Action<NctsHeader> setupNctsHeader = (nctsHeader) =>
		{
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "TestLRN";

			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Netherlands);
			mrnEntryNumber.CE_EntryNum = "TestMRN";

			var outgoingEdiInterchange = nctsHeader.Factory.New<EDIInterchange>();
			outgoingEdiInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.NLCustoms;
			outgoingEdiInterchange.EI_InterchangeNum = "71";
			outgoingEdiInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outgoingEdiInterchange.EI_Status = EDIInterchangeStatusList.Codes.Sent;

			var outgoingEdiMessage = nctsHeader.Factory.New<NCTSMessage>();
			outgoingEdiMessage.EM_ApplicationCode = ApplicationCodeList.Codes.NLCustoms;
			outgoingEdiMessage.EM_EI = outgoingEdiInterchange.PK;
			outgoingEdiMessage.EM_LinkedObject = nctsHeader.MovementHeader;
			outgoingEdiMessage.EM_MessageNum = "00000000000001";
			outgoingEdiMessage.EM_MessageSubType = "015";
			outgoingEdiMessage.EM_MessageType = NLEDIMessageTypes.Codes.NCT;
			outgoingEdiMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			outgoingEdiMessage.EM_Status = EDIMessageStatusList.Codes.Sent;
		};

		var dataProviderMock = Mock.Of<ICC906CDataProvider>(p =>
			p.LRN == "InvalidLRN" &&
			p.MRN == "InvalidMRN" &&
			p.CorrelationIdentifier == "00000000000001" &&
			p.FunctionalErrors == new List<INCTSFunctionalError>()
			{
				Mock.Of<INCTSFunctionalError>(e =>
					e.ErrorPointer == "EP1" &&
					e.ErrorCode == "12" &&
					e.ErrorReason == "bad type one" &&
					e.OriginalAttributeValue == "11"),
				Mock.Of<INCTSFunctionalError>(e =>
					e.ErrorPointer == "EP2" &&
					e.ErrorCode == "15" &&
					e.ErrorReason == "bad type two" &&
					e.OriginalAttributeValue == "12"),
			}
		);

		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, dataProviderMock, setupNctsHeader);
		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		CombineAssertions(() =>
		{
			AssertEquals("EDIMessage - Status should be set to 'PRS - Processed'", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("NctsHeader - Message Status should be set to 'ERR - Error'", LogicalStatusList.Codes.Error, nctsHeader.EffectiveMessageStatus);
		});
	}

	protected override ICC906CDataProvider GetMessageDataProviderMock()
	{
		return Mock.Of<ICC906CDataProvider>(p =>
			p.LRN == "TestLRN" &&
			p.FunctionalErrors == new List<INCTSFunctionalError>()
			{
						Mock.Of<INCTSFunctionalError>(e =>
						e.ErrorPointer == "EP1" &&
						e.ErrorCode == "12" &&
						e.ErrorReason == "bad type one" &&
						e.OriginalAttributeValue == "11"),
						Mock.Of<INCTSFunctionalError>(e =>
						e.ErrorPointer == "EP2" &&
						e.ErrorCode == "15" &&
						e.ErrorReason == "bad type two" &&
						e.OriginalAttributeValue == "12"),
			}
		);
	}

	protected override bool SetNewCustomsStatusExpected => false;

	protected override bool SetNewPhaseExpected => false;

	protected override Action<NctsHeader> SetupNctsHeader => (nctsHeader) =>
	{
		nctsHeader.MovementHeader.BM_PaperlessInbondNum = "TestLRN";

		var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Netherlands);
		mrnEntryNumber.CE_EntryNum = "TestMRN";
	};

	protected override string ExpectedMessageStatus => LogicalStatusList.Codes.Error;
}
