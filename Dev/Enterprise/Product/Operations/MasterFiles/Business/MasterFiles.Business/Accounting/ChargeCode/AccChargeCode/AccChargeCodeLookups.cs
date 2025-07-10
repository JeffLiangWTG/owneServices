//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccChargeCodeLookups
//
//    This class should be used for overriding collections in AutoAccChargeCodeLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeCodeLookups : AutoAccChargeCodeLookups
	{
		public AccChargeCodeLookups(AutoAccChargeCode parent)
			: base(parent)
		{
			this.ChargeCode = (AccChargeCode)parent;
		}

		readonly AccChargeCode ChargeCode;

		public GoodServiceTypes GoodServiceTypes
		{
			get { return new GoodServiceTypes(); }
		}

		#region GST Tax Rates

		public AccTaxRateCollection TaxRateCollection
		{
			get
			{
				var company = ((AccChargeCode)Parent).Company;
				return new VATAccTaxRateCollection(Factory, company != null ? new ZQuery() : new ZQuery() { IsNoResultQuery = true }, company);
			}
		}

		#endregion

		#region Witholding Tax Rates

		public AccWithholdingCollection WithholdingCollection
		{
			get
			{
				var company = ((AccChargeCode)Parent).Company;
				return new AccWithholdingCollection(Factory, company);
			}
		}

		#endregion

		#region Sales Groups

		public AccGroupsCollection SalesGroupCollection
		{
			get { return new AccGroupsCollection(Factory, new ZQuery()); }
		}

		public override AccTaxOverrideGroupCollection TaxOverrideGroups
		{
			get
			{
				var query = new ZDBOnlyQuery(typeof(AccTaxOverrideGroup));
				var subQuery = new ZDBOnlySubQuery(typeof(AccTaxOverrideGroupTaxConfigurationPivot), AccTaxOverrideGroupTaxConfigurationPivotSchema.AXP_AX_TaxOverrideGroup, true);
				query.AddSubQuery(subQuery, JoinCondition.And);

				return new AccTaxOverrideGroupCollection(Factory, ((AccChargeCode)Parent).Company, query);
			}
		}

		#endregion

		#region Expense Groups

		public AccGroupsCollection ExpenseGroupCollection
		{
			get { return new AccGroupsCollection(Factory, new ZQuery()); }
		}

		#endregion

		#region Accounts

		#region GL Revenue Accounts

		public AccGLHeaderCollection GLRevenueAccountCollection
		{
			get
			{
				AccGLHeaderCollection fGLRevenueAccountCollection = new AccGLHeaderCollection(Factory, GetAccountCollectionFilter());
				return fGLRevenueAccountCollection;
			}
		}

		#endregion

		#region GL WIP Accounts

		public AccGLHeaderCollection GLWIPAccountCollection
		{
			get
			{
				AccGLHeaderCollection fGLWIPAccountCollection = new AccGLHeaderCollection(Factory, GetAccountCollectionFilter());
				return fGLWIPAccountCollection;
			}
		}

		#endregion

		#region GL Cost Accounts

		public AccGLHeaderCollection GLCostAccountCollection
		{
			get
			{
				AccGLHeaderCollection fGLCostAccountCollection = new AccGLHeaderCollection(Factory, GetAccountCollectionFilter());
				return fGLCostAccountCollection;
			}
		}

		#endregion

		#region GL Accrual Accounts

		public AccGLHeaderCollection GLAccrualAccountCollection
		{
			get
			{
				AccGLHeaderCollection fGLAccrualAccountCollection = new AccGLHeaderCollection(Factory, GetAccountCollectionFilter());
				return fGLAccrualAccountCollection;
			}
		}

		#endregion

		#region GL Disbursement Surplus Account

		public AccGLHeaderCollection GLDisbursementSurplusAccountCollection => new AccGLHeaderCollection(Factory, GetAccountCollectionFilter());

		#endregion

		#region GL Disbursement Shortfall Account

		public AccGLHeaderCollection GLDisbursementShortfallAccountCollection => new AccGLHeaderCollection(Factory, GetAccountCollectionFilter());

		#endregion

		#region Clearing Accounts

		public AccGLHeaderCollection GLCostClearingAccounts => new AccGLHeaderCollection(Factory, GetAccountCollectionFilter(true));
		public AccGLHeaderCollection GLRevenueClearingAccounts => new AccGLHeaderCollection(Factory, GetAccountCollectionFilter(true));

		#endregion

		#region Account Filter

		ZQuery GetAccountCollectionFilter(bool isForGLClearingAccount = false)
		{
			ZQuery orFilter = new ZQuery(AccGLHeaderSchema.AG_AccountType, Core.Constants.AccountType.BalanceSheetAccount);

			if (!isForGLClearingAccount)
			{
				orFilter.AddToFilter(JoinCondition.Or, AccGLHeaderSchema.AG_AccountType, SQLComparisonOperator.Equal, Core.Constants.AccountType.ProfitAndLossAccount);
			}

			ZQuery andFilter = new ZQuery(AccGLHeaderSchema.AG_ControlAccount, SQLComparisonOperator.NotEqual, true);
			andFilter.AddToFilter(orFilter, JoinCondition.And);

			if (ChargeCode.IsGlobal)
			{
				andFilter.AddToFilter(AccGLHeaderSchema.AG_IsGlobal, true);
			}
			else
			{
				ZDBOnlyQuery headerQuery = new ZDBOnlyQuery(typeof(AccGLHeader));
				ZDBOnlySubQuery companyFilterSubQuery = new ZDBOnlySubQuery(typeof(AccGLHeaderCompanyFilter), AccGLHeaderCompanyFilterSchema.ACF_AG_Header);
				companyFilterSubQuery.AddToFilter(AccGLHeaderCompanyFilterSchema.ACF_GC_Company, ChargeCode.AC_GC);
				headerQuery.AddSubQuery(companyFilterSubQuery, JoinCondition.And);
				headerQuery.AddToFilter(JoinCondition.Or, AccGLHeaderSchema.AG_IsGlobal, true);

				andFilter.AddToFilter(headerQuery);
			}

			if (isForGLClearingAccount)
			{
				andFilter.AddToFilter(AccGLHeaderSchema.AG_DisallowDirectPosting, false);
			}

			return new ZQuery(andFilter);
		}

		#endregion

		#endregion

		#region Charge Types

		public CodeDescriptionPairList AC_ChargeType_List
		{
			get
			{
				return Factory.GetCachedValue("AccChargeCodeLookups.AC_ChargeType_List",
					delegate
					{
						return new CodeDescriptionPairList(OLookUpEditType.ChargeTypes);
					}
				);
			}
		}

		#endregion

		#region Charge Groups

		public const string OriginAndLoadingGroupFilterCode = "OLC";
		public const string DestinationAndUnloadingGroupFilterCode = "DUC";
		public const string CFSGroupFilterCode = "CFS";
		public const string WHSGroupFilterCode = "WHS";
		public const string TRWGroupFilterCode = "TRW";
		public const string TWUGroupFilterCode = "TWU";
		public const string CYDGroupFilterCode = "CYD";
		public const string CYUGroupFilterCode = "CYU";

		public CodeDescriptionPairList ChargeGroupList
		{
			get { return chargeCodeGroupList ?? (chargeCodeGroupList = new ChargeCodeGroupList()); }
		}
		ChargeCodeGroupList chargeCodeGroupList;

		#endregion

		#region Charge Sub-Groups

		public CodeDescriptionPairList ChargeSubGroupList
			=> Factory.GetCachedValue("ChargeSubGroupList" + ChargeCode.AC_ChargeGroup + (!ChargeCode.AC_GC.IsEmpty ? ChargeCode.AC_GC.ToString() : string.Empty),
				() => ChargeCodeSubGroupList.GetList(ChargeCode.AC_ChargeGroup, ChargeCode.AC_GC));

		#endregion

		#region Charge Other Groups

		public CodeDescriptionPairList ChargeOtherGroupsList
		{
			get { return new ChargeOtherGroupsList(); }
		}

		#endregion

		#region Rate Calculators

		public CodeDescriptionPairList AC_RateCalculator_List
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.RateCalculators); }
		}

		#endregion

		#region AC_IATACode_List

		public CodeDescriptionPairList AC_IATACode_List
		{
			get { return new UntranslatableCodeDescriptionPairList((NoResString)"IATA AWB values cannot be translated", OLookUpEditType.AWBChargeCodes); }
		}

		#endregion

		#region Commission Products

		public ReadOnlyCodeDescriptionPairList CommissionProducts
		{
			get { return CommissionLookups.New(Factory).GetProducts(); }
		}

		#endregion

		#region Commission Services

		public ReadOnlyCodeDescriptionPairList CommissionServices
		{
			get { return CommissionLookups.New(Factory).GetServices(ChargeCode.AC_DefaultCommissionProduct); }
		}

		#endregion

		#region Commission Sub-Modules

		public ReadOnlyCodeDescriptionPairList CommissionSubModules
		{
			get { return CommissionLookups.New(Factory).GetSubModules(ChargeCode.AC_DefaultCommissionProduct, ChargeCode.AC_DefaultCommissionService); }
		}

		#endregion

		#region ConsolidationAccountingCategoryClassCollection
		public CodeDescriptionPairList ConsolidationAccountingCategoryClassCollection
		{
			get { return new ConsolidatedAccountingCategoryClassList(); }
		}
		#endregion
	}
}
