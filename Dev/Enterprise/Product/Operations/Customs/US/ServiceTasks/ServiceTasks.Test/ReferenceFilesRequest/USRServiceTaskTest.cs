using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using EDIMessage = Enterprise.Customs.US.Business.EDIMessage;

namespace Enterprise.Customs.US.ServiceTasks.Testing
{
	[TestedType(typeof(USRServiceTask))]
	sealed class USRServiceTaskTest : ServiceTaskTestCase<USRServiceTask>
	{
		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("30Seconds", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestHostedServiceAttributeParameters()
		{
			AssertSingleHostedServiceAttribute("USR", "Reference File Update Message", "USC");
		}

		[TestDate(2025, 3, 2)]
		public void TestRunTaskDependsOnUseUCMPForUSIApplicationCode()
		{
			var serviceTask = new USRServiceTask();
			InitialiseTaskSchedule(serviceTask);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.UCMPServiceTask, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, value: false))
			{
				var message = CreateMessage("A");
				RunTaskSchedule(serviceTask);
				message.Reload();
				AssertEquals("Message processed", EDIMessage.Status.Received, message.EM_Status);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.UCMPServiceTask, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, value: true))
			{
				var message = CreateMessage("B");
				RunTaskSchedule(serviceTask);
				message.Reload();
				AssertEquals("Message not processed", EDIMessage.Status.Queued, message.EM_Status);
			}

			EDIMessage CreateMessage(string messageNum)
			{
				var responseMessage = Factory.New<EDIMessage>();
				responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
				responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				responseMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse;
				responseMessage.EM_Status = EDIMessage.Status.Queued;
				responseMessage.EM_MessageNum = messageNum;
				responseMessage.EM_MessageText =
"B018888XJ5FR                                               214                  " +
"F1100411                    NO UPDATES FOUND IN THIS RANGE                      " +
"Y  8888XJ5FR00001000000000000000000000000                                       ";
				Factory.Save();
				return responseMessage;
			}
		}

		[TestDate(2009, 12, 13)]
		public void TestProcessIncomingABIMessagesForUSR()
		{
			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_EmailAddress = "dummy@where.com";
			var incomingInterchange = Factory.New<CBPEDIInterchange>();
			incomingInterchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.USCustomsImport;
			incomingInterchange.EI_From = "USC";
			incomingInterchange.EI_To = "XXX";
			incomingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingInterchange.EI_InterchangeType = ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse;
			incomingInterchange.EI_HeaderText = "A3901SV9      11270601   112706193219                                00000000208";
			incomingInterchange.EI_BodyText = "B018888XJ5FR                                               214                  " +
				"F1100411                    NO UPDATES FOUND IN THIS RANGE                      " +
				"Y  8888XJ5FR00001000000000000000000000000                                       ";
			incomingInterchange.EI_FooterText = "Z3901SV9      11270601   112706193219                                00000000208";
			incomingInterchange.EI_Status = EDIInterchange.Status.Queued;
			var incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse;
			incomingMessage.EM_MessageText = "B018888XJ5FR                                               214                  " +
				"F1100411                    NO UPDATES FOUND IN THIS RANGE                      " +
				"Y  8888XJ5FR00001000000000000000000000000                                       ";
			incomingMessage.EM_Status = EDIInterchange.Status.Queued;
			Factory.Save();
			var serviceTask = new USRServiceTask();
			InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var interchangeLoaded = factory2.Load<EDIInterchange>(incomingInterchange.PK);
			AssertEquals("interchange processing should be done by a different service task", EDIInterchange.Status.Queued, interchangeLoaded.EI_Status);
			var messageLoaded = factory2.Load<MQEDIMessage>(incomingMessage.PK);
			AssertEquals("message is processed ok", EDIMessage.Status.Received, messageLoaded.EM_Status);
		}

		public void TestProcessDailyStatementResponseMessageInUSR()
		{
			var incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.DailyStatement;
			incomingMessage.EM_MessageText = "B001101SV9PFP11243100001106232                                                  " +
"Q11101SV9  73060368  58-1234567891106230000000176000000000000 B00230675      01 " +
"Q21101SV9  73060368 0000000000000000000000           2Y      00000000000        " +
"QA01124000000001811250000001500049900000003167                                  " +
"Q31124310000  110623SV9              0000000176000000000000000000000001101      " +
"Q4000000000000000000000000000020108000000000000000100000                        " +
"QE01124000000001811250000001500049900000003167                                  " +
"Y  1101SV9PF00006                                                               ";
			incomingMessage.EM_Status = EDIInterchange.Status.Queued;
			Factory.Save();
			var serviceTask = new USRServiceTask();
			InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var messageLoaded = factory2.Load<MQEDIMessage>(incomingMessage.PK);
			AssertEquals("message is processed ok", EDIMessage.Status.Received, messageLoaded.EM_Status);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs AC messages reference file update",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
						EDIMessageSchema.Constants.EM_MessageType + "=" + ACEApplicationIdentifierCodeList.Codes.ADCVDCaseInformationQueryResponse,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs CS messages reference file update",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
						EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.Codes.AntidumpingCountervailingDutyQueryResponse,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs IT messages reference file update",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
						EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.Codes.ExtractADDCVDCaseFileResponse,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs FR messages reference file update",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
						EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs FO messages reference file update",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
						EDIMessageSchema.Constants.EM_MessageType + "=" + ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs VR messages reference file update",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
						EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.Codes.HarmonizedSystemUpdate,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs WR messages reference file update",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
						EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs UR messages reference file update",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
						EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.Codes.QueryQuotaResponse,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs QB messages reference file update",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
						EDIMessageSchema.Constants.EM_MessageType + "=" + ACEApplicationIdentifierCodeList.Codes.QuotaQueryResponse,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs HZ messages reference file update",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
						EDIMessageSchema.Constants.EM_MessageType + "=" + ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs HY messages reference file update",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
						EDIMessageSchema.Constants.EM_MessageType + "=" + ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs NR messages reference file update",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
						EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs PF messages reference file update",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
						EDIMessageSchema.Constants.EM_MessageType + "=" + ACEApplicationIdentifierCodeList.Codes.DailyStatement,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs PZ messages reference file update",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
						EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.Codes.PeriodicDailyStatementACHDebitAuthorizationEntrySummaryPresentationResponse,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs MS messages reference file update",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
						EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
				};
			}
		}
	}
}
