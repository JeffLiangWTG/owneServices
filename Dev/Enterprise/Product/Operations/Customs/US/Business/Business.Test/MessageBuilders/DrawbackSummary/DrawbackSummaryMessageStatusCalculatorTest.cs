using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class DrawbackSummaryMessageStatusCalculatorTest : TestCaseWithFactory
	{
		public void TestCalculateStatus()
		{
			var calculator = new DrawbackSummaryMessageStatusCalculator();
			var outgoing = Factory.New<MQEDIMessage>();
			outgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var incoming = Factory.New<MQEDIMessage>();
			incoming.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			outgoing.EM_MessageSubType = EM_MessageSubTypeList.Codes.DrawbackSummaryDelete;
			AssertEquals(DrawbackSummaryStatusList.Codes.AwaitingDrawbackSummaryDelete, calculator.Calculate(outgoing, ABIResponseStatus.Undefined));

			outgoing.EM_MessageSubType = EM_MessageSubTypeList.Codes.DrawbackSummaryOriginal;
			AssertEquals(DrawbackSummaryStatusList.Codes.AwaitingDrawbackSummaryOriginal, calculator.Calculate(outgoing, ABIResponseStatus.Undefined));

			outgoing.EM_MessageSubType = EM_MessageSubTypeList.Codes.DrawbackSummaryReplacement;
			AssertEquals(DrawbackSummaryStatusList.Codes.AwaitingDrawbackSummaryReplacement, calculator.Calculate(outgoing, ABIResponseStatus.Undefined));

			incoming.EM_MessageSubType = EM_MessageSubTypeList.Codes.DrawbackSummaryDelete;
			AssertEquals(DrawbackSummaryStatusList.Codes.ClearDrawbackSummaryDelete, calculator.Calculate(incoming, ABIResponseStatus.Cleared));
			AssertEquals(DrawbackSummaryStatusList.Codes.ErrorDrawbackSummaryDelete, calculator.Calculate(incoming, ABIResponseStatus.Rejected));

			incoming.EM_MessageSubType = EM_MessageSubTypeList.Codes.DrawbackSummaryOriginal;
			AssertEquals(DrawbackSummaryStatusList.Codes.ClearDrawbackSummaryOriginal, calculator.Calculate(incoming, ABIResponseStatus.Cleared));
			AssertEquals(DrawbackSummaryStatusList.Codes.ErrorDrawbackSummaryOriginal, calculator.Calculate(incoming, ABIResponseStatus.Rejected));
			AssertEquals(DrawbackSummaryStatusList.Codes.DrawbackSummaryOriginalAcceptedWithCensusWarnings, calculator.Calculate(incoming, ABIResponseStatus.CensusWarning));

			incoming.EM_MessageSubType = EM_MessageSubTypeList.Codes.DrawbackSummaryReplacement;
			AssertEquals(DrawbackSummaryStatusList.Codes.ClearDrawbackSummaryReplacement, calculator.Calculate(incoming, ABIResponseStatus.Cleared));
			AssertEquals(DrawbackSummaryStatusList.Codes.ErrorDrawbackSummaryReplacement, calculator.Calculate(incoming, ABIResponseStatus.Rejected));
			AssertEquals(DrawbackSummaryStatusList.Codes.DrawbackSummaryReplacementAcceptedWithCensusWarnings, calculator.Calculate(incoming, ABIResponseStatus.CensusWarning));
		}
	}
}
