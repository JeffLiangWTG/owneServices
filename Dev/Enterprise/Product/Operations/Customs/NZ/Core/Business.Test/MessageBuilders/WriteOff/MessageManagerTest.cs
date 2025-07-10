using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Messaging.Business;
using CusEntryHeader = Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.CusEntryHeader;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.ECIWriteOff.Testing
{
	using Enterprise.BatchProcessor;
	using Enterprise.Customs.NZ.Business.Declaration.Testing;
	using Enterprise.Customs.NZ.Business.MessageProcessors;
	using Enterprise.Customs.NZ.Business.Testing;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.Customs.NZ.TradeSingleWindow;
	using Enterprise.MasterFiles.Business;
	using NUnit.Framework;

	[TestedType(typeof(MessageManager))]
	public class MessageManagerTest : MessageBuilders.Testing.MessageManagerForDeclarationTest
	{
		[TestDate(2019, 09, 01)]
		public void TestValidateMessagingPreconditions_GSTNumberAndPrepaid()
		{
			Declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_DateOfArrival = ZDate.Today;
			var invoiceHeader1 = Declaration.Invoices[0];
			invoiceHeader1.JZ_SupplierGSTNumber = "AA111";
			invoiceHeader1.JZ_IsGSTPrePaid = "Y";
			var invoiceHeader2 = Declaration.Invoices.AddNew();
			invoiceHeader2.JZ_SupplierGSTNumber = "BB222";
			invoiceHeader2.JZ_IsGSTPrePaid = "N";

			var manager = GetNewManager(MessageManager.OperationType.SubmitMessage);
			Assert(!manager.Execute());
			AssertEquals("Cannot Send when the Supplier GST Number / Prepaid combination is not the same on all Invoices.", manager.LastHumanReadableStatus);

			invoiceHeader2.JZ_SupplierGSTNumber = "AA111";
			invoiceHeader2.JZ_IsGSTPrePaid = "Y";

			var retryManager = GetNewManager(MessageManager.OperationType.SubmitMessage);
			Assert(retryManager.Execute());
			AssertEquals("ICR Original Entry Message Generated and Ready to be sent by Service Tasks.", retryManager.LastHumanReadableStatus);
		}

		public void TestEverythingGetsResetProperlyOnASaveFailureOnANewJob()
		{
			JobDeclaration declaration = Declaration;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			MessageManager manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			CusEntryLineFee spannerInTheWorks = Factory.New<CusEntryLineFee>(); // Unassociated CusEntryLineFee should blow the save at the lower level Factory / DB Level...
			spannerInTheWorks.CF_ChargeAmount = -1m; // ...but it has to have HasChanges set to even TRY and save.
			ZString declarationEntryStatusBeforeSend = declaration.JE_EntryStatus;
			AssertEquals("manager.Execute()", false, manager.Execute());

			CusEntryHeader entryHeader = EntryHeader;
			AssertEquals("entryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.NotSentToCustoms, entryHeader.CH_EntryStatus);
			AssertEquals("declaration.JE_EntryStatus", declarationEntryStatusBeforeSend, declaration.JE_EntryStatus);
			AssertEquals("declaration.JE_EntrySubmittedDate", ZDateTime.Empty, declaration.JE_EntrySubmittedDate);
			AssertEquals("entryHeader.Messages.Count", 0, entryHeader.Messages.Count);
		}

		public void TestEnteredRemarksReadOnlyWhenQueueForManifestingTicked()
		{
			MessageManager messageManager = new MessageManager(Declaration, MessageManager.OperationType.SubmitMessage);

			Declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.NotSentToCustoms;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			AssertEquals("MessageManager.EnteredRemarksInfo.ReadOnly", false, messageManager.EnteredRemarksInfo.ReadOnly);
			messageManager.QueueForManifesting = true;
			AssertEquals("MessageManager.EnteredRemarksInfo.ReadOnly", true, messageManager.EnteredRemarksInfo.ReadOnly);
			messageManager.QueueForManifesting = false;
			AssertEquals("MessageManager.EnteredRemarksInfo.ReadOnly", false, messageManager.EnteredRemarksInfo.ReadOnly);
		}

		public virtual void TestCanQueueForManifesting()
		{
			MessageManager messageManager = new MessageManager(Declaration, MessageManager.OperationType.SubmitMessage);

			Declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.NotSentToCustoms;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			AssertEquals("MessageManager.CanQueueForManifesting", true, messageManager.CanQueueForManifesting);
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			AssertEquals("MessageManager.CanQueueForManifesting", false, messageManager.CanQueueForManifesting);

			Declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			AssertEquals("MessageManager.CanQueueForManifesting", false, messageManager.CanQueueForManifesting);
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			AssertEquals("MessageManager.CanQueueForManifesting", false, messageManager.CanQueueForManifesting);
		}

		public void TestDeclarationGetsSetToQueuedForManifestingWhenQueueForManifestingIsSet()
		{
			MessageManager messageManager = new MessageManager(Declaration, MessageManager.OperationType.SubmitMessage);
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			AssertEquals("MessageManager.CanQueueForManifesting", true, messageManager.CanQueueForManifesting);
			messageManager.EnteredRemarks = "";
			messageManager.QueueForManifesting = true;
			messageManager.Execute();
			AssertEquals("Declaration.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ReadyForManifesting, Declaration.JE_EntryStatus);
			AssertEquals("Declaration.JE_ECI_LastResponseStatus", LowValueConsignmentStatusList.Codes.NoStatusReported, Declaration.JE_ECI_LastResponseStatus);
			AssertEquals("Declaration.CusEntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.NotSentToCustoms, Declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("MessageManager.CanQueueForManifesting", true, messageManager.CanQueueForManifesting);

			Declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			AssertEquals("MessageManager.CanQueueForManifesting", false, messageManager.CanQueueForManifesting);
			AssertEquals("Declaration.IsInDatabase", true, Declaration.IsInDatabase);
		}

		public void TestGetErrorTextForNotes()
		{
			Declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			Declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			Declaration.DeclarationNumber = "12345678";
			MessageManager messageManager = new MessageManager(Declaration, MessageManager.OperationType.SubmitMessage);
			messageManager.EnteredRemarks = "";
			ZString result = messageManager.GetErrorsForEnteredValues();
			AssertEquals("Invalid Remarks: Must have Remarks sending any amendment messages.", result);
			messageManager.EnteredRemarks = "Cuckoo Squeakers";
			result = messageManager.GetErrorsForEnteredValues();
			AssertEquals("Result should be empty", "", result);
		}

		public override void TestMessageTypeToBeSentRejection()
		{
			MessageManager manager = GetNewManager(MessageManager.OperationType.SubmitMessage);
			Declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.ManifestRejected;
			Declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;
			Declaration.DeclarationNumber = "";
			AssertEquals(MessageManager.MessageType.Original, manager.MessageTypeToBeSent);
			Declaration.DeclarationNumber = "12345678";
			AssertEquals(MessageManager.MessageType.ReplaceHeaderAndLines, manager.MessageTypeToBeSent);
		}

		public void TestMessageTypeToBeSentFormalEntryToRHL()
		{
			MessageManager manager = GetNewManager(MessageManager.OperationType.SubmitMessage);
			Declaration.DeclarationNumber = "12345678";
			Declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			Declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.FormalDeclarationRequired;
			AssertEquals(MessageManager.MessageType.ReplaceHeaderAndLines, manager.MessageTypeToBeSent);
		}

		public override void TestExecuteReplaceRejectedEntry()
		{
			Declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.ManifestRejected;
			Declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;
			Declaration.JE_ECI_LastResponseStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;
			Declaration.DeclarationNumber = "12345678";
			MessageManager manager = GetNewManager(MessageManager.OperationType.SubmitMessage);
			Declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			Assert(manager.Execute());
			AssertEquals(manager.LastHumanReadableStatus, "Replacement Entry Message " + ECIWriteOff.MessageManager.MessageReportingImmediateSend);
			AssertEquals(LowValueManifestStatusList.Codes.SentToCustoms, Declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals(LowValueConsignmentStatusList.Codes.SentToCustoms, Declaration.JE_EntryStatus);
			AssertEquals(LowValueConsignmentStatusList.Codes.ConsignmentInError, Declaration.JE_ECI_LastResponseStatus);
			AssertEquals("Declaration.IsInDatabase", true, Declaration.IsInDatabase);

			var message = Declaration.CusEntryHeader.Messages[0];
			AssertEquals(MsgTransportList.Codes.TSW, message.MsgTransMode);
		}

		public void TestMessageTypeCanBeDeterminedWhenStatusIsBlank()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_DeclarationReference = "S02866347";
			declaration.JE_MasterBill = "08619635755";
			declaration.JE_HouseBill = "BNAKL2866347";
			declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "S02866347";
			outgoingMessage.EM_LinkedObject = declaration.CusEntryHeader;

			var tswACKResponse = Factory.New<TSWMessage>();
			tswACKResponse.EM_MessageType = "RES";
			tswACKResponse.EM_MessageText = S02866347ACKResponse;
			tswACKResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(tswACKResponse);
			AssertEquals("EM_MessageSubType - response message comes from TSW", TSWMessage.ResponseMessage.MessageSubTypes.Acknowledgement, tswACKResponse.EM_MessageSubType);
			AssertEquals("ACK", declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("5118958", declaration.CusEntryHeader.EntryNumber);
			AssertEquals("No customs status reported", "STC", declaration.JE_EntryStatus);

			declaration.CusEntryHeader.CH_EntryStatus = "";
			declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.NoStatusReported;
			AssertEquals("Pre-condition to set up client issue scenario (entry number returned but no status...)", LowValueConsignmentStatusList.Descriptions.NoStatusReported, declaration.JE_EntryStatusDescription);

			declaration.CusEntryHeader.Messages.RemoveAndDeleteAll();
			AssertEquals("Clear out any existing messages", 0, declaration.CusEntryHeader.Messages.Count);

			var manager = new MessageManager(declaration, MessageManager.OperationType.SubmitMessage);
			AssertEquals("Manager.Execute()", true, manager.Execute());
			var messageGenerated = declaration.CusEntryHeader.Messages[0];
			AssertNotNull(messageGenerated);
			AssertEquals("Sending message type should be able to be determined", MessageSubTypeList.Codes.Replacement, messageGenerated.EM_MessageSubType);
		}

		public void TestSendTSWCREJob()
		{
			var declaration = Declaration;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.WriteOff;
			declaration.JE_HouseBill = "CRE034029";
			var importer = Factory.LoadTop1<OrgHeader>(new ZQuery());
			declaration.JE_OH_Importer = importer.PK;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			var manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			AssertEquals("manager.Execute()", true, manager.Execute());
			var entryHeader = EntryHeader;
			AssertEquals("entryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.SentToCustoms, entryHeader.CH_EntryStatus);
			AssertEquals("entryHeader.Messages.Count", 1, entryHeader.Messages.Count);
		}

		#region  S02866347

		public const string S02866347ACKResponse =
@"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
	<Recipient>
	  <ID>00326958C</ID>
	  <RoleCode>TB</RoleCode>
	</Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
	<IssueDateTime formatCode=""204"">20180514083432</IssueDateTime>
	<FunctionalReferenceID>274206</FunctionalReferenceID>
	<FunctionCode>12</FunctionCode>
	<OverallDeclaration>
	  <Declaration>
		<ID>5118958</ID>
		<FunctionalReferenceID>S02866347</FunctionalReferenceID>
		<VersionID>1</VersionID>
		<Submitter>
		  <ID>00326958C</ID>
		</Submitter>
		<ResponsibleGovernmentAgency>
		  <ID>TSW</ID>
		</ResponsibleGovernmentAgency>
	  </Declaration>
	</OverallDeclaration>
	<Status>
	  <EffectiveDateTime formatCode=""204"">20180514083432</EffectiveDateTime>
	  <NameCode>ACK</NameCode>
	  <Pointer>
		<DocumentSectionCode>07B</DocumentSectionCode>
	  </Pointer>
	  <Pointer>
		<DocumentSectionCode>42A</DocumentSectionCode>
	  </Pointer>
	  <Pointer>
		<SequenceNumeric>1</SequenceNumeric>
		<DocumentSectionCode>08B</DocumentSectionCode>
		<TagID>G007</TagID>
	  </Pointer>
	</Status>
  </Response>
</DocumentMetadata>";

		#endregion

		#region ExpectedAlphabeticallyOrderedMessageErrors
		protected override string ExpectedAlphabeticallyOrderedMessageErrors
		{
			get
			{
				return
@"Message Error - JE_ECI_InvoiceAmount: Please enter the Commercial Invoice Amount.
Message Error - JE_ExportDate: You have not entered a Date of Export.
Message Error - JE_GoodsDescription: Please enter a goods description.
Message Error - JE_HouseBill: You have not entered a House Bill.
Message Error - JE_OH_Importer: Please enter a valid importer
Message Error - JE_OH_ShippingLine: Please enter a shipping line.
Message Error - JE_OH_Supplier: Please enter a valid supplier.
Message Error - JE_RL_NKFinalDestination: Please enter a valid destination port code.
Message Error - JE_RL_NKOrigin: Please enter a valid origin port code.
Message Error - JE_RL_NKPortOfArrival: Please enter a valid discharge port code.
Message Error - JE_RL_NKPortOfLoading: Please enter a valid loading port code.
Message Error - JE_TotalNoOfPacks: Please enter a number of outer packs.
Message Error - JE_TotalWeight: You have not entered a Weight.
Message Error - JE_TransportMode: Please enter a transport mode.
Message Error - JE_TransportMode: You have not entered a Mode of Transportation.";
			}
		}

		protected override string ExpectedAlphabeticallyOrderedMessageErrorsForTSW
		{
			get
			{
				return
@"Message Error - JE_ECI_InvoiceAmount: Please enter the Commercial Invoice Amount.
Message Error - JE_ExportDate: You have not entered a Date of Export.
Message Error - JE_GoodsDescription: Please enter a goods description.
Message Error - JE_OH_Importer: Please enter a valid importer
Message Error - JE_OH_ShippingLine: Please enter a shipping line.
Message Error - JE_OH_Supplier: Please enter a valid supplier.
Message Error - JE_RL_NKFinalDestination: Please enter a valid destination port code.
Message Error - JE_RL_NKOrigin: Please enter a valid origin port code.
Message Error - JE_RL_NKPortOfArrival: Please enter a valid discharge port code.
Message Error - JE_RL_NKPortOfLoading: Please enter a valid loading port code.
Message Error - JE_TotalNoOfPacks: Please enter a number of outer packs.
Message Error - JE_TotalWeight: You have not entered a Weight.
Message Error - JE_TransportMode: Please enter a transport mode.
Message Error - JE_TransportMode: You have not entered a Mode of Transportation.";
			}
		}
		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewManager(MessageBuilders.MessageManager.OperationType.SubmitMessage);
		}

		protected override MessageBuilders.MessageManager GetNewMessageManager(MessageBuilders.MessageManager.OperationType operationType)
		{
			return new MessageManager(Declaration, operationType);
		}

		MessageManager GetNewManager(MessageBuilders.MessageManager.OperationType operationType)
		{
			return (MessageManager)GetNewMessageManager(operationType);
		}

		protected new JobDeclaration Declaration
		{
			get { return base.Declaration; }
		}

		CusEntryHeader EntryHeader
		{
			get { return (CusEntryHeader)Declaration.CusEntryHeader; }
		}

		protected override JobDeclaration GetNewJobDeclaration()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			return declaration;
		}

		protected override void SetDeclarationToSentToCustoms()
		{
			Declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			Declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;
		}

		protected override void SetDeclarationToClearanceOK()
		{
			Declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			Declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
		}

		protected override void SetDeclarationToRejected()
		{
			Declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;
			Declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.ManifestRejected;
		}

		protected override void SetDeclarationToInspectionsAuditRequirements()
		{
			Declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentHeld;
			Declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.InspectionsAuditRequirements;
		}

		protected override void SetDeclarationToEntryInError()
		{
			Declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;
			Declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.ManifestInError;
		}
		#endregion

		protected override MessageBuilders.MessageManager.MessageType MessageTypeForCompleteReplacement
		{
			get { return MessageManager.MessageType.ReplaceHeaderAndLines; }
		}
	}

	public class NonInheritedMessageManagerTest : TestCaseWithFactory
	{
		public void TestJobNumberGetsFilledInOnMessageForNewDeclaration()
		{
			TestHelper.SetupMessagingEnvironment();
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			TestECIWriteOffCreator decCreator = new TestECIWriteOffCreator(declaration);
			decCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();
			MessageManager manager = new MessageManager(declaration, MessageManager.OperationType.SubmitMessage);
			AssertEquals("Manager.Execute()", true, manager.Execute());
			NZCMessage message = declaration.CusEntryHeader.Messages[0];
			AssertEquals("B00001000", declaration.JE_DeclarationReference);
			Assert("Checking Message Text Contains Job Number", message.EM_MessageText.Contains("<FunctionalReferenceID>B00001000</FunctionalReferenceID>"));
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			NZCustomsDataRegistry.Instance.ExportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.ImportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
		}
		#endregion
	}
}
