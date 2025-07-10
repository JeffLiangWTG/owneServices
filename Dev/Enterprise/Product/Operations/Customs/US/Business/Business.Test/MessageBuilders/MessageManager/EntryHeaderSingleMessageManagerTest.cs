using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.APHIS.ArticleCategory;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class EntryHeaderSingleMessageManagerTest : FormalEntrySingleMessageManagerTest
	{
		public void TestStatus()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;

			ImportMessageSendingActionCollection actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			EntryHeaderSingleMessageManager manager = new EntryHeaderSingleMessageManager(entry, (EntryHeaderMessageSendingAction)actions[0]);
			manager.GenerateOriginalMessages(entry);
			AssertEquals("Status is calculated", ImportMessageStatusList.Codes.AwaitingCargoReleaseOriginal, entry.CH_Status);
			manager.OnOriginalSent();
			AssertEquals("docData should be empty on a cargo release entry", 0, declaration.ActiveEntryHeaders.CargoReleaseEntry.US7501DocPrintingData.Count);
		}

		public void TestSendWhenCensusWarningChangedOnFactory()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save();

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			AssertEquals(1, actions.Count);

			var cwo = actions[0].CensusWarningCodes.AddNew();//this leads to difference between factory & db messages.
			cwo.EntryLinePK = declaration.InvoiceLines[0].JI_CL;
			cwo.ConditionCode = "1";
			cwo.OverrideCode = "A";

			actions[0].CensusWarningCodes.CopyToEntryLines();//This happens in MainSender

			Assert("Should allow this", actions.SendMessagesWithoutSaving(new SendsMessagesToCustomsShutterUpperer()));
		}

		public void TestEntrySummaryMessagesCreatesDocData()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 5000m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 5000.00m;
			invoiceLine.JI_Tariff = "1902.19.40 00";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			invoiceLine.US_PrivilegedStatusDate = new ZDateTime(2009, 1, 1);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("docData should be empty", 0, declaration.ActiveEntryHeaders.EntrySummaryEntry.US7501DocPrintingData.Count);

			CusEntryHeader entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			ImportMessageSendingActionCollection actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			EntryHeaderSingleMessageManager manager = new EntryHeaderSingleMessageManager(entry, (EntryHeaderMessageSendingAction)actions[0]);
			manager.GenerateOriginalMessages(entry);
			AssertEquals("Status is calculated", ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal, entry.CH_Status);
			manager.OnOriginalSent();
			AssertNotEquals("docData should have been created for EntrySummary message", 0, declaration.ActiveEntryHeaders.EntrySummaryEntry.US7501DocPrintingData.Count);
		}

		public void TestACEEntrySummaryMessagesCreatesDocData()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 5000m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 5000.00m;
			invoiceLine.JI_Tariff = "1902.19.40 00";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			invoiceLine.US_PrivilegedStatusDate = new ZDateTime(2009, 1, 1);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("docData should be empty", 0, declaration.ActiveEntryHeaders.EntrySummaryEntry.US7501DocPrintingData.Count);

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var manager = new EntryHeaderSingleMessageManager(entry, (EntryHeaderMessageSendingAction)actions[0]);
			manager.GenerateOriginalMessages(entry);
			AssertEquals("Status is calculated", ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal, entry.CH_Status);
			manager.OnOriginalSent();
			AssertNotEquals("docData should have been created for EntrySummary message", 0, declaration.ActiveEntryHeaders.EntrySummaryEntry.US7501DocPrintingData.Count);
		}

		public void TestFDAStatus()
		{
			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			consignee.OH_FullName = "Importer";
			OrgCusCode consigneeCustomsCode = consignee.CustomsCodes.AddNew();
			consigneeCustomsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			consigneeCustomsCode.OK_CustomsRegNo = "91-013199000";
			consigneeCustomsCode.OK_CodeType = OrgCusCode.CodeTypes.FDAEstablishmentIdentifier;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = consignee.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_CertifyCargoRelease = true;
			declaration.US_EnableENS = true;
			Factory.Save();
			AssertEquals("FDA Status - no FDA, status not yet set", ZString.Empty, declaration.FDAMsgStatus);

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;

			OrgHeader manufacturer = Factory.New<OrgHeader>();
			invoice.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			manufacturer.FillWithValidTestData();
			manufacturer.OH_FullName = "Manufacturer";
			OrgContact contact = manufacturer.Contacts.AddNew();
			contact.OC_ContactName = "Walter Doodleberry";
			contact.OC_Phone = "+1 (847) 364 5600";
			invoice.US_FDAContactName = "John";
			OrgCusCode manufacturerCode = manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "XYBEREQU6LON");

			new DeclarationTestHelper(Factory).SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.FDAs.AddNew();

			invoiceLine.FDAs[0].US_FDAQty1 = 100;
			invoiceLine.FDAs[0].US_FDAMeasure1 = FDABaseUQList.Codes.KG;

			invoiceLine.FDAs[0].US_FDAQty2 = 120;
			invoiceLine.FDAs[0].US_FDAMeasure2 = ShippingOrPackingingUnitList.Codes.Aerosol;

			invoiceLine.FDAs[0].US_ContainerDim1 = 2.89m;
			invoiceLine.FDAs[0].US_FDAContainerDimType = CylindricalRectangularList.Codes.Rectangular;
			invoiceLine.FDAs[0].US_FDACargoStorageCode = CargoStorageCodeList.Codes.AmbientTemperature;
			invoiceLine.FDAs[0].US_TradeBrandName = "TRADE NAME";
			invoiceLine.FDAs[0].US_FDACommercialDesc = "OTH DIST WINE>$3.43/L, AND";
			invoiceLine.FDAs[0].US_FDAValue = 10000.01m;
			invoiceLine.FDAs[0].US_FDAProductCode = "34AA.AA";

			Factory.Save();
			AssertEquals("FDA Message status prior to message sending should be set to FDA Required", FDAStatusList.Codes.REQ, declaration.FDAMsgStatus);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entry = declaration.ActiveEntryHeaders[0];
			ImportMessageSendingActionCollection actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			EntryHeaderSingleMessageManager manager = new EntryHeaderSingleMessageManager(entry, (EntryHeaderMessageSendingAction)actions[0]);
			manager.GenerateOriginalMessages(entry);
			AssertEquals("FDA Status is set", FDAStatusList.Codes.AWA, declaration.FDAMsgStatus);
		}

		[ExpectExceptionMessage(typeof(NotSupportedException), "Enterprise.Customs.US.Business.EntryHeaderSingleMessageManager.GenerateOriginalMessages doesn't support ImportMessageStatusList.MessageType : Export")]
		public void TestGenerateOriginalMessagesForExport()
		{
			manager = new EntryHeaderSingleMessageManager(entry, ImportMessageStatusList.MessageType.Export);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CusEntryHeader entryLoaded = factory2.Load<CusEntryHeader>(entry.PK);
			manager.GenerateOriginalMessages(entryLoaded);
		}

		[ExpectExceptionMessage(typeof(NotSupportedException), "Enterprise.Customs.US.Business.EntryHeaderSingleMessageManager.GenerateOriginalMessages doesn't support ImportMessageStatusList.MessageType : InBondUpdate")]
		public void TestGenerateOriginalMessagesForInBondWP()
		{
			manager = new EntryHeaderSingleMessageManager(entry, ImportMessageStatusList.MessageType.InBondUpdate);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CusEntryHeader entryLoaded = factory2.Load<CusEntryHeader>(entry.PK);
			manager.GenerateOriginalMessages(entryLoaded);
		}

		[ExpectExceptionMessage(typeof(NotSupportedException), "Enterprise.Customs.US.Business.EntryHeaderSingleMessageManager.GenerateWithdrawalMessages doesn't support ImportMessageStatusList.MessageType : Export")]
		public void TestGenerateWithdrawalMessagesForExport()
		{
			manager = new EntryHeaderSingleMessageManager(entry, ImportMessageStatusList.MessageType.Export);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CusEntryHeader entryLoaded = factory2.Load<CusEntryHeader>(entry.PK);
			manager.GenerateWithdrawalMessages(entryLoaded);
		}

		[ExpectExceptionMessage(typeof(NotSupportedException), "Enterprise.Customs.US.Business.EntryHeaderSingleMessageManager.GenerateWithdrawalMessages doesn't support ImportMessageStatusList.MessageType : InBondUpdate")]
		public void TestGenerateWithdrawalMessagesForInBondWP()
		{
			manager = new EntryHeaderSingleMessageManager(entry, ImportMessageStatusList.MessageType.InBondUpdate);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CusEntryHeader entryLoaded = factory2.Load<CusEntryHeader>(entry.PK);
			manager.GenerateWithdrawalMessages(entryLoaded);
		}

		public void TestGenerateWithdrawalMessagesForFormalEntry()
		{
			manager = new EntryHeaderSingleMessageManager(entry, ImportMessageStatusList.MessageType.EntrySummary);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CusEntryHeader entryLoaded = factory2.Load<CusEntryHeader>(entry.PK);
			Enterprise.Messaging.Business.EDIMessage[] result = manager.GenerateWithdrawalMessages(entryLoaded);

			AssertEquals("entryLoaded should be the entity to generate a message for. it is important to use the passed entity", 1, entryLoaded.Messages.Count);
			AssertEquals("there is one message generated", 1, result.Length);
			AssertEquals("Message sub type", EM_MessageSubTypeList.Codes.EntrySummaryDelete, result[0].EM_MessageSubType);
			AssertEquals("message type", ApplicationIdentifierCodeList.Codes.EntrySummary, result[0].EM_MessageType);
		}

		public void TestGenerateWithdrawalMessagesForFormalEntryForACE()
		{
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save();

			ImportMessageSendingActionCollection actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			manager = new EntryHeaderSingleMessageManager(entry, (EntryHeaderMessageSendingAction)actions[0]);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CusEntryHeader entryLoaded = factory2.Load<CusEntryHeader>(entry.PK);
			entryLoaded.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Enterprise.Messaging.Business.EDIMessage[] result = manager.GenerateWithdrawalMessages(entryLoaded);

			AssertEquals("entryLoaded should be the entity to generate a message for. it is important to use the passed entity", 1, entryLoaded.Messages.Count);
			AssertEquals("there is one message generated", 1, result.Length);
			AssertEquals("Message sub type", EM_MessageSubTypeList.Codes.EntrySummaryDelete, result[0].EM_MessageSubType);
			AssertEquals("ACE 7501", ACEApplicationIdentifierCodeList.Codes.EntrySummary, result[0].EM_MessageType);
		}

		public void TestGetStringRepresentationFor()
		{
			AssertEquals(MessageAttacheeRecordTypeDescriptions.Entry, new EntryHeaderSingleMessageManager(entry, ImportMessageStatusList.MessageType.EntrySummary).GetStringRepresentationFor());

			entry.EntryNumber = "0000";
			AssertEquals("Entry - 0000", new EntryHeaderSingleMessageManager(entry, ImportMessageStatusList.MessageType.EntrySummary).MessageFriendlyName);
		}

		public void TestGenerateAmendmentMessagesForEntrySummary()
		{
			CusEntryHeader ensEntry = declaration.CustomsEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ensEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			ImportMessageSendingActionCollection actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement, (x) => x.CH_Status == ImportMessageStatusList.Codes.ClearEntrySummaryOriginal);
			actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary).US_SendMessage = true;

			actions.SendMessagesWithoutSaving(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("One message should have been generated", 1, ensEntry.Messages.Count);
			AssertEquals("Status should have been updated", ImportMessageStatusList.Codes.AwaitingEntrySummaryReplace, ensEntry.CH_Status);
		}

		public void TestGenerateAmendmentMessagesForEntrySummaryForACE()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			CusEntryHeader ensEntry = declaration.CustomsEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ensEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			ImportMessageSendingActionCollection actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement, (x) => x.CH_Status == ImportMessageStatusList.Codes.ClearEntrySummaryOriginal);
			actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary).US_SendMessage = true;

			actions.SendMessagesWithoutSaving(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("One message should have been generated", 1, ensEntry.Messages.Count);
			AssertEquals("Status should have been updated", ImportMessageStatusList.Codes.AwaitingEntrySummaryReplace, ensEntry.CH_Status);
			AssertEquals("Message generated: ACE 7501", ACEApplicationIdentifierCodeList.Codes.EntrySummary, ensEntry.Messages[0].EM_MessageType);
		}

		public void TestGenerateAmendmentMessages_CreateDocPrintingDetails()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 5000m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 5000.00m;
			invoiceLine.JI_Tariff = "1902.19.40 00";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			invoiceLine.US_PrivilegedStatusDate = new ZDateTime(2009, 1, 1);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("docData should be empty", 0, declaration.ActiveEntryHeaders.EntrySummaryEntry.US7501DocPrintingData.Count);

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			ImportMessageSendingActionCollection actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary).US_SendMessage = true;

			actions.SendMessagesWithoutSaving(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("One message should have been generated", 1, entry.Messages.Count);

			IEnumerable<US7501DocPrinting> docPrintings = entry.US7501DocPrintingData.Find(x => x.US_MsgPK == entry.Messages[0].PK);
			var docPrintingData = new List<US7501DocPrinting>(new TypedEnumerable<US7501DocPrinting>(docPrintings));

			AssertEquals(1, docPrintingData.Count);
		}

		[TestDate(2016, 01, 29)]
		public void TestPGADeferMessaging()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableCRL = true;
			declaration.US_EnableENS = false;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			declaration.US_FDAADTA = new ZDateTime(2016, 01, 14, 12, 04, 05);
			declaration.US_FDAContactName = "Joo Youm";
			declaration.US_FDAContactPhoneNo = "5555555555";
			declaration.US_FDAContactEmail = "joo.youm@test.com";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Description = "TOBACCO PRODUCT";

			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.TOB;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.TOB_CSU;
			fda.US_ProductCode = "12AAB01";
			fda.US_ProdCountry = "CA";
			fda.US_Description = "TEST";
			fda.US_IntendedUseCode = "150.000";
			fda.US_Qty1 = 2323m;
			fda.US_UQ1 = FDABaseUQList.Codes.PCS;
			fda.US_Qty2 = 100m;
			fda.US_UQ2 = "CT";
			fda.US_BrandName = "BRAND NAME";

			invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Declared;
			var nhtsaHeader = invoiceLine.NHTSALines.AddNew();
			nhtsaHeader.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			nhtsaHeader.US_NHTBoxNumber = DepartmentOfTransportBoxNumberList.Codes._01;

			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
			var aphisHeader = invoiceLine.APHISHeaders.AddNew();
			aphisHeader.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			aphisHeader.Inspections.RemoveAndDeleteAll();
			aphisHeader.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.APHISVSPortVeterinarian;
			aphisHeader.US_IntendedUseCode = IntendedUseCodesList.Codes.ConsumerProductIntendedForAdolescentsAged6To8Years;
			aphisHeader.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			aphisHeader.US_CategoryCode = LiveAnimalsList.Codes.BosAndBisonDomesticCattleHumpedCattleAndBison;
			aphisHeader.US_ProductType = ProductCodeQualifiersList.Codes.TaxonomicSerialNumber;
			aphisHeader.US_ProductNumber = "183838";
			aphisHeader.US_ScientificGenusName = "BOS";
			aphisHeader.US_ScientificSpeciesName = "TAURUS";

			var inspection = aphisHeader.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.ACECargoRelease).US_SendMessage = true;
			actions.SendMessagesWithoutSaving(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("One message should have been generated", 1, seEntry.Messages.Count);
			var message = seEntry.Messages[0];
			AssertEquals("Message generated: Simplified Entry", ACEApplicationIdentifierCodeList.Codes.CargoRelease, message.EM_MessageType);
			AssertContains(@"OI        TOBACCO PRODUCT                                                       
PG01001FDATOBCSU                         150.000                                
PG02PFDP 12AAB01                                                                
PG0639 CA                                                                       
PG07BRAND NAME                                                                  
PG10                   TEST                                                     
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21PK JOO YOUM               5555555555     JOO.YOUM@TEST.COM                  
PG25                                                    000000010000            
PG261000000010000CT                                                             
PG262000000232300PCS                                                            
PG30A011420161204                                                               ", message.EM_FormattedMessageText);
			message.Delete();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.NonWeeklyEstimateFilingDate;
			actions.SendMessagesWithoutSaving(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("One message should have been generated", 1, seEntry.Messages.Count);
			message = seEntry.Messages[0];
			AssertEquals("Message generated: Simplified Entry", ACEApplicationIdentifierCodeList.Codes.CargoRelease, message.EM_MessageType);
			AssertContains(@"PG01001FDATOBCSU                         150.000                                
PG02PFDP 12AAB01                                                                
PG0639 CA                                                                       
PG07BRAND NAME                                                                  
PG10                   TEST                                                     
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21PK JOO YOUM               5555555555     JOO.YOUM@TEST.COM                  
PG25                                                    000000010000            
PG261000000010000CT                                                             
PG262000000232300PCS                                                            
PG30A011420161204                                                               
PG01001NHTMVS                                                                   
PG02P                                                                           
PG22Y946    1    CI NH1 Y01292016                                               ", message.EM_FormattedMessageText);
			message.Delete();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			seEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.ACECargoRelease).US_SendMessage = true;
			actions.SendMessagesWithoutSaving(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("One message should have been generated", 1, seEntry.Messages.Count);
			message = seEntry.Messages[0];
			AssertEquals("Message generated: Simplified Entry", ACEApplicationIdentifierCodeList.Codes.CargoRelease, message.EM_MessageType);
			AssertNotContains(@"OI        TOBACCO PRODUCT                                                       ", message.EM_FormattedMessageText);
			message.Delete();
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.ACECargoRelease).US_SendMessage = true;
			actions.SendMessagesWithoutSaving(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("One message should have been generated", 1, seEntry.Messages.Count);
			message = seEntry.Messages[0];
			AssertEquals("Message generated: Simplified Entry", ACEApplicationIdentifierCodeList.Codes.CargoRelease, message.EM_MessageType);
			AssertContains(@"OI        TOBACCO PRODUCT                                                       
PG01001FDATOBCSU                         150.000                                
PG02PFDP 12AAB01                                                                
PG0639 CA                                                                       
PG07BRAND NAME                                                                  
PG10                   TEST                                                     
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21PK JOO YOUM               5555555555     JOO.YOUM@TEST.COM                  
PG25                                                    000000010000            
PG261000000010000CT                                                             
PG262000000232300PCS                                                            
PG30A011420161204                                                               ", message.EM_FormattedMessageText);
			declaration.US_EnableENS = true;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var ensEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var action = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			action.US_SendMessage = true;
			action.US_CertifyCargoRelease = false;
			actions.SendMessagesWithoutSaving(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("One message should have been generated", 1, ensEntry.Messages.Count);
			message = ensEntry.Messages[0];
			AssertEquals("Message generated: Entry Summary", ACEApplicationIdentifierCodeList.Codes.EntrySummary, message.EM_MessageType);
			AssertContains(@"OI        TOBACCO PRODUCT                                                       
PG01001NHTMVS                                                                   
PG02P                                                                           
PG22Y946    1    CI NH1 Y01292016                                               
", message.EM_FormattedMessageText);
			message.Delete();
			ensEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			invoiceHeader.US_NHTSASignDate = ZDateTime.Today;
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			action = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			action.US_SendMessage = true;
			action.US_CertifyCargoRelease = false;
			actions.SendMessagesWithoutSaving(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("One message should have been generated", 1, ensEntry.Messages.Count);
			message = ensEntry.Messages[0];
			AssertEquals("Message generated: Entry Summary", ACEApplicationIdentifierCodeList.Codes.EntrySummary, message.EM_MessageType);
			AssertContains(@"OI        TOBACCO PRODUCT                                                       
PG01001NHTMVS                                                                   
PG02P                                                                           
PG22Y946    1    CI NH1 Y01292016                                               ", message.EM_FormattedMessageText);
			message.Delete();
			ensEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryReplace;
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			action = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			action.US_SendMessage = true;
			action.US_CertifyCargoRelease = false;
			actions.SendMessagesWithoutSaving(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("One message should have been generated", 1, ensEntry.Messages.Count);
			message = ensEntry.Messages[0];
			AssertEquals("Message generated: Entry Summary", ACEApplicationIdentifierCodeList.Codes.EntrySummary, message.EM_MessageType);
			AssertNotContains(@"OI        TOBACCO PRODUCT                                                       ", message.EM_FormattedMessageText);
			message.Delete();
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			action = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			action.US_SendMessage = true;
			action.US_CertifyCargoRelease = true;
			actions.SendMessagesWithoutSaving(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("One message should have been generated", 1, ensEntry.Messages.Count);
			message = ensEntry.Messages[0];
			AssertEquals("Message generated: Entry Summary", ACEApplicationIdentifierCodeList.Codes.EntrySummary, message.EM_MessageType);
			AssertContains(@"OI        TOBACCO PRODUCT                                                       
PG01001NHTMVS                                                                   
PG02P                                                                           
PG22Y946    1    CI NH1 Y01292016                                               ", message.EM_FormattedMessageText);
		}

		public void TestGenerateMessagesForSimplifiedEntry()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;

			var seEntry = declaration.CustomsEntryHeaders.AddNew();
			seEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.ACECargoRelease).US_SendMessage = true;
			actions.SendMessagesWithoutSaving(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("One message should have been generated", 1, seEntry.Messages.Count);
			AssertEquals("Status should have been updated", ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd, seEntry.CH_Status);
			AssertEquals("Message generated: Simplified Entry Add", ACEApplicationIdentifierCodeList.Codes.CargoRelease, seEntry.Messages[0].EM_MessageType);

			seEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			var singleAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.ACECargoRelease);
			singleAction.US_SendMessage = true;
			actions.SendMessagesWithoutSaving(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("One message should have been generated", 2, seEntry.Messages.Count);
			AssertEquals("Status should have been updated", ImportMessageStatusList.Codes.AwaitingACECargoReleaseReplace, seEntry.CH_Status);
			AssertEquals("Message generated: Simplified Entry Replace", ACEApplicationIdentifierCodeList.Codes.CargoRelease, seEntry.Messages[1].EM_MessageType);

			seEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseReplace;
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			singleAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.ACECargoRelease);
			singleAction.US_SendMessage = true;
			actions.SendMessagesWithoutSaving(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("One message should have been generated", 3, seEntry.Messages.Count);
			AssertEquals("Status should have been updated", ImportMessageStatusList.Codes.AwaitingACECargoReleaseDelete, seEntry.CH_Status);
			AssertEquals("Message generated: Simplified Entry Delete", ACEApplicationIdentifierCodeList.Codes.CargoRelease, seEntry.Messages[2].EM_MessageType);
		}

		public void TestGenerateAmendmentForPSCOriginallyFiledByOtherFiler()
		{
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EntryFiler() { EntryFilerCode = "XJ5" });

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_PSC = true;
			declaration.US_EntryFilerCode = "EEB";
			declaration.US_EnableENS = true;
			Assert(declaration.IsPSCFilingOfEntriesByOtherFiler);

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			AssertEquals("Should be able to send an original", 1, actions.Count);

			actions[0].US_PSCExplanation = "TEST";
			actions[0].US_SendMessage = true;
			actions.SendMessagesWithoutSaving(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("One message should have been generated", 1, entry.Messages.Count);

			var message = (MQEDIMessage)entry.Messages[0];
			var ens36 = message.MessageBlock.MessageBlocks.Find(block => block is Messaging.Business.MessageBuildingBlocks.ACE.Common.AENS36);
			AssertNotNull("PSC explanation is sent", ens36);
		}

		public override void TestGetCommonNotificationsForSending()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = false;
			declaration.US_EnableINB = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			CusEntryHeader ensEntry = declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.EntrySummary)[0];
			CusEntryHeader inbEntry = declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.InBondDeparture)[0];
			ensEntry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			inbEntry.CH_Status = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;

			AssertEquals("IsWaitingForResponse", true, ensEntry.IsWaitingForResponse);
			AssertEquals("IsWaitingForResponse", true, inbEntry.IsWaitingForResponse);

			Factory.Save();

			ImportMessageSendingActionCollection actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			AssertEquals("PreCondition:two actions for each entry", 2, actions.Count);

			AssertEquals("ValidationModes set", ValidationModes.EntrySummary, declaration.ValidationModes);
			declaration.RunPreSaveValidation();
			AssertEquals("PreCondition:declaration has message errors", true, declaration.HasMessageErrors);

			EntryHeaderSingleMessageManager ensManager = new EntryHeaderSingleMessageManager(ensEntry, actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary));
			EntryHeaderSingleMessageManager inbManager = new EntryHeaderSingleMessageManager(inbEntry, actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.InBondDeparture));

			MessageSendingNotificationCollection ensNotifications = ensManager.GetNotificationsForSendingAnOriginal();
			MessageSendingNotificationCollection inbNotifications = inbManager.GetNotificationsForSendingAnOriginal();

			//if you have added extra message sending notifications as opposed to business layer validations, you can increase this count
			//and check the error or warning text. EntryHeaderSingleMessageManager's Top BizObj to collect validations from is 
			//JobDeclaration to include invoice or invoice lines notifications, but the problem with this is that 
			//if we have two entries to send messages for, the validations collected from invoices or invoice lines are warned twice
			//SendMessages menu click handler in EDIMenu collects manually once from dbo.JobDeclaration
			//For US, users can send messages while waiting for responses
			AssertEquals(0, ensNotifications.ErrorCount);
			AssertEquals(0, inbNotifications.WarningCount);
		}

		protected override FormalEntrySingleMessageManager GetTestManager(Customs.Business.CusEntryHeader entryHeader) => new EntryHeaderSingleMessageManager((CusEntryHeader)entryHeader, ensMessageSendingAction);

		protected override SingleMessageManager GetNewSingleMessageManager() => GetTestManager(entry);

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = CargoReleaseTypeList.Codes.ACS;
			declaration.US_EnableENS = true;
			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();

			ImportMessageSendingActionCollection collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			ensMessageSendingAction = collection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			manager = (EntryHeaderSingleMessageManager)GetTestManager(entry);
		}

		JobDeclaration declaration;
		CusEntryHeader entry;
		EntryHeaderSingleMessageManager manager;
		EntryHeaderMessageSendingAction ensMessageSendingAction;
	}
}
