using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI.RateSelection;
using WiseRates.Constants;

namespace Enterprise.Rating.GUI.RateChooser.ViewModel
{
	/// <summary>
	///		A group of charges - Freight, BOL, or Additional
	///		There are several of these instances existing within a single
	///		ChooserRateRow
	/// </summary>
	public class ChargesViewModel : ViewModelWithNotificationBase
	{
		public class ChargeGroupView : ViewModelWithNotificationBase
		{
			public string ChargeGroup { get; set; }
			public IEnumerable<ChargeViewModel> Charges { get; set; }
			public ChargesViewModel Parent { get; set; }
		}

		public ChargesViewModel(ChargesGroup group, IRateChooserServices chooserServices, IEnumerable<ChargeViewModel> charges, bool isLCL = false)
		{
			Group = group;
			Charges = new ObservableCollection<ChargeViewModel>(charges);
			this.chooserServices = chooserServices;
			this.isLCL = isLCL;

			Initialize();

			CalculationIcon = GetCalculationIcon();

			Recalculate();
		}

		public string GroupName { get; set; }
		public string GroupShortName { get; set; }

		public void RecalculateIsAnyActive()
		{
			if (inUpdateAnyOptionalActive)
			{
				return;
			}

			bool newValue = Charges.Any(x => x.IsActive && x.Charge.IsOptional);
			if (isAnyOptionalActive != newValue)
			{
				isAnyOptionalActive = newValue;
				OnPropertyChanged(nameof(IsAnyOptionalActive));
			}
		}

		public bool IsAnyOptionalActiveEnabled { get; private set; }

		/// <summary>
		/// Setter should only be called from UI to prevent event cycles.
		/// To recalculate the value programmatically call RecalculateIsAnyActive rather than this setter.
		/// </summary>
		public bool IsAnyOptionalActive
		{
			get => isAnyOptionalActive;
			set
			{
				if (isAnyOptionalActive != value)
				{
					isAnyOptionalActive = value;
					SetIsAnyOptionalActiveFromUI(value);
					OnPropertyChanged(nameof(IsAnyOptionalActive));
				}
			}
		}
		bool isAnyOptionalActive;
		bool inUpdateAnyOptionalActive;

		void SetIsAnyOptionalActiveFromUI(bool newIsAnyActive)
		{
			// Recursion prevention
			if (inUpdateAnyOptionalActive)
			{
				return;
			}

			inUpdateAnyOptionalActive = true;

			try
			{
				foreach (var charge in Charges)
				{
					if (charge.IsActive != newIsAnyActive && (charge.Charge.IsOptional))
					{
						// This may trigger a call to RecalculateIsAnyActive
						charge.IsActive = newIsAnyActive;
					}
				}
			}
			finally
			{
				inUpdateAnyOptionalActive = false;
			}
		}

		public IEnumerable<ChargeGroupView> ChargeGroupsView { get; private set; }

		public IEnumerable<ChargeViewModel> Charges { get; }

		#region SuppressResourceStringsCheckRegion

		public string UnmappedChargesLabel => "TBA:";
		public string DSTChargesLabel => "DST:";
		public string ORGChargesLabel => "ORG:";
		public string FRTChargesLabel => "FRT:";

		#endregion

		public ChargesGroup Group { get; }

		public readonly Image CalculationIcon;

		public string ActiveChargeCodesString
		{
			get
			{
				return string.Join(", ", Charges
					.Where(c => c.IsActive)
					.Select(c => c.Code)
					.OrderBy(c => c));
			}
		}

		public string FRTChargeCodesString
		{
			get
			{
				return string.Join(", ", ActiveFRTChargesForCalculation
					.Select(c => c.Code).Distinct()
					.OrderBy(c => c));
			}
		}

		public string ORGChargeCodesString
		{
			get
			{
				return string.Join(", ", ActiveOriginChargesForCalculation
					.Select(c => c.Code).Distinct()
					.OrderBy(c => c));
			}
		}

		public string DSTChargeCodesString
		{
			get
			{
				return string.Join(", ", ActiveDestinationChargesForCalculation
					.Select(c => c.Code).Distinct()
					.OrderBy(c => c));
			}
		}

		public string UnmappedChargeCodesString
		{
			get
			{
				return string.Join(", ", Charges
					.Where(c => c.IsActive && c.RateLine == null && string.IsNullOrEmpty(c.MappedChargeGroup))
					.Select(c => c.Code)
					.OrderBy(c => c));
			}
		}

		public decimal TotalPrice { get; set; }

		public string TotalPriceCurrency { get; } = RateChooserModel.DefaultCurrency;

		public string TotalPriceString { get; set; }

		public decimal TotalFRTChargesPrice { get; set; }
		public string TotalFRTChargesPriceString { get; set; }

		public decimal TotalORGChargesPrice { get; set; }
		public string TotalORGChargesPriceString { get; set; }

		public decimal TotalDSTChargesPrice { get; set; }
		public string TotalDSTChargesPriceString { get; set; }

		public string TotalPriceErrorString
		{
			get
			{
				if (CurrenciesMissingExchangeRates == null || !CurrenciesMissingExchangeRates.Any())
				{
					return string.Empty;
				}

				var missingCurrenciesAsText = string.Join(", ", CurrenciesMissingExchangeRates.OrderBy(x => x));
				return Res.GetString("EC570689-BB3C-4E3A-B3F8-BC3149E70497", "Missing exchange rate(s) for {0} to {1}", missingCurrenciesAsText, RateChooserModel.DefaultCurrency);
			}
		}

		public IEnumerable<string> CurrenciesMissingExchangeRates { get; set; } = Enumerable.Empty<string>();
		public Visibility OverallVisibility { get; private set; }
		public bool IsActiveVisibility { get; private set; }
		public bool TotalPriceVisibility { get; private set; }

		public bool TotalPriceErrorVisibility => TotalPriceVisibility && !string.IsNullOrEmpty(TotalPriceErrorString);

		public bool CalculationIconVisibility => CalculationIcon != null;

		public bool ORGChargesVisibility { get; private set; }
		public bool FRTChargesVisibility { get; private set; }
		public bool DSTChargesVisibility { get; private set; }
		public bool UnmappedChargesVisibility { get; private set; }

		void CalculateTotalPrice(IEnumerable<ChargeViewModel> chargesToCalculate)
		{
			var (amount, currenciesMissingExchangeRates) = SumInDefaultCurrency(chargesToCalculate.Select(c => c.CalculatedPriceInDefaultCurrency()));
			TotalPrice = amount;
			TotalPriceString = ConvertToCurrentCompanyFormat(amount, currenciesMissingExchangeRates.IsNullOrEmpty(), currencyLabel: string.Empty);
			TotalPriceVisibility = chargesToCalculate.Any(x => x.CalculatedFormulaVisibility);
			CurrenciesMissingExchangeRates = currenciesMissingExchangeRates ?? Enumerable.Empty<string>();
		}

		public void Recalculate()
		{
			if (IsInDesignMode)
			{
				return;
			}

			var activeCharges = Charges.Where(c => c.IsActive && !c.IsIncluded).ToArray();
			CalculateTotalPrice(isLCL || Group == ChargesGroup.Additional ? activeCharges
				: Group == ChargesGroup.BOL
					? activeCharges.Where(c => string.IsNullOrEmpty(c.Unit))
					: activeCharges.Where(c => c.Unit == WRConstants.Units.CN));

			var (frtAmount, frtMissingExchangeRate) = SumInDefaultCurrency(ActiveFRTChargesForCalculation.Select(c => c.CalculatedPriceInDefaultCurrency()));
			TotalFRTChargesPrice = frtAmount;
			TotalFRTChargesPriceString = ConvertToCurrentCompanyFormat(frtAmount, frtMissingExchangeRate.IsNullOrEmpty());

			var (orgAmount, orgMissingExchangeRate) = SumInDefaultCurrency(ActiveOriginChargesForCalculation.Select(c => c.CalculatedPriceInDefaultCurrency()));
			TotalORGChargesPrice = orgAmount;
			TotalORGChargesPriceString = ConvertToCurrentCompanyFormat(orgAmount, orgMissingExchangeRate.IsNullOrEmpty());

			var (dstAmount, dstMissingExchangeRate) = SumInDefaultCurrency(ActiveDestinationChargesForCalculation.Select(c => c.CalculatedPriceInDefaultCurrency()));
			TotalDSTChargesPrice = dstAmount;
			TotalDSTChargesPriceString = ConvertToCurrentCompanyFormat(dstAmount, dstMissingExchangeRate.IsNullOrEmpty());

			FRTChargesVisibility = !string.IsNullOrEmpty(FRTChargeCodesString);
			ORGChargesVisibility = !string.IsNullOrEmpty(ORGChargeCodesString);
			DSTChargesVisibility = !string.IsNullOrEmpty(DSTChargeCodesString);
			UnmappedChargesVisibility = !string.IsNullOrEmpty(UnmappedChargeCodesString);

			OnPropertyChanged(nameof(TotalPriceString));
			OnPropertyChanged(nameof(CurrenciesMissingExchangeRates));
			OnPropertyChanged(nameof(GroupShortName));
			OnPropertyChanged(nameof(ActiveChargeCodesString));
			OnPropertyChanged(nameof(TotalPriceErrorString));
			OnPropertyChanged(nameof(TotalPriceErrorVisibility));
			OnPropertyChanged(nameof(TotalPriceVisibility));

			OnPropertyChanged(nameof(TotalFRTChargesPrice));
			OnPropertyChanged(nameof(TotalFRTChargesPriceString));
			OnPropertyChanged(nameof(TotalORGChargesPrice));
			OnPropertyChanged(nameof(TotalORGChargesPriceString));
			OnPropertyChanged(nameof(TotalDSTChargesPrice));
			OnPropertyChanged(nameof(TotalDSTChargesPriceString));

			OnPropertyChanged(nameof(FRTChargesVisibility));
			OnPropertyChanged(nameof(ORGChargesVisibility));
			OnPropertyChanged(nameof(DSTChargesVisibility));
			OnPropertyChanged(nameof(UnmappedChargesVisibility));

			OnPropertyChanged(nameof(FRTChargeCodesString));
			OnPropertyChanged(nameof(ORGChargeCodesString));
			OnPropertyChanged(nameof(DSTChargeCodesString));
			OnPropertyChanged(nameof(UnmappedChargeCodesString));

			var unmappedWRCharges = Charges.Where(c => c.RateLine == null && !c.IsWiseRateMapped);
			var groupCharges = FRTChargesForCalculation
								.Union(OriginChargesForCalculation)
								.Union(DestinationChargesForCalculation)
								.Union(unmappedWRCharges);

			ChargeGroupsView =
				groupCharges.GroupBy(c => c.ChargeGroup).Select(g => new ChargeGroupView() { ChargeGroup = g.Key, Charges = g.Select(c => c), Parent = this }).ToList();

			OnPropertyChanged(nameof(ChargeGroupsView));
		}

		(decimal total, HashSet<string> currenciesMissingExchangeRates) SumInDefaultCurrency(IEnumerable<Money> moneyList)
		{
			decimal amount = 0;
			HashSet<string> currenciesMissingExchangeRates = null;
			foreach (var money in moneyList)
			{
				if (money.IsValid)
				{
					amount += money.Amount;
				}
				else
				{
					currenciesMissingExchangeRates = currenciesMissingExchangeRates ?? new HashSet<string>();
					currenciesMissingExchangeRates.Add(money.Currency?.Code ?? "???");
				}
			}
			return (amount, currenciesMissingExchangeRates);
		}

		/// <param name="amountValid">
		/// If false, then no amount is displayed
		/// </param>
		/// <param name="currencyLabelOverride">
		/// If non-null, then this is the leading text in lieu of the currency code
		/// </param>
		/// <returns>
		/// A string representing the amount in the current company format.
		/// </returns>
		public string ConvertToCurrentCompanyFormat(decimal amount, bool amountValid = true, string currencyLabel = null)
		{
			currencyLabel = currencyLabel ?? RateChooserModel.DefaultCurrency;
			if (!amountValid)
			{
				return currencyLabel;
			}
			else if (!currencyLabel.IsNullOrEmpty())
			{
				return currencyLabel + " " + chooserServices?.ConvertToCurrentCompanyFormat(amount) ?? amount.ToString();
			}
			else
			{
				return chooserServices?.ConvertToCurrentCompanyFormat(amount) ?? amount.ToString();
			}
		}

		public static bool IsInDesignMode
		{
			get
			{
				return LicenseManager.CurrentContext.UsageMode == LicenseUsageMode.Designtime;
			}
		}

		void Initialize()
		{
			if (Group == ChargesGroup.Base)
			{
				GroupName = ResString.GetMultilingualString("DFDE09AF-F88B-45D9-AD0F-190F7ACA651A", "Freight");
				GroupShortName = ResString.GetMultilingualString("RateSelection.Freight", "Freight:");
			}
			else if (Group == ChargesGroup.Additional)
			{
				GroupName = ResString.GetMultilingualString("RateSelection.AdditionalCharges", "Additional Charges");
				GroupShortName = ResString.GetMultilingualString("RateSelection.Additional", "Add.:");
			}
			else
			{
				GroupName = ResString.GetMultilingualString("RateSelection.BillOfLadingShortCaption", "Bill of Lading");
				GroupShortName = ResString.GetMultilingualString("RateSelection.BillOfLading", "BOL:");
			}
		}

		Image GetCalculationIcon()
		{
			var firstCharge = Charges.FirstOrDefault();
			IsActiveVisibility = firstCharge?.IsActiveVisibility ?? false;
			isAnyOptionalActive = Charges.Any(x => x.IsActive && x.Charge != null && x.Charge.IsOptional);
			IsAnyOptionalActiveEnabled = Charges.Any(x => x.Charge != null && x.Charge.IsOptional);

			Image calculationIcon = null;
			if (firstCharge?.AutoratingCalculatedResult != null)
			{
				if (Group == ChargesGroup.BOL)
				{
					var bolChargesWithErrors = Charges.Where(x => x.Charge != null && !x.IsIncluded && x.ChargeCodeErrorVisibility);
					if (bolChargesWithErrors.Any())
					{
						calculationIcon = RateChooserImageRepository.WarningIcon;
					}
				}
				else if (Group == ChargesGroup.Additional)
				{
					var optionalChargesWithError = Charges.Where(x => x.Charge != null && !x.IsIncluded && x.Charge.IsOptional && x.ChargeCodeErrorVisibility);

					if (optionalChargesWithError.Any())
					{
						calculationIcon = RateChooserImageRepository.WarningIcon;
					}
				}
				else
				{
					var otherCharges = Charges.Where(x => x.Charge != null && !x.IsIncluded && !x.Charge.IsOptional);
					if (otherCharges.Any(x => x.ChargeCodeErrorType == ChargeCodeErrorType.Error))
					{
						calculationIcon = RateChooserImageRepository.ErrorIcon;
					}
					else if (otherCharges.Any(x => x.ChargeCodeErrorType == ChargeCodeErrorType.Warning))
					{
						calculationIcon = RateChooserImageRepository.WarningIcon;
					}
				}
			}

			return calculationIcon;
		}

		readonly IRateChooserServices chooserServices;
		readonly bool isLCL;

		public IEnumerable<ChargeViewModel> FRTChargesForCalculation => Charges.Where(c =>
				c.RateLine != null
				? FRTCharges.Contains(c.RateLine.ChargeCode?.AC_ChargeGroup)
				: FRTCharges.Contains(c.MappedChargeGroup)).Distinct();
		List<string> FRTCharges => new List<string> { ChargeCodeGroupList.Codes.Freight, ChargeCodeGroupList.Codes.Insurance };
		public IEnumerable<ChargeViewModel> ActiveFRTChargesForCalculation => FRTChargesForCalculation.Where(c => c.IsActive);

		public IEnumerable<ChargeViewModel> OriginChargesForCalculation => Charges.Where(c =>
				c.RateLine != null
				? ORGCharges.Contains(c.RateLine.ChargeCode?.AC_ChargeGroup)
				: ORGCharges.Contains(c.MappedChargeGroup)).Distinct();
		List<string> ORGCharges => new List<string> { ChargeCodeGroupList.Codes.Origin, ChargeCodeGroupList.Codes.Loading, ChargeCodeGroupList.Codes.OriginBrokerage,
		ChargeCodeGroupList.Codes.OriginBrokerageOnly,ChargeCodeGroupList.Codes.CustomsDuty };
		public IEnumerable<ChargeViewModel> ActiveOriginChargesForCalculation => OriginChargesForCalculation.Where(c => c.IsActive);

		public IEnumerable<ChargeViewModel> DestinationChargesForCalculation => Charges.Where(c =>
				c.RateLine != null
				? DSTCharges.Contains(c.RateLine.ChargeCode?.AC_ChargeGroup)
				: DSTCharges.Contains(c.MappedChargeGroup)).Distinct();
		List<string> DSTCharges => new List<string> { ChargeCodeGroupList.Codes.Destination, ChargeCodeGroupList.Codes.Unloading, ChargeCodeGroupList.Codes.Brokerage, ChargeCodeGroupList.Codes.BrokerageOnly };
		public IEnumerable<ChargeViewModel> ActiveDestinationChargesForCalculation => DestinationChargesForCalculation.Where(c => c.IsActive);

		public enum ChargesGroup
		{
			BOL,
			Base,
			Additional
		}
	}
}
