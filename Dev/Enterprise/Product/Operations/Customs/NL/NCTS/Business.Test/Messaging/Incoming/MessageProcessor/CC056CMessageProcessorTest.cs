using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.NL.Business;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC056CMessageProcessorTest : NCTSResponseMessageProcessorAbstractTest<CC056CMessageProcessor, ICC056CDataProvider>
{
	public void Test015PRE()
	{
		AssertCC056CMessageProcessor("PRE", "015", "ACC", expectedTransactionStatus: "DEL", expectedCustomsStatus: "CAN");
	}

	public void Test015PRE_Discard()
	{
		incomingMessage = SetupAndProcessMessage("PRE", "015", "ACC", GetMessageDataProviderMock("3"), SetupNctsHeader);
		AssertEquals(EDIMessageStatusList.Codes.Discarded, incomingMessage.EM_Status);
	}

	public void Test014_ACK_ACK()
	{
		AssertCC056CMessageProcessor(NCTS5DepartureCustomsStatusList.Codes.Acknowledged, NCTS5DeparturePhaseList.Codes.Cancellation, LogicalStatusList.Codes.Acknowledged);
	}

	public void Test014_PRE_ACK()
	{
		AssertCC056CMessageProcessor(NCTS5DepartureCustomsStatusList.Codes.PreLodged, NCTS5DeparturePhaseList.Codes.Cancellation, LogicalStatusList.Codes.Acknowledged);
	}

	public void Test014_PRE_SNT()
	{
		AssertCC056CMessageProcessor(NCTS5DepartureCustomsStatusList.Codes.PreLodged, NCTS5DeparturePhaseList.Codes.Cancellation, LogicalStatusList.Codes.Sent);
	}

	public void Test014_MRN_SNT()
	{
		AssertCC056CMessageProcessor(NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, NCTS5DeparturePhaseList.Codes.Cancellation, LogicalStatusList.Codes.Sent);
	}

	public void Test014_MRN_ACK()
	{
		AssertCC056CMessageProcessor(NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, NCTS5DeparturePhaseList.Codes.Cancellation, LogicalStatusList.Codes.Acknowledged);
	}

	public void Test014_REL_ACK()
	{
		AssertCC056CMessageProcessor(NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit, NCTS5DeparturePhaseList.Codes.Cancellation, LogicalStatusList.Codes.Acknowledged);
	}

	public void Test014_REL_SNT()
	{
		AssertCC056CMessageProcessor(NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit, NCTS5DeparturePhaseList.Codes.Cancellation, LogicalStatusList.Codes.Sent);
	}

	public void Test015_Blank_SNT()
	{
		AssertCC056CMessageProcessor(string.Empty, NCTS5DeparturePhaseList.Codes.Declaration, LogicalStatusList.Codes.Sent, PermitTransactionStatusList.Codes.Deleted);
	}

	public void Test015_Blank_ACK()
	{
		AssertCC056CMessageProcessor(string.Empty, NCTS5DeparturePhaseList.Codes.Declaration, LogicalStatusList.Codes.Acknowledged, PermitTransactionStatusList.Codes.Deleted);
	}

	public void Test013_ACK_ACK()
	{
		AssertCC056CMessageProcessor(NCTS5DepartureCustomsStatusList.Codes.Acknowledged, NCTS5DeparturePhaseList.Codes.Amendment, LogicalStatusList.Codes.Acknowledged);
	}

	public void Test013_ACK_SNT()
	{
		AssertCC056CMessageProcessor(NCTS5DepartureCustomsStatusList.Codes.Acknowledged, NCTS5DeparturePhaseList.Codes.Amendment, LogicalStatusList.Codes.Sent);
	}

	public void Test013_PRE_ACK()
	{
		AssertCC056CMessageProcessor(NCTS5DepartureCustomsStatusList.Codes.PreLodged, NCTS5DeparturePhaseList.Codes.Amendment, LogicalStatusList.Codes.Acknowledged);
	}

	public void Test013_PRE_SNT()
	{
		AssertCC056CMessageProcessor(NCTS5DepartureCustomsStatusList.Codes.PreLodged, NCTS5DeparturePhaseList.Codes.Amendment, LogicalStatusList.Codes.Sent);
	}

	public void Test013_MRN_ACK()
	{
		AssertCC056CMessageProcessor(NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, NCTS5DeparturePhaseList.Codes.Amendment, LogicalStatusList.Codes.Acknowledged);
	}

	public void Test013_MRN_SNT()
	{
		AssertCC056CMessageProcessor(NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, NCTS5DeparturePhaseList.Codes.Amendment, LogicalStatusList.Codes.Sent);
	}

	public void Test013_AMR_SNT()
	{
		AssertCC056CMessageProcessor(NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested, NCTS5DeparturePhaseList.Codes.Amendment, LogicalStatusList.Codes.Sent);
	}

	public void Test141ENQ_ACK()
	{
		AssertCC056CMessageProcessor(NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry, NCTS5DeparturePhaseList.Codes.NonArrivedMovement, LogicalStatusList.Codes.Acknowledged);
	}

	public void Test141_ENQ_SNT()
	{
		AssertCC056CMessageProcessor(NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry, NCTS5DeparturePhaseList.Codes.NonArrivedMovement, LogicalStatusList.Codes.Sent);
	}

	public void Test170_ACK_ACK()
	{
		AssertCC056CMessageProcessor(NCTS5DepartureCustomsStatusList.Codes.Acknowledged, NCTS5DeparturePhaseList.Codes.Presentation, LogicalStatusList.Codes.Acknowledged);
	}

	public void Test170_ACK_SNT()
	{
		AssertCC056CMessageProcessor(NCTS5DepartureCustomsStatusList.Codes.Acknowledged, NCTS5DeparturePhaseList.Codes.Presentation, LogicalStatusList.Codes.Sent);
	}

	public void Test170_PRE_ACK()
	{
		AssertCC056CMessageProcessor(NCTS5DepartureCustomsStatusList.Codes.PreLodged, NCTS5DeparturePhaseList.Codes.Presentation, LogicalStatusList.Codes.Sent);
	}

	public void TestDiscardedMessage()
	{
		incomingMessage = SetupAndProcessMessage("XXX", NCTS5DeparturePhaseList.Codes.Presentation, LogicalStatusList.Codes.Acknowledged, GetMessageDataProviderMock(), SetupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		CombineAssertions(() =>
		{
			AssertEquals("Unknown Customs Status: EDIMessage - Status is set to 'DCD - Discarded'", EDIMessageStatusList.Codes.Discarded, incomingMessage.EM_Status);
			AssertEquals("Unknown Customs Status: NctsHeader - Message Status should not be changed", LogicalStatusList.Codes.Acknowledged, nctsHeader.EffectiveMessageStatus);

			var expectedMessage = "The message with interchange was discarded, because the ‘Phase status’ and the ‘Departure Status’ of the declaration could not be mapped to correct value of the element businessRejectionType.";
			var actualMessage = incomingMessage.Notes.FindByDescription(NLConstants.Notes.Descriptions.ProcessingLog).FirstOrDefault()?.ST_NoteText;
			AssertEquals("Log reason for discarded message", expectedMessage, actualMessage);
		});
	}

	protected override ICC056CDataProvider GetMessageDataProviderMock()
	{
		return GetMessageDataProviderMock("4");
	}

	ICC056CDataProvider GetMessageDataProviderMock(string rejectionCode = "4")
	{
		return Mock.Of<ICC056CDataProvider>(p =>
			p.LRN == "24DE966882266928800171" &&
			p.BusinessRejectionType == "014" &&
			p.RejectionDateAndTime == new DateTime(2024, 1, 9, 12, 27, 16) &&
			p.RejectionCode == rejectionCode &&
			p.RejectionReason == "Invalid" &&
			p.FunctionalErrors == new List<INCTSFunctionalError>()
			{
						Mock.Of<INCTSFunctionalError>(e =>
						e.ErrorPointer == "/CC014C/TransitOperation/limitDate" &&
						e.ErrorCode == "13" &&
						e.ErrorReason == "C0840"),
			}
		);
	}

	protected override string MovementType => NctsMovementType.Codes.Departure;

	protected override bool SetNewCustomsStatusExpected => false;

	protected override bool SetNewPhaseExpected => false;

	protected override string InitialCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.Acknowledged;

	protected override string InitialPhase => NCTS5DeparturePhaseList.Codes.Cancellation;

	protected override string InitialMessageStatus => LogicalStatusList.Codes.Sent;

	protected override Action<NctsHeader> SetupNctsHeader => (nctsHeader) => { nctsHeader.MovementHeader.BM_PaperlessInbondNum = "24DE966882266928800171"; };

	protected override string ExpectedCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.Acknowledged;

	protected override string ExpectedPhase => NCTS5DeparturePhaseList.Codes.Cancellation;

	protected override string ExpectedMessageStatus => LogicalStatusList.Codes.Invalid;

	void AssertCC056CMessageProcessor(string initialCustomsStatus, string initialPhase, string initialMessageStatus, string expectedTransactionStatus = PermitTransactionStatusList.Codes.Pending, string expectedCustomsStatus = null)
	{
		var assertTransaction = initialPhase == NCTS5DeparturePhaseList.Codes.Amendment || initialPhase == NCTS5DeparturePhaseList.Codes.Declaration;

		Action<NctsHeader> setupNctsHeader = (nctsHeader) =>
		{
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "24DE966882266928800171";
			if (assertTransaction)
			{
				SetupGuaranteeTransaction(nctsHeader);
			}
		};

		var dataProviderMock = Mock.Of<ICC056CDataProvider>(p =>
			p.LRN == "24DE966882266928800171" &&
			p.BusinessRejectionType == initialPhase &&
			p.RejectionDateAndTime == new DateTime(2024, 1, 9, 12, 27, 16) &&
			p.RejectionCode == "4" &&
			p.RejectionReason == "Invalid" &&
			p.FunctionalErrors == new List<INCTSFunctionalError>()
			{
					Mock.Of<INCTSFunctionalError>(e =>
					e.ErrorPointer == "/CC015C/TransitOperation/limitDate" &&
					e.ErrorCode == "13" &&
					e.ErrorReason == "C0840"),
			});

		incomingMessage = SetupAndProcessMessage(initialCustomsStatus, initialPhase, initialMessageStatus, dataProviderMock, setupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		CombineAssertions(() =>
		{
			AssertEquals($"Customs Status {initialCustomsStatus} - Phase {initialPhase} - Message Status {initialMessageStatus}: CusInBondMoveHeader - Customs Status", expectedCustomsStatus ?? initialCustomsStatus, movementHeader.BM_CustomsStatus);
			AssertEquals($"Customs Status {initialCustomsStatus} - Phase {initialPhase} - Message Status {initialMessageStatus}: CusInBondMoveHeader - Phase should not be changed", initialPhase, movementHeader.BM_Phase);
			AssertEquals($"Customs Status {initialCustomsStatus} - Phase {initialPhase} - Message Status {initialMessageStatus}: NctsHeader - MessageStatus", LogicalStatusList.Codes.Invalid, nctsHeader.EffectiveMessageStatus);
			AssertEquals($"Customs Status {initialCustomsStatus} - Phase {initialPhase} - Message Status {initialMessageStatus}: EDIMessage - Status should be changed to 'PRS - Processed'", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);

			if (assertTransaction)
			{
				AssertEquals($"Transaction status should be {expectedTransactionStatus}", expectedTransactionStatus, guaranteeHeader.GetTransactions().ToArray()[1].CPL_TransactionStatus);
			}
		});
	}

	void SetupGuaranteeTransaction(NctsHeader nctsHeader)
	{
		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var movementHeader = nctsHeader.MovementHeader;
		var org1 = Factory.NewWithValidTestData<OrgHeader>();
		nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;
		guaranteeHeader = Factory.New<CusGuaranteeHeader>();
		guaranteeHeader.CPH_OH_PermitHolder = org1.PK;
		guaranteeHeader.CPH_Number = "GUA1";
		guaranteeHeader.CPH_StartDate = ZDate.Today.AddMonths(-1);
		guaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
		guaranteeHeader.CPH_SystemCreateTimeUtc = ZDate.Today;
		guaranteeHeader.CPH_Type = "TRA";
		guaranteeHeader.CPH_RN_NKCountryCode = "NL";
		guaranteeHeader.CPH_Balance = 1000m;

		var transaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
		transaction.CPL_Reference = "OPENING";
		transaction.CPL_TranValue = 1000m;
		transaction.CPL_Comment = "OPENING";
		transaction.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.OBL;
		var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
		guarantee.PW_BondNumber = "GUA1";
		guarantee.PW_BondAmount = 145;
		guarantee.CusGuarantee.AddTransaction(movementHeader.BM_PaperlessInbondNum,
											"NCTS write-off " + movementHeader.BM_PaperlessInbondNum + " [" + nctsHeader.MovementReferenceNumber + "]",
											"CreatedByMessageNumber",
											ZString.Empty,
											guarantee.PW_BondAmount * -1,
											0,
											status: PermitTransactionStatusList.Codes.Pending);
	}
	CusGuaranteeHeader guaranteeHeader;
}
