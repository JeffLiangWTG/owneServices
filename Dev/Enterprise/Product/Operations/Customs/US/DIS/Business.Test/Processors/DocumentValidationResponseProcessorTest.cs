using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Common.US.DIS;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class DocumentValidationResponseProcessorTest : TestCaseWithFactory
	{
		public void TestLinkBackToOriginalJob()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "Z~";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "ZAC";
			staff.GS_LoginName = "~2";
			staff.GS_EmailAddress = "staff@pretendemail.com";
			USCustomsDataRegistry.Instance.DISMessagesGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, group.PK.ToGuid());
			var outgoingMessage = GetOutgoingMessage();
			outgoingMessage.EM_MessageNum = "HYEDUSDAT_13";
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = TestHelper.DocumentValidationResponseFailedXml;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageNum = "HYEDUSDAT_13";
			Factory.Save();
			outgoingMessage.EM_MessageNum = "HYEDUSDAT_13";
			Factory.Save();
			var logger = new LoggingInformation();
			new DocumentValidationResponseProcessor(logger).Process(incomingMessage);
			AssertEquals(outgoingMessage.EM_LinkedObject.PK, incomingMessage.EM_LinkedObject.PK);
			AssertNotNull(Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains("Error processing DIS message")));
		}

		public void TestUpdateStatus()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "Z~";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "ZAC";
			staff.GS_LoginName = "~2";
			staff.GS_EmailAddress = "staff@pretendemail.com";
			USCustomsDataRegistry.Instance.DISMessagesGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, group.PK.ToGuid());
			var outgoingMessage = GetOutgoingMessage();
			outgoingMessage.EM_MessageNum = "HYEDUSDAT_13";
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			var requiredDocAddInfo = (JobRequiredDocumentAddInfo)outgoingMessage.EM_LinkedObject;
			requiredDocAddInfo.EX_Status = StatusList.Codes.AOS;
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = TestHelper.DocumentValidationResponseFailedXml;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageNum = "HYEDUSDAT_13";
			Factory.Save();
			outgoingMessage.EM_MessageNum = "HYEDUSDAT_13";
			Factory.Save();
			var logger = new LoggingInformation();
			new DocumentValidationResponseProcessor(logger).Process(incomingMessage);
			AssertEquals(StatusList.Codes.EOS, requiredDocAddInfo.EX_Status);
			var targetLog = logger.UserLogStrings.Cast<string>().FirstOrDefault(x => x.Contains("System could not locate an original sender for "));
			AssertNotNull(targetLog);
			AssertNotNull(Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains("Error processing DIS message")));
		}

		public void TestUpdateStatusForErrorOriginalSubmissionMessages()
		{
			var outgoingMessage = GetOutgoingMessage();
			outgoingMessage.EM_MessageNum = "HYEDUSDAT_13";
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			var requiredDocAddInfo = (JobRequiredDocumentAddInfo)outgoingMessage.EM_LinkedObject;
			requiredDocAddInfo.EX_Status = StatusList.Codes.AOS;
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = TestHelper.DocumentValidationResponseFailedXml;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageNum = "HYEDUSDAT_13";
			Factory.Save();
			outgoingMessage.EM_MessageNum = "HYEDUSDAT_13";
			Factory.Save();
			var logger = new LoggingInformation();
			new DocumentValidationResponseProcessor(logger).Process(incomingMessage);
			AssertEquals(StatusList.Codes.EOS, requiredDocAddInfo.EX_Status);
			AssertNotNull(requiredDocAddInfo.RequiredDocument.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.MessageStatusChange, "DIS EOS"));
			AssertEquals("DIS EOS", requiredDocAddInfo.RequiredDocument.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.MessageStatusChange).SL_Reference);
		}

		public void TestUpdateStatusForErrorReplacementSubmissionMessages()
		{
			var outgoingMessage = GetOutgoingMessage();
			outgoingMessage.EM_MessageNum = "HYEDUSDAT_13";
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			var requiredDocAddInfo = (JobRequiredDocumentAddInfo)outgoingMessage.EM_LinkedObject;
			requiredDocAddInfo.EX_Status = StatusList.Codes.ARS;
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = TestHelper.DocumentValidationResponseFailedXml;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageNum = "HYEDUSDAT_13";
			Factory.Save();
			outgoingMessage.EM_MessageNum = "HYEDUSDAT_13";
			Factory.Save();
			var logger = new LoggingInformation();
			new DocumentValidationResponseProcessor(logger).Process(incomingMessage);
			AssertEquals(StatusList.Codes.ERS, requiredDocAddInfo.EX_Status);
			AssertNotNull(requiredDocAddInfo.RequiredDocument.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.MessageStatusChange, "DIS ERS"));
			AssertEquals("DIS ERS", requiredDocAddInfo.RequiredDocument.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.MessageStatusChange).SL_Reference);
		}

		public void TestUpdateStatusForErrorWithdrawalSubmissionMessages()
		{
			var outgoingMessage = GetOutgoingMessage();
			outgoingMessage.EM_MessageNum = "HYEDUSDAT_13";
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			var requiredDocAddInfo = (JobRequiredDocumentAddInfo)outgoingMessage.EM_LinkedObject;
			requiredDocAddInfo.EX_Status = StatusList.Codes.AWS;
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = TestHelper.DocumentValidationResponseFailedXml;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageNum = "HYEDUSDAT_13";
			Factory.Save();
			outgoingMessage.EM_MessageNum = "HYEDUSDAT_13";
			Factory.Save();
			var logger = new LoggingInformation();
			new DocumentValidationResponseProcessor(logger).Process(incomingMessage);
			AssertEquals(StatusList.Codes.EWS, requiredDocAddInfo.EX_Status);
			AssertNotNull(requiredDocAddInfo.RequiredDocument.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.MessageStatusChange, "DIS EWS"));
			AssertEquals("DIS EWS", requiredDocAddInfo.RequiredDocument.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.MessageStatusChange).SL_Reference);
		}

		public void TestUpdateStatusForClearOriginalSubmissionMessages()
		{
			var outgoingMessage = GetOutgoingMessage();
			outgoingMessage.EM_MessageNum = "HYEDUSDAT_13";
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			var requiredDocAddInfo = (JobRequiredDocumentAddInfo)outgoingMessage.EM_LinkedObject;
			requiredDocAddInfo.EX_Status = StatusList.Codes.AOS;
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = TestHelper.DocumentValidationPassedResponseXml;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageNum = "HYEDUSDAT_13";
			Factory.Save();
			outgoingMessage.EM_MessageNum = "HYEDUSDAT_13";
			Factory.Save();
			var logger = new LoggingInformation();
			new DocumentValidationResponseProcessor(logger).Process(incomingMessage);
			AssertEquals(StatusList.Codes.COS, requiredDocAddInfo.EX_Status);
			AssertNotNull(requiredDocAddInfo.RequiredDocument.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.MessageStatusChange, "DIS COS"));
			AssertEquals("DIS COS", requiredDocAddInfo.RequiredDocument.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.MessageStatusChange).SL_Reference);
		}

		public void TestUpdateStatusForClearReplacementSubmissionMessages()
		{
			var outgoingMessage = GetOutgoingMessage();
			outgoingMessage.EM_MessageNum = "HYEDUSDAT_13";
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			var requiredDocAddInfo = (JobRequiredDocumentAddInfo)outgoingMessage.EM_LinkedObject;
			requiredDocAddInfo.EX_Status = StatusList.Codes.ARS;
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = TestHelper.DocumentValidationPassedResponseXml;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageNum = "HYEDUSDAT_13";
			Factory.Save();
			outgoingMessage.EM_MessageNum = "HYEDUSDAT_13";
			Factory.Save();
			var logger = new LoggingInformation();
			new DocumentValidationResponseProcessor(logger).Process(incomingMessage);
			AssertEquals(StatusList.Codes.CRS, requiredDocAddInfo.EX_Status);
			AssertNotNull(requiredDocAddInfo.RequiredDocument.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.MessageStatusChange, "DIS CRS"));
			AssertEquals("DIS CRS", requiredDocAddInfo.RequiredDocument.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.MessageStatusChange).SL_Reference);
		}

		public void TestUpdateStatusForClearWithdrawalSubmissionMessages()
		{
			var outgoingMessage = GetOutgoingMessage();
			outgoingMessage.EM_MessageNum = "HYEDUSDAT_13";
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			var requiredDocAddInfo = (JobRequiredDocumentAddInfo)outgoingMessage.EM_LinkedObject;
			requiredDocAddInfo.EX_Status = StatusList.Codes.AWS;
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = TestHelper.DocumentValidationPassedResponseXml;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageNum = "HYEDUSDAT_13";
			Factory.Save();
			outgoingMessage.EM_MessageNum = "HYEDUSDAT_13";
			Factory.Save();
			var logger = new LoggingInformation();
			new DocumentValidationResponseProcessor(logger).Process(incomingMessage);
			AssertEquals(StatusList.Codes.CWS, requiredDocAddInfo.EX_Status);
			AssertNotNull(requiredDocAddInfo.RequiredDocument.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.MessageStatusChange, "DIS CWS"));
			AssertEquals("DIS CWS", requiredDocAddInfo.RequiredDocument.Parent.UltimateDocumentParent.GetLogs().MostRecentLogByEventTime(Events.MessageStatusChange).SL_Reference);
		}

		public void TestEmailContentWithMessageHeader()
		{
			var currentStaff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "test@cargowise.com";
			currentStaff.GS_Code = "~1";
			var outgoingMessage = GetOutgoingMessage();
			outgoingMessage.EM_MessageNum = "HYEDUSDAT_13";
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_SystemCreateUser = "~1";
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = TestHelper.DocumentValidationResponseFailedXml;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageNum = "HYEDUSDAT_13";
			Factory.Save();
			outgoingMessage.EM_MessageNum = "HYEDUSDAT_13";
			Factory.Save();
			var logger = new LoggingInformation();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new DocumentValidationResponseProcessor(logger).Process(incomingMessage);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains("DIS Document Validation"));
			AssertContains("Message processing failed while validating DocumentData", email.Body);
		}

		public void TestEmailContentWithDocumentHeader()
		{
			var currentStaff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "test@cargowise.com";
			currentStaff.GS_Code = "~1";
			var outgoingMessage = GetOutgoingMessage();
			outgoingMessage.EM_MessageNum = "HYEDUSDAT_13";
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_SystemCreateUser = "~1";
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = TestHelper.DocumentValidationPassedResponseXml;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageNum = "HYEDUSDAT_13";
			Factory.Save();
			outgoingMessage.EM_MessageNum = "HYEDUSDAT_13";
			Factory.Save();
			var logger = new LoggingInformation();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new DocumentValidationResponseProcessor(logger).Process(incomingMessage);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains("DIS Generic Document Validation"));
			Assert(email.Subject.Contains("IMPORTER"));
			AssertContains("No errors found for document", email.Body);
		}

		public void TestAddOrigStatusWhenUpdateStatusFailed()
		{
			var jobDeclaration = (IUSDISHost)new TestHelper(Factory).GetJobDeclaration();
			var requiredDocument = jobDeclaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var addInfo = requiredDocument.AddInfos.AddNew();
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			addInfo.EX_Status = "";
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_LinkedObject = addInfo;
			var currentStaff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "test@cargowise.com";
			currentStaff.GS_Code = "~1";
			outgoingMessage.EM_MessageNum = "HYEDUSDAT_13";
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_SystemCreateUser = "~1";
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = TestHelper.DocumentValidationPassedResponseXml;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageNum = "HYEDUSDAT_13";
			Factory.Save();
			outgoingMessage.EM_MessageNum = "HYEDUSDAT_13";
			Factory.Save();
			var logger = new LoggingInformation();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new DocumentValidationResponseProcessor(logger).Process(incomingMessage);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains("DIS Generic Document Validation"));
			AssertContains("Orig. status", email.Body);
		}

		public void TestDISMessageReceived_PreviousNotificationsExist_MessageComplete()
		{
			var declarationBiz = new TestHelper(Factory).GetJobDeclaration();
			var eDoc = ((IDocManagerSupport)declarationBiz).DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "Test", Core.Constants.RefDocTypes.CommercialInvoice);
			eDoc.Description = "SOME DESCRIPTION";
			Factory.Save();

			var declaration = GetMergedDeclaration(declarationBiz.PK, "70041213");
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			var message = GetMessageToProcess();
			message.EM_MessageText =
"B018888SV9UC                                               34                   " +
"E121333010090110                                  SV9  70041213                 " +
"E2  CHRIS SMITH                     5555555555  123456789012                    " +
"Y  8888SV9UC00002";

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			AssertEquals("PreCondition", "2:123456789012", message.EM_ApplicationReference);
			Assert(!message.ActionAuthorised);

			var outgoingMessage = GetOutgoingMessage(declarationBiz, true);
			outgoingMessage.EM_MessageNum = "HYEDUSDAT_13";
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_MessageText = TestHelper.DocumentSubmissionXml;
			Factory.Save();

			var hostJob = (IUSDISHost)declarationBiz;
			var hostWrapper = new DISHostWrapper(hostJob);
			var disDocument = hostWrapper.DISDocuments.AddNew();
			disDocument.IDSuffix = 1;
			disDocument.Comment = "Comment";
			disDocument.DocumentLabel = "APH01";
			disDocument.DocumentDescription = eDoc.Description;
			disDocument.Status = StatusList.Codes.AOS;
			disDocument.CBPRequest.Type = CBPRequestTypeList.Codes.ACEActionNumber;
			disDocument.CBPRequest.ID = "123456789012";
			disDocument.CBPRequest.RequestDate = ZDateTime.BrettsBirthday;
			disDocument.EDocsDocumentPK = eDoc.UniqueKey;
			disDocument.RequiredDocumentPK = hostJob.RequiredDocumentsProvider.RequiredDocuments[0].PK;
			disDocument.RequiredDocumentAddInfo = (JobRequiredDocumentAddInfo)outgoingMessage.EM_LinkedObject;
			Factory.Save();

			var requiredDocAddInfo = (JobRequiredDocumentAddInfo)outgoingMessage.EM_LinkedObject;
			requiredDocAddInfo.EX_Status = StatusList.Codes.ARS;
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = TestHelper.DocumentValidationPassedResponseXml;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageNum = "HYEDUSDAT_13";
			Factory.Save();
			outgoingMessage.EM_MessageNum = "HYEDUSDAT_13";
			Factory.Save();

			var factoryReload = new BusinessObjectFactory();
			var incomingMessageReloaded = factoryReload.Load<EDIMessage>(incomingMessage.PK);
			var logger = new LoggingInformation();
			new DocumentValidationResponseProcessor(logger).Process(incomingMessageReloaded);
			factoryReload.Save();

			message.Reload();
			var messageLoaded = new BusinessObjectFactory().Load<MQEDIMessage>(message.PK);
			Assert("Should automatically mark as actions taken", messageLoaded.ActionAuthorised);
		}

		EDIMessage GetOutgoingMessage(BusinessObject declaration = null, bool createDocumnet4Host = false)
		{
			var hostJob = declaration == null ? (IUSDISHost)new TestHelper(Factory).GetJobDeclaration() : (IUSDISHost)declaration;
			var requiredDocument = hostJob.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var addInfo = requiredDocument.AddInfos.AddNew();
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			addInfo.EX_Status = StatusList.Codes.AOS;
			var result = Factory.New<EDIMessage>();
			result.EM_LinkedObject = addInfo;

			return result;
		}

		JobDeclaration GetMergedDeclaration(ZGuid declarationPK, ZString entryNumber)
		{
			var dec = Factory.Load<JobDeclaration>(declarationPK);
			dec.US_EnableENS = true;
			dec.US_EntryFilerCode = "SV9";
			dec.ImportEntryNumber = entryNumber;

			dec.Invoices.AddNew();
			dec.InvoiceLines.AddNew();

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			return dec;
		}

		MQEDIMessage GetMessageToProcess()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			message.EM_Status = EDIMessage.Status.Queued;
			return message;
		}
	}
}
