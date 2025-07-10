using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.DataTransfer.Ratings;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	[CalculatorProperty(CalculatorConstants.Type.PER, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal1", ShowWarningIfEmpty = false, RelatedTo = "Percent")]
	[CalculatorProperty(Calculator.Items.Operator.MIN, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal2", ShowWarningIfEmpty = false, RelatedTo = "Minimum")]
	[CalculatorProperty(Calculator.Items.Operator.BAS, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal3", ShowWarningIfEmpty = false, RelatedTo = "BaseRate")]
	[CalculatorProperty(Calculator.Items.Operator.MAX, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal4", ShowWarningIfEmpty = false, RelatedTo = "Maximum")]
	[CalculatorProperty(CalculatorConstants.Type.Rate, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal5", ShowWarningIfEmpty = false, RelatedTo = "Rate")]
	[CalculatorProperty(CalculatorConstants.Type.ValueOrPartThereOf, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal6", ShowWarningIfEmpty = false, RelatedTo = "ValueOrPartThereOf")]
	[CalculatorProperty(CalculatorConstants.Text.IncludeGST, RateLineItem.Schema.TM_Text, IsMandatory = true, MapTo = "Bool1", RelatedTo = "IncludeGST")]
	[CalculatorProperty(CalculatorConstants.Text.GreaterCharge, RateLineItem.Schema.TM_Text, IsMandatory = true, MapTo = "Bool2", RelatedTo = "GreaterCharge")]
	[CalculatorProperty(CalculatorConstants.Text.PER_PartThereof, RateLineItem.Schema.TM_Text, IsMandatory = true, MapTo = "Bool3", RelatedTo = "IsPartThereof")]
	public class PercentageCalculator : BasePercentageCalculator
	{
		public PercentageCalculator(IRateLine master)
			: base(master)
		{
		}

		public const string Code = RatingCalculatorCodes.Percentage;

		#region Properties

		public ZString ValueApplyTo
		{
			get
			{
				var result = ZString.Empty;

				foreach (var item in Line.ChildRateLineItems)
				{
					if (item.RateOperatorIsApplyTo())
					{
						if (!result.IsEmpty)
						{
							return Calculator.Items.Value.Multiple;
						}

						result = item.TM_Text;
					}
				}

				return result;
			}
		}

		public ZBool HasMultipleApplyTo
		{
			get { return ValueApplyTo == Calculator.Items.Value.Multiple; }
		}

		public ZDecimal Percent
		{
			get { return (ZDecimal)this[CalculatorConstants.Type.PER]; }
			set { this[CalculatorConstants.Type.PER] = value; }
		}

		public ZDecimal Minimum
		{
			get { return (ZDecimal)this[Calculator.Items.Operator.MIN]; }
			set { this[Calculator.Items.Operator.MIN] = value; }
		}

		public ZDecimal BaseRate
		{
			get { return (ZDecimal)this[Calculator.Items.Operator.BAS]; }
			set { this[Calculator.Items.Operator.BAS] = value; }
		}

		public ZDecimal Maximum
		{
			get { return (ZDecimal)this[Calculator.Items.Operator.MAX]; }
			set { this[Calculator.Items.Operator.MAX] = value; }
		}

		public ZBool IncludeGST
		{
			get { return (ZBool)this[CalculatorConstants.Text.IncludeGST]; }
			set { this[CalculatorConstants.Text.IncludeGST] = value; }
		}

		public ZBool GreaterCharge
		{
			get { return (ZBool)this[CalculatorConstants.Text.GreaterCharge]; }
			set { this[CalculatorConstants.Text.GreaterCharge] = value; }
		}

		public ZBool IsPartThereof
		{
			get { return (ZBool)this[CalculatorConstants.Text.PER_PartThereof]; }
			set { this[CalculatorConstants.Text.PER_PartThereof] = value; }
		}

		public ZDecimal Rate
		{
			get { return (ZDecimal)this[CalculatorConstants.Type.Rate]; }
			set { this[CalculatorConstants.Type.Rate] = value; }
		}

		public ZDecimal ValueOrPartThereOf
		{
			get { return (ZDecimal)this[CalculatorConstants.Type.ValueOrPartThereOf]; }
			set { this[CalculatorConstants.Type.ValueOrPartThereOf] = value; }
		}

		#endregion

		#region Quotation Lines

		protected override QuotationLineList GetQuotationLinesInternal(GetQuotationLinesParam flags)
		{
			var result = new QuotationLineList();

			var isEmpty = IsPartThereof
				? Rate.IsEmpty || ValueOrPartThereOf.IsEmpty
				: Percent.IsEmpty;

			if (isEmpty || ValueApplyTo.IsEmpty)
			{
				if (BaseRate >= Minimum)
				{
					result.Add(QuotationLine.BaseRate(Line, RateDescriptionFlags(flags)));
				}
				else
				{
					result.Add(QuotationLine.Minimum(Line, RateDescriptionFlags(flags)));
				}
			}
			else
			{
				var additionalDescription = ZString.Empty;
				var additionalLineDescription = ZString.Empty;

				if (HasMultipleApplyTo && GreaterCharge)
				{
					additionalDescription = GreaterChargeText;
					additionalLineDescription = OrText;
				}

				var isAdditionalLine = false;

				foreach (var item in Line.ChildRateLineItems)
				{
					if (item.RateOperatorIsApplyTo())
					{
						QuotationLineType lineType;
						ZString description;

						if (isAdditionalLine)
						{
							lineType = QuotationLineType.Mandatory;
							description = additionalLineDescription;
						}
						else
						{
							lineType = RateDescriptionFlags(flags) | QuotationLineType.MergeWithRateLineDescription;
							description = additionalDescription;
							isAdditionalLine = true;
						}

						ZString percentageUnit = GetPercentageUnit(item);

						if (IsPartThereof)
						{
							percentageUnit = percentageUnit.Substring(4); //This line probably is not going to work with other languages like Chinese. Need to fix.
							result.Add(QuotationLine.NewWithValue(Line, lineType, Rate, description, (NoResString)(PerText + " " + CurrencySymbol(Line.TL_RX_NKCurrency) + ValueOrPartThereOf.ToString("f2", Culture.CurrentCompanyCountryCulture) + percentageUnit)));
						}
						else
						{
							lineType |= QuotationLineType.NoCurrency;
							if ((Percent - ZArchitecture.Core.Utilities.Round(Percent, 2)) != 0m)
							{
								lineType |= QuotationLineType.f4;
							}

							result.Add(QuotationLine.NewWithValue(Line, lineType, Percent, description, (NoResString)percentageUnit));
						}
					}
				}

				result.Add(QuotationLine.BaseRate(Line, 0));
				result.Add(QuotationLine.Minimum(Line, 0));
				result.Add(QuotationLine.Maximum(Line));
			}

			return result;
		}

		static string GreaterChargeText => Res.GetString("79aa7b64-f3aa-47a7-9817-e90e2753eea7", "(Greater Charge)");

		static string OrText => Res.GetString("746767ba-985c-41a8-bb74-843a5138f590", "or");

		static string PerText => Res.GetString("ad4d4b64-0b88-42ee-b2b7-7a1737526ff1", "per");

		public override DocLineAmount GetDocLineAmount()
		{
			var result = new DocLineAmount();

			var isEmpty = IsPartThereof
				? Rate.IsEmpty || ValueOrPartThereOf.IsEmpty
				: Percent.IsEmpty;

			if (isEmpty || ValueApplyTo.IsEmpty)
			{
				var basic = QuotationLine.GetValue(Calculator.Items.Operator.BAS, Line.ChildRateLineItems);
				if (BaseRate >= Minimum)
				{
					result.SetFlat(Line.TL_RX_NKCurrency, basic);
				}
				else
				{
					result.SetMin(Line.TL_RX_NKCurrency, basic);
				}
			}
			else
			{
				var additionalDescription = ZString.Empty;
				var additionalLineDescription = ZString.Empty;

				if (HasMultipleApplyTo && GreaterCharge)
				{
					additionalDescription = GreaterChargeText;
					additionalLineDescription = OrText;
				}

				var isAdditionalLine = false;

				foreach (var item in Line.ChildRateLineItems)
				{
					if (item.RateOperatorIsApplyTo())
					{
						ZString description;

						if (isAdditionalLine)
						{
							description = additionalLineDescription;
						}
						else
						{
							description = additionalDescription;
							isAdditionalLine = true;
						}

						ZString percentageUnit = GetPercentageUnit(item);

						if (IsPartThereof)
						{
							percentageUnit = percentageUnit.Substring(4); //This line probably is not going to work with other languages like Chinese. Need to fix.
							var partThereOfDescription = (NoResString)(PerText + " " + CurrencySymbol(Line.TL_RX_NKCurrency) + ValueOrPartThereOf.ToString("f2", Culture.CurrentCompanyCountryCulture) + percentageUnit);
							result.SetPercentage(Line.TL_RX_NKCurrency, partThereOfDescription, "", Rate);
						}
						else
						{
							result.SetPercentage(Line.TL_RX_NKCurrency, description, percentageUnit, Percent);
						}
					}
				}

				var flatDocLineAmount = FlatCalculator.GetDocLineAmount(Line);
				var minDocLineAmount = MinimumCalculator.GetDocLineAmount(Line, isChargeCodeMinimum: false, isJobMinimum: false);
				var maxDocLineAmount = GetMaxDocLineAmount(Line);

				result = result + flatDocLineAmount + minDocLineAmount + maxDocLineAmount;
			}

			return result;
		}

		public static DocLineAmount GetMaxDocLineAmount(IRateLine rateLine)
		{
			var result = new DocLineAmount();

			var max = QuotationLine.GetValue(Calculator.Items.Operator.MAX, rateLine.ChildRateLineItems);
			if (!max.IsEmpty)
			{
				var docLineAmount = new DocLineAmount();
				result.SetMax(rateLine.TL_RX_NKCurrency, max);
			}

			return result;
		}

		#endregion

		#region Calculation

		protected override void CalculateInternal(CalculatorOutput calcOutput)
		{
			var parameters = calcOutput.Parameters;
			IRateLineItem greaterChargeItem;
			var chargeableMoney = this.GetValueCalculatorAppliesTo(parameters, out greaterChargeItem, IncludeGST, GreaterCharge, false);

			var chargeableMoneyDescription = this.GetChargeableDescription(greaterChargeItem, chargeableMoney);

			if (IsPartThereof)
			{
				var valueMultiplier = ValueOrPartThereOf.IsEmpty ? 0 : Math.Ceiling(chargeableMoney.Amount / ValueOrPartThereOf);

				var unit = ZString.Format("{0} {1}", Line.TL_RX_NKCurrency, ValueOrPartThereOf.ToString("f2", Culture.CurrentCompanyCountryCulture));
				var chargeable = new Quantity(valueMultiplier, unit); //i.e unit here is 100 AUD. So the rate is for example 5 AUD per every 100 AUD
				var rateInfoDescription = Res.GetString("f994ac3a-d3b9-4c66-a5c6-6044c664f724", "per {0} or part thereof for {1} {2} ({3})", unit, chargeableMoney.Unit, chargeableMoney.Amount.ToString("f2", Culture.CurrentCompanyCountryCulture), chargeableMoneyDescription);
				var rateInfo = RateInfo.CreateUNT(Rate, unit, Line.TL_RX_NKCurrency, null, rateInfoDescription, unitMultiplier: UnitMultiplier, isPartThereof: true);

				var paymentBasis = parameters.Criteria.CreatePaymentBasis(rateInfo, chargeable);
				calcOutput.Add(paymentBasis);
			}
			else
			{
				var rateInfo = RateInfo.CreatePER(Percent, Line.TL_RX_NKCurrency);
				var paymentBasis = parameters.Criteria.CreatePaymentBasis(rateInfo, chargeableMoney, null, chargeableMoneyDescription);

				calcOutput.Add(paymentBasis);
			}
			calcOutput.Minimum = Minimum;
			calcOutput.Maximum = Maximum;
			calcOutput.BaseRate = BaseRate;
		}

		#endregion

		#region Company Tariff / Cost Based Clone

		internal override void CloneLineItemsUpdatingRateValues(CompanyTariffOrCostBasedCalculator ctbCalc, RateLine clone, RateLineItem.RateTypeToUpdate rateTypeToUpdate)
		{
			clone.RateLineItems.RemoveAndDeleteAll();
			foreach (var item in RateLineBizO.RateLineItems.Cast<RateLineItem>())
			{
				var newItem = clone.RateLineItems.CloneItem(item);
				if (newItem.RateOperatorIsPER() ||
					newItem.RateOperatorIsBAS() ||
					newItem.RateOperatorIsMIN() ||
					newItem.RateOperatorIsMAX() ||
					newItem.TM_Type == CalculatorConstants.Type.Rate)
				{
					newItem.UpdateRateValue(x => x * (ctbCalc.Percent + 100) / 100, rateTypeToUpdate);

					if (newItem.RateOperatorIsBAS() || ((newItem.RateOperatorIsMIN() || newItem.RateOperatorIsMAX()) && !newItem.TM_RelevantValue.IsEmpty))
					{
						newItem.UpdateRateValue(x => x + ctbCalc.BaseRateWithApplicableIncrease, rateTypeToUpdate);

						if (newItem.RateOperatorIsMIN())
						{
							newItem.UpdateRateValue(x => x + ctbCalc.Minimum, rateTypeToUpdate);
						}
					}

					newItem.UpdateRateValue(x => Utilities.Round(x, 2), rateTypeToUpdate);
				}
			}
		}

		#endregion

		#region Costs Comparer Charges Summary

		protected override IList<ChargesSummaryItem> GetCostsComparerChargesSummaryCore(List<RateLine> lines)
		{
			if (ValueApplyTo == CalculatorConstants.Text.ChargeCode && !IsPartThereof)
			{
				var applyToItem = Line.ChildRateLineItems.FirstOrDefault(x => x.RateOperatorIsApplyTo());
				if (applyToItem != null)
				{
					foreach (var line in lines)
					{
						if (line.ChargeCode.PK == applyToItem.TM_AC)
						{
							var items = line.Calculator.GetCostsComparerChargesSummary(lines);

							if (items != null)
							{
								var results = new List<ChargesSummaryItem>();
								foreach (var item in items)
								{
									results.Add(new ChargesSummaryItem(item.Type, item.Break, item.Value * Percent / 100m, item.FlatAmount * Percent / 100m));
								}

								if (!Minimum.IsEmpty)
								{
									var minItem = new ChargesSummaryItem(Calculator.Items.Operator.MIN, Minimum);
									if (results.Contains(minItem))
									{
										var index = results.IndexOf(minItem);
										if (results[index].Value < minItem.Value)
										{
											results[index] = minItem;
										}
									}
									else
									{
										results.Add(minItem);
									}
								}

								if (!BaseRate.IsEmpty)
								{
									var baseItem = new ChargesSummaryItem(Calculator.Items.Operator.BAS, BaseRate);
									if (results.Contains(baseItem))
									{
										var index = results.IndexOf(baseItem);
										results[index] = results[index] + baseItem;
									}
									else
									{
										results.Add(baseItem);
									}
								}

								return results;
							}
						}
					}
				}
			}

			return null;
		}

		#endregion

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders => false;

		public override bool IsMeasureTypeMatchApplicable => false;
	}
}

