using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	static class ValuationDateTracker
	{
		internal static ZDateTime GetEffectiveDate(ZDateTime recordedExRateDate, ZDateTime defaultValuationDate)
		{
			var result = defaultValuationDate;

			if (recordedExRateDate.IsValid && defaultValuationDate.IsValid
				&& recordedExRateDate < defaultValuationDate
				&& recordedExRateDate.AddDays(BaseJobDeclaration.CurrencyConverterMaximumDaysToFallBack) >= defaultValuationDate)
			{
				result = recordedExRateDate;
			}

			return result.IsValid ? result : ZDateTime.Today;
		}

		public static void SetLatestRateDate(JobDeclaration declaration, IEnumerable<ICurrencyProvider> currencyProviders)
		{
			if (declaration != null)
			{
				var actualValuationDate = ZDateTime.Empty;

				if (declaration.IsImport)
				{
					ZDateTime latestValuationDate = declaration.DateOfValuation_ExportDate;

					foreach (JobComInvoiceHeader invoice in declaration.Invoices)
					{
						if (latestValuationDate < invoice.ValuationDate_ExportDate)
						{
							latestValuationDate = invoice.ValuationDate_ExportDate;
						}
					}

					if (latestValuationDate.IsValid)
					{
						var latestRateDate = GetLatestRate(declaration, latestValuationDate, currencyProviders);

						if (latestValuationDate != latestRateDate)
						{
							actualValuationDate = latestRateDate;
						}
					}
				}

				declaration.US_LatestRateDate = actualValuationDate;
			}
		}

		internal static ZDateTime GetLatestRate(JobDeclaration declaration, ZDateTime date, IEnumerable<ICurrencyProvider> foreignCurrencyProviders)
		{
			var result = ZDateTime.Empty;
			var foreignCurrencyProvider = foreignCurrencyProviders.FirstOrDefault();
			var foreignCurrency = foreignCurrencyProvider != null ? foreignCurrencyProvider.CurrencyCode : ZString.Empty;

			if (!foreignCurrency.IsEmpty)
			{
				for (int i = 0; i <= BaseJobDeclaration.CurrencyConverterMaximumDaysToFallBack; i++)
				{
					var query = GetQuery(foreignCurrency, date.AddDays(-i), declaration.Company.PK);
					var rates = declaration.Factory.LoadTop1<RefExchangeRate>(query);

					if (rates != null)
					{
						result = date.AddDays(-i);
						break;
					}
				}
			}

			return result;
		}

		static ZQuery GetQuery(ZString foreignCurrency, ZDateTime date, ZGuid companyPK)
		{
			var validSmallDateTime = ZDateTime.GetValidSmallDateTime(date);
			var query = new ZQuery(RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, validSmallDateTime);
			query.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, validSmallDateTime);
			query.AddToFilter(RefExchangeRateSchema.RE_GC, companyPK);
			query.AddToFilter(RefExchangeRateSchema.RE_ExRateType, "CUS");
			query.AddToFilter(RefExchangeRateSchema.RE_RX_NKExCurrency, foreignCurrency);
			query.OrderBy = RefExchangeRateSchema.RE_StartDate.Name + " DESC";
			return query;
		}
	}
}
