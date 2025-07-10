using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CusPermitFilterStripBusinessObject))]
	public class CusPermitFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		[TestDate(2022, 8, 8)]
		public void TestEndDateFilter()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var helper = new PermitTestDataHelper(Factory);
			var permit1 = helper.CreatePermitHeader(Core.Constants.CountryCodes.France, orgHeader.PK, "PERMIT1", ZDate.Today.AddDays(-1), ZDate.Empty, PermitQtyValIndicatorList.Codes.QTY, "IMP");
			var permit2 = helper.CreatePermitHeader(Core.Constants.CountryCodes.France, orgHeader.PK, "PERMIT2", ZDate.Today.AddDays(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.QTY, "IMP");
			var permit3 = helper.CreatePermitHeader(Core.Constants.CountryCodes.France, orgHeader.PK, "PERMIT3", ZDate.Today.AddDays(-1), ZDate.Today.AddMonths(-1), PermitQtyValIndicatorList.Codes.QTY, "IMP");

			var filter = new CusPermitFilterStripBusinessObject();
			var creationCountryFilter = (ModuleNkFilter)filter[CusPermitHeaderCollection.FilterConstants.Country];
			creationCountryFilter.Property = Core.Constants.CountryCodes.France;
			creationCountryFilter.IsActive = true;

			var permitCollection = new CusPermitHeaderCollection(Factory);
			permitCollection.AdditionalFilter = filter.Filter;
			AssertEquals("All 3 french permits should match because the only additional filter is on the country.", 3, permitCollection.Count);

			//Search valid permits
			var endDateFilter = (ModuleDateFilter)filter[CusPermitHeaderCollection.FilterConstants.EndDate];
			endDateFilter.IsActive = true;
			endDateFilter.Property1 = ZDate.Today;
			endDateFilter.Property2 = ZDate.Empty;
			endDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			permitCollection = new CusPermitHeaderCollection(Factory);
			permitCollection.AdditionalFilter = filter.Filter;
			AssertEquals("Only valid french permits should match because there is now a filter on End Date.", 2, permitCollection.Count);
			AssertContainsExactElementsInAnyOrder("permit1 is considered as valid because its End Date has been left empty.", new BaseCusPermitHeader[] { permit1, permit2 }, permitCollection);

			//Search permits within date range
			endDateFilter = (ModuleDateFilter)filter[CusPermitHeaderCollection.FilterConstants.EndDate];
			endDateFilter.IsActive = true;
			endDateFilter.Property1 = ZDate.Today;
			endDateFilter.Property2 = ZDate.Today.AddMonths(1);
			endDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			permitCollection = new CusPermitHeaderCollection(Factory);
			permitCollection.AdditionalFilter = filter.Filter;
			AssertContainsExactElementsInAnyOrder("validGuarantee2 is the only guarantee whose end date is exactly between today and next month.", new BaseCusPermitHeader[] { permit2 }, permitCollection);
		}

		public void TestPermitRule()
		{
			var guarantee1 = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			guarantee1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			var header1 = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			header1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			var rule1 = header1.CusPermitRules.AddNew();
			rule1.CPR_RuleCode = "TAR";
			rule1.CPR_ValueFrom = "076010";
			rule1.CPR_ValueTo = "076020";
			var ruleException1 = rule1.CusPermitRuleExceptions.AddNew();
			ruleException1.CPE_ValueFrom = "076015";
			ruleException1.CPE_ValueTo = "076017";
			var header2 = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			header2.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			var rule2 = header2.CusPermitRules.AddNew();
			rule2.CPR_RuleCode = "TAR";
			rule2.CPR_ValueFrom = "176010";
			rule2.CPR_ValueTo = "176020";
			Factory.Save();
			CusPermitFilterStripBusinessObject filter = new CusPermitFilterStripBusinessObject();
			var tariffFilter = (PermitRuleModuleFilter)filter[CusPermitFilterStripBusinessObject.Schema.PermitRule];
			tariffFilter.Property0 = Core.Constants.CountryCodes.SouthAfrica;
			tariffFilter.Property1 = BaseCusPermitRule.RuleCodes.Tariff;
			tariffFilter.Property2 = "076012";
			tariffFilter.IsActive = true;
			CusPermitHeaderCollection coll = new CusPermitHeaderCollection(Factory);
			coll.MatchesFilterDelegate = filter.MatchesFilter;
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals(header1.PK, coll[0].PK);
			tariffFilter.Property2 = "076015";
			coll.RefreshFromDb();
			AssertEquals(0, coll.Count);
		}

		public void TestIsCurrent()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var helper = new PermitTestDataHelper(Factory);
			var startDate = ZDate.Today;
			var endDate = ZDate.Today;
			var header1 = helper.CreatePermitHeader(Core.Constants.CountryCodes.SouthAfrica, importer.PK, "PERMIT1", startDate, endDate, PermitQtyValIndicatorList.Codes.QTY, "IMP");
			var transaction1 = helper.CreatePermitLineTransaction(header1, "QTY > 0", ZString.Empty, ZString.Empty, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 0m, 1000m);
			var header2 = helper.CreatePermitHeader(Core.Constants.CountryCodes.SouthAfrica, importer.PK, "PERMIT2", startDate, endDate, PermitQtyValIndicatorList.Codes.QTY, "IMP");
			var transaction2 = helper.CreatePermitLineTransaction(header2, "VAL CAT", ZString.Empty, ZString.Empty, PermitTransactionCategoryList.Codes.VAL, PermitTransactionTypeList.Codes.OBL, 0m, 1000m);
			var header3 = helper.CreatePermitHeader(Core.Constants.CountryCodes.SouthAfrica, importer.PK, "PERMIT3", startDate, endDate, PermitQtyValIndicatorList.Codes.QTY, "IMP");
			var transaction3 = helper.CreatePermitLineTransaction(header3, "QTY == 0", ZString.Empty, ZString.Empty, PermitTransactionCategoryList.Codes.VAL, PermitTransactionTypeList.Codes.OBL, 0m, 0m);
			var header4 = helper.CreatePermitHeader(Core.Constants.CountryCodes.SouthAfrica, importer.PK, "PERMIT4", startDate, endDate, PermitQtyValIndicatorList.Codes.QTY, "IMP");
			var header5 = helper.CreatePermitHeader(Core.Constants.CountryCodes.SouthAfrica, importer.PK, "PERMIT5", startDate, endDate, PermitQtyValIndicatorList.Codes.VAL, "IMP");
			var transaction5 = helper.CreatePermitLineTransaction(header5, "QTY > 0", ZString.Empty, ZString.Empty, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 1000m, 0m);
			var header6 = helper.CreatePermitHeader(Core.Constants.CountryCodes.SouthAfrica, importer.PK, "PERMIT6", startDate, endDate, PermitQtyValIndicatorList.Codes.BTH, "IMP");
			var transaction6 = helper.CreatePermitLineTransaction(header6, "QTY > 0", ZString.Empty, ZString.Empty, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 1000m, 1000m);
			Factory.Save();
			CusPermitFilterStripBusinessObject filter = new CusPermitFilterStripBusinessObject();
			ModuleFlagsFilter isCurrentFilter = (ModuleFlagsFilter)filter[CusPermitFilterStripBusinessObject.Schema.IsCurrent];
			isCurrentFilter.Property0 = true;
			isCurrentFilter.IsActive = true;
			CusPermitHeaderCollection coll = new CusPermitHeaderCollection(Factory);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(3, coll.Count);
			AssertCollectionContains("QTY match", header1, coll);
			AssertCollectionNotContains("Do not match VAL category", header2, coll);
			AssertCollectionNotContains("Transaction QTY not GT 0", header3, coll);
			AssertCollectionNotContains("No Transaction exist", header4, coll);
			AssertCollectionContains("VAL match", header5, coll);
			AssertCollectionContains("BTH match", header6, coll);
		}

		public void TestPermitHolder()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var header1 = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			header1.CPH_OH_PermitHolder = orgHeader1.PK;
			var header2 = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			header2.CPH_OH_PermitHolder = orgHeader2.PK;
			Factory.Save();
			CusPermitFilterStripBusinessObject filter = new CusPermitFilterStripBusinessObject();
			ModuleGuidFilter permitHolderFilter = (ModuleGuidFilter)filter[CusPermitFilterStripBusinessObject.Schema.PermitHolder];
			permitHolderFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			permitHolderFilter.Property = orgHeader1.PK;
			permitHolderFilter.IsActive = true;
			CusPermitHeaderCollection coll = new CusPermitHeaderCollection(Factory);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals(header1.PK, coll[0].PK);
		}

		public void TestPermitTypeSubType()
		{
			var header1 = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			header1.CPH_Type = "EXP";
			var header2 = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			header2.CPH_Type = "IMP";
			var header3 = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			header3.CPH_Type = "RCC";
			header3.CPH_SubType = "ACO";
			var header4 = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			header4.CPH_Type = "RCC";
			header4.CPH_SubType = "LVE";
			Factory.Save();
			CusPermitFilterStripBusinessObject filter = new CusPermitFilterStripBusinessObject();
			PermitTypeModuleFilter permitTypeSubTypeFilter = (PermitTypeModuleFilter)filter[CusPermitFilterStripBusinessObject.Schema.PermitTypeSubType];
			AssertEquals("Description", CusPermitFilterStripBusinessObject.Schema.PermitTypeSubType, permitTypeSubTypeFilter.MultilingualDescription.ToString());
			permitTypeSubTypeFilter.Property1 = "IMP";
			permitTypeSubTypeFilter.IsActive = true;
			CusPermitHeaderCollection coll = new CusPermitHeaderCollection(Factory);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals(header2.PK, coll[0].PK);
			permitTypeSubTypeFilter.Property1 = "RCC";
			permitTypeSubTypeFilter.Property2 = "LVE";
			coll = new CusPermitHeaderCollection(Factory);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals(header4.PK, coll[0].PK);
		}

		public void TestFilters()
		{
			var filter = new CusPermitFilterStripBusinessObject();
			AssertNotNull(filter[CusPermitFilterStripBusinessObject.Schema.PermitHolder]);
			AssertNotNull(filter[CusPermitFilterStripBusinessObject.Schema.PermitNumber]);
			AssertNotNull(filter[CusPermitFilterStripBusinessObject.Schema.StartDate]);
			AssertNotNull(filter[CusPermitFilterStripBusinessObject.Schema.EndDate]);
			AssertNotNull(filter[CusPermitFilterStripBusinessObject.Schema.PermitTypeSubType]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CusPermitFilterStripBusinessObject();

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			var result = new List<Tuple<string, string>>();
			result.Add(TableFilter("CusPermitRule", CusPermitFilterStripBusinessObject.Schema.PermitRule));
			result.Add(TableFilter("CusPermitRuleException", CusPermitFilterStripBusinessObject.Schema.PermitRule));
			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, CusPermitFilterStripBusinessObject.Schema.AppliesTo));
			result.Add(TableFilter(OrgAddressSchema.Constants.TableName, CusPermitFilterStripBusinessObject.Schema.AppliesTo));
			return result;
		}
	}
}
