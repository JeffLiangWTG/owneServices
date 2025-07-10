using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using USCustoms = Enterprise.Integration.Customs.US;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class DocumentReviewResponseProcessorTest : TestCaseWithFactory
	{
		public void TestDocumentReviewResponse()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "Z~";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "ZAC";
			staff.GS_LoginName = "~2";
			staff.GS_EmailAddress = "staff@pretendemail.com";
			USCustomsDataRegistry.Instance.DISMessagesGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, group.PK.ToGuid());
			var declaration = new TestHelper(Factory).GetJobDeclaration();
			declaration[JobDeclarationSchema.JE_DeclarationReference] = "BCH140007522";
			var jobDeclaration = (MasterFiles.Business.DIS.IUSDISHost)declaration;
			var requiredDocument = jobDeclaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var addInfo = requiredDocument.AddInfos.AddNew();
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			addInfo.EX_Status = Common.US.DIS.StatusList.Codes.COS;
			addInfo.EX_AddInfo = @"<DISDocument xmlns=""http://www.cargowise.com/Schemas/DISDocument""><IDSuffix>1</IDSuffix><DocumentLabel>APH01</DocumentLabel><DocumentDescription>NAFTA Certificate</DocumentDescription><Comment>test</Comment><EDocsDocumentPK>f426cc18-bea6-462a-879d-2c7cd1f4ef0e</EDocsDocumentPK><SubmitDateUTC>2016-06-21T22:00:20</SubmitDateUTC><CBPRequest><ID>UKN</ID></CBPRequest><PGAs><DISPGA><Code>APH</Code></DISPGA></PGAs></DISDocument>";
			var currentStaff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "test@cargowise.com";
			currentStaff.GS_Code = "~1";
			var message = Factory.New<EDIMessage>();
			message.EM_SystemCreateUser = currentStaff.GS_Code;
			message.EM_LinkedObject = addInfo;

			var incomingMessageRejected = Factory.New<EDIMessage>();
			incomingMessageRejected.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessageRejected.EM_MessageText = TestHelper.DocumentReviewRejectedResponseXml;
			incomingMessageRejected.EM_Status = EDIMessage.Status.Queued;

			var incomingMessageAccepted = Factory.New<EDIMessage>();
			incomingMessageAccepted.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessageAccepted.EM_MessageText = TestHelper.DocumentReviewAcceptedResponseXml;
			incomingMessageAccepted.EM_Status = EDIMessage.Status.Queued;

			var incomingMessageUnderReview = Factory.New<EDIMessage>();
			incomingMessageUnderReview.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessageUnderReview.EM_MessageText = TestHelper.DocumentReviewUnderReviewResponseXml;
			incomingMessageUnderReview.EM_Status = EDIMessage.Status.Queued;

			var incomingMessageEmptyStatus = Factory.New<EDIMessage>();
			incomingMessageEmptyStatus.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessageEmptyStatus.EM_MessageText = TestHelper.DocumentReviewEmptyStatusResponseXml;
			incomingMessageEmptyStatus.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();

			var logger = new LoggingInformation();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new DocumentReviewResponseProcessor(logger).Process(incomingMessageRejected);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("Document Review Response (Failure)", email.Subject);
			AssertContains("REJECTED", email.Body, true);
			AssertEquals(Common.US.DIS.StatusList.Codes.ERV, addInfo.EX_Status);
			AssertEquals(incomingMessageRejected.EM_LinkUniqueID, addInfo.PK);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new DocumentReviewResponseProcessor(logger).Process(incomingMessageAccepted);
			email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("Document Review Response for", email.Subject);
			AssertContains("ACCEPTED", email.Body, true);
			AssertEquals(Common.US.DIS.StatusList.Codes.ERA, addInfo.EX_Status);
			AssertEquals(incomingMessageAccepted.EM_LinkUniqueID, addInfo.PK);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new DocumentReviewResponseProcessor(logger).Process(incomingMessageUnderReview);
			email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("Document Review Response for", email.Subject);
			AssertContains("UNDER_REVIEW", email.Body, true);
			AssertEquals(Common.US.DIS.StatusList.Codes.ERR, addInfo.EX_Status);
			AssertEquals(incomingMessageUnderReview.EM_LinkUniqueID, addInfo.PK);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			addInfo.EX_Status = "";
			new DocumentReviewResponseProcessor(logger).Process(incomingMessageEmptyStatus);
			email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("Document Review Response for", email.Subject);
			AssertNotContains("Review Status", email.Body, true);
			AssertEquals("", addInfo.EX_Status);
			AssertEquals(incomingMessageUnderReview.EM_LinkUniqueID, addInfo.PK);
		}

		public void TestProcessISF()
		{
			var cusISFHeader = (BusinessObject)Factory.New<USCustoms.ISF.ICusISFHeader>();
			cusISFHeader[CusISFHeaderSchema.BF_JobReference] = "ISF140007522";
			var disHost = (MasterFiles.Business.DIS.IUSDISHost)cusISFHeader;
			var requiredDocument = disHost.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var addInfo = requiredDocument.AddInfos.AddNew();
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			addInfo.EX_Status = Common.US.DIS.StatusList.Codes.COS;
			addInfo.EX_AddInfo = @"<DISDocument xmlns=""http://www.cargowise.com/Schemas/DISDocument""><IDSuffix>1</IDSuffix><DocumentLabel>APH01</DocumentLabel><DocumentDescription>NAFTA Certificate</DocumentDescription><Comment>test</Comment><EDocsDocumentPK>f426cc18-bea6-462a-879d-2c7cd1f4ef0e</EDocsDocumentPK><SubmitDateUTC>2016-06-21T22:00:20</SubmitDateUTC><CBPRequest><ID>UKN</ID></CBPRequest><PGAs><DISPGA><Code>APH</Code></DISPGA></PGAs></DISDocument>";
			var currentStaff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "test@cargowise.com";
			currentStaff.GS_Code = "~1";
			var message = Factory.New<EDIMessage>();
			message.EM_SystemCreateUser = currentStaff.GS_Code;
			message.EM_LinkedObject = addInfo;
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = TestHelper.ISFDocumentReviewResponseXml;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();
			var logger = new LoggingInformation();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new DocumentReviewResponseProcessor(logger).Process(incomingMessage);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("Document Review Response (Failure)", email.Subject);
			AssertContains("REJECTED", email.Body);
			AssertEquals(Common.US.DIS.StatusList.Codes.ERV, addInfo.EX_Status);
			AssertEquals(incomingMessage.EM_LinkUniqueID, addInfo.PK);
			AssertNotNull(cusISFHeader.GetLogs().MostRecentLogByEventTime(Events.MessageStatusChange, "DIS ERV"));
			AssertEquals("DIS ERV", cusISFHeader.GetLogs().MostRecentLogByEventTime(Events.MessageStatusChange).SL_Reference);
			message.EM_LinkedObject = null;
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "Z~";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "ZAC";
			staff.GS_LoginName = "~2";
			staff.GS_EmailAddress = "staff@pretendemail.com";
			USCustomsDataRegistry.Instance.DISMessagesGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, group.PK.ToGuid());
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new DocumentReviewResponseProcessor(logger).Process(incomingMessage);
			AssertNotNull(Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains("Error processing DIS message")));
		}
	}
}
