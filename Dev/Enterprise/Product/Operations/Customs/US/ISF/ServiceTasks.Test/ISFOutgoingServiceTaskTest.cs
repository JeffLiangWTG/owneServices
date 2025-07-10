using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.US.ISF.ServiceTasks.Testing
{
	[TestedType(typeof(ISFOutgoingServiceTask))]
	sealed class ISFOutgoingServiceTaskTest : ServiceTaskTestCase<ISFOutgoingServiceTask>
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
			AssertSingleHostedServiceAttribute("US3", "United States ISF Outgoing Customs Messaging", "USC");
		}

		[TestDate(2009, 12, 13)]
		public void TestPackagingABIMQMessage()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var filer = new EntryFiler();
			filer.EntryFilerCode = "SV9";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_EmailAddress = "dummy@where.com";
			var header = Factory.New<CusISFHeader>();
			var outgoingMessage = (MQEDIMessage)header.Messages.AddNew(typeof(MQEDIMessage));
			outgoingMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling;
			outgoingMessage.EM_Status = EDIMessage.Status.Queued;
			outgoingMessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			outgoingMessage.EM_SystemCreateUser = currentUser.GS_Code;
			outgoingMessage.EM_MessageText = "B018888XJ5SF                                               ~11417               Y  8888XJ5SF00002";
			Factory.Save();
			outgoingMessage.EM_MessageNum = "~11417";
			outgoingMessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			Factory.Save();
			AssertEquals(EDIMessage.Status.Queued, outgoingMessage.EM_Status);
			AssertEquals(ZGuid.Empty, outgoingMessage.EM_EI);
			var serviceTask = new ISFOutgoingServiceTask();
			InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);
			outgoingMessage.Reload();
			AssertEquals(EDIMessage.Status.Sent, outgoingMessage.EM_Status);
			var outgoingInterchange = (CBPEDIInterchange)outgoingMessage.Interchange;
			AssertNotNull(outgoingInterchange);
			AssertEquals(CBPEDIInterchange.ApplicationCodes.USCustomsImport, outgoingInterchange.EI_ApplicationCode);
			AssertEquals(ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling, outgoingInterchange.EI_InterchangeType);
			AssertEquals(CBPEDIInterchange.Status.eHubQueued, outgoingInterchange.EI_Status);
			AssertEquals(CBPEDIInterchange.TransportType.eHub, outgoingInterchange.EI_TransportType);
			AssertEquals("A    SV9      12130901", outgoingInterchange.EI_HeaderText);
			AssertEquals(outgoingMessage.EM_MessageText, outgoingInterchange.EI_BodyText);
			AssertEquals("Z    SV9      12130901", outgoingInterchange.EI_FooterText);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[] {
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs ISF messages outbound",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Transmit,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + CBPEDIInterchange.ApplicationCodes.USCustomsImport,
						EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL")
				};
			}
		}
	}
}
