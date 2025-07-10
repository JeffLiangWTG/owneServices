using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Urs.Api.Integration.Interfaces;
using WiseRates.Api.Model;
using DTO = WiseRates.Api.Model;

namespace Enterprise.Rating.Business
{
	public class WiseLine : IRateLine, ICloneable
	{
		public WiseLine(BusinessObjectFactory factory, Charge wiseCharge)
		{
			Factory = factory;
			PK = ZGuid.NewZGuid();
			WiseCharge = wiseCharge;
			ChildRateLineItems = Array.Empty<IRateLineItem>();
			IncludedLines = new List<IRateLine>();
		}

		public WiseLine(BusinessObjectFactory factory, IBaseChargeDto baseCharge, ChargeType chargeType)
		{
			Factory = factory;
			PK = ZGuid.NewZGuid();
			ChildRateLineItems = Array.Empty<IRateLineItem>();
			IncludedLines = new List<IRateLine>();
			BaseCharge = baseCharge;
			this.chargeType = chargeType;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}

		public string InvalidReason
		{
			get
			{
				var errors = new StringBuilder();
				var enrty = ParentRateEntry as WiseEntry;

				foreach (var error in enrty?.Errors)
				{
					errors.AppendLine(error.Value);
				}

				foreach (var error in Errors)
				{
					errors.AppendLine(error.Value);
				}

				return errors.ToString();
			}
		}

		public IDictionary<SchemaColumn, string> Errors { get; } = new Dictionary<SchemaColumn, string>();

		public Charge WiseCharge { get; }
		public IBaseChargeDto BaseCharge { get; }

		public ZGuid PK { get; }

		public IEnumerable<IRateLineItem> ChildRateLineItems { get; set; }
		public IList<IRateLine> IncludedLines { get; }

		public CalculatorType RateCalculatorType
		{
			get => rateCalculatorType;
			set
			{
				if (rateCalculatorType != value)
				{
					rateCalculatorType = value;
					rateCalculator = CalculatorTypeConverter.EnumToCode(value);
					calculator = null;
				}
			}
		}
		CalculatorType rateCalculatorType;

		public ZString TL_RateCalculator
		{
			get => rateCalculator;
			set
			{
				if (rateCalculator != value)
				{
					rateCalculator = value;
					rateCalculatorType = CalculatorTypeConverter.CodeToEnum(value);
					calculator = null;
				}
			}
		}
		ZString rateCalculator;
		public ZString TL_Rounding { get; set; }

		public ZDecimal RoundingFactor { get; set; }

		public ZString TL_UnitFactor => ZString.Empty;

		public Calculator Calculator => calculator ??= CalculatorFactory.GetCalculator(this);
		Calculator calculator;

		public AccChargeCode ChargeCode => Factory.Load<AccChargeCode>(TL_AC);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007:Customizable Data Translation Rule", Justification = "Baseline")]
		public ZString TL_RateDesc => ChargeCode?.AC_Desc ?? ZString.Empty;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Manage the multilingual manually")]
		public ZString TL_RateDescLocal
		{
			get
			{
				if (ChargeCode != null)
				{
					return !ChargeCode.AC_LocalLanguageDescription.IsEmpty
						? ChargeCode.AC_LocalLanguageDescription
						: ChargeCode.AC_Desc;
				}

				return ZString.Empty;
			}
		}

		readonly ChargeType? chargeType;

		public ZString ChargeType
		{
			get => GetChargeTypeDescription(chargeType ?? WiseCharge.ChargeType);
		}

		static string GetChargeTypeDescription(ChargeType chargeType)
		{
			switch (chargeType)
			{
				case DTO.ChargeType.Included:
					return Res.GetString("B1155AA5-680D-470E-9BF1-99550AE2EB1D", "Included");
				case DTO.ChargeType.NotApplicable:
					return Res.GetString("4bc4ac8f-a555-4a98-b2e1-6373ad6f59f0", "Not Applicable");
				case DTO.ChargeType.SubjectTo:
					return Res.GetString("29039f80-243e-41d3-a837-2f7f9e213e1c", "Subject To");
				case DTO.ChargeType.Optional:
					return Res.GetString("8eb59ecc-3c51-43ec-8e4c-865fd4729145", "Optional");
				case DTO.ChargeType.Additional:
					return Res.GetString("e67ccab1-881a-4527-a4f6-0a3501edaecc", "Additional");
				case DTO.ChargeType.Bol:
					return Res.GetString("1bf04e38-b91e-4e28-8899-11665f2fce74", "BOL");
				case DTO.ChargeType.Freight:
					return Res.GetString("ef99a1a2-437b-4b37-b5e5-751e74f44b78", "Freight");
				case DTO.ChargeType.None:
					return string.Empty;
				default:
					var descriptions = new List<string>();
					descriptions.Add(GetChargeTypeDescription(chargeType & DTO.ChargeType.Included));
					descriptions.Add(GetChargeTypeDescription(chargeType & DTO.ChargeType.SubjectTo));
					descriptions.Add(GetChargeTypeDescription(chargeType & DTO.ChargeType.NotApplicable));
					descriptions.Add(GetChargeTypeDescription(chargeType & DTO.ChargeType.Optional));
					return string.Join(", ", descriptions.Where(d => !string.IsNullOrWhiteSpace(d)));
			}
		}

		public ZBool IsOptional { get; set; }

		public ZString Comment { get; set; }

		// ParentRateEntry is typically set once a WiseLine instance is added
		// to a RateEntry.ChildRateLines
		public IRateEntry ParentRateEntry { get; set; }

		public ZGuid TL_AC { get; set; }

		public ZString TL_WeightVolume { get; set; }

		public ZString TL_FeeChargeType => ZString.Empty;

		public ZString TL_FeeChargeLevel => ZString.Empty;

		public RefCurrency Currency
		{
			get { return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, TL_RX_NKCurrency); }
		}

		public ZString TL_RX_NKCurrency { get; set; }

		public ZDecimal TL_WeightVolumeMultiple { get; set; }

		public ZDecimal TL_RoundingFactor { get; set; }

		public ZByte TL_CompanyTariffLevel => ZByte.Zero;

		public ZBool TL_IsOnPallets => false;

		public ZString TL_Condition => ZString.Empty;

		public ZBool TL_IsWhsJobLevelCharge => false;

		public ZGuid TL_OP_ProductNumber => ZGuid.Empty;

		public ZString TL_ConditionalExpression => ZString.Empty;

		public ZString TL_ContainerOwnership => ZString.Empty;

		public ZByte TL_ActualPercentage { get; set; }

		public ZDate TL_RateStartDate => ZDate.Empty;

		public ZDate TL_RateEndDate => ZDate.Empty;

		#region Conversion Factor

		public ConversionFactor ConversionFactor { get; set; }

		public ConversionFactorViewModel ConversionFactorForBinding
		{
			get
			{
				if (conversionFactorForBinding == null)
				{
					conversionFactorForBinding = new ConversionFactorViewModel(p => new ConversionFactorLookups(p), p => new ConversionFactorValidation(p));
					conversionFactorForBinding.ConversionFactor = ConversionFactor;
				}

				return conversionFactorForBinding;
			}
		}

		ConversionFactorViewModel conversionFactorForBinding;

		#endregion

		public void SetViewAgentRatesWithoutRefreshBinding(bool viewAgent) { }

		public ZBool ViewAgentRates
		{
			get { return false; }
			set { }
		}

		public bool IsCalculatorInitialized => true;

		public BusinessObjectFactory Factory { get; }

		public ZString CarrierChargeCode { get; set; }
		public ZString CarrierChargeCodeDescription { get; set; }

		public ZString UniversalChargeCodes => WiseCharge?.ChargeCode;

		public ZString CustomCategory => WiseCharge?.CustomCategory;

		public ZString UnitMultipleAsString => RateLineHelper.GetUnitMultipleAsString(WiseCharge.UnitMultiplier);

		public ZString ChargeInformationNoteText => string.Empty;

		public ZString ChargeInternalNoteText => string.Empty;

		public IEnumerable<CustomField> CustomFields { get; set; }

		/// <summary>
		/// Returns null if no code is found.
		/// </summary>
		public object GetCustomFieldValue(string code)
		{
			return CustomFields?.Where(c => code.Equals(c?.Code)).FirstOrDefault()?.Value;
		}

		public object this[string propertyName]
		{
			get { return this.Getter(propertyName); }
			set { throw new InvalidOperationException("WiseLine should be read only"); }
		}

		public ZPropertyInfo GetZPropertyInfo(string propertyName)
		{
			return null;
		}
	}

	public class WiseLineKey
	{
		readonly Charge wiseCharge;
		readonly bool compareUnitAndConversionFactor;

		/// <summary>
		/// A strict key can be used to group charges that have the exact same elements of Unit, ConversionFactorUnit, ConversionFactorDenominatorUnit, and ConversionFactor: compareUnitAndConversionFactor is true.
		/// </summary>
		public WiseLineKey(Charge wiseCharge, bool compareUnitAndConversionFactor)
		{
			this.wiseCharge = wiseCharge;
			this.compareUnitAndConversionFactor = compareUnitAndConversionFactor;
		}

		public override bool Equals(object obj)
		{
			var other = obj as WiseLineKey;

			var result = other != null;

			// See CargoSphereRatesConverter and CargoguideRatesConverter from WiseRates solution.
			// CS: mainly, each charge has a unique RateLineID.
			//  Except:
			//  - LCL charges can have breaks (as cloning charges) which share the same RateLineID (for CMB calculator).
			//  - Two LCL charges with the same charge code and share the same RateLineID (for highest rate calculator/calculation)
			// CG: all charges have the same RateLineID
			result = result && other.wiseCharge.RateLineID == wiseCharge.RateLineID;

			result = result && other.wiseCharge.ChargeCode == wiseCharge.ChargeCode;
			result = result && other.wiseCharge.Currency == wiseCharge.Currency;
			result = result && (wiseCharge.CarrierChargeCodeInfo == null) == (other.wiseCharge.CarrierChargeCodeInfo == null) && (wiseCharge.CarrierChargeCodeInfo == null || other.wiseCharge.CarrierChargeCodeInfo.Equals(wiseCharge.CarrierChargeCodeInfo));

			if (result && (!string.IsNullOrEmpty(wiseCharge.EquipmentUnit) || !string.IsNullOrEmpty(other.wiseCharge.EquipmentUnit)))
			{
				result = wiseCharge.EquipmentUnit == other.wiseCharge.EquipmentUnit;
			}
			else if (result && compareUnitAndConversionFactor && wiseCharge.PerUnitRate > 0 && other.wiseCharge.PerUnitRate > 0)
			{
				result = other.wiseCharge.Unit == wiseCharge.Unit;
				result = result && other.wiseCharge.ConversionFactorUnit == wiseCharge.ConversionFactorUnit;
				result = result && other.wiseCharge.ConversionFactorDenominatorUnit == wiseCharge.ConversionFactorDenominatorUnit;
				result = result && other.wiseCharge.ConversionFactor.Equals(wiseCharge.ConversionFactor);
			}

			return result;
		}

		public override int GetHashCode()
		{
			int hash = 17;

			unchecked
			{
				hash = hash * 23 + (wiseCharge.ChargeCode?.GetHashCode() ?? 0);
				hash = hash * 23 + (wiseCharge.Currency?.GetHashCode() ?? 0);
				hash = hash * 23 + (wiseCharge.CarrierChargeCodeInfo?.GetHashCode() ?? 0);
				hash = hash * 23 + wiseCharge.RateLineID.GetHashCode();
			}

			return hash;
		}
	}
}
