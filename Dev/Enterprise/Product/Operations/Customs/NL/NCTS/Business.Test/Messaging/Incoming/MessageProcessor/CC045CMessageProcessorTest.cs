using System;
using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC045CMessageProcessorTest : NCTSResponseMessageProcessorAbstractTest<CC045CMessageProcessor, ICC045CDataProvider>
{
	public void TestGuaranteeUpdated()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		CombineAssertions(() =>
		{
			var logs = nctsHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsGuaranteeUpdated.Code && (x.SL_Reference == NLNctsConstants.Logs.References.CC006CMessageReceived || x.SL_Reference == NLNctsConstants.Logs.References.CC045CMessageReceived));
			AssertEquals("Only 1 CGU-event should have been created", 1, logs.Count());
			AssertEquals("Transaction", 2, guaranteeHeader.CusGuaranteeLineTransactions.Count(x => x.CPL_Reference == movementHeader.BM_PaperlessInbondNum && x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Confirmed));
		});
	}

	public void TestCESEventLog()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);
		Factory.Save();

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		CombineAssertions(() =>
		{
			var logs = nctsHeader.MovementHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && x.SL_Reference == nctsHeader.MovementHeader.BM_CustomsStatus);
			AssertEquals("Only 1 CES-event should have been created", 1, logs.Count());
			AssertEquals(new ZDateTime(2023, 6, 7, 10, 11, 12), logs.FirstOrDefault().SL_EventTime);
		});
	}

	public void TestDiscardedMessage_WRO()
	{
		Action<NctsHeader> setupNctsHeader = (nctsHeader) => SetupNctsHeaderForCC045C(nctsHeader, false, string.Empty);
		incomingMessage = SetupAndProcessMessage(NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), setupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		CombineAssertions(() =>
		{
			AssertEquals("EDIMessage - Status should be set to 'DCD - Discarded'", EDIMessageStatusList.Codes.Discarded, incomingMessage.EM_Status);

			var actualMessage = incomingMessage.Notes.FindByDescription(NLConstants.Notes.Descriptions.ProcessingLog).FirstOrDefault()?.ST_NoteText;
			AssertEquals("Log reason for discarded message", "The message with interchange 0 is discarded because its 'Status at Customs' has already the status WRO.", actualMessage);
		});
	}

	public void TestDiscardedMessage_CGULogForCC045C()
	{
		Action<NctsHeader> setupNctsHeader = (nctsHeader) => SetupNctsHeaderForCC045C(nctsHeader, false, NLNctsConstants.Logs.References.CC045CMessageReceived);
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), setupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		CombineAssertions(() =>
		{
			AssertEquals("EDIMessage - Status should be set to 'DCD - Discarded'", EDIMessageStatusList.Codes.Discarded, incomingMessage.EM_Status);

			var actualMessage = incomingMessage.Notes.FindByDescription(NLConstants.Notes.Descriptions.ProcessingLog).FirstOrDefault()?.ST_NoteText;
			AssertEquals("Log reason for discarded message", NCTSResponseMessageHelper.DiscardedMessageByLogEvent, actualMessage);
		});
	}

	public void TestDiscardedMessage_CGULogForCC006()
	{
		Action<NctsHeader> setupNctsHeader = (nctsHeader) => SetupNctsHeaderForCC045C(nctsHeader, false, NLNctsConstants.Logs.References.CC006CMessageReceived);
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), setupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		AssertEquals("EDIMessage - Status should not be set to 'DCD - Discarded'", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
	}

	public void TestDiscardedMessage_WRO_CGULogForCC045C()
	{
		Action<NctsHeader> setupNctsHeader = (nctsHeader) => SetupNctsHeaderForCC045C(nctsHeader, false, NLNctsConstants.Logs.References.CC045CMessageReceived);
		incomingMessage = SetupAndProcessMessage(NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), setupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		CombineAssertions(() =>
		{
			AssertEquals("EDIMessage - Status should be set to 'DCD - Discarded'", EDIMessageStatusList.Codes.Discarded, incomingMessage.EM_Status);

			var actualMessage = incomingMessage.Notes.FindByDescription(NLConstants.Notes.Descriptions.ProcessingLog).FirstOrDefault()?.ST_NoteText;
			AssertEquals("Log reason for discarded message", "The message with interchange 0 is discarded because its 'Status at Customs' has already the status WRO. " + NCTSResponseMessageHelper.DiscardedMessageByLogEvent, actualMessage);
		});
	}

	public void TestDiscardedMessage_WRO_CGULogForCC006C()
	{
		Action<NctsHeader> setupNctsHeader = (nctsHeader) => SetupNctsHeaderForCC045C(nctsHeader, false, NLNctsConstants.Logs.References.CC006CMessageReceived);
		incomingMessage = SetupAndProcessMessage(NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), setupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		CombineAssertions(() =>
		{
			AssertEquals("EDIMessage - Status should be set to 'DCD - Discarded'", EDIMessageStatusList.Codes.Discarded, incomingMessage.EM_Status);

			var actualMessage = incomingMessage.Notes.FindByDescription(NLConstants.Notes.Descriptions.ProcessingLog).FirstOrDefault()?.ST_NoteText;
			AssertEquals("Log reason for discarded message", "The message with interchange 0 is discarded because its 'Status at Customs' has already the status WRO.", actualMessage);
		});
	}

	protected override ICC045CDataProvider GetMessageDataProviderMock()
	{
		return Mock.Of<ICC045CDataProvider>(provider =>
			provider.MRN == "TestMRN" &&
			provider.PreparationDateTime == new DateTime(2024, 8, 9, 10, 11, 12) &&
			provider.WriteOffDate == new DateTime(2023, 6, 7) &&
			provider.Guarantor == Mock.Of<INCTSPartyWithAddressProvider>(guarantor =>
				guarantor.Id == "TestGuarantorID" &&
				guarantor.Name == "TestGuarantorName" &&
				guarantor.Address == Mock.Of<INCTSAddressProvider>(address =>
					address.StreetAndNumber == "TestGuarantorStreetAndNumber" &&
					address.City == "TestGuarantorCity" &&
					address.Postcode == "TestGuarantorPostalCode" &&
					address.Country == "TestGuarantorCountry"
				)
			)
		);
	}

	protected override string MovementType => NctsMovementType.Codes.Departure;

	protected override string InitialCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;

	protected override string InitialPhase => NCTS5DeparturePhaseList.Codes.Declaration;

	protected override Action<NctsHeader> SetupNctsHeader => (nctsHeader) => SetupNctsHeaderForCC045C(nctsHeader, true, string.Empty);

	protected override string ExpectedCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed;

	protected override string ExpectedPhase => NCTS5DeparturePhaseList.Codes.Declaration;

	protected override string ExpectedMessageStatus => LogicalStatusList.Codes.Accepted;

	void SetupNctsHeaderForCC045C(NctsHeader nctsHeader, bool setupGuarantee, string logCGUmessage)
	{
		NCTSTestHelper.SetupC0009ForEuAndCtCountries(nctsHeader.Factory);
		var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Netherlands);
		mrnEntryNumber.CE_EntryNum = "TestMRN";
		if (setupGuarantee)
		{
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "TestLRN";
			guaranteeHeader = IncomingMessageTestHelper.SetupGuarantee(nctsHeader, "TestLRN");
		}
		if (!string.IsNullOrEmpty(logCGUmessage))
		{
			nctsHeader.Logs.AddNew(AutoEvents.CustomsGuaranteeUpdated, logCGUmessage);
		}
	}

	NL.Business.CusGuaranteeHeader guaranteeHeader;
}
