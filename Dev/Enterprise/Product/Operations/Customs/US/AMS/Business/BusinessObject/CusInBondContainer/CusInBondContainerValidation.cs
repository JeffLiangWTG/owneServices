using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondContainerValidation : Customs.Business.CusInBondContainerValidation
	{
		public CusInBondContainerValidation(CusInBondContainer parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateAtLeastOneCommodityExist();
				ValidateMaximumNumberOfUNDG();
			}
		}

		void ValidateMaximumNumberOfUNDG()
		{
			if (IsInventoryRecordValidationMode && Parent.UNDGs.Count > 99)
			{
				Parent.AddRowMessageError(ValidationConstants.Container.MaximumNumberOfUNDGExceeded);
			}
		}

		void ValidateAtLeastOneCommodityExist()
		{
			if (IsInventoryRecordValidationMode && Parent.Commodities.Count == 0)
			{
				Parent.AddRowMessageError(ValidationConstants.Container.AtLeastOneCommodityIsRequired);
			}
		}

		protected override void CheckBC_ContainerNum()
		{
			base.CheckBC_ContainerNum();
			if (Parent.BC_ContainerNum.IsEmpty)
			{
				if (IsInventoryRecordValidationMode)
				{
					Parent.BC_ContainerNumInfo.AddMessageError(ValidationConstants.Container.ContainerNumberIsRequired);
				}
			}
			else
			{
				if (IsInventoryRecordValidationMode)
				{
					CheckContainerNumHasValidCharactersOnly();

					if (!Parent.BC_ContainerNumInfo.HasMessageErrors())
					{
						CheckContainerNumHasValidCheckDigit();
					}
				}
				ValidateContainerNumberIsNotDuplicated();
			}

			if (!Parent.IsNonContainerized && Parent.HasSailingLinkage && ((ISailingSynchronisationTarget<BillOfLadingContainer>)Parent).Source == null)
			{
				Parent.BC_ContainerNumInfo.AddWarning(ValidationConstants.SailingSynchronisation.ContainerMightBeIncorectlyAdded);
			}

			ValidateBC_RC();
			ValidateBC_Seal1();
			ValidateBC_Seal2();
		}

		void CheckContainerNumHasValidCharactersOnly()
		{
			var containerNumber = Parent.BC_ContainerNum;
			if (containerNumber.StartsWith(" "))
			{
				Parent.BC_ContainerNumInfo.AddMessageError(ValidationConstants.Container.ContainerNumberCanNotContainLeadingSpaces);
			}
			else if (containerNumber.KeepAlphanumericCharacters() != containerNumber.Replace(" ", ""))
			{
				Parent.BC_ContainerNumInfo.AddMessageError(ValidationConstants.Container.MustOnlyContainAlphaNumerics);
			}
		}

		void CheckContainerNumHasValidCheckDigit()
		{
			if (!Parent.IsNonContainerized)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.BC_ContainerNumInfo);
				ContainerNumberValidation.WarnIfInvalid(Parent.BC_ContainerNumInfo);
			}
		}

		protected void ValidateContainerNumberIsNotDuplicated()
		{
			var moveDetail = Parent.MoveDetail;
			if (moveDetail != null)
			{
				var foundDuplicate = false;
				foreach (var otherContainer in moveDetail.Containers)
				{
					if (otherContainer != Parent && otherContainer.BC_ContainerNum == Parent.BC_ContainerNum)
					{
						foundDuplicate = true;
						break;
					}
				}

				if (foundDuplicate)
				{
					Parent.BC_ContainerNumInfo.AddError(ValidationConstants.Container.ContainerNumberIsDuplicated);
				}
			}
		}

		protected override void CheckBC_RC()
		{
			base.CheckBC_RC();
			ListValidation.ErrorIfInvalidPK(Parent.BC_RCInfo);

			if (Parent.IsContainerNumberStartsWithSCAC && ((ICommonContainer)Parent).ContainerEquipmentDescriptionCode.IsEmpty)
			{
				Parent.BC_RCInfo.AddMessageError(ValidationConstants.Container.MissingUSContainerCode);
			}
		}

		protected override void CheckBC_Seal1()
		{
			base.CheckBC_Seal1();
			AMSCharactersValidator.ValidateCharacters(Parent.BC_Seal1Info, true);

			if (Parent.BC_Seal1.Length > 15)
			{
				Parent.BC_Seal1Info.AddMessageError(ValidationConstants.Container.MaxLengthOfSealNoExceeded);
			}
		}

		protected override void CheckBC_Seal2()
		{
			base.CheckBC_Seal2();
			AMSCharactersValidator.ValidateCharacters(Parent.BC_Seal2Info, true);

			if (Parent.BC_Seal2.Length > 15)
			{
				Parent.BC_Seal2Info.AddMessageError(ValidationConstants.Container.MaxLengthOfSealNoExceeded);
			}
		}

		protected override void CheckBC_IsEmpty()
		{
			base.CheckBC_IsEmpty();
			if (IsInventoryRecordValidationMode && !Parent.BC_IsEmpty)
			{
				var bill = Parent.Bill;
				if (bill != null && bill.B0_BillStatus == BillOfLadingStatusIndicatorList.Codes.EmptyEquipmentInstrumentsOfInternationalTrade)
				{
					Parent.BC_IsEmptyInfo.AddMessageError(ValidationConstants.Container.NotEmptyWhenBillStatusIsEmptyContainer);
				}
			}
			ValidateBC_ForeignPortKCode();
		}

		protected override void CheckBC_ForeignPortKCode()
		{
			base.CheckBC_ForeignPortKCode();
			if (IsInventoryRecordValidationMode)
			{
				if (Parent.BC_IsEmpty && Parent.BC_ForeignPortKCode.IsEmpty)
				{
					Parent.BC_ForeignPortKCodeInfo.AddMessageError(ValidationConstants.Container.ForeignPortSchKIsRequired);
				}
				ListValidation.MessageErrorIfInvalidCode(Parent.BC_ForeignPortKCodeInfo);
			}
		}

		protected override void CheckBC_RL_NKForeignPort()
		{
			base.CheckBC_RL_NKForeignPort();
			if (IsInventoryRecordValidationMode)
			{
				ListValidation.WarnIfInvalidCode(Parent.BC_RL_NKForeignPortInfo);
			}
		}

		protected override void CheckBC_TypeOfService()
		{
			base.CheckBC_TypeOfService();
			if (IsInventoryRecordValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.BC_TypeOfServiceInfo);
			}
		}

		protected new CusInBondContainer Parent
		{
			get { return (CusInBondContainer)base.Parent; }
		}

		bool IsInventoryRecordValidationMode
		{
			get
			{
				var bill = Parent.Bill;
				return bill != null && bill.IsInventoryRecordValidationMode;
			}
		}
	}
}
