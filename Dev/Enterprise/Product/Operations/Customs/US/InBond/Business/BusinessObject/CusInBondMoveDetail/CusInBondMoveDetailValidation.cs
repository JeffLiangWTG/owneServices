using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondMoveDetailValidation : Customs.Business.CusInBondMoveDetailValidation
	{
		public CusInBondMoveDetailValidation(CusInBondMoveDetail parent)
			: base(parent)
		{
		}

		protected SendingMessageValidationHelper Helper => new SendingMessageValidationHelper(Header, Parent);

		public override void ValidateAll()
		{
			var helper = Helper;
			Parent.ClearRowNotifications();
			if (!helper.IsArrivalValidationMode && !helper.IsExportationValidationMode)
			{
				base.ValidateAll();
				using (((ISingleElementListInternal)Parent).SuspendListChanged())
				{
					ValidateB9_FirstSecondaryNotifyParty();
					ValidateB9_SecondSecondaryNotifyParty();
					ValidateB9_ThirdSecondaryNotifyParty();
					ValidateB9_FourthSecondaryNotifyParty();
					ValidateAtLeastOneContainerExist();
					ValidateTotalManifestQtyIsEqualSumOfPieceCount();
				}
			}
		}

		void ValidateTotalManifestQtyIsEqualSumOfPieceCount()
		{
			if (IsInBondLevelDepartureValidationMode && Parent.IsDetailedInBond)
			{
				CusInBondBill bill = Parent.Bill;
				if (bill != null && bill.B0_ManifestQty != Parent.Containers.TotalPieceCount)
				{
					Parent.AddRowMessageError(ValidationConstants.MoveDetail.TotalManifestQtyNotEqualSumOfPieceCount);
				}
			}
		}

		void ValidateAtLeastOneContainerExist()
		{
			if (IsInBondLevelDepartureValidationMode && Parent.IsDetailedInBond && Parent.Containers.Count == 0)
			{
				Parent.AddRowMessageError(ValidationConstants.MoveDetail.AtLeastOneContainerIsRequired);
			}
		}

		public void ValidateB9_FirstSecondaryNotifyParty()
		{
			ValidateCalculatedProperty(Parent.B9_FirstSecondaryNotifyPartyInfo);
		}

		protected void CheckB9_FirstSecondaryNotifyParty()
		{
			if (IsInBondLevelDepartureValidationMode)
			{
				ValidateSecondaryNotifyPartyIsValid(Parent.B9_FirstSecondaryNotifyPartyInfo);

				var submitterABICode = Parent.SubmitterABICode;
				if (!submitterABICode.IsEmpty && GetSecondaryNotifyPartyByData(submitterABICode).Length == 0)
				{
					Parent.B9_FirstSecondaryNotifyPartyInfo.AddWarning(ValidationConstants.MoveDetail.SecondaryNotifyPartyIsNotSubmitter);
				}
			}
			ValidateB9_SecondSecondaryNotifyParty();
		}

		BusinessObject[] GetSecondaryNotifyPartyByData(ZString data)
		{
			return Parent.SecondaryNotifyParties.Find(new ZQuery(CusCodeDataSchema.CY_Data, data));
		}

		public void ValidateB9_SecondSecondaryNotifyParty()
		{
			ValidateCalculatedProperty(Parent.B9_SecondSecondaryNotifyPartyInfo);
		}

		protected void CheckB9_SecondSecondaryNotifyParty()
		{
			if (IsInBondLevelDepartureValidationMode)
			{
				if (Parent.B9_FirstSecondaryNotifyParty.IsEmpty && !Parent.B9_SecondSecondaryNotifyParty.IsEmpty)
				{
					Parent.B9_SecondSecondaryNotifyPartyInfo.AddMessageError(ValidationConstants.MoveDetail.GetSecondaryNotifyPartyIsSpecifiedOutOfOrderMessage("Second", "First"));
				}
				else
				{
					ValidateSecondaryNotifyPartyIsValid(Parent.B9_SecondSecondaryNotifyPartyInfo);
				}
			}
			ValidateB9_ThirdSecondaryNotifyParty();
			ValidateB9_FirstSecondaryNotifyParty();
		}

		public void ValidateB9_ThirdSecondaryNotifyParty()
		{
			ValidateCalculatedProperty(Parent.B9_ThirdSecondaryNotifyPartyInfo);
		}

		protected void CheckB9_ThirdSecondaryNotifyParty()
		{
			if (IsInBondLevelDepartureValidationMode)
			{
				if (Parent.B9_SecondSecondaryNotifyParty.IsEmpty && !Parent.B9_ThirdSecondaryNotifyParty.IsEmpty)
				{
					Parent.B9_ThirdSecondaryNotifyPartyInfo.AddMessageError(ValidationConstants.MoveDetail.GetSecondaryNotifyPartyIsSpecifiedOutOfOrderMessage("Third", "Second"));
				}
				else
				{
					ValidateSecondaryNotifyPartyIsValid(Parent.B9_ThirdSecondaryNotifyPartyInfo);
				}
			}
			ValidateB9_FourthSecondaryNotifyParty();
			ValidateB9_FirstSecondaryNotifyParty();
		}

		public void ValidateB9_FourthSecondaryNotifyParty()
		{
			ValidateCalculatedProperty(Parent.B9_FourthSecondaryNotifyPartyInfo);
		}

		protected void CheckB9_FourthSecondaryNotifyParty()
		{
			if (IsInBondLevelDepartureValidationMode)
			{
				if (Parent.B9_ThirdSecondaryNotifyParty.IsEmpty && !Parent.B9_FourthSecondaryNotifyParty.IsEmpty)
				{
					Parent.B9_FourthSecondaryNotifyPartyInfo.AddMessageError(ValidationConstants.MoveDetail.GetSecondaryNotifyPartyIsSpecifiedOutOfOrderMessage("Fourth", "Third"));
				}
				else
				{
					ValidateSecondaryNotifyPartyIsValid(Parent.B9_FourthSecondaryNotifyPartyInfo);
				}
			}
			ValidateB9_FirstSecondaryNotifyParty();
		}

		protected void ValidateSecondaryNotifyPartyIsValid(ZPropertyInfo info)
		{
			ZString snp = new ZString(info.Value);
			if (!snp.IsEmpty)
			{
				ZQuery query = new ZQuery(USCarrierCombinedSchema.UI_Code, snp);
				USCarrierCombined carrier = Factory.LoadTop1<USCarrierCombined>(query);
				if (carrier == null)
				{
					if (!ABIFilerValidator.IsValidFiler(snp))
					{
						CusInBondHeader header = Parent.Header;
						if (header != null && header.BH_FTZMove)
						{
							if (FIRMSCodeValidator.IsValidFIRMS(snp))
							{
								if (header.BH_FIRMS != snp)
								{
									info.AddMessageError(ValidationConstants.MoveDetail.FIRMSNotMatchJobFIRMSForFTZorBondedWarehouseWithdrawals);
								}
							}
							else
							{
								info.AddMessageError(ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValidFIRMSForFTZorBondedWarehouseWithdrawals);
							}
						}
						else
						{
							info.AddMessageError(ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValid);
						}
					}
				}
			}
		}

		protected override void CheckB9_PreviousITNumber()
		{
			base.CheckB9_PreviousITNumber();
			if ((IsInBondLevelDepartureValidationMode || IsAirInitiationAndDeletionValidationMode) && !Parent.B9_PreviousITNumber.IsEmpty)
			{
				ITNumberValidator.ValidatePreviousITNumberFormat(Parent.B9_PreviousITNumberInfo);
			}
			if (Parent.B9_PreviousITNumber.Length > 11 && IsAirValidationMode)
			{
				Parent.B9_PreviousITNumberInfo.AddMessageError(LengthExceeded);
			}
			if (!Parent.B9_PreviousITNumber.IsEmpty
				&& Parent.Header != null
				&& Parent.Header.BH_ImportTransportMode == TransportModeCodes.Codes.TruckNonContainer
				&& Parent.Header.BH_HeaderType == InBondHeaderTypeList.Codes.FullData)
			{
				Parent.B9_PreviousITNumberInfo.AddMessageError(ValidationConstants.Header.InvalidHeaderTypeForTransportModeWithPreviousITNumber);
			}
		}
		internal const string LengthExceeded = "You have entered a longer value than allowed. You are allowed to enter only 11 characters.";

		protected override void CheckB9_B0()
		{
			base.CheckB9_B0();
			if (!IsAirValidationMode)
			{
				ValidateB9_InBoundQty();
			}
			ValidateMovementDetailOfABillIsNotDuplicated();
		}

		protected void ValidateMovementDetailOfABillIsNotDuplicated()
		{
			CusInBondBill bill = Parent.Bill;
			if (bill != null)
			{
				CusInBondMoveHeader moveHeader = Parent.MoveHeader;
				if (moveHeader != null)
				{
					bool foundDuplicate = false;
					foreach (CusInBondMoveDetail otherMoveDetail in moveHeader.MovementDetails)
					{
						if (otherMoveDetail != Parent)
						{
							CusInBondBill otherBill = otherMoveDetail.Bill;
							if (otherBill != null && otherBill == bill)
							{
								foundDuplicate = true;
								break;
							}
						}
					}

					if (foundDuplicate)
					{
						Parent.B9_B0Info.AddError(ValidationConstants.MoveDetail.MovementDetailForBillIsDuplicated);
					}
				}
			}
		}

		protected override void CheckB9_BM()
		{
			base.CheckB9_BM();
			if (!Parent.IsInDatabase || Parent.B9_BMInfo.OriginalValue.CompareTo(Parent.B9_BM) != 0)
			{
				if (Parent.IsWaitingForResponse)
				{
					Parent.B9_BMInfo.AddError(ValidationConstants.MoveDetail.CannotAddMovementDetailsWhilePendingCustoms);
				}
				else
				{
					IInBondMessagingHeader moveHeader = Parent.MoveHeader;
					if (moveHeader != null && moveHeader.HasClearDepartureAdd)
					{
						Parent.B9_BMInfo.AddError(ValidationConstants.MoveDetail.CannotAddMovementDetailsWhenInBondHasBeenSubmitted);
					}
				}
			}
		}

		protected override void CheckB9_InBoundQty()
		{
			base.CheckB9_InBoundQty();
			if (IsInBondLevelDepartureValidationMode)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.B9_InBoundQtyInfo);
				CusInBondBill bill = Parent.Bill;
				if (bill != null && bill.B0_ManifestQty < Parent.B9_InBoundQty)
				{
					Parent.B9_InBoundQtyInfo.AddMessageError(ValidationConstants.MoveDetail.InBondQtyGreaterThanBillManifestQty);
				}
			}
		}

		protected BusinessObjectFactory Factory
		{
			get { return Parent.Factory; }
		}

		protected new CusInBondMoveDetail Parent
		{
			get { return (CusInBondMoveDetail)base.Parent; }
		}

		protected CusInBondHeader Header => Parent.Header;

		bool IsInBondLevelDepartureValidationMode
		{
			get
			{
				var header = Header;
				return header != null && header.IsInBondLevelDepartureValidationMode;
			}
		}

		bool IsAirInitiationAndDeletionValidationMode
		{
			get
			{
				var header = Header;
				return header != null && header.IsAirInBondInitiationAndDeletionMode;
			}
		}

		bool IsAirValidationMode
		{
			get
			{
				var header = Header;
				return header != null && header.IsAir;
			}
		}
	}
}
