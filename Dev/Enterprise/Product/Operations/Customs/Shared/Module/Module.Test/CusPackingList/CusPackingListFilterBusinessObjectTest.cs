using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CusPackingListFilterBusinessObject))]
	sealed class CusPackingListFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return (CusPackingListFilterBusinessObject)Activator.CreateInstance(GetExpectedBusinessObjectType());
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			var exclusions = base.GetFiltersExcludedFromSubgroupCheck();
			exclusions.Add(TableFilter(JobDeclarationSchema.Constants.TableName, CusPackingListFilterBusinessObject.FilterTypes.EntryNumber));
			exclusions.Add(TableFilter(CusEntryHeaderSchema.Constants.TableName, CusPackingListFilterBusinessObject.FilterTypes.EntryNumber));
			exclusions.Add(TableFilter(CusEntryNumSchema.Constants.TableName, CusPackingListFilterBusinessObject.FilterTypes.EntryNumber));
			exclusions.Add(TableFilter(JobDocAddressSchema.Constants.TableName, CusPackingListFilterBusinessObject.FilterTypes.ImporterName));
			exclusions.Add(TableFilter(OrgAddressSchema.Constants.TableName, CusPackingListFilterBusinessObject.FilterTypes.ImporterName));
			exclusions.Add(TableFilter(OrgHeaderSchema.Constants.TableName, CusPackingListFilterBusinessObject.FilterTypes.ImporterName));
			exclusions.Add(TableFilter(JobDocAddressSchema.Constants.TableName, CusPackingListFilterBusinessObject.FilterTypes.SupplierName));
			exclusions.Add(TableFilter(OrgAddressSchema.Constants.TableName, CusPackingListFilterBusinessObject.FilterTypes.SupplierName));
			exclusions.Add(TableFilter(OrgHeaderSchema.Constants.TableName, CusPackingListFilterBusinessObject.FilterTypes.SupplierName));
			return exclusions;
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = base.GetFiltersExcludedFromSubgroupCheckForCommonTables();
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, CusPackingListFilterBusinessObject.FilterTypes.EntryNumber));
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, CusPackingListFilterBusinessObject.FilterTypes.ImporterName));
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, CusPackingListFilterBusinessObject.FilterTypes.SupplierName));
			result.Add(TableFilter(JobDocAddressSchema.Constants.TableName, CusPackingListFilterBusinessObject.FilterTypes.SupplierName));
			result.Add(TableFilter(OrgAddressSchema.Constants.TableName, CusPackingListFilterBusinessObject.FilterTypes.SupplierName));
			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, CusPackingListFilterBusinessObject.FilterTypes.SupplierName));
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, CusPackingListFilterBusinessObject.FilterTypes.ImporterSupplier));
			return result;
		}

		public void TestJobNumberFilter()
		{
			var testDecl = BaseJobDeclaration.New(Factory);
			var testPackingList = testDecl.LoadOrCreateCusPackingList(Factory);
			testPackingList.PackageJob.KJ_JobID = "Job1234";
			Factory.Save();
			var filter = (ModuleNumberFilter)filterBO[CusPackingListFilterBusinessObject.FilterTypes.JobNumber];
			filter.IsActive = true;
			filter.Property = "Job1234";
			var packingLists = Factory.Load<CusPackingList>(filterBO.Filter);
			AssertEquals("Matched Job Number on Packing List", 1, packingLists.Length);
			AssertEquals("Matched Job Number on Packing List", "Job1234", packingLists[0].PackageJob.KJ_JobID);
			filter.Property = "Job12345";
			packingLists = Factory.Load<CusPackingList>(filterBO.Filter);
			AssertEquals("Not Matched Job Number on Packing List", 0, packingLists.Length);
		}

		public void TestPackingListNumberFilter()
		{
			var testDecl = BaseJobDeclaration.New(Factory);
			var testPackingList = testDecl.LoadOrCreateCusPackingList(Factory);
			testPackingList.CUL_PackingListNumber = "test1234";
			Factory.Save();
			var filter = (ModuleNumberFilter)filterBO[CusPackingListFilterBusinessObject.FilterTypes.PackingListNumber];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "test";
			Assert(testPackingList.MatchesFilter(filterBO.Filter));
			filter.Property = "1234";
			Assert(!testPackingList.MatchesFilter(filterBO.Filter));
			filter.Property = "test1234";
			Assert(testPackingList.MatchesFilter(filterBO.Filter));
		}

		public void TestPackingListDateFilter()
		{
			var testDecl = BaseJobDeclaration.New(Factory);
			var testPackingList = testDecl.LoadOrCreateCusPackingList(Factory);
			testPackingList.CUL_PackingListDate = new ZDate(2021, 5, 21);
			Factory.Save();
			var filter = (ModuleDateFilter)filterBO[CusPackingListFilterBusinessObject.FilterTypes.PackingListDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = testPackingList.CUL_PackingListDate;
			filter.Property2 = testPackingList.CUL_PackingListDate;
			Assert(testPackingList.MatchesFilter(filterBO.Filter));
			filter.Property1 = testPackingList.CUL_PackingListDate.AddDays(1);
			filter.Property2 = testPackingList.CUL_PackingListDate.AddDays(2);
			Assert(!testPackingList.MatchesFilter(filterBO.Filter));
		}

		public void TestRemarksFilter()
		{
			var testDecl = BaseJobDeclaration.New(Factory);
			var testPackingList = testDecl.LoadOrCreateCusPackingList(Factory);
			testPackingList.CUL_Remarks = "Test Remarks";
			Factory.Save();
			var filter = (ModuleTextFilter)filterBO[CusPackingListFilterBusinessObject.FilterTypes.Remarks];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "Test";
			Assert(testPackingList.MatchesFilter(filterBO.Filter));
			filter.Property = "Remarks";
			Assert(!testPackingList.MatchesFilter(filterBO.Filter));
			filter.Property = "Test Remarks";
			Assert(testPackingList.MatchesFilter(filterBO.Filter));
		}

		public void TestImporterSearch()
		{
			TestOrganisationSearch(BaseJobDeclaration.Schema.JE_OH_Importer, "JE_OH_Importer", 1);
		}

		public void TestSupplierSearch()
		{
			TestOrganisationSearch(BaseJobDeclaration.Schema.JE_OH_Supplier, "JE_OH_Supplier", 2);
		}

		void TestOrganisationSearch(string declPropName, string packingListPropName, int property1Or2)
		{
			var org = OrgHeader.New(Factory);
			org.OH_Code = "_!_!_!";
			var testDecl = BaseJobDeclaration.New(Factory);
			testDecl[declPropName] = org.PK;
			var testPackingList = testDecl.LoadOrCreateCusPackingList(Factory);
			var filter = filterBO[CusPackingListFilterBusinessObject.FilterTypes.ImporterSupplier];
			if (filter is ModuleGuidsFilter guidsFilter)
			{
				if (property1Or2 == 1)
				{
					guidsFilter.Property1 = org.PK;
				}
				else
				{
					guidsFilter.Property2 = org.PK;
				}
			}
			else if (filter is ModuleGuidFilter guidFilter)
			{
				guidFilter.Property = org.PK;
			}

			filter.IsActive = true;
			Factory.Save();
			var packingLists = Factory.Load<CusPackingList>(filterBO.Filter);
			AssertEquals("Matched " + CusPackingListFilterBusinessObject.FilterTypes.ImporterSupplier + " on Packing List", 1, packingLists.Length);
			AssertEquals("Matched " + CusPackingListFilterBusinessObject.FilterTypes.ImporterSupplier + " on Packing List", org.PK, packingLists[0].Declaration[packingListPropName]);
		}

		public void TestImporterNameFilter()
		{
			var testOrg = Factory.New<OrgHeader>();
			var testAddress = testOrg.MainAddress;
			testAddress.OA_Address1 = "Address 1";
			testOrg.OH_Code = "TESTORG1";
			testOrg.OH_FullName = "Test Name";
			var testDecl = BaseJobDeclaration.New(Factory);
			testDecl.JE_OH_Importer = testOrg.PK;
			var importerDocumentaryAddress = testDecl.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_AddressOverride = false;
			importerDocumentaryAddress.E2_CompanyName = "E2 Company Name(IMD)";
			importerDocumentaryAddress.E2_OA_Address = testAddress.PK;
			var testPackingList = testDecl.LoadOrCreateCusPackingList(Factory);
			Factory.Save();
			var importerNameFilter = (ModuleTextFilter)filterBO[CusPackingListFilterBusinessObject.FilterTypes.ImporterName];
			importerNameFilter.IsActive = true;
			importerNameFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			importerNameFilter.Property = "E2 Company";
			Assert(!testPackingList.MatchesFilter(filterBO.Filter));
			importerNameFilter.Property = "Test";
			Assert(testPackingList.MatchesFilter(filterBO.Filter));
			testAddress.OA_CompanyNameOverride = "Override companyName";
			Factory.Save();
			Assert(!testPackingList.MatchesFilter(filterBO.Filter));
			importerNameFilter.Property = "Override companyName";
			Assert(testPackingList.MatchesFilter(filterBO.Filter));
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.E2_CompanyName = "E2 Company Name(SUD)";
			Factory.Save();
			Assert(!testPackingList.MatchesFilter(filterBO.Filter));
			importerNameFilter.Property = "E2 Company";
			Assert(testPackingList.MatchesFilter(filterBO.Filter));
		}

		public void TestSupplierNameFilter()
		{
			var testOrg = Factory.New<OrgHeader>();
			var testAddress = testOrg.MainAddress;
			testAddress.OA_Address1 = "Address 1";
			testOrg.OH_Code = "TESTORG1";
			testOrg.OH_FullName = "Test Name";
			var testDecl = BaseJobDeclaration.New(Factory);
			testDecl.JE_OH_Supplier = testOrg.PK;
			var supplierDocumentaryAddress = testDecl.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = false;
			supplierDocumentaryAddress.E2_CompanyName = "E2 Company Name(SUD)";
			supplierDocumentaryAddress.E2_OA_Address = testAddress.PK;
			var testPackingList = testDecl.LoadOrCreateCusPackingList(Factory);
			Factory.Save();
			var supplierNameFilter = (ModuleTextFilter)filterBO[CusPackingListFilterBusinessObject.FilterTypes.SupplierName];
			supplierNameFilter.IsActive = true;
			supplierNameFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			supplierNameFilter.Property = "Test";
			Assert(testPackingList.MatchesFilter(filterBO.Filter));
			testAddress.OA_CompanyNameOverride = "Override companyName";
			Factory.Save();
			Assert(!testPackingList.MatchesFilter(filterBO.Filter));
			supplierNameFilter.Property = "Override companyName";
			Assert(testPackingList.MatchesFilter(filterBO.Filter));
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_CompanyName = "E2 Company Name(SUD)";
			Factory.Save();
			Assert(!testPackingList.MatchesFilter(filterBO.Filter));
			supplierNameFilter.Property = "E2 Company";
			Assert(testPackingList.MatchesFilter(filterBO.Filter));
		}

		public void TestEntryNumberFilter()
		{
			var testDecl = BaseJobDeclaration.New(Factory);
			var declEntryNumber = CusEntryNumber.New(testDecl, testDecl.JE_MessageType, testDecl.CountryCode);
			declEntryNumber.CE_EntryNum = "B00007141";
			var entryHeader = testDecl.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "B00007142";
			var testPackingList = testDecl.LoadOrCreateCusPackingList(Factory);
			Factory.Save();
			var entryNumberFilter = (ModuleTextFilter)filterBO[CusPackingListFilterBusinessObject.FilterTypes.EntryNumber];
			entryNumberFilter.IsActive = true;
			entryNumberFilter.Property = "B00007141";
			Assert(testPackingList.MatchesFilter(filterBO.Filter));
			entryNumberFilter.Property = "B00007142";
			Assert(testPackingList.MatchesFilter(filterBO.Filter));
			entryNumberFilter.Property = "B00007143";
			Assert(!testPackingList.MatchesFilter(filterBO.Filter));
		}

		public void TestDeclarationJobFilter()
		{
			var testDecl = BaseJobDeclaration.New(Factory);
			testDecl.JE_DeclarationReference = "test1234";
			var testPackingList = testDecl.LoadOrCreateCusPackingList(Factory);
			Factory.Save();
			var declarationJobNumberFilter = (ModuleTextFilter)filterBO[CusPackingListFilterBusinessObject.FilterTypes.DeclarationJobNumber];
			declarationJobNumberFilter.IsActive = true;
			declarationJobNumberFilter.Property = "test1234";
			Assert(testPackingList.MatchesFilter(filterBO.Filter));
			declarationJobNumberFilter.Property = "1234";
			Assert(!testPackingList.MatchesFilter(filterBO.Filter));
		}

		CusPackingListFilterBusinessObject filterBO;

		protected override void SetUp()
		{
			base.SetUp();
			filterBO = (CusPackingListFilterBusinessObject)GetNewFilterStripBusinessObject();
		}
	}
}
