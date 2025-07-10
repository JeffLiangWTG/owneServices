using Enterprise.Customs.Common;

namespace Enterprise.Customs.NO.Business
{
	public class IncoTermAndCustomsChargeFactory : CommonIncoTermAndCustomsChargeFactory
	{
		protected override ICustomsChargeCode[] GetCharges()
		{
			return new ICustomsChargeCode[]
			{
				GetOverseasFreight(),
				GetOverseasInsurance(),
				GetOtherCharges(),
				GetDeductionCharges(),
				ValueOfGoodsExported(),
				CustomsChargeCodeProvider.ExWorks,
				CustomsChargeCodeProvider.Commission,
				CustomsChargeCodeProvider.ForeignInlandFreight,
				CustomsChargeCodeProvider.LandingCharges,
				CustomsChargeCodeProvider.AdditionCharge,
				CustomsChargeCodeProvider.Discount,
				CustomsChargeCodeProvider.PackingCost
			};
		}

		public override CustomsChargeCode GetOverseasFreight()
		{
			var chargeCode = base.GetOverseasFreight();
			chargeCode.IsDutiable = true;
			return chargeCode;
		}

		public override CustomsChargeCode GetOverseasInsurance()
		{
			var chargeCode = base.GetOverseasInsurance();
			chargeCode.IsDutiable = true;
			return chargeCode;
		}

		public CustomsChargeCode GetOtherCharges()
		{
			var chargeCode = CustomsChargeCodeProvider.OtherCharges;
			chargeCode.IsPercentageApplicable = true;
			return chargeCode;
		}

		public CustomsChargeCode GetDeductionCharges()
		{
			var chargeCode = CustomsChargeCodeProvider.DeductionCharge;
			chargeCode.IsPercentageApplicable = true;
			return chargeCode;
		}

		public CustomsChargeCode GetAdditionCharges()
		{
			var chargeCode = CustomsChargeCodeProvider.AdditionCharge;
			chargeCode.IsPercentageApplicable = true;
			return chargeCode;
		}

		protected override void SetupCostAndFreightConfiguration()
		{
			base.SetupCostAndFreightConfiguration();
			SetupIncotermChargeConfiguration(NOIncotermCodeList.Codes.CFR, CustomsChargeTypeList.Codes.OverseasFreight, true, true, false, false);
			SetupIncotermChargeConfiguration(NOIncotermCodeList.Codes.CFR, CustomsChargeTypeList.Codes.OverseasInsurance, true, false, false, false);
		}

		protected override void SetupCostInsuranceAndFreightConfiguration()
		{
			base.SetupCostInsuranceAndFreightConfiguration();
			SetupIncotermChargeConfiguration(NOIncotermCodeList.Codes.CIF, CustomsChargeTypeList.Codes.OverseasFreight, true, true, false, false);
			SetupIncotermChargeConfiguration(NOIncotermCodeList.Codes.CIF, CustomsChargeTypeList.Codes.OverseasInsurance, true, true, false, false);
		}

		protected override void SetupCarriageAndInsurancePaidToConfiguration()
		{
			base.SetupCarriageAndInsurancePaidToConfiguration();
			SetupIncotermChargeConfiguration(NOIncotermCodeList.Codes.CIP, CustomsChargeTypeList.Codes.OverseasFreight, true, true, false, false);
			SetupIncotermChargeConfiguration(NOIncotermCodeList.Codes.CIP, CustomsChargeTypeList.Codes.OverseasInsurance, true, true, false, false);
		}
		protected override void SetupCarriagePaidToConfiguration()
		{
			base.SetupCarriagePaidToConfiguration();
			SetupIncotermChargeConfiguration(NOIncotermCodeList.Codes.CPT, CustomsChargeTypeList.Codes.OverseasFreight, true, true, false, false);
			SetupIncotermChargeConfiguration(NOIncotermCodeList.Codes.CPT, CustomsChargeTypeList.Codes.OverseasInsurance, true, false, false, false);
		}

		protected override void SetupDeliveredAtPlaceConfiguration()
		{
			base.SetupDeliveredAtPlaceConfiguration();
			SetupIncotermChargeConfiguration(NOIncotermCodeList.Codes.DAP, CustomsChargeTypeList.Codes.OverseasFreight, true, true, false, false);
			SetupIncotermChargeConfiguration(NOIncotermCodeList.Codes.DAP, CustomsChargeTypeList.Codes.LandingCharges, false, true, false, false);
			SetupIncotermChargeConfiguration(NOIncotermCodeList.Codes.DAP, CustomsChargeTypeList.Codes.OverseasInsurance, false, true, false, false);
		}

		protected override void SetupDeliveredDutyPaidConfiguration()
		{
			base.SetupDeliveredDutyPaidConfiguration();
			SetupIncotermChargeConfiguration(NOIncotermCodeList.Codes.DDP, CustomsChargeTypeList.Codes.OverseasFreight, true, true, false, false);
			SetupIncotermChargeConfiguration(NOIncotermCodeList.Codes.DDP, CustomsChargeTypeList.Codes.LandingCharges, true, true, false, false);
			SetupIncotermChargeConfiguration(NOIncotermCodeList.Codes.DDP, CustomsChargeTypeList.Codes.OverseasInsurance, false, true, false, false);
		}

		protected override void SetupFreeOnBoardConfiguration()
		{
			base.SetupFreeOnBoardConfiguration();
			SetupIncotermChargeConfiguration(NOIncotermCodeList.Codes.FOB, CustomsChargeTypeList.Codes.OverseasFreight, true, false, true, true);
			SetupIncotermChargeConfiguration(NOIncotermCodeList.Codes.FOB, CustomsChargeTypeList.Codes.OverseasInsurance, true, false, true, true);
			SetupIncotermChargeConfiguration(NOIncotermCodeList.Codes.FOB, CustomsChargeTypeList.Codes.OtherCharges, true, false, false, true);
		}

		protected override void SetupExWorksConfiguration()
		{
			base.SetupExWorksConfiguration();
			SetupIncotermChargeConfiguration(NOIncotermCodeList.Codes.EXW, CustomsChargeTypeList.Codes.PackingCost, false, true, false, false);
			SetupIncotermChargeConfiguration(NOIncotermCodeList.Codes.EXW, CustomsChargeTypeList.Codes.ForeignInlandFreight, true, false, false, false);
			SetupIncotermChargeConfiguration(NOIncotermCodeList.Codes.EXW, CustomsChargeTypeList.Codes.OtherCharges, false, false, false, true);
		}

		protected override void SetupFreeAlongsideShipConfiguration()
		{
			base.SetupFreeAlongsideShipConfiguration();
			SetupIncotermChargeConfiguration(NOIncotermCodeList.Codes.FAS, CustomsChargeTypeList.Codes.OverseasInsurance, true, true, false, true);
		}

		protected override void SetupFreeCarrierConfiguration()
		{
			base.SetupFreeCarrierConfiguration();
			SetupIncotermChargeConfiguration(NOIncotermCodeList.Codes.FCA, CustomsChargeTypeList.Codes.OverseasInsurance, true, true, false, true);
		}

		void SetupIncotermChargeConfiguration(string incoterm, string chargeCode, bool isIncludedInInvoiceAmountFixed, bool isIncludedInInvoice, bool isMandatory, bool isRecommended)
		{
			var chargeConfiguration = new ChargeConfiguration()
			{
				IsIncludedInInvoice = isIncludedInInvoice,
				IsIncludedInInvoiceAmountFixed = isIncludedInInvoiceAmountFixed,
				IsMandatory = isMandatory,
				IsRecommended = isRecommended,
			};
			AddChargeConfiguration(incoterm, chargeCode, chargeConfiguration, ignoreExisting: true);
		}

		public CustomsChargeCode ValueOfGoodsExported() => new CustomsChargeCode(NOInvoiceChargeTypesImport.Codes.ValueOfGoodsExported, NOInvoiceChargeTypesImport.Descriptions.ValueOfGoodsExported)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = false
		};
	}
}
