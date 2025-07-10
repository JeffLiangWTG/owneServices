using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using FilterConstants = Enterprise.Customs.Business.CusAuthorisationHeaderCollection.FilterConstants;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CusAuthorisationsFilterStripBusinessObject))]
	sealed class CusAuthorisationsFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		[TestDate(2022, 8, 8)]
		public void TestEndDateFilter()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var validAuthorisation1 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			validAuthorisation1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			validAuthorisation1.CPH_Number = "AUTH1";
			validAuthorisation1.CPH_OH_PermitHolder = orgHeader.PK;
			validAuthorisation1.CPH_SubType = "1";

			var validAuthorisation2 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			validAuthorisation2.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			validAuthorisation2.CPH_Number = "AUTH2";
			validAuthorisation2.CPH_OH_PermitHolder = orgHeader.PK;
			validAuthorisation2.CPH_SubType = "1";
			validAuthorisation2.CPH_EndDate = ZDate.Today.AddMonths(1);

			var expiredAuthorisation3 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			expiredAuthorisation3.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			expiredAuthorisation3.CPH_Number = "AUTH3";
			expiredAuthorisation3.CPH_OH_PermitHolder = orgHeader.PK;
			expiredAuthorisation3.CPH_SubType = "1";
			expiredAuthorisation3.CPH_EndDate = ZDate.Today.AddMonths(-1);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var authorisationCollection = new CusAuthorisationHeaderCollection(Factory);
				AssertEquals("All 3 french authorisations should match because the only filter available is implicitly set on the country.", 3, authorisationCollection.Count);

				//Search valid authorisations
				var filter = new CusAuthorisationsFilterStripBusinessObject();
				var endDateFilter = (ModuleDateFilter)filter[CusAuthorisationHeaderCollection.FilterConstants.EndDate];
				endDateFilter.IsActive = true;
				endDateFilter.Property1 = ZDate.Today;
				endDateFilter.Property2 = ZDate.Empty;
				endDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				authorisationCollection = new CusAuthorisationHeaderCollection(Factory);
				authorisationCollection.AdditionalFilter = filter.Filter;
				AssertEquals("Only valid french authorisations should match because there is now a filter on End Date.", 2, authorisationCollection.Count);
				AssertContainsExactElementsInAnyOrder("validAuthorisation1 is considered as valid because its End Date has been left empty.", new CusAuthorisationHeader[] { validAuthorisation1, validAuthorisation2 }, authorisationCollection);

				//Search authorisations within date range
				endDateFilter = (ModuleDateFilter)filter[CusAuthorisationHeaderCollection.FilterConstants.EndDate];
				endDateFilter.IsActive = true;
				endDateFilter.Property1 = ZDate.Today;
				endDateFilter.Property2 = ZDate.Today.AddMonths(1);
				endDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				authorisationCollection = new CusAuthorisationHeaderCollection(Factory);
				authorisationCollection.AdditionalFilter = filter.Filter;
				AssertContainsExactElementsInAnyOrder("validAuthorisation2 is the only authorisation whose end date is exactly between today and newt month.", new CusAuthorisationHeader[] { validAuthorisation2 }, authorisationCollection);
			}
		}
		public void TestEnableAdHocFilters()
		{
			var adHocFilter = (ModuleFlagsFilter)filter[FilterConstants.AdHoc];
			AssertNull("AdHoc Filter should not exist", adHocFilter);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				base.SetUp();
				var frFilter = new CusAuthorisationsFilterStripBusinessObject();
				adHocFilter = (ModuleFlagsFilter)frFilter[FilterConstants.AdHoc];
				AssertNotNull("AdHoc Filter should exist", adHocFilter);
				var header1 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
				header1.CPH_IsAdHoc = true;
				var header2 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
				header2.CPH_IsAdHoc = false;
				adHocFilter.IsActive = true;
				adHocFilter.Property0 = true;
				CombineAssertions(() =>
				{
					AssertEquals("Description", FilterConstants.AdHoc, adHocFilter.Description);
					AssertEquals("Category", FilterCategories.StatusAndFlags, adHocFilter.Category);
					AssertEquals("header1 should match", true, header1.MatchesFilter(frFilter.Filter));
					AssertEquals("header2 should not match", false, header2.MatchesFilter(frFilter.Filter));
				});
			}
		}

		public void TestFilters()
		{
			var holder = Factory.NewWithValidTestData<OrgHeader>();
			var holderFilter = (ModuleGuidFilter)filter[FilterConstants.AuthorisationHolder];
			holderFilter.IsActive = true;
			holderFilter.Property = holder.PK;
			var startDateFilter = (ModuleDateFilter)filter[FilterConstants.StartDate];
			startDateFilter.IsActive = true;
			startDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			startDateFilter.Property1 = ZDateTime.Today.AddDays(-2);
			startDateFilter.Property2 = ZDateTime.Today.AddDays(1);
			var endDateFilter = (ModuleDateFilter)filter[FilterConstants.EndDate];
			endDateFilter.IsActive = true;
			endDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			endDateFilter.Property1 = ZDateTime.Today.AddDays(-1);
			endDateFilter.Property2 = ZDateTime.Today.AddDays(1);
			var numberFilter = (ModuleTextFilter)filter[FilterConstants.AuthorisationNumber];
			numberFilter.IsActive = true;
			numberFilter.Property = "NUM1";
			var typeFilter = (ModuleTextFilter)filter[FilterConstants.AuthorisationType];
			typeFilter.IsActive = true;
			typeFilter.Property = CusAuthorizationHeaderTypeList.Codes.AuthorizedWeighersOfBananas;
			var cusAuthorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			cusAuthorisationHeader.CPH_ApplicationCode = ZString.Empty;
			Assert("Should not match when value not set.", !cusAuthorisationHeader.MatchesFilter(filter.Filter));
			cusAuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			Assert("Should not match when value not set.", !cusAuthorisationHeader.MatchesFilter(filter.Filter));
			cusAuthorisationHeader.CPH_OH_PermitHolder = holder.PK;
			Assert("Should not match when value not set.", !cusAuthorisationHeader.MatchesFilter(filter.Filter));
			cusAuthorisationHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
			Assert("Should not match when value not set.", !cusAuthorisationHeader.MatchesFilter(filter.Filter));
			cusAuthorisationHeader.CPH_EndDate = ZDate.Today;
			Assert("Should not match when value not set.", !cusAuthorisationHeader.MatchesFilter(filter.Filter));
			cusAuthorisationHeader.CPH_Number = "NUM1";
			Assert("Should not match when value not set.", !cusAuthorisationHeader.MatchesFilter(filter.Filter));
			cusAuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedWeighersOfBananas;
			Assert("Should match when all values set.", cusAuthorisationHeader.MatchesFilter(filter.Filter));
		}

		public void TestLookups()
		{
			CombineAssertions(() =>
			{
				var lookups = filter.Lookups;
				AssertType<CusAuthorisationsFilterLookups>("Type", lookups);
				AssertSame("Cached", lookups, filter.Lookups);
			});
		}

		public void TestCurrentStatusFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			header1.CPH_StartDate = ZDate.Today;
			header1.CPH_EndDate = ZDate.Empty;
			var header2 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			header2.CPH_StartDate = ZDate.Today.AddDays(-2);
			header2.CPH_EndDate = ZDate.Today.AddDays(-1);
			var header3 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			header3.CPH_StartDate = ZDate.Today;
			header3.CPH_EndDate = ZDate.Today.AddDays(1);
			var header4 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			header4.CPH_StartDate = ZDate.Today.AddDays(1);
			header4.CPH_EndDate = ZDate.Empty;
			var header5 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			header5.CPH_StartDate = ZDate.Today.AddDays(1);
			header5.CPH_EndDate = ZDate.Today.AddDays(2);
			Factory.Save();
			var currentStatusFilter = (ModuleTextFilter)filter[FilterConstants.IsCurrent];
			currentStatusFilter.IsActive = true;
			CombineAssertions(() =>
			{
				AssertEquals("Description", FilterConstants.IsCurrent, currentStatusFilter.Description);
				AssertEquals("Category", FilterCategories.StatusAndFlags, currentStatusFilter.Category);
				AssertEquals("List", "All, Yes, No", ((CodeDescriptionPairList)currentStatusFilter.List).CodesAsString);
				AssertEquals("Default value", "Yes", currentStatusFilter.DefaultProperty);
				currentStatusFilter.Property = ZString.Empty;
				AssertMatch("Empty filter", true, true, true, true, true);
				currentStatusFilter.Property = CurrentStatusList.Codes.ShowNonCurrentOnly;
				AssertMatch("Filter 'No'", false, true, false, true, true);
				currentStatusFilter.Property = CurrentStatusList.Codes.ShowCurrentOnly;
				AssertMatch("Filter 'Yes'", true, false, true, false, false);
				currentStatusFilter.Property = CurrentStatusList.Codes.ShowAllRecords;
				AssertMatch("Filter 'All'", true, true, true, true, true);
			});
			void AssertMatch(ZString message, bool match1, bool match2, bool match3, bool match4, bool match5)
			{
				AssertEquals(message + "->header1", match1, header1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->header2", match2, header2.MatchesFilter(filter.Filter));
				AssertEquals(message + "->header3", match3, header3.MatchesFilter(filter.Filter));
				AssertEquals(message + "->header4", match4, header4.MatchesFilter(filter.Filter));
				AssertEquals(message + "->header5", match5, header5.MatchesFilter(filter.Filter));
			}
		}

		public void TestAuthorisationDescriptionFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			var header2 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			header2.CPH_PermitDescription = "IPO Authorisation DE9281192";
			var header3 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			header3.CPH_PermitDescription = "CWP Authorisation DE3829101";
			var descriptionFilter = (ModuleTextFilter)filter[FilterConstants.AuthorisationDescription];
			descriptionFilter.IsActive = true;
			CombineAssertions(() =>
			{
				AssertEquals("Description", FilterConstants.AuthorisationDescription, descriptionFilter.Description);
				AssertEquals("Category", FilterCategories.TextSearch, descriptionFilter.Category);
				AssertEquals("MaxLength", CusPermitHeaderSchema.CPH_PermitDescription.MaxLength, descriptionFilter.MaxLength);
				descriptionFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
				AssertMatch("IsBlank", true, false, false);
				descriptionFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
				AssertMatch("IsNotBlank", false, true, true);
				descriptionFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				descriptionFilter.Property = "CWP Authorisation DE3829101";
				AssertMatch("Exact", false, false, true);
				descriptionFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				descriptionFilter.Property = "IP";
				AssertMatch("StartsWith", false, true, false);
				descriptionFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				descriptionFilter.Property = "3829";
				AssertMatch("Contains", false, false, true);
				descriptionFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				descriptionFilter.Property = "CWP Authorisation DE3829101";
				AssertMatch("NotEqual", true, true, false);
				descriptionFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				descriptionFilter.Property = "IP";
				AssertMatch("NotStartsWith", true, false, true);
				descriptionFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				descriptionFilter.Property = "3829";
				AssertMatch("NotContain", true, true, false);
			});
			void AssertMatch(ZString message, bool match1, bool match2, bool match3)
			{
				AssertEquals(message + "->header1", match1, header1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->header2", match2, header2.MatchesFilter(filter.Filter));
				AssertEquals(message + "->header3", match3, header3.MatchesFilter(filter.Filter));
			}
		}

		public void TestAuthorisationAddressFilter_Properties()
		{
			var addressFilter = (ModuleTextFilter)filter[FilterConstants.AuthorisationAddress];
			addressFilter.IsActive = true;
			CombineAssertions(() =>
			{
				AssertEquals("Description", FilterConstants.AuthorisationAddress, addressFilter.Description);
				AssertEquals("Category", FilterCategories.TextSearch, addressFilter.Category);
				AssertEquals("MaxLength", AutoOrgAddress.Schema.OA_Address1MaxLength, addressFilter.MaxLength);
				var comparisonOperatorList = addressFilter.ComparisonOperator_List.GetAllCodes();
				AssertCollectionNotContains("No IsBlank", ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank, comparisonOperatorList);
				AssertCollectionNotContains("No IsNotBlank", ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank, comparisonOperatorList);
			});
		}

		public void TestAuthorisationAddressFilter_Validation()
		{
			const string errorMessage = "Only one Authorization Address filter is allowed.";
			var addressFilter = (ModuleTextFilter)filter[FilterConstants.AuthorisationAddress];
			addressFilter.IsActive = true;
			addressFilter.Property = "Address1";
			CombineAssertions(() =>
			{
				AssertNoError("One Authorization Address filter", addressFilter.PropertyInfo, errorMessage);
				var addressFilterStrip2 = filter.FilterStrips.AddNew();
				addressFilterStrip2.FilterDescription = FilterConstants.AuthorisationAddress;
				var addressFilter2 = (ModuleTextFilter)addressFilterStrip2.CurrentModuleFilter;
				addressFilter2.IsActive = true;
				addressFilter2.Property = "Address2";
				AssertHasError("Multiple Authorization Address filters", addressFilter2.PropertyInfo, errorMessage);
			});
		}

		public void TestAuthorisationAddressFilter_Results()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "XYZ";
			var orgAddress1 = orgHeader.Addresses.AddNew();
			orgAddress1.Address1 = "166 Main Street";
			var header1 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			header1.CPH_OA_AppliesTo = orgAddress1.PK;
			var orgAddress2 = orgHeader.Addresses.AddNew();
			orgAddress2.Address1 = "243 Second Street";
			var header2 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			header2.CPH_OA_AppliesTo = orgAddress2.PK;
			var header3 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			Factory.Save();
			var addressFilter = (ModuleTextFilter)filter[FilterConstants.AuthorisationAddress];
			addressFilter.IsActive = true;
			CombineAssertions(() =>
			{
				addressFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				addressFilter.Property = "166 Main Street";
				AssertMatch("Exact", true, false, false);
				addressFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				addressFilter.Property = "2";
				AssertMatch("StartsWith", false, true, false);
				addressFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				addressFilter.Property = "66";
				AssertMatch("Contains", true, false, false);
				addressFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				addressFilter.Property = "166 Main Street";
				AssertMatch("NotEqual", false, true, false);
				addressFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				addressFilter.Property = "2";
				AssertMatch("NotStartsWith", true, false, false);
				addressFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				addressFilter.Property = "66";
				AssertMatch("NotContain", false, true, false);
			});
			void AssertMatch(ZString message, bool match1, bool match2, bool match3)
			{
				AssertEquals(message + "->header1", match1, header1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->header2", match2, header2.MatchesFilter(filter.Filter));
				AssertEquals(message + "->header3", match3, header3.MatchesFilter(filter.Filter));
			}
		}

		public void TestFranceAuthorisationTypeList()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var typeFilter = (ModuleTextFilter)filter[FilterConstants.AuthorisationType];
				typeFilter.IsActive = true;
				typeFilter.Property = "AUL";
				AssertNoNotifications("authorized value", typeFilter.PropertyInfo);
				typeFilter.Property = CusAuthorizationHeaderTypeList.Codes.AuthorizedWeighersOfBananas;
				AssertNoNotifications("authorized value", typeFilter.PropertyInfo);
				typeFilter.Property = "DOC";
				AssertHasNotifications("non authorized value", typeFilter.PropertyInfo);
			}
		}

		public void TestAustraliaAuthorisationTypeList()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var typeFilter = (ModuleTextFilter)filter[FilterConstants.AuthorisationType];
				typeFilter.IsActive = true;
				typeFilter.Property = "AUL";
				AssertHasNotifications("authorized value", typeFilter.PropertyInfo);
				typeFilter.Property = CusAuthorizationHeaderTypeList.Codes.AuthorizedWeighersOfBananas;
				AssertNoNotifications("authorized value", typeFilter.PropertyInfo);
			}
		}

		public void TestRuleDetailsFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			AddRule(header1, CusAuthorisationRuleTypeList.Codes.Location, "VAL1.1");
			AddRule(header1, "USE", "VAL3.1");
			AddRule(header1, "BRE", "VAL2");
			var header2 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			AddRule(header2, CusAuthorisationRuleTypeList.Codes.Location, "VAL2");
			AddRule(header2, "BRE", "VAL3");
			AddRule(header2, "USE", "VAL1");
			var header3 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			AddRule(header3, CusAuthorisationRuleTypeList.Codes.Location, "VAL3.1");
			AddRule(header3, "BRE", "RULE3");
			AddRule(header3, "USE", "VAL3.2");
			Factory.Save();
			var ruleDetailsFilter = (CusAuthorisationsRuleModuleFilter)filter[FilterConstants.RuleDetails];
			ruleDetailsFilter.IsActive = true;
			CombineAssertions(() =>
			{
				AssertEquals("Description", FilterConstants.RuleDetails, ruleDetailsFilter.Description);
				ruleDetailsFilter.Property1 = CusAuthorisationRuleTypeList.Codes.Location;
				ruleDetailsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				ruleDetailsFilter.Property2 = "VAL1.1";
				AssertMatch("LOC, exactly 'VAL1'", true, false, false);
				ruleDetailsFilter.Property1 = "BRE";
				ruleDetailsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				ruleDetailsFilter.Property2 = "VAL";
				AssertMatch("BRE, starts with 'VAL'", true, true, false);
				ruleDetailsFilter.Property1 = "USE";
				ruleDetailsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				ruleDetailsFilter.Property2 = "3.";
				AssertMatch("USE, contains '3.'", true, false, true);
				ruleDetailsFilter.Property1 = "USE";
				ruleDetailsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				ruleDetailsFilter.Property2 = "VAL3.1";
				AssertMatch("USE, not equal 'VAL3.1'", false, true, true);
				ruleDetailsFilter.Property1 = "BRE";
				ruleDetailsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				ruleDetailsFilter.Property2 = "VAL";
				AssertMatch("BRE, not starts with 'VAL'", false, false, true);
				ruleDetailsFilter.Property1 = CusAuthorisationRuleTypeList.Codes.Location;
				ruleDetailsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				ruleDetailsFilter.Property2 = ".1";
				AssertMatch("LOC, not contain '.1'", false, true, false);
			});
			void AssertMatch(ZString message, bool match1, bool match2, bool match3)
			{
				AssertEquals(message + "->header1", match1, header1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->header2", match2, header2.MatchesFilter(filter.Filter));
				AssertEquals(message + "->header3", match3, header3.MatchesFilter(filter.Filter));
			}
		}

		public void TestRuleDetailsFilter_RuleCodeOnly()
		{
			var header1 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			AddRule(header1, CusAuthorisationRuleTypeList.Codes.Location, "val");
			AddRule(header1, "USE", "val");
			AddRule(header1, "BRE", "val");
			var header2 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			AddRule(header2, CusAuthorisationRuleTypeList.Codes.Location, "val");
			AddRule(header2, "USE", "val");
			var header3 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			AddRule(header3, CusAuthorisationRuleTypeList.Codes.Location, "val");
			Factory.Save();
			var ruleDetailsFilter = (CusAuthorisationsRuleModuleFilter)filter[FilterConstants.RuleDetails];
			ruleDetailsFilter.IsActive = true;
			CombineAssertions(() =>
			{
				ruleDetailsFilter.Property1 = CusAuthorisationRuleTypeList.Codes.Location;
				AssertMatch("LOC", true, true, true);
				ruleDetailsFilter.Property1 = "USE";
				AssertMatch("USE", true, true, false);
				ruleDetailsFilter.Property1 = "BRE";
				AssertMatch("BRE", true, false, false);
			});
			void AssertMatch(ZString message, bool match1, bool match2, bool match3)
			{
				AssertEquals(message + "->header1", match1, header1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->header2", match2, header2.MatchesFilter(filter.Filter));
				AssertEquals(message + "->header3", match3, header3.MatchesFilter(filter.Filter));
			}
		}

		public void TestRuleDetailsFilter_RuleValueOnly()
		{
			var header1 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			AddRule(header1, CusAuthorisationRuleTypeList.Codes.Location, "RULE1.1");
			AddRule(header1, CusAuthorisationRuleTypeList.Codes.Location, "RULE1.2");
			AddRule(header1, CusAuthorisationRuleTypeList.Codes.Location, "RULE1.3");
			var header2 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			AddRule(header2, CusAuthorisationRuleTypeList.Codes.Location, "RULE2.1");
			AddRule(header2, CusAuthorisationRuleTypeList.Codes.Location, "RULE2.2");
			var header3 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			AddRule(header3, CusAuthorisationRuleTypeList.Codes.Location, "RULE3.1");
			Factory.Save();
			var ruleDetailsFilter = (CusAuthorisationsRuleModuleFilter)filter[FilterConstants.RuleDetails];
			ruleDetailsFilter.IsActive = true;
			CombineAssertions(() =>
			{
				ruleDetailsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				ruleDetailsFilter.Property2 = "RULE2.2";
				AssertMatch("Exact", false, true, false);
				ruleDetailsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				ruleDetailsFilter.Property2 = "RULE";
				AssertMatch("Starts with", true, true, true);
				ruleDetailsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				ruleDetailsFilter.Property2 = ".2";
				AssertMatch("Contains", true, true, false);
				ruleDetailsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				ruleDetailsFilter.Property2 = "RULE1.2";
				AssertMatch("Not equal", true, true, true);
				ruleDetailsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				ruleDetailsFilter.Property2 = "RULE2";
				AssertMatch("Not starts with", true, false, true);
				ruleDetailsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				ruleDetailsFilter.Property2 = ".1";
				AssertMatch("Not contain", true, true, false);
			});
			void AssertMatch(ZString message, bool match1, bool match2, bool match3)
			{
				AssertEquals(message + "->header1", match1, header1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->header2", match2, header2.MatchesFilter(filter.Filter));
				AssertEquals(message + "->header3", match3, header3.MatchesFilter(filter.Filter));
			}
		}

		public void TestCountryFilter()
		{
			var countryFilter = (ModuleNkFilter)filter[FilterConstants.Country];
			countryFilter.IsActive = true;
			CombineAssertions(() =>
			{
				AssertEquals("Description", "Country/Region", countryFilter.Description);
				AssertEquals("Visibility", FilterVisibility.AlwaysVisible, countryFilter.Visibility);
				AssertEquals("ReadOnly", true, countryFilter.ReadOnly);
				AssertEquals("Default value", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, countryFilter.DefaultProperty);
				AssertEquals("CusAuthorisationHeaderCollection is already filtering by current country, no extra filter is needed", string.Empty, filter.Filter.FilterString);
			});
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			var exclusions = base.GetFiltersExcludedFromSubgroupCheck();
			exclusions.Add(TableFilter(OrgAddressSchema.Constants.TableName, FilterConstants.AuthorisationAddress));
			exclusions.Add(TableFilter(CusPermitRuleSchema.Constants.TableName, FilterConstants.RuleDetails));
			return exclusions;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CusAuthorisationsFilterStripBusinessObject();
		protected override void SetUp()
		{
			base.SetUp();
			filter = new CusAuthorisationsFilterStripBusinessObject();
		}

		CusAuthorisationsFilterStripBusinessObject filter;
		void AddRule(CusAuthorisationHeader header, ZString ruleCode, ZString ruleValue)
		{
			var rule = header.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = ruleCode;
			rule.CPR_ValueFrom = ruleValue;
		}
	}
}
