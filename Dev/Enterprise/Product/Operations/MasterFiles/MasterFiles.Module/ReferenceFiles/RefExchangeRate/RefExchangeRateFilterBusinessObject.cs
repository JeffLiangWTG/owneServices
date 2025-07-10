using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefExchangeRateFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			var filter = filters.AddNkFilter("Currency Code", RefExchangeRateSchema.RE_RX_NKExCurrency, ModuleIDs.RefCurrency, GetCurrencyCodeList);
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ExchangeRateFilter|CurrencyCode", "Currency Code");
			filter.Visibility = FilterVisibility.AlwaysVisible;
			filter.PropertyValidation += info => ListValidation.ErrorIfInvalidCode(info, GetCurrencyCodeList());

			var filter2 = filters.AddTextFilter("Exchange Rate Type", RefExchangeRateSchema.RE_ExRateType, GetExchangeRateTypeList);
			filter2.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ExchangeRateFilter|ExchangeRateType", "Exchange Rate Type");
			filter2.Visibility = FilterVisibility.AlwaysVisible;
			filter2.PropertyValidation += info => ListValidation.ErrorIfInvalidCode(info, GetExchangeRateTypeList());

			var dateFilter = filters.AddDateFilter("Start Date", RefExchangeRateSchema.RE_StartDate);
			dateFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ExchangeRateFilter|StartDate", "Start Date");
			dateFilter.Visibility = FilterVisibility.AlwaysVisible;

			filters.AddDateFilter("Expiry Date", RefExchangeRateSchema.RE_ExpiryDate).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ExchangeRateFilter|ExpiryDate", "Expiry Date");

			return filters;
		}

		RefCurrencyCollection GetCurrencyCodeList()
		{
			var currencies = new RefCurrencyCollection(Factory);
			currencies.ApplySort(RefCurrency.Schema.RX_Code, System.ComponentModel.ListSortDirection.Ascending);
			return currencies;
		}

		public static CodeDescriptionPairList GetExchangeRateTypeList()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.CustomType);
			var applicableRates = new CodeDescriptionPairList();
			applicableRates.AddRange(AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.Value);
			applicableRates.AddRange(AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypesCompanySpecific.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			foreach (var item in applicableRates.Cast<CodeDescriptionBool>().Where(x => x.Bool))
			{
				switch (item.Code)
				{
					case Core.Constants.ExchangeRateTypes.Code.GlobalCreditControl:
						if (Env.Security.GCBExchangeRateUpdate.IsAllowed)
						{
							list.AddPair(item.Code, item.Description);
						}
						break;
					case Core.Constants.ExchangeRateTypes.Code.CustomsRate:
					case Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary:
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
		public CodeDescriptionPairList ExRateTypesList => Factory.GetCachedValue("RefExchangeRateFilterBusinessObject.ExRateTypesList", GetExchangeRateTypeList);

		protected override string IsSystemDefinedDefaultProperty => ZString.Empty;
	}
}
