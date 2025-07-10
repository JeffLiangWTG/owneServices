using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ExchangeRateTypeListProviderTest : TestCaseWithFactory
	{
		public void TestExchangeRateTypesProvider_Default()
		{
			var expectedList = AccountingMasterFilesConstants.GetDefaultExchangeRateTypesList();
			AssertContainsExactElementsInAnyOrder(expectedList, new ExchangeRateTypeListProvider().CodeDescriptionPairList);
		}

		public void TestGetDescriptionFromRateType()
		{
			var rateTypeDescription = new ExchangeRateTypeListProvider().GetDescriptionFromRateType(ExchangeRateType.Sell);

			AssertNotNullOrEmpty(rateTypeDescription);
			AssertEquals(Constants.ExchangeRateTypes.Description.SellRate, rateTypeDescription);
		}

		public void TestExchangeRateTypesProvider_OnlyBuyAndSellAndCustomsEnabled()
		{
			var newEnabledList = CreateListForExchangeRateTypesProvider_SystemLevel(item => item.Code == Constants.ExchangeRateTypes.Code.BuyRate
																			|| item.Code == Constants.ExchangeRateTypes.Code.SellRate
																			|| item.Code == Constants.ExchangeRateTypes.Code.CustomsRate);
			using (AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, newEnabledList))
			{
				var expectedList = new CodeDescriptionPairList()
				{
					new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.BuyRate, Constants.ExchangeRateTypes.Description.BuyRate),
					new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.SellRate, Constants.ExchangeRateTypes.Description.SellRate),
					new CodeDescriptionPair(Constants.ExchangeRateTypes.Code.CustomsRate, Constants.ExchangeRateTypes.Description.CustomsRate),
				};
				AssertContainsExactElementsInAnyOrder(expectedList, new ExchangeRateTypeListProvider().CodeDescriptionPairList);
			}
		}

		public void TestExchangeRateTypesProvider_AllEnabled()
		{
			var newEnabledList1 = CreateListForExchangeRateTypesProvider_SystemLevel(item => true);
			using (AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, newEnabledList1))
			{
				var newEnabledList2 = CreateListForExchangeRateTypesProvider_CompanyLevel(item => true);
				using (AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypesCompanySpecific.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newEnabledList2))
				{
					var expectedList = AccountingMasterFilesConstants.GetExchangeRateTypesList_SystemLevel();
					expectedList.AddRange(AccountingMasterFilesConstants.GetExchangeRateTypesList_CompanyLevel());
					AssertContainsExactElementsInAnyOrder(expectedList, new ExchangeRateTypeListProvider().CodeDescriptionPairList);
				}
			}
		}

		static CodeDescriptionBoolDisallowNewCodeReadOnlyValidationCollection CreateListForExchangeRateTypesProvider_SystemLevel(Func<CodeDescriptionPair, bool> isEnabled)
		{
			var newEnabledList = new CodeDescriptionBoolDisallowNewCodeReadOnlyValidationCollection();
			foreach (CodeDescriptionPair item in AccountingMasterFilesConstants.GetExchangeRateTypesList_SystemLevel())
			{
				var boolValue = isEnabled(item);
				newEnabledList.Add(item.Code, item.MultilingualDescription, boolValue);
			}
			return newEnabledList;
		}

		static CodeDescriptionBoolDisallowNewCodeReadOnlyValidationCollection CreateListForExchangeRateTypesProvider_CompanyLevel(Func<CodeDescriptionPair, bool> isEnabled)
		{
			var newEnabledList = new CodeDescriptionBoolDisallowNewCodeReadOnlyValidationCollection();
			foreach (CodeDescriptionPair item in AccountingMasterFilesConstants.GetExchangeRateTypesList_CompanyLevel())
			{
				var boolValue = isEnabled(item);
				newEnabledList.Add(item.Code, item.MultilingualDescription, boolValue);
			}
			return newEnabledList;
		}
	}
}
