using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class AsycudaBillValidationForRegularBill : ASYCUDA.Business.AsycudaBillValidationForRegularBill
	{
		public AsycudaBillValidationForRegularBill(AsycudaBill parent) : base(parent)
		{
		}

		protected new AsycudaBill Parent => (AsycudaBill)base.Parent;

		protected override ZBool NeedsToCheckABL_Consignee => false;
		protected override ZBool NeedsToCheckABL_GrossWeight => false;
		protected override ZBool NeedsToCheckABL_GrossWeightUQ => false;
		protected override ZBool NeedsToCheckABL_ManifestUQ => false;
		protected override ZBool NeedsToCheckABL_ManifestQty => false;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateIsToOrder();
			ValidateNotOwned();
			ValidatePaymentType();
			ValidateTransshipmentType();
			ValidateBillStampDutyValue();
			ValidateABL_SpecialCargoCode();
		}

		protected override void CheckCustomsEntryNumberType()
		{
			if (Parent.Header?.NeedPreviousDeclarationNumbers ?? false)
			{
				if (Parent.CustomsEntryNumberType != CusEntryNumberTypes.Turkey.PRV)
				{
					Parent.CustomsEntryNumberTypeInfo.AddMessageError(ResString.GetMultilingualString("17D9AF7B-CDF0-45AA-BC53-393CB9581348", "Previous Declaration is needed."));
				}
			}
			base.CheckCustomsEntryNumberType();
		}

		protected override void CheckCustomsEntryNumber()
		{
			if (Parent.CustomsEntryNumberType == CusEntryNumberTypes.Turkey.PRV && Parent.CustomsEntryNumber.IsEmpty)
			{
				Parent.CustomsEntryNumberInfo.AddMessageError(ResString.GetMultilingualString("E3411FCB-F671-451E-8A9C-770295358D54", "You have not entered Customs Number."));
			}
			base.CheckCustomsEntryNumber();
		}

		public void ValidatePaymentType()
		{
			ValidateCalculatedProperty(Parent.PaymentTypeInfo);
		}

		protected void CheckPaymentType()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.PaymentTypeInfo);
		}

		public void ValidateTransshipmentType()
		{
			ValidateCalculatedProperty(Parent.TransshipmentTypeInfo);
		}

		protected void CheckTransshipmentType()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.TransshipmentTypeInfo);
		}

		protected override void CheckABL_ConsigneeRegNo()
		{
			base.CheckABL_ConsigneeRegNo();
			if (Parent.Header.IsImport && !Parent.IsToOrder && Parent.Header.AMA_ManifestType != TRManifestTypes.Codes.VARONC)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ConsigneeRegNoInfo);
			}
		}

		protected override void CheckMandatoryABL_OA_Consignee()
		{
		}

		protected override void CheckABL_ShipperName()
		{
			base.CheckABL_ShipperName();
			if (Parent.Header.AMA_ManifestType != TRManifestTypes.Codes.VARONC)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ShipperNameInfo);
			}
		}

		protected override void CheckABL_ShipperRegNo()
		{
			base.CheckABL_ShipperRegNo();
			if (Parent.Header.IsExport &&
				(Parent.Header.AMA_ManifestType != TRManifestTypes.Codes.CIKONC ||
				Parent.Header.AMA_ManifestType != TRManifestTypes.Codes.DENIHR ||
				Parent.Header.AMA_ManifestType != TRManifestTypes.Codes.HAVIHR ||
				Parent.Header.AMA_ManifestType != TRManifestTypes.Codes.TESLIM))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ShipperRegNoInfo);
			}
		}

		#region IsToOrder and NotOwned4

		public void ValidateIsToOrder()
		{
			ValidateCalculatedProperty(Parent.IsToOrderInfo);
		}

		public void ValidateNotOwned()
		{
			ValidateCalculatedProperty(Parent.NotOwnedInfo);
		}

		#endregion

		public void ValidateBillStampDutyValue()
		{
			ValidateCalculatedProperty(Parent.BillStampDutyValueInfo);
		}

		protected void CheckBillStampDutyValue()
		{
			if (Parent.Header.IsSea && !Parent.BillStampDutyValue.IsEmpty)
			{
				if (Parent.Packs.Cast<AsycudaPack>().Any(x => x.HasEmptyContainer))
				{
					Parent.BillStampDutyValueInfo.AddMessageError(Res.GetString("E2AD728C-4822-4914-92D1-DA9DB678BFDF", "Manifest transport type is 'SEA' and bill has Empty type containers, do not add this stamp duty."));
				}
			}
		}

		protected override void CheckABL_NotifyPartyRegNo()
		{
			base.CheckABL_NotifyPartyRegNo();
			if (Parent.Header.IsImport && Parent.IsToOrder)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_NotifyPartyRegNoInfo);
			}
		}

		protected override void CheckABL_LocationInformation()
		{
			base.CheckABL_LocationInformation();
			if (Parent.Header.IsAir && Parent.Header.AMA_ManifestType == TRManifestTypes.Codes.HAVIHR || Parent.Header.AMA_ManifestType == TRManifestTypes.Codes.HAVITH)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_LocationInformationInfo);
			}
		}

		protected override void CheckABL_SpecialCargoCode()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_SpecialCargoCodeInfo);
		}

		protected override void CheckMandatoryABL_RL_NKOrigin()
		{
			var header = Parent.Header;
			if (header.AMA_ManifestType == TRManifestTypes.Codes.GRUPAJ && (header.IsSea || header.IsAir))
			{
				base.CheckMandatoryABL_RL_NKOrigin();
			}
		}

		protected override void CheckABL_ConsigneeName()
		{
			if (Parent.Header.AMA_ManifestType != TRManifestTypes.Codes.VARONC)
			{
				base.CheckABL_ConsigneeName();
			}
		}
	}
}
