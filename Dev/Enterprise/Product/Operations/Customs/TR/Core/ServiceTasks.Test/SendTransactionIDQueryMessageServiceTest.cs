using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Customs.TR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.TR.ServiceTasks.Testing
{
	[TestedType(typeof(SendTransactionIDQueryMessageService))]
	public class SendTransactionIDQueryMessageServiceTest : ServiceTaskTestCase<SendTransactionIDQueryMessageService>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "ASQ", hostedServiceAttribute.Code);
				AssertEquals("Description", "TR Auto-Send Query Message Service", hostedServiceAttribute.Description);
				AssertEquals("Category", "TRC", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "1Minute", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Turkey, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		public void TestHostedServiceRequirement()
		{
			var methodInfo = typeof(SendTransactionIDQueryMessageService).GetMethod(nameof(SendTransactionIDQueryMessageService.IsRequired));
			Assert("HostedServiceRequirement is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));

			CertificateRequirementChecker.ResetForTesting();
			AssertEquals("There is no Certificate configured in Turkey.", SendTransactionIDQueryMessageService.IsRequired());
			GlbExternalPasswordHelperTest.SetupGlbExternalPassword_TRK(Factory);
			CertificateRequirementChecker.ResetForTesting();
			AssertEquals(string.Empty, SendTransactionIDQueryMessageService.IsRequired());
		}

		public void TestSendTransactionIDQueryMessageServiceQueue()
		{
			var queueProvider = new SendTransactionIDQueryMessageServiceQueue() as IHostedServiceQueueProvider;
			var initialSize = queueProvider.QueueResult.QueueSize;
			var itemDaysOld = 3;

			var pollingTransaction1 = Factory.NewWithValidTestData<CusPollingTransaction>();
			pollingTransaction1.CPT_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			pollingTransaction1.CPT_Type = TRMessageTypes.Codes.TRO;
			pollingTransaction1.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN;
			pollingTransaction1.CPT_NumberOfAttempts = 5;
			pollingTransaction1.CPT_EarliestTimeOfNextAttemptUtc = ZDateTime.UtcNow.AddDays(-itemDaysOld);

			var pollingTransaction2 = Factory.NewWithValidTestData<CusPollingTransaction>();
			pollingTransaction2.CPT_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			pollingTransaction2.CPT_Type = TRMessageTypes.Codes.TRO;
			pollingTransaction2.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND;
			pollingTransaction2.CPT_NumberOfAttempts = 5;
			pollingTransaction2.CPT_EarliestTimeOfNextAttemptUtc = ZDateTime.UtcNow;

			var pollingTransaction3 = Factory.NewWithValidTestData<CusPollingTransaction>();
			pollingTransaction3.CPT_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			pollingTransaction3.CPT_Type = TRMessageTypes.Codes.TRO;
			pollingTransaction3.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN;
			pollingTransaction3.CPT_NumberOfAttempts = 5;
			pollingTransaction3.CPT_EarliestTimeOfNextAttemptUtc = ZDateTime.UtcNow.AddMinutes(5);

			var pollingTransaction4 = Factory.NewWithValidTestData<CusPollingTransaction>();
			pollingTransaction4.CPT_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			pollingTransaction4.CPT_Type = TRMessageTypes.Codes.TRE;
			pollingTransaction4.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN;
			pollingTransaction4.CPT_NumberOfAttempts = 5;
			pollingTransaction4.CPT_EarliestTimeOfNextAttemptUtc = ZDateTime.UtcNow;

			var pollingTransaction5 = Factory.NewWithValidTestData<CusPollingTransaction>();
			pollingTransaction5.CPT_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			pollingTransaction5.CPT_Type = TRMessageTypes.Codes.TRN;
			pollingTransaction5.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN;
			pollingTransaction5.CPT_NumberOfAttempts = 5;
			pollingTransaction5.CPT_EarliestTimeOfNextAttemptUtc = ZDateTime.UtcNow;

			var pollingTransaction6 = Factory.NewWithValidTestData<CusPollingTransaction>();
			pollingTransaction6.CPT_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			pollingTransaction6.CPT_Type = TRMessageTypes.Codes.T1N;
			pollingTransaction6.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN;
			pollingTransaction6.CPT_NumberOfAttempts = 5;
			pollingTransaction6.CPT_EarliestTimeOfNextAttemptUtc = ZDateTime.UtcNow;
			Factory.Save();

			AssertEquals("There should be one CusPollingTransaction added to the queue", 4, queueProvider.QueueResult.QueueSize - initialSize);
			NUnit.Framework.Assert.That((int)queueProvider.QueueResult.MaximumItemAge.TotalSeconds, NUnit.Framework.Is.EqualTo((int)TimeSpan.FromDays(itemDaysOld).TotalSeconds).Within(60));
		}

		[TestDate(2022, 04, 01, 12, 0, 0)]
		public void TestAutoSendT1OQueryMessageForTRO()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var trackingID = new ZGuid("AF48DDBA-4CBA-44EF-B64A-CC27D240B9F8");
				var queryGUID = "4142285b-6b4f-4eb8-9bfa-6baa3b884b06";
				var manifest = Factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
				manifest.AMA_RN_NKCountry = "TR";
				manifest.AMA_ManifestType = "DENITH";
				var requestInterchange = CreateInterchange(TRMessageTypes.Codes.TRO, EDIInterchange.Status.Sent, EDIInterchange.Direction.Transmit, trackingID, "Request message");
				var requestMessage = CreateMessage(TRMessageTypes.Codes.TRO, EDIMessage.Status.Sent, EDIMessage.Direction.Transmit, requestInterchange.PK, AsycudaManifestHeaderSchema.Constants.TableName, manifest.PK, "Request message");
				var responseInterchange = CreateInterchange(TRMessageTypes.Codes.TRO, EDIInterchange.Status.Received, EDIInterchange.Direction.Receive, trackingID, "Response message");
				var responseMessage = CreateMessage(TRMessageTypes.Codes.TRO, EDIMessage.Status.Received, EDIMessage.Direction.Receive, responseInterchange.PK, AsycudaManifestHeaderSchema.Constants.TableName, manifest.PK, "Response message");
				manifest.Messages.Clear();

				var pollingTransaction = Factory.New<CusPollingTransaction>();
				pollingTransaction.CPT_ApplicationCode = Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.TRCustoms;
				pollingTransaction.CPT_Type = TRMessageTypes.Codes.TRO;
				pollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN;
				pollingTransaction.CPT_NumberOfAttempts = 5;
				pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc = new ZDateTime(2022, 4, 1, 15, 0, 0);
				pollingTransaction.CPT_TransactionID = queryGUID;
				pollingTransaction.CPT_ParentID = responseMessage.PK;
				Factory.Save();

				var task = new SendTransactionIDQueryMessageService_ForTest();
				var logger = task.GetLogger();
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);

				CombineAssertions(() =>
				{
					AssertEquals("There should be no T1O message created", 0, manifest.Messages.Find(new ZQuery(EDIMessageSchema.EM_MessageType, "T1O")).Length);
					Assert("Information should be logged", logger.Logs.Any(l => l.Message.Contains("No polling transaction record needs to be processed") && l.Type == Integration.LogType.Information));
				});

				pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc = ZDateTime.UtcNow;
				Factory.Save();
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);

				var newFactory = new BusinessObjectFactory();
				manifest = newFactory.Load<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>(manifest.PK);
				pollingTransaction = newFactory.Load<CusPollingTransaction>(pollingTransaction.PK);
				CombineAssertions(() =>
				{
					AssertEquals("There should be one T1O message created", 1, manifest.Messages.Find(new ZQuery(EDIMessageSchema.EM_MessageType, "T1O")).Length);
					AssertEquals(Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND, pollingTransaction.CPT_Status);
					AssertEquals("Pending", pollingTransaction.CPT_StatusReason);
					Assert("Information should be logged", logger.Logs.Any(l => l.Message.Contains($"Polling transaction processed, TransactionID: {pollingTransaction.CPT_TransactionID}. Message sent successfully.") && l.Type == Integration.LogType.Information));
				});
			}
		}

		public void TestAutoSendT1EQueryMessageForTRE()
		{
			var trackingID = new ZGuid("AF48DDBA-4CBA-44EF-B64A-CC27D240B9F8");
			var queryGUID = "4142285b-6b4f-4eb8-9bfa-6baa3b884b06";
			var eTradeHeader = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			var requestInterchange = CreateInterchange(TRMessageTypes.Codes.TRE, EDIInterchange.Status.Sent, EDIInterchange.Direction.Transmit, trackingID, "Request message");
			var requestMessage = CreateMessage(TRMessageTypes.Codes.TRE, EDIMessage.Status.Sent, EDIMessage.Direction.Transmit, requestInterchange.PK, AsycudaManifestHeaderSchema.Constants.TableName, eTradeHeader.PK, "Request message");
			var responseInterchange = CreateInterchange(TRMessageTypes.Codes.TRE, EDIInterchange.Status.Received, EDIInterchange.Direction.Receive, trackingID, "Response message");
			var responseMessage = CreateMessage(TRMessageTypes.Codes.TRE, EDIMessage.Status.Received, EDIMessage.Direction.Receive, responseInterchange.PK, AsycudaManifestHeaderSchema.Constants.TableName, eTradeHeader.PK, "Response message");

			var pollingTransaction = Factory.New<CusPollingTransaction>();
			pollingTransaction.CPT_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			pollingTransaction.CPT_Type = TRMessageTypes.Codes.TRE;
			pollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN;
			pollingTransaction.CPT_NumberOfAttempts = 5;
			pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc = ZDateTime.UtcNow;
			pollingTransaction.CPT_TransactionID = queryGUID;
			pollingTransaction.CPT_ParentID = responseMessage.PK;
			Factory.Save();

			var task = new SendTransactionIDQueryMessageService();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);

			var newFactory = new BusinessObjectFactory();
			var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, eTradeHeader.PK);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, TRMessageTypes.Codes.T1E);
			var messages = newFactory.Load<EDIMessage>(query);

			CombineAssertions("A TRC T1E TRX QUE message should be created", () =>
			{
				AssertEquals("Count", 1, messages.Length);
				var message = messages[0];
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.TRCustoms, message.EM_ApplicationCode);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
			});
		}

		[TestDate(2022, 05, 01, 12, 0, 0)]
		public void TestAutoSendT1NQueryMessageForTRN()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var trackingID = new ZGuid("AF48DDBA-4CBA-44EF-B64A-CC27D240B9F8");
				var nctsHeader = Factory.New<Integration.Customs.TR.ICusInBondHeader>();
				var requestInterchange = CreateInterchange(TRMessageTypes.Codes.TRN, EDIInterchange.Status.Sent, EDIInterchange.Direction.Transmit, trackingID, "Request message");
				var requestMessage = CreateMessage(TRMessageTypes.Codes.TRN, EDIMessage.Status.Sent, EDIMessage.Direction.Transmit, requestInterchange.PK, CusInBondHeaderSchema.Constants.TableName, nctsHeader.PK, "Request message");
				var responseInterchange = CreateInterchange(TRMessageTypes.Codes.TRN, EDIInterchange.Status.Received, EDIInterchange.Direction.Receive, trackingID, "Response message");
				var responseMessage = CreateMessage(TRMessageTypes.Codes.TRN, EDIMessage.Status.Received, EDIMessage.Direction.Receive, responseInterchange.PK, CusInBondHeaderSchema.Constants.TableName, nctsHeader.PK, "Response message");

				var pollingTransaction = Factory.New<CusPollingTransaction>();
				pollingTransaction.CPT_ApplicationCode = Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.TRCustoms;
				pollingTransaction.CPT_Type = TRMessageTypes.Codes.TRN;
				pollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN;
				pollingTransaction.CPT_NumberOfAttempts = 5;
				pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc = ZDateTime.UtcNow;
				pollingTransaction.CPT_TransactionID = "5082D1A82A01C3A6E0536803A8C0E158";
				pollingTransaction.CPT_ParentID = responseMessage.PK;
				Factory.Save();

				var task = new SendTransactionIDQueryMessageService_ForTest();
				var logger = task.GetLogger();
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);

				pollingTransaction.Reload();
				CombineAssertions(() =>
				{
					AssertEquals("There should be one T1N message created", 1, nctsHeader.Messages.Find(new ZQuery(EDIMessageSchema.EM_MessageType, TRMessageTypes.Codes.T1N)).Length);
					AssertEquals(Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND, pollingTransaction.CPT_Status);
					AssertEquals("Pending", pollingTransaction.CPT_StatusReason);
					Assert("Information should be logged", logger.Logs.Any(l => l.Message.Contains($"Polling transaction processed, TransactionID: {pollingTransaction.CPT_TransactionID}. Message sent successfully.") && l.Type == Integration.LogType.Information));
				});
			}
		}

		public void TestAutoSendT2NQueryMessageForT1N()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var trackingID = new ZGuid("AF48DDBA-4CBA-44EF-B64A-CC27D240B9F8");
				var nctsHeader = Factory.New<Integration.Customs.TR.ICusInBondHeader>();

				var requestInterchangeTRN = CreateInterchange(TRMessageTypes.Codes.TRN, EDIInterchange.Status.Sent, EDIInterchange.Direction.Transmit, trackingID, "Request message");
				CreateMessage(TRMessageTypes.Codes.TRN, EDIMessage.Status.Sent, EDIMessage.Direction.Transmit, requestInterchangeTRN.PK, CusInBondHeaderSchema.Constants.TableName, nctsHeader.PK, "Request message");

				var requestInterchange = CreateInterchange(TRMessageTypes.Codes.T1N, EDIInterchange.Status.Sent, EDIInterchange.Direction.Transmit, trackingID, "Request interchange");
				var requestMessage = CreateMessage(TRMessageTypes.Codes.T1N, EDIMessage.Status.Sent, EDIMessage.Direction.Transmit, requestInterchange.PK, CusInBondHeaderSchema.Constants.TableName, nctsHeader.PK, "Request message");
				var responseInterchange = CreateInterchange(TRMessageTypes.Codes.T1N, EDIInterchange.Status.Received, EDIInterchange.Direction.Receive, trackingID, "Response interchange");
				var responseMessage = CreateMessage(TRMessageTypes.Codes.T1N, EDIMessage.Status.Received, EDIMessage.Direction.Receive, responseInterchange.PK, CusInBondHeaderSchema.Constants.TableName, nctsHeader.PK, "Response message");

				var pollingTransaction = Factory.New<CusPollingTransaction>();
				pollingTransaction.CPT_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
				pollingTransaction.CPT_Type = TRMessageTypes.Codes.T1N;
				pollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN;
				pollingTransaction.CPT_NumberOfAttempts = 5;
				pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc = ZDateTime.UtcNow;
				pollingTransaction.CPT_TransactionID = "39906267";
				pollingTransaction.CPT_ParentID = responseMessage.PK;
				Factory.Save();

				var task = new SendTransactionIDQueryMessageService_ForTest();
				var logger = task.GetLogger();
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);

				pollingTransaction.Reload();
				CombineAssertions(() =>
				{
					AssertEquals("There should be one T2N message created", 1, nctsHeader.Messages.Find(new ZQuery(EDIMessageSchema.EM_MessageType, TRMessageTypes.Codes.T2N)).Length);
					AssertEquals("CPT_Status", Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND, pollingTransaction.CPT_Status);
					Assert("Information should be logged", logger.Logs.Any(l => l.Message.Contains($"Polling transaction processed, TransactionID: {pollingTransaction.CPT_TransactionID}. Message sent successfully.") && l.Type == Integration.LogType.Information));
				});
			}
		}

		public void TestRunTaskLog()
		{
			var task = new SendTransactionIDQueryMessageService_ForTest();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
			var logger = task.GetLogger();
			CombineAssertions("completed successfully", () =>
			{
				Assert("start", logger.Logs.Any(log => log.Message.Contains("TR Auto-Send Query Message Queue service task started.") && log.Type == Integration.LogType.Information));
				Assert("complete", logger.Logs.Any(l => l.Message.Contains("TR Auto-Send Query Message Queue service task completed.") && l.Type == Integration.LogType.Information));
			});

			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();
				task = new SendTransactionIDQueryMessageService_ForTest();
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task, cts.Token);
				logger = task.GetLogger();
				CombineAssertions("completed successfully", () =>
				{
					Assert("start", logger.Logs.Any(log => log.Message.Contains("TR Auto-Send Query Message Queue service task started.") && log.Type == Integration.LogType.Information));
					Assert("catch exception", logger.Logs.Any(l => l.Message.Contains("ASQ service task ended abruptly.") && l.Type == Integration.LogType.Error));
				});
			}
		}

		EDIInterchange CreateInterchange(ZString interchangeType, ZString status, ZString direction, ZGuid sessionGUID, ZString bodyText)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_Status = status;
			interchange.EI_ReceiveTransmit = direction;
			interchange.EI_IsActive = true;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.TRCustoms;
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_From = status == EDIInterchange.Direction.Transmit ? "CW1" : "TR Customs Test";
			interchange.EI_To = status == EDIInterchange.Direction.Transmit ? "TR Customs Test" : "CW1";
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_SessionGUID = sessionGUID;
			interchange.EI_BodyText = bodyText;

			return interchange;
		}

		EDIMessage CreateMessage(ZString messageType, ZString status, ZString direction, ZGuid interchangePK, ZString linkTableName, ZGuid linkID, ZString messageText)
		{
			var message = Factory.New<TRBaseMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = status;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageText = messageText;
			message.EM_LinkTable = linkTableName;
			message.EM_LinkUniqueID = linkID;
			message.EM_EI = interchangePK;

			return message;
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		class SendTransactionIDQueryMessageService_ForTest : SendTransactionIDQueryMessageService
		{
			public LoggingInformation GetLogger()
			{
				return Logger;
			}
		}
	}
}
