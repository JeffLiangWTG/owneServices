using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils.Tests;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.GraphEngine.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Scheduler.GraphEngine;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	[TestedType(typeof(UCMServiceTaskKeyGen))]
	class UCMServiceTaskKeyGenTest : ServiceTaskTestCase<UCMServiceTaskKeyGen>
	{
		public void TestCorrectlySetupInUniversalCustomsMessageProcessors()
		{
			AssertNoExceptionThrown(() => InitialiseAndRunTaskSchedule(new UCMServiceTaskKeyGen()));
		}

		public void TestEndToEnd()
		{
			var branch1 = ProcessingManagerTest.SetupBranch(Factory, Core.Constants.CountryCodes.NewZealand);
			var branch2 = branch1.Company.Branches.AddNew();
			branch2.GB_Code = "N@%";
			branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var dec1 = Factory.New<BaseJobDeclaration>();
			dec1.JE_GB = branch1.PK;
			var dec2 = Factory.New<BaseJobDeclaration>();
			dec2.JE_GB = branch2.PK;
			var message1 = ProcessingManagerTest.CreateMessage(Factory, "A@#", "MT1", "MN1234A", "J1");
			var message2 = ProcessingManagerTest.CreateMessage(Factory, "A@#", "MT1", "MN1234B", "J1");
			message2.EM_MessageSubType = "KD3";
			var message3 = ProcessingManagerTest.CreateMessage(Factory, "A@#", "MT1", "MN1234C", "J2");
			var message4 = ProcessingManagerTest.CreateMessage(Factory, "A@#", "MT1", "MN1234D", "J2");
			var message5 = ProcessingManagerTest.CreateMessage(Factory, "A@#", "MT1", "MN1234E", "J1");
			var message6 = ProcessingManagerTest.CreateMessage(Factory, "A&#", "MT1", "MN1234F", "J1");
			Factory.Save();

			var messageProcessor = new UniversalCustomsMessageProcessorTestClass();
			messageProcessor.GetLinkedBusinessObjectMetaDataForTesting = (message, _) =>
			{
				var dec = message.EM_MessageOwner == "J1" ? dec1 : dec2;
				return new LinkedBusinessObjectMetaData(dec.TableName, dec.PK, dec.JE_GB, dec.JE_DeclarationReference);
			};

			messageProcessor.GetBranchForTesting = (_, _, linkedBusinessObjectBranchPk) => linkedBusinessObjectBranchPk;

			messageProcessor.GetSerializationKeysResultForTesting = (message, _, linkedBusinessObjectMetaData) =>
			{
				var keys = new HashSet<string> { linkedBusinessObjectMetaData.JobNumber };
				if (message.EM_MessageSubType == "KD3")
				{
					keys.Add("EXT");
				}

				return new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, keys);
			};

			using (new UCMPProcessorsRegistrationSubstitute(("A@#",messageProcessor)))
			{
				var logger = InitialiseAndRunTaskSchedule(new UCMServiceTaskKeyGen());
				var testLockProvider = TestLockProvider.CreateDbLockProvider(Db.Connection);
				var engine = new EDIMessageGrEngine(new LoggingInformation(), "A@#", GrEngineServiceSetting.KeyGen, testLockProvider);

				message1.Reload();
				ProcessingManagerTest.AssertMessage(message1, dec1.JE_GB, dec1.PK, dec1.TableName, EDIMessage.Status.PreProcessedOK);
				message2.Reload();
				ProcessingManagerTest.AssertMessage(message2, dec1.JE_GB, dec1.PK, dec1.TableName, EDIMessage.Status.PreProcessedOK);
				message3.Reload();
				ProcessingManagerTest.AssertMessage(message3, dec2.JE_GB, dec2.PK, dec2.TableName, EDIMessage.Status.PreProcessedOK);
				message4.Reload();
				ProcessingManagerTest.AssertMessage(message4, dec2.JE_GB, dec2.PK, dec2.TableName, EDIMessage.Status.PreProcessedOK);
				message5.Reload();
				ProcessingManagerTest.AssertMessage(message5, dec1.JE_GB, dec1.PK, dec1.TableName, EDIMessage.Status.PreProcessedOK);
				message6.Reload();
				ProcessingManagerTest.AssertMessage(message6, GlbBranch.CurrentBranch.PK, ZGuid.Empty, ZString.Empty, EDIMessage.Status.Queued);

				ProcessingManagerTest.AssertContains(logger.ToString().SplitByLine(),
					new[]
					{
						"Calculating Keys for Message",
					}, @"Information|Calculating Keys for Message (A@#-MT1-MN1234A|J1)
Information|Calculating Keys for Message (A@#-MT1-MN1234B|KD3|J1)
Information|Calculating Keys for Message (A@#-MT1-MN1234C|J2)
Information|Calculating Keys for Message (A@#-MT1-MN1234D|J2)
Information|Calculating Keys for Message (A@#-MT1-MN1234E|J1)
");

				var queueStates = engine.Dequeuer.LoadAll().ToArray();
				AssertEquals("queueStates.Length", 5, queueStates.Length);
				ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[0], message1.PK.ToGuid(), "MN1234A", new string[] { dec1.JE_DeclarationReference }, Guid.Empty, QueueStatusCodes.Codes.PreKey);
				ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[1], message2.PK.ToGuid(), "MN1234B", new string[] { dec1.JE_DeclarationReference, "EXT" }, Guid.Empty, QueueStatusCodes.Codes.PreKey);
				ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[2], message3.PK.ToGuid(), "MN1234C", new string[] { dec2.JE_DeclarationReference }, Guid.Empty, QueueStatusCodes.Codes.PreKey);
				ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[3], message4.PK.ToGuid(), "MN1234D", new string[] { dec2.JE_DeclarationReference }, Guid.Empty, QueueStatusCodes.Codes.PreKey);
				ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[4], message5.PK.ToGuid(), "MN1234E", new string[] { dec1.JE_DeclarationReference }, Guid.Empty, QueueStatusCodes.Codes.PreKey);
			}
		}

		public TestServiceLogger InitialiseAndRunTaskSchedule() => InitialiseAndRunTaskSchedule(new UCMServiceTaskKeyGen());

		public void TestOptions()
		{
			AssertEquals("1minute", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		public void TestHasUniversalCustomsMessagingSubscribers()
		{
			var methodInfo = typeof(UCMServiceTaskKeyGen).GetMethod(nameof(UCMServiceTaskKeyGen.HasUniversalCustomsMessagingSubscribers), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
			Assert("[HostedServiceRequirement] is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));

			using (new UCMPProcessorsRegistrationSubstitute())
			{
				AssertEquals("UCMServiceTaskKeyGen.HasUniversalCustomsMessagingSubscribers()", "There is no Customs message subscribed to Universal Customs Message Processing", UCMServiceTaskKeyGen.HasUniversalCustomsMessagingSubscribers());
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
