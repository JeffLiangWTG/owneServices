using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	public class AccAlternateChartCurrencyTranslationLookups : AutoAccAlternateChartCurrencyTranslationLookups
	{
		public AccAlternateChartCurrencyTranslationLookups(AutoAccAlternateChartCurrencyTranslation parent) : base(parent)
		{
		}

		public CodeDescriptionPairList AccounTypeList
		{
			get
			{
				if (accounTypeList == null)
				{
					accounTypeList = BaseLookUps.BSHPAndLAccountTypeList;
				}

				return accounTypeList;
			}
		}
		CodeDescriptionPairList accounTypeList;

		public CodeDescriptionPairList CurrencyTranslationLevelList
		{
			get
			{
				if (currencyTranslationLevelList == null)
				{
					currencyTranslationLevelList = AccountingMasterFilesConstants.CurrencyTranslationLevelList;
				}

				return currencyTranslationLevelList;
			}
		}
		CodeDescriptionPairList currencyTranslationLevelList;

		public CodeDescriptionPairList ExRateTypeList
		{
			get
			{
				if (exRateTypes == null)
				{
					exRateTypes = AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.Value.GetActiveCodeDescriptionPairList();
					var companyLevelRateTypes = AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypesCompanySpecific.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty).GetActiveCodeDescriptionPairList();
					exRateTypes.AddRange(companyLevelRateTypes);
					exRateTypes.RemoveCode(ExchangeRateTypes.Code.IATARate);
					exRateTypes.RemoveCode(ExchangeRateTypes.Code.CustomsRate);
					exRateTypes.RemoveCode(ExchangeRateTypes.Code.CustomsRateSecondary);
					exRateTypes.RemoveCode(ExchangeRateTypes.Code.CustomsMeasureEURExRate);
				}

				return exRateTypes;
			}
		}
		CodeDescriptionPairList exRateTypes;

		public override AccAlternateGLAccountCollection AlternateAccounts
		{
			get
			{
				return new AccAlternateGLAccountCollection(Factory, new ZQuery(AccAlternateGLAccountSchema.AGA_AAC_AlternateChart, accAlternateChartCurrencyTranslation.ART_AAC_AlternateChart));
			}
		}

		BaseAlternateGLAccountLookups BaseLookUps => baseLookUps ?? (baseLookUps = new BaseAlternateGLAccountLookups(Parent.Factory));
		BaseAlternateGLAccountLookups baseLookUps;

		AccAlternateChartCurrencyTranslation accAlternateChartCurrencyTranslation => Parent as AccAlternateChartCurrencyTranslation;
	}
}
