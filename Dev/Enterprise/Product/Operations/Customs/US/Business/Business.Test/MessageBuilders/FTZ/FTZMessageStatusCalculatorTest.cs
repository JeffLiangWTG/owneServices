using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class FTZMessageStatusCalculatorTest : TestCaseWithFactory
	{
		public void TestCalculateStatus()
		{
			var calculator = new FTZMessageStatusCalculator();
			var outgoing = Factory.New<MQEDIMessage>();
			outgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZAdmissionDelete;
			AssertEquals(FTZMessageStatusList.Codes.AwaitingFTZAdmissionDelete, calculator.Calculate(outgoing, false, false));

			outgoing.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZAdmissionAdd;
			AssertEquals(FTZMessageStatusList.Codes.AwaitingFTZAdmissionAdd, calculator.Calculate(outgoing, false, false));

			outgoing.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZAdmissionReplace;
			AssertEquals(FTZMessageStatusList.Codes.AwaitingFTZAdmissionAmend, calculator.Calculate(outgoing, false, false));

			outgoing.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZPermitToTransfer;
			AssertEquals(FTZMessageStatusList.Codes.AwaitingPermitToTransfer, calculator.Calculate(outgoing, false, false));

			outgoing.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZPermitToTransferArrival;
			AssertEquals(FTZMessageStatusList.Codes.AwaitingPermitToTransferArrival, calculator.Calculate(outgoing, false, false));

			outgoing.EM_MessageSubType = EM_MessageSubTypeList.Codes.FZConcurrence;
			AssertEquals(FTZMessageStatusList.Codes.AwaitingConcurrence, calculator.Calculate(outgoing, false, false));

			outgoing.EM_MessageSubType = EM_MessageSubTypeList.Codes.FZUnconcurrence;
			AssertEquals(FTZMessageStatusList.Codes.AwaitingUnconcurrence, calculator.Calculate(outgoing, false, false));

			outgoing.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZCancelPermitToTransfer;
			AssertEquals(FTZMessageStatusList.Codes.AwaitingCancelPermitToTransfer, calculator.Calculate(outgoing, false, false));

			outgoing.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZSendPermitToTransferUnArrival;
			AssertEquals(FTZMessageStatusList.Codes.AwaitingPermitToTransferUnArrival, calculator.Calculate(outgoing, false, false));

			var incoming = Factory.New<MQEDIMessage>();
			incoming.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incoming.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZAdmissionDelete;
			AssertEquals(FTZMessageStatusList.Codes.ClearFTZAdmissionDelete, calculator.Calculate(incoming, false, false));
			AssertEquals(FTZMessageStatusList.Codes.ErrorFTZAdmissionDelete, calculator.Calculate(incoming, true, false));

			incoming.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZAdmissionAdd;
			AssertEquals(FTZMessageStatusList.Codes.ClearFTZAdmissionAdd, calculator.Calculate(incoming, false, false));
			AssertEquals(FTZMessageStatusList.Codes.ClearFTZAdmissionAddWithWarnings, calculator.Calculate(incoming, false, true));
			AssertEquals(FTZMessageStatusList.Codes.ErrorFTZAdmissionAdd, calculator.Calculate(incoming, true, false));

			incoming.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZAdmissionReplace;
			AssertEquals(FTZMessageStatusList.Codes.ClearFTZAdmissionAmend, calculator.Calculate(incoming, false, false));
			AssertEquals(FTZMessageStatusList.Codes.ErrorFTZAdmissionAmend, calculator.Calculate(incoming, true, false));

			incoming.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZPermitToTransfer;
			AssertEquals(FTZMessageStatusList.Codes.ClearPermitToTransfer, calculator.Calculate(incoming, false, false));
			AssertEquals(FTZMessageStatusList.Codes.ErrorPermitToTransfer, calculator.Calculate(incoming, true, false));

			incoming.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZPermitToTransferArrival;
			AssertEquals(FTZMessageStatusList.Codes.PermitToTransferArrived, calculator.Calculate(incoming, false, false));
			AssertEquals(FTZMessageStatusList.Codes.ErrorPermitToTransferArrival, calculator.Calculate(incoming, true, false));

			incoming.EM_MessageSubType = EM_MessageSubTypeList.Codes.FZConcurrence;
			AssertEquals(FTZMessageStatusList.Codes.ClearConcurrence, calculator.Calculate(incoming, false, false));
			AssertEquals(FTZMessageStatusList.Codes.ErrorConcurrence, calculator.Calculate(incoming, true, false));

			incoming.EM_MessageSubType = EM_MessageSubTypeList.Codes.FZUnconcurrence;
			AssertEquals(FTZMessageStatusList.Codes.ClearUnconcurrence, calculator.Calculate(incoming, false, false));
			AssertEquals(FTZMessageStatusList.Codes.ErrorUnconcurrence, calculator.Calculate(incoming, true, false));

			incoming.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZCancelPermitToTransfer;
			AssertEquals(FTZMessageStatusList.Codes.PermitToTransferCancelAccepted, calculator.Calculate(incoming, false, false));
			AssertEquals(FTZMessageStatusList.Codes.PermitToTransferCancelUnauthorized, calculator.Calculate(incoming, true, false));

			incoming.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZSendPermitToTransferUnArrival;
			AssertEquals(FTZMessageStatusList.Codes.PermitToTransferUnArrived, calculator.Calculate(incoming, false, false));
			AssertEquals(FTZMessageStatusList.Codes.PermitToTransferArrivalCancelUnauthorized, calculator.Calculate(incoming, true, false));
		}
	}
}
