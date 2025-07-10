using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class AccGLHeaderFilterBusinessObject : FilterStripBusinessObject
	{
		public AccGLHeaderFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddStatusFilters(filters);
			AddOtherFilters(filters);

			return filters;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("GL Account", AccGLHeaderSchema.AG_AccountNum).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGLHeaderFilter|GLAccount", "GL Account");
			filters.AddFiltersForTranslatableText("Description", AccGLHeaderSchema.AG_Description, typeof(AccGLHeader), ResString.GetMultilingualString("Accounting|AccGLHeaderFilter|Description", "Description"));
			filters.AddTextFilter("Company Filter", GetCompanyFilter).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGLHeaderFilter|CompanyFilter", "Company Filter");
			filters.AddTextFilter("Local Account Code", GetLocalAccountCode).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGLHeaderFilter|LocalAccountCode", "Local Account Code");
			filters.AddTextFilter("Local Account Description", GetLocalAccountDescription).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGLHeaderFilter|LocalAccountDescription", "Local Account Description");

			if (AccountingMasterFilesUtils.HasGLAccountSelectionAndEntry)
			{
				filters.AddTextFilter("Alternate Account", GetAlternateAccount)
					.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGLHeaderFilter|AlternateAccount", "Alternate Account");
				filters.AddTextFilter("Alternate Account Name", GetAlternateAccountName)
					.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGLHeaderFilter|AlternateAccountName", "Alternate Account Name");
			}
		}

		ZQuery GetCompanyFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (value.IsEmpty)
			{
				return new ZQuery();
			}
			else if (value == "ALL")
			{
				return new ZQuery(AccGLHeaderSchema.AG_IsGlobal, true);
			}
			else
			{
				var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, comparisonOperator, value.SubstringSafe(0, GlbCompanySchema.GC_Code.MaxLength)));
				if (company != null)
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccGLHeader));
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(AccGLHeaderCompanyFilter), AccGLHeaderCompanyFilterSchema.ACF_AG_Header);
					subQuery.AddToFilter(AccGLHeaderCompanyFilterSchema.ACF_GC_Company, company.PK);
					query.AddSubQuery(subQuery, JoinCondition.And);
					query.AddToFilter(JoinCondition.And, AccGLHeaderSchema.AG_IsGlobal, false);

					return query;
				}
				else
				{
					return ZQuery.NoResultQuery;
				}
			}
		}

		ZQuery GetLocalAccountCode(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetLocalAccountCodeAndDescription(comparisonOperator, value, AccGLAccountDescriptorSchema.AJ_LocalAccountNumber);
		}

		ZQuery GetLocalAccountDescription(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetLocalAccountCodeAndDescription(comparisonOperator, value, AccGLAccountDescriptorSchema.AJ_AccountDescription);
		}

		ZQuery GetLocalAccountCodeAndDescription(SQLComparisonOperator comparisonOperator, ZString value, SchemaStringColumn column)
		{
			if (value.IsEmpty)
			{
				return new ZQuery();
			}
			else
			{
				var descriptionQuery = new ZDBOnlyQuery(typeof(AccGLAccountDescriptor));
				descriptionQuery.AddToFilter(column, comparisonOperator, value.Substring(0, column.MaxLength));
				descriptionQuery.AddToFilter(AccGLAccountDescriptorSchema.AJ_Language, GlbStaff.CurrentUser.GS_WorkingLanguage);
				descriptionQuery.AddToFilter(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				descriptionQuery.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, AccGLAccountDescriptor.ReportTypeCOA);
				var descriptorPKs = Factory.Load<AccGLAccountDescriptor>(descriptionQuery).Select(x => x.ParentGLHeaderPK);

				if (descriptorPKs.Any())
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccGLHeader));
					query.AddToFilter(AccGLHeaderSchema.PK, descriptorPKs);

					return query;
				}
				else
				{
					return ZQuery.NoResultQuery;
				}
			}
		}

		ZQuery GetAlternateAccount(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetAlternateAccountAndName(comparisonOperator, value, AccAlternateGLAccountSchema.AGA_AccountNum);
		}

		ZQuery GetAlternateAccountName(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetAlternateAccountAndName(comparisonOperator, value, AccAlternateGLAccountSchema.AGA_Description);
		}

		ZQuery GetAlternateAccountAndName(SQLComparisonOperator comparisonOperator, ZString value, SchemaStringColumn column)
		{
			if (!value.IsEmpty)
			{
				var query = new ZDBOnlyQuery(typeof(AccAlternateGLAccount));
				query.AddToFilter(column, comparisonOperator, value);
				var alternateGLAccounts = Factory.Load<AccAlternateGLAccount>(query);
				var glHeaderPKs = alternateGLAccounts
					.Where(x => x.AGA_AAC_AlternateChart == AccountingMasterFilesRegistry.Instance.GLAccountSelectionAndEntry.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty))
					.SelectMany(x => x.AlternateGLAccountAttributes.Cast<AccAlternateGLAccountAttribute>().Select(y => y.AAA_AG_GLHeader));

				if (glHeaderPKs.Any())
				{
					query = new ZDBOnlyQuery(typeof(AccGLHeader));
					query.AddToFilter(AccGLHeaderSchema.PK, glHeaderPKs);

					return query;
				}
			}

			return ZQuery.NoResultQuery;
		}

		#endregion

		#region Status

		void AddStatusFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter controlStatusFilter = filters.AddTextFilter("Control Status", GetControlStatusFilter, ControlStatus);
			controlStatusFilter.Category = FilterCategories.StatusAndFlags;
			controlStatusFilter.DefaultProperty = "ALL";
			controlStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGLHeaderFilter|ControlStatus", "Control Status");
			showUnmappedAccountsFilter = filters.AddFlagsFilter("Unmapped accounts", new string[] { Res.GetString("Accounting|AccGLHeaderFilter|ShowUnmappedAccountsOnly", "Show unmapped accounts only") }, new GetFlagsQuery[] { SwitchShowUnmappedAccountsOnlyQuery });
			showUnmappedAccountsFilter.Category = FilterCategories.StatusAndFlags;
			showUnmappedAccountsFilter.DefaultProperties[Res.GetString("Accounting|AccGLHeaderFilter|ShowUnmappedAccountsOnly", "Show unmapped accounts only")] = false;
			showUnmappedAccountsFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGLHeaderFilter|UnmappedAccounts", "Unmapped accounts");

			ModuleTextFilter globalStatusFilter = filters.AddTextFilter("Global Status", GetGlobalStatusFilter, GlobalStatus);
			globalStatusFilter.Category = FilterCategories.StatusAndFlags;
			globalStatusFilter.DefaultProperty = "ALL";
			globalStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGLHeaderFilter|GlobalStatus", "Global Status");
		}

		ZQuery GetControlStatusFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			if (value == "CONTROL")
			{
				query.AddToFilter(AccGLHeaderSchema.AG_ControlAccount, ZBool.True);
			}
			else if (value == (ZString)(NoResString)"NOT CONTROL")
			{
				query.AddToFilter(AccGLHeaderSchema.AG_ControlAccount, ZBool.False);
			}

			return query;
		}

		ZQuery SwitchShowUnmappedAccountsOnlyQuery(ZBool value)
		{
			return new ZQuery();
		}

		ModuleFlagsFilter showUnmappedAccountsFilter;

		ZQuery GetGlobalStatusFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			if (value == "GLOBAL")
			{
				query.AddToFilter(AccGLHeaderSchema.AG_IsGlobal, ZBool.True);
			}
			else if (value == (ZString)(NoResString)"NOT GLOBAL")
			{
				query.AddToFilter(AccGLHeaderSchema.AG_IsGlobal, ZBool.False);
			}

			return query;
		}

		#endregion

		#region Other

		void AddOtherFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter accountTypeFilter = filters.AddTextFilter("Account Type", GetAccountTypeFilter, AccountTypes);
			accountTypeFilter.Category = FilterCategories.Other;
			accountTypeFilter.DefaultProperty = "ALL";
			accountTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGLHeaderFilter|AccountType", "Account Type");

			ModuleTextFilter subAccountTypeFilter = filters.AddTextFilter("Sub Account Type", GetSubAccountTypeFilter, SubAccountTypes);
			subAccountTypeFilter.Category = FilterCategories.Other;
			subAccountTypeFilter.DefaultProperty = ALL;
			subAccountTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGLHeaderFilter|SubAccountType", "Sub Account Type");

			ModuleTextFilter languageFilter = filters.AddTextFilter("Language", GetLanguageQuery, Languages);
			languageFilter.Category = FilterCategories.Other;
			languageFilter.DefaultProperty = "ANY";
			languageFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGLHeaderFilter|Language", "Language");

			ModuleTextFilter cashFlowTypeFilter = filters.AddTextFilter("Cash Flow Type", GetCashFlowTypeFilter, CashFlowTypes);
			cashFlowTypeFilter.Category = FilterCategories.Other;
			cashFlowTypeFilter.DefaultProperty = "ALL";
			cashFlowTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGLHeaderFilter|CashFlowType", "Cash Flow Type");
		}

		ZQuery GetAccountTypeFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			if (value != "ALL")
			{
				query.AddToFilter(AccGLHeaderSchema.AG_AccountType, value);
			}

			return query;
		}

		ZQuery GetSubAccountTypeFilter(ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccGLHeader));
			if (value != ALL)
			{
				ZDBOnlySubQuery subAccountQuery = new ZDBOnlySubQuery(typeof(AccGLHeaderSubAccount), AccGLHeaderSubAccountSchema.ASA_AG);
				subAccountQuery.AddToFilter(AccGLHeaderSubAccountSchema.ASA_SubClass, SubAccountCodeConverter.ConvertSubClassCodeToSubAccountDBParentTableCode(value));
				query.AddSubQuery(subAccountQuery, JoinCondition.And);
			}

			return query;
		}

		ZQuery GetLanguageQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			if (value != "ANY")
			{
				if (showUnmappedAccountsFilter.IsActive && showUnmappedAccountsFilter.Property0)
				{
					ZDBOnlyQuery glHeaderQuery = new ZDBOnlyQuery(typeof(AccGLHeader));
					ZString filterString = "(" + AccGLHeader.Schema.PK + " NOT IN (SELECT "
						+ AccGLDescriptorPivot.Schema.YJ_AG + " FROM " + AccGLDescriptorPivotSchema.Constants.SqlSchemaName + "." + AccGLDescriptorPivotSchema.Constants.TableName
						+ " INNER JOIN " + AccGLAccountDescriptorSchema.Constants.SqlSchemaName + "." + AccGLAccountDescriptorSchema.Constants.TableName + " ON " +
						AccGLDescriptorPivot.Schema.YJ_AJ + " = " + AccGLAccountDescriptor.Schema.PK +
						" WHERE " + AccGLDescriptorPivot.Schema.YJ_AG + " IS NOT NULL AND "
						+ AccGLAccountDescriptor.Schema.AJ_Language + " = " + "@Language ) AND ( "
						+ AccGLHeader.Schema.AG_AccountType + " = " + "@ProfitAndLoss"
						+ " OR " + AccGLHeader.Schema.AG_AccountType + " = " + "@BalanceSheet"
						+ "))";
					ZSqlParameterCollection sqlParams = new ZSqlParameterCollection();
					sqlParams.Add("@Language", value, AccGLAccountDescriptorSchema.AJ_Language);
					sqlParams.Add("@ProfitAndLoss", AccountTypeComboBoxConstants.ProfitAndLossAccount, AccGLHeaderSchema.AG_AccountType);
					sqlParams.Add("@BalanceSheet", AccountTypeComboBoxConstants.BalanceSheetAccount, AccGLHeaderSchema.AG_AccountType);
					glHeaderQuery.AddFilterAndZSQLParameterCollection(filterString, sqlParams);
					query.AddToFilter(glHeaderQuery, JoinCondition.And);
				}
				else
				{
					ZDBOnlyQuery glHeaderQuery = new ZDBOnlyQuery(typeof(AccGLHeader));
					ZDBOnlySubQuery glDesPivotrQuery = new ZDBOnlySubQuery(typeof(AccGLDescriptorPivot), AccGLDescriptorPivotSchema.YJ_AG);
					ZDBOnlySubQuery glDescriptorQuery = new ZDBOnlySubQuery(typeof(AccGLAccountDescriptor), AccGLAccountDescriptorSchema.PK);
					glDescriptorQuery.AddToFilter(JoinCondition.And, AccGLAccountDescriptorSchema.AJ_Language, SQLComparisonOperator.Equal, value);
					glDesPivotrQuery.AddSubQuery(AccGLDescriptorPivotSchema.YJ_AJ, glDescriptorQuery, JoinCondition.And);

					glHeaderQuery.AddSubQuery(AccGLHeaderSchema.PK, glDesPivotrQuery, JoinCondition.And);
					query.AddToFilter(glHeaderQuery, JoinCondition.And);
				}
			}

			return query;
		}

		ZQuery GetCashFlowTypeFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			if (value != "ALL")
			{
				query.AddToFilter(AccGLHeaderSchema.AG_CashFlowType, value);
			}

			return query;
		}

		#endregion

		#endregion

		#region Lookups

		#region AccountTypes List

		CodeDescriptionPairList AccountTypes
		{
			get
			{
				if (fAccountTypes == null)
				{
					fAccountTypes = new CodeDescriptionPairList();
					fAccountTypes.AddPair("ALL", Res.GetString("Accounting|AccGLHeaderFilter|All", "All"));
					fAccountTypes.AddPair(Core.Constants.AccountType.BalanceSheetAccount, Res.GetString("Accounting|AccGLHeaderFilter|BalanceSheet", "Balance Sheet"));
					fAccountTypes.AddPair(Core.Constants.AccountType.ProfitAndLossAccount, Res.GetString("Accounting|AccGLHeaderFilter|ProfitLoss", "Profit & Loss"));
					fAccountTypes.AddPair(Core.Constants.AccountType.Total, Res.GetString("Accounting|AccGLHeaderFilter|TotalAccount", "Total Account"));
					fAccountTypes.AddPair(Core.Constants.AccountType.Header, Res.GetString("Accounting|AccGLHeaderFilter|Header", "Header"));
					fAccountTypes.AddPair(Core.Constants.AccountType.Consolidation, Res.GetString("Accounting|AccGLHeaderFilter|ConsolidatedAccount", "Consolidated Account"));
					fAccountTypes.AddPair(Core.Constants.AccountType.Alternate, Res.GetString("Accounting|AccGLHeaderFilter|AlternateAccount", "Alternate Account"));
					fAccountTypes.AddPair(Core.Constants.AccountType.Note, Res.GetString("Accounting|AccGLHeaderFilter|Note", "Note"));
					fAccountTypes.AddPair(Core.Constants.AccountType.Undefined, Res.GetString("Accounting|AccGLHeaderFilter|Undefined", "Undefined / Invalid"));
				}
				return fAccountTypes;
			}
		}

		CodeDescriptionPairList fAccountTypes;

		#endregion

		#region AccountTypes List

		CodeDescriptionPairList SubAccountTypes
		{
			get
			{
				if (subAccountTypes == null)
				{
					subAccountTypes = new AccountingMasterFilesConstants.SubAccountTypeList();
					subAccountTypes.Insert(0, new CodeDescriptionPair(ALL, Res.GetString("3527b82b-efd8-46ba-bfd7-e0b86242daa0", "All")));
				}
				return subAccountTypes;
			}
		}

		CodeDescriptionPairList subAccountTypes;

		#endregion

		#region CashFlowTypes List

		CodeDescriptionPairList CashFlowTypes
		{
			get
			{
				if (fCashFlowTypes == null)
				{
					fCashFlowTypes = new CodeDescriptionPairList();
					fCashFlowTypes.AddPair("ALL", Res.GetString("Accounting|AccGLHeaderFilter|All", "All"));
					foreach (CashFlowActivityConfiguration cashFlowActivity in AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration.Value)
					{
						fCashFlowTypes.AddPair(cashFlowActivity.Code, cashFlowActivity.Description);
					}
				}
				return fCashFlowTypes;
			}
		}

		CodeDescriptionPairList fCashFlowTypes;

		#endregion

		#region ControlStatus List

		CodeDescriptionPairList ControlStatus
		{
			get
			{
				if (fControlStatus == null)
				{
					fControlStatus = new CodeDescriptionPairList();
					fControlStatus.AddPair("ALL", Res.GetString("Accounting|AccGLHeaderFilter|All", "All"));
					fControlStatus.AddPair("CONTROL", Res.GetString("Accounting|AccGLHeaderFilter|Control", "Control"));
					fControlStatus.AddPair((NoResString)"NOT CONTROL", Res.GetString("Accounting|AccGLHeaderFilter|NotControl", "Not Control"));
				}

				return fControlStatus;
			}
		}

		CodeDescriptionPairList fControlStatus;

		#endregion

		#region GlobalStatus List

		CodeDescriptionPairList GlobalStatus
		{
			get
			{
				if (fGlobalStatus == null)
				{
					fGlobalStatus = new CodeDescriptionPairList();
					fGlobalStatus.AddPair("ALL", Res.GetString("Accounting|AccGLHeaderFilter|All", "All"));
					fGlobalStatus.AddPair("GLOBAL", Res.GetString("Accounting|AccGLHeaderFilter|Global", "Global"));
					fGlobalStatus.AddPair((NoResString)"NOT GLOBAL", Res.GetString("Accounting|AccGLHeaderFilter|NotGlobal", "Not Global"));
				}

				return fGlobalStatus;
			}
		}

		CodeDescriptionPairList fGlobalStatus;

		#endregion

		#region Languages List

		public CodeDescriptionPairList Languages
		{
			get
			{
				if (fLanguages == null)
				{
					fLanguages = new CodeDescriptionPairList(OLookUpEditType.Language);
					fLanguages.Insert(0, new CodeDescriptionPair("ANY", Res.GetString("Accounting|AccGLHeaderFilter|AnyLanguage", "Any Language")));
				}

				return fLanguages;
			}
		}

		CodeDescriptionPairList fLanguages;

		#endregion

		#endregion

		const string ALL = "ALL";
	}
}
