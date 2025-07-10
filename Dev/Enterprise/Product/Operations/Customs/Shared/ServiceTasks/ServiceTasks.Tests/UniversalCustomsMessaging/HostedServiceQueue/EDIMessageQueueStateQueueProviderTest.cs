using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Scheduler.GraphEngine;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	class EDIMessageQueueStateQueueProviderTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();
			var queueStateFactory = new EDIMessageQueueStateFactory(new LoggingInformation(), "_T3", () => new GrEngineLogOptions(), 1);
			var queueState1 = queueStateFactory.NewQueueState(message, Array.Empty<string>(), "MSG1234");
			queueState1.UpdateStatus(QueueStatusCodes.Codes.Queued);
			var queueState2 = queueStateFactory.NewQueueState(message, Array.Empty<string>(), "MSG1235");
			queueState2.UpdateStatus(QueueStatusCodes.Codes.Blocked);
			var queueState3 = queueStateFactory.NewQueueState(message, Array.Empty<string>(), "MSG1236");
			queueState3.UpdateStatus(QueueStatusCodes.Codes.Queued);
			var queueState4 = queueStateFactory.NewQueueState(message, Array.Empty<string>(), "MSG1237");
			queueState4.UpdateStatus(QueueStatusCodes.Codes.Queued);
			queueStateFactory.InsertAsQueued(new[] { queueState1, queueState2, queueState3, queueState4 });
			var provider = new EDIMessageQueueStateQueueProvider("STC", "TEST NAME", "_T3", QueueStatusCodes.Codes.Queued);
			AssertIHostedServiceQueue(provider, "STC", "TEST NAME", 3);
		}

		public static void AssertIHostedServiceQueue(IHostedServiceQueue hostedServiceQueue, string serviceTaskCode, string name, int queueSize)
		{
			AssertEquals("ServiceTaskCode", serviceTaskCode, hostedServiceQueue.ServiceTaskCode);
			AssertEquals("Name", name, hostedServiceQueue.Name);
			AssertEquals("provider.QueueResult.QueueSize", queueSize, hostedServiceQueue.QueueResult.QueueSize);
		}
	}
}
