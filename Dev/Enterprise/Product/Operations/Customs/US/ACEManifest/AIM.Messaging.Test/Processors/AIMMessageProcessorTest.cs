using Enterprise.BatchProcessor;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	public class AIMMessageProcessorTest : TestCase
	{
		public void TestAIMMessageProcessor()
		{
			var processor = new AIMMessageProcessor(new LoggingInformation());
			AssertEquals(ApplicationCodeList.Codes.USAMA, processor.ApplicationCode);
			AssertEquals(false, processor.RequiresPreProcessing);
		}
	}
}
