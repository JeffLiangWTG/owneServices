using System;

namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public sealed class CurrencyRateSearchParamCurrencyRateWrapper : ICurrencyRateSearchParamCurrencyRate
	{
		public CurrencyRateSearchParamCurrencyRateWrapper(DateTime fromDate, DateTime toDate, string currencyTypeID)
		{
			this.fromDate = fromDate;
			this.toDate = toDate;
			this.currencyTypeID = currencyTypeID;
		}

		string ICurrencyRateSearchParamCurrencyRate.currencyTypeID => currencyTypeID;

		DateTime ICurrencyRateSearchParamCurrencyRate.fromDate => fromDate;

		DateTime ICurrencyRateSearchParamCurrencyRate.toDate => toDate;

		readonly DateTime fromDate;
		readonly DateTime toDate;
		readonly string currencyTypeID;
	}
}
