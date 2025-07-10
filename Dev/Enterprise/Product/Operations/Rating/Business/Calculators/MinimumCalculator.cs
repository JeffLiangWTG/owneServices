using System.Collections.Generic;
using System.Linq;
using CargoWise.DataTransfer.Ratings;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	[CalculatorProperty(MinimumCalculator.Items.MIN, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal1", RelatedTo = "MinimumValue")]
	[CalculatorProperty(MinimumCalculator.Items.MinimumType, RateLineItem.Schema.TM_Text, IsMandatory = true, MapTo = "String1", InitialValue = CalculatorConstants.Text.MIN_Job)]
	[CalculatorProperty("IsJobMinimum", IsCalculatedField = true, MapTo = "Bool1", RelatedTo = "IsJobMinimum")]
	[CalculatorProperty("IsChargeCodeMinimum", IsCalculatedField = true, MapTo = "Bool2", RelatedTo = "IsChargeCodeMinimum")]
	public class MinimumCalculator : Calculator, IDependentCalculator
	{
		public MinimumCalculator(IRateLine master)
			: base(master)
		{
		}

		public const string Code = RatingCalculatorCodes.Minimum;

		#region Calculator Item Codes

		public new abstract class Items
		{
			public const string MIN = "MIN";
			public const string MinimumType = "MNT";
		}

		#endregion

		#region Weight/Volume

		public override bool IsJobLevelAvailable
		{
			get { return false; }
		}

		public override bool IsMeasureTypeMatchApplicable
		{
			get { return false; }
		}

		#endregion

		#region Properties

		public ZDecimal MinimumValue
		{
			get { return (ZDecimal)this[MinimumCalculator.Items.MIN]; }
			set { this[MinimumCalculator.Items.MIN] = value; }
		}

		RateLineItem MinimumItem
		{
			get { return FindRateLineItem(MinimumCalculator.Items.MIN); }
		}

		public ZBool IsJobMinimum
		{
			get { return (ZString)this[MinimumCalculator.Items.MinimumType] == (ZString)CalculatorConstants.Text.MIN_Job; }
			set
			{
				if (value)
				{
					this[MinimumCalculator.Items.MinimumType] = (ZString)CalculatorConstants.Text.MIN_Job;
				}
			}
		}

		public ZPropertyInfo IsJobMinimumInfo
		{
			get { return Line.GetZPropertyInfo("Calculator+Bool1"); }
		}

		public ZBool IsChargeCodeMinimum
		{
			get { return (ZString)this[MinimumCalculator.Items.MinimumType] == (ZString)CalculatorConstants.Text.MIN_ChargeCode; }
			set
			{
				if (value)
				{
					this[MinimumCalculator.Items.MinimumType] = (ZString)CalculatorConstants.Text.MIN_ChargeCode;
				}
			}
		}

		public ZPropertyInfo IsChargeCodeMinimumInfo
		{
			get { return Line.GetZPropertyInfo("Calculator+Bool2"); }
		}

		#endregion

		#region Quotation Lines

		protected override QuotationLineList GetQuotationLinesInternal(GetQuotationLinesParam flags)
		{
			var result = new QuotationLineList();

			if (IsChargeCodeMinimum)
			{
				result.Add(QuotationLine.NewWithValue(Line, RateDescriptionFlags(flags) | QuotationLineType.MergeWithRateLineDescription,
					MinimumCalculator.Items.MIN, Res.GetString("2926814c-a28b-4aed-b0bd-32c9d6923fb6", "- Charge Code Minimum"), (NoResString)ZString.Empty));
			}
			else if (IsJobMinimum)
			{
				result.Add(QuotationLine.NewWithValue(Line, RateDescriptionFlags(flags) | QuotationLineType.MergeWithRateLineDescription,
					MinimumCalculator.Items.MIN, Res.GetString("2c4e399b-f5c8-4b59-a138-462a1d4bd063", "- Job Minimum"), (NoResString)ZString.Empty));
			}

			return result;
		}

		public override DocLineAmount GetDocLineAmount()
			=> GetDocLineAmount(Line, IsChargeCodeMinimum, IsJobMinimum);

		public static DocLineAmount GetDocLineAmount(IRateLine rateLine, bool isChargeCodeMinimum, bool isJobMinimum)
		{
			var result = new DocLineAmount();

			var min = QuotationLine.GetValue(Calculator.Items.Operator.MIN, rateLine.ChildRateLineItems);

			if (!min.IsEmpty)
			{
				var description = "";

				if (isChargeCodeMinimum)
				{
					description = Res.GetString("5810495B-242D-45AA-83D2-0341D9E7761D", "Charge Code Minimum");
				}
				else if (isJobMinimum)
				{
					description = Res.GetString("F235F8C6-0BAA-4F11-A50A-5DEC17222C11", "Job Minimum");
				}
				else
				{
					description = Res.GetString("50165A9C-A4CE-4C23-914E-C53181E0C085", "Minimum");
				}

				var docLineAmount = new DocLineAmount();
				result.SetMin(rateLine.TL_RX_NKCurrency, min, description);
			}

			return result;
		}

		#endregion

		#region Calculation

		protected override void CalculateInternal(CalculatorOutput calcOutput)
		{
			var parameters = calcOutput.Parameters;
			var minimum = MinimumValue;

			IRateLineItem greaterChargeItem;
			var money = this.GetValueCalculatorAppliesTo(parameters, out greaterChargeItem, false, false, false);

			if (minimum > money.Amount)
			{
				var description = ZString.Empty;

				if (IsJobMinimum)
				{
					parameters.Results.RemoveAll();
					description = Res.GetString("4efd7266-7be8-4c8b-bd5d-fb975735eee7", "Job Minimum");
				}
				else if (IsChargeCodeMinimum)
				{
					var resultsToRemove = parameters.Results.Where(ari => ari.ChargeCode.PK == Line.ChargeCode.PK).ToList();
					resultsToRemove.ForEach(res => parameters.Results.Remove(res));
					description = Res.GetString("71a6adfd-b6bb-4e63-bad5-516c941417c5", "Charge Code Minimum");
				}

				var rateInfo = RateInfo.CreateMIN(minimum, Line.TL_RX_NKCurrency, description.ToString());

				var calculationResult = parameters.Criteria.CreatePaymentBasis(rateInfo, money, null, description);
				calcOutput.Add(calculationResult);
			}
		}

		#endregion

		#region Company Tariff / Cost Based Clone

		internal override void CloneLineItemsUpdatingRateValues(CompanyTariffOrCostBasedCalculator ctbCalc, RateLine clone, RateLineItem.RateTypeToUpdate rateTypeToUpdate)
		{
			var clonedCalc = clone.Calculator as MinimumCalculator;
			if (clonedCalc != null)
			{
				var args = new BusinessObjectCloneArgs(new string[2] { RateLineItemsSchema.Constants.PK, RateLineItemsSchema.Constants.TM_TL });
				clonedCalc.MinimumItem.CopyPersistentValuesFrom(MinimumItem, args);

				clonedCalc.MinimumItem.UpdateRateValue(
					x => { return ZArchitecture.Core.Utilities.Round(x * (ctbCalc.Percent + 100) / 100 + ctbCalc.PerUnit + ctbCalc.BaseRateWithApplicableIncrease, 2); },
					rateTypeToUpdate);
			}
		}

		#endregion

		#region Costs Comparer Charges Summary

		protected override IList<ChargesSummaryItem> GetCostsComparerChargesSummaryCore(List<RateLine> lines)
		{
			return new ChargesSummaryItem[]
			{
				new ChargesSummaryItem(MinimumCalculator.Items.MIN, MinimumValue)
			};
		}

		#endregion

		#region FreightRatePerChargeable

		public override List<PaymentBasis> GetPricePerSingleChargeable()
		{
			var result = new List<PaymentBasis>();

			var minRateInfo = RateInfo.CreateMIN(MinimumValue, Line.TL_RX_NKCurrency, null, null);
			result.Add(new PaymentBasis(default, minRateInfo, AdapterType.RateEntry, string.Empty));

			return result;
		}

		#endregion

		IRateLine IDependentCalculator.Master => Line;

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders => false;
	}
}

