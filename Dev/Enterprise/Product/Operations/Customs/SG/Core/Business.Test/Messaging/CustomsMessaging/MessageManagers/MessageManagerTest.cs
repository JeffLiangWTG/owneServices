using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.SG.Business.CustomsMessaging;
using Enterprise.Customs.SG.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	class MessageManagerTest : TestCaseWithFactory
	{
		#region TestSendMessage
		public void TestSendMessageInLimitedSize()
		{
			var docs = new SupportingDocumentCollectionTest.eDocs { new SupportingDocumentCollectionTest.eDoc { FileName = "some document name.pdf", DocType = "doc1", FileSizeInMB = 7m }, new SupportingDocumentCollectionTest.eDoc { FileName = "some other name.xls", DocType = "doc2", FileSizeInMB = 6m } };
			var additionalMessageInformation = new AdditionalMessageInformation(null, docs, AdditionalMessageInformation.BoundFormTypes.Declaration, Factory, string.Empty);
			additionalMessageInformation.AM_Broker = "TST";
			SupportingDocument document1 = additionalMessageInformation.SupportingDocuments.AddNew();
			document1.eDoc = docs[0].UniqueKey;
			SupportingDocument document2 = additionalMessageInformation.SupportingDocuments.AddNew();
			document2.eDoc = docs[1].UniqueKey;
			Declaration.AdditionalMessageInformation = additionalMessageInformation;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			using (SGCustomsDataRegistry.Instance.MaximumMessageSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 11))
			{
				var sender = new SendsMessagesToCustomsShutterUpperer(false);
				var result = MessageManager.SendOriginalMessages(sender);
				Assert("Cant send the message as its size is greater than the value of SGCustomsRegistry.Instance.MaximumMessageSize.", !result);
				AssertEquals(@"The size of the message to Customs is greater than the Maximum Message Size - 11(MB). Please review these attachments and reduce the size of the files being submitted. You can also change the Maximum Message Size at Customs -> Country or Region Specific -> Singapore -> Tradenet v4 -> MHUB -> Maximum Message Size", sender.InvalidOperationText);
			}

			using (SGCustomsDataRegistry.Instance.MaximumMessageSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 100))
			{
				var sender = new SendsMessagesToCustomsShutterUpperer(false);
				var result = MessageManager.SendOriginalMessages(sender);
				Assert("Should send the message as its size is less than the value of SGCustomsRegistry.Instance.MaximumMessageSize.", result);
				AssertNull(sender.InvalidOperationText);
			}
		}

		[TestDate(2007, 1, 1)]
		public void TestSendAMessageGenericTest()
		{
			AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
			additionalMessageInformation.AM_Broker = "TST";
			Declaration.AdditionalMessageInformation = additionalMessageInformation;
			SupportingDocument supportingDocument = additionalMessageInformation.SupportingDocuments.AddNew();
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			MessageManager.SendOriginalMessages(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, EntryHeader.Messages.Count);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, Declaration.JE_GS_NKCusAgent);
			AssertEquals(new ZDateTime(2007, 1, 1), Declaration.JE_EntrySubmittedDate);
			CUSDECEDIMessage message = (CUSDECEDIMessage)EntryHeader.Messages[0];
			AssertEquals(CUSDECEDIMessage.Status.Queued, message.EM_Status);
			AssertEquals(MessageTypeCodeList.Codes.INP, message.EM_MessageType);
			AssertEquals(CUSDECEDIMessage.Declaration, message.EM_MessageSubType);
			AssertEquals(1, EntryHeader.Messages.Count);
			AssertEquals(Core.SGConstants.DeclarationStatus.DeclarationPending, EntryHeader.CH_Status);
			foreach (JobComInvoiceHeader invoiceHeader in Declaration.Invoices)
			{
				AssertEquals(ZDateTime.Today, invoiceHeader.EffectiveValuationDate);
				AssertEquals(ZDateTime.Today, invoiceHeader.JZ_ValuationDateOverride);
			}

			EDIInterchange sG4Interchange = EntryHeader.Messages[0].Interchange;
			AssertNull("The interchange should not have been created yet", sG4Interchange);
		}

		[GuiTest, TestDate(2007, 1, 1)]
		public void TestSendAMessageGenericTestDeniedParty()
		{
			var sender = new SendsMessagesToCustomsShutterUpperer(false);
			AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
			additionalMessageInformation.AM_Broker = "TST";
			SupportingDocument supportingDocument = additionalMessageInformation.SupportingDocuments.AddNew();
			Declaration.AdditionalMessageInformation = additionalMessageInformation;
			Declaration.MessageInitiator = sender;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			Declaration.DPSFreightMovementRestricted = true;
			sender.InvalidOperationText = "";
			MessageManager.SendOriginalMessages(sender);
			AssertEquals("Warning text", "Unable to submit message due to Denied Party Screening cancellation.", sender.InvalidOperationText);
			Declaration.DPSFreightMovementRestricted = false;
			sender.InvalidOperationText = "";
			MessageManager.SendOriginalMessages(sender);
			AssertEquals("Warning text", "", sender.InvalidOperationText);
			AssertEquals(1, EntryHeader.Messages.Count);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, Declaration.JE_GS_NKCusAgent);
			AssertEquals(new ZDateTime(2007, 1, 1), Declaration.JE_EntrySubmittedDate);
			CUSDECEDIMessage message = (CUSDECEDIMessage)EntryHeader.Messages[0];
			AssertEquals(CUSDECEDIMessage.Status.Queued, message.EM_Status);
			AssertEquals(MessageTypeCodeList.Codes.INP, message.EM_MessageType);
			AssertEquals(CUSDECEDIMessage.Declaration, message.EM_MessageSubType);
			AssertEquals(1, EntryHeader.Messages.Count);
			AssertEquals(Core.SGConstants.DeclarationStatus.DeclarationPending, EntryHeader.CH_Status);
			foreach (JobComInvoiceHeader invoiceHeader in Declaration.Invoices)
			{
				AssertEquals(ZDateTime.Today, invoiceHeader.EffectiveValuationDate);
				AssertEquals(ZDateTime.Today, invoiceHeader.JZ_ValuationDateOverride);
			}

			EDIInterchange sG4Interchange = EntryHeader.Messages[0].Interchange;
			AssertNull("The interchange should not have been created yet", sG4Interchange);
		}

		public void TestSetEM_SendWithMessageErrors()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
				additionalMessageInformation.AM_Broker = "TST";
				Declaration.AdditionalMessageInformation = additionalMessageInformation;
				Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
				var importer = Declaration.Factory.New<OrgHeader>();
				importer.OH_Code = "IMDS3234";
				importer.OH_IsConsignee = true;
				importer.MainAddress.OA_Address1 = "AD";
				importer.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "12345678901J");
				importer.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
				Declaration.JE_OH_Importer = importer.PK;
				var invoiceLine = Declaration.InvoiceLines[0];
				invoiceLine.JI_Description = "BOB";
				invoiceLine.JI_CustomsQuantity = 1m;
				MessageManager.SendOriginalMessages(new SendsMessagesToCustomsShutterUpperer());
				CUSDECEDIMessage message = (CUSDECEDIMessage)EntryHeader.Messages[0];
				AssertEquals(false, message.EM_SendWithMessageErrors);
				EntryHeader.Messages.RemoveAndDeleteAllFromTest();
				Declaration.JE_OH_Importer = ZGuid.Empty;
				Declaration.Validation.ValidateAll();
				MessageManager.SendOriginalMessages(new SendsMessagesToCustomsShutterUpperer());
				message = (CUSDECEDIMessage)EntryHeader.Messages[0];
				AssertEquals(true, message.EM_SendWithMessageErrors);
			}
		}

		[TestDate(2007, 1, 2)]
		public void TestSendMessageINP()
		{
			AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
			additionalMessageInformation.AM_Broker = "TST";
			Declaration.AdditionalMessageInformation = additionalMessageInformation;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			MessageManager.SendOriginalMessages(new SendsMessagesToCustomsShutterUpperer());
			CUSDECEDIMessage message = (CUSDECEDIMessage)EntryHeader.Messages[0];
			Assert(message.EM_MessageText.Contains("+INPDEC'BGM"));
			AssertEquals("Declaration status should have been updated", Core.SGConstants.DeclarationStatus.DeclarationPending, Declaration.JE_EntryStatus);
		}

		public void TestSendD09BMessageIPT()
		{
			declaration = null;
			AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
			additionalMessageInformation.AM_Broker = "TST";
			Declaration.AdditionalMessageInformation = additionalMessageInformation;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			MessageManager.SendOriginalMessages(new SendsMessagesToCustomsShutterUpperer());
			CUSDECEDIMessage message = (CUSDECEDIMessage)EntryHeader.Messages[0];
			Assert("Message must be an Edifact D09B message", message.EM_MessageText.Contains("UNH+WTG+CUSDEC:D:09B:UN:041+IPTDEC'BGM+914"));
			AssertEquals("Declaration status should have been updated", Core.SGConstants.DeclarationStatus.DeclarationPending, Declaration.JE_EntryStatus);
		}

		[TestDate(2007, 1, 4)]
		public void TestSendMessageOUT()
		{
			AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
			additionalMessageInformation.AM_Broker = "TST";
			Declaration.AdditionalMessageInformation = additionalMessageInformation;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			Declaration.SG_Cert1AdditionalDetails = "CERT DETAILS SHOULD NOT BE INCLUDED";
			MessageManager.SendOriginalMessages(new SendsMessagesToCustomsShutterUpperer());
			CUSDECEDIMessage message = (CUSDECEDIMessage)EntryHeader.Messages[0];
			Assert(message.EM_MessageText.Contains("+OUTDEC'BGM"));
			Assert(message.EM_MessageText.IndexOf("CERT DETAILS SHOULD NOT BE INCLUDED") == -1);
			AssertEquals("Declaration status should have been updated", Core.SGConstants.DeclarationStatus.DeclarationPending, Declaration.JE_EntryStatus);
		}

		[TestDate(2007, 1, 5)]
		public void TestSendMessageOUTWithCO()
		{
			AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
			additionalMessageInformation.AM_Broker = "TST";
			Declaration.AdditionalMessageInformation = additionalMessageInformation;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			Declaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.NA;
			Declaration.SG_Cert1AdditionalDetails = "CERT DETAILS SHOULD BE INCLUDED";
			MessageManager.SendOriginalMessages(new SendsMessagesToCustomsShutterUpperer());
			CUSDECEDIMessage message = (CUSDECEDIMessage)EntryHeader.Messages[0];
			Assert(message.EM_MessageText.Contains("+OUTDEC'BGM"));
			Assert(message.EM_MessageText.Contains("CERT DETAILS SHOULD BE INCLUDED"));
			AssertEquals("Declaration status should have been updated", Core.SGConstants.DeclarationStatus.DeclarationPending, Declaration.JE_EntryStatus);
		}

		[TestDate(2007, 1, 6)]
		public void TestSendMessageTNP()
		{
			AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
			additionalMessageInformation.AM_Broker = "TST";
			Declaration.AdditionalMessageInformation = additionalMessageInformation;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			MessageManager.SendOriginalMessages(new SendsMessagesToCustomsShutterUpperer());
			CUSDECEDIMessage message = (CUSDECEDIMessage)EntryHeader.Messages[0];
			Assert(message.EM_MessageText.Contains("+TNPDEC'BGM"));
			AssertEquals("Declaration status should have been updated", Core.SGConstants.DeclarationStatus.DeclarationPending, Declaration.JE_EntryStatus);
		}

		[TestDate(2007, 1, 7)]
		public void TestSendMessageCOO()
		{
			AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
			additionalMessageInformation.AM_Broker = "TST";
			Declaration.AdditionalMessageInformation = additionalMessageInformation;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			MessageManager.SendOriginalMessages(new SendsMessagesToCustomsShutterUpperer());
			CUSDECEDIMessage message = (CUSDECEDIMessage)EntryHeader.Messages[0];
			Assert(message.EM_MessageText.Contains("+COODEC'BGM"));
			AssertEquals("Declaration status should have been updated", Core.SGConstants.DeclarationStatus.DeclarationPending, Declaration.JE_EntryStatus);
		}

		public void TestMessageIsTestMessage()
		{
			AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
			additionalMessageInformation.AM_Broker = "TST";
			Declaration.AdditionalMessageInformation = additionalMessageInformation;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			MessageManager.SendOriginalMessages(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, EntryHeader.Messages.Count);
			CUSDECEDIMessage message = (CUSDECEDIMessage)EntryHeader.Messages[0];
			AssertEquals("Default is Production message generation", false, message.EM_IsTestMessage);
			SGCustomsDataRegistry.Instance.SendTestMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			MessageManager.SendOriginalMessages(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(2, EntryHeader.Messages.Count);
			message = (CUSDECEDIMessage)EntryHeader.Messages[1];
			AssertEquals("This Should be a Test Message", true, message.EM_IsTestMessage);
		}

		#endregion
		#region TestAmendmentMessage
		[TestDate(2007, 1, 7)]
		public void TestAmendmentAMessageGenericTest()
		{
			AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Amendment, Factory);
			additionalMessageInformation.AM_Broker = "TST";
			Declaration.AdditionalMessageInformation = additionalMessageInformation;
			SupportingDocument supportingDocument = additionalMessageInformation.SupportingDocuments.AddNew();
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			MessageManager.SendAmendmentMessages(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, EntryHeader.Messages.Count);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, Declaration.JE_GS_NKCusAgent);
			AssertEquals(new ZDateTime(2007, 1, 7), Declaration.JE_EntrySubmittedDate);
			CUSDECEDIMessage message = (CUSDECEDIMessage)EntryHeader.Messages[0];
			AssertEquals(CUSDECEDIMessage.Status.Queued, message.EM_Status);
			AssertEquals(MessageTypeCodeList.Codes.INP, message.EM_MessageType);
			AssertEquals(CUSDECEDIMessage.Amendment, message.EM_MessageSubType);
			AssertEquals(1, EntryHeader.Messages.Count);
			AssertEquals(1, EntryHeader.Messages[0].MessageAttachments.Count);
			AssertEquals(Core.SGConstants.DeclarationStatus.AmendmentPending, EntryHeader.CH_Status);
			EDIInterchange sG4Interchange = EntryHeader.Messages[0].Interchange;
			AssertNull("The interchange shuold not be created at this point", sG4Interchange);
		}

		[TestDate(2007, 1, 8)]
		public void TestAmendmentMessageINP()
		{
			AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Amendment, Factory);
			additionalMessageInformation.AM_Broker = "TST";
			Declaration.AdditionalMessageInformation = additionalMessageInformation;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			MessageManager.SendAmendmentMessages(new SendsMessagesToCustomsShutterUpperer());
			CUSDECEDIMessage message = (CUSDECEDIMessage)EntryHeader.Messages[0];
			Assert(message.EM_MessageText.Contains("+INPUPD'BGM+914"));
		}

		[TestDate(2007, 1, 9)]
		public void TestAmendmentMessageIPT()
		{
			AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Amendment, Factory);
			additionalMessageInformation.AM_Broker = "TST";
			Declaration.AdditionalMessageInformation = additionalMessageInformation;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			MessageManager.SendAmendmentMessages(new SendsMessagesToCustomsShutterUpperer());
			CUSDECEDIMessage message = (CUSDECEDIMessage)EntryHeader.Messages[0];
			Assert(message.EM_MessageText.Contains("+IPTUPD'BGM+914"));
		}

		[TestDate(2007, 1, 10)]
		public void TestAmendmentMessageOUT()
		{
			AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Amendment, Factory);
			additionalMessageInformation.AM_Broker = "TST";
			Declaration.AdditionalMessageInformation = additionalMessageInformation;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			Declaration.SG_Cert1AdditionalDetails = "CERT DETAILS SHOULD NOT BE INCLUDED";
			MessageManager.SendAmendmentMessages(new SendsMessagesToCustomsShutterUpperer());
			CUSDECEDIMessage message = (CUSDECEDIMessage)EntryHeader.Messages[0];
			Assert(message.EM_MessageText.Contains("+OUTUPD'BGM+914"));
			Assert(message.EM_MessageText.IndexOf("CERT DETAILS SHOULD NOT BE INCLUDED") == -1);
		}

		[TestDate(2007, 1, 11)]
		public void TestAmendmentMessageOUTWithCO()
		{
			AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Amendment, Factory);
			additionalMessageInformation.AM_Broker = "TST";
			Declaration.AdditionalMessageInformation = additionalMessageInformation;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			Declaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.NA;
			Declaration.SG_Cert1AdditionalDetails = "CERT DETAILS SHOULD BE INCLUDED";
			MessageManager.SendAmendmentMessages(new SendsMessagesToCustomsShutterUpperer());
			CUSDECEDIMessage message = (CUSDECEDIMessage)EntryHeader.Messages[0];
			Assert(message.EM_MessageText.Contains("+OUTUPD'BGM+914"));
			Assert(message.EM_MessageText.Contains("CERT DETAILS SHOULD BE INCLUDED"));
		}

		[TestDate(2007, 1, 12)]
		public void TestAmendmentMessageTNP()
		{
			AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Amendment, Factory);
			additionalMessageInformation.AM_Broker = "TST";
			Declaration.AdditionalMessageInformation = additionalMessageInformation;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			MessageManager.SendAmendmentMessages(new SendsMessagesToCustomsShutterUpperer());
			CUSDECEDIMessage message = (CUSDECEDIMessage)EntryHeader.Messages[0];
			Assert(message.EM_MessageText.Contains("+TNPUPD'BGM+914"));
		}

		public void TestAmendmentMessageIncrementsLineSequenceNumber()
		{
			declaration = null;
			AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Amendment, Factory);
			additionalMessageInformation.AM_Broker = "TST";
			Declaration.AdditionalMessageInformation = additionalMessageInformation;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			MessageManager.SendOriginalMessages(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, EntryHeader.Messages.Count);
			AssertEquals(1, EntryHeader.MergedLines.Count);
			var invoiceHeader = declaration.Invoices[0];
			var invLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invLine2.JI_Tariff = "49011000";
			var invLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invLine3.JI_Tariff = "87032450";
			var invLine4 = invoiceHeader.JobComInvoiceLines.AddNew();
			invLine4.JI_Tariff = "62011000";
			EntryHeader.MergedLines.AddNew();
			EntryHeader.MergedLines.AddNew();
			EntryHeader.MergedLines.AddNew();
			Factory.Save();
			AssertEquals(4, EntryHeader.MergedLines.Count);
			AssertEquals("Original Line number", 1, (int)EntryHeader.MergedLines[0].CL_LineNumber);
			AssertEquals("Added Line numbers are undetermined", ZInt.Zero, EntryHeader.MergedLines[1].CL_LineNumber);
			AssertEquals("Added Line numbers are undetermined", ZInt.Zero, EntryHeader.MergedLines[2].CL_LineNumber);
			AssertEquals("Added Line numbers are undetermined", ZInt.Zero, EntryHeader.MergedLines[3].CL_LineNumber);
			messageManager = null;
			MessageManager.SendAmendmentMessages(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(4, EntryHeader.MergedLines.Count);
			AssertEquals("Line number should now be correctly sequenced", 1, (int)EntryHeader.MergedLines[0].CL_LineNumber);
			AssertEquals("Line number should now be correctly sequenced", 2, (int)EntryHeader.MergedLines[1].CL_LineNumber);
			AssertEquals("Line number should now be correctly sequenced", 3, (int)EntryHeader.MergedLines[2].CL_LineNumber);
			AssertEquals("Line number should now be correctly sequenced", 4, (int)EntryHeader.MergedLines[3].CL_LineNumber);
			AssertEquals("Now has amendment msg", 2, EntryHeader.Messages.Count);
			CUSDECEDIMessage message = (CUSDECEDIMessage)EntryHeader.Messages[1];
			Assert(message.EM_MessageText.Contains("CST+1"));
			Assert(message.EM_MessageText.Contains("CST+2+49011000"));
			Assert(message.EM_MessageText.Contains("CST+3+87032450"));
			Assert(message.EM_MessageText.Contains("CST+4+62011000"));
		}

		#endregion
		#region TestRefundMessage
		[TestDate(2007, 1, 7)]
		public void TestRefundMessageTest()
		{
			RefundAdditionalMessageInformation additionalMessageInformation = new RefundAdditionalMessageInformation(Declaration, Factory);
			additionalMessageInformation.AM_Broker = "TST";
			additionalMessageInformation.AM_RefundCode = UpdateIndicatorCodeList.Codes.FRF;
			Declaration.AdditionalMessageInformation = additionalMessageInformation;
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			SupportingDocument supportingDocument = additionalMessageInformation.SupportingDocuments.AddNew();
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			MessageManager.SendRefundMessage(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, EntryHeader.Messages.Count);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, Declaration.JE_GS_NKCusAgent);
			AssertEquals(new ZDateTime(2007, 1, 7), Declaration.JE_EntrySubmittedDate);
			CUSDECEDIMessage message = (CUSDECEDIMessage)EntryHeader.Messages[0];
			AssertEquals(CUSDECEDIMessage.Status.Queued, message.EM_Status);
			AssertEquals(MessageTypeCodeList.Codes.IPT, message.EM_MessageType);
			AssertEquals(CUSDECEDIMessage.Refund, message.EM_MessageSubType);
			AssertEquals(1, EntryHeader.Messages.Count);
			AssertEquals(1, EntryHeader.Messages[0].MessageAttachments.Count);
			AssertEquals(Core.SGConstants.DeclarationStatus.RefundPending, EntryHeader.CH_Status);
			Assert(message.EM_MessageText.Contains("CST'GEI+5+:Y'FTX+ACD+++FRF"));
			Assert(message.EM_MessageText.Contains("+IPTUPD'BGM+916"));
			AssertEquals("Declaration status should have been updated", Core.SGConstants.DeclarationStatus.RefundPending, Declaration.JE_EntryStatus);
		}

		#endregion
		#region TestCancellationMessage
		[TestDate(2007, 1, 13)]
		public void TestCancellationAMessageGenericTest()
		{
			AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Cancellation, Factory);
			additionalMessageInformation.AM_Broker = "TST";
			Declaration.AdditionalMessageInformation = additionalMessageInformation;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			MergeAndCallCancellationMethod();
			AssertEquals(1, EntryHeader.Messages.Count);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, Declaration.JE_GS_NKCusAgent);
			AssertEquals(new ZDateTime(2007, 1, 13), Declaration.JE_EntrySubmittedDate);
			CUSDECEDIMessage message = (CUSDECEDIMessage)EntryHeader.Messages[0];
			AssertEquals(CUSDECEDIMessage.Status.Queued, message.EM_Status);
			AssertEquals(MessageTypeCodeList.Codes.INP, message.EM_MessageType);
			AssertEquals(CUSDECEDIMessage.Cancellation, message.EM_MessageSubType);
			AssertEquals("Declaration status should have been updated", Core.SGConstants.DeclarationStatus.CancellationPending, Declaration.JE_EntryStatus);
		}

		[TestDate(2007, 1, 14)]
		public void TestCancellationMessageINP()
		{
			AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Cancellation, Factory);
			additionalMessageInformation.AM_Broker = "TST";
			Declaration.AdditionalMessageInformation = additionalMessageInformation;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			MergeAndCallCancellationMethod();
			CUSDECEDIMessage message = (CUSDECEDIMessage)EntryHeader.Messages[0];
			Assert(message.EM_MessageText.Contains("+INPUPD'BGM+915+"));
			AssertEquals("Declaration status should have been updated", Core.SGConstants.DeclarationStatus.CancellationPending, Declaration.JE_EntryStatus);
		}

		[TestDate(2007, 1, 15)]
		public void TestCancellationMessageIPT()
		{
			AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Cancellation, Factory);
			additionalMessageInformation.AM_Broker = "TST";
			Declaration.AdditionalMessageInformation = additionalMessageInformation;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			MergeAndCallCancellationMethod();
			CUSDECEDIMessage message = (CUSDECEDIMessage)EntryHeader.Messages[0];
			Assert(message.EM_MessageText.Contains("+IPTUPD'BGM+915+"));
			AssertEquals("Declaration status should have been updated", Core.SGConstants.DeclarationStatus.CancellationPending, Declaration.JE_EntryStatus);
		}

		[TestDate(2007, 1, 16)]
		public void TestCancellationMessageOUT()
		{
			AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Cancellation, Factory);
			additionalMessageInformation.AM_Broker = "TST";
			Declaration.AdditionalMessageInformation = additionalMessageInformation;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			MergeAndCallCancellationMethod();
			CUSDECEDIMessage message = (CUSDECEDIMessage)EntryHeader.Messages[0];
			Assert(message.EM_MessageText.Contains("+OUTUPD'BGM+915+"));
			AssertEquals("Declaration status should have been updated", Core.SGConstants.DeclarationStatus.CancellationPending, Declaration.JE_EntryStatus);
		}

		[TestDate(2007, 1, 17)]
		public void TestCancellationMessageTNP()
		{
			AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Cancellation, Factory);
			additionalMessageInformation.AM_Broker = "TST";
			Declaration.AdditionalMessageInformation = additionalMessageInformation;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			MergeAndCallCancellationMethod();
			CUSDECEDIMessage message = (CUSDECEDIMessage)EntryHeader.Messages[0];
			Assert(message.EM_MessageText.Contains("+TNPUPD'BGM+915+"));
			AssertEquals("Declaration status should have been updated", Core.SGConstants.DeclarationStatus.CancellationPending, Declaration.JE_EntryStatus);
		}

		void MergeAndCallCancellationMethod()
		{
			Declaration.DoMerge();
			MessageManager.SendCancellationMessages(new SendsMessagesToCustomsShutterUpperer());
		}

		#endregion
		public void TestXMLMessage_INPDEC()
		{
			var info = (AdditionalMessageInformation)Declaration.AdditionalMessageInformation;
			info.AM_Broker = "TST";
			info.SupportingDocuments.AddNew();
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			var entryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders.AddNew();
			entryHeader.Factory.RefreshEnabled = false;
			entryHeader.CH_EntryStatus = "XXX";
			entryHeader.Factory.Save();
			using (SGCustomsDataRegistry.Instance.EnableXMLTradeNetMessaging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				MessageManager.SendOriginalMessages(new SendsMessagesToCustomsShutterUpperer());
				AssertType<CUSDECEDIMessage>("Should be the EDIFACT version as the value of EnableXMLMessage is false.", EntryHeader.Messages.LastMessage);
				MessageManager.SendAmendmentMessages(new SendsMessagesToCustomsShutterUpperer());
				AssertType<CUSDECEDIMessage>("Should be the EDIFACT version as the value of EnableXMLMessage is false.", EntryHeader.Messages.LastMessage);
			}

			using (SGCustomsDataRegistry.Instance.EnableXMLTradeNetMessaging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				MessageManager.SendOriginalMessages(new SendsMessagesToCustomsShutterUpperer());
				var xmlMessage = EntryHeader.Messages.LastMessage;
				AssertType<SGXmlEDIMessage>("Should be the XML version as the value of EnableXMLMessage is now true.", xmlMessage);
				var expectedContent = @"
  <InboundMessage>
    <inp:InNonPayment>
      <inp:Header>
        <cbc:MessageReference>WTGB00001000</cbc:MessageReference>
        <cac:UniqueReferenceNumber>
          <cbc:ID>AAA374M</cbc:ID>";
				AssertContains(expectedContent, xmlMessage.EM_MessageText);
				MessageManager.SendAmendmentMessages(new SendsMessagesToCustomsShutterUpperer());
				var xmlAmendmentMessage = EntryHeader.Messages.LastMessage;
				AssertType<SGXmlEDIMessage>("Should be the XML version as the value of EnableXMLMessage and SendINPUPDMessage are all true.", xmlAmendmentMessage);
				var expectedAmendmentContent = @"
  <InboundMessage>
    <inp:InNonPaymentUpdate>
      <cac:Update>
        <cbc:UpdateIndicatorCode>AME</cbc:UpdateIndicatorCode>
        <cbc:UpdateRequestNumber>1</cbc:UpdateRequestNumber>
        <cbc:UpdatePermitNumber />
      </cac:Update>
      <inp:Declaration>
        <inp:Header>
          <cbc:MessageReference>WTGB00001000</cbc:MessageReference>
          <cac:UniqueReferenceNumber>
            <cbc:ID>AAA374M</cbc:ID>";
				AssertContains(expectedAmendmentContent, xmlAmendmentMessage.EM_MessageText);
			}
		}

		public void TestXMLMessage_IPTDEC()
		{
			var info = (AdditionalMessageInformation)Declaration.AdditionalMessageInformation;
			info.AM_Broker = "TST";
			info.SupportingDocuments.AddNew();
			using (SGCustomsDataRegistry.Instance.EnableXMLTradeNetMessaging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
				var entryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders.AddNew();
				entryHeader.Factory.RefreshEnabled = false;
				entryHeader.CH_EntryStatus = "XXX";
				entryHeader.Factory.Save();
				using (SGCustomsDataRegistry.Instance.EnableXMLTradeNetMessaging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					MessageManager.SendOriginalMessages(new SendsMessagesToCustomsShutterUpperer());
					AssertType<CUSDECEDIMessage>("Should be the EDIFACT version as the value of EnableXMLMessage is false.", EntryHeader.Messages.LastMessage);
					MessageManager.SendAmendmentMessages(new SendsMessagesToCustomsShutterUpperer());
					AssertType<CUSDECEDIMessage>("Should be the EDIFACT version as the value of EnableXMLMessage is false.", EntryHeader.Messages.LastMessage);
				}

				using (SGCustomsDataRegistry.Instance.EnableXMLTradeNetMessaging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					MessageManager.SendOriginalMessages(new SendsMessagesToCustomsShutterUpperer());
					var xmlMessage = EntryHeader.Messages.LastMessage;
					AssertType<SGXmlEDIMessage>("Should be the XML version as the value of EnableXMLMessage and SendINPDECMessage are all true.", xmlMessage);
					var expectedContent = @"
  <InboundMessage>
    <ipt:InPayment>
      <ipt:Header>
        <cbc:MessageReference>WTGB00001000</cbc:MessageReference>
        <cac:UniqueReferenceNumber>
          <cbc:ID>AAA374M</cbc:ID>";
					AssertContains(expectedContent, xmlMessage.EM_MessageText);
				}

				using (SGCustomsDataRegistry.Instance.EnableXMLTradeNetMessaging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					MessageManager.SendAmendmentMessages(new SendsMessagesToCustomsShutterUpperer());
					AssertType<CUSDECEDIMessage>("Should be the EDIFACT version as the value of EnableXMLTradeNetMessaging is false.", EntryHeader.Messages.LastMessage);
				}

				using (SGCustomsDataRegistry.Instance.EnableXMLTradeNetMessaging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					MessageManager.SendAmendmentMessages(new SendsMessagesToCustomsShutterUpperer());
					var xmlAmendmentMessage = EntryHeader.Messages.LastMessage;
					AssertType<SGXmlEDIMessage>("Should be the XML version as the value of EnableXMLMessage and SendIPTUPDMessage are all true.", xmlAmendmentMessage);
					var expectedAmendmentContent = @"
  <InboundMessage>
    <ipt:InPaymentUpdate>
      <cac:Update>
        <cbc:UpdateIndicatorCode>AME</cbc:UpdateIndicatorCode>
        <cbc:UpdateRequestNumber>1</cbc:UpdateRequestNumber>
        <cbc:UpdatePermitNumber />
      </cac:Update>
      <ipt:Declaration>
        <ipt:Header>
          <cbc:MessageReference>WTGB00001000</cbc:MessageReference>
          <cac:UniqueReferenceNumber>
            <cbc:ID>AAA374M</cbc:ID>";
					AssertContains(expectedAmendmentContent, xmlAmendmentMessage.EM_MessageText);
				}
			}
		}

		public void TestExceptionHandled()
		{
			AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
			additionalMessageInformation.AM_Broker = "TST";
			Declaration.AdditionalMessageInformation = additionalMessageInformation;
			SupportingDocument supportingDocument = additionalMessageInformation.SupportingDocuments.AddNew();
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			CusEntryHeader entryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders.AddNew();
			entryHeader.Factory.RefreshEnabled = false;
			entryHeader.CH_EntryStatus = "XXX";
			entryHeader.Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			CusEntryHeader concurrentEntry = factory2.Load<CusEntryHeader>(entryHeader.PK);
			concurrentEntry.CH_EntryStatus = "AAA";
			factory2.Save();
			MessageManager.SendOriginalMessages(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(0, entryHeader.Messages.Count);
		}

		public void TestStatusIsNotUpdatedWithoutGenerationOfInterchange()
		{
			AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
			additionalMessageInformation.AM_Broker = "TST";
			Declaration.AdditionalMessageInformation = additionalMessageInformation;
			var supportingDocument = additionalMessageInformation.SupportingDocuments.AddNew();
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			Factory.Save();
			var entryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders.AddNew();
			entryHeader.Factory.RefreshEnabled = false;
			entryHeader.CH_EntryStatus = "XXX";
			entryHeader.Factory.Save();
			// create concurrent entry clash to force failure of Interchange creation...
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var concurrentEntry = factory2.Load<CusEntryHeader>(entryHeader.PK);
			concurrentEntry.CH_EntryStatus = "AAA";
			factory2.Save();
			MessageManager.SendOriginalMessages(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(0, entryHeader.Messages.Count);
			AssertEquals("Declaration status should not have been updated", ZString.Empty, Declaration.JE_EntryStatus);
		}

		#region Create MessageAttachments

		public void TestCreateMessageAttachmentsWithEDIFACTMessage()
		{
			AssertCreateMessageAttachments(MessageType.EDIFact);
		}

		public void TestCreateMessageAttachmentsWithXmlMessage()
		{
			AssertCreateMessageAttachments(MessageType.XML);
		}

		void AssertCreateMessageAttachments(MessageType messageType)
		{
			using (SGCustomsDataRegistry.Instance.EnableXMLTradeNetMessaging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, messageType == MessageType.XML))
			{
				var edoc = new SupportingDocumentCollectionTest.eDoc { FileName = "CIV - ALBAR À Ã Æ Œ È Ë Õ Ñ Ü Ý MOSTRA 74_PIVENE SINGAPUR (MBE) 08.04.2021.PDF", DocType = "doc", };
				var docs = new SupportingDocumentCollectionTest.eDocs { edoc };
				var additionalMessageInformation = new AdditionalMessageInformation(null, docs, AdditionalMessageInformation.BoundFormTypes.Declaration, Factory, string.Empty);
				additionalMessageInformation.AM_Broker = "TST";
				var document = additionalMessageInformation.SupportingDocuments.AddNew();
				document.eDoc = edoc.UniqueKey;
				Declaration.AdditionalMessageInformation = additionalMessageInformation;
				Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
				var entryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders.AddNew();
				entryHeader.Factory.RefreshEnabled = false;
				entryHeader.CH_EntryStatus = "XXX";
				entryHeader.Factory.Save();
				MessageManager.SendOriginalMessages(new SendsMessagesToCustomsShutterUpperer());
				var message = EntryHeader.Messages.LastMessage;
				var ediMessageAttach = (EDIMessageAttach)message.MessageAttachments.Single();
				CombineAssertions(() =>
				{
					AssertEquals("doc - CIV_-_ALBAR_MOSTRA_74_PIVENE_SINGAPUR_(MBE)_08.04.2021.PDF", ediMessageAttach.EG_FileName);
					AssertEquals(edoc.UniqueKey, ediMessageAttach.EG_StorageDocsGuid);
				}

				);
			}
		}

		public void TestCreateMessageAttachmentHasValidFileName()
		{
			using (SGCustomsDataRegistry.Instance.EnableXMLTradeNetMessaging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var edoc = new SupportingDocumentCollectionTest.eDoc { FileName = "CIV - CIÂ PLÂ -Â POÂ 1693715Â -Â SGN+MYÂ -Â FINAL.PDF", DocType = "doc", };
				var docs = new SupportingDocumentCollectionTest.eDocs { edoc };
				var additionalMessageInformation = new AdditionalMessageInformation(null, docs, AdditionalMessageInformation.BoundFormTypes.Declaration, Factory, string.Empty);
				additionalMessageInformation.AM_Broker = "TST";
				var document = additionalMessageInformation.SupportingDocuments.AddNew();
				document.eDoc = edoc.UniqueKey;
				Declaration.AdditionalMessageInformation = additionalMessageInformation;
				Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
				var entryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders.AddNew();
				entryHeader.Factory.RefreshEnabled = false;
				entryHeader.CH_EntryStatus = "XXX";
				entryHeader.Factory.Save();
				MessageManager.SendOriginalMessages(new SendsMessagesToCustomsShutterUpperer());
				var message = EntryHeader.Messages.LastMessage;
				var ediMessageAttach = (EDIMessageAttach)message.MessageAttachments.Single();
				CombineAssertions(() =>
				{
					AssertEquals("doc - CIV_-_CI_PL_-_PO_1693715_-_SGNMY_-_FINAL.PDF", ediMessageAttach.EG_FileName);
					AssertEquals(edoc.UniqueKey, ediMessageAttach.EG_StorageDocsGuid);
				}

				);
			}
		}

		#endregion

		#region Message Manager
		IMessageManager MessageManager
		{
			get
			{
				return messageManager ?? (messageManager = new MessageManagerTestClass(Declaration));
			}
		}

		MessageManagerTestClass messageManager;
		protected virtual IMessageManager GetMessageManagerCore(JobDeclaration declarationForTest)
		{
			return new MessageManagerTestClass(declarationForTest);
		}

		#endregion

		#region Declaration
		JobDeclarationForTest Declaration
		{
			get
			{
				if (declaration == null)
				{
					GlbStaff testSGBroker = Factory.New<GlbStaff>();
					testSGBroker.GS_Code = "TST";
					testSGBroker.GS_FullName = "Test SG4 Broker";
					var wrapper = SGGlbStaffWrapper.Get(testSGBroker);
					wrapper.Tradenetv4Password.GP_UserID = "ASDF23L";
					GlbStaff.CurrentUser.GS_Code = testSGBroker.GS_Code;
					declaration = Factory.New<JobDeclarationForTest>();
					declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
					declaration.JE_GS_NKCusAgent = testSGBroker.GS_Code;
					JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
					invoiceHeader.JobComInvoiceLines.AddNew();
					invoiceHeader.JZ_IncoTerm = Constants.IncoTerms.FreeOnBoard;
					AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
					declaration.AdditionalMessageInformation = additionalMessageInformation;
				}

				return declaration;
			}
		}

		JobDeclarationForTest declaration;
		#endregion

		#region EntryHeader
		CusEntryHeader EntryHeader
		{
			get
			{
				return (CusEntryHeader)Declaration.ActiveEntryHeaders[0];
			}
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			SGCustomsDataRegistry.Instance.EnableXMLTradeNetMessaging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		#region Test Class
		public class MessageManagerTestClass : MessageManager
		{
			public MessageManagerTestClass(JobDeclaration declaration) : base(declaration)
			{
			}

			protected override ZString MessageApplicationCode => ApplicationCodeList.Codes.SGCustomsTradenet4;
		}

		class JobDeclarationForTest : JobDeclaration
		{
			public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
				DPSPartiesForTesting = Array.Empty<ScreeningParty>();
			}

			protected override bool IsDPSFreightMovementRestrictedCore()
			{
				return DPSFreightMovementRestricted;
			}

			public bool DPSFreightMovementRestricted
			{
				get;
				set;
			}

			protected override ScreeningParty[] GetScreeningPartiesCore()
			{
				return DPSPartiesForTesting;
			}

			public ScreeningParty[] DPSPartiesForTesting
			{
				get;
				set;
			}
		}
		#endregion
	}
}
