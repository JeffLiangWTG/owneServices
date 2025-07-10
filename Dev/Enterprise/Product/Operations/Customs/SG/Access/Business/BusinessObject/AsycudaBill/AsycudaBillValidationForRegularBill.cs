using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.SG.Access.Business
{
	public partial class AsycudaBillValidationForRegularBill : ASYCUDA.Business.AsycudaBillValidationForRegularBill
	{
		public AsycudaBillValidationForRegularBill(AsycudaBill parent)
			: base(parent)
		{
		}

		protected new AsycudaBill Parent => (AsycudaBill)base.Parent;
		protected AsycudaManifestHeader Header => Parent.Header;

		protected override void CheckABL_BillNumber()
		{
			base.CheckABL_BillNumber();
			var header = Header;
			if (header != null && !Parent.Packs.Any() && header.IsPackedItemLevelManifestType)
			{
				Parent.ABL_BillNumberInfo.AddMessageError("This manifest type requires each Bill to have Packs entered.");
			}
		}

		protected override void CheckABL_FreightValue()
		{
			base.CheckABL_FreightValue();
			MandatoryValidation.MessageErrorIfIsZero(Parent.ABL_FreightValueInfo);
			CheckTotalPackLinePriceValue();
		}

		void CheckTotalPackLinePriceValue()
		{
			var freightCurrency = Parent.ABL_RX_NKFreightValueCurrency;
			if (!freightCurrency.IsEmpty)
			{
				var hasDifferentCurrency = false;
				var totalLinePrice = 0m;
				foreach (var pack in Parent.Packs.Cast<AsycudaPack>())
				{
					if (freightCurrency == pack.LinePriceCurrency)
					{
						totalLinePrice += pack.LinePrice;
					}
					else
					{
						hasDifferentCurrency = true;
						break;
					}
				}
				if (hasDifferentCurrency)
				{
					Parent.ABL_FreightValueInfo.AddWarning(ValidationConstants.Bill.SGGoodsValueCurrencyNotSameWithLinePriceCurrency);
				}
				else if (totalLinePrice != Parent.ABL_FreightValue)
				{
					Parent.ABL_FreightValueInfo.AddMessageError(ValidationConstants.Bill.SGGoodsValueNotMatchTotalLinePrice);
				}
			}
		}

		protected override void CheckABL_RX_NKFreightValueCurrency()
		{
			ValidationHelper.CheckCurrency(Parent.ABL_RX_NKFreightValueCurrencyInfo, Parent.ABL_FreightValue, true);
		}

		protected override void CheckABL_RL_NKFinalDestination()
		{
			base.CheckABL_RL_NKFinalDestination();
			if (!Parent.ABL_RL_NKFinalDestination.IsEmpty)
			{
				var header = Header;
				if (header != null)
				{
					if (header.IsExport)
					{
						if (Parent.ABL_RL_NKFinalDestination.SubstringSafe(0, 2) == Core.Constants.CountryCodes.Singapore)
						{
							Parent.ABL_RL_NKFinalDestinationInfo.AddMessageError(ValidationConstants.PortCannotSingaporePortForExport("Destination"));
						}
					}
					else if (header.IsImport)
					{
						if (Parent.ABL_RL_NKFinalDestination.SubstringSafe(0, 2) != Core.Constants.CountryCodes.Singapore)
						{
							Parent.ABL_RL_NKFinalDestinationInfo.AddMessageError(ValidationConstants.PortMustBeSingaporePortForImport("Destination"));
						}
					}
				}
			}
		}

		protected override void CheckABL_RL_NKOrigin()
		{
			base.CheckABL_RL_NKOrigin();
			if (!Parent.ABL_RL_NKOrigin.IsEmpty)
			{
				var header = Header;
				if (header != null)
				{
					if (header.IsExport)
					{
						if (Parent.ABL_RL_NKOrigin.SubstringSafe(0, 2) != Core.Constants.CountryCodes.Singapore)
						{
							Parent.ABL_RL_NKOriginInfo.AddMessageError(ValidationConstants.PortMustBeSingaporePortForExport("Origin"));
						}
					}
					else if (header.IsImport)
					{
						if (Parent.ABL_RL_NKOrigin.SubstringSafe(0, 2) == Core.Constants.CountryCodes.Singapore)
						{
							Parent.ABL_RL_NKOriginInfo.AddMessageError(ValidationConstants.PortCannotBeSingaporePortForImport("Origin"));
						}
					}
				}
			}
		}

		protected override ZBool NeedsToCheckABL_GrossWeight => false;

		protected override ZBool NeedsToCheckABL_GrossWeightUQ => false;

		protected override ZBool NeedsToCheckABL_ManifestQty => false;

		protected override ZBool NeedsToCheckABL_ManifestUQ => false;

		protected override ZBool NeedsToCheckABL_Consignee => false;
	}
}
