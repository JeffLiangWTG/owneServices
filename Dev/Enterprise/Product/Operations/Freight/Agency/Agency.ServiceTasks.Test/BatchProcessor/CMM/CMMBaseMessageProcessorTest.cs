using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;

namespace Enterprise.Freight.Agency.ServiceTasks.Testing
{
	internal class CMMBaseMessageProcessorTest : BaseAgencyTest
	{
		public void TestOrderAndHint()
		{
			var processor = new CMMBaseMessageProcessorForTest();
			var query = processor.GetMessageProcessorQueryExposed();
			var hint = query.TableIndexHints.Single();
			AssertEquals("EM_MessageNum, EM_SystemCreateTimeUtc", query.OrderBy);
			AssertEquals("NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc", hint.IndexName);
		}

		public void TestProcessCODECOMessages()
		{
			EDIMessage message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.ContainerManagement;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();
			Processor.ExecuteBatch();
			Factory.ReloadAll<EDIMessage>();
			AssertEquals("message should have been processed", EDIMessage.Status.Failed, message.EM_Status);
		}

		public void TestProcessCARRIMessages()
		{
			EDIMessage message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.ContainerManagement;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();
			Processor.ExecuteBatch();
			Factory.ReloadAll<EDIMessage>();
			AssertEquals("message should have been processed", EDIMessage.Status.Failed, message.EM_Status);
		}

		#region Implementation
		CMMBaseMessageProcessor Processor
		{
			get
			{
				return processor ?? (processor = new CMMBaseMessageProcessor());
			}
		}

		CMMBaseMessageProcessor processor;
		class CMMBaseMessageProcessorForTest : CMMBaseMessageProcessor
		{
			public ZQuery GetMessageProcessorQueryExposed() => GetMessageProcessorQuery();
		}
		#endregion
	}
}
