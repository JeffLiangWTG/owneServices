using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PE.Manifest.Business
{
	public class AsycudaBillValidationForRegularBill : ASYCUDA.Business.AsycudaBillValidationForRegularBill
	{
		public AsycudaBillValidationForRegularBill(AsycudaBill parent) : base(parent)
		{
		}

		protected new AsycudaBill Parent => (AsycudaBill)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateCargoNature();
			ValidateCargoCondition();
		}

		public void ValidateCargoNature()
		{
			ValidateCalculatedProperty(Parent.CargoNatureInfo);
		}

		public void ValidateCargoCondition()
		{
			ValidateCalculatedProperty(Parent.CargoConditionInfo);
		}

		protected void CheckCargoNature()
		{
			if (Parent.Header.IsSea)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CargoNatureInfo);
			}
		}

		protected void CheckCargoCondition()
		{
			if (Parent.Header.IsSea)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CargoConditionInfo);
			}
		}

		protected override void CheckABL_GoodsDescription()
		{
			base.CheckABL_GoodsDescription();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_GoodsDescriptionInfo);
		}

		protected override void CheckABL_OA_Shipper()
		{
			base.CheckABL_OA_Shipper();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_OA_ShipperInfo);
		}

		protected override void CheckABL_Volume()
		{
			base.CheckABL_Volume();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_VolumeInfo);
		}

		protected override void CheckABL_VolumeUQ()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_VolumeUQInfo);
		}

		protected override void CheckABL_OA_NotifyParty()
		{
			base.CheckABL_OA_NotifyParty();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_OA_NotifyPartyInfo);
		}

		protected override void CheckABL_BillIssueDate()
		{
			base.CheckABL_BillIssueDate();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_BillIssueDateInfo);
		}

		protected override ZBool NeedsToCheckABL_GrossWeightMatchSumOfPacks => true;

		protected override ZBool NeedsToCheckABL_VolumeMatchSumOfPacks => true;

		protected override INotificationType ABL_ManifestQtyMatchSumOfPacksNotificationType => CargoWise.EntityFramework.NotificationType.MessageError;

		protected override INotificationType NotificationTypeForDuplicateBillNumber => CargoWise.EntityFramework.NotificationType.Error;

		protected override void CheckABL_ShipperRegNoType()
		{
			base.CheckABL_ShipperRegNoType();

			Check_RegNoType(Parent.ABL_ShipperRegNoTypeInfo, Parent.ABL_RN_NKShipperCountry);
		}

		protected override void CheckABL_ShipperRegNo()
		{
			base.CheckABL_ShipperRegNo();

			if (Parent.ABL_RN_NKShipperCountry == Core.Constants.CountryCodes.Peru)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ShipperRegNoInfo);
			}
		}

		protected override void CheckABL_ConsigneeRegNoType()
		{
			base.CheckABL_ConsigneeRegNoType();

			Check_RegNoType(Parent.ABL_ConsigneeRegNoTypeInfo, Parent.ABL_RN_NKConsigneeCountry);
		}

		protected override void CheckABL_ConsigneeRegNo()
		{
			base.CheckABL_ConsigneeRegNo();

			if (Parent.ABL_RN_NKConsigneeCountry == Core.Constants.CountryCodes.Peru)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ConsigneeRegNoInfo);
			}
		}

		protected override void CheckABL_NotifyPartyRegNoType()
		{
			base.CheckABL_NotifyPartyRegNoType();

			Check_RegNoType(Parent.ABL_NotifyPartyRegNoTypeInfo, Parent.ABL_RN_NKNotifyPartyCountry);
		}

		protected override void CheckABL_NotifyPartyRegNo()
		{
			base.CheckABL_NotifyPartyRegNo();

			if (Parent.ABL_RN_NKNotifyPartyCountry == Core.Constants.CountryCodes.Peru)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_NotifyPartyRegNoInfo);
			}
		}

		protected void Check_RegNoType(ZPropertyInfo regNoTypeInfo, ZString country)
		{
			var regNoType = (ZString)regNoTypeInfo.Value;

			if (!regNoType.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(regNoTypeInfo);

				if (country == Core.Constants.CountryCodes.Peru)
				{
					if (regNoType != OrgCusCode.PeruCodeTypes.GovernmentTaxFileCode && regNoType != OrgCusCode.PeruCodeTypes.DNI)
					{
						regNoTypeInfo.AddMessageError(ResString.GetMultilingualString("53CE4042-3A92-490F-947D-C318E6E07FB5", "RUC or DNI type should be selected"));
					}
				}
			}
			else
			{
				if (country == Core.Constants.CountryCodes.Peru)
				{
					MandatoryValidation.MessageErrorIfNotEntered(regNoTypeInfo);
				}
			}
		}
	}
}
