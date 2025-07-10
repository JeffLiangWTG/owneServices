using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class AccBankAccountFilterBusinessObject : FilterStripBusinessObject
	{
		public AccBankAccountFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();
			AddTextFilters(result);
			AddRelatedItemFilters(result);
			AddHiddenFilters(result);

			return result;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Bank Code", AccBankAccountSchema.AB_Code).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccBankAccountFilter|BankCode", "Bank Code");
			filters.AddTextFilter("Account Number", AccBankAccountSchema.AB_AccountNum).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccBankAccountFilter|AccountNumber", "Account Number");
			filters.AddTextFilter("Description", AccBankAccountSchema.AB_Desc).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccBankAccountFilter|Description", "Description");
			filters.AddTextFilter("Bank Name", AccBankAccountSchema.AB_BankName).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccBankAccountFilter|BankName", "Bank Name");
			filters.AddTextFilter("Account Type", AccBankAccountSchema.AB_AccountType, new AccountTypeCodeDescriptionPairList()).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccBankAccountFilter|AccountType", "Account Type");
		}

		#endregion

		#region Related Items

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			ModuleGuidFilter branchFilter = filters.AddGuidFilter("Branch", ModuleIDs.GlbBranch, AccBankAccountSchema.AB_GB, Branches);
			branchFilter.Category = FilterCategories.Other;
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccBankAccountFilter|Branch", "Branch");

			ModuleNkFilter currencyNkFilter = filters.AddNkFilter("Currency", AccBankAccountSchema.AB_RX_NKAccountCurrency, ModuleIDs.RefCurrency, Currencies);
			currencyNkFilter.Category = FilterCategories.Other;
			currencyNkFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccBankAccountFilter|Currency", "Currency");
		}

		void AddHiddenFilters(ModuleFilterCollection filters)
		{
			ModuleGuidFilter hiddenGuidFilter = filters.AddGuidFilter("Company", ModuleIDs.GlbCompany, AccBankAccountSchema.AB_GC, Companies);
			hiddenGuidFilter.Property = GlbCompany.CurrentCompany.PK;
			hiddenGuidFilter.DefaultProperty = GlbCompany.CurrentCompany.PK;
			hiddenGuidFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			hiddenGuidFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccBankAccountFilter|Company", "Company");
		}

		#endregion

		#endregion

		#region Lookups

		public GlbBranchCollection Branches
		{
			get
			{
				if (fBranches == null)
				{
					fBranches = new GlbBranchCollection(Factory);
				}
				return fBranches;
			}
		}

		GlbBranchCollection fBranches;

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

		RefCurrencyCollection fCurrencies;

		GlbCompanyCollection Companies
		{
			get { return fCompanies ?? (fCompanies = new GlbCompanyCollection(Factory)); }
		}

		GlbCompanyCollection fCompanies;

		#endregion
	}
}
