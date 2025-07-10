using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class ReconDeclarationIReconciliationTest : TestCaseWithFactory
	{
		public void TestChangedLineDutyDescriptionIsNotPopulatedForMessaging()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration1.US_EnableENS = true;
			declaration1.US_BondProducerAccNo = "12";
			declaration1.Invoices.AddNew();
			var invoiceLine = declaration1.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "9102.11.1010";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_LinePrice = 3406.00m;
			invoiceLine.US_98GoodsValue = 1852.00m;

			declaration1.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var ensEntry1 = declaration1.CustomsEntryHeaders[0];
			var reconInnerDec = Factory.New<JobDeclaration>();
			var reconDeclaration = new ReconDeclaration(reconInnerDec);
			var reconOriginalEntry = reconDeclaration.OriginalEntries.Count > 0 ? reconDeclaration.OriginalEntries[0] : reconDeclaration.OriginalEntries.AddNew();
			reconOriginalEntry.CH_OrigEntryReference = "XJ5" + ensEntry1.EntryNumber;
			AssertEquals("CH_CH_OrigEntry is updated", ensEntry1.PK, reconOriginalEntry.CH_CH_OriginalEntry);

			var invoice = reconOriginalEntry.Invoice;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_InvoiceNumber = "HAOA780";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "KR";
			invoice.US_UC_NKCountryOfOrigin = "KR";

			var recinvoiceLine = invoice.JobComInvoiceLines.AddNew();
			recinvoiceLine.JI_Tariff = "3201.90.1000";
			recinvoiceLine.JI_CustomsQuantity = 50m;
			recinvoiceLine.JI_LinePrice = 3000m;
			recinvoiceLine.US_Duty = 125m;
			recinvoiceLine.US_R_OrigEntryLineNo = "1";
			recinvoiceLine.US_R_OrigTariff = recinvoiceLine.JI_Tariff;
			recinvoiceLine.US_R_OrigCV = 1000m;
			recinvoiceLine.US_R_OrigDuty = 17m;

			_ = new ReconDeclarationIReconciliation(reconDeclaration);
			AssertEquals(1, reconDeclaration.ChangedLines.Count);
			AssertEquals("", reconDeclaration.ChangedLines[0].US_OrigDutyRateDesc);
		}

		public void TestOfficeCodePreparerDistrictPort()
		{
			USCustomsDataRegistry.Instance.ARecordOfficeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12");
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "3901");

			AssertEquals("OfficeCode", "12", IRecon.OfficeCode);
			AssertEquals("PreparerDistrictPort", "3901", IRecon.PreparerDistrictPort);
		}

		public void TestMessageStatus()
		{
			IRecon.MessageStatus = ReconMessageStatusList.Codes.AwaitingReconDelete;
			AssertEquals(ReconMessageStatusList.Descriptions.AwaitingReconDelete, ReconDeclaration.MessageStatusDescription);
		}

		public void TestReconEntryNumber()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			ReconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("PreCondition:Entry Filer Code", "XJ5", ReconDeclaration.US_EntryFilerCode);
			ReconEntry.EntryNumber = "7347843";
			AssertEquals("XJ57347843", IRecon.EntryNumber);
		}

		public void TestUS_Paid()
		{
			ReconDeclaration.ReconWrappedJobDeclaration.US_Paid = "Y";
			AssertEquals("Y", IRecon.US_Paid);
		}

		public void TestPort()
		{
			ReconDeclaration.US_SchDEntry = "8888";
			AssertEquals("8888", IRecon.ProcessingDistrictPort);
		}

		public void TestImporterOfRecord()
		{
			AssertEquals("ImporterOfRecord number", "", IRecon.ImporterID);

			OrgHeader importer = Factory.New<OrgHeader>();
			ReconDeclaration.IOROrgPK = importer.PK;
			AssertEquals("ImporterOfRecord number", "", IRecon.ImporterID);

			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "897543987");
			AssertEquals("ImporterOfRecord number", "897543987", IRecon.ImporterID);
		}

		public void TestSuretyCode()
		{
			ReconDeclaration.US_SuretyCode = "891";
			AssertEquals("891", IRecon.SuretyCode);
		}

		public void TestUS_EstimatedEntryDate()
		{
			ReconDeclaration.US_EstimatedEntryDate = ZDateTime.BrettsBirthday;
			AssertEquals(ZDate.BrettsBirthday, IRecon.EstimatedReconciliationEntrySummaryDate);
		}

		public void TestIssueCode()
		{
			ReconDeclaration.US_IssueCode = ReconIssueCodeList.Codes._9802Recon;
			AssertEquals("IssueCode", ReconIssueCodeList.Codes._9802Recon, IRecon.IssueCode);
		}

		public void TestAggregateReconciliationIndicator()
		{
			ReconDeclaration.US_IsAggregate = true;
			AssertEquals(true, IRecon.AggregateReconciliationIndicator);

			ReconDeclaration.US_IsAggregate = false;
			AssertEquals(false, IRecon.AggregateReconciliationIndicator);
		}

		public void TestIncreaseRefundIndicator()
		{
			var originalEntry = ReconDeclaration.OriginalEntries.AddNew();
			originalEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 10m);
			originalEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 20m);
			AssertEquals("Mixed", "3", IRecon.IncreaseRefundIndicator);

			originalEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Wines, 30m);
			AssertEquals("Refund", "2", IRecon.IncreaseRefundIndicator);

			ReconDeclaration.US_R_Waive = true;
			AssertEquals("No change", "1", IRecon.IncreaseRefundIndicator);
			ReconDeclaration.US_R_Waive = false;

			originalEntry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 10m);
			originalEntry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 20m);
			originalEntry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Wines, 30m);
			AssertEquals("No change", "1", IRecon.IncreaseRefundIndicator);
		}

		public void TestEarliestImportDate()
		{
			ReconDeclaration.US_IssueCode = ReconIssueCodeList.Codes.FTA;

			ReconOriginalEntryHeader originalEntry1 = ReconDeclaration.OriginalEntries.AddNew();
			originalEntry1.US_ImportDate = new ZDateTime(2008, 1, 1);
			AssertEquals(new ZDateTime(2008, 1, 1), IRecon.EarliestImportDate);

			ReconOriginalEntryHeader originalEntry2 = ReconDeclaration.OriginalEntries.AddNew();
			originalEntry2.US_ImportDate = new ZDateTime(2008, 1, 10);
			AssertEquals(new ZDateTime(2008, 1, 1), IRecon.EarliestImportDate);

			ReconOriginalEntryHeader originalEntry3 = ReconDeclaration.OriginalEntries.AddNew();
			originalEntry3.US_ImportDate = new ZDateTime(2007, 12, 10);
			AssertEquals(new ZDateTime(2007, 12, 10), IRecon.EarliestImportDate);

			ReconDeclaration.US_IssueCode = ReconIssueCodeList.Codes.Value9802Recon;
			AssertEquals(ZDate.Empty, IRecon.EarliestImportDate);
		}

		public void TestEarliestEntrySummaryDate()
		{
			ReconOriginalEntryHeader originalEntry1 = ReconDeclaration.OriginalEntries.AddNew();
			originalEntry1.US_R_ReleaseDate = new ZDateTime(2008, 1, 1);
			originalEntry1.US_PaymentDate = new ZDateTime(2008, 1, 2);
			AssertEquals(new ZDateTime(2008, 1, 2), IRecon.EarliestEntrySummaryDate);

			ReconOriginalEntryHeader originalEntry2 = ReconDeclaration.OriginalEntries.AddNew();
			originalEntry2.US_R_ReleaseDate = new ZDateTime(2008, 1, 10);
			originalEntry2.US_PaymentDate = new ZDateTime(2008, 1, 11);
			AssertEquals(new ZDateTime(2008, 1, 2), IRecon.EarliestEntrySummaryDate);

			ReconOriginalEntryHeader originalEntry3 = ReconDeclaration.OriginalEntries.AddNew();
			originalEntry3.US_R_ReleaseDate = new ZDateTime(2007, 12, 10);
			originalEntry2.US_PaymentDate = new ZDateTime(2007, 12, 9);
			AssertEquals(new ZDateTime(2007, 12, 9), IRecon.EarliestEntrySummaryDate);

			ReconDeclaration.US_IssueCode = ReconIssueCodeList.Codes.FTA;
			AssertEquals(ZDateTime.Empty, IRecon.EarliestEntrySummaryDate);
		}

		public void TestAgentBrokerReferenceID()
		{
			AssertEquals("optional field, blank for now until we know what goes here", "", IRecon.AgentBrokerReferenceID);
		}

		public void TestBrokerReferenceNumber()
		{
			Declaration.JE_DeclarationReference = "1234567890";
			AssertEquals("234567890", IRecon.BrokerReferenceNumber);
		}

		public void TestTextComment()
		{
			ReconDeclaration.US_Comment = " Blah blah blah ";
			AssertEquals("Blah blah blah", IRecon.TextComment);
		}

		public void TestPaymentTypeIndicator()
		{
			ReconDeclaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			AssertEquals(PaymentTypeList.Codes.IndividualBasis, IRecon.PaymentTypeIndicator);
		}

		public void TestPreliminaryStatementPrintDate()
		{
			ReconDeclaration.US_PreliminaryStatementPrintDate = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.BrettsBirthday, IRecon.PreliminaryStatementPrintDate);
		}

		public void TestClientBranchDesignation()
		{
			ReconDeclaration.US_ClientBranchDesignation = "45";
			AssertEquals("45", IRecon.ClientBranchDesignation);
		}

		public void TestDutyTaxFeeAndInterest()
		{
			DeclarationTestHelper.SetReconInterestInRegistry(new ZDate(1999, 1, 1), new ZDate(1999, 03, 31), 7m);//7%
			DeclarationTestHelper.SetReconInterestInRegistry(new ZDate(1999, 4, 1), new ZDate(1999, 09, 30), 8m);//8%

			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_PreliminaryStatementPrintDate = new ZDate(1999, 9, 15);

			ReconOriginalEntryHeader entry1 = reconDeclaration.OriginalEntries.AddNew();
			entry1.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4300m);
			entry1.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4600m);
			entry1.US_PaymentDate = new ZDate(1999, 1, 5);

			ReconOriginalEntryHeader entry2 = reconDeclaration.OriginalEntries.AddNew();
			entry2.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Raspberry, 4000m);
			entry2.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Raspberry, 4670m);
			entry2.US_PaymentDate = new ZDate(1999, 4, 12);

			ReconOriginalEntryHeader entry3 = reconDeclaration.OriginalEntries.AddNew();
			entry3.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Wines, 4000m);
			entry3.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Wines, 4600m);
			entry3.US_PaymentDate = new ZDate(1999, 5, 28);

			reconDeclaration.US_IsAggregate = false;
			new ReconInterestCalculator(new ReconInterestDataProviderReconDec(reconDeclaration)).Execute();

			ReconDeclarationIReconciliation ireconData = new ReconDeclarationIReconciliation(reconDeclaration);
			AssertEquals("DutyPaymentAmount", 300m, ireconData.DutyPaymentAmount);
			AssertEquals("FeePaymentAmount", 670m, ireconData.FeePaymentAmount);
			AssertEquals("TaxPaymentAmount", 600m, ireconData.TaxPaymentAmount);

			reconDeclaration.US_IsAggregate = true;
			reconDeclaration.US_R_IsNoChangeAgg = true;
			new ReconInterestCalculator(new ReconInterestDataProviderReconDec(reconDeclaration)).Execute();

			AssertEquals("entry1 Interest", 0m, entry1.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest));
			AssertEquals("entry2 Interest", 0m, entry2.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest));
			AssertEquals("entry3 Interest", 0m, entry3.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest));

			AssertEquals("DutyPaymentAmount should be zero for aggregated recon - no entry level duty and fees calculation.", 0m, ireconData.DutyPaymentAmount);
			AssertEquals("FeePaymentAmount should be zero for aggregated recon - no entry level duty and fees calculation.", 0m, ireconData.FeePaymentAmount);
			AssertEquals("TaxPaymentAmount should be zero for aggregated recon - no entry level duty and fees calculation.", 0m, ireconData.TaxPaymentAmount);
		}

		public void TestDutyFeeTaxPaymentAmountWhenNegative()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			ReconOriginalEntryHeader entry1 = reconDeclaration.OriginalEntries.AddNew();
			entry1.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 3300m);
			entry1.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 3000m);

			ReconOriginalEntryHeader entry2 = reconDeclaration.OriginalEntries.AddNew();
			entry2.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Raspberry, 3670m);
			entry2.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Raspberry, 3000m);

			ReconOriginalEntryHeader entry3 = reconDeclaration.OriginalEntries.AddNew();
			entry3.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Wines, 3600m);
			entry3.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Wines, 3000m);

			ReconDeclarationIReconciliation ireconData = new ReconDeclarationIReconciliation(reconDeclaration);
			AssertEquals("DutyPaymentAmount should be zero when the difference results in negative", 0m, ireconData.DutyPaymentAmount);
			AssertEquals("TaxPaymentAmount should be zero when the difference results in negative", 0m, ireconData.TaxPaymentAmount);
			AssertEquals("FeePaymentAmount should be zero when the difference results in negative", 0m, ireconData.FeePaymentAmount);
		}

		public void TestAddMessage()
		{
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			IRecon.AddMessage(message);

			AssertEquals(true, ReconEntry.Messages.Contains(message));
			AssertEquals(ReconEntry.PK, message.EM_LinkUniqueID);
			AssertEquals(CusEntryHeaderSchema.Constants.TableName, message.EM_LinkTable);
		}

		public void TestImportEntries()
		{
			List<IReconciliationImportEntry> list = new List<IReconciliationImportEntry>(IRecon.ImportEntries);
			AssertEquals(0, list.Count);

			ReconDeclaration.OriginalEntries.AddNew();
			list = new List<IReconciliationImportEntry>(IRecon.ImportEntries);
			AssertEquals(1, list.Count);
		}

		public void TestLogCustomsCommencedIfNeeded()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var iReconData = new ReconDeclarationIReconciliation(reconDeclaration);
			((IReconciliation)iReconData).LogCustomsCommencedIfNeeded();

			var query = new ZQuery(StmALogSchema.SL_Parent, reconDeclaration.ReconWrappedJobDeclaration.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, "CCC");

			var customsCommencedLogs = reconDeclaration.ReconWrappedJobDeclaration.Logs.Find(query);
			AssertEquals("One Customs Commenced Log exists", 1, customsCommencedLogs.Length);
			AssertEquals("Log event reference", "Reconciliation", customsCommencedLogs[0].SL_Reference);
			AssertEquals("Log event description", "Customs Commenced", customsCommencedLogs[0].SL_EventDescription);
		}

		public void TestEntryLineGroups()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.US_IssueCode = ReconIssueCodeList.Codes._9802Recon;
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ52";
			originalEntry.US_ImportDate = new ZDateTime(2007, 12, 31);
			originalEntry.US_R_DutyRateDate = new ZDateTime(2008, 1, 5);
			originalEntry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			originalEntry.US_SchDEntry = "1234";
			originalEntry.US_R_CalcOrigDuty = true;

			var invoice = originalEntry.Invoice;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_InvoiceNumber = "HAOA780";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "KR";
			invoice.US_UC_NKCountryOfOrigin = "KR";

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "9101.90.1000";
			invoiceLine1.JI_CustomsQuantity = 50m;
			invoiceLine1.US_Duty = 40m;
			invoiceLine1.US_R_OrigDuty = 20m;
			invoiceLine1.US_SPI = "P1";
			invoiceLine1.US_R_OrigSPI = "S1";

			invoiceLine1.JI_LinePrice = 3000m;
			invoiceLine1.US_SupTariff = "9813.00.8015";
			invoiceLine1.US_SupDuty = 40m;
			invoiceLine1.US_R_OrigSupDuty = 20m;
			invoiceLine1.US_R_OrigEntryLineNo = "1";

			SetReconOriginalValues(invoiceLine1);

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "9201.90.1000";
			invoiceLine2.JI_CustomsQuantity = 50m;
			invoiceLine2.US_Duty = 40m;
			invoiceLine2.US_R_OrigDuty = 20m;
			invoiceLine2.US_SPI = "P1";
			invoiceLine2.US_R_OrigSPI = "S1";

			invoiceLine2.JI_LinePrice = 3000m;
			invoiceLine2.US_SupTariff = "9813.00.8015";
			invoiceLine2.US_SupDuty = 40m;
			invoiceLine2.US_R_OrigSupDuty = 20m;
			invoiceLine2.US_R_OrigEntryLineNo = "1";
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			SetReconOriginalValues(invoiceLine2);

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "9301.90.1000";
			invoiceLine3.JI_CustomsQuantity = 50m;
			invoiceLine3.US_Duty = 40m;
			invoiceLine3.US_R_OrigDuty = 20m;
			invoiceLine3.US_SPI = "P1";
			invoiceLine3.US_R_OrigSPI = "S1";
			invoiceLine3.JI_LinePrice = 3000m;
			invoiceLine3.US_R_OrigEntryLineNo = "1";
			invoiceLine3.JI_ParentID = invoiceLine1.PK;
			SetReconOriginalValues(invoiceLine3);

			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "9101.90.1000";
			invoiceLine4.JI_CustomsQuantity = 50m;
			invoiceLine4.US_Duty = 40m;
			invoiceLine4.US_R_OrigDuty = 20m;
			invoiceLine4.US_SPI = "P2";
			invoiceLine4.US_R_OrigSPI = "S1";

			invoiceLine4.JI_LinePrice = 3000m;
			invoiceLine4.US_SupTariff = "9813.00.8015";
			invoiceLine4.US_SupDuty = 40m;
			invoiceLine4.US_R_OrigSupDuty = 20m;
			invoiceLine4.US_R_OrigEntryLineNo = "2";

			SetReconOriginalValues(invoiceLine4);

			var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "9201.90.1000";
			invoiceLine5.JI_CustomsQuantity = 50m;
			invoiceLine5.US_Duty = 40m;
			invoiceLine5.US_R_OrigDuty = 20m;
			invoiceLine5.US_SPI = "P2";
			invoiceLine5.US_R_OrigSPI = "S1";

			invoiceLine5.JI_LinePrice = 3000m;
			invoiceLine5.US_SupTariff = "9813.00.8015";
			invoiceLine5.US_SupDuty = 40m;
			invoiceLine5.US_R_OrigSupDuty = 20m;
			invoiceLine5.US_R_OrigEntryLineNo = "2";
			invoiceLine5.JI_ParentID = invoiceLine4.PK;
			SetReconOriginalValues(invoiceLine5);

			var invoiceLine6 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_Tariff = "9301.90.1000";
			invoiceLine6.JI_CustomsQuantity = 50m;
			invoiceLine6.US_Duty = 40m;
			invoiceLine6.US_R_OrigDuty = 20m;
			invoiceLine6.US_SPI = "P2";
			invoiceLine6.US_R_OrigSPI = "S1";
			invoiceLine6.JI_LinePrice = 3000m;
			invoiceLine6.US_R_OrigEntryLineNo = "2";
			invoiceLine6.JI_ParentID = invoiceLine4.PK;
			SetReconOriginalValues(invoiceLine6);

			var reconciliation = new ReconDeclarationIReconciliation(reconDec);
			var entryLineGroups = reconciliation.EntryLineGroups;

			AssertEquals(2, entryLineGroups.Count());
			var entryLineGroup1 = entryLineGroups.ToArray()[0];
			var entryLineGroup2 = entryLineGroups.ToArray()[1];

			AssertEquals("9813008015", entryLineGroup1.OriginalHTS);
			AssertEquals("9813008015", entryLineGroup1.ReconHTS);

			AssertEquals("9813008015", entryLineGroup2.OriginalHTS);
			AssertEquals("9813008015", entryLineGroup2.ReconHTS);

			AssertEquals(5, entryLineGroup1.SecondaryLines.Count());
			AssertEquals(5, entryLineGroup2.SecondaryLines.Count());
			var secondaryLines1 = entryLineGroup1.SecondaryLines.ToArray();
			AssertEquals("9101901000", secondaryLines1[0].OriginalHTS);
			AssertEquals("9813008015", secondaryLines1[1].OriginalHTS);
			AssertEquals("9201901000", secondaryLines1[2].OriginalHTS);
			AssertEquals("9813008015", secondaryLines1[3].OriginalHTS);
			AssertEquals("9301901000", secondaryLines1[4].OriginalHTS);

			var secondaryLines2 = entryLineGroup1.SecondaryLines.ToArray();
			AssertEquals("9101901000", secondaryLines2[0].OriginalHTS);
			AssertEquals("9813008015", secondaryLines2[1].OriginalHTS);
			AssertEquals("9201901000", secondaryLines2[2].OriginalHTS);
			AssertEquals("9813008015", secondaryLines2[3].OriginalHTS);
			AssertEquals("9301901000", secondaryLines2[4].OriginalHTS);

			AssertEquals(1, entryLineGroup1.OriginalEntryLines.Count());
			AssertEquals(1, entryLineGroup2.OriginalEntryLines.Count());
		}

		public void TestQualifyingGoodFreeTradeDec()
		{
			var reconDec = ReconDeclaration;
			var iRecon = IRecon;
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.FTA;
			Assert("QualifyingGoodFreeTradeDec is ture", iRecon.QualifyingGoodFreeTradeDec);

			reconDec.US_IssueCode = ReconIssueCodeList.Codes.Value9802Recon;
			Assert("QualifyingGoodFreeTradeDec is ture", !iRecon.QualifyingGoodFreeTradeDec);
		}

		void SetReconOriginalValues(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_R_OrigTariff = invoiceLine.JI_Tariff;
			invoiceLine.US_R_OrigCV = invoiceLine.JI_CustomsValue >= invoiceLine.US_98GoodsValue ? (ZDecimal)(invoiceLine.JI_CustomsValue - invoiceLine.US_98GoodsValue) : invoiceLine.JI_CustomsValue;
			invoiceLine.US_R_OrigFirstQty = invoiceLine.JI_CustomsQuantity;
			invoiceLine.US_R_OrigSecondQty = invoiceLine.JI_CustomsSecondQuantity;

			invoiceLine.US_R_OrigSupTariff = invoiceLine.US_SupTariff;
			invoiceLine.US_R_Orig98Value = invoiceLine.US_98GoodsValue;
			invoiceLine.US_R_OrigSupQty1 = invoiceLine.US_SupQty1;
			invoiceLine.US_R_OrigSupQty2 = invoiceLine.US_SupQty2;
			invoiceLine.US_R_OrigSupQty3 = invoiceLine.US_SupQty3;
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}

		ReconDeclarationIReconciliation iRecon;
		ReconDeclarationIReconciliation IRecon => iRecon ?? (iRecon = new ReconDeclarationIReconciliation(ReconDeclaration));

		ReconDeclaration reconDeclaration;
		ReconDeclaration ReconDeclaration => reconDeclaration ?? (reconDeclaration = new ReconDeclaration(Declaration));

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());

		CusEntryHeader reconEntry;
		CusEntryHeader ReconEntry
		{
			get
			{
				if (reconEntry == null)
				{
					ReconDeclaration reconDec = ReconDeclaration;
					reconDec.US_EntryFilerCode = "XJ5";
					foreach (CusEntryHeader entry in Declaration.CustomsEntryHeaders)
					{
						if (entry.CH_MessageType == CusEntryHeaderMessageTypeList.Codes.ReconEntry)
						{
							reconEntry = entry;
							break;
						}
					}
				}
				return reconEntry;
			}
		}
	}
}
