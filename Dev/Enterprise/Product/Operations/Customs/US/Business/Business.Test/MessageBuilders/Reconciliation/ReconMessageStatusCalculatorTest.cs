using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class ReconMessageStatusCalculatorTest : TestCaseWithFactory
	{
		public void TestCalculateStatus()
		{
			var calculator = new ReconMessageStatusCalculator();
			var outgoing = Factory.New<MQEDIMessage>();
			outgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var incoming = Factory.New<MQEDIMessage>();
			incoming.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			outgoing.EM_MessageSubType = EM_MessageSubTypeList.Codes.ReconDelete;
			AssertEquals(ReconMessageStatusList.Codes.AwaitingReconDelete, calculator.Calculate(outgoing, ABIResponseStatus.Undefined, false));

			outgoing.EM_MessageSubType = EM_MessageSubTypeList.Codes.ReconOriginal;
			AssertEquals(ReconMessageStatusList.Codes.AwaitingReconOriginal, calculator.Calculate(outgoing, ABIResponseStatus.Undefined, false));

			outgoing.EM_MessageSubType = EM_MessageSubTypeList.Codes.ReconReplace;
			AssertEquals(ReconMessageStatusList.Codes.AwaitingReconReplace, calculator.Calculate(outgoing, ABIResponseStatus.Undefined, false));

			incoming.EM_MessageSubType = EM_MessageSubTypeList.Codes.ReconDelete;
			AssertEquals(ReconMessageStatusList.Codes.ClearReconDelete, calculator.Calculate(incoming, ABIResponseStatus.Cleared, false));
			AssertEquals(ReconMessageStatusList.Codes.ErrorReconDelete, calculator.Calculate(incoming, ABIResponseStatus.Rejected, false));

			incoming.EM_MessageSubType = EM_MessageSubTypeList.Codes.ReconOriginal;
			AssertEquals(ReconMessageStatusList.Codes.ClearReconOriginal, calculator.Calculate(incoming, ABIResponseStatus.Cleared, false));
			AssertEquals(ReconMessageStatusList.Codes.ReconOriginalAcceptedWarnings, calculator.Calculate(incoming, ABIResponseStatus.Cleared, true));
			AssertEquals(ReconMessageStatusList.Codes.ErrorReconOriginal, calculator.Calculate(incoming, ABIResponseStatus.Rejected, false));

			incoming.EM_MessageSubType = EM_MessageSubTypeList.Codes.ReconReplace;
			AssertEquals(ReconMessageStatusList.Codes.ClearReconReplace, calculator.Calculate(incoming, ABIResponseStatus.Cleared, false));
			AssertEquals(ReconMessageStatusList.Codes.ReconReplaceAcceptedWarnings, calculator.Calculate(incoming, ABIResponseStatus.Cleared, true));
			AssertEquals(ReconMessageStatusList.Codes.ErrorReconReplace, calculator.Calculate(incoming, ABIResponseStatus.Rejected, true));
		}
	}
}
