using System;
using System.Data;
using System.Text;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.TradeSingleWindow.Testing
{
	[TestedType(typeof(TSWMessage))]
	public class TSWMessageTest : EDIMessageTest
	{
		public void TestSetEM_StatusToFailedResetsDTRStatus() => CombineAssertions(() =>
		{
			var transhipmentRequest = Factory.New<TranshipmentRequest>();
			transhipmentRequest.C4_Status = CombinedMovementStatus.Codes.STC;
			var message = Factory.New<TSWMessage>();
			message.EM_LinkedObject = transhipmentRequest;
			AssertEquals("Precondition: C4_Status = 'STC'", CombinedMovementStatus.Codes.STC, transhipmentRequest.C4_Status);

			message.EM_Status = EDIMessage.Status.Failed;
			AssertEquals("C4_Status is cleared when EM_Status = 'FAL'", ZString.Empty, transhipmentRequest.C4_Status);
		});

		public void TestMsgTransMode()
		{
			var message = Factory.New<TSWMessage>();
			message.EM_MessageType = MessageTypeList.Codes.ANA;
			AssertEquals(Enterprise.Customs.NZ.Business.MsgTransportList.Codes.TSW, message.MsgTransMode);
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var messageLoaded = factory.Load<NZCMessage>(message.PK);//should load it as TSWMessage
			AssertEquals(Enterprise.Customs.NZ.Business.MsgTransportList.Codes.TSW, messageLoaded.MsgTransMode);
		}

		public void TestFormattedMessageDoesNotIncludesEM_MessageInterpretation()
		{
			var message = Factory.New<TSWMessage>();
			message.EM_MessageType = MessageTypeList.Codes.OCR;
			message.EM_MessageText = @"
<Line1>Hello<\Line1>
   <Child1>thing<\Child1>
<Something Else>FUGGER<\Something Else>
".Trim();
			message.EM_MessageInterpretation = "Interpreted\r\nMessage";

			var expectedresult = @"
<Line1>Hello<\Line1>
   <Child1>thing<\Child1>
<Something Else>FUGGER<\Something Else>
".Trim();
			AssertMultilineASCIIEquals("message.EM_FormattedMessageText Should NOT Include EM_MessageInterpretation", expectedresult.Trim(), message.EM_FormattedMessageText);
		}

		public void TestMessageInterpretationForSentMessages()
		{
			var message = Factory.New<TSWMessage>();
			message.EM_MessageType = MessageTypeList.Codes.OCR;
			message.EM_MessageSubType = "ORG";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_ApplicationReference = "JobNo5";
			message.EM_MessageText = @"
<Line1>Hello<\Line1>
   <Child1>thing<\Child1>
<Something Else>FUGGER<\Something Else>
".Trim();
			var expectedResult = @"Original OCR sent to Customs
Functional Reference ID: JobNo5
Declarant: ";
			AssertMultilineASCIIEquals("EM_MessageInterpretation should show basic info for transmitting messages", expectedResult, message.EM_MessageInterpretation);
		}

		public void TestMessageDefaults()
		{
			NZCustomsDataRegistry.Instance.ExportOrnTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var message = Factory.New<TSWMessage>();
			message.EM_MessageType = MessageTypeList.Codes.OCR;
			AssertEquals("EM_ApplicationCode", TSWMessage.ApplicationCodes.NewZealandCustoms, message.EM_ApplicationCode);
			AssertEquals("EM_ReceiveTransmit", TSWMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("EM_Status", TSWMessage.Status.Queued, message.EM_Status);
			AssertEquals("EM_IsTestMessage", true, message.EM_IsTestMessage);
		}

		public void TestEM_MessageSubTypeDescription()
		{
			var message = Factory.New<TSWMessage>();
			message.EM_MessageType = MessageTypeList.Codes.OCR;
			AssertEquals("message.EM_MessageSubTypeDescription", ZString.Empty, message.EM_MessageSubTypeDescription);
			message.EM_MessageSubType = TransactionTypeList.Codes.Original;
			AssertEquals("message.EM_MessageSubTypeDescription", TransactionTypeList.Descriptions.Original, message.EM_MessageSubTypeDescription);
			message.EM_MessageSubType = TransactionTypeList.Codes.Replace;
			AssertEquals("message.EM_MessageSubTypeDescription", TransactionTypeList.Descriptions.Replace, message.EM_MessageSubTypeDescription);
			message.EM_MessageSubType = TransactionTypeList.Codes.Cancel;
			AssertEquals("message.EM_MessageSubTypeDescription", TransactionTypeList.Descriptions.Cancel, message.EM_MessageSubTypeDescription);
			message.EM_MessageSubType = "ZXZ";
			AssertEquals("message.EM_MessageSubTypeDescription", "ZXZ", message.EM_MessageSubTypeDescription);
		}

		public void TestTSWMessageGetADeclarationReferenceWhenTSWMessageIsSavedAndTheBranchComesFromTheDeclaration()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_GB = branch.PK;

			var message = declaration.Messages.AddNew(typeof(TSWMessage));
			message.EM_MessageType = MessageTypeList.Codes.OCR;
			message.EM_MessageText = TSWConstants.SendersReferencePlaceHolder + ":" + TSWMessage.MessageNumberPlaceHolder + "'";
			Factory.Save();
			AssertEquals("Precondition: Declaration.JE_DeclarationReference", "B00001000", declaration.JE_DeclarationReference);
			AssertEquals("Precondition: Message.EM_MessageNum", "1", message.EM_MessageNum);
			AssertEquals("B00001000:1'", message.EM_MessageText);
			AssertEquals("Message.EM_GB", branch.PK, message.EM_GB);
		}

		public void TestTSWMessageSenderReference()
		{
			Db.Connection.BeginTransaction();
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			var entryHeader = declaration.CusEntryHeader;
			entryHeader.CH_BGMReference = TSWConstants.SendersReferencePlaceHolder;
			var message = entryHeader.Messages.AddNew(typeof(TSWMessage));
			message.EM_MessageType = "";
			message.EM_MessageText = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<DocumentMetadata xmlns=\"urn:wco:datamodel:WCO:DM:1\">\n<WCODataModelVersion>3.2</WCODataModelVersion>\n<WCODocumentName>IM</WCODocumentName>\n<CountryCode>NZ</CountryCode>\n<AgencyAssignedCustomizedDocumentName>IM1</AgencyAssignedCustomizedDocumentName>\n<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>\n<Declaration xmlns=\"urn:wco:datamodel:WCO:DeclarationModel:1\">\r\n  <ID />\r\n  <TypeCode>I10</TypeCode>\r\n  <FunctionalReferenceID>" + TSWConstants.SendersReferencePlaceHolder + "</FunctionalReferenceID>\r\n  <FunctionCode>1</FunctionCode>\r\n  <Submitter>\r\n    <ID>00009917B</ID>\r\n  </Submitter>\r\n  <Declarant>\r\n    <ID />\r\n  </Declarant>\r\n</Declaration>\n</DocumentMetadata>";
			Factory.Save();
			AssertEquals("Precondition: Declaration.JE_DeclarationReference", "B00001000", declaration.JE_DeclarationReference);
			AssertEquals("Precondition: Message.EM_MessageNum", "1", message.EM_MessageNum);
			AssertEquals("Message Text should have had sender reference placeholder replaced", true, message.EM_MessageText.Contains("<FunctionalReferenceID>B00001000</FunctionalReferenceID>"));
			AssertEquals("EM_ApplicationReference", "B00001000", message.EM_ApplicationReference);
			AssertEquals("CH_BGMReference", "B00001000", entryHeader.CH_BGMReference);
			Db.Connection.RollbackTransaction();
		}

		public void TestTSWMessageSenderReferenceWhenUserGeneratedLongJobNumber()
		{
			Db.Connection.BeginTransaction();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.Sight;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_DeclarationReference = "BSYDAIR00429810";
			var entryHeader = declaration.CusEntryHeader;
			var message = entryHeader.Messages.AddNew(typeof(TSWMessage));
			message.EM_MessageType = MessageTypeList.Codes.I51;
			message.EM_MessageText = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<DocumentMetadata xmlns=\"urn:wco:datamodel:WCO:DM:1\">\n<WCODataModelVersion>3.2</WCODataModelVersion>\n<WCODocumentName>IM</WCODocumentName>\n<CountryCode>NZ</CountryCode>\n<AgencyAssignedCustomizedDocumentName>IM1</AgencyAssignedCustomizedDocumentName>\n<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>\n<Declaration xmlns=\"urn:wco:datamodel:WCO:DeclarationModel:1\">\r\n  <ID />\r\n  <TypeCode>I51</TypeCode>\r\n  <FunctionalReferenceID>" + TSWConstants.SendersReferencePlaceHolder + "</FunctionalReferenceID>\r\n  <FunctionCode>1</FunctionCode>\r\n  <Submitter>\r\n    <ID>00009917B</ID>\r\n  </Submitter>\r\n  <Declarant>\r\n    <ID />\r\n  </Declarant>\r\n</Declaration>\n</DocumentMetadata>";
			Factory.Save();
			AssertEquals("Precondition: Declaration.JE_DeclarationReference", "BSYDAIR00429810", declaration.JE_DeclarationReference);
			AssertEquals("Precondition: Message.EM_MessageNum", "1", message.EM_MessageNum);
			AssertEquals("Message Text should have had sender reference placeholder replaced", true, message.EM_MessageText.Contains("<FunctionalReferenceID>SIT00000001</FunctionalReferenceID>"));
			AssertEquals("EM_ApplicationReference", "SIT00000001", message.EM_ApplicationReference);
			AssertEquals("CH_BGMReference", "SIT00000001", entryHeader.CH_BGMReference);
			Db.Connection.RollbackTransaction();
		}

		public void TestTSWMessageSenderReferenceFromConsol()
		{
			Db.Connection.BeginTransaction();
			var consol = Factory.New<ForwardingConsol>();
			var message = consol.Messages.AddNew(typeof(TSWMessage));
			message.EM_MessageType = "";
			message.EM_MessageText = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<DocumentMetadata xmlns=\"urn:wco:datamodel:WCO:DM:1\">\n<WCODataModelVersion>3.2</WCODataModelVersion>\n<WCODocumentName>IM</WCODocumentName>\n<CountryCode>NZ</CountryCode>\n<AgencyAssignedCustomizedDocumentName>IM1</AgencyAssignedCustomizedDocumentName>\n<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>\n<Declaration xmlns=\"urn:wco:datamodel:WCO:DeclarationModel:1\">\r\n  <ID />\r\n  <TypeCode>IPI</TypeCode>\r\n  <FunctionalReferenceID>" + TSWConstants.SendersReferencePlaceHolder + "</FunctionalReferenceID>\r\n  <FunctionCode>1</FunctionCode>\r\n  <Submitter>\r\n    <ID>00009917B</ID>\r\n  </Submitter>\r\n  <Declarant>\r\n    <ID />\r\n  </Declarant>\r\n</Declaration>\n</DocumentMetadata>";
			Factory.Save();
			AssertEquals("Precondition: consol.JobNumber", "C00001000", consol.JobNumber);
			AssertEquals("Precondition: Message.EM_MessageNum", "1", message.EM_MessageNum);
			AssertEquals("Message Text should have had sender reference placeholder replaced", true, message.EM_MessageText.Contains("<FunctionalReferenceID>C00001000</FunctionalReferenceID>"));
			AssertEquals("EM_ApplicationReference", "C00001000", message.EM_ApplicationReference);
			Db.Connection.RollbackTransaction();
		}

		public void TestResetToQueue()
		{
			var message = Factory.New<TSWMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Sent;

			message.ResetToQueuedStatus();
			Factory.Save();

			var messageLoaded = Factory.Load<TSWMessage>(message.PK);
			AssertEquals(typeof(TSWMessage), messageLoaded.GetType());
			AssertEquals("Status should be queued", EDIMessage.Status.Queued, message.EM_Status);
		}

		public void TestResetToQueuedStatusDoesNotResetMessageTypeSubType()
		{
			var message = Factory.New<TSWMessage>();
			message.EM_MessageType = MessageTypeList.Codes.TWR;
			message.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms;
			message.ResetToQueuedStatus();
			AssertEquals("EM_MessageType", MessageTypeList.Codes.TWR, message.EM_MessageType);
			AssertEquals("EM_MessageSubType", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, message.EM_MessageSubType);
		}

		public void TestMessageInterpretationIncludesAttachments()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = "NZC";
			declaration.JE_DeclarationReference = "B00002614";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("%DOCX <Invoice> %EOF\n"), "file1.docx", "INV");
			AssertEquals("Declaration document should be available for selection in attachment drop down control", 1, declaration.eDocsForSelection.Count);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var message = entryHeader.Messages.AddNew(typeof(TSWMessage));
			message.EM_MessageType = declaration.MappedTSWMessageSubType;
			message.EM_ApplicationReference = declaration.JE_DeclarationReference;
			message.EM_IsTestMessage = declaration.IsInTestMode;
			message.EM_LinkedObject = entryHeader;
			message.EM_MessageSubType = "ORG";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf - 8""?>
<DocumentMetadata xmlns = ""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>IM</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>IM1</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
	<Declaration xmlns = ""urn:wco:datamodel:WCO:DeclarationModel:1"">
		<TypeCode>I10</TypeCode>
		<FunctionalReferenceID>B00002614</FunctionalReferenceID>
		<FunctionCode>9</FunctionCode>
		<TotalGrossMassMeasure unitCode = ""KGM"">100</TotalGrossMassMeasure>
		<JurisdictionDateTime formatCode = ""102"">20160727</JurisdictionDateTime>
		<Submitter>
			<ID>00009908C</ID>
		</Submitter>
		<AdditionalDocument>
			<CategoryCode>INV</CategoryCode>
			<ImageBinaryObject mimeCode = ""application/vnd.openxmlformats-officedocument.wordprocessingml.document"" filename = ""file1.docx"">ATTACHED</ImageBinaryObject>
		</AdditionalDocument>
		<AdditionalInformation>
		   <Content>Test maximum size attachment file (10, 240, 000 bytes)</Content>
		</AdditionalInformation>
		<AdditionalInformation>
			<StatementCode>PDO</StatementCode>
			<StatementTypeCode>OIN</StatementTypeCode>
		</AdditionalInformation>
		<AdditionalInformation>
			<StatementDescription>tes099,ADBOOKS LTD (NZ CUSTOMS)</StatementDescription>
			<StatementTypeCode>MAC</StatementTypeCode>
		</AdditionalInformation>
		<Agent>
			<ID>00009908C</ID>
			<RoleCode>CB</RoleCode>
		</Agent>
	</Declaration>
</DocumentMetadata>".Trim();

			var additionalMessageInformation = new Business.TradeSingleWindow.AdditionalMessageInformation(null, declaration.eDocsForSelection, TSWTransactionTypes.Original, declaration.Factory, "");
			additionalMessageInformation.GatherPotentialSupportingDocuments();
			IStorageDocsBaseCollection documentCollection = declaration.eDocsForSelection[0];
			IeDoc attachment = documentCollection[0];
			additionalMessageInformation.SupportingDocuments.Add((BusinessObject)attachment);

			foreach (IeDoc cusAttachment in additionalMessageInformation.SupportingDocuments)
			{
				EDIMessageAttach ediMessageAttach = message.MessageAttachments.AddNew();
				ediMessageAttach.EG_FileName = cusAttachment.FileName;
				ediMessageAttach.EG_StorageDocsGuid = cusAttachment.UniqueKey;
				ediMessageAttach.EG_EdiMsgDocType = cusAttachment.DocType;
			}

			var messageAttached = message.MessageAttachments[0];
			AssertEquals("attachment is linked to message", messageAttached.EG_EM, message.PK);
			var attachedDoc = messageAttached.GetAttachment();
			AssertEquals("Should find document attached to declaration that was selected for inclusion with this message", attachment, attachedDoc);
			AssertEquals("attached eDoc file name", "file1.docx", attachedDoc.FileName);

			var expectedResult = @"Original I10 sent to Customs
Functional Reference ID: B00002614
Declarant: 

Attached Documents:
INV	file1.docx";
			AssertMultilineASCIIEquals("EM_MessageInterpretation should show attached document info for transmitting messages", expectedResult, message.EM_MessageInterpretation);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
		}
		#endregion
	}

	[TestedType(typeof(TSWMessage))]
	public class TSWMessageInternalTest : ZArchitecture.Business.Testing.EnterpriseBusinessObjectTestCase
	{
		public void TestGetSendersReferenceEntryHeader()
		{
			var entryMessage = Factory.New<TSWMessage_ForTesting>();
			entryMessage.EM_LinkedObject = Factory.New(typeof(CusEntryHeader));
			((CusEntryHeader)entryMessage.EM_LinkedObject).CH_BGMReference = "T123488/W";
			AssertEquals("SendersReference from dbo.CusEntryHeader", "T123488W", entryMessage.Call_GetSendersReference());
		}

		public void TestGetSendersReferenceDeclaration()
		{
			var decMessage = Factory.New<TSWMessage_ForTesting>();
			decMessage.EM_LinkedObject = Factory.New(typeof(JobDeclaration));
			((JobDeclaration)decMessage.EM_LinkedObject).JE_DeclarationReference = "B000293827";
			AssertEquals("SendersReference from Declaration", "B000293827", decMessage.Call_GetSendersReference());
		}

		public void TestGetSendersReferenceConsol()
		{
			var consolMessage = Factory.New<TSWMessage_ForTesting>();
			consolMessage.EM_LinkedObject = Factory.New(typeof(ForwardingConsol));
			((ForwardingConsol)consolMessage.EM_LinkedObject).JK_UniqueConsignRef = "C004892828";
			AssertEquals("SendersReference from Consol", "C004892828", consolMessage.Call_GetSendersReference());
		}

		public void TestGetSendersReferenceMAWB()
		{
			var mawbMessage = Factory.New<TSWMessage_ForTesting>();
			mawbMessage.EM_LinkedObject = Factory.New(typeof(Business.Express.CusMAWB));
			((Business.Express.CusMAWB)mawbMessage.EM_LinkedObject).CM_MessageReference = "MB/729320";
			AssertEquals("SendersReference from MAWB", "MB729320", mawbMessage.Call_GetSendersReference());
		}

		public void TestConsolReferenceIsSetAppropriately()
		{
			Db.Connection.BeginTransaction();
			var consolMessage = Factory.New<TSWMessage_ForTesting>();
			var consol = Factory.New<ForwardingConsol>();
			consolMessage.EM_LinkedObject = consol;
			consol.JK_UniqueConsignRef = "C004892828";
			var msgRefNum = consol.Factory.New<Common.CusEntryNumber>();
			msgRefNum.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
			msgRefNum.CE_RN_NKCountryCode = Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			msgRefNum.CE_ParentTable = ForwardingConsol.Schema.TableName;
			msgRefNum.CE_ParentID = consol.PK;
			msgRefNum.CE_EntryLineReference = TSWConstants.SendersReferencePlaceHolder;

			AssertEquals("SendersReference from Consol should be obtained from number factory when place holder used", "OCR00000001", consolMessage.Call_GetSendersReference());
			Db.Connection.RollbackTransaction();
		}

		public void TestReferenceGeneratedWhenIsWriteOffChangedToFormal()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_DeclarationReference = "B000429810";

			var entryHeader = (Business.Declaration.ECIWriteOff.CusEntryHeader)declaration.CusEntryHeader;
			entryHeader.CH_LastEntryStyle = JobMessageSubTypeList.Codes.WriteOff;

			var entryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNum.CE_EntryNum = "75328491";
			entryNum.CE_EntryType = CusEntryNumberTypeList.Codes.ECIWriteOff;
			entryNum.CE_ParentID = entryHeader.PK;
			entryNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			Factory.Save();

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Factory.Save();

			var formalEntryHeader = (Business.Declaration.FormalEntry.CusEntryHeader)declaration.CustomsEntryHeaders[1];
			var entryMessage = Factory.New<TSWMessage_ForTesting>();
			entryMessage.EM_LinkedObject = formalEntryHeader;
			AssertEquals("SenderReferenceNumber when IsWriteOffChangedToFormal needs to be a new unique reference so should use the write-off entry reference appended with 'F'", "B000429810F", entryMessage.Call_GetSendersReference());
		}

		public void TestReferenceGeneratedWhenFormalIsChangedToIPI()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_DeclarationReference = "B000429810";

			var entryHeader = (Business.Declaration.FormalEntry.CusEntryHeader)declaration.CusEntryHeader;
			entryHeader.CH_LastEntryStyle = JobMessageSubTypeList.Codes.Normal;

			var entryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNum.CE_EntryNum = "75328491";
			entryNum.CE_EntryType = CusEntryNumberTypeList.Codes.FormalEntry;
			entryNum.CE_ParentID = entryHeader.PK;
			entryNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			Factory.Save();

			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Factory.Save();

			var ipiEntryHeader = (Business.Declaration.FormalEntry.PrimaryIndustriesCusEntryHeader)declaration.CustomsEntryHeaders.AddNew(typeof(Business.Declaration.FormalEntry.PrimaryIndustriesCusEntryHeader));
			var entryMessage = Factory.New<TSWMessage_ForTesting>();
			entryMessage.EM_LinkedObject = ipiEntryHeader;
			AssertEquals("SenderReferenceNumber when IsFormalChangedToIPI needs to be a new unique reference so should use the original formal entry reference appended with 'I'", "B000429810I", entryMessage.Call_GetSendersReference());
		}

		public void TestFormalChangedToIPIReferenceOnAmendmentToIPI()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_DeclarationReference = "B000429810";

			var entryHeader = (Business.Declaration.FormalEntry.CusEntryHeader)declaration.CusEntryHeader;
			entryHeader.CH_LastEntryStyle = JobMessageSubTypeList.Codes.Normal;

			var entryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNum.CE_EntryNum = "75328491";
			entryNum.CE_EntryType = CusEntryNumberTypeList.Codes.FormalEntry;
			entryNum.CE_ParentID = entryHeader.PK;
			entryNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			Factory.Save();

			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Factory.Save();

			var ipiEntryHeader = (Business.Declaration.FormalEntry.PrimaryIndustriesCusEntryHeader)declaration.CustomsEntryHeaders[1];
			var entryMessage = Factory.New<TSWMessage_ForTesting>();
			entryMessage.EM_LinkedObject = ipiEntryHeader;
			AssertEquals("SenderReferenceNumber when IsFormalChangedToIPI needs to be a new unique reference so should use the original formal entry reference appended with 'I'", "B000429810I", entryMessage.Call_GetSendersReference());

			var responseMessage = Factory.New<TSWMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_LinkedObject = ipiEntryHeader;

			var amendedIPIMessage = Factory.New<TSWMessage_ForTesting>();
			amendedIPIMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			amendedIPIMessage.EM_LinkedObject = ipiEntryHeader;
			AssertEquals("SenderReferenceNumber when an IPI is amended remains with the original IPI reference that was generated", "B000429810I", amendedIPIMessage.Call_GetSendersReference());
		}

		public void TestStandardIPIDeclarationReference()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_DeclarationReference = "B000372110";

			var ipiEntryHeader = declaration.CusEntryHeader;
			var entryMessage = Factory.New<TSWMessage_ForTesting>();
			entryMessage.EM_LinkedObject = ipiEntryHeader;
			AssertEquals("SenderReferenceNumber for IPI entry should ALWAYS be the job reference number appended with 'I'", "B000372110I", entryMessage.Call_GetSendersReference());

			var amendedIPIMessage = Factory.New<TSWMessage_ForTesting>();
			amendedIPIMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			amendedIPIMessage.EM_LinkedObject = ipiEntryHeader;
			AssertEquals("SenderReferenceNumber when an IPI is amended remains with the IPI job reference that was originally generated", "B000372110I", amendedIPIMessage.Call_GetSendersReference());
		}

		class TSWMessage_ForTesting : TSWMessage
		{
			public TSWMessage_ForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public string Call_GetSendersReference()
			{
				return GetSendersReference();
			}
		}
	}
}
