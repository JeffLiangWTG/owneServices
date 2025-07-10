using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class CommodityValidation : CusInBondCargoDescValidation
	{
		public CommodityValidation(Commodity parent)
			: base(parent)
		{
		}

		new Commodity Parent
		{
			get { return (Commodity)base.Parent; }
		}

		#region CheckBY_Description

		protected override void CheckBY_Description()
		{
			base.CheckBY_Description();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_DescriptionInfo);
			if (Parent.ValidateAllHasBeenRun)
			{
				ValidateChildCollections(Parent.BY_DescriptionInfo);
			}
		}

		void ValidateChildCollections(ZPropertyInfo notificationInfo)
		{
			var shipment = Parent.Shipment;
			var shipmentType = shipment.B0_ShipmentType;
			if (shipmentType == ShipmentTypes.Codes.Inbond && shipment.InBond.IsExport && !Parent.HarmonizedNumbers.Any())
			{
				notificationInfo.AddMessageError("Harmonized Numbers are required for Pre-filed In-bond.");
			}
			else if (shipmentType == ShipmentTypes.Codes.BRASS && !Parent.C4Codes.Any())
			{
				notificationInfo.AddMessageError("C4 Codes are required for Border Release Advance Selectivity Subsystem (BRASS).");
			}
		}

		#endregion

		#region CheckBY_BJ_Equipment

		protected override void CheckBY_BJ_Equipment()
		{
			base.CheckBY_BJ_Equipment();
			ListValidation.ErrorIfInvalidPK(Parent.BY_BJ_EquipmentInfo);
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_BJ_EquipmentInfo);
		}

		#endregion

		#region CheckBY_HarmonizedNumbers

		public void ValidateBY_HarmonizedNumbers()
		{
			((IValidationInternals)this).Validate(Parent.BY_HarmonizedNumbersInfo, CheckBY_HarmonizedNumbers);
		}

		protected void CheckBY_HarmonizedNumbers()
		{
			if (Parent.HarmonizedNumbers.Cast<CusCodeData>().Any(h => h.CY_DataInfo.HasNotifications()))
			{
				Parent.BY_HarmonizedNumbersInfo.AddMessageError("Some of the harmonized numbers are invalid. Please check Harmonized Numbers tab for more details.");
			}
		}

		#endregion

		#region CheckBY_HazardousGoodsIdentifier

		public void ValidateBY_HazardousGoodsIdentifier()
		{
			((IValidationInternals)this).Validate(Parent.BY_HazardousGoodsIdentifierInfo, CheckBY_HazardousGoodsIdentifier);
		}

		protected void CheckBY_HazardousGoodsIdentifier()
		{
			var undgs = Parent.UNDGs;
			if (undgs.Count == 1)
			{
				undgs[0].Validation.ValidateDI_DG();
				Parent.BY_HazardousGoodsIdentifierInfo.AddAllNotificationsFrom(undgs[0].DI_DGInfo);
			}
		}

		#endregion

		#region CheckBY_HazardousGoodsContact

		public void ValidateBY_HazardousGoodsContact()
		{
			((IValidationInternals)this).Validate(Parent.BY_HazardousGoodsContactInfo, CheckBY_HazardousGoodsContact);
		}

		protected void CheckBY_HazardousGoodsContact()
		{
			var undgs = Parent.UNDGs;
			if (undgs.Count == 1)
			{
				undgs[0].Validation.ValidateDI_OC_DGContact();
				Parent.BY_HazardousGoodsContactInfo.AddAllNotificationsFrom(undgs[0].DI_OC_DGContactInfo);
			}
		}

		#endregion

		#region CheckBY_ManifestUnitCode

		protected override void CheckBY_ManifestUnitCode()
		{
			base.CheckBY_ManifestUnitCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.BY_ManifestUnitCodeInfo);
			MandatoryValidation.MessageErrorIfUnitNotEntered(Parent.BY_ManifestUnitCodeInfo, Parent.BY_PieceCountInfo);
		}

		#endregion

		#region CheckBY_MonetaryValue

		protected override void CheckBY_MonetaryValue()
		{
			base.CheckBY_MonetaryValue();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.BY_MonetaryValueInfo);
			var shipmentType = Parent.Shipment.B0_ShipmentType.ToString();
			var types = new[] { ShipmentTypes.Codes.LowValue, ShipmentTypes.Codes.Inbond, ShipmentTypes.Codes.GoodsAstray };
			if (types.Contains(shipmentType) && Parent.BY_MonetaryValue.IsEmpty)
			{
				Parent.BY_MonetaryValueInfo.AddMessageError("Customs Value is required for Low Value Entries Informal, Pre-filed In-bond and Goods Astray.");
			}
			else if (shipmentType == ShipmentTypes.Codes.LowValue && Parent.BY_MonetaryValue > 2500)
			{
				Parent.BY_MonetaryValueInfo.AddMessageError("Customs value may not exceed $2500 for a Low Value Entries.");
			}
		}

		#endregion

		#region CheckBY_PieceCount

		protected override void CheckBY_PieceCount()
		{
			base.CheckBY_PieceCount();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.BY_PieceCountInfo);
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_PieceCountInfo);
		}

		#endregion

		#region CheckBY_RN_NKCountryOfOrigin

		protected override void CheckBY_RN_NKCountryOfOrigin()
		{
			base.CheckBY_RN_NKCountryOfOrigin();
			ListValidation.MessageErrorIfInvalidCode(Parent.BY_RN_NKCountryOfOriginInfo);
			if (Parent.Shipment.B0_ShipmentType == ShipmentTypes.Codes.LowValue && Parent.BY_RN_NKCountryOfOrigin.IsEmpty)
			{
				Parent.BY_RN_NKCountryOfOriginInfo.AddMessageError("Country Of Origin is required for Low Value Entries Informal.");
			}
		}

		#endregion

		#region CheckBY_GrossWeight

		protected override void CheckBY_GrossWeight()
		{
			base.CheckBY_GrossWeight();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.BY_GrossWeightInfo);
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_GrossWeightInfo);
		}

		#endregion

		#region CheckBY_GrossWeightUnit

		protected override void CheckBY_GrossWeightUnit()
		{
			base.CheckBY_GrossWeightUnit();
			ListValidation.MessageErrorIfInvalidCode(Parent.BY_GrossWeightUnitInfo);
			MandatoryValidation.MessageErrorIfUnitNotEntered(Parent.BY_GrossWeightUnitInfo, Parent.BY_GrossWeightInfo);
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			Parent.ValidateAllHasBeenRun = true;
			base.ValidateAll();
		}

		#endregion
	}
}
