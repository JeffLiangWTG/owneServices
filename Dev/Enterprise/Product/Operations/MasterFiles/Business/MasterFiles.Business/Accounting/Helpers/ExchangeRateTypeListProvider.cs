using System;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class ExchangeRateTypeListProvider : IRefExchangeRateTypes, ICodeDescriptionPairListProvider
	{
		public CodeDescriptionPairList CodeDescriptionPairList
		{
			get
			{
				var result = AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.Value.GetActiveCodeDescriptionPairList();
				var companyLevelRateTypes = AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypesCompanySpecific.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).GetActiveCodeDescriptionPairList();
				result.AddRange(companyLevelRateTypes);
				return result;
			}
		}

		public string GetDescriptionFromRateType(ExchangeRateType rateType)
		{
			var exchangeRateType = ExchangeRate.GetExchangeRateType(rateType);
			return CodeDescriptionPairList.GetDescriptionFromCode(exchangeRateType);
		}
	}
}
