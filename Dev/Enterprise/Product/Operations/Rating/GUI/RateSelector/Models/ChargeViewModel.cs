using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI.RateSelection;
using Enterprise.Rating.GUI.RateSelector.Services;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	public abstract class ChargeViewModel : ViewModelWithNotificationBase
	{
		public string IncludedText => Res.GetString("d6dceb71-c5e2-4225-a1ce-4e764be45735", "Included");

		protected ChargeViewModel()
		{
		}

		protected ChargeViewModel(BusinessObjectFactory factory, ICurrencyConverter currencyConverter)
		{
			Factory = Argument.NotNull(factory, nameof(factory));
			CurrencyConverter = Argument.NotNull(currencyConverter, nameof(currencyConverter));
		}

		#region Properties

		public virtual string ChargeCode
		{
			get => chargeCode;
			set
			{
				chargeCode = value;
				OnPropertyChanged(nameof(ChargeCode));
			}
		}

		public virtual string ChargeCodeDescription
		{
			get => chargeCodeDescription;
			set
			{
				chargeCodeDescription = value;
				OnPropertyChanged(nameof(ChargeCodeDescription));
			}
		}

		public virtual string ChargeCodeError
		{
			get => chargeCodeError;
			set
			{
				chargeCodeError = value;
				OnPropertyChanged(nameof(ChargeCodeError));
			}
		}

		public virtual ErrorLevel ChargeCodeErrorLevel
		{
			get => chargeCodeErrorLevel;
			set
			{
				chargeCodeErrorLevel = value;
				OnPropertyChanged(nameof(ChargeCodeErrorLevel));
				OnPropertyChanged(nameof(IsValid));
			}
		}

		public decimal Amount { get; set; }
		public string Currency { get; set; }
		public string LocalCurrency { get; set; }

		public bool IsIncluded { get; set; }
		public bool IsOptional { get; set; }
		public bool IsValid => !ErrorLevel.HasFlag(ErrorLevel.Error);
		public AutoRateInfo AutoRateInfo { get; protected set; }

		public decimal LocalAmount { get; set; }

		public string LocalAmountError
		{
			get => localAmountError;
			set
			{
				localAmountError = value;
				OnPropertyChanged(nameof(LocalAmountError));
				OnPropertyChanged(nameof(LocalAmountErrorLevel));
			}
		}
		public ErrorLevel LocalAmountErrorLevel => string.IsNullOrEmpty(LocalAmountError) ? ErrorLevel.None : ErrorLevel.Warning;

		public string CalculationDescription { get; set; }
		public virtual bool DisplayPrice => Env.Security.MaintainConsolJobInvoicingAllowViewPricesInRateSelection.IsAllowed;

		public virtual bool IsSelected
		{
			get => !IsOptional || isSelected;
			set
			{
				if (IsOptional)
				{
					isSelected = value;
					OnPropertyChanged(nameof(IsSelected));
				}
			}
		}

		public AccChargeCode AccChargeCode { get; protected set; }

		public virtual ErrorLevel ErrorLevel => ChargeCodeErrorLevel;

		#endregion

		protected void PopulateLocalAmounts(string charge, decimal amount, string currency)
		{
			LocalCurrency = GlbCompany.CurrentCompany.LocalCurrency.Code;

			if (amount == 0 || string.IsNullOrEmpty(currency))
			{
				return;
			}

			var refCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Currency);
			if (refCurrency == null)
			{
				LocalAmountError = Res.GetString("5f202872-3627-449e-9753-a6a6a70e558b", "{0} currency for Charge {1} is invalid", Currency, charge);
				LocalAmount = 0;

				return;
			}

			var localAmount = CurrencyConverter.Convert(new Money(amount, refCurrency), GlbCompany.CurrentCompany.LocalCurrency);
			if (!localAmount.IsValid)
			{
				LocalAmountError = Res.GetString("8d985468-3440-4b21-a3ba-17dd5417ce55", "No valid exchange rate between {0} and {1} is found for Charge {2}", Currency, LocalCurrency, charge);
				LocalAmount = 0;

				return;
			}

			LocalAmount = localAmount.Amount;
			LocalAmountError = string.Empty;
		}

		protected BusinessObjectFactory Factory { get; }

		ICurrencyConverter CurrencyConverter { get; }

		bool isSelected;
		string chargeCode;
		string chargeCodeDescription;
		string chargeCodeError;
		ErrorLevel chargeCodeErrorLevel;
		string localAmountError;
	}
}
