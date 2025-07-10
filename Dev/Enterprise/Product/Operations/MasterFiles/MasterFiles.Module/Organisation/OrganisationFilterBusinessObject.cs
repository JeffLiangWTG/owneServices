using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Environment;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;
using static Enterprise.Freight.Integration.Forwarding;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Module
{
	public class OrganisationFilterBusinessObject : FilterStripBusinessObject
	{
		#region Construction

		public OrganisationFilterBusinessObject()
			: this(OrgModuleType.Standard)
		{
			this.QueryObjectType = typeof(OrgHeader);
		}

		public OrganisationFilterBusinessObject(OrgModuleType moduleType)
		{
			fModuleType = moduleType;
			SetOrgTypePropertiesBasedOnModuleType();
		}

		protected OrgModuleType fModuleType = OrgModuleType.Standard;

		#endregion

		#region Description Constants

		public static class Descriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string NameFilter = "Name";

			#endregion
		}

		#endregion

		#region Default Values + Loading

		void SetOrgTypePropertiesBasedOnModuleType()
		{
			using (SuspendSettingHasChanges())
			{
				SetOrgTypePropertiesBasedOnModuleTypeCore();
			}
		}

		protected virtual bool ShouldAddWorkflowCustomFieldsFilters
		{
			get { return true; }
		}

		public override void SetAdditionalFilterDefaults(ZString code, IBusinessObjectCollection orgCollection)
		{
			bool shouldSetupFilterFromNotes = code == OrgHeader.UnmatchedOrganisationCode;

			IFilterBusinessObjectDefaultsProvider provider = orgCollection as IFilterBusinessObjectDefaultsProvider;
			if (provider != null)
			{
				IOrganisationDefaultProvider iCondOrgFieldDefaultProvider = orgCollection as IOrganisationDefaultProvider;

				if (iCondOrgFieldDefaultProvider != null)
				{
					if (shouldSetupFilterFromNotes)
					{
						OrganisationsFindBoxListHelper.SetFilterBusinessObjectFromUnmatchOrgRecord(orgCollection);
					}

					foreach (OrgFieldDefault current in iCondOrgFieldDefaultProvider.ConditionalDefaults)
					{
						ZString filterName;

						if (FilterObjectPropertyDictionary.TryGetValue(current.FieldName, out filterName))
						{
							if (!shouldSetupFilterFromNotes)
							{
								var filterPropertyName = filterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + Res.GetString("f58d6851-9e83-457a-a053-fa83eccffc10", "Property");
								if (provider.FilterBusinessObjectDefaults.ContainsDefaultFor(filterPropertyName))
								{
									provider.FilterBusinessObjectDefaults.Remove(filterPropertyName);
								}
							}
							else
							{
								FilterBusinessObjectDefault filter = new FilterBusinessObjectDefault(filterName, "Property", current.Value);
								provider.FilterBusinessObjectDefaults.Add(filter);
							}
						}
					}
				}
			}
		}

		public override void SetInitialCodeForSearch(ZString code, Type typeOfElementsToFind)
		{
			if (code != OrgHeader.UnmatchedOrganisationCode)
			{
				base.SetInitialCodeForSearch(code, typeOfElementsToFind);
			}
		}

		protected virtual void SetOrgTypePropertiesBasedOnModuleTypeCore()
		{
		}

		#endregion

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddOtherFilters(filters);
			AddNumberFilters(filters);
			AddTextFilters(filters);
			AddFlagsFilters(filters);
			AddDateFilters(filters);
			AddLocationFilters(filters);
			AddRelationshipGuidsFilters(filters);
			AddRelationshipFlagsFilters(filters);
			AddOrgTypeRelatedFilters(filters);
			AddTradeLaneRelatedFilters(filters);
			AddRegistrationNumberFilters(filters);
			SecurityProvider.AddCRMSecurityFilterStrips(Factory, filters);
			AddShippingLineFilters(filters);
			AddSalesRelationActivityFilters(filters);
			AddCreditScoresFilters(filters);

			if (!IsProductivityWiseModeEnabled)
			{
				AddValueAnalysisFilters(filters);
			}

			AddHiddenFilters(filters);

			return filters;
		}

		bool IsProductivityWiseModeEnabled { get; } = DataRegistry.Instance.ProductivityWiseModeEnabled;

		#endregion

		#region Value Analysis Filters

		void AddValueAnalysisFilters(ModuleFilterCollection filters)
		{
			ObjectFactory.Get<IValueAnalysisModuleHelper>()?.AddValueAnalysisModuleFilterStrips(OrgHeaderSchema.PK, filters, Factory, typeof(OrgHeader));
		}

		#endregion

		#region Organisation Extract Tool Filters

		void AddOtherFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddDateFilter("No Transaction", GetNoTransactionsQuery);
			filter.Category = FilterCategories.Other;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|NoTransaction", "No Transaction");

			filter = filters.AddDateFilter("No Transaction in Current Company", GetNoTransactionsInCurrentCompanyQuery);
			filter.Category = FilterCategories.Other;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|NoTransactionInCurrentCompany", "No Transaction in Current Company");

			OrgTransactionsModuleFilter orgTransactionsModuleFilter = new OrgTransactionsModuleFilter("Min Transactions in Date Range");
			orgTransactionsModuleFilter.Category = FilterCategories.Other;
			orgTransactionsModuleFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|MinTransactionsInDateRange", "Min Transactions in Date Range");
			filters.AddCustomFilter(orgTransactionsModuleFilter);

			string flagName = Res.GetString("MasterFiles|OrganisationFilter|WithOutstandingTransactionsInAnyCompany", "With Outstanding Transactions in any Company");
			ModuleFlagsFilter accountingTransactionsFilter = filters.AddFlagsFilter("Accounting Transactions - Creditor", new string[] { flagName }, new GetFlagsQuery[] { GetOutstandingAccountingTransactionsQueryCreditor });
			accountingTransactionsFilter.Category = FilterCategories.Other;
			accountingTransactionsFilter.DefaultProperties[flagName] = true;
			accountingTransactionsFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|AccountingTransactionsCreditor", "Accounting Transactions - Creditor");
			ModuleFlagsFilter accountingTransactionsFilterDebtor = filters.AddFlagsFilter("Accounting Transactions - Debtor", new string[] { flagName }, new GetFlagsQuery[] { GetOutstandingAccountingTransactionsQueryDebtor });
			accountingTransactionsFilterDebtor.Category = FilterCategories.Other;
			accountingTransactionsFilterDebtor.DefaultProperties[flagName] = true;
			accountingTransactionsFilterDebtor.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|AccountingTransactionsDebtor", "Accounting Transactions - Debtor");

			var globalCreditGroup = filters.AddGuidFilter("Global Credit Group", ModuleIDs.Organisation, GetGlobalCreditGroup, GlobalCreditGroups);
			globalCreditGroup.Category = FilterCategories.Other;
			globalCreditGroup.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|GlobalCreditGroup", "Global Credit Group");

			if (GlbCompany.CurrentCompany.IsEnabledForTaxFrameworkConfiguration(Factory))
			{
				var orgTaxConfigurationModuleFilter = new OrgTaxConfigurationModuleFilter("Tax Configuration");
				orgTaxConfigurationModuleFilter.Category = FilterCategories.Other;
				orgTaxConfigurationModuleFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|TaxConfiguration", "Tax Configuration");
				filters.AddCustomFilter(orgTaxConfigurationModuleFilter);
			}
		}

		#endregion

		#region Number Filters

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "it's an identifier")]
		protected const string AchievableBusinessDescription = "Achievable Business";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "it's an identifier")]
		protected const string NumberOfEmployeesDescription = "Related Staff";

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			var countSubGroup = new CountSubGroup();

			ModuleNumberRangeFilter employeeCountFilter = new ModuleNumberRangeFilter(NumberOfEmployeesDescription, GetEmployeeCountQuery);
			employeeCountFilter.PropertyType = ZCalcEditPropertyType.Int;
			employeeCountFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|RelatedStaff", "Employee Count");
			employeeCountFilter.SubGroup = countSubGroup;
			filters.AddCustomFilter(employeeCountFilter);

			ModuleNumberRangeFilter achievableBusinessFilter = new ModuleNumberRangeFilter(AchievableBusinessDescription, GetAchievableBusinessQuery);
			achievableBusinessFilter.PropertyType = ZCalcEditPropertyType.Int;
			achievableBusinessFilter.MultilingualDescription = ResString.GetMultilingualString("ZClientEDI|OrganisationFilter|AchievableBusiness", "Achievable Business");
			achievableBusinessFilter.SubGroup = countSubGroup;
			filters.AddCustomFilter(achievableBusinessFilter);

			if (!IsProductivityWiseModeEnabled)
			{
				ModuleNumberRangeFilter filterNumber = filters.AddNumberRangeFilter("Delivery Route Sequence", GetDeliveryRouteSequence);
				filterNumber.PropertyType = ZCalcEditPropertyType.Short;
				filterNumber.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|DeliveryRouteSequence", "Delivery Route Sequence");
				filterNumber.MinValue = 0;
				filterNumber.SubGroup = new AddressCountSubGroup();
			}

			ModuleTextFilter clientNumberFilter = filters.AddTextFilter("Client Number", GetClientNumberQuery);
			clientNumberFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ClientNumber", "Client Number");
			clientNumberFilter.SubGroup = new CompanyDataSubGroup(OrgCompanyDataSchema.OB_IsDebtor);
			clientNumberFilter.MaxLength = OrgCompanyDataSchema.OB_ARClientNumber.MaxLength;
			clientNumberFilter.Category = FilterCategories.NumbersAndReferences;

			var codeMappingForeignFilter = new OrgCodeMappingForeignModuleFilter("Code Mapping (Foreign Code)");
			codeMappingForeignFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|CodeMappingForeign", "Code Mapping (Foreign)");
			codeMappingForeignFilter.Category = FilterCategories.NumbersAndReferences;
			filters.AddCustomFilter(codeMappingForeignFilter);

			var codeMappingLocalFilter = new OrgCodeMappingLocalModuleFilter("Code Mapping (Local Code)");
			codeMappingLocalFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|CodeMappingLocal", "Code Mapping (Local)");
			codeMappingLocalFilter.Category = FilterCategories.NumbersAndReferences;
			filters.AddCustomFilter(codeMappingLocalFilter);

			var approvalNumberFilterNumber = filters.AddTextFilter(SupplyChainSecurityConfiguration.ApprovalNumberFilterText, GetApprovalNumberQuery);
			approvalNumberFilterNumber.MultilingualDescription = SupplyChainSecurityConfiguration.ApprovalNumberFilterDescription;
			approvalNumberFilterNumber.MaxLength = OrgCountryDataSchema.OV_EXApprovalNumber.MaxLength;
			approvalNumberFilterNumber.Category = FilterCategories.NumbersAndReferences;
		}

		protected class CountSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(OrgHeader));

				var subQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH);
				subQuery.AddToFilter(filter);

				result.AddSubQuery(subQuery, JoinCondition.And);

				return result;
			}
		}

		protected class AddressCountSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(OrgHeader));

				var subQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH);
				subQuery.AddToFilter(filter);

				result.AddSubQuery(subQuery, JoinCondition.And);

				return result;
			}
		}

		ZQuery GetDeliveryRouteSequence(INumericZType value1, INumericZType value2)
		{
			var query = new ZQuery();
			ModuleNumberRangeFilter.AddToFilters(query, OrgAddressSchema.OA_DeliveryRouteSequence, value1, value2);

			return query;
		}

		ZQuery GetClientNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();
			query.AddToFilter(OrgCompanyDataSchema.OB_ARClientNumber, comparisonOperator, value);
			query.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);

			return query;
		}

		ZQuery GetEmployeeCountQuery(INumericZType value1, INumericZType value2)
		{
			return GetQueryWithModuleNumberRangerFilter(OrgMiscServSchema.OM_CMNoOfEmployees, value1, value2);
		}

		ZQuery GetApprovalNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var countryOrLicenceEconomicGroupingCode = SupplyChainSecurityConfiguration.IsEnabled && !SupplyChainSecurityConfiguration.LicenceEconomicGroupingCode.IsEmpty
					? SupplyChainSecurityConfiguration.LicenceEconomicGroupingCode
					: GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			var countryDataQuery = new ZDBOnlySubQuery(typeof(OrgCountryData), OrgCountryDataSchema.OV_OH_OrgHeader);
			countryDataQuery.AddToFilter(OrgCountryDataSchema.OV_EXApprovalNumber, comparisonOperator, value);
			countryDataQuery.AddToFilter(OrgCountryDataSchema.OV_RN_NKClientCountryRelation, countryOrLicenceEconomicGroupingCode);

			query.AddSubQuery(countryDataQuery, JoinCondition.And);

			return query;
		}

		ZQuery GetAchievableBusinessQuery(INumericZType value1, INumericZType value2)
		{
			return GetQueryWithModuleNumberRangerFilter(OrgMiscServSchema.OM_CMAcheivableClientRevenue, value1, value2, ZCalcEditPropertyType.Int);
		}

		protected ZQuery GetQueryWithModuleNumberRangerFilter(SchemaNumericColumn filterColumn, INumericZType value1, INumericZType value2, ZCalcEditPropertyType? propertyType = null)
		{
			var query = new ZQuery();
			ModuleNumberRangeFilter.AddToFilters(query, filterColumn, value1, value2, propertyType);

			return query;
		}

		#endregion

		#region Text Filters

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var oA_OHSubGroup = new OA_OHSubGroup();
			var oC_OHSubGroup = new OC_OHSubGroup();
			var o7_O5SubGroup = new O7_O5SubGroup();
			var codeFilter = filters.AddTextFilter("Code", OrgHeaderSchema.OH_Code);
			codeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|Code", "Code");

			var orgNameFilter = filters.AddTextFilter(Descriptions.NameFilter, OrgHeaderSchema.OH_FullName);
			orgNameFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|Name", Descriptions.NameFilter);
			orgNameFilter.Prefix = "N";

			var additionalCompanyNameFilter = filters.AddTextFilter("Additional Company Names", OrgAddressSchema.OA_CompanyNameOverride);
			additionalCompanyNameFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|AdditionalCompanyNames", "Additional Company Names");
			additionalCompanyNameFilter.SubGroup = oA_OHSubGroup;

			var relatedCompanyNameFilter = filters.AddTextFilter("Related Company Name", OrgBrandOrRelatedNameSchema.P1_RelatedName);
			relatedCompanyNameFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|RelatedCompanyName", "Related Company Name");
			relatedCompanyNameFilter.SubGroup = new P1_OHSubGroup();

			var regNoFilter = filters.AddTextFilter("Registration Number", GetRegistrationNumberQuery);
			regNoFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|RegistrationNumber", "Registration Number");

			var addressLinesfilter = new OrgAddressWithActiveStatusModuleTextFilter("Address", GetAddressLinesQuery)
			{
				MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|AddressLines", "Address Lines"),
				SubGroup = oA_OHSubGroup,
				Category = FilterCategories.TextSearch,
			};
			filters.AddCustomFilter(addressLinesfilter);

			var address1Filter = new OrgAddressWithActiveStatusModuleTextFilter("Address 1", GetAddress1Query)
			{
				MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|Address1", "Address 1"),
				SubGroup = oA_OHSubGroup,
				Category = FilterCategories.TextSearch,
			};
			filters.AddCustomFilter(address1Filter);

			var address2Filter = new OrgAddressWithActiveStatusModuleTextFilter("Address 2", GetAddress2Query)
			{
				MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|Address2", "Address 2"),
				SubGroup = oA_OHSubGroup,
				Category = FilterCategories.TextSearch,
			};
			filters.AddCustomFilter(address2Filter);

			var additionalAddressInfoFilter = new OrgAddressWithActiveStatusModuleTextFilter("Additional Address Info", GetAdditionalAddressQuery)
			{
				MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|AdditionalAddress", "Additional Address Info"),
				SubGroup = oA_OHSubGroup,
				Category = FilterCategories.TextSearch,
			};
			filters.AddCustomFilter(additionalAddressInfoFilter);

			var cityFilter = new OrgAddressWithActiveStatusModuleTextFilter("City", GetCityQuery)
			{
				MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|City", "City"),
				SubGroup = oA_OHSubGroup,
				Category = FilterCategories.TextSearch,
			};
			filters.AddCustomFilter(cityFilter);

			var stateFilter = new OrgAddressWithActiveStatusModuleTextFilter("State", GetStateQuery)
			{
				MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|State", "State"),
				SubGroup = oA_OHSubGroup,
				Category = FilterCategories.TextSearch,
			};
			filters.AddCustomFilter(stateFilter);

			var postCodeFilter = new OrgAddressWithActiveStatusModuleTextFilter("Post Code", GetPostCodeQuery)
			{
				MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|PostCode", "Post Code"),
				SubGroup = oA_OHSubGroup,
				Category = FilterCategories.TextSearch,
			};
			filters.AddCustomFilter(postCodeFilter);

			var exclusiveGatewayServiceFilter = filters.AddNkFilter("Exclusive Gateway Service", GetExclusiveGatewayServiceQuery, ModuleIDs.ServiceLevel, new RefServiceLevelCollection(Factory));
			exclusiveGatewayServiceFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ExclusiveGatewayService", "Exclusive Gateway Service");
			exclusiveGatewayServiceFilter.SubGroup = o7_O5SubGroup;
			exclusiveGatewayServiceFilter.Category = FilterCategories.TextSearch;

			var phoneFilter = filters.AddTextFilter("Phone", OrgAddressSchema.OA_Phone);
			phoneFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|Phone", "Phone");
			phoneFilter.SubGroup = oA_OHSubGroup;

			var mobileFilter = filters.AddTextFilter("Mobile", OrgAddressSchema.OA_Mobile);
			mobileFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|Mobile", "Mobile");
			mobileFilter.SubGroup = oA_OHSubGroup;

			var faxFilter = filters.AddTextFilter("Fax", OrgAddressSchema.OA_Fax);
			faxFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|Fax", "Fax");
			faxFilter.SubGroup = oA_OHSubGroup;

			var emailFilter = filters.AddTextFilter("Email", OrgAddressSchema.OA_Email);
			emailFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|Email", "Email");
			emailFilter.SubGroup = oA_OHSubGroup;

			var webFilter = filters.AddTextFilter("Web", OrgWebURLSchema.PU_URL);
			webFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|Web", "Web");
			webFilter.SubGroup = new PU_OHSubGroup();

			var contactNameFilter = new OrgContactsActiveStatusAndInfoModuleFilter("Contact Name", GetContactNameQuery)
			{
				MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ContactName", "Contact Name"),
				SubGroup = oC_OHSubGroup,
				Category = FilterCategories.TextSearch,
				DefaultActiveStatus = OrgContactsActiveStatusAndInfoModuleFilter.StatusActive
			};
			filters.AddCustomFilter(contactNameFilter);

			var contactWorkPhoneFilter = filters.AddTextFilter("Contact Work Phone", GetContactPhoneQuery);
			contactWorkPhoneFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ContactWorkPhone", "Contact Work Phone");

			var contactMobilePhoneFilter = filters.AddTextFilter("Contact Mobile", GetContactMobileQuery);
			contactMobilePhoneFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ContactMobile", "Contact Mobile");

			var contactFaxFilter = filters.AddTextFilter("Contact Fax", GetContactFaxQuery);
			contactFaxFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ContactFax", "Contact Fax");

			var contactEmailFilter = filters.AddTextFilter("Contact Email", GetContactEmailQuery);
			contactEmailFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ContactEmail", "Contact Email");

			var branchFilter = filters.AddTextFilter("Branch", GetBranchQuery);
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|Branch", "Branch");

			var externalDebtorCodeFilter = filters.AddTextFilter("External Debtor Code", (op, val) => GetExternalCodeQuery(op, val, OrgCompanyDataSchema.OB_ARExternalDebtorCode, OrgCusCode.CodeTypes.ExternalDebtorAccountCode));
			externalDebtorCodeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ExternalDebtorCode", "External Debtor Code");
			externalDebtorCodeFilter.MaxLength = OrgCusCodeSchema.OK_CustomsRegNo.MaxLength;

			var externalCreditorCodeFilter = filters.AddTextFilter("External Creditor Code", (op, val) => GetExternalCodeQuery(op, val, OrgCompanyDataSchema.OB_APExternalCreditorCode, OrgCusCode.CodeTypes.ExternalCreditorAccountCode));
			externalCreditorCodeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ExternalCreditorCode", "External Creditor Code");
			externalCreditorCodeFilter.MaxLength = OrgCusCodeSchema.OK_CustomsRegNo.MaxLength;

			if (!IsProductivityWiseModeEnabled)
			{
				var deliveryRouteFilter = filters.AddTextFilter("Delivery Route", OrgAddressSchema.OA_DeliveryRoute, Env.Registry.DeliveryRoutesList);
				deliveryRouteFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|DeliveryRoute", "Delivery Route");
				deliveryRouteFilter.SubGroup = oA_OHSubGroup;
			}

			var branchManagementCodeFilter = filters.AddTextFilter("Branch Management Code", GetBranchManagementCodeQuery, BranchManagementCodeList);
			branchManagementCodeFilter.MultilingualDescription = ResString.GetMultilingualString("befead81-3e88-4225-8493-62dcf24559c1", "Branch Management Code");
			branchManagementCodeFilter.SubGroup = new BranchManagementCodeSubGroup();

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Canada)
			{
				var accountSecurityNumberFilter = filters.AddTextFilter("Account Security Number", GetAccountSecurityNumberQuery);
				accountSecurityNumberFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|AccountSecurityNumber", "Account Security Number");
			}
		}

		#endregion

		#region Flags Filters

		void AddFlagsFilters(ModuleFilterCollection filters)
		{
			if (fModuleType != OrgModuleType.CompetitorIntelligence)
			{
				var secondaryOrgTypeFilter = new OrgSecondaryTypeModuleFilter("Secondary Type", OrgFilterHelper.GetSecondaryOrgTypeFilter, OrgFilterHelper.GetSecondaryOrgTypes(fModuleType));
				secondaryOrgTypeFilter.Category = FilterCategories.StatusAndFlags;
				secondaryOrgTypeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SecondaryType", "Secondary Type");
				filters.AddCustomFilter(secondaryOrgTypeFilter);
			}

			var organizationLanguageFilter = filters.AddTextFilter("Language", OrgHeaderSchema.OH_Language, Language_List);
			organizationLanguageFilter.Category = FilterCategories.StatusAndFlags;
			organizationLanguageFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|Language", "Organization Language");

			var categoryFilter = filters.AddTextFilter("Category", OrgHeaderSchema.OH_Category, Category_List);
			categoryFilter.Category = FilterCategories.StatusAndFlags;
			categoryFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|Category", "Category");

			var accountTypeFilter = filters.AddTextFilter("Organization – Account Type", GetAccountTypeFilter, AccountType_List);
			accountTypeFilter.Category = FilterCategories.StatusAndFlags;
			accountTypeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|AccountType", "Organization – Account Type");

			var knownShipperFilter = filters.AddTextFilter(SupplyChainSecurityConfiguration.KnownShipperFilterText, GetKnownShipperQuery, KnownShipperList);
			knownShipperFilter.Category = FilterCategories.StatusAndFlags;
			knownShipperFilter.MultilingualDescription = SupplyChainSecurityConfiguration.KnownShipperFilterDescription;

			var creditNotYetApproved = filters.AddFlagsFilter("Credit Not Yet Approved", new[] { Res.GetString("MasterFiles|OrganisationFilter|CreditNotYetApproved", "Credit Not Yet Approved") }, new GetFlagsQuery[] { GetCreditNotYetApprovedQuery });
			creditNotYetApproved.Category = FilterCategories.StatusAndFlags;
			creditNotYetApproved.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|CreditNotYetApproved", "Credit Not Yet Approved");

			if (!IsProductivityWiseModeEnabled)
			{
				var ratesSecurity = filters.AddTextFilter("Rates' Security", GetRatesSecurityFilter, RatesSecurityList);
				ratesSecurity.Category = FilterCategories.StatusAndFlags;
				ratesSecurity.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|RatesSecurity", "Rates' Security");
				ratesSecurity.SubGroup = new RatesSecuritySubGroup();
			}

			ModuleTextFilter filterExternalValidationStatus = filters.AddTextFilter("External Validation Status", GetExternalValidationFilter, ExternalValidationStatusList);
			filterExternalValidationStatus.Category = FilterCategories.StatusAndFlags;
			filterExternalValidationStatus.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ExternalValidationStatus", "External Validation Status");

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Brazil && ObjectFactory.Get<Enterprise.Integration.Customs.BR.IBRCustomsDataRegistry>().EnableForeignOperator)
			{
				var isForeignOperatorFilter = filters.AddTextFilter(OrgConstants.FilterControl.IsForeignOperator.FilterName, GetIsForeignOperatorQuery, IsForeignOperatorFilterOptions);
				isForeignOperatorFilter.Category = FilterCategories.StatusAndFlags;
				isForeignOperatorFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|IsForeignOperator", OrgConstants.FilterControl.IsForeignOperator.FilterName);
			}
		}

		#endregion

		#region Date Filters

		void AddDateFilters(ModuleFilterCollection filters)
		{
			if (fModuleType == OrgModuleType.Standard)
			{
				filters.AddDateFilter(OrgConstants.FilterControl.OrgDateFilterList.Description.STDARLastChecked, GetSTDARLastChecked).
					MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|STDARLastChecked", "AR Last QA");
				filters.AddDateFilter(OrgConstants.FilterControl.OrgDateFilterList.Description.STDAPLastChecked, GetSTDAPLastChecked).
					MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|STDAPLastChecked", "AP Last QA");

				if (!IsProductivityWiseModeEnabled)
				{
					filters.AddDateFilter(OrgConstants.FilterControl.OrgDateFilterList.Description.STDCRShipExpected, GetSTDCRShipExpected).
						MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|STDCRShipExpected", "CNR 1st Ship Exp");
					filters.AddDateFilter(OrgConstants.FilterControl.OrgDateFilterList.Description.STDCEShipExpected, GetSTDCEShipExpected).
						MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|STDCEShipExpected", "CNE 1st Ship Exp");
				}

				filters.AddDateFilter(OrgConstants.FilterControl.OrgDateFilterList.Description.STDCreditReviewDate, GetSTDCreditReviewDate).
					MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|STDCreditReviewDate", "AR Acct Review Due");
			}

			if (fModuleType == OrgModuleType.ClientIntelligence || fModuleType == OrgModuleType.Standard)
			{
				filters.AddDateFilter(OrgConstants.FilterControl.OrgDateFilterList.Description.SALCSDateLastCall, GetSALCSDateLastCall, true, true).
					MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALCSDateLastCall", "Last Actual Communication");
				filters.AddDateFilter(OrgConstants.FilterControl.OrgDateFilterList.Description.SALCSDateNextCall, GetSALCSDateNextCall, true, true).
					MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALCSDateNextCall", "Next Scheduled Communication");
				filters.AddDateFilter(OrgConstants.FilterControl.OrgDateFilterList.Description.SALCSDateLastUnactioned, GetSALCSDateLastUnactioned, true, true).
					MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALCSDateLastUnactioned", "Last Un-Actioned Communication");
				filters.AddDateFilter(OrgConstants.FilterControl.OrgDateFilterList.Description.SALCSEstClose, GetSALCSEstClose).
					MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALCSEstClose", "SAL Estimated Close");
				filters.AddDateFilter(OrgConstants.FilterControl.OrgDateFilterList.Description.SALCRClientComm, GetSALCRClientComm).
					MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALCRClientComm", "SAL Client Commenced");
			}

			if (SupplyChainSecurityConfiguration.IsAddressLevelScheme && !IsProductivityWiseModeEnabled)
			{
				filters.AddDateFilter(OrgConstants.FilterControl.OrgDateFilterList.Code.KnownShipperExpiryDate, GetKnownShipperExpiryDateQuery)
					.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|KnownApprovedDate", "Known/Approved Date");
			}

			filters.AddDateFilter(OrgConstants.FilterControl.OrgDateFilterList.Description.Created, OrgHeaderSchema.OH_SystemCreateTimeUtc, convertFromLocalToUTC: true)
				.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|Created", "Created");

			filters.AddDateFilter(OrgConstants.FilterControl.OrgDateFilterList.Description.PowerOfAttorneyValidToDate, GetPowerOfAttorneyValidToDate).
				MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|PowerOfAttorneyValidToDate", "Power Of Attorney Valid To Date");

			if (!IsProductivityWiseModeEnabled)
			{
				var lastScreenDateFilter = filters.AddDateFilter(OrgConstants.FilterControl.OrgDateFilterList.Description.LastScreenDate, GetLastScreenDateQuery, true, true);
				lastScreenDateFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|LastScreenDate", "Last Screening Date");
				lastScreenDateFilter.HideFutureDates = true;
			}

			if (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) == Core.Constants.CountryCodes.UnitedStates)
			{
				if (!IsProductivityWiseModeEnabled)
				{
					var importerBondQueriedFilter = filters.AddDateFilter(OrgConstants.FilterControl.OrgDateFilterList.Description.ImporterBondQueried, GetImporterBondQueried);
					importerBondQueriedFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|USOrganisationFilter|ImporterBondQueried", "Importer Bond Last Queried");
					importerBondQueriedFilter.HideFutureDates = true;
				}
			}
		}
		#endregion

		#region Location Filters

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			AddClosestPortFilter(filters);
			AddMainAddressCountryFilter(filters);

			if (!IsProductivityWiseModeEnabled)
			{
				var salTradeLanesFilter = filters.AddNkFilter(OrgConstants.FilterControl.UNLOCOType.SALTradeLanes, ViewLocationSchema.VLO_Code, ModuleIDs.ViewLocation, ViewLocations);
				salTradeLanesFilter.Category = FilterCategories.Locations;
				salTradeLanesFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALTradeLanes", "Sales Trade Lanes");
				salTradeLanesFilter.SubGroup = new SALTradeLanesSubGroup();
				salTradeLanesFilter.SupportsFiltersMatchComparisonOperator = false;

				var cmpTradeLanesFilter = filters.AddNkFilter(OrgConstants.FilterControl.UNLOCOType.CMPTradeLanes, ViewLocationSchema.VLO_Code, ModuleIDs.ViewLocation, ViewLocations);
				cmpTradeLanesFilter.Category = FilterCategories.Locations;
				cmpTradeLanesFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|CMPTradeLanes", "Competitors Lanes");
				cmpTradeLanesFilter.SubGroup = new CMPTradeLanesSubGroup();
				cmpTradeLanesFilter.SupportsFiltersMatchComparisonOperator = false;

				var forwarderAppPortFilter = filters.AddNkFilter(OrgConstants.FilterControl.UNLOCOType.ForwarderAppPort, GetAppPort, ModuleIDs.Location, Locations);
				forwarderAppPortFilter.Category = FilterCategories.Locations;
				forwarderAppPortFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ForwarderAppPort", "Forwarder Appointed Port");
				forwarderAppPortFilter.SubGroup = new ForwarderAppPortSubGroup();
				forwarderAppPortFilter.SupportsFiltersMatchComparisonOperator = false;

				var carrierAppPortFilter = filters.AddNkFilter(OrgConstants.FilterControl.UNLOCOType.CarrierAppPort, GetAppPort, ModuleIDs.Location, Locations);
				carrierAppPortFilter.Category = FilterCategories.Locations;
				carrierAppPortFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|CarrierAppPort", "Carrier Appointed Port");
				carrierAppPortFilter.SubGroup = new CarrierAppPortSubGroup();
				carrierAppPortFilter.SupportsFiltersMatchComparisonOperator = false;
			}

			var orgForwarderModuleFilter = new OrgLocatedWithinModuleFilter("Located Within", Factory);
			orgForwarderModuleFilter.Category = FilterCategories.Locations;
			orgForwarderModuleFilter.MultilingualDescription = ResString.GetMultilingualString("503603E2-D312-4DDA-9ACF-27ACC9BD4E43", "Located Within");
			filters.AddCustomFilter(orgForwarderModuleFilter);
		}

		void AddClosestPortFilter(ModuleFilterCollection filters)
		{
			ModuleNkFilter closestPortFilter = filters.AddNkFilter(OrgConstants.FilterControl.UNLOCOType.OrgPort, GetMainUNLOCOQuery, ModuleIDs.Location, Locations);
			closestPortFilter.Category = FilterCategories.Locations;
			closestPortFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|OrgPort", "Main UNLOCO");

			if (!Locations.AllowZones)
			{
				closestPortFilter.Visibility = FilterVisibility.AlwaysVisible;
				closestPortFilter.DefaultProperty = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				closestPortFilter.PropertyValidation = ClosestPortCountryFilterValidation;
			}
		}

		void ClosestPortCountryFilterValidation(ZPropertyInfo info)
		{
			if (!((ZString)info.Value).StartsWith(GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
			{
				string errorMessage = Res.GetString("b3f64399-a3a3-4401-9e0e-2d9165a48c29", @"Your current security rights only allow you to view organizations based in your current login country/region ({0}).
If you think this is incorrect, please contact your system administrator.", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				info.AddError(errorMessage);
			}
		}

		void AddMainAddressCountryFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddNkFilter(OrgConstants.FilterControl.OrgAddress.Country, GetMainAddressCountryQuery, ModuleIDs.RefCountry, Countries);
			filter.Category = FilterCategories.Locations;
			filter.MultilingualDescription = ResString.GetMultilingualString("29C1FD1C-9BDC-4E3C-84D8-9D301C95CBD8", "Country/Region");
		}

		ZQuery GetMainAddressCountryQuery(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH);
			orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_RN_NKCountryCode, SQLComparisonOperator.Equal, value);

			var orgAddressCapabilitySubQuery = new ZDBOnlySubQuery(typeof(OrgAddressCapability), OrgAddressCapabilitySchema.PZ_OA);
			orgAddressCapabilitySubQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_IsMainAddress, SQLComparisonOperator.Equal, 1);
			orgAddressCapabilitySubQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_AddressType, SQLComparisonOperator.Equal, AddressType.OFC);

			orgAddressSubQuery.AddSubQuery(OrgAddressSchema.PK, orgAddressCapabilitySubQuery, JoinCondition.And);
			query.AddSubQuery(OrgHeaderSchema.PK, orgAddressSubQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region Workflow Filters

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();

			var workflowHelper = new WorkflowFilterStripsHelper(typeof(OrgHeader), JobInvoicingConsumerTypes.Organisation.Code, Factory);
			workflowHelper.SetShouldAddWorkflowCustomFieldsFilters(ShouldAddWorkflowCustomFieldsFilters);
			helpers.Add(workflowHelper);

			return helpers;
		}

		#endregion

		#region Relationship Guids Filters

		void AddRelationshipGuidsFilters(ModuleFilterCollection filters)
		{
			if (fModuleType == OrgModuleType.Standard)
			{
				AddARFilters(filters);
				AddAPFilters(filters);

				if (!IsProductivityWiseModeEnabled)
				{
					AddCNRFilters(filters);
					AddCNEFilters(filters);
					AddFWDFilters(filters);
				}
			}
			if (fModuleType == OrgModuleType.ClientIntelligence && Env.Registry.OrgShowARTab)
			{
				AddARFilters(filters);
			}
			if (fModuleType == OrgModuleType.ClientIntelligence && Env.Registry.OrgShowConsigneeConsignorTab)
			{
				AddCNRFilters(filters);
				AddCNEFilters(filters);
			}
			if (fModuleType == OrgModuleType.Standard || fModuleType == OrgModuleType.ClientIntelligence)
			{
				AddSALFilters(filters);
			}
			if (Env.Security.OrgDetailsViewCompanysStaffAssignments.IsAllowed)
			{
				AddRelationshipManagerFilters(filters);
			}
			AddRelatedPartiesModuleFilters(filters);
		}

		protected virtual void AddRelatedPartiesModuleFilters(ModuleFilterCollection filters)
		{
			OrgRelatedPartiesModuleFilter orgRelatedPartiesModuleFilter = new OrgRelatedPartiesModuleFilter("Related Parties");
			orgRelatedPartiesModuleFilter.Category = RelationshipOrgStaff;
			orgRelatedPartiesModuleFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|RelatedParties", "Related Parties");
			filters.AddCustomFilter(orgRelatedPartiesModuleFilter);
		}

		protected readonly FilterCategory RelationshipOrgStaff = new FilterCategory(ResString.GetMultilingualString("MasterFiles|OrganisationFilter|RelationshipOrgStaff", "Relationship Org./Staff"));

		void AddARFilters(ModuleFilterCollection filters)
		{
			var debtorSubGroup = new CompanyDataSubGroup(OrgCompanyDataSchema.OB_IsDebtor);

			ModuleFilter filter = filters.AddGuidFilter(
				OrgConstants.FilterControl.GuidRelationships.Description.ARAcctGroup,
				ModuleIDs.OrgDebtorGroup,
				OrgCompanyDataSchema.OB_OJ_ARDebtorGroup,
				GetRelationshipGuidList(OrgConstants.FilterControl.GuidRelationships.Code.ARAcctGroup));
			filter.Category = RelationshipOrgStaff;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ARAcctGroup", "Receivables Debtor Group");
			filter.SubGroup = debtorSubGroup;

			filter = filters.AddNkFilter(
				OrgConstants.FilterControl.GuidRelationships.Description.ARCurrency,
				OrgCompanyDataSchema.OB_RX_NKARDDefltCurrency,
				ModuleIDs.RefCurrency,
				GetRelationshipGuidList(OrgConstants.FilterControl.GuidRelationships.Code.ARCurrency));
			filter.Category = RelationshipOrgStaff;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ARCurrency", "Receivables Currency");
			filter.SubGroup = debtorSubGroup;
		}

		void AddAPFilters(ModuleFilterCollection filters)
		{
			var creditorSubGroup = new CompanyDataSubGroup(OrgCompanyDataSchema.OB_IsCreditor);

			ModuleFilter filter = filters.AddGuidFilter(
				OrgConstants.FilterControl.GuidRelationships.Description.APAcctGroup,
				ModuleIDs.OrgCreditorGroup,
				OrgCompanyDataSchema.OB_OG_APCreditorGroup,
				GetRelationshipGuidList(OrgConstants.FilterControl.GuidRelationships.Code.APAcctGroup));
			filter.Category = RelationshipOrgStaff;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|APAcctGroup", "Payables Creditor Group");
			filter.SubGroup = creditorSubGroup;

			filter = filters.AddGuidFilter(
				OrgConstants.FilterControl.GuidRelationships.Description.APBankAccount,
				ModuleIDs.AccBankAccount,
				OrgCompanyDataSchema.OB_AB_APDefaultBankAccount,
				GetRelationshipGuidList(OrgConstants.FilterControl.GuidRelationships.Code.APBankAccount));
			filter.Category = RelationshipOrgStaff;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|APBankAccount", "Payables Default Bank Account");
			filter.SubGroup = creditorSubGroup;

			filter = filters.AddGuidFilter(
				OrgConstants.FilterControl.GuidRelationships.Description.APChargeCode,
				ModuleIDs.AccChargeCode,
				OrgCompanyDataSchema.OB_AC_APDefaultChargeCode,
				GetRelationshipGuidList(OrgConstants.FilterControl.GuidRelationships.Code.APChargeCode));
			filter.Category = RelationshipOrgStaff;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|APChargeCode", "Payables Charge Code");
			filter.SubGroup = creditorSubGroup;
		}

		void AddCNRFilters(ModuleFilterCollection filters)
		{
			var consignorSubGroup = new MiscServSubGroup(OrgHeaderSchema.OH_IsConsignor);
			var consignorStaffSubGroup = new StaffAssignmentSubGroup(OrgHeaderSchema.OH_IsConsignor);

			ModuleFilter filter = filters.AddNkFilter(
				OrgConstants.FilterControl.GuidRelationships.Description.CNRCountry,
				OrgMiscServSchema.OM_RN_NKEXDefaultCntryOfOrigin,
				ModuleIDs.RefCountry,
				GetRelationshipGuidList(OrgConstants.FilterControl.GuidRelationships.Code.CNRCountry));
			filter.Category = RelationshipOrgStaff;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|CNRCountry", "Consignor Country/Region of Origin");
			filter.SubGroup = consignorSubGroup;

			filter = filters.AddNkFilter(
				OrgConstants.FilterControl.GuidRelationships.Description.CNRCurrency,
				OrgMiscServSchema.OM_RX_NKEXDefCurrency,
				ModuleIDs.RefCurrency,
				GetRelationshipGuidList(OrgConstants.FilterControl.GuidRelationships.Code.CNRCurrency));
			filter.Category = RelationshipOrgStaff;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|CNRCurrency", "Consignor Default Currency");
			filter.SubGroup = consignorSubGroup;

			filter = filters.AddGuidFilter(
				OrgConstants.FilterControl.GuidRelationships.Description.CNRSeaCartageCordinator,
				ModuleIDs.GlbStaff,
				GetCNRSeaCartageCordinatorFilter,
				GetRelationshipGuidList(OrgConstants.FilterControl.GuidRelationships.Code.CNRSeaCartageCordinator));
			filter.Category = RelationshipOrgStaff;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|CNRSeaCartageCordinator", "Consignor Sea Port Transport Coordinator");
			filter.SubGroup = consignorStaffSubGroup;

			filter = filters.AddGuidFilter(
				OrgConstants.FilterControl.GuidRelationships.Description.CNRAirCartageCordinator,
				ModuleIDs.GlbStaff,
				GetCNRAirCartageCordinatorFilter,
				GetRelationshipGuidList(OrgConstants.FilterControl.GuidRelationships.Code.CNRAirCartageCordinator));
			filter.Category = RelationshipOrgStaff;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|CNRAirCartageCordinator", "Consignor Air Port Transport Coordinator");
			filter.SubGroup = consignorStaffSubGroup;

			filter = filters.AddGuidFilter(
				OrgConstants.FilterControl.GuidRelationships.Description.CNRSeaCustomerServiceRep,
				ModuleIDs.GlbStaff,
				GetCNRSeaCustomerServiceRepFilter,
				GetRelationshipGuidList(OrgConstants.FilterControl.GuidRelationships.Code.CNRSeaCustomerServiceRep));
			filter.Category = RelationshipOrgStaff;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|CNRSeaCustomerServiceRep", "Consignor Sea Service Representative");
			filter.SubGroup = consignorStaffSubGroup;

			filter = filters.AddGuidFilter(
				OrgConstants.FilterControl.GuidRelationships.Description.CNRAirCustomerServiceRep,
				ModuleIDs.GlbStaff,
				GetCNRAirCustomerServiceRepFilter,
				GetRelationshipGuidList(OrgConstants.FilterControl.GuidRelationships.Code.CNRAirCustomerServiceRep));
			filter.Category = RelationshipOrgStaff;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|CNRAirCustomerServiceRep", "Consignor Air Service Representative");
			filter.SubGroup = consignorStaffSubGroup;
		}

		void AddCNEFilters(ModuleFilterCollection filters)
		{
			var consigneeSubGroup = new StaffAssignmentSubGroup(OrgHeaderSchema.OH_IsConsignee);

			ModuleFilter filter = filters.AddGuidFilter(
				OrgConstants.FilterControl.GuidRelationships.Description.CNESeaCustomerServiceRep,
				ModuleIDs.GlbStaff,
				GetCNESeaCustomerServiceRepFilter,
				GetRelationshipGuidList(OrgConstants.FilterControl.GuidRelationships.Code.CNESeaCustomerServiceRep));
			filter.Category = RelationshipOrgStaff;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|CNESeaCustomerServiceRep", "Consignee Sea Service Representative");

			filter = filters.AddGuidFilter(
				OrgConstants.FilterControl.GuidRelationships.Description.CNEAirCustomerServiceRep,
				ModuleIDs.GlbStaff,
				GetCNEAirCustomerServiceRepFilter,
				GetRelationshipGuidList(OrgConstants.FilterControl.GuidRelationships.Code.CNEAirCustomerServiceRep));
			filter.Category = RelationshipOrgStaff;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|CNEAirCustomerServiceRep", "Consignee Air Service Representative");
			filter.SubGroup = consigneeSubGroup;

			filter = filters.AddGuidFilter(
				OrgConstants.FilterControl.GuidRelationships.Description.CNESeaCartageCordinator,
				ModuleIDs.GlbStaff,
				GetCNESeaCartageCordinatorFilter,
				GetRelationshipGuidList(OrgConstants.FilterControl.GuidRelationships.Code.CNESeaCartageCordinator));
			filter.Category = RelationshipOrgStaff;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|CNESeaCartageCordinator", "Consignee Sea Port Transport Coordinator");
			filter.SubGroup = consigneeSubGroup;

			filter = filters.AddGuidFilter(
				OrgConstants.FilterControl.GuidRelationships.Description.CNEAirCartageCordinator,
				ModuleIDs.GlbStaff,
				GetCNEAirCartageCordinatorFilter,
				GetRelationshipGuidList(OrgConstants.FilterControl.GuidRelationships.Code.CNEAirCartageCordinator));
			filter.Category = RelationshipOrgStaff;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|CNEAirCartageCordinator", "Consignee Air Port Transport Coordinator");
			filter.SubGroup = consigneeSubGroup;
		}

		void AddFWDFilters(ModuleFilterCollection filters)
		{
			var forwarderSubGroup = new MiscServSubGroup(OrgHeaderSchema.OH_IsForwarder);

			ModuleFilter filter = filters.AddNkFilter(
				OrgConstants.FilterControl.GuidRelationships.Description.FWDCurrency,
				OrgMiscServSchema.OM_RX_NKFWDefCurrency,
				ModuleIDs.RefCurrency,
				GetRelationshipGuidList(OrgConstants.FilterControl.GuidRelationships.Code.FWDCurrency));
			filter.Category = RelationshipOrgStaff;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|FWDCurrency", "Forwarder Default Currency");
			filter.SubGroup = forwarderSubGroup;
		}

		void AddSALFilters(ModuleFilterCollection filters)
		{
			var staffSubGroup = new StaffAssignmentSubGroup(null);
			var salesLeadSubGroup = new MiscServSubGroup(OrgHeaderSchema.OH_IsSalesLead);

			ModuleFilter filter = filters.AddGuidFilter(
				OrgConstants.FilterControl.GuidRelationships.Description.SALOverallRep,
				ModuleIDs.GlbStaff,
				GetSALOverallRepFilter,
				GetRelationshipGuidList(OrgConstants.FilterControl.GuidRelationships.Code.SALOverallRep));
			filter.Category = RelationshipOrgStaff;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALOverallRep", "Sales Overall Representative");
			filter.SubGroup = staffSubGroup;

			filter = filters.AddTextFilter("Sales Rep Assigned", GetSalesRepAssignedFilter, SalesRepAssigned_List);
			filter.Category = RelationshipOrgStaff;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SalesRepAssigned", "Sales Rep Assigned");

			filter = filters.AddGuidFilter(
				OrgConstants.FilterControl.GuidRelationships.Description.SALImportAirRep,
				ModuleIDs.GlbStaff,
				GetSALImportAirRepFilter,
				GetRelationshipGuidList(OrgConstants.FilterControl.GuidRelationships.Code.SALImportAirRep));
			filter.Category = RelationshipOrgStaff;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALImportAirRep", "Sales Import Air Representative");
			filter.SubGroup = staffSubGroup;

			filter = filters.AddGuidFilter(
				OrgConstants.FilterControl.GuidRelationships.Description.SALImportSeaRep,
				ModuleIDs.GlbStaff,
				GetSALImportSeaRepFilter,
				GetRelationshipGuidList(OrgConstants.FilterControl.GuidRelationships.Code.SALImportSeaRep));
			filter.Category = RelationshipOrgStaff;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALImportSeaRep", "Sales Import Sea Representative");
			filter.SubGroup = staffSubGroup;

			filter = filters.AddGuidFilter(
				OrgConstants.FilterControl.GuidRelationships.Description.SALExportAirRep,
				ModuleIDs.GlbStaff,
				GetSALExportAirRepFilter,
				GetRelationshipGuidList(OrgConstants.FilterControl.GuidRelationships.Code.SALExportAirRep));
			filter.Category = RelationshipOrgStaff;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALExportAirRep", "Sales Export Air Representative");
			filter.SubGroup = staffSubGroup;

			filter = filters.AddGuidFilter(
				OrgConstants.FilterControl.GuidRelationships.Description.SALExportSeaRep,
				ModuleIDs.GlbStaff,
				GetSALExportSeaRepFilter,
				GetRelationshipGuidList(OrgConstants.FilterControl.GuidRelationships.Code.SALExportSeaRep));
			filter.Category = RelationshipOrgStaff;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALExportSeaRep", "Sales Export Sea Representative");
			filter.SubGroup = staffSubGroup;

			filter = filters.AddGuidFilter(
				OrgConstants.FilterControl.GuidRelationships.Description.SALWarehousingRep,
				ModuleIDs.GlbStaff,
				GetSALWarehousingRepFilter,
				GetRelationshipGuidList(OrgConstants.FilterControl.GuidRelationships.Code.SALWarehousingRep));
			filter.Category = RelationshipOrgStaff;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALWarehousingRep", "Sales Warehousing Representative");
			filter.SubGroup = staffSubGroup;

			filter = filters.AddGuidFilter(
				OrgConstants.FilterControl.GuidRelationships.Description.SALOverallAccountManager,
				ModuleIDs.GlbStaff,
				GetSALOverallAccountManagerFilter,
				GetRelationshipGuidList(OrgConstants.FilterControl.GuidRelationships.Code.SALOverallAccountManager));
			filter.Category = RelationshipOrgStaff;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALOverallAccountManager", "Sales Overall Account Manager");
			filter.SubGroup = staffSubGroup;

			filter = filters.AddNkFilter(OrgConstants.FilterControl.GuidRelationships.Description.SALMainExpCommodity, OrgMiscServSchema.OM_RH_NKCMMainExportCmdty, ModuleIDs.RefCommodityCode, Commodities);
			filter.Category = RelationshipOrgStaff;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALMainExpCommodity", "Sales Main Export Commodity");
			filter.SubGroup = salesLeadSubGroup;

			filter = filters.AddNkFilter(OrgConstants.FilterControl.GuidRelationships.Description.SALMainImpCommodity, OrgMiscServSchema.OM_RH_NKCMMainImportCmdty, ModuleIDs.RefCommodityCode, Commodities);
			filter.Category = RelationshipOrgStaff;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALMainImpCommodity", "Sales Main Import Commodity");
			filter.SubGroup = salesLeadSubGroup;
		}

		protected readonly FilterCategory staffAssignments = new FilterCategory(ResString.GetMultilingualString("MasterFiles|OrganisationFilter|StaffAssignments", "Staff Assignments"));

		void AddRelationshipManagerFilters(ModuleFilterCollection filters)
		{
			var staffAssignmentsSubGroup = new StaffAssignmentsSubGroup();
			var staffSubGroup = new StaffAssignmentsStaffSubGroup(staffAssignmentsSubGroup);
			var companySubGroup = new StaffAssignmentsCompanySubGroup(staffAssignmentsSubGroup);

			ModuleFilter filter = filters.AddTextFilter(
				OrgConstants.FilterControl.DropEditRelationships.Description.RMStaffRole,
				GetRMStaffRoleFilter,
				GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.RMStaffRole));
			filter.Category = staffAssignments;
			filter.MultilingualDescription =
				ResString.GetMultilingualString("MasterFiles|OrganisationFilter|RMStaffRole",
					"Staff Assignments - Role");
			filter.SubGroup = staffAssignmentsSubGroup;

			filter = filters.AddGuidFilter(
				OrgConstants.FilterControl.GuidRelationships.Description.RMStaff,
				ModuleIDs.GlbStaff,
				GetRMStaffFilter,
				GetRelationshipGuidList(OrgConstants.FilterControl.GuidRelationships.Code.RMStaff));
			filter.Category = staffAssignments;
			filter.MultilingualDescription =
				ResString.GetMultilingualString("MasterFiles|OrganisationFilter|RMStaff",
					"Staff Assignments - Staff");
			filter.SubGroup = staffSubGroup;

			filter = filters.AddTextFilter(
				OrgConstants.FilterControl.DropEditRelationships.Description.RMStaffDepartment,
				GetRMStaffDepartmentFilter,
				GetDropEditRelationship_List(
					OrgConstants.FilterControl.DropEditRelationships.Code.RMStaffDepartment));
			filter.Category = staffAssignments;
			filter.MultilingualDescription = ResString.GetMultilingualString(
				"MasterFiles|OrganisationFilter|RMStaffDepartment", "Staff Assignments - Department");
			filter.SubGroup = staffAssignmentsSubGroup;

			filter = filters.AddGuidFilter(
				OrgConstants.FilterControl.GuidRelationships.Description.RMStaffCompany,
				ModuleIDs.GlbCompany,
				GetRMStaffCompanyFilter,
				GetRelationshipGuidList(OrgConstants.FilterControl.GuidRelationships.Code.RMStaffCompany));
			filter.Category = staffAssignments;
			filter.MultilingualDescription =
				ResString.GetMultilingualString("MasterFiles|OrganisationFilter|RMStaffCompany",
					"Staff Assignments - Company");
			filter.SubGroup = companySubGroup;

			if (!Env.Security.OrgDetailsViewOtherCompanysStaffAssignments.IsAllowed)
			{
				filter.ReadOnly = true;
				((ModuleGuidFilter)filter).Property = GlbCompany.CurrentCompany.PK;
			}

			filter = new ModuleGuidForeignCollectionFilter("Staff Assignments", ModuleIDs.StaffAssignments, OrgHeaderSchema.PK, OrgStaffAssignmentsSchema.O8_OH, new ActiveBusinessObjectCollection<OrgStaffAssignments>(Factory), typeof(OrgHeader));
			filter.Category = staffAssignments;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|StaffAssignments", "Staff Assignments");
			filters.AddFilter(filter);
		}

		#endregion

		#region Relationship Flags Filters

		[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		void AddRelationshipFlagsFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter;

			var debtorSubGroup = new CompanyDataSubGroup(OrgCompanyDataSchema.OB_IsDebtor);
			var creditorSubGroup = new CompanyDataSubGroup(OrgCompanyDataSchema.OB_IsCreditor);
			var consignorSubGroup = new MiscServSubGroup(OrgHeaderSchema.OH_IsConsignor);
			var consigneeSubGroup = new MiscServSubGroup(OrgHeaderSchema.OH_IsConsignee);
			var forwarderSubGroup = new MiscServSubGroup(OrgHeaderSchema.OH_IsForwarder);
			var miscFreightServicesSubGroup = new MiscServSubGroup(OrgHeaderSchema.OH_IsMiscFreightServices);
			var addressSubGroup = new OA_OHSubGroup();
			var salesLeadSubGroup = new MiscServSubGroup(OrgHeaderSchema.OH_IsSalesLead);

			var competitorSubGroup = new MiscServSubGroup(OrgHeaderSchema.OH_IsCompetitor);

			if (fModuleType == OrgModuleType.Standard)
			{
				if (!IsProductivityWiseModeEnabled)
				{
					filter = filters.AddTextFilter(
						OrgConstants.FilterControl.DropEditRelationships.Description.CustomsCodeType,
						OrgCusCodeSchema.OK_CodeType,
						GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.CustomsCodeType));
					filter.Category = RelationshipFlagsCategory;
					filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|CustomsCodeType", "Customs Code Type");
					filter.SubGroup = new OK_OHSubGroup();

					filter = filters.AddTextFilter(
						OrgConstants.FilterControl.DropEditRelationships.Description.GlobalRateTarriff,
						GetGlobalRateTarriffFilter,
						GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.GlobalRateTarriff));
					filter.Category = RelationshipFlagsCategory;
					filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|GlobalRateTarriff", "Company Tariff");
					filter.SubGroup = new GlobalRateTariffSubGroup();
				}

				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.ARAcctRelationship,
					OrgCompanyDataSchema.OB_ARCategory,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.ARAcctRelationship));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ARAcctRelationship", "Receivables Accounts Relationship");
				filter.SubGroup = debtorSubGroup;

				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.ARConsolidation,
					OrgCompanyDataSchema.OB_ARConsolidatedAccountingCategory,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.ARConsolidation));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ARConsolidation", "Receivables Consolidation Category");
				filter.SubGroup = debtorSubGroup;

				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.ARStdInvoiceTerms,
					GetARStdInvoiceTermsFilter,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.ARStdInvoiceTerms));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ARStdInvoiceTerms", "Receivables Standard Invoice Terms");

				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.ARDisbInvoiceTerms,
					GetARDisbInvoiceTermsFilter,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.ARDisbInvoiceTerms));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ARDisbInvoiceTerms", "Receivables Disbursement Invoice Terms");

				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.APAcctRelationship,
					OrgCompanyDataSchema.OB_APCategory,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.APAcctRelationship));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|APAcctRelationship", "Payables Accounts Relationship");
				filter.SubGroup = creditorSubGroup;

				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.APConsolidation,
					OrgCompanyDataSchema.OB_ARConsolidatedAccountingCategory,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.APConsolidation));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|APConsolidation", "Payables Consolidation Category");
				filter.SubGroup = creditorSubGroup;

				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.APPaymentTerms,
					OrgCompanyDataSchema.OB_APPaymentTerms,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.APPaymentTerms));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|APPaymentTerms", "Payables Payment Terms");
				filter.SubGroup = new APInvoiceTermsSubGroup();

				if (!IsProductivityWiseModeEnabled)
				{
					filter = filters.AddTextFilter(
						OrgConstants.FilterControl.DropEditRelationships.Description.CNRExportCategory,
						OrgMiscServSchema.OM_EXExporterCategory,
						GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.CNRExportCategory));
					filter.Category = RelationshipFlagsCategory;
					filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|CNRExportCategory", "Consignor Exporter Category");
					filter.SubGroup = consignorSubGroup;

					filter = filters.AddTextFilter(
						OrgConstants.FilterControl.DropEditRelationships.Description.CNRINCOTerm,
						OrgMiscServSchema.OM_EXDefaultIncoTerm,
						GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.CNRINCOTerm));
					filter.Category = RelationshipFlagsCategory;
					filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|CNRIncoterm", "Consignor Incoterm");
					filter.SubGroup = consignorSubGroup;

					filter = filters.AddTextFilter(
						OrgConstants.FilterControl.DropEditRelationships.Description.CNEImportCategory,
						OrgMiscServSchema.OM_IMImporterCategory,
						GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.CNEImportCategory));
					filter.Category = RelationshipFlagsCategory;
					filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|CNEImportCategory", "Consignee Importer Category");
					filter.SubGroup = consigneeSubGroup;

					filter = filters.AddTextFilter(
						OrgConstants.FilterControl.DropEditRelationships.Description.CNEMergeCusLines,
						OrgMiscServSchema.OM_IMMergeCustomsInvoiceLinesBy,
						GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.CNEMergeCusLines));
					filter.Category = RelationshipFlagsCategory;
					filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|CNEMergeCusLines", "Consignee Merge Customs Lines By");
					filter.SubGroup = consigneeSubGroup;

					filter = filters.AddTextFilter(
						OrgConstants.FilterControl.DropEditRelationships.Description.CNESendAirDocs,
						OrgMiscServSchema.OM_IMSendImportDocsTo,
						GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.CNESendAirDocs));
					filter.Category = RelationshipFlagsCategory;
					filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|CNESendAirDocs", "Consignee Send Air Documents To");
					filter.SubGroup = consigneeSubGroup;

					filter = filters.AddTextFilter(
						OrgConstants.FilterControl.DropEditRelationships.Description.CNESendSeaDocs,
						OrgMiscServSchema.OM_IMSendSeaImportDocsTo,
						GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.CNESendSeaDocs));
					filter.Category = RelationshipFlagsCategory;
					filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|CNESendSeaDocs", "Consignee Send Sea Documents To");
					filter.SubGroup = consigneeSubGroup;

					filter = filters.AddTextFilter(
						OrgConstants.FilterControl.DropEditRelationships.Description.FWDAgentCategory,
						OrgMiscServSchema.OM_FWAgentCategory,
						GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.FWDAgentCategory));
					filter.Category = RelationshipFlagsCategory;
					filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|FWDAgentCategory", "Forwarder Agent Category");
					filter.SubGroup = forwarderSubGroup;
				}

				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.SRVUsagePreference,
					OrgMiscServSchema.OM_SVServicesCategory,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.SRVUsagePreference));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SRVUsagePreference", "Service Provider Usage Preference");
				filter.SubGroup = miscFreightServicesSubGroup;

				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.ADRType,
					OrgAddressCapabilitySchema.PZ_AddressType,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.ADRType));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ADRType", "Address Type");
				filter.SubGroup = new ADRTypeSubGroup();

				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.ADRLanguage,
					OrgAddressSchema.OA_Language,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.ADRLanguage));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ADRLanguage", "Address Language");
				filter.SubGroup = addressSubGroup;

				AddOrgSecurityGroupFilters(filters, RelationshipFlagsCategory);

				if (!IsProductivityWiseModeEnabled)
				{
					filter = filters.AddTextFilter(
						OrgConstants.FilterControl.DropEditRelationships.Description.FCLEquipment,
						OrgAddressSchema.OA_FCLEquipmentNeeded,
						GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.FCLEquipment));
					filter.Category = RelationshipFlagsCategory;
					filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|FCLEquipment", "Equipment Needed For FCL Drop Mode");
					filter.SubGroup = addressSubGroup;

					filter = filters.AddTextFilter(
						OrgConstants.FilterControl.DropEditRelationships.Description.LCLEquipment,
						OrgAddressSchema.OA_LCLEquipmentNeeded,
						GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.LCLEquipment));
					filter.Category = RelationshipFlagsCategory;
					filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|LCLEquipment", "Equipment Needed For LCL Drop Mode");
					filter.SubGroup = addressSubGroup;

					filter = filters.AddTextFilter(
						OrgConstants.FilterControl.DropEditRelationships.Description.AirEquipment,
						OrgAddressSchema.OA_AIREquipmentNeeded,
						GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.AirEquipment));
					filter.Category = RelationshipFlagsCategory;
					filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|AirEquipment", "Equipment Needed For Air Drop Mode");
					filter.SubGroup = addressSubGroup;

					var feesAndChargesFilter = new FeesAndChargesFilter("Fees And Charges", GetFeesAndChargesQuery)
					{
						Category = RelationshipFlagsCategory,
						MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|FeesAndCharges", "Fees And Charges"),
						SubGroup = new FeesAndChargesSubGroup()
					};
					filters.AddCustomFilter(feesAndChargesFilter);
				}
			}

			if (fModuleType == OrgModuleType.Standard || fModuleType == OrgModuleType.ClientIntelligence)
			{
				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.SALCategory,
					OrgMiscServSchema.OM_CMSalesCategory,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.SALCategory));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALCategory", "Sales Client Category");
				filter.SubGroup = salesLeadSubGroup;

				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.SALClientSize,
					OrgMiscServSchema.OM_CMClientSize,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.SALClientSize));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALClientSize", "Sales Client Size");
				filter.SubGroup = salesLeadSubGroup;

				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.SALIndustryVertical,
					OrgMiscServSchema.OM_CMIndustryVertical,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.SALIndustryVertical));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALIndustryVertical", "Sales Client Vertical Market");
				filter.SubGroup = salesLeadSubGroup;

				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.SALPeriodOfActivity,
					OrgMiscServSchema.OM_CMPeriodOfActivity,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.SALPeriodOfActivity));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALPeriodOfActivity", "Sales Client Period of Activity");
				filter.SubGroup = salesLeadSubGroup;

				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.SALTerritory,
					OrgMiscServSchema.OM_CMSalesTerritory,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.SALTerritory));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALTerritory", "Sales Client Territory");
				filter.SubGroup = salesLeadSubGroup;

				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.SALOutlook,
					OrgMiscServSchema.OM_CMGrowthOutlook,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.SALOutlook));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALOutlook", "Sales Client Growth Outlook");
				filter.SubGroup = salesLeadSubGroup;

				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.SALCompActivity,
					OrgMiscServSchema.OM_CMCompetitorActivity,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.SALCompActivity));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALCompActivity", "Sales Main Competitor Activity");
				filter.SubGroup = salesLeadSubGroup;

				var salesMainCompetitorFilter = new OrgSalesMainCompetitorModuleFilter("Sales Main Competitor On", GetSalesMainCompetitorOnQuery);
				salesMainCompetitorFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SalesMainCompetitor", "Sales Main Competitor On");
				salesMainCompetitorFilter.Category = RelationshipFlagsCategory;
				filters.AddCustomFilter(salesMainCompetitorFilter);

				var hasMainCompetitorFilter = new OrgHasMainCompetitorModuleFilter("Has Main Competitor On", GetHasMainCompetitorOnQuery);
				hasMainCompetitorFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|HasMainCompetitor", "Has Main Competitor On");
				hasMainCompetitorFilter.Category = RelationshipFlagsCategory;
				filters.AddCustomFilter(hasMainCompetitorFilter);

				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.SALAirCosts,
					OrgMiscServSchema.OM_CMOverallEffectOfClientOnAirfreightCosts,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.SALAirCosts));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALAirCosts", "Sales Client Effect on Air Costs");
				filter.SubGroup = salesLeadSubGroup;

				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.SALLCLCosts,
					OrgMiscServSchema.OM_CMOverallEffectOfClientOnLCLCosts,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.SALLCLCosts));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALLCLCosts", "Sales Client Effect on LCL Costs");
				filter.SubGroup = salesLeadSubGroup;

				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.SALTEUCosts,
					OrgMiscServSchema.OM_CMOverallEffectOfClientOnTEUCosts,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.SALTEUCosts));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALTEUCosts", "Sales Client Effect on TEU Costs");
				filter.SubGroup = salesLeadSubGroup;

				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.SALWarehouseCosts,
					OrgMiscServSchema.OM_CMOverallEffectOfClientOnWarehousingCosts,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.SALWarehouseCosts));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALWarehouseCosts", "Sales Client Effect on Warehousing Costs");
				filter.SubGroup = salesLeadSubGroup;

				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.SALOtherCosts,
					OrgMiscServSchema.OM_CMOverallEffectOfClientOnOtherCosts,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.SALOtherCosts));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALOtherCosts", "Sales Client Effect on Other Costs");
				filter.SubGroup = salesLeadSubGroup;

				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.SALClientRelationship,
					GetSALClientRelationshipFilter,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.SALClientRelationship));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALClientRelationship", "Sales Overall Client Relation");
				filter.SubGroup = salesLeadSubGroup;

				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.SALDesireToRemain,
					GetSALDesireToRemainFilter,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.SALDesireToRemain));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALDesireToRemain", "Sales Client Desire To Remain With Company");
				filter.SubGroup = salesLeadSubGroup;

				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.SALEaseToPoach,
					GetSALEaseToPoachFilter,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.SALEaseToPoach));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALEaseToPoach", "Sales Ease Client Can Be Poached");
				filter.SubGroup = salesLeadSubGroup;

				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.SALElectronicIntegration,
					GetSALElectronicIntegrationFilter,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.SALElectronicIntegration));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALElectronicIntegration", "Sales Amount of Electronic Integration");
				filter.SubGroup = salesLeadSubGroup;

				var organisationFilterHelper = new OrganisationFilterHelper();
				ModuleTextFilter orgMarketingModuleFilter = filters.AddTextFilter(
					"Marketing Options",
					(value) => organisationFilterHelper.GetMarketingOptionsFilter(value).AddToFilter(OrgHeaderSchema.OH_IsSalesLead, ZBool.True),
					organisationFilterHelper.GetMarketingOptions);
				orgMarketingModuleFilter.Category = RelationshipFlagsCategory;
				orgMarketingModuleFilter.MultilingualDescription = ResString.GetMultilingualString("207618AD-7CD3-4EE9-B0D8-E401DEBD436F", "Marketing Options");
				orgMarketingModuleFilter.ShowDescription = false;
			}

			if ((fModuleType == OrgModuleType.Standard || fModuleType == OrgModuleType.CompetitorIntelligence) && !IsProductivityWiseModeEnabled)
			{
				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.CMPServiceType,
					OrgMiscServSchema.OM_CITypeOfService,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.CMPServiceType));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|CMPServiceType", "Competitor Service Type");
				filter.SubGroup = competitorSubGroup;

				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.CMPSellStyle,
					OrgMiscServSchema.OM_CISellingStyle,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.CMPSellStyle));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|CMPSellStyle", "Competitor Selling Style");
				filter.SubGroup = competitorSubGroup;

				filter = filters.AddTextFilter(
					OrgConstants.FilterControl.DropEditRelationships.Description.CMPCategory,
					OrgMiscServSchema.OM_CICompetitorCategory,
					GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.CMPCategory));
				filter.Category = RelationshipFlagsCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|CMPCategory", "Competitor Category");
				filter.SubGroup = competitorSubGroup;
			}
		}

		protected FilterCategory RelationshipFlagsCategory
		{
			get => relationshipFlagsCategory ?? (relationshipFlagsCategory = new FilterCategory(ResString.GetMultilingualString("MasterFiles|OrganisationFilter|RelationshipFlags", "Relationship Flags")));
		}
		FilterCategory relationshipFlagsCategory;

		#region OrgSecurityGroup

		void AddOrgSecurityGroupFilters(ModuleFilterCollection filters, FilterCategory category)
		{
			var filter = filters.AddGuidFilter(
				"Security Group",
				ModuleIDs.GlbGroup,
				GetSecurityGroupFilter,
				OrgSecurityGroupsCollection);
			filter.Category = category;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|OrgSecurityGroupFilter", "Security Group");
		}

		protected ZQuery GetSecurityGroupFilter(ZGuid value)
		{
			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			var subquery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH);
			subquery.AddToFilter(OrgMiscServSchema.OM_GG_OrgSecurityGroup, SQLComparisonOperator.Equal, value);
			query.AddSubQuery(OrgHeaderSchema.PK, subquery, JoinCondition.And);
			return query;
		}

		public GlbGroupCollection OrgSecurityGroupsCollection => new GlbGroupCollection(Factory);
		#endregion

		#endregion

		#region Organisation Type Related Filters

		readonly FilterCategory organisationTypeCategory = new FilterCategory(ResString.GetMultilingualString("MasterFiles|OrganisationFilter|OrganisationType", "Organization Type"));

		void AddOrgTypeRelatedFilters(ModuleFilterCollection filters)
		{
			if (!IsProductivityWiseModeEnabled)
			{
				ModuleFilter filter = filters.AddGuidFilter("Consignee - Related Consignor", ModuleIDs.Organisation, OrgSupplierBuyerLinkSchema.OL_OH_Supplier, Consignors);
				filter.Category = organisationTypeCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ConsigneeRelatedConsignor", "Consignee - Related Consignor");
				filter.SubGroup = new SupplierBuyerLinkSubGroup(OrgHeaderSchema.OH_IsConsignee, OrgSupplierBuyerLinkSchema.OL_OH_Buyer);

				filter = filters.AddGuidFilter("Consignor - Related Consignee", ModuleIDs.Organisation, OrgSupplierBuyerLinkSchema.OL_OH_Buyer, Consignees);
				filter.Category = organisationTypeCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ConsignorRelatedConsignee", "Consignor - Related Consignee");
				filter.SubGroup = new SupplierBuyerLinkSubGroup(OrgHeaderSchema.OH_IsConsignor, OrgSupplierBuyerLinkSchema.OL_OH_Supplier);

				filter = filters.AddTextFilter(OrgConstants.FilterControl.DropEditRelationships.Description.MCRCarrierCategory, OrgMiscServSchema.OM_CRCarrierCategory, GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.MCRCarrierCategory));
				filter.Category = organisationTypeCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|MCRCarrierCategory", "Carrier - Carrier Category");
				filter.SubGroup = new MiscServSubGroup(OrgHeaderSchema.OH_IsShippingProvider);

				OrgForwarderModuleFilter orgForwarderModuleFilter = new OrgForwarderModuleFilter("Forwarder - Agent Status");
				orgForwarderModuleFilter.Category = organisationTypeCategory;
				orgForwarderModuleFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ForwarderAgentStatus", "Forwarder - Agent Status");
				filters.AddCustomFilter(orgForwarderModuleFilter);
			}

			OrgReceivablesModuleFilter orgReceivablesModuleFilter = new OrgReceivablesModuleFilter("Receivables - Invoice Number");
			orgReceivablesModuleFilter.Category = organisationTypeCategory;
			orgReceivablesModuleFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ReceivablesInvoiceNumber", "Receivables - Invoice Number");
			filters.AddCustomFilter(orgReceivablesModuleFilter);

			OrgTypeModuleFilter orgTypeModuleFilter = new OrgTypeModuleFilter("Organisation Types");
			orgTypeModuleFilter.Category = organisationTypeCategory;
			orgTypeModuleFilter.ModuleType = fModuleType;
			orgTypeModuleFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|OrganisationTypes", "Organization Types");
			filters.AddCustomFilter(orgTypeModuleFilter);
		}

		#endregion

		#region Related Trade Lane Filters

		void AddTradeLaneRelatedFilters(ModuleFilterCollection filters)
		{
			if (!IsProductivityWiseModeEnabled)
			{
				FilterCategory relatedTradeLanesCategory = new FilterCategory(ResString.GetMultilingualString("MasterFiles|OrganisationFilter|RelatedTradeLanes", "Related Trade Lanes"));
				TradeLaneSubGroup tradeLaneSubGroup = new TradeLaneSubGroup(filters);
				TradeLaneProductSubGroup tradeLaneProductSubGroup = new TradeLaneProductSubGroup(tradeLaneSubGroup);
				TradeLaneDetailSubGroup tradeLaneDetailSubGroup = new TradeLaneDetailSubGroup(tradeLaneSubGroup);

				ModuleTextFilter tradeModefilter = filters.AddTextFilter(OrgConstants.FilterControl.DropEditRelationships.Description.SALTradeMode, GetSALTradeModeFilter, GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.SALTradeMode));
				tradeModefilter.Category = relatedTradeLanesCategory;
				tradeModefilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALTradeMode", "Sales Monthly Trade Mode");

				ModuleTextFilter tradeLaneIndustryVertical = filters.AddTextFilter(OrgConstants.FilterControl.DropEditRelationships.Description.SALTradeLaneIndustryVertical, GetTradeLaneIndustryVerticalFilter, GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.SALTradeLaneIndustryVertical));
				tradeLaneIndustryVertical.Category = relatedTradeLanesCategory;
				tradeLaneIndustryVertical.SubGroup = tradeLaneDetailSubGroup;
				tradeLaneIndustryVertical.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALTradeLaneIndustryVertical", "Sales Trade Lane Vertical Market");

				ModuleTextFilter tradeLanePeriodOfActivity = filters.AddTextFilter(OrgConstants.FilterControl.DropEditRelationships.Description.SALTradeLanePeriodOfActivity, GetTradeLanePeriodOfActivityFilter, GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.SALTradeLanePeriodOfActivity));
				tradeLanePeriodOfActivity.Category = relatedTradeLanesCategory;
				tradeLanePeriodOfActivity.SubGroup = tradeLaneDetailSubGroup;
				tradeLanePeriodOfActivity.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALTradeLanePeriodOfActivity", "Sales Trade Lane Period of Activity");

				ModuleLocationFilter locationFilter = filters.AddLocationFilter(OrgConstants.FilterControl.UNLOCOType.SALTradeLaneLocation, GetTradeLaneLocationFilter, Locations, Locations);
				locationFilter.MaxLength = ViewLocationSchema.VLO_Code.MaxLength;
				locationFilter.SetItemDescriptions(Res.GetData("MasterFiles|OrganisationFilter|RelatedTradeLanesDirection|Origin", "Origin"), Res.GetData("MasterFiles|OrganisationFilter|RelatedTradeLanesDirection|Destination", "Destination"));
				locationFilter.Category = relatedTradeLanesCategory;
				locationFilter.SubGroup = tradeLaneSubGroup;
				locationFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALTradeLaneLocation", "Sales Trade Lane Origin / Destination");

				ModuleTextFilter salesProductfilter = filters.AddTextFilter(OrgConstants.FilterControl.DropEditRelationships.Description.SALTransportMode, GetTradeLaneProductFilter, GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.SALTransportMode));
				salesProductfilter.Category = relatedTradeLanesCategory;
				salesProductfilter.SubGroup = tradeLaneProductSubGroup;
				salesProductfilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALTradeLaneProduct", "Sales Trade Lane Product");

				ModuleNkFilter commodityfilter = filters.AddNkFilter(OrgConstants.FilterControl.GuidRelationships.Description.SALTradeLaneCommodity, GetTradeLaneCommodityFilter, ModuleIDs.RefCommodityCode, Commodities);
				commodityfilter.Category = relatedTradeLanesCategory;
				commodityfilter.SubGroup = tradeLaneDetailSubGroup;
				commodityfilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALTradeLaneCommodity", "Sales Trade Lane Commodity");

				ModuleTextFilter statusfilter = filters.AddTextFilter(OrgConstants.FilterControl.DropEditRelationships.Description.SALTradeStatus, GetTradeLaneStatusFilter, GetDropEditRelationship_List(OrgConstants.FilterControl.DropEditRelationships.Code.SALTradeStatus));
				statusfilter.Category = relatedTradeLanesCategory;
				statusfilter.SubGroup = tradeLaneDetailSubGroup;
				statusfilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|SALTradeLaneStatus", "SAL Trade Status");
			}
		}

		#endregion

		#region Registration Number Filters

		void AddRegistrationNumberFilters(ModuleFilterCollection filters)
		{
			FilterCategory registrationNumbersCategory = new FilterCategory(ResString.GetMultilingualString("MasterFiles|OrganisationFilter|RegistrationNumbers", "Registration Numbers"));

			OrgRegistrationCountryAndTypeModuleFilter registrationCountryAndTypeFilter = new OrgRegistrationCountryAndTypeModuleFilter("Registration Country/Type", GetRegistrationCountryAndTypeQuery, Countries, GetDefaultRegistrationTypeList(), UpdateRegistrationTypeList, GetDefaultRegistrationTypeList, false);
			registrationCountryAndTypeFilter.Category = registrationNumbersCategory;
			registrationCountryAndTypeFilter.MultilingualDescription = ResString.GetMultilingualString("d690d257-070e-4960-b9a6-2c5fc278c43d", "Registration Country(Region)/Type");
			filters.AddCustomFilter(registrationCountryAndTypeFilter);

			OrgRegistrationCountryAndTypeModuleFilter noRegistrationCountryAndTypeFilter = new OrgRegistrationCountryAndTypeModuleFilter("No Registration Country/Type", GetRegistrationCountryAndTypeQuery, Countries, GetDefaultRegistrationTypeList(), UpdateRegistrationTypeList, GetDefaultRegistrationTypeList, true);
			noRegistrationCountryAndTypeFilter.Category = registrationNumbersCategory;
			noRegistrationCountryAndTypeFilter.MultilingualDescription = ResString.GetMultilingualString("f715ad7a-b118-4986-8cd0-3631cded2d1d", "No Registration Country(Region)/Type");
			filters.AddCustomFilter(noRegistrationCountryAndTypeFilter);
		}

		ZQuery GetRegistrationCountryAndTypeQuery(ZString value1, ZString value2)
		{
			var query = new ZQuery();
			var subQuery = new ZQuery();
			if (!value1.IsEmpty)
			{
				subQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, value1);
			}
			if (!value2.IsEmpty)
			{
				subQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, value2);
			}
			query.AddToFilter(subQuery);
			return query;
		}

		CodeDescriptionPairList GetDefaultRegistrationTypeList()
		{
			return new OrgCodeLists().CustomsCodes_List((RefCountry)null);
		}

		void UpdateRegistrationTypeList(IList typeList, ZString countryCode)
		{
			CodeDescriptionPairList listToUpdate = typeList as CodeDescriptionPairList;

			if (listToUpdate != null)
			{
				listToUpdate.Clear();
				if (!countryCode.IsEmpty)
				{
					RefCountry country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);
					if (country != null)
					{
						listToUpdate.AddRangeOverwriteIfExists(new OrgCodeLists().CustomsCodes_List(country));
					}
				}

				if (listToUpdate.Count == 0)
				{
					listToUpdate.AddRangeOverwriteIfExists(new OrgCodeLists().CustomsCodes_List((RefCountry)null));
				}

				listToUpdate.Sort();
			}
		}

		#endregion

		#region External Debtor/Creditor Code Filters

		ZQuery GetExternalCodeQuery(SQLComparisonOperator comparisonOperator, ZString value, SchemaStringColumn externalCodeField, string codeType)
		{
			var query = new ZDBOnlyQuery(typeof(OrgHeader));

			var orgCompanyDataSubQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH, comparisonOperator == SpecialComparisonOperator.IsBlank);
			orgCompanyDataSubQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);

			var cusCodeQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
			cusCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, codeType);
			cusCodeQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.Country.Code);

			var subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			subQuery.AddToFilter(externalCodeField, ZString.Empty);

			if (comparisonOperator == SpecialComparisonOperator.IsBlank || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				orgCompanyDataSubQuery.AddToFilter(externalCodeField, SQLComparisonOperator.NotEqual, string.Empty);

				cusCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, SQLComparisonOperator.NotEqual, string.Empty);
				cusCodeQuery.AddSubQuery(OrgCusCodeSchema.OK_OH, subQuery, JoinCondition.And);
			}
			else
			{
				var externalCodeSubQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.PK);
				externalCodeSubQuery.AddToFilter(externalCodeField, comparisonOperator, value);
				externalCodeSubQuery.AddToFilter(externalCodeField, SQLComparisonOperator.NotEqual, ZString.Empty);

				orgCompanyDataSubQuery.AddSubQuery(externalCodeSubQuery, JoinCondition.And);

				cusCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, comparisonOperator, value);
				cusCodeQuery.AddSubQuery(OrgCusCodeSchema.OK_OH, subQuery, JoinCondition.And);
			}
			orgCompanyDataSubQuery.AddAsUnionQuery(cusCodeQuery);
			query.AddSubQuery(orgCompanyDataSubQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region Hidden Filters

		void AddHiddenFilters(ModuleFilterCollection filters)
		{
			ModuleFilter hiddenGuidFilter = filters.AddTextFilter("SystemDefinedOrg", GetSystemDefinedOrg);
			hiddenGuidFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
		}

		ZQuery GetSystemDefinedOrg(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();
			//	please synch any changes to GetGlowIndexQuery as well
			query.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation);
			query.AddToFilter(JoinCondition.And, OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation);

			return query;
		}

		#endregion

		#region Shipping Line Filters

		void AddShippingLineFilters(ModuleFilterCollection filters)
		{
			var shippingLineSubGroup = new OH_RSL_ShippingLineSubGroup();
			var shippingLineName = filters.AddTextFilter("Shipping Line Name", RefShippingLineSchema.RSL_CarrierName);
			shippingLineName.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ShippingLineName", "Shipping Line Name");
			shippingLineName.SubGroup = shippingLineSubGroup;

			var shippingLineSCAC = filters.AddTextFilter("Shipping Line SCAC", RefShippingLineSchema.RSL_StandardCarrierAlphaCode);
			shippingLineSCAC.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ShippingLineSCAC", "Shipping Line SCAC");
			shippingLineSCAC.SubGroup = shippingLineSubGroup;

			var shippingLineC1C = filters.AddTextFilter("Shipping Line C1C", RefShippingLineSchema.RSL_CargoWiseOneCode);
			shippingLineC1C.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ShippingLineC1C", "Shipping Line C1C");
			shippingLineC1C.SubGroup = shippingLineSubGroup;

			var shippingLineSCACOrC1CFilter = filters.AddTextFilter("Shipping Line SCAC or C1C", RefShippingLineFilterBusinessObject.GetSCACorC1CQuery);
			shippingLineSCACOrC1CFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ShippingLineSCACorC1C", "Shipping Line SCAC or C1C");
			shippingLineSCACOrC1CFilter.MaxLength = Math.Max(RefShippingLineSchema.RSL_StandardCarrierAlphaCode.MaxLength, RefShippingLineSchema.RSL_CargoWiseOneCode.MaxLength);
			shippingLineSCACOrC1CFilter.SubGroup = shippingLineSubGroup;

			var shippingLineIntegrationsEnabled = filters.AddFlagsFilter("Shipping Line Integrations Enabled", RefShippingLineFilterBusinessObject.GetFlagsNames(), RefShippingLineFilterBusinessObject.GetFlagColumns());
			shippingLineIntegrationsEnabled.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ShippingLineIntegrationsEnabled", "Shipping Line Integrations Enabled");
			shippingLineIntegrationsEnabled.SubGroup = shippingLineSubGroup;

			var shippingLine = filters.AddGuidFilter("Shipping Line", ModuleIDs.RefShippingLine, OrgHeaderSchema.OH_RSL_ShippingLine, new RefShippingLineCollection(Factory));
			shippingLine.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|ShippingLine", "Shipping Line");
		}

		#endregion

		#region Sales Relation Activity Filters

		void AddSalesRelationActivityFilters(ModuleFilterCollection filters)
		{
			filters.AddCustomFilter(new OrganisationHasSalesRelationFilter(ZString.Empty, ZString.Empty, OrgHeaderSchema.PK));
			filters.AddCustomFilter(new OrganisationRecentActivityDateFilter(ZString.Empty, ZString.Empty, OrgHeaderSchema.PK));
		}

		#endregion

		#region CreditScores

		void AddCreditScoresFilters(ModuleFilterCollection filters)
		{
			if (OrganisationsDataRegistry.Instance.EnableCreditReports.Value &&
				OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.Value.Cast<CreditReportItem>().Any(x => x.CountryEnabledForCompany && x.CountryCode == Env.CurrentCompany.Country.Code))
			{
				var subGroup = new CreditScoresSubGroup();
				var creditScoresCategory = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("799436B5-2BDD-4639-AC7C-6918CA9E62DF", "Credit Scores"));

				var dnbCreditRatingFilter = new OrgCreditScoresDnBRatingModuleFilter("D&B Rating");
				dnbCreditRatingFilter.MultilingualDescription = ResString.GetMultilingualString("E2093A51-185C-4A59-8BC6-FE76EC52B602", "D&B Rating");
				dnbCreditRatingFilter.Category = creditScoresCategory;
				dnbCreditRatingFilter.SubGroup = subGroup;
				filters.AddCustomFilter(dnbCreditRatingFilter);

				var latePaymentRiskFilter = new ModuleNumberRangeFilter("Late Payment Risk", GetLatePaymentRiskQuery);
				latePaymentRiskFilter.PropertyType = ZCalcEditPropertyType.Int;
				latePaymentRiskFilter.MultilingualDescription = ResString.GetMultilingualString("1DFDE74B-570B-406D-9ED7-4992D07A3387", "Late Payment Risk");
				latePaymentRiskFilter.Category = creditScoresCategory;
				latePaymentRiskFilter.SubGroup = subGroup;
				latePaymentRiskFilter.MinValue = 101;
				latePaymentRiskFilter.MaxValue = 799;
				filters.AddCustomFilter(latePaymentRiskFilter);

				var failureRiskFilter = new ModuleNumberRangeFilter("Failure Risk", GetFailureRiskQuery);
				failureRiskFilter.PropertyType = ZCalcEditPropertyType.Int;
				failureRiskFilter.MultilingualDescription = ResString.GetMultilingualString("16AC4C47C-E2F5-4B40-A54F-42110AF6BA40", "Failure Risk");
				failureRiskFilter.Category = creditScoresCategory;
				failureRiskFilter.SubGroup = subGroup;
				failureRiskFilter.MinValue = 1001;
				failureRiskFilter.MaxValue = 1999;
				filters.AddCustomFilter(failureRiskFilter);
			}
		}

		ZQuery GetLatePaymentRiskQuery(INumericZType value1, INumericZType value2)
		{
			return GetQueryWithModuleNumberRangerFilter(OrgMiscServSchema.OM_CCLatePaymentScore, value1, value2);
		}

		ZQuery GetFailureRiskQuery(INumericZType value1, INumericZType value2)
		{
			return GetQueryWithModuleNumberRangerFilter(OrgMiscServSchema.OM_CCFailureRiskScore, value1, value2);
		}

		protected class CreditScoresSubGroup : ModuleFilterSubGroup
		{
			string[] AvailableCountriesForCreditReport => availableCountriesForCreditReport ?? (availableCountriesForCreditReport = OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.Value.Cast<CreditReportItem>().Where(x => x.CountryEnabledForOrganisation).Select(x => x.CountryCode.ToString()).ToArray());
			string[] availableCountriesForCreditReport;

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(OrgHeader));

				var subQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH);
				subQuery.AddToFilter(filter);

				result.AddSubQuery(subQuery, JoinCondition.And);
				if (AvailableCountriesForCreditReport.Length > 0)
				{
					result.AddToFilter(GetOrganisationCountryQuery());
				}

				return result;
			}

			[SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "use Like comparison")]
			ZQuery GetOrganisationCountryQuery()
			{
				var query = new ZQuery();
				foreach (var countryCode in AvailableCountriesForCreditReport)
				{
					query.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.Like, countryCode + "%");
				}

				return query;
			}
		}

		#endregion

		#region Lookups

		#region Branch Management Codes

		CodeDescriptionPairList BranchManagementCodeList
		{
			get { return AccountingMasterFilesRegistry.Instance.BranchManagementCodes.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).GetActiveCodeDescriptionPairList(); }
		}

		#endregion

		#region Sales Product Types

		public CodeDescriptionPairList SalesProductTypes
		{
			get
			{
				return Factory.GetCachedValue("OrganisationFilterBusinessObject|SalesProductTypes", () =>
				{
					var result = new CodeDescriptionPairList();
					var products = Factory.Load<IOrgSalesProduct>(new ZQuery());
					foreach (var product in products)
					{
						result.AddPair(product.MP_Code, product.MP_NameMultilingual);
					}
					result.Sort();

					return result;
				});
			}
		}

		#endregion

		#region DropEditRelationship_List

		OrgCodeLists fOrgCodeLists;

		public virtual ReadOnlyCodeDescriptionPairList GetDropEditRelationship_List(string relationshipCode)
		{
			if (fOrgCodeLists == null)
			{
				fOrgCodeLists = new OrgCodeLists();
			}

			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.CustomType);

			switch (relationshipCode)
			{
				case OrgConstants.FilterControl.DropEditRelationships.Code.CustomsCodeType:
					return fOrgCodeLists.CustomsCodes_List(GlbBranch.CurrentBranch.Country);

				case OrgConstants.FilterControl.DropEditRelationships.Code.GlobalRateTarriff:
					return fOrgCodeLists.CompanyTariffLevels_List(Factory, GlbCompany.CurrentCompany.PK);

				case OrgConstants.FilterControl.DropEditRelationships.Code.ARAcctRelationship:
					return Env.Registry.ReceivablesCategoryList;

				case OrgConstants.FilterControl.DropEditRelationships.Code.ARConsolidation:
				case OrgConstants.FilterControl.DropEditRelationships.Code.APConsolidation:
					return AccountingMasterFilesRegistry.Instance.ConsolidatedAccountingCategoryList.Value.ToReadOnlyCodeDescriptionPairList();

				case OrgConstants.FilterControl.DropEditRelationships.Code.ARStdInvoiceTerms:
				case OrgConstants.FilterControl.DropEditRelationships.Code.ARDisbInvoiceTerms:
					return new ARInvoiceTermsList();

				case OrgConstants.FilterControl.DropEditRelationships.Code.APPaymentTerms:
					return new APInvoiceTermsList();

				case OrgConstants.FilterControl.DropEditRelationships.Code.ARGrpImportCharges:
				case OrgConstants.FilterControl.DropEditRelationships.Code.ARGrpExportCharges:
					return OrgCodeLists.InvoiceLineGroupings_List;

				case OrgConstants.FilterControl.DropEditRelationships.Code.APAcctRelationship:
					return Env.Registry.PayablesCategoryList;

				case OrgConstants.FilterControl.DropEditRelationships.Code.CNRExportCategory:
					return Env.Registry.ExporterCategoryList;

				case OrgConstants.FilterControl.DropEditRelationships.Code.CNRINCOTerm:
					return new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms);

				case OrgConstants.FilterControl.DropEditRelationships.Code.FCLEquipment:
					return new FCLEquipmentNeededList();

				case OrgConstants.FilterControl.DropEditRelationships.Code.LCLEquipment:
				case OrgConstants.FilterControl.DropEditRelationships.Code.AirEquipment:
					return new LCLAIREquipmentNeededList();

				case OrgConstants.FilterControl.DropEditRelationships.Code.CNEImportCategory:
					return Env.Registry.ImporterCategoryList;

				case OrgConstants.FilterControl.DropEditRelationships.Code.CNEMergeCusLines:
					return OrgCodeLists.MergeInvoiceLinesBy_List;

				case OrgConstants.FilterControl.DropEditRelationships.Code.CNESendAirDocs:
				case OrgConstants.FilterControl.DropEditRelationships.Code.CNESendSeaDocs:
					return OrgCodeLists.SendImportDocsTo_List;

				case OrgConstants.FilterControl.DropEditRelationships.Code.FWDAgentCategory:
					return Env.Registry.AgentCategoryList;

				case OrgConstants.FilterControl.DropEditRelationships.Code.SRVUsagePreference:
					return Env.Registry.ServicesCategoryList;

				case OrgConstants.FilterControl.DropEditRelationships.Code.ADRType:
					return OrgCodeLists.AddressType_List(Factory);

				case OrgConstants.FilterControl.DropEditRelationships.Code.ADRLanguage:
					return new CodeDescriptionPairList(OLookUpEditType.Language);

				case OrgConstants.FilterControl.DropEditRelationships.Code.CNTType:
					return OrgCodeLists.ContactType_List;

				case OrgConstants.FilterControl.DropEditRelationships.Code.SALCategory:
					return Env.Registry.SalesCategoryList;

				case OrgConstants.FilterControl.DropEditRelationships.Code.SALClientSize:
					return Registry.Business.OrganisationsDataRegistry.Instance.ClientSizeList.Value;

				case OrgConstants.FilterControl.DropEditRelationships.Code.SALIndustryVertical:
				case OrgConstants.FilterControl.DropEditRelationships.Code.SALTradeLaneIndustryVertical:
					return Registry.Business.OrganisationsDataRegistry.Instance.IndustryVerticalTypes.Value.GetCodeDescriptionPairList();

				case OrgConstants.FilterControl.DropEditRelationships.Code.SALPeriodOfActivity:
				case OrgConstants.FilterControl.DropEditRelationships.Code.SALTradeLanePeriodOfActivity:
					return Registry.Business.OrganisationsDataRegistry.Instance.PeriodOfActivityTypes.Value.GetCodeDescriptionPairList();

				case OrgConstants.FilterControl.DropEditRelationships.Code.SALTerritory:
					return Env.Registry.SalesTerritoryList;

				case OrgConstants.FilterControl.DropEditRelationships.Code.SALOutlook:
					return Env.Registry.SalesGrowthOutlookList;

				case OrgConstants.FilterControl.DropEditRelationships.Code.SALCompActivity:
					return Env.Registry.CompetitorActivityList;

				case OrgConstants.FilterControl.DropEditRelationships.Code.SALTradeMode:
					return new CodeDescriptionPairList(OLookUpEditType.SalesMode);

				case OrgConstants.FilterControl.DropEditRelationships.Code.SALTransportMode:
					return SalesProductTypes;

				case OrgConstants.FilterControl.DropEditRelationships.Code.SALTradeStatus:
					return new OrgTradeDetail.TradeLaneStatus();

				case OrgConstants.FilterControl.DropEditRelationships.Code.SALAirCosts:
				case OrgConstants.FilterControl.DropEditRelationships.Code.SALLCLCosts:
				case OrgConstants.FilterControl.DropEditRelationships.Code.SALTEUCosts:
				case OrgConstants.FilterControl.DropEditRelationships.Code.SALWarehouseCosts:
				case OrgConstants.FilterControl.DropEditRelationships.Code.SALOtherCosts:
					return Env.Registry.SalesEffectOnCostList;

				case OrgConstants.FilterControl.DropEditRelationships.Code.SALClientRelationship:
				case OrgConstants.FilterControl.DropEditRelationships.Code.SALDesireToRemain:
				case OrgConstants.FilterControl.DropEditRelationships.Code.SALEaseToPoach:
				case OrgConstants.FilterControl.DropEditRelationships.Code.SALElectronicIntegration:
					return OrgCodeLists.ZeroToTen_List;

				case OrgConstants.FilterControl.DropEditRelationships.Code.CMPServiceType:
					return Env.Registry.CompetitorActivityList;

				case OrgConstants.FilterControl.DropEditRelationships.Code.CMPSellStyle:
					return Env.Registry.SalesStyleList;

				case OrgConstants.FilterControl.DropEditRelationships.Code.CMPCategory:
					return Env.Registry.CompetitorCategoryList;

				case OrgConstants.FilterControl.DropEditRelationships.Code.RMStaffRole:
					return Env.Registry.OrgStaffMemberAssignmentRoles;

				case OrgConstants.FilterControl.DropEditRelationships.Code.RMStaffDepartment:
					return RMDepartmentCodes;

				case OrgConstants.FilterControl.DropEditRelationships.Code.MCRCarrierCategory:
					return Env.Registry.CarrierCategoryList;

				default:
					return list;
			}
		}

		#endregion

		#region Language_List

		public CodeDescriptionPairList Language_List => new CodeDescriptionPairList(OLookUpEditType.Language);

		#endregion

		#region  Category_List

		public CodeDescriptionPairList Category_List
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				list.AddRange(new CodeDescriptionPairList(OLookUpEditType.OrgHeaderCategory));
				return list;
			}
		}

		#endregion

		#region AgentStatusList

		public CodeDescriptionPairList AgentStatus_List
		{
			get
			{
				if (fAgentStatus_List == null)
				{
					fAgentStatus_List = new AgentStatusList();
				}
				return fAgentStatus_List;
			}
		}
		AgentStatusList fAgentStatus_List;

		#endregion

		#region Carrier Type List

		public CodeDescriptionPairList CarrierType_List
		{
			get
			{
				if (fCarrierType_List == null)
				{
					fCarrierType_List = new CarrierOrForwarderType();
				}
				return fCarrierType_List;
			}
		}
		CarrierOrForwarderType fCarrierType_List;

		#endregion

		#region AccountType_List

		public CodeDescriptionPairList AccountType_List
		{
			get
			{
				var list = new CodeDescriptionPairList();

				list.AddPair(OrgConstants.FilterControl.AccountType.Code.GlobalAccount, Res.GetString("Common|AccountTypeList|GlobalAccount", "Global Account Only"));
				list.AddPair(OrgConstants.FilterControl.AccountType.Code.NonGlobalAccount, Res.GetString("Common|AccountTypeList|NonGlobalAccount", "Non-Global Account Only"));
				list.AddPair(OrgConstants.FilterControl.AccountType.Code.NationalAccount, Res.GetString("Common|AccountTypeList|NationalAccount", "National Account Only"));
				list.AddPair(OrgConstants.FilterControl.AccountType.Code.NonNationalAccount, Res.GetString("Common|AccountTypeList|NonNationalAccount", "Non-National Account Only"));
				list.AddPair(OrgConstants.FilterControl.AccountType.Code.TemporaryAccount, Res.GetString("Common|AccountTypeList|TemporaryAccount", "Temporary Account Only"));
				list.AddPair(OrgConstants.FilterControl.AccountType.Code.NonTemporaryAccount, Res.GetString("Common|AccountTypeList|NonTemporaryAccount", "Non-Temporary Account Only"));

				return list;
			}
		}

		#endregion

		#region RatesSecurityList

		public CodeDescriptionPairList RatesSecurityList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();

				list.AddPair(OrgConstants.FilterControl.RatesSecurity.Code.All, OrgConstants.FilterControl.RatesSecurity.Description.All);
				list.AddRange(OrganisationsDataRegistry.Instance.RatesSecurity.Value);

				return list;
			}
		}

		#endregion

		#region ExternalValidationStatusList

		public CodeDescriptionPairList ExternalValidationStatusList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(OrgConstants.FilterControl.ExternalValidationStatus.Code.Passed, OrgConstants.FilterControl.ExternalValidationStatus.Description.Passed);
				list.AddPair(OrgConstants.FilterControl.ExternalValidationStatus.Code.Failed, OrgConstants.FilterControl.ExternalValidationStatus.Description.Failed);
				list.AddPair(OrgConstants.FilterControl.ExternalValidationStatus.Code.NotCompleted, OrgConstants.FilterControl.ExternalValidationStatus.Description.NotCompleted);
				list.AddPair(OrgConstants.FilterControl.ExternalValidationStatus.Code.NotRun, OrgConstants.FilterControl.ExternalValidationStatus.Description.NotRun);

				return list;
			}
		}

		#endregion

		#region SalesRepAssigned_List

		public CodeDescriptionPairList SalesRepAssigned_List
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();

				list.AddPair(OrgConstants.FilterControl.SalesRepAssigned.Code.All, Res.GetString("Organisation|SalesRepAssignedList|All", "Show All"));
				list.AddPair(OrgConstants.FilterControl.SalesRepAssigned.Code.Assigned, Res.GetString("Organisation|SalesRepAssignedList|Assigned", "Show Assigned Only"));
				list.AddPair(OrgConstants.FilterControl.SalesRepAssigned.Code.NotAssigned, Res.GetString("Organisation|SalesRepAssignedList|NotAssigned", "Show Not Assigned Only"));

				return list;
			}
		}

		#endregion

		#region Main Details Filters

		public void AddForARAcctGroup(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetCompanyDataQueryWith2Fields(OrgCompanyDataSchema.OB_IsDebtor, OrgCompanyDataSchema.OB_OJ_ARDebtorGroup, value));
		}

		public void AddForARCurrency(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetCompanyDataQueryWith2Fields(OrgCompanyDataSchema.OB_IsDebtor, OrgCompanyDataSchema.OB_RX_NKARDDefltCurrency, value));
		}

		public void AddForAPAcctGroup(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetCompanyDataQueryWith2Fields(OrgCompanyDataSchema.OB_IsCreditor, OrgCompanyDataSchema.OB_OG_APCreditorGroup, value));
		}

		public void AddForAPBankAccount(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetCompanyDataQueryWith2Fields(OrgCompanyDataSchema.OB_IsCreditor, OrgCompanyDataSchema.OB_AB_APDefaultBankAccount, value));
		}

		public void AddForAPChargeCode(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetCompanyDataQueryWith2Fields(OrgCompanyDataSchema.OB_IsCreditor, OrgCompanyDataSchema.OB_AC_APDefaultChargeCode, value));
		}

		public void AddForCNRCountry(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetMiscServQuery(OrgHeaderSchema.OH_IsConsignor, OrgMiscServSchema.OM_RN_NKEXDefaultCntryOfOrigin, value));
		}

		public void AddForCNRCurrency(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetMiscServQuery(OrgHeaderSchema.OH_IsConsignor, OrgMiscServSchema.OM_RX_NKEXDefCurrency, value));
		}

		public void AddForCNRSeaCartageCordinator(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetStaffAssignmentsQueryInner("TES", StaffAssignmentRoles.Codes.CartageCoordinator, value));
		}

		public void AddForCNRAirCartageCordinator(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetStaffAssignmentsQueryInner("TEA", StaffAssignmentRoles.Codes.CartageCoordinator, value));
		}

		public void AddForCNRSeaCustomerServiceRep(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetStaffAssignmentsQueryInner("FES", StaffAssignmentRoles.Codes.CustomerServiceRep, value));
		}

		public void AddForCNRAirCustomerServiceRep(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetStaffAssignmentsQueryInner("FEA", StaffAssignmentRoles.Codes.CustomerServiceRep, value));
		}

		public void AddForCNESeaCartageCordinator(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetStaffAssignmentsQueryInner("TIS", StaffAssignmentRoles.Codes.CartageCoordinator, value));
		}

		public void AddForCNEAirCartageCordinator(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetStaffAssignmentsQueryInner("TIA", StaffAssignmentRoles.Codes.CartageCoordinator, value));
		}

		public void AddForCNESeaCustomerServiceRep(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetStaffAssignmentsQuery(OrgHeaderSchema.OH_IsConsignee, "FIS", StaffAssignmentRoles.Codes.CustomerServiceRep, value, @operator));
		}

		public void AddForCNEAirCustomerServiceRep(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetStaffAssignmentsQueryInner("FIA", StaffAssignmentRoles.Codes.CustomerServiceRep, value));
		}

		public void AddForFWDCurrency(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetMiscServQuery(OrgHeaderSchema.OH_IsForwarder, OrgMiscServSchema.OM_RX_NKFWDefCurrency, value));
		}

		public void AddForSALOverallRep(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetStaffAssignmentsQueryInner("ALL", StaffAssignmentRoles.Codes.SalesRep, value));
		}

		public void AddForSALImportAirRep(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetStaffAssignmentsQueryInner("FIA", StaffAssignmentRoles.Codes.SalesRep, value));
			query.AddToFilter(GetStaffAssignmentsQueryInner("ALL", StaffAssignmentRoles.Codes.SalesRep, value), JoinCondition.Or);
		}

		public void AddForSALImportSeaRep(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetStaffAssignmentsQueryInner("FIS", StaffAssignmentRoles.Codes.SalesRep, value));
			query.AddToFilter(GetStaffAssignmentsQueryInner("ALL", StaffAssignmentRoles.Codes.SalesRep, value), JoinCondition.Or);
		}

		public void AddForSALExportAirRep(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetStaffAssignmentsQueryInner("FEA", StaffAssignmentRoles.Codes.SalesRep, value));
			query.AddToFilter(GetStaffAssignmentsQueryInner("ALL", StaffAssignmentRoles.Codes.SalesRep, value), JoinCondition.Or);
		}

		public void AddForSALExportSeaRep(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetStaffAssignmentsQueryInner("FES", StaffAssignmentRoles.Codes.SalesRep, value));
			query.AddToFilter(GetStaffAssignmentsQueryInner("ALL", StaffAssignmentRoles.Codes.SalesRep, value), JoinCondition.Or);
		}

		public void AddForSALWarehousingRep(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetStaffAssignmentsQueryInner("WAR", StaffAssignmentRoles.Codes.SalesRep, value));
			query.AddToFilter(GetStaffAssignmentsQueryInner("ALL", StaffAssignmentRoles.Codes.SalesRep, value), JoinCondition.Or);
		}

		protected class GlobalRateTariffSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZQuery query = new ZQuery();

				ZDBOnlyQuery queryToAdd = new ZDBOnlyQuery(typeof(OrgHeader));
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgRateTariffLevel), OrgRateTariffLevelSchema.P7_OH);
				subQuery.AddToFilter(filter);
				queryToAdd.AddSubQuery(subQuery, JoinCondition.And);
				query.AddToFilter(queryToAdd);

				return query;
			}
		}

		public void AddForARStdInvoiceTerms(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetARInvoiceTermsQuery(value));
		}

		public void AddForARDisbInvoiceTerms(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetARInvoiceTermsQuery(value, OrgARTermsLookups.InvoiceTypes.DSB.Code));
		}

		protected class ADRTypeSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZQuery();

				ZDBOnlyQuery queryToAdd = new ZDBOnlyQuery(typeof(OrgHeader));
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH);
				ZDBOnlySubQuery subQueryCapability = new ZDBOnlySubQuery(typeof(OrgAddressCapability), OrgAddressCapabilitySchema.PZ_OA);
				subQueryCapability.AddToFilter(filter);
				subQuery.AddSubQuery(subQueryCapability, JoinCondition.And);
				queryToAdd.AddSubQuery(subQuery, JoinCondition.And);

				query.AddToFilter(queryToAdd);

				return query;
			}
		}

		public void AddForSALClientRelationship(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZByte result;
			if (ZByte.TryParse((ZString)value, out result))
			{
				query.AddToFilter(OrgMiscServSchema.OM_CMOverallClientRelation, SQLComparisonOperator.Equal, result);
			}
		}

		public void AddForSALDesireToRemain(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZByte result;
			if (ZByte.TryParse((ZString)value, out result))
			{
				query.AddToFilter(OrgMiscServSchema.OM_CMClientsDesireToRemain, SQLComparisonOperator.Equal, result);
			}
		}

		public void AddForSALEaseToPoach(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZByte result;
			if (ZByte.TryParse((ZString)value, out result))
			{
				query.AddToFilter(OrgMiscServSchema.OM_CMEaseClientCanBePoached, SQLComparisonOperator.Equal, result);
			}
		}

		public void AddForSALElectronicIntegration(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZByte result;
			if (ZByte.TryParse((ZString)value, out result))
			{
				query.AddToFilter(OrgMiscServSchema.OM_CMAmountOfElectronicIntegration, SQLComparisonOperator.Equal, result);
			}
		}

		#region UNLOCOFilter delegates

		public void AddForMainUNLOCO(ZQuery query, object value)
		{
			query.AddToFilter(LocationHelper.GetLocationFilter(Factory, (ZString)value, OrgHeaderSchema.OH_RL_NKClosestPort, typeof(OrgHeader)));
		}

		protected class SALTradeLanesSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZQuery();

				var orgQuery = new ZDBOnlyQuery(typeof(OrgHeader));

				var supplierOrgSubQuery = new ZDBOnlySubQuery(typeof(OrgSales), OrgSalesSchema.OW_OH_Supplier);
				OrgSalesCollection.AddIsNotHiddenFilter(supplierOrgSubQuery);
				supplierOrgSubQuery.AddToFilter(OrgSalesSchema.OW_IsTraded, ZBool.False);
				var supplierDestinationSubQuery = new ZDBOnlySubQuery(typeof(ViewLocation), OrgSalesSchema.OW_DestinationID);
				supplierDestinationSubQuery.AddToFilter(filter);
				supplierOrgSubQuery.AddSubQuery(supplierDestinationSubQuery, JoinCondition.And);

				var buyerOrgSubQuery = new ZDBOnlySubQuery(typeof(OrgSales), OrgSalesSchema.OW_OH_Buyer);
				OrgSalesCollection.AddIsNotHiddenFilter(buyerOrgSubQuery);
				buyerOrgSubQuery.AddToFilter(OrgSalesSchema.OW_IsTraded, ZBool.False);
				var buyerOriginSubQuery = new ZDBOnlySubQuery(typeof(ViewLocation), OrgSalesSchema.OW_OriginID);
				buyerOriginSubQuery.AddToFilter(filter);
				buyerOrgSubQuery.AddSubQuery(buyerOriginSubQuery, JoinCondition.And);

				orgQuery.AddSubQuery(supplierOrgSubQuery, JoinCondition.Or);
				orgQuery.AddSubQuery(buyerOrgSubQuery, JoinCondition.Or);

				orgQuery.AddToFilter(JoinCondition.And, OrgHeaderSchema.OH_IsSalesLead, ZBool.True);

				query.AddToFilter(orgQuery);

				return query;
			}
		}

		protected class CMPTradeLanesSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZQuery();

				ZDBOnlyQuery competitorQuery = new ZDBOnlyQuery(typeof(OrgHeader));
				competitorQuery.AddToFilter(OrgHeaderSchema.OH_IsCompetitor, ZBool.True);

				ZDBOnlySubQuery tradeProspectSubQuery = new ZDBOnlySubQuery(typeof(OrgTradeProspect), OrgTradeProspectSchema.PAP_OH_Competitor);
				ZDBOnlySubQuery tradeDetailSubQuery = new ZDBOnlySubQuery(typeof(OrgTradeDetail), OrgTradeDetailSchema.PK);
				ZDBOnlySubQuery salesSubQuery = new ZDBOnlySubQuery(typeof(OrgSales), OrgSalesSchema.PK);
				OrgSalesCollection.AddIsNotHiddenFilter(salesSubQuery);
				salesSubQuery.AddToFilter(OrgSalesSchema.OW_IsTraded, ZBool.False);

				var originSubQuery = new ZDBOnlySubQuery(typeof(ViewLocation), OrgSalesSchema.OW_OriginID);
				originSubQuery.AddToFilter(filter);

				var destinationSubQuery = new ZDBOnlySubQuery(typeof(ViewLocation), OrgSalesSchema.OW_DestinationID);
				destinationSubQuery.AddToFilter(filter);

				ZDBOnlyQuery salesOriginDestinationQuery = new ZDBOnlyQuery(typeof(OrgSales));
				salesOriginDestinationQuery.AddSubQuery(originSubQuery, JoinCondition.Or);
				salesOriginDestinationQuery.AddSubQuery(destinationSubQuery, JoinCondition.Or);
				salesSubQuery.AddToFilter(salesOriginDestinationQuery, JoinCondition.And);

				tradeDetailSubQuery.AddSubQuery(OrgTradeDetailSchema.PA_OW, salesSubQuery, JoinCondition.And);
				tradeProspectSubQuery.AddSubQuery(OrgTradeProspectSchema.PAP_PA, tradeDetailSubQuery, JoinCondition.And);
				competitorQuery.AddSubQuery(tradeProspectSubQuery, JoinCondition.And);
				query.AddToFilter(competitorQuery);

				return query;
			}
		}

		#endregion

		#region RelationshipGuidList

		public virtual IBusinessObjectCollection GetRelationshipGuidList(string guidRelationshipFilterCode)
		{
			switch (guidRelationshipFilterCode)
			{
				case OrgConstants.FilterControl.GuidRelationships.Code.APAcctGroup:
					return CreditorGroups;

				case OrgConstants.FilterControl.GuidRelationships.Code.ARAcctGroup:
					return DebtorGroups;

				case OrgConstants.FilterControl.GuidRelationships.Code.APBankAccount:
					return BankAccounts;

				case OrgConstants.FilterControl.GuidRelationships.Code.APChargeCode:
					return ChargeCodes;

				case OrgConstants.FilterControl.GuidRelationships.Code.CNRCountry:
					return Countries;

				case OrgConstants.FilterControl.GuidRelationships.Code.ARCurrency:
				case OrgConstants.FilterControl.GuidRelationships.Code.CNRCurrency:
				case OrgConstants.FilterControl.GuidRelationships.Code.FWDCurrency:
					return Currencies;

				case OrgConstants.FilterControl.GuidRelationships.Code.SALOverallRep:
				case OrgConstants.FilterControl.GuidRelationships.Code.SALImportAirRep:
				case OrgConstants.FilterControl.GuidRelationships.Code.SALImportSeaRep:
				case OrgConstants.FilterControl.GuidRelationships.Code.SALExportAirRep:
				case OrgConstants.FilterControl.GuidRelationships.Code.SALExportSeaRep:
				case OrgConstants.FilterControl.GuidRelationships.Code.SALWarehousingRep:
				case OrgConstants.FilterControl.GuidRelationships.Code.CNRSeaCartageCordinator:
				case OrgConstants.FilterControl.GuidRelationships.Code.CNRAirCartageCordinator:
				case OrgConstants.FilterControl.GuidRelationships.Code.CNRSeaCustomerServiceRep:
				case OrgConstants.FilterControl.GuidRelationships.Code.CNRAirCustomerServiceRep:
				case OrgConstants.FilterControl.GuidRelationships.Code.CNESeaCustomerServiceRep:
				case OrgConstants.FilterControl.GuidRelationships.Code.CNEAirCustomerServiceRep:
				case OrgConstants.FilterControl.GuidRelationships.Code.CNESeaCartageCordinator:
				case OrgConstants.FilterControl.GuidRelationships.Code.CNEAirCartageCordinator:
				case OrgConstants.FilterControl.GuidRelationships.Code.SALOverallAccountManager:
				case OrgConstants.FilterControl.GuidRelationships.Code.RMStaff:
					return Staff;

				case OrgConstants.FilterControl.GuidRelationships.Code.RMStaffCompany:
					return Companies;

				default:
					return OrgHeaders;
			}
		}

		#endregion

		#region Locations Collections

		public ViewLocationCollection ViewLocations
		{
			get
			{
				return new ViewLocationCollection(Factory);
			}
		}

		public LocationCollection Locations
		{
			get { return new LocationCollection(Factory, Env.Security.OrganisationAllowSearchOutsideLoginCountry.IsAllowed); }
		}

		#endregion

		#region OrgHeaders

		protected OrganisationsFindBoxCollection fOrgHeaders;
		public OrganisationsFindBoxCollection OrgHeaders
		{
			get
			{
				if (fOrgHeaders == null)
				{
					fOrgHeaders = new OrganisationsFindBoxCollection(Factory);
				}
				return fOrgHeaders;
			}
		}

		#endregion

		#region OrgGlobalCreditGroups

		public GlobalCreditGroupCollection GlobalCreditGroups => orgGlobalCreditGroups ?? (orgGlobalCreditGroups = new GlobalCreditGroupCollection(Factory));
		GlobalCreditGroupCollection orgGlobalCreditGroups;

		protected ZQuery GetGlobalCreditGroup(ZGuid value)
		{
			var query = new ZDBOnlyQuery(typeof(OrgHeader));

			var subquery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH);
			subquery.AddToFilter(OrgMiscServSchema.OM_OH_ARGlobalCreditGroup, SQLComparisonOperator.Equal, value);

			query.AddSubQuery(OrgHeaderSchema.PK, subquery, JoinCondition.And);

			return query;
		}

		#endregion

		#region Consignors

		public OrganisationsFindBoxCollection Consignors
		{
			get
			{
				return new ConsignorCollection(Factory);
			}
		}

		#endregion

		#region Consignees

		public OrganisationsFindBoxCollection Consignees
		{
			get
			{
				return new ConsigneeCollection(Factory);
			}
		}

		#endregion

		#region Competitors

		public BrokerCollection CustomsCompetitors
		{
			get
			{
				if (customsCompetitors == null)
				{
					customsCompetitors = new BrokerCollection(Factory);
				}
				return customsCompetitors;
			}
		}

		BrokerCollection customsCompetitors;

		public ForwarderCollection ForwardingCompetitors
		{
			get
			{
				if (forwardingCompetitors == null)
				{
					forwardingCompetitors = new ForwarderCollection(Factory);
				}
				return forwardingCompetitors;
			}
		}

		ForwarderCollection forwardingCompetitors;

		#endregion

		#region BankAccounts

		protected AccBankAccountCollection fBankAccounts;
		public AccBankAccountCollection BankAccounts
		{
			get
			{
				if (fBankAccounts == null)
				{
					GlbBranch branch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
					fBankAccounts = new AccBankAccountCollection(Factory, branch);
				}
				return fBankAccounts;
			}
		}

		#endregion

		#region ChargeCodes

		protected AccChargeCodeCollection fChargeCodes;
		public AccChargeCodeCollection ChargeCodes
		{
			get
			{
				if (fChargeCodes == null)
				{
					fChargeCodes = new AccChargeCodeCollection(Factory, new ZQuery(AccChargeCodeSchema.AC_DepartmentFilterList, SQLComparisonOperator.Contains, "ALL"));
				}
				return fChargeCodes;
			}
		}

		#endregion

		#region Countries

		protected RefCountryCollection fCountries;
		public RefCountryCollection Countries
		{
			get
			{
				if (fCountries == null)
				{
					fCountries = new RefCountryCollection(Factory);
				}
				return fCountries;
			}
		}

		#endregion

		#region Departments

		public CodeDescriptionPairList RMDepartmentCodes
		{
			get { return OrgStaffAssignmentsLookupsImplementer.Get(Factory).DepartmentCodes; }
		}

		#endregion

		#region CreditorGroups

		protected OrgCreditorGroupCollection fCreditorGroups;
		public OrgCreditorGroupCollection CreditorGroups
		{
			get
			{
				if (fCreditorGroups == null)
				{
					fCreditorGroups = new OrgCreditorGroupCollection(Factory);
				}
				return fCreditorGroups;
			}
		}

		#endregion

		#region Currencies

		protected RefCurrencyCollection fCurrencies;
		public RefCurrencyCollection Currencies
		{
			get
			{
				if (fCurrencies == null)
				{
					fCurrencies = new RefCurrencyCollection(Factory);
				}
				return fCurrencies;
			}
		}

		#endregion

		#region DebtorGroups

		protected OrgDebtorGroupCollection fDebtorGroups;
		public OrgDebtorGroupCollection DebtorGroups
		{
			get
			{
				if (fDebtorGroups == null)
				{
					fDebtorGroups = new OrgDebtorGroupCollection(Factory);
				}
				return fDebtorGroups;
			}
		}

		#endregion

		#region Staff

		protected GlbStaffCollection fStaff;
		public GlbStaffCollection Staff
		{
			get
			{
				if (fStaff == null)
				{
					fStaff = new GlbStaffCollection(Factory);
				}
				return fStaff;
			}
		}

		#endregion

		#region Company

		protected GlbCompanyCollection companies;
		public GlbCompanyCollection Companies
		{
			get { return companies ?? (companies = new GlbCompanyCollection(Factory)); }
		}

		#endregion

		#region Commodities

		public RefCommodityCodeCollection Commodities
		{
			get { return commodities ?? (commodities = new RefCommodityCodeCollection(Factory)); }
		}
		protected RefCommodityCodeCollection commodities;

		#endregion

		#endregion

		#region Known Shipper List

		CodeDescriptionPairList KnownShipperList => SupplyChainSecurityConfiguration.KnownShipperFilterList;

		#endregion

		#region Is Foreign Operator Filter Options

		public CodeDescriptionPairList IsForeignOperatorFilterOptions
		{
			get
			{
				var list = new CodeDescriptionPairList();

				list.AddPair(OrgConstants.FilterControl.IsForeignOperator.Code.Yes, Res.GetString("MasterFiles|IsForeignOperator|ForeignOperator", "Foreign Operator"));
				list.AddPair(OrgConstants.FilterControl.IsForeignOperator.Code.No, Res.GetString("MasterFiles|IsForeignOperator|NonForeignOperator", "Non-Foreign Operator"));
				list.AddPair(OrgConstants.FilterControl.IsForeignOperator.Code.All, Res.GetString("MasterFiles|IsForeignOperator|All", "All"));

				return list;
			}
		}

		#endregion

		#region Organisation Type Filter Category

		protected FilterCategory OrgTypeCategory
		{
			get
			{
				return organisationTypeCategory;
			}
		}

		#endregion

		#endregion

		#region Captions

		protected ZString RelatedConsigneesText = Res.GetString("MasterFiles|OrganisationFilter|RelatedConsignor", "Related Consignor:");
		protected ZString RelatedConsignorsText = Res.GetString("MasterFiles|OrganisationFilter|RelatedConsignee", "Related Consignee:");
		protected ZString SalesClientText = Res.GetString("MasterFiles|OrganisationFilter|SalesClientType", "Sales Client Type:");
		protected ZString InvoiceNumberText = Res.GetString("MasterFiles|OrganisationFilter|InvoiceNumber", "Invoice Number:");
		protected ZString AgentStatusText = Res.GetString("MasterFiles|OrganisationFilter|AgentStatus", "Agent Status:");

		#endregion

		#region Delegates

		#region Sales Commodity Delegates

		protected ZQuery GetMainImportCommodityFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(GetMiscServQuery(OrgHeaderSchema.OH_IsSalesLead, OrgMiscServSchema.OM_RH_NKCMMainImportCmdty, value));
			return query;
		}

		protected ZQuery GetMainExportCommodityFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(GetMiscServQuery(OrgHeaderSchema.OH_IsSalesLead, OrgMiscServSchema.OM_RH_NKCMMainExportCmdty, value));
			return query;
		}

		protected ZQuery GetRMStaffRoleFilter(ZString value)
		{
			var query = new ZQuery();
			query.AddToFilter(new ZQuery(OrgStaffAssignmentsSchema.O8_Role, value));
			return query;
		}

		protected ZQuery GetRMStaffFilter(ZGuid value)
		{
			var query = new ZQuery();
			query.AddToFilter(new ZQuery(GlbStaffSchema.PK, value));
			return query;
		}

		protected ZQuery GetRMStaffDepartmentFilter(ZString value)
		{
			var query = new ZQuery();
			query.AddToFilter(new ZQuery(OrgStaffAssignmentsSchema.O8_Department, value));
			return query;
		}

		protected ZQuery GetRMStaffCompanyFilter(ZGuid value)
		{
			var query = new ZQuery();
			query.AddToFilter(new ZQuery(GlbCompanySchema.PK, value));
			return query;
		}

		#endregion

		#region Trade Lane Related Delegates

		#region Trade Mode

		protected ZQuery GetSALTradeModeFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			AddForSALTradeMode(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		public void AddForSALTradeMode(ZQuery query1, SQLComparisonOperator @operator, object value)
		{
			ZQuery result = null;
			if (value.Equals(Core.Constants.MovementCodes.Import))
			{
				var query = new ZDBOnlyQuery(typeof(OrgHeader));
				query.AddToFilter(OrgHeaderSchema.OH_IsSalesLead, ZBool.True);
				var subQuery = new ZDBOnlySubQuery(typeof(OrgSales), OrgSalesSchema.OW_OH_Buyer);
				OrgSalesCollection.AddIsNotHiddenFilter(subQuery);
				subQuery.AddToFilter(OrgSalesSchema.OW_IsTraded, ZBool.False);
				query.AddSubQuery(subQuery, JoinCondition.And);

				result = query;
			}
			else if (value.Equals(Core.Constants.MovementCodes.Export))
			{
				var query = new ZDBOnlyQuery(typeof(OrgHeader));
				query.AddToFilter(OrgHeaderSchema.OH_IsSalesLead, ZBool.True);
				var subQuery = new ZDBOnlySubQuery(typeof(OrgSales), OrgSalesSchema.OW_OH_Supplier);
				OrgSalesCollection.AddIsNotHiddenFilter(subQuery);
				subQuery.AddToFilter(OrgSalesSchema.OW_IsTraded, ZBool.False);
				query.AddSubQuery(subQuery, JoinCondition.And);

				result = query;
			}
			else if (!value.Equals(""))
			{
				result = new ZQuery();
				result.IsNoResultQuery = true;
			}

			if (result != null)
			{
				query1.AddToFilter(result);
			}
		}

		#endregion

		#region Trade Lane Location

		protected ZQuery GetTradeLaneLocationFilter(ZString origin, ZString destination)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgSales));
			if (!origin.IsEmpty)
			{
				var originSubQuery = new ZDBOnlySubQuery(typeof(ViewLocation), OrgSalesSchema.OW_OriginID);
				originSubQuery.AddToFilter(ViewLocationSchema.VLO_Code, SQLComparisonOperator.StartsWith, origin);
				result.AddSubQuery(originSubQuery, JoinCondition.And);
			}
			if (!destination.IsEmpty)
			{
				var destinationSubQuery = new ZDBOnlySubQuery(typeof(ViewLocation), OrgSalesSchema.OW_DestinationID);
				destinationSubQuery.AddToFilter(ViewLocationSchema.VLO_Code, SQLComparisonOperator.StartsWith, destination);
				result.AddSubQuery(destinationSubQuery, JoinCondition.And);
			}
			return result;
		}

		#endregion

		#region Industry Vertical

		ZQuery GetTradeLaneIndustryVerticalFilter(ZString value)
		{
			var main = new ZDBOnlyQuery(typeof(OrgTradeDetail));
			var mainProspect = new ZDBOnlySubQuery(typeof(OrgTradeProspect), OrgTradeProspectSchema.PAP_PA);
			mainProspect.AddToFilter(OrgTradeProspectSchema.PAP_IndustryVertical, value);
			main.AddSubQuery(mainProspect, JoinCondition.And);

			var fallback = new ZDBOnlyQuery(typeof(OrgTradeDetail));
			var fallbackProspect = new ZDBOnlySubQuery(typeof(OrgTradeProspect), OrgTradeProspectSchema.PAP_PA);
			fallbackProspect.AddToFilter(OrgTradeProspectSchema.PAP_IndustryVertical, ZString.Empty);
			fallback.AddSubQuery(fallbackProspect, JoinCondition.And);

			var sqlParamter = new ZSqlParameterCollection();
			sqlParamter.Add("@OM_CMIndustryVertical", value, OrgMiscServSchema.OM_CMIndustryVertical);
			fallback.AddFilterAndZSQLParameterCollection(@"
PA_OW IN
(
	SELECT OW_PK
	FROM
		dbo.OrgSales
		JOIN dbo.OrgHeader ON OH_PK = COALESCE(OW_OH_Primary, OW_OH_Buyer, OW_OH_Supplier)
		JOIN dbo.OrgMiscServ ON OM_OH = OH_PK
	WHERE
		OM_CMIndustryVertical = @OM_CMIndustryVertical
)", sqlParamter);

			return new ZQuery(main, JoinCondition.Or, fallback);
		}

		#endregion

		#region Period of Activity

		ZQuery GetTradeLanePeriodOfActivityFilter(ZString value)
		{
			var main = new ZDBOnlyQuery(typeof(OrgTradeDetail));
			var mainProspect = new ZDBOnlySubQuery(typeof(OrgTradeProspect), OrgTradeProspectSchema.PAP_PA);
			mainProspect.AddToFilter(OrgTradeProspectSchema.PAP_PeriodOfActivity, value);
			main.AddSubQuery(mainProspect, JoinCondition.And);

			var fallback = new ZDBOnlyQuery(typeof(OrgTradeDetail));
			var fallbackProspect = new ZDBOnlySubQuery(typeof(OrgTradeProspect), OrgTradeProspectSchema.PAP_PA);
			fallbackProspect.AddToFilter(OrgTradeProspectSchema.PAP_PeriodOfActivity, ZString.Empty);
			fallback.AddSubQuery(fallbackProspect, JoinCondition.And);

			var sqlParamter = new ZSqlParameterCollection();
			sqlParamter.Add("@OM_CMPeriodOfActivity", value, OrgMiscServSchema.OM_CMPeriodOfActivity);
			fallback.AddFilterAndZSQLParameterCollection(@"
PA_OW IN
(
	SELECT OW_PK
	FROM
		dbo.OrgSales
		JOIN dbo.OrgHeader ON OH_PK = COALESCE(OW_OH_Primary, OW_OH_Buyer, OW_OH_Supplier)
		JOIN dbo.OrgMiscServ ON OM_OH = OH_PK
	WHERE
		OM_CMPeriodOfActivity = @OM_CMPeriodOfActivity
)", sqlParamter);

			return new ZQuery(main, JoinCondition.Or, fallback);
		}

		#endregion

		#region Trade Lane Details

		protected ZQuery GetTradeLaneCommodityFilter(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(OrgTradeDetail));
			var prospect = new ZDBOnlySubQuery(typeof(OrgTradeProspect), OrgTradeProspectSchema.PAP_PA);
			prospect.AddToFilter(OrgTradeProspectSchema.PAP_RH_NKCommodityCode, value);
			query.AddSubQuery(prospect, JoinCondition.And);
			return query;
		}

		protected ZQuery GetTradeLaneProductFilter(ZString value)
		{
			return new ZQuery(OrgSalesProductSchema.MP_Code, value);
		}

		protected ZQuery GetTradeLaneStatusFilter(ZString value)
		{
			return new ZQuery(OrgTradeDetailSchema.PA_Status, value);
		}

		#endregion

		#region Sub Group

		protected class TradeLaneSubGroup : ModuleFilterSubGroup
		{
			public TradeLaneSubGroup(ModuleFilterCollection filters)
				: base()
			{
				this.filters = filters;
			}
			readonly ModuleFilterCollection filters;

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZQuery result = new ZQuery(OrgHeaderSchema.OH_IsSalesLead, ZBool.True);

				ZDBOnlySubQuery salesBuyerSubQuery = new ZDBOnlySubQuery(typeof(OrgSales), OrgSalesSchema.OW_OH_Buyer);
				OrgSalesCollection.AddIsNotHiddenFilter(salesBuyerSubQuery);
				salesBuyerSubQuery.AddToFilter(OrgSalesSchema.OW_IsTraded, ZBool.False);
				ZDBOnlySubQuery salesSupplierSubQuery = new ZDBOnlySubQuery(typeof(OrgSales), OrgSalesSchema.OW_OH_Supplier);
				OrgSalesCollection.AddIsNotHiddenFilter(salesSupplierSubQuery);
				salesSupplierSubQuery.AddToFilter(OrgSalesSchema.OW_IsTraded, ZBool.False);
				salesBuyerSubQuery.AddToFilter(filter);
				salesSupplierSubQuery.AddToFilter(filter);

				ZDBOnlyQuery combinedSubQuery = new ZDBOnlyQuery(typeof(OrgHeader));

				ZString tradeMode = GetTradeModeFilterValue();
				if (tradeMode == Core.Constants.MovementCodes.Import)
				{
					combinedSubQuery.AddSubQuery(salesBuyerSubQuery, JoinCondition.And);
				}
				else if (tradeMode == Core.Constants.MovementCodes.Export)
				{
					combinedSubQuery.AddSubQuery(salesSupplierSubQuery, JoinCondition.And);
				}
				else
				{
					combinedSubQuery.AddSubQuery(salesBuyerSubQuery, JoinCondition.Or);
					combinedSubQuery.AddSubQuery(salesSupplierSubQuery, JoinCondition.Or);

					ZDBOnlySubQuery salesPrimarySubQuery = new ZDBOnlySubQuery(typeof(OrgSales), OrgSalesSchema.OW_OH_Primary);
					OrgSalesCollection.AddIsNotHiddenFilter(salesPrimarySubQuery);
					salesPrimarySubQuery.AddToFilter(OrgSalesSchema.OW_IsTraded, ZBool.False);
					salesPrimarySubQuery.AddToFilter(filter);
					combinedSubQuery.AddSubQuery(salesPrimarySubQuery, JoinCondition.Or);
				}

				result.AddToFilter(combinedSubQuery);

				return result;
			}

			protected ZString GetTradeModeFilterValue()
			{
				ZString result = ZString.Empty;

				if (filters[OrgConstants.FilterControl.DropEditRelationships.Description.SALTradeMode] != null
					&& filters[OrgConstants.FilterControl.DropEditRelationships.Description.SALTradeMode].IsActive)
				{
					result = ((ModuleTextFilter)filters[OrgConstants.FilterControl.DropEditRelationships.Description.SALTradeMode]).Property;
				}

				return result;
			}
		}

		protected class TradeLaneDetailSubGroup : ModuleFilterSubGroup
		{
			public TradeLaneDetailSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgSales));
				ZDBOnlySubQuery detailSubQuery = new ZDBOnlySubQuery(typeof(OrgTradeDetail), OrgTradeDetailSchema.PA_OW);
				detailSubQuery.AddToFilter(filter);
				result.AddSubQuery(detailSubQuery, JoinCondition.And);
				return result;
			}
		}

		protected class TradeLaneProductSubGroup : ModuleFilterSubGroup
		{
			public TradeLaneProductSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgSales));
				ZDBOnlySubQuery productSubQuery = new ZDBOnlySubQuery(typeof(IOrgSalesProduct), OrgSalesSchema.OW_MP_Product);
				productSubQuery.AddToFilter(filter);
				result.AddSubQuery(productSubQuery, JoinCondition.And);
				return result;
			}
		}

		#endregion

		#endregion

		#region Text Filter Delegates

		protected class OA_OHSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH);
				subQuery.AddToFilter(filter);

				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			}
		}

		protected class OK_OHSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
				subQuery.AddToFilter(filter);

				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			}
		}

		protected class P1_OHSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgBrandOrRelatedName), OrgBrandOrRelatedNameSchema.P1_OH);
				subQuery.AddToFilter(filter);

				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			}
		}

		protected class PU_OHSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgWebURL), OrgWebURLSchema.PU_OH);
				subQuery.AddToFilter(filter);

				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			}
		}

		protected class OC_OHSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.OC_OH);
				subQuery.AddToFilter(filter);

				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			}
		}

		protected class O7_O5SubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
				ZDBOnlySubQuery subQueryO5 = new ZDBOnlySubQuery(typeof(OrgAppointedAgentPorts), OrgAppointedAgentPortsSchema.O5_OH);
				ZDBOnlySubQuery subQueryO7 = new ZDBOnlySubQuery(typeof(OrgExclusiveGatewayService), OrgExclusiveGatewayServiceSchema.O7_O5_AgentPort);

				subQueryO7.AddToFilter(filter);
				subQueryO5.AddSubQuery(subQueryO7, JoinCondition.And);
				query.AddSubQuery(subQueryO5, JoinCondition.And);

				return query;
			}
		}

		protected class OH_RSL_ShippingLineSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(OrgHeader));

				var subQuery = new ZDBOnlySubQuery(typeof(RefShippingLine), RefShippingLineSchema.PK);
				subQuery.AddToFilter(filter);

				query.AddSubQuery(OrgHeaderSchema.OH_RSL_ShippingLine, subQuery, JoinCondition.And);
				return query;
			}
		}

		protected ZQuery GetBrandOrRelatedNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();
			query.AddToFilter(OrgBrandOrRelatedNameSchema.P1_RelatedName, comparisonOperator, value);

			return query;
		}

		protected ZDBOnlyQuery GetRegistrationNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery subQuery;
			ZDBOnlySubQuery notExistSubQuery;
			subQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
			subQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, comparisonOperator, value.SubstringSafe(0, OrgCusCodeSchema.OK_CustomsRegNo.MaxLength));
			notExistSubQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH, true);

			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				query.AddSubQuery(notExistSubQuery, JoinCondition.And);
			}
			else
			{
				query.AddSubQuery(subQuery, JoinCondition.And);
				if (comparisonOperator == SQLComparisonOperator.NotEqual || comparisonOperator == SQLComparisonOperator.NotContains || comparisonOperator == SQLComparisonOperator.DoesNotStartWith)
				{
					query.AddSubQuery(notExistSubQuery, JoinCondition.Or);
				}
			}
			return query;
		}

		protected ZDBOnlyQuery GetAddressesQuery(SchemaColumn orgAddressField, SQLComparisonOperator comparisonOperator, object value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH);
			subQuery.AddToFilter(orgAddressField, comparisonOperator, value);

			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		protected ZQuery GetAddressLinesQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();
			query.AddToFilter(OrgAddressSchema.OA_Address1, comparisonOperator, value.SubstringSafe(0, OrgAddressSchema.OA_Address1.MaxLength));
			query.AddToFilter(JoinCondition.Or, OrgAddressSchema.OA_Address2, comparisonOperator, value.SubstringSafe(0, OrgAddressSchema.OA_Address2.MaxLength));
			return query;
		}

		protected ZQuery GetAddress1Query(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();
			query.AddToFilter(OrgAddressSchema.OA_Address1, comparisonOperator, value.SubstringSafe(0, OrgAddressSchema.OA_Address1.MaxLength));
			return query;
		}

		protected ZQuery GetAddress2Query(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();
			query.AddToFilter(OrgAddressSchema.OA_Address2, comparisonOperator, value.SubstringSafe(0, OrgAddressSchema.OA_Address2.MaxLength));
			return query;
		}

		protected ZQuery GetAdditionalAddressQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(OrgAddress));
			var subQuery = new ZDBOnlySubQuery(typeof(OrgAddressAdditionalInfo), OrgAddressAdditionalInfoSchema.OAI_OA_Address);
			subQuery.AddToFilter(OrgAddressAdditionalInfoSchema.OAI_AdditionalInfo, comparisonOperator, value.SubstringSafe(0, OrgAddressAdditionalInfoSchema.OAI_AdditionalInfo.MaxLength));
			query.AddSubQuery(OrgAddressSchema.PK, subQuery, JoinCondition.And);
			return query;
		}

		protected ZQuery GetCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();
			query.AddToFilter(OrgAddressSchema.OA_CompanyNameOverride, comparisonOperator, value);
			return query;
		}

		protected ZQuery GetCityQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();
			query.AddToFilter(OrgAddressSchema.OA_City, comparisonOperator, value.SubstringSafe(0, OrgAddressSchema.OA_City.MaxLength));
			return query;
		}

		protected ZQuery GetWebQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgWebURL), OrgWebURLSchema.PU_OH);
			subQuery.AddToFilter(OrgWebURLSchema.PU_URL, comparisonOperator, value);

			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		protected ZQuery GetStateQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();
			query.AddToFilter(OrgAddressSchema.OA_State, comparisonOperator, value.SubstringSafe(0, OrgAddressSchema.OA_State.MaxLength));
			return query;
		}

		protected ZQuery GetPostCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();
			query.AddToFilter(OrgAddressSchema.OA_PostCode, comparisonOperator, value.SubstringSafe(0, OrgAddressSchema.OA_PostCode.MaxLength));
			return query;
		}

		protected ZQuery GetExclusiveGatewayServiceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();

			query.AddToFilter(
				OrgExclusiveGatewayServiceSchema.O7_RS_NKGatewayService,
				comparisonOperator,
				value.SubstringSafe(0, OrgExclusiveGatewayServiceSchema.O7_RS_NKGatewayService.MaxLength));

			return query;
		}

		protected ZQuery GetPhoneQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();
			query.AddToFilter(OrgAddressSchema.OA_Phone, comparisonOperator, value);
			return query;
		}

		protected ZQuery GetMobileQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();
			query.AddToFilter(OrgAddressSchema.OA_Mobile, comparisonOperator, value);
			return query;
		}

		protected ZQuery GetFaxQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH);
			subQuery.AddToFilter(OrgAddressSchema.OA_Fax, comparisonOperator, value);

			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		protected ZQuery GetEmailQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH);
			subQuery.AddToFilter(OrgAddressSchema.OA_Email, comparisonOperator, value);

			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		protected ZQuery GetContactNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.OC_OH);
			subQuery.AddToFilter(OrgContactSchema.OC_ContactName, comparisonOperator, value);

			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		protected ZQuery GetContactPhoneQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

			ZDBOnlySubQuery contactSubQuery = GetContactPhoneSubQuery(OrgContactSchema.OC_Phone, new[] { PhoneContactItemDescriptionList.Codes.Work, PhoneContactItemDescriptionList.Codes.Work2 }, comparisonOperator, value);
			query.AddSubQuery(contactSubQuery, JoinCondition.And);
			return query;
		}

		protected ZQuery GetContactMobileQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

			ZDBOnlySubQuery contactSubQuery = GetContactPhoneSubQuery(OrgContactSchema.OC_Mobile, new[] { PhoneContactItemDescriptionList.Codes.Mobile, PhoneContactItemDescriptionList.Codes.Mobile2 }, comparisonOperator, value);
			query.AddSubQuery(contactSubQuery, JoinCondition.And);
			return query;
		}

		protected ZQuery GetContactFaxQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

			ZDBOnlySubQuery contactSubQuery = GetContactPhoneSubQuery(OrgContactSchema.OC_Fax, new[] { PhoneContactItemDescriptionList.Codes.Fax }, comparisonOperator, value);
			query.AddSubQuery(contactSubQuery, JoinCondition.And);
			return query;
		}

		ZDBOnlySubQuery GetContactPhoneSubQuery(SchemaStringColumn orgContactPhoneColumn, string[] contactItemDescriptions, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var isNegativeOperator = comparisonOperator.IsNegativeSQLOperator();

			var contactSubQuery = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.OC_OH);
			var contactPhoneColumnQuery = new ZQuery(orgContactPhoneColumn, comparisonOperator, value.SubstringSafe(0, orgContactPhoneColumn.MaxLength));
			var contactItemSubQuery = new ZDBOnlySubQuery(typeof(OrgContactItem), OrgContactItemSchema.OI_OC, isNegativeOperator);
			contactItemSubQuery.AddToFilter(OrgContactItemSchema.OI_ContactItemType, OrgContactItemTypes.Codes.Phone);
			contactItemSubQuery.AddToFilter(OrgContactItemSchema.OI_Description, contactItemDescriptions);
			contactItemSubQuery.AddToFilter(OrgContactItemSchema.OI_Address, comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery(), value.SubstringSafe(0, OrgContactItemSchema.OI_Address.MaxLength));

			var joinCondition = isNegativeOperator ? JoinCondition.And : JoinCondition.Or;
			contactSubQuery.AddToFilter(contactPhoneColumnQuery, joinCondition);
			contactSubQuery.AddSubQuery(contactItemSubQuery, joinCondition);
			return contactSubQuery;
		}

		protected ZQuery GetContactEmailQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var isNegativeOperator = comparisonOperator.IsNegativeSQLOperator();

			var contactSubQuery = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.OC_OH);
			var primaryEmailQuery = new ZQuery(OrgContactSchema.OC_Email, comparisonOperator, value.SubstringSafe(0, OrgContactSchema.OC_Email.MaxLength));
			var emailItemSubQuery = new ZDBOnlySubQuery(typeof(OrgContactItem), OrgContactItemSchema.OI_OC, isNegativeOperator);
			emailItemSubQuery.AddToFilter(OrgContactItemSchema.OI_ContactItemType, OrgContactItemTypes.Codes.Email);
			emailItemSubQuery.AddToFilter(OrgContactItemSchema.OI_Address, comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery(), value.SubstringSafe(0, OrgContactItemSchema.OI_Address.MaxLength));

			var joinCondition = isNegativeOperator ? JoinCondition.And : JoinCondition.Or;
			contactSubQuery.AddToFilter(primaryEmailQuery, joinCondition);
			contactSubQuery.AddSubQuery(emailItemSubQuery, joinCondition);

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			query.AddSubQuery(contactSubQuery, JoinCondition.And);
			return query;
		}

		#region GetBranchManagementCodeQuery

		protected class BranchManagementCodeSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(OrgHeader));

				var orgCompanyDataSubQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
				orgCompanyDataSubQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);

				var branchManagementCodeQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK);
				branchManagementCodeQuery.AddToFilter(filter);
				orgCompanyDataSubQuery.AddSubQuery(OrgCompanyDataSchema.OB_GB_ControllingBranch, branchManagementCodeQuery, JoinCondition.And);

				result.AddSubQuery(orgCompanyDataSubQuery, JoinCondition.And);

				return result;
			}
		}

		ZQuery GetBranchManagementCodeQuery(ZString value)
		{
			var result = new ZQuery();

			if (!value.IsEmpty)
			{
				result.AddToFilter(GlbBranchSchema.GB_AccountingGroupCode, value);
			}

			return result;
		}

		ZQuery GetAccountSecurityNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(OrgCountryData), OrgCountryDataSchema.OV_OH_OrgHeader, IsNegativeSqlOperatorOrBlank(comparisonOperator));
			subQuery.AddToFilter(OrgCountryDataSchema.OV_RN_NKClientCountryRelation, Constants.CountryCodes.Canada);
			subQuery.AddFilterAndZSQLParameterCollection(GetFilterForSecurityNumber(comparisonOperator, value), null);
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		string GetFilterForSecurityNumber(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (IsNegativeSqlOperatorOrBlank(comparisonOperator))
			{
				comparisonOperator = GetNegatingSQLOperatorIfNotInSubquery(comparisonOperator);
			}

			var filterBuilder = new ZStringBuilder(OrgCountryDataSchema.OV_ImportCustomsDefaultAddInfo.Name);
			filterBuilder.Append((NoResString)" LIKE '%<AccountSecurityNumber>");

			if (comparisonOperator == SQLComparisonOperator.Equal)
			{
				filterBuilder.Append(value);
			}
			else if (comparisonOperator == SQLComparisonOperator.StartsWith)
			{
				filterBuilder.AppendFormat("{0}%", value);
			}
			else if (comparisonOperator == SQLComparisonOperator.Contains)
			{
				filterBuilder.AppendFormat("%{0}%", value);
			}
			else if (comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				filterBuilder.Append("%");
			}
			filterBuilder.Append("</AccountSecurityNumber>%' ");
			return filterBuilder.ToString();
		}

		static bool IsNegativeSqlOperatorOrBlank(SQLComparisonOperator comparisonOperator)
		{
			return comparisonOperator.IsNegativeSQLOperator() || comparisonOperator == SpecialComparisonOperator.IsBlank;
		}

		SQLComparisonOperator GetNegatingSQLOperatorIfNotInSubquery(SQLComparisonOperator comparisonOperator)
		{
			var result = comparisonOperator;
			if (result.IsNegativeSQLOperator())
			{
				result = result.GetNegatingSQLOperatorIfNotInSubquery();
			}
			else if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				result = SpecialComparisonOperator.IsNotBlank;
			}
			return result;
		}

		#endregion

		protected ZQuery GetBranchQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

			ZDBOnlySubQuery orgCompanyDataSubQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH, comparisonOperator == SpecialComparisonOperator.IsBlank);
			orgCompanyDataSubQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);

			if (comparisonOperator == SpecialComparisonOperator.IsBlank || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				orgCompanyDataSubQuery.AddToFilter(OrgCompanyDataSchema.OB_GB_ControllingBranch, SQLComparisonOperator.NotEqual, null);
			}
			else
			{
				ZDBOnlySubQuery glbBranchSubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), OrgCompanyDataSchema.OB_GB_ControllingBranch);
				glbBranchSubQuery.AddToFilter(GlbBranchSchema.GB_Code, comparisonOperator, value.SubstringSafe(0, GlbBranchSchema.GB_Code.MaxLength));
				orgCompanyDataSubQuery.AddSubQuery(glbBranchSubQuery, JoinCondition.And);
			}

			query.AddSubQuery(orgCompanyDataSubQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region Date Filters Delegates

		ZQuery GetKnownShipperExpiryDateQuery(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery countryDataQuery = new ZDBOnlySubQuery(typeof(OrgCountryData), OrgCountryDataSchema.OV_OH_OrgHeader);

			var countryOrEUCode = Constants.CountryCodes.IsInEuropeanUnionAviationSecurityScheme(GlbCompany.CurrentCompany.Country.Code) && FreightDataRegistry.Instance.EnableSupplyChainSecurity_EU.Value
				? (ZString)Constants.CountryCodes.EuropeanUnion
				: GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			countryDataQuery.AddToFilter(OrgCountryDataSchema.OV_RN_NKClientCountryRelation, countryOrEUCode);
			AddDateRange(countryDataQuery, comparisonOperator, JoinCondition.And, OrgCountryDataSchema.OV_EXApprovalExpiryDate, dateFrom.Date, dateTo.Date);
			query.AddSubQuery(countryDataQuery, JoinCondition.And);

			return query;
		}

		protected ZQuery GetSTDARLastChecked(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			return GetForeignTableQueryForCompanyData(OrgCompanyDataSchema.OB_IsDebtor, OrgCompanyDataSchema.OB_ARQualityAssuredCheckedDate, dateFrom, dateTo, comparisonOperator);
		}

		protected ZQuery GetSTDAPLastChecked(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			return GetForeignTableQueryForCompanyData(OrgCompanyDataSchema.OB_IsCreditor, OrgCompanyDataSchema.OB_APQualityAssuredCheckedDate, dateFrom, dateTo, comparisonOperator);
		}

		protected ZQuery GetSTDCRShipExpected(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			return GetSupplierBuyerLinkQuery(OrgHeaderSchema.OH_IsConsignor, OrgSupplierBuyerLinkSchema.OL_OH_Supplier, OrgSupplierBuyerLinkSchema.OL_InitialShipmentExpected, dateFrom, dateTo, comparisonOperator);
		}

		protected ZQuery GetSTDCEShipExpected(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			return GetSupplierBuyerLinkQuery(OrgHeaderSchema.OH_IsConsignee, OrgSupplierBuyerLinkSchema.OL_OH_Buyer, OrgSupplierBuyerLinkSchema.OL_InitialShipmentExpected, dateFrom, dateTo, comparisonOperator);
		}

		protected ZQuery GetSTDCreditReviewDate(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			return GetForeignTableQueryForCompanyData(OrgCompanyDataSchema.OB_IsDebtor, OrgCompanyDataSchema.OB_ARAccountAndCreditReviewDue, dateFrom, dateTo, comparisonOperator);
		}

		protected ZQuery GetSALCSDateLastCall(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			return GetMiscServQuery(OrgHeaderSchema.OH_IsSalesLead, OrgMiscServSchema.OM_CMLastCallDate, dateFrom, dateTo, comparisonOperator);
		}

		protected ZQuery GetSALCSDateNextCall(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			ZString filterString = @"
				OH_PK in
					(
						SELECT
							OH_PK
						FROM dbo.OrgHeader oh
						WHERE
							(
							SELECT MIN(OQ_NextCall)
							FROM dbo.OrgSalesCall a
							WHERE
								a.OQ_CallDate IS NULL
								AND a.OQ_NextCall >= '{0}'
								AND a.OQ_OH = oh.OH_PK
								AND a.OQ_Status NOT IN ({1})
							) {2}
					)";

			return GetSALCSQuery(comparisonOperator, dateFrom, dateTo, filterString);
		}

		protected ZQuery GetSALCSDateLastUnactioned(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			ZString filterString = @"
				OH_PK in
					(
						SELECT
							OH_PK
						FROM dbo.OrgHeader oh
						WHERE
							(
							SELECT MAX(OQ_NextCall)
							FROM dbo.OrgSalesCall a
							WHERE
								a.OQ_CallDate IS NULL
								AND a.OQ_NextCall >= ISNULL((SELECT MAX(OQ_CallDate) As lastCall FROM dbo.OrgSalesCall b WHERE a.OQ_OH = b.OQ_OH), '1900-01-01')
								AND a.OQ_NextCall <= '{0}'
								AND a.OQ_OH = oh.OH_PK
								AND a.OQ_Status NOT IN ({1})
							) {2}
					)";

			return GetSALCSQuery(comparisonOperator, dateFrom, dateTo, filterString);
		}

		protected ZQuery GetImporterBondQueried(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			ZString notIn = "";
			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				notIn = "NOT";
				comparisonOperator = DateComparisonOperator.HasDateEntered;
			}

			var parameters = new ZSqlParameterCollection();
			if (TryGetWhereClauseFromDateComparison(comparisonOperator, dateFrom, dateTo, parameters, EDIMessageSchema.EM_SystemCreateTimeUtc, out var whereClause))
			{
				string latestImporterBondQueryMessageFormat = @"
				OH_PK {0} IN
				(
					SELECT EM_LinkUniqueID
					FROM
					(
						SELECT EM_LinkUniqueID, MAX(EM_SystemCreateTimeUtc) AS EM_SystemCreateTimeUtc
						FROM dbo.EDIMessage
						WHERE EM_LinkUniqueID IS NOT NULL
						AND EM_ReceiveTransmit = 'TRX'
						AND EM_MessageType = 'KI'
						AND EM_IsActive = 1
						GROUP BY EM_LinkUniqueID
					) AS LatestMessage
					WHERE EM_SystemCreateTimeUtc {1}
				)";

				ZString messageSubQuery = ZString.Format(latestImporterBondQueryMessageFormat, notIn, whereClause);
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
				ZDBOnlySubQuery orgCusCodeQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
				orgCusCodeQuery.AddToFilter(OrgCusCodeSchema.OK_IsValid, true);
				var newquery = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.USACodeTypes.EmployerIdentificationNumber);
				newquery.AddToFilter(JoinCondition.Or, OrgCusCodeSchema.OK_CodeType, OrgCusCode.USACodeTypes.SocialSecurityNumber);
				newquery.AddToFilter(JoinCondition.Or, OrgCusCodeSchema.OK_CodeType, OrgCusCode.USACodeTypes.CBPAssignedNumber);
				orgCusCodeQuery.AddToFilter(newquery);

				query.AddSubQuery(orgCusCodeQuery, JoinCondition.And);
				query.AddFilterAndZSQLParameterCollection(messageSubQuery, parameters);
				return query;
			}
			else
			{
				return new ZQuery();
			}
		}

		bool TryGetWhereClauseFromDateComparison(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo, ZSqlParameterCollection parameters, SchemaDateTimeColumn dateColumn, out ZString whereClause)
		{
			var successful = true;

			switch (comparisonOperator)
			{
				case DateComparisonOperator.HasNoDateEntered:
					whereClause = (NoResString)"IS NULL";
					break;
				case DateComparisonOperator.HasDateEntered:
					whereClause = (NoResString)"IS NOT NULL";
					break;
				default:
					if (dateFrom.IsEmpty && dateTo.IsEmpty)
					{
						successful = false;
						whereClause = string.Empty;
					}
					else if (!dateFrom.IsEmpty)
					{
						parameters.Add("@dateFrom", dateFrom, dateColumn);

						if (dateTo.IsEmpty)
						{
							whereClause = (NoResString)">= @dateFrom";
						}
						else
						{
							parameters.Add("@dateTo", dateTo, dateColumn);
							whereClause = (NoResString)"BETWEEN @dateFrom and @dateTo";
						}
					}
					else
					{
						parameters.Add("@dateTo", dateTo, dateColumn);
						whereClause = (NoResString)"<= @dateTo";
					}

					break;
			}

			return successful;
		}

		ZQuery GetSALCSQuery(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo, string filterString)
		{
			var closedStatusCodes =
				from CommunicationStatus status
					in OrganisationsDataRegistry.Instance.CommunicationStatusList.Value
				where status.Closed
				select "'" + status.Code + "'";

			var closedStatusesString = string.Join(",", closedStatusCodes);
			closedStatusesString = !string.IsNullOrEmpty(closedStatusesString) ? closedStatusesString : "''";

			var parameters = new ZSqlParameterCollection();
			if (TryGetWhereClauseFromDateComparison(comparisonOperator, dateFrom, dateTo, parameters, OrgSalesCallSchema.OQ_NextCall, out var whereClause))
			{
				filterString = string.Format(filterString, ZDateTime.UtcNow.SqlFormat, closedStatusesString, whereClause);
				var result = new ZDBOnlyQuery(typeof(OrgHeader));
				result.AddFilterAndZSQLParameterCollection(filterString, parameters);

				return result;
			}
			else
			{
				return new ZQuery();
			}
		}

		protected ZQuery GetSALCSEstClose(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			return GetMiscServQuery(OrgHeaderSchema.OH_IsSalesLead, OrgMiscServSchema.OM_CMEstimatedDateToClose, dateFrom, dateTo, comparisonOperator);
		}

		protected ZQuery GetSALCRClientComm(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			return GetMiscServQuery(OrgHeaderSchema.OH_IsSalesLead, OrgMiscServSchema.OM_CMClientCommenced, dateFrom, dateTo, comparisonOperator);
		}

		ZQuery GetPowerOfAttorneyValidToDate(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(JobRequiredDocument), JobRequiredDocumentSchema.EQ_ParentID);
			subQuery.AddToFilter(JobRequiredDocumentSchema.EQ_DocType, Constants.RefDocTypes.PowerOfAttorney);
			subQuery.AddToFilter(JobRequiredDocumentSchema.EQ_ParentTableCode, OrgHeaderSchema.Constants.Prefix);

			AddDateRange(subQuery, comparisonOperator, JoinCondition.And, JobRequiredDocumentSchema.EQ_ValidToDate, dateFrom.Date, dateTo.Date);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		protected ZQuery GetLastScreenDateQuery(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			var parameters = new ZSqlParameterCollection();
			if (TryGetWhereClauseFromDateComparison(comparisonOperator, dateFrom, dateTo, parameters, StmEntityScreeningLogSchema.PJ_SystemCreateTimeUtc, out var whereClause))
			{
				string orgPartyScreeningStatusQuery = FormattableString.Invariant($@"
				OH_PK IN (
					SELECT OH_PK
					FROM (
						SELECT top 1 with ties
							PJ_ParentID OH_PK,
							PJ_SystemCreateTimeUtc LastScreenDate
						FROM dbo.StmEntityScreeningLog
						WHERE PJ_ParentTableCode = '{OrgHeaderSchema.Constants.Prefix}'
						AND PJ_Status IN ('{DeniedPartyConstants.LogsScreeningStatus.MatchedDeniedParty}', '{DeniedPartyConstants.LogsScreeningStatus.PotentialMatchesFound}', '{DeniedPartyConstants.LogsScreeningStatus.ScreenedClear}')
						ORDER BY ROW_NUMBER() over (
								partition by PJ_ParentID
								order by PJ_SystemCreateTimeUtc DESC,
								PJ_Sequence DESC)
					) a
					where LastScreenDate {whereClause}
				)"
				);

				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
				query.AddFilterAndZSQLParameterCollection(new ZString(orgPartyScreeningStatusQuery), parameters);

				return query;
			}
			else
			{
				return new ZQuery();
			}
		}

		#endregion

		#region Locations Delegates

		protected ZQuery GetMainUNLOCOQuery(ZString value)
		{
			ZQuery query = new ZQuery();

			AddForMainUNLOCO(query, value);

			return query;
		}

		protected class ForwarderAppPortSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZQuery query = new ZQuery();

				ZDBOnlyQuery forwarderQuery = new ZDBOnlyQuery(typeof(OrgHeader));
				forwarderQuery.AddToFilter(OrgHeaderSchema.OH_IsForwarder, ZBool.True);
				ZDBOnlySubQuery appPortsSubQuery = new ZDBOnlySubQuery(typeof(OrgAppointedAgentPorts), OrgAppointedAgentPortsSchema.O5_OH);

				appPortsSubQuery.AddToFilter(filter);
				forwarderQuery.AddSubQuery(appPortsSubQuery, JoinCondition.And);
				query.AddToFilter(forwarderQuery);

				return query;
			}
		}

		protected class CarrierAppPortSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZQuery query = new ZQuery();

				ZDBOnlyQuery carrierQuery = new ZDBOnlyQuery(typeof(OrgHeader));
				carrierQuery.AddToFilter(OrgHeaderSchema.OH_IsShippingProvider, ZBool.True);
				ZDBOnlySubQuery appCarPortsSubQuery = new ZDBOnlySubQuery(typeof(OrgAppointedAgentPorts), OrgAppointedAgentPortsSchema.O5_OH);

				appCarPortsSubQuery.AddToFilter(filter);
				carrierQuery.AddSubQuery(appCarPortsSubQuery, JoinCondition.And);
				query.AddToFilter(carrierQuery);

				return query;
			}
		}

		protected ZQuery GetAppPort(ZString value)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(OrgAppointedAgentPortsSchema.O5_PortOrCountry, SQLComparisonOperator.StartsWith, value.SubstringSafe(0, OrgAppointedAgentPortsSchema.O5_PortOrCountry.MaxLength));
			return query;
		}

		#endregion

		#region Flags and Statuses Delegates

		ZQuery GetKnownShipperQuery(ZString value)
		{
			if (value != "ALL")
			{
				var countryOrLicenceEconomicGroupingCode = SupplyChainSecurityConfiguration.IsEnabled && !SupplyChainSecurityConfiguration.LicenceEconomicGroupingCode.IsEmpty
					? SupplyChainSecurityConfiguration.LicenceEconomicGroupingCode
					: GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

				var query = new ZDBOnlyQuery(typeof(OrgHeader));
				var countryDataQuery = new ZDBOnlySubQuery(typeof(OrgCountryData), OrgCountryDataSchema.OV_OH_OrgHeader);

				countryDataQuery.AddToFilter(OrgCountryDataSchema.OV_EXApprovedOrMajorExporter, value);
				countryDataQuery.AddToFilter(OrgCountryDataSchema.OV_RN_NKClientCountryRelation, countryOrLicenceEconomicGroupingCode);
				AddKnownShipperAddressQuery(countryDataQuery);
				query.AddSubQuery(countryDataQuery, JoinCondition.And);

				if (value == "NO")
				{
					var noCountryDataRecordsForOrgHeader = new ZDBOnlySubQuery(typeof(OrgCountryData), OrgCountryDataSchema.OV_OH_OrgHeader, true);
					noCountryDataRecordsForOrgHeader.AddToFilter(OrgCountryDataSchema.OV_RN_NKClientCountryRelation, countryOrLicenceEconomicGroupingCode);
					AddKnownShipperAddressQuery(noCountryDataRecordsForOrgHeader);
					query.AddSubQuery(noCountryDataRecordsForOrgHeader, JoinCondition.Or);
				}

				return query;
			}
			else
			{
				return new ZQuery();
			}
		}

		void AddKnownShipperAddressQuery(ZQuery query)
		{
			if (SupplyChainSecurityConfiguration.IsAddressLevelScheme)
			{
				query.AddToFilter(OrgCountryDataSchema.OV_OA_ApprovedLocation, SQLComparisonOperator.NotEqual, DBNull.Value);
			}
		}

		ZQuery GetCreditNotYetApprovedQuery(ZBool value)
		{
			return GetCompanyDataQueryWith2Fields(OrgCompanyDataSchema.OB_IsDebtor, OrgCompanyDataSchema.OB_ARCreditApproved, !value);
		}

		ZQuery GetIsForeignOperatorQuery(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			if (value == OrgConstants.FilterControl.IsForeignOperator.Code.Yes)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.BR.ICusBRForeignOperator), CusBRForeignOperatorSchema.BFR_OH_ForeignOperator);
				query.AddSubQuery(subQuery, JoinCondition.And);
			}
			else if (value == OrgConstants.FilterControl.IsForeignOperator.Code.No)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.BR.ICusBRForeignOperator), CusBRForeignOperatorSchema.BFR_OH_ForeignOperator, true);
				query.AddSubQuery(subQuery, JoinCondition.And);
			}
			return query;
		}

		#region Account Types Delegates

		protected ZQuery GetAccountTypeFilter(ZString value)
		{
			var accountType = value;
			var query = new ZQuery();

			if (accountType == OrgConstants.FilterControl.AccountType.Code.GlobalAccount)
			{
				query.AddToFilter(JoinCondition.And, OrgHeaderSchema.OH_IsGlobalAccount, SQLComparisonOperator.Equal, ZBool.True);
			}
			else if (accountType == OrgConstants.FilterControl.AccountType.Code.NonGlobalAccount)
			{
				query.AddToFilter(JoinCondition.And, OrgHeaderSchema.OH_IsGlobalAccount, SQLComparisonOperator.Equal, ZBool.False);
			}
			else if (accountType == OrgConstants.FilterControl.AccountType.Code.NationalAccount)
			{
				query.AddToFilter(JoinCondition.And, OrgHeaderSchema.OH_IsNationalAccount, SQLComparisonOperator.Equal, ZBool.True);
			}
			else if (accountType == OrgConstants.FilterControl.AccountType.Code.NonNationalAccount)
			{
				query.AddToFilter(JoinCondition.And, OrgHeaderSchema.OH_IsNationalAccount, SQLComparisonOperator.Equal, ZBool.False);
			}
			else if (accountType == OrgConstants.FilterControl.AccountType.Code.TemporaryAccount)
			{
				query.AddToFilter(JoinCondition.And, OrgHeaderSchema.OH_IsTempAccount, SQLComparisonOperator.Equal, ZBool.True);
			}
			else if (accountType == OrgConstants.FilterControl.AccountType.Code.NonTemporaryAccount)
			{
				query.AddToFilter(JoinCondition.And, OrgHeaderSchema.OH_IsTempAccount, SQLComparisonOperator.Equal, ZBool.False);
			}

			return query;
		}

		protected ZQuery GetSalesRepAssignedFilter(ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery sub = null;
			ZDBOnlySubQuery subsub = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), OrgStaffAssignmentsSchema.PK);
			subsub.AddToFilter(OrgStaffAssignmentsSchema.O8_GC, GlbCompany.CurrentCompany.PK);
			subsub.AddToFilter(JoinCondition.Or, OrgStaffAssignmentsSchema.O8_GC, DBNull.Value);

			if (value == OrgConstants.FilterControl.SalesRepAssigned.Code.Assigned)
			{
				sub = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), OrgStaffAssignmentsSchema.O8_OH);
				sub.AddToFilter(OrgStaffAssignmentsSchema.O8_Role, StaffAssignmentRoles.Codes.SalesRep);
			}
			if (value == OrgConstants.FilterControl.SalesRepAssigned.Code.NotAssigned)
			{
				sub = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), OrgStaffAssignmentsSchema.O8_OH, ZBool.True);
				sub.AddToFilter(OrgStaffAssignmentsSchema.O8_Role, StaffAssignmentRoles.Codes.SalesRep);
			}

			if (sub != null)
			{
				sub.AddSubQuery(subsub, JoinCondition.And);
				query.AddSubQuery(sub, JoinCondition.And);
			}

			return query;
		}

		protected class RatesSecuritySubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			}
		}

		protected ZQuery GetRatesSecurityFilter(ZString value)
		{
			ZString ratesSecurity = value;
			ZQuery query = new ZQuery();

			if (ratesSecurity == OrgConstants.FilterControl.RatesSecurity.Code.All)
			{
				query.AddToFilter(OrgCompanyDataSchema.OB_RateSecurityGroup, SQLComparisonOperator.NotEqual, ZString.Empty);
			}
			else
			{
				query.AddToFilter(OrgCompanyDataSchema.OB_RateSecurityGroup, SQLComparisonOperator.Equal, value);
			}

			return query;
		}

		protected ZQuery GetExternalValidationFilter(ZString validationStatus)
		{
			var query = new ZDBOnlyQuery(typeof(OrgHeader));

			string eventCode1 = string.Empty;
			string eventCode2 = string.Empty;
			string eventCode3 = string.Empty;

			if (validationStatus == OrgConstants.FilterControl.ExternalValidationStatus.Code.Passed
					|| validationStatus == OrgConstants.FilterControl.ExternalValidationStatus.Code.Failed
					|| validationStatus == OrgConstants.FilterControl.ExternalValidationStatus.Code.NotCompleted)
			{
				if (validationStatus == OrgConstants.FilterControl.ExternalValidationStatus.Code.Passed)
				{
					eventCode1 = AutoEvents.ExternalValidationPassedCode;
					eventCode2 = AutoEvents.ExternalValidationFailedCode;
					eventCode3 = AutoEvents.ExternalValidationNotCompletedCode;
				}
				else if (validationStatus == OrgConstants.FilterControl.ExternalValidationStatus.Code.Failed)
				{
					eventCode1 = AutoEvents.ExternalValidationFailedCode;
					eventCode2 = AutoEvents.ExternalValidationPassedCode;
					eventCode3 = AutoEvents.ExternalValidationNotCompletedCode;
				}
				else if (validationStatus == OrgConstants.FilterControl.ExternalValidationStatus.Code.NotCompleted)
				{
					eventCode1 = AutoEvents.ExternalValidationNotCompletedCode;
					eventCode2 = AutoEvents.ExternalValidationFailedCode;
					eventCode3 = AutoEvents.ExternalValidationPassedCode;
				}

				query.AddFilterAndZSQLParameterCollection(OrgHeader.Schema.PK + " IN ( " +
						" SELECT logs1." + StmALog.Schema.SL_Parent +
						" FROM  " + StmALogSchema.Constants.SqlSchemaName + "." + StmALogSchema.Constants.TableName + " logs1 " +
						" WHERE logs1." + StmALog.Schema.SL_SE_NKEvent + "= @EventCode1 " +
						" AND NOT EXISTS ( " +
							" SELECT 1 " +
							" FROM  " + StmALogSchema.Constants.SqlSchemaName + "." + StmALogSchema.Constants.TableName + " logs2 " +
							" WHERE logs2." + StmALog.Schema.SL_Parent + " = logs1." + StmALog.Schema.SL_Parent +
							" AND (logs2." + StmALog.Schema.SL_SE_NKEvent + " = @EventCode2 " + " OR logs2." + StmALog.Schema.SL_SE_NKEvent + " = @EventCode3) " +
							" AND logs2." + StmALog.Schema.SL_PostedTimeUtc + " > logs1." + StmALog.Schema.SL_PostedTimeUtc +
						")" +
					")", new ZSqlParameterCollection(
							ZSqlParameter.New("@EventCode1", eventCode1, StmALogSchema.SL_SE_NKEvent),
							ZSqlParameter.New("@EventCode2", eventCode2, StmALogSchema.SL_SE_NKEvent),
							ZSqlParameter.New("@EventCode3", eventCode3, StmALogSchema.SL_SE_NKEvent)
						));
			}
			else if (validationStatus == OrgConstants.FilterControl.ExternalValidationStatus.Code.NotRun)
			{
				eventCode1 = AutoEvents.ExternalValidationNotCompletedCode;
				eventCode2 = AutoEvents.ExternalValidationFailedCode;
				eventCode3 = AutoEvents.ExternalValidationPassedCode;

				query.AddFilterAndZSQLParameterCollection(OrgHeader.Schema.PK + " NOT IN ( " +
						" SELECT " + StmALog.Schema.SL_Parent +
						" FROM  " + StmALogSchema.Constants.SqlSchemaName + "." + StmALogSchema.Constants.TableName +
						" WHERE " + StmALog.Schema.SL_SE_NKEvent + " IN (@EventCode1, @EventCode2, @EventCode3) " +
					")", new ZSqlParameterCollection(
							ZSqlParameter.New("@EventCode1", eventCode1, StmALogSchema.SL_SE_NKEvent),
							ZSqlParameter.New("@EventCode2", eventCode2, StmALogSchema.SL_SE_NKEvent),
							ZSqlParameter.New("@EventCode3", eventCode3, StmALogSchema.SL_SE_NKEvent)
						));
			}

			return query;
		}

		#endregion

		#region Relationship2 (Former Dropedit) Delegates

		protected ZQuery GetGlobalRateTarriffFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			ZByte result;
			if (ZByte.TryParse(value, out result))
			{
				query.AddToFilter(OrgRateTariffLevelSchema.P7_GC, GlbCompany.CurrentCompany.PK);
				query.AddToFilter(OrgRateTariffLevelSchema.P7_TariffLevel, result);
			}
			return query;
		}

		protected ZQuery GetARStdInvoiceTermsFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			AddForARStdInvoiceTerms(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		protected ZQuery GetARDisbInvoiceTermsFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			AddForARDisbInvoiceTerms(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		protected ZQuery GetSALClientRelationshipFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			AddForSALClientRelationship(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		protected ZQuery GetSALDesireToRemainFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			AddForSALDesireToRemain(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		protected ZQuery GetSALEaseToPoachFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			AddForSALEaseToPoach(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		protected ZQuery GetSALElectronicIntegrationFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			AddForSALElectronicIntegration(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		protected class FeesAndChargesSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var mainQuery = new ZDBOnlyQuery(typeof(OrgHeader));
				var subQuery = new ZDBOnlySubQuery(typeof(OrgRateFeeChargeLevel), OrgRateFeeChargeLevelSchema.ORF_OH);
				subQuery.AddToFilter(filter);
				mainQuery.AddSubQuery(subQuery, JoinCondition.And);
				return mainQuery;
			}
		}

		public ZQuery GetFeesAndChargesQuery(ZString serviceType, ZString serviceLevel)
		{
			var subQuery = new ZQuery();

			if (!serviceType.IsEmpty)
			{
				subQuery.AddToFilter(OrgRateFeeChargeLevelSchema.ORF_ServiceType, SQLComparisonOperator.Equal, serviceType);

				if (!serviceLevel.IsEmpty)
				{
					subQuery.AddToFilter(OrgRateFeeChargeLevelSchema.ORF_Level, SQLComparisonOperator.Equal, serviceLevel);
				}
			}

			return subQuery;
		}

		#endregion

		#region Accounting Transactions

		ZQuery GetOutstandingAccountingTransactionsQueryCreditor(ZBool value)
		{
			return GetOutstandingAccountingTransactionsQuery(value, "AP");
		}

		ZQuery GetOutstandingAccountingTransactionsQueryDebtor(ZBool value)
		{
			return GetOutstandingAccountingTransactionsQuery(value, "AR");
		}

		ZQuery GetOutstandingAccountingTransactionsQuery(ZBool value, ZString ledger)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.AH_OH, !value);
			subQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ledger);
			subQuery.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_FullyPaidDate, DBNull.Value);
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#endregion

		#region Organisation and Staff Delegates

		#region Relationship Guids Delegates

		protected ZQuery GetARAcctGroupFilter(ZGuid value)
		{
			ZQuery query = new ZQuery();
			AddForARAcctGroup(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		protected ZQuery GetARCurrencyFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			AddForARCurrency(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		protected ZQuery GetAPAcctGroupFilter(ZGuid value)
		{
			ZQuery query = new ZQuery();
			AddForAPAcctGroup(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		protected ZQuery GetAPBankAccountFilter(ZGuid value)
		{
			ZQuery query = new ZQuery();
			AddForAPBankAccount(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		protected ZQuery GetAPChargeCodeFilter(ZGuid value)
		{
			ZQuery query = new ZQuery();
			AddForAPChargeCode(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		protected ZQuery GetCNRCountryFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			AddForCNRCountry(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		protected ZQuery GetCNRCurrencyFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			AddForCNRCurrency(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		protected ZQuery GetCNRSeaCartageCordinatorFilter(ZGuid value)
		{
			ZQuery query = new ZQuery();
			AddForCNRSeaCartageCordinator(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		protected ZQuery GetCNRAirCartageCordinatorFilter(ZGuid value)
		{
			ZQuery query = new ZQuery();
			AddForCNRAirCartageCordinator(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		protected ZQuery GetCNRSeaCustomerServiceRepFilter(ZGuid value)
		{
			ZQuery query = new ZQuery();
			AddForCNRSeaCustomerServiceRep(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		protected ZQuery GetCNRAirCustomerServiceRepFilter(ZGuid value)
		{
			ZQuery query = new ZQuery();
			AddForCNRAirCustomerServiceRep(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		protected ZQuery GetCNESeaCustomerServiceRepFilter(SQLComparisonOperator comparisonOperator, object value)
		{
			ZQuery query = new ZQuery();
			AddForCNESeaCustomerServiceRep(query, comparisonOperator, value);
			return query;
		}

		protected ZQuery GetCNEAirCustomerServiceRepFilter(ZGuid value)
		{
			ZQuery query = new ZQuery();
			AddForCNEAirCustomerServiceRep(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		protected ZQuery GetCNESeaCartageCordinatorFilter(ZGuid value)
		{
			ZQuery query = new ZQuery();
			AddForCNESeaCartageCordinator(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		protected ZQuery GetCNEAirCartageCordinatorFilter(ZGuid value)
		{
			ZQuery query = new ZQuery();
			AddForCNEAirCartageCordinator(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		protected ZQuery GetFWDCurrencyFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			AddForFWDCurrency(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		protected ZQuery GetSALOverallRepFilter(ZGuid value)
		{
			ZQuery query = new ZQuery();
			AddForSALOverallRep(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		protected ZQuery GetSALImportAirRepFilter(ZGuid value)
		{
			ZQuery query = new ZQuery();
			AddForSALImportAirRep(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		protected ZQuery GetSALImportSeaRepFilter(ZGuid value)
		{
			ZQuery query = new ZQuery();
			AddForSALImportSeaRep(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		protected ZQuery GetSALExportAirRepFilter(ZGuid value)
		{
			ZQuery query = new ZQuery();
			AddForSALExportAirRep(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		protected ZQuery GetSALExportSeaRepFilter(ZGuid value)
		{
			ZQuery query = new ZQuery();
			AddForSALExportSeaRep(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		protected ZQuery GetSALWarehousingRepFilter(ZGuid value)
		{
			ZQuery query = new ZQuery();
			AddForSALWarehousingRep(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		protected ZQuery GetSALOverallAccountManagerFilter(ZGuid value)
		{
			return GetStaffAssignmentsQueryInner("ALL", StaffAssignmentRoles.Codes.AccountManager, value);
		}

		#endregion

		#region OrgType Related Delegates

		protected ZQuery GetNoTransactionsQuery(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			ZDBOnlyQuery mainQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.AH_OH, true);
			subQuery.AddToFilter(AccTransactionHeaderSchema.AH_OH, SQLComparisonOperator.NotEqual, null);
			AddDateRange(subQuery, comparisonOperator, JoinCondition.And, AccTransactionHeaderSchema.AH_PostDate, dateFrom.Date, dateTo.Date);
			mainQuery.AddSubQuery(subQuery, JoinCondition.And);

			return mainQuery;
		}

		protected ZQuery GetNoTransactionsInCurrentCompanyQuery(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			ZDBOnlyQuery mainQuery = new ZDBOnlyQuery(typeof(OrgHeader));

			ZDBOnlySubQuery branchSubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), AccTransactionHeaderSchema.AH_GB);
			branchSubQuery.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);

			ZDBOnlySubQuery transactionsSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.AH_OH, true);
			transactionsSubQuery.AddSubQuery(branchSubQuery, JoinCondition.And);
			AddDateRange(transactionsSubQuery, comparisonOperator, JoinCondition.And, AccTransactionHeaderSchema.AH_PostDate, dateFrom.Date, dateTo.Date);
			transactionsSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_OH, SQLComparisonOperator.NotEqual, null);
			transactionsSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, SQLComparisonOperator.Equal, new ZString[] { "AR", "AP" });

			mainQuery.AddSubQuery(transactionsSubQuery, JoinCondition.And);

			return mainQuery;
		}

		protected ZQuery GetRelatedConsignorQuery(ZGuid value)
		{
			ZDBOnlyQuery consignQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			consignQuery = GetSupplierBuyerLinkQuery(OrgHeaderSchema.OH_IsConsignee, OrgSupplierBuyerLinkSchema.OL_OH_Buyer, OrgSupplierBuyerLinkSchema.OL_OH_Supplier, value, SQLComparisonOperator.Equal);

			return consignQuery;
		}

		protected ZQuery GetRelatedConsigneeQuery(ZGuid value)
		{
			ZDBOnlyQuery consignQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			consignQuery = GetSupplierBuyerLinkQuery(OrgHeaderSchema.OH_IsConsignor, OrgSupplierBuyerLinkSchema.OL_OH_Supplier, OrgSupplierBuyerLinkSchema.OL_OH_Buyer, value, SQLComparisonOperator.Equal);

			return consignQuery;
		}

		protected ZQuery GetHasMainCompetitorOnQuery(ZBool hasCompetitor, ZString competitorType)
		{
			var orgCompetitorSubQuery = new ZDBOnlySubQuery(typeof(OrgCompetitor), OrgCompetitorSchema.OCP_OH_Parent, notIn: !hasCompetitor);

			var companyFilter = new ZQuery(OrgCompetitorSchema.OCP_GC_Company, null);
			companyFilter.AddToFilter(JoinCondition.Or, OrgCompetitorSchema.OCP_GC_Company, GlbCompany.CurrentCompany.PK);
			orgCompetitorSubQuery.AddToFilter(companyFilter);

			orgCompetitorSubQuery.AddToFilter(OrgCompetitorSchema.OCP_Type, competitorType);

			var resultQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			resultQuery.AddSubQuery(orgCompetitorSubQuery, JoinCondition.And);

			return resultQuery;
		}

		protected ZQuery GetSalesMainCompetitorOnQuery(ZGuid competitor, ZString competitorType)
		{
			var orgCompetitorSubQuery = new ZDBOnlySubQuery(typeof(OrgCompetitor), OrgCompetitorSchema.OCP_OH_Parent);

			var companyFilter = new ZQuery(OrgCompetitorSchema.OCP_GC_Company, null);
			companyFilter.AddToFilter(JoinCondition.Or, OrgCompetitorSchema.OCP_GC_Company, GlbCompany.CurrentCompany.PK);
			orgCompetitorSubQuery.AddToFilter(companyFilter);

			orgCompetitorSubQuery.AddToFilter(OrgCompetitorSchema.OCP_OH_Competitor, competitor);
			orgCompetitorSubQuery.AddToFilter(OrgCompetitorSchema.OCP_Type, competitorType);

			var resultQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			resultQuery.AddSubQuery(orgCompetitorSubQuery, JoinCondition.And);

			return resultQuery;
		}

		protected ZQuery GetRegistrationCountryQuery(ZString value)
		{
			ZQuery result = new ZQuery();
			result.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, value);
			return result;
		}

		protected ZQuery GetRegistrationTypeQuery(ZString value)
		{
			ZQuery result = new ZQuery();
			result.AddToFilter(OrgCusCodeSchema.OK_CodeType, value);
			return result;
		}

		#endregion

		#endregion

		#region Org Supplier Buyer Link

		protected ZDBOnlyQuery GetSupplierBuyerLinkQuery(SchemaColumn orgHeaderField, SchemaColumn foreignKey, SchemaColumn supplierBuyerField, object foreignValue, SQLComparisonOperator @operator)
		{
			return GetForeignTableQuery(orgHeaderField, typeof(OrgSupplierBuyerLink), foreignKey, supplierBuyerField, foreignValue, @operator);
		}

		protected ZDBOnlyQuery GetSupplierBuyerLinkQuery(SchemaColumn orgHeaderField, SchemaColumn foreignKey, SchemaDateTimeColumn supplierBuyerField, ZDateTime fromDate, ZDateTime toDate, DateComparisonOperator comparisonOperator)
		{
			return GetForeignTableQuery(orgHeaderField, typeof(OrgSupplierBuyerLink), foreignKey, supplierBuyerField, fromDate, toDate, comparisonOperator);
		}

		#endregion

		#region Staff Assignments

		protected class StaffAssignmentsSubGroup : ModuleFilterSubGroup
		{
			public StaffAssignmentsSubGroup() : base()
			{ }

			public StaffAssignmentsSubGroup(ModuleFilterSubGroup parent) : base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(OrgHeader));
				var staffAssignmentsSubQuery = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), OrgStaffAssignmentsSchema.O8_OH);
				staffAssignmentsSubQuery.AddToFilter(filter);
				result.AddSubQuery(staffAssignmentsSubQuery, JoinCondition.And);
				return result;
			}
		}

		protected ZDBOnlyQuery GetStaffAssignmentsQuery(SchemaColumn orgHeaderField, ZString department, ZString role, object staffPK)
		{
			return GetStaffAssignmentsQuery(orgHeaderField, department, role, staffPK, SQLComparisonOperator.Equal);
		}

		protected class StaffAssignmentSubGroup : ModuleFilterSubGroup
		{
			public StaffAssignmentSubGroup(SchemaColumn orgHeaderField)
			{
				this.OrgHeaderField = orgHeaderField;
			}

			readonly SchemaColumn OrgHeaderField;

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
				if (OrgHeaderField != null)
				{
					query.AddToFilter(OrgHeaderField, ZBool.True);
				}

				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), OrgStaffAssignmentsSchema.O8_OH, false);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(subQuery, JoinCondition.And);

				return query;
			}
		}

		ZQuery GetStaffAssignmentsQueryInner(ZString department, ZString role, object staffPK)
		{
			GlbStaff staff = staffPK != null && staffPK != DBNull.Value ? Factory.Load<GlbStaff>((ZGuid)staffPK) : null;

			ZQuery subQuery = new ZQuery();
			ZQuery subSubQuery = new ZQuery(OrgStaffAssignmentsSchema.O8_GC, GlbCompany.CurrentCompany.PK);
			subSubQuery.AddToFilter(JoinCondition.Or, OrgStaffAssignmentsSchema.O8_GC, DBNull.Value);
			subQuery.AddToFilter(subSubQuery);
			subQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_Department, department);
			subQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_Role, role);
			subQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible, SQLComparisonOperator.Equal, (staff != null ? staff.GS_Code : ZString.Empty));

			return subQuery;
		}

		ZDBOnlyQuery GetStaffAssignmentsQuery(SchemaColumn orgHeaderField, ZString department, ZString role, object staffPK, SQLComparisonOperator comparisonOperator)
		{
			GlbStaff staff = staffPK != null && staffPK != DBNull.Value ? Factory.Load<GlbStaff>((ZGuid)staffPK) : null;

			bool notIn = false;
			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				notIn = true;
				comparisonOperator = SpecialComparisonOperator.IsNotBlank;
			}
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			if (orgHeaderField != null)
			{
				query.AddToFilter(orgHeaderField, ZBool.True);
			}
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), OrgStaffAssignmentsSchema.O8_OH, notIn);
			ZQuery subSubQuery = new ZQuery(OrgStaffAssignmentsSchema.O8_GC, GlbCompany.CurrentCompany.PK);
			subSubQuery.AddToFilter(JoinCondition.Or, OrgStaffAssignmentsSchema.O8_GC, DBNull.Value);
			subQuery.AddToFilter(subSubQuery);
			subQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_Department, department);
			subQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_Role, role);
			subQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible, comparisonOperator, (staff != null ? staff.GS_Code : ZString.Empty));
			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region Org Misc Serv

		protected ZDBOnlyQuery GetMiscServQuery(SchemaColumn orgHeaderField, SchemaColumn miscServField)
		{
			return GetMiscServQuery(orgHeaderField, miscServField, ZBool.True);
		}

		protected ZDBOnlyQuery GetMiscServQuery(SchemaColumn orgHeaderField, SchemaColumn miscServField, object miscServValue)
		{
			return GetMiscServQuery(orgHeaderField, miscServField, miscServValue, SQLComparisonOperator.Equal);
		}

		protected ZDBOnlyQuery GetMiscServQuery(SchemaColumn orgHeaderField, SchemaColumn miscServField, object miscServValue, SQLComparisonOperator @operator)
		{
			return GetForeignTableQuery(orgHeaderField, typeof(OrgMiscServ), OrgMiscServSchema.OM_OH, miscServField, miscServValue, @operator);
		}

		protected ZDBOnlyQuery GetMiscServQuery(SchemaColumn orgHeaderField, SchemaDateTimeColumn miscServField, ZDateTime fromDate, ZDateTime toDate, DateComparisonOperator comparisonOperator)
		{
			return GetForeignTableQuery(orgHeaderField, typeof(OrgMiscServ), OrgMiscServSchema.OM_OH, miscServField, fromDate, toDate, comparisonOperator);
		}

		#endregion

		#region OrgHeader and OrgCompanyData

		protected ZDBOnlyQuery GetOrgCompanyDataQuery(SchemaColumn orgHeaderField, SchemaColumn compDataField, object compDataValue, SQLComparisonOperator @operator)
		{
			return GetForeignTableQuery(orgHeaderField, typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH, compDataField, compDataValue, @operator);
		}

		protected ZDBOnlyQuery GetOrgHeaderAndCompanyDataQuery(SchemaColumn orgCompanyDataField, SchemaColumn miscServField)
		{
			return GetOrgHeaderAndCompanyDataQuery(orgCompanyDataField, miscServField, ZBool.True);
		}

		protected ZDBOnlyQuery GetOrgHeaderAndCompanyDataQuery(SchemaColumn orgHeaderField, SchemaColumn companyDataField, object companyDataValue)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			query.AddToFilter(orgHeaderField, ZBool.True);

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(JoinCondition.And, companyDataField, SQLComparisonOperator.Equal, companyDataValue);

			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region OrgCompanyData and OrgMiscServ

		protected ZDBOnlyQuery GetMiscServAndCompanyDataQuery(SchemaColumn orgCompanyDataField, SchemaColumn miscServField)
		{
			return GetMiscServAndCompanyDataQuery(orgCompanyDataField, miscServField, ZBool.True, SQLComparisonOperator.Equal);
		}

		protected ZDBOnlyQuery GetMiscServAndCompanyDataQuery(SchemaColumn orgCompanyDataField, SchemaColumn miscServField, object miscServValue, SQLComparisonOperator @operator)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(orgCompanyDataField, ZBool.True);
			query.AddSubQuery(subQuery, JoinCondition.And);

			subQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH);
			subQuery.AddToFilter(JoinCondition.And, miscServField, @operator, miscServValue);

			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region OrgCompanyDataWith2Fields

		protected ZDBOnlyQuery GetCompanyDataQueryWith2Fields(SchemaColumn orgCompanyDataField, SchemaColumn companyDataField)
		{
			return GetForeignTableQueryForCompanyDataWith2Fields(orgCompanyDataField, companyDataField, ZBool.True);
		}

		protected ZDBOnlyQuery GetCompanyDataQueryWith2Fields(SchemaColumn orgCompanyDataField, SchemaColumn companyDataField, object companyDataValue)
		{
			return GetForeignTableQueryForCompanyDataWith2Fields(orgCompanyDataField, companyDataField, companyDataValue);
		}

		#endregion

		#region Generic Table Query Methods

		protected class MiscServSubGroup : ModuleFilterSubGroup
		{
			public MiscServSubGroup(SchemaColumn orgHeaderField)
			{
				this.OrgHeaderField = orgHeaderField;
			}

			readonly SchemaColumn OrgHeaderField;

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
				query.AddToFilter(OrgHeaderField, ZBool.True);

				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH);
				subQuery.AddToFilter(filter);

				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			}
		}

		protected class SupplierBuyerLinkSubGroup : ModuleFilterSubGroup
		{
			public SupplierBuyerLinkSubGroup(SchemaColumn orgHeaderField, SchemaColumn foreignKey)
			{
				this.OrgHeaderField = orgHeaderField;
				this.ForeignKey = foreignKey;
			}

			readonly SchemaColumn OrgHeaderField;
			readonly SchemaColumn ForeignKey;

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
				query.AddToFilter(OrgHeaderField, ZBool.True);

				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgSupplierBuyerLink), ForeignKey);
				subQuery.AddToFilter(filter);

				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			}
		}

		protected ZDBOnlyQuery GetForeignTableQuery(SchemaColumn orgHeaderField, Type foreignType, SchemaColumn foreignKey, SchemaColumn foreignField, object foreignValue)
		{
			return GetForeignTableQuery(orgHeaderField, foreignType, foreignKey, foreignField, foreignValue, SQLComparisonOperator.Equal);
		}

		protected ZDBOnlyQuery GetForeignTableQuery(SchemaColumn orgHeaderField, Type foreignType, SchemaColumn foreignKey, SchemaColumn foreignField, object foreignValue, SQLComparisonOperator @operator)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			query.AddToFilter(orgHeaderField, ZBool.True);

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(foreignType, foreignKey);
			subQuery.AddToFilter(JoinCondition.And, foreignField, @operator, foreignValue);

			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		protected ZDBOnlyQuery GetForeignTableQuery(SchemaColumn orgHeaderField, Type foreignType, SchemaColumn foreignKey, SchemaDateTimeColumn foreignField, ZDateTime fromDate, ZDateTime toDate, DateComparisonOperator comparisonOperator)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			query.AddToFilter(orgHeaderField, ZBool.True);

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(foreignType, foreignKey);
			AddDateRange(subQuery, comparisonOperator, JoinCondition.And, foreignField, fromDate.Date, toDate.Date);

			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		protected ZDBOnlyQuery GetForeignTableQuery(SchemaColumn orgHeaderField, Type foreignType, SchemaColumn foreignKey)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			query.AddToFilter(orgHeaderField, ZBool.True);
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(foreignType, foreignKey);

			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		protected ZDBOnlyQuery GetForeignTableQueryForCompanyData(SchemaColumn orgCompanyDataField, object value)
		{
			return GetForeignTableQueryForCompanyData(orgCompanyDataField, value, SQLComparisonOperator.Equal);
		}

		protected ZDBOnlyQuery GetForeignTableQueryForCompanyData(SchemaColumn orgCompanyDataField, object value, SQLComparisonOperator @operator)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(JoinCondition.And, orgCompanyDataField, @operator, value);
			subQuery.AddToFilter(JoinCondition.And, OrgCompanyDataSchema.OB_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);

			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		protected class CompanyDataSubGroup : ModuleFilterSubGroup
		{
			public CompanyDataSubGroup(SchemaColumn orgCompanyDataField)
			{
				this.OrgCompanyDataField = orgCompanyDataField;
			}

			readonly SchemaColumn OrgCompanyDataField;

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
				subQuery.AddToFilter(OrgCompanyDataField, ZBool.True);
				query.AddSubQuery(subQuery, JoinCondition.And);

				subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
				subQuery.AddToFilter(filter);
				subQuery.AddToFilter(JoinCondition.And, OrgCompanyDataSchema.OB_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);

				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			}
		}

		protected ZDBOnlyQuery GetForeignTableQueryForCompanyDataWith2Fields(SchemaColumn orgCompanyDataField, SchemaColumn foreignField, object foreignValue, SQLComparisonOperator @operator)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(orgCompanyDataField, ZBool.True);
			query.AddSubQuery(subQuery, JoinCondition.And);

			subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(JoinCondition.And, foreignField, @operator, foreignValue);
			subQuery.AddToFilter(JoinCondition.And, OrgCompanyDataSchema.OB_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);

			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		protected ZDBOnlyQuery GetARInvoiceTermsQuery(object invoiceTerm, string invoiceType = null)
		{
			ZDBOnlyQuery resultQuery = new ZDBOnlyQuery(typeof(OrgHeader));

			string sqlText = "";
			if (invoiceType == null || invoiceType == OrgARTermsLookups.InvoiceTypes.All.Code)
			{
				sqlText = @"
OH_PK IN
(
	SELECT OrgHeader.OH_PK
	FROM
		dbo.OrgHeader
		LEFT JOIN dbo.OrgCompanyData on OrgCompanyData.OB_OH = OrgHeader.OH_PK AND OrgCompanyData.OB_GC = @Company AND OrgCompanyData.OB_IsDebtor = 1
		LEFT JOIN dbo.OrgARTerms AS ARTerm ON OrgCompanyData.OB_PK = ARTerm.PY_OB and ARTerm.PY_InvoiceClass = @InvoiceClassALL

		LEFT JOIN dbo.OrgRelatedParty AS OrgRelatedPartyARSettlementGroup ON OrgRelatedPartyARSettlementGroup.PR_OH_Parent = Orgheader.OH_PK AND OrgRelatedPartyARSettlementGroup.PR_GC = OrgCompanyData.OB_GC AND OrgRelatedPartyARSettlementGroup.PR_PartyType = @PartyTypeARS AND OrgRelatedPartyARSettlementGroup.PR_FreightDirection = @FreightDirectionAR
		LEFT JOIN dbo.OrgHeader SettlementGroup ON SettlementGroup.OH_PK = OrgRelatedPartyARSettlementGroup.PR_OH_RelatedParty
		LEFT JOIN dbo.OrgCompanyData SettlementOrgCompanyData ON SettlementOrgCompanyData.OB_OH = SettlementGroup.OH_PK  AND SettlementOrgCompanyData.OB_GC = OrgCompanyData.OB_GC
		LEFT JOIN dbo.OrgARTerms AS SettlementARTerm ON SettlementOrgCompanyData.OB_PK = SettlementARTerm.PY_OB AND SettlementARTerm.PY_InvoiceClass = @InvoiceClassALL
	WHERE
		CASE
			WHEN ARTerm.PY_InvoiceTerm = @InvoiceTermDEF
			THEN SettlementARTerm.PY_InvoiceTerm
			ELSE ARTerm.PY_InvoiceTerm
		END = @InvoiceTerm
)
";
			}
			else
			{
				sqlText = @"
OH_PK IN
(
	SELECT OrgHeader.OH_PK
	FROM
		dbo.OrgHeader
		LEFT JOIN dbo.OrgCompanyData on OrgCompanyData.OB_OH = OrgHeader.OH_PK AND OrgCompanyData.OB_GC = @Company AND OrgCompanyData.OB_IsDebtor = 1
		LEFT JOIN dbo.OrgARTerms AS ARTerm ON OrgCompanyData.OB_PK = ARTerm.PY_OB and ARTerm.PY_InvoiceClass = @InvoiceClassDSB
		LEFT JOIN dbo.OrgARTerms AS FallbackARTerm ON OrgCompanyData.OB_PK = FallbackARTerm.PY_OB and FallbackARTerm.PY_InvoiceClass = @InvoiceClassALL

		LEFT JOIN dbo.OrgRelatedParty AS OrgRelatedPartyARSettlementGroup ON OrgRelatedPartyARSettlementGroup.PR_OH_Parent = Orgheader.OH_PK AND OrgRelatedPartyARSettlementGroup.PR_GC = OrgCompanyData.OB_GC AND OrgRelatedPartyARSettlementGroup.PR_PartyType = @PartyTypeARS AND OrgRelatedPartyARSettlementGroup.PR_FreightDirection = @FreightDirectionAR
		LEFT JOIN dbo.OrgHeader SettlementGroup ON SettlementGroup.OH_PK = OrgRelatedPartyARSettlementGroup.PR_OH_RelatedParty
		LEFT JOIN dbo.OrgCompanyData SettlementOrgCompanyData ON SettlementOrgCompanyData.OB_OH = SettlementGroup.OH_PK  AND SettlementOrgCompanyData.OB_GC = OrgCompanyData.OB_GC
		LEFT JOIN dbo.OrgARTerms AS SettlementARTerm ON SettlementOrgCompanyData.OB_PK = SettlementARTerm.PY_OB AND SettlementARTerm.PY_InvoiceClass = ISNULL(ARTerm.PY_InvoiceClass, FallbackARTerm.PY_InvoiceClass)
		LEFT JOIN dbo.OrgARTerms AS SettlementFallbackARTerm ON SettlementOrgCompanyData.OB_PK = SettlementFallbackARTerm.PY_OB and SettlementFallbackARTerm.PY_InvoiceClass = @InvoiceClassALL
	WHERE
		CASE
			WHEN ISNULL(ARTerm.PY_InvoiceTerm, FallbackARTerm.PY_InvoiceTerm) = @InvoiceTermDEF
			THEN ISNULL(SettlementARTerm.PY_InvoiceTerm, SettlementFallbackARTerm.PY_InvoiceTerm)
			ELSE ISNULL(ARTerm.PY_InvoiceTerm, FallbackARTerm.PY_InvoiceTerm)
		END = @InvoiceTerm
)
";
			}

			ZSqlParameterCollection sqlParameters = new ZSqlParameterCollection();
			sqlParameters.Add("@Company", GlbCompany.CurrentCompany.PK, OrgCompanyDataSchema.OB_GC);
			sqlParameters.Add("@InvoiceClassDSB", OrgARTermsLookups.InvoiceTypes.DSB.Code, OrgARTermsSchema.PY_InvoiceClass);
			sqlParameters.Add("@InvoiceClassALL", OrgARTermsLookups.InvoiceTypes.All.Code, OrgARTermsSchema.PY_InvoiceClass);
			sqlParameters.Add("@InvoiceTermDEF", OrgARTermsLookups.DefaultInvoiceTerm.Code, OrgARTermsSchema.PY_InvoiceTerm);
			sqlParameters.Add("@InvoiceTerm", invoiceTerm, OrgARTermsSchema.PY_InvoiceTerm);
			sqlParameters.Add("@PartyTypeARS", RelatedPartyTypeList.Codes.ARSettlementGroup, OrgRelatedPartySchema.PR_PartyType);
			sqlParameters.Add("@FreightDirectionAR", RelatedPartyDirectionList.Codes.AR, OrgRelatedPartySchema.PR_FreightDirection);

			resultQuery.AddFilterAndZSQLParameterCollection(sqlText, sqlParameters);

			return resultQuery;
		}

		protected class APInvoiceTermsSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery resultQuery = new ZDBOnlyQuery(typeof(OrgHeader));

				Func<object, bool, ZDBOnlySubQuery> getAPInvoiceTerm = (term, checkIsCreditor) =>
				{
					ZDBOnlySubQuery apInvoiceTerm = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
					if (checkIsCreditor)
					{
						apInvoiceTerm.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
					}
					apInvoiceTerm.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
					if (term == null)
					{
						apInvoiceTerm.AddToFilter(filter);
					}
					else
					{
						apInvoiceTerm.AddToFilter(OrgCompanyDataSchema.OB_APPaymentTerms, term);
					}
					return apInvoiceTerm;
				};

				ZDBOnlySubQuery defaultAPInvoiceTermQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);

				ZDBOnlySubQuery relatedPartySubQuery = new ZDBOnlySubQuery(typeof(OrgRelatedParty), OrgRelatedPartySchema.PR_OH_Parent);
				relatedPartySubQuery.AddToFilter(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.APSettlementGroup);
				relatedPartySubQuery.AddToFilter(OrgRelatedPartySchema.PR_FreightDirection, RelatedPartyDirectionList.Codes.AP);
				relatedPartySubQuery.AddToFilter(OrgRelatedPartySchema.PR_GC, GlbCompany.CurrentCompany.PK);

				ZDBOnlySubQuery settlementAPInvoiceTermQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
				settlementAPInvoiceTermQuery.AddSubQuery(getAPInvoiceTerm(null, false), JoinCondition.And);

				relatedPartySubQuery.AddSubQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, settlementAPInvoiceTermQuery, JoinCondition.And);

				defaultAPInvoiceTermQuery.AddSubQuery(getAPInvoiceTerm(OrgCompanyDataLookups.DefaultInvoiceTerm.Code, true), JoinCondition.And);
				defaultAPInvoiceTermQuery.AddSubQuery(relatedPartySubQuery, JoinCondition.And);
				defaultAPInvoiceTermQuery.AddAsUnionQuery(getAPInvoiceTerm(null, true));

				resultQuery.AddSubQuery(defaultAPInvoiceTermQuery, JoinCondition.And);

				return resultQuery;
			}
		}

		protected ZDBOnlyQuery GetForeignTableQueryForCompanyData(SchemaColumn orgCompanyDataField, SchemaDateTimeColumn foreignField, ZDateTime fromDate, ZDateTime toDate, DateComparisonOperator comparisonOperator)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(orgCompanyDataField, ZBool.True);
			query.AddSubQuery(subQuery, JoinCondition.And);

			subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			AddDateRange(subQuery, comparisonOperator, JoinCondition.And, foreignField, fromDate.Date, toDate.Date);
			subQuery.AddToFilter(JoinCondition.And, OrgCompanyDataSchema.OB_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);

			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		protected ZDBOnlyQuery GetForeignTableQueryForCompanyDataWith2Fields(SchemaColumn orgCompanyDataField, SchemaColumn foreignField, object foreignValue)
		{
			return GetForeignTableQueryForCompanyDataWith2Fields(orgCompanyDataField, foreignField, foreignValue, SQLComparisonOperator.Equal);
		}

		protected ZDBOnlyQuery GetForeignTableQueryForLocationFields(SchemaColumn orgHeaderField, Type foreignType, SchemaColumn foreignKey, SchemaColumn foreignField, object foreignValue)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			query.AddToFilter(orgHeaderField, ZBool.True);

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(foreignType, foreignKey);
			subQuery.AddToFilter(LocationHelper.GetLocationFilter(Factory, foreignValue.ToString(), foreignField, typeof(OrgHeader)));

			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#endregion

		#region EquivalentFilterObjectDictionary

		static Dictionary<ZString, ZString> FilterObjectPropertyDictionary
		{
			get
			{
				if (equivalentFilterObjectDictionary == null)
				{
					equivalentFilterObjectDictionary = new Dictionary<ZString, ZString>();
					equivalentFilterObjectDictionary.Add(OrgHeader.Schema.OH_FullName, Descriptions.NameFilter);
					equivalentFilterObjectDictionary.Add(OrgAddress.Schema.OA_Address1, OrgConstants.FilterControl.OrgAddress.Address);
					equivalentFilterObjectDictionary.Add(OrgAddress.Schema.OA_PostCode, OrgConstants.FilterControl.OrgAddress.PostCode);
					equivalentFilterObjectDictionary.Add(OrgAddress.Schema.OA_City, OrgConstants.FilterControl.OrgAddress.City);
					equivalentFilterObjectDictionary.Add(OrgAddress.Schema.OA_State, OrgConstants.FilterControl.OrgAddress.State);
				}
				return equivalentFilterObjectDictionary;
			}
		}
		static Dictionary<ZString, ZString> equivalentFilterObjectDictionary;

		#endregion

		#region SupplyChainSecurityConfiguration

		internal ISupplyChainSecurityConfiguration SupplyChainSecurityConfiguration
		{
			get { return supplyChainSecurityConfiguration ?? (supplyChainSecurityConfiguration = ObjectFactory.Get<ISupplyChainSecurityConfigurationHelper>().GetConfiguration()); }
		}
		ISupplyChainSecurityConfiguration supplyChainSecurityConfiguration;

		#endregion

		readonly OrganisationCRMSecurityProvider SecurityProvider = new OrganisationCRMSecurityProvider();
		readonly OrganisationFilterHelper OrgFilterHelper = new OrganisationFilterHelper();

		#region Index Search Filters

		protected override SearchField ResolveSearchField(SearchField searchField)
		{
			var fieldNameUpper = searchField.FieldName.ToUpperInvariant();

			switch (fieldNameUpper)
			{
				case SearchFieldConstants.OrganizationType:
					var orgTypeFilter = new IndexSearchOrgTypeModuleFilter("Organisation Types");
					orgTypeFilter.Category = FilterCategories.StatusAndFlags;
					orgTypeFilter.ModuleType = fModuleType;
					orgTypeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|OrganisationTypes", "Organization Types");
					return new SearchFieldOverride(searchField, indexFilterOverride: orgTypeFilter);
				default:
					return base.ResolveSearchField(searchField);
			}
		}

		protected override ModuleFilterCollection GetModuleFiltersFromGlowCore()
		{
			var filters = base.GetModuleFiltersFromGlowCore();

			var systemDefinedOrgFilter = new IndexSearchModuleTextFilter("SystemDefinedOrg", SearchField.Create($"CODE"), GetGlowIndexQuery, new CodeDescriptionPairList());
			systemDefinedOrgFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			filters.AddFilter(systemDefinedOrgFilter);

			return filters;
		}

		IGlowQuery GetGlowIndexQuery(SearchField searchField, ZString value)
		{
			return new BooleanQuery(
				BooleanOperator.And,
				new NotEqualQuery(new Term($"CODE", OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Code)),
				new NotEqualQuery(new Term($"CODE", OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Code)));
		}

		static class SearchFieldConstants
		{
			public const string OrganizationType = "ORGANIZATIONTYPE";
		}

		#endregion
	}
}
