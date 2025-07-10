using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils.Tests;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.GraphEngine.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Scheduler.GraphEngine;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	sealed class KeyGenUniversalCustomsApplicationTypeMessageProcessorTest : TestCaseWithFactory
	{
		public void TestChangesToMessageInKeyGenIsNotAllowed()
		{
			var nzBranch = ProcessingManagerTest.SetupBranch(Factory, Core.Constants.CountryCodes.NewZealand);
			var dec = Factory.New<BaseJobDeclaration>();
			var message1 = ProcessingManagerTest.CreateMessage(Factory, "A@#", "MT1", "MN1234A", "MSG1");
			message1.EM_MessageSubType = "KJ2";
			Factory.Save();

			var logger = new LoggingInformation();
			var testLockProvider = TestLockProvider.CreateDbLockProvider(Db.Connection);
			var engine = new EDIMessageGrEngine(logger, "A@#", GrEngineServiceSetting.KeyGen, testLockProvider);
			var messageProcessor = new UniversalCustomsMessageProcessorTestClass();

			messageProcessor.GetLinkedBusinessObjectMetaDataForTesting = (message, logger) =>
			{
				message.EM_MessageSubType = "AB4";
				return new LinkedBusinessObjectMetaData(dec.TableName, dec.PK, dec.JE_GB, dec.JE_DeclarationReference);
			};

			messageProcessor.GetBranchForTesting = (_, _, _) => nzBranch.PK;

			messageProcessor.GetSerializationKeysResultForTesting = (_, _, _) =>
			{
				return new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string> { "DEF2" });
			};

			var keyGenMessageProcessor = new KeyGenUniversalCustomsApplicationTypeMessageProcessor(logger, "A@#", messageProcessor);
			keyGenMessageProcessor.ProcessMessage(message1);

			AssertEquals("LastMessageReported", $"{typeof(UniversalCustomsMessageProcessorTestClass).FullName} should not modify the message during UCK processing.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			message1.Reload();
			ProcessingManagerTest.AssertMessage(message1, nzBranch.PK, dec.PK, dec.TableName, EDIMessage.Status.PreProcessedOK);
			AssertEquals("EM_MessageSubType ", "KJ2", message1.EM_MessageSubType);

			var queueStates = engine.Dequeuer.LoadAll().ToArray();
			AssertEquals("queueStates.Length", 1, queueStates.Length);
			ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[0], message1.PK.ToGuid(), "MN1234A", new[] { "DEF2" }, Guid.Empty, QueueStatusCodes.Codes.PreKey);
		}

		public void TestGetLinkedBusinessObjectMetaData_HandleNull()
		{
			var message1 = ProcessingManagerTest.CreateMessage(Factory, "A@#", "MT1", "MN1234A", "MSG1");
			Factory.Save();

			var logger = new LoggingInformation();
			var testLockProvider = TestLockProvider.CreateDbLockProvider(Db.Connection);
			var engine = new EDIMessageGrEngine(logger, "A@#", GrEngineServiceSetting.KeyGen, testLockProvider);
			var messageProcessor = new UniversalCustomsMessageProcessorTestClass();

			messageProcessor.GetLinkedBusinessObjectMetaDataForTesting = (_, logger) => null;

			var keyGenMessageProcessor = new KeyGenUniversalCustomsApplicationTypeMessageProcessor(logger, "A@#", messageProcessor);
			keyGenMessageProcessor.ProcessMessage(message1);

			var queueStates = engine.Dequeuer.LoadAll().ToArray();
			AssertEquals("queueStates.Length", 1, queueStates.Length);
			ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[0], message1.PK.ToGuid(), "MN1234A", new[] { "Unknown" }, Guid.Empty, QueueStatusCodes.Codes.PreKey);
		}

		public void TestGetBranch_HandleNull()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var message1 = ProcessingManagerTest.CreateMessage(Factory, "A@#", "MT1", "MN1234A", "MSG1");
			Factory.Save();

			var logger = new LoggingInformation();
			var testLockProvider = TestLockProvider.CreateDbLockProvider(Db.Connection);
			var engine = new EDIMessageGrEngine(logger, "A@#", GrEngineServiceSetting.KeyGen, testLockProvider);
			var messageProcessor = new UniversalCustomsMessageProcessorTestClass();

			messageProcessor.GetLinkedBusinessObjectMetaDataForTesting = (message, logger) =>
			{
				return new LinkedBusinessObjectMetaData(dec.TableName, dec.PK, dec.JE_GB, dec.JE_DeclarationReference);
			};

			messageProcessor.GetBranchForTesting = (_, _, _) => null;

			var keyGenMessageProcessor = new KeyGenUniversalCustomsApplicationTypeMessageProcessor(logger, "A@#", messageProcessor);
			keyGenMessageProcessor.ProcessMessage(message1);

			var queueStates = engine.Dequeuer.LoadAll().ToArray();
			AssertEquals("queueStates.Length", 1, queueStates.Length);
			ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[0], message1.PK.ToGuid(), "MN1234A", new[] { "Unknown" }, Guid.Empty, QueueStatusCodes.Codes.PreKey);
		}

		public void TestGetSerializationKeysResult_HandleNull()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var message1 = ProcessingManagerTest.CreateMessage(Factory, "A@#", "MT1", "MN1234A", "MSG1");
			Factory.Save();

			var logger = new LoggingInformation();
			var testLockProvider = TestLockProvider.CreateDbLockProvider(Db.Connection);
			var engine = new EDIMessageGrEngine(logger, "A@#", GrEngineServiceSetting.KeyGen, testLockProvider);
			var messageProcessor = new UniversalCustomsMessageProcessorTestClass();

			messageProcessor.GetLinkedBusinessObjectMetaDataForTesting = (message, logger) =>
			{
				return new LinkedBusinessObjectMetaData(dec.TableName, dec.PK, dec.JE_GB, dec.JE_DeclarationReference);
			};

			messageProcessor.GetBranchForTesting = (message, _, _) => message.EM_GB;

			messageProcessor.GetSerializationKeysResultForTesting = (_, _, _) => null;

			var keyGenMessageProcessor = new KeyGenUniversalCustomsApplicationTypeMessageProcessor(logger, "A@#", messageProcessor);
			keyGenMessageProcessor.ProcessMessage(message1);

			var queueStates = engine.Dequeuer.LoadAll().ToArray();
			AssertEquals("queueStates.Length", 1, queueStates.Length);
			ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[0], message1.PK.ToGuid(), "MN1234A", new[] { SerializationKeysResult.ForceSerialProcessingKey }, Guid.Empty, QueueStatusCodes.Codes.PreKey);
		}

		public void TestHandleInvalidData()
		{
			var nzBranch = ProcessingManagerTest.SetupBranch(Factory, Core.Constants.CountryCodes.NewZealand);
			var dec = Factory.New<BaseJobDeclaration>();
			var message1 = ProcessingManagerTest.CreateMessage(Factory, "A@#", "MT1", "MN1234A", "MSG1");
			Factory.Save();
			var branchPK = ZGuid.Empty;
			var linkUniqueID = ZGuid.Empty;
			var linkTableName = ZString.Empty;

			var logger = new LoggingInformation();
			var testLockProvider = TestLockProvider.CreateDbLockProvider(Db.Connection);
			var engine = new EDIMessageGrEngine(logger, "A@#", GrEngineServiceSetting.KeyGen, testLockProvider);
			var messageProcessor = new UniversalCustomsMessageProcessorTestClass();

			messageProcessor.GetLinkedBusinessObjectMetaDataForTesting = (_, _) => new LinkedBusinessObjectMetaData(linkTableName, linkUniqueID, branchPK, ZString.Empty);

			messageProcessor.GetBranchForTesting = (_, _, _) => branchPK;

			messageProcessor.GetSerializationKeysResultForTesting = (_, _, _) =>
			{
				return new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string> { "DEF2" });
			};

			var keyGenMessageProcessor = new KeyGenUniversalCustomsApplicationTypeMessageProcessor(logger, "A@#", messageProcessor);
			keyGenMessageProcessor.ProcessMessage(message1);

			message1.Reload();
			ProcessingManagerTest.AssertMessage(message1, GlbBranch.CurrentBranch.PK, ZGuid.Empty, ZString.Empty, EDIMessage.Status.PreProcessedOK);

			AssertEquals("Logger", 0, logger.UserLogStrings.Count);

			var queueStates = engine.Dequeuer.LoadAll().ToArray();
			AssertEquals("queueStates.Length", 1, queueStates.Length);
			ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[0], message1.PK.ToGuid(), "MN1234A", new[] { "DEF2" }, Guid.Empty, QueueStatusCodes.Codes.PreKey);
		}

		public void TestProcessMessage()
		{
			var nzBranch = ProcessingManagerTest.SetupBranch(Factory, Core.Constants.CountryCodes.NewZealand);
			var dec = Factory.New<BaseJobDeclaration>();
			var message1 = ProcessingManagerTest.CreateMessage(Factory, "A@#", "MT1", "MN1234A", "MSG1");
			Factory.Save();

			var logger = new LoggingInformation();
			var testLockProvider = TestLockProvider.CreateDbLockProvider(Db.Connection);
			var engine = new EDIMessageGrEngine(logger, "A@#", GrEngineServiceSetting.KeyGen, testLockProvider);
			var messageProcessor = new UniversalCustomsMessageProcessorTestClass();

			messageProcessor.GetLinkedBusinessObjectMetaDataForTesting = (message, logger) =>
			{
				logger.Log($"{message.EM_MessageType}-{message.EM_MessageNum}");

				return new LinkedBusinessObjectMetaData(dec.TableName, dec.PK, dec.JE_GB, dec.JE_DeclarationReference);
			};

			messageProcessor.GetBranchForTesting = (_, _, _) => nzBranch.PK;

			messageProcessor.GetSerializationKeysResultForTesting = (_, _, _) =>
			{
				return new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string> { "DEF2" });
			};

			var keyGenMessageProcessor = new KeyGenUniversalCustomsApplicationTypeMessageProcessor(logger, "A@#", messageProcessor);
			keyGenMessageProcessor.ProcessMessage(message1);

			message1.Reload();
			ProcessingManagerTest.AssertMessage(message1, nzBranch.PK, dec.PK, dec.TableName, EDIMessage.Status.PreProcessedOK);

			AssertEquals("Logger", "MT1-MN1234A", logger.UserLogStrings.Cast<string>().Single().Trim());

			var queueStates = engine.Dequeuer.LoadAll().ToArray();
			AssertEquals("queueStates.Length", 1, queueStates.Length);
			ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[0], message1.PK.ToGuid(), "MN1234A", new[] { "DEF2" }, Guid.Empty, QueueStatusCodes.Codes.PreKey);
		}

		public void TestProcessMessageDiscard()
		{
			var nzBranch = ProcessingManagerTest.SetupBranch(Factory, Core.Constants.CountryCodes.NewZealand);
			var dec = Factory.New<BaseJobDeclaration>();
			var message1 = ProcessingManagerTest.CreateMessage(Factory, "A@#", "MT1", "MN1234A", "MSG1");
			Factory.Save();

			var logger = new LoggingInformation();
			var testLockProvider = TestLockProvider.CreateDbLockProvider(Db.Connection);
			var engine = new EDIMessageGrEngine(logger, "A@#", GrEngineServiceSetting.KeyGen, testLockProvider);
			var messageProcessor = new UniversalCustomsMessageProcessorTestClass();

			messageProcessor.GetLinkedBusinessObjectMetaDataForTesting = (_, _) => LinkedBusinessObjectMetaData.Empty;

			messageProcessor.GetBranchForTesting = (_, _, _) => ZGuid.Empty;

			messageProcessor.GetSerializationKeysResultForTesting = (_, _, _) =>
			{
				return ProcessingResult.New(SerializationKeysResult.UnconstrainedParallelProcessing, (NoResString)"Because not good");
			};

			var keyGenMessageProcessor = new KeyGenUniversalCustomsApplicationTypeMessageProcessor(logger, "A@#", messageProcessor);
			keyGenMessageProcessor.ProcessMessage(message1);

			message1.Reload();
			ProcessingManagerTest.AssertMessage(message1, GlbBranch.CurrentBranch.PK, ZGuid.Empty, ZString.Empty, EDIMessage.Status.Discarded);

			AssertEquals("Logger", "Status set to Discarded due to the following reason: Because not good", logger.UserLogStrings.Cast<string>().Single().Trim());

			var queueStates = engine.Dequeuer.LoadAll().ToArray();
			AssertEquals("queueStates.Length", 0, queueStates.Length);
		}

		public void TestMessageFriendlyName()
		{
			var logger = new LoggingInformation();
			var testLockProvider = TestLockProvider.CreateDbLockProvider(Db.Connection);
			var messageProcessor = new UniversalCustomsMessageProcessorTestClass();
			var keyGenMessageProcessor = new KeyGenUniversalCustomsApplicationTypeMessageProcessor(logger, "A@#", messageProcessor);
			AssertEquals("MessageFriendlyName", "Key Generation Universal Customs Messaging", keyGenMessageProcessor.MessageFriendlyName);
		}
	}
}
