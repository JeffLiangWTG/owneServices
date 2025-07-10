//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccTaxConfigurationLookups
//
//    This class should be used for overriding collections in AutoAccTaxConfigurationLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.MasterFiles.Business
{
	public class AccTaxConfigurationLookups : AutoAccTaxConfigurationLookups
	{
		public AccTaxConfigurationLookups(AutoAccTaxConfiguration parent) : base(parent)
		{
		}

		public CodeDescriptionPairList Ledger => new TaxConfigurationLedgers();

		public CodeDescriptionPairList TaxRealisationMethods => new TaxRealisationMethods();

		public CodeDescriptionPairList TaxRecordCreationTrigger => new TaxRecordCreationTrigger();

		public CodeDescriptionPairList CancellationPolicyMethods => new CancellationPolicyMethods();

		public CodeDescriptionPairList RecoveryMethods => new TaxRecoveryMethods();

		public CodeDescriptionPairList ETC_ThresholdMethods
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.Add(AccountingMasterFilesTaxFrameworkConstants.ETC_ThresholdMethods.NoThreshold);
				if (Parent.TaxSystem != null)
				{
					var taxSuperType = Parent.TaxSystem.TaxSuperType;
					if (taxSuperType == TaxSuperTypeList.Perceptions.Code
						|| taxSuperType == TaxSuperTypeList.RetentionInInvoice.Code
						|| taxSuperType == TaxSuperTypeList.ValueAddedTax.Code)
					{
						var isTransactionLevelGroupThresholdMethodSupported = false;
						var isTransactionLevelTaxBaseThresholdMethodSupported = true;

						var thresholdMethodProvider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Parent.ETC_RN_NKCountry) as IInstanceProvider<ITaxFrameworkThresholdMethodProvider>)?.Get();
						if (thresholdMethodProvider != null)
						{
							if (taxSuperType == TaxSuperTypeList.RetentionInInvoice.Code)
							{
								isTransactionLevelGroupThresholdMethodSupported = thresholdMethodProvider.IsTransactionLevelGroupThresholdMethodSupported;
							}
							isTransactionLevelTaxBaseThresholdMethodSupported = thresholdMethodProvider.IsTransactionLevelTaxBaseThresholdMethodSupported;
						}

						result.Add(AccountingMasterFilesTaxFrameworkConstants.ETC_ThresholdMethods.TransactionLevel);
						if (isTransactionLevelGroupThresholdMethodSupported)
						{
							result.Add(AccountingMasterFilesTaxFrameworkConstants.ETC_ThresholdMethods.TransactionLevelGroup);
						}

						if (isTransactionLevelTaxBaseThresholdMethodSupported)
						{
							result.Add(AccountingMasterFilesTaxFrameworkConstants.ETC_ThresholdMethods.TransactionLevelTaxBase);
						}
					}
				}
				return result;
			}
		}

		public CodeDescriptionPairList TaxAmountRoundingMethods => new TaxAmountRoundingMethods();

		public CodeDescriptionPairList TaxAuthorities
		{
			get
			{
				return ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetTaxFrameworkConfigurationHelper().GetTaxAuthorities(Parent.ETC_RN_NKCountry);
			}
		}

		public CodeDescriptionPairList TaxSystems
		{
			get
			{
				var parentOrigin = Parent.ETC_ParentTableCode == GlbCompanySchema.Constants.Prefix ? AccountingMasterFilesTaxFrameworkConstants.TaxSystemRegistrationLevels.Company.Code : AccountingMasterFilesTaxFrameworkConstants.TaxSystemRegistrationLevels.Branch.Code;
				return ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetTaxFrameworkConfigurationHelper().GetTaxSystems(Parent.ETC_RN_NKCountry, parentOrigin);
			}
		}

		public override AccGLHeaderCollection TaxControlAccounts => new AccGLHeaderCollection(Factory, GetQueryForGLAccounts());
		public override AccGLHeaderCollection TaxExpenseAccounts => new AccGLHeaderCollection(Factory, GetQueryForGLAccounts());
		public override AccGLHeaderCollection TaxPendingControlAccounts => new AccGLHeaderCollection(Factory, GetQueryForGLAccounts());

		ZQuery GetQueryForGLAccounts()
		{
			var company = Parent.Company;
			if (company == null)
			{
				return new ZQuery { IsNoResultQuery = true };
			}

			var query = new ZQuery(AccGLHeaderSchema.AG_AccountType, Core.Constants.AccountType.ProfitAndLossAccount);
			query.AddToFilter(JoinCondition.Or, AccGLHeaderSchema.AG_AccountType, SQLComparisonOperator.Equal, Core.Constants.AccountType.BalanceSheetAccount);
			query.AddToFilter(AccGLHeaderSchema.AG_ControlAccount, ZBool.False);
			query.AddToFilter(AccGLHeaderSchema.AG_IsActive, ZBool.True);

			var dbDBOnlyQuery = new ZDBOnlyQuery(typeof(AccGLHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(AccGLHeaderCompanyFilter), AccGLHeaderCompanyFilterSchema.ACF_AG_Header);
			subQuery.AddToFilter(AccGLHeaderCompanyFilterSchema.ACF_GC_Company, company.PK);
			dbDBOnlyQuery.AddSubQuery(subQuery, JoinCondition.And);
			dbDBOnlyQuery.AddToFilter(JoinCondition.Or, AccGLHeaderSchema.AG_IsGlobal, true);
			query.AddToFilter(dbDBOnlyQuery);

			return query;
		}

		new AccTaxConfiguration Parent
		{
			get { return (AccTaxConfiguration)base.Parent; }
		}
	}
}
