using Enterprise.BatchProcessor;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.Customs.ZA.Business.UCMP;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing.UCMP
{
	sealed class ZACMessageProcessorProviderTest : TestCase
	{
		public void TestGetMessageProcessor_CONTRL() =>
			AssertType<CONTRLMessageProcessor>(ZACMessageProcessorFactory.GetMessageProcessor(SARSEDIMessage.MessageTypes.CONTRL, logger));

		public void TestGetMessageProcessor_CUSCAR() =>
			AssertType<CUSCARMessageProcessor>(ZACMessageProcessorFactory.GetMessageProcessor(SARSEDIMessage.MessageTypes.CUSCAR, logger));

		public void TestGetMessageProcessor_CUSRES() =>
			AssertType<CUSRESMessageProcessor>(ZACMessageProcessorFactory.GetMessageProcessor(SARSEDIMessage.MessageTypes.CUSRES, logger));

		public void TestGetMessageProcessor_CUSRES_REQDOC() =>
			AssertType<CUSRES_REQDOCMessageProcessor>(ZACMessageProcessorFactory.GetMessageProcessor(SARSEDIMessage.MessageTypes.CUSRES_REQDOC, logger));

		public void TestGetMessageProcessor_GENRAL() =>
			AssertType<GENRALMessageProcessor>(ZACMessageProcessorFactory.GetMessageProcessor(SARSEDIMessage.MessageTypes.GENRAL, logger));

		public void TestGetMessageProcessor_STATAC() =>
			AssertType<STATACMessageProcessor>(ZACMessageProcessorFactory.GetMessageProcessor(SARSEDIMessage.MessageTypes.STATAC, logger));

		public void TestGetMessageProcessor_Unsupported() =>
			AssertType<ZACMessageProcessorFactory.UnsupportedTypeMessageProcessor>(
				ZACMessageProcessorFactory.GetMessageProcessor("NOT_A_ZAC_MESSAGE_TYPE", logger));

		readonly LoggingInformation logger = new ();
	}
}
