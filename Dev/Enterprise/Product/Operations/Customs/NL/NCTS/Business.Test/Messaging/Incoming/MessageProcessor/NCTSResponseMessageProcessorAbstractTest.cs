using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.NL.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

abstract class NCTSResponseMessageProcessorAbstractTest<TMessageProcessor, TDataProvider> : TestCaseWithFactory
	where TMessageProcessor : NCTSResponseMessageProcessor<TDataProvider>
	where TDataProvider : INCTSIncomingDataProvider
{
	public void TestCustomsStatus() => CombineAssertions(() =>
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var linkedObject = incomingMessage.EM_LinkedObject;
		if (linkedObject is NctsHeader objectHeader)
		{
			movementHeader = objectHeader.ArrivalMovementHeader;
		}
		else if (linkedObject is NctsDepartureMovementHeader departureMovementHeader)
		{
			movementHeader = departureMovementHeader;
		}

		AssertEquals("CusInBondMoveHeader Customs Status should be " + ExpectedCustomsStatus, ExpectedCustomsStatus, movementHeader.BM_CustomsStatus);
		if (!SetNewCustomsStatusExpected)
		{
			AssertEquals("CusInBondMoveHeader Customs Status should be equal to initial value " + InitialCustomsStatus, InitialCustomsStatus, movementHeader.BM_CustomsStatus);
		}
	});

	public void TestPhase() => CombineAssertions(() =>
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var linkedObject = incomingMessage.EM_LinkedObject;
		if (linkedObject is NctsHeader objectHeader)
		{
			movementHeader = objectHeader.ArrivalMovementHeader;
		}
		else if (linkedObject is NctsDepartureMovementHeader departureMovementHeader)
		{
			movementHeader = departureMovementHeader;
		}

		AssertEquals("CusInBondMoveHeader Phase should be " + ExpectedPhase, ExpectedPhase, movementHeader.BM_Phase);
		if (!SetNewPhaseExpected)
		{
			AssertEquals("CusInBondMoveHeader Phase should be equal to initial value " + InitialPhase, InitialPhase, movementHeader.BM_Phase);
		}
	});

	public void TestMessageStatus() => CombineAssertions(() =>
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var linkedObject = incomingMessage.EM_LinkedObject;
		if (linkedObject is NctsHeader objectHeader)
		{
			movementHeader = objectHeader.ArrivalMovementHeader;
		}
		else if (linkedObject is NctsDepartureMovementHeader departureMovementHeader)
		{
			movementHeader = departureMovementHeader;
		}

		AssertEquals("Message Status Should be " + ExpectedMessageStatus, ExpectedMessageStatus, movementHeader.Header.EffectiveMessageStatus);
		if (!SetNewMessageStatusExpected)
		{
			AssertEquals("NctsHeader EffectiveMessageStatus should be equal to initial value " + InitialMessageStatus, InitialMessageStatus, movementHeader.Header.EffectiveMessageStatus);
		}
	});

	public void TestActivateForceRegenerateLocalReferenceNumber() => CombineAssertions(() =>
	{
		if (MovementType == NctsMovementType.Codes.Departure)
		{
			incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

			var linkedObject = incomingMessage.EM_LinkedObject;
			if (linkedObject is NctsHeader objectHeader)
			{
				movementHeader = objectHeader.ArrivalMovementHeader;
			}
			else if (linkedObject is NctsDepartureMovementHeader departureMovementHeader)
			{
				movementHeader = departureMovementHeader;
			}

			if (string.IsNullOrEmpty(InitialCustomsStatus) && incomingMessage.EM_LinkedObject is NctsDepartureMovementHeader)
			{
				AssertEquals("ActivateForceRegenerateLocalReferenceNumber should be activated for departure and empty initial customs status.", true, ((NctsDepartureMovementHeader)movementHeader).ActivateForceRegenerateLocalReferenceNumber);
			}
			else if (incomingMessage.EM_LinkedObject is NctsDepartureMovementHeader)
			{
				AssertEquals("ActivateForceRegenerateLocalReferenceNumber should not be activated for departure when initial customs status was filled in.", false, ((NctsDepartureMovementHeader)movementHeader).ActivateForceRegenerateLocalReferenceNumber);
			}
		}
		else
		{
			Assert(true);
		}
	});

	public void TestEdiMessageStatus()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		AssertEquals("EDIMessage Status should be 'PRS'", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
	}

	public void TestCorrectBranchAndCompany()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);
		var linkedObject = incomingMessage.EM_LinkedObject;

		if (linkedObject is NctsHeader objectHeader)
		{
			AssertEquals("Branch and Company have been corrected", objectHeader.RegistryBranchPK, incomingMessage.EM_GB);
		}
		else if (linkedObject is NctsDepartureMovementHeader departureMovementHeader)
		{
			AssertEquals("Branch and Company have been corrected", departureMovementHeader.RegistryBranchPK, incomingMessage.EM_GB);
		}
	}

	public void TestFailedMessage()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(MovementType);
		var movementHeader = MovementType == NctsMovementType.Codes.Departure ? nctsHeader.MovementHeader : (NctsCommonMovementHeader)nctsHeader.ArrivalMovementHeader;
		movementHeader.BM_CustomsStatus = InitialCustomsStatus;
		movementHeader.BM_Phase = InitialPhase;
		nctsHeader.EffectiveMessageStatus = InitialMessageStatus;
		SetupNctsHeader?.Invoke(nctsHeader);

		var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Netherlands);
		mrnEntryNumber.CE_EntryNum = "INVALIDMRN";
		movementHeader.BM_PaperlessInbondNum = "INVALIDLRN";
		nctsHeader.LocalReferenceNumber = "INVALIDLRN";

		var dataProviderMock = GetMessageDataProviderMock();
		if (dataProviderMock != null)
		{
			mock.Protected()
				.Setup<TDataProvider>("GetMessageDataProvider", ItExpr.IsAny<EDIMessage>())
				.Returns(dataProviderMock)
				.Verifiable();
		}

		Factory.Save();

		var incomingMessage = Factory.New<NLEDIMessage>();
		incomingMessage.EM_MessageNum = "11";
		incomingMessage.EM_MessageType = "NCT";
		incomingMessage.EM_ReceiveTransmit = "RCV";
		var processor = messageProcessor;
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("EDIMessage status should be 'FAL'", EDIMessageStatusList.Codes.Failed, incomingMessage.EM_Status);
			AssertNull("EDI Message Linked object", incomingMessage.EM_LinkedObject);
		});
	}

	protected abstract TDataProvider GetMessageDataProviderMock();

	protected EDIMessage SetupAndProcessMessage(string initialCustomsStatus, string initialPhase, string initialMessageStatus, TDataProvider setupMessageDataProviderMock, Action<NctsHeader> setupNctsHeader)
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(MovementType);
		var movementHeader = MovementType == NctsMovementType.Codes.Departure ? nctsHeader.MovementHeader : (NctsCommonMovementHeader)nctsHeader.ArrivalMovementHeader;
		movementHeader.BM_CustomsStatus = initialCustomsStatus;
		movementHeader.BM_Phase = initialPhase;
		nctsHeader.EffectiveMessageStatus = initialMessageStatus;
		setupNctsHeader?.Invoke(nctsHeader);

		if (setupMessageDataProviderMock != null)
		{
			mock.Protected()
				.Setup<TDataProvider>("GetMessageDataProvider", ItExpr.IsAny<EDIMessage>())
				.Returns(setupMessageDataProviderMock)
				.Verifiable();
		}

		Factory.Save();

		var incomingMessage = Factory.New<NLEDIMessage>();
		incomingMessage.EM_MessageNum = "11";
		incomingMessage.EM_MessageType = "NCT";
		incomingMessage.EM_ReceiveTransmit = "RCV";
		var company = Factory.New<GlbCompany>();
		company.GC_Code = "NEW";
		company.GC_RN_NKCountryCode = "AU";
		var newBranch = company.Branches.AddNew();
		newBranch.GB_Code = "NEW";
		incomingMessage.EM_GB = newBranch.PK;

		var processor = messageProcessor;
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);

		return incomingMessage;
	}

	protected virtual string InitialCustomsStatus { get; } = NCTS5DepartureCustomsStatusList.Codes.AcceptedBySystem;

	protected virtual string InitialPhase { get; } = NctsMovementHeaderTransactionStatusList.Codes.Declaration;

	protected virtual string InitialMessageStatus { get; } = LogicalStatusList.Codes.Accepted;

	protected abstract Action<NctsHeader> SetupNctsHeader { get; }

	protected virtual string ExpectedCustomsStatus { get; } = NCTS5DepartureCustomsStatusList.Codes.AcceptedBySystem;

	protected virtual string ExpectedPhase { get; } = NctsMovementHeaderTransactionStatusList.Codes.Declaration;

	protected virtual string ExpectedMessageStatus { get; } = LogicalStatusList.Codes.Accepted;

	protected NCTSResponseMessageProcessor<TDataProvider> messageProcessor => mock.Object;

	protected string ExpectedMessageFriendlyName => "NCTS Response";

	protected virtual string MovementType => NctsMovementType.Codes.Departure;

	protected virtual bool SetNewCustomsStatusExpected => true;

	protected virtual bool SetNewMessageStatusExpected => true;

	protected virtual bool SetNewPhaseExpected => true;

	protected LoggingInformation logger = new LoggingInformation();

	protected EDIMessage incomingMessage;

	Mock<TMessageProcessor> mock => _mock ??= new Mock<TMessageProcessor>(logger) { CallBase = true };
	Mock<TMessageProcessor> _mock;
	NctsCommonMovementHeader movementHeader;
}
