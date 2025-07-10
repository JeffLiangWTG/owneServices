using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Customs.US.ISF.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.US.ISF.ServiceTasks.Testing
{
	[TestedType(typeof(ISFServiceTask))]
	sealed class ISFServiceTaskTest : ServiceTaskTestCase<ISFServiceTask>
	{
		public void TestCanRunInAnyBranch()
		{
			var hostedServiceAttribute = GetHostedServiceAttributes().Single();
			Assert("CanRunInAnyBranch", hostedServiceAttribute.CanRunInAnyBranch);
		}

		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("30Seconds", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestHostedServiceAttributeParameters()
		{
			AssertSingleHostedServiceAttribute("ISF", "United States ISF Customs Messaging", "USC");
		}

		public void TestRunTaskDependsOnUseUCMPForUSIApplicationCode()
		{
			var header = Factory.New<CusISFHeader>();
			var serviceTask = new ISFServiceTask();
			InitialiseTaskSchedule(serviceTask);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.UCMPServiceTask, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, value: false))
			{
				var message = AddMessage(header, "2709", "~11112", new ZDateTime(2009, 11, 13, 2, 3, 2), false);
				message.EM_MessageType = "SF";
				Factory.Save();
				RunTaskSchedule(serviceTask);
				message.Reload();
				AssertEquals("Message processed", EDIMessage.Status.Received, message.EM_Status);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.UCMPServiceTask, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, value: true))
			{
				var message = AddMessage(header, "2709", "~11113", new ZDateTime(2009, 11, 13, 2, 3, 2), false);
				message.EM_MessageType = "SF";
				Factory.Save();
				Factory.Save();
				RunTaskSchedule(serviceTask);
				message.Reload();
				AssertEquals("Message not processed", EDIMessage.Status.Queued, message.EM_Status);
			}
		}

		[TestDate(2009, 12, 13)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateISFMessageUsageReportIfNeeded()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			var header = Factory.New<CusISFHeader>();
			AddMessage(header, "2709", "~11111", new ZDateTime(2009, 11, 13, 1, 2, 3), true);
			var message1 = AddMessage(header, "2709", "~11112", new ZDateTime(2009, 11, 13, 2, 3, 2), false);
			var message1a = AddMessage(header, "2709", "~11112", new ZDateTime(2009, 11, 13, 2, 3, 2), true);
			message1.EM_MessageType = "SF";
			message1a.EM_MessageType = "SF";
			AddMessage(header, "3708", "~11113", new ZDateTime(2009, 11, 12, 1, 2, 3), true);
			AddMessage(header, "2709", "~11114", new ZDateTime(2009, 10, 13, 3, 2, 3), true);
			var message2 = AddMessage(header, "2709", "~11115", new ZDateTime(2009, 10, 13, 4, 3, 4), true);
			message2.EM_MessageSubType = "REP";
			AddMessage(header, "2709", "~11116", new ZDateTime(2009, 10, 14, 3, 2, 3), true);
			AddMessage(header, "2709", "~11117", new ZDateTime(2009, 9, 1, 0, 0, 1), true);
			var message3 = AddMessage(header, "2709", "~11118", new ZDateTime(2009, 9, 1, 0, 0, 1), true);
			message3.EM_ApplicationCode = "USE";
			AddMessage(header, "2709", "~11119", new ZDateTime(2009, 8, 31, 23, 59, 59), true);
			AddMessage(header, "2709", "~11120", new ZDateTime(2009, 12, 1, 0, 0, 1), true);
			Factory.Save();
			var serviceTask = new ISFServiceTask();
			InitialiseTaskSchedule(serviceTask);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			RunTaskSchedule(serviceTask);
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			ISFRegistry.Instance.ImporterSecurityFilingMessageUsageReportDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2009, 11, 13));
			RunTaskSchedule(serviceTask);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("ISF Data Report", email.Subject);
			AssertEquals(true, email.Recipients.Contains("transactionbilling@cargowise.com"));
			AssertContains(@"<tr>
      <td class=""content"">
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Company Name</th><th>Company Code</th><th>Enterprise Code</th><th>Server Code</th><th>Year</th><th>Month</th><th>Port Code</th><th>Count</th></tr></thead><tr><td>Eagle Datamation International</td><td>EDI</td><td>EDI</td><td>DAT</td><td>2009</td><td>9</td><td>2709</td><td>1</td></tr><tr><td>Eagle Datamation International</td><td>EDI</td><td>EDI</td><td>DAT</td><td>2009</td><td>10</td><td>2709</td><td>2</td></tr><tr><td>Eagle Datamation International</td><td>EDI</td><td>EDI</td><td>DAT</td><td>2009</td><td>11</td><td>2709</td><td>1</td></tr><tr><td>Eagle Datamation International</td><td>EDI</td><td>EDI</td><td>DAT</td><td>2009</td><td>11</td><td>3708</td><td>1</td></tr></table>      </td>
    </tr>", email.Body);
			AttachmentDef attachmentFound = null;
			foreach (AttachmentDef attachment in email.Attachments)
			{
				if (attachment.DisplayName == "DataReport.CSV")
				{
					attachmentFound = attachment;
					break;
				}
			}

			AssertFileSameAsString(BaseSourcePath + @"Enterprise\Product\Operations\Customs\US\ISF\ServiceTasks\Testing\TestData.csv", Encoding.ASCII.GetString(attachmentFound.Data));
		}

		[TestDate(2009, 12, 13)]
		public void TestGenerateISFMessageUsageReportIfNeededForNoData()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			var serviceTask = new ISFServiceTask();
			ISFRegistry.Instance.ImporterSecurityFilingMessageUsageReportDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2009, 11, 13));
			InitialiseTaskSchedule(serviceTask);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			AssertNoExceptionThrown("We should not throw an exception when we access CurrentBranch", () =>
			{
				RunTaskSchedule(serviceTask);
			});
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("ISF Data Report", email.Subject);
			AssertEquals(true, email.Recipients.Contains("transactionbilling@cargowise.com"));
			AssertContains(@"      <td class=""content"">
No record of transactions were found.      </td>
    </tr>", email.Body);
			AttachmentDef attachmentFound = null;
			foreach (AttachmentDef attachment in email.Attachments)
			{
				if (attachment.DisplayName == "DataReport.CSV")
				{
					attachmentFound = attachment;
					break;
				}
			}

			AssertNull(attachmentFound);
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			ISFRegistry.Instance.ImporterSecurityFilingMessageUsageReportDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Today.AddYears(1));
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[] {
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs ISF response messages inbound",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
						EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs ISF status messages inbound",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
						EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingStatusAdvisory,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
					new TaskNudgeInformationForTest(EDIMessageSchema.Constants.TableName,
						"US Customs ISF messages inbound",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
						EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL")
				};
			}
		}

		MQEDIMessage AddMessage(CusISFHeader entry, ZString port, ZString messageNum, ZDateTime createTime, bool addISFAcceptedText)
		{
			var isfAcceptedText = addISFAcceptedText ? "SF9002   ISF ACCEPTED                                                           " : "";
			var message = (MQEDIMessage)entry.Messages.AddNew(typeof(MQEDIMessage));
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			message.EM_MessageSubType = "ADD";
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_MessageText = string.Format("B01{0}XJ5SN                                               {1}" +
				"SF10101ACTEI 91-013199000           11XJ5-19756574533    91-013199000   018     " +
				"SF15OBAPLU1112222                                                               {2}Y  {0}XJ5SN00003",
				port.Left(4).PadRight(4), messageNum.Left(21).PadRight(21), isfAcceptedText);
			message.EM_SystemCreateTimeUtc = createTime;
			return message;
		}
	}
}
