using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
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
	[TestedType(typeof(ABIOutgoingServiceTask))]
	sealed class ABIOutgoinServiceTaskTest : ServiceTaskTestCase<ABIOutgoingServiceTask>
	{
		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("30Seconds", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestHostedServiceAttributeParameters()
		{
			AssertSingleHostedServiceAttribute("US2", "United States ABI Messages Outbound", "USC");
		}

		[TestDate(2009, 12, 13)]
		public void TestPackagingABIMQMessage()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var filer = new EntryFiler();
			filer.EntryFilerCode = "SV9";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);
			using (var dir1 = new TempDirectory())
			{
				var company1PK = GlbCompany.CurrentCompany.PK.ToGuid();
				DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
				var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
				currentUser.GS_EmailAddress = "dummy@where.com";
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_DeclarationReference = "B44000001";
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
				entry.CH_BGMReference = "B44000001";
				var outgoingMessage = (MQEDIMessage)entry.Messages.AddNew(typeof(MQEDIMessage));
				outgoingMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummary;
				outgoingMessage.EM_Status = EDIMessage.Status.Queued;
				outgoingMessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
				outgoingMessage.EM_SystemCreateUser = currentUser.GS_Code;
				outgoingMessage.EM_MessageText = "B018888XJ5EI                                               ~11417               Y  8888XJ5EI00002";
				Factory.Save();
				outgoingMessage.EM_MessageNum = "~11417";
				outgoingMessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
				Factory.Save();
				AssertEquals(EDIMessage.Status.Queued, outgoingMessage.EM_Status);
				AssertEquals(ZGuid.Empty, outgoingMessage.EM_EI);
				var serviceTask = new ABIOutgoingServiceTask();
				InitialiseTaskSchedule(serviceTask);
				RunTaskSchedule(serviceTask);
				outgoingMessage.Reload();
				AssertEquals(EDIMessage.Status.Sent, outgoingMessage.EM_Status);
				var outgoingInterchange = (CBPEDIInterchange)outgoingMessage.Interchange;
				AssertNotNull(outgoingInterchange);
				AssertEquals(CBPEDIInterchange.ApplicationCodes.USCustomsImport, outgoingInterchange.EI_ApplicationCode);
				AssertEquals(ApplicationIdentifierCodeList.Codes.EntrySummary, outgoingInterchange.EI_InterchangeType);
				AssertEquals(CBPEDIInterchange.Status.eHubQueued, outgoingInterchange.EI_Status);
				AssertEquals(EDIInterchange.TransportType.eHub, outgoingInterchange.EI_TransportType);
				AssertEquals("A    SV9      12130901", outgoingInterchange.EI_HeaderText);
				AssertEquals(outgoingMessage.EM_MessageText, outgoingInterchange.EI_BodyText);
				AssertEquals("Z    SV9      12130901", outgoingInterchange.EI_FooterText);
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs ABI Messages Outbound",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
						EDIMessageSchema.Constants.EM_MessageType + "!=" + ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
				};
			}
		}
	}
}
