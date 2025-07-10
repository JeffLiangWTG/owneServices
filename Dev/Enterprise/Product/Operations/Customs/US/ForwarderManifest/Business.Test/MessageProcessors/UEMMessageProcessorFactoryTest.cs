using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	public class UEMMessageProcessorFactoryTest : BaseMessageProcessorTest
	{
		public void TestValidBranchesForMessageFilter()
		{
			var query = uemMessageProcessorFactory.GetMessageProcessorQuery();
			AssertNotContains("Query does not contain EM_GB.", EDIMessage.Schema.EM_GB, query.LiteralTextSqlFormatted);
		}

		public void TestOverrideGetMessageProcessors()
		{
			var query = new ZQuery();
			uemMessageProcessorFactory.AddApplicationCodeFilters(query);
			AssertContains("ApplicationCodeFilters", "EM_ApplicationCode = \'UEM\'", query.LiteralTextSqlFormatted);
		}

		protected override void SetUp()
		{
			base.SetUp();
			uemMessageProcessorFactory = new UEMMessageProcessorFactory(new LoggingInformation());
		}

		UEMMessageProcessorFactory uemMessageProcessorFactory;
	}
}
