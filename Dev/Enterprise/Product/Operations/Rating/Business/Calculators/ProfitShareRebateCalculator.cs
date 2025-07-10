using System.Linq;
using CargoWise.DataTransfer.Ratings;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	[CalculatorProperty(CalculatorConstants.Type.PER, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal1", ShowWarningIfEmpty = false, RelatedTo = "Percent")]
	[CalculatorProperty(Calculator.Items.Operator.MIN, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal2", ShowWarningIfEmpty = false, RelatedTo = "Minimum")]
	[CalculatorProperty(Calculator.Items.Operator.BAS, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal3", ShowWarningIfEmpty = false, RelatedTo = "BaseRate")]
	[CalculatorProperty(Calculator.Items.Operator.MAX, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal4", ShowWarningIfEmpty = false, RelatedTo = "Maximum")]
	[CalculatorProperty(CalculatorConstants.Text.ZeroWhenLoss, RateLineItem.Schema.TM_Text, IsMandatory = true, MapTo = "Bool1", RelatedTo = "ZeroWhenLoss")]
	public sealed class ProfitShareRebateCalculator : BasePercentageCalculator
	{
		public ProfitShareRebateCalculator(IRateLine master)
			: base(master)
		{
		}

		public const string Code = RatingCalculatorCodes.ProfitShareRebate;

		#region Properties

		public ZDecimal Percent
		{
			get { return (ZDecimal)this[CalculatorConstants.Type.PER]; }
			set { this[CalculatorConstants.Type.PER] = value; }
		}

		public ZDecimal Minimum
		{
			get { return (ZDecimal)this[Items.Operator.MIN]; }
			set { this[Items.Operator.MIN] = value; }
		}

		public ZDecimal BaseRate
		{
			get { return (ZDecimal)this[Items.Operator.BAS]; }
			set { this[Items.Operator.BAS] = value; }
		}

		public ZDecimal Maximum
		{
			get { return (ZDecimal)this[Items.Operator.MAX]; }
			set { this[Items.Operator.MAX] = value; }
		}

		public ZBool ZeroWhenLoss
		{
			get { return (ZBool)this[CalculatorConstants.Text.ZeroWhenLoss]; }
			set { this[CalculatorConstants.Text.ZeroWhenLoss] = value; }
		}

		#endregion

		#region Calculation

		protected override void CalculateInternal(CalculatorOutput calcOutput)
		{
			var parameters = calcOutput.Parameters;
			var job = parameters.Criteria.InvoicingSupporter?.Job;
			if (job == null)
			{
				var names = string.Join(",", parameters.Criteria.HumanReadableNames());
				var failureMessage = Res.GetString("2e560842-5ab1-432b-8046-dafff3f466c6", "Profit Share Calculator cannot be run as no Job was found for the following: {0}.", names);
				calcOutput.FailureMessage = failureMessage;
				return;
			}

			var chargeableMoney = job.GetTotalProfitAndLossLocal(DoesCalculatorApplyToChargeCode);
			var localCurrency = GlbCompany.CurrentCompany.LocalCurrency;

			if (ZeroWhenLoss && chargeableMoney.Amount < 0m)
			{
				chargeableMoney = new Money(0m, localCurrency, Res.GetString("939a59b1-a9b1-4d6e-a05d-3ec2f5e3d5a8", "Zero When Loss"));
			}

			ZString chargeableMoneyDescription;

			if (Line.TL_RX_NKCurrency != localCurrency.RX_Code)
			{
				chargeableMoney = new Money(chargeableMoney.Amount, chargeableMoney.Currency, (chargeableMoney.Source + " " + chargeableMoney).Trim());
				chargeableMoneyDescription = this.GetChargeableDescription(null, chargeableMoney);

				var costOrSell = parameters.Criteria.CurrencyConverter.RateType == ExchangeRateType.Sell ? CostSell.Revenue : CostSell.Cost;
				chargeableMoney = parameters.Criteria.CurrencyConverter.ConvertExact(chargeableMoney, Line.Currency, ZGuid.Empty, costOrSell);
			}
			else
			{
				chargeableMoneyDescription = this.GetChargeableDescription(null, chargeableMoney);
			}

			var rateInfo = RateInfo.CreatePER(Percent, Line.TL_RX_NKCurrency);
			var paymentBasis = parameters.Criteria.CreatePaymentBasis(rateInfo, chargeableMoney, null, chargeableMoneyDescription);
			calcOutput.Add(paymentBasis);
			calcOutput.Minimum = Minimum;
			calcOutput.Maximum = Maximum;
			calcOutput.BaseRate = BaseRate;
		}

		bool DoesCalculatorApplyToChargeCode(JobCharge charge)
		{
			var doesApply = Line.ChildRateLineItems
				.Where(i => i != null && i.RateOperatorIsApplyTo())
				.Any(i => DoesItemApplyToChargeCode(i, charge.ChargeCode));

			return doesApply;
		}

		bool DoesItemApplyToChargeCode(IRateLineItem applyToItem, AccChargeCode chargeCode)
		{
			switch (applyToItem.TM_Text)
			{
				case CalculatorConstants.Text.AllCharges:
					return true;

				case CalculatorConstants.Text.FreightCharges:
					return chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Freight;

				case CalculatorConstants.Text.OriginCharges:
					return chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Origin;

				case CalculatorConstants.Text.DestinationCharges:
					return chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Destination;

				case CalculatorConstants.Text.ChargeCode:
					return chargeCode != null && applyToItem.TM_AC == chargeCode.PK;

				default:
					return false;
			}
		}

		#endregion

		#region Calculator Overrides

		internal override void CloneLineItemsUpdatingRateValues(CompanyTariffOrCostBasedCalculator source, RateLine clone, RateLineItem.RateTypeToUpdate rateTypeToUpdate)
		{
			clone.RateLineItems.RemoveAndDeleteAll(); //Not applicable as this calculator is always applied after AutoRating has been completed.
		}

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders => false;

		public override bool IsMeasureTypeMatchApplicable => false;

		#endregion
	}
}

