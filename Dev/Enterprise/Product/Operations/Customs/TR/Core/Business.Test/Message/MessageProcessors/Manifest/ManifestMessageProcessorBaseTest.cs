using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	public abstract class ManifestMessageProcessorAbstractTest<T> : TestCaseWithFactory
		where T : ManifestMessageProcessorBase
	{
		public void TestMessageFriendlyName()
		{
			AssertEquals("MessageFriendlyName", "TR Manifest Message Processor", Processor.MessageFriendlyName);
		}

		public void TestApplicationCode()
		{
			AssertEquals(EDIMessage.ApplicationCodes.TRCustoms, Processor.ApplicationCode);
		}

		protected abstract ManifestMessageProcessorBase Processor { get; }

		protected override void SetUp()
		{
			base.SetUp();
			logger = new LoggingInformation();
		}
		protected LoggingInformation logger;
	}
}
