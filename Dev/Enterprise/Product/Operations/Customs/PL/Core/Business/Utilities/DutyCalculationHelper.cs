using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.PL.Business;

public static class DutyCalculationHelper
{
	public static bool HasCUDExchangeRateOrSpecifiedDateIsInvalid(BusinessObjectFactory factory, ZDateTime exchangeRateDate) => !exchangeRateDate.IsValid || HasCUDExchangeRate(factory, exchangeRateDate);

	static bool HasCUDExchangeRate(BusinessObjectFactory factory, ZDateTime exchangeRateDate)
	{
		ZQuery sqlFilter = new ZQuery();
		sqlFilter.AddToFilter(RefExchangeRateSchema.RE_RX_NKExCurrency, Core.Constants.CurrencyCodes.EuropeanUnion);
		sqlFilter.AddToFilter(RefExchangeRateSchema.RE_GC, Env.CurrentCompany.PK);
		sqlFilter.AddToFilter(RefExchangeRateSchema.RE_ExRateType, Core.Constants.ExchangeRateTypes.Code.CustomsMeasureEURExRate);
		sqlFilter.AddToFilter(RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThanOrEqualTo, exchangeRateDate);
		sqlFilter.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualTo, exchangeRateDate);

		var rates = new RefExchangeRateCollection(factory);
		rates.AdditionalFilter = sqlFilter;

		return rates.Count >= 1;
	}
}
