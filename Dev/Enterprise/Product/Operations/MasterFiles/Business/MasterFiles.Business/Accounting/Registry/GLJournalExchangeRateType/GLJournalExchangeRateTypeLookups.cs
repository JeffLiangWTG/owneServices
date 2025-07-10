using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class GLJournalExchangeRateTypeLookups
	{
		public GLJournalExchangeRateTypeLookups(ZGuid companyPK)
		{
			CompanyPK = companyPK;
		}

		public CodeDescriptionPairList ExchangeRateTypes
		{
			get
			{
				if (exchangeRateTypes == null)
				{
					exchangeRateTypes = AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.Value.GetActiveCodeDescriptionPairList();
					var companyLevelRateTypes = AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypesCompanySpecific.GetValueWithoutFallback(CompanyPK.ToGuid(), Guid.Empty, Guid.Empty).GetActiveCodeDescriptionPairList();
					exchangeRateTypes.AddRange(companyLevelRateTypes);
				}

				return exchangeRateTypes;
			}
		}
		CodeDescriptionPairList exchangeRateTypes;

		public ZGuid CompanyPK { get; }
	}
}
