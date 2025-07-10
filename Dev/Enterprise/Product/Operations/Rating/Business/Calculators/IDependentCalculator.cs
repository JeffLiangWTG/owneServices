using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Rating.Business
{
	public interface IDependentCalculator
	{
		IRateLine Master { get; }
	}

	public static class DependentCalculatorExtensions
	{
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "By design.")]
		public static Quantity GetValueCalculatorAppliesTo(this IDependentCalculator calculator, AutoRatingCalculatorParameters parameters, out IRateLineItem greaterChargeItem, bool includeGST, bool useGreaterCharge, bool showFullSource)
		{
			var results = new List<Quantity>();
			var applicableItems = calculator.Master.ChildRateLineItems.Where(i => i.RateOperatorIsApplyToOrMNT());

			greaterChargeItem = null;
			var greaterChargeItemMoneyList = new List<Quantity>();

			foreach (var currentItem in applicableItems)
			{
				var currentItemMoney = GetAmountItemAppliesTo(currentItem, includeGST, parameters);
				if (currentItemMoney.Any(x => !x.IsValid))
				{
					return default(Quantity);
				}

				if (useGreaterCharge
					&& (!greaterChargeItemMoneyList.Any()
						|| currentItemMoney.Sum(x => x.Amount) >= greaterChargeItemMoneyList.Sum(x => x.Amount)))
				{
					greaterChargeItemMoneyList = currentItemMoney;
					greaterChargeItem = currentItem;
				}

				results.AddRange(currentItemMoney);
			}

			var moneyList = useGreaterCharge && greaterChargeItem != null
				? greaterChargeItemMoneyList
				: results;

			var total = moneyList.Sum(money => money.Amount);
			var source = GetSourceDescription(moneyList, parameters.Factory, showFullSource);

			return new Money(total, calculator.Master.Currency, source);
		}

		static List<Quantity> GetAmountItemAppliesTo(IRateLineItem applyToItem, bool includeGST, AutoRatingCalculatorParameters parameters)
		{
			switch (applyToItem.TM_Text.ToUpper())
			{
				case CalculatorConstants.Text.ChargeCode:
				case CalculatorConstants.Text.MIN_Job:
				case CalculatorConstants.Text.MIN_ChargeCode:
				case CalculatorConstants.Text.AllCharges:
				case CalculatorConstants.Text.OriginCharges:
				case CalculatorConstants.Text.FreightCharges:
				case CalculatorConstants.Text.DestinationCharges:
				case CalculatorConstants.Text.LoadingCharges:
				case CalculatorConstants.Text.OriginCustomsBrokerageCharges:
				case CalculatorConstants.Text.CustomsBrokerageCharges:
				case CalculatorConstants.Text.UnloadingCharges:
				case Calculator.Items.Value.DisbursementApplyToTypes.Disbursements:
				case Calculator.Items.Value.DisbursementApplyToTypes.CustomsDisbursement:
					return GetApplicableMoneyForItem(applyToItem, includeGST, parameters);

				default:
					return new List<Quantity> { parameters.Criteria.GetApplicableValue(parameters.Logger, applyToItem) };
			}
		}

		class DependentCharge
		{
			public DependentCharge(IAutoRatingChargeInfo jobCharge, CostSell costSell, AutoRatingCalculatorParameters parameters)
			{
				this.charge = Argument.NotNull(jobCharge, "jobCharge");
				this.parameters = Argument.NotNull(parameters, "parameters");

				OperationalJobRef = costSell == CostSell.Cost ? jobCharge.CostReference : jobCharge.SellReference;
				RatingBehavior = costSell == CostSell.Cost ? jobCharge.CostRatingBehavior : jobCharge.SellRatingBehavior;
				ChargeCode = jobCharge.ChargeCode;

				ContainerCode = jobCharge.RateAttributes?.GetSingleValue<string>(JobChargeAttribTypeList.Codes.ContainerCode);
				CommodityCode = jobCharge.RateAttributes?.GetSingleValue<string>(JobChargeAttribTypeList.Codes.Commodity);
			}

			public DependentCharge(AutoRateInfo info)
			{
				this.info = Argument.NotNull(info, "info");

				OperationalJobRef = info.OperationalJobRef;
				ContainerCode = info.Line?.ParentRateEntry?.Container?.RC_Code ?? ZString.Empty;
				CommodityCode = info.Line?.ParentRateEntry?.TI_RH_NKCommodityCode ?? ZString.Empty;
				ChargeCode = info.ChargeCode;
				AutoRatedFor = info.AutoRatedFor.FirstOrDefault();

				var containerAttr = info.Attributes.Attributes
					.FirstOrDefault(c => c.Code == JobChargeAttribTypeList.Codes.ContainerCode);

				var commodityAttr = info.Attributes.Attributes
					.FirstOrDefault(c => c.Code == JobChargeAttribTypeList.Codes.Commodity);

				ContainerCode = containerAttr?.Value ?? ZString.Empty;
				CommodityCode = commodityAttr?.Value ?? ZString.Empty;
			}

			public BusinessObject AutoRatedFor { get; }
			public string OperationalJobRef { get; }
			public AccChargeCode ChargeCode { get; }
			public ZString RatingBehavior { get; }
			public ZString ContainerCode { get; }
			public ZString CommodityCode { get; }

			public RefContainer Container
			{
				get
				{
					var factory = info?.Factory ?? parameters.Factory;
					return factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, ContainerCode);
				}
			}

			public bool IsJobCharge => charge != null;

			readonly IAutoRatingChargeInfo charge;
			readonly AutoRateInfo info;
			readonly AutoRatingCalculatorParameters parameters;

			public bool IsPercentageApplied(IRateLine percentageRateLine)
			{
				if (info != null)
				{
					return info.PercentageLinesApplied.Contains(percentageRateLine.PK);
				}

				if (charge != null && parameters.RatingContext.PercentageLinesApplied.TryGetValue(charge.PK, out var percentageLinesApplied))
				{
					return percentageLinesApplied.Contains(percentageRateLine.PK);
				}

				return false;
			}

			public bool CanReautorate(IRateLineItem applyToItem)
			{
				return charge == null || charge.CanReautorate(applyToItem.CostOrSell(), parameters.Criteria.OperationalJobCode);
			}

			public Money GetAmount(IRateLineItem applyToItem, RatingCriteria criteria)
			{
				ZDecimal amount;
				var isRevenue = applyToItem.CostOrSell() == CostSell.Revenue;

				if (charge != null)
				{
					var currency = isRevenue ? charge.SellCurrency : charge.CostCurrency;
					if (currency != null)
					{
						amount = isRevenue ? charge.SellAmount : charge.CostAmount;
					}
					else
					{
						amount = isRevenue ? charge.LocalSellAmount : charge.LocalCostAmount;
						currency = criteria.Company.LocalCurrency;
					}

					return new Money(amount, currency, charge.ChargeCode.AC_Code + "*");
				}

				if (applyToItem.ParentRateLine.ViewAgentRates)
				{
					amount = info.AgentAmount;
					if (amount.IsEmpty)
					{
						if (isRevenue && !RatingDataRegistry.Instance.UnspecifiedSellShouldForceZeroToBePulledThrough.Value)
						{
							amount = info.Amount;
						}
						else if (!isRevenue && !RatingDataRegistry.Instance.UnspecifiedCostShouldForceZeroToBePulledThrough.Value)
						{
							amount = info.Amount;
						}
					}
				}
				else
				{
					amount = info.Amount;
				}

				var currencyFromInfo = RefCurrency.LoadFromCurrencyCode(info.Factory, info.Currency);

				return new Money(amount, currencyFromInfo, info.ChargeCode.AC_Code);
			}

			public decimal AddGST(IRateLineItem applyToItem, decimal beforeTaxAmount, RatingCriteria criteria)
			{
				var chargeCode = applyToItem.TM_Text == Calculator.Items.Value.DisbursementApplyToTypes.CustomsDisbursement
									 ? applyToItem.ParentRateLine.Factory.Load<AccChargeCode>(RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value)
									 : ChargeCode;

				AccTaxRate gstRate;
				var taxDate = ZDate.Today;

				if (criteria.Job != null)
				{
					ZGuid gstRatePK;

					var jobCharge = charge as JobCharge;
					if (jobCharge != null)
					{
						if (applyToItem.CostOrSell() == CostSell.Cost)
						{
							gstRatePK = jobCharge.JR_AT_CostGSTRate;
							if (!gstRatePK.IsEmpty && !jobCharge.JR_CostTaxDate.IsEmpty)
							{
								taxDate = jobCharge.JR_CostTaxDate;
							}
						}
						else
						{
							gstRatePK = jobCharge.JR_AT_SellGSTRate;
							if (!gstRatePK.IsEmpty && !jobCharge.JR_SellTaxDate.IsEmpty)
							{
								taxDate = jobCharge.JR_SellTaxDate;
							}
						}

						if (gstRatePK.IsEmpty)
						{
							gstRatePK = criteria.GetGSTID(jobCharge, applyToItem.CostOrSell());
						}
					}
					else
					{
						gstRatePK = criteria.GetGSTID(chargeCode, applyToItem.CostOrSell());
					}

					gstRate = criteria.Job.Factory.Load<AccTaxRate>(gstRatePK);
				}
				else
				{
					gstRate = chargeCode != null ? chargeCode.GSTRate : null;
				}

				if (gstRate != null)
				{
					beforeTaxAmount = beforeTaxAmount * (1m + (gstRate.GetRate(taxDate) / 100m));
				}

				return beforeTaxAmount;
			}

			public void RecordPercentageApplied(ZGuid pk)
			{
				var set = charge != null
					? parameters.RatingContext.PercentageLinesApplied.GetOrAdd(charge.PK)
					: info.PercentageLinesApplied;

				set.Add(pk);
			}
		}

		static IEnumerable<DependentCharge> GetDependentCharges(IRateLineItem applyToItem, AutoRatingCalculatorParameters parameters)
		{
			var charges = new List<DependentCharge>();

			var newCharges = GetNewApplicableCharges(applyToItem, parameters).ToList();
			var existingCharges = GetExistingApplicableCharges(applyToItem, parameters).ToList();

			// If there are existing charges on the job we need to check their rating behavior to make sure
			// which charges will end-up on the job (existing, new or both) and thus can be used by the dependent calculator.
			// Otherwise, we can end-up in a situation where Minimum or Percentage calculator applies its calculation to an existing 
			// charge which then gets overwritten by a newly calculated charge
			foreach (var existingCharge in existingCharges)
			{
				var matchingNewCharges = newCharges
					.Where(c => c.ChargeCode.PK == existingCharge.ChargeCode.PK)
					.ToArray();

				if (matchingNewCharges.Any() && existingCharge.CanReautorate(applyToItem))
				{
					// New charges will replace the existing one, so, we keep new charges
					continue;
				}

				// New charges can't replace the existing one, so, we take the existing one...
				charges.Add(existingCharge);

				// ... and new ones if the existing one doesn't stop them from autorating, otherwise we skip new charges
				if (existingCharge.RatingBehavior == JobChargeLookups.StopFromAutorating)
				{
					foreach (var newCharge in matchingNewCharges)
					{
						newCharges.Remove(newCharge);
					}
				}
			}

			charges.AddRange(newCharges);
			return charges;
		}

		static IEnumerable<DependentCharge> GetNewApplicableCharges(IRateLineItem applyToItem, AutoRatingCalculatorParameters parameters)
		{
			var charges = new List<DependentCharge>();

			// parameters.Result stores calculated charges for the current rating adapter being calculated while
			// results from the RatingContext store charges calculated for the current autorating session (we can autorate
			// multiple rating adapters per session) including customs charges as well. So, we need to check both collections
			// to find existing charges.
			var results = parameters.Results.Concat(parameters.RatingContext.InterimAutoRatingResults).Distinct();
			charges.AddRange(results.Select(info => new DependentCharge(info)));

			charges = charges.Where(c => IsApplicable(applyToItem, c, parameters)).ToList();
			return charges;
		}

		static IEnumerable<DependentCharge> GetExistingApplicableCharges(IRateLineItem applyToItem, AutoRatingCalculatorParameters parameters)
		{
			var charges = parameters.Criteria.GetExistingCharges()
				.Select(charge => new DependentCharge(charge, applyToItem.CostOrSell(), parameters))
				.Where(c => IsApplicable(applyToItem, c, parameters))
				.ToArray();

			return charges;
		}

		static bool IsApplicable(IRateLineItem applyToItem, DependentCharge charge, AutoRatingCalculatorParameters parameters)
		{
			if (applyToItem.CostOrSell() == CostSell.Cost && _Rating.Sell)
			{
				return false;
			}

			if (charge.ChargeCode == null)
			{
				return false;
			}

			var entry = applyToItem?.ParentRateLine?.ParentRateEntry;
			if (entry == null)
			{
				return false;
			}

			var result = !charge.IsPercentageApplied(applyToItem.ParentRateLine);
			result = result && MatchingByJob(charge, parameters);
			result = result && charge.ChargeCode.PK != applyToItem.ParentRateLine.ChargeCode.PK || applyToItem.ParentRateLine.Calculator is MinimumCalculator;
			result = result && !parameters.CalculationOrderResolver.AreInConflict(applyToItem, charge.ChargeCode);
			result = result && parameters.CalculationOrderResolver.IsItemApplicableToChargeCode(applyToItem, charge.ChargeCode);
			result = result && charge.ChargeCode.AC_IsActive;
			result = result && (entry.TI_RH_NKCommodityCode.IsEmpty || charge.CommodityCode.IsEmpty || entry.TI_RH_NKCommodityCode == charge.CommodityCode);
			result = result && MatchingByContainer(entry, charge);

			return result;
		}

		static bool MatchingByJob(DependentCharge charge, AutoRatingCalculatorParameters parameters)
		{
			// If we know the job a charge autorated for we want to match it so that
			// charges for one job don't match charges from others of the same job type
			if (charge.AutoRatedFor != null && parameters.Criteria.AutoRatedFor.Any(a => a.GetType() == charge.AutoRatedFor.GetType()))
			{
				return parameters.Criteria.AutoRatedFor.Contains(charge.AutoRatedFor);
			}

			// If a charge doesn't have details about the job it was autorated for,
			// we try to match by job reference.
			return charge.OperationalJobRef == parameters.Criteria.OperationalJobRef;
		}

		static bool MatchingByContainer(IRateEntry entry, DependentCharge charge)
		{
			var result = entry.Container == null || charge.ContainerCode.IsEmpty;
			result = result || entry.Container.RC_Code == charge.ContainerCode;
			result = result || entry.TI_MatchContainerRateClass && entry.Container.RC_HandlingRateClass == charge.Container.RC_HandlingRateClass;

			return result;
		}

		static List<Quantity> GetApplicableMoneyForItem(IRateLineItem applyToItem, bool includeGST, AutoRatingCalculatorParameters parameters)
		{
			var results = new List<Quantity>();
			var dependentCharges = GetDependentCharges(applyToItem, parameters);
			var rateLine = applyToItem.ParentRateLine;
			var exchangeRateType = applyToItem.CostOrSell() == CostSell.Cost
				? ExchangeRateType.Buy
				: ExchangeRateType.Sell;

			foreach (var charge in dependentCharges)
			{
				var mergedMoneyResult = charge.GetAmount(applyToItem, parameters.Criteria);
				var existingAmount = mergedMoneyResult.Amount;
				if (!existingAmount.IsEmpty)
				{
					charge.RecordPercentageApplied(rateLine.PK);

					if (!parameters.Criteria.Convert(ref existingAmount, mergedMoneyResult.Currency, rateLine, exchangeRateType)) //It is not correct to use rounding here. Will address it soon. -SY 2011
					{
						if (charge.IsJobCharge)
						{
							return new List<Quantity> { default };
						}

						continue;
					}

					if (includeGST)
					{
						existingAmount = charge.AddGST(applyToItem, existingAmount, parameters.Criteria);
					}

					if (mergedMoneyResult.Amount != existingAmount)
					{
						var source = mergedMoneyResult.Source;
						mergedMoneyResult = new Money(existingAmount, rateLine.Currency, source);
					}

					results.Add(mergedMoneyResult);
				}
			}

			return results;
		}

		#region Get Description

		static ZString GetDescriptionForDependentItems(this IDependentCalculator calculator, IRateLineItem greaterChargeItem)
		{
			var countryPK = calculator.Master.ParentRateEntry.Country().PK;
			if (greaterChargeItem != null)
			{
				return GetDescriptionInternal(greaterChargeItem, countryPK);
			}

			var result = new List<string>();
			foreach (var item in calculator.Master.ChildRateLineItems.Where(item => item.RateOperatorIsApplyTo()))
			{
				result.Add(GetDescriptionInternal(item, countryPK));
			}

			return string.Join(" + ", result);
		}

		static ZString GetDescriptionInternal(IRateLineItem applyToItem, ZGuid countryPK)
		{
			var result = applyToItem.TM_Text == CalculatorConstants.Text.ChargeCode && applyToItem.TM_AC != ZGuid.Empty
				? applyToItem.ParentRateLine.Factory.Load<AccChargeCode>(applyToItem.TM_AC)?.AC_Code
				: ValueApplyToListCodeDescriptionPairList.GetDescriptionFromCode(applyToItem.TM_Text, countryPK);

			return !result.HasValue || result.Value.IsEmpty ? (ZString)"?" : result.Value;
		}

		public static ZString GetChargeableDescription(this IDependentCalculator calculator, IRateLineItem greaterChargeItem, Quantity monetaryValue)
		{
			var dependentItemsDescription = calculator.GetDescriptionForDependentItems(greaterChargeItem);

			if (string.IsNullOrWhiteSpace(dependentItemsDescription))
			{
				return monetaryValue.Source;
			}

			if (!string.IsNullOrWhiteSpace(monetaryValue.Source))
			{
				if (monetaryValue.Source.Contains(dependentItemsDescription, StringComparison.OrdinalIgnoreCase))
				{
					return monetaryValue.Source;
				}

				if (dependentItemsDescription.Contains(monetaryValue.Source, StringComparison.OrdinalIgnoreCase))
				{
					return dependentItemsDescription;
				}

				return dependentItemsDescription + " " + monetaryValue.Source;
			}

			return dependentItemsDescription;
		}

		#region SuppressResourceStringsCheckRegion

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "All the arguments are formatted strings.")]
		static ZString GetSourceDescription(List<Quantity> moneyList, BusinessObjectFactory factory, bool fullSource)
		{
			switch (moneyList.Count)
			{
				case 0:
					return ZString.Empty;

				case 1:
					var money = moneyList[0];
					return fullSource ? $"{money.Unit} {GetRoundedMoneyAmount(money, factory)} ({money.Source})" : money.Source.ToString();

				default:
					return GetSourceDescriptionOrderedByAmount(moneyList, factory, fullSource);
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "All the arguments are formatted strings.")]
		static ZString GetSourceDescriptionOrderedByAmount(IEnumerable<Quantity> moneyList, BusinessObjectFactory factory, bool fullSource)
		{
			var orderedMoneyList = moneyList.OrderBy(x => x.Amount).ThenBy(x => x.Reference).ToArray();

			var currencies = orderedMoneyList.Select(m => GetCurrency(m, factory)).ToArray();

			// The issue has been solved in WI00259517, but we kept this as a safety guard.
			// WI00263068 created to delete this issue reporter if nothing reported in 6 months.
			// Please defer the work item WI00263068 if you faced a null CurrencyConverter issue!
			if (currencies.Any(c => c == null))
			{
				var msg = new ZStringBuilder();
				msg.AppendLine("One of ammounts has invalid currency:");   // Just an issue report

				foreach (var money in orderedMoneyList)
				{
					msg.AppendLine(Invariant($"{money.Amount} | {money.Unit} | {money.Source} | {money.Reference}"));                                                    // Just an issue report
				}

				ErrorReporter.ReportOnce("WI00229625_InvalidCurrency", msg.ToString());
				return Res.GetString("b2c14f1e-3300-11e9-8b0b-1c1b0d09faa1", "Cannot summarize when money has invalid currencies.");
			}

			if (!currencies.AllSame(c => c.PK))
			{
				return Res.GetString("9d7073b9-3d9d-49f0-ad78-f68a4064ec3c", "Cannot summarize when money has different currencies.");
			}

			var currency = currencies[0];
			var totalAmount = GetRoundedMoneyAmount(new Quantity(orderedMoneyList.Sum(m => m.Amount), currency.Code), factory);
			var sources = string.Join(" + ", orderedMoneyList.Select(money => $"{money.Source} {GetRoundedMoneyAmount(money, factory)}"));

			return fullSource ? $"{currency.Code} {totalAmount} ({sources})" : sources;
		}

		static ZString GetRoundedMoneyAmount(Quantity money, BusinessObjectFactory factory)
		{
			var currency = GetCurrency(money, factory);
			if (currency != null)
			{
				return money.Round(currency.Decimals).Amount.ToString(currency.Decimals);
			}

			return ZString.Empty;
		}

		static RefCurrency GetCurrency(Quantity quantity, BusinessObjectFactory factory)
		{
			return RefCurrency.LoadFromCurrencyCode(factory, quantity.Unit);
		}

		#endregion

		#endregion

		#region Calculation Order

		public static ZInt GetCalculationOrder(this IDependentCalculator calculator)
		{
			var item = calculator.Master.ChildRateLineItems.FirstOrDefault(x => x.RateOperatorIsSequenceItem());
			return (item != null ? item.TM_RelevantValue : ZDecimal.Zero).ToZInt();
		}

		public static void SetCalculationOrder(this IDependentCalculator calculator, ZInt value)
		{
			var item = (RateLineItem)calculator.Master.ChildRateLineItems.FirstOrDefault(x => x.RateOperatorIsSequenceItem()) ?? calculator.AddApplyToItem(Calculator.Items.Value.CalculationOrder);
			item.TM_Value = (ZDecimal)value;
		}

		#endregion

		#region Validation

		public static void ValidateTM_AC(this IDependentCalculator calculator, RateLineItem lineItem)
		{
			var rateLineChargeCode = calculator.Master.ChargeCode;
			var shouldCheckPercentageOf = rateLineChargeCode != null
				&& lineItem.RateOperatorIsApplyTo()
				&& lineItem.TM_Text == CalculatorConstants.Text.ChargeCode;

			if (shouldCheckPercentageOf)
			{
				var chargeCode = lineItem.ChargeCode;
				if (chargeCode == null)
				{
					lineItem.TM_ACInfo.AddError(ErrorMessages.ChargeCodeRequiredForPercentageCalculator);
				}
				else
				{
					if (rateLineChargeCode.PK == chargeCode.PK)
					{
						lineItem.TM_ACInfo.AddError(ErrorMessages.SameChargeCodes);
					}
					else
					{
						var rateLine = calculator.Master as RateLine;
						var isGlobal = true;
						if (rateLine != null)
						{
							var ratingHeader = rateLine.Factory.Load<RatingHeader>(rateLine.Parent.TI_TH);
							isGlobal = ratingHeader.IsGlobal();
						}

						if (!chargeCode.IsGlobal && isGlobal)
						{
							lineItem.TM_ACInfo.AddError(ErrorMessages.GlobalChargeCodeIsRequired);
						}
						else if (chargeCode.IsGlobal && !isGlobal)
						{
							lineItem.TM_ACInfo.AddError(ErrorMessages.LocalChargeCodeIsRequired);
						}
					}
				}
			}
		}

		public static void ValidateTM_Text(this IDependentCalculator calculator, RateLineItem lineItem)
		{
			if (lineItem.RateOperatorIsApplyTo())
			{
				ListValidation.ErrorIfInvalidCode(lineItem.TM_TextInfo, lineItem.Lookups.PercentageApplyToList);
			}
		}

		#endregion

		public static RateLineItem AddApplyToItem(this IDependentCalculator calculator, string applyTo)
		{
			var item = (calculator.Master as RateLine).RateLineItems.AddNew();
			item.TM_Type = CalculatorConstants.Type.ApplyTo;
			item.TM_Text = applyTo;

			return item;
		}
	}
}
