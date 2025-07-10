using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.MessageBuilders.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Common.ZA;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.MessageManagers.Testing
{
	[TestedType(typeof(EDIFACTStatusCalculator))]
	sealed class EDIFACTStatusCalculatorTest : EDIFACTMessageStatusCalculatorTestCase
	{
		public override void TestIsClear()
		{
			AssertEquals("IsClear", false, calculator.IsClear(EntryStatusList.Codes.Clear));
			if (ErrorReporter.LastMessageReported == "Enterprise.Customs.ZA.Business.MessageManagers.EDIFACTStatusCalculator.IsClear(ZString currentJobStatus) is not supported yet.")
			{
				ErrorReporter.Clear();
			}
		}

		public override void TestIsLodged()
		{
			AssertEquals("IsLodged", false, calculator.IsLodged(EntryStatusList.Codes.Clear));
			if (ErrorReporter.LastMessageReported == "Enterprise.Customs.ZA.Business.MessageManagers.EDIFACTStatusCalculator.IsLodged(ZString currentJobStatus) is not supported yet.")
			{
				ErrorReporter.Clear();
			}
		}

		public override void TestIsWithdrawn()
		{
			AssertEquals("IsWithdrawn", false, calculator.IsWithdrawn(EntryStatusList.Codes.Cancelled));
			if (ErrorReporter.LastMessageReported == "Enterprise.Customs.ZA.Business.MessageManagers.EDIFACTStatusCalculator.IsWithdrawn(ZString currentJobStatus) is not supported yet.")
			{
				ErrorReporter.Clear();
			}
		}

		public void TestIsAwaitingReply()
		{
			AssertEquals("IsAwaitingReply", true, calculator.IsAwaitingReply(ZAMessageStatusList.Codes.AwaitingResponse));
			AssertEquals("IsAwaitingReply", false, calculator.IsAwaitingReply(ZAMessageStatusList.Codes.Acknowledged));
		}

		public void TestGetMessageAwaitingStatus()
		{
			AssertEquals("GetMessageAwaitingStatus", ZAMessageStatusList.Codes.AwaitingResponse, calculator.GetMessageAwaitingStatus(MessageSubTypeCodes.Codes.Original));
		}

		public void TestGetMessageAcknowledgedStatus()
		{
			AssertEquals("GetMessageAcknowledgedStatus", ZAMessageStatusList.Codes.Acknowledged, calculator.GetMessageAcknowledgedStatus(MessageSubTypeCodes.Codes.Original));
		}

		public void TestGetMessageClearedStatus()
		{
			AssertEquals("GetMessageClearedStatus", ZAMessageStatusList.Codes.Acknowledged, calculator.GetMessageClearedStatus(MessageSubTypeCodes.Codes.Original));
		}

		public void TestGetMessageRejectedStatus()
		{
			AssertEquals("GetMessageRejectedStatus", ZAMessageStatusList.Codes.Error, calculator.GetMessageRejectedStatus(MessageSubTypeCodes.Codes.Original));
		}

		public override void TestCalculatedJobStatus()
		{
			// TODO: ToBeImplemented, Status Transition logic to be implemented later #Victor 20160422
			AssertEquals(ZString.Empty, calculator.CalculatedJobStatus(null));
		}

		public override void TestMessageTypeDescription()
		{
			AssertEquals("EXAMPL", calculator.MessageTypeDescription);
		}

		protected override EDIFACTMessageStatusCalculator GetCalculator()
		{
			return new EDIFACTStatusCalculator("EXAMPL");
		}
	}
}
