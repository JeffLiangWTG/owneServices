using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CusChargeInfo : ICusCharge
	{
		public CusChargeInfo(RefCurrency currency, ZDateTime dateOfValuation)
		{
			if (currency == null)
			{
				throw new ArgumentNullException(nameof(currency));
			}

			if (!dateOfValuation.IsValid)
			{
				throw new ArgumentException("Date of Valuation is Invalid");
			}

			this.currency = currency;
			this.dateOfValuation = dateOfValuation;
		}

		#region ICusCharge Members

		public ZDecimal Amount
		{
			get { return amount; }
			set { amount = value; }
		}
		ZDecimal amount;

		public ZDecimal Percentage
		{
			get { return percentage; }
			set { percentage = value; }
		}
		ZDecimal percentage;

		public ZString CurrencyCode
		{
			get { return Currency != null ? Currency.RX_Code : ZString.Empty; }
		}

		public ZDecimal ExchangeRate
		{
			get { return Currency == null ? ZDecimal.Zero : CurrencyConverter.GetExchangeRate(Currency); }
		}

		#endregion

		#region Implementation

		public RefCurrency Currency
		{
			get { return currency; }
		}
		readonly RefCurrency currency;

		public void Add(Money money)
		{
			Money convertedCharge = CurrencyConverter.ConvertExact(money, Currency);
			Amount += convertedCharge.Amount;
		}

		CurrencyConverter CurrencyConverter
		{
			get { return currencyConverter ?? (currencyConverter = CurrencyConverter.New(new BusinessObjectFactory(), dateOfValuation, Enterprise.ZArchitecture.Core.ExchangeRateType.Customs, 0)); }
		}
		CurrencyConverter currencyConverter;

		readonly ZDateTime dateOfValuation;

		#endregion
	}
}
