using System;
using System.IO;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	sealed class BIRDDataImporterTest : TestCaseWithFactory
	{
		public const string TestResource = "Enterprise.Customs.US.DataTransfer.Testing.BIRDUpdate.TestFiles.";
		public void TestNotifyUnkownExceptionInDetail()
		{
			var importer = new BIRDDataImporterForTest(Factory);
			var notifications = new NotificationCollection();
			importer.ImportData(new StreamReader(GetType().Assembly.GetManifestResourceStream(TestResource + "BIRDTestText.txt")), "BIRDTestText.txt", notifications, SourceInfo.EmptySourceInfo);
			Assert(notifications.ContainsNotificationContaining("blabla--this is a test--blalba"));
		}

		public void TestUpdate()
		{
			var importer = new BIRDDataImporter(Factory);
			var notifications = new NotificationCollection();
			importer.ImportData(new StreamReader(GetType().Assembly.GetManifestResourceStream(TestResource + "BIRDTestText.txt")), "BIRDTestText.txt", notifications, SourceInfo.EmptySourceInfo);
			var declaration = Factory.LoadTop1<JobDeclaration>(Customs.Business.EntryNumberQueryGenerator.GetEntryNumberQuery(SQLComparisonOperator.Equal, "60019292", Core.Constants.CountryCodes.UnitedStates));
			AssertNotNull(declaration);
		}

		public void TestProcessACEOrgIntoACSDeclaration()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgWrapper = OrgHeaderWrapper.New(org);
			org.OH_Code = "ABCEXPCHI";
			org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EntryFilerCode, "XJ5");
			org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199000");
			org.EDICommunicationsModes.AddNew().EK_Module = EDICommunicationsMode.Modules.US_BIRD;
			Factory.Save();
			var notifications = new NotificationCollection();
			var importer = new BIRDDataImporter(Factory);
			var reader = new StreamReader(GetType().Assembly.GetManifestResourceStream(TestResource + "BIRDOrg.txt"));
			AssertNoExceptionThrown(delegate
			{
				importer.ImportData(reader, "BIRDOrg.txt", notifications, SourceInfo.EmptySourceInfo);
			});
			var declaration = Factory.LoadTop1<JobDeclaration>(Customs.Business.EntryNumberQueryGenerator.GetEntryNumberQuery(SQLComparisonOperator.Equal, "70027533", Core.Constants.CountryCodes.UnitedStates));
			AssertNotNull(declaration);
			var imp = declaration.JE_OH_Importer;
			AssertEquals(org.PK, declaration.JE_OH_Importer);
			AssertEquals(JobApplicationCodeList.Codes.ACS, declaration.JE_ApplicationCode);
			AssertEquals(2, declaration.InvoiceLines.Count);
			var line1 = declaration.InvoiceLines.OfType<JobComInvoiceLine>().First(x => x.JI_LineNo == 1);
			var line2 = declaration.InvoiceLines.OfType<JobComInvoiceLine>().First(x => x.JI_LineNo == 2);
			AssertEquals(SecondarySpecProgIndicatorList.Codes.X, line1.US_SecondarySPI);
			AssertEquals(SecondarySpecProgIndicatorList.Codes.V, line2.US_SecondarySPI);
			AssertEquals(line1.PK, line2.JI_ParentID);
		}

		public void TestProcessDateFile()
		{
			var notifications = new NotificationCollection();
			var importer = new BIRDDataImporter(Factory);
			var reader = new StreamReader(GetType().Assembly.GetManifestResourceStream(TestResource + "BIRDDate.txt"));
			var countOfDecs = Factory.Load<JobDeclaration>(new ZQuery()).Length;
			AssertNoExceptionThrown(delegate
			{
				importer.ImportData(reader, "BIRDDate.txt", notifications, SourceInfo.EmptySourceInfo);
			});
			AssertEquals("No declaration should have been created when this is imported", countOfDecs, Factory.Load<JobDeclaration>(new ZQuery()).Length);
		}

		public void TestIgnoreAndProcessUnknownRecords()
		{
			var notifications = new NotificationCollection();
			var importer = new BIRDDataImporter(Factory);
			var reader = new StreamReader(GetType().Assembly.GetManifestResourceStream(TestResource + "BIRDWithUnknownBlock.txt"));
			AssertNoExceptionThrown(delegate
			{
				importer.ImportData(reader, "BIRDWithUnknownBlock.txt", notifications, SourceInfo.EmptySourceInfo);
			});
		}

		public void TestProcessMalformedFileWithoutZZRecord()
		{
			var notifications = new NotificationCollection();
			var importer = new BIRDDataImporter(Factory);
			var reader = new StreamReader(GetType().Assembly.GetManifestResourceStream(TestResource + "BIRDWithNoZZRecord.txt"));
			AssertNoExceptionThrown(delegate
			{
				importer.ImportData(reader, "BIRDWithNoZZRecord.txt", notifications, SourceInfo.EmptySourceInfo);
			});
			AssertEquals("Error should be logged", ACSBIRDProcessor.NoZZBlock, notifications.GetErrors().ToMessageListString());
		}

		public void TestProcessMalformedFileWithoutAARecord()
		{
			var notifications = new NotificationCollection();
			var importer = new BIRDDataImporter(Factory);
			var reader = new StreamReader(GetType().Assembly.GetManifestResourceStream(TestResource + "BIRDWithNoAARecord.txt"));
			AssertNoExceptionThrown(delegate
			{
				importer.ImportData(reader, "BIRDWithNoAARecord.txt", notifications, SourceInfo.EmptySourceInfo);
			});
			AssertEquals("Error should be logged", ACSBIRDProcessor.NoAABlock, notifications.GetErrors().ToMessageListString());
		}

		public void TestProcessENFile()
		{
			var notifications = new NotificationCollection();
			var importer = new BIRDDataImporter(Factory);
			var reader = new StreamReader(GetType().Assembly.GetManifestResourceStream(TestResource + "BIRDEN.txt"));
			var countOfDecs = Factory.Load<JobDeclaration>(new ZQuery()).Length;
			AssertNoExceptionThrown(delegate
			{
				importer.ImportData(reader, "BIRDEN.txt", notifications, SourceInfo.EmptySourceInfo);
			});
			AssertEquals("No declaration should have been created when this is imported", 0, Factory.Load<JobDeclaration>(new ZQuery()).Length);
		}

		public void TestUpdateExistingDeclarationForEntrySummary()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.US.Business.JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "60019292";
			AssertEquals("PreCondition", ZString.Empty, declaration.US_EntryType);
			AssertEquals("PreCondition", 0, declaration.Invoices.Count);
			AssertEquals("PreCondition", 0, declaration.InvoiceLines.Count);
			Factory.Save();
			var notifications = new NotificationCollection();
			var importer = new BIRDDataImporter(Factory);
			importer.ImportData(new StreamReader(GetType().Assembly.GetManifestResourceStream(TestResource + "BIRDTestText.txt")), "BIRDTestText.txt", notifications, SourceInfo.EmptySourceInfo);
			AssertEquals("Declaration should have been updated", EntryTypeList.Codes.ConsumptionFreeDutiable, declaration.US_EntryType);
			AssertEquals("Declaration should have been updated", 1, declaration.Invoices.Count);
			AssertEquals("Declaration should have been updated - one Invoice Line with Sup Tariff should be created", 1, declaration.InvoiceLines.Count);
			AssertEquals("Sup Tariff for Invoice Line should be created", "9801007000", declaration.InvoiceLines[0].US_SupTariff);
			AssertEquals("JI_Tariff for Invoice Line should be created", "8802200015", declaration.InvoiceLines[0].JI_Tariff);
			var factory2 = new BusinessObjectFactory();
			var decLoaded = factory2.Load<JobDeclaration>(declaration.PK);
			AssertEquals("Saved to eDocs", "BIRDTestText.txt", decLoaded.DocManagerInfo.AllEDocs[0].FileName);
		}

		public void TestNoDeserialiseExceptionThrown()
		{
			var notifications = new NotificationCollection();
			var importer = new BIRDDataImporter(Factory);
			AssertNoExceptionThrown(delegate
			{
				importer.ImportData(new StreamReader(GetType().Assembly.GetManifestResourceStream(TestResource + "BIRDCannotBeSerialised.txt")), "BIRDCannotBeSerialised.txt", notifications, SourceInfo.EmptySourceInfo);
			});
#if NET
			AssertEquals("PrivilegedStatusFilingDate is wrong, import cannot be finished", @"The BIRD message has invalid data and system could not proceed. Please correct the invalid data and try again.
Data Mismatch in Message.

Error reading PrivilegedStatusFilingDate from block ENS40.
'006036' is not a valid MMddyy format (Parameter 'value')", notifications.GetErrors().ToMessageListString());
			notifications = new NotificationCollection();
			importer = new BIRDDataImporter(Factory);
#else
			AssertEquals("PrivilegedStatusFilingDate is wrong, import cannot be finished", @"The BIRD message has invalid data and system could not proceed. Please correct the invalid data and try again.
Data Mismatch in Message.

Error reading PrivilegedStatusFilingDate from block ENS40.
'006036' is not a valid MMddyy format
Parameter name: value", notifications.GetErrors().ToMessageListString());
			notifications = new NotificationCollection();
			importer = new BIRDDataImporter(Factory);
#endif

			AssertNoExceptionThrown(delegate
			{
				importer.ImportData(new StreamReader(GetType().Assembly.GetManifestResourceStream(TestResource + "BIRDCannotBeSerialised_2.txt")), "BIRDCannotBeSerialised_2.txt", notifications, SourceInfo.EmptySourceInfo);
			});

#if NET
			AssertEquals("Block 50 is wrong, import cannot be finished", @"The BIRD message has invalid data and system could not proceed. Please correct the invalid data and try again.
Data Mismatch in Message.

Error reading DateOfExportation from block ENS50.
'11111' is not a valid MMddyy format (Parameter 'value')", notifications.GetErrors().ToMessageListString());
#else
			AssertEquals("Block 50 is wrong, import cannot be finished", @"The BIRD message has invalid data and system could not proceed. Please correct the invalid data and try again.
Data Mismatch in Message.

Error reading DateOfExportation from block ENS50.
'11111' is not a valid MMddyy format
Parameter name: value", notifications.GetErrors().ToMessageListString());
#endif
		}

		public void TestSendAcknowledgementOf7501Or3461Receipt()
		{
			var broker = Factory.NewWithValidTestData<OrgHeader>();
			broker.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EntryFilerCode, "113");
			broker.EDICommunicationsModes.AddNew().EK_Module = EDICommunicationsMode.Modules.US_BIRD;
			Factory.Save();
			var notifications = new NotificationCollection();
			var importer = new BIRDDataImporter(Factory);
			importer.ImportData(new StreamReader(GetType().Assembly.GetManifestResourceStream(TestResource + "BIRDWithUnknownBlock.txt")), "BIRDWithUnknownBlock.txt", notifications, SourceInfo.EmptySourceInfo);
			ZQuery query = new GenAddOnColumnQueryHelper(typeof(JobDeclaration)).GetQueryOnGenAddOnColumn(JobDeclaration.Schema.US_BRDRefNo, "3303279");
			var declaration = Factory.LoadTop1<JobDeclaration>(query);
			AssertNotNull(declaration);
			query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImportCode);
			AssertEquals(1, declaration.Logs.Find(query).Length);
		}

		public void TestUpdateExistingDeclarationByOriginatingBrokerRef()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var notifications = new NotificationCollection();
			var importer = new BIRDDataImporter(Factory);
			importer.ImportData(new StreamReader(GetType().Assembly.GetManifestResourceStream(TestResource + "BIRDNoEntryNoText.txt")), "BIRDNoEntryNoText.txt", notifications, SourceInfo.EmptySourceInfo);
			Factory.Save();
			Assert("imported successfully without entry number", !notifications.HasErrors());
			ZQuery query = new GenAddOnColumnQueryHelper(typeof(JobDeclaration)).GetQueryOnGenAddOnColumn(JobDeclaration.Schema.US_BRDRefNo, "B00001001");
			var declaration = Factory.LoadTop1<JobDeclaration>(query);
			AssertNotNull(declaration);
			declaration.Invoices.DeleteAll();
			var entryHeader = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entryHeader?.Messages.RemoveAndDeleteAllFromTest();
			Factory.Save();
			importer.ImportData(new StreamReader(GetType().Assembly.GetManifestResourceStream(TestResource + "BIRDNoEntryNoText.txt")), "BIRDNoEntryNoText.txt", notifications, SourceInfo.EmptySourceInfo);
			declaration.Reload();
			AssertEquals(JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals("Should have updated the existing declaration", 1, declaration.Invoices.Count);
			AssertEquals("Should have updated the existing declaration", 1, declaration.InvoiceLines.Count);
			AssertEquals("Sup Tariff", "9801007000", declaration.InvoiceLines[0].US_SupTariff);
			AssertEquals("Classification Tariff", "8802200015", declaration.InvoiceLines[0].JI_Tariff);
		}

		public void TestCannotUpdateIfThereAreMoreThanOneDecFound()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.US.Business.JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "60019292";
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.US.Business.JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "60019292";
			Factory.Save();
			var notifications = new NotificationCollection();
			var importer = new BIRDDataImporter(Factory);
			importer.ImportData(new StreamReader(GetType().Assembly.GetManifestResourceStream(TestResource + "BIRDTestText.txt")), "BIRDTestText.txt", notifications, SourceInfo.EmptySourceInfo);
			AssertContains(string.Format(ACSBIRDProcessor.MoreThanOneDeclarationWithEntryFilerCodeAndEntryNumber, "XJ560019292"), notifications.ToUniqueMessageListString());
		}

		public void TestBIRDImportWarningIsAddedToDeclarationNote()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.US.Business.JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "60019292";
			AssertEquals("PreCondition", ZString.Empty, declaration.US_EntryType);
			AssertEquals("PreCondition", 0, declaration.Invoices.Count);
			AssertEquals("PreCondition", 0, declaration.InvoiceLines.Count);
			Factory.Save();
			var notifications = new NotificationCollection();
			var importer = new BIRDDataImporter(Factory);
			importer.ImportData(new StreamReader(GetType().Assembly.GetManifestResourceStream(TestResource + "BIRDTestText.txt")), "BIRDTestText.txt", notifications, SourceInfo.EmptySourceInfo);
			AssertEquals("PreCondition", true, notifications.HasNotifications());
			AssertEquals("A note is added", 1, declaration.Notes.VisibleNotes.Count);
			var noteAdded = declaration.Notes.VisibleNotes[0];
			AssertEquals(ACSBIRDProcessor.BIRDImportWarningsNoteDescription, noteAdded.ST_Description);
			AssertEquals(true, noteAdded.ST_ForceRead);
			var warningMessageExpected = @"There is no organization with this entry filer code, 'XJ5'. The system cannot communicate with this filer automatically without this field in declarations. Please create an organization and specify its entry filer code in Organization > Config > Customs Codes.
Importer of Record: There is no organization found with this number, 91-013199000. You should create an organization record with this number and set the organization for the field.
Ultimate Consignee: There is no organization found with this number, 91-013199000. You should create an organization record with this number and set the organization for the field.
Manufacturer/Supplier: There is no organization found with this number, ECPEFCIA113MAN. You should create an organization record with this number and set the organization for the field.
Manufacturer/Supplier: There is no organization matching this MID, ECPEFCIA113MAN. System has created an organization named ECPEFCIA113MAN and a manufacturer query to Customs has been queued. On response, system will populate its name and address.";
			AssertContains(warningMessageExpected, noteAdded.ST_NoteDataAsText);
		}

		public void TestProcessBIRDLiquidationFromExternalBroker()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.US.Business.JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "ENT_03948";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			var notifications = new NotificationCollection();
			var importer = new BIRDDataImporter(Factory);
			importer.ImportData(new StreamReader(GetType().Assembly.GetManifestResourceStream(TestResource + "BIRDLiquidation.txt")), "BIRDLiquidation.txt", notifications, SourceInfo.EmptySourceInfo);
			Factory.Save();
			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, MQEDIMessage.ApplicationCodes.USCustomsImport);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation);
			query.AddToFilter(EDIMessageSchema.EM_Status, MQEDIMessage.Status.Queued);
			var message = Factory.LoadTop1<MQEDIMessage>(query);
			AssertNotNull(message);
			AssertNull("Not linked, but it will be when the queued message is processed", message.EM_LinkedObject);
			AssertMultilineASCIIEquals("ABI Message to Customs", @"B00    XJ5NR                                                                    
N12222XJ5ENT_0394801NNN-NN-NNNN 000000137000000000563000000013700000000056301   
N22222XJ5ENT_0394812030714BROKR-REFCBP_DOC_FILING_LOC6071407           000008247
N42222XJ5ENT_039480000000010000000000200000000003000000000040000000000500       
Y      XJ5NR00003", message.EM_FormattedMessageText);
			new USRIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			var liquidation = (CusLiquidation)message.EM_LinkedObject;
			AssertNotNull("liquidation object created", liquidation);
			AssertEquals(declaration, liquidation.Declaration);
			AssertEquals("marked as BIRD message input", ApplicationIdentifierCodeList.Codes.BIRDTransaction, message.EM_ApplicationReference);
		}

		sealed class ACSBIRDProcessorForTest : ACSBIRDProcessor
		{
			public ACSBIRDProcessorForTest(BusinessObjectFactoryProvider factoryProvider) : base(factoryProvider)
			{
			}

			protected override void ProcessFromHeaderToFooter(string birdApplicationID, JobDeclaration declaration, BlockControlGenerator<BRDAA, BRDZZ> generator, INotifications notifications)
				=> throw new Exception("blabla--this is a test--blalba");
		}

		sealed class BIRDDataImporterForTest : BIRDDataImporter
		{
			public BIRDDataImporterForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			internal override IBIRDProcessor DefaultProcessor => new ACSBIRDProcessorForTest(FactoryProvider);
		}
	}
}
