using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using BillTypeList = Enterprise.Customs.Business.BillTypeList;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(EntryHeaderFilterBusinessObject))]
	sealed class EntryHeaderFilterBusinessObjectTest : Customs.Module.Testing.EntryHeaderFilterBusinessObjectAbstractTest
	{
		public void TestFTAReconIssueQuery()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_NAFTAReconIndicator = true;
			var entry = declaration1.CustomsEntryHeaders.AddNew();
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_NAFTAReconIndicator = false;
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			DeleteAllGenAddOnColumn(declaration1);
			DeleteAllGenAddOnColumn(declaration2);
			Factory.Save();
			var filterBizObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizObj[DeclarationFilterConstants.FTAReconIndicator];
			filter.IsActive = true;
			filter.Property0 = true;
			Assert("entry1 meets the filter", entry.MatchesFilter(filterBizObj.Filter));
			Assert("entry2 does not meet the filter", !entry2.MatchesFilter(filterBizObj.Filter));
			filter.Property0 = false;
			Assert("entry1 does not meet the filter", !entry.MatchesFilter(filterBizObj.Filter));
			Assert("entry2 meets the filter", entry2.MatchesFilter(filterBizObj.Filter));
		}

		public void TestEntryNumberFilterDescription()
		{
			var filterObj = GetNewFilterStripBusinessObject().ModuleFilters;
			AssertNull(filterObj[EntryHeaderFilterBusinessObject.Constants.EntryNumber]);
			var entryNumberFilter = (ModuleNumberFilter)filterObj[EntryHeaderFilterBusinessObject.Schema.EntryNumber];
			AssertEquals("Entry Number (ENS)", entryNumberFilter.LocalizedDescription);
		}

		public void TestExclusiveFilterByJobNumber()
		{
			var testFilterBizO = new EntryHeaderFilterBusinessObject();
			var jobNumberFilter = (testFilterBizO[EntryHeaderFilterBusinessObject.Constants.JobNumber]);
			AssertNotNull("Job Number filter should exist", jobNumberFilter);
			var filters = testFilterBizO.ModuleFilters.ToSortedArrayWithIsExclusiveLast();
			jobNumberFilter = filters[filters.Length - 1];
			AssertEquals("Job Number filter is the exclusive filter", EntryHeaderFilterBusinessObject.Constants.JobNumber, jobNumberFilter.Description);
		}

		public void TestGetEntryNumberQuery()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			declaration1.AllocateEntryNumber("QW123456");
			var declaration2 = Factory.New<JobDeclaration>();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			declaration2.AllocateEntryNumber("AS123456");
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleNumberFilter)filterObj[EntryHeaderFilterBusinessObject.Schema.EntryNumber];
			filter.Property = "QW123456";
			filter.IsActive = true;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			filter.Property = "AS123456";
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
		}

		public void TestNotReconciledQuery()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_NAFTAReconIndicator = true;
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_OtherReconIndicator = ReconIssueCodeList.Codes._9802Recon;
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.US_OtherReconIndicator = ReconIssueCodeList.Codes.ClassRecon;
			var entry3 = declaration3.CustomsEntryHeaders.AddNew();
			entry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var declaration4 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry4 = declaration4.CustomsEntryHeaders.AddNew();
			entry4.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var reconEntry1 = reconDec.OriginalEntries.AddNew();
			reconEntry1.CH_CH_OriginalEntry = entry1.PK; //reconciled
			var reconEntry2 = reconDec.OriginalEntries.AddNew();
			reconEntry2.CH_CH_OriginalEntry = entry2.PK; //reconciled
			var reconEntry3 = reconDec.OriginalEntries.AddNew(); //recon for an entry by a third party
			DeleteAllGenAddOnColumn(declaration1);
			DeleteAllGenAddOnColumn(declaration2);
			DeleteAllGenAddOnColumn(declaration3);
			DeleteAllGenAddOnColumn(declaration4);
			Factory.Save();
			var bizObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleFlagsFilter)bizObj[DeclarationFilterConstants.Reconciliation];
			filter.IsActive = true;
			filter.Property0 = true;
			AssertEquals("declaration1 has been reconciled", false, entry1.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration2 has been reconciled", false, entry2.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration3 has not been reconciled", true, entry3.MatchesFilter(bizObj.Filter));
			AssertEquals("declaration4 is not marked as reconcilable", false, entry4.MatchesFilter(bizObj.Filter));
		}

		public void TestGetImportationDateQuery()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_DateOfArrival = new ZDateTime(2008, 1, 1);
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var entry2 = declaration1.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_DateOfArrival = new ZDateTime(2008, 5, 1);
			var entry3 = declaration2.CustomsEntryHeaders.AddNew();
			entry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			Factory.Save();
			var filterBizObj = new EntryHeaderFilterBusinessObject();
			var importDate = (ModuleDateFilter)filterBizObj[EntryHeaderFilterBusinessObject.Schema.ImportationDate];
			importDate.IsActive = true;
			importDate.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			importDate.Property2 = new ZDateTime(2008, 4, 30);
			Assert("entry1 meets the filter", entry1.MatchesFilter(filterBizObj.Filter));
			Assert("entry2 meets the filter", entry2.MatchesFilter(filterBizObj.Filter));
			Assert("entry3 does not meet the filter", !entry3.MatchesFilter(filterBizObj.Filter));
			importDate.Property1 = new ZDateTime(2008, 5, 1);
			importDate.Property2 = ZDateTime.Empty;
			Assert("entry1 does not meet the filter", !entry1.MatchesFilter(filterBizObj.Filter));
			Assert("entry2 does not meet the filter", !entry2.MatchesFilter(filterBizObj.Filter));
			Assert("entry3 meets the filter", entry3.MatchesFilter(filterBizObj.Filter));
		}

		public void TestGetSuretyCodeQuery()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.US_SuretyCode = "891";
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry1.CH_BGMReference = "TEST_ENTRY1";
			var entry2 = declaration1.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry2.CH_BGMReference = "TEST_ENTRY2";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.US_SuretyCode = "892";
			var entry3 = declaration2.CustomsEntryHeaders.AddNew();
			entry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry3.CH_BGMReference = "TEST_ENTRY3";
			DeleteAllGenAddOnColumn(declaration1);
			DeleteAllGenAddOnColumn(declaration2);
			Factory.Save();
			var filterBizObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizObj[EntryHeaderFilterBusinessObject.Schema.SuretyCode];
			filter.IsActive = true;
			filter.Property = "891";
			Assert("entry1 meets the filter", entry1.MatchesFilter(filterBizObj.Filter));
			Assert("entry2 meets the filter", entry2.MatchesFilter(filterBizObj.Filter));
			Assert("entry3 does not meet the filter", !entry3.MatchesFilter(filterBizObj.Filter));
			filter.Property = "892";
			Assert("entry1 does not meet the filter", !entry1.MatchesFilter(filterBizObj.Filter));
			Assert("entry2 does not meet the filter", !entry2.MatchesFilter(filterBizObj.Filter));
			Assert("entry3 meets the filter", entry3.MatchesFilter(filterBizObj.Filter));
		}

		public void TestGetImporterOfRecordQuery()
		{
			var organisation1 = Factory.New<OrgHeader>();
			organisation1.FillWithValidTestData();
			organisation1.OH_IsConsignee = true;
			var organisation2 = Factory.New<OrgHeader>();
			organisation2.FillWithValidTestData();
			organisation2.OH_IsConsignee = true;
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.IOROrgPK = organisation1.PK;
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var entry2 = declaration1.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.IOROrgPK = organisation2.PK;
			var entry3 = declaration2.CustomsEntryHeaders.AddNew();
			entry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			Factory.Save();
			var filterBizObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleGuidFilter)filterBizObj[EntryHeaderFilterBusinessObject.Schema.ImporterOfRecord];
			filter.IsActive = true;
			filter.Property = organisation1.PK;
			Assert("entry1 meets the filter", entry1.MatchesFilter(filterBizObj.Filter));
			Assert("entry2 meets the filter", entry2.MatchesFilter(filterBizObj.Filter));
			Assert("entry3 does not meet the filter", !entry3.MatchesFilter(filterBizObj.Filter));
			filter.Property = organisation2.PK;
			Assert("entry1 does not meet the filter", !entry1.MatchesFilter(filterBizObj.Filter));
			Assert("entry2 does not meet the filter", !entry2.MatchesFilter(filterBizObj.Filter));
			Assert("entry3 meets the filter", entry3.MatchesFilter(filterBizObj.Filter));
		}

		public void TestGetPortOfEntryQuery()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.US_SchDEntry = "8888";
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var entry2 = declaration1.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.US_SchDEntry = "1234";
			var entry3 = declaration2.CustomsEntryHeaders.AddNew();
			entry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			DeleteAllGenAddOnColumn(declaration1);
			DeleteAllGenAddOnColumn(declaration2);
			Factory.Save();
			var filterBizObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleNkFilter)filterBizObj[EntryHeaderFilterBusinessObject.Schema.PortOfEntry];
			filter.IsActive = true;
			filter.Property = "8888";
			Assert("entry1 meets the filter", entry1.MatchesFilter(filterBizObj.Filter));
			Assert("entry2 meets the filter", entry2.MatchesFilter(filterBizObj.Filter));
			Assert("entry3 does not meet the filter", !entry3.MatchesFilter(filterBizObj.Filter));
			filter.Property = "1234";
			Assert("entry1 does not meet the filter", !entry1.MatchesFilter(filterBizObj.Filter));
			Assert("entry2 does not meet the filter", !entry2.MatchesFilter(filterBizObj.Filter));
			Assert("entry3 meets the filter", entry3.MatchesFilter(filterBizObj.Filter));
		}

		public void TestGetImportSourceQuery()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.US_SchDEntry = "8888";
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var entry2 = declaration1.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.US_SchDEntry = "4934";
			var entry3 = declaration2.CustomsEntryHeaders.AddNew();
			entry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.US_SchDEntry = "5134";
			var entry4 = declaration3.CustomsEntryHeaders.AddNew();
			entry4.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			DeleteAllGenAddOnColumn(declaration1);
			DeleteAllGenAddOnColumn(declaration2);
			DeleteAllGenAddOnColumn(declaration3);
			Factory.Save();
			var filterBizObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizObj[EntryHeaderFilterBusinessObject.Schema.ImportSource];
			filter.IsActive = true;
			filter.Property = ReconciliationImportEntrySourceList.Codes.FiftyStates;
			Assert("entry1 meets the filter", entry1.MatchesFilter(filterBizObj.Filter));
			Assert("entry2 meets the filter", entry2.MatchesFilter(filterBizObj.Filter));
			Assert("entry3 does not meet the filter", !entry3.MatchesFilter(filterBizObj.Filter));
			Assert("entry4 does not meet the filter", !entry4.MatchesFilter(filterBizObj.Filter));
			filter.Property = ReconciliationImportEntrySourceList.Codes.PuertoRico;
			Assert("entry1 does not meet the filter", !entry1.MatchesFilter(filterBizObj.Filter));
			Assert("entry2 does not meet the filter", !entry2.MatchesFilter(filterBizObj.Filter));
			Assert("entry3 meets the filter", entry3.MatchesFilter(filterBizObj.Filter));
			Assert("entry4 does not meet the filter", !entry4.MatchesFilter(filterBizObj.Filter));
			filter.Property = ReconciliationImportEntrySourceList.Codes.VirginIslands;
			Assert("entry1 does not meet the filter", !entry1.MatchesFilter(filterBizObj.Filter));
			Assert("entry2 does not meet the filter", !entry2.MatchesFilter(filterBizObj.Filter));
			Assert("entry3 does not meet the filter", !entry3.MatchesFilter(filterBizObj.Filter));
			Assert("entry4 meets the filter", entry4.MatchesFilter(filterBizObj.Filter));
			filter.Property = ZString.Empty;
			Assert("entry1 does not meet the filter", entry1.MatchesFilter(filterBizObj.Filter));
			Assert("entry2 does not meet the filter", entry2.MatchesFilter(filterBizObj.Filter));
			Assert("entry3 does not meet the filter", entry3.MatchesFilter(filterBizObj.Filter));
			Assert("entry4 does not meet the filter", entry4.MatchesFilter(filterBizObj.Filter));
		}

		public void TestOtherReconIndicator()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.US_OtherReconIndicator = ReconIssueCodeList.Codes.ValueRecon;
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var entry2 = declaration1.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.US_OtherReconIndicator = ReconIssueCodeList.Codes.Value9802Recon;
			var entry3 = declaration2.CustomsEntryHeaders.AddNew();
			entry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			DeleteAllGenAddOnColumn(declaration1);
			DeleteAllGenAddOnColumn(declaration2);
			Factory.Save();
			var filterBizObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizObj[EntryHeaderFilterBusinessObject.Schema.ReconIssue];
			filter.IsActive = true;
			filter.Property = ReconIssueCodeList.Codes.ValueRecon;
			Assert("entry1 meets the filter", entry1.MatchesFilter(filterBizObj.Filter));
			Assert("entry2 meets the filter", entry2.MatchesFilter(filterBizObj.Filter));
			Assert("entry3 does not meet the filter", !entry3.MatchesFilter(filterBizObj.Filter));
			filter.Property = ReconIssueCodeList.Codes.Value9802Recon;
			Assert("entry1 does not meet the filter", !entry1.MatchesFilter(filterBizObj.Filter));
			Assert("entry2 does not meet  the filter", !entry2.MatchesFilter(filterBizObj.Filter));
			Assert("entry3 meets the filter", entry3.MatchesFilter(filterBizObj.Filter));
		}

		public void TestNAFTAReconIndicator()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.US_NAFTAReconIndicator = true;
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var entry2 = declaration1.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.US_NAFTAReconIndicator = false;
			var entry3 = declaration2.CustomsEntryHeaders.AddNew();
			entry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			Factory.Save();
			var filterBizObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizObj[DeclarationFilterConstants.FTAReconIndicator];
			filter.IsActive = true;
			filter.Property0 = true;
			Assert("entry1 meets the filter", entry1.MatchesFilter(filterBizObj.Filter));
			Assert("entry2 meets the filter", entry2.MatchesFilter(filterBizObj.Filter));
			Assert("entry3 does not meet the filter", !entry3.MatchesFilter(filterBizObj.Filter));
			filter.Property0 = false;
			Assert("entry1 does not meet the filter", !entry1.MatchesFilter(filterBizObj.Filter));
			Assert("entry2 does not meet  the filter", !entry2.MatchesFilter(filterBizObj.Filter));
			Assert("entry3 meets the filter", entry3.MatchesFilter(filterBizObj.Filter));
		}

		public void TestExcludeIORFilingTheirOwnRecQuery()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_FileTheirOwnRecon = true;
			var entry = declaration1.CustomsEntryHeaders.AddNew();
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			Factory.Save();
			var filterBizObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizObj[DeclarationFilterConstants.ExcludeIORFilingTheirOwnRec];
			filter.IsActive = true;
			filter.Property0 = true;
			Assert("entry1 does not meet the filter", !entry.MatchesFilter(filterBizObj.Filter));
			Assert("entry2 meets the filter", entry2.MatchesFilter(filterBizObj.Filter));
		}

		public void TestEntryTypeQuery()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			declaration1.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			var declaration2 = Factory.New<JobDeclaration>();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			declaration2.US_EntryType = EntryTypeList.Codes.Warehouse;
			DeleteAllGenAddOnColumn(declaration1);
			DeleteAllGenAddOnColumn(declaration2);
			Factory.Save();
			var filterBizObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizObj[EntryHeaderFilterBusinessObject.Schema.EntryType];
			filter.IsActive = true;
			filter.Property = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Assert("entry1 meets the filter", entry1.MatchesFilter(filterBizObj.Filter));
			Assert("entry2 does not match the filter", !entry2.MatchesFilter(filterBizObj.Filter));
			filter.Property = EntryTypeList.Codes.Warehouse;
			Assert("entry1 does not meet the filter", !entry1.MatchesFilter(filterBizObj.Filter));
			Assert("entry2 meets  the filter", entry2.MatchesFilter(filterBizObj.Filter));
		}

		public void TestGetMasterBillQuery()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			var cusDecHouseBill1 = declaration1.Bills.AddNew();
			cusDecHouseBill1.CU_BillNum = "ABC1111";
			cusDecHouseBill1.CU_BillType = BillTypeList.Codes.MasterBill;
			var declaration2 = Factory.New<JobDeclaration>();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			var cusDecHouseBill2 = declaration2.Bills.AddNew();
			cusDecHouseBill2.CU_JE = declaration2.PK;
			cusDecHouseBill2.CU_BillNum = "ABC2222";
			cusDecHouseBill2.CU_BillType = BillTypeList.Codes.HouseBill;
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleNumberFilter)filterObj[EntryHeaderFilterBusinessObject.Schema.MasterBill];
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "ABC";
			filter.IsActive = true;
			Assert("entry1 meets the filter", entry1.MatchesFilter(filterObj.Filter));
			Assert("entry2 does not meets the filter", !entry2.MatchesFilter(filterObj.Filter));
		}

		public void TestGetHouseBillQuery()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			var cusDecHouseBill1 = declaration1.Bills.AddNew();
			cusDecHouseBill1.CU_BillNum = "ABC1111";
			cusDecHouseBill1.CU_BillType = BillTypeList.Codes.MasterBill;
			var declaration2 = Factory.New<JobDeclaration>();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			var cusDecHouseBill2 = declaration2.Bills.AddNew();
			cusDecHouseBill2.CU_JE = declaration2.PK;
			cusDecHouseBill2.CU_BillNum = "ABC2222";
			cusDecHouseBill2.CU_BillType = BillTypeList.Codes.HouseBill;
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleNumberFilter)filterObj[EntryHeaderFilterBusinessObject.Schema.HouseBill];
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "ABC";
			filter.IsActive = true;
			Assert("entry1 does not meets the filter", !entry1.MatchesFilter(filterObj.Filter));
			Assert("entry2 meets the filter", entry2.MatchesFilter(filterObj.Filter));
		}

		public void TestGetReleaseDateQuery()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_EntryAuthorisationDate = new ZDateTime(2019, 2, 1);
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_EntryAuthorisationDate = new ZDateTime(2019, 2, 6);
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			Factory.Save();
			var filterBizObj = new EntryHeaderFilterBusinessObject();
			var releaseDate = (ModuleDateFilter)filterBizObj[JobDeclaration.Constants.USFilterConstants.ReleaseDate];
			releaseDate.IsActive = true;
			releaseDate.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			releaseDate.Property2 = new ZDateTime(2019, 1, 30);
			releaseDate.Property2 = new ZDateTime(2019, 2, 3);
			Assert("entry1 meets the filter", entry1.MatchesFilter(filterBizObj.Filter));
			Assert("entry2 does not meet the filter", !entry2.MatchesFilter(filterBizObj.Filter));
		}

		public void TestModuleFiltersAreAdded()
		{
			var moduleFilters = new List<ZString>()
			{
				EntryHeaderFilterBusinessObject.Schema.ImportationDate,
				EntryHeaderFilterBusinessObject.Schema.SuretyCode,
				EntryHeaderFilterBusinessObject.Schema.ImporterOfRecord,
				EntryHeaderFilterBusinessObject.Schema.PortOfEntry,
				EntryHeaderFilterBusinessObject.Schema.ReconIssue,
				EntryHeaderFilterBusinessObject.Schema.EntryType,
				EntryHeaderFilterBusinessObject.Schema.MasterBill,
				EntryHeaderFilterBusinessObject.Schema.HouseBill,
			};
			var filterObj = GetNewFilterStripBusinessObject();
			foreach (var filter in moduleFilters)
			{
				AssertNotNull(filterObj[filter]);
			}
		}

		public void TestSuretyCodeDefaultComparison()
		{
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[EntryHeaderFilterBusinessObject.Schema.SuretyCode];
			AssertEquals(ModuleTextFilter.ComparisonConstants.Exact, filter.ComparisonOperator);
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var reconOriginalEntry = reconDec.OriginalEntries.AddNew();
			reconOriginalEntry.CH_OrigEntryReference = "XJ5";
			filterObj.SetExternalDefaults(reconOriginalEntry.Lookups.Entries);
			filter = (ModuleTextFilter)filterObj[EntryHeaderFilterBusinessObject.Schema.SuretyCode];
			AssertEquals(ModuleTextFilter.ComparisonConstants.Exact, filter.ComparisonOperator);
		}

		public void TestDuplicateStatusFilter()
		{
			var filterBizO = new EntryHeaderFilterBusinessObject();
			var filters = new FilterBusinessObjectDefaults();
			filters.Add(new FilterBusinessObjectDefault("FTA Recon", "Property0", ZBool.True));
			using (var module = new EntryHeaderModule())
			{
				var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
				using (var reconDeclarationForm = new ReconDeclarationForm(reconDec))
				{
					module.ParentModalFormOwner = reconDeclarationForm;
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
				}

				using (var declarationForm = new JobDeclarationForm())
				{
					module.ParentModalFormOwner = declarationForm;
					filterBizO.SetExternalDefaults(filters);
					filterBizO.LoadLayout(null);
					var strips = filterBizO.FilterStrips;
					AssertEquals(3, strips.Count);
					var strip1 = strips[1];
					AssertEquals("Entry Type", strip1.FilterDescription);
					AssertEquals(EntryTypeList.Codes.ConsumptionFreeDutiable, strip1.CurrentModuleFilter["Property"]);
					var textModule1 = strip1.CurrentModuleFilter as ModuleTextFilter;
					AssertNotNull(textModule1);
					var strip2 = strips[2];
					AssertEquals("Entry Type", strip2.FilterDescription);
					AssertEquals(EntryTypeList.Codes.InformalFreeDutiable, strip2.CurrentModuleFilter["Property"]);
					var textModule2 = strip2.CurrentModuleFilter as ModuleTextFilter;
					AssertNotNull(textModule2);
				}
			}
		}

		void DeleteAllGenAddOnColumn(JobDeclaration declaration)
		{
			var query = new ZQuery(GenAddOnColumnSchema.XA_ParentID, declaration.PK);
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, declaration.TablePrefix);
			Factory.Load<GenAddOnColumn>(query).DeleteAll();
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = base.GetFiltersExcludedFromSubgroupCheckForCommonTables();
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, EntryHeaderFilterBusinessObject.Schema.SuretyCode));
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, EntryHeaderFilterBusinessObject.Schema.ImportSource));
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, EntryHeaderFilterBusinessObject.Schema.EntryType));
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, EntryHeaderFilterBusinessObject.Schema.PortOfEntry));
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, DeclarationFilterConstants.FTAReconIndicator));
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, EntryHeaderFilterBusinessObject.Schema.ImporterOfRecord));
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, EntryHeaderFilterBusinessObject.Schema.ReconIssue));
			result.Add(TableFilter(GenAddOnColumnSchema.Constants.TableName, EntryHeaderFilterBusinessObject.Schema.SuretyCode));
			result.Add(TableFilter(GenAddOnColumnSchema.Constants.TableName, EntryHeaderFilterBusinessObject.Schema.ImportSource));
			result.Add(TableFilter(GenAddOnColumnSchema.Constants.TableName, EntryHeaderFilterBusinessObject.Schema.EntryType));
			result.Add(TableFilter(GenAddOnColumnSchema.Constants.TableName, EntryHeaderFilterBusinessObject.Schema.PortOfEntry));
			result.Add(TableFilter(GenAddOnColumnSchema.Constants.TableName, DeclarationFilterConstants.FTAReconIndicator));
			result.Add(TableFilter(GenAddOnColumnSchema.Constants.TableName, EntryHeaderFilterBusinessObject.Schema.ImporterOfRecord));
			result.Add(TableFilter(GenAddOnColumnSchema.Constants.TableName, EntryHeaderFilterBusinessObject.Schema.ReconIssue));
			result.Add(TableFilter(CusDecHouseBillSchema.Constants.TableName, EntryHeaderFilterBusinessObject.Schema.MasterBill));
			return result;
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			var result = base.GetFiltersExcludedFromSubgroupCheckForCommonTables();
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, EntryHeaderFilterBusinessObject.Schema.SuretyCode));
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, EntryHeaderFilterBusinessObject.Schema.ImportSource));
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, EntryHeaderFilterBusinessObject.Schema.EntryType));
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, EntryHeaderFilterBusinessObject.Schema.PortOfEntry));
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, DeclarationFilterConstants.FTAReconIndicator));
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, EntryHeaderFilterBusinessObject.Schema.ImporterOfRecord));
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, EntryHeaderFilterBusinessObject.Schema.ReconIssue));
			result.Add(TableFilter(GenAddOnColumnSchema.Constants.TableName, EntryHeaderFilterBusinessObject.Schema.SuretyCode));
			result.Add(TableFilter(GenAddOnColumnSchema.Constants.TableName, EntryHeaderFilterBusinessObject.Schema.ImportSource));
			result.Add(TableFilter(GenAddOnColumnSchema.Constants.TableName, EntryHeaderFilterBusinessObject.Schema.EntryType));
			result.Add(TableFilter(GenAddOnColumnSchema.Constants.TableName, EntryHeaderFilterBusinessObject.Schema.PortOfEntry));
			result.Add(TableFilter(GenAddOnColumnSchema.Constants.TableName, DeclarationFilterConstants.FTAReconIndicator));
			result.Add(TableFilter(GenAddOnColumnSchema.Constants.TableName, EntryHeaderFilterBusinessObject.Schema.ImporterOfRecord));
			result.Add(TableFilter(GenAddOnColumnSchema.Constants.TableName, EntryHeaderFilterBusinessObject.Schema.ReconIssue));
			return result;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EntryHeaderFilterBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			try
			{
				//GlbCompany.CurrentCompany.SetCountry changes GC_RN_NKCountryCode, and needs to be saved to db as JobDeclarationFilter(DBOnlyQuery) is performed in FilterObject.
				GlbCompany.CurrentCompany.Factory.Save();
			}
			finally
			{
				((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = false;
			}
		}
	}
}
