using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondContainerValidation : Customs.Business.CusInBondContainerValidation
	{
		public CusInBondContainerValidation(CusInBondContainer parent)
			: base(parent)
		{
		}

		protected SendingMessageValidationHelper Helper => new SendingMessageValidationHelper(Header, Parent, Parent.ShouldSend);

		public override void ValidateAll()
		{
			var helper = Helper;
			Parent.ClearRowNotifications();
			if (!helper.IsArrivalValidationMode && !helper.IsExportationValidationMode)
			{
				base.ValidateAll();
				using (((ISingleElementListInternal)Parent).SuspendListChanged())
				{
					ValidateAtLeastOneCommodityExist();
					ValidateUNDGsNotExceedLimit();
				}
			}
			else
			{
				helper.CheckPropertiesWhenSendingMessage();
			}
		}

		void ValidateUNDGsNotExceedLimit()
		{
			if (IsInBondLevelDepartureValidationMode && Parent.UNDGs.Count > 99)
			{
				Parent.AddRowMessageError(ValidationConstants.Container.MaximumNumberOfUNDGExceeded);
			}
		}

		void ValidateAtLeastOneCommodityExist()
		{
			var parent = Parent;
			if (IsInBondLevelDepartureValidationMode && parent.Commodities.Count == 0)
			{
				var header = Header;
				if (header.IsAMS || header.IsDocumentOnly)
				{
					parent.AddRowWarning(ValidationConstants.Container.AtLeastOneCommodityIsRequired);
				}
				else
				{
					parent.AddRowMessageError(ValidationConstants.Container.AtLeastOneCommodityIsRequired);
				}
			}
		}

		protected override void CheckBC_ContainerNum()
		{
			if (Helper.ShouldValidation)
			{
				base.CheckBC_ContainerNum();
				if (Parent.BC_ContainerNum.IsEmpty)
				{
					if ((Parent.IsDetailedInBond && IsInBondLevelDepartureValidationMode) || Helper.IsContainerArrivalValidationMode || Helper.IsContainerExportationValidationMode)
					{
						Parent.BC_ContainerNumInfo.AddMessageError(ValidationConstants.Container.ContainerNumberIsRequired);
					}
				}
				else
				{
					CheckContainerNumHasValidCharactersOnly();
					if (!Parent.BC_ContainerNumInfo.HasMessageErrors())
					{
						CheckContainerNumHasValidCheckDigit();
					}
					ValidateContainerNumberIsNotDuplicated();
				}
				ValidateBC_RC();
				ValidateBC_Seal1();
				ValidateBC_Seal2();
			}
		}

		void CheckContainerNumHasValidCharactersOnly()
		{
			if (Parent.BC_ContainerNum.KeepChars("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789") != Parent.BC_ContainerNum)
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
			CusInBondMoveDetail moveDetail = Parent.MoveDetail;
			if (moveDetail != null)
			{
				bool foundDuplicate = false;
				foreach (CusInBondContainer otherContainer in moveDetail.Containers)
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
			if (Parent.IsDetailedInBond && IsInBondLevelDepartureValidationMode && !Parent.IsNonContainerized)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BC_RCInfo);
				ListValidation.ErrorIfInvalidPK(Parent.BC_RCInfo);
			}
		}

		protected override void CheckBC_Seal1()
		{
			base.CheckBC_Seal1();
			if (Parent.IsDetailedInBond && IsInBondLevelDepartureValidationMode && !Parent.IsNonContainerized)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BC_Seal1Info);
			}

			if (Parent.BC_Seal1.Length > MaxLengthOfSealNumber)
			{
				Parent.BC_Seal1Info.AddMessageError(ValidationConstants.Container.MaxLengthOfSealExceeded);
			}
		}

		protected override void CheckBC_Seal2()
		{
			base.CheckBC_Seal2();

			if (Parent.BC_Seal2.Length > MaxLengthOfSealNumber)
			{
				Parent.BC_Seal2Info.AddMessageError(ValidationConstants.Container.MaxLengthOfSealExceeded);
			}
		}
		const int MaxLengthOfSealNumber = 15;

		protected new CusInBondContainer Parent
		{
			get { return (CusInBondContainer)base.Parent; }
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
	}
}
