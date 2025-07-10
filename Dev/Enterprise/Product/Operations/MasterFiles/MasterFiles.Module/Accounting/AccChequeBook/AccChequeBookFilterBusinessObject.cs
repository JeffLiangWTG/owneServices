using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class AccChequeBookFilterBusinessObject : FilterStripBusinessObject
	{
		public AccChequeBookFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddHiddenFilters(filters);
			AddOtherFilters(filters);

			return filters;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Code", AccChequeBookSchema.AK_Code).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccChequeBookFilter|Code", "Code");
			filters.AddTextFilter("Description", AccChequeBookSchema.AK_Desc).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccChequeBookFilter|Description", "Description");
		}

		#endregion

		#region Other

		void AddOtherFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter accountTypeFilter = filters.AddTextFilter("Auto Print", GetAutoPrintFilter, AutoPrintTypes);
			accountTypeFilter.Category = FilterCategories.Other;
			accountTypeFilter.DefaultProperty = "ALL";
			accountTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccChequeBookFilter|AutoPrint", "Auto Print");
		}

		ZQuery GetAutoPrintFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			if (!value.IsEmpty)
			{
				if (value == Core.Constants.AutoPrintTypes.AutoPrint)
				{
					query.AddToFilter(AccChequeBookSchema.AK_AutoPrintCheque, true);
				}
				else if (value == Core.Constants.AutoPrintTypes.Manual)
				{
					query.AddToFilter(AccChequeBookSchema.AK_AutoPrintCheque, false);
				}
			}

			return query;
		}

		#endregion

		#region Hidden

		void AddHiddenFilters(ModuleFilterCollection filters)
		{
			ModuleGuidFilter hiddenGuidFilter = filters.AddGuidFilter("Current Company Bank Accounts", ModuleIDs.AccBankAccount, AccBankAccountSchema.AB_GC, BankAccounts);
			hiddenGuidFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			hiddenGuidFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccChequeBookFilter|CurrentCompanyBankAccounts", "Current Company Bank Accounts");
			hiddenGuidFilter.Property = Env.CurrentCompany.PK;
			hiddenGuidFilter.SubGroup = new BankAccountsSubGroup();
		}

		class BankAccountsSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccChequeBook));
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(AccBankAccount), AccChequeBookSchema.AK_AB);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(subQuery, JoinCondition.And);

				return query;
			}
		}

		#endregion

		#endregion

		#region Lookups

		#region BankAccounts

		public AccBankAccountCollection BankAccounts
		{
			get
			{
				if (fBankAccounts == null)
				{
					fBankAccounts = new AccBankAccountCollection(Factory);
				}

				return fBankAccounts;
			}
		}

		AccBankAccountCollection fBankAccounts;

		public virtual CodeDescriptionPairList AutoPrintTypes
		{
			get
			{
				if (fAutoPrintTypes == null)
				{
					fAutoPrintTypes = new CodeDescriptionPairList(OLookUpEditType.AutoPrintTypes);
					fAutoPrintTypes.Insert(0, new CodeDescriptionPair("ALL", Res.GetString("Accounting|AccChequeBookFilter|All", "All")));
				}

				return fAutoPrintTypes;
			}
		}

		CodeDescriptionPairList fAutoPrintTypes;

		#endregion

		#endregion

	}
}
