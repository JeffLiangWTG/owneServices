using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils.Tests;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging;
using Enterprise.GraphEngine.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Scheduler.GraphEngine;
using NUnit.Framework;

namespace Enterprise.Customs.ServiceTasks.Testing
{
	class EDIMessageQueueStateFactoryTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestLoadNotifiedReturnsNonLockedChain()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var factory1 = new BusinessObjectFactory();
				var message = factory1.New<EDIMessage>();
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				factory1.Save();
				var queueStateFactory = new EDIMessageQueueStateFactory(new LoggingInformation(), "_T3", () => new GrEngineLogOptions(), 1);
				var queueState1 = queueStateFactory.NewQueueState(message, Array.Empty<string>(), "MSG1234");
				queueState1.UpdateStatus(QueueStatusCodes.Codes.Queued);
				var chainId = Guid.NewGuid();
				queueState1.SetChainId(chainId);
				var queueState2 = queueStateFactory.NewQueueState(message, Array.Empty<string>(), "MSG1235");
				queueState2.UpdateStatus(QueueStatusCodes.Codes.Blocked);
				queueState2.SetChainId(chainId);
				var queueState3 = queueStateFactory.NewQueueState(message, Array.Empty<string>(), "MSG1236");
				queueState3.UpdateStatus(QueueStatusCodes.Codes.Queued);
				var queueState4 = queueStateFactory.NewQueueState(message, Array.Empty<string>(), "MSG1237");
				queueState4.UpdateStatus(QueueStatusCodes.Codes.Blocked);
				queueStateFactory.InsertAsQueued(new[] { queueState1, queueState2, queueState3, queueState4 });
				var testLockProvider = TestLockProvider.CreateDbLockProvider(Db.Connection);
				var logger = new LoggingInformation();
				var engine = new EDIMessageGrEngine(logger, "_T3", GrEngineServiceSetting.Worker, testLockProvider);
				var dequeuer = engine.Dequeuer;
				Db.Connection.ExecuteNonQuery($"UPDATE dbo.EDIMessageQueueState SET EQS_Status = 'NTF', EQS_SystemLastEditUser = '~BP', EQS_SystemLastEditTimeUtc = GETUTCDATE() WHERE EQS_PK IN ('{queueState1.Identifier}', '{queueState3.Identifier}')");

				Assertion.Assert(connection.TryGetLock("_T3DEQUEUEGRENGINE" + chainId.ToString().ToUpperInvariant(), out var chainAppLock));
				var notifiedQueues = engine.Dequeuer.LoadNotified().ToArray();
				Assertion.AssertEquals("notifiedQueues", 1, notifiedQueues.Length);
				Assertion.AssertEquals("notifiedQueues[0].Identifier", queueState3.Identifier, notifiedQueues[0].Identifier);

				chainAppLock.Dispose();
				notifiedQueues = engine.Dequeuer.LoadNotified().ToArray();
				Assertion.AssertEquals("notifiedQueues", 1, notifiedQueues.Length);
				Assertion.AssertEquals("notifiedQueues[0].Identifier", queueState1.Identifier, notifiedQueues[0].Identifier);
			}
		}
	}
}
