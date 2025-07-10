using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.ZA.Business
{
	public partial class IncoTermAndCustomsChargeFactory : CommonIncoTermAndCustomsChargeFactory
	{
		protected override void SetupCarriageAndInsurancePaidToConfiguration()
		{
			base.SetupCarriageAndInsurancePaidToConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, IncoTermAndCustomsChargeFactory.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false }, true);
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, IncoTermAndCustomsChargeFactory.IntellectualValue, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupCarriagePaidToConfiguration()
		{
			base.SetupCarriagePaidToConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, IncoTermAndCustomsChargeFactory.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false }, true);
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, IncoTermAndCustomsChargeFactory.IntellectualValue, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupCostAndFreightConfiguration()
		{
			base.SetupCostAndFreightConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, IncoTermAndCustomsChargeFactory.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false }, true);
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, IncoTermAndCustomsChargeFactory.IntellectualValue, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupCostInsuranceAndFreightConfiguration()
		{
			base.SetupCostInsuranceAndFreightConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, IncoTermAndCustomsChargeFactory.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false }, true);
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, IncoTermAndCustomsChargeFactory.IntellectualValue, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupDeliveredAtPlaceConfiguration()
		{
			base.SetupDeliveredAtPlaceConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, IncoTermAndCustomsChargeFactory.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false }, true);
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, IncoTermAndCustomsChargeFactory.IntellectualValue, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupDeliveredAtTerminalConfiguration()
		{
			base.SetupDeliveredAtTerminalConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, IncoTermAndCustomsChargeFactory.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false }, true);
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, IncoTermAndCustomsChargeFactory.IntellectualValue, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupDeliveredDutyPaidConfiguration()
		{
			base.SetupDeliveredDutyPaidConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, IncoTermAndCustomsChargeFactory.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false }, true);
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, IncoTermAndCustomsChargeFactory.IntellectualValue, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupErrorConfiguration()
		{
			base.SetupErrorConfiguration();
			AddChargeConfiguration(ErrorIncoTermCode, IncoTermAndCustomsChargeFactory.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false }, true);
			AddChargeConfiguration(ErrorIncoTermCode, IncoTermAndCustomsChargeFactory.IntellectualValue, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupExWorksConfiguration()
		{
			base.SetupExWorksConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, IncoTermAndCustomsChargeFactory.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false }, true);
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, IncoTermAndCustomsChargeFactory.IntellectualValue, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupFreeAlongsideShipConfiguration()
		{
			base.SetupFreeAlongsideShipConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, IncoTermAndCustomsChargeFactory.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false }, true);
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, IncoTermAndCustomsChargeFactory.IntellectualValue, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupFreeCarrierConfiguration()
		{
			base.SetupFreeCarrierConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, IncoTermAndCustomsChargeFactory.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false }, true);
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, IncoTermAndCustomsChargeFactory.IntellectualValue, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupFreeOnBoardConfiguration()
		{
			base.SetupFreeOnBoardConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, IncoTermAndCustomsChargeFactory.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false }, true);
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, IncoTermAndCustomsChargeFactory.IntellectualValue, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override ICustomsChargeCode[] GetCharges()
		{
			var result = new List<ICustomsChargeCode>(base.GetCharges().Where(x => x.Code != CustomsChargeTypeList.Codes.Discount));

			foreach (CustomsChargeCode chargeCode in result)
			{
				chargeCode.ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice;
			}

			result.Add(Discount);
			return result.ToArray();
		}
	}

	public class ImportIncoTermAndCustomsChargeFactory : IncoTermAndCustomsChargeFactory
	{
		protected override ICustomsChargeCode[] GetCharges()
		{
			var result = new List<ICustomsChargeCode>(base.GetCharges());
			result.Add(IntellectualValue);
			return result.ToArray();
		}
	}
}
