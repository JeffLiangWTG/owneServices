using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class Money : IMoney
	{
		#region Constructors

		public Money(Money moneyToCopy)
		{
			if (moneyToCopy != null)
			{
				Amount = moneyToCopy.Amount;
				fCurrency = moneyToCopy.Currency;
				IsValid = moneyToCopy.IsValid;
				source = moneyToCopy.Source;
			}
		}

		public Money(Money moneyToCopy, bool overrideIsValid)
			: this(moneyToCopy)
		{
			CheckValidForInstantiation(overrideIsValid);
			IsValid = moneyToCopy.IsValid && overrideIsValid;
		}

		public Money(ZDecimal amount, ICurrency currency, string source = "")
		{
			Amount = amount;
			fCurrency = currency;
			IsValid = currency != null;
			this.source = source;
		}

		public Money(ZDecimal amount, ICurrency currency, bool overrideIsValid, string source = "")
			: this(amount, currency, source)
		{
			CheckValidForInstantiation(overrideIsValid);
			IsValid = overrideIsValid;
		}

		#endregion

		readonly string source;

		public override bool Equals(object obj)
		{
			Money money = obj as Money;
			if (money == null)
			{
				return false;
			}

			if (Object.ReferenceEquals(money, this))
			{
				return true;
			}

			return money.Amount == Amount
				&& (Amount.IsEmpty || CurrenciesAreEqual(money.Currency, Currency))
				&& money.IsValid == IsValid;
		}

		bool CurrenciesAreEqual(ICurrency currency1, ICurrency currency2)
		{
			if (Object.Equals(currency1, currency2))
			{
				return true;
			}

			if (currency1 == null || currency2 == null)
			{
				return false;
			}

			return currency1.Code == currency2.Code
				&& currency1.PK == currency2.PK
				&& currency1.Decimals == currency2.Decimals;
		}

		public override int GetHashCode()
		{
			return Amount.IsEmpty ? 0 : ToString().GetHashCode();
		}

		public static Money Empty
		{
			get { return empty ?? (empty = new EmptyMoney()); }
		}
		[ThreadStatic]
		static Money empty;

		public static Money Invalid
		{
			get
			{
				if (invalid == null || oldCurrency != Env.CurrentCompany.LocalCurrency)
				{
					invalid = new Money(0, Env.CurrentCompany.LocalCurrency, false);
					oldCurrency = Env.CurrentCompany.LocalCurrency;
				}
				return invalid;
			}
		}

		[ThreadStatic]
		static Money invalid;
		[ThreadStatic]
		static ICurrency oldCurrency;

		protected virtual void CheckValidForInstantiation(bool overrideIsValid)
		{
			if (!IsValid && overrideIsValid)
			{
				throw new ApplicationException("A money object was attempted to be forced valid, when the money being copied was invalid");
			}
		}

		public readonly ZDecimal Amount;
		public virtual ICurrency Currency
		{
			get { return fCurrency; }
		}
		readonly ICurrency fCurrency;
		public readonly bool IsValid;

		public bool IsEmpty
		{
			get { return Amount.IsEmpty; }
		}

		public ZString Source
		{
			get { return source; }
		}

		/// <summary>
		/// The function attempts gets the string representation of the amount with the number of specified
		/// decimal points.  If the result exceeds the specified maximum number if characters, trim trailing zeros
		/// </summary>
		public ZString GetTrimmedDecimalAmountString(int maximumCharacters, bool loseData)
		{
			ZString result = Round().Amount.ToString(Currency == null ? 2 : Currency.Decimals);
			while (result[result.Length - 1] == '.' || (result.Length > maximumCharacters && result.IndexOf('.') != -1 && (loseData || result[result.Length - 1] == '0')))
			{
				result = result.Substring(0, result.Length - 1);
			}
			return result;
		}

		#region Operator Overloads

		public static Money operator *(Money aMoney, ZDecimal multiplier)
		{
			return new Money(aMoney.Amount * multiplier, aMoney.Currency, aMoney.IsValid);
		}

		public static Money operator /(Money aMoney, ZDecimal divisor)
		{
			return new Money(aMoney.Amount / divisor, aMoney.Currency, aMoney.IsValid);
		}

		#endregion

		#region Rounding

		public Money RoundDown()
		{
			return RoundDown(Currency.Decimals);
		}

		public Money FuzzyRoundDown()
		{
			return FuzzyRoundDown(Currency.Decimals);
		}

		public Money RoundDown(int decimalPlaces)
		{
			int multiplyBy = (int)Math.Pow(10, decimalPlaces);
			decimal truncAmount = decimal.Truncate(Amount * multiplyBy);
			return new Money(truncAmount / multiplyBy, Currency, IsValid);
		}

		public Money FuzzyRoundDown(int decimalPlaces)
		{
			return RoundUp(5).RoundDown(decimalPlaces);
		}

		public Money RoundUp()
		{
			return RoundUp(Currency.Decimals);
		}

		public Money RoundUp(int decimalPlaces)
		{
			int multiplyBy = (int)Math.Pow(10, decimalPlaces);
			decimal truncAmount = decimal.Truncate(Amount * multiplyBy);
			if (Amount * multiplyBy != truncAmount)
			{
				truncAmount++;
			}
			return new Money(truncAmount / multiplyBy, Currency, IsValid);
		}

		public virtual Money Round()
		{
			return Round(Currency == null ? GlbCompany.CurrentCompany.LocalCurrency.Decimals : Currency.Decimals);
		}

		public virtual Money Round(int decimalPlaces)
		{
			return new Money(ZArchitecture.Core.Utilities.Round(Amount, decimalPlaces), Currency, IsValid);
		}

		#endregion

		public override string ToString()
		{
			if (Currency == null)
			{
				return Round().Amount.ToString(GlbCompany.CurrentCompany.LocalCurrency.Decimals);
			}

			var currencyCode = Currency.Code;
			var amountString = Round().Amount.ToString("N" + Currency.Decimals, Culture.CurrentCompanyCountryCulture);

			return Res.IsRightToLeft(Res.CurrentLanguage)
				? currencyCode + " " + amountString
				: amountString + " " + currencyCode;
		}

		public static Quantity FromMoney(Money money) => money;

		public static implicit operator Quantity(Money money) => new Quantity(money.Amount, money.Currency.Code, money.source);

		#region IMoney Members

		ZDecimal IMoney.Amount
		{
			get { return Amount; }
		}

		ICurrency IMoney.Currency
		{
			get { return Currency; }
		}

		#endregion
	}
}
