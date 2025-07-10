using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.DataTransfer.Ratings;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	[CalculatorProperty(FirstPlusAdditionalCalculator.Items.FST, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal1", RelatedTo = "First")]
	[CalculatorProperty(FirstPlusAdditionalCalculator.Items.ADD, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal2", RelatedTo = "Additional")]
	public class FirstPlusAdditionalCalculator : Calculator
	{
		public FirstPlusAdditionalCalculator(IRateLine master)
			: base(master)
		{
		}

		public const string Code = RatingCalculatorCodes.FirstPlusAdditional;

		#region Calculator Item Codes

		public new abstract class Items
		{
			public const string FST = "FST";
			public const string ADD = "ADD";
		}

		#endregion

		#region Properties

		public ZDecimal First
		{
			get { return (ZDecimal)this[FirstPlusAdditionalCalculator.Items.FST]; }
			set { this[FirstPlusAdditionalCalculator.Items.FST] = value; }
		}

		public ZDecimal Additional
		{
			get { return (ZDecimal)this[FirstPlusAdditionalCalculator.Items.ADD]; }
			set { this[FirstPlusAdditionalCalculator.Items.ADD] = value; }
		}

		RateLineItem FirstItem
		{
			get { return FindRateLineItem(FirstPlusAdditionalCalculator.Items.FST); }
		}

		RateLineItem AdditionalItem
		{
			get { return FindRateLineItem(FirstPlusAdditionalCalculator.Items.ADD); }
		}

		#endregion

		#region Quotation Lines

		protected override QuotationLineList GetQuotationLinesInternal(GetQuotationLinesParam flags)
		{
			var result = new QuotationLineList();

			result.Add(QuotationLine.Header(Line, RateDescriptionFlags(flags)));
			result.Add(QuotationLine.NewWithValue(Line, QuotationLineType.Mandatory, FirstPlusAdditionalCalculator.Items.FST, Res.GetString("8ace6781-4a31-4ffb-a674-305579efd026", "First") + " " + UnitDescription(true), (NoResString)ZString.Empty));
			result.Add(QuotationLine.NewWithValue(Line, QuotationLineType.Mandatory, FirstPlusAdditionalCalculator.Items.ADD, Res.GetString("66524fdb-d42d-41fe-878f-acaeeffe5f58", "Thereafter"), ResString.GetMultilingualString("fc7bbf5e-097e-45cb-b20d-36f41e890d4b", "per {0}", UnitDescription(false))));

			return result;
		}

		public override DocLineAmount GetDocLineAmount()
		{
			var result = new DocLineAmount();

			var first = QuotationLine.GetValue(FirstPlusAdditionalCalculator.Items.FST, Line.ChildRateLineItems);
			var additional = QuotationLine.GetValue(FirstPlusAdditionalCalculator.Items.ADD, Line.ChildRateLineItems);
			result.SetFirstAddAdditional(Line.TL_RX_NKCurrency, Line.Calculator.UnitDescription(true), first, additional);

			return result;
		}

		#endregion

		#region Calculation

		protected override void CalculateInternal(CalculatorOutput calcOutput)
		{
			var parameters = calcOutput.Parameters;
			var allItems = ChargeableAmount(parameters);
			if (!allItems.Amount.IsEmpty)
			{
				var rateLineUnit = GetUnit(parameters);
				if (rateLineUnit == QuantityUnit.CN)
				{
					CalculatePerUnit(calcOutput, allItems, rateLineUnit);
				}
				else
				{
					var criteria = parameters.Criteria;
					var quantityReference = allItems.Reference;
					var results = new List<PaymentBasis>();

					var firstCharge = First;
					if (!firstCharge.IsEmpty)
					{
						var paymentBasis = CreatePaymentBasis(criteria, firstCharge, 1, rateLineUnit, allItems.Unit, quantityReference);
						results.Add(paymentBasis);
					}

					ZDecimal additionalItems = (decimal)Math.Ceiling((double)(allItems.Amount - 1m));
					var additionalCharge = Additional;
					if (!additionalItems.IsEmpty && !additionalCharge.IsEmpty)
					{
						var paymentBasis = CreatePaymentBasis(criteria, additionalCharge, additionalItems, rateLineUnit, allItems.Unit, quantityReference);
						results.Add(paymentBasis);
					}

					calcOutput.Add(results);
				}
			}
		}

		void CalculatePerUnit(CalculatorOutput calcOutput, Quantity chargeableAmount, ZString rateLineUnit)
		{
			var parameters = calcOutput.Parameters;
			var criteria = parameters.Criteria;
			var calcLog = calcOutput.CalculationLog;
			var references = chargeableAmount.Reference.Split(',').Select(r => r.Trim()).Where(n => !n.IsEmpty).ToList();
			var perChargeableBasis = new List<PaymentBasis>();

			var firstCharge = First;
			var firstChargeable = references.Count > 0 ? references[0] : ZString.Empty;
			if (!firstCharge.IsEmpty)
			{
				var paymentBasis = CreatePaymentBasis(criteria, firstCharge, 1, rateLineUnit, chargeableAmount.Unit, firstChargeable);

				calcLog.AddPerContainerUnitCalculation(1, firstCharge);
				perChargeableBasis.Add(paymentBasis);
			}

			if (references.Count > 0 && !firstChargeable.IsEmpty)
			{
				references.RemoveAt(0);
			}

			ZDecimal additionalItems = (decimal)Math.Ceiling((double)(chargeableAmount.Amount - 1m));
			var additionalCharge = Additional;
			if (!additionalItems.IsEmpty && !additionalCharge.IsEmpty)
			{
				foreach (var number in references)
				{
					calcLog.AddPerContainerUnitCalculation(1, additionalCharge);

					var paymentBasis = CreatePaymentBasis(criteria, additionalCharge, 1, rateLineUnit, chargeableAmount.Unit, number);
					perChargeableBasis.Add(paymentBasis);
				}

				var referencesWithoutNumber = additionalItems.ToZInt() - references.Count;
				if (referencesWithoutNumber > 0)
				{
					calcLog.AddPerContainerUnitCalculation(referencesWithoutNumber, additionalCharge / UnitMultiplier);

					var paymentBasis = CreatePaymentBasis(criteria, additionalCharge, referencesWithoutNumber, rateLineUnit, chargeableAmount.Unit, string.Empty);
					perChargeableBasis.Add(paymentBasis);
				}
			}

			calcOutput.Add(perChargeableBasis);
		}

		PaymentBasis CreatePaymentBasis(RatingCriteria criteria, ZDecimal perUnitRate, ZDecimal quantityAmount, ZString rateLineUnit, ZString chargeableUnit, ZString quantityReference)
		{
			var rateInfo = RateInfo.CreateUNT(perUnitRate / UnitMultiplier, rateLineUnit, Line.TL_RX_NKCurrency, UnitDescription(false), unitMultiplier: UnitMultiplier);
			var quantity = new Quantity(quantityAmount, chargeableUnit, reference: quantityReference);
			var chargeableUnitDescription = UnitDescriptionInternal(Line.TL_WeightVolume, addPlural: true, addContainerCode: true);

			return criteria.CreatePaymentBasis(rateInfo, quantity, chargeableUnitDescription);
		}

		public override bool SupportsProductLineUnitFactor => true;

		#endregion

		#region Company Tariff / Cost Based Clone

		internal override void CloneLineItemsUpdatingRateValues(CompanyTariffOrCostBasedCalculator ctbCalc, RateLine clone, RateLineItem.RateTypeToUpdate rateTypeToUpdate)
		{
			var clonedCalc = clone.Calculator as FirstPlusAdditionalCalculator;
			if (clonedCalc != null)
			{
				var args = new BusinessObjectCloneArgs(new string[2] { RateLineItemsSchema.Constants.PK, RateLineItemsSchema.Constants.TM_TL });
				clonedCalc.FirstItem.CopyPersistentValuesFrom(FirstItem, args);
				clonedCalc.AdditionalItem.CopyPersistentValuesFrom(AdditionalItem, args);

				clonedCalc.FirstItem.UpdateRateValue(
					x => { return ZArchitecture.Core.Utilities.Round(x * (ctbCalc.Percent + 100) / 100 + ctbCalc.PerUnit + ctbCalc.BaseRateWithApplicableIncrease, 2); },
					rateTypeToUpdate);
				clonedCalc.AdditionalItem.UpdateRateValue(
					x => { return ZArchitecture.Core.Utilities.Round(x * (ctbCalc.Percent + 100) / 100 + ctbCalc.PerUnit, 2); },
					rateTypeToUpdate);
			}
		}

		#endregion

		#region Costs Comparer Charges Summary

		protected override IList<ChargesSummaryItem> GetCostsComparerChargesSummaryCore(List<RateLine> lines)
		{
			return new ChargesSummaryItem[]
			{
				new ChargesSummaryItem(Calculator.Items.Operator.BAS, First - Additional),
				new ChargesSummaryItem(Calculator.Items.Operator.UNT, Additional)
			};
		}

		#endregion

		#region FreightRatePerChargeable

		public override List<PaymentBasis> GetPricePerSingleChargeable()
		{
			var result = new List<PaymentBasis>();
			var unitDescription = UnitDescriptionInternal(Line.TL_WeightVolume, addContainerCode: true);
			var firstCharge = RateInfo.CreateUNT(First, Unit, Line.TL_RX_NKCurrency, unitDescription, unitMultiplier: UnitMultiplier);
			var chargeable = new Quantity(1m, Unit);
			result.Add(new PaymentBasis(chargeable, firstCharge, AdapterType.RateEntry, string.Empty));

			if (!Additional.IsEmpty)
			{
				var additionalCharge = RateInfo.CreateUNT(Additional, Unit, Line.TL_RX_NKCurrency, UnitDescription(false), unitMultiplier: UnitMultiplier);
				result.Add(new PaymentBasis(chargeable, additionalCharge, AdapterType.RateEntry, string.Empty));
			}

			return result;
		}

		#endregion

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders => false;
	}
}

