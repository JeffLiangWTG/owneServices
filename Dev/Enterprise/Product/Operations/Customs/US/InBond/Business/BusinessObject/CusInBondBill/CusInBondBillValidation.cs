using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondBillValidation : Customs.Business.CusInBondBillValidation
	{
		public CusInBondBillValidation(CusInBondBill parent)
			: base(parent)
		{
			isAMSHBREffective = US.Business.ZZCustomsFunctionality.IsAMSHBREffective;
		}
		readonly bool isAMSHBREffective;

		protected SendingMessageValidationHelper Helper => new SendingMessageValidationHelper(Header, Parent, Parent.ShouldSend);

		public override void ValidateAll()
		{
			var helper = Helper;
			if (!helper.IsArrivalValidationMode && !helper.IsExportationValidationMode)
			{
				base.ValidateAll();
				using (((ISingleElementListInternal)Parent).SuspendListChanged())
				{
					ValidateB0_PlaceOfReceiptDCode();
				}
			}
			else
			{
				helper.CheckPropertiesWhenSendingMessage();
			}
		}

		public void ValidateB0_PlaceOfReceiptDCode()
		{
			ValidateCalculatedProperty(Parent.B0_PlaceOfReceiptDCodeInfo);
		}

		protected void CheckB0_PlaceOfReceiptDCode()
		{
			if (IsDocumentOnly)
			{
				ListValidation.WarnIfInvalidCode(Parent.B0_PlaceOfReceiptDCodeInfo);
			}
			else if (!IsAirValidationModes)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.B0_PlaceOfReceiptDCodeInfo);
			}
		}

		protected override void CheckB0_PortOfLadingKCode()
		{
			base.CheckB0_PortOfLadingKCode();
			if (IsInBondLevelDepartureValidationMode)
			{
				var header = Parent.Header;
				if (header != null && header.IsDetailedInBond && !header.IsAir)
				{
					if (!header.BH_FTZMove)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.B0_PortOfLadingKCodeInfo);
					}
					else if (Parent.B0_PortOfLadingKCode != CusInBondBill.FTZForeignPortOfLading)
					{
						Parent.B0_PortOfLadingKCodeInfo.AddMessageError(ValidationConstants.Bill.FTZForeignPortOfLading);
					}
				}
			}
			if (IsDocumentOnly)
			{
				ListValidation.WarnIfInvalidCode(Parent.B0_PortOfLadingKCodeInfo);
			}
			else if (!IsAirValidationModes)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.B0_PortOfLadingKCodeInfo);
			}
		}

		protected override void CheckB0_ManifestQty()
		{
			base.CheckB0_ManifestQty();
			if (!IsAirValidationModes && Parent.B0_ManifestQty < 1 && Parent.IsDetailedInBond)
			{
				Parent.B0_ManifestQtyInfo.AddMessageError(ValidationConstants.Bill.ManifestQuantityMustBeGreaterThanZero);
			}
		}

		protected override void CheckB0_ManifestUQ()
		{
			if (!IsAirValidationModes && !IsDiversionRequestMode)
			{
				base.CheckB0_ManifestUQ();
				if (Parent.B0_ManifestUQ.IsEmpty)
				{
					if (Parent.IsDetailedInBond)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.B0_ManifestUQInfo);
					}
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.B0_ManifestUQInfo);
				}
			}
		}

		protected override void CheckB0_Weight()
		{
			base.CheckB0_Weight();
			if (Parent.IsDetailedInBond && IsInBondLevelDepartureValidationMode)
			{
				WeightValidator.ValidateWeight(Parent.Weight, Parent.B0_WeightInfo);
			}
			ValidateB0_WeightUQ();
		}

		protected override void CheckB0_WeightUQ()
		{
			if (!IsAirValidationModes && !IsDiversionRequestMode)
			{
				base.CheckB0_WeightUQ();
				if (Parent.IsDetailedInBond && IsInBondLevelDepartureValidationMode)
				{
					WeightValidator.ValidateWeightUQ(Parent.Weight, Parent.B0_WeightUQInfo);
				}
				ListValidation.MessageErrorIfInvalidCode(Parent.B0_WeightUQInfo);
			}
		}

		protected WeightValidator WeightValidator
		{
			get { return weightValidator ?? (weightValidator = new WeightValidator()); }
		}
		WeightValidator weightValidator;

		protected override void CheckB0_Volume()
		{
			base.CheckB0_Volume();
			if (Parent.IsDetailedInBond && IsInBondLevelDepartureValidationMode)
			{
				VolumeValidator.ValidateVolume(Parent.Volume, Parent.B0_VolumeInfo);
			}
			ValidateB0_VolumeUQ();
		}

		protected override void CheckB0_VolumeUQ()
		{
			if (!IsAirValidationModes && !IsDiversionRequestMode)
			{
				base.CheckB0_VolumeUQ();
				if (Parent.IsDetailedInBond && IsInBondLevelDepartureValidationMode)
				{
					VolumeValidator.ValidateVolumeUQ(Parent.Volume, Parent.B0_VolumeUQInfo);
				}
				ListValidation.MessageErrorIfInvalidCode(Parent.B0_VolumeUQInfo);
			}
		}

		protected VolumeValidator VolumeValidator
		{
			get { return volumeValidator ?? (volumeValidator = new VolumeValidator()); }
		}
		VolumeValidator volumeValidator;

		protected override void CheckB0_IssuerCode()
		{
			if (Helper.ShouldValidation)
			{
				base.CheckB0_IssuerCode();
				var header = Parent.Header;

				if (header != null && header.BH_FTZMove && Parent.IsIssuerCodeComesFromFrimCode && Parent.B0_IssuerCode != header.BH_FIRMS)
				{
					Parent.B0_IssuerCodeInfo.AddMessageError(ValidationConstants.Bill.IssuerCodeShouldBeTheSameAsFirms);
				}
				else
				{
					if (Parent.B0_IssuerCode.IsEmpty)
					{
						if (header != null && (IsInBondLevelDepartureValidationMode || IsAirValidationModes || Helper.IsBillOfLadingArrivalValidationMode || Helper.IsContainerArrivalValidationMode || Helper.IsBillOfLadingExportationValidationMode || Helper.IsContainerExportationValidationMode) && !header.BH_FTZMove)
						{
							MandatoryValidation.MessageErrorIfNotEntered(Parent.B0_IssuerCodeInfo, "Master Issuer Code");
						}
					}
					else
					{
						if (IsDocumentOnly)
						{
							ListValidation.WarnIfInvalidCode(Parent.B0_IssuerCodeInfo);
						}
						else if (!IsAirValidationModes && !IsDiversionRequestMode)
						{
							ListValidation.MessageErrorIfInvalidCode(Parent.B0_IssuerCodeInfo);
						}
					}
				}
			}
		}

		protected override void CheckB0_MasterBillNumber()
		{
			if (Helper.ShouldValidation)
			{
				base.CheckB0_MasterBillNumber();

				if (IsInBondLevelDepartureValidationMode
					|| IsBillOfLadingDeleteModes
					|| IsAirInBondInitiationAndDeletionValidationMode
					|| Helper.IsBillOfLadingArrivalValidationMode
					|| Helper.IsContainerArrivalValidationMode
					|| Helper.IsBillOfLadingExportationValidationMode
					|| Helper.IsContainerExportationValidationMode)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.B0_MasterBillNumberInfo);
					ValidateBillUniqueCodeIsNotDuplicated(Parent.B0_MasterBillNumberInfo, (IsAirValidationModes || (isAMSHBREffective && IsSeaValidationMode)) ? ValidationConstants.Bill.BillUniqueCodeIsDuplicated : Parent.IsFTZMove ? ValidationConstants.Bill.OriginalEntryAdmissionNumberIsDuplicated : ValidationConstants.Bill.BillNumberIsDuplicated);
					ValidateBillNumberContainsSpecialCharacters(Parent.B0_MasterBillNumberInfo);
					if (IsAirInBondInitiationAndDeletionValidationMode)
					{
						new CusInBondAirWayBillValidator(Parent).ValidateAndAddMessageError(Parent.B0_MasterBillNumberInfo);
					}
					else
					{
						ValidateBillNumberLengthExceed(Parent.B0_MasterBillNumberInfo);
					}
				}
			}
		}

		protected override void CheckB0_HouseBillNumber()
		{
			base.CheckB0_HouseBillNumber();

			if (IsInBondLevelDepartureValidationMode || IsAirValidationModes || IsBillOfLadingDeleteModes)
			{
				ValidateBillNumberLengthExceed(Parent.B0_HouseBillNumberInfo);
				ValidateBillNumberContainsSpecialCharacters(Parent.B0_HouseBillNumberInfo);

				if (IsAirValidationModes)
				{
					ValidateBillUniqueCodeIsNotDuplicated(Parent.B0_HouseBillNumberInfo, ValidationConstants.Bill.BillUniqueCodeIsDuplicated);
				}
			}

			if (isAMSHBREffective && Parent.IsSea)
			{
				ValidateBillUniqueCodeIsNotDuplicated(Parent.B0_HouseBillNumberInfo, ValidationConstants.Bill.BillUniqueCodeIsDuplicated);
				if (!Parent.B0_HouseBillIssuerCode.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.B0_HouseBillNumberInfo);
				}
			}
		}

		protected override void CheckB0_HouseBillIssuerCode()
		{
			base.CheckB0_HouseBillIssuerCode();

			if (isAMSHBREffective)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.B0_HouseBillIssuerCodeInfo);

				if (Parent.IsSea && !Parent.B0_HouseBillNumber.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.B0_HouseBillIssuerCodeInfo);
				}
			}
		}

		void ValidateBillNumberLengthExceed(ZPropertyInfo billNumberInfo)
		{
			var billNumber = (ZString)billNumberInfo.Value;

			if (!billNumber.IsEmpty && billNumber.Length > 12)
			{
				billNumberInfo.AddMessageError(ValidationConstants.Bill.BillNumberLengthExceeded);
			}
		}

		void ValidateBillNumberContainsSpecialCharacters(ZPropertyInfo billNumberInfo)
		{
			var billNumber = (ZString)billNumberInfo.Value;
			var header = Parent.Header;
			if (header != null && !header.IsAir && header.BH_FTZMove)
			{
				if (billNumber.Length > billNumber.KeepChars(OriginalEntryAdmissionNumberValidCharacters).Length)
				{
					billNumberInfo.AddMessageError(ValidationConstants.Bill.OrignalEntryAdmissionNumberContainsSpecialCharacters);
				}
			}
			else
			{
				if (billNumber.Length > billNumber.KeepAlphanumericCharacters().Length)
				{
					billNumberInfo.AddMessageError(ValidationConstants.Bill.BillNumberContainsSpecialCharacters);
				}
			}
		}
		public const string OriginalEntryAdmissionNumberValidCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-";

		protected void ValidateBillUniqueCodeIsNotDuplicated(ZPropertyInfo propertyInfo, string messageError)
		{
			if (!Parent.BillUniqueCode.IsEmpty)
			{
				var header = Parent.Header;
				if (header != null)
				{
					var foundDuplicate = false;
					foreach (var otherBill in header.Bills)
					{
						if (otherBill.PK != Parent.PK && otherBill.BillUniqueCode == Parent.BillUniqueCode)
						{
							foundDuplicate = true;
							break;
						}
					}

					if (foundDuplicate)
					{
						propertyInfo.AddMessageError(messageError);
					}
				}
			}
		}

		protected new CusInBondBill Parent
		{
			get { return (CusInBondBill)base.Parent; }
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

		bool IsAirInBondInitiationAndDeletionValidationMode
		{
			get
			{
				var header = Header;
				return header != null && header.IsAirInBondInitiationAndDeletionMode;
			}
		}

		bool IsAirValidationModes
		{
			get
			{
				var header = Header;
				return header != null && header.IsAir;
			}
		}

		bool IsSeaValidationMode
		{
			get
			{
				var header = Header;
				return header != null && header.IsSea;
			}
		}

		bool IsBillOfLadingDeleteModes
		{
			get
			{
				var header = Header;
				return header != null && header.IsBillOfLadingDeleteMode;
			}
		}

		bool IsDiversionRequestMode
		{
			get
			{
				var header = Header;
				return header != null && header.IsDiversionRequestMode;
			}
		}

		bool IsDocumentOnly
		{
			get
			{
				var header = Header;
				return header != null && header.IsDocumentOnly;
			}
		}
	}
}
