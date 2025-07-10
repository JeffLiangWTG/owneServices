using System;
using System.Collections.Generic;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC022CMessageProcessorTest : NCTSResponseMessageProcessorAbstractTest<CC022CMessageProcessor, ICC022CDataProvider>
{
	protected override ICC022CDataProvider GetMessageDataProviderMock()
	{
		return Mock.Of<ICC022CDataProvider>(m =>
			m.MRN == "TestMRN" &&
			m.AmendmentNotificationDateAndTime == new DateTime(1994, 2, 1) &&
			m.FunctionalErrors == new List<INCTSFunctionalError>()
			{
						Mock.Of<INCTSFunctionalError>(e =>
						e.SequenceNumeric == 1 &&
						e.ErrorPointer == "EP1" &&
						e.ErrorCode == "12" &&
						e.ErrorReason == "bad type one" &&
						e.OriginalAttributeValue == "11"),
			}
		);
	}

	protected override string InitialCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.Acknowledged;

	protected override Action<NctsHeader> SetupNctsHeader => (nctsHeader) =>
	{
		var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Netherlands);
		mrnEntryNumber.CE_EntryNum = "TestMRN";
		nctsHeader.MovementHeader.BM_SubApplicationCode = "D";
	};

	protected override string ExpectedCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;

	protected override string ExpectedPhase => NctsMovementHeaderTransactionStatusList.Codes.Declaration;

	protected override string ExpectedMessageStatus => LogicalStatusList.Codes.Accepted;
}
