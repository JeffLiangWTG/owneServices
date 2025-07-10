using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefExchangeRateLookups : AutoRefExchangeRateLookups
	{
		public RefExchangeRateLookups(AutoRefExchangeRate parent)
			: base(parent)
		{
		}

		new RefExchangeRate Parent => (RefExchangeRate)base.Parent;

		#region Companies

		public virtual GlbCompanyCollection Companies
		{
			get { return new GlbCompanyCollection(Factory); }
		}

		#endregion

		#region ExRateTypes

		public CodeDescriptionPairList ExRateTypes => Factory.GetCachedValue("RefExchangeRateLookups.ExRateTypes", GetExchangeRateTypeList);

		public static CodeDescriptionPairList GetExchangeRateTypeList()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.CustomType);
			var systemLevelRates = AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.Value;
			var companyLevelRates = AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypesCompanySpecific.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var applicableCustomRates = new CodeDescriptionBoolCollection(systemLevelRates);
			applicableCustomRates.AddRange(companyLevelRates);

			foreach (var item in applicableCustomRates.Cast<CodeDescriptionBool>().Where(x => x.Bool))
			{
				switch (item.Code)
				{
					case Core.Constants.ExchangeRateTypes.Code.GlobalCreditControl:
						if (HasGlobalcreditSecurity)
						{
							list.AddPair(item.Code, item.Description);
						}
						break;
					case Core.Constants.ExchangeRateTypes.Code.CustomsRate:
					case Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary:
					case Core.Constants.ExchangeRateTypes.Code.CustomsMeasureEURExRate:
						if (Env.Security.CustomsExchangeRateUpdate.IsAllowed)
						{
							list.AddPair(item.Code, item.Description);
						}
						break;
					default:
						list.AddPair(item.Code, item.Description);
						break;
				}
			}

			return list;
		}

		static bool HasGlobalcreditSecurity => Env.Security.GCBExchangeRateUpdate.IsAllowed;

		#endregion

		#region Currencies

		public RefCurrencyCollection Currencies
		{
			get
			{
				if (Parent?.IsIndiaCustomsRate ?? false)
				{
					if (fCurrenciesIN_List == null)
					{
						var currencyCodes = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.Universal.IRefCusCodeListTypesListProvider>()
							.GetList(Factory, Core.Constants.CountryCodes.India, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.INCustomsStandardCurrency, ZDateTime.Today, null)
							.Cast<CargoWise.Integration.ICodeDescription>().Select(x => x.Code);

						fCurrenciesIN_List = new RefCurrencyCollection(Factory, new ZQuery(RefCurrencySchema.RX_Code, currencyCodes));
						fCurrenciesIN_List.ApplySort(RefCurrency.Schema.RX_Code, System.ComponentModel.ListSortDirection.Ascending);
					}
					return fCurrenciesIN_List;
				}
				else if (fCurrencies_List == null)
				{
					fCurrencies_List = new RefCurrencyCollection(Factory);
					fCurrencies_List.ApplySort(RefCurrency.Schema.RX_Code, System.ComponentModel.ListSortDirection.Ascending);
				}
				return fCurrencies_List;
			}
		}

		RefCurrencyCollection fCurrencies_List;

		RefCurrencyCollection fCurrenciesIN_List;

		#endregion

		#region LocalClients

		public OrganisationsFindBoxCollection LocalClients
		{
			get { return fOrganisations_List ?? (fOrganisations_List = new DebtorCollection(Factory)); }
		}

		OrganisationsFindBoxCollection fOrganisations_List;

		#endregion
	}
}
