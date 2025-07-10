using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class ForeignPortProcessorTest : TestCaseWithFactory
	{
		[StressTest]
		public void TestMessageStatus()
		{
			BlockControlGenerator generator = new ABIOutputBlockControlGenerator<AABIOutputB, AABIOutputY>();
			generator.B.ApplicationIdentifier = ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse;
			var f104 = new ERFF104();
			f104.ForeignPortOfLadingCode = "Z564";
			f104.ForeignPortOfLadingName = "DUMMY PORT";
			generator.AddMessageBlock(f104);

			var ediMessage = Factory.New<MQEDIMessage>();
			ediMessage.EM_ReceiveTransmit = "RCV";
			ediMessage.EM_MessageText = generator.Serialise();
			ediMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse;
			AssertEquals("QUE", ediMessage.EM_Status);
			new USRMessageProcessorFactory(new LoggingInformation()).ProcessMessage(ediMessage);
			AssertEquals("RCV", ediMessage.EM_Status);
		}
	}
}
