using System.Collections.Generic;
using ChargeConfiguration = Enterprise.Customs.Common.ChargeConfiguration;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public partial class TSWIncoTermAndCustomsChargeFactory : IncoTermAndCustomsChargeFactory
	{
		protected override void SetupCarriageAndInsurancePaidToConfiguration()
		{
			base.SetupCarriageAndInsurancePaidToConfiguration();
			AddChargeConfiguration(IncoTermList.Codes.CarriageAndInsurancePaidTo, TSWIncoTermAndCustomsChargeFactory.RoyaltiesCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupCarriagePaidToConfiguration()
		{
			base.SetupCarriagePaidToConfiguration();
			AddChargeConfiguration(IncoTermList.Codes.CarriagePaidTo, TSWIncoTermAndCustomsChargeFactory.RoyaltiesCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupCostAndFreightConfiguration()
		{
			base.SetupCostAndFreightConfiguration();
			AddChargeConfiguration(IncoTermList.Codes.CostAndFreight, TSWIncoTermAndCustomsChargeFactory.RoyaltiesCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupCostInsuranceAndFreightConfiguration()
		{
			base.SetupCostInsuranceAndFreightConfiguration();
			AddChargeConfiguration(IncoTermList.Codes.CostInsuranceAndFreight, TSWIncoTermAndCustomsChargeFactory.RoyaltiesCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupDeliveredAtPlaceConfiguration()
		{
			base.SetupDeliveredAtPlaceConfiguration();
			AddChargeConfiguration(IncoTermList.Codes.DeliveredAtPlace, TSWIncoTermAndCustomsChargeFactory.RoyaltiesCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupDeliveredAtTerminalConfiguration()
		{
			base.SetupDeliveredAtTerminalConfiguration();
			AddChargeConfiguration(IncoTermList.Codes.DeliveredAtTerminal, TSWIncoTermAndCustomsChargeFactory.RoyaltiesCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupDeliveredAtPlaceUnloadedConfiguration()
		{
			base.SetupDeliveredAtPlaceUnloadedConfiguration();
			AddChargeConfiguration(IncoTermList.Codes.DeliveredAtPlaceUnloaded, TSWIncoTermAndCustomsChargeFactory.RoyaltiesCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupDeliveredDutyPaidConfiguration()
		{
			base.SetupDeliveredDutyPaidConfiguration();
			AddChargeConfiguration(IncoTermList.Codes.DeliveredDutyPaid, TSWIncoTermAndCustomsChargeFactory.RoyaltiesCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupErrorConfiguration()
		{
			base.SetupErrorConfiguration();
			AddChargeConfiguration(ErrorIncoTermCode, TSWIncoTermAndCustomsChargeFactory.RoyaltiesCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupExWorksConfiguration()
		{
			base.SetupExWorksConfiguration();
			AddChargeConfiguration(IncoTermList.Codes.ExWorks, TSWIncoTermAndCustomsChargeFactory.RoyaltiesCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupFreeAlongsideShipConfiguration()
		{
			base.SetupFreeAlongsideShipConfiguration();
			AddChargeConfiguration(IncoTermList.Codes.FreeAlongsideShip, TSWIncoTermAndCustomsChargeFactory.RoyaltiesCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupFreeCarrierConfiguration()
		{
			base.SetupFreeCarrierConfiguration();
			AddChargeConfiguration(IncoTermList.Codes.FreeCarrier, TSWIncoTermAndCustomsChargeFactory.RoyaltiesCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupFreeOnBoardConfiguration()
		{
			base.SetupFreeOnBoardConfiguration();
			AddChargeConfiguration(IncoTermList.Codes.FreeOnBoard, TSWIncoTermAndCustomsChargeFactory.RoyaltiesCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override Common.ICustomsChargeCode[] GetCharges()
		{
			var result = new List<Common.ICustomsChargeCode>(base.GetCharges());
			result.Add(RoyaltiesCharge);
			return result.ToArray();
		}
	}
}
