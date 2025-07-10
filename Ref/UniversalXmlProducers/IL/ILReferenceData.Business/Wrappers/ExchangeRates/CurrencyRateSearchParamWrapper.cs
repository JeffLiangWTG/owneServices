using System;

namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public sealed class CurrencyRateSearchParamWrapper : ICurrencyRateSearchParam
	{
		public CurrencyRateSearchParamWrapper(DateTime transmissionDateTime, DateTime fromDate, DateTime toDate, string currencyTypeID)
		{
			this.transmissionDateTime = transmissionDateTime;
			this.fromDate = fromDate;
			this.toDate = toDate;
			this.currencyTypeID = currencyTypeID;
		}

		public ICurrencyRateSearchParamCurrencyRate CurrencyRate => new CurrencyRateSearchParamCurrencyRateWrapper(fromDate, toDate, currencyTypeID);

		public IRequestContentHeader RequestContentHeader => new RequestContentHeaderWrapper(transmissionDateTime);

		readonly DateTime transmissionDateTime;
		readonly DateTime fromDate;
		readonly DateTime toDate;
		readonly string currencyTypeID;
	}
}
