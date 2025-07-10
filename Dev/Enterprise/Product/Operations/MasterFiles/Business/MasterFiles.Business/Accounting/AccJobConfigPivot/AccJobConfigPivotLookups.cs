using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccJobConfigPivotLookups : AutoAccJobConfigPivotLookups
	{
		public AccJobConfigPivotLookups(AutoAccJobConfigPivot parent) : base(parent)
		{
		}

		public new ExchangeRateCurrencyConfiguration Parent => (ExchangeRateCurrencyConfiguration)base.Parent;
		#region ExRateTypeList

		public CodeDescriptionPairList ExRateTypeList => GetExRateTypeList();

		CodeDescriptionPairList GetExRateTypeList()
		{
			var result = new CodeDescriptionPairList();

			var registryDataSystemLevel = AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.Value;
			var registryDataApplicable = new CodeDescriptionBoolCollection();

			switch (Parent.Level)
			{
				case AccExRateConfigurationLevelEnum.System:
					{
						registryDataApplicable.AddRange(registryDataSystemLevel);
						break;
					}
				case AccExRateConfigurationLevelEnum.Company:
				case AccExRateConfigurationLevelEnum.Creditor:
				case AccExRateConfigurationLevelEnum.CreditorGroup:
				case AccExRateConfigurationLevelEnum.Debtor:
				case AccExRateConfigurationLevelEnum.DebtorGroup:
					{
						var registryDataCompanyLevel = AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypesCompanySpecific.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
						registryDataApplicable.AddRange(registryDataSystemLevel);
						registryDataApplicable.AddRange(registryDataCompanyLevel);
						break;
					}
				default:
					break;
			}

			foreach (var item in registryDataApplicable
									.Cast<CodeDescriptionBool>()
									.Where(item => item.Bool && CheckValidExRateType(item.Code)))
			{
				result.AddPair(item.Code, item.Description);
			}

			return result;

			bool CheckValidExRateType(ZString exRateTypeCode)
			{
				ZString[] exRateTypeAllowList =	{
				Constants.ExchangeRateTypes.Code.BuyRate,
				Constants.ExchangeRateTypes.Code.SellRate,
				Constants.ExchangeRateTypes.Code.CustomsRate,
				Constants.ExchangeRateTypes.Code.PeriodEndRate,
				Constants.ExchangeRateTypes.Code.IATARate };

				if (exRateTypeAllowList.Contains(exRateTypeCode))
				{
					return true;
				}

				var exRateTypeRegex = new Regex(@"^(?:[CL](?!00)\d{2})$");

				if (exRateTypeRegex.IsMatch(exRateTypeCode))
				{
					return true;
				}

				return false;
			}
		}

		#endregion

		#region Currencies

		public RefCurrencyCollection Currencies
		{
			get
			{
				return new RefCurrencyCollection(Factory);
			}
		}

		#endregion
	}
}
