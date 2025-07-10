using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Module.Testing;

[TestedType(typeof(AuthorisationRuleFilterBusinessObject))]
sealed class AuthorisationRuleFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
{
	public void TestFilters()
	{
		var ruleTypeFilter = (ModuleTextFilter)filter["Rule Code"];
		ruleTypeFilter.IsActive = true;
		ruleTypeFilter.Property = "LOC";
		var ruleCodeFilter = (ModuleTextFilter)filter["Value"];
		ruleCodeFilter.IsActive = true;
		ruleCodeFilter.Property = "WAW";
		var descriptionFilter = (ModuleTextFilter)filter["Description"];
		descriptionFilter.IsActive = true;
		descriptionFilter.Property = "Test";
		var authorisationNumberFilter = (ModuleTextFilter)filter["Authorization Number"];
		authorisationNumberFilter.IsActive = true;
		authorisationNumberFilter.Property = "11234";
		var authorisationTypeFilter = (ModuleTextFilter)filter["Authorization Type"];
		authorisationTypeFilter.IsActive = true;
		authorisationTypeFilter.Property = "ACE";
		var holder = Factory.NewWithValidTestData<OrgHeader>();
		var authorizationHolderFilter = (ModuleGuidFilter)filter["Authorization Holder"];
		authorizationHolderFilter.IsActive = true;
		authorizationHolderFilter.Property = holder.PK;
		var countryFilter = (ModuleNkFilter)filter["Country/Region"];
		countryFilter.IsActive = true;
		countryFilter.Property = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		var cusAuthorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		var cusAuthorisationRule = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
		cusAuthorisationRule.CPR_RuleCode = "WRA";
		cusAuthorisationRule.CPR_ValueFrom = "WAD";

		CombineAssertions(() =>
		{
			cusAuthorisationHeader.CPH_Type = "ACE";
			Factory.Save();
			AssertEquals("Should not match when value not set.", false, cusAuthorisationRule.MatchesFilter(filter.Filter));
			cusAuthorisationHeader.CPH_Number = "11234";
			Factory.Save();
			AssertEquals("Should not match when value not set.", false, cusAuthorisationRule.MatchesFilter(filter.Filter));
			cusAuthorisationHeader.CPH_OH_PermitHolder = holder.PK;
			Factory.Save();
			AssertEquals("Should not match when value not set.", false, cusAuthorisationRule.MatchesFilter(filter.Filter));
			cusAuthorisationRule.CPR_RuleCode = "LOC";
			cusAuthorisationRule.CPR_ValueFrom = "WAF";
			Factory.Save();
			AssertEquals("Should not match when value not set.", false, cusAuthorisationRule.MatchesFilter(filter.Filter));
			cusAuthorisationRule.CPR_ValueFrom = "WAW";
			Factory.Save();
			AssertEquals("Should not match when value not set.", false, cusAuthorisationRule.MatchesFilter(filter.Filter));
			cusAuthorisationRule.CPR_Description = "Test";
			Factory.Save();
			AssertEquals("Should match when all values set.", true, cusAuthorisationRule.MatchesFilter(filter.Filter));
		});
	}

	public void TestAuthorisationNumberFilter()
	{
		var header1 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		header1.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		var rule1 = header1.CusAuthorisationRules.AddNew();
		header1.CPH_Number = "Number1";
		rule1.CPR_RuleCode = "LOC";
		rule1.CPR_ValueFrom = "WAW";
		var header2 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		header2.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		var rule2 = header2.CusAuthorisationRules.AddNew();
		header2.CPH_Number = "Number2";
		rule2.CPR_RuleCode = "LOC";
		rule2.CPR_ValueFrom = "WAW";
		var header3 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		header3.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		var rule3 = header3.CusAuthorisationRules.AddNew();
		header3.CPH_Number = "Mm3";
		rule3.CPR_RuleCode = "LOC";
		rule3.CPR_ValueFrom = "WAW";
		Factory.Save();
		var authorisationNumberFilter = (ModuleTextFilter)filter["Authorization Number"];
		authorisationNumberFilter.IsActive = true;

		CombineAssertions(() =>
		{
			AssertEquals("Description", "Authorization Number", authorisationNumberFilter.Description);
			authorisationNumberFilter.Property = "Number1";
			authorisationNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			AssertMatch("Exact", true, false, false);
			authorisationNumberFilter.Property = "Number";
			authorisationNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			AssertMatch("StartsWith", true, true, false);
			authorisationNumberFilter.Property = "m";
			authorisationNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			AssertMatch("Contains", true, true, true);
		});
		void AssertMatch(ZString message, bool match1, bool match2, bool match3)
		{
			AssertEquals(message + "->rule1", match1, rule1.MatchesFilter(filter.Filter));
			AssertEquals(message + "->rule2", match2, rule2.MatchesFilter(filter.Filter));
			AssertEquals(message + "->rule3", match3, rule3.MatchesFilter(filter.Filter));
		}
	}

	public void TestCountryFilter()
	{
		var countryFilter = (ModuleNkFilter)filter["Country/Region"];
		countryFilter.IsActive = true;
		CombineAssertions(() =>
		{
			AssertEquals("Description", "Country/Region", countryFilter.Description);
			AssertEquals("Visibility", FilterVisibility.AlwaysVisible, countryFilter.Visibility);
			AssertEquals("ReadOnly", true, countryFilter.ReadOnly);
			AssertEquals("Default value", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, countryFilter.DefaultProperty);
		});
	}

	public void TestAuthorisationTypeFilter()
	{
		var header1 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		header1.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		var rule1 = header1.CusAuthorisationRules.AddNew();
		rule1.CPR_RuleCode = "LOC";
		header1.CPH_Type = "ACE";
		rule1.CPR_ValueFrom = "WAW";
		var header2 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		header2.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		var rule2 = header2.CusAuthorisationRules.AddNew();
		rule2.CPR_RuleCode = "LOC";
		header2.CPH_Type = "ACF";
		rule2.CPR_ValueFrom = "WAW";
		var header3 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		header3.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		var rule3 = header3.CusAuthorisationRules.AddNew();
		rule3.CPR_RuleCode = "LOC";
		header3.CPH_Type = "ACB";
		rule3.CPR_ValueFrom = "WAW";
		Factory.Save();
		var authorisationTypeFilter = (ModuleTextFilter)filter["Authorization Type"];
		authorisationTypeFilter.IsActive = true;

		CombineAssertions(() =>
		{
			AssertEquals("Description", "Authorization Type", authorisationTypeFilter.Description);
			authorisationTypeFilter.Property = "ACE";
			AssertMatch("Exact", true, false, false);
			authorisationTypeFilter.Property = "ACF";
			AssertMatch("StartsWith", false, true, false);
			authorisationTypeFilter.Property = "ACB";
			AssertMatch("Contains", false, false, true);
		});
		void AssertMatch(ZString message, bool match1, bool match2, bool match3)
		{
			AssertEquals(message + "->rule1", match1, rule1.MatchesFilter(filter.Filter));
			AssertEquals(message + "->rule2", match2, rule2.MatchesFilter(filter.Filter));
			AssertEquals(message + "->rule3", match3, rule3.MatchesFilter(filter.Filter));
		}
	}

	public void TestAuthorisationHolderFilter()
	{
		var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
		var header1 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		header1.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		var rule1 = header1.CusAuthorisationRules.AddNew();
		rule1.CPR_RuleCode = "LOC";
		rule1.CPR_ValueFrom = "WAW";
		header1.CPH_OH_PermitHolder = orgHeader1.PK;
		var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
		var header2 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		header2.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		var rule2 = header2.CusAuthorisationRules.AddNew();
		rule2.CPR_RuleCode = "LOC";
		rule2.CPR_ValueFrom = "WAW";
		header2.CPH_OH_PermitHolder = orgHeader2.PK;
		Factory.Save();
		var authorisationHolderFilter = (ModuleGuidFilter)filter["Authorization Holder"];
		authorisationHolderFilter.IsActive = true;

		CombineAssertions(() =>
		{
			AssertEquals("Description", "Authorization Holder", authorisationHolderFilter.Description);
			authorisationHolderFilter.Property = orgHeader1.PK;
			AssertEquals("Should match when values same.", true, rule1.MatchesFilter(filter.Filter));
			authorisationHolderFilter.Property = orgHeader2.PK;
			AssertEquals("Should match when values same.", true, rule2.MatchesFilter(filter.Filter));
		});
	}

	public void TestAuthorisationStartDateFilter()
	{
		var header1 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		header1.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		var rule1 = header1.CusAuthorisationRules.AddNew();
		rule1.CPR_RuleCode = "LOC";
		header1.CPH_Type = "ACE";
		header1.CPH_StartDate = ZDate.Today;
		rule1.CPR_ValueFrom = "WAW";
		var header2 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		header2.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		var rule2 = header2.CusAuthorisationRules.AddNew();
		rule2.CPR_RuleCode = "LOC";
		header2.CPH_Type = "ACE";
		header2.CPH_StartDate = ZDate.Today.AddDays(1);
		rule2.CPR_ValueFrom = "WAW";
		var header3 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		header3.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		var rule3 = header3.CusAuthorisationRules.AddNew();
		rule3.CPR_RuleCode = "LOC";
		header3.CPH_Type = "ACE";
		header3.CPH_StartDate = ZDate.Today.AddDays(-1);
		rule3.CPR_ValueFrom = "WAW";
		Factory.Save();
		var authorisationCollection = new CusAuthorisationHeaderCollection(Factory);
		var startDateFilter = (ModuleDateFilter)filter["Start Date"];
		startDateFilter.IsActive = true;

		CombineAssertions(() =>
		{
			AssertEquals("Description", "Start Date", startDateFilter.Description);
			startDateFilter.Property1 = ZDate.Today;
			startDateFilter.Property2 = ZDate.Today;
			startDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertMatch("Today", true, false, false);
			startDateFilter.Property2 = ZDate.Today.AddDays(1);
			startDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertMatch("Tomorrow", true, true, false);
			startDateFilter.Property1 = ZDate.Today.AddDays(-1);
			startDateFilter.Property2 = ZDate.Today;
			startDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertMatch("Yesterday", true, false, true);
		});
		void AssertMatch(ZString message, bool match1, bool match2, bool match3)
		{
			AssertEquals(message + "->rule1", match1, rule1.MatchesFilter(filter.Filter));
			AssertEquals(message + "->rule2", match2, rule2.MatchesFilter(filter.Filter));
			AssertEquals(message + "->rule3", match3, rule3.MatchesFilter(filter.Filter));
		}
	}

	public void TestAuthorisationEndDateFilter()
	{
		var header1 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		header1.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		var rule1 = header1.CusAuthorisationRules.AddNew();
		rule1.CPR_RuleCode = "LOC";
		header1.CPH_Type = "ACE";
		header1.CPH_StartDate = ZDate.Today.AddMonths(-1);
		header1.CPH_EndDate = ZDate.Today;
		rule1.CPR_ValueFrom = "WAW";
		var header2 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		header2.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		var rule2 = header2.CusAuthorisationRules.AddNew();
		rule2.CPR_RuleCode = "LOC";
		header2.CPH_Type = "ACE";
		header2.CPH_StartDate = ZDate.Today.AddMonths(-1);
		header2.CPH_EndDate = ZDate.Today.AddDays(1);
		rule2.CPR_ValueFrom = "WAW";
		var header3 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		header3.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		var rule3 = header3.CusAuthorisationRules.AddNew();
		rule3.CPR_RuleCode = "LOC";
		header3.CPH_Type = "ACE";
		header3.CPH_StartDate = ZDate.Today.AddMonths(-1);
		header3.CPH_EndDate = ZDate.Today.AddDays(-1);
		rule3.CPR_ValueFrom = "WAW";
		Factory.Save();
		var authorisationCollection = new CusAuthorisationHeaderCollection(Factory);
		var endDateFilter = (ModuleDateFilter)filter["End Date"];
		endDateFilter.IsActive = true;

		CombineAssertions(() =>
		{
			AssertEquals("Description", "End Date", endDateFilter.Description);
			endDateFilter.Property1 = ZDate.Today;
			endDateFilter.Property2 = ZDate.Today;
			endDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertMatch("Today", true, false, false);
			endDateFilter.Property2 = ZDate.Today.AddDays(1);
			endDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertMatch("Tomorrow", true, true, false);
			endDateFilter.Property1 = ZDate.Today.AddDays(-1);
			endDateFilter.Property2 = ZDate.Today;
			endDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertMatch("Yesterday", true, false, true);
		});
		void AssertMatch(ZString message, bool match1, bool match2, bool match3)
		{
			AssertEquals(message + "->rule1", match1, rule1.MatchesFilter(filter.Filter));
			AssertEquals(message + "->rule2", match2, rule2.MatchesFilter(filter.Filter));
			AssertEquals(message + "->rule3", match3, rule3.MatchesFilter(filter.Filter));
		}
	}

	public void TestAuthorisationTypeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
		helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, null, grouping);
		helper.CreateCusCodeType("AUTH", "Authorisation");
		helper.CreateCusCodeList("EUN", "AUTH", "ACE", "Self-Assessment", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		Factory.Save();

		var typeFilter = (ModuleTextFilter)filter["Authorization Type"];
		typeFilter.IsActive = true;
		typeFilter.Property = "AUL";
		AssertHasNotifications("authorized value", typeFilter.PropertyInfo);
		typeFilter.Property = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
		AssertNoNotifications("authorized value", typeFilter.PropertyInfo);
	}

	public void TestRuleCodeFilter()
	{
		var rule1 = Factory.NewWithValidTestData<CusAuthorisationRule>();
		rule1.CPR_RuleCode = "LOC";
		rule1.CPR_ValueFrom = "WAW";
		var rule2 = Factory.NewWithValidTestData<CusAuthorisationRule>();
		rule2.CPR_RuleCode = "LOD";
		rule2.CPR_ValueFrom = "WAW";
		Factory.Save();
		var ruleTypeFilter = (ModuleTextFilter)filter["Rule Code"];
		ruleTypeFilter.IsActive = true;

		CombineAssertions(() =>
		{
			AssertEquals("Description", "Rule Code", ruleTypeFilter.Description);
			ruleTypeFilter.Property = "LOC";
			AssertMatch("LOC", true, false);
			ruleTypeFilter.Property = "LOD";
			AssertMatch("LOD", false, true);
		});
		void AssertMatch(ZString message, bool match1, bool match2)
		{
			AssertEquals(message + "->rule1", match1, rule1.MatchesFilter(filter.Filter));
			AssertEquals(message + "->rule2", match2, rule2.MatchesFilter(filter.Filter));
		}
	}

	public void TestValueFilter()
	{
		var rule1 = Factory.NewWithValidTestData<CusAuthorisationRule>();
		rule1.CPR_RuleCode = "LOC";
		rule1.CPR_ValueFrom = "WAW";
		var rule2 = Factory.NewWithValidTestData<CusAuthorisationRule>();
		rule2.CPR_RuleCode = "LOC";
		rule2.CPR_ValueFrom = "WAQ";
		var rule3 = Factory.NewWithValidTestData<CusAuthorisationRule>();
		rule3.CPR_RuleCode = "LOC";
		rule3.CPR_ValueFrom = "WDA";
		Factory.Save();
		var ruleCodeFilter = (ModuleTextFilter)filter["Value"];
		ruleCodeFilter.IsActive = true;

		CombineAssertions(() =>
		{
			AssertEquals("Description", "Value", ruleCodeFilter.Description);
			ruleCodeFilter.Property = "WAW";
			ruleCodeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			AssertMatch("Exact", true, false, false);
			ruleCodeFilter.Property = "WA";
			ruleCodeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			AssertMatch("StartsWith", true, true, false);
			ruleCodeFilter.Property = "A";
			ruleCodeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			AssertMatch("Contains", true, true, true);
		});
		void AssertMatch(ZString message, bool match1, bool match2, bool match3)
		{
			AssertEquals(message + "->rule1", match1, rule1.MatchesFilter(filter.Filter));
			AssertEquals(message + "->rule2", match2, rule2.MatchesFilter(filter.Filter));
			AssertEquals(message + "->rule3", match3, rule3.MatchesFilter(filter.Filter));
		}
	}

	public void TestDescriptionFilter()
	{
		var rule1 = Factory.NewWithValidTestData<CusAuthorisationRule>();
		rule1.CPR_RuleCode = "LOC";
		rule1.CPR_ValueFrom = "WAW";
		rule1.CPR_Description = "TEST";
		var rule2 = Factory.NewWithValidTestData<CusAuthorisationRule>();
		rule2.CPR_RuleCode = "LOC";
		rule2.CPR_ValueFrom = "WAW";
		rule2.CPR_Description = "TESQ";
		var rule3 = Factory.NewWithValidTestData<CusAuthorisationRule>();
		rule3.CPR_RuleCode = "LOC";
		rule3.CPR_ValueFrom = "WAW";
		rule3.CPR_Description = "ES";
		Factory.Save();
		var descriptionFilter = (ModuleTextFilter)filter["Description"];
		descriptionFilter.IsActive = true;

		CombineAssertions(() =>
		{
			AssertEquals("Description", "Description", descriptionFilter.Description);
			descriptionFilter.Property = "TEST";
			descriptionFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			AssertMatch("Exact", true, false, false);
			descriptionFilter.Property = "TES";
			descriptionFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			AssertMatch("StartsWith", true, true, false);
			descriptionFilter.Property = "ES";
			descriptionFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			AssertMatch("Contains", true, true, true);
		});
		void AssertMatch(ZString message, bool match1, bool match2, bool match3)
		{
			AssertEquals(message + "->rule1", match1, rule1.MatchesFilter(filter.Filter));
			AssertEquals(message + "->rule2", match2, rule2.MatchesFilter(filter.Filter));
			AssertEquals(message + "->rule3", match3, rule3.MatchesFilter(filter.Filter));
		}
	}

	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new AuthorisationRuleFilterBusinessObject();

	protected override void SetUp()
	{
		base.SetUp();
		filter = new AuthorisationRuleFilterBusinessObject();
	}
	AuthorisationRuleFilterBusinessObject filter;
}
