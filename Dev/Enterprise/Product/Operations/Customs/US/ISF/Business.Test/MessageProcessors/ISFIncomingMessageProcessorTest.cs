using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class ISFIncomingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestMessageProcessorProcessesMessage()
		{
			var otherCompany = Factory.New<GlbCompany>();
			otherCompany.GC_Code = "Z!Z";
			otherCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var otherBranch = otherCompany.Branches.AddNew();
			otherBranch.GB_Code = "Z!Z";
			otherBranch.GB_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var otherCompanyPK = otherCompany.PK.ToGuid();
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			var currentStaff = Factory.Load<GlbStaff>(MasterFiles.Business.GlbStaff.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "bob@where.com";
			var group = Factory.Load<GlbGroup>(Enterprise.Environment.Env.Registry.PostMasterGroup);
			group.Staff.Add(currentStaff);
			var otherCompanyBessage = CreateTestMessage("103");
			otherCompanyBessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			otherCompanyBessage.EM_MessageText = "ZZ";
			otherCompanyBessage.EM_GB = otherBranch.PK;
			var isfMessage = CreateTestMessage("102");
			isfMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			isfMessage.EM_MessageText = "ZZ";
			var ensMessage = CreateTestMessage("101");
			ensMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			ensMessage.EM_MessageText = "ZZ";
			Factory.Save();
			new ISFIncomingMessageProcessor().ExecuteBatch();
			isfMessage.Reload();
			AssertEquals("Should process as part of this company and valid message type", MQEDIMessage.Status.Failed, isfMessage.EM_Status);
			ensMessage.Reload();
			AssertEquals("Should not process as not valid message type", MQEDIMessage.Status.Queued, ensMessage.EM_Status);
			otherCompanyBessage.Reload();
			AssertEquals("Should not process as not part of this company", MQEDIMessage.Status.Queued, otherCompanyBessage.EM_Status);
		}

		MQEDIMessage CreateTestMessage(string messageNum)
		{
			var testMessage = Factory.New<MQEDIMessage>();
			testMessage.EM_Status = MQEDIMessage.Status.Queued;
			testMessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			testMessage.EM_MessageNum = messageNum;
			return testMessage;
		}
	}
}
