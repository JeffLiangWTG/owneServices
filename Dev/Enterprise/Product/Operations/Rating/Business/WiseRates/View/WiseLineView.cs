using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Rating.Business
{
	public class WiseLineView : NonPersistentBusinessObject, IRateLine, IGetZPropertyInfo, INeedCodeMappings
	{
		public WiseLineView(IRateLine wiseLine) : base(wiseLine.Factory)
		{
			Argument.NotNull(wiseLine, nameof(wiseLine));

			this.wiseLine = wiseLine;
		}

		readonly IRateLine wiseLine;

		public WiseLine UnderlyingWiseLine => wiseLine as WiseLine;
		public string InvalidReason => wiseLine.InvalidReason;
		public IRateEntry ParentRateEntry => wiseLine.ParentRateEntry;
		public IEnumerable<IRateLineItem> ChildRateLineItems => wiseLine.ChildRateLineItems.Select(x => new WiseLineItemView(x));
		public IList<IRateLine> IncludedLines => wiseLine.IncludedLines;

		public WiseLineItemViewsCollection ChildWiseRateLineItemViews
		{
			get
			{
				if (childWiseRateLineItemViews == null)
				{
					childWiseRateLineItemViews = new WiseLineItemViewsCollection(wiseLine?.ChildRateLineItems ?? Enumerable.Empty<IRateLineItem>());
					RegisterEditableChildObject(childWiseRateLineItemViews);
				}

				return childWiseRateLineItemViews;
			}
		}

		WiseLineItemViewsCollection childWiseRateLineItemViews;

		public Calculator Calculator => fCalculator ?? (fCalculator = CalculatorFactory.GetCalculator(this));
		Calculator fCalculator;

		public CalculatorType RateCalculatorType => wiseLine.RateCalculatorType;

		[List("Lookups.RateCalculators")]
		public ZString TL_RateCalculator => wiseLine.TL_RateCalculator;
		public ZPropertyInfo TL_RateCalculatorInfo => GetZPropertyInfo(nameof(TL_RateCalculator));

		[List("Lookups.Roundings")]
		public ZString TL_Rounding => wiseLine.TL_Rounding;

		public ZDecimal RoundingFactor => wiseLine.RoundingFactor;

		public AccChargeCode ChargeCode => wiseLine.ChargeCode;

		[List("Lookups.ChargeCodes")]
		public ZGuid TL_AC => wiseLine.TL_AC;
		public ZPropertyInfo TL_ACInfo => GetZPropertyInfo(nameof(TL_AC));

		[List("Lookups.WeightVolumes")]
		public ZString TL_WeightVolume => wiseLine.TL_WeightVolume;

		[List("Lookups.FeeChargeTypes")]
		public ZString TL_FeeChargeType => wiseLine.TL_FeeChargeType;

		[List("Lookups.FeeChargeLevels")]
		public ZString TL_FeeChargeLevel => wiseLine.TL_FeeChargeLevel;

		public RefCurrency Currency => wiseLine.Currency;

		public ZByte TL_CompanyTariffLevel => wiseLine.TL_CompanyTariffLevel;

		public ZBool TL_IsOnPallets => wiseLine.TL_IsOnPallets;

		[List("Lookups.RateLineConditions")]
		public ZString TL_Condition => wiseLine.TL_Condition;

		public ZBool TL_IsWhsJobLevelCharge => wiseLine.TL_IsWhsJobLevelCharge;

		public ZGuid TL_OP_ProductNumber => wiseLine.TL_OP_ProductNumber;

		public ZString TL_ConditionalExpression => wiseLine.TL_ConditionalExpression;

		public ZString TL_ContainerOwnership => wiseLine.TL_ContainerOwnership;

		public ZByte TL_ActualPercentage => wiseLine.TL_ActualPercentage;

		public ConversionFactor ConversionFactor => wiseLine.ConversionFactor;

		public ConversionFactorViewModel ConversionFactorForBinding => wiseLine.ConversionFactorForBinding;

		[List("Lookups.Currencies")]
		public ZString TL_RX_NKCurrency => wiseLine.TL_RX_NKCurrency;
		public ZPropertyInfo TL_RX_NKCurrencyInfo => GetZPropertyInfo(nameof(TL_RX_NKCurrency));

		public ZDecimal TL_WeightVolumeMultiple => wiseLine.TL_WeightVolumeMultiple;

		public ZDecimal TL_RoundingFactor => wiseLine.TL_RoundingFactor;

		public ZString TL_UnitFactor => ZString.Empty;

		public ZString TL_RateDesc => wiseLine.TL_RateDesc;

		public ZString TL_RateDescLocal => wiseLine.TL_RateDescLocal;

		public ZDate TL_RateStartDate => wiseLine.TL_RateStartDate;

		public ZDate TL_RateEndDate => wiseLine.TL_RateEndDate;

		public ZString CarrierChargeCodeDescription => wiseLine.CarrierChargeCodeDescription;

		public ZString ChargeType => wiseLine.ChargeType;

		public ZString Comment => wiseLine.Comment;

		public bool IsBulkRateUpdateActionLine => false;

		public ZDecimal CompanyTariffDiscount => ZDecimal.Zero;

		public ZString ChargeInformationNoteText => wiseLine.ChargeInformationNoteText;

		public ZString ChargeInternalNoteText => wiseLine.ChargeInternalNoteText;

		public void SetViewAgentRatesWithoutRefreshBinding(bool viewAgent) => wiseLine.SetViewAgentRatesWithoutRefreshBinding(viewAgent);
		public ZBool ViewAgentRates
		{
			get => wiseLine.ViewAgentRates;
			set => wiseLine.ViewAgentRates = value;
		}

		public bool IsCalculatorInitialized => wiseLine.IsCalculatorInitialized;

		#region ViewCalculatorForBinding

		public bool ViewResultsVisible => false;
		public bool IsBreakWeightVolumeAvailable => Calculator.IsBreakUnitAvailable;  //TODO add to interface

		public RateLine.CalculatorWrapper ViewCalculatorForBinding
		{
			get
			{
				if (viewCalculatorForBinding.Calculator != Calculator)
				{
					viewCalculatorForBinding = new RateLine.CalculatorWrapper(Calculator);
				}

				return viewCalculatorForBinding;
			}
		}

		RateLine.CalculatorWrapper viewCalculatorForBinding;

		public Calculator ViewCalculator => Calculator;

		[BusinessObjectTestExclude]
		public ZBool ViewResults => false;

		public RateLinesLookups Lookups => fLookups ?? (fLookups = new RateLinesLookups(this, Factory));
		RateLinesLookups fLookups;

		ZPropertyInfo IGetZPropertyInfo.GetZPropertyInfo(string propertyName)
		{
			return this.GetZPropertyInfo(propertyName);
		}

		#endregion

		#region Business Object Overrides

		public WiseLineViewValidation Validation => new WiseLineViewValidation(this);

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
		}

		#endregion

		#region INeedCodeMappings

		List<UnmappedForeignCode> INeedCodeMappings.UnmappedCodes => unmappedCodes;
		readonly List<UnmappedForeignCode> unmappedCodes = new List<UnmappedForeignCode>();

		bool INeedCodeMappings.NeedsCodeMapping => unmappedCodes.Any();
		void INeedCodeMappings.SetUnmappedCodes(IEnumerable<UnmappedForeignCode> codesNeedsMapping)
		{
			unmappedCodes.AddRange(codesNeedsMapping);
		}

		ZGuid INeedCodeMappings.CarrierOrgHeaderPK
		{
			get
			{
				if (this.UnderlyingWiseLine?.ParentRateEntry is WiseEntry wiseEntry)
				{
					return wiseEntry.Carrier?.PK ?? ZGuid.Empty;
				}

				return ZGuid.Empty;
			}
		}

		public ZString UniversalChargeCodes => wiseLine.UniversalChargeCodes;

		public ZString CarrierChargeCode => wiseLine.CarrierChargeCode;

		public ZString UnitMultipleAsString => RateLineHelper.GetUnitMultipleAsString(TL_WeightVolumeMultiple);

		#endregion
	}
}
