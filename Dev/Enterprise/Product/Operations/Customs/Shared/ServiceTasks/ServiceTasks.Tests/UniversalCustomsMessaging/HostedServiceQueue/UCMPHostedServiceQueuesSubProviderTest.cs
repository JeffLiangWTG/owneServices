using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;
using Enterprise.Scheduler.GraphEngine;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	class UCMPHostedServiceQueuesSubProviderTest : TestCaseWithFactory
	{
		public void TestIHostedServiceQueuesSubProviderMembers()
		{
			var message1 = Factory.New<EDIMessage>();
			message1.EM_ApplicationCode = "_T3";
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var message2 = Factory.New<EDIMessage>();
			message2.EM_ApplicationCode = "_T3";
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var message3 = Factory.New<EDIMessage>();
			message3.EM_ApplicationCode = "_T3";
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var message4 = Factory.New<EDIMessage>();
			message4.EM_ApplicationCode = "_T4";
			message4.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();
			var queueStateFactory = new EDIMessageQueueStateFactory(new LoggingInformation(), "_T3", () => new GrEngineLogOptions(), 1);
			var queueState1 = queueStateFactory.NewQueueState(message1, Array.Empty<string>(), "MSG1231");
			queueState1.UpdateStatus(QueueStatusCodes.Codes.Queued);
			var queueState2 = queueStateFactory.NewQueueState(message1, Array.Empty<string>(), "MSG1232");
			queueState2.UpdateStatus(QueueStatusCodes.Codes.Blocked);
			var queueState3 = queueStateFactory.NewQueueState(message1, Array.Empty<string>(), "MSG1233");
			queueState3.UpdateStatus(QueueStatusCodes.Codes.Queued);
			var queueState4 = queueStateFactory.NewQueueState(message1, Array.Empty<string>(), "MSG1234");
			queueState4.UpdateStatus(QueueStatusCodes.Codes.Queued);
			var queueState5 = queueStateFactory.NewQueueState(message1, Array.Empty<string>(), "MSG1235");
			queueState5.UpdateStatus(QueueStatusCodes.Codes.Blocked);
			var queueState6 = queueStateFactory.NewQueueState(message1, Array.Empty<string>(), "MSG1236");
			queueState6.UpdateStatus(QueueStatusCodes.Codes.PreKey);
			queueStateFactory.InsertAsQueued(new[] { queueState1, queueState2, queueState3, queueState4, queueState5, queueState6 });

			using (new UCMPProcessorsRegistrationSubstitute(("_T3", new UniversalCustomsMessageProcessorTestClass())))
			{
				IHostedServiceQueuesSubProvider provider = new UCMPHostedServiceQueuesSubProvider();
				var queues = provider.Queues;
				AssertSame("Queues", queues, provider.Queues);
				var queuesArray = queues.ToArray();
				AssertEquals("queuesArray.Length", 4, queuesArray.Length);
				EDIMessageQueueStateQueueProviderTest.AssertIHostedServiceQueue(queuesArray[0], UniversalCustomsMessagingConstants.ServiceTaskCodes.KeyGen, "UCK-_T3 Queued Service Task", 2);
				EDIMessageQueueStateQueueProviderTest.AssertIHostedServiceQueue(queuesArray[1], UniversalCustomsMessagingConstants.ServiceTaskCodes.Master, "UCI-_T3 PreKey Service Task", 1);
				EDIMessageQueueStateQueueProviderTest.AssertIHostedServiceQueue(queuesArray[2], UniversalCustomsMessagingConstants.ServiceTaskCodes.Master, "UCI-_T3 Blocked Service Task", 2);
				EDIMessageQueueStateQueueProviderTest.AssertIHostedServiceQueue(queuesArray[3], UniversalCustomsMessagingConstants.ServiceTaskCodes.Worker, "UCQ-_T3 Queued Service Task", 3);
			}
		}
	}
}
