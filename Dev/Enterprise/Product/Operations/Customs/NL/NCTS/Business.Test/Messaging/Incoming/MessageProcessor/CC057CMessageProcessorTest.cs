using System;
using System.Collections.Generic;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Integration;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing
{
	sealed class CC057CMessageProcessorTest : NCTSResponseMessageProcessorAbstractTest<CC057CMessageProcessor, ICC057CDataProvider>
	{
		public void Test007_Blank_ACK()
		{
			AssertCC056CMessageProcessor(string.Empty, NCTS5ArrivalPhaseList.Codes.Arrival, LogicalStatusList.Codes.Sent);
		}

		public void Test044_UAP_SNT()
		{
			AssertCC056CMessageProcessor(NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, NCTS5ArrivalPhaseList.Codes.UnloadingRemarks, LogicalStatusList.Codes.Sent);
		}

		public void Test044_UAP_ACK()
		{
			AssertCC056CMessageProcessor(NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, NCTS5ArrivalPhaseList.Codes.UnloadingRemarks, LogicalStatusList.Codes.Acknowledged);
		}

		public void Test044_ULR_SNT()
		{
			AssertCC056CMessageProcessor(NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks, NCTS5ArrivalPhaseList.Codes.UnloadingRemarks, LogicalStatusList.Codes.Sent);
		}

		public void Test044_ULR_ACK()
		{
			AssertCC056CMessageProcessor(NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks, NCTS5ArrivalPhaseList.Codes.UnloadingRemarks, LogicalStatusList.Codes.Acknowledged);
		}

		protected override ICC057CDataProvider GetMessageDataProviderMock() => GetMessageDataProviderMock(InitialPhase);

		protected override string MovementType => NctsMovementType.Codes.Arrival;

		protected override bool SetNewCustomsStatusExpected => false;

		protected override bool SetNewPhaseExpected => false;

		protected override string InitialCustomsStatus => string.Empty;

		protected override string InitialPhase => NCTS5ArrivalPhaseList.Codes.Arrival;

		protected override string InitialMessageStatus => LogicalStatusList.Codes.Sent;

		protected override Action<NctsHeader> SetupNctsHeader => (nctsHeader) => SetupMrnHeader(nctsHeader);

		protected override string ExpectedCustomsStatus => string.Empty;

		protected override string ExpectedPhase => NCTS5ArrivalPhaseList.Codes.Arrival;

		protected override string ExpectedMessageStatus => LogicalStatusList.Codes.Invalid;

		void AssertCC056CMessageProcessor(string initialCustomsStatus, string initialPhase, string initialMessageStatus)
		{
			var dataProviderMock = GetMessageDataProviderMock(initialPhase);
			incomingMessage = SetupAndProcessMessage(initialCustomsStatus, initialPhase, initialMessageStatus, dataProviderMock, SetupNctsHeader);

			var nctsHeader = incomingMessage.EM_LinkedObject as NctsHeader;
			var movementHeader = nctsHeader.ArrivalMovementHeader;

			CombineAssertions(() =>
			{
				AssertEquals("EDIMessage - Status is set to 'PRS - Processed'", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
				AssertEquals("CusInBondMoveHeader - Customs Status did not change", initialCustomsStatus, movementHeader.BM_CustomsStatus);
				AssertEquals("CusInBondMoveHeader - Phase Status did not change", initialPhase, movementHeader.BM_Phase);
				AssertEquals("NctsHeader - MessageStatus changed to 'INV - Invalid'", LogicalStatusList.Codes.Invalid, nctsHeader.EffectiveMessageStatus);
			});
		}

		ICC057CDataProvider GetMessageDataProviderMock(string businessRejectionType)
		{
			return Mock.Of<ICC057CDataProvider>(p =>
				p.MRN == "22NL000000000012J3" &&
				p.BusinessRejectionType == businessRejectionType &&
				p.RejectionDateAndTime == new DateTime(2024, 03, 28, 12, 25, 45) &&
				p.RejectionCode == "4" &&
				p.RejectionReason == "Invalid" &&
				p.CustomsOfficeOfDestinationActualReferenceNumber == "NL000432" &&
				p.TraderAtDestinationIdentificationNumber == "ID" &&
				p.FunctionalErrors == new List<INCTSFunctionalError>()
				{
					Mock.Of<INCTSFunctionalError>(e =>
					e.ErrorPointer == "EP1" &&
					e.ErrorCode == "12" &&
					e.ErrorReason == "bad type one" &&
					e.OriginalAttributeValue == "value 11"),
					Mock.Of<INCTSFunctionalError>(e =>
					e.ErrorPointer == "EP2" &&
					e.ErrorCode == "15" &&
					e.ErrorReason == "bad type two" &&
					e.OriginalAttributeValue == "value 12"),
				}
			);
		}

		void SetupMrnHeader(NctsHeader nctsHeader)
		{
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Netherlands);
			mrnEntryNumber.CE_EntryNum = "22NL000000000012J3";
		}
	}
}
