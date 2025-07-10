using CargoWise.Types;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.NL.NCTS.Business.Testing.Messaging;

public class AutoSendNctsP5MessageProcessorTest : EU.NCTS.Business.Testing.AutoSendNctsP5MessageProcessorAbstractTest
{
	protected override void PrepareNctsHeaderForDepartureTestCore(EU.NCTS.Business.NctsHeader nctsHeader)
	{
		base.PrepareNctsHeaderForDepartureTestCore(nctsHeader);

		var departureMovementHeader = nctsHeader.MovementHeader;
		departureMovementHeader.BM_InBondEntryType = "T";
		departureMovementHeader.BM_AdditionalDeclarationType = "A";

		nctsHeader.LocalReferenceNumber = "LRN123";
	}

	protected override void AssertEntryAndMessageResultForDepartureEndToEndTest(EU.NCTS.Business.NctsHeader nctsHeader)
	{
		CombineAssertions(() =>
		{
			AssertEquals("SNT", nctsHeader.EffectiveMessageStatus);

			AssertEquals(1, nctsHeader.MovementHeader.Messages.Count);

			var message = nctsHeader.MovementHeader.Messages[0];
			AssertEquals(nameof(message.EM_ApplicationCode), "NLC", message.EM_ApplicationCode);
			AssertEquals(nameof(message.EM_MessageSubType), "015", message.EM_MessageSubType);
			AssertEquals(nameof(message.EM_ApplicationReference), ZString.Empty, message.EM_ApplicationReference);
			AssertEquals(nameof(message.EM_ReceiveTransmit), ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(nameof(message.EM_Status), EDIMessageStatusList.Codes.Queued, message.EM_Status);
		});
	}

	protected override void PrepareNctsHeaderForArrivalTestCore(EU.NCTS.Business.NctsHeader nctsHeader)
	{
		base.PrepareNctsHeaderForArrivalTestCore(nctsHeader);
		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "MRN123";
	}

	protected override void AssertEntryAndMessageResultForArrivalEndToEndTest(EU.NCTS.Business.NctsHeader nctsHeader)
	{
		CombineAssertions(() =>
		{
			AssertEquals("SNT", nctsHeader.EffectiveMessageStatus);

			AssertEquals(1, nctsHeader.Messages.Count);

			var message = nctsHeader.Messages[0];
			AssertEquals(nameof(message.EM_ApplicationCode), "NLC", message.EM_ApplicationCode);
			AssertEquals(nameof(message.EM_MessageSubType), "007", message.EM_MessageSubType);
			AssertEquals(nameof(message.EM_ApplicationReference), ZString.Empty, message.EM_ApplicationReference);
			AssertEquals(nameof(message.EM_ReceiveTransmit), ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(nameof(message.EM_Status), EDIMessageStatusList.Codes.Queued, message.EM_Status);
		});
	}
}
