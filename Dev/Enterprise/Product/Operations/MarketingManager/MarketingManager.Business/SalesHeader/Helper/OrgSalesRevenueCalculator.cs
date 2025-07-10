using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class OrgSalesRevenueCalculator
	{
		public OrgSalesRevenueCalculator(BusinessObjectFactory factory, ISalesValueAssociatedEntity salesAssociatedEntity)
		{
			Argument.NotNull(factory, "factory");

			this.factory = factory;
			this.salesAssociatedEntity = salesAssociatedEntity;
		}

		#region EntityCurrency

		public ZString EntityCurrencyCode
		{
			get
			{
				return salesAssociatedEntity != null ?
					salesAssociatedEntity.ValueCurrency :
					GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			}
		}

		public RefCurrency EntityCurrency
		{
			get { return factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, EntityCurrencyCode); }
		}

		#endregion

		#region GetTotalInEntityCurrency

		public void AddFetchHintsForView(IEnumerable<EntitySalesWrapper> salesWrappers, string columnName)
		{
			foreach (var sales in salesWrappers)
			{
				sales.FetchStrategy.FetchForView(new[] { new TableColumn("", columnName) });
			}
			foreach (var detail in salesWrappers.SelectMany(x => x.EntityTradeDetails))
			{
				detail.FetchStrategy.FetchForView(new[] { new TableColumn("", columnName) });
			}
		}

		public ZDecimal GetTotalInEntityCurrency<T>(IEnumerable<T> elements, Func<T, ZString> elementCurrencyGetter, Func<T, ZDecimal> elementAmountGetter)
		{
			var elementsGroupedByCurrency = elements.GroupBy(x => elementCurrencyGetter(x));
			foreach (var currencyGrouping in elementsGroupedByCurrency)
			{
				factory.AddFetchHint(RefCurrencySchema.RX_Code, currencyGrouping.Key);
			}

			ZDecimal amount = 0m;
			foreach (var currencyGrouping in elementsGroupedByCurrency)
			{
				var foreignCurrencyCode = currencyGrouping.Key;
				var foreignTotal = currencyGrouping.Sum(x => elementAmountGetter(x));

				if (foreignCurrencyCode == EntityCurrencyCode || foreignCurrencyCode.IsEmpty || EntityCurrencyCode.IsEmpty)
				{
					amount += foreignTotal;
				}
				else
				{
					var foreignCurrency = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, foreignCurrencyCode);
					amount += CurrencyConverter.ConvertRounded(new Money(foreignTotal, foreignCurrency), EntityCurrency).Amount;
				}
			}

			return amount;
		}

		#endregion

		#region Exchange Rate

		public ZDecimal GetExchangeRate(RefCurrency fromCurrency)
		{
			if (fromCurrency == null || EntityCurrencyCode.IsEmpty)
			{
				return 1;
			}
			else
			{
				var targetCurrencyExchangeRate = CurrencyConverter.GetExchangeRate(EntityCurrency);
				if (targetCurrencyExchangeRate == 0)
				{
					return 0;
				}

				if (fromCurrency.PK == CurrencyConverter.LocalCurrency.PK)
				{
					return 1 / targetCurrencyExchangeRate;
				}
				else
				{
					return CurrencyConverter.GetExchangeRate(fromCurrency) / targetCurrencyExchangeRate;
				}
			}
		}

		public GlbCompany CompanyForExchangeRate
		{
			get
			{
				if (companyForExchangeRate == null)
				{
					if (salesAssociatedEntity != null && salesAssociatedEntity.CompanyPk.HasValue)
					{
						companyForExchangeRate = factory.Load<GlbCompany>(salesAssociatedEntity.CompanyPk.Value);
					}

					if (companyForExchangeRate == null)
					{
						return GlbCompany.CurrentCompany;
					}
				}

				return companyForExchangeRate;
			}
		}
		GlbCompany companyForExchangeRate;

		public ZDateTime DateForExchangeRate
		{
			get
			{
				if (!dateForExchangeRate.HasValue)
				{
					if (salesAssociatedEntity == null || !salesAssociatedEntity.DateForExchangeRate.IsValid)
					{
						dateForExchangeRate = ZDateTime.Today;
					}
					else
					{
						dateForExchangeRate = salesAssociatedEntity.DateForExchangeRate;
					}
				}

				return dateForExchangeRate.Value;
			}
		}
		ZDateTime? dateForExchangeRate;

		public IEnumerable<RefCurrency> GetMissingExchangeRatesToConvertTo(RefCurrency currency)
		{
			if (currency != null)
			{
				if (EntityCurrency != null && CurrencyConverter.GetExchangeRate(EntityCurrency).IsEmpty)
				{
					yield return EntityCurrency;
				}

				if ((EntityCurrency == null || currency.PK != EntityCurrency.PK) && currency.PK != CurrencyConverter.LocalCurrency.PK)
				{
					if (CurrencyConverter.GetExchangeRate(currency).IsEmpty)
					{
						yield return currency;
					}
				}
			}
		}

		#endregion

		#region Implementation

		readonly BusinessObjectFactory factory;
		readonly ISalesValueAssociatedEntity salesAssociatedEntity;

		#region CurrencyConverter

		CurrencyConverter CurrencyConverter
		{
			get
			{
				if (currencyConverter == null)
				{
					currencyConverter = CurrencyConverter.New(CompanyForExchangeRate, factory, DateForExchangeRate, Enterprise.ZArchitecture.Core.ExchangeRateType.Sell, 30);
				}

				return currencyConverter;
			}
		}
		CurrencyConverter currencyConverter;

		#endregion

		#endregion
	}
}
