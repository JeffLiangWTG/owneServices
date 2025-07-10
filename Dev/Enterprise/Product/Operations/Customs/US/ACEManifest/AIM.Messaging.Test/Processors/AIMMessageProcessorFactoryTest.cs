using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	public class AIMMessageProcessorFactoryTest : TestCase
	{
		public void TestAIMMessageProcessorFactory()
		{
			var processorFactory = new AIMMessageProcessorFactoryForTest(new LoggingInformation());

			AssertNull(processorFactory.ValidBranchesForMessageFilterExposed);

			var processors = processorFactory.GetMessageProcessorsExposed();
			AssertEquals(1, processors.OfType<AIMMessageProcessor>().Count());
		}

		public void TestOrderAndHint()
		{
			var processor = new AIMMessageProcessorFactoryForTest(new LoggingInformation());
			var query = processor.GetMessageProcessorQueryExposed();
			var hint = query.TableIndexHints.Single();

			AssertEquals("EM_MessageNum, EM_SystemCreateTimeUtc", query.OrderBy);
			AssertEquals("NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc", hint.IndexName);
		}
	}

	public class AIMMessageProcessorFactoryForTest : AIMMessageProcessorFactory
	{
		public AIMMessageProcessorFactoryForTest(LoggingInformation logger) : base(logger)
		{
		}

		public ZQuery ValidBranchesForMessageFilterExposed => ValidBranchesForMessageFilter;
		public List<ApplicationTypeMessageProcessor> GetMessageProcessorsExposed() => GetMessageProcessors();
		public ZQuery GetMessageProcessorQueryExposed() => GetMessageProcessorQuery();
	}
}
