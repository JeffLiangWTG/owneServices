using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.Business
{
	public class HVLVItemLineValidation : AutoHVLVItemLineValidation
	{
		public HVLVItemLineValidation(AutoHVLVItemLine parent) : base(parent)
		{
		}

		public new HVLVItemLine Parent => (HVLVItemLine)base.Parent;

		protected override void CheckHVS_IntrinsicValue()
		{
			base.CheckHVS_IntrinsicValue();
			CompareValidation.CheckNumberNotNegative(Parent.HVS_IntrinsicValueInfo);
		}

		protected override void CheckHVS_CustomsValue()
		{
			base.CheckHVS_CustomsValue();
			CompareValidation.CheckNumberNotNegative(Parent.HVS_CustomsValueInfo);
		}

		protected override void CheckHVS_GrossWeight()
		{
			base.CheckHVS_GrossWeight();
			CompareValidation.CheckNumberNotNegative(Parent.HVS_GrossWeightInfo);
		}

		protected override void CheckHVS_NetWeight()
		{
			base.CheckHVS_NetWeight();
			CompareValidation.CheckNumberNotNegative(Parent.HVS_NetWeightInfo);
		}

		protected override void CheckHVS_Quantity()
		{
			base.CheckHVS_Quantity();
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.HVS_QuantityInfo, 1);
		}

		protected override void CheckHVS_WeightUnit()
		{
			base.CheckHVS_WeightUnit();
			ListValidation.ErrorIfInvalidCode(Parent.HVS_WeightUnitInfo);
			if (!Parent.HVS_GrossWeight.IsEmpty || !Parent.HVS_NetWeight.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.HVS_WeightUnitInfo);
			}
		}

		protected override void CheckHVS_RN_NKOriginCountryCode()
		{
			base.CheckHVS_RN_NKOriginCountryCode();
			if (!Parent.HVS_OriginTariff.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.HVS_RN_NKOriginCountryCodeInfo);
			}

			if (!Parent.HVS_RN_NKOriginCountryCode.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.HVS_RN_NKOriginCountryCodeInfo);
			}
		}

		protected override void CheckHVS_DestinationTariff()
		{
			base.CheckHVS_DestinationTariff();

			var shipment = Parent.ParentItem?.Shipment;

			if (shipment != null &&
				shipment.JobDirection == MasterFiles.Business.Directions.Import &&
				Parent.ShipmentDestinationCountryCode == CountryCodes.UnitedStates)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.HVS_DestinationTariffInfo);

				if (!Parent.HVS_DestinationTariff.IsEmpty)
				{
					var tariff = new USCTariff.Loader(Parent.Factory).LoadBestMatch(Parent.HVS_DestinationTariff, ZDate.Today);
					if (tariff == null)
					{
						Parent.HVS_DestinationTariffInfo.AddMessageError(Res.GetString("8F9D4361-50B5-414C-A8F6-0CE3A31227E6", "Tariff unable to be found."));
					}
				}
			}
		}

		protected override void CheckHVS_OriginTariff()
		{
			base.CheckHVS_OriginTariff();
			if (!Parent.HVS_RN_NKOriginCountryCode.IsEmpty)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.HVS_OriginTariffInfo);
			}
		}

		protected override void CheckHVS_GoodsDescription()
		{
			base.CheckHVS_GoodsDescription();
			DiacriticsValidation.WarnIfContainsAnyDiacritics(Parent.HVS_GoodsDescriptionInfo);
		}

		protected override void CheckHVS_OriginGoodsDescription()
		{
			base.CheckHVS_OriginGoodsDescription();
			DiacriticsValidation.WarnIfContainsAnyDiacritics(Parent.HVS_OriginGoodsDescriptionInfo);
		}

		protected override void CheckHVS_ItemURL()
		{
			base.CheckHVS_ItemURL();
			if (!string.IsNullOrEmpty(Parent.HVS_ItemURL) && !UrlValidation.IsValidUrl(Parent.HVS_ItemURL))
			{
				Parent.HVS_ItemURLInfo.AddWarning(Res.GetString("a6b2272a-3c21-46b0-95b2-064cddadb994", "Invalid Item URL."));
			}

			DiacriticsValidation.WarnIfContainsAnyDiacritics(Parent.HVS_ItemURLInfo);
		}
	}
}
