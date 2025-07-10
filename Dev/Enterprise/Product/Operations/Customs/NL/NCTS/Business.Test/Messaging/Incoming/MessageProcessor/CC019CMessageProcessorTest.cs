using System;
using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Messaging.Integration;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC019CMessageProcessorTest : NCTSResponseMessageProcessorAbstractTest<CC019CMessageProcessor, ICC019CDataProvider>
{
	public void TestDiscardedMessage()
	{
		incomingMessage = SetupAndProcessMessage(NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		CombineAssertions(() =>
		{
			AssertEquals("EDIMessage - Status should be 'DCD - Discareded'", EDIMessageStatusList.Codes.Discarded, incomingMessage.EM_Status);
			AssertEquals("CusInBondMoveHeader - Customs Status should not be changed", NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed, movementHeader.BM_CustomsStatus);
			AssertEquals("CusInBondMoveHeader - Phase should not be changed", InitialPhase, movementHeader.BM_Phase);
			AssertEquals("NctsHeader - MessageStatus should not be changed", InitialMessageStatus, nctsHeader.EffectiveMessageStatus);

			var expectedMessage = "The message with interchange was discarded, because the Status at Customs of the declaration is not REL.";
			var actualMessage = incomingMessage.Notes.FindByDescription(NLConstants.Notes.Descriptions.ProcessingLog).FirstOrDefault()?.ST_NoteText;
			AssertEquals("Log reason for discarded message", expectedMessage, actualMessage);
		});
	}

	protected override ICC019CDataProvider GetMessageDataProviderMock()
	{
		return Mock.Of<ICC019CDataProvider>(provider =>
			provider.MRN == "TestMRN" &&
			provider.DiscrepanciesNotificationDate == new DateTime(2024, 8, 1) &&
			provider.DiscrepanciesNotificationText == "TestDiscrepanciesNotificationText" &&
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

	protected override string InitialCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.Acknowledged;

	protected override string InitialPhase => string.Empty;

	protected override Action<NctsHeader> SetupNctsHeader => nctsHeader =>
	{
		var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Netherlands);
		mrnEntryNumber.CE_EntryNum = "TestMRN";
	};

	protected override string ExpectedCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.DiscrepanciesAtDestination;

	protected override string ExpectedPhase => NCTS5DeparturePhaseList.Codes.Declaration;

	protected override string ExpectedMessageStatus => LogicalStatusList.Codes.Accepted;
}
