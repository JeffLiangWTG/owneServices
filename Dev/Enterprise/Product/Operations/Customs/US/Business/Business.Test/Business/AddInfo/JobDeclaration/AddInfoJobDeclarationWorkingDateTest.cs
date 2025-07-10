using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AddInfoJobDeclarationWorkingDateTest : TestCaseWithFactory
	{
		public void TestGeneratePrelimStmtDate()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_EntryAuthorisationDate = new ZDate(2008, 05, 01);
			var statementData = new DefaultStatementPrintDate();
			statementData.DoDefaultPrelimStatementPrintDate = true;
			statementData.NumberOfDays = 8;
			USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, statementData);
			AssertEquals(new ZDateTime(2008, 05, 13), TestWorkingDate.GeneratePrelimStmtDate(declaration));
			statementData.NumberOfDays = 5;
			USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, statementData);
			declaration.JE_EntryAuthorisationDate = new ZDate(2008, 06, 23);
			AssertEquals(new ZDateTime(2008, 06, 30), TestWorkingDate.GeneratePrelimStmtDate(declaration));
			declaration.JE_EntryAuthorisationDate = new ZDate(2009, 07, 02);
			AssertEquals("Weekend & 1 public holiday", new ZDateTime(2009, 07, 10), TestWorkingDate.GeneratePrelimStmtDate(declaration));
			statementData.NumberOfDays = 10;
			USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, statementData);
			declaration.JE_EntryAuthorisationDate = new ZDate(2008, 06, 01);
			AssertEquals(new ZDateTime(2008, 06, 13), TestWorkingDate.GeneratePrelimStmtDate(declaration));
		}

		[TestDate(2011, 08, 08)]
		public void TestGeneratePSDForLiveEntry()
		{
			var statementData = new DefaultStatementPrintDate();
			statementData.DoDefaultPrelimStatementPrintDate = true;
			statementData.NumberOfDays = 1;
			USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, statementData);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			declaration.US_EstimatedEntryDate = new ZDateTime(2011, 8, 8);
			AssertEquals("PSD calculated from Estimated EntryDate", new ZDateTime(2011, 8, 9), declaration.US_PreliminaryStatementPrintDate);
			AssertEquals("PMS", "09", declaration.US_PeriodicStatementMM);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2011, 9, 9);
			declaration.US_PresentationDate = new ZDateTime(2011, 9, 5);
			AssertEquals("PSD calculated from Estimated EntryDate", new ZDateTime(2011, 8, 9), declaration.US_PreliminaryStatementPrintDate);
			AssertEquals("PMS", "09", declaration.US_PeriodicStatementMM);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2011, 8, 9);
			declaration.US_PresentationDate = new ZDateTime(2011, 9, 6);
			AssertEquals("PSD calculated from Estimated EntryDate", new ZDateTime(2011, 8, 9), declaration.US_PreliminaryStatementPrintDate);
			AssertEquals("PMS", "09", declaration.US_PeriodicStatementMM);
		}

		[TestDate(2011, 03, 15)]
		public void TestGeneratePrelimStmtDateForReconciliation()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			reconDeclaration.US_EstimatedEntryDate = new ZDateTime(2011, 03, 19); // Saturday
			AssertEquals("Should be Monday", new ZDateTime(2011, 03, 21), TestWorkingDate.GeneratePrelimStmtDate(reconDeclaration));
			reconDeclaration.US_EstimatedEntryDate = new ZDateTime(2011, 03, 17);
			AssertEquals("Should be the same day, because Estimated Entry Date is in the future", new ZDateTime(2011, 03, 17), TestWorkingDate.GeneratePrelimStmtDate(reconDeclaration));
		}

		public void TestGeneratePrelimStmtDateOrgOverride()
		{
			var iOR1 = Factory.NewWithValidTestData<OrgHeader>();
			var addInfo = new OrgImpAddInfo((ZPropertyInfoString)iOR1.CountryData.OV_ImportCustomsDefaultAddInfoInfo);
			addInfo.ZO_SPDNumberOfDays = 5;
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_EntryAuthorisationDate = new ZDate(2008, 05, 01);
			declaration.IOROrgPK = iOR1.PK;
			var statementData = new DefaultStatementPrintDate();
			statementData.DoDefaultPrelimStatementPrintDate = true;
			statementData.NumberOfDays = 8;
			USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, statementData);
			AssertEquals("WorkingDate should generate based on Organisation number of days", new ZDateTime(2008, 05, 08), TestWorkingDate.GeneratePrelimStmtDate(declaration));
			declaration.JE_EntryAuthorisationDate = new ZDate(2009, 07, 02);
			AssertEquals("Weekend & 1 public holiday", new ZDateTime(2009, 07, 10), TestWorkingDate.GeneratePrelimStmtDate(declaration));
			var iOR2 = Factory.NewWithValidTestData<OrgHeader>();
			var addInfo2 = new OrgImpAddInfo((ZPropertyInfoString)iOR2.CountryData.OV_ImportCustomsDefaultAddInfoInfo);
			addInfo2.ZO_SPDNumberOfDays = 10;
			Factory.Save();
			declaration.IOROrgPK = iOR2.PK;
			declaration.JE_EntryAuthorisationDate = new ZDate(2008, 06, 01);
			AssertEquals(new ZDateTime(2008, 06, 13), TestWorkingDate.GeneratePrelimStmtDate(declaration));
		}

		public void TestGeneratePrelimStmtDateOrgOverrideWithDefaultHardCodedHolidays()
		{
			var iOR1 = Factory.NewWithValidTestData<OrgHeader>();
			var addInfo = new OrgImpAddInfo((ZPropertyInfoString)iOR1.CountryData.OV_ImportCustomsDefaultAddInfoInfo);
			addInfo.ZO_SPDNumberOfDays = 5;
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.IOROrgPK = iOR1.PK;
			declaration.JE_EntryAuthorisationDate = new ZDate(2008, 05, 01);
			var statementData = new DefaultStatementPrintDate();
			statementData.DoDefaultPrelimStatementPrintDate = true;
			statementData.NumberOfDays = 8;
			USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, statementData);
			AssertEquals("WorkingDate should generate based on Organisation number of days", new ZDateTime(2008, 05, 08), TestWorkingDate.GeneratePrelimStmtDate(declaration));
			declaration.JE_EntryAuthorisationDate = new ZDate(2008, 06, 02);
			AssertEquals("Weekend & 1 public holiday", new ZDateTime(2008, 06, 9), TestWorkingDate.GeneratePrelimStmtDate(declaration));
			var iOR2 = Factory.NewWithValidTestData<OrgHeader>();
			var addInfo2 = new OrgImpAddInfo((ZPropertyInfoString)iOR2.CountryData.OV_ImportCustomsDefaultAddInfoInfo);
			addInfo2.ZO_SPDNumberOfDays = 10;
			Factory.Save();
			declaration.IOROrgPK = iOR2.PK;
			declaration.JE_EntryAuthorisationDate = new ZDate(2008, 06, 01);
			AssertEquals(new ZDateTime(2008, 06, 13), TestWorkingDate.GeneratePrelimStmtDate(declaration));
		}

		public void TestGeneratePrelimStmtDateOrgOverrideWithDefaultHardCodedHolidays1()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_EntryAuthorisationDate = new ZDate(2009, 5, 21);
			var statementData = new DefaultStatementPrintDate();
			statementData.DoDefaultPrelimStatementPrintDate = true;
			statementData.NumberOfDays = 8;
			USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, statementData);
			AssertEquals("2 Weekends & 1 public holiday", new ZDateTime(2009, 6, 3), TestWorkingDate.GeneratePrelimStmtDate(declaration));
		}

		public void TestNoCountryDataOnOrgOverride()
		{
			var iOR = Factory.New<OrgHeader>();
			iOR.OH_Code = "IMP1";
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.IOROrgPK = iOR.PK;
			declaration.JE_EntryAuthorisationDate = new ZDate(2008, 05, 01);
			var statementData = new DefaultStatementPrintDate();
			statementData.DoDefaultPrelimStatementPrintDate = true;
			statementData.NumberOfDays = 8;
			USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, statementData);
			AssertEquals("No country data should not crash", new ZDateTime(2008, 05, 13), TestWorkingDate.GeneratePrelimStmtDate(declaration));
		}

		public void TestGenerateDeferredTaxDueDateForConsumptionAndQuota()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTaxWithEFT;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			declaration.US_PaymentDueDate = new ZDateTime(2013, 12, 05);
			declaration.US_DeferredTaxDueDate = TestWorkingDate.GenerateDeferredTaxDueDate(declaration);
			AssertEquals("Tax Due Date based on Duty Due Date, because collection date is empty", new ZDateTime(2013, 12, 27), declaration.US_DeferredTaxDueDate);
			entry.US_CollectionDate = new ZDateTime(2013, 12, 19);
			AssertEquals("Tax Due Date based on collection date", new ZDateTime(2014, 01, 14), declaration.US_DeferredTaxDueDate);
			entry.US_CollectionDate = new ZDateTime(2013, 08, 01);
			AssertEquals("Tax Due Date based on collection date, recalculate if collection date changed", new ZDateTime(2013, 08, 29), declaration.US_DeferredTaxDueDate);
			var ior = Factory.NewWithValidTestData<OrgHeader>();
			declaration.IOROrgPK = ior.PK;
			declaration.IORWrapper.ZO_DefTaxDateCalcOption = DefTaxDueDateCalculationOptionList.Codes.REL;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 12, 06);
			AssertEquals("Tax Due Date based on Release Date, because importer has indicator overriden, ignore Registry", new ZDateTime(2013, 12, 27), declaration.US_DeferredTaxDueDate);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			DataRegistry.Business.USCustomsDataRegistry.Instance.DefTaxDueDateCalculationOption.SetValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, DefTaxDueDateCalculationOptionList.Codes.REL);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 01, 07);
			AssertEquals("Tax Due Date based on Release Date, because Registry overriden", new ZDateTime(2014, 01, 29), declaration.US_DeferredTaxDueDate);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 01, 15);
			AssertEquals("Tax Due Date based on Release Date, because Registry overriden", new ZDateTime(2014, 01, 29), declaration.US_DeferredTaxDueDate);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 02, 28);
			AssertEquals("Tax Due Date based on Release Date, because Registry overriden", new ZDateTime(2014, 03, 14), declaration.US_DeferredTaxDueDate);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 05, 03);
			AssertEquals(@"Tax Due Date based on Release Date, estimated Due Date falls on Customs Public Holiday 29/05/2017,
			so actual date should be 26th of May 2017, Friday", new ZDateTime(2017, 05, 26), declaration.US_DeferredTaxDueDate);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 02, 05);
			AssertEquals(@"Tax Due Date based on Release Date", new ZDateTime(2013, 02, 28), declaration.US_DeferredTaxDueDate);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2012, 02, 05);
			AssertEquals(@"Tax Due Date based on Release Date", new ZDateTime(2012, 02, 29), declaration.US_DeferredTaxDueDate);
			declaration.US_FixDefTaxDueDate = true;
			declaration.US_DeferredTaxDueDate = new ZDateTime(2012, 03, 14);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2012, 02, 04);
			AssertEquals(@"Tax Due Date should not be recalculated, because this is user input and date is locked", new ZDateTime(2012, 03, 14), declaration.US_DeferredTaxDueDate);
			declaration.US_FixDefTaxDueDate = false;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2012, 02, 06);
			AssertEquals(@"User input should be overriden. Date recalculated, because Deferred Tax Due Date was not locked", new ZDateTime(2012, 02, 29), declaration.US_DeferredTaxDueDate);
			declaration.IORWrapper.ZO_DefTaxDateCalcOption = DefTaxDueDateCalculationOptionList.Codes.COL;
			entry.US_CollectionDate = new ZDateTime(2012, 08, 03);
			AssertEquals("Tax Due Date based on collection date even if registry is set to use Release date, because Importer settings is collection date.", new ZDateTime(2012, 08, 29), declaration.US_DeferredTaxDueDate);
		}

		public void TestGenerateDeferredTaxDueDateForWarehouse()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTaxWithEFT;
			declaration.US_EstimatedEntryDate = new ZDateTime(2013, 12, 05);
			declaration.US_DeferredTaxDueDate = TestWorkingDate.GenerateDeferredTaxDueDate(declaration);
			AssertEquals(@"Tax Due Date based on Estimated Entry Date for Warehouse entry types. 
			Date falls on first semimonthly period, so Tax Due Date should be same month", new ZDateTime(2013, 12, 27), declaration.US_DeferredTaxDueDate);
			declaration.US_EstimatedEntryDate = new ZDateTime(2013, 04, 25);
			declaration.US_DeferredTaxDueDate = TestWorkingDate.GenerateDeferredTaxDueDate(declaration);
			AssertEquals(@"Tax Due Date based on Estimated Entry Date for Warehouse entry types.
			Date falls on second semimonthly period, so Tax Due Date is next month", new ZDateTime(2013, 05, 14), declaration.US_DeferredTaxDueDate);
			declaration.US_EstimatedEntryDate = new ZDateTime(2013, 09, 21);
			declaration.US_DeferredTaxDueDate = TestWorkingDate.GenerateDeferredTaxDueDate(declaration);
			AssertEquals(@"Date to calculate on falls to first accelerated payment period (exception) for EFT taxpayers.
			Due Date is 29th, Sunday, so date should be Friday 27th", new ZDateTime(2013, 09, 27), declaration.US_DeferredTaxDueDate);
		}

		public void TestGenerateDeferredTaxDueDateExceptions()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTaxWithEFT;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			declaration.US_PaymentDueDate = new ZDateTime(2013, 09, 06);
			declaration.US_DeferredTaxDueDate = TestWorkingDate.GenerateDeferredTaxDueDate(declaration);
			AssertEquals(@"Tax Due Date based on Duty Due Date, because collection date is empty. 
			This is September, but not an exception. 29th is Sunday, so date should be Friday 27th", new ZDateTime(2013, 09, 27), declaration.US_DeferredTaxDueDate);
			entry.US_CollectionDate = new ZDateTime(2013, 09, 18);
			AssertEquals(@"Date to calculate on falls to first accelerated payment period (exception) for EFT taxpayers.
			Due Date is 29th, Sunday, so date should be Friday 27th", new ZDateTime(2013, 09, 27), declaration.US_DeferredTaxDueDate);
			entry.US_CollectionDate = new ZDateTime(2014, 09, 19);
			AssertEquals(@"Date to calculate on falls to first accelerated payment period (exception) for EFT taxpayers.
			Due Date is 29th, Monday", new ZDateTime(2014, 09, 29), declaration.US_DeferredTaxDueDate);
			entry.US_CollectionDate = new ZDateTime(2014, 09, 27);
			AssertEquals(@"Date to calculate on falls to second accelerated payment period (exception) for EFT taxpayers. Due Date is 14th of October", new ZDateTime(2014, 10, 14), declaration.US_DeferredTaxDueDate);
			declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTax;
			entry.US_CollectionDate = ZDateTime.Empty;
			declaration.US_PaymentDueDate = new ZDateTime(2013, 09, 16);
			declaration.US_DeferredTaxDueDate = TestWorkingDate.GenerateDeferredTaxDueDate(declaration);
			AssertEquals(@"Date to calculate on falls to first accelerated payment period (exception) for non-EFT taxpayers.
			Due Date is 28th of September, Saturday. So final date should be 27th, Friday", new ZDateTime(2013, 09, 27), declaration.US_DeferredTaxDueDate);
			declaration.US_PaymentDueDate = new ZDateTime(2014, 09, 16);
			declaration.US_DeferredTaxDueDate = TestWorkingDate.GenerateDeferredTaxDueDate(declaration);
			AssertEquals(@"Date to calculate on falls to first accelerated payment period (exception) for non-EFT taxpayers.
			Due Date is 28th of September 2014, Sunday. So final date should be 26th, Friday", new ZDateTime(2014, 09, 26), declaration.US_DeferredTaxDueDate);
			declaration.US_PaymentDueDate = new ZDateTime(2014, 09, 26);
			declaration.US_DeferredTaxDueDate = TestWorkingDate.GenerateDeferredTaxDueDate(declaration);
			AssertEquals(@"Date to calculate on falls to second accelerated payment period (exception) for non-EFT taxpayers.
			Due Date is 14th of October 2014, Tuesday", new ZDateTime(2014, 10, 14), declaration.US_DeferredTaxDueDate);
		}

		public void TestGet11thWorkingDayOfMonth()
		{
			var date1 = ZDateTime.Empty;
			var testDate1 = TestWorkingDate.Get11thWorkingDayOfMonth(date1.Date);
			AssertEquals("ZDateTime.Empty", ZDateTime.Empty, testDate1);
			var date2 = ZDateTime.Invalid;
			var testDate2 = TestWorkingDate.Get11thWorkingDayOfMonth(date2.Date);
			AssertEquals("ZDateTime.Empty", ZDateTime.Empty, testDate2);
			var date3 = new ZDateTime(2020, 09, 02);
			var testDate3 = TestWorkingDate.Get11thWorkingDayOfMonth(date3.Date);
			AssertEquals("ZDateTime.Empty", new ZDateTime(2020, 09, 16), testDate3);
		}

		AddInfoJobDeclarationWorkingDate testWorkingDate;
		AddInfoJobDeclarationWorkingDate TestWorkingDate => testWorkingDate ?? (testWorkingDate = new AddInfoJobDeclarationWorkingDate());
	}
}
