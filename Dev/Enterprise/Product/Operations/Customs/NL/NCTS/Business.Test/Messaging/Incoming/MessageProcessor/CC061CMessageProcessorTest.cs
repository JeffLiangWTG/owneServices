using System;
using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC061CMessageProcessorTest : NCTSResponseMessageProcessorAbstractTest<CC061CMessageProcessor, ICC061CDataProvider>
{
	public void TestProcessMessage_UAP()
	{
		incomingMessage = SetupAndProcessMessage(NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);
		var nctsHeader = incomingMessage.EM_LinkedObject as NctsHeader;

		CombineAssertions(() =>
		{
			AssertEquals("Message is processed with Customs Status UAP", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("Customs Status is set to CO1", NLNCTS5ArrivalCustomsStatusList.Codes.DecisionToControl, nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
		});
	}

	public void TestProcessMessage_ULR()
	{
		incomingMessage = SetupAndProcessMessage(NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);
		var nctsHeader = incomingMessage.EM_LinkedObject as NctsHeader;

		CombineAssertions(() =>
		{
			AssertEquals("Message is processed with Customs Status ULR", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("Customs Status is set to CO1", NLNCTS5ArrivalCustomsStatusList.Codes.DecisionToControl, nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
		});
	}

	public void TestCTLService()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);
		var nctsHeader = incomingMessage.EM_LinkedObject as NctsHeader;

		CombineAssertions(() => 
		{ 
			var services = nctsHeader.Services.Find(x => x.ES_ServiceCode == NLConstants.ServiceTypes.ControlByCustoms);
			AssertNotNull(services);
			AssertEquals("1 CTL service should be added", 1, services.Count());
			AssertEquals("Booked Date should be 2022-04-01T12:53:26", new DateTime(2024, 8, 9, 14, 09, 32), services.First().ES_Booked);
			AssertEquals("Service note", "Decision for a physical control", services.First().ES_ServiceNote);
			AssertEquals("Service reference should be equal to MRN", "24DE966882266928800171", services.First().ES_References);
		});
	}

	public void TestCIPEvent()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);
		var nctsHeader = incomingMessage.EM_LinkedObject as NctsHeader;

		CombineAssertions(() =>
		{
			var cipEvents = nctsHeader.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.CustomsImpedimentReceivedCode);
			AssertNotNull(cipEvents);
			AssertEquals("1 CIP-event should be added", 1, cipEvents.Count());
			AssertEquals("Event time should be 2022-04-01 12:53:26", new DateTime(2024, 8, 9, 14, 09, 32), cipEvents.First().SL_EventTime);
		});
	}

	public void TestDiscarededMessage()
	{
		var dataProviderMock = Mock.Of<ICC061CDataProvider>(p =>
									p.MRN == "24DE966882266928800171" &&
									p.ControlNotificationDateAndTime == new DateTime(2024, 8, 9, 14, 09, 32)
								);
		incomingMessage = SetupAndProcessMessage(NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, InitialPhase, InitialMessageStatus, dataProviderMock, SetupNctsHeader);
		var nctsHeader = incomingMessage.EM_LinkedObject as NctsHeader;

		CombineAssertions(() =>
		{
			AssertEquals("EDIMessage - Status should be set to 'DCD - Discarded'", EDIMessageStatusList.Codes.Discarded, incomingMessage.EM_Status);
			var expectedMessage = "The message with interchange is discarded because its 'Status at Customs' is not blanks, UAP or ULR.";
			var actualMessage = incomingMessage.Notes.FindByDescription(NLConstants.Notes.Descriptions.ProcessingLog).FirstOrDefault()?.ST_NoteText;
			AssertEquals("Log reason for discarded message", expectedMessage, actualMessage);
		});
	}

	protected override ICC061CDataProvider GetMessageDataProviderMock()
	{
		return Mock.Of<ICC061CDataProvider>(p =>
			p.MRN == "24DE966882266928800171" &&
			p.ControlNotificationDateAndTime == new DateTime(2024, 8, 9, 14, 09, 32)
		);
	}

	protected override Action<NctsHeader> SetupNctsHeader => (nctsHeader) =>
	{
		var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Netherlands);
		mrnEntryNumber.CE_EntryNum = "24DE966882266928800171";
	};

	protected override string MovementType => NctsMovementType.Codes.Arrival;

	protected override string InitialCustomsStatus => string.Empty;

	protected override string InitialPhase => NCTS5ArrivalPhaseList.Codes.Arrival;

	protected override string ExpectedCustomsStatus => NLNCTS5ArrivalCustomsStatusList.Codes.DecisionToControl;

	protected override string ExpectedPhase => NCTS5ArrivalPhaseList.Codes.Arrival;

	protected override bool SetNewPhaseExpected => false;

	protected override string ExpectedMessageStatus => LogicalStatusList.Codes.Accepted;
}
