using Enterprise.Customs.Common;

namespace Enterprise.Customs.TW.Business
{
	public class ImportIncoTermAndCustomsChargeFactory : IncoTermAndCustomsChargeFactory
	{
		#region CustomsChargeCode
		protected override void SetupExWorksConfiguration()
		{
			var incoTerm = Core.Constants.IncoTerms.ExWorks;
			AddChargeConfiguration(incoTerm, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });

			AddChargeConfiguration(incoTerm, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });

			AddChargeConfiguration(incoTerm, GetForeignInlandFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });

			AddChargeConfiguration(incoTerm, GetAdditionCharge(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });

			AddDeduction(incoTerm);

			AddChargeConfiguration(incoTerm, GetLandingCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });

			AddChargeConfiguration(incoTerm, GetPackingCost(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupFreeOnBoardConfiguration()
		{
			var incoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			AddChargeConfiguration(incoTerm, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });

			AddChargeConfiguration(incoTerm, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });

			AddChargeConfiguration(incoTerm, GetForeignInlandFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });

			AddAdditionAndDeduction(incoTerm);

			AddChargeConfiguration(incoTerm, GetLandingCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });

			AddChargeConfiguration(incoTerm, GetPackingCost(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupFreeAlongsideShipConfiguration()
		{
			var incoTerm = Core.Constants.IncoTerms.FreeAlongsideShip;
			AddChargeConfiguration(incoTerm, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });

			AddChargeConfiguration(incoTerm, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });

			AddChargeConfiguration(incoTerm, GetForeignInlandFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });

			AddAdditionAndDeduction(incoTerm);

			AddChargeConfiguration(incoTerm, GetLandingCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });

			AddChargeConfiguration(incoTerm, GetPackingCost(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		public static CustomsChargeCode OverseasFreight
		{
			get
			{
				var chargeCode = CustomsChargeCodeProvider.OverseasFreight;
				chargeCode.IsDutiable = true;
				chargeCode.IsDutiableDeemedForThisCharge = true;
				chargeCode.IsStatisticalValueApplicable = false;
				chargeCode.IsStatisticalValueApplicableDeemed = true;
				chargeCode.IsPercentageApplicable = false;
				return chargeCode;
			}
		}

		public static CustomsChargeCode OverseasInsurance
		{
			get
			{
				var chargeCode = CustomsChargeCodeProvider.OverseasInsurance;
				chargeCode.IsDutiable = true;
				chargeCode.IsDutiableDeemedForThisCharge = true;
				chargeCode.IsStatisticalValueApplicable = false;
				chargeCode.IsStatisticalValueApplicableDeemed = true;
				chargeCode.IsPercentageApplicable = true;
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
				chargeCode.IsStatisticalValueApplicable = true;
				chargeCode.IsStatisticalValueApplicableDeemed = true;
				chargeCode.IsPercentageApplicable = false;
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
				chargeCode.IsStatisticalValueApplicable = false;
				chargeCode.IsStatisticalValueApplicableDeemed = true;
				chargeCode.IsPercentageApplicable = false;
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
				chargeCode.IsStatisticalValueApplicable = true;
				chargeCode.IsStatisticalValueApplicableDeemed = true;
				chargeCode.IsPercentageApplicable = false;
				return chargeCode;
			}
		}

		public static CustomsChargeCode AdditionCharge
		{
			get
			{
				var chargeCode = CustomsChargeCodeProvider.AdditionCharge;
				chargeCode.IsDutiable = true;
				chargeCode.IsDutiableDeemedForThisCharge = false;
				chargeCode.IsStatisticalValueApplicable = false;
				chargeCode.IsStatisticalValueApplicableDeemed = true;
				chargeCode.IsPercentageApplicable = false;
				chargeCode.IsIncludedInITOTDeemedForThisCharge = true;
				chargeCode.IsIncludedInITOTIfDeemed = false;
				return chargeCode;
			}
		}

		public static CustomsChargeCode DeductionCharge
		{
			get
			{
				var chargeCode = CustomsChargeCodeProvider.DeductionCharge;
				chargeCode.IsDutiable = false;
				chargeCode.IsDutiableDeemedForThisCharge = true;
				chargeCode.IsStatisticalValueApplicable = true;
				chargeCode.IsStatisticalValueApplicableDeemed = true;
				chargeCode.IsPercentageApplicable = false;
				chargeCode.IsIncludedInITOTDeemedForThisCharge = true;
				chargeCode.IsIncludedInITOTIfDeemed = true;
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
			AddChargeConfiguration(incoterm, GetAdditionCharge(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
		}

		protected override void AddDeduction(string incoterm)
		{
			AddChargeConfiguration(incoterm, GetDeductionCharge(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}
	}
}
