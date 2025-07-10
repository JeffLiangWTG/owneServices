using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class StatementDateChangeRequestTest : TestCaseWithFactory
	{
		[ExpectNoExceptions()]
		public void TestGenerateStatementDateChangeRequest_Issue173337()
		{
			SetUpAutoSendStatementDateChangeRequestRegistry("ALL");
			SetUpDefaultPSDRegistry(8);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(1);

			var entryENS = declaration.ActiveEntryHeaders.AddNew();
			entryENS.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryENS.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			var entryCRL = declaration.ActiveEntryHeaders.AddNew();
			entryCRL.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;

			AssertEquals(0, declaration.Messages.Count);
			Factory.Save();

			declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			Factory.Save();

			AssertEquals(1, declaration.Messages.Count);
		}

		[TestDate(2015, 07, 25)]
		public void TestBuildStatementDateChangeRequestForLiveEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_SchDEntry = "8888";
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2011, 1, 2);

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.EntryNumber = "12345";
			new StatementDateChangeRequest(declaration).BuildStatementDateChangeRequestUsingCurrentPSD();

			AssertEquals(1, declaration.Messages.Count);
			AssertEquals(EM_MessageSubTypeList.Codes.StatementUpdateMessage, declaration.Messages[0].EM_MessageSubType);
			AssertContains("H8888XJ5  12345   2010211", declaration.Messages[0].EM_MessageText);
		}

		[TestDate(2014, 6, 2)]
		public void TestUpdateReleaseDateTwiceInOneFactory_CS00312881()
		{
			SetUpDefaultPSDRegistry(8);
			SetUpAutoSendStatementDateChangeRequestRegistry("ALL");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_SchDEntry = "8888";
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2014, 6, 11);
			declaration.US_PeriodicStatementMM = "06";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.EntryNumber = "12345";
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save();

			//two RR messages processed in the same batch
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 6, 2);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 5, 30);

			Factory.Save();
			AssertNull("No need to send STU as PSD calculated is PSD accepted so far", declaration.Messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_MessageType == ApplicationIdentifierCodeList.Codes.StatementDeleteTransaction));
		}

		public void TestNoMessageGeneratedByRegistryDefault()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			var entry = Factory.NewWithValidTestData<CusEntryHeader>();
			entry.CH_JE = declaration.PK;
			declaration.US_EntryType = "01";
			entry.EntryNumber = "12345";
			AssertEquals("Pre-condition", 0, declaration.Messages.Count);

			new StatementDateChangeRequest(declaration).GenerateStatementDateChangeRequestIfNecessary();
			AssertEquals("Statement Date Change Request Message should not be auto generated under registry default setting", 0, declaration.Messages.Count);
		}

		public void TestNoMessageGeneratedWhenPSPDIsEmpty()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Empty;
			var entry = Factory.NewWithValidTestData<CusEntryHeader>();
			entry.CH_JE = declaration.PK;
			declaration.US_EntryType = "01";
			entry.EntryNumber = "12345";
			AssertEquals("Pre-condition", 0, declaration.Messages.Count);

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2008, 07, 10);

			AssertEquals("Statement Date Change Request Message should not be auto generated if preliminary statement print date is empty", 0, declaration.Messages.Count);
		}

		public void TestNoMessageGeneratedWhenEntryAuthorisationDateIsEmpty()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Empty;
			var entry = Factory.NewWithValidTestData<CusEntryHeader>();
			entry.CH_JE = declaration.PK;
			declaration.US_EntryType = "01";
			entry.EntryNumber = "12345";
			AssertEquals("Statement Date Change Request Message should not be auto generated if JE_EntryAuthorisationDate is empty", 0, declaration.Messages.Count);
		}

		public void TestNoMessageGeneratedWhenEntryExistsButNotCleared()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Empty;
			var entry = Factory.NewWithValidTestData<CusEntryHeader>();
			entry.CH_JE = declaration.PK;
			declaration.US_EntryType = "01";
			entry.EntryNumber = "12345";
			AssertEquals("Pre-condition", 0, declaration.Messages.Count);

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2008, 07, 10);

			AssertEquals("Statement Date Change Request Message should not be auto generated entry is not cleared", 0, declaration.Messages.Count);
		}

		[TestDate(2008, 07, 04)]
		public void TestNoMessageGeneratedWhenPSPDIsSameAsNewCalculationOfPSPD()
		{
			SetUpDefaultPSDRegistry(1);
			SetUpAutoSendStatementDateChangeRequestRegistry("ALL");

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2008, 07, 08);
			declaration.US_PeriodicStatementMM = "08";
			var entry = Factory.NewWithValidTestData<CusEntryHeader>();
			entry.CH_JE = declaration.PK;
			declaration.US_EntryType = "01";
			entry.EntryNumber = "12345";
			AssertEquals("Pre-condition", 0, declaration.Messages.Count);

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2008, 07, 07);

			AssertEquals("Statement Date Change Request Message should not be auto generated if the new calculation of the preliminary statement print date based on the release date is the same as the current PSPD", 0, declaration.Messages.Count);
		}

		[TestDate(2008, 07, 04)]
		public void TestNoMessageGeneratedWhenNewDateIsTodayOrEalier()
		{
			SetUpDefaultPSDRegistry(1);
			SetUpAutoSendStatementDateChangeRequestRegistry("ALL");

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2008, 07, 04);
			declaration.US_PeriodicStatementMM = "08";

			var entry = Factory.NewWithValidTestData<CusEntryHeader>();
			entry.CH_JE = declaration.PK;
			declaration.US_EntryType = "01";
			entry.EntryNumber = "12345";
			AssertEquals("Pre-condition", 0, declaration.Messages.Count);

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2008, 07, 03);

			AssertEquals("Statement Date Change Request Message should not be auto generated if the new calculation of the preliminary statement print date based on the release date is today or earlier", 0, declaration.Messages.Count);
		}

		[TestDate(2008, 07, 05)]
		public void TestNoMsgGeneratedWhenPresentationDateHasValue()
		{
			SetUpDefaultPSDRegistry(8);
			SetUpAutoSendStatementDateChangeRequestRegistry("ALL");

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2008, 07, 10);
			declaration.US_PresentationDate = new ZDateTime(2008, 07, 15);
			var entry = Factory.NewWithValidTestData<CusEntryHeader>();
			entry.CH_JE = declaration.PK;
			declaration.US_EntryType = "01";
			entry.EntryNumber = "12345";
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2008, 07, 08);

			AssertEquals("Statement Date Change Request Message should not be auto generated if the Presentation Date has a value", 0, declaration.Messages.Count);
		}

		[TestDate(2008, 07, 05)]
		public void TestNoMsgGeneratedWhenLiveEntry()
		{
			SetUpDefaultPSDRegistry(8);
			SetUpAutoSendStatementDateChangeRequestRegistry("ALL");

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2008, 07, 10);
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;
			var entry = Factory.NewWithValidTestData<CusEntryHeader>();
			entry.CH_JE = declaration.PK;
			declaration.US_EntryType = "01";
			entry.EntryNumber = "12345";
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2008, 07, 08);

			AssertEquals("Statement Date Change Request Message should not be auto generated if the Live Entry Indicator is set", 0, declaration.Messages.Count);
		}

		[TestDate(2008, 07, 05)]
		public void TestMessageIsNotGeneratedForOrgExcluded()
		{
			SetUpDefaultPSDRegistry(5);
			SetUpAutoSendStatementDateChangeRequestRegistry("ORG");

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			((OrgImpAddInfo)importer.CountryData.ImpAddInfo).ZO_DoNotAutoGenerateSDCR = false;
			Factory.Save();

			var iOR = Factory.NewWithValidTestData<OrgHeader>();
			((OrgImpAddInfo)iOR.CountryData.ImpAddInfo).ZO_DoNotAutoGenerateSDCR = true;
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2008, 07, 10);
			declaration.JE_OH_Importer = importer.PK;
			declaration.IOROrgPK = iOR.PK;
			var entry = Factory.NewWithValidTestData<CusEntryHeader>();
			entry.CH_JE = declaration.PK;
			declaration.US_EntryType = "01";
			entry.EntryNumber = "12345";
			AssertEquals("Pre-condition", 0, declaration.Messages.Count);

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2008, 07, 08);
			Factory.Save();
			AssertEquals("Message should not be sent for this organisation as it has been flagged to opt out of Auto ENS sending", 0, declaration.Messages.Count);
		}

		[TestDate(2008, 07, 05)]
		public void TestMessageIsGeneratedForOrgIndicated()
		{
			SetUpDefaultPSDRegistry(5);
			SetUpAutoSendStatementDateChangeRequestRegistry("ORG");

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var iOR = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_OH_Importer = importer.PK;
			declaration.IOROrgPK = iOR.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_SchDEntry = "8888";
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporterWithSuffixes;
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2008, 07, 10);
			declaration.US_EntryType = "01";

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "12345";
			Factory.Save();

			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save();
			AssertEquals("Pre-condition", 0, declaration.Messages.Count);

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2008, 07, 08);

			Factory.Save();

			AssertEquals(1, declaration.Messages.Count);
			AssertEquals(EM_MessageSubTypeList.Codes.StatementUpdateMessage, declaration.Messages[0].EM_MessageSubType);
			AssertEquals("Message generation expected", "B  8888XJ5SU                                               EDIEDIDAT_1          H8888XJ5  12345   5071508                                                       Y  8888XJ5SU", declaration.Messages[0].EM_MessageText);
		}

		[TestDate(2008, 09, 19)]
		public void TestMessageIsNotGeneratedForPeriodicMMChangeWhenOrgExcluded()
		{
			SetUpDefaultPSDRegistry(5);
			SetUpAutoSendStatementDateChangeRequestRegistry("ORG");

			var iOR = Factory.NewWithValidTestData<OrgHeader>();
			((OrgImpAddInfo)iOR.CountryData.ImpAddInfo).ZO_DoNotAutoGenerateSDCR = true;
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2008, 09, 22);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;
			declaration.US_PeriodicStatementMM = "09";
			declaration.IOROrgPK = iOR.PK;
			var entry = Factory.NewWithValidTestData<CusEntryHeader>();
			entry.CH_JE = declaration.PK;
			declaration.US_EntryType = "01";
			entry.EntryNumber = "12345";
			AssertEquals("Pre-condition", 0, declaration.Messages.Count);

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2008, 09, 15);
			Factory.Save();
			AssertEquals("Message should not be sent for this Period MM change as importer of record organisation has been flagged to opt out of Auto ENS sending", 0, declaration.Messages.Count);
		}

		[TestDate(2008, 07, 05)]
		public void TestGenerateStatementDateChangeRequest()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			branch.GB_Code = "EYL";

			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, "8888");

			DefaultStatementPrintDate statementData = new DefaultStatementPrintDate();
			statementData.DoDefaultPrelimStatementPrintDate = true;
			statementData.NumberOfDays = 8;
			USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, statementData);

			AutoSendStatementDateChangeRequest autoGenerate = new AutoSendStatementDateChangeRequest();
			autoGenerate.OverrideAllOrByOrganisation = "ALL";
			USCustomsDataRegistry.Instance.AutoSendSDCR.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, autoGenerate);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_SchDEntry = "8888";
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2008, 07, 10);
			declaration.US_EntryType = "01";
			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			var entry = declaration.CustomsEntryHeaders[0];
			entry.EntryNumber = "12345";
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save();

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2008, 07, 08);
			Factory.Save();

			AssertEquals(1, declaration.Messages.Count);
			AssertEquals(EM_MessageSubTypeList.Codes.StatementUpdateMessage, declaration.Messages[0].EM_MessageSubType);
			AssertEquals("Message generation expected", "B  8888XJ5SU                                               EDIEDIDAT_1          H8888XJ5  12345   3071808                                                       Y  8888XJ5SU", declaration.Messages[0].EM_MessageText);
			AssertEquals(branch.PK, declaration.Messages[0].EM_GB);
		}

		[TestDate(2008, 09, 19)]
		public void TestMessageIsNotGeneratedForPeriodicMonthChangeOnIncorrectPaymentType()
		{
			SetUpDefaultPSDRegistry(5);
			SetUpAutoSendStatementDateChangeRequestRegistry("ORG");

			var iOR = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2008, 09, 22);
			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			declaration.US_PeriodicStatementMM = "09";
			declaration.IOROrgPK = iOR.PK;
			var entry = Factory.NewWithValidTestData<CusEntryHeader>();
			entry.CH_JE = declaration.PK;
			declaration.US_EntryType = "01";
			entry.EntryNumber = "12345";
			AssertEquals("Pre-condition", 0, declaration.Messages.Count);

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2008, 09, 15);

			Factory.Save();

			AssertEquals("SDCR message should not generate - no change to PSPD & PeriodicMonthChange not required payment type", 0, declaration.Messages.Count);
		}

		public void TestSDGRIsNotGeneratedOnDateExclusion()
		{
			SetUpDefaultPSDRegistry(8);
			SetUpAutoSendStatementDateChangeRequestRegistry("ALL");

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Today;
			var entry = Factory.NewWithValidTestData<CusEntryHeader>();
			entry.CH_JE = declaration.PK;
			declaration.US_EntryType = "01";
			entry.EntryNumber = "12345";
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today;

			Factory.Save();

			AssertEquals("Statement Date Change Request Message should not be auto generated if today's date is not less than current Preliminary Statement Date", 0, declaration.Messages.Count);
		}

		[TestDate(2008, 09, 19)]
		public void TestMessageIsGeneratedForPeriodicMonthChange()
		{
			SetUpDefaultPSDRegistry(5);
			SetUpAutoSendStatementDateChangeRequestRegistry("ORG");

			var iOR = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.IOROrgPK = iOR.PK;
			declaration.US_EntryType = "01";
			declaration.US_SchDEntry = "8888";
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2008, 09, 23);
			declaration.US_PeriodicStatementMM = "09";

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "12345";
			Factory.Save();

			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save();
			AssertEquals("Pre-condition", 0, declaration.Messages.Count);

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2008, 09, 15);

			Factory.Save();

			AssertEquals(1, declaration.Messages.Count);
			AssertEquals(EM_MessageSubTypeList.Codes.StatementUpdateMessage, declaration.Messages[0].EM_MessageSubType);
			AssertEquals("Message generation expected", "B  8888XJ5SU                                               EDIEDIDAT_1          H8888XJ5  12345   7092208  10                                                   Y  8888XJ5SU", declaration.Messages[0].EM_MessageText);
		}

		[TestDate(2008, 07, 05)]
		public void TestNoMessageGeneratedWhenPSDIsFixed()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			branch.GB_Code = "EYL";

			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, "8888");

			SetUpDefaultPSDRegistry(8);
			SetUpAutoSendStatementDateChangeRequestRegistry("ALL");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_SchDEntry = "8888";
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2008, 07, 10);
			declaration.US_FixPSD = true;
			declaration.US_EntryType = "01";
			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			var entry = declaration.CustomsEntryHeaders[0];
			entry.EntryNumber = "12345";
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryReplace;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2008, 07, 08);

			Factory.Save();

			AssertEquals("Statement Date Change Request Message should not be auto generated when the preliminary statement print date has been fixed", 0, declaration.Messages.Count);
		}

		[TestDate(2010, 06, 01)]
		public void TestGenerationOfPeriodicStatementMonth()
		{
			SetUpDefaultPSDRegistry(5);
			SetUpAutoSendStatementDateChangeRequestRegistry("ORG");

			var iOR = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.IOROrgPK = iOR.PK;
			declaration.US_EntryType = "01";
			declaration.US_SchDEntry = "8888";
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2010, 06, 10);
			declaration.US_PeriodicStatementMM = "07";

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "12345";
			Factory.Save();

			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save();
			AssertEquals("Pre-condition", 0, declaration.Messages.Count);

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2010, 06, 15);

			Factory.Save();

			AssertEquals(1, declaration.Messages.Count);
			AssertEquals(EM_MessageSubTypeList.Codes.StatementUpdateMessage, declaration.Messages[0].EM_MessageSubType);
			AssertEquals("Message generation expected", "B  8888XJ5SU                                               EDIEDIDAT_1          H8888XJ5  12345   7062210  07                                                   Y  8888XJ5SU", declaration.Messages[0].EM_MessageText);
		}

		[TestDate(2010, 06, 01)]
		public void TestGenerationOfPeriodicStatementMonthWhenStatementDateRollsToNextMonth()
		{
			SetUpDefaultPSDRegistry(5);
			SetUpAutoSendStatementDateChangeRequestRegistry("ORG");

			var iOR = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.IOROrgPK = iOR.PK;
			declaration.US_EntryType = "01";
			declaration.US_SchDEntry = "8888";
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2010, 06, 25);
			declaration.US_PeriodicStatementMM = "07";

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "12345";
			Factory.Save();

			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save();
			AssertEquals("Pre-condition", 0, declaration.Messages.Count);

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2010, 06, 29);

			Factory.Save();

			AssertEquals(1, declaration.Messages.Count);
			AssertEquals(EM_MessageSubTypeList.Codes.StatementUpdateMessage, declaration.Messages[0].EM_MessageSubType);
			AssertEquals("Message generation expected - PeriodicStatementMonth should still be 07 - based on JE_EntryAuthorisationDate not StatementDate", "B  8888XJ5SU                                               EDIEDIDAT_1          H8888XJ5  12345   7070710  07                                                   Y  8888XJ5SU", declaration.Messages[0].EM_MessageText);
		}

		[TestDate(2011, 01, 17)]
		public void TestSendStatementUpdateMessageWhenArrivalDateIsUpdatedVia3461_CS00135185()
		{
			SetUpDefaultPSDRegistry(8);
			SetUpAutoSendStatementDateChangeRequestRegistry("ALL");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = "01";
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_SchDEntry = "8888";
			declaration.US_EntryDate = new ZDateTime(2011, 01, 31);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;

			AssertEquals("PSD is defaulted", new ZDateTime(2011, 2, 10), declaration.US_PreliminaryStatementPrintDate);
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			Factory.Save();

			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			declaration.DeriveDeclarationStatus();
			Factory.Save();

			AssertEquals("PSDAccepted should have been updated", new ZDateTime(2011, 2, 10), declaration.US_PSDAccepted);

			declaration.US_EntryDate = new ZDateTime(2011, 01, 18);
			Factory.Save();

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2011, 1, 18);
			Factory.Save();

			declaration.US_EnableCRL = true;
			AssertEquals("PSD is updated accordingly", new ZDateTime(2011, 1, 28), declaration.US_PreliminaryStatementPrintDate);

			declaration.DoMerge();
			var crEntry = declaration.CustomsEntryHeaders[1];
			crEntry.CH_Status = ImportMessageStatusList.Codes.AwaitingCargoReleaseOriginal;
			Factory.Save();
			AssertEquals("PSDAccepted shouldn't change as it should be updated only when ENS is accepted or StatementUpdate is accepted", new ZDateTime(2011, 2, 10), declaration.US_PSDAccepted);

			crEntry.CH_Status = ImportMessageStatusList.Codes.ClearCargoReleaseOriginal;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2011, 01, 18);
			Factory.Save();
			AssertEquals("PSDAccepted shouldn't change as it should be updated only when ENS is accepted or StatementUpdate is accepted", new ZDateTime(2011, 2, 10), declaration.US_PSDAccepted);

			var statementUpdateMessage = (MQEDIMessage)declaration.Messages.Find(new ZQuery(EDIMessageSchema.EM_MessageSubType, EM_MessageSubTypeList.Codes.StatementUpdateMessage))[0];
			var astuh = statementUpdateMessage.MessageBlock.MessageBlocks.OfType<ASTUH>().FirstOrDefault();
			AssertEquals("PSD sent in SU", new ZDate(2011, 1, 28), astuh.PreliminaryStatementPrintDate);
		}

		[TestDate(2011, 01, 17)]
		public void TestAutomaticSendingForLiveEntryWhenReleaseDateArrives()
		{
			SetUpDefaultPSDRegistry(8);
			SetUpAutoSendStatementDateChangeRequestRegistry("ALL");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_SchDEntry = "8888";
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2011, 2, 28);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.EntryNumber = "12345";
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save();
			AssertEquals("PreCondition", 0, declaration.Messages.Count);

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2011, 1, 18);
			declaration.DeriveDeclarationStatus();
			Factory.Save();
			AssertEquals("Automatic PSD should have been sent", 1, declaration.Messages.Count);
		}

		[TestDate(2011, 01, 17)]
		public void TestAutomaticSendingForNonLiveEntryWhenReleaseDateArrives()
		{
			SetUpDefaultPSDRegistry(8);
			SetUpAutoSendStatementDateChangeRequestRegistry("ALL");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_SchDEntry = "8888";
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.No;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2011, 1, 18);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.EntryNumber = "12345";
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save();
			AssertEquals("PreCondition", 0, declaration.Messages.Count);

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2011, 1, 18);
			Factory.Save();
			AssertEquals("Automatic PSD should have been sent", 1, declaration.Messages.Count);
		}

		[TestDate(2011, 05, 01)]
		public void TestMessageNotSentForPeriodicStatementMonthWhenNonPeriodicPaymentType()
		{
			SetUpDefaultPSDRegistry(9);
			SetUpAutoSendStatementDateChangeRequestRegistry("ALL");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_EntryDate = new ZDateTime(2011, 05, 01);
			declaration.US_PeriodicStatementMM = "05";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.EntryNumber = "01000048";
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			AssertEquals("Precondition: no messages for entry", 0, declaration.Messages.Count);

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2011, 05, 01, 20, 55, 0);
			Factory.Save();

			AssertEquals("Statement Update Message should not be sent, because non-periodic payment type", 0, declaration.Messages.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbBranch.CurrentBranch.SetCountry("US");
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
		}

		void SetUpDefaultPSDRegistry(ZInt numberOfDays)
		{
			var statementData = new DefaultStatementPrintDate();
			statementData.DoDefaultPrelimStatementPrintDate = true;
			statementData.NumberOfDays = numberOfDays;
			USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, statementData);
		}

		void SetUpAutoSendStatementDateChangeRequestRegistry(ZString overrideAllOrByOrganisation)
		{
			var autoGenerate = new AutoSendStatementDateChangeRequest();
			autoGenerate.OverrideAllOrByOrganisation = overrideAllOrByOrganisation;
			USCustomsDataRegistry.Instance.AutoSendSDCR.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, autoGenerate);
		}
	}
}
