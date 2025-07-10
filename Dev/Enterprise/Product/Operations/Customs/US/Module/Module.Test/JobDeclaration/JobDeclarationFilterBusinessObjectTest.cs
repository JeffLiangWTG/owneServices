using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(JobDeclarationFilterBusinessObject))]
	sealed class JobDeclarationFilterBusinessObjectTest : Customs.Module.Testing.JobDeclarationFilterBusinessObjectTest
	{
		public void TestSuretyCodeQueryWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_SuretyCode = "AB";
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_SuretyCode = "ABC";
			DeleteAllGenAddOnColumn(declaration2);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			DeleteAllGenAddOnColumn(declaration3);

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Recon;
			declaration4.US_SuretyCode = "ABE";
			DeleteAllGenAddOnColumn(declaration4);

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.SuretyCode];
			filter.IsActive = true;

			filter.Property = "AB";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			Assert(declaration1.MatchesFilter(bizObj.Filter));
			Assert(declaration2.MatchesFilter(bizObj.Filter));
			Assert(declaration4.MatchesFilter(bizObj.Filter));

			filter.Property = ZString.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			Assert(!declaration1.MatchesFilter(bizObj.Filter));
			Assert(!declaration2.MatchesFilter(bizObj.Filter));
			Assert(declaration3.MatchesFilter(bizObj.Filter));
			Assert(!declaration4.MatchesFilter(bizObj.Filter));
		}

		public void TestDeferredTaxDueDateFilterWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_DeferredTaxDueDate = new ZDateTime(2013, 8, 30);
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_DeferredTaxDueDate = new ZDateTime(2013, 11, 29);
			DeleteAllGenAddOnColumn(declaration2);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			DeleteAllGenAddOnColumn(declaration3);

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleDateFilter)bizObj[DeclarationFilterConstants.DeferredTaxDueDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property2 = new ZDateTime(2013, 8, 30);//to-date

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(bizObj.Filter);
			AssertEquals("One declaration should be found", 1, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should not be found", false, coll.Contains(declaration2));

			filter.Property2 = new ZDateTime(2013, 11, 30);//to-date
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			filter.Property1 = new ZDateTime(2013, 09, 27);//from-date
			coll.Load(bizObj.Filter);
			AssertEquals("One declaration should be found", 1, coll.Count);
			AssertEquals("declaration1 should not be found", false, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			filter.Property2 = ZDateTime.Empty;//clear out date values
			filter.Property1 = ZDateTime.Empty;
			coll.Load(bizObj.Filter);
			AssertEquals("All declarations should be found", 3, coll.Count);
			AssertEquals("declaration3 should now be shown as well", true, coll.Contains(declaration3));

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			coll.Load(bizObj.Filter);
			AssertEquals("One declarations should be found", 1, coll.Count);
			AssertEquals("declaration1 should not be found", false, coll.Contains(declaration1));
			AssertEquals("declaration2 should not be found", false, coll.Contains(declaration2));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration3));
		}

		public void TestSearchWithBondNumberWithoutGenAddOnColumn()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec1.US_BondProducerAccNo = "1234";
			DeleteAllGenAddOnColumn(dec1);

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec2.US_BondProducerAccNo = "5678";
			DeleteAllGenAddOnColumn(dec2);

			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_MessageType = JobMessageTypeList.Codes.Import;
			DeleteAllGenAddOnColumn(dec3);

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.BondNumber];
			filter.IsActive = true;

			filter.Property = dec1.US_BondProducerAccNo;
			Assert(dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));
			Assert(!dec3.MatchesFilter(bizObj.Filter));

			filter.Property = dec2.US_BondProducerAccNo;
			Assert(!dec1.MatchesFilter(bizObj.Filter));
			Assert(dec2.MatchesFilter(bizObj.Filter));
			Assert(!dec3.MatchesFilter(bizObj.Filter));
		}

		public void TestSearchWithBIRDRefWithoutGenAddOnColumn()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec1.US_BRDRefNo = "11";
			DeleteAllGenAddOnColumn(dec1);

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec2.US_BRDRefNo = "12";
			DeleteAllGenAddOnColumn(dec2);

			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_MessageType = JobMessageTypeList.Codes.Import;
			DeleteAllGenAddOnColumn(dec3);

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.BIRDBrokerRef];
			filter.IsActive = true;

			filter.Property = "11";
			Assert(dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));

			filter.Property = ZString.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			Assert(dec3.MatchesFilter(bizObj.Filter));
			Assert(!dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			Assert(!dec3.MatchesFilter(bizObj.Filter));
			Assert(dec1.MatchesFilter(bizObj.Filter));
			Assert(dec2.MatchesFilter(bizObj.Filter));
		}

		public void TestSearchWithFilerCodeWithoutGenAddOnColumn()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec1.US_EntryFilerCode = "XJ5";
			DeleteAllGenAddOnColumn(dec1);

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec2.US_EntryFilerCode = "ABC";
			DeleteAllGenAddOnColumn(dec2);

			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_MessageType = JobMessageTypeList.Codes.Import;
			DeleteAllGenAddOnColumn(dec3);

			var dec4 = Factory.New<JobDeclaration>();
			dec4.JE_MessageType = JobMessageTypeList.Codes.Recon;
			dec4.US_EntryFilerCode = "REC";
			DeleteAllGenAddOnColumn(dec4);

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.Filer];
			filter.IsActive = true;

			filter.Property = "ABC";
			Assert(!dec1.MatchesFilter(bizObj.Filter));
			Assert(dec2.MatchesFilter(bizObj.Filter));
			Assert(!dec4.MatchesFilter(bizObj.Filter));

			filter.Property = "XJ5";
			Assert(dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));
			Assert(!dec4.MatchesFilter(bizObj.Filter));

			filter.Property = "REC";
			Assert(!dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));
			Assert(dec4.MatchesFilter(bizObj.Filter));

			filter.Property = "";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			Assert(!dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));
			Assert(dec3.MatchesFilter(bizObj.Filter));
			Assert(!dec4.MatchesFilter(bizObj.Filter));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			Assert(dec1.MatchesFilter(bizObj.Filter));
			Assert(dec2.MatchesFilter(bizObj.Filter));
			Assert(!dec3.MatchesFilter(bizObj.Filter));
			Assert(dec4.MatchesFilter(bizObj.Filter));
		}

		public void TestDispositionCodeWithoutGenAddOnColumn()
		{
			var dec1 = Factory.NewWithValidTestData<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec1.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec1.US_BondType = "9";
			dec1.US_BondDispositionCode = BondDispositionCodeList.Codes.CAB;
			DeleteAllGenAddOnColumn(dec1);

			var dec2 = Factory.NewWithValidTestData<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec2.US_BondType = "8";
			dec2.US_BondDispositionCode = BondDispositionCodeList.Codes.CAB;
			DeleteAllGenAddOnColumn(dec2);

			var dec3 = Factory.NewWithValidTestData<JobDeclaration>();
			dec3.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec3.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec3.US_BondType = "9";
			dec3.US_BondDispositionCode = BondDispositionCodeList.Codes.CUB;
			dec3.US_BondDispositionCode2 = BondDispositionCodeList.Codes.CEB;
			DeleteAllGenAddOnColumn(dec3);

			var dec4 = Factory.NewWithValidTestData<JobDeclaration>();
			dec4.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec4.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec4.US_BondType = "9";
			dec4.US_BondDispositionCode = ZString.Empty;
			DeleteAllGenAddOnColumn(dec4);

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.BasicSTBDisposition];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			Assert(!dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));
			Assert(!dec3.MatchesFilter(bizObj.Filter));
			Assert(dec4.MatchesFilter(bizObj.Filter));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			Assert(dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));
			Assert(dec3.MatchesFilter(bizObj.Filter));
			Assert(!dec4.MatchesFilter(bizObj.Filter));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = BondDispositionCodeList.Codes.CAB;
			Assert(dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));
			Assert(!dec3.MatchesFilter(bizObj.Filter));

			var bizObj2 = new JobDeclarationFilterBusinessObject();
			var filter2 = (ModuleTextFilter)bizObj2[DeclarationFilterConstants.AdditionalBondDisposition];
			filter2.IsActive = true;
			filter2.Property = BondDispositionCodeList.Codes.CEB;
			Assert(!dec1.MatchesFilter(bizObj2.Filter));
			Assert(!dec2.MatchesFilter(bizObj2.Filter));
			Assert(dec3.MatchesFilter(bizObj2.Filter));
		}

		public void TestEntryModeQueryWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_EntryMode = EntryModeList.Codes.RLF;
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_EntryMode = EntryModeList.Codes.Paired;
			DeleteAllGenAddOnColumn(declaration2);

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.EntryMode];
			AssertEquals(FilterCategories.ModesAndTypes, filter.Category);
			filter.IsActive = true;
			filter.Property = EntryModeList.Codes.RLF;
			AssertEquals("declaration1 matches filter", true, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 does not match filter", false, declaration2.MatchesFilter(bizObj.Filter));

			filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.EntryMode];
			filter.IsActive = true;
			filter.Property = EntryModeList.Codes.Paired;
			AssertEquals("declaration1 does not match filter", false, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 matches filter", true, declaration2.MatchesFilter(bizObj.Filter));
		}

		public void TestTIBClosedQueryWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.Invoices.AddNew();
			declaration1.InvoiceLines.AddNew().JI_AddInfo = "UC_NKCountryOfOrigin=KR*SPI=N/A*SecondarySPI=Z";
			declaration1.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration2.Invoices.AddNew();
			declaration2.InvoiceLines.AddNew().JI_AddInfo = "UC_NKCountryOfOrigin=KR*SPI=N/A*SecondarySPI=Z";
			DeleteAllGenAddOnColumn(declaration2);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.Invoices.AddNew();
			declaration3.InvoiceLines.AddNew().JI_AddInfo = "UC_NKCountryOfOrigin=KR*SPI=N/A*SecondarySPI=Z*FDAIndicator=D";
			declaration3.Logs.AddNew(Events.RecordAudited, Enterprise.ZArchitecture.Business.Internal.BusinessObjectLogger.PrefixIndicator + Customs.US.Business.AuditFieldsList.Codes.SPI, ZDateTime.BrettsBirthday.ToOffset());
			DeleteAllGenAddOnColumn(declaration3);

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration4.Invoices.AddNew();
			declaration4.InvoiceLines.AddNew().JI_AddInfo = "UC_NKCountryOfOrigin=KR*SPI=AU*SecondarySPI=Z*FDAIndicator=C";
			declaration4.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration4.Logs.AddNew(Events.RecordAudited, Enterprise.ZArchitecture.Business.Internal.BusinessObjectLogger.PrefixIndicator + Customs.US.Business.AuditFieldsList.Codes.FDA, ZDateTime.BrettsBirthday.AddYears(1).ToOffset());
			declaration4.Logs.AddNew(Events.StatusUpdated, Enterprise.ZArchitecture.Business.Internal.BusinessObjectLogger.PrefixIndicator + Customs.US.Business.AuditFieldsList.Codes.TIB, ZDateTime.BrettsBirthday.AddDays(1).ToOffset());
			DeleteAllGenAddOnColumn(declaration4);

			var declaration5 = Factory.New<JobDeclaration>();
			declaration5 = Factory.New<JobDeclaration>();
			declaration5.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration5.Invoices.AddNew();
			declaration5.InvoiceLines.AddNew().JI_AddInfo = "UC_NKCountryOfOrigin=KR*SPI=N/A*SecondarySPI=Z*FDAIndicator=C";
			declaration5.Logs.AddNew(Events.RecordAudited, Enterprise.ZArchitecture.Business.Internal.BusinessObjectLogger.PrefixIndicator + Customs.US.Business.AuditFieldsList.Codes.FDA);
			DeleteAllGenAddOnColumn(declaration5);

			var entry = declaration5.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryDelete;

			var declaration6 = Factory.New<JobDeclaration>();
			declaration6 = Factory.New<JobDeclaration>();
			declaration6.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration6.Invoices.AddNew();
			declaration6.InvoiceLines.AddNew().JI_AddInfo = "UC_NKCountryOfOrigin=KR*SPI=N/A*SecondarySPI=Z*FDAIndicator=C";
			DeleteAllGenAddOnColumn(declaration6);

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleFlagsFilter)bizObj[DeclarationFilterConstants.TIBClosed];
			filter.IsActive = true;
			filter.Property1 = true;//close required

			AssertEquals("declaration1 should be closed", true, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration4 already closed", false, declaration4.MatchesFilter(bizObj.Filter));

			filter.Property0 = true;//closed
			AssertEquals("PreCondition", false, filter.Property1);

			AssertEquals("declaration1 not closed", false, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration4 already closed", true, declaration4.MatchesFilter(bizObj.Filter));

			//all declaration shown
			filter.Property0 = false;
			filter.Property1 = false;
			AssertEquals("declaration1", true, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2", true, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration3", true, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration4", true, declaration4.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration5", true, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration6", true, declaration1.MatchesFilter(bizObj.Filter));
		}

		public void TestAnticipatedLiquidationDateFilterWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			DeleteAllGenAddOnColumn(declaration1);
			var entry1 = declaration1.ActiveEntryHeaders.AddNew();
			entry1.US_ALDate = ZDateTime.Today.AddDays(-1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			DeleteAllGenAddOnColumn(declaration2);
			var entry2 = declaration2.ActiveEntryHeaders.AddNew();

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			DeleteAllGenAddOnColumn(declaration3);
			var entry3 = declaration3.ActiveEntryHeaders.AddNew();
			entry3.US_ALDate = ZDateTime.Today.AddDays(-45);

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Export;
			DeleteAllGenAddOnColumn(declaration4);
			var entry4 = declaration4.ActiveEntryHeaders.AddNew();

			var declaration5 = Factory.New<JobDeclaration>();
			declaration5.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			DeleteAllGenAddOnColumn(declaration5);
			declaration5.US_ALDate = ZDateTime.Today.AddDays(1);

			var declaration6 = Factory.New<JobDeclaration>();
			declaration6.JE_MessageType = JobMessageTypeList.Codes.Recon;
			DeleteAllGenAddOnColumn(declaration6);
			var reconDeclaration = new ReconDeclaration(declaration6);
			var entry = reconDeclaration.ReconEntry.GetEntry();
			entry.US_ALDate = ZDateTime.Today.AddDays(2);

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var liqDate = (ModuleDateFilter)bizObj[DeclarationFilterConstants.AnticipLiquidationDate];
			liqDate.IsActive = true;
			liqDate.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			liqDate.Property2 = ZDateTime.Today;

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration3));

			liqDate.Property2 = ZDateTime.Today.AddDays(2);
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found", 4, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration3));
			AssertEquals("declaration5 should be found", true, coll.Contains(declaration5));
			AssertEquals("declaration6 should be found", true, coll.Contains(declaration6));

			liqDate.Property2 = ZDateTime.Empty;
			liqDate.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Last7Days;
			coll.Load(bizObj.Filter);
			AssertEquals("One declaration should be found", 1, coll.Count);
			AssertEquals("declaration1 should not be shown", true, coll.Contains(declaration1));

			liqDate.PropertySearch = ModuleDateFilter.HasDateEntered;
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found", 4, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration3));
			AssertEquals("declaration5 should be found", true, coll.Contains(declaration5));
			AssertEquals("declaration6 should be found", true, coll.Contains(declaration6));

			liqDate.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration2));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration4));
		}

		public void TestEntryDateFilterWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_EntryDate = new ZDateTime(2009, 8, 30);
			declaration1.MergeManager.DisablePreSaveMergeRequirementForTesting();
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_EntryDate = new ZDateTime(2009, 7, 15);
			declaration2.MergeManager.DisablePreSaveMergeRequirementForTesting();
			DeleteAllGenAddOnColumn(declaration2);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			DeleteAllGenAddOnColumn(declaration3);

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var entryDate = (ModuleDateFilter)bizObj[DeclarationFilterConstants.EntryDate];
			entryDate.IsActive = true;
			entryDate.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			entryDate.Property2 = new ZDateTime(2009, 8, 30);//to-date

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			entryDate.Property1 = new ZDateTime(2009, 7, 31);//from-date
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("declaration2 should not be shown", false, coll.Contains(declaration2));
			AssertEquals("declaration1 should still be shown", true, coll.Contains(declaration1));

			entryDate.Property2 = ZDateTime.Empty;//clear out date values
			entryDate.Property1 = ZDateTime.Empty;
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("All declarations should be found", 3, coll.Count);
			AssertEquals("declaration3 should now be shown as well", true, coll.Contains(declaration3));
		}

		public void TestExportDateFilterWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration1.US_DateOfExport = new ZDateTime(2022, 12, 05);
			declaration1.JE_ExportDate = new ZDateTime(2022, 11, 25);
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration2.JE_ExportDate = new ZDateTime(2022, 12, 15);
			DeleteAllGenAddOnColumn(declaration2);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration3.US_DateOfExport = new ZDateTime(2022, 12, 25);
			DeleteAllGenAddOnColumn(declaration3);

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Export;
			DeleteAllGenAddOnColumn(declaration4);

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var exportDate = (ModuleDateFilter)bizObj[DeclarationFilterConstants.ExportDate];
			exportDate.IsActive = true;
			exportDate.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			exportDate.Property1 = new ZDateTime(2022, 12, 12); //from-date
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("2 declarations should be found", 2, coll.Count);
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration3));

			exportDate.Property1 = ZDateTime.Empty;
			exportDate.Property2 = new ZDateTime(2022, 12, 6); //to-date
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("1 declarations should be found", 1, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));

			exportDate.Property2 = new ZDateTime(2022, 12, 1);
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("0 declarations should be found", 0, coll.Count);

			exportDate.Property1 = ZDateTime.Empty; //clear out date values
			exportDate.Property2 = ZDateTime.Empty;
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("All declarations should be found", 4, coll.Count);
			AssertEquals("declaration1 should now be shown as well", true, coll.Contains(declaration1));
			AssertEquals("declaration4 should now be shown as well", true, coll.Contains(declaration4));

			exportDate.PropertySearch = ModuleDateFilter.HasDateEntered;
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("3 declarations should be found", 3, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration3));

			exportDate.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("1 declarations should be found", 1, coll.Count);
			AssertEquals("declaration4 should be found", true, coll.Contains(declaration4));
		}

		public void TestEstimatedEntryDateFilterWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_EstimatedEntryDate = new ZDateTime(2009, 8, 30);
			declaration1.MergeManager.DisablePreSaveMergeRequirementForTesting();
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_EstimatedEntryDate = new ZDateTime(2009, 7, 15);
			declaration2.MergeManager.DisablePreSaveMergeRequirementForTesting();
			DeleteAllGenAddOnColumn(declaration2);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			DeleteAllGenAddOnColumn(declaration3);

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration4.US_EstimatedEntryDate = new ZDateTime(2009, 7, 15);
			declaration4.MergeManager.DisablePreSaveMergeRequirementForTesting();
			DeleteAllGenAddOnColumn(declaration4);

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var estimatedEntryDate = (ModuleDateFilter)bizObj[DeclarationFilterConstants.EstimatedEntryDate];
			estimatedEntryDate.IsActive = true;
			estimatedEntryDate.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			estimatedEntryDate.Property2 = new ZDateTime(2009, 8, 30);//to-date

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found", 3, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));
			AssertEquals("declaration4 should be found", true, coll.Contains(declaration4));

			estimatedEntryDate.Property1 = new ZDateTime(2009, 7, 31);//from-date
			coll.Load(bizObj.Filter);
			AssertEquals("declaration2 should not be shown", false, coll.Contains(declaration2));
			AssertEquals("declaration1 should still be shown", true, coll.Contains(declaration1));
			AssertEquals("declaration4 should not be shown", false, coll.Contains(declaration4));

			declaration2.US_EstimatedEntryDate = new ZDateTime(2009, 8, 01); // re-calculation of payment due on this dec
			Factory.Save();
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found again", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found again", true, coll.Contains(declaration2));
			AssertEquals("declaration4 should not be shown", false, coll.Contains(declaration4));

			estimatedEntryDate.Property2 = ZDateTime.Empty;//clear out date values
			estimatedEntryDate.Property1 = ZDateTime.Empty;
			coll.Load(bizObj.Filter);
			AssertEquals("All declarations should be found", 4, coll.Count);
			AssertEquals("declaration3 should now be shown as well", true, coll.Contains(declaration3));
		}

		public void TestPreliminaryStatementPrintDateFilterWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_PreliminaryStatementPrintDate = new ZDateTime(2009, 8, 30);
			declaration1.MergeManager.DisablePreSaveMergeRequirementForTesting();
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_PreliminaryStatementPrintDate = new ZDateTime(2009, 7, 15);
			declaration2.MergeManager.DisablePreSaveMergeRequirementForTesting();
			DeleteAllGenAddOnColumn(declaration2);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			DeleteAllGenAddOnColumn(declaration3);

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Recon;
			declaration4.US_PreliminaryStatementPrintDate = new ZDateTime(2009, 7, 15);
			declaration4.MergeManager.DisablePreSaveMergeRequirementForTesting();
			DeleteAllGenAddOnColumn(declaration4);

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var prelimStmPrintDate = (ModuleDateFilter)bizObj[DeclarationFilterConstants.PrelimStatemPrintDate];
			prelimStmPrintDate.IsActive = true;
			prelimStmPrintDate.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			prelimStmPrintDate.Property2 = new ZDateTime(2009, 8, 30);//to-date

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found", 3, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));
			AssertEquals("declaration4 should be found", true, coll.Contains(declaration4));

			prelimStmPrintDate.Property1 = new ZDateTime(2009, 7, 31);//from-date
			coll.Load(bizObj.Filter);
			AssertEquals("declaration2 should not be shown", false, coll.Contains(declaration2));
			AssertEquals("declaration1 should still be shown", true, coll.Contains(declaration1));
			AssertEquals("declaration4 should not be shown", false, coll.Contains(declaration4));

			declaration2.US_PreliminaryStatementPrintDate = new ZDateTime(2009, 8, 01); // re-calculation of payment due on this dec
			Factory.Save();
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found again", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found again", true, coll.Contains(declaration2));
			AssertEquals("declaration4 should not be shown", false, coll.Contains(declaration4));

			prelimStmPrintDate.Property2 = ZDateTime.Empty;//clear out date values
			prelimStmPrintDate.Property1 = ZDateTime.Empty;
			coll.Load(bizObj.Filter);
			AssertEquals("All declarations should be found", 4, coll.Count);
			AssertEquals("declaration3 should now be shown as well", true, coll.Contains(declaration3));
		}

		public void TestPresentationDateFilterWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.US_PresentationDate = new ZDateTime(2011, 08, 31);
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.US_PresentationDate = new ZDateTime(2011, 07, 31);
			DeleteAllGenAddOnColumn(declaration2);

			var declaration3 = Factory.New<JobDeclaration>();
			DeleteAllGenAddOnColumn(declaration3);

			Factory.Save();

			var presentationDateFilter = (ModuleDateFilter)FilterBO[DeclarationFilterConstants.PresentationDate];
			presentationDateFilter.IsActive = true;
			presentationDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);

			presentationDateFilter.Property2 = new ZDateTime(2011, 08, 31); // to-date
			coll.Load(FilterBO.Filter);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			presentationDateFilter.Property1 = new ZDateTime(2011, 08, 01); // from-date
			coll.Load(FilterBO.Filter);
			AssertEquals("1 declaration should be found", 1, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should not be found", false, coll.Contains(declaration2));

			presentationDateFilter.Property1 = ZDateTime.Empty;
			presentationDateFilter.Property2 = ZDateTime.Empty;
			coll.Load(FilterBO.Filter);
			AssertEquals("All declarations should be found", 3, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration3));
		}

		public void TestPaymentDueDateFilterWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_PaymentDueDate = new ZDateTime(2009, 8, 30);
			declaration1.MergeManager.DisablePreSaveMergeRequirementForTesting();
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_PaymentDueDate = new ZDateTime(2009, 7, 15);
			declaration2.MergeManager.DisablePreSaveMergeRequirementForTesting();
			DeleteAllGenAddOnColumn(declaration2);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			DeleteAllGenAddOnColumn(declaration3);

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var paymentDueDates = (ModuleDateFilter)bizObj[DeclarationFilterConstants.PaymentDueDate];
			paymentDueDates.IsActive = true;
			paymentDueDates.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			paymentDueDates.Property2 = new ZDateTime(2009, 8, 30);//to-date

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			paymentDueDates.Property1 = new ZDateTime(2009, 7, 31);//from-date
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("declaration2 should not be shown", false, coll.Contains(declaration2));
			AssertEquals("declaration1 should still be shown", true, coll.Contains(declaration1));

			declaration2.US_PaymentDueDate = new ZDateTime(2009, 8, 01); // re-calculation of payment due on this dec
			Factory.Save();
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found again", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found again", true, coll.Contains(declaration2));

			paymentDueDates.Property2 = ZDateTime.Empty;//clear out date values
			paymentDueDates.Property1 = ZDateTime.Empty;
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("All declarations should be found", 3, coll.Count);
			AssertEquals("declaration3 should now be shown as well", true, coll.Contains(declaration3));
		}

		public void TestEBondMessageStatusFilterWithoutGenAddOnColumn()
		{
			var dec1 = Factory.NewWithValidTestData<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec1.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec1.US_BondType = "9";
			dec1.US_InsuranceDisposition = InsuranceDispositionCodeList.Codes.SentToSurety;
			DeleteAllGenAddOnColumn(dec1);

			var dec2 = Factory.NewWithValidTestData<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec2.US_BondType = "8";
			dec2.US_InsuranceDisposition = InsuranceDispositionCodeList.Codes.SentToSurety;
			DeleteAllGenAddOnColumn(dec2);

			var dec3 = Factory.NewWithValidTestData<JobDeclaration>();
			dec3.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec3.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec3.US_BondType = "9";
			dec3.US_InsuranceDisposition = ZString.Empty;
			DeleteAllGenAddOnColumn(dec3);

			var dec4 = Factory.NewWithValidTestData<JobDeclaration>();
			dec4.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec4.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec4.US_BondType = "9";
			dec4.US_InsuranceDisposition = InsuranceDispositionCodeList.Codes.AcceptedByCBP;
			DeleteAllGenAddOnColumn(dec4);

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.EBondMessageStatus];
			filter.IsActive = true;
			filter.Property = InsuranceDispositionCodeList.Codes.SentToSurety;
			Assert(dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));
			Assert(!dec3.MatchesFilter(bizObj.Filter));
			Assert(!dec4.MatchesFilter(bizObj.Filter));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			Assert(!dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));
			Assert(dec3.MatchesFilter(bizObj.Filter));
			Assert(dec4.MatchesFilter(bizObj.Filter));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			Assert(!dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));
			Assert(dec3.MatchesFilter(bizObj.Filter));
			Assert(!dec4.MatchesFilter(bizObj.Filter));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			Assert(dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));
			Assert(!dec3.MatchesFilter(bizObj.Filter));
			Assert(dec4.MatchesFilter(bizObj.Filter));
		}

		public void TestPaperlessFilterWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>(); // Import - CRL
			var declaration2 = Factory.New<JobDeclaration>(); // Export
			var declaration3 = Factory.New<JobDeclaration>(); // Import - CRL & ENS, but ENS not sent
			var declaration4 = Factory.New<JobDeclaration>(); // Import - ENS sent
			var declaration5 = Factory.New<JobDeclaration>(); // Import - ENS sent
			var declaration6 = Factory.New<JobDeclaration>(); // Import - CRL & ENS
			var declaration7 = Factory.New<JobDeclaration>(); // Import - ENS sent

			declaration1.JE_MessageType = "IMP";
			declaration3.JE_MessageType = "IMP";
			declaration4.JE_MessageType = "IMP";
			declaration5.JE_MessageType = "IMP";
			declaration6.JE_MessageType = "IMP";
			declaration7.JE_MessageType = "IMP";

			DeleteAllGenAddOnColumn(declaration1);
			DeleteAllGenAddOnColumn(declaration2);
			DeleteAllGenAddOnColumn(declaration3);
			DeleteAllGenAddOnColumn(declaration4);
			DeleteAllGenAddOnColumn(declaration5);
			DeleteAllGenAddOnColumn(declaration6);
			DeleteAllGenAddOnColumn(declaration7);

			var dec1Entry = declaration1.CustomsEntryHeaders.AddNew();
			dec1Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;

			var dec3CRLEntry = declaration3.CustomsEntryHeaders.AddNew();
			dec3CRLEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			var dec3ENSEntry = declaration3.CustomsEntryHeaders.AddNew();
			dec3ENSEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var dec4Entry = declaration4.CustomsEntryHeaders.AddNew();
			dec4Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			dec4Entry.CH_Status = MessageStatusListENS.Codes.AwaitingEntrySummaryOriginal;

			var dec5Entry = declaration5.CustomsEntryHeaders.AddNew();
			dec5Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			dec5Entry.CH_Status = MessageStatusListENS.Codes.ClearEntrySummaryOriginal;

			var dec6CRLEntry = declaration6.CustomsEntryHeaders.AddNew();
			dec6CRLEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			var dec6ENSEntry = declaration6.CustomsEntryHeaders.AddNew();
			dec6ENSEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			dec6ENSEntry.CH_Status = MessageStatusListENS.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings;

			var dec7Entry = declaration5.CustomsEntryHeaders.AddNew();
			dec7Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			dec7Entry.CH_Status = MessageStatusListENS.Codes.ClearEntrySummaryOriginal;

			declaration4.US_PaperlessEntry = YesNoDefaultList.Codes.Yes;
			declaration5.US_PaperlessEntry = YesNoDefaultList.Codes.Yes;
			declaration6.US_PaperlessEntry = YesNoDefaultList.Codes.No;
			declaration7.US_PaperlessEntry = YesNoDefaultList.Codes.Yes;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.Paperless];
			filter.IsActive = true;
			filter.Property = ZString.Empty;

			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals("Empty search should have found all 7 declarations", 7, collection.Count);
			AssertEquals("declaration2 (export entry) should be in list", true, collection.Contains(declaration2));

			filter.Property = YesNoDefaultList.Codes.No;
			collection.Load(FilterBO.Filter);
			AssertEquals("Paperless search for 'No' should find 1 Import declaration with an Entry Summary entry and no paperless status", 1, collection.Count);
			AssertEquals("declaration6 should be returned", true, collection.Contains(declaration6));

			filter.Property = YesNoDefaultList.Codes.Yes;
			collection.Load(FilterBO.Filter);
			AssertEquals("Paperless search for 'Yes' should have found 3 declarations", 3, collection.Count);
			AssertEquals("declaration4 should be returned", true, collection.Contains(declaration4));
			AssertEquals("declaration5 should be returned", true, collection.Contains(declaration5));
			AssertEquals("declaration7 should be returned", true, collection.Contains(declaration7));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			collection.Load(FilterBO.Filter);
			AssertEquals("Search for Paperless 'Not True' should only find 4 declarations without a Paperless Status of Y", 4, collection.Count);
			AssertEquals("declaration1 should be returned", true, collection.Contains(declaration1));
			AssertEquals("declaration2 should be returned", true, collection.Contains(declaration2));
			AssertEquals("declaration3 should be returned", true, collection.Contains(declaration3));
			AssertEquals("declaration6 should be returned", true, collection.Contains(declaration6));

			filter.Property = YesNoDefaultList.Codes.No;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			collection.Load(FilterBO.Filter);
			AssertEquals("Search for Paperless 'Not False' should have found 6 declarations without a Paperless Status of N", 6, collection.Count);
			AssertEquals("declaration1 should be returned", true, collection.Contains(declaration1));
			AssertEquals("declaration2 should be returned", true, collection.Contains(declaration2));
			AssertEquals("declaration3 should be returned", true, collection.Contains(declaration3));
			AssertEquals("declaration4 should be returned", true, collection.Contains(declaration4));
			AssertEquals("declaration5 should be returned", true, collection.Contains(declaration5));
			AssertEquals("declaration7 should be returned", true, collection.Contains(declaration7));
		}

		public void TestPSCFilterWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.US_PSC = true;
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.US_PSC = true;
			DeleteAllGenAddOnColumn(declaration2);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.US_PSC = false;
			DeleteAllGenAddOnColumn(declaration3);

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.US_PSC = false;
			DeleteAllGenAddOnColumn(declaration4);

			var declaration5 = Factory.New<JobDeclaration>();
			declaration5.US_PSC = false;
			DeleteAllGenAddOnColumn(declaration5);

			Factory.Save();

			var pscFilter = (ModuleFlagsFilter)FilterBO[DeclarationFilterConstants.PSCIndicator];
			pscFilter.IsActive = true;

			pscFilter.Property0 = true;
			Assert("Dec1", declaration1.MatchesFilter(FilterBO.Filter));
			Assert("Dec2", declaration2.MatchesFilter(FilterBO.Filter));
			Assert("Dec3", !declaration3.MatchesFilter(FilterBO.Filter));
			Assert("Dec4", !declaration4.MatchesFilter(FilterBO.Filter));
			Assert("Dec5", !declaration5.MatchesFilter(FilterBO.Filter));

			// if unticked, return all
			pscFilter.Property0 = false;
			Assert("Dec1", declaration1.MatchesFilter(FilterBO.Filter));
			Assert("Dec2", declaration2.MatchesFilter(FilterBO.Filter));
			Assert("Dec3", declaration3.MatchesFilter(FilterBO.Filter));
			Assert("Dec4", declaration4.MatchesFilter(FilterBO.Filter));
			Assert("Dec5", declaration5.MatchesFilter(FilterBO.Filter));
		}

		public void TestElectronicInvoiceRequestedFilterWithoutGenAddOnColumn()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec1.US_IsAIIRequested = true;
			DeleteAllGenAddOnColumn(dec1);

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec2.US_IsAIIRequested = true;
			DeleteAllGenAddOnColumn(dec2);

			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec3.US_IsAIIRequested = false;
			DeleteAllGenAddOnColumn(dec3);

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleFlagsFilter)bizObj[DeclarationFilterConstants.ElectronicInvoiceRequested];
			filter.IsActive = true;
			filter.Property0 = true; // AII Requested

			AssertEquals("declaration1 - AII requested", true, dec1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 - AII requested", true, dec2.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration3 - AII not requested", false, dec3.MatchesFilter(bizObj.Filter));

			filter.Property0 = false; // all declarations
			AssertEquals(true, dec1.MatchesFilter(bizObj.Filter));
			AssertEquals(true, dec2.MatchesFilter(bizObj.Filter));
			AssertEquals(true, dec3.MatchesFilter(bizObj.Filter));
		}

		public void TestCarrierSCACFilterWithoutGenAddOnColumn()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsTransportClient = true;
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "HLKP");

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration1.JE_AgentsReference = "Export Dec with Carrier SCAC";
			declaration1.JE_OH_ShippingLine = carrier.PK;
			declaration1.US_UI_NKCarrierSCAC = ZString.Empty;
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration2.JE_AgentsReference = "Export Dec, no Carrier SCAC";
			DeleteAllGenAddOnColumn(declaration2);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.JE_AgentsReference = "Import Dec with Carrier SCAC";
			declaration3.US_UI_NKCarrierSCAC = "HLKP";
			DeleteAllGenAddOnColumn(declaration3);

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration4.JE_AgentsReference = "Import Dec, no Carrier SCAC";
			DeleteAllGenAddOnColumn(declaration4);

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingProvider = true;
			carrier2.OH_IsTransportClient = true;

			var declaration5 = Factory.New<JobDeclaration>();
			declaration5.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration5.JE_AgentsReference = "Export with Ship Line without SCAC";
			declaration5.JE_OH_ShippingLine = carrier2.PK;
			DeleteAllGenAddOnColumn(declaration5);

			var declaration6 = Factory.New<JobDeclaration>();
			declaration6.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration6.JE_AgentsReference = "Export Dec with Carrier SCAC";
			declaration6.US_UI_NKCarrierSCAC = "HLKP";
			DeleteAllGenAddOnColumn(declaration6);

			var declaration7 = Factory.New<JobDeclaration>();
			declaration7.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration7.JE_AgentsReference = "Export Dec with Carrier SCAC";
			declaration7.JE_OH_ShippingLine = carrier.PK;
			declaration7.US_UI_NKCarrierSCAC = "APLU";
			DeleteAllGenAddOnColumn(declaration7);

			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();
			carrier3.OH_IsShippingProvider = true;
			carrier3.OH_IsTransportClient = true;
			carrier3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "ALPU");

			var declaration8 = Factory.New<JobDeclaration>();
			declaration8.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration8.JE_AgentsReference = "Export Dec with Carrier SCAC";
			declaration8.JE_OH_ShippingLine = carrier3.PK;
			declaration8.US_UI_NKCarrierSCAC = "HLKP";
			DeleteAllGenAddOnColumn(declaration8);

			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.CarrierSCAC];
			filter.IsActive = true;
			filter.Property = ZString.Empty;

			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals("Empty Carrier SCAC search should have found all declarations", 8, collection.Count);

			filter.Property = "HLKP";
			collection.Load(FilterBO.Filter);
			AssertEquals("Carrier SCAC search should have found 3 declarations", 4, collection.Count);
			AssertEquals("declaration1 should be returned", true, collection.Contains(declaration1));
			AssertEquals("declaration3 should be returned", true, collection.Contains(declaration3));
			AssertEquals("declaration6 should be returned", true, collection.Contains(declaration6));
			AssertEquals("declaration8 should be returned", true, collection.Contains(declaration8));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			filter.Property = "HLKP";
			collection.Load(FilterBO.Filter);
			AssertEquals("Carrier SCAC search should have found 3 declarations", 4, collection.Count);
			AssertEquals("declaration2 should be returned", true, collection.Contains(declaration2));
			AssertEquals("declaration4 should be returned", true, collection.Contains(declaration4));
			AssertEquals("declaration5 should be returned", true, collection.Contains(declaration5));
			AssertEquals("declaration7 should be returned", true, collection.Contains(declaration7));

			filter.Property = ZString.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			collection.Load(FilterBO.Filter);
			AssertEquals("Blank Carrier SCAC search should have found 3 declarations", 3, collection.Count);
			AssertEquals("declaration2 should be returned", true, collection.Contains(declaration2));
			AssertEquals("declaration4 should be returned", true, collection.Contains(declaration4));
			AssertEquals("declaration5 should be returned", true, collection.Contains(declaration5));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			collection.Load(FilterBO.Filter);
			AssertEquals("Not Blank Carrier SCAC search should have found 5 declarations", 5, collection.Count);
			AssertEquals("declaration1 should be returned", true, collection.Contains(declaration1));
			AssertEquals("declaration3 should be returned", true, collection.Contains(declaration3));
			AssertEquals("declaration6 should be returned", true, collection.Contains(declaration6));
			AssertEquals("declaration7 should be returned", true, collection.Contains(declaration7));
			AssertEquals("declaration8 should be returned", true, collection.Contains(declaration8));
		}

		public void TestLocationOfGoodsFilterWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			DeleteAllGenAddOnColumn(declaration2);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			DeleteAllGenAddOnColumn(declaration3);

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Import;
			DeleteAllGenAddOnColumn(declaration4);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "LWH1", "Misaka", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "LWH2", "Kanzaki", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			declaration1.US_US_NKLocationOfGoods = "LWH1";
			declaration2.US_US_NKLocationOfGoods = "LWH2";
			declaration3.US_US_NKLocationOfGoods = "LWH1";
			declaration4.US_US_NKLocationOfGoods = ZString.Empty;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.LocationOfGoods];
			filter.IsActive = true;
			filter.Property = ZString.Empty;

			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals("Empty FIRMS Code search should have found all 4 declarations", 4, collection.Count);
			AssertEquals("declaration4 (without Firms Code) should be in list", true, collection.Contains(declaration4));

			filter.Property = "LWH2";
			collection.Load(FilterBO.Filter);
			AssertEquals("Firms Code search for LWH2 should have found only 1 declaration", 1, collection.Count);
			AssertEquals("declaration2 should be returned", true, collection.Contains(declaration2));

			filter.Property = "LWH1";
			collection.Load(FilterBO.Filter);
			AssertEquals("Firms Code search for LWH1 should have found 2 declarations", 2, collection.Count);
			AssertEquals("declaration1 should be returned", true, collection.Contains(declaration1));
			AssertEquals("declaration3 should be returned", true, collection.Contains(declaration3));

			filter.Property = "";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			collection.Load(FilterBO.Filter);
			AssertEquals("Firms Code search for blank filter should have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration4 should be returned", true, collection.Contains(declaration4));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			collection.Load(FilterBO.Filter);
			AssertEquals("Firms Code search for not blank filter should have found 3 declarations", 3, collection.Count);
			AssertEquals("declaration1 should be returned", true, collection.Contains(declaration1));
			AssertEquals("declaration2 should be returned", true, collection.Contains(declaration2));
			AssertEquals("declaration3 should be returned", true, collection.Contains(declaration3));
		}

		public void TestNotReconciledQueryCSSScenarioWithoutGenAddOnColumn()
		{
			// declaration with NAFTA & another reconciliation type. When other type is reconciled, job should still appear in filter list if searching for Not Reconciled NAFTA jobs (& vice versa)
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_OtherReconIndicator = ReconIssueCodeList.Codes.ValueRecon;
			declaration1.US_NAFTAReconIndicator = true;
			DeleteAllGenAddOnColumn(declaration1);
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var reconDec1 = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec1.US_IssueCode = ReconIssueCodeList.Codes.ValueRecon;
			var reconEntry1 = reconDec1.OriginalEntries.AddNew();
			reconEntry1.CH_CH_OriginalEntry = entry1.PK; // VL reconciled
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.NotYetReconciled];
			filter.IsActive = true;
			filter.Property = ReconIssueCodeList.Codes.ValueRecon;
			AssertEquals("declaration1 has been reconciled for 'VL' Recon. Issue", false, declaration1.MatchesFilter(bizObj.Filter));

			filter.IsActive = true;
			filter.Property = ReconIssueCodeList.Codes.FTA;
			AssertEquals("declaration1 has not been reconciled for NAFTA Recon - it should be found as 'not reconciled' in this search.", true, declaration1.MatchesFilter(bizObj.Filter));

			var reconDec1ForNafta = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec1ForNafta.US_IssueCode = ReconIssueCodeList.Codes.FTA;
			var reconEntry1A = reconDec1ForNafta.OriginalEntries.AddNew();
			reconEntry1A.CH_CH_OriginalEntry = entry1.PK; // NAFTA now also reconciled
			Factory.Save();
			AssertEquals("declaration1 has now been reconciled for NAFTA Recon as well - it should not therefore be found as 'not reconciled' in this search.", false, declaration1.MatchesFilter(bizObj.Filter));

			// check filter with multiple jobs....
			var declaration2 = Factory.New<JobDeclaration>(); // not reconciled: VL flagged
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_OtherReconIndicator = ReconIssueCodeList.Codes.ValueRecon;
			DeleteAllGenAddOnColumn(declaration2);
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var declaration3 = Factory.New<JobDeclaration>(); // not reconciled: C9 & NAFTA flagged
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.US_OtherReconIndicator = ReconIssueCodeList.Codes.Class9802Recon;
			declaration3.US_NAFTAReconIndicator = true;
			DeleteAllGenAddOnColumn(declaration3);
			var entry3 = declaration3.CustomsEntryHeaders.AddNew();
			entry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var declaration4 = Factory.New<JobDeclaration>(); // not reconciled: NAFTA flagged
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration4.US_NAFTAReconIndicator = true;
			DeleteAllGenAddOnColumn(declaration4);
			var entry4 = declaration4.CustomsEntryHeaders.AddNew();
			entry4.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var declaration5 = Factory.New<JobDeclaration>(); // reconciled: C9 flagged
			declaration5.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration5.US_OtherReconIndicator = ReconIssueCodeList.Codes.Class9802Recon;
			DeleteAllGenAddOnColumn(declaration5);
			var entry5 = declaration5.CustomsEntryHeaders.AddNew();
			entry5.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var reconDec5 = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec5.US_IssueCode = ReconIssueCodeList.Codes.Class9802Recon;
			var reconEntry5 = reconDec5.OriginalEntries.AddNew();
			reconEntry5.CH_CH_OriginalEntry = entry5.PK; // C9 reconciled

			var declaration6 = Factory.New<JobDeclaration>(); // NON reconciled job
			declaration6.JE_MessageType = JobMessageTypeList.Codes.Import;
			DeleteAllGenAddOnColumn(declaration6);
			var entry6 = declaration6.CustomsEntryHeaders.AddNew();
			entry6.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			Factory.Save();

			filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.NotYetReconciled];
			filter.IsActive = true;
			filter.Property = ReconIssueCodeList.Codes.ValueRecon;
			AssertEquals("declaration1 has been reconciled for 'VL' Recon. Issue", false, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 has not been reconciled for 'VL' Recon. Issue - should be returned in filtered list", true, declaration2.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration3 is not required for VL recon", false, declaration3.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration4 is not required for VL recon", false, declaration4.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration5 is not required for VL recon", false, declaration5.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration6 is non recon dec", false, declaration6.MatchesFilter(bizObj.Filter));

			filter.Property = ReconIssueCodeList.Codes.Class9802Recon;
			AssertEquals("declaration1 is not required for C9 recon", false, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 is not required for C9 recon", false, declaration2.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration3 has not been reconciled for 'C9' Recon. Issue - should be returned in filtered list", true, declaration3.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration4 is not required for C9 recon", false, declaration4.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration5 has  been reconciled for 'C9' Recon. Issue", false, declaration5.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration6 is non recon dec", false, declaration6.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration6 is non recon dec", false, declaration6.MatchesFilter(bizObj.Filter));

			filter.Property = ReconIssueCodeList.Codes.FTA;
			AssertEquals("declaration1 has been reconciled for 'NF' Recon. Issue", false, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 is not required for NF recon", false, declaration2.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration3 has not been reconciled for 'NF' Recon. Issue - should be returned in filtered list", true, declaration3.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration4 has not been reconciled for 'NF' Recon. Issue - should be returned in filtered list", true, declaration4.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration5 is not required for NF recon", false, declaration5.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration6 is non recon dec", false, declaration6.MatchesFilter(bizObj.Filter));
		}

		public void TestImportSpecialistTeamFilterWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.US_TeamNo = "807";
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.US_TeamNo = "106";
			DeleteAllGenAddOnColumn(declaration2);

			var declaration3 = Factory.New<JobDeclaration>();
			DeleteAllGenAddOnColumn(declaration3);

			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.ImportSpecialistTeam];
			filter.IsActive = true;
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals("Empty filter search should have found 3 declarations", 3, collection.Count);

			filter.Property = "807";
			collection.Load(FilterBO.Filter);
			AssertEquals("search shuld have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration1 should be there", true, collection.Contains(declaration1));

			filter.Property = "1";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			collection.Load(FilterBO.Filter);
			AssertEquals("search should have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration2 should be there", true, collection.Contains(declaration2));

			filter.Property = "";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			collection.Load(FilterBO.Filter);
			AssertEquals("search should have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration3 should be there", true, collection.Contains(declaration3));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			collection.Load(FilterBO.Filter);
			AssertEquals("search should have found 2 declarations", 2, collection.Count);
			AssertEquals("declaration1 should be there", true, collection.Contains(declaration1));
			AssertEquals("declaration2 should be there", true, collection.Contains(declaration2));
			AssertEquals("declaration3 should not be there", false, collection.Contains(declaration3));
		}

		public void TestSchDLoadingFilterWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.US_SchDLoading = "1523";
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.US_SchDLoading = "2021";
			DeleteAllGenAddOnColumn(declaration2);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.US_SchDLoading = "3030";
			DeleteAllGenAddOnColumn(declaration3);

			var declaration4 = Factory.New<JobDeclaration>();
			DeleteAllGenAddOnColumn(declaration4);

			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.LoadingSchedDK];
			filter.IsActive = true;
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals("Empty filter search should have found 4 declarations", 4, collection.Count);

			filter.Property = "1523";
			collection.Load(FilterBO.Filter);
			AssertEquals("search shuld have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration1 should be there", true, collection.Contains(declaration1));

			filter.Property = "2021";
			collection.Load(FilterBO.Filter);
			AssertEquals("should now have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration2 should be there", true, collection.Contains(declaration2));

			filter.Property = "3030";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			collection.Load(FilterBO.Filter);
			AssertEquals("search should have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration3 should be there", true, collection.Contains(declaration3));

			filter.Property = "";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			collection.Load(FilterBO.Filter);
			AssertEquals("search should have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration4 should be there", true, collection.Contains(declaration4));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			collection.Load(FilterBO.Filter);
			AssertEquals("search should have found 3 declarations", 3, collection.Count);
			AssertEquals("declaration1 should be there", true, collection.Contains(declaration1));
			AssertEquals("declaration2 should be there", true, collection.Contains(declaration2));
			AssertEquals("declaration3 should be there", true, collection.Contains(declaration2));
		}

		public void TestSchDArrivalFilterWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.US_SchDArrival = "1500";
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.US_SchDArrival = "2000";
			DeleteAllGenAddOnColumn(declaration2);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.US_SchDArrival = "3000";
			DeleteAllGenAddOnColumn(declaration3);

			var declaration4 = Factory.New<JobDeclaration>();
			DeleteAllGenAddOnColumn(declaration4);

			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.DischargeSchedDK];
			filter.IsActive = true;
			filter.Property = ZString.Empty;
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals("Empty filter search should have found 4 declarations", 4, collection.Count);

			filter.Property = "1500";
			collection.Load(FilterBO.Filter);
			AssertEquals("search shuld have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration1 should be there", true, collection.Contains(declaration1));

			filter.Property = "2000";
			collection.Load(FilterBO.Filter);
			AssertEquals("should now have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration2 should be there", true, collection.Contains(declaration2));

			filter.Property = "3000";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			collection.Load(FilterBO.Filter);
			AssertEquals("search should have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration3 should be there", true, collection.Contains(declaration3));

			filter.Property = "";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			collection.Load(FilterBO.Filter);
			AssertEquals("search should have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration4 should be there", true, collection.Contains(declaration4));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			collection.Load(FilterBO.Filter);
			AssertEquals("search should have found 3 declarations", 3, collection.Count);
			AssertEquals("declaration1 should be there", true, collection.Contains(declaration1));
			AssertEquals("declaration2 should be there", true, collection.Contains(declaration2));
			AssertEquals("declaration3 should be there", true, collection.Contains(declaration3));
		}

		public void TestPortOfEntryQueryWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_SchDEntry = "1234";
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_SchDEntry = "2345";
			DeleteAllGenAddOnColumn(declaration2);

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleNkFilter)bizObj[DeclarationFilterConstants.PortOfEntry];
			AssertEquals(FilterCategories.Locations, filter.Category);
			filter.IsActive = true;
			filter.Property = "1234";
			AssertEquals("declaration1 matches filter", true, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 does not match filter", false, declaration2.MatchesFilter(bizObj.Filter));

			filter = (ModuleNkFilter)bizObj[DeclarationFilterConstants.PortOfEntry];
			filter.IsActive = true;
			filter.Property = "2345";
			AssertEquals("declaration1 does not match filter", false, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 matches filter", true, declaration2.MatchesFilter(bizObj.Filter));
		}

		public void TestExportPortQueryWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration1.US_SchDExport = "1234";
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration2.US_SchDExport = "2345";
			DeleteAllGenAddOnColumn(declaration2);

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleNkFilter)bizObj[DeclarationFilterConstants.ExportPort];
			AssertEquals(FilterCategories.Locations, filter.Category);
			filter.IsActive = true;
			filter.Property = "1234";
			AssertEquals("declaration1 matches filter", true, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 does not match filter", false, declaration2.MatchesFilter(bizObj.Filter));

			filter = (ModuleNkFilter)bizObj[DeclarationFilterConstants.ExportPort];
			filter.IsActive = true;
			filter.Property = "2345";
			AssertEquals("declaration1 does not match filter", false, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 matches filter", true, declaration2.MatchesFilter(bizObj.Filter));
		}

		public void TestPreparerDistrictPortFilterWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_PreparerDistrictPort = "1234";
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_PreparerDistrictPort = "2345";
			DeleteAllGenAddOnColumn(declaration2);

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleNkFilter)bizObj[DeclarationFilterConstants.PreparerDistrictPort];
			AssertEquals(FilterCategories.Locations, filter.Category);
			filter.IsActive = true;
			filter.Property = "1234";
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarations.Load(bizObj.Filter);
			AssertEquals("declaration1 matches filter", true, declarations.Contains(declaration1));
			AssertEquals("declaration2 does not match filter", false, declarations.Contains(declaration2));

			filter.Property = "2345";
			declarations.Load(bizObj.Filter);
			AssertEquals("declaration1 does not match filter", false, declarations.Contains(declaration1));
			AssertEquals("declaration2 matches filter", true, declarations.Contains(declaration2));
		}

		void DeleteAllGenAddOnColumn(JobDeclaration declaration)
		{
			var query = new ZQuery(GenAddOnColumnSchema.XA_ParentID, declaration.PK);
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, declaration.TablePrefix);
			Factory.Load<GenAddOnColumn>(query).DeleteAll();
		}

		public void TestDispositionCode()
		{
			var dec1 = Factory.NewWithValidTestData<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec1.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec1.US_BondType = "9";
			dec1.US_BondDispositionCode = BondDispositionCodeList.Codes.CAB;

			var dec2 = Factory.NewWithValidTestData<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec2.US_BondType = "8";
			dec2.US_BondDispositionCode = BondDispositionCodeList.Codes.CAB;

			var dec3 = Factory.NewWithValidTestData<JobDeclaration>();
			dec3.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec3.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec3.US_BondType = "9";
			dec3.US_BondDispositionCode = BondDispositionCodeList.Codes.CUB;
			dec3.US_BondDispositionCode2 = BondDispositionCodeList.Codes.CEB;

			var dec4 = Factory.NewWithValidTestData<JobDeclaration>();
			dec4.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec4.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec4.US_BondType = "9";
			dec4.US_BondDispositionCode = ZString.Empty;
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.BasicSTBDisposition];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			Assert(!dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));
			Assert(!dec3.MatchesFilter(bizObj.Filter));
			Assert(dec4.MatchesFilter(bizObj.Filter));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			Assert(dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));
			Assert(dec3.MatchesFilter(bizObj.Filter));
			Assert(!dec4.MatchesFilter(bizObj.Filter));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = BondDispositionCodeList.Codes.CAB;
			Assert(dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));
			Assert(!dec3.MatchesFilter(bizObj.Filter));

			var bizObj2 = new JobDeclarationFilterBusinessObject();
			var filter2 = (ModuleTextFilter)bizObj2[DeclarationFilterConstants.AdditionalBondDisposition];
			filter2.IsActive = true;
			filter2.Property = BondDispositionCodeList.Codes.CEB;
			Assert(!dec1.MatchesFilter(bizObj2.Filter));
			Assert(!dec2.MatchesFilter(bizObj2.Filter));
			Assert(dec3.MatchesFilter(bizObj2.Filter));
		}

		public void TestEBondMessageStatusFilter()
		{
			var dec1 = Factory.NewWithValidTestData<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec1.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec1.US_BondType = "9";
			dec1.US_InsuranceDisposition = InsuranceDispositionCodeList.Codes.SentToSurety;

			var dec2 = Factory.NewWithValidTestData<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec2.US_BondType = "8";
			dec2.US_InsuranceDisposition = InsuranceDispositionCodeList.Codes.SentToSurety;

			var dec3 = Factory.NewWithValidTestData<JobDeclaration>();
			dec3.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec3.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec3.US_BondType = "9";
			dec3.US_InsuranceDisposition = ZString.Empty;

			var dec4 = Factory.NewWithValidTestData<JobDeclaration>();
			dec4.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec4.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec4.US_BondType = "9";
			dec4.US_InsuranceDisposition = InsuranceDispositionCodeList.Codes.AcceptedByCBP;
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.EBondMessageStatus];
			filter.IsActive = true;
			filter.Property = InsuranceDispositionCodeList.Codes.SentToSurety;
			Assert(dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));
			Assert(!dec3.MatchesFilter(bizObj.Filter));
			Assert(!dec4.MatchesFilter(bizObj.Filter));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			Assert(!dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));
			Assert(dec3.MatchesFilter(bizObj.Filter));
			Assert(dec4.MatchesFilter(bizObj.Filter));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			Assert(!dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));
			Assert(dec3.MatchesFilter(bizObj.Filter));
			Assert(!dec4.MatchesFilter(bizObj.Filter));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			Assert(dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));
			Assert(!dec3.MatchesFilter(bizObj.Filter));
			Assert(dec4.MatchesFilter(bizObj.Filter));
		}

		public void TestDISStatusFilter()
		{
			var dec1 = Factory.NewWithValidTestData<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			var doc1 = dec1.DocsAndCartage.RequiredDocuments.AddNew();
			doc1.FillWithValidTestData();
			var docAddInfo1 = doc1.AddInfos.AddNew();
			docAddInfo1.FillWithValidTestData();
			docAddInfo1.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			docAddInfo1.EX_Status = Enterprise.Customs.Common.US.DIS.StatusList.Codes.AOS;

			var dec2 = Factory.NewWithValidTestData<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			var doc2 = dec2.DocsAndCartage.RequiredDocuments.AddNew();
			doc2.FillWithValidTestData();
			var docAddInfo2 = doc2.AddInfos.AddNew();
			docAddInfo2.FillWithValidTestData();
			docAddInfo2.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			docAddInfo2.EX_Status = Enterprise.Customs.Common.US.DIS.StatusList.Codes.AOS;
			var doc3 = dec2.DocsAndCartage.RequiredDocuments.AddNew();
			doc3.FillWithValidTestData();
			var docAddInfo3 = doc3.AddInfos.AddNew();
			docAddInfo3.FillWithValidTestData();
			docAddInfo3.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			docAddInfo3.EX_Status = Enterprise.Customs.Common.US.DIS.StatusList.Codes.COS;

			var dec3 = Factory.NewWithValidTestData<JobDeclaration>();
			dec3.JE_MessageType = JobMessageTypeList.Codes.Import;
			var doc4 = dec3.DocsAndCartage.RequiredDocuments.AddNew();
			doc4.FillWithValidTestData();
			var docAddInfo4 = doc4.AddInfos.AddNew();
			docAddInfo4.FillWithValidTestData();
			docAddInfo4.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			docAddInfo4.EX_Status = Enterprise.Customs.Common.US.DIS.StatusList.Codes.COS;

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.DISStatus];
			filter.IsActive = true;

			filter.Property = Enterprise.Customs.Common.US.DIS.StatusList.Codes.AOS;
			Assert(dec1.MatchesFilter(bizObj.Filter));
			Assert(dec2.MatchesFilter(bizObj.Filter));
			Assert(!dec3.MatchesFilter(bizObj.Filter));
		}

		public void TestSearchWithBIRDRef()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec1.US_BRDRefNo = "11";

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec2.US_BRDRefNo = "12";

			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_MessageType = JobMessageTypeList.Codes.Import;

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.BIRDBrokerRef];
			filter.IsActive = true;

			filter.Property = "11";
			Assert(dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));

			filter.Property = ZString.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			Assert(dec3.MatchesFilter(bizObj.Filter));
			Assert(!dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			Assert(!dec3.MatchesFilter(bizObj.Filter));
			Assert(dec1.MatchesFilter(bizObj.Filter));
			Assert(dec2.MatchesFilter(bizObj.Filter));
		}

		public void TestStatusNotificationThatRequiresActions()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec1.US_EnableENS = true;
			dec1.US_EntryFilerCode = "XJ5";

			dec1.Invoices.AddNew();
			dec1.InvoiceLines.AddNew();
			dec1.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var message = Factory.New<MQEDIMessage>();
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			message.EM_ApplicationReference = ENSStatusDispositionCodeList._1;
			message.EM_LinkedObject = dec1.ActiveEntryHeaders.EntrySummaryEntry;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec2.US_EnableENS = true;
			dec2.US_EntryFilerCode = "XJ5";

			dec2.Invoices.AddNew();
			dec2.InvoiceLines.AddNew();
			dec2.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			message2.EM_ApplicationReference = ENSStatusDispositionCodeList._7;
			message2.EM_LinkedObject = dec2.ActiveEntryHeaders.EntrySummaryEntry;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.StatusNotificationDispositionCode];
			filter.IsActive = true;
			filter.Property = ENSStatusDispositionCodeList._1;
			CombineAssertions(() =>
			{
				Assert("Dec1 for 1", dec1.MatchesFilter(bizObj.Filter));
				Assert("Dec2 for 1", !dec2.MatchesFilter(bizObj.Filter));

				filter.OrCategory = FilterOrCategory.Red;
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				var multiValueFilter = (ISupportMultiValuesFilter)filter;
				Assert("Force group filters with MultiValueQueryDelegate", multiValueFilter.CanGroup);
				var combinedValue = new List<ZString> { "1", "2", "4" };
				AssertEquals("In OrCategory", "JE_PK IN (SELECT CH_JE FROM dbo.CusEntryHeader WHERE CH_MessageType = 'ENS' and (CH_PK IN (SELECT EM_LinkUniqueID FROM dbo.EDIMessage WHERE EM_MessageType = 'UC' and EM_ApplicationCode = 'USI' and (EM_ApplicationReference like '1%' or EM_ApplicationReference like '2%' or EM_ApplicationReference like '4%'))))", multiValueFilter.GetCombinedQuery(combinedValue).LiteralTextADO);

				filter.OrCategory = FilterOrCategory.None;
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				AssertEquals("Not In OrCategory", "JE_PK IN (SELECT CH_JE FROM dbo.CusEntryHeader WHERE CH_MessageType = 'ENS' and (CH_PK IN (SELECT EM_LinkUniqueID FROM dbo.EDIMessage WHERE EM_MessageType = 'UC' and EM_ApplicationCode = 'USI' and (EM_ApplicationReference not like '1%' and EM_ApplicationReference not like '2%' and EM_ApplicationReference not like '4%'))))", multiValueFilter.GetCombinedQuery(combinedValue).LiteralTextADO);
			});
		}

		public void TestEntrySummaryActionsCompleted()
		{
			var dec1 = GetSimpleMergedDeclaration();

			var message = Factory.New<MQEDIMessage>();
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			message.EM_ApplicationReference = ENSStatusDispositionCodeList._1 + ":1234567";
			message.EM_LinkedObject = dec1.ActiveEntryHeaders.EntrySummaryEntry;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.SetToComplete();

			var dec2 = GetSimpleMergedDeclaration();

			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			message2.EM_ApplicationReference = ENSStatusDispositionCodeList._3 + ":1234567";
			message2.EM_LinkedObject = dec2.ActiveEntryHeaders.EntrySummaryEntry;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			dec1.ReCalculateENSAction();
			dec2.ReCalculateENSAction();

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.EntrySummaryActions];
			filter.IsActive = true;
			filter.Property = DeclarationFilterConstants.Incomplete;

			Assert("Dec1", !dec1.MatchesFilter(bizObj.Filter));
			AssertNotEquals(EM_ActionStatusList.Codes.Incomplete, message.EM_ActionStatus);

			Assert("Dec2", dec2.MatchesFilter(bizObj.Filter));
			AssertEquals("Incomplete", EM_ActionStatusList.Codes.Incomplete, message2.EM_ActionStatus);

			filter.Property = DeclarationFilterConstants.ALL;// do not exclude. include all!
			Assert("Dec1", dec1.MatchesFilter(bizObj.Filter));
			Assert("Dec2", dec2.MatchesFilter(bizObj.Filter));
		}

		public void TestEntrySummaryActions_CS00214544()
		{
			//action taken
			var declaration1 = GetSimpleMergedDeclaration();

			var declaration1_message1 = Factory.New<MQEDIMessage>();
			declaration1_message1.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			declaration1_message1.EM_ApplicationReference = ENSStatusDispositionCodeList._1 + ":1234567";
			declaration1_message1.EM_LinkedObject = declaration1.ActiveEntryHeaders.EntrySummaryEntry;
			declaration1_message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			declaration1_message1.SetToComplete();
			declaration1_message1.Logs.MostRecentLog.Cancel();

			//do not require action
			var declaration2 = GetSimpleMergedDeclaration();

			var declaration2_message1 = Factory.New<MQEDIMessage>();
			declaration2_message1.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			declaration2_message1.EM_ApplicationReference = ENSStatusDispositionCodeList._7 + ":1234567";
			declaration2_message1.EM_LinkedObject = declaration2.ActiveEntryHeaders.EntrySummaryEntry;
			declaration2_message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			//action taken for disposition 2 and doesn't required for code 9
			var declaration3 = GetSimpleMergedDeclaration();

			var declaration3_message1 = Factory.New<MQEDIMessage>();
			declaration3_message1.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			declaration3_message1.EM_ApplicationReference = ENSStatusDispositionCodeList._2 + ":1234567";
			declaration3_message1.EM_LinkedObject = declaration3.ActiveEntryHeaders.EntrySummaryEntry;
			declaration3_message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			declaration3_message1.SetToComplete();

			var declaration3_message2 = Factory.New<MQEDIMessage>();
			declaration3_message2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			declaration3_message2.EM_ApplicationReference = ENSStatusDispositionCodeList._8 + ":1234567";
			declaration3_message2.EM_LinkedObject = declaration3.ActiveEntryHeaders.EntrySummaryEntry;
			declaration3_message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			//action required and not taken
			var declaration4 = GetSimpleMergedDeclaration();

			var declaration4_message1 = Factory.New<MQEDIMessage>();
			declaration4_message1.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			declaration4_message1.EM_ApplicationReference = ENSStatusDispositionCodeList._2 + ":1234567";
			declaration4_message1.EM_LinkedObject = declaration4.ActiveEntryHeaders.EntrySummaryEntry;
			declaration4_message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			declaration1.ReCalculateENSAction();
			declaration2.ReCalculateENSAction();
			declaration3.ReCalculateENSAction();
			declaration4.ReCalculateENSAction();

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.EntrySummaryActions];
			filter.IsActive = true;
			filter.Property = DeclarationFilterConstants.Incomplete;

			Assert("Has ATH log, but it was Cancelled", declaration1.MatchesFilter(bizObj.Filter));
			Assert("Has not ATH log", declaration2.MatchesFilter(bizObj.Filter));
			Assert("One message has not ATH log", declaration3.MatchesFilter(bizObj.Filter));
			Assert("Has not ATH log", declaration4.MatchesFilter(bizObj.Filter));

			filter.Property = DeclarationFilterConstants.ALL;// do not exclude. include all!

			Assert(declaration1.MatchesFilter(bizObj.Filter));
			Assert(declaration2.MatchesFilter(bizObj.Filter));
			Assert(declaration3.MatchesFilter(bizObj.Filter));
			Assert(declaration4.MatchesFilter(bizObj.Filter));
		}

		public void TestSearchWithExternalBroker()
		{
			var externalBroker1 = Factory.NewWithValidTestData<OrgHeader>();
			var externalBroker2 = Factory.NewWithValidTestData<OrgHeader>();
			var depot = Factory.NewWithValidTestData<OrgHeader>();

			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec1.JE_OH_ExternalBroker = externalBroker1.PK;

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec2.JE_OH_ExternalBroker = externalBroker2.PK;
			dec2.DepotDocAddress.E2_OA_Address = depot.MainAddress.PK;

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleGuidFilter)bizObj[DeclarationFilterConstants.OrgFilterTypes.ExternalBroker];
			filter.IsActive = true;

			filter.Property = externalBroker1.PK;
			Assert(dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));
		}

		public void TestSearchWithBondNumber()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec1.US_BondProducerAccNo = "1234";
			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec2.US_BondProducerAccNo = "5678";
			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_MessageType = JobMessageTypeList.Codes.Import;

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.BondNumber];
			filter.IsActive = true;

			filter.Property = dec1.US_BondProducerAccNo;
			Assert(dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));
			Assert(!dec3.MatchesFilter(bizObj.Filter));

			filter.Property = dec2.US_BondProducerAccNo;
			Assert(!dec1.MatchesFilter(bizObj.Filter));
			Assert(dec2.MatchesFilter(bizObj.Filter));
			Assert(!dec3.MatchesFilter(bizObj.Filter));
		}

		public void TestSearchWithStatementNumber()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_DeclarationReference = "B0098";
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec1.US_EntryFilerCode = "XJ5";
			var entry1 = dec1.ActiveEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry1.EntryNumber = "123";

			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_PaymentType = "1";
			statement1.B2_StatementNumber = "1111";

			var line1 = statement1.StatementLines.AddNew();
			line1.B3_EntryNum = entry1.EntryNumber;
			line1.B3_EntryFilerCode = "XJ5";
			line1.B3_Status = StatementLineStatusList.Codes.Active;
			line1.B3_BrokerReference = "B0098";

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_DeclarationReference = "B0099";
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec2.US_EntryFilerCode = "XJ5";
			var entry2 = dec2.ActiveEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry2.EntryNumber = "456";

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_PaymentType = "1";
			statement2.B2_StatementNumber = "2222";

			var line2 = statement2.StatementLines.AddNew();
			line2.B3_EntryNum = entry2.EntryNumber;
			line2.B3_EntryFilerCode = "XJ5";
			line2.B3_Status = StatementLineStatusList.Codes.Active;
			line2.B3_BrokerReference = "B0099";

			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_DeclarationReference = "B0100";
			dec3.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec3.US_EntryFilerCode = "XJ5";
			var entry3 = dec3.ActiveEntryHeaders.AddNew();
			entry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry3.EntryNumber = "789";

			var statement3 = Factory.New<CusStatementHeader>();
			statement3.B2_PaymentType = "1";
			statement3.B2_StatementNumber = "3333";

			var line3 = statement3.StatementLines.AddNew();
			line3.B3_EntryNum = entry3.EntryNumber;
			line3.B3_EntryFilerCode = "XJ5";
			line3.B3_Status = StatementLineStatusList.Codes.Deleted;
			line3.B3_BrokerReference = "B0100";

			var dec4 = Factory.New<JobDeclaration>();
			dec4.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.StatementNo];
			filter.IsActive = true;
			filter.Property = "1111";
			Assert(dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));
			Assert(!dec3.MatchesFilter(bizObj.Filter));
			Assert(!dec4.MatchesFilter(bizObj.Filter));

			filter.Property = "2222";
			Assert(!dec1.MatchesFilter(bizObj.Filter));
			Assert(dec2.MatchesFilter(bizObj.Filter));
			Assert(!dec3.MatchesFilter(bizObj.Filter));
			Assert(!dec4.MatchesFilter(bizObj.Filter));

			filter.Property = "3333";
			Assert(!dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));
			Assert(!dec3.MatchesFilter(bizObj.Filter));
			Assert(!dec4.MatchesFilter(bizObj.Filter));

			filter.Clear();
			Assert(dec1.MatchesFilter(bizObj.Filter));
			Assert(dec2.MatchesFilter(bizObj.Filter));
			Assert(dec3.MatchesFilter(bizObj.Filter));
			Assert(dec4.MatchesFilter(bizObj.Filter));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			Assert(!dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));
			Assert("Matches blank filter, because statement deleted for this declaration", dec3.MatchesFilter(bizObj.Filter));
			Assert(dec4.MatchesFilter(bizObj.Filter));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			Assert(dec1.MatchesFilter(bizObj.Filter));
			Assert(dec2.MatchesFilter(bizObj.Filter));
			Assert("Statement is blank because line is deleted for this declaration", !dec3.MatchesFilter(bizObj.Filter));
			Assert(!dec4.MatchesFilter(bizObj.Filter));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "2";
			Assert(!dec1.MatchesFilter(bizObj.Filter));
			Assert(dec2.MatchesFilter(bizObj.Filter));
			Assert(!dec3.MatchesFilter(bizObj.Filter));
			Assert(!dec4.MatchesFilter(bizObj.Filter));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			filter.Property = "3";
			Assert(!dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));
			Assert("Statement Line is deleted", !dec3.MatchesFilter(bizObj.Filter));
			Assert(!dec4.MatchesFilter(bizObj.Filter));
		}

		public void TestShipmentReferenceNoSearch()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "123";
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;

			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			entry.CH_BGMReference = "456";

			var declaration1 = Factory.New<JobDeclaration>();
			entry = declaration1.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "123";

			var declaration3 = Factory.New<JobDeclaration>();
			var entry3 = declaration3.CustomsEntryHeaders.AddNew();
			entry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.ShipperReferenceNumber];
			filter.IsActive = true;
			filter.Property = "123";

			Assert(declaration.MatchesFilter(bizObj.Filter));
			Assert(!declaration1.MatchesFilter(bizObj.Filter));

			filter.Property = "456";
			Assert(declaration.MatchesFilter(bizObj.Filter));
			Assert(!declaration1.MatchesFilter(bizObj.Filter));

			filter.Property = ZString.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			Assert(!declaration.MatchesFilter(bizObj.Filter));
			Assert(!declaration1.MatchesFilter(bizObj.Filter));
			Assert(declaration3.MatchesFilter(bizObj.Filter));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			Assert(declaration.MatchesFilter(bizObj.Filter));
			Assert(!declaration1.MatchesFilter(bizObj.Filter));
			Assert(!declaration3.MatchesFilter(bizObj.Filter));
		}

		public void TestSearchWithFilerCode()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec1.US_EntryFilerCode = "XJ5";

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec2.US_EntryFilerCode = "ABC";

			var dec3 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.Filer];
			filter.IsActive = true;

			filter.Property = "ABC";
			Assert(!dec1.MatchesFilter(bizObj.Filter));
			Assert(dec2.MatchesFilter(bizObj.Filter));

			filter.Property = "XJ5";
			Assert(dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));

			filter.Property = "";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			Assert(!dec1.MatchesFilter(bizObj.Filter));
			Assert(!dec2.MatchesFilter(bizObj.Filter));
			Assert(dec3.MatchesFilter(bizObj.Filter));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			Assert(dec1.MatchesFilter(bizObj.Filter));
			Assert(dec2.MatchesFilter(bizObj.Filter));
			Assert(!dec3.MatchesFilter(bizObj.Filter));
		}

		public void TestGetElectronicInvoiceStatusQuery()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader1_forDec1 = dec1.Invoices.AddNew();
			invoiceHeader1_forDec1.JZ_MessageStatus = EIStatusForTest1;
			var invoiceHeader2_forDec1 = dec1.Invoices.AddNew();
			invoiceHeader2_forDec1.JZ_MessageStatus = EIStatusForTest1;

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader1_forDec2 = dec2.Invoices.AddNew();
			invoiceHeader1_forDec2.JZ_MessageStatus = EIStatusForTest1;
			var invoiceHeader2_forDec2 = dec2.Invoices.AddNew();
			invoiceHeader2_forDec2.JZ_MessageStatus = EIStatusForTest2;

			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader1_forDec3 = dec3.Invoices.AddNew();
			invoiceHeader1_forDec3.JZ_MessageStatus = MessageStatusListEI.Codes.NotSent;
			var invoiceHeader2_forDec3 = dec3.Invoices.AddNew();
			invoiceHeader2_forDec3.JZ_MessageStatus = EIStatusForTest2;

			var dec4 = Factory.New<JobDeclaration>();
			dec4.JE_MessageType = JobMessageTypeList.Codes.Import;

			var dec5 = Factory.New<JobDeclaration>();
			dec5.JE_MessageType = JobMessageTypeList.Codes.Export;

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.ElectronicInvoiceStatus];
			filter.IsActive = true;

			filter.Property = EIStatusForTest1;
			AssertEquals(true, dec1.MatchesFilter(bizObj.Filter));
			AssertEquals(true, dec2.MatchesFilter(bizObj.Filter));
			AssertEquals(false, dec3.MatchesFilter(bizObj.Filter));
			AssertEquals(false, dec4.MatchesFilter(bizObj.Filter));
			AssertEquals("Export Job should not show in filter", false, dec5.MatchesFilter(bizObj.Filter));

			filter.Property = EIStatusForTest2;
			AssertEquals(false, dec1.MatchesFilter(bizObj.Filter));
			AssertEquals(true, dec2.MatchesFilter(bizObj.Filter));
			AssertEquals(true, dec3.MatchesFilter(bizObj.Filter));
			AssertEquals(false, dec4.MatchesFilter(bizObj.Filter));
			AssertEquals("Export Job should not show in filter", false, dec5.MatchesFilter(bizObj.Filter));

			filter.Property = DeclarationFilterConstants.MessageStatus.NotSentForFilter;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			AssertEquals(false, dec1.MatchesFilter(bizObj.Filter));
			AssertEquals(false, dec2.MatchesFilter(bizObj.Filter));
			AssertEquals("no result query for Not Sent unless using exact", false, dec3.MatchesFilter(bizObj.Filter));
			AssertEquals(false, dec4.MatchesFilter(bizObj.Filter));
			AssertEquals("Export Job should not show in filter", false, dec5.MatchesFilter(bizObj.Filter));

			filter.Property = UnusedEIStatusForTest;
			AssertEquals(false, dec1.MatchesFilter(bizObj.Filter));
			AssertEquals(false, dec2.MatchesFilter(bizObj.Filter));
			AssertEquals(false, dec3.MatchesFilter(bizObj.Filter));
			AssertEquals(false, dec4.MatchesFilter(bizObj.Filter));
			AssertEquals("Export Job should not show in filter", false, dec5.MatchesFilter(bizObj.Filter));

			filter.Property = DeclarationFilterConstants.MessageStatus.NotSentForFilter;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			AssertEquals(false, dec1.MatchesFilter(bizObj.Filter));
			AssertEquals(false, dec2.MatchesFilter(bizObj.Filter));
			AssertEquals(true, dec3.MatchesFilter(bizObj.Filter));
			AssertEquals("Import declarations without invoices should also show for Not Sent", true, dec4.MatchesFilter(bizObj.Filter));
			AssertEquals("Export Job should not show in filter", false, dec5.MatchesFilter(bizObj.Filter));
		}

		public void TestElectronicInvoiceStatusForNotSent()
		{
			CreateTestDecsWithAndWithoutStatuses();

			//create decs with multiple EI status
			var dec1EISent = Factory.New<JobDeclaration>();
			dec1EISent.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader1_forDec1 = dec1EISent.Invoices.AddNew();
			invoiceHeader1_forDec1.JZ_MessageStatus = MessageStatusListEI.Codes.ClearElectronicInvoiceOriginal;
			var invoiceHeader2_forDec1 = dec1EISent.Invoices.AddNew();
			invoiceHeader2_forDec1.JZ_MessageStatus = MessageStatusListEI.Codes.AwaitingElectronicInvoiceReplace;

			var dec2EIMultipleNotSent = Factory.New<JobDeclaration>();
			dec2EIMultipleNotSent.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader1_forDec2 = dec2EIMultipleNotSent.Invoices.AddNew();
			invoiceHeader1_forDec2.JZ_MessageStatus = MessageStatusListEI.Codes.ErrorElectronicInvoiceOriginal;
			var invoiceHeader2_forDec2 = dec2EIMultipleNotSent.Invoices.AddNew();
			invoiceHeader2_forDec2.JZ_MessageStatus = "";

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var statusFilter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.ElectronicInvoiceStatus];
			statusFilter.IsActive = true;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should have found all created test declarations", 16, coll.Count);

			statusFilter.Property = DeclarationFilterConstants.MessageStatus.NotSentForFilter;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should find 12 Import declarations - 8 that do not have an invoice or invoice status & 1 that has 2 EI's one of which is still not sent and others for SE", 12, coll.Count);

			statusFilter.Property = MessageStatusListEI.Codes.ClearElectronicInvoiceOriginal;
			statusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("When using 'Not Equals', jobs with MUL status should show even where 1 or more of those contained EI's has the status not being searched", 13, coll.Count);
			AssertEquals(true, dec1EISent.MatchesFilter(bizObj.Filter));
			AssertEquals(true, dec2EIMultipleNotSent.MatchesFilter(bizObj.Filter));
		}

		public void TestNotReconciledQueryCSSScenario()
		{
			// declaration with NAFTA & another reconciliation type. When other type is reconciled, job should still appear in filter list if searching for Not Reconciled NAFTA jobs (& vice versa)
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_OtherReconIndicator = ReconIssueCodeList.Codes.ValueRecon;
			declaration1.US_NAFTAReconIndicator = true;
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var reconDec1 = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec1.US_IssueCode = ReconIssueCodeList.Codes.ValueRecon;
			var reconEntry1 = reconDec1.OriginalEntries.AddNew();
			reconEntry1.CH_CH_OriginalEntry = entry1.PK; // VL reconciled
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.NotYetReconciled];
			filter.IsActive = true;
			filter.Property = ReconIssueCodeList.Codes.ValueRecon;
			AssertEquals("declaration1 has been reconciled for 'VL' Recon. Issue", false, declaration1.MatchesFilter(bizObj.Filter));

			filter.IsActive = true;
			filter.Property = ReconIssueCodeList.Codes.FTA;
			AssertEquals("declaration1 has not been reconciled for NAFTA Recon - it should be found as 'not reconciled' in this search.", true, declaration1.MatchesFilter(bizObj.Filter));

			var reconDec1ForNafta = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec1ForNafta.US_IssueCode = ReconIssueCodeList.Codes.FTA;
			var reconEntry1A = reconDec1ForNafta.OriginalEntries.AddNew();
			reconEntry1A.CH_CH_OriginalEntry = entry1.PK; // NAFTA now also reconciled
			Factory.Save();
			AssertEquals("declaration1 has now been reconciled for NAFTA Recon as well - it should not therefore be found as 'not reconciled' in this search.", false, declaration1.MatchesFilter(bizObj.Filter));

			// check filter with multiple jobs....
			var declaration2 = Factory.New<JobDeclaration>(); // not reconciled: VL flagged
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_OtherReconIndicator = ReconIssueCodeList.Codes.ValueRecon;
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var declaration3 = Factory.New<JobDeclaration>(); // not reconciled: C9 & NAFTA flagged
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.US_OtherReconIndicator = ReconIssueCodeList.Codes.Class9802Recon;
			declaration3.US_NAFTAReconIndicator = true;
			var entry3 = declaration3.CustomsEntryHeaders.AddNew();
			entry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var declaration4 = Factory.New<JobDeclaration>(); // not reconciled: NAFTA flagged
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration4.US_NAFTAReconIndicator = true;
			var entry4 = declaration4.CustomsEntryHeaders.AddNew();
			entry4.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var declaration5 = Factory.New<JobDeclaration>(); // reconciled: C9 flagged
			declaration5.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration5.US_OtherReconIndicator = ReconIssueCodeList.Codes.Class9802Recon;
			var entry5 = declaration5.CustomsEntryHeaders.AddNew();
			entry5.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var reconDec5 = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec5.US_IssueCode = ReconIssueCodeList.Codes.Class9802Recon;
			var reconEntry5 = reconDec5.OriginalEntries.AddNew();
			reconEntry5.CH_CH_OriginalEntry = entry5.PK; // C9 reconciled

			var declaration6 = Factory.New<JobDeclaration>(); // NON reconciled job
			declaration5.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry6 = declaration6.CustomsEntryHeaders.AddNew();
			entry6.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			Factory.Save();

			filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.NotYetReconciled];
			filter.IsActive = true;
			filter.Property = ReconIssueCodeList.Codes.ValueRecon;
			AssertEquals("declaration1 has been reconciled for 'VL' Recon. Issue", false, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 has not been reconciled for 'VL' Recon. Issue - should be returned in filtered list", true, declaration2.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration3 is not required for VL recon", false, declaration3.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration4 is not required for VL recon", false, declaration4.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration5 is not required for VL recon", false, declaration5.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration6 is non recon dec", false, declaration6.MatchesFilter(bizObj.Filter));

			filter.Property = ReconIssueCodeList.Codes.Class9802Recon;
			AssertEquals("declaration1 is not required for C9 recon", false, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 is not required for C9 recon", false, declaration2.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration3 has not been reconciled for 'C9' Recon. Issue - should be returned in filtered list", true, declaration3.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration4 is not required for C9 recon", false, declaration4.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration5 has  been reconciled for 'C9' Recon. Issue", false, declaration5.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration6 is non recon dec", false, declaration6.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration6 is non recon dec", false, declaration6.MatchesFilter(bizObj.Filter));

			filter.Property = ReconIssueCodeList.Codes.FTA;
			AssertEquals("declaration1 has been reconciled for 'NF' Recon. Issue", false, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 is not required for NF recon", false, declaration2.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration3 has not been reconciled for 'NF' Recon. Issue - should be returned in filtered list", true, declaration3.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration4 has not been reconciled for 'NF' Recon. Issue - should be returned in filtered list", true, declaration4.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration5 is not required for NF recon", false, declaration5.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration6 is non recon dec", false, declaration6.MatchesFilter(bizObj.Filter));
		}

		public void TestSPIAuditQuery()
		{
			GetDeclarationsForAuditFiltersTesting();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleFlagsFilter)bizObj[DeclarationFilterConstants.SPINotApplicableAudit];
			filter.IsActive = true;
			filter.Property1 = true;//audit required

			AssertEquals("Audit required: declaration1 has been not audited", true, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("Audit required: declaration2 is export", false, declaration2.MatchesFilter(bizObj.Filter));
			AssertEquals("Audit required: declaration3 has been audited", false, declaration3.MatchesFilter(bizObj.Filter));
			AssertEquals("Audit required: declaration4 does not have invoice lines with SPI N/A", false, declaration4.MatchesFilter(bizObj.Filter));
			AssertEquals("Audit required: declaration5 has been not audited, but deleted", false, declaration5.MatchesFilter(bizObj.Filter));

			filter.Property0 = true;//audited
			AssertEquals("PreCondition", false, filter.Property1);

			AssertEquals("Audited: declaration1 has been not audited", false, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("Audited: declaration2 is export", false, declaration2.MatchesFilter(bizObj.Filter));
			AssertEquals("Audited: declaration3 has been audited", true, declaration3.MatchesFilter(bizObj.Filter));
			AssertEquals("Audited: declaration4 does not have invoice lines with SPI N/A", false, declaration4.MatchesFilter(bizObj.Filter));
			AssertEquals("Audited: declaration5 has been not audited, but deleted", false, declaration5.MatchesFilter(bizObj.Filter));
		}

		public void TestSPIAuditDateQuery()
		{
			GetDeclarationsForAuditFiltersTesting();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleDateFilter)bizObj[DeclarationFilterConstants.SPINotApplicableAuditDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.BrettsBirthday;
			filter.Property2 = ZDateTime.BrettsBirthday.AddMonths(1);

			AssertEquals("declaration3", true, declaration3.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2", false, declaration4.MatchesFilter(bizObj.Filter));

			bizObj = new JobDeclarationFilterBusinessObject();
			filter = (ModuleDateFilter)bizObj[DeclarationFilterConstants.SPINotApplicableAuditDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property2 = ZDateTime.Empty;
			AssertEquals("declaration3", true, declaration3.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration4", true, declaration4.MatchesFilter(bizObj.Filter));

			bizObj = new JobDeclarationFilterBusinessObject();
			filter = (ModuleDateFilter)bizObj[DeclarationFilterConstants.SPINotApplicableAuditDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			AssertEquals("declaration1", true, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 is Export Declaration", false, declaration2.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration4", true, declaration4.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration5", true, declaration5.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration6", true, declaration6.MatchesFilter(bizObj.Filter));

			bizObj = new JobDeclarationFilterBusinessObject();
			filter = (ModuleDateFilter)bizObj[DeclarationFilterConstants.SPINotApplicableAuditDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			AssertEquals("declaration3", true, declaration3.MatchesFilter(bizObj.Filter));
		}

		public void TestRRPFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_SchDEntry = "1234";
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.Invoices.AddNew();
			declaration1.InvoiceLines.AddNew().JI_AddInfo = "UC_NKCountryOfOrigin=KR*SPI=N/A*SecondarySPI=Z*FDAIndicator=C";
			declaration1.Logs.AddNew(Events.RecordAudited, Enterprise.ZArchitecture.Business.Internal.BusinessObjectLogger.PrefixIndicator + Customs.US.Business.AuditFieldsList.Codes.FDA);
			var entry = declaration1.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			entry.CH_Status = ImportMessageStatusList.Codes.ReplaceRequestPending;
			Factory.Save();

			CreateTestDecsWithStatus();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var statusFilter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.CargoReleaseStatus];
			statusFilter.IsActive = true;
			statusFilter.Property = ImportMessageStatusList.Codes.ReplaceRequestPending;
			AssertEquals("declaration1 matches filter", true, declaration1.MatchesFilter(bizObj.Filter));

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should have found two declarations", 1, coll.Count);
			AssertEquals("Contains CRL Dec1 with RRP status", true, coll.Contains(declaration1));
		}

		public void TestFDAAuditQuery()
		{
			GetDeclarationsForAuditFiltersTesting();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleFlagsFilter)bizObj[DeclarationFilterConstants.FDANotApplicableAudit];
			filter.IsActive = true;
			filter.Property1 = true;//audit required

			AssertEquals("Audit required: declaration1 does not have invoice lines with FDA Disclaimed", false, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("Audit required: declaration2 is export", false, declaration2.MatchesFilter(bizObj.Filter));
			AssertEquals("Audit required: declaration3 does not have invoice lines with FDA Disclaimed", false, declaration3.MatchesFilter(bizObj.Filter));

			AssertEquals("Audit required: declaration4 has been audited", false, declaration4.MatchesFilter(bizObj.Filter));
			AssertEquals("Audit required: declaration5 has been audited, but deleted", false, declaration5.MatchesFilter(bizObj.Filter));
			AssertEquals("Audit required: declaration6 has been not audited", true, declaration6.MatchesFilter(bizObj.Filter));

			filter.Property0 = true;//audited
			AssertEquals("PreCondition", false, filter.Property1);

			AssertEquals("Audited: declaration1 does not have invoice lines with FDA Disclaimed", false, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("Audited: declaration2 is export", false, declaration2.MatchesFilter(bizObj.Filter));
			AssertEquals("Audited: declaration3 does not have invoice lines with FDA Disclaimed", false, declaration3.MatchesFilter(bizObj.Filter));
			AssertEquals("Audited: declaration4 has been audited", true, declaration4.MatchesFilter(bizObj.Filter));
			AssertEquals("Audited: declaration5 has been audited, but deleted", true, declaration5.MatchesFilter(bizObj.Filter));
			AssertEquals("Audited: declaration6 has been not audited", false, declaration6.MatchesFilter(bizObj.Filter));
		}

		public void TestFDAAuditDateQuery()
		{
			GetDeclarationsForAuditFiltersTesting();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleDateFilter)bizObj[DeclarationFilterConstants.FDANotApplicableAuditDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.BrettsBirthday;
			filter.Property2 = ZDateTime.BrettsBirthday.AddYears(2);

			AssertEquals("declaration3", false, declaration3.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration4", true, declaration4.MatchesFilter(bizObj.Filter));

			bizObj = new JobDeclarationFilterBusinessObject();
			filter = (ModuleDateFilter)bizObj[DeclarationFilterConstants.FDANotApplicableAuditDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property2 = ZDateTime.Empty;
			AssertEquals("declaration1", true, declaration3.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2", true, declaration4.MatchesFilter(bizObj.Filter));

			bizObj = new JobDeclarationFilterBusinessObject();
			filter = (ModuleDateFilter)bizObj[DeclarationFilterConstants.FDANotApplicableAuditDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			AssertEquals("declaration4", true, declaration4.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration5", true, declaration5.MatchesFilter(bizObj.Filter));

			bizObj = new JobDeclarationFilterBusinessObject();
			filter = (ModuleDateFilter)bizObj[DeclarationFilterConstants.FDANotApplicableAuditDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			AssertEquals("declaration1", true, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 is Export Declaration", false, declaration2.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration3", true, declaration3.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration6", true, declaration6.MatchesFilter(bizObj.Filter));
		}

		public void TestCWOAuditQuery()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.CustomsEntryHeaders[0].CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_EnableENS = true;
			declaration2.US_EntryFilerCode = "XJ5";
			declaration2.Invoices.AddNew();
			declaration2.InvoiceLines.AddNew();
			declaration2.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration2.CustomsEntryHeaders[0].CH_Status = ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithCensusWarnings;

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.US_EnableENS = true;
			declaration3.US_EntryFilerCode = "XJ5";
			declaration3.Invoices.AddNew();
			declaration3.InvoiceLines.AddNew();
			declaration3.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration3.CustomsEntryHeaders[0].CH_Status = ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithCensusWarnings;
			declaration3.Logs.AddNew(Events.RecordAudited, Enterprise.ZArchitecture.Business.Internal.BusinessObjectLogger.PrefixIndicator + Customs.US.Business.AuditFieldsList.Codes.CensusWarning, ZDateTime.BrettsBirthday.AddYears(1).ToOffset());
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleFlagsFilter)bizObj[DeclarationFilterConstants.CWNotAuditedAudit];
			filter.IsActive = true;
			filter.Property1 = true;//audit required

			AssertEquals("declaration", false, declaration.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2", true, declaration2.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration3", false, declaration3.MatchesFilter(bizObj.Filter));

			filter.Property0 = true;// audited
			AssertEquals("declaration", false, declaration.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2", false, declaration2.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration3", true, declaration3.MatchesFilter(bizObj.Filter));
		}

		public void TestCWOAuditDateQuery()
		{
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_EnableENS = true;
			declaration2.US_EntryFilerCode = "XJ5";
			declaration2.Invoices.AddNew();
			declaration2.InvoiceLines.AddNew();
			declaration2.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration2.CustomsEntryHeaders[0].CH_Status = ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithCensusWarnings;
			declaration2.Logs.AddNew(Events.RecordAudited, Enterprise.ZArchitecture.Business.Internal.BusinessObjectLogger.PrefixIndicator + Customs.US.Business.AuditFieldsList.Codes.CensusWarning, ZDateTime.BrettsBirthday.ToOffset());

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.US_EnableENS = true;
			declaration3.US_EntryFilerCode = "XJ5";
			declaration3.Invoices.AddNew();
			declaration3.InvoiceLines.AddNew();
			declaration3.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration3.CustomsEntryHeaders[0].CH_Status = ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithCensusWarnings;
			declaration3.Logs.AddNew(Events.RecordAudited, Enterprise.ZArchitecture.Business.Internal.BusinessObjectLogger.PrefixIndicator + Customs.US.Business.AuditFieldsList.Codes.CensusWarning, ZDateTime.BrettsBirthday.AddYears(1).ToOffset());
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleDateFilter)bizObj[DeclarationFilterConstants.CWOAuditDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.BrettsBirthday.AddDays(1);
			filter.Property2 = ZDateTime.BrettsBirthday.AddYears(2);

			AssertEquals("declaration2", false, declaration2.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration3", true, declaration3.MatchesFilter(bizObj.Filter));
		}

		public void TestTIBClosedQuery()
		{
			GetDeclarationsForAuditFiltersTesting();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleFlagsFilter)bizObj[DeclarationFilterConstants.TIBClosed];
			filter.IsActive = true;
			filter.Property1 = true;//close required

			AssertEquals("declaration1 should be closed", true, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration4 already closed", false, declaration4.MatchesFilter(bizObj.Filter));

			filter.Property0 = true;//closed
			AssertEquals("PreCondition", false, filter.Property1);

			AssertEquals("declaration1 not closed", false, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration4 already closed", true, declaration4.MatchesFilter(bizObj.Filter));

			//all declaration shown
			filter.Property0 = false;
			filter.Property1 = false;
			AssertEquals("declaration1", true, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2", true, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration3", true, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration4", true, declaration4.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration5", true, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration6", true, declaration1.MatchesFilter(bizObj.Filter));
		}

		public void TestTIBClosedDateQuery()
		{
			GetDeclarationsForAuditFiltersTesting();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleDateFilter)bizObj[DeclarationFilterConstants.TIBClosedDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.BrettsBirthday;
			filter.Property2 = ZDateTime.BrettsBirthday.AddMonths(1);

			AssertEquals("declaration4", true, declaration4.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration1", false, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2", false, declaration2.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration3", false, declaration3.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration5", false, declaration5.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration6", false, declaration6.MatchesFilter(bizObj.Filter));

			bizObj = new JobDeclarationFilterBusinessObject();
			filter = (ModuleDateFilter)bizObj[DeclarationFilterConstants.TIBClosedDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			AssertEquals("declaration4", true, declaration4.MatchesFilter(bizObj.Filter));

			bizObj = new JobDeclarationFilterBusinessObject();
			filter = (ModuleDateFilter)bizObj[DeclarationFilterConstants.TIBClosedDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;

			AssertEquals("declaration1", true, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 is Export Declaration", false, declaration2.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration3", true, declaration3.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration5", true, declaration5.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration6", true, declaration6.MatchesFilter(bizObj.Filter));
		}

		public void TestImporterOfRecordQuery()
		{
			var importerOfRecord = Factory.New<OrgHeader>();
			importerOfRecord.FillWithValidTestData();

			var importerOfRecord2 = Factory.New<OrgHeader>();
			importerOfRecord2.FillWithValidTestData();

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.IOROrgPK = importerOfRecord.PK;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.IOROrgPK = importerOfRecord2.PK;

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleGuidFilter)bizObj[DeclarationFilterConstants.ImporterOfRecord];
			filter.IsActive = true;
			filter.Property = importerOfRecord.PK;
			AssertEquals("declaration1 matches filter", true, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 does not match filter", false, declaration2.MatchesFilter(bizObj.Filter));

			filter = (ModuleGuidFilter)bizObj[DeclarationFilterConstants.ImporterOfRecord];
			filter.IsActive = true;
			filter.Property = importerOfRecord2.PK;
			AssertEquals("declaration1 does not match filter", false, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 matches filter", true, declaration2.MatchesFilter(bizObj.Filter));
		}

		public void TestReconIssueIndicatorQuery()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_OtherReconIndicator = ReconIssueCodeList.Codes.ValueRecon;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_OtherReconIndicator = ReconIssueCodeList.Codes.ValueClassRecon;

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.ReconIssue];
			filter.IsActive = true;
			filter.Property = ReconIssueCodeList.Codes.ValueRecon;
			AssertEquals("declaration1 matches filter", true, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 does not match filter", false, declaration2.MatchesFilter(bizObj.Filter));

			filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.ReconIssue];
			filter.IsActive = true;
			filter.Property = ReconIssueCodeList.Codes.ValueClassRecon;
			AssertEquals("declaration1 does not match filter", false, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 matches filter", true, declaration2.MatchesFilter(bizObj.Filter));
		}

		public void TestNAFTAReconIssueIndicatorQuery()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_NAFTAReconIndicator = true;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleFlagsFilter)bizObj[DeclarationFilterConstants.FTAReconIndicator];
			filter.IsActive = true;
			filter.Property0 = true;
			AssertEquals("declaration1 matches filter", true, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 does not match filter", false, declaration2.MatchesFilter(bizObj.Filter));

			filter = (ModuleFlagsFilter)bizObj[DeclarationFilterConstants.FTAReconIndicator];
			filter.IsActive = true;
			filter.Property0 = false;
			AssertEquals("declaration1 does not match filter", false, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 matches filter", true, declaration2.MatchesFilter(bizObj.Filter));
		}

		public void TestExcludeIORFilingTheirOwnRecQuery()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_FileTheirOwnRecon = true;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleFlagsFilter)bizObj[DeclarationFilterConstants.ExcludeIORFilingTheirOwnRec];
			filter.IsActive = true;
			filter.Property0 = true;
			AssertEquals("declaration1 does not match filter", false, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 matches filter", true, declaration2.MatchesFilter(bizObj.Filter));

			filter = (ModuleFlagsFilter)bizObj[DeclarationFilterConstants.ExcludeIORFilingTheirOwnRec];
			filter.IsActive = true;
			filter.Property0 = false;
			AssertEquals("declaration1 matches filter", true, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 matches filter", true, declaration2.MatchesFilter(bizObj.Filter));
		}

		public void TestPortOfEntryQuery()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_SchDEntry = "1234";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_SchDEntry = "2345";

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Recon;
			declaration3.US_SchDEntry = "2345";

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleNkFilter)bizObj[DeclarationFilterConstants.PortOfEntry];
			AssertEquals(FilterCategories.Locations, filter.Category);
			filter.IsActive = true;
			filter.Property = "1234";
			AssertEquals("declaration1 matches filter", true, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 does not match filter", false, declaration2.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration3 does not match filter", false, declaration3.MatchesFilter(bizObj.Filter));

			filter = (ModuleNkFilter)bizObj[DeclarationFilterConstants.PortOfEntry];
			filter.IsActive = true;
			filter.Property = "2345";
			AssertEquals("declaration1 does not match filter", false, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 matches filter", true, declaration2.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration3 matches filter", true, declaration3.MatchesFilter(bizObj.Filter));
		}

		public void TestExportPortQuery()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration1.US_SchDExport = "1234";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration2.US_SchDExport = "2345";

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleNkFilter)bizObj[DeclarationFilterConstants.ExportPort];
			AssertEquals(FilterCategories.Locations, filter.Category);
			filter.IsActive = true;
			filter.Property = "1234";
			AssertEquals("declaration1 matches filter", true, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 does not match filter", false, declaration2.MatchesFilter(bizObj.Filter));

			filter = (ModuleNkFilter)bizObj[DeclarationFilterConstants.ExportPort];
			filter.IsActive = true;
			filter.Property = "2345";
			AssertEquals("declaration1 does not match filter", false, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 matches filter", true, declaration2.MatchesFilter(bizObj.Filter));
		}

		public void TestEntryModeQuery()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_EntryMode = EntryModeList.Codes.RLF;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_EntryMode = EntryModeList.Codes.Paired;

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.EntryMode];
			AssertEquals(FilterCategories.ModesAndTypes, filter.Category);
			filter.IsActive = true;
			filter.Property = EntryModeList.Codes.RLF;
			AssertEquals("declaration1 matches filter", true, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 does not match filter", false, declaration2.MatchesFilter(bizObj.Filter));

			filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.EntryMode];
			filter.IsActive = true;
			filter.Property = EntryModeList.Codes.Paired;
			AssertEquals("declaration1 does not match filter", false, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 matches filter", true, declaration2.MatchesFilter(bizObj.Filter));
		}

		public void TestEntryStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_AgentsReference = "~";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_EntryStatus = ImportEntryStatusList.Codes.CRF;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_AgentsReference = "~";
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_EntryStatus = ImportEntryStatusList.Codes.CRN;

			var entry3 = declaration2.CustomsEntryHeaders.AddNew();
			entry3.CH_EntryStatus = ImportEntryStatusList.Codes.CRF;

			declaration.JE_AgentsReference = "~1";
			declaration2.JE_AgentsReference = "~2";
			Factory.Save();

			JobDeclarationFilterBusinessObject bizObj = new JobDeclarationFilterBusinessObject();
			var entryStatusFilter = (EntryStatusFilter)bizObj[FilterBO.EntryStatusText];
			entryStatusFilter.IsActive = true;
			entryStatusFilter.Property = ImportEntryStatusList.Codes.CRN;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			query.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_AgentsReference, SQLComparisonOperator.StartsWith, "~");
			coll.Load(query);
			//both declarations should be there because filter is empty. EntryStatus invisible for US.
			AssertEquals("declaration1 should be there", true, coll.Contains(declaration));
			AssertEquals("declaration2 should be there", true, coll.Contains(declaration2));

			entryStatusFilter.Property = ImportEntryStatusList.Codes.CRF;
			query = bizObj.Filter;
			query.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_AgentsReference, SQLComparisonOperator.StartsWith, "~");
			coll.Load(query);
			AssertEquals("declaration1 should be there", true, coll.Contains(declaration));
			AssertEquals("declaration2 should be there", true, coll.Contains(declaration2));
		}

		public override void TestJE_EntryStatusFilterProperties()
		{
			var entryStatusFilter = (EntryStatusFilter)filterBO[filterBO.EntryStatusText];
			entryStatusFilter.IsActive = true;
			AssertEquals(JobDeclaration.Schema.JE_EntryStatusMaxLength, entryStatusFilter.MaxLength);
			AssertEquals(false, entryStatusFilter.ShowComparisonOperator);
			AssertEquals(false, entryStatusFilter.ShowFilterType);
		}

		public void TestFDAMsgStatusFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var declaration2 = Factory.New<JobDeclaration>();
			var declaration3 = Factory.New<JobDeclaration>();
			var declaration4 = Factory.New<JobDeclaration>();
			var declaration5 = Factory.New<JobDeclaration>();

			declaration1.FDAMsgStatus = FDAStatusList.Codes.ACC;
			declaration2.FDAMsgStatus = FDAStatusList.Codes.ERR;
			declaration3.FDAMsgStatus = FDAStatusList.Codes.ACC;
			declaration4.FDAMsgStatus = ZString.Empty;
			declaration5.FDAMsgStatus = FDAStatusList.Codes.UNK;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.FDAMsgStatus];
			filter.IsActive = true;
			filter.Property = ZString.Empty;

			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals("Empty search should have found all 5 declarations", 5, collection.Count);
			AssertEquals("declaration4 (without FDA Status) should be in list", true, collection.Contains(declaration4));

			filter.Property = FDAStatusList.Codes.ERR;
			collection.Load(FilterBO.Filter);
			AssertEquals("FDA Status search for ERR should have found only 1 declaration", 1, collection.Count);
			AssertEquals("declaration2 should be returned", true, collection.Contains(declaration2));

			filter.Property = FDAStatusList.Codes.ACC;
			collection.Load(FilterBO.Filter);
			AssertEquals("FDA Status search for ACC should have found 2 declarations", 2, collection.Count);
			AssertEquals("declaration1 should be returned", true, collection.Contains(declaration1));
			AssertEquals("declaration3 should be returned", true, collection.Contains(declaration3));

			filter.Property = FDAStatusList.Codes.UNK;
			collection.Load(FilterBO.Filter);
			AssertEquals("FDA Status search for UNK should have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration5 should be returned", true, collection.Contains(declaration5));
		}

		public void TestFDAStatusFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var declaration2 = Factory.New<JobDeclaration>();
			var declaration3 = Factory.New<JobDeclaration>();
			var declaration4 = Factory.New<JobDeclaration>();
			var declaration5 = Factory.New<JobDeclaration>();

			declaration1.FDAStatus = FDAEntryLevelDispositionCodeList.Codes._01;
			declaration2.FDAStatus = FDAEntryLevelDispositionCodeList.Codes._06;
			declaration3.FDAStatus = FDAEntryLevelDispositionCodeList.Codes._06;
			declaration5.FDAStatus = FDAEntryLevelDispositionCodeList.Codes._02;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.FDAStatus];
			filter.IsActive = true;
			filter.Property = ZString.Empty;

			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals("Empty search should have found all 5 declarations", 5, collection.Count);
			AssertEquals("declaration4 (without FDA Status) should be in list", true, collection.Contains(declaration4));

			filter.Property = FDAEntryLevelDispositionCodeList.Codes._01;
			collection.Load(FilterBO.Filter);
			AssertEquals("FDA Status search for 'FDA REVIEW' should have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration1 should be returned", true, collection.Contains(declaration1));

			filter.Property = FDAEntryLevelDispositionCodeList.Codes._06;
			collection.Load(FilterBO.Filter);
			AssertEquals("FDA Status search for 'FDA MAY PROCEED' should have found 2 declarations", 2, collection.Count);
			AssertEquals("declaration2 should be returned", true, collection.Contains(declaration2));
			AssertEquals("declaration3 should be returned", true, collection.Contains(declaration3));

			filter.Property = FDAEntryLevelDispositionCodeList.Codes._02;
			collection.Load(FilterBO.Filter);
			AssertEquals("FDA Status search for 'FDA HOLD' should have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration5 should be returned", true, collection.Contains(declaration5));
		}

		public void TestEntryTypeFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_AgentsReference = "Entry Type 01";
			declaration1.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_AgentsReference = "Entry Type 66";
			declaration2.US_EntryType = EntryTypeList.Codes.Baggage;

			var declaration3 = Factory.New<JobDeclaration>();
			declaration2.JE_AgentsReference = "Entry Type 25";
			declaration3.US_EntryType = EntryTypeList.Codes.PermanentExhibition;

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_AgentsReference = "Entry Type 66";
			declaration4.US_EntryType = EntryTypeList.Codes.Baggage;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.EntryType];
			filter.IsActive = true;
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals("Empty Entry Type search should have found 4 declarations", 4, collection.Count);

			filter.Property = EntryTypeList.Codes.Baggage;
			collection.Load(FilterBO.Filter);
			AssertEquals("Entry Type search for code 66 should have found 2 declarations", 2, collection.Count);
			AssertEquals("declaration2 should be there", true, collection.Contains(declaration2));
			AssertEquals("declaration4 should be there", true, collection.Contains(declaration4));

			filter.Property = EntryTypeList.Codes.PermanentExhibition;
			collection.Load(FilterBO.Filter);
			AssertEquals("Should now have found only 1 declarations", 1, collection.Count);
			AssertEquals("declaration3 should be there", true, collection.Contains(declaration3));

			filter.Property = "09";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			collection.Load(FilterBO.Filter);
			AssertEquals("Search for code 09 should have found 0 declarations", 0, collection.Count);

			filter.Property = EntryTypeList.Codes.ConsumptionFreeDutiable;
			collection.Load(FilterBO.Filter);
			AssertEquals("Code 01 should find 1 declaration", 1, collection.Count);
			AssertEquals("declaration1 should be there", true, collection.Contains(declaration1));
		}

		public void TestEntryReleaseDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today.AddDays(-20);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_EntryAuthorisationDate = ZDateTime.Today.AddDays(-10);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_EntryAuthorisationDate = ZDateTime.Today;
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var entryReleaseDate = (ModuleDateFilter)bizObj[DeclarationFilterConstants.EntryReleaseDate];
			entryReleaseDate.IsActive = true;
			entryReleaseDate.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			entryReleaseDate.Property2 = ZDateTime.Today.AddDays(-30);//to-date

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;

			coll.Load(query);
			AssertEquals("declaration1 should not be there", false, coll.Contains(declaration));
			AssertEquals("declaration2 should not be there", false, coll.Contains(declaration2));
			AssertEquals("declaration3 should not be there", false, coll.Contains(declaration3));

			entryReleaseDate.Property1 = ZDateTime.Today.AddDays(-20);
			entryReleaseDate.Property2 = ZDateTime.Today;//to-date
			query = bizObj.Filter;

			coll.Load(query);
			AssertEquals("declaration1 should be there", true, coll.Contains(declaration));
			AssertEquals("declaration2 should be there", true, coll.Contains(declaration2));
			AssertEquals("declaration3 should be there", true, coll.Contains(declaration3));
		}

		public void TestEntrySubmittedDate()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var entry = declaration1.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = "ENS";
			entry.CH_EntrySubmittedDate = ZDateTime.Today;

			var declaration2 = Factory.New<JobDeclaration>();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = "ENS";
			entry2.CH_EntrySubmittedDate = ZDateTime.BrettsBirthday;

			declaration1.JE_AgentsReference = "~1";
			declaration2.JE_AgentsReference = "~2";
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var entrySubmittedDate = (ModuleDateFilter)bizObj[DeclarationFilterConstants.EntrySubmittedDate];
			entrySubmittedDate.IsActive = true;
			entrySubmittedDate.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			entrySubmittedDate.Property2 = ZDateTime.BrettsBirthday;//to-date

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			query.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_AgentsReference, SQLComparisonOperator.StartsWith, "~");
			coll.Load(query);
			AssertEquals("declaration1 should not be there", false, coll.Contains(declaration1));
			AssertEquals("declaration2 should be there", true, coll.Contains(declaration2));

			entrySubmittedDate.Property1 = ZDateTime.BrettsBirthday;
			entrySubmittedDate.Property2 = ZDateTime.Today;//to-date
			query = bizObj.Filter;
			query.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_AgentsReference, SQLComparisonOperator.StartsWith, "~");
			coll.Load(query);
			AssertEquals("declaration1 should be there", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be there", true, coll.Contains(declaration2));
		}

		public void TestPaperlessFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>(); // Import - CRL
			var declaration2 = Factory.New<JobDeclaration>(); // Export
			var declaration3 = Factory.New<JobDeclaration>(); // Import - CRL & ENS, but ENS not sent
			var declaration4 = Factory.New<JobDeclaration>(); // Import - ENS sent
			var declaration5 = Factory.New<JobDeclaration>(); // Import - ENS sent
			var declaration6 = Factory.New<JobDeclaration>(); // Import - CRL & ENS
			var declaration7 = Factory.New<JobDeclaration>(); // Import - ENS sent

			declaration1.JE_MessageType = "IMP";
			declaration3.JE_MessageType = "IMP";
			declaration4.JE_MessageType = "IMP";
			declaration5.JE_MessageType = "IMP";
			declaration6.JE_MessageType = "IMP";
			declaration7.JE_MessageType = "IMP";

			var dec1Entry = declaration1.CustomsEntryHeaders.AddNew();
			dec1Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;

			var dec3CRLEntry = declaration3.CustomsEntryHeaders.AddNew();
			dec3CRLEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			var dec3ENSEntry = declaration3.CustomsEntryHeaders.AddNew();
			dec3ENSEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var dec4Entry = declaration4.CustomsEntryHeaders.AddNew();
			dec4Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			dec4Entry.CH_Status = MessageStatusListENS.Codes.AwaitingEntrySummaryOriginal;

			var dec5Entry = declaration5.CustomsEntryHeaders.AddNew();
			dec5Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			dec5Entry.CH_Status = MessageStatusListENS.Codes.ClearEntrySummaryOriginal;

			var dec6CRLEntry = declaration6.CustomsEntryHeaders.AddNew();
			dec6CRLEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			var dec6ENSEntry = declaration6.CustomsEntryHeaders.AddNew();
			dec6ENSEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			dec6ENSEntry.CH_Status = MessageStatusListENS.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings;

			var dec7Entry = declaration5.CustomsEntryHeaders.AddNew();
			dec7Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			dec7Entry.CH_Status = MessageStatusListENS.Codes.ClearEntrySummaryOriginal;

			declaration4.US_PaperlessEntry = YesNoDefaultList.Codes.Yes;
			declaration5.US_PaperlessEntry = YesNoDefaultList.Codes.Yes;
			declaration6.US_PaperlessEntry = YesNoDefaultList.Codes.No;
			declaration7.US_PaperlessEntry = YesNoDefaultList.Codes.Yes;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.Paperless];
			filter.IsActive = true;
			filter.Property = ZString.Empty;

			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals("Empty search should have found all 7 declarations", 7, collection.Count);
			AssertEquals("declaration2 (export entry) should be in list", true, collection.Contains(declaration2));

			filter.Property = YesNoDefaultList.Codes.No;
			collection.Load(FilterBO.Filter);
			AssertEquals("Paperless search for 'No' should find 1 Import declaration with an Entry Summary entry and no paperless status", 1, collection.Count);
			AssertEquals("declaration6 should be returned", true, collection.Contains(declaration6));

			filter.Property = YesNoDefaultList.Codes.Yes;
			collection.Load(FilterBO.Filter);
			AssertEquals("Paperless search for 'Yes' should have found 3 declarations", 3, collection.Count);
			AssertEquals("declaration4 should be returned", true, collection.Contains(declaration4));
			AssertEquals("declaration5 should be returned", true, collection.Contains(declaration5));
			AssertEquals("declaration7 should be returned", true, collection.Contains(declaration7));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			collection.Load(FilterBO.Filter);
			AssertEquals("Search for Paperless 'Not True' should only find 4 declarations without a Paperless Status of Y", 4, collection.Count);
			AssertEquals("declaration1 should be returned", true, collection.Contains(declaration1));
			AssertEquals("declaration2 should be returned", true, collection.Contains(declaration2));
			AssertEquals("declaration3 should be returned", true, collection.Contains(declaration3));
			AssertEquals("declaration6 should be returned", true, collection.Contains(declaration6));

			filter.Property = YesNoDefaultList.Codes.No;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			collection.Load(FilterBO.Filter);
			AssertEquals("Search for Paperless 'Not False' should have found 6 declarations without a Paperless Status of N", 6, collection.Count);
			AssertEquals("declaration1 should be returned", true, collection.Contains(declaration1));
			AssertEquals("declaration2 should be returned", true, collection.Contains(declaration2));
			AssertEquals("declaration3 should be returned", true, collection.Contains(declaration3));
			AssertEquals("declaration4 should be returned", true, collection.Contains(declaration4));
			AssertEquals("declaration5 should be returned", true, collection.Contains(declaration5));
			AssertEquals("declaration7 should be returned", true, collection.Contains(declaration7));
		}

		public void TestEntrySubmittedDateHasDateHasNoDateFilterWithMultipleCusEntryHeaders()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_AgentsReference = "~1";
			var dec1Entry1 = declaration1.CustomsEntryHeaders.AddNew();
			dec1Entry1.CH_MessageType = "BCR";
			dec1Entry1.CH_EntrySubmittedDate = ZDateTime.Today;
			var dec1Entry2 = declaration1.CustomsEntryHeaders.AddNew();
			dec1Entry2.CH_MessageType = "ENS";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_AgentsReference = "~2";
			var dec2Entry1 = declaration2.CustomsEntryHeaders.AddNew();
			dec2Entry1.CH_MessageType = "CRL";
			var dec2Entry2 = declaration2.CustomsEntryHeaders.AddNew();
			dec2Entry2.CH_MessageType = "ENS";
			dec2Entry2.CH_EntrySubmittedDate = ZDateTime.Today;

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var entryReleaseDate = (ModuleDateFilter)bizObj[DeclarationFilterConstants.EntrySubmittedDate];
			entryReleaseDate.IsActive = true;
			entryReleaseDate.PropertySearch = ModuleDateFilter.HasDateEntered;

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			query.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_AgentsReference, SQLComparisonOperator.StartsWith, "~");
			coll.Load(query);
			AssertEquals("declaration1 should not be in filter - submitted date not on ENS", false, coll.Contains(declaration1));
			AssertEquals("declaration2 should be in filter", true, coll.Contains(declaration2));

			entryReleaseDate.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			query = bizObj.Filter;
			query.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_AgentsReference, SQLComparisonOperator.StartsWith, "~");
			coll.Load(query);
			AssertEquals("declaration1 with submitted date (but on BCR entry) should now not be in filter - no submitted date on ENS entry", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should not be in filter", false, coll.Contains(declaration2));
		}

		public void TestGetLiquidationDate()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var liquidation1 = Factory.New<CusLiquidation>();
			liquidation1.B8_LiquidationDate = ZDateTime.Today;
			liquidation1.B8_SystemCreateDate = new ZDateTime(2012, 07, 10);
			declaration1.Liquidations.Add(liquidation1);

			var declaration2 = Factory.New<JobDeclaration>();
			var liquidation2 = Factory.New<CusLiquidation>();
			liquidation2.B8_LiquidationDate = new ZDateTime(2009, 11, 20);
			liquidation2.B8_SystemCreateDate = new ZDateTime(2012, 07, 10);
			declaration2.Liquidations.Add(liquidation2);
			var liquidation2_1 = Factory.New<CusLiquidation>();
			liquidation2_1.B8_LiquidationDate = ZDateTime.Empty;
			liquidation2_1.B8_SystemCreateDate = new ZDateTime(2012, 07, 09);
			declaration2.Liquidations.Add(liquidation2_1);

			var declaration3 = Factory.New<JobDeclaration>();
			var liquidation3 = Factory.New<CusLiquidation>();
			liquidation3.B8_LiquidationDate = ZDateTime.Empty;
			liquidation3.B8_SystemCreateDate = new ZDateTime(2012, 07, 10);
			declaration3.Liquidations.Add(liquidation3);

			var declaration4 = Factory.New<JobDeclaration>();

			var declaration5 = Factory.New<JobDeclaration>();
			var liquid5_1 = Factory.New<CusLiquidation>();
			liquid5_1.B8_LiquidationDate = ZDateTime.Today;
			liquid5_1.B8_SystemCreateDate = new ZDateTime(2012, 07, 11);
			declaration5.Liquidations.Add(liquid5_1);
			var liquid5_2 = Factory.New<CusLiquidation>();
			liquid5_2.B8_LiquidationDate = ZDateTime.Empty;
			liquid5_2.B8_SystemCreateDate = new ZDateTime(2012, 07, 10);
			declaration5.Liquidations.Add(liquid5_2);

			var declaration6 = Factory.New<JobDeclaration>();
			var liquid6 = Factory.New<CusLiquidation>();
			liquid6.B8_LiquidationDate = ZDateTime.Today;
			liquid6.B8_SystemCreateDate = ZDateTime.Empty;
			declaration6.Liquidations.Add(liquid6);

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var liquidationDate = (ModuleDateFilter)bizObj[DeclarationFilterConstants.LiquidationDate];
			liquidationDate.IsActive = true;
			liquidationDate.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			liquidationDate.Property2 = ZDateTime.BrettsBirthday;//to-date

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("declaration1 should not be there", false, coll.Contains(declaration1));
			AssertEquals("declaration2 should be there", false, coll.Contains(declaration2));

			liquidationDate.Property1 = ZDateTime.BrettsBirthday;
			liquidationDate.Property2 = ZDateTime.Today.AddDays(-2);
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("declaration1 should not be there", false, coll.Contains(declaration1));
			AssertEquals("declaration2 should be there", true, coll.Contains(declaration2));

			liquidationDate.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			query = bizObj.Filter;
			coll.RemoveAll();
			coll.Load(query);
			AssertEquals("declaration1 should not be there", false, coll.Contains(declaration1));
			AssertEquals("declaration2 should not be there", false, coll.Contains(declaration2));
			AssertEquals("declaration3 should be there", true, coll.Contains(declaration3));
			AssertEquals("declaration4 should be there", true, coll.Contains(declaration4));
			AssertEquals("declaration5 should not be there", false, coll.Contains(declaration5));
			AssertEquals("declaration6 should not be there", false, coll.Contains(declaration6));

			liquidationDate.PropertySearch = ModuleDateFilter.HasDateEntered;
			query = bizObj.Filter;
			coll.RemoveAll();
			coll.Load(query);
			AssertEquals("declaration1 should be there", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be there", true, coll.Contains(declaration2));
			AssertEquals("declaration3 should not be there", false, coll.Contains(declaration3));
			AssertEquals("declaration4 should not there", false, coll.Contains(declaration4));
			AssertEquals("declaration5 should be there", true, coll.Contains(declaration5));
			AssertEquals("declaration6 should be there", true, coll.Contains(declaration6));
		}

		public void TestLookups()
		{
			var filterBizObj = new JobDeclarationFilterBusinessObject();
			AssertEquals("Lookups of correct type", typeof(JobDeclarationFilterLookups), filterBizObj.Lookups.GetType());
		}

		public override void TestJE_EntryStatus_NotSentFilter()
		{
			Assert("Filter does not contain CH_EntryStatus = '' when entry status empty", FilterBO.Filter.LiteralTextADO.IndexOf("CH_EntryStatus = ''") == -1);

			var entryStatusFilter = (EntryStatusFilter)FilterBO[FilterBO.EntryStatusText];
			entryStatusFilter.IsActive = true;
			entryStatusFilter.Property = DeclarationFilterConstants.EntryStatus.NotSentForFilter;
			Assert("Filter is empty.", FilterBO.Filter.LiteralTextADO.IndexOf("CH_EntryStatus = ''") == -1);
		}

		public void TestGetTIBExpiryDate()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.US_TIBExpiryDate = new ZDateTime(2005, 5, 30);
			declaration1.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var declaration2 = Factory.New<JobDeclaration>();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.US_TIBExpiryDate = new ZDateTime(2006, 7, 15);
			declaration2.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var entry3 = declaration2.CustomsEntryHeaders.AddNew();

			Factory.Save();
			declaration1.JE_AgentsReference = "~1";
			declaration2.JE_AgentsReference = "~2";
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var tibDates = (ModuleDateFilter)bizObj[DeclarationFilterConstants.TIBExpiryDate];
			tibDates.IsActive = true;
			tibDates.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			tibDates.Property2 = new ZDateTime(2006, 7, 15);//to-date

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			query.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_AgentsReference, SQLComparisonOperator.StartsWith, "~");
			coll.Load(query);
			AssertEquals("declaration1 should be there", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be there", true, coll.Contains(declaration2));

			tibDates.Property1 = new ZDateTime(2005, 5, 31);//from-date
			query = bizObj.Filter;
			query.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_AgentsReference, SQLComparisonOperator.StartsWith, "~");
			coll.Load(bizObj.Filter);
			AssertEquals("declaration1 should not be there", false, coll.Contains(declaration1));
			AssertEquals("declaration2 should be there", true, coll.Contains(declaration2));
		}

		public void TestITNumberFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			var bill = declaration1.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "1";
			declaration1.JE_AgentsReference = "ITNumber";
			declaration1.PrimaryMasterBill.ITNumber = "257700052";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_AgentsReference = "No ITNumber";

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			var billDec3 = declaration3.Bills.AddNew();
			billDec3.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			billDec3.CU_BillNum = "3";
			declaration3.JE_AgentsReference = "Similar ITNumber";

			var houseBill = billDec3.ChildBills.AddNew();
			billDec3.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			billDec3.CU_BillNum = "house";
			billDec3.ITNumber = "25770005239";
			billDec3.ITAndSplitDetails.AddNew().US_ITNumber = "V123456789";
			billDec3.ITAndSplitDetails.AddNew().US_ITNumber = "987654";
			Factory.Save();

			var filter = (ModuleNumberFilter)FilterBO[DeclarationFilterConstants.ITNumber];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "2577";

			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals("Starts with search should have found 2 declarations", 2, collection.Count);
			AssertEquals("declaration1 should be there", true, collection.Contains(declaration1));
			AssertEquals("declaration3 should also be there", true, collection.Contains(declaration3));

			filter.Property = "257700052";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			collection.Load(FilterBO.Filter);
			AssertEquals("Exact search should only have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration1 should be there", true, collection.Contains(declaration1));
			AssertEquals("declaration3 should NOT be there", false, collection.Contains(declaration3));

			filter.Property = "V123456789";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			collection.Load(FilterBO.Filter);
			AssertEquals("Exact search should only have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration3 should NOT be there", true, collection.Contains(declaration3));

			filter.Property = "987654";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			collection.Load(FilterBO.Filter);
			AssertEquals("Exact search should only have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration3 should NOT be there", true, collection.Contains(declaration3));

			filter.Property = "NO MATCH";
			collection.Load(FilterBO.Filter);
			AssertEquals("Should now have found no declarations", 0, collection.Count);
		}

		[TestDate(2019, 03, 04)]
		public void TestInBondClosedDate()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;

			var inbond1 = (Customs.Business.CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			inbond1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			inbond1.BH_ParentID = declaration1.PK;
			inbond1.BH_ParentTableCode = declaration1.TablePrefix;
			inbond1.MovementHeader.BM_InBondClosedDate = ZDateTime.Today;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;

			var inbond2 = (Customs.Business.CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			inbond2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			inbond2.BH_ParentID = declaration2.PK;
			inbond2.BH_ParentTableCode = declaration2.TablePrefix;
			inbond2.MovementHeader.BM_InBondClosedDate = ZDateTime.Today.AddDays(-10);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;

			Factory.Save();

			var dateFilter = (ModuleDateFilter)FilterBO["In-Bond Closed Date"];
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = ZDateTime.Today.AddDays(-5);

			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals("Search should have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration1 should be there", true, collection.Contains(declaration1));
			AssertEquals("declaration2 should NOT be there", false, collection.Contains(declaration2));
			AssertEquals("declaration3 should NOT be there", false, collection.Contains(declaration3));

			dateFilter.Property1 = ZDateTime.Today.AddDays(-20);
			collection.Load(FilterBO.Filter);
			AssertEquals("Search should have found 2 declarations", 2, collection.Count);
			AssertEquals("declaration1 should be there", true, collection.Contains(declaration1));
			AssertEquals("declaration2 should be there", true, collection.Contains(declaration2));
			AssertEquals("declaration3 should NOT be there", false, collection.Contains(declaration3));

			dateFilter.IsActive = false;
			collection.Load(FilterBO.Filter);
			AssertEquals("Search should have found 3 declarations", 3, collection.Count);
			AssertEquals("declaration1 should be there", true, collection.Contains(declaration1));
			AssertEquals("declaration2 should be there", true, collection.Contains(declaration2));
			AssertEquals("declaration3 should be there", true, collection.Contains(declaration3));
		}

		public void TestInBondEntryType()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = USJobMessageTypeList.Codes.Import;

			var inbond1 = (Customs.Business.CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			inbond1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			inbond1.BH_ParentID = declaration1.PK;
			inbond1.BH_ParentTableCode = declaration1.TablePrefix;
			inbond1.MovementHeader.BM_InBondEntryType = "61";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = USJobMessageTypeList.Codes.Import;

			var inbond2 = (Customs.Business.CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			inbond2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			inbond2.BH_ParentID = declaration2.PK;
			inbond2.BH_ParentTableCode = declaration2.TablePrefix;
			inbond2.MovementHeader.BM_InBondEntryType = "62";

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = USJobMessageTypeList.Codes.Import;

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = USJobMessageTypeList.Codes.Import;

			var inbond4 = (Customs.Business.CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			inbond4.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			inbond4.BH_ParentID = declaration4.PK;
			inbond4.BH_ParentTableCode = declaration4.TablePrefix;
			inbond4.MovementHeader.BM_InBondEntryType = "62";

			var inbond5 = (Customs.Business.CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			inbond5.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			inbond5.BH_ParentID = declaration4.PK;
			inbond5.BH_ParentTableCode = declaration4.TablePrefix;
			inbond5.MovementHeader.BM_InBondEntryType = "63";

			Factory.Save();

			var typeFilter = (ModuleTextFilter)FilterBO["In-Bond Entry Type"];
			typeFilter.IsActive = true;
			typeFilter.Property = "61";

			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals("Search should have found 1 declarations", 1, collection.Count);
			AssertEquals("declaration1 should be there", true, collection.Contains(declaration1));
			AssertEquals("declaration2 should not be there", false, collection.Contains(declaration2));
			AssertEquals("declaration3 should not be there", false, collection.Contains(declaration3));
			AssertEquals("declaration4 should not be there", false, collection.Contains(declaration4));

			typeFilter.Property = "62";
			collection.Load(FilterBO.Filter);
			AssertEquals("Search should have found 2 declarations", 2, collection.Count);
			AssertEquals("declaration1 should not be there", false, collection.Contains(declaration1));
			AssertEquals("declaration2 should be there", true, collection.Contains(declaration2));
			AssertEquals("declaration3 should not be there", false, collection.Contains(declaration3));
			AssertEquals("declaration4 should be there", true, collection.Contains(declaration4));

			typeFilter.Property = "63";
			collection.Load(FilterBO.Filter);
			AssertEquals("Search should have found 1 declarations", 1, collection.Count);
			AssertEquals("declaration1 should not be there", false, collection.Contains(declaration1));
			AssertEquals("declaration2 should not be there", false, collection.Contains(declaration2));
			AssertEquals("declaration3 should not be there", false, collection.Contains(declaration3));
			AssertEquals("declaration4 should be there", true, collection.Contains(declaration4));

			typeFilter.IsActive = false;
			collection.Load(FilterBO.Filter);
			AssertEquals("Search should have found 4 declarations", 4, collection.Count);
			AssertEquals("declaration1 should be there", true, collection.Contains(declaration1));
			AssertEquals("declaration2 should be there", true, collection.Contains(declaration2));
			AssertEquals("declaration3 should be there", true, collection.Contains(declaration3));
			AssertEquals("declaration4 should be there", true, collection.Contains(declaration4));
		}

		public void TestCarrierFilter()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsTransportClient = true;

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_AgentsReference = "Dec with Carrier";
			declaration1.JE_OH_ShippingLine = carrier.PK;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_AgentsReference = "No Carriers";

			Factory.Save();

			var filter = (ModuleGuidFilter)FilterBO[DeclarationFilterConstants.Carrier];
			filter.IsActive = true;
			filter.Property = ZGuid.Empty;

			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals("Empty Carrier search should have found both declarations", 2, collection.Count);

			filter.Property = carrier.PK;
			collection.Load(FilterBO.Filter);
			AssertEquals("Carrier search for carrier should have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration1 should be there", true, collection.Contains(declaration1));
		}

		public void TestCarrierSCACFilter()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsTransportClient = true;
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "HLKP");

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration1.JE_AgentsReference = "Export Dec with Carrier SCAC";
			declaration1.JE_OH_ShippingLine = carrier.PK;
			declaration1.US_UI_NKCarrierSCAC = ZString.Empty;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration2.JE_AgentsReference = "Export Dec, no Carrier SCAC";

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.JE_AgentsReference = "Import Dec with Carrier SCAC";
			declaration3.US_UI_NKCarrierSCAC = "HLKP";

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration4.JE_AgentsReference = "Import Dec, no Carrier SCAC";

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingProvider = true;
			carrier2.OH_IsTransportClient = true;

			var declaration5 = Factory.New<JobDeclaration>();
			declaration5.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration5.JE_AgentsReference = "Export with Ship Line without SCAC";
			declaration5.JE_OH_ShippingLine = carrier2.PK;

			var declaration6 = Factory.New<JobDeclaration>();
			declaration6.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration6.JE_AgentsReference = "Export Dec with Carrier SCAC";
			declaration6.US_UI_NKCarrierSCAC = "HLKP";

			var declaration7 = Factory.New<JobDeclaration>();
			declaration7.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration7.JE_AgentsReference = "Export Dec with Carrier SCAC";
			declaration7.JE_OH_ShippingLine = carrier.PK;
			declaration7.US_UI_NKCarrierSCAC = "APLU";

			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();
			carrier3.OH_IsShippingProvider = true;
			carrier3.OH_IsTransportClient = true;
			carrier3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "ALPU");

			var declaration8 = Factory.New<JobDeclaration>();
			declaration8.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration8.JE_AgentsReference = "Export Dec with Carrier SCAC";
			declaration8.JE_OH_ShippingLine = carrier3.PK;
			declaration8.US_UI_NKCarrierSCAC = "HLKP";

			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.CarrierSCAC];
			filter.IsActive = true;
			filter.Property = ZString.Empty;

			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals("Empty Carrier SCAC search should have found all declarations", 8, collection.Count);

			filter.Property = "HLKP";
			collection.Load(FilterBO.Filter);
			AssertEquals("Carrier SCAC search should have found 3 declarations", 4, collection.Count);
			AssertEquals("declaration1 should be returned", true, collection.Contains(declaration1));
			AssertEquals("declaration3 should be returned", true, collection.Contains(declaration3));
			AssertEquals("declaration6 should be returned", true, collection.Contains(declaration6));
			AssertEquals("declaration8 should be returned", true, collection.Contains(declaration8));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			filter.Property = "HLKP";
			collection.Load(FilterBO.Filter);
			AssertEquals("Carrier SCAC search should have found 3 declarations", 4, collection.Count);
			AssertEquals("declaration2 should be returned", true, collection.Contains(declaration2));
			AssertEquals("declaration4 should be returned", true, collection.Contains(declaration4));
			AssertEquals("declaration5 should be returned", true, collection.Contains(declaration5));
			AssertEquals("declaration7 should be returned", true, collection.Contains(declaration7));

			filter.Property = ZString.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			collection.Load(FilterBO.Filter);
			AssertEquals("Blank Carrier SCAC search should have found 3 declarations", 3, collection.Count);
			AssertEquals("declaration2 should be returned", true, collection.Contains(declaration2));
			AssertEquals("declaration4 should be returned", true, collection.Contains(declaration4));
			AssertEquals("declaration5 should be returned", true, collection.Contains(declaration5));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			collection.Load(FilterBO.Filter);
			AssertEquals("Not Blank Carrier SCAC search should have found 5 declarations", 5, collection.Count);
			AssertEquals("declaration1 should be returned", true, collection.Contains(declaration1));
			AssertEquals("declaration3 should be returned", true, collection.Contains(declaration3));
			AssertEquals("declaration6 should be returned", true, collection.Contains(declaration6));
			AssertEquals("declaration7 should be returned", true, collection.Contains(declaration7));
			AssertEquals("declaration8 should be returned", true, collection.Contains(declaration8));
		}

		public void TestGetPGAStatusQuery()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration1.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration2.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration3.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration4.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration4.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;

			//declaration1
			var dis1_1 = declaration1.EntryPGACusDispositions.AddNew();
			dis1_1.CDI_StatusKey = "FDA";
			dis1_1.CDI_Status = "01";
			dis1_1.CDI_StatusDate = ZDateTime.Now;
			dis1_1.CDI_Type = "PES";

			var dis1_2 = declaration1.EntryPGACusDispositions.AddNew();
			dis1_2.CDI_StatusKey = "EPA";
			dis1_2.CDI_Status = "07";
			dis1_2.CDI_StatusDate = ZDateTime.Now;
			dis1_2.CDI_Type = "PES";

			var dis1_3 = declaration1.EntryPGACusDispositions.AddNew();
			dis1_3.CDI_StatusKey = "FIS";
			dis1_3.CDI_Status = "MC";
			dis1_3.CDI_StatusDate = ZDateTime.Now;
			dis1_3.CDI_Type = "PES";

			//declaration2
			var dis2_1 = declaration2.EntryPGACusDispositions.AddNew();
			dis2_1.CDI_StatusKey = "FDA";
			dis2_1.CDI_Status = "07";
			dis2_1.CDI_StatusDate = ZDateTime.Now;
			dis2_1.CDI_Type = "PES";

			var dis2_2 = declaration2.EntryPGACusDispositions.AddNew();
			dis2_2.CDI_StatusKey = "EPA";
			dis2_2.CDI_Status = "MC";
			dis2_2.CDI_StatusDate = ZDateTime.Now;
			dis2_2.CDI_Type = "PES";

			//declaration3
			var dis3_1 = declaration3.EntryPGACusDispositions.AddNew();
			dis3_1.CDI_StatusKey = "EPA";
			dis3_1.CDI_Status = "01";
			dis3_1.CDI_StatusDate = ZDateTime.Now;
			dis3_1.CDI_Type = "PES";

			//declaration4

			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.PGAStatus];
			filter.IsActive = true;

			filter.Property = DeclarationFilterConstants.FilterCodeAndDesc.ALLMayProceedManuallyClosed;
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals(collection.Count, 1);
			AssertEquals("declaration1 should be returned", true, collection.Contains(declaration2));
			AssertEquals(false, collection.Contains(declaration1));
			AssertEquals(false, collection.Contains(declaration3));
			AssertEquals(false, collection.Contains(declaration4));

			filter.Property = DeclarationFilterConstants.FilterCodeAndDesc.HasAnyManuallyClosed;
			collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals(collection.Count, 2);
			AssertEquals("declaration1 should be returned", true, collection.Contains(declaration1));
			AssertEquals("declaration2 should be returned", true, collection.Contains(declaration2));
			AssertEquals(false, collection.Contains(declaration3));
			AssertEquals(false, collection.Contains(declaration4));

			filter.Property = DeclarationFilterConstants.FilterCodeAndDesc.HasAnyPGAStatus;
			collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals(collection.Count, 3);
			AssertEquals("declaration1 should be returned", true, collection.Contains(declaration1));
			AssertEquals("declaration2 should be returned", true, collection.Contains(declaration2));
			AssertEquals("declaration3 should be returned", true, collection.Contains(declaration3));
			AssertEquals(false, collection.Contains(declaration4));

			filter.Property = DeclarationFilterConstants.FilterCodeAndDesc.HasNoPGAStatus;
			collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals(collection.Count, 1);
			AssertEquals(false, collection.Contains(declaration1));
			AssertEquals(false, collection.Contains(declaration2));
			AssertEquals(false, collection.Contains(declaration3));
			AssertEquals(true, collection.Contains(declaration4));
		}

		public void TestLocationOfGoodsFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Import;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "LWH1", "Misaka", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "LWH2", "Kanzaki", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			declaration1.US_US_NKLocationOfGoods = "LWH1";
			declaration2.US_US_NKLocationOfGoods = "LWH2";
			declaration3.US_US_NKLocationOfGoods = "LWH1";
			declaration4.US_US_NKLocationOfGoods = ZString.Empty;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.LocationOfGoods];
			filter.IsActive = true;
			filter.Property = ZString.Empty;

			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals("Empty FIRMS Code search should have found all 4 declarations", 4, collection.Count);
			AssertEquals("declaration4 (without Firms Code) should be in list", true, collection.Contains(declaration4));

			filter.Property = "LWH2";
			collection.Load(FilterBO.Filter);
			AssertEquals("Firms Code search for LWH2 should have found only 1 declaration", 1, collection.Count);
			AssertEquals("declaration2 should be returned", true, collection.Contains(declaration2));

			filter.Property = "LWH1";
			collection.Load(FilterBO.Filter);
			AssertEquals("Firms Code search for LWH1 should have found 2 declarations", 2, collection.Count);
			AssertEquals("declaration1 should be returned", true, collection.Contains(declaration1));
			AssertEquals("declaration3 should be returned", true, collection.Contains(declaration3));

			filter.Property = "";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			collection.Load(FilterBO.Filter);
			AssertEquals("Firms Code search for blank filter should have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration4 should be returned", true, collection.Contains(declaration4));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			collection.Load(FilterBO.Filter);
			AssertEquals("Firms Code search for not blank filter should have found 3 declarations", 3, collection.Count);
			AssertEquals("declaration1 should be returned", true, collection.Contains(declaration1));
			AssertEquals("declaration2 should be returned", true, collection.Contains(declaration2));
			AssertEquals("declaration3 should be returned", true, collection.Contains(declaration3));
		}

		public void TestCargoReleaseStatus()
		{
			CreateTestDecsWithStatus();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var statusFilter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.CargoReleaseStatus];
			statusFilter.IsActive = true;
			statusFilter.Property = ImportMessageStatusList.Codes.ClearBorderCargoReleaseOriginal;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should have found two declarations", 2, coll.Count);
			AssertEquals("Contains CRL Dec1 with CBO status", true, coll.Contains(cRLDeclaration1));
			AssertEquals("Contains CRL Dec3 with CBO status", true, coll.Contains(cRLDeclaration3));

			statusFilter.Property = ImportMessageStatusList.Codes.ClearArrival;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should have found only one declaration", 1, coll.Count);
			AssertEquals("Should contain CRL Dec2 with CAV status", true, coll.Contains(cRLDeclaration2));

			statusFilter.Property = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Filter result should be nil", 0, coll.Count);

			statusFilter.Property = ImportMessageStatusList.Codes.ClearBorderCargoReleaseOriginal;
			statusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Not equal filter result should find all Import decs not with CBO cargo release status", 9, coll.Count);

			statusFilter.Property = "C";
			statusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Starts with filter result should find all decs with cargo release status beginning with 'C'", 6, coll.Count);

			statusFilter.Property = DeclarationFilterConstants.MessageStatus.NotSentForFilter;
			statusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should find 10 non CRL Import declarations", 5, coll.Count);
		}

		public void TestCargoReleaseStatusForNotSent()
		{
			CreateTestDecsWithAndWithoutStatuses();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var statusFilter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.CargoReleaseStatus];
			statusFilter.IsActive = true;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should have found all created test declarations", 14, coll.Count);

			statusFilter.Property = DeclarationFilterConstants.MessageStatus.NotSentForFilter;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should find 9 Import declarations that don't have a CRL Entry or CRL Entry with a status", 9, coll.Count);
		}

		public void TestSimplifiedEntryStatuses()
		{
			CreateTestDecsWithStatus();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var statusFilter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.CargoReleaseStatus];
			statusFilter.IsActive = true;
			statusFilter.Property = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should have found 2 declarations", 2, coll.Count);
			AssertEquals(true, coll.Contains(simplifiedEntryDeclaration1));
			AssertEquals(true, coll.Contains(simplifiedEntryDeclaration3));

			statusFilter.Property = ImportMessageStatusList.Codes.ClearACECargoReleaseDelete;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should have found only one declaration", 1, coll.Count);
			AssertEquals(true, coll.Contains(simplifiedEntryDeclaration2));

			statusFilter.Property = ImportMessageStatusList.Codes.AwaitingACECargoReleaseReplace;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("No declarations found", 0, coll.Count);

			statusFilter.Property = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			statusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Not equal filter result should find all Import decs not with clear simplified entry status", 9, coll.Count);

			statusFilter.Property = "C";
			statusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Starts with filter result should find all decs with simplified entry status beginning with 'C'", 6, coll.Count);

			statusFilter.Property = DeclarationFilterConstants.MessageStatus.NotSentForFilter;
			statusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should find 5 non SE Import declarations", 5, coll.Count);

			var impDeclaration = Factory.New<JobDeclaration>();
			impDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			impDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			impDeclaration.US_EnableCRL = true;
			var entry = impDeclaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			var bill = impDeclaration.Bills.AddNew();
			bill.CU_BillNum = "Test0011";
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.DispositionCodes.AddNewIfNotExist("93", ZDateTime.Today, BillDispositionSourceList.Codes.SO);
			Factory.Save();

			bizObj = new JobDeclarationFilterBusinessObject();
			statusFilter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.SimplifiedEntryBillStatus];
			statusFilter.IsActive = true;
			statusFilter.Property = "93";
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			query = bizObj.Filter;
			coll.Load(query);

			AssertEquals(2, coll.Count);
			AssertEquals(simplifiedEntryDeclaration1, coll[0]);
			AssertEquals(impDeclaration, coll[1]);
		}

		public void TestSimplifiedEntryStatusesHoldOrExam()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			var bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill1.CU_BillNum = "Test1";
			bill1.DispositionCodes.AddNewIfNotExist(Enterprise.Customs.US.Messaging.Business.DispositionList.Codes._51, ZDateTime.Today, BillDispositionSourceList.Codes.SO);
			bill1.CU_MessageStatus = "51";
			Factory.Save();

			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill2.CU_BillNum = "Test2";
			bill2.DispositionCodes.AddNewIfNotExist(Enterprise.Customs.US.Messaging.Business.DispositionList.Codes._93, ZDateTime.Today, BillDispositionSourceList.Codes.SO);
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();

			var statusFilter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.SimplifiedEntryBillStatus];
			statusFilter.IsActive = true;
			statusFilter.Property = Enterprise.Customs.Common.US.SEBillProcessingResultList.BillStatusHoldOrExam;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals(1, coll.Count);
			AssertEquals(declaration, coll[0]);

			statusFilter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.BillHoldOrExam];
			statusFilter.IsActive = true;
			statusFilter.Property = Enterprise.Customs.Common.US.SEBillProcessingResultList.BillStatusHoldOrExam;
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals(1, coll.Count);
			AssertEquals(declaration, coll[0]);
		}

		[TestDate(2021, 07, 10)]
		public void TestSimplifiedEntryBillStatus_DispositionsHaveSameDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			var bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill1.CU_BillNum = "TEST1";
			bill1.DispositionCodes.AddNewIfNotExist(Enterprise.Customs.US.Messaging.Business.DispositionList.Codes._58, ZDateTime.Today, BillDispositionSourceList.Codes.SO);
			bill1.DispositionCodes.AddNewIfNotExist(Enterprise.Customs.US.Messaging.Business.DispositionList.Codes._95, ZDateTime.Today, BillDispositionSourceList.Codes.SO);
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();

			var statusFilter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.SimplifiedEntryBillStatus];
			statusFilter.IsActive = true;
			statusFilter.Property = "58";
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals(1, coll.Count);
			AssertEquals(declaration, coll[0]);

			statusFilter.Property = "95";
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals(1, coll.Count);
			AssertEquals(declaration, coll[0]);
		}

		public void TestSimplifiedEntryBillStatusWithCorrectCompany()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			company.GC_Code = "UC1";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "UB1";

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration1.US_EnableCRL = true;
			declaration1.ImportEntryNumber = "12345678";
			declaration1.JE_GB = branch.PK;
			var bill1 = declaration1.Bills.AddNew();
			bill1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill1.CU_BillNum = "TEST1";
			bill1.DispositionCodes.AddNewIfNotExist(Enterprise.Customs.US.Messaging.Business.DispositionList.Codes._95, ZDateTime.Today, BillDispositionSourceList.Codes.SO);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration2.US_EnableCRL = true;
			var bill2 = declaration2.Bills.AddNew();
			bill2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill2.CU_BillNum = "TEST2";
			bill2.DispositionCodes.AddNewIfNotExist(Enterprise.Customs.US.Messaging.Business.DispositionList.Codes._95, ZDateTime.Today, BillDispositionSourceList.Codes.SO);
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var statusFilter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.SimplifiedEntryBillStatus];
			statusFilter.IsActive = true;
			statusFilter.Property = "95";
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals(1, coll.Count);
			AssertEquals(declaration2, coll[0]);
		}

		public void TestSimplifiedEntryStatusesLastStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			var bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill1.CU_BillNum = "Test1";
			bill1.DispositionCodes.AddNewIfNotExist(Enterprise.Customs.US.Messaging.Business.DispositionList.Codes._51, ZDateTime.Today.AddDays(-1), BillDispositionSourceList.Codes.SO);
			bill1.DispositionCodes.AddNewIfNotExist(Enterprise.Customs.US.Messaging.Business.DispositionList.Codes._93, ZDateTime.Today, BillDispositionSourceList.Codes.SO);

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var statusFilter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.SimplifiedEntryBillStatus];
			statusFilter.IsActive = true;
			statusFilter.Property = Enterprise.Customs.US.Messaging.Business.DispositionList.Codes._51;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Last status is '93'", 0, coll.Count);

			bizObj = new JobDeclarationFilterBusinessObject();
			statusFilter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.SimplifiedEntryBillStatus];
			statusFilter.IsActive = true;
			statusFilter.Property = Enterprise.Customs.US.Messaging.Business.DispositionList.Codes._93;
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals(1, coll.Count);
			AssertEquals(declaration, coll[0]);
		}

		public void TestBillHoldOrExam()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			var bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill1.CU_BillNum = "Test1";

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var statusFilter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.BillHoldOrExam];
			statusFilter.IsActive = true;
			statusFilter.Property = HLDOrEXMStatusList.Codes.Yes;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("SequenceNumber = 0 record should be excluded", 0, coll.Count);

			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill2.CU_BillNum = "Test2";
			bill2.CU_MessageStatus = "53";

			Factory.Save();

			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals(1, coll.Count);
			AssertEquals(declaration, coll[0]);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration2.US_EnableCRL = true;
			var bill3 = declaration2.Bills.AddNew();
			bill3.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill3.CU_BillNum = "Test3";

			Factory.Save();

			statusFilter.Property = HLDOrEXMStatusList.Codes.All;
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals(2, coll.Count);

			statusFilter.Property = HLDOrEXMStatusList.Codes.No;
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals(1, coll.Count);
			AssertEquals(declaration2, coll[0]);
		}

		public void TestEntrySummaryStatus()
		{
			CreateTestDecsWithStatus();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var statusFilter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.EntrySummaryStatus];
			statusFilter.IsActive = true;
			statusFilter.Property = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should have found two declarations", 2, coll.Count);
			AssertEquals("Contains ENS Dec1 with AEQ status", true, coll.Contains(eNSDeclaration1));
			AssertEquals("Contains ENS Dec2 with AEQ status", true, coll.Contains(eNSDeclaration2));

			statusFilter.Property = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should have found one declaration", 1, coll.Count);
			AssertEquals("Should contain ENS Dec2 with CDO status", true, coll.Contains(eNSDeclaration2));

			statusFilter.Property = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Filter result should be nil", 0, coll.Count);

			statusFilter.Property = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			statusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Not equal filter result should find all Import decs not AEO entry summary status", 9, coll.Count);

			statusFilter.Property = DeclarationFilterConstants.MessageStatus.NotSentForFilter;
			statusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should find 9 Import declarations that do not have an ENS entry or ENS status", 9, coll.Count);

			var eNSDeclaration3 = Factory.New<JobDeclaration>();
			eNSDeclaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			var eNSentry3 = eNSDeclaration2.CustomsEntryHeaders.AddNew();
			eNSentry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			eNSentry3.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryDelete;

			statusFilter.Property = ImportMessageStatusList.Codes.ClearEntrySummaryDelete;
			statusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Not equal filter result should find all Import decs not CED entry summary status", 11, coll.Count);

			var cancelledDeclaration = Factory.New<JobDeclaration>();
			cancelledDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var cancelledEntry = cancelledDeclaration.CustomsEntryHeaders.AddNew();
			cancelledEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			cancelledEntry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryCanceled;
			Factory.Save();

			statusFilter.Property = ImportMessageStatusList.Codes.EntrySummaryCanceled;
			statusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should find 1 declaration with cancelled entry", 1, coll.Count);
			Assert(coll.Contains(cancelledDeclaration));
		}

		public void TestEntryStatusForNotSent()
		{
			CreateTestDecsWithAndWithoutStatuses();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var statusFilter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.EntrySummaryStatus];
			statusFilter.IsActive = true;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should have found all created test declarations", 14, coll.Count);

			statusFilter.Property = DeclarationFilterConstants.MessageStatus.NotSentForFilter;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should find 10 Import declarations that don't either have an ENS Entry or an ENS Entry with a status", 10, coll.Count);
		}

		public void TestExportStatus()
		{
			CreateTestDecsWithStatus();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var statusFilter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.ExportStatus];
			statusFilter.IsActive = true;
			statusFilter.Property = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should have found two declarations", 2, coll.Count);
			AssertEquals("Contains EXP Dec1 with OSC status", true, coll.Contains(eXPDeclaration1));
			AssertEquals("Contains EXP Dec2 with OSC status", true, coll.Contains(eXPDeclaration2));

			statusFilter.Property = AESDirectCustomsEntryStatus.Codes.AwaitingReplacementResponse;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should have found one declaration", 1, coll.Count);
			AssertEquals("Should contain EXP Dec3 with ARR status", true, coll.Contains(eXPDeclaration3));

			statusFilter.Property = AESDirectCustomsEntryStatus.Codes.ReplacementSEDRequired;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Filter result should be nil", 0, coll.Count);

			statusFilter.Property = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			statusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Not equal filter result should find Export decs not with OSC export status", 1, coll.Count);

			statusFilter.Property = "R";
			statusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Starts with filter result should find no Export decs starting with 'R' export status", 0, coll.Count);

			var entry = eXPDeclaration1.ActiveEntryHeaders[0];
			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.ReplacementSEDClear;
			Factory.Save();

			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Starts with filter result should now find 1 dec with export status starting with 'R'", 1, coll.Count);

			var entry3 = eXPDeclaration3.ActiveEntryHeaders[0];
			entry3.CH_Status = AESDirectCustomsEntryStatus.Codes.ReplacementSEDRequired;
			Factory.Save();

			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Starts with filter result should now find 2 decs with export status starting with 'R'", 2, coll.Count);
		}

		public void TestExportStatusForNotSent()
		{
			CreateTestDecsWithAndWithoutStatuses();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var statusFilter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.ExportStatus];
			statusFilter.IsActive = true;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should have found all created test declarations", 14, coll.Count);

			statusFilter.Property = DeclarationFilterConstants.MessageStatus.NotSentForFilter;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should find both Export declarations that don't have an Export status", 2, coll.Count);
		}

		public void TestExportStatusForMultipleEntriesStatus()
		{
			CreateTestDecsWithStatus();
			CreateAdditionalExportTestDecs();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var statusFilter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.ExportStatus];
			statusFilter.IsActive = true;
			statusFilter.Property = AESDirectCustomsEntryStatus.Codes.MultipleEntriesStatus;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should have found 4 declarations with multiple status", 3, coll.Count);
			AssertEquals("Contains EXP Dec4 with MES status", true, coll.Contains(eXPDeclaration4));
			AssertEquals("Contains EXP Dec6 with MES status", true, coll.Contains(eXPDeclaration6));
			AssertEquals("Contains EXP Dec8 with MES status", true, coll.Contains(eXPDeclaration8));
		}

		public void TestBLUStatus()
		{
			CreateTestDecsWithStatus();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var statusFilter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.BLUMessageStatus];
			statusFilter.IsActive = true;
			statusFilter.Property = ImportMessageStatusList.Codes.ClearBillOfLadingUpdate;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should have found two declarations", 2, coll.Count);
			AssertEquals("Contains CRLDeclaration1 with Bill of Lading Update status of CBU", true, coll.Contains(cRLDeclaration1));
			AssertEquals("Contains CRLDeclaration3 with CBO status", true, coll.Contains(cRLDeclaration3));

			statusFilter.Property = ImportMessageStatusList.Codes.AwaitingBillOfLadingUpdate;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should have found only one declaration", 1, coll.Count);
			AssertEquals("Should contain ENSDeclaration1 with Bill of Lading Update status of ABU", true, coll.Contains(eNSDeclaration1));

			statusFilter.Property = ImportMessageStatusList.Codes.ErrorBillOfLadingUpdate;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Filter result should have found 1 declaration", 1, coll.Count);
			AssertEquals("Should contain ENSDeclaration2 with Bill of Lading Update status of EBU", true, coll.Contains(eNSDeclaration2));

			statusFilter.Property = ImportMessageStatusList.Codes.ErrorBillOfLadingUpdate;
			statusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Not equal filter result should find all decs not EBU bill of lading status", 13, coll.Count);
		}

		public void TestStatementStatusFilter()
		{
			CreateTestDecsWithStatus();
			var entry = eNSDeclaration1.ActiveEntryHeaders[0];
			AssertEquals("Should have entry summary", CusEntryHeaderMessageTypeList.Codes.EntrySummary, entry.CH_MessageType);
			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			var entryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNum.CE_ParentTable = JobDeclarationSchema.Constants.TableName;
			entryNum.CE_ParentID = eNSDeclaration1.PK;
			entryNum.CE_EntryType = "IMP";
			entryNum.CE_EntryNum = "78374859";

			var stmtHeader = Factory.NewWithValidTestData<CusStatementHeader>();
			stmtHeader.B2_Status = StatementHeaderStatusList.Codes.Preliminary;

			var stmtLine = Factory.NewWithValidTestData<CusStatementLine>();
			stmtLine.B3_B2 = stmtHeader.PK;
			stmtLine.B3_EntryNum = "78374859";

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var stmtStatusFilter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.StatementStatus];
			stmtStatusFilter.IsActive = true;
			stmtStatusFilter.Property = StatementHeaderStatusList.Codes.Preliminary;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);

			AssertEquals("Should have found one declaration with Statment Status", 1, coll.Count);
			AssertEquals("Contains ENSDeclaration1 with statment status of PRE", true, coll.Contains(eNSDeclaration1));

			stmtStatusFilter.Property = StatementHeaderStatusList.Codes.Final;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should not have found any declarations", 0, coll.Count);

			stmtStatusFilter.Property = StatementHeaderStatusList.Codes.Preliminary;
			stmtStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Not equal filter result should find all decs not PRE statement status", 13, coll.Count);

			var entryNum2 = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNum2.CE_ParentTable = JobDeclarationSchema.Constants.TableName;
			entryNum2.CE_ParentID = eNSDeclaration2.PK;
			entryNum2.CE_EntryType = "IMP";
			entryNum2.CE_EntryNum = "78374873";

			var stmtHeader2 = Factory.NewWithValidTestData<CusStatementHeader>();
			stmtHeader2.B2_Status = StatementHeaderStatusList.Codes.Preliminary;

			var stmtLine2 = Factory.NewWithValidTestData<CusStatementLine>();
			stmtLine2.B3_B2 = stmtHeader.PK;
			stmtLine2.B3_EntryNum = "78374873";

			Factory.Save();

			stmtStatusFilter.Property = StatementHeaderStatusList.Codes.Preliminary;
			stmtStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Not equal filter result should now find 1 less PRE statement status", 12, coll.Count);

			stmtStatusFilter.Property = "P";
			stmtStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Starts with filter result should find 2 declarations with statement status starting with 'P'", 2, coll.Count);
		}

		public void TestPaymentStatusFilter()
		{
			CreateTestDecsWithStatus();
			var entry = eNSDeclaration1.ActiveEntryHeaders[0];
			AssertEquals("Should have entry summary", CusEntryHeaderMessageTypeList.Codes.EntrySummary, entry.CH_MessageType);
			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			var entryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNum.CE_ParentTable = JobDeclarationSchema.Constants.TableName;
			entryNum.CE_ParentID = eNSDeclaration1.PK;
			entryNum.CE_EntryType = "IMP";
			entryNum.CE_EntryNum = "78374859";

			var stmtHeader = Factory.NewWithValidTestData<CusStatementHeader>();
			stmtHeader.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			stmtHeader.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;

			var stmtLine = Factory.NewWithValidTestData<CusStatementLine>();
			stmtLine.B3_B2 = stmtHeader.PK;
			stmtLine.B3_EntryNum = "78374859";

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var pymtStatusFilter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.PaymentStatus];
			pymtStatusFilter.IsActive = true;
			pymtStatusFilter.Property = PaymentStatusList.Codes.PaymentAuthorizationAccepted;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);

			AssertEquals("Should have found one declaration with Payment Status", 1, coll.Count);
			AssertEquals("Contains ENSDeclaration1 with payment status of PAA", true, coll.Contains(eNSDeclaration1));

			pymtStatusFilter.Property = PaymentStatusList.Codes.PaymentFailed;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should not have found any declarations", 0, coll.Count);

			pymtStatusFilter.Property = PaymentStatusList.Codes.PaymentAuthorizationAccepted;
			pymtStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Not equal filter result should find all decs not PAA payment status", 13, coll.Count);

			var entryNum2 = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNum2.CE_ParentTable = JobDeclarationSchema.Constants.TableName;
			entryNum2.CE_ParentID = eNSDeclaration2.PK;
			entryNum2.CE_EntryType = "IMP";
			entryNum2.CE_EntryNum = "78374873";

			var stmtHeader2 = Factory.NewWithValidTestData<CusStatementHeader>();
			stmtHeader2.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			stmtHeader2.B2_PaymentStatus = PaymentStatusList.Codes.PaymentInProgress;

			var stmtLine2 = Factory.NewWithValidTestData<CusStatementLine>();
			stmtLine2.B3_B2 = stmtHeader2.PK;
			stmtLine2.B3_EntryNum = "78374873";

			Factory.Save();

			pymtStatusFilter.Property = PaymentStatusList.Codes.PaymentInProgress;
			pymtStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("equal filter result should now find 1 new PYG payment status", 1, coll.Count);
			AssertEquals("Contains ENSDeclaration2 with payment status of PAA", true, coll.Contains(eNSDeclaration2));

			pymtStatusFilter.Property = PaymentStatusList.Codes.PaymentInProgress;
			pymtStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Not equal filter result should find 13 decs without PYG payment status", 13, coll.Count);

			pymtStatusFilter.Property = "P";
			pymtStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Starts with filter result should find 2 declarations with statement status starting with 'P'", 2, coll.Count);

			stmtLine2.B3_Status = StatementLineStatusList.Codes.Deleted;
			Factory.Save();
			coll.Load(query);
			AssertEquals("Declaration has statement line with B3_Status == 'DEL' does not include.", 1, coll.Count);

			pymtStatusFilter.Property = PaymentStatusList.Codes.PaymentInProgress;
			pymtStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Not equal filter result should find 13 decs without PYG payment status (Not include the 'DEL')", 14, coll.Count);
		}

		public void TestWithBothStatementStatuses()
		{
			CreateTestDecsWithStatus();
			var entry = eNSDeclaration1.ActiveEntryHeaders[0];
			AssertEquals("Should have entry summary", CusEntryHeaderMessageTypeList.Codes.EntrySummary, entry.CH_MessageType);
			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			var entryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNum.CE_ParentTable = JobDeclarationSchema.Constants.TableName;
			entryNum.CE_ParentID = eNSDeclaration1.PK;
			entryNum.CE_EntryType = "IMP";
			entryNum.CE_EntryNum = "78374859";

			var stmtHeader = Factory.NewWithValidTestData<CusStatementHeader>();
			stmtHeader.B2_Status = StatementHeaderStatusList.Codes.Final;
			stmtHeader.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;

			var stmtLine = Factory.NewWithValidTestData<CusStatementLine>();
			stmtLine.B3_B2 = stmtHeader.PK;
			stmtLine.B3_EntryNum = "78374859";

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var stmtStatusFilter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.StatementStatus];
			stmtStatusFilter.IsActive = true;
			stmtStatusFilter.Property = StatementHeaderStatusList.Codes.Final;

			var pymtStatusFilter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.PaymentStatus];
			pymtStatusFilter.IsActive = true;
			pymtStatusFilter.Property = PaymentStatusList.Codes.PaymentAuthorizationAccepted;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);

			AssertEquals("Should have found one declaration with both Statment & Payment Status filtered", 1, coll.Count);
			AssertEquals("Contains ENSDeclaration1 with payment status of PAA & statement status of FIN", true, coll.Contains(eNSDeclaration1));

			pymtStatusFilter.Property = PaymentStatusList.Codes.PaymentFailed;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should now not found any declaration as payment status changed", 0, coll.Count);
		}

		public void TestTotalPayableQuery()
		{
			CreateTestDecsWithStatus();
			var entry = eNSDeclaration1.ActiveEntryHeaders[0];
			AssertEquals("Should have entry summary", CusEntryHeaderMessageTypeList.Codes.EntrySummary, entry.CH_MessageType);
			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			entry.CH_TotalPaid = 7500m;
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var totalPayableFilter = (ModuleNumberRangeFilter)bizObj[DeclarationFilterConstants.TotalDutiesAndFees];
			totalPayableFilter.IsActive = true;
			totalPayableFilter.Property1 = 1000m;
			totalPayableFilter.Property2 = 10000m;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);

			AssertEquals("Should have found one declaration within payment range", 1, coll.Count);
			AssertEquals("Contains ENSDeclaration1", true, coll.Contains(eNSDeclaration1));

			totalPayableFilter.Property1 = 7501m;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should not have found any declarations", 0, coll.Count);

			totalPayableFilter.Property1 = 1000m;
			totalPayableFilter.Property2 = 7499m;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should not have found any declarations", 0, coll.Count);

			totalPayableFilter.Property1 = 0m;
			totalPayableFilter.Property2 = 7500m;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should have found one declaration within payment range", 1, coll.Count);

			var entry2 = eNSDeclaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry2.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			entry2.CH_TotalPaid = 0m;
			Factory.Save();

			var dutiesAndFeesQueryFilter = (ModuleNumberRangeFilter)bizObj[DeclarationFilterConstants.TotalDutiesAndFees];
			dutiesAndFeesQueryFilter.Property1 = 0m;
			dutiesAndFeesQueryFilter.Property2 = 9000m;
			dutiesAndFeesQueryFilter.IsActive = true;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("declaration1 is in Collection", true, coll.Contains(eNSDeclaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, coll.Contains(eNSDeclaration2.PK));

			var dutiesAndFeesQueryFilter2 = (ModuleNumberRangeFilter)bizObj[DeclarationFilterConstants.TotalDutiesAndFees];
			dutiesAndFeesQueryFilter2.Property1 = 8000m;
			dutiesAndFeesQueryFilter2.Property2 = 9000m;
			dutiesAndFeesQueryFilter2.IsActive = true;
			query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Should have found one declaration within payment range", 0, coll.Count);
		}

		public void TestPaymentDueDateFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_PaymentDueDate = new ZDateTime(2009, 8, 30);
			declaration1.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_PaymentDueDate = new ZDateTime(2009, 7, 15);
			declaration2.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var paymentDueDates = (ModuleDateFilter)bizObj[DeclarationFilterConstants.PaymentDueDate];
			paymentDueDates.IsActive = true;
			paymentDueDates.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			paymentDueDates.Property2 = new ZDateTime(2009, 8, 30);//to-date

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			paymentDueDates.Property1 = new ZDateTime(2009, 7, 31);//from-date
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("declaration2 should not be shown", false, coll.Contains(declaration2));
			AssertEquals("declaration1 should still be shown", true, coll.Contains(declaration1));

			declaration2.US_PaymentDueDate = new ZDateTime(2009, 8, 01); // re-calculation of payment due on this dec
			Factory.Save();
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found again", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found again", true, coll.Contains(declaration2));

			paymentDueDates.Property2 = ZDateTime.Empty;//clear out date values
			paymentDueDates.Property1 = ZDateTime.Empty;
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("All declarations should be found", 3, coll.Count);
			AssertEquals("declaration3 should now be shown as well", true, coll.Contains(declaration3));
		}

		public void TestTotalOutstandingFilter()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_DeclarationReference = "GAZ0000001";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			CreateTransactionData(declaration, 391.88);

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_DeclarationReference = "GAZ0000002";
			declaration2.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.JE_DeclarationReference = "GAZ0000003";
			CreateTransactionData(declaration3, 1500);
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var totalOutstanding = (ModuleNumberRangeFilter)bizObj[DeclarationFilterConstants.TotalOutstanding];
			totalOutstanding.IsActive = true;
			totalOutstanding.Property1 = 100m;
			totalOutstanding.Property2 = 1600m;

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration should be found", true, coll.Contains(declaration));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration3));

			totalOutstanding.Property1 = 400m;
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("Only 1 declaration should now be in this range & found", 1, coll.Count);
			AssertEquals("declaration should not be found", false, coll.Contains(declaration));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration3));

			totalOutstanding.Property2 = 1000m;
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("no declarations should now be in this range", 0, coll.Count);

			var declaration4 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration4.JE_DeclarationReference = "GAZ0000004";
			CreateTransactionData(declaration4, 0);
			Factory.Save();

			totalOutstanding.Property1 = 0m;
			totalOutstanding.Property2 = 0m;
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("no declarations should now be in this range", 1, coll.Count);
			AssertEquals("declaration4 should be found", true, coll.Contains(declaration4));
		}

		public void TestTotalInvoicedAmount()
		{
			CreateChgCodes();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_DeclarationReference = "GAZ0000001";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			var accTransHeader = CreateTransactionData(declaration, 400);
			CreateTransactionLineData(declaration, accTransHeader, 400);

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_DeclarationReference = "GAZ0000002";
			declaration2.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.JE_DeclarationReference = "GAZ0000003";
			var accTransHeader3 = CreateTransactionData(declaration3, 600);
			CreateTransactionLineData(declaration3, accTransHeader3, 600);
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var totalInvoiced = (ModuleNumberRangeFilter)bizObj[DeclarationFilterConstants.TotalInvoiced];
			totalInvoiced.IsActive = true;
			totalInvoiced.Property1 = 400m;
			totalInvoiced.Property2 = 600m;

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration should be found", true, coll.Contains(declaration));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration3));

			totalInvoiced.Property1 = 401m;
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("Only 1 declaration should now be in this range & found", 1, coll.Count);
			AssertEquals("declaration should not be found", false, coll.Contains(declaration));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration3));

			totalInvoiced.Property2 = 599m;
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("no declarations should now be in this range", 0, coll.Count);

			totalInvoiced.Property1 = 0m;
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("There are 2 declarations should now be in this range & found", 1, coll.Count);
			AssertEquals("declaration should not be found", false, coll.Contains(declaration2));
		}

		public void TestTotalBilledAmount()
		{
			CreateChgCodes();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_DeclarationReference = "GAZ0000001";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			var accTransHeader = CreateTransactionData(declaration, 400);
			CreateTransactionLineData(declaration, accTransHeader, 400);

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_DeclarationReference = "GAZ0000002";
			declaration2.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.JE_DeclarationReference = "GAZ0000003";
			var accTransHeader3 = CreateTransactionData(declaration3, 600);
			CreateTransactionLineData(declaration3, accTransHeader3, 600);
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var totalBilled = (ModuleNumberRangeFilter)bizObj[DeclarationFilterConstants.TotalBilled];
			totalBilled.IsActive = true;
			totalBilled.Property1 = 200m;
			totalBilled.Property2 = 300m;

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration should be found", true, coll.Contains(declaration));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration3));

			totalBilled.Property1 = 201m;
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("Only 1 declaration should now be in this range & found", 1, coll.Count);
			AssertEquals("declaration should not be found", false, coll.Contains(declaration));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration3));

			totalBilled.Property2 = 299m;
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("no declarations should now be in this range", 0, coll.Count);

			totalBilled.Property1 = 0m;
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("There are 2 declarations should now be in this range & found", 1, coll.Count);
			AssertEquals("declaration should not be found,because it hasn't billedamount", false, coll.Contains(declaration2));
		}

		public override void TestAgentReferenceFilters()
		{
			Assert(true); // Agent Reference not used in US system
		}

		public void TestStatusFilterOptions()
		{
			var cargoReleaseStatusFilter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.CargoReleaseStatus];
			AssertEquals("cargoReleaseStatus Filter list should have 3 options", 3, cargoReleaseStatusFilter.ComparisonOperator_List.Count);
			AssertEquals("cargoReleaseStatus Filter list should have 'exact' as default", ModuleTextFilter.ComparisonConstants.Exact, cargoReleaseStatusFilter.ComparisonOperator);
			AssertEquals("cargoReleaseStatus Filter list should have 'starts with' as another option", true, cargoReleaseStatusFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.StartsWith));
			AssertEquals("cargoReleaseStatus Filter list should have 'not equal' as another option", true, cargoReleaseStatusFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.NotEqual));

			var entrySummaryStatusFilter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.EntrySummaryStatus];
			AssertEquals("entrySummaryStatus Filter list should have 3 options", 3, entrySummaryStatusFilter.ComparisonOperator_List.Count);
			AssertEquals("entrySummaryStatus Filter list should have 'exact' as default", ModuleTextFilter.ComparisonConstants.Exact, entrySummaryStatusFilter.ComparisonOperator);
			AssertEquals("entrySummaryStatus Filter list should have 'starts with' as another option", true, entrySummaryStatusFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.StartsWith));
			AssertEquals("entrySummaryStatus Filter list should have 'not equal' as another option", true, entrySummaryStatusFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.NotEqual));

			var exportStatusFilter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.ExportStatus];
			AssertEquals("exportStatus Filter list should have 3 options", 3, exportStatusFilter.ComparisonOperator_List.Count);
			AssertEquals("exportStatus Filter list should have 'exact' as default", ModuleTextFilter.ComparisonConstants.Exact, exportStatusFilter.ComparisonOperator);
			AssertEquals("exportStatus Filter list should have 'starts with' as another option", true, exportStatusFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.StartsWith));
			AssertEquals("exportStatus Filter list should have 'not equal' as another option", true, exportStatusFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.NotEqual));

			var electronicInvoiceStatusFilter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.ElectronicInvoiceStatus];
			AssertEquals("electronicInvoiceStatus Filter list should have 3 options", 3, electronicInvoiceStatusFilter.ComparisonOperator_List.Count);
			AssertEquals("electronicInvoiceStatus Filter list should have 'exact' as default", ModuleTextFilter.ComparisonConstants.Exact, electronicInvoiceStatusFilter.ComparisonOperator);
			AssertEquals("electronicInvoiceStatus Filter list should have 'starts with' as another option", true, electronicInvoiceStatusFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.StartsWith));
			AssertEquals("electronicInvoiceStatus Filter list should have 'not equal' as another option", true, electronicInvoiceStatusFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.NotEqual));

			var bluMessageStatusFilter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.BLUMessageStatus];
			AssertEquals("bluMessageStatus Filter list should have 3 options", 3, bluMessageStatusFilter.ComparisonOperator_List.Count);
			AssertEquals("bluMessageStatus Filter list should have 'exact' as default", ModuleTextFilter.ComparisonConstants.Exact, bluMessageStatusFilter.ComparisonOperator);
			AssertEquals("bluMessageStatus Filter list should have 'starts with' as another option", true, bluMessageStatusFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.StartsWith));
			AssertEquals("bluMessageStatus Filter list should have 'not equal' as another option", true, bluMessageStatusFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.NotEqual));

			var fdaMsgStatusFilter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.FDAMsgStatus];
			AssertEquals("fdaMsgStatus Filter list should have 3 options", 3, fdaMsgStatusFilter.ComparisonOperator_List.Count);
			AssertEquals("fdaMsgStatus Filter list should have 'exact' as default", ModuleTextFilter.ComparisonConstants.Exact, fdaMsgStatusFilter.ComparisonOperator);
			AssertEquals("fdaMsgStatus Filter list should have 'starts with' as another option", true, fdaMsgStatusFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.StartsWith));
			AssertEquals("fdaMsgStatus Filter list should have 'not equal' as another option", true, fdaMsgStatusFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.NotEqual));

			var statementStatusFilter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.StatementStatus];
			AssertEquals("statementStatus Filter list should have 3 options", 3, statementStatusFilter.ComparisonOperator_List.Count);
			AssertEquals("statementStatus Filter list should have 'exact' as default", ModuleTextFilter.ComparisonConstants.Exact, statementStatusFilter.ComparisonOperator);
			AssertEquals("statementStatus Filter list should have 'starts with' as another option", true, statementStatusFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.StartsWith));
			AssertEquals("statementStatus Filter list should have 'not equal' as another option", true, statementStatusFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.NotEqual));

			var paymentStatusFilter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.PaymentStatus];
			AssertEquals("paymentStatu Filter list should have 3 options", 3, paymentStatusFilter.ComparisonOperator_List.Count);
			AssertEquals("paymentStatu Filter list should have 'exact' as default", ModuleTextFilter.ComparisonConstants.Exact, paymentStatusFilter.ComparisonOperator);
			AssertEquals("paymentStatu Filter list should have 'starts with' as another option", true, paymentStatusFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.StartsWith));
			AssertEquals("paymentStatu Filter list should have 'not equal' as another option", true, paymentStatusFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.NotEqual));
		}

		public void TestSchDLoadingFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.US_SchDLoading = "1523";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.US_SchDLoading = "2021";
			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.US_SchDLoading = "3030";
			var declaration4 = Factory.New<JobDeclaration>();
			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.LoadingSchedDK];
			filter.IsActive = true;
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals("Empty filter search should have found 4 declarations", 4, collection.Count);

			filter.Property = "1523";
			collection.Load(FilterBO.Filter);
			AssertEquals("search shuld have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration1 should be there", true, collection.Contains(declaration1));

			filter.Property = "2021";
			collection.Load(FilterBO.Filter);
			AssertEquals("should now have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration2 should be there", true, collection.Contains(declaration2));

			filter.Property = "3030";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			collection.Load(FilterBO.Filter);
			AssertEquals("search should have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration3 should be there", true, collection.Contains(declaration3));

			filter.Property = "";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			collection.Load(FilterBO.Filter);
			AssertEquals("search should have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration4 should be there", true, collection.Contains(declaration4));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			collection.Load(FilterBO.Filter);
			AssertEquals("search should have found 3 declarations", 3, collection.Count);
			AssertEquals("declaration1 should be there", true, collection.Contains(declaration1));
			AssertEquals("declaration2 should be there", true, collection.Contains(declaration2));
			AssertEquals("declaration3 should be there", true, collection.Contains(declaration2));
		}

		public void TestSchDArrivalFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.US_SchDArrival = "1500";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.US_SchDArrival = "2000";
			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.US_SchDArrival = "3000";
			var declaration4 = Factory.New<JobDeclaration>();
			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.DischargeSchedDK];
			filter.IsActive = true;
			filter.Property = ZString.Empty;
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals("Empty filter search should have found 4 declarations", 4, collection.Count);

			filter.Property = "1500";
			collection.Load(FilterBO.Filter);
			AssertEquals("search shuld have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration1 should be there", true, collection.Contains(declaration1));

			filter.Property = "2000";
			collection.Load(FilterBO.Filter);
			AssertEquals("should now have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration2 should be there", true, collection.Contains(declaration2));

			filter.Property = "3000";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			collection.Load(FilterBO.Filter);
			AssertEquals("search should have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration3 should be there", true, collection.Contains(declaration3));

			filter.Property = "";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			collection.Load(FilterBO.Filter);
			AssertEquals("search should have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration4 should be there", true, collection.Contains(declaration4));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			collection.Load(FilterBO.Filter);
			AssertEquals("search should have found 3 declarations", 3, collection.Count);
			AssertEquals("declaration1 should be there", true, collection.Contains(declaration1));
			AssertEquals("declaration2 should be there", true, collection.Contains(declaration2));
			AssertEquals("declaration3 should be there", true, collection.Contains(declaration3));
		}

		public void TestPaymentTypeFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration2.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration4.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.PaymentType];
			filter.IsActive = true;
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals("Empty Payment Type search should have found 4 declarations", 4, collection.Count);

			filter.Property = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			collection.Load(FilterBO.Filter);
			AssertEquals("BatchedByDailyPrintDateAndFilerCode search shuld have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration1 should be there", true, collection.Contains(declaration1));

			filter.Property = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			collection.Load(FilterBO.Filter);
			AssertEquals("BatchedByDailyPrintDateAndImporter should now have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration2 should be there", true, collection.Contains(declaration2));

			filter.Property = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			collection.Load(FilterBO.Filter);
			AssertEquals("BatchedByPeriodicPrintDateAndFilerDate search should have found 2 declarations", 2, collection.Count);
			AssertEquals("declaration3 should be there", true, collection.Contains(declaration3));
			AssertEquals("declaration4 should be there", true, collection.Contains(declaration4));
		}

		public void TestPaymentByBrokerQuery()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.BrokerToPayIndicator = YesNoDefaultList.Codes.Yes;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.BrokerToPayIndicator = YesNoDefaultList.Codes.Yes;

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.BrokerToPayIndicator = YesNoDefaultList.Codes.No;

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.BrokerToPayIndicator = YesNoDefaultList.Codes.No;

			var declaration5 = Factory.New<JobDeclaration>();
			declaration5.BrokerToPayIndicator = YesNoDefaultList.Codes.Default;

			var declaration6 = Factory.New<JobDeclaration>();
			declaration6.BrokerToPayIndicator = YesNoDefaultList.Codes.Default;

			var declaration7 = Factory.New<JobDeclaration>();
			declaration7.BrokerToPayIndicator = ZString.Empty;

			var declaration8 = Factory.New<JobDeclaration>();
			declaration8.BrokerToPayIndicator = ZString.Empty;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.PaymentByBroker];
			filter.IsActive = true;
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals("8 declarations should be found", 8, collection.Count);

			filter.Property = YesNoDefaultList.Codes.Yes;
			collection.Load(FilterBO.Filter);
			AssertEquals("'yes' should found 2 declarations", 2, collection.Count);
			AssertEquals("declaration1 should be there", true, collection.Contains(declaration1));
			AssertEquals("declaration2 should be there", true, collection.Contains(declaration2));

			filter.Property = YesNoDefaultList.Codes.No;
			collection.Load(FilterBO.Filter);
			AssertEquals("'no' should found 2 declarations", 2, collection.Count);
			AssertEquals("declaration3 should be there", true, collection.Contains(declaration3));
			AssertEquals("declaration4 should be there", true, collection.Contains(declaration4));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			collection.Load(FilterBO.Filter);
			AssertEquals("'empty value' should found 4 declarations", 4, collection.Count);
			AssertEquals("declaration5 should be there", true, collection.Contains(declaration5));
			AssertEquals("declaration6 should be there", true, collection.Contains(declaration6));
			AssertEquals("declaration7 should be there", true, collection.Contains(declaration7));
			AssertEquals("declaration8 should be there", true, collection.Contains(declaration8));
		}

		public void TestPreliminaryStatementPrintDateFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_PreliminaryStatementPrintDate = new ZDateTime(2009, 8, 30);
			declaration1.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_PreliminaryStatementPrintDate = new ZDateTime(2009, 7, 15);
			declaration2.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var prelimStmPrintDate = (ModuleDateFilter)bizObj[DeclarationFilterConstants.PrelimStatemPrintDate];
			prelimStmPrintDate.IsActive = true;
			prelimStmPrintDate.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			prelimStmPrintDate.Property2 = new ZDateTime(2009, 8, 30);//to-date

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			prelimStmPrintDate.Property1 = new ZDateTime(2009, 7, 31);//from-date
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("declaration2 should not be shown", false, coll.Contains(declaration2));
			AssertEquals("declaration1 should still be shown", true, coll.Contains(declaration1));

			declaration2.US_PreliminaryStatementPrintDate = new ZDateTime(2009, 8, 01); // re-calculation of payment due on this dec
			Factory.Save();
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found again", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found again", true, coll.Contains(declaration2));

			prelimStmPrintDate.Property2 = ZDateTime.Empty;//clear out date values
			prelimStmPrintDate.Property1 = ZDateTime.Empty;
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("All declarations should be found", 3, coll.Count);
			AssertEquals("declaration3 should now be shown as well", true, coll.Contains(declaration3));
		}

		public void TestEstimatedEntryDateFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_EstimatedEntryDate = new ZDateTime(2009, 8, 30);
			declaration1.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_EstimatedEntryDate = new ZDateTime(2009, 7, 15);
			declaration2.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var estimatedEntryDate = (ModuleDateFilter)bizObj[DeclarationFilterConstants.EstimatedEntryDate];
			estimatedEntryDate.IsActive = true;
			estimatedEntryDate.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			estimatedEntryDate.Property2 = new ZDateTime(2009, 8, 30);//to-date

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			estimatedEntryDate.Property1 = new ZDateTime(2009, 7, 31);//from-date
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("declaration2 should not be shown", false, coll.Contains(declaration2));
			AssertEquals("declaration1 should still be shown", true, coll.Contains(declaration1));

			declaration2.US_EstimatedEntryDate = new ZDateTime(2009, 8, 01); // re-calculation of payment due on this dec
			Factory.Save();
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found again", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found again", true, coll.Contains(declaration2));

			estimatedEntryDate.Property2 = ZDateTime.Empty;//clear out date values
			estimatedEntryDate.Property1 = ZDateTime.Empty;
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("All declarations should be found", 3, coll.Count);
			AssertEquals("declaration3 should now be shown as well", true, coll.Contains(declaration3));
		}

		public void TestElectronicInvoiceRequestedFilter()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec1.US_IsAIIRequested = true;

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec2.US_IsAIIRequested = true;

			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec3.US_IsAIIRequested = false;

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleFlagsFilter)bizObj[DeclarationFilterConstants.ElectronicInvoiceRequested];
			filter.IsActive = true;
			filter.Property0 = true; // AII Requested

			AssertEquals("declaration1 - AII requested", true, dec1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 - AII requested", true, dec2.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration3 - AII not requested", false, dec3.MatchesFilter(bizObj.Filter));

			filter.Property0 = false; // all declarations
			AssertEquals(true, dec1.MatchesFilter(bizObj.Filter));
			AssertEquals(true, dec2.MatchesFilter(bizObj.Filter));
			AssertEquals(true, dec3.MatchesFilter(bizObj.Filter));
		}

		public void TestReleaseStatusFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.ReleaseStatus = CRLReleaseStatusList.Codes.CAN;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.ReleaseStatus = CRLReleaseStatusList.Codes.DEL;

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.ReleaseStatus = CRLReleaseStatusList.Codes.EXM;

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.ReleaseStatus = CRLReleaseStatusList.Codes.HLD;

			var declaration5 = Factory.New<JobDeclaration>();
			declaration5.ReleaseStatus = CRLReleaseStatusList.Codes.NRL;

			var declaration6 = Factory.New<JobDeclaration>();
			declaration6.ReleaseStatus = CRLReleaseStatusList.Codes.REL;

			var declaration7 = Factory.New<JobDeclaration>();

			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.ReleaseStatus];
			filter.IsActive = true;
			filter.Property = ZString.Empty;

			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals("Empty search should have found all declarations", 7, collection.Count);

			filter.Property = CRLReleaseStatusList.Codes.CAN;
			collection.Load(FilterBO.Filter);
			AssertEquals(1, collection.Count);
			AssertEquals("declaration1 should be returned", true, collection.Contains(declaration1));

			filter.Property = CRLReleaseStatusList.Codes.DEL;
			collection.Load(FilterBO.Filter);
			AssertEquals(1, collection.Count);
			AssertEquals("declaration2 should be returned", true, collection.Contains(declaration2));

			filter.Property = CRLReleaseStatusList.Codes.EXM;
			collection.Load(FilterBO.Filter);
			AssertEquals(1, collection.Count);
			AssertEquals("declaration3 should be returned", true, collection.Contains(declaration3));

			filter.Property = CRLReleaseStatusList.Codes.HLD;
			collection.Load(FilterBO.Filter);
			AssertEquals(1, collection.Count);
			AssertEquals("declaration4 should be returned", true, collection.Contains(declaration4));

			filter.Property = CRLReleaseStatusList.Codes.NRL;
			collection.Load(FilterBO.Filter);
			AssertEquals(1, collection.Count);
			AssertEquals("declaration5 should be returned", true, collection.Contains(declaration5));

			filter.Property = CRLReleaseStatusList.Codes.REL;
			collection.Load(FilterBO.Filter);
			AssertEquals(1, collection.Count);
			AssertEquals("declaration6 should be returned", true, collection.Contains(declaration6));
		}

		public void TestExportDateFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration1.US_DateOfExport = new ZDateTime(2022, 12, 05);
			declaration1.JE_ExportDate = new ZDateTime(2022, 11, 25);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration2.JE_ExportDate = new ZDateTime(2022, 12, 15);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration3.US_DateOfExport = new ZDateTime(2022, 12, 25);

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Export;
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var exportDate = (ModuleDateFilter)bizObj[DeclarationFilterConstants.ExportDate];
			exportDate.IsActive = true;
			exportDate.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			exportDate.Property1 = new ZDateTime(2022, 12, 12); //from-date
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("2 declarations should be found", 2, coll.Count);
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration3));

			exportDate.Property1 = ZDateTime.Empty;
			exportDate.Property2 = new ZDateTime(2022, 12, 6); //to-date
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("1 declarations should be found", 1, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));

			exportDate.Property2 = new ZDateTime(2022, 12, 1);
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("0 declarations should be found", 0, coll.Count);

			exportDate.Property1 = ZDateTime.Empty; //clear out date values
			exportDate.Property2 = ZDateTime.Empty;
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("All declarations should be found", 4, coll.Count);
			AssertEquals("declaration1 should now be shown as well", true, coll.Contains(declaration1));
			AssertEquals("declaration4 should now be shown as well", true, coll.Contains(declaration4));

			exportDate.PropertySearch = ModuleDateFilter.HasDateEntered;
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("3 declarations should be found", 3, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration3));

			exportDate.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("1 declarations should be found", 1, coll.Count);
			AssertEquals("declaration4 should be found", true, coll.Contains(declaration4));
		}

		public void TestEntryDateFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_EntryDate = new ZDateTime(2009, 8, 30);
			declaration1.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_EntryDate = new ZDateTime(2009, 7, 15);
			declaration2.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var entryDate = (ModuleDateFilter)bizObj[DeclarationFilterConstants.EntryDate];
			entryDate.IsActive = true;
			entryDate.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			entryDate.Property2 = new ZDateTime(2009, 8, 30);//to-date

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = bizObj.Filter;
			coll.Load(query);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			entryDate.Property1 = new ZDateTime(2009, 7, 31);//from-date
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("declaration2 should not be shown", false, coll.Contains(declaration2));
			AssertEquals("declaration1 should still be shown", true, coll.Contains(declaration1));

			entryDate.Property2 = ZDateTime.Empty;//clear out date values
			entryDate.Property1 = ZDateTime.Empty;
			query = bizObj.Filter;
			coll.Load(bizObj.Filter);
			AssertEquals("All declarations should be found", 3, coll.Count);
			AssertEquals("declaration3 should now be shown as well", true, coll.Contains(declaration3));
		}

		public void TestAnticipatedLiquidationDateFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry1 = declaration1.ActiveEntryHeaders.AddNew();
			entry1.US_ALDate = ZDateTime.Today.AddDays(-1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry2 = declaration2.ActiveEntryHeaders.AddNew();

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry3 = declaration3.ActiveEntryHeaders.AddNew();
			entry3.US_ALDate = ZDateTime.Today.AddDays(-45);

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entry4 = declaration4.ActiveEntryHeaders.AddNew();

			var declaration5 = Factory.New<JobDeclaration>();
			declaration5.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration5.US_ALDate = ZDateTime.Today.AddDays(1);

			var declaration6 = Factory.New<JobDeclaration>();
			declaration6.JE_MessageType = JobMessageTypeList.Codes.Recon;
			var reconDeclaration = new ReconDeclaration(declaration6);
			var entry = reconDeclaration.ReconEntry.GetEntry();
			entry.US_ALDate = ZDateTime.Today.AddDays(2);

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var liqDate = (ModuleDateFilter)bizObj[DeclarationFilterConstants.AnticipLiquidationDate];
			liqDate.IsActive = true;
			liqDate.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			liqDate.Property2 = ZDateTime.Today;

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration3));

			liqDate.Property2 = ZDateTime.Today.AddDays(2);
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found", 4, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration3));
			AssertEquals("declaration5 should be found", true, coll.Contains(declaration5));
			AssertEquals("declaration6 should be found", true, coll.Contains(declaration6));

			liqDate.Property2 = ZDateTime.Empty;
			liqDate.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Last7Days;
			coll.Load(bizObj.Filter);
			AssertEquals("One declaration should be found", 1, coll.Count);
			AssertEquals("declaration1 should not be shown", true, coll.Contains(declaration1));

			liqDate.PropertySearch = ModuleDateFilter.HasDateEntered;
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found", 4, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration3));
			AssertEquals("declaration5 should be found", true, coll.Contains(declaration5));
			AssertEquals("declaration6 should be found", true, coll.Contains(declaration6));

			liqDate.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration2));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration4));
		}

		public void TestISFBillStatusForFTZAndIMX()
		{
			var declaration1WithS2 = Factory.New<JobDeclaration>();
			declaration1WithS2.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration1WithS2.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration1WithS2.JE_DeclarationReference = "B00001_S2";
			declaration1WithS2.JE_HouseBill = "BILL1";
			declaration1WithS2.PrimaryHouseBill.US_UI_NKBillIssuerSCAC = "XXXA";
			var isfXXXABill1S2 = CreateISFJob(ZDateTime.Now.AddSeconds(-15), "XXXABILL1", BillTypeList.Codes.HouseBillOfLading, DispositionCodeList.Codes.S2);
			Factory.Save();
			AssertEquals("PreCondition", DispositionCodeList.Codes.S2, declaration1WithS2.ISFBillStatus);
			var filter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.ISFBillStatus];
			var comparisonOperatorList = filter.ComparisonOperator_List;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			var results = Factory.Load<JobDeclaration>(filter.Query);
			AssertCollectionContains(declaration1WithS2, results);

			declaration1WithS2.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			Factory.Save();
			results = Factory.Load<JobDeclaration>(filter.Query);
			AssertCollectionContains(declaration1WithS2, results);

			declaration1WithS2.JE_MessageType = JobMessageTypeList.Codes.Miscellaneous;
			Factory.Save();
			results = Factory.Load<JobDeclaration>(filter.Query);
			AssertCollectionContains(declaration1WithS2, results);

			declaration1WithS2.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.Save();
			results = Factory.Load<JobDeclaration>(filter.Query);
			AssertCollectionContains(declaration1WithS2, results);
		}

		public void TestGetISFBillDataSearchIn6Month()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec1.JE_TransportMode = Core.Constants.TransportModes.Sea;
			dec1.JE_DeclarationReference = "B00003";
			dec1.JE_HouseBill = "BILL3";
			dec1.PrimaryHouseBill.US_UI_NKBillIssuerSCAC = "XXXB";
			var dec1bill1 = dec1.Bills.AddNew();
			dec1bill1.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			dec1bill1.CU_BillNum = "BILL4";
			dec1bill1.US_UI_NKBillIssuerSCAC = "XXXB";
			var isfCreateTime = ZDateTime.Now;
			CreateISFJob(isfCreateTime.AddYears(-2), "XXXBBILL4", BillTypeList.Codes.HouseBillOfLading, DispositionCodeList.Codes.S3);

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec2.JE_TransportMode = Core.Constants.TransportModes.Sea;
			dec2.JE_DeclarationReference = "B00001";
			dec2.JE_HouseBill = "BILL1";
			dec2.PrimaryHouseBill.US_UI_NKBillIssuerSCAC = "XXXA";
			CreateISFJob(isfCreateTime.AddSeconds(-15), "XXXABILL1", BillTypeList.Codes.OceanBillOfLading, DispositionCodeList.Codes.S1);

			Factory.Save();

			AssertEquals("No search result", ZString.Empty, dec1.ISFBillStatus);
			AssertEquals("S1", dec2.ISFBillStatus);
		}

		public void TestISFBillStatusFilter()
		{
			var existingDeclarations = Factory.Load<JobDeclaration>(new ZQuery() { IgnoreActiveFilter = true });
			var declaration1WithS2 = Factory.New<JobDeclaration>();
			declaration1WithS2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1WithS2.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration1WithS2.JE_DeclarationReference = "B00001_S2";
			declaration1WithS2.JE_HouseBill = "BILL1";
			declaration1WithS2.PrimaryHouseBill.US_UI_NKBillIssuerSCAC = "XXXA";
			var isfCreateTime = ZDateTime.Now;
			var isfXXXABill1S2 = CreateISFJob(isfCreateTime.AddSeconds(-15), "XXXABILL1", BillTypeList.Codes.HouseBillOfLading, DispositionCodeList.Codes.S2);

			var declaration2WithS3 = Factory.New<JobDeclaration>();
			declaration2WithS3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2WithS3.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration2WithS3.JE_DeclarationReference = "B00002_S3";
			declaration2WithS3.JE_HouseBill = "BILL2";
			declaration2WithS3.PrimaryHouseBill.US_UI_NKBillIssuerSCAC = "XXXB";
			var isfXXXBBill2S3 = CreateISFJob(isfCreateTime.AddSeconds(-14), "XXXBBILL2", BillTypeList.Codes.HouseBillOfLading, DispositionCodeList.Codes.S3);

			var declaration3WithS2_S3 = Factory.New<JobDeclaration>();
			declaration3WithS2_S3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3WithS2_S3.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration3WithS2_S3.JE_DeclarationReference = "B00003_S2_S3";
			declaration3WithS2_S3.JE_HouseBill = "BILL3";
			declaration3WithS2_S3.PrimaryHouseBill.US_UI_NKBillIssuerSCAC = "XXXB";
			var declaration3WithS2_S3Bill2 = declaration3WithS2_S3.Bills.AddNew();
			declaration3WithS2_S3Bill2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			declaration3WithS2_S3Bill2.CU_BillNum = "BILL4";
			declaration3WithS2_S3Bill2.US_UI_NKBillIssuerSCAC = "XXXB";
			var isfXXXBBill3S2 = CreateISFJob(isfCreateTime.AddSeconds(-13), "XXXBBILL3", BillTypeList.Codes.HouseBillOfLading, DispositionCodeList.Codes.S2);
			var isfXXXBBill4S3 = CreateISFJob(isfCreateTime.AddSeconds(-12), "XXXBBILL4", BillTypeList.Codes.HouseBillOfLading, DispositionCodeList.Codes.S3);

			var declaration4WithS4_Blank = Factory.New<JobDeclaration>();
			declaration4WithS4_Blank.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration4WithS4_Blank.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration4WithS4_Blank.JE_DeclarationReference = "B00004_S4_BLANK";
			declaration4WithS4_Blank.JE_HouseBill = "BILL5";
			declaration4WithS4_Blank.PrimaryHouseBill.US_UI_NKBillIssuerSCAC = "XXXC";
			var declaration4WithS4_BlankBillBlank = declaration4WithS4_Blank.Bills.AddNew();
			declaration4WithS4_BlankBillBlank.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			declaration4WithS4_BlankBillBlank.CU_BillNum = "BILL6";
			declaration4WithS4_BlankBillBlank.US_UI_NKBillIssuerSCAC = "XXXC";
			var isfXXXBBILL5S4 = CreateISFJob(isfCreateTime.AddSeconds(-11), "XXXCBILL5", BillTypeList.Codes.HouseBillOfLading, DispositionCodeList.Codes.S4);

			var declaration5WithMasterAndHouseMatch = Factory.New<JobDeclaration>();
			declaration5WithMasterAndHouseMatch.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration5WithMasterAndHouseMatch.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration5WithMasterAndHouseMatch.JE_DeclarationReference = "B00007_NoOB_HBMatch";
			declaration5WithMasterAndHouseMatch.JE_MasterBill = "BILL7";
			declaration5WithMasterAndHouseMatch.PrimaryMasterBill.US_UI_NKBillIssuerSCAC = "XXXB";
			declaration5WithMasterAndHouseMatch.JE_HouseBill = "BILL8";
			declaration5WithMasterAndHouseMatch.PrimaryHouseBill.US_UI_NKBillIssuerSCAC = "XXXB";
			var isfXXXBBILL7S5 = CreateISFJob(isfCreateTime.AddSeconds(-10), "XXXBBILL7", BillTypeList.Codes.OceanBillOfLading, DispositionCodeList.Codes.S5);
			var isfXXXBBILL8S6 = CreateISFJob(isfCreateTime.AddSeconds(-9), "XXXBBILL8", BillTypeList.Codes.HouseBillOfLading, DispositionCodeList.Codes.S6);

			var airDeclarationThatShouldNotMatchS2 = Factory.New<JobDeclaration>();
			airDeclarationThatShouldNotMatchS2.JE_MessageType = JobMessageTypeList.Codes.Import;
			airDeclarationThatShouldNotMatchS2.JE_TransportMode = Core.Constants.TransportModes.Air;
			airDeclarationThatShouldNotMatchS2.JE_DeclarationReference = "B00008_AIR";
			airDeclarationThatShouldNotMatchS2.JE_HouseBill = "BILL9";
			airDeclarationThatShouldNotMatchS2.PrimaryHouseBill.US_UI_NKBillIssuerSCAC = "XXXA";
			var isfXXXBBILL9SS = CreateISFJob(isfCreateTime.AddSeconds(-8), "XXXABILL9", BillTypeList.Codes.HouseBillOfLading, DispositionCodeList.Codes.S2);

			var exportDeclarationThatShouldNotMatchS2 = Factory.New<JobDeclaration>();
			exportDeclarationThatShouldNotMatchS2.JE_MessageType = JobMessageTypeList.Codes.Export;
			exportDeclarationThatShouldNotMatchS2.JE_TransportMode = Core.Constants.TransportModes.Sea;
			exportDeclarationThatShouldNotMatchS2.JE_DeclarationReference = "B00009_EXPORT";
			exportDeclarationThatShouldNotMatchS2.JE_HouseBill = "BILL10";
			exportDeclarationThatShouldNotMatchS2.PrimaryHouseBill.US_UI_NKBillIssuerSCAC = "XXXB";
			var isfXXXBBILL10S2 = CreateISFJob(isfCreateTime.AddSeconds(-7), "XXXBBILL10", BillTypeList.Codes.HouseBillOfLading, DispositionCodeList.Codes.S2);

			var declarationTooOld = Factory.New<JobDeclaration>();
			declarationTooOld.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationTooOld.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declarationTooOld.JE_DeclarationReference = "B00010_OLD";
			declarationTooOld.JE_HouseBill = "BILL11";
			declarationTooOld.PrimaryHouseBill.US_UI_NKBillIssuerSCAC = "XXXA";
			declarationTooOld.JE_SystemCreateTimeUtc = ZDateTime.Now.AddYears(-2);
			var isfXXXABILL11S2 = CreateISFJob(isfCreateTime.AddSeconds(-6), "XXXABILL11", BillTypeList.Codes.HouseBillOfLading, DispositionCodeList.Codes.S2);

			var declarationWithNoISFMatch = Factory.New<JobDeclaration>();
			declarationWithNoISFMatch.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationWithNoISFMatch.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declarationWithNoISFMatch.JE_DeclarationReference = "B00011_NOMATCH";
			declarationWithNoISFMatch.JE_HouseBill = "BILL12";
			declarationWithNoISFMatch.PrimaryHouseBill.US_UI_NKBillIssuerSCAC = "XXXC";

			var declarationWithNoBill = Factory.New<JobDeclaration>();
			declarationWithNoBill.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationWithNoBill.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declarationWithNoBill.JE_DeclarationReference = "B00012_NOBILL";
			Factory.Save();

			AssertEquals("PreCondition", DispositionCodeList.Codes.S2, declaration1WithS2.ISFBillStatus);
			AssertEquals("PreCondition", DispositionCodeList.Codes.S3, declaration2WithS3.ISFBillStatus);
			AssertEquals("PreCondition", ISFStatusHelper.Multiple, declaration3WithS2_S3.ISFBillStatus);
			AssertEquals("PreCondition", DispositionCodeList.Codes.S4 + ",No Status", declaration4WithS4_Blank.ISFBillStatus);
			AssertEquals("PreCondition: Only match to lowest bill", DispositionCodeList.Codes.S6, declaration5WithMasterAndHouseMatch.ISFBillStatus);
			AssertEquals("PreCondition: Only Sea jobs should be matched", ZString.Empty, airDeclarationThatShouldNotMatchS2.ISFBillStatus);
			AssertEquals("PreCondition: Only Import jobs should be matched", ZString.Empty, exportDeclarationThatShouldNotMatchS2.ISFBillStatus);
			AssertEquals("PreCondition: Old jobs are ignored", ZString.Empty, declarationTooOld.ISFBillStatus);
			AssertEquals("PreCondition: No Matching", ZString.Empty, declarationWithNoISFMatch.ISFBillStatus);

			var filter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.ISFBillStatus];
			var comparisonOperatorList = filter.ComparisonOperator_List;
			AssertEquals(4, comparisonOperatorList.Count);
			AssertEquals(true, comparisonOperatorList.ContainsCode(ModuleTextFilter.ComparisonConstants.Exact));
			AssertEquals(true, comparisonOperatorList.ContainsCode(ModuleTextFilter.ComparisonConstants.NotEqual));
			AssertEquals(true, comparisonOperatorList.ContainsCode(ModuleTextFilter.ComparisonConstants.IsBlank));
			AssertEquals(true, comparisonOperatorList.ContainsCode(ModuleTextFilter.ComparisonConstants.IsNotBlank));
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;

			var results = Factory.Load<JobDeclaration>(filter.Query);
			AssertEquals(5, results.Length);
			AssertCollectionContains(declaration1WithS2, results);
			AssertCollectionContains(declaration2WithS3, results);
			AssertCollectionContains(declaration3WithS2_S3, results);
			AssertCollectionContains(declaration4WithS4_Blank, results);
			AssertCollectionContains(declaration5WithMasterAndHouseMatch, results);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			results = Factory.Load<JobDeclaration>(filter.Query);
			AssertEquals(5 + existingDeclarations.Length, results.Length);
			AssertCollectionContains(airDeclarationThatShouldNotMatchS2, results);
			AssertCollectionContains(exportDeclarationThatShouldNotMatchS2, results);
			AssertCollectionContains(declarationTooOld, results);
			AssertCollectionContains(declarationWithNoISFMatch, results);
			AssertCollectionContains(declarationWithNoBill, results);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = DispositionCodeList.Codes.S2;
			results = Factory.Load<JobDeclaration>(filter.Query);
			AssertEquals(2, results.Length);
			AssertCollectionContains(declaration1WithS2, results);
			AssertCollectionContains(declaration3WithS2_S3, results);

			filter.Property = DispositionCodeList.Codes.S3;
			results = Factory.Load<JobDeclaration>(filter.Query);
			AssertEquals(2, results.Length);
			AssertCollectionContains(declaration2WithS3, results);
			AssertCollectionContains(declaration3WithS2_S3, results);

			filter.Property = DispositionCodeList.Codes.S4;
			results = Factory.Load<JobDeclaration>(filter.Query);
			AssertEquals(1, results.Length);
			AssertEquals(declaration4WithS4_Blank, results[0]);

			filter.Property = DispositionCodeList.Codes.S5;
			results = Factory.Load<JobDeclaration>(filter.Query);
			AssertEquals("Should not match to master bill when house bill exists", 0, results.Length);

			filter.Property = DispositionCodeList.Codes.S6;
			results = Factory.Load<JobDeclaration>(filter.Query);
			AssertEquals(1, results.Length);
			AssertEquals(declaration5WithMasterAndHouseMatch, results[0]);

			filter.Property = ISFStatusHelper.Multiple;
			results = Factory.Load<JobDeclaration>(filter.Query);
			AssertEquals(2, results.Length);
			AssertCollectionContains(declaration3WithS2_S3, results);
			AssertCollectionContains(declaration4WithS4_Blank, results);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter.Property = DispositionCodeList.Codes.S2;
			results = Factory.Load<JobDeclaration>(filter.Query);
			AssertEquals(9, results.Length);
			AssertCollectionNotContains(declaration1WithS2, results);
			AssertCollectionContains(declaration2WithS3, results);
			AssertCollectionContains("Declaration should matched as one of the Bill is not S2", declaration3WithS2_S3, results);
			AssertCollectionContains(declaration4WithS4_Blank, results);
			AssertCollectionContains(declaration5WithMasterAndHouseMatch, results);
			AssertCollectionContains(airDeclarationThatShouldNotMatchS2, results);
			AssertCollectionContains(exportDeclarationThatShouldNotMatchS2, results);
			AssertCollectionContains(declarationTooOld, results);
			AssertCollectionContains(declarationWithNoISFMatch, results);
			AssertCollectionContains(declarationWithNoBill, results);

			filter.Property = ISFStatusHelper.Multiple;
			results = Factory.Load<JobDeclaration>(filter.Query);
			AssertEquals(8, results.Length);
			AssertCollectionNotContains(declaration3WithS2_S3, results);
			AssertCollectionNotContains(declaration4WithS4_Blank, results);
			AssertCollectionContains(declaration1WithS2, results);
			AssertCollectionContains(declaration2WithS3, results);
			AssertCollectionContains(declaration5WithMasterAndHouseMatch, results);
			AssertCollectionContains(airDeclarationThatShouldNotMatchS2, results);
			AssertCollectionContains(exportDeclarationThatShouldNotMatchS2, results);
			AssertCollectionContains(declarationTooOld, results);
			AssertCollectionContains(declarationWithNoISFMatch, results);
			AssertCollectionContains(declarationWithNoBill, results);
		}

		public void TestApplicationCodeFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var declaration5 = Factory.New<JobDeclaration>();
			declaration5.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			Factory.Save();

			var appCode = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.ApplicationCode];
			appCode.IsActive = true;

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);

			appCode.Property = JobApplicationCodeList.Codes.ACE;
			coll.Load(FilterBO.Filter);
			AssertEquals("Three declarations should be found", 3, coll.Count);
			AssertEquals("declaration 1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration 2 should be found", true, coll.Contains(declaration2));
			AssertEquals("declaration 3 should be found", true, coll.Contains(declaration3));

			appCode.Property = JobApplicationCodeList.Codes.ACS;
			coll.Load(FilterBO.Filter);
			AssertEquals("Three declarations should be found", 2, coll.Count);
			AssertEquals("declaration 4 should be found", true, coll.Contains(declaration4));
			AssertEquals("declaration 5 should be found", true, coll.Contains(declaration5));
		}

		public void TestNotifyPartyFilter()
		{
			var notifyParty1 = Factory.NewWithValidTestData<OrgHeader>();
			var notifyParty2 = Factory.NewWithValidTestData<OrgHeader>();

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_OH_NotifyParty = notifyParty1.PK;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_OH_NotifyParty = notifyParty1.PK;

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_OH_NotifyParty = notifyParty2.PK;

			Factory.Save();

			var notifyPartyFilter = (ModuleGuidFilter)FilterBO[DeclarationFilterConstants.NotifyParty];
			notifyPartyFilter.IsActive = true;

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);

			notifyPartyFilter.Property = notifyParty1.PK;
			coll.Load(FilterBO.Filter);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration 1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration 2 should be found", true, coll.Contains(declaration2));

			notifyPartyFilter.Property = notifyParty2.PK;
			coll.Load(FilterBO.Filter);
			AssertEquals("One declarations should be found", 1, coll.Count);
			AssertEquals("declaration 3 should be found", true, coll.Contains(declaration3));
		}

		public void TestSoldToPartyFilter()
		{
			var soldToParty1 = Factory.NewWithValidTestData<OrgHeader>();
			var soldToParty2 = Factory.NewWithValidTestData<OrgHeader>();

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_OA_SoldToPartyAddress = soldToParty1.MainAddress.PK;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_OA_SoldToPartyAddress = soldToParty1.MainAddress.PK;

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_OA_SoldToPartyAddress = soldToParty2.MainAddress.PK;

			Factory.Save();

			var notifyPartyFilter = (ModuleGuidFilter)FilterBO[DeclarationFilterConstants.SoldToParty];
			notifyPartyFilter.IsActive = true;

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);

			notifyPartyFilter.Property = soldToParty1.PK;
			coll.Load(FilterBO.Filter);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration 1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration 2 should be found", true, coll.Contains(declaration2));

			notifyPartyFilter.Property = soldToParty2.PK;
			coll.Load(FilterBO.Filter);
			AssertEquals("One declarations should be found", 1, coll.Count);
			AssertEquals("declaration 3 should be found", true, coll.Contains(declaration3));
		}

		public void TestPSCFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.US_PSC = true;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.US_PSC = true;

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.US_PSC = false;

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.US_PSC = false;

			var declaration5 = Factory.New<JobDeclaration>();
			declaration5.US_PSC = false;

			Factory.Save();

			var pscFilter = (ModuleFlagsFilter)FilterBO[DeclarationFilterConstants.PSCIndicator];
			pscFilter.IsActive = true;

			pscFilter.Property0 = true;
			Assert("Dec1", declaration1.MatchesFilter(FilterBO.Filter));
			Assert("Dec2", declaration2.MatchesFilter(FilterBO.Filter));
			Assert("Dec3", !declaration3.MatchesFilter(FilterBO.Filter));
			Assert("Dec4", !declaration4.MatchesFilter(FilterBO.Filter));
			Assert("Dec5", !declaration5.MatchesFilter(FilterBO.Filter));

			// if unticked, return all
			pscFilter.Property0 = false;
			Assert("Dec1", declaration1.MatchesFilter(FilterBO.Filter));
			Assert("Dec2", declaration2.MatchesFilter(FilterBO.Filter));
			Assert("Dec3", declaration3.MatchesFilter(FilterBO.Filter));
			Assert("Dec4", declaration4.MatchesFilter(FilterBO.Filter));
			Assert("Dec5", declaration5.MatchesFilter(FilterBO.Filter));
		}

		public void TestPresentationDateFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.US_PresentationDate = new ZDateTime(2011, 08, 31);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.US_PresentationDate = new ZDateTime(2011, 07, 31);

			var declaration3 = Factory.New<JobDeclaration>();

			Factory.Save();

			var presentationDateFilter = (ModuleDateFilter)FilterBO[DeclarationFilterConstants.PresentationDate];
			presentationDateFilter.IsActive = true;
			presentationDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);

			presentationDateFilter.Property2 = new ZDateTime(2011, 08, 31); // to-date
			coll.Load(FilterBO.Filter);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			presentationDateFilter.Property1 = new ZDateTime(2011, 08, 01); // from-date
			coll.Load(FilterBO.Filter);
			AssertEquals("1 declaration should be found", 1, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should not be found", false, coll.Contains(declaration2));

			presentationDateFilter.Property1 = ZDateTime.Empty;
			presentationDateFilter.Property2 = ZDateTime.Empty;
			coll.Load(FilterBO.Filter);
			AssertEquals("All declarations should be found", 3, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration3));
		}

		public void TestUltimateConsigneeFilter()
		{
			var ultimateConsignee1 = Factory.NewWithValidTestData<OrgHeader>();
			var ultimateConsignee2 = Factory.NewWithValidTestData<OrgHeader>();

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_OA_ConsigneeAddress = ultimateConsignee1.MainAddress.PK;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_OA_ConsigneeAddress = ultimateConsignee1.MainAddress.PK;

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_OA_ConsigneeAddress = ultimateConsignee2.MainAddress.PK;

			Factory.Save();

			var ultimateConsigneeFilter = (ModuleGuidFilter)FilterBO[DeclarationFilterConstants.UltinateConsignee];
			ultimateConsigneeFilter.IsActive = true;

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);

			ultimateConsigneeFilter.Property = ultimateConsignee1.PK;
			coll.Load(FilterBO.Filter);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration 1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration 2 should be found", true, coll.Contains(declaration2));

			ultimateConsigneeFilter.Property = ultimateConsignee2.PK;
			coll.Load(FilterBO.Filter);
			AssertEquals("One declarations should be found", 1, coll.Count);
			AssertEquals("declaration 3 should be found", true, coll.Contains(declaration3));
		}

		public void TestCensusWarningOverrideFilter()
		{
			var dec1 = Factory.New<JobDeclaration>();
			var invoiceHeader1 = dec1.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			var cusCode1 = invoiceLine1.FeeCusCodes.AddNew();
			cusCode1.CY_Type = "CWO";
			cusCode1.CY_Code = "27D";
			cusCode1.CY_Data = "03";

			var dec2 = Factory.New<JobDeclaration>();
			var invoiceHeader2 = dec2.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			var cusCode2 = invoiceLine2.FeeCusCodes.AddNew();

			var dec3 = Factory.New<JobDeclaration>();

			Factory.Save();

			var censusWarningsOverridenFilter = (ModuleFlagsFilter)FilterBO[DeclarationFilterConstants.CensusWarningsOverriden];
			censusWarningsOverridenFilter.IsActive = true;

			censusWarningsOverridenFilter.Property0 = true;
			AssertEquals("Dec1", dec1.MatchesFilter(FilterBO.Filter), true);
			AssertEquals("Dec2", dec2.MatchesFilter(FilterBO.Filter), false);
			AssertEquals("Dec3", dec3.MatchesFilter(FilterBO.Filter), false);

			censusWarningsOverridenFilter.Property0 = false;
			AssertEquals("Dec1", dec1.MatchesFilter(FilterBO.Filter), true);
			AssertEquals("Dec2", dec2.MatchesFilter(FilterBO.Filter), true);
			AssertEquals("Dec3", dec3.MatchesFilter(FilterBO.Filter), true);
		}

		public void TestDeferredTaxDueDateFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_DeferredTaxDueDate = new ZDateTime(2013, 8, 30);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_DeferredTaxDueDate = new ZDateTime(2013, 11, 29);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleDateFilter)bizObj[DeclarationFilterConstants.DeferredTaxDueDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property2 = new ZDateTime(2013, 8, 30);//to-date

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(bizObj.Filter);
			AssertEquals("One declaration should be found", 1, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should not be found", false, coll.Contains(declaration2));

			filter.Property2 = new ZDateTime(2013, 11, 30);//to-date
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			filter.Property1 = new ZDateTime(2013, 09, 27);//from-date
			coll.Load(bizObj.Filter);
			AssertEquals("One declaration should be found", 1, coll.Count);
			AssertEquals("declaration1 should not be found", false, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			filter.Property2 = ZDateTime.Empty;//clear out date values
			filter.Property1 = ZDateTime.Empty;
			coll.Load(bizObj.Filter);
			AssertEquals("All declarations should be found", 3, coll.Count);
			AssertEquals("declaration3 should now be shown as well", true, coll.Contains(declaration3));

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			coll.Load(bizObj.Filter);
			AssertEquals("One declarations should be found", 1, coll.Count);
			AssertEquals("declaration1 should not be found", false, coll.Contains(declaration1));
			AssertEquals("declaration2 should not be found", false, coll.Contains(declaration2));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration3));
		}

		public void TestDeferredIndicatorFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTax;
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.NotApplicableOrNoDeferredTax;
			declaration2.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.BulkLiquorDeferred;
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.BulkLiquorDeferred;
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Miscellaneous;

			var declaration5 = Factory.New<JobDeclaration>();
			declaration5.JE_MessageType = JobMessageTypeList.Codes.Import;

			var declaration6 = Factory.New<JobDeclaration>();
			declaration6.JE_MessageType = JobMessageTypeList.Codes.Export;

			var declaration7 = Factory.New<JobDeclaration>();
			declaration7.JE_MessageType = JobMessageTypeList.Codes.FTZ;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.DeferredIndicator];
			filter.IsActive = true;
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals("Search should have found 5 declarations", 7, collection.Count);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter.Property = TaxDeferIndicatorList.Codes.DeferredTax;
			collection.Load(FilterBO.Filter);
			AssertEquals("Search should have found 4 declarations", 4, collection.Count);
			AssertEquals("declaration should be there", true, collection.Contains(declaration2));
			AssertEquals("declaration should be there", true, collection.Contains(declaration3));
			AssertEquals("declaration should be there", true, collection.Contains(declaration4));
			AssertEquals("declaration should be there", true, collection.Contains(declaration5));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = TaxDeferIndicatorList.Codes.BulkLiquorDeferred;
			collection.Load(FilterBO.Filter);
			AssertEquals("Search shuld have found 2 declarations", 2, collection.Count);
			AssertEquals("declaration should be there", true, collection.Contains(declaration3));
			AssertEquals("declaration should be there", true, collection.Contains(declaration4));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter.Property = TaxDeferIndicatorList.Codes.BulkLiquorDeferred;
			collection.Load(FilterBO.Filter);
			AssertEquals("Should now have found 3 declarations", 3, collection.Count);
			AssertEquals("declaration should be there", true, collection.Contains(declaration1));
			AssertEquals("declaration should be there", true, collection.Contains(declaration2));
			AssertEquals("declaration should be there", true, collection.Contains(declaration5));
		}

		public void TestImportSpecialistTeamFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_TeamNo = "807";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_TeamNo = "106";

			var declaration3 = Factory.New<JobDeclaration>();

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Recon;
			declaration4.US_TeamNo = "108";

			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.ImportSpecialistTeam];
			filter.IsActive = true;
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals("Empty filter search should have found 4 declarations", 4, collection.Count);

			filter.Property = "807";
			collection.Load(FilterBO.Filter);
			AssertEquals("search shuld have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration1 should be there", true, collection.Contains(declaration1));

			filter.Property = "1";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			collection.Load(FilterBO.Filter);
			AssertEquals("search should have found 1 declaration", 2, collection.Count);
			AssertEquals("declaration2 should be there", true, collection.Contains(declaration2));
			AssertEquals("declaration4 should be there", true, collection.Contains(declaration4));

			filter.Property = "";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			collection.Load(FilterBO.Filter);
			AssertEquals("search should have found 1 declaration", 1, collection.Count);
			AssertEquals("declaration3 should be there", true, collection.Contains(declaration3));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			collection.Load(FilterBO.Filter);
			AssertEquals("search should have found 3 declarations", 3, collection.Count);
			AssertEquals("declaration1 should be there", true, collection.Contains(declaration1));
			AssertEquals("declaration2 should be there", true, collection.Contains(declaration2));
			AssertEquals("declaration3 should not be there", false, collection.Contains(declaration3));
			AssertEquals("declaration4 should be there", true, collection.Contains(declaration4));
		}

		public void TestCargoReleaseTypeFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Export;

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.CargoReleaseType];
			AssertEquals(FilterCategories.ModesAndTypes, filter.Category);
			filter.IsActive = true;
			filter.Property = CargoReleaseTypeList.Codes.SE;
			AssertEquals("declaration2 matches filter", true, declaration2.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration1 does not match filter", false, declaration1.MatchesFilter(bizObj.Filter));

			filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.CargoReleaseType];
			filter.IsActive = true;
			filter.Property = CargoReleaseTypeList.Codes.CR;
			AssertEquals("declaration1 does not match filter", false, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 does not match filter", false, declaration2.MatchesFilter(bizObj.Filter));
		}

		public void TestContainsSplitShipmentsFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var bill1 = declaration1.Bills.AddNew();
			var bill2 = declaration1.Bills.AddNew();
			bill2.US_SESplitShip = true;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var bill1_2 = declaration2.Bills.AddNew();
			bill1_2.US_SESplitShip = true;

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var bill1_3 = declaration3.Bills.AddNew();

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Export;

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleFlagsFilter)bizObj[DeclarationFilterConstants.ContainsSplitShipments];
			filter.IsActive = true;

			filter.Property0 = true;
			AssertEquals("declaration1 matches filter", true, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 matches filter", true, declaration2.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration3 does not match filter", false, declaration3.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration4 does not match filter", false, declaration4.MatchesFilter(bizObj.Filter));

			filter.Property0 = false;
			AssertEquals("declaration1 matches filter", true, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 matches filter", true, declaration2.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration3 matches filter", true, declaration3.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration4 matches filter", true, declaration4.MatchesFilter(bizObj.Filter));
		}

		public void TestFilingOption()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration1.US_CommodityFilingOption = AESCommodityFilingOptionList.Codes._2Predeparture;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration2.US_CommodityFilingOption = AESCommodityFilingOptionList.Codes._4Postdeparture;

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.FilingOption];
			filter.IsActive = true;
			filter.Property = AESCommodityFilingOptionList.Codes._4Postdeparture;
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals(1, collection.Count);
			Assert(collection.Contains(declaration2));

			filter.Property = AESCommodityFilingOptionList.Codes._2Predeparture;
			collection.Load(FilterBO.Filter);
			AssertEquals(1, collection.Count);
		}

		[TestDate(2014, 03, 03)]
		public void TestInvoiceExportDate()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration1.US_DateOfExport = ZDateTime.Today;
			declaration1.Invoices.AddNew();
			declaration1.Invoices[0].InvoiceLines.AddNew();

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration2.US_DateOfExport = ZDateTime.Today.AddDays(-10);
			declaration2.Invoices.AddNew();
			declaration2.Invoices[0].InvoiceLines.AddNew();

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration3.JE_ExportDate = ZDateTime.Today;
			declaration3.Invoices.AddNew();
			declaration3.Invoices[0].InvoiceLines.AddNew();

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration4.Invoices.AddNew();
			declaration4.Invoices[0].InvoiceLines.AddNew();

			var declaration5 = Factory.New<JobDeclaration>();
			declaration5.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration5.Invoices.AddNew();
			declaration5.Invoices[0].InvoiceLines.AddNew();
			Factory.Save();

			var filter = (ModuleDateFilter)FilterBO[DeclarationFilterConstants.InvoiceExportDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(FilterBO.Filter);
			AssertEquals(3, coll.Count);
			Assert(coll.Contains(declaration1));
			Assert(coll.Contains(declaration2));
			Assert(coll.Contains(declaration3));

			filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			coll.Load(FilterBO.Filter);
			AssertEquals(2, coll.Count);
			Assert(coll.Contains(declaration1));
			Assert(coll.Contains(declaration3));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Today.AddDays(-15);
			filter.Property2 = ZDateTime.Today.AddDays(2);
			coll.Load(FilterBO.Filter);
			AssertEquals(3, coll.Count);
			Assert(coll.Contains(declaration1));
			Assert(coll.Contains(declaration2));
			Assert(coll.Contains(declaration3));

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			coll.Load(FilterBO.Filter);
			AssertEquals(1, coll.Count);
			Assert(coll.Contains(declaration4));
		}

		public void TestSoldEnRouteQuery()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration1.US_SoldEnRouteIndicator = YesNoDefaultList.Codes.Yes;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration2.US_SoldEnRouteIndicator = YesNoDefaultList.Codes.Yes;

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration3.US_SoldEnRouteIndicator = YesNoDefaultList.Codes.No;

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration4.US_SoldEnRouteIndicator = YesNoDefaultList.Codes.No;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.SoldEnRoute];
			filter.IsActive = true;
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(FilterBO.Filter);
			AssertEquals("all declarations should be found", 4, collection.Count);

			filter.Property = YesNoDefaultList.Codes.Yes;
			collection.Load(FilterBO.Filter);
			AssertEquals("'yes' should found 2 declarations", 2, collection.Count);
			AssertEquals("declaration1 should be there", true, collection.Contains(declaration1));
			AssertEquals("declaration2 should be there", true, collection.Contains(declaration2));

			filter.Property = YesNoDefaultList.Codes.No;
			collection.Load(FilterBO.Filter);
			AssertEquals("'no' should found 1 declaration", 1, collection.Count);
			AssertEquals("declaration3 should be there", true, collection.Contains(declaration3));
		}

		public void TestDuplicateStatusFilter()
		{
			var filterBizO = new JobDeclarationFilterBusinessObject();
			var filters = new FilterBusinessObjectDefaults();
			filters.Add(new FilterBusinessObjectDefault("FTA Recon", "Property0", ZBool.True));
			var module = new JobDeclarationModule();
			using (var popup = new EmbeddedModulePopup(module))
			{
				var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
				var reconStrategy = new ReconBulkImportPopupOKButtonStrategy(popup, reconDec);
				module.OverrideModuleDecisionProvider(reconStrategy.ModuleDecisionProvider);
				filterBizO.ParentModule = module;
				filterBizO.SetExternalDefaults(filters);
				filterBizO.LoadLayout(null);

				var strips = filterBizO.FilterStrips;
				AssertEquals(3, strips.Count);

				var strip1 = strips[1];
				AssertEquals("ENS (Entry Summary) Status", strip1.FilterDescription);
				AssertEquals(ImportMessageStatusList.Codes.ClearEntrySummaryDelete, strip1.CurrentModuleFilter["Property"]);
				var textModule1 = strip1.CurrentModuleFilter as ModuleTextFilter;
				AssertNotNull(textModule1);
				AssertEquals(ModuleTextFilter.ComparisonConstants.NotEqual, textModule1.ComparisonOperator);

				var strip2 = strips[2];
				AssertEquals("ENS (Entry Summary) Status", strip2.FilterDescription);
				AssertEquals(ImportMessageStatusList.Codes.EntrySummaryCanceled, strip2.CurrentModuleFilter["Property"]);
				var textModule2 = strip2.CurrentModuleFilter as ModuleTextFilter;
				AssertNotNull(textModule2);
				AssertEquals(ModuleTextFilter.ComparisonConstants.NotEqual, textModule2.ComparisonOperator);

				var declaration = Factory.New<JobDeclaration>();
				var declarationStrategy = new JobDeclarationBulkImportPopupOKButtonStrategy(popup, declaration);
				module.OverrideModuleDecisionProvider(declarationStrategy.ModuleDecisionProvider);
				filterBizO.SetExternalDefaults(filters);
				filterBizO.LoadLayout(null);

				strips = filterBizO.FilterStrips;
				AssertEquals(3, strips.Count);

				strip1 = strips[1];
				AssertEquals("Entry Type", strip1.FilterDescription);
				AssertEquals(EntryTypeList.Codes.ConsumptionFreeDutiable, strip1.CurrentModuleFilter["Property"]);
				textModule1 = strip1.CurrentModuleFilter as ModuleTextFilter;
				AssertNotNull(textModule1);

				strip2 = strips[2];
				AssertEquals("Entry Type", strip2.FilterDescription);
				AssertEquals(EntryTypeList.Codes.InformalFreeDutiable, strip2.CurrentModuleFilter["Property"]);
				textModule2 = strip2.CurrentModuleFilter as ModuleTextFilter;
				AssertNotNull(textModule2);
			}
		}

		public void TestFTZStatusFilters()
		{
			var ftzDeclaration1 = Factory.New<JobDeclaration>();
			ftzDeclaration1.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			ftzDeclaration1.AdmissionStatus = FTZMessageStatusList.Codes.ClearFTZAdmissionAdd;
			ftzDeclaration1.FTZConcurrenceStatus = FTZMessageStatusList.Codes.ClearConcurrence;

			var ftzDeclaration2 = Factory.New<JobDeclaration>();
			ftzDeclaration2.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			ftzDeclaration2.FTZArrivalStatus = FTZMessageStatusList.Codes.ClearGoodsArrival;
			ftzDeclaration2.FTZPTTStatus = FTZMessageStatusList.Codes.ClearPermitToTransfer;

			var ftzDeclaration3 = Factory.New<JobDeclaration>();
			ftzDeclaration3.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			ftzDeclaration3.AdmissionStatus = FTZMessageStatusList.Codes.ClearFTZAdmissionAdd;
			ftzDeclaration3.FTZPTTStatus = FTZMessageStatusList.Codes.ErrorPermitToTransfer;
			ftzDeclaration3.FTZConcurrenceStatus = FTZMessageStatusList.Codes.ErrorConcurrence;
			ftzDeclaration3.FTZDeliveryOfGoodsStatus = FTZMessageStatusList.Codes.AwaitingDeliveryOfGoods;
			ftzDeclaration3.FTZArrivalStatus = FTZMessageStatusList.Codes.ErrorGoodsArrival;

			var ftzDeclaration4 = Factory.New<JobDeclaration>();
			ftzDeclaration4.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			ftzDeclaration4.AdmissionStatus = FTZMessageStatusList.Codes.ClearFTZAdmissionAdd;
			ftzDeclaration4.FTZPTTStatus = FTZMessageStatusList.Codes.ClearPermitToTransfer;
			ftzDeclaration4.FTZConcurrenceStatus = FTZMessageStatusList.Codes.ClearConcurrence;
			ftzDeclaration4.FTZDeliveryOfGoodsStatus = FTZMessageStatusList.Codes.ClearDeliveryOfGoods;
			ftzDeclaration4.FTZArrivalStatus = FTZMessageStatusList.Codes.AwaitingGoodsArrival;

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var statusFilter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.FTZAdmissionStatus];
			statusFilter.IsActive = true;
			statusFilter.Property = FTZMessageStatusList.Codes.ClearFTZAdmissionAdd;

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(bizObj.Filter);
			AssertEquals(3, coll.Count);
			AssertEquals(true, coll.Contains(ftzDeclaration1));
			AssertEquals(true, coll.Contains(ftzDeclaration3));
			AssertEquals(true, coll.Contains(ftzDeclaration4));

			var statusFilter2 = (ModuleTextFilter)bizObj[DeclarationFilterConstants.FTZConcurrenceStatus];
			statusFilter2.IsActive = true;
			statusFilter2.Property = FTZMessageStatusList.Codes.ClearConcurrence;

			coll.Load(bizObj.Filter);
			AssertEquals(2, coll.Count);
			AssertEquals(true, coll.Contains(ftzDeclaration1));
			AssertEquals(true, coll.Contains(ftzDeclaration4));

			statusFilter.IsActive = false;
			statusFilter2.IsActive = false;

			var statusFilter3 = (ModuleTextFilter)bizObj[DeclarationFilterConstants.FTZPTTStatus];
			statusFilter3.IsActive = true;
			statusFilter3.Property = FTZMessageStatusList.Codes.ClearPermitToTransfer;

			coll.Load(bizObj.Filter);
			AssertEquals(2, coll.Count);
			AssertEquals(true, coll.Contains(ftzDeclaration2));
			AssertEquals(true, coll.Contains(ftzDeclaration4));

			var statusFilter4 = (ModuleTextFilter)bizObj[DeclarationFilterConstants.FTZGoodsArrivalStatus];
			statusFilter4.IsActive = true;
			statusFilter4.Property = FTZMessageStatusList.Codes.AwaitingGoodsArrival;

			coll.Load(bizObj.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(true, coll.Contains(ftzDeclaration4));
		}

		public void TestWarehouseEntryNumberAndEntryFilter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;

			declaration.US_WHSEntryFilerCode = "ABC";
			declaration.US_WHSEntryNumber = "12345667";

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var entryFilter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.WarehouseEntryFiler];
			entryFilter.IsActive = true;
			entryFilter.Property = "AB";
			entryFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(bizObj.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(true, coll.Contains(declaration));

			entryFilter.Property = "AC";
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(bizObj.Filter);
			AssertEquals(0, coll.Count);

			bizObj = new JobDeclarationFilterBusinessObject();
			var entryNumberFilter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.WarehouseEntryNumber];
			entryNumberFilter.IsActive = true;
			entryNumberFilter.Property = "123456";
			entryNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;

			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(bizObj.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(true, coll.Contains(declaration));

			entryNumberFilter.Property = "123457";
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(bizObj.Filter);
			AssertEquals(0, coll.Count);
		}

		[ExpectNoExceptions]
		public void TestGetInvoiceExportDateQueryWithDateIsNull()
		{
			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleDateFilter)bizObj[DeclarationFilterConstants.InvoiceExportDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.BrettsBirthday;

			AssertEquals(false, filter.Query.LiteralTextADO.Contains("<= null"));
		}

		public void TestMultipleImporterEINFiltersInGroup()
		{
			var bizObj = new JobDeclarationFilterBusinessObject();
			var importerEINFilter1 = (ModuleTextFilter)bizObj[DeclarationFilterConstants.ImporterEIN];
			importerEINFilter1.IsActive = true;
			importerEINFilter1.Property = "12-3456";
			importerEINFilter1.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;
			var importerEINFilter2 = (ModuleTextFilter)bizObj.ModuleFilters.AddNewDuplicateFilter(importerEINFilter1);
			importerEINFilter2.IsActive = true;
			importerEINFilter2.Property = "77777";
			importerEINFilter2.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;
			var importerEINFilter3 = (ModuleTextFilter)bizObj.ModuleFilters.AddNewDuplicateFilter(importerEINFilter1);
			importerEINFilter3.IsActive = true;
			importerEINFilter3.Property = "66666";
			importerEINFilter3.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;

			var query = bizObj.ModuleFilters.GetFilterQuery(new[] { importerEINFilter1, importerEINFilter2, importerEINFilter3 });
			AssertEquals("Group the same filters",
				"JE_OH_Importer IN (SELECT OH_PK FROM dbo.OrgHeader WHERE OH_PK IN (SELECT OK_OH FROM dbo.OrgCusCode WHERE OK_CodeType = 'EIN' and OK_RN_NKCodeCountry = 'US' and (OK_CustomsRegNo <> '12-3456' and OK_CustomsRegNo <> '77777' and OK_CustomsRegNo <> '66666')))",
				query.LiteralTextADO);
		}

		public void TestImporterEINFilter()
		{
			var organization1 = Factory.New<OrgHeader>();
			organization1.OH_Code = "INCTEST1";
			organization1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-3456789XX");
			organization1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ABIRoutingCode, "34-56789012");

			var organization2 = Factory.New<OrgHeader>();
			organization2.OH_Code = "INCTEST2";
			organization2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "34-56789012XX");
			organization2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ABIRoutingCode, "12-3456789");

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_OH_Importer = organization1.PK;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_OH_Importer = organization2.PK;
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var importerEINFilter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.ImporterEIN];
			importerEINFilter.IsActive = true;
			importerEINFilter.Property = "12-3456";
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarations.Load(bizObj.Filter);
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));

			declaration1.JE_OH_Importer = Guid.Empty;
			declaration1.IOROrgPK = organization1.PK;
			declaration2.JE_OH_Importer = Guid.Empty;
			declaration2.IOROrgPK = organization2.PK;
			importerEINFilter.Property = ZString.Empty;
			Factory.Save();

			var importerOfRecordEIN = (ModuleTextFilter)bizObj[DeclarationFilterConstants.ImporterOfRecordEIN];
			importerOfRecordEIN.IsActive = true;
			importerOfRecordEIN.Property = "34-5678";
			declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarations.Load(bizObj.Filter);
			AssertEquals("declaration1 is not in Collection", false, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is in Collection", true, declarations.Contains(declaration2.PK));
		}

		public void TestPreparerDistrictPortFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_PreparerDistrictPort = "1234";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_PreparerDistrictPort = "2345";

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleNkFilter)bizObj[DeclarationFilterConstants.PreparerDistrictPort];
			AssertEquals(FilterCategories.Locations, filter.Category);
			filter.IsActive = true;
			filter.Property = "1234";
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarations.Load(bizObj.Filter);
			AssertEquals("declaration1 matches filter", true, declarations.Contains(declaration1));
			AssertEquals("declaration2 does not match filter", false, declarations.Contains(declaration2));

			filter.Property = "2345";
			declarations.Load(bizObj.Filter);
			AssertEquals("declaration1 does not match filter", false, declarations.Contains(declaration1));
			AssertEquals("declaration2 matches filter", true, declarations.Contains(declaration2));
		}

		public void TestSPIInvoiceLineFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice1 = declaration1.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.US_SPI = "AA";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice2 = declaration2.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.US_SPI = "BB";
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.SPIInvLine];
			filter.IsActive = true;
			filter.Property = "AA";
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarations.Load(bizObj.Filter);
			AssertEquals("declaration1 matches filter", true, declarations.Contains(declaration1));
			AssertEquals("declaration2 does not match filter", false, declarations.Contains(declaration2));

			filter.Property = "BB";
			declarations.Load(bizObj.Filter);
			AssertEquals("declaration1 does not match filter", false, declarations.Contains(declaration1));
			AssertEquals("declaration2 matches filter", true, declarations.Contains(declaration2));
		}

		public void TestPGAReplaceUpdateNeededFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.US_PGAReplaceUpdateNeeded = YesNoDefaultList.Codes.Yes;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.US_PGAReplaceUpdateNeeded = ZString.Empty;
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleFlagsFilter)bizObj[DeclarationFilterConstants.PGAReplaceUpdateNeeded];
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);
			filter.IsActive = true;
			filter.Property0 = true;
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarations.Load(bizObj.Filter);
			AssertEquals("declaration1 matches filter", true, declarations.Contains(declaration1));
			AssertEquals("declaration2 does not match filter", false, declarations.Contains(declaration2));
		}

		public void TestPGAExpeditedReleaseFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.US_PGAExpeditedRelease = true;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.US_PGAExpeditedRelease = false;
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleFlagsFilter)bizObj[DeclarationFilterConstants.PGAExpeditedRelease];
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);
			filter.IsActive = true;
			filter.Property0 = true;

			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarations.Load(bizObj.Filter);
			AssertEquals("declaration1 matches filter", true, declarations.Contains(declaration1));
			AssertEquals("declaration2 does not match filter", false, declarations.Contains(declaration2));
		}

		public void TestCaroRelResponseWithCMT()
		{
			//action taken
			var declaration1 = GetSimpleMergedDeclaration();
			var declaration1_message1 = Factory.New<MQEDIMessage>();
			declaration1_message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			declaration1_message1.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus;
			declaration1_message1.EM_ApplicationReference = ReferenceIdentifierQualifierCodeList.Codes.CMT;
			declaration1_message1.EM_LinkedObject = declaration1.ActiveEntryHeaders.EntrySummaryEntry;
			declaration1_message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			declaration1_message1.SetToComplete();

			var entry = declaration1.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			entry.EntryNumber = "00941598";
			var cusEntry1 = declaration1.ActiveEntryHeaders.SimplifiedEntry;
			cusEntry1.Messages.Add(declaration1_message1);

			//action required and not taken
			var declaration2 = GetSimpleMergedDeclaration();
			var declaration2_message1 = Factory.New<MQEDIMessage>();
			declaration2_message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			declaration2_message1.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus;
			declaration2_message1.EM_ApplicationReference = ReferenceIdentifierQualifierCodeList.Codes.CMT;
			declaration2_message1.EM_LinkedObject = declaration2.ActiveEntryHeaders.EntrySummaryEntry;
			declaration2_message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			declaration2_message1.EM_MessageText =
			"B003002906SO                                                                    " +
			"SO103002906  00941598 0113-611944100KKLUYM MATURITY         41E  040217         " +
			"SO20CMTTRANSFER FOR EXAM TO CES MERCER  PLEASE UPLOAD ENT                       " +
			"SO20CMTRY DOCS TO DIS                                                           " +
			"SO20CR B00101688                                                                " +
			"SO40RKKLUNB3706038                                         00001960     00001960" +
			"SO50040317105895BILL ARRIVED                                                    " +
			"SO60040317105822RELEASE DATE UPDATE                     04031701                " +
			"SO60040317105898RELEASED                                04031701                " +
			"SO60040317105801ONE USG                                                         " +
			"Y  3002906SO00000";

			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			entry2.EntryNumber = "00941598";

			var cusEntry2 = declaration2.ActiveEntryHeaders.SimplifiedEntry;
			cusEntry2.Messages.Add(declaration2_message1);

			declaration1.ReCalculateCRLAction();
			declaration2.ReCalculateCRLAction();

			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.CargoReleaseComments];
			filter.IsActive = true;
			filter.Property = DeclarationFilterConstants.Incomplete;

			Assert("Has ATH log", !declaration1.MatchesFilter(bizObj.Filter));
			Assert("Has not ATH log", declaration2.MatchesFilter(bizObj.Filter));

			filter.Property = DeclarationFilterConstants.ALL;
			Assert(declaration1.MatchesFilter(bizObj.Filter));
			Assert(declaration2.MatchesFilter(bizObj.Filter));
		}

		public void TestPGACorrectionStatusFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.US_PGACorrectionStatus = PGACorrectionStatusList.Codes.AwaitingPGADataCorrection;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.US_PGACorrectionStatus = PGACorrectionStatusList.Codes.ClearPGADataCorrection;
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.PGACorrectionStatus];
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);
			filter.IsActive = true;
			filter.Property = PGACorrectionStatusList.Codes.AwaitingPGADataCorrection;
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarations.Load(bizObj.Filter);
			AssertEquals("declaration1 matches filter", true, declarations.Contains(declaration1));
			AssertEquals("declaration2 does not match filter", false, declarations.Contains(declaration2));

			filter.Property = PGACorrectionStatusList.Codes.ClearPGADataCorrection;
			declarations.Load(bizObj.Filter);
			AssertEquals("declaration1 does not match filter", false, declarations.Contains(declaration1));
			AssertEquals("declaration2 matches filter", true, declarations.Contains(declaration2));
		}

		public void TestQuotaStatusFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.US_QuotaStatus = CargoReleaseProcessingResultList.Codes.QuotaPending;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.US_QuotaStatus = CargoReleaseProcessingResultList.Codes.QuotaAccepted;
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.QuotaStatus];
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);
			filter.IsActive = true;
			filter.Property = CargoReleaseProcessingResultList.Codes.QuotaPending;
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarations.Load(bizObj.Filter);
			AssertEquals("declaration1 matches filter", true, declarations.Contains(declaration1));
			AssertEquals("declaration2 does not match filter", false, declarations.Contains(declaration2));

			filter.Property = CargoReleaseProcessingResultList.Codes.QuotaAccepted;
			declarations.Load(bizObj.Filter);
			AssertEquals("declaration1 does not match filter", false, declarations.Contains(declaration1));
			AssertEquals("declaration2 matches filter", true, declarations.Contains(declaration2));
		}

		public void TestFieldLengthOfFlightVoyageVesselFilterIsLimited()
		{
			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextAndNkFilter)bizObj[DeclarationFilterConstants.FlightVoyageVessel];
			AssertEquals("Max length should equal with the value in JobDeclarationSchema", filter.MaxLength, JobDeclarationSchema.JE_VoyageFlightNo.MaxLength);
			AssertEquals(JobDeclarationSchema.JE_VoyageFlightNo.MaxLength, filter.MaxLength);
			AssertEquals(JobDeclarationSchema.JE_VesselName.MaxLength, filter.NkMaxLength);
		}

		public void TestAESSeverityFilter()
		{
			var declarationA = GetExportDeclaration();
			AddAESDispositionsToNewEntry(declarationA, ("11A", "W"), ("12A", "X"));
			AddAESDispositionsToNewEntry(declarationA, ("21A", "Y"), ("22A", "Z"));
			var declarationB = GetExportDeclaration();
			AddAESDispositionsToNewEntry(declarationB, ("11B", "A"), ("12B", "B"));
			AddAESDispositionsToNewEntry(declarationB, ("21B", "C"), ("22B", "D"));
			var declarationC = GetExportDeclaration();
			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.AESSeverity];

			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);
			var expectedOperatorCodes = new[]
			{
				ModuleTextFilter.ComparisonConstants.Exact, ModuleTextFilter.ComparisonConstants.NotEqual,
				ModuleTextFilter.ComparisonConstants.IsBlank, ModuleTextFilter.ComparisonConstants.IsNotBlank
			};
			AssertContainsExactElementsInExactOrder(expectedOperatorCodes, filter.ComparisonOperator_List.GetAllCodes());

			filter.IsActive = true;
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "W";
			declarations.Load(FilterBO.Filter);
			AssertCollectionNotContains(declarationA, declarations);
			AssertCollectionNotContains(declarationB, declarations);
			AssertCollectionNotContains(declarationC, declarations);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "X";
			declarations.Load(FilterBO.Filter);
			AssertCollectionContains(declarationA, declarations);
			AssertCollectionNotContains(declarationB, declarations);
			AssertCollectionNotContains(declarationC, declarations);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "Z";
			declarations.Load(FilterBO.Filter);
			AssertCollectionContains(declarationA, declarations);
			AssertCollectionNotContains(declarationB, declarations);
			AssertCollectionNotContains(declarationC, declarations);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter.Property = "X";
			declarations.Load(FilterBO.Filter);
			AssertCollectionNotContains(declarationA, declarations);
			AssertCollectionContains(declarationB, declarations);
			AssertCollectionContains(declarationC, declarations);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			filter.Property = "";
			declarations.Load(FilterBO.Filter);
			AssertCollectionNotContains(declarationA, declarations);
			AssertCollectionNotContains(declarationB, declarations);
			AssertCollectionContains(declarationC, declarations);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			filter.Property = "";
			declarations.Load(FilterBO.Filter);
			AssertCollectionContains(declarationA, declarations);
			AssertCollectionContains(declarationB, declarations);
			AssertCollectionNotContains(declarationC, declarations);
		}

		public void TestAESSeverityFilter_SpaceCode()
		{
			var declarationA = GetExportDeclaration();
			AddAESDispositionsToNewEntry(declarationA, ("11A", DeclarationFilterConstants.FilterCodeAndDesc.Space));
			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO[DeclarationFilterConstants.AESSeverity];
			filter.IsActive = true;
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = DeclarationFilterConstants.FilterCodeAndDesc.Space;
			declarations.Load(FilterBO.Filter);
			AssertCollectionContains(declarationA, declarations);
		}

		public void TestAESResponseCodeFilter()
		{
			var declarationA = GetExportDeclaration();
			AddAESDispositionsToNewEntry(declarationA, ("11A", "W"), ("12A", "X"));
			AddAESDispositionsToNewEntry(declarationA, ("21A", "Y"), ("22A", "Z"));
			var declarationB = GetExportDeclaration();
			AddAESDispositionsToNewEntry(declarationB, ("11B", "A"), ("12B", "B"));
			AddAESDispositionsToNewEntry(declarationB, ("21B", "C"), ("22B", "D"));
			var declarationC = GetExportDeclaration();
			Factory.Save();

			var filter = (ModuleNkFilter)FilterBO[DeclarationFilterConstants.AESResponseCode];

			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);
			var expectedOperatorCodes = new[]
			{
				ModuleTextFilter.ComparisonConstants.Exact, ModuleTextFilter.ComparisonConstants.NotEqual,
				ModuleTextFilter.ComparisonConstants.IsBlank, ModuleTextFilter.ComparisonConstants.IsNotBlank
			};
			AssertContainsExactElementsInExactOrder(expectedOperatorCodes, filter.ComparisonOperator_List.GetAllCodes());

			filter.IsActive = true;
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "11A";
			declarations.Load(FilterBO.Filter);
			AssertCollectionNotContains(declarationA, declarations);
			AssertCollectionNotContains(declarationB, declarations);
			AssertCollectionNotContains(declarationC, declarations);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "12A";
			declarations.Load(FilterBO.Filter);
			AssertCollectionContains(declarationA, declarations);
			AssertCollectionNotContains(declarationB, declarations);
			AssertCollectionNotContains(declarationC, declarations);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "22A";
			declarations.Load(FilterBO.Filter);
			AssertCollectionContains(declarationA, declarations);
			AssertCollectionNotContains(declarationB, declarations);
			AssertCollectionNotContains(declarationC, declarations);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter.Property = "12A";
			declarations.Load(FilterBO.Filter);
			AssertCollectionNotContains(declarationA, declarations);
			AssertCollectionContains(declarationB, declarations);
			AssertCollectionContains(declarationC, declarations);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			filter.Property = "";
			declarations.Load(FilterBO.Filter);
			AssertCollectionNotContains(declarationA, declarations);
			AssertCollectionNotContains(declarationB, declarations);
			AssertCollectionContains(declarationC, declarations);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			filter.Property = "";
			declarations.Load(FilterBO.Filter);
			AssertCollectionContains(declarationA, declarations);
			AssertCollectionContains(declarationB, declarations);
			AssertCollectionNotContains(declarationC, declarations);
		}

		JobDeclaration GetExportDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			return declaration;
		}

		void AddAESDispositionsToNewEntry(JobDeclaration jobDeclaration, params (ZString status, ZString statusKey)[] pairs)
		{
			var entry = jobDeclaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = jobDeclaration.JE_MessageType;
			entry.EntryNumber = "S40002510" + jobDeclaration.ActiveEntryHeaders.Count;
			var cusDispositions = entry.AESCusDispositions;
			foreach ((ZString status, ZString statusKey) pair in pairs)
			{
				var disposition = cusDispositions.AddNew();
				disposition.CDI_Status = pair.status;
				disposition.CDI_StatusDate = ZDateTime.Today;
				disposition.CDI_StatusKey = pair.statusKey;
				disposition.CDI_Sequence = (short)cusDispositions.Count;
				disposition.CDI_SystemCreateTimeUtc = new ZDateTime(2000 + cusDispositions.Count, 1, 1);
			}
		}

		JobDeclarationFilterBusinessObject FilterBO => (JobDeclarationFilterBusinessObject)filterBO;

		Integration.Customs.US.ISF.ICusISFHeader CreateISFJob(ZDateTime createTime, ZString billNumber, ZString billType, ZString customsStatus)
		{
			var header = Factory.New<Integration.Customs.US.ISF.ICusISFHeader>();
			header.BF_SystemCreateTimeUtc = createTime;
			var bill = CreateISFBill(header.PK, billNumber, billType, customsStatus);
			return header;
		}

		Integration.Customs.US.ISF.ICusISFBill CreateISFBill(ZGuid headerPK, ZString billNumber, ZString billType, ZString customsStatus)
		{
			var bill = Factory.New<Integration.Customs.US.ISF.ICusISFBill>();
			bill.BB_BF = headerPK;
			bill.BB_BillType = billType;
			bill.BB_BillNum = billNumber;
			bill.BB_CustomsStatus = customsStatus;
			return bill;
		}

		void CreateTestDecsWithStatus()
		{
			eXPDeclaration1 = Factory.New<JobDeclaration>();
			eXPDeclaration1.JE_MessageType = JobMessageTypeList.Codes.Export;
			var eXPEntry1 = eXPDeclaration1.CustomsEntryHeaders.AddNew();
			eXPEntry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			eXPEntry1.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;

			cRLDeclaration1 = Factory.New<JobDeclaration>();
			cRLDeclaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			cRLDeclaration1.BLUStatus = ImportMessageStatusList.Codes.ClearBillOfLadingUpdate;
			var cRLentry1 = cRLDeclaration1.CustomsEntryHeaders.AddNew();
			cRLentry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease;
			cRLentry1.CH_Status = ImportMessageStatusList.Codes.ClearBorderCargoReleaseOriginal;

			cRLDeclaration2 = Factory.New<JobDeclaration>();
			cRLDeclaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			var cRLentry2 = cRLDeclaration2.CustomsEntryHeaders.AddNew();
			cRLentry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			cRLentry2.CH_Status = ImportMessageStatusList.Codes.ClearArrival;

			simplifiedEntryDeclaration1 = Factory.New<JobDeclaration>();
			simplifiedEntryDeclaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			simplifiedEntryDeclaration1.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			simplifiedEntryDeclaration1.US_EnableCRL = true;
			var seEntry1 = simplifiedEntryDeclaration1.CustomsEntryHeaders.AddNew();
			seEntry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			seEntry1.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			var bill = simplifiedEntryDeclaration1.Bills.AddNew();
			bill.CU_BillNum = "Test0011";
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.DispositionCodes.AddNewIfNotExist("93", ZDateTime.Today, BillDispositionSourceList.Codes.SO);

			simplifiedEntryDeclaration2 = Factory.New<JobDeclaration>();
			simplifiedEntryDeclaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			simplifiedEntryDeclaration2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var seEntry2 = simplifiedEntryDeclaration2.CustomsEntryHeaders.AddNew();
			seEntry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			seEntry2.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseDelete;

			simplifiedEntryDeclaration3 = Factory.New<JobDeclaration>();
			simplifiedEntryDeclaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			simplifiedEntryDeclaration3.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var seEntry3 = simplifiedEntryDeclaration3.CustomsEntryHeaders.AddNew();
			seEntry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			seEntry3.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			var bill2 = simplifiedEntryDeclaration3.Bills.AddNew();
			bill2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill2.CU_BillNum = "Test1";
			bill2.DispositionCodes.AddNewIfNotExist("95", ZDateTime.Today, BillDispositionSourceList.Codes.SO);
			var bill3 = simplifiedEntryDeclaration3.Bills.AddNew();
			bill3.CU_BillNum = "Test2";
			bill3.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;

			eNSDeclaration1 = Factory.New<JobDeclaration>();
			eNSDeclaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			eNSDeclaration1.BLUStatus = ImportMessageStatusList.Codes.AwaitingBillOfLadingUpdate;
			var eNSentry = eNSDeclaration1.CustomsEntryHeaders.AddNew();
			eNSentry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			eNSentry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;

			iNBDeclaration1 = Factory.New<JobDeclaration>();
			iNBDeclaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			var iNBentry1 = iNBDeclaration1.CustomsEntryHeaders.AddNew();
			iNBentry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			iNBentry1.CH_Status = ImportMessageStatusList.Codes.ErrorFDACorrection;

			iNBDeclaration2 = Factory.New<JobDeclaration>();
			iNBDeclaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			var iNBentry2 = iNBDeclaration2.CustomsEntryHeaders.AddNew();
			iNBentry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			iNBentry2.CH_Status = ImportMessageStatusList.Codes.ClearFDACorrection;

			iNBDeclaration3 = Factory.New<JobDeclaration>();
			iNBDeclaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			var iNBentry3 = iNBDeclaration3.CustomsEntryHeaders.AddNew();
			iNBentry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			iNBentry3.CH_Status = ImportMessageStatusList.Codes.ErrorFDACorrection;

			eXPDeclaration2 = Factory.New<JobDeclaration>();
			eXPDeclaration2.JE_MessageType = JobMessageTypeList.Codes.Export;
			var eXPEntry2 = eXPDeclaration2.CustomsEntryHeaders.AddNew();
			eXPEntry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			eXPEntry2.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;

			eXPDeclaration3 = Factory.New<JobDeclaration>();
			eXPDeclaration3.JE_MessageType = JobMessageTypeList.Codes.Export;
			var eXPEntry3 = eXPDeclaration3.CustomsEntryHeaders.AddNew();
			eXPEntry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			eXPEntry3.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingReplacementResponse;

			cRLDeclaration3 = Factory.New<JobDeclaration>();
			cRLDeclaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			cRLDeclaration3.BLUStatus = ImportMessageStatusList.Codes.ClearBillOfLadingUpdate;
			var cRLentry3 = cRLDeclaration3.CustomsEntryHeaders.AddNew();
			cRLentry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			cRLentry3.CH_Status = ImportMessageStatusList.Codes.ClearBorderCargoReleaseOriginal;

			eNSDeclaration2 = Factory.New<JobDeclaration>();
			eNSDeclaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			eNSDeclaration2.BLUStatus = ImportMessageStatusList.Codes.ErrorBillOfLadingUpdate;
			var eNSentry2A = eNSDeclaration2.CustomsEntryHeaders.AddNew();
			eNSentry2A.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			eNSentry2A.CH_Status = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			var eNSentry2B = eNSDeclaration2.CustomsEntryHeaders.AddNew();
			eNSentry2B.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			eNSentry2B.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			Factory.Save();
		}

		void CreateTestDecsWithAndWithoutStatuses()
		{
			eXPDeclaration1 = Factory.New<JobDeclaration>();
			eXPDeclaration1.JE_MessageType = JobMessageTypeList.Codes.Export;
			var eXPEntry1 = eXPDeclaration1.CustomsEntryHeaders.AddNew();
			eXPEntry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			eXPEntry1.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;

			cRLDeclaration1 = Factory.New<JobDeclaration>();
			cRLDeclaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			cRLDeclaration1.BLUStatus = ImportMessageStatusList.Codes.ClearBillOfLadingUpdate;
			var cRLentry1 = cRLDeclaration1.CustomsEntryHeaders.AddNew();
			cRLentry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease;
			cRLentry1.CH_Status = ImportMessageStatusList.Codes.ClearBorderCargoReleaseOriginal;

			cRLDeclaration2 = Factory.New<JobDeclaration>();
			cRLDeclaration2.JE_MessageType = JobMessageTypeList.Codes.Import;

			simplifiedEntryDeclaration1 = Factory.New<JobDeclaration>();
			simplifiedEntryDeclaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			simplifiedEntryDeclaration1.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			simplifiedEntryDeclaration1.US_EnableCRL = true;
			var seEntry1 = simplifiedEntryDeclaration1.CustomsEntryHeaders.AddNew();
			seEntry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			seEntry1.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;

			simplifiedEntryDeclaration2 = Factory.New<JobDeclaration>();
			simplifiedEntryDeclaration2.JE_MessageType = JobMessageTypeList.Codes.Import;

			simplifiedEntryDeclaration3 = Factory.New<JobDeclaration>();
			simplifiedEntryDeclaration3.JE_MessageType = JobMessageTypeList.Codes.Import;

			eNSDeclaration1 = Factory.New<JobDeclaration>();
			eNSDeclaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			eNSDeclaration1.BLUStatus = ImportMessageStatusList.Codes.AwaitingBillOfLadingUpdate;
			var eNSentry = eNSDeclaration1.CustomsEntryHeaders.AddNew();
			eNSentry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			eNSentry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;

			iNBDeclaration1 = Factory.New<JobDeclaration>();
			iNBDeclaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			var iNBentry1 = iNBDeclaration1.CustomsEntryHeaders.AddNew();
			iNBentry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			iNBentry1.CH_Status = ImportMessageStatusList.Codes.ErrorFDACorrection;

			iNBDeclaration2 = Factory.New<JobDeclaration>();
			iNBDeclaration2.JE_MessageType = JobMessageTypeList.Codes.Import;

			iNBDeclaration3 = Factory.New<JobDeclaration>();
			iNBDeclaration3.JE_MessageType = JobMessageTypeList.Codes.Import;

			eXPDeclaration2 = Factory.New<JobDeclaration>();
			eXPDeclaration2.JE_MessageType = JobMessageTypeList.Codes.Export;

			eXPDeclaration3 = Factory.New<JobDeclaration>();
			eXPDeclaration3.JE_MessageType = JobMessageTypeList.Codes.Export;

			cRLDeclaration3 = Factory.New<JobDeclaration>();
			cRLDeclaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			cRLDeclaration3.BLUStatus = ImportMessageStatusList.Codes.ClearBillOfLadingUpdate;

			eNSDeclaration2 = Factory.New<JobDeclaration>();
			eNSDeclaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			eNSDeclaration2.BLUStatus = ImportMessageStatusList.Codes.ErrorBillOfLadingUpdate;
			Factory.Save();
		}

		void CreateAdditionalExportTestDecs()
		{
			eXPDeclaration4 = Factory.New<JobDeclaration>();
			eXPDeclaration4.JE_MessageType = JobMessageTypeList.Codes.Export;
			var eXPDec4Entry1 = eXPDeclaration4.CustomsEntryHeaders.AddNew();
			eXPDec4Entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			eXPDec4Entry1.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;

			var eXPDec4Entry2 = eXPDeclaration4.CustomsEntryHeaders.AddNew();
			eXPDec4Entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			eXPDec4Entry2.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingReplacementResponse;

			eXPDeclaration5 = Factory.New<JobDeclaration>();
			eXPDeclaration5.JE_MessageType = JobMessageTypeList.Codes.Export;
			var eXPDec5Entry1 = eXPDeclaration5.CustomsEntryHeaders.AddNew();
			eXPDec5Entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			eXPDec5Entry1.CH_Status = AESDirectCustomsEntryStatus.Codes.Compliance;

			eXPDeclaration6 = Factory.New<JobDeclaration>();
			eXPDeclaration6.JE_MessageType = JobMessageTypeList.Codes.Export;
			var eXPDec6Entry1 = eXPDeclaration6.CustomsEntryHeaders.AddNew();
			eXPDec6Entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			eXPDec6Entry1.CH_Status = AESDirectCustomsEntryStatus.Codes.DeleteSEDClear;

			var eXPDec6Entry2 = eXPDeclaration6.CustomsEntryHeaders.AddNew();
			eXPDec6Entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			eXPDec6Entry2.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse;

			eXPDeclaration7 = Factory.New<JobDeclaration>();
			eXPDeclaration7.JE_MessageType = JobMessageTypeList.Codes.Export;
			var eXPDec7Entry1 = eXPDeclaration7.CustomsEntryHeaders.AddNew();
			eXPDec7Entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			eXPDec7Entry1.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;

			var eXPDec7Entry2 = eXPDeclaration7.CustomsEntryHeaders.AddNew();
			eXPDec7Entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			eXPDec7Entry2.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;

			eXPDeclaration8 = Factory.New<JobDeclaration>();
			eXPDeclaration8.JE_MessageType = JobMessageTypeList.Codes.Export;
			var eXPDec8Entry1 = eXPDeclaration8.CustomsEntryHeaders.AddNew();
			eXPDec8Entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			eXPDec8Entry1.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;

			var eXPDec8Entry2 = eXPDeclaration8.CustomsEntryHeaders.AddNew();
			eXPDec8Entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			eXPDec8Entry2.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;

			var eXPDec8Entry3 = eXPDeclaration8.CustomsEntryHeaders.AddNew();
			eXPDec8Entry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			eXPDec8Entry3.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse;

			Factory.Save();
		}

		void GetDeclarationsForAuditFiltersTesting()
		{
			declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.Invoices.AddNew();
			declaration1.InvoiceLines.AddNew().JI_AddInfo = "UC_NKCountryOfOrigin=KR*SPI=N/A*SecondarySPI=Z";
			declaration1.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;

			declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration2.Invoices.AddNew();
			declaration2.InvoiceLines.AddNew().JI_AddInfo = "UC_NKCountryOfOrigin=KR*SPI=N/A*SecondarySPI=Z";

			declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.Invoices.AddNew();
			declaration3.InvoiceLines.AddNew().JI_AddInfo = "UC_NKCountryOfOrigin=KR*SPI=N/A*SecondarySPI=Z*FDAIndicator=D";
			declaration3.Logs.AddNew(Events.RecordAudited, Enterprise.ZArchitecture.Business.Internal.BusinessObjectLogger.PrefixIndicator + Customs.US.Business.AuditFieldsList.Codes.SPI, ZDateTime.BrettsBirthday.ToOffset());

			declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration4.Invoices.AddNew();
			declaration4.InvoiceLines.AddNew().JI_AddInfo = "UC_NKCountryOfOrigin=KR*SPI=AU*SecondarySPI=Z*FDAIndicator=C";
			declaration4.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration4.Logs.AddNew(Events.RecordAudited, Enterprise.ZArchitecture.Business.Internal.BusinessObjectLogger.PrefixIndicator + Customs.US.Business.AuditFieldsList.Codes.FDA, ZDateTime.BrettsBirthday.AddYears(1).ToOffset());
			declaration4.Logs.AddNew(Events.StatusUpdated, Enterprise.ZArchitecture.Business.Internal.BusinessObjectLogger.PrefixIndicator + Customs.US.Business.AuditFieldsList.Codes.TIB, ZDateTime.BrettsBirthday.AddDays(1).ToOffset());

			declaration5 = Factory.New<JobDeclaration>();
			declaration5.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration5.Invoices.AddNew();
			declaration5.InvoiceLines.AddNew().JI_AddInfo = "UC_NKCountryOfOrigin=KR*SPI=N/A*SecondarySPI=Z*FDAIndicator=C";
			declaration5.Logs.AddNew(Events.RecordAudited, Enterprise.ZArchitecture.Business.Internal.BusinessObjectLogger.PrefixIndicator + Customs.US.Business.AuditFieldsList.Codes.FDA);

			var entry = declaration5.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryDelete;

			declaration6 = Factory.New<JobDeclaration>();
			declaration6.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration6.Invoices.AddNew();
			declaration6.InvoiceLines.AddNew().JI_AddInfo = "UC_NKCountryOfOrigin=KR*SPI=N/A*SecondarySPI=Z*FDAIndicator=C";

			Factory.Save();
		}

		JobDeclaration GetSimpleMergedDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			return declaration;
		}

		AccTransactionHeader CreateTransactionData(JobDeclaration declaration, ZDecimal invoiceAmount)
		{
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = declaration.PK;
			jobHeader.JH_ParentTableCode = "JE";
			jobHeader.JH_JobNum = declaration.JE_DeclarationReference;

			var accTransHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			accTransHeader.AH_JH = jobHeader.PK;
			accTransHeader.AH_Ledger = "AR";
			accTransHeader.AH_TransactionCategory = "DBT";
			accTransHeader.AH_InvoiceAmount = invoiceAmount;
			accTransHeader.AH_OutstandingAmount = invoiceAmount;

			return accTransHeader;
		}

		void CreateTransactionLineData(JobDeclaration declaration, AccTransactionHeader accTransHeader, ZDecimal amount)
		{
			var accTransLine1 = Factory.NewWithValidTestData<AccTransactionLines>();
			accTransLine1.AL_AH = accTransHeader.PK;
			accTransLine1.AL_LineAmount = amount * 0.25m;
			accTransLine1.AL_AC = testChgCode.PK;
			var accTransLine2 = Factory.NewWithValidTestData<AccTransactionLines>();
			accTransLine2.AL_AH = accTransHeader.PK;
			accTransLine2.AL_LineAmount = amount * 0.25m;
			accTransLine2.AL_AC = testChgCode2.PK;
			var accTransLine3 = Factory.NewWithValidTestData<AccTransactionLines>();
			accTransLine3.AL_AH = accTransHeader.PK;
			accTransLine3.AL_LineAmount = amount * 0.5m;
			accTransLine3.AL_AC = testChgCode3.PK;
		}

		void CreateChgCodes()
		{
			testChgCode = Factory.NewWithValidTestData<AccChargeCode>();
			testChgCode.AC_ChargeType = "DSB";
			testChgCode.AC_Code = "TST";
			testChgCode.AC_GC = GlbCompany.CurrentCompany.PK;

			testChgCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			testChgCode2.AC_ChargeType = "DSB";
			testChgCode2.AC_Code = "TST2";
			testChgCode2.AC_GC = GlbCompany.CurrentCompany.PK;

			testChgCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			testChgCode3.AC_ChargeType = "TST";
			testChgCode3.AC_Code = "TST3";
			testChgCode3.AC_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();
		}
		AccChargeCode testChgCode;
		AccChargeCode testChgCode2;
		AccChargeCode testChgCode3;
		JobDeclaration eXPDeclaration1;
		JobDeclaration eXPDeclaration2;
		JobDeclaration eXPDeclaration3;
		JobDeclaration eXPDeclaration4;
		JobDeclaration eXPDeclaration5;
		JobDeclaration eXPDeclaration6;
		JobDeclaration eXPDeclaration7;
		JobDeclaration eXPDeclaration8;
		JobDeclaration cRLDeclaration1;
		JobDeclaration cRLDeclaration2;
		JobDeclaration cRLDeclaration3;
		JobDeclaration eNSDeclaration1;
		JobDeclaration eNSDeclaration2;
		JobDeclaration iNBDeclaration1;
		JobDeclaration iNBDeclaration2;
		JobDeclaration iNBDeclaration3;
		JobDeclaration simplifiedEntryDeclaration1;
		JobDeclaration simplifiedEntryDeclaration2;
		JobDeclaration simplifiedEntryDeclaration3;
		new JobDeclaration declaration1;
		new JobDeclaration declaration2;
		new JobDeclaration declaration3;
		new JobDeclaration declaration4;
		new JobDeclaration declaration5;
		new JobDeclaration declaration6;

		const string EIStatusForTest1 = MessageStatusListEI.Codes.ClearElectronicInvoiceOriginal;
		const string EIStatusForTest2 = MessageStatusListEI.Codes.ErrorElectronicInvoiceOriginal;
		const string UnusedEIStatusForTest = MessageStatusListEI.Codes.AwaitingElectronicInvoiceReplace;
	}
}
