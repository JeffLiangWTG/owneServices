using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.DataTransfer.Ratings;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	[CalculatorProperty(CalculatorConstants.Type.PER, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal2", ShowWarningIfEmpty = false, RelatedTo = "Percent")]
	[CalculatorProperty(Calculator.Items.Operator.MIN, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal4", ShowWarningIfEmpty = false, RelatedTo = "Minimum")]
	[CalculatorProperty(Calculator.Items.Operator.BAS, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal1", ShowWarningIfEmpty = false, RelatedTo = "BaseRate")]
	[CalculatorProperty(Calculator.Items.Operator.UNT, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal3", ShowWarningIfEmpty = false, RelatedTo = "PerUnit")]
	[CalculatorProperty(CalculatorConstants.Type.PRU, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal5", ShowWarningIfEmpty = false, RelatedTo = "PerUnitPercent")]
	[CalculatorProperty(Calculator.Items.Operator.Minus, RateLineItem.Schema.TM_RelevantValue)]
	[CalculatorProperty(Calculator.Items.Operator.Plus, RateLineItem.Schema.TM_RelevantValue)]
	[CalculatorProperty(Calculator.Items.CalculationOrder, RateLineItem.Schema.TM_Text, IsMandatory = true, MapTo = "String1", RelatedTo = "CalculationOrder")]
	[CalculatorProperty(CartageCalculator.Items.EquipmentType, RateLineItem.Schema.TM_Text, IsMandatory = true, MapTo = "String2", RelatedTo = "EquipmentType")]
	[CalculatorProperty(AgencyCalculator.Items.MessageType, RateLineItem.Schema.TM_Text, IsMandatory = true, MapTo = "String3", RelatedTo = "MessageType")]
	[CalculatorProperty(AgencyCalculator.Items.MessageSubType, RateLineItem.Schema.TM_Text, IsMandatory = true, MapTo = "String4", RelatedTo = "MessageSubType")]
	[CalculatorProperty(CalculatorConstants.Type.ApplyTo, RateLineItem.Schema.TM_Text)]
	public class CompanyTariffOrCostBasedCalculator : Calculator
	{
		public CompanyTariffOrCostBasedCalculator(IRateLine master)
			: base(master)
		{
		}

		public const string CostBasedCode = RatingCalculatorCodes.CostBased;
		public const string CompanyTariffBasedCode = RatingCalculatorCodes.CompanyTariffBased;

		#region Initialisation

		protected override IEnumerable<IRateLineItem> CheckOrCreateItems()
		{
			var perRateLineItem = FindRateLineItem(CalculatorConstants.Type.PER);
			var setFromMarkup = (Line.UsesCostBasedCalculator() && perRateLineItem == null);
			var setFromDiscount = (Line.UsesCompanyTariffBasedCalculator() && perRateLineItem == null);

			var itemList = base.CheckOrCreateItems();

			if (setFromMarkup && Line.ParentRateEntry != null)
			{
				var setPercent = true;
				var minimumAndPerUnit = Line.DefaultMarkupMinimumAndPerUnit();
				if (!minimumAndPerUnit[0].IsEmpty || !minimumAndPerUnit[1].IsEmpty)
				{
					setPercent = false;
					SetItem(Calculator.Items.Operator.MIN, minimumAndPerUnit[0], itemList);
					SetItem(Calculator.Items.Operator.UNT, minimumAndPerUnit[1], itemList);
				}

				if (setPercent)
				{
					SetItem(CalculatorConstants.Type.PER, Line.DefaultMarkupPercentage(), itemList);
				}
			}
			else if (setFromDiscount)
			{
				SetItem(CalculatorConstants.Type.PER, -Line.CompanyTariffDiscount(), itemList);
				SetItem(CalculatorConstants.Type.PRU, -Line.CompanyTariffDiscount(), itemList);
			}

			RateLineItems.CountChanged += RateLineItems_CountChanged;

			return itemList;
		}

		void SetItem(string itemType, ZDecimal value, IEnumerable<IRateLineItem> itemList)
		{
			var item1 = itemList.FirstOrDefault(item => item.TM_Type == itemType) as RateLineItem;

			if (item1 != null)
			{
				using (item1.SuspendSettingHasChanges())
				using (item1.GetValidationSuspender())
				{
					item1.TM_RelevantValue = value;
				}
			}
		}

		#endregion

		#region Properties

		public ZDecimal BaseRate
		{
			get { return (ZDecimal)this[Calculator.Items.Operator.BAS]; }
			set { this[Calculator.Items.Operator.BAS] = value; }
		}

		public ZDecimal BaseRateWithApplicableIncrease
		{
			get
			{
				var result = BaseRate;
				if (CalculationOrder == Items.IncreaseFirst)
				{
					result *= PercentageIncrease;
				}
				return result;
			}
		}

		public ZDecimal Percent
		{
			get { return (ZDecimal)this[CalculatorConstants.Type.PER]; }
			set { this[CalculatorConstants.Type.PER] = value; }
		}

		public ZDecimal PerUnitPercent
		{
			get { return (ZDecimal)this[CalculatorConstants.Type.PRU]; }
			set { this[CalculatorConstants.Type.PRU] = value; }
		}

		public ZDecimal PerUnitWithApplicableIncrease
		{
			get
			{
				var result = PerUnit;

				if (CalculationOrder == Items.IncreaseFirst)
				{
					result *= (PerUnitPercent + 100) / 100;
				}

				return result;
			}
		}

		public ZDecimal PerUnit
		{
			get { return (ZDecimal)this[Calculator.Items.Operator.UNT]; }
			set { this[Calculator.Items.Operator.UNT] = value; }
		}

		public ZDecimal MinimumWithApplicableIncrease
		{
			get
			{
				var result = Minimum;

				if (CalculationOrder == Items.IncreaseFirst)
				{
					result *= PercentageIncrease;
				}

				return result;
			}
		}

		public ZDecimal PercentageIncrease => (Percent + 100) / 100;

		public ZDecimal Minimum
		{
			get { return (ZDecimal)this[Calculator.Items.Operator.MIN]; }
			set { this[Calculator.Items.Operator.MIN] = value; }
		}

		public ZString CalculationOrder
		{
			get { return (ZString)this[Items.CalculationOrder]; }
			set { this[Items.CalculationOrder] = value; }
		}

		/// <summary>
		///		Gets or sets the line the CST calculator has to be applied to.
		///		It is used by Company Tariff module where the user can manually accept
		///		cost line to a tariff.
		/// </summary>
		public ZString ApplyToLine
		{
			get => (ZString)this[CalculatorConstants.Type.ApplyTo];
			set => this[CalculatorConstants.Type.ApplyTo] = value;
		}

		public bool IsCostBasedCalculator
		{
			get { return Line.UsesCostBasedCalculator(); }
		}

		public bool IsCompanyTariffBasedCalculator
		{
			get { return Line.UsesCompanyTariffBasedCalculator(); }
		}

		#region EquipmentType

		public override ZString EquipmentType
		{
			get { return (ZString)this[CartageCalculator.Items.EquipmentType]; }
			set { this[CartageCalculator.Items.EquipmentType] = value; }
		}

		protected override bool ShowEquipmentTypeInternal => true;

		RateLine ApplyToLineRateLine
		{
			get
			{
				if (applyToLineRateLine == null && !ApplyToLine.IsEmpty && ZGuid.TryParse(ApplyToLine, out var linePK))
				{
					applyToLineRateLine = Line.Factory.Load<RateLine>(linePK);
				}
				return applyToLineRateLine;
			}
		}
		RateLine applyToLineRateLine;

		#endregion

		#region MessageTypeSubType

		public override ZString MessageType
		{
			get { return (ZString)this[AgencyCalculator.Items.MessageType]; }
			set { this[AgencyCalculator.Items.MessageType] = value; }
		}

		public override ZString MessageSubType
		{
			get { return (ZString)this[AgencyCalculator.Items.MessageSubType]; }
			set { this[AgencyCalculator.Items.MessageSubType] = value; }
		}

		protected override bool ShowMessageTypeSubTypeInternal
		{
			get
			{
				return !Line.IsBulkRateUpdateActionLine() && Line.ChargeCode != null && Line.ChargeCode.AC_RateCalculator == AgencyCalculator.Code;
			}
		}

		#endregion

		protected internal override bool ValueIsReadOnly(RateLineItem item)
		{
			var result = item.IsDeleted || base.ValueIsReadOnly(item);
			result = result || ((item.RateOperatorIsPER() || item.RateOperatorIsUNT()) && RateLineItems.Count > 0);
			return result;
		}

		void RateLineItems_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (OldRateLineItemsCount == 0)
			{
				foreach (var item in RateLineBizO.RateLineItems.Cast<RateLineItem>())
				{
					item.ClearReadOnlyFields();
				}
			}

			OldRateLineItemsCount = RateLineItems.Count;
		}

		int OldRateLineItemsCount;

		#endregion

		#region Validation

		public override void ValidateTM_Text(RateLineItem lineItem)
		{
			base.ValidateTM_Text(lineItem);

			if (lineItem.TM_Type == Items.CalculationOrder)
			{
				ValidateCalculationOrderFields
				(
					lineItem,
					lineItem.TM_TextInfo,
					Percent,
					BaseRate,
					PerUnitPercent,
					PerUnit,
					calculationOrder: lineItem.TM_Text
				);

				if (!lineItem.TM_Text.IsEmpty)
				{
					ListValidation.ErrorIfInvalidCode(lineItem.TM_TextInfo, CalculationOrderList);
				}
			}
		}

		static void ValidateCalculationOrderFields(RateLineItem rateLineItem, ZPropertyInfo propertyInfo, ZDecimal percent, ZDecimal baseRate, ZDecimal perUnitPercent, ZDecimal perUnit, ZString calculationOrder)
		{
			if ((!rateLineItem.IsInDatabase || rateLineItem.HasChanges) // not validating for old record
				&& calculationOrder.IsEmpty
				&& ((percent != 0m && baseRate != 0m) || (perUnitPercent != 0m && perUnit != 0m)))
			{
				propertyInfo.AddError(ErrorMessages.CalculationOrderShouldBeSetWhenPercentageAndFixedChangeAreSet);
			}
		}

		public override void ValidateTM_Value(RateLineItem lineItem)
		{
			base.ValidateTM_Value(lineItem);

			ValidateValue(lineItem, lineItem.TM_Value, lineItem.TM_ValueInfo);
		}

		public override void ValidateTM_AgentDeclaredRate(RateLineItem lineItem)
		{
			base.ValidateTM_AgentDeclaredRate(lineItem);

			ValidateValue(lineItem, lineItem.TM_AgentDeclaredRate, lineItem.TM_AgentDeclaredRateInfo);
		}

		void ValidateValue(RateLineItem lineItem, ZDecimal value, ZPropertyInfo info)
		{
			if (Line.UsesCostBasedCalculator())
			{
				if (lineItem.RateOperatorIsPER())
				{
					if (value < Line.MinimumMarkupPercentage())
					{
						info.AddWarning(string.Format(CultureInfo.InvariantCulture, ErrorMessages.GetMinimumCostMarkupNotMet(Line.MinimumMarkupPercentage())));
					}
				}
				else if (lineItem.RateOperatorIsMIN() || lineItem.RateOperatorIsUNT())
				{
					var markups = Line.MinimumMarkupMinimumAndPerUnit();
					if ((!markups[0].IsEmpty || !markups[1].IsEmpty) &&
						((lineItem.RateOperatorIsMIN() && value < markups[0]) ||
						(lineItem.RateOperatorIsUNT() && value < markups[1])))
					{
						info.AddWarning(ErrorMessages.MinimumCostMarkupNotMet2);
					}
				}
			}

			if (lineItem.RateOperatorIsPER())
			{
				ValidateCalculationOrderFields
				(
					lineItem,
					info,
					percent: lineItem.TM_Value,
					BaseRate,
					PerUnitPercent,
					PerUnit,
					CalculationOrder
				);
			}
			else if (lineItem.RateOperatorIsBAS())
			{
				ValidateCalculationOrderFields
				(
					lineItem,
					info,
					Percent,
					baseRate: lineItem.TM_Value,
					PerUnitPercent,
					PerUnit,
					CalculationOrder
				);
			}
			else if (lineItem.RateOperatorIsUNT())
			{
				ValidateCalculationOrderFields
				(
					lineItem,
					info,
					Percent,
					BaseRate,
					PerUnitPercent,
					perUnit: lineItem.TM_Value,
					CalculationOrder
				);
			}
			else if (lineItem.RateOperatorIs(CalculatorConstants.Type.PRU))
			{
				ValidateCalculationOrderFields
				(
					lineItem,
					info,
					Percent,
					BaseRate,
					perUnitPercent: lineItem.TM_Value,
					PerUnit,
					CalculationOrder
				);
			}
		}

		protected override void ValidateTM_TypeCore(RateLineItem lineItem)
		{
			if (RateLineItems.Contains(lineItem))
			{
				ValidateGridCalculator(lineItem, false, false, false);
			}
		}

		protected override IEnumerable<RateLineItem> GetItemsForGridValidation(RateLineItem lineItem)
		{
			return RateLineBizO.RateLineItems
				.Cast<RateLineItem>()
				.Where(x => RateLineItems.Contains(x));
		}

		#endregion

		#region Lists

		CodeDescriptionPairList fCalculationOrderList;
		protected CodeDescriptionPairList CalculationOrderList
		{
			get { return fCalculationOrderList ?? (fCalculationOrderList = FindRateLineItem(Items.CalculationOrder).Lookups.CalculationOrderList); }
		}

		public override CodeDescriptionPairList List1
		{
			get { return CalculationOrderList; }
		}

		public override CodeDescriptionPairList List2
		{
			get { return RateLineBizO.Lookups.EquipmentTypes; }
		}

		public override CodeDescriptionPairList List3
		{
			get { return RateLineBizO.Lookups.MessageTypeList; }
		}

		public override CodeDescriptionPairList List4
		{
			get { return RateLineBizO.Lookups.MessageSubTypeList; }
		}

		#endregion

		#region Quotation Lines

		protected override QuotationLineList GetQuotationLinesInternal(GetQuotationLinesParam flags)
		{
			return GetQuotationLines();
		}

		public QuotationLineList GetQuotationLines()
		{
			throw new NotSupportedException("Cost/CompanyTariff based rates should be translated into cloned rates using other calculators before reaching this point.");
		}

		#endregion

		#region Calculation

		protected override (IEnumerable<CalculationResult> results, string error) CalculateResult(AutoRatingCalculatorParameters parameters)
		{
			if (isCalculating)
			{
				return (Enumerable.Empty<CalculationResult>(), string.Empty);
			}

			try
			{
				isCalculating = true;

				var (results, error) = CalculateResults(parameters);
				if (!string.IsNullOrEmpty(error))
				{
					return (Enumerable.Empty<CalculationResult>(), error);
				}

				if (!results.Any())
				{
					return (Enumerable.Empty<CalculationResult>(), GetFailedReason());
				}

				var unit = GetUnit(parameters);

				foreach (var result in results)
				{
					SetAttributes(result, parameters, Line, unit);
				}

				return (results, string.Empty);
			}
			finally
			{
				isCalculating = false;
			}
		}

		(IEnumerable<CalculationResult> result, string error) CalculateResults(AutoRatingCalculatorParameters parameters)
		{
			if (Line.RateCalculatorType == CalculatorType.CostBased)
			{
				// For CST calculator we calculate based on existing calculated charges on the job
				var (bases, error) = CalculateFromExistingCharges(parameters);
				if (!string.IsNullOrEmpty(error))
				{
					return (bases, error);
				}

				// If there are no related cost charges found on the job we want to fallback to related cost rates so that
				// it was possible to autorate revenue without autorating costs
				if (!bases.Any())
				{
					return CalculateFromRelatedRates(parameters);
				}

				return (bases, string.Empty);
			}
			else
			{
				return CalculateFromRelatedRates(parameters);
			}
		}

		(IEnumerable<CalculationResult> results, string error) CalculateFromExistingCharges(AutoRatingCalculatorParameters parameters)
		{
			var results = new List<CalculationResult>();

			var unit = GetUnit(parameters);
			var relatedCosts = GetRelatedExistingCosts(parameters);

			foreach (var cost in relatedCosts)
			{
				var (result, calculationError) = CalculateBases(cost.bases, parameters, unit);

				if (!string.IsNullOrEmpty(calculationError))
				{
					return (Enumerable.Empty<CalculationResult>(), calculationError);
				}

				if (result != null)
				{
					result.ChargePK = cost.charge.PK;
					results.Add(result);
				}
			}

			return (results, string.Empty);
		}

		IEnumerable<(IAutoRatingChargeInfo charge, IEnumerable<PaymentBasis> bases)> GetRelatedExistingCosts(AutoRatingCalculatorParameters parameters)
		{
			var result = new List<(IAutoRatingChargeInfo cost, IEnumerable<PaymentBasis> bases)>();

			var relatedCosts = parameters.Criteria.GetExistingCharges()
				.Where(c => IsCostApplicable(c, parameters))
				.ToArray();

			foreach (var cost in relatedCosts)
			{
				var bases = JobPaymentBasisExtensions.ConvertCostPaymentBasis(parameters.Criteria, cost, Line.ViewAgentRates);
				result.Add((cost, bases));
			}

			return result;
		}

		(IEnumerable<CalculationResult> results, string error) CalculateFromRelatedRates(AutoRatingCalculatorParameters parameters)
		{
			var results = new List<CalculationResult>();

			var unit = GetUnit(parameters);
			var lines = GetRelatedLines(parameters);

			foreach (var line in lines)
			{
				var originalValue = line.ViewAgentRates;

				try
				{
					line.SetViewAgentRatesWithoutRefreshBinding(Line.ViewAgentRates);

					var (calcResults, error) = line.Calculator.Calculate(parameters);
					if (!string.IsNullOrEmpty(error))
					{
						return (Enumerable.Empty<CalculationResult>(), error);
					}

					foreach (var calcResult in calcResults)
					{
						var (result, calculationError) = CalculateBases(calcResult.PaymentBases, parameters, unit);

						if (!string.IsNullOrEmpty(calculationError))
						{
							return (Enumerable.Empty<CalculationResult>(), calculationError);
						}

						if (result != null)
						{
							result.ChargePK = calcResult.ChargePK;
							results.Add(result);
						}
					}
				}
				finally
				{
					line.SetViewAgentRatesWithoutRefreshBinding(originalValue);
				}
			}

			return (results, string.Empty);
		}

		#region Related Lines

		public IEnumerable<IRateLine> GetRelatedLines(AutoRatingCalculatorParameters parameters)
		{
			// First we look for Accepted cost line (cost rate can be accepted in tariff module)
			if (ApplyToLineRateLine != null)
			{
				var isCosting = ParentRateEntry.IsCosting();
				var (_, jobDate) = parameters.Criteria.GetEffectiveDate(ApplyToLineRateLine, shouldAcceptInvalidDateWhenFallbackIsEnabled: false, isCosting: isCosting);

				if (ApplyToLineRateLine.IsJobDateOutOfRange(jobDate))
				{
					var lines = GetRelatedLinesCore(parameters);
					if (lines.Any())
					{
						return lines;
					}
				}

				return [ApplyToLineRateLine];
			}

			// if there are no accepted cost, we look for matching cost among all costs
			return GetRelatedLinesCore(parameters);
		}

		IEnumerable<IRateLine> GetRelatedLinesCore(AutoRatingCalculatorParameters parameters)
		{
			var cache = parameters.Criteria.Cache.CostBasedCalculatorRelatedLines;

			List<IRateLine> result;

			if (!cache.TryGetValue(Line.PK, out result))
			{
				cache.Add(Line.PK, result = new List<IRateLine>());

				if (Line.ChargeCode != null)
				{
					var relatedLines = Line.UsesCostBasedCalculator()
						? GetRelatedCostLines(parameters)
						: GetRelatedTariffLines(parameters);

					result.AddRange(relatedLines);
					result.Remove(Line);
				}
			}

			return result;
		}

		IEnumerable<IRateLine> GetRelatedCostLines(AutoRatingCalculatorParameters parameters)
		{
			var criteria = parameters.Criteria;
			var costsLoader = new CostRatesLoader(Line.Factory, new DummyLogger());

			// We should be able to call costLoader.Load without providing transport providers. It should load
			// transport providers for the criteria itself which in theory should be the same as the ones required by
			// the CST calculator. But, there is an old logic which defines a specific order of providers (GetTransportProviders)
			// for CST calculator purposes and a weird test (CompanyTariffOrCostLineCloneHelperTest.TestCostingFromSupplierThenCarrier)
			// which fail if the order is different. So, we have to keep this old logic. But it is something to investigate. It should not be like this.
			var transportProviders = GetTransportProviders(Line, criteria);
			var costs = costsLoader.Load(criteria, transportProviders);

			var oldCostChargesFilter = criteria.ChargeCodeGroups.CostChargesFilter;
			try
			{
				criteria.ChargeCodeGroups.CostChargesFilter = ChargeCodeFilter.AutorateAll;

				var lines = FilterRelatedLines(costs, parameters, true);
				return lines;
			}
			finally
			{
				criteria.ChargeCodeGroups.CostChargesFilter = oldCostChargesFilter;
			}
		}

		/// <summary>
		///		Returns transport providers to check for the related cost lines. As per comment in GetRelatedCostLines, it is expected
		///		to be a specific order. The list looks a bit weird, as it has allProvidersAndContractors twice, it feels like it was done
		///		on purpose for some reason to maintain an order, but it is not clear why. It is something to investigate. It is
		///		a very old functionality.
		/// </summary>
		ZGuid[] GetTransportProviders(IRateLine line, RatingCriteria criteria)
		{
			var transportProviders = new HashSet<ZGuid>();

			var allProvidersAndContractors = line.ChargeCode != null ? criteria.GetTransportProvidersAndContractorPKs(line.ChargeCode) : new HashSet<ZGuid>();

			if (!line.ParentRateEntry.TI_OH_Supplier.IsEmpty && allProvidersAndContractors.Contains(line.ParentRateEntry.TI_OH_Supplier))
			{
				transportProviders.Add(line.ParentRateEntry.TI_OH_Supplier);
				transportProviders.UnionWith(allProvidersAndContractors);
			}

			transportProviders.Add(line.ParentRateEntry.TI_OH_TransportProvider);
			transportProviders.UnionWith(allProvidersAndContractors);
			return transportProviders.ToArray();
		}

		IEnumerable<IRateLine> GetRelatedTariffLines(AutoRatingCalculatorParameters parameters)
		{
			var thisRatingHeader = Line.ParentRateEntry.ParentRatingHeader;
			var thisRateEntry = Line.ParentRateEntry;

			if (!Line.TL_FeeChargeType.IsEmpty && !Line.IsFeeChargeApplicable(thisRatingHeader.Header))
			{
				return [];
			}

			var criteria = parameters.Criteria;
			var company = criteria.Company;
			var effectiveDate = criteria.GetEffectiveDateWithFallback(Line, isCosting: _Rating.Cost).date;

			var baseCompanyTariffLevel = thisRateEntry.IsCompanyTariff()
				? 1
				: RatingCache.GetCompanyTariffLevel(thisRateEntry.ParentRatingHeader.Header, thisRateEntry.Company(), thisRateEntry.TI_RateCategory, thisRateEntry.TI_Mode, criteria.Direction, effectiveDate).level;

			if (baseCompanyTariffLevel == 0)
			{
				baseCompanyTariffLevel = 1;
			}

			var ratesLoader = parameters.AutoRater.RevenueRatesLoader;
			var tariffs = RatingCache.GetCompanyTariffs(baseCompanyTariffLevel, company, () => ratesLoader.GetCompanyTariff(baseCompanyTariffLevel, company));
			var rates = ratesLoader.Load(criteria, tariffs);
			if (!rates.Any())
			{
				return [];
			}

			var lines = FilterRelatedLines(rates, parameters, false);
			lines = lines
				.Where(Line.IsFeeChargeSame)
				.Where(x => x.ParentRateEntry.ParentRatingHeader.TH_GlobalRateLevel != thisRatingHeader.TH_GlobalRateLevel)
				.Distinct();

			return lines;
		}

		IEnumerable<IRateLine> FilterRelatedLines(IEnumerable<IRateEntry> rates, AutoRatingCalculatorParameters parameters, bool isCosting)
		{
			var criteria = parameters.Criteria;
			var rateType = isCosting ? (NoResString)"Costing" : (NoResString)"Company Tariff";
			var logPrefix = ZString.Format((NoResString)"(Loading {0} for {1})", rateType, Line.DisplayInfo());
			var loggerWithPrefix = parameters.Logger.WithPrefix(logPrefix + " ").DebugOnly();     //this one is too hard for humans to comprehend

			var filteredRates = RateEntryFilter.Filter(criteria, isCosting, rates, parameters.Factory, loggerWithPrefix);
			filteredRates = filteredRates.Where(x => !Line.ParentRateEntry.TI_RC.IsValid || x.IsSameContainerOrSameClass(Line.ParentRateEntry.TI_RC, Line.ParentRateEntry.TI_MatchContainerRateClass))
				.Where(x =>
				{
					var carrier = x.TI_OH_TransportProvider.IsEmpty
						? x.ParentRatingHeader.TH_OH
						: x.TI_OH_TransportProvider;

					return
						Line.ParentRateEntry.TI_OH_TransportProvider.IsEmpty ||
						carrier.IsEmpty ||
						carrier == Line.ParentRateEntry.TI_OH_TransportProvider;
				})
				.ToList();

			using (var linesRepository = new RateLinesRepository(criteria, filteredRates.ToList(), Line.ChargeCode, loggerWithPrefix))
			{
				var fastLine = criteria.Cache.GetOrCreateFastLine(Line);
				linesRepository.Remove(fastLine, ZString.Format((NoResString)"using {0} line instead", rateType));      // log message, subject to change, more for support people as of now

				var options = new NotApplicableRateLineRemover.FilterOptions
				{
					IsCosting = isCosting,
					DisablePaymentTermsFilter = true,
					DisableIrrelevantTariffsFilter = true,
					IsRebateCalculationMode = parameters.RatingContext.IsInRebateCalculationMode
				};

				var remover = new NotApplicableRateLineRemover();
				remover.RemoveNotApplicable(parameters, linesRepository, options, parameters.RatingContext.DialogService);
				parameters.RemoveSimilarCharges(linesRepository);

				return linesRepository.GetIRateLines().WhereNotNull().ToList();
			}
		}

		#endregion

		(CalculationResult results, string error) CalculateBases(IEnumerable<PaymentBasis> bases, AutoRatingCalculatorParameters parameters, string unit)
		{
			if (!bases.Any())
			{
				return (null, string.Empty);
			}

			var basesInRateCurrency = GetInRateCurrency(bases, parameters.Criteria);
			var mergedBases = MergePaymentBases(basesInRateCurrency);

			var (updatedBases, error) = ApplyChanges(mergedBases, parameters);
			if (!string.IsNullOrEmpty(error))
			{
				return (null, error);
			}

			var result = new CalculationResult(Line, updatedBases, unit);
			return (result, string.Empty);
		}

		#region Cost Applicability

		bool IsCostApplicable(IAutoRatingChargeInfo cost, AutoRatingCalculatorParameters parameters)
		{
			var costAttributes = cost.RateAttributes;
			var result = cost.ChargeCode?.PK == Line.TL_AC;
			result = result && IsApplicableByTransportProvider(cost);
			result = result && IsApplicableByCartageLeg(costAttributes, parameters);
			result = result && IsApplicableByContainerType(costAttributes);
			result = result && IsApplicableByContainerNumber(costAttributes, parameters);
			result = result && IsApplicableByCommodity(costAttributes);
			result = result && IsApplicableByProduct(costAttributes, parameters);

			return result;
		}

		bool IsApplicableByTransportProvider(IAutoRatingChargeInfo cost)
		{
			return ParentRateEntry.TI_OH_TransportProvider.IsEmpty ||
					cost.CostAccountPK.IsEmpty ||
					cost.CostAccountPK == ParentRateEntry.TI_OH_TransportProvider;
		}

		bool IsApplicableByCartageLeg(RateAttributeSet costAttributes, AutoRatingCalculatorParameters parameters)
		{
			var cartageLeg = costAttributes.GetSingleValue<Guid>(JobChargeAttribTypeList.Codes.CartageLegPK);

			return cartageLeg == Guid.Empty ||
				parameters.CartageLegPKFilter.IsEmpty ||
				cartageLeg == parameters.CartageLegPKFilter;
		}

		bool IsApplicableByContainerType(RateAttributeSet costAttributes)
		{
			if (Line.ParentRateEntry.Container == null)
			{
				return true;
			}

			var containerCode = costAttributes.GetSingleValue<string>(JobChargeAttribTypeList.Codes.ContainerCode);
			if (containerCode.IsNullOrEmpty())
			{
				return true;
			}

			if (Line.ParentRateEntry.TI_MatchContainerRateClass)
			{
				var costContainer = Line.Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, containerCode));
				if (costContainer == null)
				{
					return false;
				}

				return costContainer.RC_HandlingRateClass == Line.ParentRateEntry.Container.RC_HandlingRateClass;
			}
			else
			{
				return containerCode == Line.ParentRateEntry.Container.RC_Code;
			}
		}

		bool IsApplicableByContainerNumber(RateAttributeSet costAttributes, AutoRatingCalculatorParameters parameters)
		{
			var containerNumber = costAttributes.GetSingleValue<string>(JobChargeAttribTypeList.Codes.ContainerNumber);
			return containerNumber.IsNullOrEmpty() ||
				   !parameters.ContainerNumberFilter.HasValue ||
				   parameters.ContainerNumberFilter.Value.IsEmpty ||
				   containerNumber == parameters.ContainerNumberFilter.Value;
		}

		bool IsApplicableByCommodity(RateAttributeSet costAttributes)
		{
			var commodity = costAttributes.GetSingleValue<string>(JobChargeAttribTypeList.Codes.Commodity);
			return Line.ParentRateEntry.TI_RH_NKCommodityCode.IsEmpty ||
				   commodity.IsNullOrEmpty() ||
				   Line.ParentRateEntry.TI_RH_NKCommodityCode == commodity;
		}

		bool IsApplicableByProduct(RateAttributeSet costAttributes, AutoRatingCalculatorParameters parameters)
		{
			var product = parameters.ProductFilter;
			var chargeProduct = costAttributes.GetSingleValue<string>(JobChargeAttribTypeList.Codes.Product);

			return
				product == null ||
				product.OP_PartNum.IsEmpty ||
				chargeProduct.IsNullOrEmpty() ||
				product.OP_PartNum == chargeProduct;
		}

		#endregion

		IEnumerable<PaymentBasis> GetInRateCurrency(IEnumerable<PaymentBasis> bases, RatingCriteria criteria)
		{
			var adjustedBases = bases
				.Select(b => GetInRateCurrency(b, criteria))
				.Where(b => b.HasValue)
				.Select(b => b.Value)
				.ToList();

			return adjustedBases;
		}

		PaymentBasis? GetInRateCurrency(PaymentBasis basis, RatingCriteria criteria)
		{
			if (basis.Currency == Line.TL_RX_NKCurrency || Line.TL_RX_NKCurrency.IsEmpty || basis.Currency.IsEmpty)
			{
				return basis;
			}

			var exchangeRateType = GetExchangeRateType();
			var baseCurrency =
				criteria.Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code,
					basis.Currency);

			ZDecimal newPerUnit = basis.RateInfo.PerUnitRate.GetValueOrDefault();
			ZDecimal newFlatRate = basis.RateInfo.FlatRate.GetValueOrDefault();
			ZDecimal newMinRate = basis.RateInfo.MinRate;
			ZDecimal newMaxRate = basis.RateInfo.MaxRate;

			if (basis.RateInfo.PerUnitRate.HasValue &&
				!criteria.Convert(ref newPerUnit, baseCurrency, Line, exchangeRateType))
			{
				return null;
			}

			if (basis.RateInfo.FlatRate.HasValue &&
				!criteria.Convert(ref newFlatRate, baseCurrency, Line, exchangeRateType))
			{
				return null;
			}

			if (basis.RateInfo.MinRate != decimal.MinValue &&
				!criteria.Convert(ref newMinRate, baseCurrency, Line, exchangeRateType))
			{
				return null;
			}

			if (basis.RateInfo.MaxRate != decimal.MaxValue &&
				!criteria.Convert(ref newMaxRate, baseCurrency, Line, exchangeRateType))
			{
				return null;
			}

			var newRateInfo = RateInfo.GetWithConvertedCurrency(
				basis.RateInfo,
				basis.RateInfo.PerUnitRate.HasValue ? newPerUnit : null,
				basis.RateInfo.FlatRate.HasValue ? newFlatRate : null,
				newMinRate,
				newMaxRate,
				Line.TL_RX_NKCurrency);

			var unitDescription = basis.RateInfo.UnitDescription;
			if (!unitDescription.HasValue && basis.RateInfo.Unit.HasValue)
			{
				unitDescription = UnitDescriptionInternal(basis.RateInfo.Unit.Value, basis.RateInfo.UnitMultiplier);
			}

			newRateInfo.UnitDescription = unitDescription;

			return basis.UpdateRateInfo(newRateInfo);
		}

		#region Apply Changes

		(IEnumerable<PaymentBasis> updatedBases, string error) ApplyChanges(IEnumerable<PaymentBasis> costPaymentBases, AutoRatingCalculatorParameters parameters)
		{
			var updatedPaymentBases = new List<PaymentBasis>();

			var units = GetCostUnit(costPaymentBases);
			if (!units.AllSame() && PerUnit != 0)
			{
				// We don't have Unit on CST calculator and thus applying Per Unit Increase to a charge calculated for different units
				// will result to at least strange result. For example, for cost calculated as:
				//	100 KG x  $10 @ KG
				//    2 M3 x $200 @ M3
				//
				// Applying +$5 Per Unit Increase will be strange as for different unit it will have different weight.
				// To fix it, we need to add support of units to CST calculator.
				return (Enumerable.Empty<PaymentBasis>(), Res.GetString("84127018-8612-49c9-835e-b867fb01fac6", "Per Unit Increase can't be applied as related cost is calculated for multiple units"));
			}

			foreach (var basis in costPaymentBases)
			{
				if (basis.RateInfo.Type == RateInfo.RateInfoType.FLT || basis.RateInfo.IsPercentage || basis.RateInfo.IsPartThereof)
				{
					if (Percent != 0)
					{
						var percentageInfo = RateInfo.CreatePER(Percent + 100, Line.TL_RX_NKCurrency);
						var updatedBasis = PaymentBasis.PercentageFromOriginal(basis, percentageInfo);

						updatedPaymentBases.Add(updatedBasis);
					}
					else
					{
						updatedPaymentBases.Add(basis);
					}
				}
				else if (basis.RateInfo.Type == RateInfo.RateInfoType.UNT)
				{
					var perUnitRate = IsSliding()
						? GetBreakItem(Line.ChildRateLineItems, parameters, basis.Chargeable)?.TM_RelevantValue ?? 0
						: PerUnit;

					// Unit multiplier gets applied to per unit rate in payment basis. For example, the following rate:
					// $100 per 1000 KG gets stored as $0.1 per 1 KG. But, to properly apply per unit increase, we need to
					// apply it to the original rate value.
					//
					// So, the following happens:
					// 1. Cost rate $100 per 1000 KG is stored as $0.1 per 1 KG
					// 2. $0.1 per 1 KG gets converted back to $100 per 1000 KG
					// 3. Per Unit Increase (lets say $20) gets applied to $100, i.e. the rate becomes $120 per 1000 KG
					// 4. We apply multiplier to the updated value so that it is properly stored like underlying cost, i.e.:
					//    $120 per 1000 KG gets converted to $0.12 per 1 KG
					var unitMultiplier = basis.RateInfo.UnitMultiplier ?? 1;
					var costRate = basis.RateInfo.PerUnitRate.Value * unitMultiplier;
					var updatedRate = IncreaseAmount(costRate, perUnitRate, PerUnitPercent);
					updatedRate /= unitMultiplier;

					var chargeableUnitDescription = GetChargeableUnitDescription(basis);
					var rateUnitDescription = UnitDescriptionInternal(basis.RateInfo.Unit.Value, unitMultiplier: basis.RateInfo.UnitMultiplier);

					var updatedBasis = parameters.Criteria.CreatePaymentBasis(
						RateInfo.CreateUNT(
							updatedRate,
							basis.RateInfo.Unit.Value,
							Line.TL_RX_NKCurrency,
							rateUnitDescription,
							basis.RateInfo.RateInfoDescription,
							basis.RateInfo.CartageZoneDescription,
							basis.RateInfo.UnitMultiplier,
							basis.RateInfo.IsPartThereof),
						basis.Chargeable,
						chargeableUnitDescription,
						basis.ChargeableDescription);
					updatedBasis.UnroundedChargeable = basis.UnroundedChargeable;

					updatedPaymentBases.Add(updatedBasis);
				}
				else if (basis.RateInfo.Type == RateInfo.RateInfoType.MIN)
				{
					// Handled separately
				}
				else if (basis.RateInfo.Type == RateInfo.RateInfoType.MAX)
				{
					var updatedAmount = IncreaseAmount(basis.RateInfo.MaxRate, 0, Percent);
					var newRate = RateInfo.CreateMAX(updatedAmount, Line.TL_RX_NKCurrency);
					var updatedBasis = parameters.Criteria.CreatePaymentBasis(newRate, default);

					updatedPaymentBases.Add(updatedBasis);
				}
				else
				{
					updatedPaymentBases.Add(basis);
				}
			}

			var costMinBasis = costPaymentBases.SingleOrDefault(b => b.RateInfo.Type == RateInfo.RateInfoType.MIN);
			var updatedMin = IncreaseAmount(costMinBasis.RateInfo.MinRate, Minimum, Percent);
			if (updatedMin != 0)
			{
				var newRate = RateInfo.CreateMIN(updatedMin, Line.TL_RX_NKCurrency);
				var minBasis = parameters.Criteria.CreatePaymentBasis(newRate, default);
				updatedPaymentBases.Add(minBasis);
			}

			if (BaseRate != 0)
			{
				var baseAmount = IncreaseAmount(0, BaseRate, Percent);
				var baseBases = parameters.Criteria.CreatePaymentBasis(RateInfo.CreateFLT(baseAmount, Line.TL_RX_NKCurrency), default);
				updatedPaymentBases.Add(baseBases);
			}

			return (updatedPaymentBases, string.Empty);
		}

		IEnumerable<ZString> GetCostUnit(IEnumerable<PaymentBasis> bases)
		{
			var costUnits = bases
				.Where(b => b.RateInfo.Type == RateInfo.RateInfoType.UNT && !b.RateInfo.IsPercentage && !b.RateInfo.IsPartThereof && b.RateInfo.Unit.HasValue)
				.Select(b => b.RateInfo.Unit.Value)
				.ToArray();

			return costUnits;
		}

		string GetChargeableUnitDescription(PaymentBasis cost)
		{
			if (cost.ChargeableUnitDescription.HasValue)
			{
				return cost.ChargeableUnitDescription.Value;
			}

			if (!cost.RateInfo.Unit.HasValue)
			{
				return string.Empty;
			}

			// For containers, we want unit to include container code so that chargeable description looks like this:
			// 5 20GP Container(s)
			//
			// The UnitDescriptionInternal in the base Calculator class does it, but it adds container code
			// from the parent RateEntry. But, the CST calculator is applied to related cost or company tariff and thus container code
			// on the related cost or company tariff has to be used.
			if (cost.RateInfo.Unit.Value == QuantityUnit.CN)
			{
				// Whenever possible, chargeable unit for per container calculation is container code, i.e. 20GP, 40GP, etc.
				// Otherwise it is generic container unit - CN. We don't want to add generic CN to the description. If container
				// code can't be determined then it has to be null so that generated description is "5 Container(s)" instead of
				// "5 CN Container(s)"
				//
				// It smells, but this is all we can do for now. Payment Basis doesn't save chargeable description, so, we should do
				// our best to generate it from fields we have.
				var containerCode = cost.Chargeable.Unit != QuantityUnit.CN
					? cost.Chargeable.Unit
					: null;

				return GetContainerUnitDescription(containerCode, true);
			}

			return UnitDescriptionInternal(cost.RateInfo.Unit.Value, addPlural: true, addContainerCode: true);
		}

		decimal IncreaseAmount(decimal amount, decimal flatIncrease, decimal percentageIncrease)
		{
			if (amount == 0 && flatIncrease == 0)
			{
				return 0;
			}

			var percent = (percentageIncrease + 100) / 100;
			var increasedAmount = CalculationOrder == Items.IncreaseFirst
				? (amount + flatIncrease) * percent
				: amount * percent + flatIncrease;

			return increasedAmount;
		}

		#endregion

		ExchangeRateType GetExchangeRateType() => _Rating.Cost
			? ExchangeRateType.Buy
			: ExchangeRateType.Sell;

		string GetFailedReason()
		{
			return Res.GetString("ef198055-c16b-490b-b6dd-034775cd21e6",
				"charge code is using the {0} Based Calculator, however there are conflicting or no {1} rates found.",
				IsCompanyTariffBasedCalculator
						? Res.GetString("76705bdc-5da8-4132-b65b-7d098c01d5bb", "Company Tariff")
						: Res.GetString("bc167e6b-5da4-487c-b88e-d025744b8b68", "Cost"),
					IsCompanyTariffBasedCalculator
						? Res.GetString("49155880-a597-4ccd-8590-2188bdffadf2", "tariff")
						: Res.GetString("14bc5903-106c-4f64-ad02-fa112f2e4bf1", "costing"));
		}

		public override Calculator GetBaseCalculator(AutoRatingCalculatorParameters parameters)
		{
			Calculator result;
			if (!parameters.Criteria.Cache.BaseCalculators.TryGetValue(Line.PK, out result))
			{
				if (isGettingBaseCalculator)
				{
					return this;
				}

				IEnumerable<IRateLine> originalLines;

				try
				{
					isGettingBaseCalculator = true;

					originalLines = GetRelatedLines(parameters);

					result = !originalLines.Any()
						? this
						: originalLines.First().Calculator.GetBaseCalculator(parameters);
				}
				finally
				{
					isGettingBaseCalculator = false;
				}

				ReportErrorForDuplicateRateLinesInCriteriaCacheBaseCalculators(RateLineBizO, originalLines, parameters.Criteria.Cache.BaseCalculators);

				parameters.Criteria.Cache.BaseCalculators.Add(Line.PK, result);
			}

			return result;
		}
		bool isGettingBaseCalculator;

		void ReportErrorForDuplicateRateLinesInCriteriaCacheBaseCalculators(RateLine rateLine, IEnumerable<IRateLine> originalRateLines, Dictionary<ZGuid, Calculator> baseCalculatorCache)
		{
			if (baseCalculatorCache.ContainsKey(rateLine.PK))
			{
				ErrorReporter.ReportOnce
				(
					"CompanyTariffOrCostBasedCalculator.DuplicateRateLinesInCriteriaCacheBaseCalculators",
					$@"CST/CTB Calculator:
{LogRateLine(RateLineBizO)}

Original RateLines:
{string.Join("\r\n\r\n", originalRateLines.Select(originaLine => LogRateLine(originaLine)))}"
				);
			}

			string LogRateLine(IRateLine rateLine)
				=> $@"* Factory => Instance: {rateLine.Factory._Instance}
* RatingHeader => HashCode: {rateLine.ParentRateEntry.ParentRatingHeader.GetHashCode()}, PK: {rateLine.ParentRateEntry.ParentRatingHeader.PK}, RateType: {rateLine.ParentRateEntry.ParentRatingHeader.TH_RateType}, GlobalRateLevel: {rateLine.ParentRateEntry.ParentRatingHeader.TH_GlobalRateLevel}, OrgHeader: {rateLine.ParentRateEntry.ParentRatingHeader.Header?.OH_Code ?? ""}
* RateEntry => HashCode: {rateLine.ParentRateEntry.GetHashCode()}, PK: {rateLine.ParentRateEntry.PK}, Category: {rateLine.ParentRateEntry.TI_RateCategory}, Mode: {rateLine.ParentRateEntry.TI_Mode}, Origin: {rateLine.ParentRateEntry.TI_OriginLRC}, Destination: {rateLine.ParentRateEntry.TI_DestinationLRC}
* RateLine => HashCode: {rateLine.GetHashCode()}, PK: {rateLine.PK}, Charge: {rateLine.ChargeCode?.AC_Code ?? ""}, Description: {rateLine.TL_RateDesc}, Calculator: {rateLine.TL_RateCalculator}";
		}

		#region GetUnits

		protected internal override IEnumerable<ZString> GetUnits(AutoRatingCalculatorParameters parameters)
		{
			if (!parameters.Criteria.Cache.CostBasedCalculatorUnits.TryGetValue(Line.PK, out var units))
			{
				units = GetRelatedUnits(parameters);
			}

			return units;
		}

		IEnumerable<ZString> GetRelatedUnits(AutoRatingCalculatorParameters parameters)
		{
			var units = Enumerable.Empty<ZString>();

			if (Line.RateCalculatorType == CalculatorType.CostBased)
			{
				// Since the new CST calculator calculates amounts using existing charges,
				// we try to find units on charges we are going to calculate...
				var costs = GetRelatedExistingCosts(parameters);
				units = costs
					.Select(c =>
					{
						// If a charge has different units it is ignored as Cost Based calculator doesn't
						// support cost charges calculated from multiple units
						return GetCostUnit(c.bases).SameOrDefault();
					})
					.Distinct()
					.Where(u => !u.IsEmpty)
					.ToArray();
			}

			if (!units.Any())
			{
				var baseCalc = GetBaseCalculator(parameters);
				if (baseCalc != this)
				{
					units = baseCalc.GetUnits(parameters);
				}
			}

			return units;
		}

		#endregion

		#endregion

		#region GetCloneCode

		internal override ZString GetCloneCode(CompanyTariffOrCostBasedCalculator source)
		{
			return ZString.Empty;
		}

		#endregion

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders => false;

		public override bool IsMeasureTypeMatchApplicable => false;

		bool isCalculating;
	}
}

