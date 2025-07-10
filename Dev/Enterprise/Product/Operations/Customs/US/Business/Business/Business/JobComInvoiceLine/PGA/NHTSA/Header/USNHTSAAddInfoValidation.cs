//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSNHTSAAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSNHTSAAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using BoxNumberList = Enterprise.Customs.US.Business.DepartmentOfTransportBoxNumberList;

namespace Enterprise.Customs.US.Business
{
	public class USNHTSAAddInfoValidation : AutoUSNHTSAAddInfoValidation
	{
		public USNHTSAAddInfoValidation(AutoUSNHTSAAddInfo parent) : base(parent)
		{
		}

		new USNHTSAAddInfo Parent
		{
			get { return (USNHTSAAddInfo)base.Parent; }
		}

		NHTSAHeader Header
		{
			get { return Parent.Parent as NHTSAHeader; }
		}

		protected override void CheckUS_NHTProgramCode()
		{
			base.CheckUS_NHTProgramCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_NHTProgramCodeInfo, Parent.Lookups.AgencyProgramCodes);

			if (Header.IsPGAValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NHTProgramCodeInfo);

				if (Header.US_NHTBoxNumber == BoxNumberList.Codes._2A && !Header.IsMotorVehicles && Header.US_NHTProgramCode != NHTSAProgramCodeList.Codes.REI)
				{
					Parent.US_NHTProgramCodeInfo.AddMessageError(OnlyREIAndMVSAreAllowedFor2A);
				}

				if (Header.NHTSADetails.Count == 0)
				{
					Parent.US_NHTProgramCodeInfo.AddMessageError(AtLeastOneDetailsLineRequired);
				}
			}

			ValidateUS_NHTBoxNumber();
		}
		internal const string OnlyREIAndMVSAreAllowedFor2A = "Only program codes REI and MVS are allowed if Box Number 2A is selected below.";
		internal const string AtLeastOneDetailsLineRequired = "At least one Details line is required.";

		protected override void CheckUS_NHTElectronicImage()
		{
			base.CheckUS_NHTElectronicImage();

			if (Header.IsPGAValidationOn)
			{
				var electronicImageRequired = (Parent.US_NHTBoxNumber == BoxNumberList.Codes._2B && Header.NHTSADocuments.HasDocument(NHTSADocumentTypeList.Codes._872))
										  || (Parent.US_NHTBoxNumber == BoxNumberList.Codes._03 && Header.NHTSADocuments.HasDocument(NHTSADocumentTypeList.Codes._165))
										  || ((Parent.US_NHTBoxNumber == BoxNumberList.Codes._06 || Parent.US_NHTBoxNumber == BoxNumberList.Codes._12) && Header.NHTSADocuments.HasDocument(NHTSADocumentTypeList.Codes._874))
										  || (Parent.US_NHTBoxNumber == BoxNumberList.Codes._09 && (Header.NHTSADocuments.HasDocument(NHTSADocumentTypeList.Codes._875) || Header.NHTSADocuments.HasDocument(NHTSADocumentTypeList.Codes._958)));

				if (electronicImageRequired && !Parent.US_NHTElectronicImage)
				{
					Parent.US_NHTElectronicImageInfo.AddMessageError(ValidationConstants.NHTSA.ElectronicImageIsRequired);
				}
				else if (!electronicImageRequired && Parent.US_NHTElectronicImage)
				{
					Parent.US_NHTElectronicImageInfo.AddWarning(ValidationConstants.NHTSA.ElectronicImageIsNotRequired);
				}
			}
		}

		protected override void CheckUS_OA_NHTOwner()
		{
			base.CheckUS_OA_NHTOwner();

			var header = Header;
			if (header.IsPGAValidationOn)
			{
				if (header.US_NHTBoxNumber == BoxNumberList.Codes._2B || header.US_NHTBoxNumber == BoxNumberList.Codes._05 || Header.NHTSADocuments.HasOrganization(NHTSAOrganizationTypeList.Codes.Owner) || Header.US_CertifyingIndividual == PartyTypeList.Codes.Owner)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_OA_NHTOwnerInfo);
				}
				OrganisationValidation.ValidateStateForPGAAddress(Parent.US_OA_NHTOwnerInfo, header.OwnerAddress);
				OrganisationValidation.ValidateCharactorsForAddressDescription(header.US_OA_NHTOwnerInfo, header.OwnerAddress);
			}
		}

		protected override void CheckUS_NHTFabricatingMFRAddress()
		{
			base.CheckUS_NHTFabricatingMFRAddress();

			var header = Header;
			if (header != null && header.IsPGAValidationOn)
			{
				var fabricatingManufacturerAddress = header.FabricatingManufacturerAddress;
				if (fabricatingManufacturerAddress == null)
				{
					if (header.US_NHTProgramCode == NHTSAProgramCodeList.Codes.REI || header.NHTSADocuments.HasOrganization(NHTSAOrganizationTypeList.Codes.FabricatingManufacturer))
					{
						Parent.US_NHTFabricatingMFRAddressInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Fabricating Manufacturer"));
					}
				}
				else
				{
					if (header.IsTMCCodeRequired)
					{
						var tireCode = fabricatingManufacturerAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.TireManufacturerCode, Core.Constants.CountryCodes.UnitedStates);
						if (tireCode.IsEmpty)
						{
							Parent.US_NHTFabricatingMFRAddressInfo.AddMessageError(ZString.Format(ManufacturerCodeIsRequired, "Tire", NHTSACategoryCode_REITYPList.Codes.REI1, OrgCusCode.USACodeTypes.TireManufacturerCode));
						}
					}
					else if (header.IsGMCCodeRequired)
					{
						var glazingCode = fabricatingManufacturerAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.GlazingManufacturerCode, Core.Constants.CountryCodes.UnitedStates);
						if (glazingCode.IsEmpty)
						{
							Parent.US_NHTFabricatingMFRAddressInfo.AddMessageError(ZString.Format(ManufacturerCodeIsRequired, "Glazing", NHTSACategoryCode_REITYPList.Codes.REI7, OrgCusCode.USACodeTypes.GlazingManufacturerCode));
						}
					}
					OrganisationValidation.ValidateStateForPGAAddress(Parent.US_NHTFabricatingMFRAddressInfo, fabricatingManufacturerAddress);
					OrganisationValidation.ValidateCharactorsForAddressDescription(header.US_NHTFabricatingMFRAddressInfo, header.FabricatingManufacturerAddress);
				}
			}
		}
		internal const string ManufacturerCodeIsRequired = "Manufacturer does not have a {0} Manufacturer Code. This is required when Category Code = {1}.\r\n{2} code can be added to the organization under Details > Config > Registration Numbers.";

		protected override void CheckUS_NHTOriginalMFRAddress()
		{
			base.CheckUS_NHTOriginalMFRAddress();

			var header = Header;
			if (header.IsPGAValidationOn)
			{
				if (header.US_NHTOriginalMFRAddress.IsEmpty)
				{
					if (header.NHTSADocuments.HasOrganization(NHTSAOrganizationTypeList.Codes.OriginalVehicleManufacturer))
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NHTOriginalMFRAddressInfo);
					}
				}
				else
				{
					OrganisationValidation.ValidateStateForPGAAddress(Parent.US_NHTOriginalMFRAddressInfo, header.OriginalVehicleManufacturerAddress);
					OrganisationValidation.ValidateCharactorsForAddressDescription(header.US_NHTOriginalMFRAddressInfo, header.OriginalVehicleManufacturerAddress);
				}
			}

			ValidateUS_NHTBoxNumber();
		}

		protected override void CheckUS_OA_NHTRetailer()
		{
			base.CheckUS_OA_NHTRetailer();

			var header = Header;
			if (header.IsPGAValidationOn)
			{
				if ((header.US_NHTBoxNumber == BoxNumberList.Codes._2A && header.US_NHTProgramCode == NHTSAProgramCodeList.Codes.REI) || Header.NHTSADocuments.HasOrganization(NHTSAOrganizationTypeList.Codes.RetailerOrDistributor))
				{
					if (header.US_OA_NHTRetailer.IsEmpty)
					{
						Parent.US_OA_NHTRetailerInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Retailer/Distributor"));
					}
				}

				if (!header.US_OA_NHTRetailer.IsEmpty)
				{
					OrganisationValidation.ValidateStateForPGAAddress(Parent.US_OA_NHTRetailerInfo, header.RetailerDistributorAddress);
					OrganisationValidation.ValidateCharactorsForAddressDescription(header.US_OA_NHTRetailerInfo, header.RetailerDistributorAddress);
				}
			}
		}

		protected override void CheckUS_NHTEmbassyNationality()
		{
			base.CheckUS_NHTEmbassyNationality();

			if (Header.IsPGAValidationOn && Header.US_NHTBoxNumber == BoxNumberList.Codes._06)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_NHTEmbassyNationalityInfo, Parent.Lookups.USCountries);
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NHTEmbassyNationalityInfo);
			}
			else
			{
				ListValidation.WarnIfInvalidCode(Parent.US_NHTEmbassyNationalityInfo, Parent.Lookups.USCountries);
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void CheckUS_NHTBoxNumber()
		{
			base.CheckUS_NHTBoxNumber();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_NHTBoxNumberInfo, Parent.Lookups.BoxNumbers);

			if (Header.IsPGAValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NHTBoxNumberInfo);

				if (Header.US_NHTBoxNumber == BoxNumberList.Codes._2B && !Header.NHTSADocuments.HasDocument(NHTSADocumentTypeList.Codes._872))
				{
					AddMessageErrorIfBoxNumberIfInvalidAgainstWithDocuments(Parent.US_NHTBoxNumberInfo, Header.US_NHTBoxNumber, NHTSADocumentTypeList.Codes._872, ZString.Empty);
				}
				else if (Header.US_NHTBoxNumber == BoxNumberList.Codes._03)
				{
					if (!Header.NHTSADocuments.HasDocument(NHTSADocumentTypeList.Codes._165))
					{
						AddMessageErrorIfBoxNumberIfInvalidAgainstWithDocuments(Parent.US_NHTBoxNumberInfo, Header.US_NHTBoxNumber, NHTSADocumentTypeList.Codes._165, ZString.Empty);
					}

					CheckPermitAndLicenseExists(Parent.US_NHTBoxNumberInfo, NHTSALPCOTypeList.Codes.NH0, delegate
					{ return false; }, ZString.Empty);
					CheckPermitAndLicenseExists(Parent.US_NHTBoxNumberInfo, NHTSALPCOTypeList.Codes.NH3, delegate
					{ return false; }, ZString.Empty);
				}
				else if ((Header.US_NHTBoxNumber == BoxNumberList.Codes._06 || Header.US_NHTBoxNumber == BoxNumberList.Codes._12) && !Header.NHTSADocuments.HasDocument(NHTSADocumentTypeList.Codes._874))
				{
					AddMessageErrorIfBoxNumberIfInvalidAgainstWithDocuments(Parent.US_NHTBoxNumberInfo, Header.US_NHTBoxNumber, NHTSADocumentTypeList.Codes._874, ZString.Empty);
				}
				else if (Header.US_NHTBoxNumber == BoxNumberList.Codes._07)
				{
					if (!Header.US_NHTOriginalMFRAddress.IsEmpty && !Header.NHTSADocuments.HasDocument(NHTSADocumentTypeList.Codes._871))
					{
						Parent.US_NHTBoxNumberInfo.AddMessageError(Document871RequiredWhenVOMReported);
					}
					else if (Header.US_NHTOriginalMFRAddress.IsEmpty)
					{
						if (Header.NHTSADocuments.HasDocument(NHTSADocumentTypeList.Codes._871))
						{
							Parent.US_NHTBoxNumberInfo.AddMessageError(Document871NotRequiredWhenVOMIsNotReported);
						}

						CheckPermitAndLicenseExists(Parent.US_NHTBoxNumberInfo, NHTSALPCOTypeList.Codes.NH2, delegate
						{ return Header.US_NHTOriginalMFRAddress.IsEmpty; }, "Original Vehicle Manufacturer is not reported");
					}
				}
				else if (Header.US_NHTBoxNumber == BoxNumberList.Codes._09 && !(Header.NHTSADocuments.HasDocument(NHTSADocumentTypeList.Codes._875) && Header.NHTSADocuments.HasDocument(NHTSADocumentTypeList.Codes._958)))
				{
					AddMessageErrorIfBoxNumberIfInvalidAgainstWithDocuments(Parent.US_NHTBoxNumberInfo, Header.US_NHTBoxNumber, NHTSADocumentTypeList.Codes._875, NHTSADocumentTypeList.Codes._958);
				}
				else if (Header.US_NHTBoxNumber == BoxNumberList.Codes._10)
				{
					CheckPermitAndLicenseExists(Parent.US_NHTBoxNumberInfo, NHTSALPCOTypeList.Codes.NH2, delegate
					{ return false; }, ZString.Empty);
				}
				else if (Header.US_NHTBoxNumber == BoxNumberList.Codes._13)
				{
					CheckPermitAndLicenseExists(Parent.US_NHTBoxNumberInfo, NHTSALPCOTypeList.Codes.NH0, delegate
					{ return false; }, ZString.Empty);
					CheckPermitAndLicenseExists(Parent.US_NHTBoxNumberInfo, NHTSALPCOTypeList.Codes.NH2, delegate
					{ return false; }, ZString.Empty);
				}
			}

			ValidateUS_NHTProgramCode();
			ValidateUS_NHTOriginalMFRAddress();
		}
		internal const string Document871RequiredWhenVOMReported = "Document 871 is required when Box Number is '07' and Original Vehicle Manufacturer is reported.";
		internal const string Document871NotRequiredWhenVOMIsNotReported = "Document 871 is not required when Box Number is '07' and Original Vehicle Manufacturer is not reported. Or please enter an Original Vehicle Manufacturer if you plan to submit document 871.";

		void AddMessageErrorIfBoxNumberIfInvalidAgainstWithDocuments(ZPropertyInfo propertyInfo, ZString boxNumber, ZString documentType1, ZString documentType2)
		{
			var docTypde1Description = Header.AddInfoLookups.DocumentTypes.GetDescriptionFromCode(documentType1);
			var additionalDocTypeDescription = !documentType2.IsEmpty ? " and " + documentType2 + " - " + Header.AddInfoLookups.DocumentTypes.GetDescriptionFromCode(documentType2) : string.Empty;
			propertyInfo.AddMessageError(ZString.Format(ValidationConstants.NHTSA.DocumentRequired, documentType1, docTypde1Description, additionalDocTypeDescription, documentType2.IsEmpty ? "is" : "are", boxNumber));
		}

		void CheckPermitAndLicenseExists(ZPropertyInfo propertyInfo, ZString requiredLPCOType, Func<ZBool> additionalConditionMatched, ZString additionalMessage)
		{
			var permissionExists = Header.NHTSADetails.OfType<NHTSADetails>().Any(x => x.PermitAndLicenses.OfType<NHTSAPermitAndLicenses>().Any(lpco => lpco.US_NHTLPCOType == requiredLPCOType));
			if (!permissionExists)
			{
				var messageError = "Permit And License with LPCO Type '{0}' is required when Box Number is '{1}'";
				if (additionalConditionMatched != null && additionalConditionMatched() && !additionalMessage.IsEmpty)
				{
					messageError += " and " + additionalMessage;
				}

				propertyInfo.AddMessageError(ZString.Format(messageError + ".", requiredLPCOType, Header.US_NHTBoxNumber));
			}
		}

		protected override void CheckUS_NHTTravelDocType()
		{
			base.CheckUS_NHTTravelDocType();

			if (Header.IsTravelDocumentRequired)
			{
				if (Header.IsPGAValidationOn)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NHTTravelDocTypeInfo);
				}
				ListValidation.MessageErrorIfInvalidCode(Parent.US_NHTTravelDocTypeInfo, Parent.Lookups.TravelDocumentTypes);
			}
			else
			{
				ListValidation.WarnIfInvalidCode(Parent.US_NHTTravelDocTypeInfo, Parent.Lookups.TravelDocumentTypes);
			}
		}

		protected override void CheckUS_NHTTravelDocNumber()
		{
			base.CheckUS_NHTTravelDocNumber();

			if (Header.IsTravelDocumentRequired && Header.IsPGAValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NHTTravelDocNumberInfo);
			}
		}

		protected override void CheckUS_NHTTravelDocNationality()
		{
			base.CheckUS_NHTTravelDocNationality();

			if (Header.IsTravelDocumentRequired)
			{
				if (Header.IsPGAValidationOn)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NHTTravelDocNationalityInfo);
				}
				ListValidation.MessageErrorIfInvalidCode(Parent.US_NHTTravelDocNationalityInfo, Parent.Lookups.USCountries);
			}
			else
			{
				ListValidation.WarnIfInvalidCode(Parent.US_NHTTravelDocNationalityInfo, Parent.Lookups.USCountries);
			}
		}

		protected override void CheckUS_NHTDOTSuretyCode()
		{
			base.CheckUS_NHTDOTSuretyCode();

			if (Header.IsDOTBondRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NHTDOTSuretyCodeInfo);

				if (!Regex.IsMatch(Parent.US_NHTDOTSuretyCode, @"^[0-9]{3}$"))
				{
					Parent.US_NHTDOTSuretyCodeInfo.AddMessageError(ValidationConstants.NHTSA.InvalidDOTSuretyCodeFormat);
				}
			}
		}

		protected override void CheckUS_NHTDOTBondNumber()
		{
			base.CheckUS_NHTDOTBondNumber();

			if (Header.IsDOTBondRequired && Header.IsPGAValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NHTDOTBondNumberInfo);
			}
		}

		protected override void CheckUS_NHTDOTBondType()
		{
			base.CheckUS_NHTDOTBondType();

			if (Header.IsDOTBondRequired)
			{
				if (Header.IsPGAValidationOn)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NHTDOTBondTypeInfo);
				}
				ListValidation.MessageErrorIfInvalidCode(Parent.US_NHTDOTBondTypeInfo, Parent.Lookups.BondTypes);
			}
			else
			{
				ListValidation.WarnIfInvalidCode(Parent.US_NHTDOTBondTypeInfo, Parent.Lookups.BondTypes);
			}
		}

		protected override void CheckUS_NHTDOTBondAmount()
		{
			base.CheckUS_NHTDOTBondAmount();

			if (Header.IsPGAValidationOn)
			{
				if (Header.IsDOTBondRequired || !Header.US_NHTDOTSuretyCode.IsEmpty || !Header.US_NHTDOTBondNumber.IsEmpty || !Header.US_NHTDOTBondType.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NHTDOTBondAmountInfo);
				}
			}
		}

		protected override void CheckUS_CertifyingIndividual()
		{
			base.CheckUS_CertifyingIndividual();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_CertifyingIndividualInfo, Parent.Lookups.NHTSACertifyingIndividualList);

			if (Header.IsPGAValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_CertifyingIndividualInfo);
			}

			ValidateUS_OA_NHTOwner();
			ValidateUS_PGAContactName();
			ValidateUS_PGAContactPhoneNo();
			ValidateUS_PGAContactEmail();
		}

		protected override void CheckUS_PGAContactName()
		{
			base.CheckUS_PGAContactName();

			if (Header.IsPGAValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PGAContactNameInfo);
			}
		}

		protected override void CheckUS_PGAContactPhoneNo()
		{
			base.CheckUS_PGAContactPhoneNo();

			if (Header.IsPGAValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PGAContactPhoneNoInfo);
			}
		}

		protected override void CheckUS_PGAContactEmail()
		{
			base.CheckUS_PGAContactEmail();

			if (Header.IsPGAValidationOn)
			{
				if (!Parent.US_PGAContactEmail.IsEmpty)
				{
					if (!EmailAddressValidation.IsEmailAddressValid(Parent.US_PGAContactEmail))
					{
						Parent.US_PGAContactEmailInfo.AddWarning("Invalid email format");
					}
				}
				else
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PGAContactEmailInfo);
				}
			}
		}

		protected override void CheckUS_IntendedUseCode()
		{
			base.CheckUS_IntendedUseCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_IntendedUseCodeInfo, Parent.Lookups.IntendedUseCodes);
		}

		protected override void CheckUS_IntendedUseDesc()
		{
			base.CheckUS_IntendedUseDesc();
			if (Header.IsPGAValidationOn && Header.US_IntendedUseCode == IntendedUseCodesList.Codes.ForOtherUse)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_IntendedUseDescInfo);
			}
		}
	}
}
