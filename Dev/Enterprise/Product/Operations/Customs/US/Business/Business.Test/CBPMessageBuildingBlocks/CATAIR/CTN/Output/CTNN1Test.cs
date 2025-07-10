using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Output.Testing
{
	sealed class CTNN1Test : TestCaseWithFactory
	{
		public void TestForwardedLiquidationMessage()
		{
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.BIRDTransaction;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;// when forwarded to external broker
			message.EM_MessageNum = "~15000";
			message.EM_Status = EDIMessage.Status.Sent;
			message.EM_MessageText = "AANR  XJ68888B00004773                  20091207204452                          N12222XJ5 1000241401NNN-NN-NNNN 000000137000000000563000000013700000000056301   N22222XJ5 1000241412030714BROKR-REFCBP_DOC_FILING_LOC607140700000000000000008247N42222XJ5 100024140000000010000000000200000000003000000000040000000000500       ZZNR  00000000301";

			AssertNoExceptionThrown(() => _ = message.MessageBlock);
		}
	}
}
