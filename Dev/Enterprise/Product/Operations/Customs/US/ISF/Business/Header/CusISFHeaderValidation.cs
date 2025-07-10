using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.ISF.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.Business
{
	public class CusISFHeaderValidation : AutoCusISFHeaderValidation
	{
		public CusISFHeaderValidation(AutoCusISFHeader parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateBF_MasterBill();
				ValidateBF_OceanBill();
				ValidateBF_HouseBill();
				ValidateBF_SuretyCode();
				ValidateBF_EntryNumber();
				ValidateBF_BondReferenceNumber();
				ValidateBF_ImporterFullName();
			}
		}

		public void ValidateBF_EntryNumber()
		{
			ValidateCalculatedProperty(Parent.BF_EntryNumberInfo);
		}

		protected void CheckBF_EntryNumber()
		{
			if (!Parent.BF_EntryNumber.IsEmpty && !Regex.IsMatch(Parent.BF_EntryNumber, @"^[A-Z0-9]{3}[0-9]{8}$", RegexOptions.IgnoreCase))
			{
				Parent.BF_EntryNumberInfo.AddMessageError(ValidationConstants.Bill.CBPEntryNumberRightFormat);
			}
		}

		protected override void CheckBF_CustomsReference()
		{
			if (!Parent.BF_CustomsReference.IsEmpty)
			{
				if (!Parent.IsInDatabase || !Parent.BF_CustomsReferenceInfo.OriginalValue.Equals(Parent.BF_CustomsReference))
				{
					string warningMessage = ValidationConstants.Header.OnlyEnterCustomsReferenceIfModifyingJobFromAnotherSystem;
					Parent.BF_CustomsReferenceInfo.AddWarning(warningMessage);

					ZGuid companyPK = Parent.RegistryCompanyPK;

					ZString entryFilerCode = USCustomsDataRegistry.Instance.EntryFiler.GetValueWithoutFallback(companyPK.ToGuid(), Guid.Empty, Guid.Empty).EntryFilerCode;

					if (!entryFilerCode.IsEmpty && Parent.BF_CustomsReference.Left(3) != entryFilerCode)
					{
						string errorMessage = ValidationConstants.Header.CustomsReferenceFirst3CharsShouldBeSameAsEntryFilerCode(entryFilerCode);
						Parent.BF_CustomsReferenceInfo.AddError(errorMessage);
					}

					if (!new Regex("^\\w{3}-\\d{11}$").IsMatch(Parent.BF_CustomsReference))
					{
						string errorMessage = ValidationConstants.Header.CustomsReferenceFormat;
						Parent.BF_CustomsReferenceInfo.AddError(errorMessage);
					}
				}
			}
		}

		public void ValidateBF_HouseBill()
		{
			ValidateCalculatedProperty(Parent.BF_HouseBillInfo);
		}

		protected override void CheckBF_NumOfHarmChars()
		{
			switch (Parent.BF_NumOfHarmChars)
			{
				case NumberOfHarmonizedDigitsList.Codes.Six:
				case NumberOfHarmonizedDigitsList.Codes.Eight:
				case NumberOfHarmonizedDigitsList.Codes.Ten:
					break; // Do nothing
				default:
					Parent.BF_NumOfHarmCharsInfo.AddWarning(ValidationConstants.Header.NumberOfHarmonizedDigitsToReport);
					break;
			}
			if (Parent.Lines.Count == 0)
			{
				Parent.BF_NumOfHarmCharsInfo.AddMessageError(ValidationConstants.Header.AtLeastOneHarmonizedTariffScheduleIsEntered);
			}
		}

		protected void CheckBF_HouseBill()
		{
			CheckHouseBillOrOceanBillIsEntered(Parent.BF_HouseBillInfo);
			CharacterValidation.Validate(Parent.BF_HouseBillInfo, CharacterValidation.Type.AlphaNumeric);
			BillNumberFormatValidation.Validate(Parent.BF_HouseBillInfo, BillTypeList.Codes.HouseBillOfLading, Parent.Factory);
			BillDuplicationValidator.ValidateBillIsNotDuplicated(Parent.HouseBill, Parent.BF_HouseBillInfo);
			ValidateBF_MasterBill();
			ValidateBF_OceanBill();
		}

		void CheckHouseBillOrOceanBillIsEntered(ZPropertyInfo info)
		{
			if ((Parent.BF_HouseBill.IsEmpty && Parent.BF_OceanBill.IsEmpty)
				|| (!Parent.BF_HouseBill.IsEmpty && !Parent.BF_OceanBill.IsEmpty))
			{
				info.AddMessageError(ValidationConstants.Header.HouseOrOceanBillIsRequired);
			}
		}

		void CheckHouseBillAndMasterBillAreEntered(ZPropertyInfo info)
		{
			if (Parent.BF_HouseBill.IsEmpty && !Parent.BF_MasterBill.IsEmpty)
			{
				info.AddMessageError(ValidationConstants.Bill.OnlySpecifyMasterBillWhenThereIsHouseBill);
			}
		}

		public void ValidateBF_MasterBill()
		{
			ValidateCalculatedProperty(Parent.BF_MasterBillInfo);
		}

		protected void CheckBF_MasterBill()
		{
			CheckHouseBillAndMasterBillAreEntered(Parent.BF_MasterBillInfo);
			CharacterValidation.Validate(Parent.BF_MasterBillInfo, CharacterValidation.Type.AlphaNumeric);
			BillNumberFormatValidation.Validate(Parent.BF_MasterBillInfo, BillTypeList.Codes.MasterBillOfLading, Parent.Factory);
			BillDuplicationValidator.ValidateBillIsNotDuplicated(Parent.MasterBill, Parent.BF_MasterBillInfo);
			ValidateBF_HouseBill();
		}

		public void ValidateBF_OceanBill()
		{
			ValidateCalculatedProperty(Parent.BF_OceanBillInfo);
		}

		protected void CheckBF_OceanBill()
		{
			CheckHouseBillOrOceanBillIsEntered(Parent.BF_OceanBillInfo);
			CharacterValidation.Validate(Parent.BF_OceanBillInfo, CharacterValidation.Type.AlphaNumeric);
			BillNumberFormatValidation.Validate(Parent.BF_OceanBillInfo, BillTypeList.Codes.OceanBillOfLading, Parent.Factory);
			BillDuplicationValidator.ValidateBillIsNotDuplicated(Parent.OceanBill, Parent.BF_OceanBillInfo);
			ValidateBF_HouseBill();
		}

		protected override void CheckBF_SendEquipment()
		{
			base.CheckBF_SendEquipment();
			if (Parent.BF_SendEquipmentInfo.HasChanges)
			{
				foreach (CusISFEquip isfEquip in Parent.Equipments)
				{
					isfEquip.Validation.ValidateBE_ContainerNum();
				}
			}
		}

		public void ValidateBF_ImporterFullName()
		{
			ValidateCalculatedProperty(Parent.BF_ImporterFullNameInfo);
		}

		protected void CheckBF_ImporterFullName()
		{
			if (Parent.BF_ImporterFullName.IsEmpty && (Parent.BF_ImporterCodeType == ImporterCodeTypeList.Codes.Passport || Parent.BF_ImporterCodeType == ImporterCodeTypeList.Codes.SocialSecurity))
			{
				Parent.BF_ImporterFullNameInfo.AddMessageError(ValidationConstants.Header.ImporterFullNameIsRequiredForPassportOrSocialSecurityNumber);
			}
			if (Parent.BF_ImporterFullName.Length > ISFSF30.EntityNameMaxLength)
			{
				Parent.BF_ImporterFullNameInfo.AddWarning(string.Format("Importer Name is too long. Only the first {0} characters will be sent.", ISFSF30.EntityNameMaxLength));
			}
			ACEOceanManifestIllegalCharacters.WarnIllegalCharactersWereReplaced(Parent.BF_ImporterFullNameInfo);
			EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(Parent.BF_ImporterFullNameInfo);
		}

		protected override void CheckBF_ConsigneeFullName()
		{
			if (Parent.BF_ConsigneeFullName.IsEmpty && (Parent.BF_ConsigneeCodeType == ConsigneeCodeTypeList.Codes.Passport || Parent.BF_ConsigneeCodeType == ConsigneeCodeTypeList.Codes.SocialSecurity))
			{
				Parent.BF_ConsigneeFullNameInfo.AddMessageError(ValidationConstants.Header.ConsigneeFullNameIsRequiredForPassportOrSocialSecurityNumber);
			}
			ACEOceanManifestIllegalCharacters.WarnIllegalCharactersWereReplaced(Parent.BF_ConsigneeFullNameInfo);
			EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(Parent.BF_ConsigneeFullNameInfo);
		}

		protected override void CheckBF_ShipmentSubType()
		{
			if (Parent.BF_ShipmentType == ShipmentTypeList.Codes.Informal)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BF_ShipmentSubTypeInfo);
			}
		}

		protected override void CheckBF_EstimatedValue()
		{
			if (Parent.BF_ShipmentType == ShipmentTypeList.Codes.Informal && Parent.BF_EstimatedValue.IsEmpty)
			{
				Parent.BF_EstimatedValueInfo.AddMessageError(ValidationConstants.Header.EstimatedValueIsRequiredForInformalShipmentType);
			}
		}

		protected override void CheckBF_EstimatedQuantity()
		{
			if (Parent.BF_ShipmentType == ShipmentTypeList.Codes.Informal && Parent.BF_EstimatedQuantity.IsEmpty)
			{
				Parent.BF_EstimatedQuantityInfo.AddMessageError(ValidationConstants.Header.EstimatedQuantityIsRequiredForInformalShipmentType);
			}
		}

		protected override void CheckBF_EstimatedQuantityUQ()
		{
			if (Parent.BF_ShipmentType == ShipmentTypeList.Codes.Informal && Parent.BF_EstimatedQuantityUQ.IsEmpty)
			{
				Parent.BF_EstimatedQuantityUQInfo.AddMessageError(ValidationConstants.Header.EstimatedQuantityUQIsRequiredForInformalShipmentType);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.BF_EstimatedQuantityUQInfo);
		}

		protected override void CheckBF_EstimatedWeight()
		{
			if (Parent.BF_ShipmentType == ShipmentTypeList.Codes.Informal)
			{
				WeightValidator.ValidateWeight(Parent.EstimatedWeight, Parent.BF_EstimatedWeightInfo);
			}
			ValidateBF_EstimatedWeightUQ();
		}

		protected WeightValidator WeightValidator
		{
			get { return weightValidator ?? (weightValidator = new WeightValidator() { MaximumWholeWeightAllowed = MaximumWholeWeightAllowed }); }
		}
		WeightValidator weightValidator;
		const decimal MaximumWholeWeightAllowed = 99999999999m;

		protected override void CheckBF_EstimatedWeightUQ()
		{
			if (Parent.BF_ShipmentType == ShipmentTypeList.Codes.Informal)
			{
				WeightValidator.ValidateWeightUQ(Parent.EstimatedWeight, Parent.BF_EstimatedWeightUQInfo);

				if (Parent.BF_EstimatedWeightUQ.IsEmpty)
				{
					Parent.BF_EstimatedWeightUQInfo.AddMessageError(WeightUQRequiredForInformal);
				}
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.BF_EstimatedWeightUQInfo, Parent.Lookups.WeightUQList, (NoResString)ValidWeightUQRequired);
		}

		internal const string WeightUQRequiredForInformal = "Weight UQ; A Weight UQ is required when the Shipment Type is 'Informal'.";
		internal const string ValidWeightUQRequired = "Please select a valid Weight UQ.";

		public void ValidateBF_SuretyCode()
		{
			ValidateCalculatedProperty(Parent.BF_SuretyCodeInfo);
		}

		protected void CheckBF_SuretyCode()
		{
			if (Parent.BF_SuretyCode.IsEmpty)
			{
				if (Parent.BF_BondType == ImporterBondTypeList.Codes.SingleTransactionBond)
				{
					if (Parent.BF_BondActivityCode == ISFBondActivityCodeList.Codes.ISFBond16)
					{
						Parent.BF_SuretyCodeInfo.AddMessageError(ValidationConstants.Header.BondSuretyCodeIsRequiredForISFBondSingleTransaction);
					}
				}
			}
			else
			{
				CharacterValidation.Validate(Parent.BF_SuretyCodeInfo, CharacterValidation.Type.AlphaNumeric);
				if (Parent.BF_SuretyCode.Length != 3 || Parent.BF_SuretyCode.KeepNumericCharacters() != Parent.BF_SuretyCode)
				{
					Parent.BF_SuretyCodeInfo.AddMessageError(ValidationConstants.Bill.SuretyCodeRightFormat);
				}

				if (Parent.BF_BondType != BondTypeList.Codes.SingleTransactionBond || Parent.BF_BondActivityCode != ISFBondActivityCodeList.Codes.ISFBond16)
				{
					Parent.BF_SuretyCodeInfo.AddMessageError(ValidationConstants.Bill.SuretyCodeIsOnlyRequired);
				}
				else if (!Parent.IsBondDataRequired)
				{
					Parent.BF_SuretyCodeInfo.AddWarning(ValidationConstants.Bill.SuretyCodeIsOnlyRequiredForShipmentTypes);
				}
			}
			ValidateBF_BondReferenceNumber();
		}

		public void ValidateBF_BondReferenceNumber()
		{
			ValidateCalculatedProperty(Parent.BF_BondReferenceNumberInfo);
		}

		protected void CheckBF_BondReferenceNumber()
		{
			if (Parent.BF_BondReferenceNumber.IsEmpty)
			{
				if (Parent.IsBond16SingleTransaction)
				{
					Parent.BF_BondReferenceNumberInfo.AddMessageError(ValidationConstants.Bill.BondReferenceNumberIsRequired);
				}
			}
			else
			{
				if (!Parent.IsBond16SingleTransaction)
				{
					Parent.BF_BondReferenceNumberInfo.AddMessageError(ValidationConstants.Bill.BondReferenceNumberIsOnlyRequired);
				}
				else if (!Parent.IsBondDataRequired)
				{
					Parent.BF_BondReferenceNumberInfo.AddWarning(ValidationConstants.Bill.BondReferenceNumberIsOnlyRequiredForShipmentTypes);
				}

				if (Parent.BF_BondType == ImporterBondTypeList.Codes.SingleTransactionBond && !Parent.BF_SuretyCode.IsEmpty)
				{
					BillDuplicationValidator.ValidateBillIsNotDuplicated(Parent.BondReferenceNumber, Parent.BF_BondReferenceNumberInfo);
				}
			}
		}

		protected new CusISFHeader Parent
		{
			get { return (CusISFHeader)base.Parent; }
		}

		protected override void CheckBF_TransportMode()
		{
			base.CheckBF_TransportMode();
			ListValidation.MessageErrorIfInvalidCode(Parent.BF_TransportModeInfo);
		}

		protected override void CheckBF_EntryType()
		{
			base.CheckBF_EntryType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BF_EntryTypeInfo);
			if ((Parent.BF_EntryType == SubmissionTypeList.Codes.LateISF10 || Parent.BF_EntryType == SubmissionTypeList.Codes.LateISF5) && ZDateTime.Today >= new ZDateTime(2015, 1, 10))
			{
				Parent.BF_EntryTypeInfo.AddMessageError(InvalidLateType);
			}

			ValidateBF_ShipmentType();
		}

		internal const string InvalidLateType = "Late ISF submissions are no longer accepted (effective as of January 10, 2015).";

		protected override void CheckBF_ShipmentType()
		{
			base.CheckBF_ShipmentType();

			ListValidation.MessageErrorIfInvalidCode(Parent.BF_ShipmentTypeInfo);
			if (Parent.IsISF10Entry)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BF_ShipmentTypeInfo);
				if (Parent.BF_ShipmentType == ShipmentTypeList.Codes.Carnet)
				{
					ValidateAtLeastOneCarnetReferenceIsEntered();
				}
			}
			else if (Parent.IsISF5Entry && Parent.BF_ShipmentType != ShipmentTypeList.Codes.StandardOrRegularFilings)
			{
				Parent.BF_ShipmentTypeInfo.AddMessageError(ValidationConstants.Header.StandardOrRegularFilingsIsRequiredWhenISF5);
			}

			ValidateBF_BondActivityCode();
			ValidateBF_BondNumberOrHolder();
			ValidateBF_BondReferenceNumber();
			ValidateBF_BondType();
			ValidateBF_SuretyCode();
			ValidateBF_ImporterCodeType();
			ValidateBF_ConsigneeCodeType();
			ValidateBF_ShipmentSubType();
			ValidateBF_EstimatedValue();
			ValidateBF_EstimatedQuantity();
			ValidateBF_EstimatedQuantityUQ();
			ValidateBF_EstimatedWeight();
			ValidateBF_EstimatedWeightUQ();
		}

		void ValidateAtLeastOneCarnetReferenceIsEntered()
		{
			if (Parent.ReferenceDatas.GetFirstMatchingType(BillTypeList.Codes.CarnetIssuingCountryCodeAndCarnetNumber) == null)
			{
				Parent.BF_ShipmentTypeInfo.AddMessageError(ValidationConstants.Bill.CarnetReferenceIsRequired);
			}
		}

		protected override void CheckBF_ImporterCodeType()
		{
			base.CheckBF_ImporterCodeType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BF_ImporterCodeTypeInfo);

			if (Parent.BF_ImporterCodeType == ImporterCodeTypeList.Codes.Passport && !ShipmentTypeList.IsPassportAllowed(Parent.BF_ShipmentType))
			{
				Parent.BF_ImporterCodeTypeInfo.AddMessageError(ValidationConstants.Header.PassportIsOnlyUsedFor03Or05Or06ShipmentType);
			}

			ValidateBF_ImporterCode();
			ValidateBF_DateOfBirth();
			ValidateBF_CountryOfIssue();
			ValidateBF_ImporterFullName();
		}

		protected override void CheckBF_ImporterCode()
		{
			base.CheckBF_ImporterCode();
			if (Parent.BF_ImporterCode.IsEmpty)
			{
				if (Parent.Lookups.ImporterCodeTypes.ContainsCode(Parent.BF_ImporterCodeType))
				{
					Parent.BF_ImporterCodeInfo.AddMessageError(ValidationConstants.Header.ValueIsRequiredForEntity(Parent.Lookups.ImporterCodeTypes.GetDescriptionFromCode(Parent.BF_ImporterCodeType)));
				}
			}
			else
			{
				if (Parent.BF_OH_Importer.IsEmpty)
				{
					JobRequiredDocument poa = Parent.RequiredDocuments.GetDocByType(Core.Constants.RefDocTypes.PowerOfAttorney) ?? Parent.RequiredDocuments.GetDocByType(Core.Constants.RefDocTypes.PowerOfAttorneyCustoms);

					if (poa != null)
					{
						new PowerOfAttorneyValidator().ValidatePowerOfAttorneyDocumentDates(Parent.BF_ImporterCodeInfo, poa, "ISF");
					}
				}
				ValidateEntityIdentifierNumber(Parent.BF_ImporterCodeInfo, Parent.BF_ImporterCodeType);
			}
			ValidateBF_ImporterCodeType();
		}

		void ValidateEntityIdentifierNumber(ZPropertyInfo info, ZString codeType)
		{
			if (codeType == CodeTypeList.Codes.SCAC)
			{
				ListValidation.WarnIfInvalidCode(info, Parent.Lookups.USCarrierList);
			}
			else
			{
				EntityIdentifierNumberValidation.Validate(info, codeType);
				ZString number = (ZString)info.Value;
				if (!number.IsEmpty && !info.HasMessageErrors())
				{
					if (codeType == CodeTypeList.Codes.CBPAssignedNumber)
					{
						ValidateIRSOrCBPAssignedNumberIsOnCustomsFile(info, codeType, number);
					}
					else if (codeType == CodeTypeList.Codes.IRS)
					{
						ValidateIRSOrCBPAssignedNumberIsOnCustomsFile(info, codeType, number);
					}
				}
			}
		}

		void ValidateIRSOrCBPAssignedNumberIsOnCustomsFile(ZPropertyInfo info, ZString codeType, ZString number)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

			ZDBOnlySubQuery orgRegoQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
			orgRegoQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, codeType);
			orgRegoQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, number);
			query.AddSubQuery(orgRegoQuery, JoinCondition.And);
			OrgHeader org = Parent.Factory.LoadTop1<OrgHeader>(query);
			if (org == null)
			{
				info.AddWarning(ValidationConstants.Header.EntityMightNotBeRegisteredInCustoms(codeType));
			}
			else
			{
				OrganisationValidation.ValidateOrganisationRegisteredInCustoms(info, org, ValidationConstants.Header.OrganisationShouldBeRegisteredInCustoms(codeType, org.OH_Code));
			}
		}

		protected override void CheckBF_OH_Importer()
		{
			base.CheckBF_OH_Importer();
			if (!Parent.BF_OH_Importer.IsEmpty)
			{
				new PowerOfAttorneyValidator().Validate(Parent, Parent.Importer, Parent.BF_OH_ImporterInfo);
				OrgHeader org = Parent.Importer;
				if (Parent.BF_OH_Importer == OrgHeader.UnmatchedOrganisationPK)
				{
					Parent.BF_OH_ImporterInfo.AddMessageError(ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
				}
			}
			ValidateBF_ImporterCode();
		}

		protected override void CheckBF_DateOfBirthIsValidZDateTimeRange()
		{
		}

		protected override void CheckBF_DateOfBirth()
		{
			base.CheckBF_DateOfBirth();

			if (Parent.BF_DateOfBirth.IsEmpty)
			{
				if ((Parent.BF_ImporterCodeType == ImporterCodeTypeList.Codes.SocialSecurity ||
					Parent.BF_ImporterCodeType == ImporterCodeTypeList.Codes.Passport) && ShipmentTypeList.IsDOBRequired(Parent.BF_ShipmentType))
				{
					Parent.BF_DateOfBirthInfo.AddMessageError(ValidationConstants.Header.DateOfBirthIsRequired);
				}
			}

			if (Parent.BF_DateOfBirth > ZDateTime.Today)
			{
				Parent.BF_DateOfBirthInfo.AddMessageError(ValidationConstants.Header.DateOfBirthIsInvalid);
			}
		}

		protected override void CheckBF_CountryOfIssue()
		{
			base.CheckBF_CountryOfIssue();

			ListValidation.MessageErrorIfInvalidCode(Parent.BF_CountryOfIssueInfo);

			if (Parent.BF_ImporterCodeType == ImporterCodeTypeList.Codes.Passport && Parent.BF_CountryOfIssue.IsEmpty)
			{
				Parent.BF_CountryOfIssueInfo.AddMessageError(ValidationConstants.Header.CountryOfIssueIsRequiredForPassport);
			}
		}

		protected override void CheckBF_ConsigneeCode()
		{
			base.CheckBF_ConsigneeCode();
			if (Parent.IsISF10Entry)
			{
				if (Parent.BF_ConsigneeCode.IsEmpty)
				{
					if (Parent.Lookups.ConsigneeCodeTypes.ContainsCode(Parent.BF_ConsigneeCodeType))
					{
						Parent.BF_ConsigneeCodeInfo.AddMessageError(ValidationConstants.Header.ValueIsRequiredForEntity(Parent.Lookups.ConsigneeCodeTypes.GetDescriptionFromCode(Parent.BF_ConsigneeCodeType)));
					}
				}
				else
				{
					ValidateEntityIdentifierNumber(Parent.BF_ConsigneeCodeInfo, Parent.BF_ConsigneeCodeType);
				}
			}
			ValidateBF_ConsigneeCodeType();
		}

		protected override void CheckBF_ConsigneeCodeType()
		{
			base.CheckBF_ConsigneeCodeType();
			ListValidation.MessageErrorIfInvalidCode(Parent.BF_ConsigneeCodeTypeInfo);
			if (Parent.BF_ConsigneeCodeType == ConsigneeCodeTypeList.Codes.Passport && !ShipmentTypeList.IsPassportAllowed(Parent.BF_ShipmentType))
			{
				Parent.BF_ConsigneeCodeTypeInfo.AddMessageError(ValidationConstants.Header.PassportIsOnlyUsedFor03Or05Or06ShipmentType);
			}
			else if (!Parent.BF_ConsigneeCode.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BF_ConsigneeCodeTypeInfo, "Consignee ID Type; a Consignee ID Type is required when the Cosignee ID is entered.");
			}

			if (Parent.BF_ConsigneeCodeType == ConsigneeCodeTypeList.Codes.CBPAssignedNumber && Parent.BF_ImporterCodeType == ConsigneeCodeTypeList.Codes.CBPAssignedNumber)
			{
				Parent.BF_ConsigneeCodeTypeInfo.AddWarning(ValidationConstants.Header.ForeignBasedConsigneeWithForeignBasedImporter);
			}

			ValidateBF_ConsigneeCountryOfIssue();
			ValidateBF_ConsigneeDateOfBirth();
			ValidateBF_ConsigneeCode();
			ValidateBF_ConsigneeFullName();
		}

		protected override void CheckBF_ConsigneeDateOfBirthIsValidZDateTimeRange()
		{
		}

		protected override void CheckBF_ConsigneeDateOfBirth()
		{
			base.CheckBF_ConsigneeDateOfBirth();

			if (Parent.BF_ConsigneeDateOfBirth.IsEmpty)
			{
				if ((Parent.BF_ConsigneeCodeType == ConsigneeCodeTypeList.Codes.Passport ||
					Parent.BF_ConsigneeCodeType == ConsigneeCodeTypeList.Codes.SocialSecurity) && ShipmentTypeList.IsDOBRequired(Parent.BF_ShipmentType))
				{
					Parent.BF_ConsigneeDateOfBirthInfo.AddMessageError(ValidationConstants.Header.ConsigneeDateOfBirthIsRequired);
				}
			}

			if (Parent.BF_ConsigneeDateOfBirth > ZDateTime.Today)
			{
				Parent.BF_ConsigneeDateOfBirthInfo.AddMessageError(ValidationConstants.Header.DateOfBirthIsInvalid);
			}
		}

		protected override void CheckBF_ConsigneeCountryOfIssue()
		{
			base.CheckBF_ConsigneeCountryOfIssue();

			ListValidation.MessageErrorIfInvalidCode(Parent.BF_ConsigneeCountryOfIssueInfo);

			if (Parent.BF_ConsigneeCodeType == ConsigneeCodeTypeList.Codes.Passport && Parent.BF_ConsigneeCountryOfIssue.IsEmpty)
			{
				Parent.BF_ConsigneeCountryOfIssueInfo.AddMessageError(ValidationConstants.Header.ConsigneeCountryOfIssueIsRequiredForPassport);
			}
		}

		protected override void CheckBF_RL_NKPlaceOfDelivery()
		{
			base.CheckBF_RL_NKPlaceOfDelivery();
			if (Parent.IsISF5Entry)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BF_RL_NKPlaceOfDeliveryInfo);
			}
		}

		protected override void CheckBF_RL_NKPortOfUnload()
		{
			base.CheckBF_RL_NKPortOfUnload();
			if (Parent.IsISF5Entry)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BF_RL_NKPortOfUnloadInfo);
			}
		}

		protected override void CheckBF_BondNumberOrHolder()
		{
			base.CheckBF_BondNumberOrHolder();

			if (Parent.BF_BondNumberOrHolder.IsEmpty)
			{
				if (Parent.IsBondDataRequired)
				{
					Parent.BF_BondNumberOrHolderInfo.AddMessageError(ValidationConstants.Header.BondDetailIsRequiredForShipmentTypes(Parent.BF_BondNumberOrHolderInfo.HumanReadableName));
				}
			}
			else
			{
				if (!(EmployerIdentificationNumberValidator.IsValidEIN(Parent.BF_BondNumberOrHolder) || CBPAssignedNumberValidator.IsValidCBPAssignedNumber(Parent.BF_BondNumberOrHolder) || SocialSecurityNumberValidator.IsValidSSN(Parent.BF_BondNumberOrHolder)))
				{
					Parent.BF_BondNumberOrHolderInfo.AddMessageError(ValidationConstants.Header.BondHolderIsInvalid);
				}
				else if (!Parent.IsBondDataRequired)
				{
					Parent.BF_BondNumberOrHolderInfo.AddWarning(ValidationConstants.Header.BondDetailIsOnlyRequiredForShipmentTypes(Parent.BF_BondNumberOrHolderInfo.HumanReadableName));
				}
			}
		}

		protected override void CheckBF_LineMergeStyle()
		{
			base.CheckBF_LineMergeStyle();
			if (Parent.BF_LineMergeStyle.IsEmpty)
			{
				Parent.BF_LineMergeStyleInfo.AddWarning(ValidationConstants.Header.LineMergeStyleNotSpecified);
			}
			else
			{
				ListValidation.WarnIfInvalidCode(Parent.BF_LineMergeStyleInfo, Parent.Lookups.MergeStyleList, ValidationConstants.Header.LineMergeStyleIsInvalid);
			}
		}

		protected override void CheckBF_ActionReasonCode()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BF_ActionReasonCodeInfo);
		}

		protected override void CheckBF_BondActivityCode()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.BF_BondActivityCodeInfo);

			if (Parent.BF_BondActivityCode.IsEmpty)
			{
				if (Parent.IsBondDataRequired)
				{
					Parent.BF_BondActivityCodeInfo.AddMessageError(ValidationConstants.Header.BondDetailIsRequiredForShipmentTypes(Parent.BF_BondActivityCodeInfo.HumanReadableName));
				}
			}
			else
			{
				if (!Parent.IsBondDataRequired)
				{
					Parent.BF_BondActivityCodeInfo.AddWarning(ValidationConstants.Header.BondDetailIsOnlyRequiredForShipmentTypes(Parent.BF_BondActivityCodeInfo.HumanReadableName));
				}
			}

			ValidateBF_SuretyCode();
			ValidateBF_BondType();
			ValidateBF_BondReferenceNumber();
		}

		protected override void CheckBF_BondType()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.BF_BondTypeInfo);
			if (Parent.BF_BondType.IsEmpty)
			{
				if (Parent.IsBondDataRequired)
				{
					Parent.BF_BondTypeInfo.AddMessageError(ValidationConstants.Header.BondDetailIsRequiredForShipmentTypes(Parent.BF_BondTypeInfo.HumanReadableName));
				}
			}
			else
			{
				if (Parent.BF_BondType == BondTypeList.Codes.SingleTransactionBond && Parent.BF_BondActivityCode != ISFBondActivityCodeList.Codes.ISFBond16)
				{
					Parent.BF_BondTypeInfo.AddMessageError(ValidationConstants.Header.BondType9MayOnlyUsedWithBondActivityCode16);
				}
				else if (!Parent.IsBondDataRequired)
				{
					Parent.BF_BondTypeInfo.AddWarning(ValidationConstants.Header.BondDetailIsOnlyRequiredForShipmentTypes(Parent.BF_BondTypeInfo.HumanReadableName));
				}
			}

			if (!Parent.BF_BondTypeInfo.HasNotifications())
			{
				CheckIfBondTypeIsOnTheFile();
			}

			ValidateBF_SuretyCode();
			ValidateBF_BondReferenceNumber();
		}

		void CheckIfBondTypeIsOnTheFile()
		{
			var activityCode = ConvertISFBondActivityCodeToCommonActivityCode(Parent.BF_BondActivityCode);
			if (!activityCode.IsEmpty && !Parent.BF_BondType.IsEmpty)
			{
				if (Parent.Importer != null)
				{
					var importer = OrgHeaderWrapper.New(Parent.Importer);
					if (importer.BondDetails.Count > 0)
					{
						var bondDetail = importer.BondDetails.GetActiveBondDetailDataFor(new List<ZString>(new[] { activityCode }), Parent.BF_BondType, ZDateTime.Today);
						if (bondDetail == null)
						{
							Parent.BF_BondTypeInfo.AddWarning(ValidationConstants.Header.NoValidBondOnFile(Parent.BF_BondActivityCode, Parent.BF_BondType, ZDateTime.Today));
						}
					}
				}
			}
		}

		internal static ZString ConvertISFBondActivityCodeToCommonActivityCode(ZString iSFBondActivityCode)
		{
			switch (iSFBondActivityCode)
			{
				case ISFBondActivityCodeList.Codes.ImporterOrBroker:
					return ActivityCodeList.Codes._1;
				case ISFBondActivityCodeList.Codes.CustodianOfBondedMerchandise:
					return ActivityCodeList.Codes._2;
				case ISFBondActivityCodeList.Codes.InternationalCarrier:
					return ActivityCodeList.Codes._3;
				case ISFBondActivityCodeList.Codes.ForeignTradeZoneOperator:
					return ActivityCodeList.Codes._4;
				case ISFBondActivityCodeList.Codes.ISFBond16:
					return ActivityCodeList.Codes._16;
				default:
					return ZString.Empty;
			}
		}

		protected override void CheckBF_SCAC()
		{
			base.CheckBF_SCAC();
			ListValidation.WarnIfInvalidCode(Parent.BF_SCACInfo);
			CharacterValidation.Validate(Parent.BF_SCACInfo, CharacterValidation.Type.AlphaOnly);

			if (!Parent.BF_SCAC.IsEmpty && Parent.BF_SCAC.Length != 4)
			{
				Parent.BF_SCACInfo.AddMessageError(InvalidSCACFormat);
			}
		}
		internal const string InvalidSCACFormat = "The Standard Carrier Alpha Code (SCAC) should be 4 characters in length.";

		protected override void CheckBF_GB()
		{
			base.CheckBF_GB();
			ListValidation.ErrorIfInvalidPK(Parent.BF_GBInfo, Parent.Lookups.Branches);

			var restrictJob = ISFRegistry.Instance.ISFUSRestrictISFJobs.GetFallBackValueAtAllLevels(Parent.RegistryCompanyPK, Parent.RegistryBranchPK, Guid.Empty);
			if (restrictJob && GlbStaff.CurrentUser.GS_GB_HomeBranch != Parent.BF_GB)
			{
				Parent.BF_GBInfo.AddError(InvalidateBranchForISF);
			}
		}
		internal const string InvalidateBranchForISF = "Branch belongs to another company.  Branch should belong to the home company according to the registry setting: Registry -> United States Customs -> ABI -> Importer Security Filing ->  Restrict ISF jobs to the Home Branch.";
	}
}
