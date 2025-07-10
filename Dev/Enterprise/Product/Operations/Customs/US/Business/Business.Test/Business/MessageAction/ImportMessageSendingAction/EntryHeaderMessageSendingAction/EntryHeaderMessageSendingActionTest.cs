using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(EntryHeaderMessageSendingAction))]
	sealed class EntryHeaderMessageSendingActionTest : XmlSerializableNonPersistentBusinessObjectTest<EntryHeaderMessageSendingAction>
	{
		public override void TestSchemaPropertiesHaveFields()
		{
			Assert(true);
		}

		public void TestSaveMessageActionAsBusinessObjectXmlSerializer()
		{
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableAII = true;
			AssertNotNull(ENSEntry);
			Declaration.US_IsInvoiceByRequest = false;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			var collection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original);
			var action = new EntryHeaderMessageSendingAction(ENSEntry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			action.US_SE_ContactName = "KNZ";
			var testSerializeString = action.Serialize().Replace("\r\n", "").Replace(" ", "");
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var reLoadENSEntry = newFactory.Load<CusEntryHeader>(ENSEntry.PK);
			var action2 = new EntryHeaderMessageSendingAction(reLoadENSEntry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			action2.Deserialize(testSerializeString);
			Assert(action.US_SE_ContactName == "KNZ");
		}

		public void TestSetDefaults()
		{
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableAII = true;
			AssertNotNull(ENSEntry);
			Declaration.US_IsInvoiceByRequest = false;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			ImportMessageSendingActionCollection collection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original);
			EntryHeaderMessageSendingAction action = new EntryHeaderMessageSendingAction(ENSEntry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			AssertEquals("IsEntrySummary", true, action.IsEntrySummary);
			ENSEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			action = new EntryHeaderMessageSendingAction(ENSEntry, ImportMessageStatusList.MessageType.CargoRelease, collection);
			AssertEquals("IsEntrySummary", false, action.IsEntrySummary);
			ENSEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			Declaration.US_IsInvoiceByRequest = true;
			action = new EntryHeaderMessageSendingAction(ENSEntry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			AssertEquals("IsEntrySummary", true, action.IsEntrySummary);
			Declaration.US_CertifyCargoRelease = true;
			collection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original);
			action = new EntryHeaderMessageSendingAction(ENSEntry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			AssertEquals("CertifyCargoRelease", true, action.US_CertifyCargoRelease);
			Declaration.US_CertifyCargoRelease = false;
			action = new EntryHeaderMessageSendingAction(ENSEntry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			AssertEquals("CertifyCargoRelease", false, action.US_CertifyCargoRelease);
			Declaration.US_CertifyCargoRelease = true;
			ENSEntry.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.Certified;
			action = new EntryHeaderMessageSendingAction(ENSEntry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			AssertEquals("CertifyCargoRelease", false, action.US_CertifyCargoRelease);
			ENSEntry.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.CertificationSentAckPending;
			action = new EntryHeaderMessageSendingAction(ENSEntry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			AssertEquals("CertifyCargoRelease", false, action.US_CertifyCargoRelease);
			ENSEntry.US_CRLCertStatus = "";
			action = new EntryHeaderMessageSendingAction(ENSEntry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			AssertEquals("CertifyCargoRelease", true, action.US_CertifyCargoRelease);
			var disposition = Declaration.DispositionCodes.AddNew(); // Disposition codes are added by "RR" messages which indicate certification
			action = new EntryHeaderMessageSendingAction(ENSEntry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			AssertEquals("CertifyCargoRelease", false, action.US_CertifyCargoRelease);
			action = new EntryHeaderMessageSendingAction(CRLEntry, ImportMessageStatusList.MessageType.CargoRelease, collection);
			AssertEquals("CertifyCargoRelease", false, action.US_CertifyCargoRelease);
			disposition.Delete();
			action = new EntryHeaderMessageSendingAction(ENSEntry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			AssertEquals("CertifyCargoRelease", true, action.US_CertifyCargoRelease);
			action = new EntryHeaderMessageSendingAction(CRLEntry, ImportMessageStatusList.MessageType.CargoRelease, collection);
			AssertEquals("CertifyCargoRelease", true, action.US_CertifyCargoRelease);
			Declaration.US_CertifyCargoRelease = false;
			action = new EntryHeaderMessageSendingAction(CRLEntry, ImportMessageStatusList.MessageType.CargoRelease, collection);
			AssertEquals("Should always be true if not certified CertifyCargoRelease", true, action.US_CertifyCargoRelease);
		}

		public void TestIncludePGABlocksForACECargoRelease()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = false;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Description = "testing";
			var vehicleLine = invoiceLine.VehicleLines.AddNew();
			vehicleLine.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			AssertEquals(1, actions.Count);
			var action = (EntryHeaderMessageSendingAction)actions[0];
			Assert(ACEEntrySummaryMessageSendingOption.New(action).US_CertifyCargoRelease);
			action.US_SendMessage = true;
			actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.SimplifiedEntry.Messages[0];
			var oiRecord = message.MessageBlock.MessageBlocks.FirstOrDefault(x => x is MessageBuildingBlocks.Common.AENSOI);
			AssertNotNull("For ACE Cargo release, certify for cargo release should be true and PGA blocks should be included", oiRecord);
		}

		public void TestMessageContents()
		{
			var collection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original);
			var action = new EntryHeaderMessageSendingAction(ENSEntry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			action.US_SendMessage = true;
			action.US_CertifyCargoRelease = false;
			AssertMultilineASCIIEquals("Entry Summary without certification", @"------------------APLB------------------
 Block Number (2-3)             :1
 Application Identifier (11-12) :EI
 User Data (60-80)              :<<MSGNO PLACEHOLDER>>

-----------------ENS10------------------
 Update Action Code (3-3) :A
 Bond Type (49-49)        :0
 Entry Number (62-70)     :<E#PLCH>

-----------------ENS20------------------

-----------------ENS30------------------
 Summary Certification Code (37-37) :0

-----------------ENS90------------------
 Deferred Tax Indicator (25-25) :0

------------------APLY------------------
 Application Identifier (11-12)                            :EI
 Number Of Transaction Detail Records In The Block (13-17) :4", action.US_MessageContents);
			action.US_CertifyCargoRelease = true;
			AssertMultilineASCIIEquals("Entry Summary with certification", @"------------------APLB------------------
 Block Number (2-3)             :1
 Application Identifier (11-12) :EI
 User Data (60-80)              :<<MSGNO PLACEHOLDER>>

-----------------ENS10------------------
 Update Action Code (3-3) :A
 Bond Type (49-49)        :0
 Entry Number (62-70)     :<E#PLCH>

-----------------ENS20------------------

-----------------ENS30------------------
 Summary Certification Code (37-37) :0
 Release Certification Code (38-38) :1

-----------------ENS90------------------
 Deferred Tax Indicator (25-25) :0

------------------APLY------------------
 Application Identifier (11-12)                            :EI
 Number Of Transaction Detail Records In The Block (13-17) :4", action.US_MessageContents);
			action = new EntryHeaderMessageSendingAction(CRLEntry, ImportMessageStatusList.MessageType.CargoRelease, collection);
			action.US_SendMessage = true;
			action.US_CertifyCargoRelease = false;
			AssertMultilineASCIIEquals("Cargo Release without certification", @"------------------APLB------------------
 Block Number (2-3)             :1
 Application Identifier (11-12) :HI
 User Data (60-80)              :<<MSGNO PLACEHOLDER>>

-----------------CRLH1------------------
 Update Action Code (3-3) :A
 Entry Number (11-19)     :<E#PLCH>
 Bond Type Code (40-40)   :0

-----------------CRLH2------------------
 Total Entry Value (29-38) :0

------------------APLY------------------
 Application Identifier (11-12)                            :HI
 Number Of Transaction Detail Records In The Block (13-17) :2", action.US_MessageContents);
			action.US_CertifyCargoRelease = true;
			AssertMultilineASCIIEquals("Cargo Release with certification", @"------------------APLB------------------
 Block Number (2-3)             :1
 Application Identifier (11-12) :HI
 User Data (60-80)              :<<MSGNO PLACEHOLDER>>

-----------------CRLH1------------------
 Update Action Code (3-3)           :A
 Entry Number (11-19)               :<E#PLCH>
 Bond Type Code (40-40)             :0
 Release Certification Code (41-41) :1

-----------------CRLH2------------------
 Total Entry Value (29-38) :0

------------------APLY------------------
 Application Identifier (11-12)                            :HI
 Number Of Transaction Detail Records In The Block (13-17) :2", action.US_MessageContents);
			action = new EntryHeaderMessageSendingAction(INBEntry, ImportMessageStatusList.MessageType.InBondDeparture, collection);
			action.US_SendMessage = true;
			AssertMultilineASCIIEquals("InBond Departure", ImportMessageSendingAction.MessageContentPreviewNotSupportedYet, action.US_MessageContents);
			var bCREntry = Declaration.CustomsEntryHeaders.AddNew();
			bCREntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease;
			bCREntry.CH_CH_PrimeEntry = ENSEntry.PK;
			action = new EntryHeaderMessageSendingAction(bCREntry, ImportMessageStatusList.MessageType.BorderCargoRelease, collection);
			action.US_SendMessage = true;
			action.US_CertifyCargoRelease = false;
			AssertMultilineASCIIEquals("Border Cargo Release", @"------------------APLB------------------
 Block Number (2-3)             :1
 Application Identifier (11-12) :HN
 User Data (60-80)              :<<MSGNO PLACEHOLDER>>

-----------------BCR01------------------
 Update Action Code (3-3) :A
 Entry Number (11-18)     :<E#PLCH>
 Bond Type (33-33)        :0

------------------APLY------------------
 Application Identifier (11-12)                            :HN
 Number Of Transaction Detail Records In The Block (13-17) :1", action.US_MessageContents);
			action.US_CertifyCargoRelease = true;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_EnableCRL = true;
			var bill = Declaration.Bills.AddNew();
			bill.US_SESplitShip = true;
			Declaration.US_US_NKLocationOfGoods = "S002";
			Declaration.US_US_NKCentralizedExamSite = "A001";
			Declaration.JE_VoyageFlightNo = "001TR";
			var simplifiedEntry = Declaration.CustomsEntryHeaders.AddNew();
			simplifiedEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			simplifiedEntry.CH_CH_PrimeEntry = ENSEntry.PK;
			collection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Replacement);
			action = new EntryHeaderMessageSendingAction(simplifiedEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			action.US_SendMessage = true;
			action.US_SE_ContactName = "Johnny B. Broker";
			action.US_SE_ContactPhone = "555-0100";
			action.US_SE_ReasonCode = "01";
			action.US_SE_MultipleDispositionsIndic = true;
			action.US_SE_ActionType = ACECargoReleaseActionType.Codes.Replace;
			AssertMultilineASCIIEquals("Simplified Entry", @"---------------AABIInputB---------------
 Application Identifier Code (11-12)    :SE
 Filer Preparers User Data Text (60-80) :<<MSGNO PLACEHOLDER>>

----------------ASESE10-----------------
 Update Action Code (5-5)      :R
 Entry Number (11-18)          :11111
 Bond Type Code (39-39)        :0
 Estimated Entry Value (40-49) :0

----------------ASESE11-----------------
 Location Of Goods F I R M S (12-15)        :S002
 Elected Exam Site F I R M S (16-19)        :A001
 Voyage Flight Trip Manifest Number (40-44) :001TR

----------------ASESE13-----------------
 Contact Name (5-44)                           :JOHNNY B. BROKER
 Contact Phone (45-59)                         :555-0100
 Reason Code (60-61)                           :01
 Multiple Cargo Dispositions Indicator (62-62) :1
 Split Shipment Indicator (64-64)              :1

----------------ASESE20-----------------
 Reference Identifier Qualifier (5-7) :CR

---------------AABIInputY---------------
 Application Identifier Code (11-12) :SE", action.US_MessageContents);
			AssertEquals("Should not be saved to declaration on this stage", false, Declaration.US_SEMultiCargoDispInd);
			action.US_SE_ActionType = ACECargoReleaseActionType.Codes.Update;
			AssertMultilineASCIIEquals("Simplified Entry", @"---------------AABIInputB---------------
 Application Identifier Code (11-12)    :SE
 Filer Preparers User Data Text (60-80) :<<MSGNO PLACEHOLDER>>

----------------ASESE10-----------------
 Update Action Code (5-5)      :U
 Entry Number (11-18)          :11111
 Bond Type Code (39-39)        :0
 Estimated Entry Value (40-49) :0

----------------ASESE11-----------------
 Location Of Goods F I R M S (12-15)        :S002
 Elected Exam Site F I R M S (16-19)        :A001
 Voyage Flight Trip Manifest Number (40-44) :001TR

----------------ASESE13-----------------
 Contact Name (5-44)                           :JOHNNY B. BROKER
 Contact Phone (45-59)                         :555-0100
 Reason Code (60-61)                           :01
 Multiple Cargo Dispositions Indicator (62-62) :1
 Split Shipment Indicator (64-64)              :1

----------------ASESE20-----------------
 Reference Identifier Qualifier (5-7) :CR

---------------AABIInputY---------------
 Application Identifier Code (11-12) :SE", action.US_MessageContents);
			var seEntry = Declaration.ActiveEntryHeaders.SimplifiedEntry;
			collection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original);
			action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			var message = builder.PopulateMessage();
			message.EM_MessageNum = "HYEDUSCMT_196571";
			var rcvMessage = Factory.New<MQEDIMessage>();
			rcvMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			rcvMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			rcvMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			rcvMessage.EM_MessageNum = "HYEDUSCMT_196571";
			rcvMessage.EM_MessageText = @"B001101SV9SX                                               HYEDUSCMT_196571     " +
				"SE10ASV9  71032807 01EI 58-12345678911800000100001101  1101                     " +
				"SE15RAPLUMST0802186                                        00000010     N       " +
				"SE20CR B00173079                                                                " +
				"SE9002   SE DATA ACCEPTED                                                       " +
				"Y  1101SV9SX00004";
			rcvMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-1);
			Declaration.ActiveEntryHeaders.SimplifiedEntry.Messages.Add(rcvMessage);
			collection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Deletion);
			action = new EntryHeaderMessageSendingAction(simplifiedEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			action.US_SendMessage = true;
			action.US_SE_ContactName = "Johnny B. Broker";
			action.US_SE_ContactPhone = "555-0100";
			action.US_SE_MultipleDispositionsIndic = true;
			AssertMultilineASCIIEquals("Simplified Entry", @"---------------AABIInputB---------------
 Application Identifier Code (11-12)    :SE
 Filer Preparers User Data Text (60-80) :<<MSGNO PLACEHOLDER>>

----------------ASESE10-----------------
 Update Action Code (5-5)      :D
 Entry Number (11-18)          :11111
 Bond Type Code (39-39)        :0
 Estimated Entry Value (40-49) :0

----------------ASESE13-----------------
 Contact Name (5-44)                           :JOHNNY B. BROKER
 Contact Phone (45-59)                         :555-0100
 Multiple Cargo Dispositions Indicator (62-62) :1
 Split Shipment Indicator (64-64)              :1

----------------ASESE20-----------------
 Reference Identifier Qualifier (5-7) :CR

---------------AABIInputY---------------
 Application Identifier Code (11-12) :SE", action.US_MessageContents);
		}

		public void TestDefaultUS_SE_MultipleDispositionsIndic()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.ImportEntryNumber = "00000016";
			Factory.Save();
			declaration.US_SEMultiCargoDispInd = true;
			var coll = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			AssertEquals("US_SE_MultipleDispositionsIndic should not been defaulted", false, coll[0].US_SE_MultipleDispositionsIndic);
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			coll = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			AssertEquals("US_SE_MultipleDispositionsIndic should not been defaulted", false, coll[0].US_SE_MultipleDispositionsIndic);
			coll = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			AssertEquals("US_SE_MultipleDispositionsIndic should not been defaulted", false, coll[0].US_SE_MultipleDispositionsIndic);
			declaration.US_EnableCRL = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			coll = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var clrAction = coll.Cast<EntryHeaderMessageSendingAction>().FirstOrDefault(x => x.IsACECargoRelease);
			AssertEquals("US_SE_MultipleDispositionsIndic should not been defaulted", false, clrAction.US_SE_MultipleDispositionsIndic);
			declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			coll = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			clrAction = coll.Cast<EntryHeaderMessageSendingAction>().FirstOrDefault(x => x.IsACECargoRelease);
			AssertEquals("US_SE_MultipleDispositionsIndic should not been defaulted", false, clrAction.US_SE_MultipleDispositionsIndic);
			coll = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			clrAction = coll.Cast<EntryHeaderMessageSendingAction>().FirstOrDefault(x => x.IsACECargoRelease);
			AssertEquals("US_SE_MultipleDispositionsIndic should be defaulted", true, clrAction.US_SE_MultipleDispositionsIndic);
		}

		public void TestDefaultUS_Paid()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.ImportEntryNumber = "00000016";
			Factory.Save();
			declaration.US_Paid = YesNoDefaultList.Codes.Yes;
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			var coll = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			AssertEquals("Paid should have been defaulted", YesNoDefaultList.Codes.Yes, coll[0].US_Paid);
			declaration.US_Paid = ZString.Empty;
			declaration.US_PSC = true;
			coll = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			AssertEquals("PSC means PaymentType won't be sent", ZString.Empty, coll[0].US_Paid);
			declaration.US_PSC = false;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			coll = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			AssertEquals("No statement record exists yet", YesNoDefaultList.Codes.No, coll[0].US_Paid);
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_EntryFilerCode = "XJ5";
			statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_EntryNum = "00000016";
			AssertNotNull(statementLine.Declaration);
			declaration.US_Paid = ZString.Empty;
			coll = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			AssertEquals("statement is not finalised", YesNoDefaultList.Codes.No, coll[0].US_Paid);
			statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;
			declaration.US_Paid = ZString.Empty;
			coll = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			AssertEquals("statement is not finalised", YesNoDefaultList.Codes.No, coll[0].US_Paid);
			statement.B2_Status = StatementHeaderStatusList.Codes.Final;
			statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;
			declaration.US_Paid = ZString.Empty;
			coll = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			AssertEquals("statement is not finalised", YesNoDefaultList.Codes.Yes, coll[0].US_Paid);
			declaration.US_Paid = ZString.Empty;
			declaration.US_PSC = true;
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ErrorEntrySummaryOriginal;
			coll = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			AssertEquals("PSC means PaymentType won't be sent", ZString.Empty, coll[0].US_Paid);
			declaration.US_Paid = YesNoDefaultList.Codes.Yes;
			declaration.US_PSC = true;
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ErrorEntrySummaryOriginal;
			coll = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			AssertEquals("PSC means PaymentType won't be sent2", ZString.Empty, coll[0].US_Paid);
		}

		public void TestSetDefaultsForJobReadyForPosting()
		{
			Declaration.US_JobReadyForPost = true;
			EntryHeaderMessageSendingAction ensAction = SendingActionCollection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			Assert(ensAction.US_JobReadyForPosting);
			EntryHeaderMessageSendingAction crlAction = SendingActionCollection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.CargoRelease);
			Assert(!crlAction.US_JobReadyForPosting);
		}

		public void TestSendUS_SendMessageSetBackToFalse()
		{
			EntryHeaderMessageSendingAction ensAction = SendingActionCollection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			ensAction.US_SendMessage = true;
			ensAction.US_CertifyCargoRelease = true;
			ensAction.US_SendMessage = false;
			AssertEquals("US_CertifyCargoRelease is set back to false again: US_CertifyCargoRelease is readonly", false, ensAction.US_CertifyCargoRelease);
		}

		public void TestJobReadyForPost()
		{
			EntryHeaderMessageSendingAction ensAction = SendingActionCollection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			ensAction.US_SendMessage = false;
			Assert(ensAction.US_JobReadyForPosting_ReadOnly);
			ensAction.US_SendMessage = true;
			Assert(!ensAction.US_JobReadyForPosting_ReadOnly);
			ensAction.US_JobReadyForPosting = true;
			SendingActionCollection.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Assert(Declaration.US_JobReadyForPost);
		}

		public void TestJobReadyForPostForWithdrawal()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.ActiveEntryHeaders[0].CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			ImportMessageSendingActionCollection coll = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			EntryHeaderMessageSendingAction ensAction = SendingActionCollection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			ensAction.US_SendMessage = true;
			Assert("Job Ready for posting should not be open for edit when trying to delete", !ensAction.US_JobReadyForPosting_ReadOnly);
		}

		public void TestCertifyCargoReleaseDoesNotDefaultDeclarationValueOnceCertified()
		{
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_CertifyCargoRelease = true;
			var disposition = Declaration.DispositionCodes.AddNew(); // Disposition codes are added by "RR" messages which indicate certification
			EntryHeaderMessageSendingAction ensAction = SendingActionCollection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			AssertEquals("US_CertifyCargoRelease should not default from Declaration when cargo release has already been certified", false, ensAction.US_CertifyCargoRelease);
			ensAction.US_SendMessage = false;
			AssertEquals("US_CertifyCargoRelease should be turned off if entry summary send is unchecked", false, ensAction.US_CertifyCargoRelease);
			ensAction.US_SendMessage = true;
			AssertEquals("US_CertifyCargoRelease should remain unchecked as entry already certified", false, ensAction.US_CertifyCargoRelease);
			ensAction.US_CertifyCargoRelease = false;
			Declaration.US_CertifyCargoRelease = false;
			Assert(!Declaration.IsCargoReleaseValidationMode);
			ensAction.US_CertifyCargoRelease = true;
			AssertEquals("Validation mode should be Cargo Release", true, Declaration.IsCargoReleaseValidationMode);
		}

		public void TestUS_MessageDescription()
		{
			AssertEquals("PreCondition:three elements", 3, SendingActionCollection.Count);
			EntryHeaderMessageSendingAction entrySummaryAction = SendingActionCollection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			AssertNotNull(entrySummaryAction);
			ENSEntry.Declaration.US_EntryType = EntryTypeList.Codes.ImmediateExportation;
			AssertEquals("EntrySummary (11111)", entrySummaryAction.US_MessageDescription);
			EntryHeaderMessageSendingAction cargoReleaseAction = SendingActionCollection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.CargoRelease);
			AssertNotNull(cargoReleaseAction);
			AssertEquals("CargoRelease (11111)", cargoReleaseAction.US_MessageDescription);
			EntryHeaderMessageSendingAction inbondAction = SendingActionCollection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.InBondDeparture);
			INBEntry.EntryNumber = "22222";
			//this is so that RandomHeader returns a value
			CusEntryLine entryLine = INBEntry.AllEntryLines.AddNew();
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			Declaration.US_InbondType = "61";
			AssertNotNull(inbondAction);
			AssertEquals("InBondDeparture (22222)", inbondAction.US_MessageDescription);
		}

		public void TestDefaultFromLastFailedTransmission()
		{
			ENSEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Declaration.US_PSC = true;
			ENSEntry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryReplace;
			ENSEntry.CH_Status = ImportMessageStatusList.Codes.ErrorEntrySummaryReplace;
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			message.EM_MessageText = "B  3902SV9AE                                               6009006              10ASV9  10001954 3902B00005133   0110 X           2020711       Y               1191-01319900091-013199000                                  IL                  20AAAA3902013111A001APL EMERALD                                                 21V123W                                                                         2200000001PK                                                                    23MAAAAOBLACE27                                                                 318B 891                                                                        36THIS IS A TEST PSC EXPLANATION TEXT THIS IS A TEST PSC EXPLANATION TEXT THI   36S IS A TEST PSC EXPLANATION TEXT THIS IS A TEST PSC EXPLANATION TEXT THIS I   36S A TEST PSC EXPLANATION TEXT THIS IS A TEST PSC EXPLANATION TEXT THIS IS A   36 TEST PSC EXPLANATION TEXT  THIS IS A TEST PSC EXPLANATION TEXT               40  001 CNTW012411        0000000015602670000009000    N                        44COMMERCIAL DESCRIPTION                                                        47MTHLIATHA191NAK                                                               47C91-013199000                                                                 47S91-013199000                                                                 508516710020 0000185000 0000050000 000005546800NO                               6250100006250                                                                   6249900010500                                                                   63L01L04                                                                        40  002 CNTW012411        0000000017602670000009000    N                        44COMMERCIAL DESCRIPTION                                                        47MTHLIATHA191NAK                                                               47C91-013199000                                                                 47S91-013199000                                                                 508516710020 0000203500 0000055000 000005546800NO                               6250100006875                                                                   6249900011550                                                                   40  003 CNTW012411        0000000018602670000009000    N                        44COMMERCIAL DESCRIPTION                                                        47MTHLIATHA191NAK                                                               47C91-013199000                                                                 47S91-013199000                                                                 508516710020 0000222000 0000060000 000005546800NO                               6250100007500                                                                   6249900012600                                                                   63L05                                                                           895010000002062549900000034650                                                  9000000610500 00000055275 00000000000 00000000000 00000000000                   Y  3902SV9AE";
			ENSEntry.Messages.Add(message);
			var coll = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original);
			var entrySummaryAction = coll.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			Assert("This should have been defaulted", !entrySummaryAction.US_PSCExplanation.IsEmpty);
		}

		public void TestCopyPSCReasonsAndExplanation()
		{
			var query = new ZQuery();
			query.AddToFilter(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.PSCReasonCodes);
			int existingCount = Factory.GetDatabaseCount(typeof(PSCReasonCusCodeData), query);
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_PSC = true;
			var action = new EntryHeaderMessageSendingAction(ENSEntry, ImportMessageStatusList.MessageType.EntrySummary, SendingActionCollection);
			action.US_SendMessage = true;
			var pscCode1 = action.PSCReasonCodes.AddNew();
			pscCode1.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.Entry;
			pscCode1.Reason1 = PSCHeaderReasonList.Codes.H01;
			action.US_PSCExplanation = "Some Explanations";
			action.CopyPSCReasonsAndExplanation();
			Factory.Save();
			var query1 = new ZQuery();
			query1.AddToFilter(CusAddInfoSchema.B7_ParentTableCode, CusEntryHeaderSchema.Constants.Prefix);
			query1.AddToFilter(CusAddInfoSchema.B7_Type, CusCodeDataTypeList.Codes.PSCReasonCodes);
			query1.AddToFilter(CusAddInfoSchema.B7_ParentID, ENSEntry.PK);
			var explanations = Factory.Load<PSCExplanationCusAddInfo>(query1);
			AssertNotNull("Explanation should be saved", explanations);
			AssertEquals("There should be 1 explanation", 1, explanations.Length);
			var explanation = explanations[0];
			AssertEquals("PSC Explanation", "Some Explanations", explanation.B7_AddInfoData);
			int count = Factory.GetDatabaseCount(typeof(PSCReasonCusCodeData), query);
			AssertEquals("There should be 1 more PSC Reason", existingCount + 1, count);
			action.US_PSCExplanation = "Another Explanation";
			action.CopyPSCReasonsAndExplanation();
			Factory.Save();
			explanations = Factory.Load<PSCExplanationCusAddInfo>(query1);
			AssertNotNull("Explanation should be saved", explanations);
			AssertEquals("There should be 1 explanation", 1, explanations.Length);
			explanation = explanations[0];
			AssertEquals("PSC Explanation", "Another Explanation", explanation.B7_AddInfoData);
			count = Factory.GetDatabaseCount(typeof(PSCReasonCusCodeData), query);
			AssertEquals("There should be 1 more PSC Reason", existingCount + 1, count);
		}

		public void TestIsPaidRelevant()
		{
			var ensAction = SendingActionCollection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			AssertEquals(false, ensAction.IsPaidRelevant);
			var actions = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Replacement);
			AssertEquals(false, ensAction.IsPaidRelevant);
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			AssertEquals(false, ensAction.IsPaidRelevant);
			ENSEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			AssertEquals(true, ensAction.IsPaidRelevant);
			Declaration.US_PSC = true;
			AssertEquals(false, ensAction.IsPaidRelevant);
			var clrAction = SendingActionCollection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.CargoRelease);
			AssertEquals(false, ensAction.IsPaidRelevant);
		}

		public void TestMessageContentsForTemporaryImportationBond()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_SchDEntry = "8888";
			Declaration.US_EnableENS = true;
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var collection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.ExtendTIB);
			var action = new EntryHeaderMessageSendingAction(ENSEntry, ImportMessageStatusList.MessageType.TemporaryImportationBond, collection);
			action.US_SendMessage = true;
			AssertMultilineASCIIEquals("New Extension TIB message", @"---------------AABIInputB---------------
 Filer Code (8-10)                      :XJ5
 Application Identifier Code (11-12)    :TE
 Filer Preparers User Data Text (60-80) :<<MSGNO PLACEHOLDER>>

-----------------ATIBXA-----------------
 District Port Of Entry Summary (3-6)    :8888
 Broker Number Or Entry Filer Code (7-9) :XJ5
 Entry Number (10-18)                    :11111
 Extension Closure Code (19-19)          :1

---------------AABIInputY---------------
 Filer Code (8-10)                   :XJ5
 Application Identifier Code (11-12) :TE", action.US_MessageContents);
			collection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.ClosureTIB);
			action = new EntryHeaderMessageSendingAction(ENSEntry, ImportMessageStatusList.MessageType.TemporaryImportationBond, collection);
			action.US_SendMessage = true;
			AssertMultilineASCIIEquals("New Closure TIB message", @"---------------AABIInputB---------------
 Filer Code (8-10)                      :XJ5
 Application Identifier Code (11-12)    :TE
 Filer Preparers User Data Text (60-80) :<<MSGNO PLACEHOLDER>>

-----------------ATIBXA-----------------
 District Port Of Entry Summary (3-6)    :8888
 Broker Number Or Entry Filer Code (7-9) :XJ5
 Entry Number (10-18)                    :11111
 Extension Closure Code (19-19)          :2

---------------AABIInputY---------------
 Filer Code (8-10)                   :XJ5
 Application Identifier Code (11-12) :TE", action.US_MessageContents);
		}

		protected override BusinessObject GetNewBusinessObject() => new EntryHeaderMessageSendingAction(ENSEntry, ImportMessageStatusList.MessageType.EntrySummary, SendingActionCollection);

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				}

				return declaration;
			}
		}

		CusEntryHeader ensEntry;
		CusEntryHeader ENSEntry
		{
			get
			{
				if (ensEntry == null)
				{
					ensEntry = Declaration.CustomsEntryHeaders.AddNew();
					ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
					ensEntry.EntryNumber = "11111";
				}

				return ensEntry;
			}
		}

		CusEntryHeader crlEntry;
		CusEntryHeader CRLEntry
		{
			get
			{
				if (crlEntry == null)
				{
					crlEntry = Declaration.CustomsEntryHeaders.AddNew();
					crlEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
					crlEntry.CH_CH_PrimeEntry = ENSEntry.PK;
				}

				return crlEntry;
			}
		}

		CusEntryHeader inbEntry;
		CusEntryHeader INBEntry
		{
			get
			{
				if (inbEntry == null)
				{
					inbEntry = Declaration.CustomsEntryHeaders.AddNew();
					inbEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
				}

				return inbEntry;
			}
		}

		ImportMessageSendingActionCollection sendingActionCollection;
		ImportMessageSendingActionCollection SendingActionCollection
		{
			get
			{
				if (sendingActionCollection == null)
				{
					var ensEntry = ENSEntry; //need to be touched
					var inbEntry = INBEntry;
					var crlEntry = CRLEntry;
					sendingActionCollection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original);
				}

				return sendingActionCollection;
			}
		}
	}
}
