using System.Drawing;
using System.Linq;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI.RateSelection;
using Enterprise.ZArchitecture.Core;
using WiseRates.Api.Model;

namespace Enterprise.Rating.GUI.RateChooser.ViewModel
{
	public class ChargeViewModel : ViewModelWithNotificationBase
	{
		/// <summary>
		/// This constructor to be used only for sample data.
		/// </summary>
		public ChargeViewModel(string code, string description, string calculatedFormula, bool selected, string calculatedAmount)
		{
			Code = code;
			Description = description;
			CalculatedFormula = calculatedFormula;
			IsActive = selected;
			CalculatedAmount = calculatedAmount;
			HandlingOffice = (NoResString)"<Handling office goes here>";
		}

		/// <summary>
		/// Constructor for WiseRate
		/// </summary>
		public ChargeViewModel(
			Charge charge,
			WiseLine line,
			ChooserRateEntry chooserRateEntry,
			bool showIsActive)
		{
			this.chooserRateEntry = chooserRateEntry;
			Charge = charge;
			wiseLine = line;
			mappedChargeCode = line.ChargeCode;
			isActive = chooserRateEntry.IsActive(charge);
			IsActiveEnabled = Charge.IsOptional;
			HandlingOffice = GetHandlingOffice();

			if (mappedChargeCode != null)
			{
				Code = mappedChargeCode.AC_Code;
				Description = line.DescriptionWithInclusiveCharges();
			}
			else
			{
				Code = charge.ChargeCode;
				Description = line.DescriptionWithInclusiveCharges();
				ChargeCodeErrorVisibility = true;
				ChargeCodeErrorType = ChargeCodeErrorType.Error;
			}

			var calc = chooserRateEntry.CalculatedResult?.FirstOrDefault(x => x.Line == line);

			if (calc == null)
			{
				ChargeCodeErrorVisibility = true;
				ChargeCodeErrorType = ChargeCodeErrorType.Warning;
			}

			if (!line.IsValidRate())
			{
				ChargeCodeErrorVisibility = true;
				ChargeCodeErrorType = ChargeCodeErrorType.Error;
			}

			IsActiveVisibility = showIsActive;
			var priceBy = System.Convert.ToString(wiseLine.CustomFields?.FirstOrDefault(x => x.Code == Rate.CustomFields.CargoSphere.PriceBy)?.Value);
			SetCalculatedResults(calc, priceBy);
		}

		/// <summary>
		/// Constructor for CW1 rate
		/// </summary>
		public ChargeViewModel(IRateLine rateLine, ChooserRateEntry chooserRateEntry)
		{
			this.chooserRateEntry = chooserRateEntry;
			RateLine = rateLine;
			isActive = true;
			IsActiveEnabled = false;

			var chargeCode = rateLine.ChargeCode;
			Code = chargeCode?.AC_Code ?? string.Empty;
			Description = chargeCode?.AC_DescMultilingual ?? string.Empty;

			IsActiveVisibility = false;
			SetCalculatedResults(chooserRateEntry.CalculatedResult?.FirstOrDefault(x => x.Line == rateLine));
		}

		string GetHandlingOffice()
		{
			return Charge.ProviderCustomFields
				.Where(c => c.Code == Rate.CustomFields.CargoSphere.HandlingOffice)
				.Select(c => c.Value)
				.Cast<string>()
				.FirstOrDefault();
		}

		void SetCalculatedResults(AutoRateInfo autoRateInfo, string priceBy = "")
		{
			if (autoRateInfo != null)
			{
				CalculatedAmount = autoRateInfo.Amount.ToString();

				var convertedMoney = chooserRateEntry.ChooserServices.ConvertToDefaultCurrency((decimal)autoRateInfo.Amount, autoRateInfo.Currency);

				CalculatedAmountString =
					convertedMoney.IsValid
					? chooserRateEntry.ChooserServices.ConvertToCurrentCompanyFormat(convertedMoney.Amount)
					: string.Empty;

				CalculatedFormula = autoRateInfo.CalculationDescription;
				CalculatedFormulaToolTip = string.IsNullOrEmpty(priceBy) ? autoRateInfo.CalculationDescription.ToString() : priceBy;
			}
		}

		readonly ChooserRateEntry chooserRateEntry;
		public Charge Charge { get; }
		public IRateLine RateLine { get; }
		public bool IsActiveEnabled { get; }
		readonly AccChargeCode mappedChargeCode;

		readonly WiseLine wiseLine;

		public bool IsActive
		{
			get => isActive;
			set
			{
				if (isActive != value)
				{
					isActive = value;
					chooserRateEntry?.SetActive(Charge, value);
					OnPropertyChanged(nameof(IsActive));
				}
			}
		}
		bool isActive;

		public bool IsIncluded => !string.IsNullOrEmpty(Charge?.FreightInclusiveCarriageCharge);
		public string Unit => Charge?.Unit ?? RateLine?.TL_WeightVolume;

		public string Code { get; private set; }
		public string Description { get; private set; }
		public string CalculatedAmount { get; private set; }
		public string CalculatedAmountString { get; private set; }
		public string CalculatedFormula { get; private set; }
		public string CalculatedFormulaToolTip { get; private set; }
		public string HandlingOffice { get; private set; }

		public bool CalculatedFormulaVisibility => Env.Security.MaintainConsolJobInvoicingAllowViewPricesInRateSelection.IsAllowed;
		public bool IsActiveVisibility { get; }
		public bool ChargeCodeErrorVisibility { get; }

		public Image ErrorIcon { get; } = RateChooserImageRepository.ErrorIcon;

		public Image WarningIcon { get; } = RateChooserImageRepository.WarningIcon;

		string UnmappedChargeCodeErrorText => Res.GetString("594EF650-5183-42B3-AB7B-8F200DC4D735", @"This universal charge code is not mapped.
The Apply button will prompt to map any selected unmapped codes.");

		public string ChargeCodeErrorToolTipText
		{
			get
			{
				if (chargeCodeErrorToolTipText == null)
				{
					chargeCodeErrorToolTipText = string.Empty;
					if (Charge != null && mappedChargeCode == null)
					{
						chargeCodeErrorToolTipText += UnmappedChargeCodeErrorText;
					}

					if (RateLine != null && !RateLine.IsValidRate())
					{
						chargeCodeErrorToolTipText += System.Environment.NewLine + RateLine.InvalidReason;
					}

					if (wiseLine != null && !wiseLine.IsValidRate())
					{
						chargeCodeErrorToolTipText += System.Environment.NewLine + wiseLine.InvalidReason;
					}

					if (wiseLine != null && !chooserRateEntry.CalculatedResult.Select(c => c.ChargeCode).Contains(wiseLine.ChargeCode))
					{
						if (!string.IsNullOrWhiteSpace(chargeCodeErrorToolTipText))
						{
							chargeCodeErrorToolTipText += System.Environment.NewLine;
						}

						chargeCodeErrorToolTipText += Res.GetString("F6183CF5-8895-42EE-8643-1CF91B356E89", "Not applicable");
					}
				}

				return chargeCodeErrorToolTipText;
			}
		}

		public Money CalculatedPriceInDefaultCurrency()
		{
			if (decimal.TryParse(CalculatedAmount, out var calculatedAmount))
			{
				if (Charge != null)
				{
					return chooserRateEntry.ChooserServices.ConvertToDefaultCurrency(calculatedAmount, Charge.Currency);
				}

				if (RateLine != null)
				{
					return chooserRateEntry.ChooserServices.ConvertToDefaultCurrency(calculatedAmount, RateLine.TL_RX_NKCurrency);
				}
			}

			return Money.Empty;
		}

		string chargeCodeErrorToolTipText;

		public AutoRateInfoCollection AutoratingCalculatedResult => chooserRateEntry?.CalculatedResult;

		public string MappedChargeGroup => mappedChargeCode?.AC_ChargeGroup;

		public string ChargeGroup => RateLine != null ? (string)RateLine.ChargeCode.AC_ChargeGroup : (MappedChargeGroup ?? missingMappingChargeGroup);

		string missingMappingChargeGroup => Res.GetString("3abb1dd4-cc3a-48cc-967d-9bf88c3dd80d", "Missing Mapping");

		public bool IsWiseRateMapped => wiseLine != null && mappedChargeCode != null;

		public ChargeCodeErrorType ChargeCodeErrorType { get; }
	}

	public enum ChargeCodeErrorType
	{
		None,
		Warning,
		Error
	}
}
