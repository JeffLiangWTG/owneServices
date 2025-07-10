using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Messaging.Integration;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC060CMessageProcessorTest : NCTSResponseMessageProcessorAbstractTest<CC060CMessageProcessor, ICC060CDataProvider>
{
	public void TestService_C01()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		CombineAssertions(() =>
		{
			var services = nctsHeader.Services.Find(x => x.ES_ServiceCode == NLConstants.ServiceTypes.ControlByCustoms);
			AssertNotNull(services);
			AssertEquals("1 CTL service should be added", 1, services.Count());
			AssertEquals("Booked Date should be 2022-04-01T12:53:26", new ZDateTime(2022, 04, 01, 12, 53, 26), services.First().ES_Booked);
			AssertEquals("Service note", "Intention to control. Type of Controls: 1. 10 Documentary controls DocumentControle mbt facturen; 2. 40 Physical controls Fysieke controles; 3. 45 Sampling Sampling;", services.First().ES_ServiceNote);
			AssertEquals("Service reference should be equal to MRN", "22NL000000000012J1", services.First().ES_References);
		});
	}

	public void TestService_C02()
	{
		var dataProviderMock = SetupDataProviderMock(NCTS5NotificationTypes.Codes.AdditionalDocumentsRequest);
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, dataProviderMock, SetupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		CombineAssertions(() =>
		{
			AssertEquals("CusInBondMoveHeader - Customs Status should be set to 'C02'", NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest, movementHeader.BM_CustomsStatus);
			var services = nctsHeader.Services.Find(x => x.ES_ServiceCode == NLConstants.ServiceTypes.ControlByCustoms);
			AssertNotNull(services);
			AssertEquals("1 CTL service should be added", 1, services.Count());
			AssertEquals("Booked Date should be 2022-04-01T12:53:26", new ZDateTime(2022, 04, 01, 12, 53, 26), services.First().ES_Booked);
			AssertEquals("Service note", "Decision to control. Type of Controls: 1. 10 Documentary controls DocumentControle mbt facturen; 2. 40 Physical controls Fysieke controles; 3. 45 Sampling Sampling;", services.First().ES_ServiceNote);
			AssertEquals("Service reference should be equal to MRN", "22NL000000000012J1", services.First().ES_References);
		});
	}

	public void TestService_C03()
	{
		var dataProviderMock = SetupDataProviderMock(NCTS5NotificationTypes.Codes.IntentionToControl);
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, dataProviderMock, SetupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		CombineAssertions(() =>
		{
			AssertEquals("CusInBondMoveHeader - Customs Status should be set to 'C03'", NCTS5DepartureCustomsStatusList.Codes.IntentionToControl, movementHeader.BM_CustomsStatus);
			var services = nctsHeader.Services.Find(x => x.ES_ServiceCode == NLConstants.ServiceTypes.ControlByCustoms);
			AssertNotNull(services);
			AssertEquals("1 CTL service should be added", 1, services.Count());
			AssertEquals("Booked Date should be 2022-04-01T12:53:26", new ZDateTime(2022, 04, 01, 12, 53, 26), services.First().ES_Booked);
			AssertEquals("Service note", "Additional document request. Type of Controls: 1. 10 Documentary controls DocumentControle mbt facturen; 2. 40 Physical controls Fysieke controles; 3. 45 Sampling Sampling;", services.First().ES_ServiceNote);
			AssertEquals("Service reference should be equal to MRN", "22NL000000000012J1", services.First().ES_References);
		});
	}

	public void TestDiscardedMessage()
	{
		incomingMessage = SetupAndProcessMessage(NCTS5DepartureCustomsStatusList.Codes.RequestForAdvice, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		CombineAssertions(() =>
		{
			AssertEquals("EDIMessage - Status should be set to 'DCD - Discarded'", EDIMessageStatusList.Codes.Discarded, incomingMessage.EM_Status);
			AssertEquals("CusInBondMoveHeader - Customs Status should not be changed", NCTS5DepartureCustomsStatusList.Codes.RequestForAdvice, movementHeader.BM_CustomsStatus);
			AssertEquals("CusInBondMoveHeader - Phase should not be changed", InitialPhase, movementHeader.BM_Phase);
			AssertEquals("NctsHeader - Message Status should not be changed", InitialMessageStatus, nctsHeader.EffectiveMessageStatus);

			var expectedMessage = "The message with interchange was discarded, because the Departure Status of the declaration is not ACK, PRE or MRN.";
			var actualMessage = incomingMessage.Notes.FindByDescription(NLConstants.Notes.Descriptions.ProcessingLog).FirstOrDefault()?.ST_NoteText;
			AssertEquals("Discarded message, Declaration found but with invalid Customs Status - Log reason for discarded message", expectedMessage, actualMessage);
		});
	}

	protected override ICC060CDataProvider GetMessageDataProviderMock() => SetupDataProviderMock(NCTS5NotificationTypes.Codes.DecisionToControlAndRequestedDocumentsIfNeeded);

	protected override string InitialCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;

	protected override string InitialPhase => NCTS5DeparturePhaseList.Codes.Amendment;

	protected override string InitialMessageStatus => LogicalStatusList.Codes.Sent;

	protected override Action<NctsHeader> SetupNctsHeader => (nctsHeader) => SetupHeader(nctsHeader);

	protected override string ExpectedCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.DecisionToControl;

	protected override string ExpectedPhase => NCTS5DeparturePhaseList.Codes.Declaration;

	protected override string ExpectedMessageStatus => LogicalStatusList.Codes.Accepted;

	void SetupHeader(NctsHeader nctsHeader)
	{
		nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRN123";

		var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Netherlands);
		mrnEntryNumber.CE_EntryNum = "22NL000000000012J1";
	}

	ICC060CDataProvider SetupDataProviderMock(string notificationType)
	{
		return Mock.Of<ICC060CDataProvider>(m =>
			m.MRN == "22NL000000000012J1" &&
			m.NotificationType == notificationType &&
			m.LRN == "LRN123" &&
			m.ControlNotificationDateAndTime == new DateTime(2022, 04, 01, 12, 53, 26) &&
			m.TypeOfControls == new List<INCTSTypeOfControlsXmlProvider>()
			{
				Mock.Of<INCTSTypeOfControlsXmlProvider>(t =>
				t.SequenceNumeric == 1 &&
				t.Type == "10" &&
				t.Text == "DocumentControle mbt facturen"),
				Mock.Of<INCTSTypeOfControlsXmlProvider>(t =>
				t.SequenceNumeric == 2 &&
				t.Type == "40" &&
				t.Text == "Fysieke controles"),
				Mock.Of<INCTSTypeOfControlsXmlProvider>(t =>
				t.SequenceNumeric == 3 &&
				t.Type == "45" &&
				t.Text == "Sampling"),
			} &&
			m.RequestedDocument == new List<INCTSRequestedDocumentXmlProvider>()
			{
				Mock.Of<INCTSRequestedDocumentXmlProvider>(d =>
				d.SequenceNumeric == 1 &&
				d.DocumentType == "N380" &&
				d.Description == "FK852364"),
				Mock.Of<INCTSRequestedDocumentXmlProvider>(d =>
				d.SequenceNumeric == 2 &&
				d.DocumentType == "N853" &&
				d.Description == "7418364"),
				Mock.Of<INCTSRequestedDocumentXmlProvider>(d =>
				d.SequenceNumeric == 3 &&
				d.DocumentType == "U114" &&
				d.Description == "ORIGIN 74987"),
			}
		);
	}
}
