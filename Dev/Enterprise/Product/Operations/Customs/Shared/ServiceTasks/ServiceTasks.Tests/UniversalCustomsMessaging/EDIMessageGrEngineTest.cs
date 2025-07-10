using System;
using CargoWise.Data;
using CargoWise.Data.Utils.Tests;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.GraphEngine.ServiceTasks;
using Enterprise.Scheduler.GraphEngine;
using NUnit.Framework;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	class EDIMessageGrEngineTest : TransactionedTestCase
	{
		public void TestSettings()
		{
			using (CustomsDataRegistry.Instance.UCKMessagesPerBatch.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 29))
			using (CustomsDataRegistry.Instance.UCIMessagesPerBatch.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 45))
			using (CustomsDataRegistry.Instance.UCMMessageQueueCapacity.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3523))
			using (CustomsDataRegistry.Instance.UCKPreEnqueuerMaxBacklogSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 512))
			{
				var testLockProvider = TestLockProvider.CreateDbLockProvider(Db.Connection);
				var engine = new EDIMessageGrEngine(new LoggingInformation(), "A@#", GrEngineServiceSetting.KeyGen, testLockProvider);
				AssertEquals("Setting", GrEngineServiceSetting.KeyGen, engine.Setting);
				AssertEquals("IsEnabled", true, engine.IsEnabled);
				GrEngineServiceSetup<EDIMessageQueueState> setup = engine.Setup;
				AssertEquals("BatchSize", 29, engine.BatchSize);
				AssertEquals("FlipperBatchSize", 45, engine.FlipperBatchSize);
				AssertEquals("Capacity", 3523, engine.Capacity);
				AssertEquals("PreKeyBacklogSize", 512, engine.PreKeyBacklogSize);
				AssertEquals("ShouldCreateKeys", true, engine.ShouldCreateKeys);
			}
		}
	}
}
