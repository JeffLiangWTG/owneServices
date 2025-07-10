using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public abstract class CurrencyConverter
	{
		protected CurrencyConverter(GlbCompany company, BusinessObjectFactory factory, ZDateTime dateForRate, ExchangeRateType rateType, bool roundToTargetCurrencyDecimals)
			: this(factory, roundToTargetCurrencyDecimals)
		{
			fCompany = company;
			this.DateForRate = dateForRate;
			this.RateType = rateType;
		}

		protected CurrencyConverter(GlbCompany company, BusinessObjectFactory factory, ZDateTime dateForRate, ExchangeRateType rateType)
			: this(company, factory, dateForRate, rateType, true) { }

		protected CurrencyConverter(BusinessObjectFactory factory, ZDateTime dateForRate, ExchangeRateType rateType)
			: this(GlbCompany.CurrentCompany, factory, dateForRate, rateType)
		{
		}

		protected CurrencyConverter(BusinessObjectFactory factory) : this(factory, true) { }

		protected CurrencyConverter(BusinessObjectFactory factory, bool roundToTargetCurrencyDecimals)
			: this(GlbCompany.CurrentCompany, factory, roundToTargetCurrencyDecimals) { }

		protected CurrencyConverter(GlbCompany company, BusinessObjectFactory factory) : this(company, factory, true) { }

		protected CurrencyConverter(GlbCompany company, BusinessObjectFactory factory, bool roundToTargetCurrencyDecimals)
		{
			fCompany = company;
			this.Factory = factory;
			RoundToTargetCurrencyDecimals = roundToTargetCurrencyDecimals;
		}

		bool RoundToTargetCurrencyDecimals { get; }

		public static CurrencyConverter New(BusinessObjectFactory factory, ZDateTime dateForRate, ExchangeRateType rateType, int maximumDaysToFallback)
		{
			return new RefCurrencyCurrencyConverter(factory, dateForRate, rateType, maximumDaysToFallback);
		}

		public static CurrencyConverter New(BusinessObjectFactory factory, ZDateTime dateForRate, ExchangeRateType rateType, bool roundToTargetCurrencyDecimals)
		{
			return new RefCurrencyCurrencyConverter(factory, dateForRate, rateType, roundToTargetCurrencyDecimals);
		}

		public static CurrencyConverter New(GlbCompany company, BusinessObjectFactory factory, ZDateTime dateForRate, ExchangeRateType rateType, int maximumDaysToFallback)
		{
			return new RefCurrencyCurrencyConverter(company, factory, dateForRate, rateType, maximumDaysToFallback);
		}

		public static CurrencyConverter New(BusinessObjectFactory factory)
		{
			return new RefCurrencyCurrencyConverter(factory);
		}

		public readonly BusinessObjectFactory Factory;

		protected virtual ZDateTime DateForRateCore
		{
			get { return dateForRate; }
			set
			{
				dateForRate = value;
			}
		}
		ZDateTime dateForRate;

		public ZDateTime DateForRate
		{
			get { return DateForRateCore; }
			set
			{
				DateForRateCore = value;
			}
		}

		protected virtual ExchangeRateType RateTypeCore
		{
			get { return rateType; }
			set
			{
				rateType = value;
			}
		}
		ExchangeRateType rateType;

		public ExchangeRateType RateType
		{
			get { return RateTypeCore; }
			set
			{
				RateTypeCore = value;
			}
		}

		public virtual int MaximumDaysToFallback
		{
			get { return maximumDaysToFallback; }
		}
		protected int maximumDaysToFallback;

		public GlbCompany Company
		{
			get
			{
				if (fCompany == null)
				{
					fCompany = GlbCompany.CurrentCompany;
				}

				return fCompany;
			}
		}

		GlbCompany fCompany;

		public RefCurrency LocalCurrency
		{
			get
			{
				return LocalCurrencyCodeOverride.IsEmpty ? Company.LocalCurrency : RefCurrency.LoadFromCurrencyCode(Factory, LocalCurrencyCodeOverride);
			}
		}

		public virtual ZString LocalCurrencyCodeOverride
		{
			get { return ZString.Empty; }
		}

		public ZBool IsReciprocal
		{
			get { return IsReciprocalOverride ?? Company.GC_IsReciprocal; }
		}

		public virtual ZBool? IsReciprocalOverride
		{
			get { return null; }
		}

		protected virtual ZBool IsConverterValidCore
		{
			get { return ZBool.True; }
		}

		public ZBool IsConverterValid
		{
			get { return IsConverterValidCore; }
		}

		public abstract ZDecimal GetExchangeRate(ICurrency currency);
		public abstract ZDecimal GetExchangeRate(ICurrency currency, out ZDateTime foundRateDate);

		public Money ConvertRounded(Money monetaryAmount, ICurrency destinationCurrency)
		{
			return ConvertExact(monetaryAmount, destinationCurrency, true);
		}

		public Money ConvertRounded(Money monetaryAmount, ZArchitecture.Environment.Currency destinationCurrency)
		{
			RefCurrency currency = (RefCurrency)(Factory.Load(typeof(RefCurrency), destinationCurrency.PK));
			return ConvertRounded(monetaryAmount, currency);
		}

		public Money ConvertExact(Money monetaryAmount, ZArchitecture.Environment.Currency destinationCurrency)
		{
			RefCurrency currency = (RefCurrency)(Factory.Load(typeof(RefCurrency), destinationCurrency.PK));
			return ConvertExact(monetaryAmount, currency);
		}

		public Money ConvertExact(Money monetaryAmount, ZArchitecture.Environment.Currency destinationCurrency, bool roundToDestinationCurrencyDecimals)
		{
			RefCurrency currency = (RefCurrency)(Factory.Load(typeof(RefCurrency), destinationCurrency.PK));
			return ConvertExact(monetaryAmount, currency, roundToDestinationCurrencyDecimals);
		}

		/// <summary>
		/// Converts an amount from the current currency to the destination currency for the current CurrencyConverter date and rate type.
		/// </summary>
		/// <param name="monetaryAmount"></param>
		/// <param name="destinationCurrency"></param>
		/// <param name="roundToDestinationCurrencyDecimals">Define whether the result will be rounded to the destination currency number of decimal places.</param>
		/// <returns></returns>
		public Money ConvertExact(Money monetaryAmount, ICurrency destinationCurrency, bool? roundToDestinationCurrencyDecimals = null)
		{
			return ConvertExact(monetaryAmount, destinationCurrency, currency => GetExchangeRate(currency), roundToDestinationCurrencyDecimals);
		}

		protected Money ConvertExact(Money monetaryAmount, ICurrency destinationCurrency, Func<ICurrency, ZDecimal> getExchangeRate, bool? roundToDestinationCurrencyDecimals)
		{
			Money resultAmount;
			if (destinationCurrency == null)
			{
				resultAmount = Money.Empty;
			}
			else if (monetaryAmount == null || monetaryAmount.Currency == null)
			{
				resultAmount = new Money(0, destinationCurrency);
			}
			else if (monetaryAmount.Currency.PK == destinationCurrency.PK)
			{
				resultAmount = monetaryAmount;
			}
			else if (monetaryAmount.Currency.PK == LocalCurrency.PK)
			{
				ZDecimal exchangeRate = getExchangeRate(destinationCurrency);
				resultAmount = ConvertUsingRate(monetaryAmount, destinationCurrency, exchangeRate, IsReciprocal);
			}
			else
			{
				var exRate = getExchangeRate(monetaryAmount.Currency);
				Money localAmount = ConvertUsingRate(monetaryAmount, LocalCurrency, exRate, !IsReciprocal);

				resultAmount = ConvertExact(localAmount, destinationCurrency, getExchangeRate, roundToDestinationCurrencyDecimals);
			}

			if ((roundToDestinationCurrencyDecimals ?? RoundToTargetCurrencyDecimals) && destinationCurrency != null)
			{
				resultAmount = Round(resultAmount, destinationCurrency);
			}

			return resultAmount;
		}

		Money Round(Money monetaryAmount, ICurrency destinationCurrency)
		{
			return new Money(ZArchitecture.Core.Utilities.Round(monetaryAmount.Amount, destinationCurrency.Decimals), destinationCurrency);
		}

		public Money Subtract(Money initialAmount, Money amountToBeSubtracted)
		{
			Money invertedAmount = new Money(-amountToBeSubtracted.Amount, amountToBeSubtracted.Currency, amountToBeSubtracted.IsValid);
			return Add(initialAmount, invertedAmount);
		}

		public Money Add(Money initialAmount, Money amountToBeAdded)
		{
			Money result;
			if (initialAmount == null || initialAmount.Currency == null || initialAmount.IsEmpty)
			{
				result = new Money(amountToBeAdded);
			}
			else if (amountToBeAdded == null || amountToBeAdded.Currency == null || amountToBeAdded.IsEmpty)
			{
				result = new Money(initialAmount);
			}
			else if (amountToBeAdded.Currency.Code == initialAmount.Currency.Code)
			{
				result = new Money(initialAmount.Amount + amountToBeAdded.Amount, initialAmount.Currency);
			}
			else
			{
				RefCurrency destinationCurrency = LocalCurrency;

				Money initialAmountInLocalCurrency = ConvertExact(initialAmount, destinationCurrency);
				Money amountToBeAddedInLocalCurrency = ConvertExact(amountToBeAdded, destinationCurrency);
				result = new Money(initialAmountInLocalCurrency.Amount + amountToBeAddedInLocalCurrency.Amount, LocalCurrency);

				if (RoundToTargetCurrencyDecimals)
				{
					result = Round(result, destinationCurrency);
				}
			}

			return result;
		}

		public Money Min(Money moneyA, Money moneyB)
		{
			if (ConvertExact(moneyA, LocalCurrency).Amount <= ConvertExact(moneyB, LocalCurrency).Amount)
			{
				return moneyA;
			}
			else
			{
				return moneyB;
			}
		}

		public Money Max(Money moneyA, Money moneyB)
		{
			if (ConvertExact(moneyA, LocalCurrency).Amount >= ConvertExact(moneyB, LocalCurrency).Amount)
			{
				return moneyA;
			}
			else
			{
				return moneyB;
			}
		}

		#region Implementation

		protected Money ConvertUsingRate(Money monetaryAmount, ICurrency destinationCurrency, decimal exchangeRate, bool divide)
		{
			Money result;

			if (exchangeRate != 0)
			{
				result = new Money(divide ? monetaryAmount.Amount / exchangeRate : monetaryAmount.Amount * exchangeRate, destinationCurrency);
			}
			else
			{
				result = new Money(0, destinationCurrency, false);
			}

			return result;
		}

		#endregion
	}
}
