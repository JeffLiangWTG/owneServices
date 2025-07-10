using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.AMS.Messaging.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public abstract class CommonCusInBondBillValidation : Customs.Business.CusInBondBillValidation
	{
		protected CommonCusInBondBillValidation(CusInBondBill parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateB0_ForeignPortOfUnladingKCode();
				ValidateB0_PlaceOfDelivery();
			}
		}

		public void ValidateB0_ForeignPortOfUnladingKCode()
		{
			ValidateCalculatedProperty(Parent.B0_ForeignPortOfUnladingKCodeInfo);
		}

		protected virtual void CheckB0_ForeignPortOfUnladingKCode()
		{
		}

		public void ValidateB0_PlaceOfDelivery()
		{
			ValidateCalculatedProperty(Parent.B0_PlaceOfDeliveryInfo);
		}

		protected virtual void CheckB0_PlaceOfDelivery()
		{
		}

		bool HasNVOCCBill
		{
			get
			{
				var header = Parent.Header;
				return header != null && header.Bills.HasNVOCCBill;
			}
		}

		protected override void CheckB0_MasterBillNumber()
		{
			base.CheckB0_MasterBillNumber();
			if (IsMasterBillDetailRequired)
			{
				if (!Parent.B0_IssuerCode.IsEmpty || !Parent.IsOceanBillType || HasNVOCCBill)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.B0_MasterBillNumberInfo);
				}

				var masterBillNumberTruncated = Parent.B0_MasterBillNumber.KeepAlphanumericCharacters();
				if (masterBillNumberTruncated.Length > 12)
				{
					Parent.B0_MasterBillNumberInfo.AddMessageError(ValidationConstants.Bill.BillOfLadingNumberIsTooLong(Parent.B0_MasterBillNumber));
				}
				if (!Parent.B0_MasterBillNumber.IsLettersAndNumbersOnlyOrEmpty)
				{
					Parent.B0_MasterBillNumberInfo.AddMessageError(ValidationConstants.Bill.BillOfLadingNumberAlphanumeric);
				}

				var scac = Parent.B0_IssuerCode;

				if (!scac.IsEmpty && masterBillNumberTruncated.StartsWith(scac, StringComparison.CurrentCultureIgnoreCase))
				{
					Parent.B0_MasterBillNumberInfo.AddMessageError(ValidationConstants.Bill.MasterNumberIsInvalidWithSCACCode);
				}
			}
			ValidateB0_IssuerCode();
		}

		protected override void CheckB0_IssuerCode()
		{
			base.CheckB0_IssuerCode();
			if (IsMasterBillDetailRequired)
			{
				if (!Parent.B0_MasterBillNumber.IsEmpty || !Parent.IsOceanBillType || HasNVOCCBill)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.B0_IssuerCodeInfo);
				}
				ListValidation.MessageErrorIfInvalidCode(Parent.B0_IssuerCodeInfo);
			}
			ValidateB0_MasterBillNumber();
		}

		protected override void CheckB0_Firms()
		{
			base.CheckB0_Firms();
			if (IsInventoryRecordValidationMode || IsPTTValidationMode)
			{
				if (Parent.B0_BillStatus == BillOfLadingStatusIndicatorList.Codes.InternationalMailDirectDischargeAtMailFacility
					|| Parent.B0_BillStatus == BillOfLadingStatusIndicatorList.Codes.InternationalMailInBondToInternationalMailFacility
					|| IsPTTValidationMode)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.B0_FirmsInfo);
				}
				ListValidation.MessageErrorIfInvalidCode(Parent.B0_FirmsInfo);

				if (Parent.Header != null)
				{
					var firms = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(
								Parent.Factory,
								Parent.B0_Firms,
								Core.Constants.CountryCodes.UnitedStates,
								Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode,
								ZDateTime.Today,
								null,
								new[] {
									new RefCusCodeListAttributeFilter(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DistrictPortCode, SQLComparisonOperator.Equal, new ZString[] { Parent.Header.BH_PortUnladingDCode })
									});
					if (firms == null)
					{
						Parent.B0_FirmsInfo.AddNotification(IsPTTValidationMode ? NotificationType.MessageError : NotificationType.Warning, ValidationConstants.Bill.FIRMSDoesNotMatchDDPP);
					}
				}
			}
		}

		protected new CusInBondBill Parent
		{
			get { return (CusInBondBill)base.Parent; }
		}

		protected bool IsMasterBillDetailRequired
		{
			get { return Parent.ValidationModes != ValidationModes.None && !IsChangeEstDateOfArrivalValidationMode && !IsVesselArrivalValidationMode && !IsVesselDepartureValidationMode; }
		}

		protected bool IsInventoryRecordValidationMode
		{
			get { return Parent.IsInventoryRecordValidationMode; }
		}

		protected bool IsVesselArrivalValidationMode
		{
			get { return Parent.IsVesselArrivalValidationMode; }
		}

		protected bool IsVesselDepartureValidationMode
		{
			get { return Parent.IsVesselDepartureValidationMode; }
		}

		protected bool IsPTTValidationMode
		{
			get { return Parent.IsPTTValidationMode; }
		}

		protected bool IsChangeEstDateOfArrivalValidationMode
		{
			get { return Parent.IsChangeEstDateOfArrivalValidationMode; }
		}
	}
}
