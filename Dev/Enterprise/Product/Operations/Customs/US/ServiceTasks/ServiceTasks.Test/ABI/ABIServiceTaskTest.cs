using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using EDIMessage = Enterprise.Customs.US.Business.EDIMessage;

namespace Enterprise.Customs.US.ServiceTasks.Testing
{
	[TestedType(typeof(ABIServiceTask))]
	sealed class ABIServiceTaskTest : ServiceTaskTestCase<ABIServiceTask>
	{
		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("30Seconds", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestHostedServiceAttributeParameters()
		{
			AssertSingleHostedServiceAttribute("ABI", "United States ABI Customs Messaging", "USC");
		}

		[TestDate(2025, 3, 2)]
		public void TestRunTaskDependsOnUseUCMPForUSIApplicationCode()
		{
			var serviceTask = new ABIServiceTask();
			InitialiseTaskSchedule(serviceTask);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.UCMPServiceTask, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, value: false))
			{
				var message = CreateMessage();
				RunTaskSchedule(serviceTask);
				message.Reload();
				AssertEquals("Message processed", EDIMessage.Status.Received, message.EM_Status);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.UCMPServiceTask, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, value: true))
			{
				var message = CreateMessage();
				RunTaskSchedule(serviceTask);
				message.Reload();
				AssertEquals("Message not processed", EDIMessage.Status.Queued, message.EM_Status);
			}
		}

		public void TestRunTaskOnFTZMessageFailure()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.FTZZoneID = "153A153";
			declaration.FTZYear = "17";
			declaration.FTZControlNumber = "TOS01015";
			var admissionAddResponse = Factory.New<MQEDIMessage>();
			admissionAddResponse.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			admissionAddResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			admissionAddResponse.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			admissionAddResponse.EM_Status = EDIMessage.Status.Queued;
			admissionAddResponse.EM_MessageNum = "HYEDUSCMT_192246";
			admissionAddResponse.EM_MessageText = "B003901SV9NF                                               HYEDUSCMT_192246     X0                                                                              X1 FX12   NOT A KNOWN ACE APPLICATION ID CODE                                   X0 BLOCK  000001 REF ID: 3901 SV9    NF HYEDUSCMT_192246                        X1 FX12   NOT A KNOWN ACE APPLICATION ID CODE                                   X0                                                                              X1RF999   BATCH REJECTED                                                        Y  3901SV9NF00000";
			Factory.Save();
			var serviceTask = new ABIServiceTask();
			InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);
			AssertNotContains("Unable to cast object of type 'Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common.APLB' to type 'Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output.AABIOutputB'.", serviceTask.ServiceLogger.ToString());
		}

		[TestDate(2009, 12, 13)]
		public void TestRunTaskOnISFMessageFailure()
		{
			var admissionAddResponse = Factory.New<MQEDIMessage>();
			admissionAddResponse.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			admissionAddResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			admissionAddResponse.EM_MessageType = "";
			admissionAddResponse.EM_Status = EDIMessage.Status.Queued;
			admissionAddResponse.EM_MessageNum = "";
			admissionAddResponse.EM_MessageText = "B00                                                        B                    X0 BLOCK  000001 REF ID: 3901 SV9    SF HYEDUSCMT_192246                        X1 FX17   FILER NOT AUTHORIZED                                                  X1RF999   BATCH REJECTED                                                        Y           00003";
			Factory.Save();
			var serviceTask = new ABIServiceTask();
			InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);
			AssertNotContains("Unable to cast object of type 'Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common.APLB' to type 'Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output.AABIOutputB'.", serviceTask.ServiceLogger.ToString());
		}

		public void TestRunTaskOnACEERMessageFailure()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQuery;
			message.EM_MessageNum = "KNALNMPR1_6662938";
			message.EM_Status = EDIMessage.Status.Sent;
			message.EM_MessageOwner = "ACE";
			message.EM_MessageText = "B  3001101EQ                                               KNALNMPR1_6662938    J1   101  06532765                                                              Y  3001101EQ";
			var responseMessage = CreateMessage();
			Factory.Save();
			var serviceTask = new ABIServiceTask();
			InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);
			responseMessage.Reload();
			AssertNotContains("Unable to cast object of type 'Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common.APLB' to type 'Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output.AABIOutputB'.", serviceTask.ServiceLogger.ToString());
			AssertEquals("KNALNMPR1_6662938", responseMessage.EM_MessageNum);
			AssertEquals(EDIMessage.Status.Received, responseMessage.EM_Status);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs ABI Messages Inbound",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
						EDIMessageSchema.Constants.EM_MessageType + "!=" + ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling,
						EDIMessageSchema.Constants.EM_MessageType + "!=" + ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse,
						EDIMessageSchema.Constants.EM_MessageType + "!=" + ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingStatusAdvisory,
						EDIMessageSchema.Constants.EM_MessageType + "!=" + ACEApplicationIdentifierCodeList.Codes.ADCVDCaseInformationQueryResponse,
						EDIMessageSchema.Constants.EM_MessageType + "!=" + ApplicationIdentifierCodeList.Codes.AntidumpingCountervailingDutyQueryResponse,
						EDIMessageSchema.Constants.EM_MessageType + "!=" + ApplicationIdentifierCodeList.Codes.ExtractADDCVDCaseFileResponse,
						EDIMessageSchema.Constants.EM_MessageType + "!=" + ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse,
						EDIMessageSchema.Constants.EM_MessageType + "!=" + ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse,
						EDIMessageSchema.Constants.EM_MessageType + "!=" + ApplicationIdentifierCodeList.Codes.HarmonizedSystemUpdate,
						EDIMessageSchema.Constants.EM_MessageType + "!=" + ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem,
						EDIMessageSchema.Constants.EM_MessageType + "!=" + ApplicationIdentifierCodeList.Codes.QueryQuotaResponse,
						EDIMessageSchema.Constants.EM_MessageType + "!=" + ACEApplicationIdentifierCodeList.Codes.QuotaQueryResponse,
						EDIMessageSchema.Constants.EM_MessageType + "!=" + ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse,
						EDIMessageSchema.Constants.EM_MessageType + "!=" + ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse,
						EDIMessageSchema.Constants.EM_MessageType + "!=" + ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
						EDIMessageSchema.Constants.EM_MessageType + "!=" + ACEApplicationIdentifierCodeList.Codes.DailyStatement,
						EDIMessageSchema.Constants.EM_MessageType + "!=" + ApplicationIdentifierCodeList.Codes.PeriodicDailyStatementACHDebitAuthorizationEntrySummaryPresentationResponse,
						EDIMessageSchema.Constants.EM_MessageType + "!=" + ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement),
				};
			}
		}

		EDIMessage CreateMessage()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQuery;
			message.EM_MessageNum = "KNALNMPR1_6662938";
			message.EM_Status = EDIMessage.Status.Sent;
			message.EM_MessageOwner = "ACE";
			message.EM_MessageText = "B  3001101EQ                                               KNALNMPR1_6662938    J1   101  06532765                                                              Y  3001101EQ";

			var responseMessage = Factory.New<EDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQueryResponse;
			responseMessage.EM_Status = EDIMessage.Status.Queued;
			responseMessage.EM_MessageNum = "B";
			responseMessage.EM_MessageText = "B00                                                        B                    X0 BLOCK  000001 REF ID: 3001 101    EQ KNALNMPR1_6662938                       X1 FX20   FILER NOT AUTHORIZED FOR APPLICATION ID                               X1RF999   BATCH REJECTED                                                        Y           00003                                                               ";
			Factory.Save();
			return responseMessage;
		}
	}
}
