using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class EBondIncomingMessageProcessorTest : TestCaseWithFactory
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessMessage()
		{
			EBondMssageProcessorFactoryTest.CreateDeclarationForEBondMessage(Factory);
			var interchange = EBondMssageProcessorFactoryTest.CreateInterchangeForEBondMessage(Factory);
			Factory.Save();

			var messageProcessor = new EBondIncomingMessageProcessor();
			messageProcessor.ExecuteBatch();

			var message = interchange.ContainedMessages[0];
			AssertEquals("Should create a matched message and process it.", EDIMessage.Status.Received, message.EM_Status);
		}
	}
}
