using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils.Tests;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.GraphEngine.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Scheduler.GraphEngine;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	[TestedType(typeof(UCMServiceTaskWorker))]
	class UCMServiceTaskWorkerTest : ServiceTaskTestCase<UCMServiceTaskWorker>
	{
		public void TestCorrectlySetupInUniversalCustomsMessageProcessors()
		{
			AssertNoExceptionThrown(() => InitialiseAndRunTaskSchedule(new UCMServiceTaskWorker()));
		}

		public void TestEndToEnd()
		{
			using (new UCMPProcessorsRegistrationSubstitute())
			{
				var branch1 = ProcessingManagerTest.SetupBranch(Factory, Core.Constants.CountryCodes.NewZealand);
				var branch1PK = branch1.PK;
				var branch2 = branch1.Company.Branches.AddNew();
				branch2.GB_Code = "N@%";
				branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				var branch2PK = branch2.PK;
				var dec1 = Factory.New<BaseJobDeclaration>();
				dec1.JE_GB = branch1PK;
				var dec1PK = dec1.PK;
				var dec2 = Factory.New<BaseJobDeclaration>();
				dec2.JE_GB = branch2PK;
				var dec2PK = dec2.PK;
				var date = ZDateTime.UtcNow;
				int count = -30;
				var outgoingMessage1 = ProcessingManagerTest.CreateMessage(Factory, "_T1", "MT1", "MN1234A", ZString.Empty);
				outgoingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				outgoingMessage1.EM_GB = branch2PK;
				outgoingMessage1.EM_LinkedObject = dec1;
				outgoingMessage1.MessageNumberStrategy = new TestMessageNumberStrategy("MN1234A");
				outgoingMessage1.EM_SystemCreateTimeUtc = date.AddDays(++count);
				var outgoingMessage2 = ProcessingManagerTest.CreateMessage(Factory, "_T1", "MT1", "MN1234B", ZString.Empty);
				outgoingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				outgoingMessage2.EM_GB = branch1PK;
				outgoingMessage2.EM_LinkedObject = dec1;
				outgoingMessage2.MessageNumberStrategy = new TestMessageNumberStrategy("MN1234B");
				outgoingMessage2.EM_SystemCreateTimeUtc = date.AddDays(++count);
				var outgoingMessage3 = ProcessingManagerTest.CreateMessage(Factory, "_T1", "MT1", "MN1234C", ZString.Empty);
				outgoingMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				outgoingMessage3.EM_GB = branch2PK;
				outgoingMessage3.EM_LinkedObject = dec1;
				outgoingMessage3.MessageNumberStrategy = new TestMessageNumberStrategy("MN1234C");
				outgoingMessage3.EM_SystemCreateTimeUtc = date.AddDays(++count);
				var outgoingMessage4 = ProcessingManagerTest.CreateMessage(Factory, "_T1", "MT1", "MN1234D", ZString.Empty);
				outgoingMessage4.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				outgoingMessage4.EM_GB = branch2PK;
				outgoingMessage4.EM_LinkedObject = dec2;
				outgoingMessage4.MessageNumberStrategy = new TestMessageNumberStrategy("MN1234D");
				outgoingMessage4.EM_SystemCreateTimeUtc = date.AddDays(++count);
				var outgoingMessage5 = ProcessingManagerTest.CreateMessage(Factory, "_T1", "MT1", "MN1234E", ZString.Empty);
				outgoingMessage5.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				outgoingMessage5.EM_GB = branch1PK;
				outgoingMessage5.EM_LinkedObject = dec2;
				outgoingMessage5.MessageNumberStrategy = new TestMessageNumberStrategy("MN1234E");
				outgoingMessage5.EM_SystemCreateTimeUtc = date.AddDays(++count);
				var outgoingMessage6 = ProcessingManagerTest.CreateMessage(Factory, "A&#", "MT1", "MN1234F", ZString.Empty);
				outgoingMessage6.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				outgoingMessage6.EM_GB = branch2PK;
				outgoingMessage6.EM_LinkedObject = dec2;
				outgoingMessage6.MessageNumberStrategy = new TestMessageNumberStrategy("MN1234F");
				outgoingMessage6.EM_SystemCreateTimeUtc = date.AddDays(++count);
				var outgoingMessage7 = ProcessingManagerTest.CreateMessage(Factory, "_T1", "MT1", "MN1234G", ZString.Empty);
				outgoingMessage7.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				outgoingMessage7.EM_GB = branch1PK;
				outgoingMessage7.MessageNumberStrategy = new TestMessageNumberStrategy("MN1234G");
				outgoingMessage7.EM_SystemCreateTimeUtc = date.AddDays(++count);

				var incomingMessage1 = ProcessingManagerTest.CreateMessage(Factory, "_T1", "MT1", "MN1234A", "J1");
				incomingMessage1.EM_SystemCreateTimeUtc = date.AddDays(++count);
				var incomingMessage2 = ProcessingManagerTest.CreateMessage(Factory, "_T1", "MT1", "MN1234B", "J1");
				incomingMessage2.EM_SystemCreateTimeUtc = date.AddDays(++count);
				var incomingMessage3 = ProcessingManagerTest.CreateMessage(Factory, "_T1", "MT1", "MN1234C", "J2");
				incomingMessage3.EM_SystemCreateTimeUtc = date.AddDays(++count);
				var incomingMessage4 = ProcessingManagerTest.CreateMessage(Factory, "_T1", "MT1", "MN1234D", "J3");
				incomingMessage4.EM_SystemCreateTimeUtc = date.AddDays(++count);
				var incomingMessage5 = ProcessingManagerTest.CreateMessage(Factory, "_T1", "MT1", "MN1234E", "J3");
				incomingMessage5.EM_SystemCreateTimeUtc = date.AddDays(++count);
				var incomingMessage6 = ProcessingManagerTest.CreateMessage(Factory, "A&#", "MT1", "MN1234F", "J1");
				incomingMessage6.EM_SystemCreateTimeUtc = date.AddDays(++count);
				var incomingMessage7 = ProcessingManagerTest.CreateMessage(Factory, "_T1", "MT1", "MN1234G", "J4");
				incomingMessage7.EM_SystemCreateTimeUtc = date.AddDays(++count);
				var incomingMessage8 = ProcessingManagerTest.CreateMessage(Factory, "_T1", "MT1", "MN1234E", "J3");
				incomingMessage8.EM_SystemCreateTimeUtc = date.AddDays(++count);
				var incomingMessage9 = ProcessingManagerTest.CreateMessage(Factory, "_T1", "MT1", "MN1234E", "J3");
				incomingMessage9.EM_ApplicationReference = "AppRef1";
				incomingMessage9.EM_SystemCreateTimeUtc = date.AddDays(++count);
				var incomingMessage10 = ProcessingManagerTest.CreateMessage(Factory, "_T1", "MT1", "MN1234E", "J3");
				incomingMessage10.EM_ApplicationReference = "AppRef1";
				incomingMessage10.EM_MessageSubType = "MST";
				incomingMessage10.EM_SystemCreateTimeUtc = date.AddDays(++count);
				Factory.Save();
				var incomingMessage1PK = incomingMessage1.PK.ToGuid();
				var incomingMessage2PK = incomingMessage2.PK.ToGuid();
				var incomingMessage3PK = incomingMessage3.PK.ToGuid();
				var incomingMessage4PK = incomingMessage4.PK.ToGuid();
				var incomingMessage5PK = incomingMessage5.PK.ToGuid();
				var incomingMessage6PK = incomingMessage6.PK.ToGuid();
				var incomingMessage7PK = incomingMessage7.PK.ToGuid();
				var incomingMessage8PK = incomingMessage8.PK.ToGuid();
				var incomingMessage9PK = incomingMessage9.PK.ToGuid();
				var incomingMessage10PK = incomingMessage10.PK.ToGuid();
				var dec1JE_DeclarationReference = dec1.JE_DeclarationReference;
				var dec2JE_DeclarationReference = dec2.JE_DeclarationReference;

				var testTypeCollection = new UCMEDIMessageTestTypeCollection();
				var testType = testTypeCollection.AddNew();
				testType.ApplicationCode = "_T1";
				using (CustomsDataRegistry.Instance.UCMTestApplicationCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testTypeCollection))
				using (CustomsDataRegistry.Instance.EnableUCMTest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
				{
					var testLockProvider = TestLockProvider.CreateDbLockProvider(Db.Connection);
					var engine = new EDIMessageGrEngine(new LoggingInformation(), "_T1", GrEngineServiceSetting.KeyGen, testLockProvider);

					CombineAssertions("Key Gen 1", () =>
					{
						var serviceTask = new UCMServiceTaskKeyGen();
						var logger = new TestServiceLogger();
						serviceTask.ServiceLogger = logger;
						serviceTask.RunTask();
						incomingMessage1.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage1, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK);
						incomingMessage2.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage2, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK);
						incomingMessage3.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage3, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK);
						incomingMessage4.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage4, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK);
						incomingMessage5.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage5, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK);
						incomingMessage6.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage6, GlbBranch.CurrentBranch.PK, ZGuid.Empty, ZString.Empty, EDIMessage.Status.Queued);
						incomingMessage7.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage7, branch1PK, ZGuid.Empty, ZString.Empty, EDIMessage.Status.PreProcessedOK);
						incomingMessage8.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage8, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK);
						incomingMessage9.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage9, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK);
						incomingMessage10.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage10, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK);

						ProcessingManagerTest.AssertContains(logger.ToString().SplitByLine(),
							new[]
							{
							"Calculating Keys for Message",
							}, @"Information|Calculating Keys for Message (_T1-MT1-MN1234A|J1)
Information|Calculating Keys for Message (_T1-MT1-MN1234B|J1)
Information|Calculating Keys for Message (_T1-MT1-MN1234C|J2)
Information|Calculating Keys for Message (_T1-MT1-MN1234D|J3)
Information|Calculating Keys for Message (_T1-MT1-MN1234E|J3)
Information|Calculating Keys for Message (_T1-MT1-MN1234G|J4)
Information|Calculating Keys for Message (_T1-MT1-MN1234E|J3)
Information|Calculating Keys for Message (_T1-MT1-MN1234E|AppRef1|J3)
Information|Calculating Keys for Message (_T1-MT1-MN1234E|MST|AppRef1|J3)
");

						var queueStates = engine.Dequeuer.LoadAll().ToArray();
						AssertEquals("queueStates.Length", 9, queueStates.Length);
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[0], incomingMessage1PK, "MN1234A", new string[] { dec1JE_DeclarationReference, "J1" }, Guid.Empty, QueueStatusCodes.Codes.PreKey, "1 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[1], incomingMessage2PK, "MN1234B", new string[] { dec1JE_DeclarationReference, "J1" }, Guid.Empty, QueueStatusCodes.Codes.PreKey, "2 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[2], incomingMessage3PK, "MN1234C", new string[] { dec1JE_DeclarationReference, "J2" }, Guid.Empty, QueueStatusCodes.Codes.PreKey, "3 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[3], incomingMessage4PK, "MN1234D", new string[] { dec2JE_DeclarationReference, "J3" }, Guid.Empty, QueueStatusCodes.Codes.PreKey, "4 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[4], incomingMessage5PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "J3" }, Guid.Empty, QueueStatusCodes.Codes.PreKey, "5 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[5], incomingMessage7PK, "MN1234G", new string[] { SerializationKeysResult.ForceSerialProcessingKey }, Guid.Empty, QueueStatusCodes.Codes.PreKey, "6 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[6], incomingMessage8PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "J3" }, Guid.Empty, QueueStatusCodes.Codes.PreKey, "7 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[7], incomingMessage9PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "AppRef1", "J3" }, Guid.Empty, QueueStatusCodes.Codes.PreKey, "8 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[8], incomingMessage10PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "AppRef1", "J3" }, Guid.Empty, QueueStatusCodes.Codes.PreKey, "9 ");
					});

					CombineAssertions("Master 1", () =>
					{
						var serviceTask = new UCMServiceTaskMaster();
						var logger = new TestServiceLogger();
						serviceTask.ServiceLogger = logger;
						serviceTask.RunTask();
						incomingMessage1.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage1, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK);
						incomingMessage2.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage2, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK);
						incomingMessage3.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage3, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK);
						incomingMessage4.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage4, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK);
						incomingMessage5.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage5, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK);
						incomingMessage6.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage6, GlbBranch.CurrentBranch.PK, ZGuid.Empty, ZString.Empty, EDIMessage.Status.Queued);
						incomingMessage7.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage7, branch1PK, ZGuid.Empty, ZString.Empty, EDIMessage.Status.PreProcessedOK);
						incomingMessage8.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage8, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK);
						incomingMessage9.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage9, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK);
						incomingMessage10.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage10, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK);

						ProcessingManagerTest.AssertContains(logger.ToString().SplitByLine(),
							new[]
							{
							"Messages loaded",
							"Entities",
							"Loaded messages",
							"Messages at",
							"Oldest message:",
							"Number of chains",
							"Chain Length",
							"Releasing messages for",
							"Could not release any of the",
							"Messages Released:",
							"Deleting ",
							}, @"Information|Releasing messages for _T1.
Information|Entities loaded (9)
Information|Entities added (9)
Debug|Could not release any of the [9] items remaining. Waiting for items to be processed.
Information|Messages Released: 9 Messages Loaded: 9
");

						var queueStates = engine.Dequeuer.LoadAll().ToArray();
						AssertEquals("queueStates.Length", 9, queueStates.Length);
						var chainId1 = queueStates[0].ChainID;
						var chainId2 = queueStates[3].ChainID;
						AssertNotEquals("ChainID1", Guid.Empty, chainId1);
						AssertNotEquals("ChainID2", Guid.Empty, chainId2);
						AssertNotEquals("ChainID1 and ChainID2", chainId1, chainId2);
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[0], incomingMessage1PK, "MN1234A", new string[] { dec1JE_DeclarationReference, "J1" }, chainId1, QueueStatusCodes.Codes.Queued, "1 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[1], incomingMessage2PK, "MN1234B", new string[] { dec1JE_DeclarationReference, "J1" }, chainId1, QueueStatusCodes.Codes.Blocked, "2 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[2], incomingMessage3PK, "MN1234C", new string[] { dec1JE_DeclarationReference, "J2" }, chainId1, QueueStatusCodes.Codes.Blocked, "3 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[3], incomingMessage4PK, "MN1234D", new string[] { dec2JE_DeclarationReference, "J3" }, chainId2, QueueStatusCodes.Codes.Queued, "4 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[4], incomingMessage5PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "J3" }, chainId2, QueueStatusCodes.Codes.Blocked, "5 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[5], incomingMessage7PK, "MN1234G", new string[] { SerializationKeysResult.ForceSerialProcessingKey }, Guid.Empty, QueueStatusCodes.Codes.Queued, "6 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[6], incomingMessage8PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "J3" }, chainId2, QueueStatusCodes.Codes.Blocked, "7 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[7], incomingMessage9PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "AppRef1", "J3" }, chainId2, QueueStatusCodes.Codes.Blocked, "8 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[8], incomingMessage10PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "AppRef1", "J3" }, chainId2, QueueStatusCodes.Codes.Blocked, "9 ");
					});

					var incomingMessage11 = ProcessingManagerTest.CreateMessage(Factory, "_T1", "MT1", "MN1234B", "J1");
					var incomingMessage11PK = incomingMessage11.PK.ToGuid();
					incomingMessage11.EM_SystemCreateTimeUtc = date.AddDays(++count);
					var incomingMessage12 = ProcessingManagerTest.CreateMessage(Factory, "_T1", "MT1", "MN1234E", "J3");
					var incomingMessage12PK = incomingMessage12.PK.ToGuid();
					incomingMessage12.EM_SystemCreateTimeUtc = date.AddDays(++count);
					var incomingMessage13 = ProcessingManagerTest.CreateMessage(Factory, "_T1", "MT1", "MN1234E", "J3");
					incomingMessage13.EM_ApplicationReference = "AppRef1";
					var incomingMessage13PK = incomingMessage13.PK.ToGuid();
					incomingMessage13.EM_SystemCreateTimeUtc = date.AddDays(++count);
					var incomingMessage14 = ProcessingManagerTest.CreateMessage(Factory, "_T1", "MT1", "MN1234E", "J3");
					incomingMessage14.EM_ApplicationReference = "AppRef1";
					incomingMessage14.EM_MessageSubType = "MST";
					var incomingMessage14PK = incomingMessage14.PK.ToGuid();
					incomingMessage14.EM_SystemCreateTimeUtc = date.AddDays(++count);
					Factory.Save();

					CombineAssertions("Master 2", () =>
					{
						var serviceTask = new UCMServiceTaskMaster();
						var logger = new TestServiceLogger();
						serviceTask.ServiceLogger = logger;
						serviceTask.RunTask();
						incomingMessage1.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage1, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "1 ");
						incomingMessage2.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage2, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "2 ");
						incomingMessage3.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage3, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "3 ");
						incomingMessage4.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage4, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "4 ");
						incomingMessage5.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage5, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "5 ");
						incomingMessage6.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage6, GlbBranch.CurrentBranch.PK, ZGuid.Empty, ZString.Empty, EDIMessage.Status.Queued, "6 ");
						incomingMessage7.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage7, branch1PK, ZGuid.Empty, ZString.Empty, EDIMessage.Status.PreProcessedOK, "7 ");
						incomingMessage8.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage8, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "8 ");
						incomingMessage9.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage9, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "9 ");
						incomingMessage10.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage10, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "10 ");
						incomingMessage11.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage11, GlbBranch.CurrentBranch.PK, ZGuid.Empty, ZString.Empty, EDIMessage.Status.Queued, "11 ");
						incomingMessage12.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage12, GlbBranch.CurrentBranch.PK, ZGuid.Empty, ZString.Empty, EDIMessage.Status.Queued, "12 ");
						incomingMessage13.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage13, GlbBranch.CurrentBranch.PK, ZGuid.Empty, ZString.Empty, EDIMessage.Status.Queued, "13 ");
						incomingMessage14.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage14, GlbBranch.CurrentBranch.PK, ZGuid.Empty, ZString.Empty, EDIMessage.Status.Queued, "14 ");

						ProcessingManagerTest.AssertContains(logger.ToString().SplitByLine(),
							new[]
							{
							"Messages loaded",
							"Entities",
							"Loaded messages",
							"Messages at",
							"Oldest message:",
							"Number of chains",
							"Chain Length",
							"Releasing messages for",
							"Could not release any of the",
							"Messages Released:",
							"Deleting ",
							}, @"Information|Releasing messages for _T1.
Information|Entities loaded (9)
Information|Entities added (9)
Debug|Could not release any of the [9] items remaining. Waiting for items to be processed.
Information|Messages Released: 0 Messages Loaded: 9
");

						var queueStates = engine.Dequeuer.LoadAll().ToArray();
						AssertEquals("queueStates.Length", 9, queueStates.Length);
						var chainId1 = queueStates[0].ChainID;
						var chainId2 = queueStates[3].ChainID;
						AssertNotEquals("ChainID1", Guid.Empty, chainId1);
						AssertNotEquals("ChainID2", Guid.Empty, chainId2);
						AssertNotEquals("ChainID1 and ChainID2", chainId1, chainId2);
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[0], incomingMessage1PK, "MN1234A", new string[] { dec1JE_DeclarationReference, "J1" }, chainId1, QueueStatusCodes.Codes.Queued, "1 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[1], incomingMessage2PK, "MN1234B", new string[] { dec1JE_DeclarationReference, "J1" }, chainId1, QueueStatusCodes.Codes.Blocked, "2 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[2], incomingMessage3PK, "MN1234C", new string[] { dec1JE_DeclarationReference, "J2" }, chainId1, QueueStatusCodes.Codes.Blocked, "3 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[3], incomingMessage4PK, "MN1234D", new string[] { dec2JE_DeclarationReference, "J3" }, chainId2, QueueStatusCodes.Codes.Queued, "4 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[4], incomingMessage5PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "J3" }, chainId2, QueueStatusCodes.Codes.Blocked, "5 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[5], incomingMessage7PK, "MN1234G", new string[] { SerializationKeysResult.ForceSerialProcessingKey }, Guid.Empty, QueueStatusCodes.Codes.Queued, "6 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[6], incomingMessage8PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "J3" }, chainId2, QueueStatusCodes.Codes.Blocked, "7 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[7], incomingMessage9PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "AppRef1", "J3" }, chainId2, QueueStatusCodes.Codes.Blocked, "8 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[8], incomingMessage10PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "AppRef1", "J3" }, chainId2, QueueStatusCodes.Codes.Blocked, "9 ");
					});

					CombineAssertions("Key Gen 2", () =>
					{
						var serviceTask = new UCMServiceTaskKeyGen();
						var logger = new TestServiceLogger();
						serviceTask.ServiceLogger = logger;
						serviceTask.RunTask();
						incomingMessage1.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage1, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "1 ");
						incomingMessage2.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage2, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "2 ");
						incomingMessage3.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage3, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "3 ");
						incomingMessage4.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage4, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "4 ");
						incomingMessage5.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage5, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "5 ");
						incomingMessage6.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage6, GlbBranch.CurrentBranch.PK, ZGuid.Empty, ZString.Empty, EDIMessage.Status.Queued, "6 ");
						incomingMessage7.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage7, branch1PK, ZGuid.Empty, ZString.Empty, EDIMessage.Status.PreProcessedOK, "7 ");
						incomingMessage8.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage8, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "8 ");
						incomingMessage9.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage9, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "9 ");
						incomingMessage10.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage10, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "10 ");
						incomingMessage11.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage11, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "11 ");
						incomingMessage12.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage12, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "12 ");
						incomingMessage13.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage13, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "13 ");
						incomingMessage14.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage14, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "14 ");

						ProcessingManagerTest.AssertContains(logger.ToString().SplitByLine(),
							new[]
							{
							"Calculating Keys for Message",
							}, @"Information|Calculating Keys for Message (_T1-MT1-MN1234B|J1)
Information|Calculating Keys for Message (_T1-MT1-MN1234E|J3)
Information|Calculating Keys for Message (_T1-MT1-MN1234E|AppRef1|J3)
Information|Calculating Keys for Message (_T1-MT1-MN1234E|MST|AppRef1|J3)
");

						var queueStates = engine.Dequeuer.LoadAll().ToArray();
						AssertEquals("queueStates.Length", 13, queueStates.Length);
						var chainId1 = queueStates[0].ChainID;
						var chainId2 = queueStates[3].ChainID;
						AssertNotEquals("ChainID1", Guid.Empty, chainId1);
						AssertNotEquals("ChainID2", Guid.Empty, chainId2);
						AssertNotEquals("ChainID1 and ChainID2", chainId1, chainId2);
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[0], incomingMessage1PK, "MN1234A", new string[] { dec1JE_DeclarationReference, "J1" }, chainId1, QueueStatusCodes.Codes.Queued, "1 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[1], incomingMessage2PK, "MN1234B", new string[] { dec1JE_DeclarationReference, "J1" }, chainId1, QueueStatusCodes.Codes.Blocked, "2 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[2], incomingMessage3PK, "MN1234C", new string[] { dec1JE_DeclarationReference, "J2" }, chainId1, QueueStatusCodes.Codes.Blocked, "3 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[3], incomingMessage4PK, "MN1234D", new string[] { dec2JE_DeclarationReference, "J3" }, chainId2, QueueStatusCodes.Codes.Queued, "4 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[4], incomingMessage5PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "J3" }, chainId2, QueueStatusCodes.Codes.Blocked, "5 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[5], incomingMessage7PK, "MN1234G", new string[] { SerializationKeysResult.ForceSerialProcessingKey }, Guid.Empty, QueueStatusCodes.Codes.Queued, "6 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[6], incomingMessage8PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "J3" }, chainId2, QueueStatusCodes.Codes.Blocked, "7 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[7], incomingMessage9PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "AppRef1", "J3" }, chainId2, QueueStatusCodes.Codes.Blocked, "8 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[8], incomingMessage10PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "AppRef1", "J3" }, chainId2, QueueStatusCodes.Codes.Blocked, "9 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[9], incomingMessage11PK, "MN1234B", new string[] { dec1JE_DeclarationReference, "J1" }, Guid.Empty, QueueStatusCodes.Codes.PreKey, "10 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[10], incomingMessage12PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "J3" }, Guid.Empty, QueueStatusCodes.Codes.PreKey, "11 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[11], incomingMessage13PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "AppRef1", "J3" }, Guid.Empty, QueueStatusCodes.Codes.PreKey, "12 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[12], incomingMessage14PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "AppRef1", "J3" }, Guid.Empty, QueueStatusCodes.Codes.PreKey, "13 ");
					});

					CombineAssertions("Master 3", () =>
					{
						var serviceTask = new UCMServiceTaskMaster();
						var logger = new TestServiceLogger();
						serviceTask.ServiceLogger = logger;
						serviceTask.RunTask();
						incomingMessage1.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage1, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "1 ");
						incomingMessage2.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage2, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "2 ");
						incomingMessage3.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage3, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "3 ");
						incomingMessage4.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage4, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "4 ");
						incomingMessage5.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage5, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "5 ");
						incomingMessage6.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage6, GlbBranch.CurrentBranch.PK, ZGuid.Empty, ZString.Empty, EDIMessage.Status.Queued, "6 ");
						incomingMessage7.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage7, branch1PK, ZGuid.Empty, ZString.Empty, EDIMessage.Status.PreProcessedOK, "7 ");
						incomingMessage8.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage8, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "8 ");
						incomingMessage9.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage9, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "9 ");
						incomingMessage10.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage10, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "10 ");
						incomingMessage11.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage11, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "11 ");
						incomingMessage12.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage12, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "12 ");
						incomingMessage13.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage13, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "13 ");
						incomingMessage14.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage14, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.PreProcessedOK, "14 ");

						ProcessingManagerTest.AssertContains(logger.ToString().SplitByLine(),
							new[]
							{
							"Messages loaded",
							"Entities",
							"Loaded messages",
							"Messages at",
							"Oldest message:",
							"Number of chains",
							"Chain Length",
							"Releasing messages for",
							"Could not release any of the",
							"Messages Released:",
							"Deleting ",
							}, @"Information|Releasing messages for _T1.
Information|Entities loaded (9)
Information|Entities added (9)
Information|Entities loaded (4)
Information|Entities added (4)
Debug|Could not release any of the [13] items remaining. Waiting for items to be processed.
Information|Messages Released: 4 Messages Loaded: 13
");

						var queueStates = engine.Dequeuer.LoadAll().ToArray();
						AssertEquals("queueStates.Length", 13, queueStates.Length);
						var chainId1 = queueStates[0].ChainID;
						var chainId2 = queueStates[3].ChainID;
						AssertNotEquals("ChainID1", Guid.Empty, chainId1);
						AssertNotEquals("ChainID2", Guid.Empty, chainId2);
						AssertNotEquals("ChainID1 and ChainID2", chainId1, chainId2);
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[0], incomingMessage1PK, "MN1234A", new string[] { dec1JE_DeclarationReference, "J1" }, chainId1, QueueStatusCodes.Codes.Queued, "1 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[1], incomingMessage2PK, "MN1234B", new string[] { dec1JE_DeclarationReference, "J1" }, chainId1, QueueStatusCodes.Codes.Blocked, "2 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[2], incomingMessage3PK, "MN1234C", new string[] { dec1JE_DeclarationReference, "J2" }, chainId1, QueueStatusCodes.Codes.Blocked, "3 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[3], incomingMessage4PK, "MN1234D", new string[] { dec2JE_DeclarationReference, "J3" }, chainId2, QueueStatusCodes.Codes.Queued, "4 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[4], incomingMessage5PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "J3" }, chainId2, QueueStatusCodes.Codes.Blocked, "5 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[5], incomingMessage7PK, "MN1234G", new string[] { SerializationKeysResult.ForceSerialProcessingKey }, Guid.Empty, QueueStatusCodes.Codes.Queued, "6 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[6], incomingMessage8PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "J3" }, chainId2, QueueStatusCodes.Codes.Blocked, "7 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[7], incomingMessage9PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "AppRef1", "J3" }, chainId2, QueueStatusCodes.Codes.Blocked, "8 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[8], incomingMessage10PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "AppRef1", "J3" }, chainId2, QueueStatusCodes.Codes.Blocked, "9 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[9], incomingMessage11PK, "MN1234B", new string[] { dec1JE_DeclarationReference, "J1" }, chainId1, QueueStatusCodes.Codes.Blocked, "10 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[10], incomingMessage12PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "J3" }, chainId2, QueueStatusCodes.Codes.Blocked, "11 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[11], incomingMessage13PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "AppRef1", "J3" }, chainId2, QueueStatusCodes.Codes.Blocked, "12 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[12], incomingMessage14PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "AppRef1", "J3" }, chainId2, QueueStatusCodes.Codes.Blocked, "13 ");
					});

					CombineAssertions("Worker 1", () =>
					{
						var logger = InitialiseAndRunTaskSchedule(new UCMServiceTaskWorker());
						incomingMessage1.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage1, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "1 ");
						incomingMessage2.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage2, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "2 ");
						incomingMessage3.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage3, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "3 ");
						incomingMessage4.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage4, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "4 ");
						incomingMessage5.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage5, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "5 ");
						incomingMessage6.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage6, GlbBranch.CurrentBranch.PK, ZGuid.Empty, ZString.Empty, EDIMessage.Status.Queued, "6 ");
						incomingMessage7.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage7, branch1PK, ZGuid.Empty, ZString.Empty, EDIMessage.Status.Received, "7 ");
						incomingMessage8.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage8, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "8 ");
						incomingMessage9.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage9, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "9 ");
						incomingMessage10.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage10, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "10 ");
						incomingMessage11.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage11, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "11 ");
						incomingMessage12.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage12, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "12 ");
						incomingMessage13.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage13, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "13 ");
						incomingMessage14.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage14, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "14 ");

						var queueStates = engine.Dequeuer.LoadAll().ToArray();
						AssertEquals("queueStates.Length", 13, queueStates.Length);
						var chainId1 = queueStates[0].ChainID;
						var chainId2 = queueStates[3].ChainID;
						AssertNotEquals("ChainID1", Guid.Empty, chainId1);
						AssertNotEquals("ChainID2", Guid.Empty, chainId2);
						AssertNotEquals("ChainID1 and ChainID2", chainId1, chainId2);
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[0], incomingMessage1PK, "MN1234A", new string[] { dec1JE_DeclarationReference, "J1" }, chainId1, QueueStatusCodes.Codes.Notify, "1 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[1], incomingMessage2PK, "MN1234B", new string[] { dec1JE_DeclarationReference, "J1" }, chainId1, QueueStatusCodes.Codes.Notify, "2 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[2], incomingMessage3PK, "MN1234C", new string[] { dec1JE_DeclarationReference, "J2" }, chainId1, QueueStatusCodes.Codes.Notify, "3 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[3], incomingMessage4PK, "MN1234D", new string[] { dec2JE_DeclarationReference, "J3" }, chainId2, QueueStatusCodes.Codes.Notify, "4 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[4], incomingMessage5PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "J3" }, chainId2, QueueStatusCodes.Codes.Notify, "5 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[5], incomingMessage7PK, "MN1234G", new string[] { SerializationKeysResult.ForceSerialProcessingKey }, Guid.Empty, QueueStatusCodes.Codes.Notify, "6 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[6], incomingMessage8PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "J3" }, chainId2, QueueStatusCodes.Codes.Notify, "7 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[7], incomingMessage9PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "AppRef1", "J3" }, chainId2, QueueStatusCodes.Codes.Notify, "8 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[8], incomingMessage10PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "AppRef1", "J3" }, chainId2, QueueStatusCodes.Codes.Notify, "9 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[9], incomingMessage11PK, "MN1234B", new string[] { dec1JE_DeclarationReference, "J1" }, chainId1, QueueStatusCodes.Codes.Notify, "10 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[10], incomingMessage12PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "J3" }, chainId2, QueueStatusCodes.Codes.Notify, "11 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[11], incomingMessage13PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "AppRef1", "J3" }, chainId2, QueueStatusCodes.Codes.Notify, "12 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[12], incomingMessage14PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "AppRef1", "J3" }, chainId2, QueueStatusCodes.Codes.Notify, "13 ");
					});

					CombineAssertions("Worker 2", () =>
					{
						var logger = InitialiseAndRunTaskSchedule(new UCMServiceTaskWorker());
						incomingMessage1.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage1, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "1 ");
						incomingMessage2.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage2, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "2 ");
						incomingMessage3.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage3, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "3 ");
						incomingMessage4.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage4, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "4 ");
						incomingMessage5.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage5, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "5 ");
						incomingMessage6.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage6, GlbBranch.CurrentBranch.PK, ZGuid.Empty, ZString.Empty, EDIMessage.Status.Queued, "6 ");
						incomingMessage7.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage7, branch1PK, ZGuid.Empty, ZString.Empty, EDIMessage.Status.Received, "7 ");
						incomingMessage8.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage8, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "8 ");
						incomingMessage9.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage9, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "9 ");
						incomingMessage10.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage10, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "10 ");
						incomingMessage11.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage11, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "11 ");
						incomingMessage12.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage12, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "12 ");
						incomingMessage13.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage13, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "13 ");
						incomingMessage14.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage14, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "14 ");

						var queueStates = engine.Dequeuer.LoadAll().ToArray();
						AssertEquals("queueStates.Length", 13, queueStates.Length);
						var chainId1 = queueStates[0].ChainID;
						var chainId2 = queueStates[3].ChainID;
						AssertNotEquals("ChainID1", Guid.Empty, chainId1);
						AssertNotEquals("ChainID2", Guid.Empty, chainId2);
						AssertNotEquals("ChainID1 and ChainID2", chainId1, chainId2);
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[0], incomingMessage1PK, "MN1234A", new string[] { dec1JE_DeclarationReference, "J1" }, chainId1, QueueStatusCodes.Codes.Notify, "1 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[1], incomingMessage2PK, "MN1234B", new string[] { dec1JE_DeclarationReference, "J1" }, chainId1, QueueStatusCodes.Codes.Notify, "2 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[2], incomingMessage3PK, "MN1234C", new string[] { dec1JE_DeclarationReference, "J2" }, chainId1, QueueStatusCodes.Codes.Notify, "3 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[3], incomingMessage4PK, "MN1234D", new string[] { dec2JE_DeclarationReference, "J3" }, chainId2, QueueStatusCodes.Codes.Notify, "4 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[4], incomingMessage5PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "J3" }, chainId2, QueueStatusCodes.Codes.Notify, "5 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[5], incomingMessage7PK, "MN1234G", new string[] { SerializationKeysResult.ForceSerialProcessingKey }, Guid.Empty, QueueStatusCodes.Codes.Notify, "6 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[6], incomingMessage8PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "J3" }, chainId2, QueueStatusCodes.Codes.Notify, "7 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[7], incomingMessage9PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "AppRef1", "J3" }, chainId2, QueueStatusCodes.Codes.Notify, "8 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[8], incomingMessage10PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "AppRef1", "J3" }, chainId2, QueueStatusCodes.Codes.Notify, "9 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[9], incomingMessage11PK, "MN1234B", new string[] { dec1JE_DeclarationReference, "J1" }, chainId1, QueueStatusCodes.Codes.Notify, "10 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[10], incomingMessage12PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "J3" }, chainId2, QueueStatusCodes.Codes.Notify, "11 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[11], incomingMessage13PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "AppRef1", "J3" }, chainId2, QueueStatusCodes.Codes.Notify, "12 ");
						ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[12], incomingMessage14PK, "MN1234E", new string[] { dec2JE_DeclarationReference, "AppRef1", "J3" }, chainId2, QueueStatusCodes.Codes.Notify, "13 ");
					});

					CombineAssertions("Master 4", () =>
					{
						var serviceTask = new UCMServiceTaskMaster();
						var logger = new TestServiceLogger();
						serviceTask.ServiceLogger = logger;
						serviceTask.RunTask();
						incomingMessage1.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage1, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "1 ");
						incomingMessage2.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage2, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "2 ");
						incomingMessage3.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage3, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "3 ");
						incomingMessage4.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage4, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "4 ");
						incomingMessage5.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage5, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "5 ");
						incomingMessage6.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage6, GlbBranch.CurrentBranch.PK, ZGuid.Empty, ZString.Empty, EDIMessage.Status.Queued, "6 ");
						incomingMessage7.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage7, branch1PK, ZGuid.Empty, ZString.Empty, EDIMessage.Status.Received, "7 ");
						incomingMessage8.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage8, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "8 ");
						incomingMessage9.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage9, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "9 ");
						incomingMessage10.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage10, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "10 ");
						incomingMessage11.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage11, branch1PK, dec1PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "11 ");
						incomingMessage12.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage12, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "12 ");
						incomingMessage13.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage13, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "13 ");
						incomingMessage14.Reload();
						ProcessingManagerTest.AssertMessage(incomingMessage14, branch2PK, dec2PK, JobDeclarationSchema.Constants.TableName, EDIMessage.Status.Received, "14 ");

						ProcessingManagerTest.AssertContains(logger.ToString().SplitByLine(),
							new[]
							{
							"Messages loaded",
							"Entities",
							"Loaded messages",
							"Messages at",
							"Oldest message:",
							"Number of chains",
							"Chain Length",
							"Releasing messages for",
							"Could not release any of the",
							"Messages Released:",
							"Deleting ",
							}, @"Information|Releasing messages for _T1.
Information|Entities loaded (13)
Information|Entities removed (13)
Information|Messages Released: 13 Messages Loaded: 0
");

						var queueStates = engine.Dequeuer.LoadAll().ToArray();
						AssertEquals("queueStates.Length", 13, queueStates.Length);
					});

					CombineAssertions("Master 5", () =>
					{
						using var command = Db.Connection.Command("UPDATE dbo.EDIMessageQueueState SET EQS_SystemCreateTimeUtc = '2024-01-01', EQS_SystemLastEditTimeUtc = '2024-01-01', EQS_SystemLastEditUser = '~BP' WHERE EQS_ApplicationCode = '_T1'");
						command.ExecuteNonQuery();
						var queueStateFactory = new EDIMessageQueueStateFactory(new LoggingInformation(), "_T1", () => new GrEngineLogOptions(), 1);
						var serviceTask = new UCMServiceTaskMaster();
						var logger = new TestServiceLogger();
						serviceTask.ServiceLogger = logger;
						serviceTask.RunTask();

						ProcessingManagerTest.AssertContains(logger.ToString().SplitByLine(),
							new[]
							{
							"Messages loaded",
							"Entities",
							"Loaded messages",
							"Messages at",
							"Oldest message:",
							"Number of chains",
							"Chain Length",
							"Releasing messages for",
							"Could not release any of the",
							"Messages Released:",
							"Deleting ",
							}, @"Information|Releasing messages for _T1.
Information|Deleting 13 processed rows from 1 hours ago for _T1.
Information|Messages Released: 0 Messages Loaded: 0
");

						var queueStates = queueStateFactory.LoadAll(100).ToArray();
						AssertEquals("queueStates.Length", 0, queueStates.Length);
					});
				}
			}
		}

		public void TestOptions()
		{
			AssertEquals("1minute", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		public void TestHasUniversalCustomsMessagingSubscribers()
		{
			var methodInfo = typeof(UCMServiceTaskWorker).GetMethod(nameof(UCMServiceTaskWorker.HasUniversalCustomsMessagingSubscribers), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
			Assert("[HostedServiceRequirement] is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));

			using (new UCMPProcessorsRegistrationSubstitute())
			{
				AssertEquals("UCMServiceTaskWorker.HasUniversalCustomsMessagingSubscribers()", "There is no Customs message subscribed to Universal Customs Message Processing", UCMServiceTaskWorker.HasUniversalCustomsMessagingSubscribers());
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
