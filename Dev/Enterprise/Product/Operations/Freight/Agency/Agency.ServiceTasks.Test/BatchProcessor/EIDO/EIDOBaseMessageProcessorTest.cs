using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;

namespace Enterprise.Freight.Agency.ServiceTasks.Testing
{
	internal class EIDOBaseMessageProcessorTest : BaseAgencyTest
	{
		public void TestOrderAndHint()
		{
			var processor = new EIDOBaseMessageProcessorForTest();
			var query = processor.GetMessageProcessorQueryExposed();
			var hint = query.TableIndexHints.Single();
			AssertEquals("EM_MessageNum, EM_SystemCreateTimeUtc", query.OrderBy);
			AssertEquals("NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc", hint.IndexName);
		}

		public void TestProcessesEIDOMessages()
		{
			EDIMessage message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EIDO;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();
			Processor.ExecuteBatch();
			Factory.ReloadAll<EDIMessage>();
			AssertEquals("message should have been processed", EDIMessage.Status.Failed, message.EM_Status);
		}

		#region Implementation
		EIDOBaseMessageProcessor Processor
		{
			get
			{
				return processor ?? (processor = new EIDOBaseMessageProcessor());
			}
		}

		EIDOBaseMessageProcessor processor;
		class EIDOBaseMessageProcessorForTest : EIDOBaseMessageProcessor
		{
			public ZQuery GetMessageProcessorQueryExposed() => GetMessageProcessorQuery();
		}
		#endregion
	}
}
