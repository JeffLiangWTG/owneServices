using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.UY.Manifest.Business
{
	public class AsycudaBillValidationForRegularBill : ASYCUDA.Business.AsycudaBillValidationForRegularBill
	{
		public AsycudaBillValidationForRegularBill(AsycudaBill parent) : base(parent)
		{
		}

		protected new AsycudaBill Parent => (AsycudaBill)base.Parent;

		protected override ZBool NeedsToCheckABL_GrossWeightMatchSumOfPacks => true;

		protected override ZBool NeedsToCheckABL_VolumeMatchSumOfPacks => true;

		#region  Shipper

		protected override void CheckABL_ShipperRegNoType()
		{
			base.CheckABL_ShipperRegNoType();

			ListValidation.MessageErrorIfInvalidCode(Parent.ABL_ShipperRegNoTypeInfo);
		}

		protected override void CheckABL_ShipperRegNo()
		{
			base.CheckABL_ShipperRegNo();
			var parent = Parent;

			Check_RegNo(parent.ABL_ShipperRegNoInfo, parent.ABL_ShipperRegNoType);
		}

		protected override void CheckABL_ShipperName()
		{
			base.CheckABL_ShipperName();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ShipperNameInfo);
		}

		#endregion

		#region Consignee

		protected override void CheckABL_ConsigneeRegNoType()
		{
			base.CheckABL_ConsigneeRegNoType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_ConsigneeRegNoTypeInfo);
		}

		protected override void CheckABL_ConsigneeRegNo()
		{
			base.CheckABL_ConsigneeRegNo();
			var parent = Parent;

			MandatoryValidation.MessageErrorIfNotEntered(parent.ABL_ConsigneeRegNoInfo);

			Check_RegNo(parent.ABL_ConsigneeRegNoInfo, parent.ABL_ConsigneeRegNoType);
		}

		protected override void CheckABL_ConsigneeName()
		{
			base.CheckABL_ConsigneeName();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ConsigneeNameInfo);
		}

		protected override void CheckABL_RN_NKConsigneeCountry()
		{
			base.CheckABL_RN_NKConsigneeCountry();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_RN_NKConsigneeCountryInfo);
		}

		#endregion

		#region Notify Party

		protected override void CheckABL_NotifyPartyRegNoType()
		{
			base.CheckABL_NotifyPartyRegNoType();
			ListValidation.MessageErrorIfInvalidCode(Parent.ABL_NotifyPartyRegNoTypeInfo);
		}

		protected override void CheckABL_NotifyPartyRegNo()
		{
			base.CheckABL_NotifyPartyRegNo();
			var parent = Parent;

			MandatoryValidation.WarnIfNotEntered(parent.ABL_NotifyPartyRegNoInfo);

			Check_RegNo(parent.ABL_NotifyPartyRegNoInfo, parent.ABL_NotifyPartyRegNoType);
		}

		protected override void CheckABL_NotifyPartyName()
		{
			base.CheckABL_NotifyPartyName();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_NotifyPartyNameInfo);
		}

		protected override void CheckABL_NotifyPartyPhone()
		{
			base.CheckABL_NotifyPartyPhone();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_NotifyPartyPhoneInfo);
		}

		protected override void CheckABL_NotifyPartyStreet1()
		{
			base.CheckABL_NotifyPartyStreet1();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_NotifyPartyStreet1Info);
		}

		#endregion

		protected override void CheckABL_BillNumber()
		{
			base.CheckABL_BillNumber();

			if (Parent.Packs.Count == 0)
			{
				Parent.ABL_BillNumberInfo.AddMessageError(ResString.GetMultilingualString("06DC2F3D-49BE-4092-A45C-5E502C11792A", "At least one Pack should be inserted"));
			}
		}

		protected override void CheckABL_RL_NKFinalDestination()
		{
			base.CheckABL_RL_NKFinalDestination();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_RL_NKFinalDestinationInfo);
		}

		protected override void CheckABL_Volume()
		{
			base.CheckABL_Volume();
			MandatoryValidation.MessageErrorIfIsZero(Parent.ABL_VolumeInfo);
		}

		protected override void CheckABL_PrepaidCollect()
		{
			base.CheckABL_PrepaidCollect();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_PrepaidCollectInfo);
		}

		#region Helpers

		protected void Check_RegNo(ZPropertyInfo regNoInfo, ZString regNoType)
		{
			var regNo = (ZString)regNoInfo.Value;
			var lengthRegNoCID = 8;
			var lengthRegNo = 12;

			if (!regNo.IsEmpty)
			{
				if (regNoType == UruguayOrgCusCodeInfo.OrgCusCodes.CID && regNo.Length != lengthRegNoCID)
				{
					regNoInfo.AddMessageError(ResString.GetMultilingualString("EE124426-6C4C-4A71-AEFB-7FD96710DD19", "The length should be: {0}", lengthRegNoCID));
				}
				else if (regNoType == UruguayOrgCusCodeInfo.OrgCusCodes.RUT)
				{
					if (regNo.Length != lengthRegNo || !regNo.IsNumbersOnlyOrEmpty)
					{
						regNoInfo.AddMessageError(ResString.GetMultilingualString("3AE56AC3-D9DA-4628-870F-890ACAD1D6F9", "The RUT code should be: {0} digits only", lengthRegNo));
					}
					else
					{
						if (!UYRUTCodeValidator.IsValidCheckDigit(regNo))
						{
							regNoInfo.AddMessageError(ResString.GetMultilingualString("B5AA570C-BEB4-4F61-89DB-7087E8178FB3", @"The registration number entered is not valid. The last character (check digit) is incorrect."));
						}
					}
				}
			}
		}

		#endregion
	}
}
