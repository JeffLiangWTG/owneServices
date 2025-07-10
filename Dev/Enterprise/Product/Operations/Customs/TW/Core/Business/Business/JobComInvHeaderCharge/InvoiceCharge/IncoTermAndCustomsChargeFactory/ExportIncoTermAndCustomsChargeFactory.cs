using Enterprise.Customs.Common;

namespace Enterprise.Customs.TW.Business
{
	public class ExportIncoTermAndCustomsChargeFactory : IncoTermAndCustomsChargeFactory
	{
		#region CustomsChargeCode
		public static CustomsChargeCode OverseasFreight
		{
			get
			{
				var chargeCode = CustomsChargeCodeProvider.OverseasFreight;
				chargeCode.IsDutiable = false;
				chargeCode.IsDutiableDeemedForThisCharge = true;
				chargeCode.IsVATible = true;
				chargeCode.IsVATibleDeemedForThisCharge = true;
				chargeCode.IsPercentageApplicable = false;
				chargeCode.IsIncludedInITOTDeemedForThisCharge = false;
				chargeCode.IsIncoTermNeutral = false;
				return chargeCode;
			}
		}

		public static CustomsChargeCode OverseasInsurance
		{
			get
			{
				var chargeCode = CustomsChargeCodeProvider.OverseasInsurance;
				chargeCode.IsDutiable = false;
				chargeCode.IsDutiableDeemedForThisCharge = true;
				chargeCode.IsVATible = true;
				chargeCode.IsVATibleDeemedForThisCharge = true;
				chargeCode.IsPercentageApplicable = true;
				chargeCode.IsIncludedInITOTDeemedForThisCharge = false;
				chargeCode.IsIncoTermNeutral = false;
				return chargeCode;
			}
		}

		public static CustomsChargeCode ForeignInlandFreight
		{
			get
			{
				var chargeCode = CustomsChargeCodeProvider.ForeignInlandFreight;
				chargeCode.IsDutiable = true;
				chargeCode.IsDutiableDeemedForThisCharge = true;
				chargeCode.IsVATible = true;
				chargeCode.IsVATibleDeemedForThisCharge = true;
				chargeCode.IsPercentageApplicable = false;
				chargeCode.IsIncludedInITOTDeemedForThisCharge = false;
				chargeCode.IsIncoTermNeutral = false;
				return chargeCode;
			}
		}

		public static CustomsChargeCode LandingCharges
		{
			get
			{
				var chargeCode = CustomsChargeCodeProvider.LandingCharges;
				chargeCode.IsDutiable = false;
				chargeCode.IsDutiableDeemedForThisCharge = true;
				chargeCode.IsVATible = true;
				chargeCode.IsVATibleDeemedForThisCharge = true;
				chargeCode.IsPercentageApplicable = false;
				chargeCode.IsIncludedInITOTDeemedForThisCharge = true;
				chargeCode.IsIncoTermNeutral = false;
				return chargeCode;
			}
		}

		public static CustomsChargeCode PackingCost
		{
			get
			{
				var chargeCode = CustomsChargeCodeProvider.PackingCost;
				chargeCode.IsDutiable = true;
				chargeCode.IsDutiableDeemedForThisCharge = true;
				chargeCode.IsVATible = true;
				chargeCode.IsVATibleDeemedForThisCharge = true;
				chargeCode.IsPercentageApplicable = false;
				chargeCode.IsIncludedInITOTDeemedForThisCharge = false;
				chargeCode.IsIncoTermNeutral = false;
				return chargeCode;
			}
		}

		public static CustomsChargeCode AdditionCharge
		{
			get
			{
				var chargeCode = CustomsChargeCodeProvider.AdditionCharge;
				chargeCode.IsDutiable = false;
				chargeCode.IsDutiableDeemedForThisCharge = true;
				chargeCode.IsVATible = true;
				chargeCode.IsVATibleDeemedForThisCharge = true;
				chargeCode.IsIncludedInITOTDeemedForThisCharge = true;
				chargeCode.IsIncludedInITOTIfDeemed = true;
				chargeCode.IsIncoTermNeutral = true;
				return chargeCode;
			}
		}

		public static CustomsChargeCode DeductionCharge
		{
			get
			{
				var chargeCode = CustomsChargeCodeProvider.DeductionCharge;
				chargeCode.IsDutiable = true;
				chargeCode.IsDutiableDeemedForThisCharge = true;
				chargeCode.IsVATible = false;
				chargeCode.IsVATibleDeemedForThisCharge = true;
				chargeCode.IsIncludedInITOTDeemedForThisCharge = true;
				chargeCode.IsIncludedInITOTIfDeemed = true;
				chargeCode.IsIncoTermNeutral = true;
				return chargeCode;
			}
		}
		#endregion

		public override CustomsChargeCode GetAdditionCharge() => AdditionCharge;

		public override CustomsChargeCode GetDeductionCharge() => DeductionCharge;

		public override CustomsChargeCode GetForeignInlandFreight() => ForeignInlandFreight;

		public override CustomsChargeCode GetLandingCharges() => LandingCharges;

		public override CustomsChargeCode GetOverseasFreight() => OverseasFreight;

		public override CustomsChargeCode GetOverseasInsurance() => OverseasInsurance;

		public override CustomsChargeCode GetPackingCost() => PackingCost;

		protected override void AddAddition(string incoterm)
		{
			AddChargeConfiguration(incoterm, GetAdditionCharge(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
		}

		protected override void AddDeduction(string incoterm)
		{
			AddChargeConfiguration(incoterm, GetDeductionCharge(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}
	}
}
