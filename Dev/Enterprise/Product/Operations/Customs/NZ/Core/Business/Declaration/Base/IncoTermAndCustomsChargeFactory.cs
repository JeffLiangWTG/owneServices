using Enterprise.Customs.Common;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public partial class IncoTermAndCustomsChargeFactory : CommonIncoTermAndCustomsChargeFactory
	{
		protected override void SetupIncotermChargeConfigurations()
		{
			base.SetupIncotermChargeConfigurations();
			SetupDeliveredAtPlaceUnloadedConfiguration();
		}

		protected virtual void SetupDeliveredAtPlaceUnloadedConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, CustomsChargeCodeProvider.Commission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}
	}
}
