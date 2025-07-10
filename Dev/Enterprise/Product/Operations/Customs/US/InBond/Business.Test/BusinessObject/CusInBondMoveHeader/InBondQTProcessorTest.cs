using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageProcessors;
using Enterprise.Customs.US.Business.MessageProcessors.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class InBondQTProcessorTest : ABIProcessorTest<InBondProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		public void TestMessageStatusChangeEventsIsCancelled()
		{
			StmALog departureClearedLog = moveHeader.Logs.AddNew(Events.MessageStatusChange, ImportMessageStatusList.Codes.ClearDepartureOriginal);
			Factory.Save();
			Logs inBondLogs = moveHeader.Logs;
			LogsForNominatedEvent messageStatusChangeEvents = new LogsForNominatedEvent(inBondLogs, Events.MessageStatusChange);
			AssertEquals("Pre-condition - CDO Log", 1, messageStatusChangeEvents.Count);
			AssertEquals("Pre-condition - Original departure clearance", false, departureClearedLog.SL_IsCancelled);
			outgoingMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureDelete;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureDelete;
			AddMessageBlocksToProcessor(
				"10D61568741110   OTT13901     0001500069-9999999JC N                            ",
				"9502209 INBOND DELETED                                                          ");
			processor.Process();
			Factory.Save();
			inBondLogs = moveHeader.Logs;
			LogsForNominatedEvent messageCancelHeaderEvents = new LogsForNominatedEvent(inBondLogs, Events.MessageCancelHeader);
			AssertEquals("MessageStatusChangeEvents should now return zero as event should have been cancelled", 0, messageStatusChangeEvents.Count);
			AssertEquals("Original departure clearance should now be cancelled by Delete clear message", true, departureClearedLog.SL_IsCancelled);
			AssertEquals("MessageCancelHeaderEvents should return 1 for cancelled event", 1, messageCancelHeaderEvents.Count);
		}

		protected override void EndToEndCore()
		{
			Assert(true);
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest => 0;

		CusInBondHeader inBondHeader;
		CusInBondMoveHeader moveHeader;
		InBondProcessor processor;
		MQEDIMessage message;
		MQEDIMessage outgoingMessage;
		protected override void SetUp()
		{
			base.SetUp();
			inBondHeader = Factory.New<CusInBondHeader>();
			moveHeader = inBondHeader.MovementHeaders.AddNew();
			outgoingMessage = Factory.New<MQEDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = moveHeader;
			outgoingMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureOriginal;
			outgoingMessage.EM_MessageText = "A " + MQEDIMessage.MessageNumberPlaceHolder + " B";
			Factory.Save();
			message = Factory.New<MQEDIMessage>();
			message.EM_MessageNum = outgoingMessage.EM_MessageNum;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse;
			processor = new InBondProcessor();
			processor.Message = message;
		}

		void AddMessageBlocksToProcessor(params string[] messageBlocks)
		{
			BlockControlGenerator block = OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, messageBlocks);
			foreach (MessageBlock messageBlock in block.MessageBlocks)
			{
				processor.AddMessageBlock(messageBlock);
			}
		}
	}
}
