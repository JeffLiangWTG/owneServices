using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondMoveHeaderValidation : US.Business.CusInBondMoveHeaderValidation
	{
		public CusInBondMoveHeaderValidation(CusInBondMoveHeader parent)
			: base(parent)
		{
		}

		protected new CusInBondMoveHeader Parent
		{
			get { return (CusInBondMoveHeader)base.Parent; }
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateBM_ManifestSequenceNumber();
				ValidateAtLeastOneMovementDetailsExist();
			}
		}

		public void ValidateBM_ManifestSequenceNumber()
		{
			ValidateCalculatedProperty(Parent.BM_ManifestSequenceNumberInfo);
		}

		protected void CheckBM_ManifestSequenceNumber()
		{
			if (Parent.BM_ManifestSequenceNumber.IsEmpty && Parent.IsAMSMovement && Parent.ValidationModes != ValidationModes.None && !ValidationModesCalculator.IsThisValidationOn(Parent.ValidationModes, ValidationModes.InventoryRecord))
			{
				Parent.BM_ManifestSequenceNumberInfo.AddMessageError(ValidationConstants.MoveHeader.ManifestSequenceNumberIsRequired);
			}
		}

		void ValidateAtLeastOneMovementDetailsExist()
		{
			if ((IsInBondArrivalValidationMode || IsInBondExportationValidationMode || IsInBondTOLValidationMode) && Parent.MovementDetails.Count == 0)
			{
				Parent.AddRowMessageError(ValidationConstants.MoveHeader.AtLeastOneMoveDetailIsRequired);
			}
		}

		protected override void CheckInBondNumber()
		{
			if (Parent.IsInBondMovement)
			{
				base.CheckInBondNumber();
			}
		}

		protected override void CheckBM_InBondCarrierID()
		{
			base.CheckBM_InBondCarrierID();
			var isMessageValidationMode = IsSubsequentInBondValidationMode || IsPermitToTransferValidationMode || IsInBondDiversionValidationMode;
			if (isMessageValidationMode)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_InBondCarrierIDInfo);
			}

			if (!Parent.BM_InBondCarrierID.IsEmpty)
			{
				if (!(EmployerIdentificationNumberValidator.IsValidEIN(Parent.BM_InBondCarrierID) || CBPAssignedNumberValidator.IsValidCBPAssignedNumber(Parent.BM_InBondCarrierID) || SocialSecurityNumberValidator.IsValidSSN(Parent.BM_InBondCarrierID)))
				{
					Parent.BM_InBondCarrierIDInfo.AddNotification(isMessageValidationMode ? NotificationType.MessageError : NotificationType.Warning, ValidationConstants.MoveHeader.InBondCarrierIDValid);
				}

				if ((Parent.IsPTTMovement || IsPermitToTransferValidationMode) && Parent.MovementDetails.Count == 0)
				{
					Parent.BM_InBondCarrierIDInfo.AddNotification(isMessageValidationMode ? NotificationType.MessageError : NotificationType.Warning, ValidationConstants.MoveHeader.AtLeastOneMoveDetailIsRequired);
				}
			}
		}

		protected override void CheckBM_ArrivalDate()
		{
			base.CheckBM_ArrivalDate();
			if (IsInBondArrivalValidationMode)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_ArrivalDateInfo);
				if (Parent.BM_ArrivalDate > ZDateTime.Now)
				{
					Parent.BM_ArrivalDateInfo.AddMessageError(ValidationConstants.MoveHeader.ArrivalDateExceedsTodaysDate);
				}
			}
		}

		protected override void CheckBM_TOLCarrierID()
		{
			if (IsInBondTOLValidationMode)
			{
				base.CheckBM_TOLCarrierID();
				var carrierID = Parent.BM_TOLCarrierID;
				if (!carrierID.IsEmpty && (!(EmployerIdentificationNumberValidator.IsValidEIN(carrierID) || CBPAssignedNumberValidator.IsValidCBPAssignedNumber(carrierID) || SocialSecurityNumberValidator.IsValidSSN(carrierID))))
				{
					Parent.BM_TOLCarrierIDInfo.AddMessageError(ValidationConstants.MoveHeader.InBondCarrierIDValid);
				}
				else
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_TOLCarrierIDInfo);
				}
			}
		}

		protected override void CheckBM_TOLCarrierCode()
		{
			base.CheckBM_TOLCarrierCode();
			if (IsInBondTOLValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BM_TOLCarrierCodeInfo);
			}
		}

		protected override void CheckBM_TOLCityName()
		{
			base.CheckBM_TOLCityName();
			if (IsInBondTOLValidationMode)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_TOLCityNameInfo);
			}
			ValidateBM_TOLStateCode();
		}

		protected override void CheckBM_TOLDate()
		{
			base.CheckBM_TOLDate();
			if (IsInBondTOLValidationMode)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_TOLDateInfo);
			}
		}

		protected override void CheckBM_TOLStateCode()
		{
			if (IsInBondTOLValidationMode)
			{
				base.CheckBM_TOLStateCode();
				if (!Parent.BM_TOLCityName.IsEmpty && Parent.BM_TOLStateCode.IsEmpty)
				{
					Parent.BM_TOLStateCodeInfo.AddMessageError(ValidationConstants.MoveHeader.TOLStateCodeIsRequired);
				}
				else if (!Parent.BM_TOLStateCode.IsEmpty)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.BM_TOLStateCodeInfo);
				}
			}
		}

		protected override void CheckBM_InBondEntryType()
		{
			base.CheckBM_InBondEntryType();
			if (IsSubsequentInBondValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BM_InBondEntryTypeInfo);
			}
			ValidateBM_ForeignDestPortKCode();
			ValidateBM_DestinationPortCode();
		}

		protected override void CheckBM_InBondCarrierSCAC()
		{
			base.CheckBM_InBondCarrierSCAC();
			if (IsSubsequentInBondValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BM_InBondCarrierSCACInfo);
			}
		}

		protected override void CheckBM_DestinationPortCode()
		{
			base.CheckBM_DestinationPortCode();
			if (IsSubsequentInBondValidationMode || IsInBondArrivalValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BM_DestinationPortCodeInfo);
			}

			if (IsSubsequentInBondValidationMode && !Parent.BM_IsSubsequentInBond && Parent.BM_InBondEntryType == InbondCommonTypeList.Codes._1ImmediateTransport)
			{
				var header = Parent.Header;
				if (header != null && Parent.BM_DestinationPortCode == header.BH_PortUnladingDCode)
				{
					Parent.BM_DestinationPortCodeInfo.AddMessageError(ValidationConstants.MoveHeader.USDestinationShouldNotMatchPortOfArrivalFor61EntryType);
				}
			}
		}

		#region Validation Modes

		bool IsSubsequentInBondValidationMode
		{
			get { return Parent.IsSubsequentInBondValidationMode; }
		}

		bool IsInBondArrivalValidationMode
		{
			get { return Parent.IsInBondArrivalValidationMode; }
		}

		bool IsInBondExportationValidationMode
		{
			get { return Parent.IsInBondExportationValidationMode; }
		}

		bool IsInBondTOLValidationMode
		{
			get { return Parent.IsInBondTOLValidationMode; }
		}

		bool IsInBondDiversionValidationMode
		{
			get { return Parent.IsInBondDiversionValidationMode; }
		}

		bool IsPermitToTransferValidationMode
		{
			get { return Parent.IsPermitToTransferValidationMode; }
		}

		#endregion
	}
}
