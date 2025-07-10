using System;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.TW.Business
{
	public abstract class IncoTermAndCustomsChargeFactory : CommonIncoTermAndCustomsChargeFactory
	{
		public override bool IsThisChargeDiscount(ZString chargeCode)
		{
			return chargeCode == CustomsChargeTypeList.Codes.DeductionCharge || chargeCode == CustomsChargeTypeList.Codes.Discount;
		}

		protected override void SetupIncotermChargeConfigurations()
		{
			SetupCostInsuranceAndFreightConfiguration();
			SetupCostAndFreightConfiguration();
			SetupFreeOnBoardConfiguration();
			SetupCostAndInsuranceConfiguration();
			SetupFreeAlongsideShipConfiguration();
			SetupExWorksConfiguration();
			SetupFreeCarrierConfiguration();
			SetupCarriagePaidToConfiguration();
			SetupDeliveredAtTerminalConfiguration();
			SetupDeliveredAtPlaceConfiguration();
			SetupDeliveredDutyPaidConfiguration();
			SetupCarriageAndInsurancePaidToConfiguration();
			SetupDeliveredAtPlaceUnloadedConfiguration();
		}

		protected abstract void AddAddition(string incoterm);

		protected abstract void AddDeduction(string incoterm);

		protected void AddAdditionAndDeduction(string incoterm)
		{
			AddAddition(incoterm);
			AddDeduction(incoterm);
		}

		protected virtual void SetupCostAndInsuranceConfiguration()
		{
			var incoTerm = Core.Constants.IncoTerms.CostAndInsurance;
			AddChargeConfiguration(incoTerm, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, GetForeignInlandFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetLandingCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetPackingCost(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddAdditionAndDeduction(incoTerm);
		}

		protected virtual void SetupDeliveredAtPlaceUnloadedConfiguration()
		{
			var incoTerm = Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded;

			AddChargeConfiguration(incoTerm, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, GetForeignInlandFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetLandingCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetPackingCost(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddAdditionAndDeduction(incoTerm);
		}

		protected override void SetupErrorConfiguration()
		{
		}

		protected override void SetupCostInsuranceAndFreightConfiguration()
		{
			var incoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			AddChargeConfiguration(incoTerm, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, GetForeignInlandFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetLandingCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetPackingCost(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddAdditionAndDeduction(incoTerm);
		}

		protected override void SetupFreeOnBoardConfiguration()
		{
			var incoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			AddChargeConfiguration(incoTerm, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetForeignInlandFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetLandingCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetPackingCost(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddAdditionAndDeduction(incoTerm);
		}

		protected override void SetupCostAndFreightConfiguration()
		{
			var incoTerm = Core.Constants.IncoTerms.CostAndFreight;
			AddChargeConfiguration(incoTerm, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetForeignInlandFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetLandingCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetPackingCost(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddAdditionAndDeduction(incoTerm);
		}

		protected override void SetupExWorksConfiguration()
		{
			var incoTerm = Core.Constants.IncoTerms.ExWorks;
			AddChargeConfiguration(incoTerm, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetForeignInlandFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });			
			AddChargeConfiguration(incoTerm, GetLandingCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetPackingCost(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddAdditionAndDeduction(incoTerm);
		}

		protected override void SetupFreeAlongsideShipConfiguration()
		{
			var incoTerm = Core.Constants.IncoTerms.FreeAlongsideShip;
			AddChargeConfiguration(incoTerm, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetForeignInlandFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetLandingCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetPackingCost(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddAdditionAndDeduction(incoTerm);
		}

		protected override void SetupFreeCarrierConfiguration()
		{
			var incoTerm = Core.Constants.IncoTerms.FreeCarrier;
			AddChargeConfiguration(incoTerm, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetForeignInlandFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetLandingCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetPackingCost(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddAdditionAndDeduction(incoTerm);
		}

		protected override void SetupCarriagePaidToConfiguration()
		{
			var incoTerm = Core.Constants.IncoTerms.CarriagePaidTo;
			AddChargeConfiguration(incoTerm, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetForeignInlandFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetLandingCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetPackingCost(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddAdditionAndDeduction(incoTerm);
		}

		protected override void SetupDeliveredAtTerminalConfiguration()
		{
			var incoTerm = Core.Constants.IncoTerms.DeliveredAtTerminal;
			AddChargeConfiguration(incoTerm, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, GetForeignInlandFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetLandingCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetPackingCost(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddAdditionAndDeduction(incoTerm);
		}

		protected override void SetupDeliveredAtPlaceConfiguration()
		{
			var incoTerm = Core.Constants.IncoTerms.DeliveredAtPlace;
			AddChargeConfiguration(incoTerm, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, GetForeignInlandFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetLandingCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetPackingCost(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddAdditionAndDeduction(incoTerm);
		}

		protected override void SetupDeliveredDutyPaidConfiguration()
		{
			var incoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			AddChargeConfiguration(incoTerm, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, GetForeignInlandFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetLandingCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetPackingCost(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddAdditionAndDeduction(incoTerm);
		}

		protected override void SetupCarriageAndInsurancePaidToConfiguration()
		{
			var incoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
			AddChargeConfiguration(incoTerm, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, GetForeignInlandFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetLandingCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, GetPackingCost(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddAdditionAndDeduction(incoTerm);
		}

		public override CustomsChargeCode GetOverseasFreight() => throw new NotImplementedException("Need to override this method");

		public override CustomsChargeCode GetOverseasInsurance() => throw new NotImplementedException("Need to override this method");

		public abstract CustomsChargeCode GetForeignInlandFreight();

		public abstract CustomsChargeCode GetAdditionCharge();

		public abstract CustomsChargeCode GetDeductionCharge();

		public abstract CustomsChargeCode GetLandingCharges();

		public abstract CustomsChargeCode GetPackingCost();

		public override bool MakeFlagsReadOnlyWhenDeemed => true;

		protected override ICustomsChargeCode[] GetCharges()
		{
			return new ICustomsChargeCode[]
				{
					GetPackingCost(),
					GetOverseasFreight(),
					GetOverseasInsurance(),
					GetForeignInlandFreight(),
					GetLandingCharges(),
					GetAdditionCharge(),
					GetDeductionCharge()
				};
		}
	}
}

