using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(GuaranteesFilterStripBusinessObject))]
	sealed class GuaranteesFilterStripBusinessObjectTest : GuaranteesFilterStripBusinessObjectAbstractTest
	{
		const string GuaranteeTypeTZ1 = "TZ1";

		public void TestGuaranteeTypeFilter()
		{
			var filter = new GuaranteesFilterStripBusinessObject();
			var referenceFilter = (ModuleTextFilter)filter[GuaranteesFilterStripBusinessObject.FilterConstants.GuaranteeType];
			AssertNotNull(referenceFilter);
			referenceFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			referenceFilter.Property = GuaranteeTypeTZ1;
			referenceFilter.IsActive = true;
			var guaranteesCollection = new CusGuaranteeHeaderCollection(Factory);
			guaranteesCollection.AdditionalFilter = filter.Filter;
			AssertEquals($"Only guarantees with guarantee type starting with {GuaranteeTypeTZ1} match filters.", 1, guaranteesCollection.Count);
			AssertEquals($"Only {nameof(guaranteeHeader1)} matches guarantee type filter.", guaranteeHeader1.PK, guaranteesCollection[0].PK);
		}

		[TestDate(2022, 8, 8)]
		public void TestEndDateFilter()
		{
			var validGuarantee1 = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			validGuarantee1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			validGuarantee1.CPH_Number = "G1";
			validGuarantee1.CPH_OH_PermitHolder = organization1.PK;
			validGuarantee1.CPH_SubType = "1";

			var validGuarantee2 = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			validGuarantee2.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			validGuarantee2.CPH_Number = "G2";
			validGuarantee2.CPH_OH_PermitHolder = organization1.PK;
			validGuarantee2.CPH_SubType = "1";
			validGuarantee2.CPH_EndDate = ZDate.Today.AddMonths(1);

			var expiredGuarantee3 = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			expiredGuarantee3.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			expiredGuarantee3.CPH_Number = "G3";
			expiredGuarantee3.CPH_OH_PermitHolder = organization1.PK;
			expiredGuarantee3.CPH_SubType = "1";
			expiredGuarantee3.CPH_EndDate = ZDate.Today.AddMonths(-1);

			var filter = new GuaranteesFilterStripBusinessObject();
			var creationCountryFilter = (ModuleNkFilter)filter[GuaranteesFilterStripBusinessObject.FilterConstants.CreationCountry];
			creationCountryFilter.Property = Core.Constants.CountryCodes.France;
			creationCountryFilter.IsActive = true;

			var guaranteesCollection = new CusGuaranteeHeaderCollection(Factory);
			guaranteesCollection.AdditionalFilter = filter.Filter;
			AssertEquals("All 3 french guarantees should match because the only additional filter is on the country.", 3, guaranteesCollection.Count);

			//Search valid guarantees
			var endDateFilter = (ModuleDateFilter)filter[GuaranteesFilterStripBusinessObject.FilterConstants.EndDate];
			endDateFilter.IsActive = true;
			endDateFilter.Property1 = ZDate.Today;
			endDateFilter.Property2 = ZDate.Empty;
			endDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			guaranteesCollection = new CusGuaranteeHeaderCollection(Factory);
			guaranteesCollection.AdditionalFilter = filter.Filter;
			AssertEquals("Only valid french guarantees should match because there is now a filter on End Date.", 2, guaranteesCollection.Count);
			AssertContainsExactElementsInAnyOrder("validGuarantee1 is considered as valid because its End Date has been left empty.", new BaseCusGuaranteeHeader[] { validGuarantee1, validGuarantee2 }, guaranteesCollection);

			//Search guarantees within date range
			endDateFilter = (ModuleDateFilter)filter[GuaranteesFilterStripBusinessObject.FilterConstants.EndDate];
			endDateFilter.IsActive = true;
			endDateFilter.Property1 = ZDate.Today;
			endDateFilter.Property2 = ZDate.Today.AddMonths(1);
			endDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			guaranteesCollection = new CusGuaranteeHeaderCollection(Factory);
			guaranteesCollection.AdditionalFilter = filter.Filter;
			AssertContainsExactElementsInAnyOrder("validGuarantee2 is the only guarantee whose end date is exactly between today and next month.", new BaseCusGuaranteeHeader[] { validGuarantee2 }, guaranteesCollection);
		}

		[TestDate(2019, 9, 5)]
		public void TestTransactionDateFilter()
		{
			var filter = new GuaranteesFilterStripBusinessObject();
			var dateFilter = (ModuleDateFilter)filter[GuaranteesFilterStripBusinessObject.FilterConstants.TransactionDate];
			AssertNotNull(dateFilter);
			dateFilter.IsActive = true;
			var guaranteesCollection = new CusGuaranteeHeaderCollection(Factory);
			guaranteesCollection.AdditionalFilter = filter.Filter;
			AssertEquals(2, guaranteesCollection.Count);
			AssertEquals("guaranteeHeader1 has transactions within no date range", guaranteeHeader1.PK, guaranteesCollection[0].PK);
			AssertEquals("guaranteeHeader2 has transactions within no date range", guaranteeHeader2.PK, guaranteesCollection[1].PK);
			dateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Last3Mths;
			guaranteesCollection = new CusGuaranteeHeaderCollection(Factory);
			guaranteesCollection.AdditionalFilter = filter.Filter;
			AssertEquals(1, guaranteesCollection.Count);
			AssertEquals("Only guaranteeHeader2 has transactions within the date range", guaranteeHeader2.PK, guaranteesCollection[0].PK);
		}

		public void TestGuaranteesReferenceFilter()
		{
			var filter = new GuaranteesFilterStripBusinessObject();
			var referenceFilter = (ModuleTextFilter)filter[GuaranteesFilterStripBusinessObject.FilterConstants.Reference];
			AssertNotNull(referenceFilter);
			referenceFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
			referenceFilter.Property = "P";
			referenceFilter.IsActive = true;
			var guaranteesCollection = new CusGuaranteeHeaderCollection(Factory);
			guaranteesCollection.AdditionalFilter = filter.Filter;
			AssertEquals(1, guaranteesCollection.Count);
			AssertEquals("Only guaranteeHeader2 has transactions with a reference that starts with P", guaranteeHeader2.PK, guaranteesCollection[0].PK);
		}

		[TestDate(2019, 9, 5)]
		public void TestGuaranteesReferenceFilterValidation()
		{
			var filter = new GuaranteesFilterStripBusinessObject();
			filter.FilterStrips.AddNew(GuaranteesFilterStripBusinessObject.FilterConstants.Reference);
			var referenceFilter = (ModuleTextFilter)filter[GuaranteesFilterStripBusinessObject.FilterConstants.Reference];
			AssertNotNull(referenceFilter);
			referenceFilter.IsActive = true;
			filter.FilterStrips.AddNew(GuaranteesFilterStripBusinessObject.FilterConstants.TransactionDate);
			var dateFilter = (ModuleDateFilter)filter[GuaranteesFilterStripBusinessObject.FilterConstants.TransactionDate];
			dateFilter.IsActive = true;
			AssertNotNull(dateFilter);
			filter.FilterStrips.AddNew(GuaranteesFilterStripBusinessObject.FilterConstants.TransactionDate);
			referenceFilter.Validation.ValidateAll();
			AssertHasError(referenceFilter.PropertyInfo, "Text based filters are costly to run on SQL Servers. To avoid having a query that times out before completion, please add a Transaction Date based filter with a range of 3 months or less.");
			dateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Last6Mths;
			referenceFilter.Validation.ValidateAll();
			AssertHasError(referenceFilter.PropertyInfo, "Text based filters are costly to run on SQL Servers. To avoid having a query that times out before completion, please add a Transaction Date based filter with a range of 3 months or less.");
			dateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Last3Mths;
			referenceFilter.Validation.ValidateAll();
			AssertNoError(referenceFilter.PropertyInfo, "Text based filters are costly to run on SQL Servers. To avoid having a query that times out before completion, please add a Transaction Date based filter with a range of 3 months or less.");
			dateFilter.Property1 = new ZDate(2019, 6, 5);
			dateFilter.Property2 = new ZDate(2019, 9, 6);
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			referenceFilter.Validation.ValidateAll();
			AssertHasError(referenceFilter.PropertyInfo, "Text based filters are costly to run on SQL Servers. To avoid having a query that times out before completion, please add a Transaction Date based filter with a range of 3 months or less.");
			dateFilter.Property1 = new ZDate(2019, 6, 5);
			dateFilter.Property2 = new ZDate(2019, 9, 5);
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			referenceFilter.Validation.ValidateAll();
			AssertNoError(referenceFilter.PropertyInfo, "Text based filters are costly to run on SQL Servers. To avoid having a query that times out before completion, please add a Transaction Date based filter with a range of 3 months or less.");
		}

		public void TestGuaranteeHolder()
		{
			var filter = new GuaranteesFilterStripBusinessObject();
			var guaranteeHolderFilter = (ModuleGuidFilter)filter[GuaranteesFilterStripBusinessObject.FilterConstants.GuaranteeHolder];
			AssertNotNull(guaranteeHolderFilter);
			guaranteeHolderFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			guaranteeHolderFilter.Property = organization1.PK;
			guaranteeHolderFilter.IsActive = true;
			var coll = new CusGuaranteeHeaderCollection(Factory);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals(guaranteeHeader1.PK, coll[0].PK);
		}

		public void TestGuaranteeNumber()
		{
			var filter = new GuaranteesFilterStripBusinessObject();
			var guaranteeNumberFilter = (ModuleTextFilter)filter[GuaranteesFilterStripBusinessObject.FilterConstants.GuaranteeNumber];
			AssertNotNull(guaranteeNumberFilter);
			guaranteeNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			guaranteeNumberFilter.Property = guaranteeHeader1.CPH_Number;
			guaranteeNumberFilter.IsActive = true;
			var coll = new CusGuaranteeHeaderCollection(Factory);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals(guaranteeHeader1.PK, coll[0].PK);
		}

		public void TestGuaranteeSubType()
		{
			var filter = new GuaranteesFilterStripBusinessObject();
			var guaranteeSubTypeFilter = (ModuleTextFilter)filter[GuaranteesFilterStripBusinessObject.FilterConstants.GuaranteeSubType];
			AssertNotNull(guaranteeSubTypeFilter);
			guaranteeSubTypeFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			guaranteeSubTypeFilter.Property = guaranteeHeader1.CPH_SubType;
			guaranteeSubTypeFilter.IsActive = true;
			var coll = new CusGuaranteeHeaderCollection(Factory);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals(guaranteeHeader1.PK, coll[0].PK);
		}

		public void TestGuaranteeHoldersFilter()
		{
			var filter = new GuaranteesFilterStripBusinessObject();
			var guaranteeHoldersFilter = (ModuleGuidsFilter)filter[GuaranteesFilterStripBusinessObject.FilterConstants.GuaranteeHolders];
			AssertNotNull(guaranteeHoldersFilter);
			guaranteeHoldersFilter.Property1 = organization1.PK;
			guaranteeHoldersFilter.Property2 = organization2.PK;
			guaranteeHoldersFilter.IsActive = true;
			var guaranteesCollection = new CusGuaranteeHeaderCollection(Factory);
			guaranteesCollection.AdditionalFilter = filter.Filter;
			AssertEquals(1, guaranteesCollection.Count);
			AssertEquals("Only guaranteeHeader1 has one of the guarantee holders in the filter", guaranteeHeader1.PK, guaranteesCollection[0].PK);
		}

		public void TestCreationCountryFilter()
		{
			var filter = new GuaranteesFilterStripBusinessObject();
			var creationCountryFilter = (ModuleNkFilter)filter[GuaranteesFilterStripBusinessObject.FilterConstants.CreationCountry];
			creationCountryFilter.IsActive = true;
			CombineAssertions(() =>
			{
				AssertEquals("Description", "Creation Country", creationCountryFilter.Description);
				AssertEquals("Category", FilterCategories.Organisations, creationCountryFilter.Category);
				AssertEquals("MaxLength", 2, creationCountryFilter.MaxLength);
				AssertEquals("Visibility", FilterVisibility.AlwaysVisible, creationCountryFilter.Visibility);
				AssertEquals("Default value", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, creationCountryFilter.DefaultProperty);
				creationCountryFilter.Property = Core.Constants.CountryCodes.Germany;
				AssertMatch("Germany", true, false);
				creationCountryFilter.Property = Core.Constants.CountryCodes.Australia;
				AssertMatch("Australia", false, true);
			});
			void AssertMatch(ZString message, bool match1, bool match2)
			{
				AssertEquals(message + "->guaranteeHeader1", match1, guaranteeHeader1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->guaranteeHeader2", match2, guaranteeHeader2.MatchesFilter(filter.Filter));
			}
		}

		public void TestOrganisationCountryFilter()
		{
			var filter = new GuaranteesFilterStripBusinessObject();
			var organisationCountryFilter = (ModuleNkFilter)filter[GuaranteesFilterStripBusinessObject.FilterConstants.OrganisationCountry];
			organisationCountryFilter.IsActive = true;
			CombineAssertions(() =>
			{
				AssertEquals("Description", "Organization Country", organisationCountryFilter.Description);
				AssertEquals("Category", FilterCategories.Organisations, organisationCountryFilter.Category);
				AssertEquals("MaxLength", 2, organisationCountryFilter.MaxLength);
				organisationCountryFilter.Property = Core.Constants.CountryCodes.France;
				AssertMatch("France", true, false);
				organisationCountryFilter.Property = Core.Constants.CountryCodes.Germany;
				AssertMatch("Germany", false, true);
			});
			void AssertMatch(ZString message, bool match1, bool match2)
			{
				AssertEquals(message + "->guaranteeHeader1", match1, guaranteeHeader1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->guaranteeHeader2", match2, guaranteeHeader2.MatchesFilter(filter.Filter));
			}
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			var exclusions = base.GetFiltersExcludedFromSubgroupCheck();
			exclusions.Add(TableFilter(CusPermitLineTransactionSchema.Constants.TableName, GuaranteesFilterStripBusinessObject.FilterConstants.Reference));
			exclusions.Add(TableFilter(OrgHeaderSchema.Constants.TableName, GuaranteesFilterStripBusinessObject.FilterConstants.OrganisationCountry));
			return exclusions;
		}

		protected override void SetUp()
		{
			base.SetUp();
			organization1 = Factory.NewWithValidTestData<OrgHeader>();
			organization1.OH_Code = "Org1";
			organization1.OH_RL_NKClosestPort = "FR123";
			organization2 = Factory.NewWithValidTestData<OrgHeader>();
			organization2.OH_Code = "Org2";
			organization2.OH_RL_NKClosestPort = "AU123";
			organization3 = Factory.NewWithValidTestData<OrgHeader>();
			organization3.OH_Code = "Org3";
			organization3.OH_RL_NKClosestPort = "DE123";
			AddGuarantees();
			Factory.Save();
		}

		void AddGuarantees()
		{
			guaranteeHeader1 = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			guaranteeHeader1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			guaranteeHeader1.CPH_Number = "G1";
			guaranteeHeader1.CPH_OH_PermitHolder = organization1.PK;
			guaranteeHeader1.CPH_Type = GuaranteeTypeTZ1;
			guaranteeHeader1.CPH_SubType = "1";
			AddPermitLine(guaranteeHeader1, "ARS", new ZDate(2019, 6, 4));
			guaranteeHeader2 = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			guaranteeHeader2.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			guaranteeHeader2.CPH_Number = "G2";
			guaranteeHeader2.CPH_OH_PermitHolder = organization3.PK;
			guaranteeHeader2.CPH_Type = "TZ2";
			guaranteeHeader2.CPH_SubType = "2";
			AddPermitLine(guaranteeHeader2, "PAA", new ZDate(2019, 6, 4));
			AddPermitLine(guaranteeHeader2, "PEE", new ZDate(2019, 6, 5));
			AddPermitLine(guaranteeHeader2, "POO", new ZDate(2019, 9, 5));
			AddPermitLine(guaranteeHeader2, "PZZ", new ZDate(2019, 9, 6));
		}

		BaseCusGuaranteeHeader guaranteeHeader1;
		BaseCusGuaranteeHeader guaranteeHeader2;
		OrgHeader organization1;
		OrgHeader organization2;
		OrgHeader organization3;
	}
}
