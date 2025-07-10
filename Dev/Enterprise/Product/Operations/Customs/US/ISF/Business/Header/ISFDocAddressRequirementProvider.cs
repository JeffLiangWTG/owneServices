using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFDocAddressRequirementProvider
	{
		public ISFDocAddressRequirementProvider(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		public ISFDocAddressRequirement ManufacturerDocAddressRequirement
		{
			get
			{
				if (manufacturerDocAddressRequirement == null)
				{
					manufacturerDocAddressRequirement = CreateNewISFDocAddressRequirement(DocAddressType.Manufacturer);
					manufacturerDocAddressRequirement.GetRegistrationNumberResult = GetRegistrationNumberResult;
					manufacturerDocAddressRequirement.DefaultMax = 0;
					AddDocAddressValidation(manufacturerDocAddressRequirement);
					manufacturerDocAddressRequirement.LookupsGovRegNumTypes = GetOtherPartiesCodeTypeList;
				}
				return manufacturerDocAddressRequirement;
			}
		}
		ISFDocAddressRequirement manufacturerDocAddressRequirement;

		public ISFDocAddressRequirement ConsigneeDocAddressRequirement
		{
			get
			{
				if (consigneeDocAddressRequirement == null)
				{
					consigneeDocAddressRequirement = CreateNewISFDocAddressRequirement(DocAddressType.ConsigneeAddress, ContactType.Consignee);
					consigneeDocAddressRequirement.GetRegistrationNumberResult = GetRegistrationNumberResultForConsignee;
					consigneeDocAddressRequirement.DefaultMax = 0;
					AddConsigneeValidation(consigneeDocAddressRequirement);
					consigneeDocAddressRequirement.LookupsGovRegNumTypes = GetConsigneeCodeTypeList;
				}
				return consigneeDocAddressRequirement;
			}
		}
		ISFDocAddressRequirement consigneeDocAddressRequirement;

		public ISFDocAddressRequirement ShipToPartyDocAddressRequirement
		{
			get
			{
				if (shipToPartyDocAddressRequirement == null)
				{
					shipToPartyDocAddressRequirement = CreateNewISFDocAddressRequirement(DocAddressType.ShipToParty);
					shipToPartyDocAddressRequirement.GetRegistrationNumberResult = (JobDocAddress address) =>
					{
						return new RegistrationNumberResult(address.Factory, true,
										delegate
										{
											var result = new RegistrationNumber() { NumberType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem };
											var cusCode = GetCusCode(address, OrgCusCode.USACodeTypes.FIRMSCode, OrgCusCode.USACodeTypes.DataUniversalNumberingSystemPlus4, OrgCusCode.CodeTypes.DataUniversalNumberingSystem);
											if (cusCode != null)
											{
												result.Number = cusCode.OK_CustomsRegNo;
												result.NumberType = cusCode.OK_CodeType;
											}
											return result;
										});
					};
					shipToPartyDocAddressRequirement.IsMandatory = true;
					shipToPartyDocAddressRequirement.DefaultMax = 0;
					AddDocAddressValidation(shipToPartyDocAddressRequirement);
					shipToPartyDocAddressRequirement.ValidateOrganisationPKUponMandatoryRequirement = DocAddressValidateOrganisationPKForBoth;
					shipToPartyDocAddressRequirement.LookupsGovRegNumTypes = GetShipToPartyCodeTypeList;
				}
				return shipToPartyDocAddressRequirement;
			}
		}
		ISFDocAddressRequirement shipToPartyDocAddressRequirement;

		public ISFDocAddressRequirement GetOtherPartyISFDocAddressRequirement(DocAddressType addressType, ZString entryType, bool canBeUSParty)
		{
			ISFDocAddressRequirement result;
			if (!OtherPartyISFDocAddressRequirements.TryGetValue(addressType, out result))
			{
				result = CreateNewISFDocAddressRequirement(addressType);
				result.IsMandatory = true;
				result.DefaultMax = 0; // only for testing
				AddDocAddressValidation(result);
				if (canBeUSParty)
				{
					result.GetRegistrationNumberResult = GetRegistrationNumberResultForUSParty;
					result.LookupsGovRegNumTypes = GetOtherUSPartiesCodeTypeList;
					result.ValidateSocialSecurityNumber = ValidateSocialSecurityNumber;
					result.ValidateSocialSecurityNumberDateOfBirth = ValidateSocialSecurityNumberDateOfBirth;
				}
				else
				{
					result.GetRegistrationNumberResult = GetRegistrationNumberResult;
					result.LookupsGovRegNumTypes = GetOtherPartiesCodeTypeList;
				}
				OtherPartyISFDocAddressRequirements.Add(addressType, result);
				switch (entryType)
				{
					case SubmissionTypeList.Codes.ISF5:
						result.ValidateOrganisationPKUponMandatoryRequirement = DocAddressValidateOrganisationPKForISF5;
						break;
					case SubmissionTypeList.Codes.ISF10:
						result.ValidateOrganisationPKUponMandatoryRequirement = DocAddressValidateOrganisationPKForISF10;
						break;
					default:
						result.ValidateOrganisationPKUponMandatoryRequirement = DocAddressValidateOrganisationPKForBoth;
						break;
				}
			}
			return result;
		}

		ISFDocAddressRequirement CreateNewISFDocAddressRequirement(DocAddressType addressType, ContactType defaultContactType)
		{
			var result = new ISFDocAddressRequirement(addressType, defaultContactType);
			result.ValidateOrganisationPK = ValidateOrganisationPK;
			return result;
		}

		ISFDocAddressRequirement CreateNewISFDocAddressRequirement(DocAddressType addressType)
		{
			var result = new ISFDocAddressRequirement(addressType);
			result.ValidateOrganisationPK = ValidateOrganisationPK;
			return result;
		}

		void ValidateOrganisationPK(JobDocAddressValidation validation)
		{
			var parent = validation.Parent as ISFDocAddress;
			if (parent.OrganisationPK == OrgHeader.UnmatchedOrganisationPK)
			{
				parent.OrganisationPKInfo.AddMessageError(ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
			}
			else if (parent.HasRealOrganisation)
			{
				if (parent.IsPassportIDGovRegNumType)
				{
					ZString errorMessage = PassportNumberValidator.Validate(parent.E2_PassportID);
					if (!errorMessage.IsEmpty)
					{
						parent.OrganisationPKInfo.AddMessageError(errorMessage);
					}
					parent.OrganisationPKInfo.AddWarning(ValidationConstants.Organisation.PassportDataToReportToCustoms(parent.E2_PassportID, parent.E2_Contact, parent.E2_PassportDateOfBirth, parent.E2_PassportCountryOfIssue));
				}
				else if (parent.IsSocialSecurityNumberGovRegNumType)
				{
					ZString errorMessage = SocialSecurityNumberValidator.Validate(parent.E2_SocialSecurityNumber);
					if (!errorMessage.IsEmpty)
					{
						parent.OrganisationPKInfo.AddMessageError(errorMessage);
					}
					parent.OrganisationPKInfo.AddWarning(ValidationConstants.Organisation.SocialSecurityDataToReportToCustoms(parent.E2_SocialSecurityNumber, parent.E2_Contact, parent.E2_SocialSecurityNumberDateOfBirth));
				}
			}
			validation.ValidateE2_Contact();
		}

		Dictionary<DocAddressType, ISFDocAddressRequirement> OtherPartyISFDocAddressRequirements
		{
			get { return fOtherPartyISFDocAddressRequirements ?? (fOtherPartyISFDocAddressRequirements = new Dictionary<DocAddressType, ISFDocAddressRequirement>()); }
		}
		Dictionary<DocAddressType, ISFDocAddressRequirement> fOtherPartyISFDocAddressRequirements;

		CodeDescriptionPairList GetShipToPartyCodeTypeList(JobDocAddressLookups lookups)
		{
			return factory.GetCachedValue<ShipToPartyCodeTypeList>();
		}

		CodeDescriptionPairList GetOtherPartiesCodeTypeList(JobDocAddressLookups lookups)
		{
			return factory.GetCachedValue<OtherPartiesCodeTypeList>();
		}

		CodeDescriptionPairList GetOtherUSPartiesCodeTypeList(JobDocAddressLookups lookups)
		{
			return factory.GetCachedValue<OtherUSPartiesCodeTypeList>();
		}

		CodeDescriptionPairList GetConsigneeCodeTypeList(JobDocAddressLookups lookups)
		{
			return factory.GetCachedValue<ConsigneeCodeTypeList>();
		}

		void AddConsigneeValidation(ISFDocAddressRequirement consigneeDocAddressRequirement)
		{
			consigneeDocAddressRequirement.ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address = ValidateE2_OA_Address;
			consigneeDocAddressRequirement.ValidateDelegateToBeFiredUponValidationOf_E2_AddressOverride = ValidateE2_AddressOverride;
			consigneeDocAddressRequirement.ValidateGovRegNo = ConsigneeValidateGovRegNo;
			consigneeDocAddressRequirement.ValidateGovRegNumType = ConsigneeValidateGovRegNumType;
			consigneeDocAddressRequirement.ValidateCompanyName = DocAddressDoNothingValidator;
			consigneeDocAddressRequirement.ValidateAddress1 = DocAddressDoNothingValidator;
			consigneeDocAddressRequirement.ValidateCompanyName = DocAddressDoNothingValidator;
			consigneeDocAddressRequirement.ValidateCity = DocAddressDoNothingValidator;
			consigneeDocAddressRequirement.ValidatePassportID = ValidatePassportID;
			consigneeDocAddressRequirement.ValidatePassportCountryOfIssue = ValidatePassportCountryOfIssue;
			consigneeDocAddressRequirement.ValidatePassportDateOfBirth = ValidatePassportDateOfBirth;
			consigneeDocAddressRequirement.ValidateOrganisationPK = ConsigneeValidateOrganisationPK;
			consigneeDocAddressRequirement.ValidateSocialSecurityNumber = ValidateSocialSecurityNumber;
			consigneeDocAddressRequirement.ValidateSocialSecurityNumberDateOfBirth = ValidateSocialSecurityNumberDateOfBirth;
			consigneeDocAddressRequirement.ValidateContact = ValidateContact;
		}

		void AddDocAddressValidation(ISFDocAddressRequirement requirement)
		{
			requirement.ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address = ValidateE2_OA_Address;
			requirement.ValidateDelegateToBeFiredUponValidationOf_E2_AddressOverride = ValidateE2_AddressOverride;
			requirement.ValidateCompanyName = DocAddressValidateCompanyName;
			requirement.ValidateAddress1 = DocAddressValidateAddress1;
			requirement.ValidateCity = DocAddressValidateCity;
			requirement.ValidatePostCode = DocAddressValidatePostCode;
			requirement.ValidateState = DocAddressValidateState;
			requirement.ValidateCountry = DocAddressValidateCountry;
			requirement.ValidateGovRegNo = DocAddressValidateGovRegNo;
			requirement.ValidateContact = ValidateContact;
		}

		void ConsigneeValidateOrganisationPK(JobDocAddressValidation validation)
		{
			var docAddress = validation.Parent as ISFDocAddress;
			if (!docAddress.E2_AddressOverride && docAddress.HasRealOrganisation)
			{
				CusISFHeader header = docAddress.Parent as CusISFHeader;
				if (header != null && header.IsISF10Entry)
				{
					if (docAddress.E2_GovRegNum.IsEmpty)
					{
						docAddress.OrganisationPKInfo.AddMessageError(ValidationConstants.Organisation.OrganizationRequiresEIN_CBN_ECN_SSN_PAS);
					}
					else if (docAddress.IsPassportIDGovRegNumType)
					{
						ZString errorMessage = PassportNumberValidator.Validate(docAddress.E2_PassportID);
						if (!errorMessage.IsEmpty)
						{
							docAddress.OrganisationPKInfo.AddMessageError(errorMessage);
						}
						docAddress.OrganisationPKInfo.AddWarning(ValidationConstants.Organisation.PassportDataToReportToCustoms(docAddress.E2_PassportID, docAddress.E2_Contact, docAddress.E2_PassportDateOfBirth, docAddress.E2_PassportCountryOfIssue));
					}
					else if (docAddress.IsSocialSecurityNumberGovRegNumType)
					{
						ZString errorMessage = SocialSecurityNumberValidator.Validate(docAddress.E2_SocialSecurityNumber);
						if (!errorMessage.IsEmpty)
						{
							docAddress.OrganisationPKInfo.AddMessageError(errorMessage);
						}
						docAddress.OrganisationPKInfo.AddWarning(ValidationConstants.Organisation.SocialSecurityDataToReportToCustoms(docAddress.E2_SocialSecurityNumber, docAddress.E2_Contact, docAddress.E2_SocialSecurityNumberDateOfBirth));
					}
				}
			}
			validation.ValidateE2_Contact();
		}

		void ValidatePassportID(JobDocAddressValidation validation)
		{
			var docAddress = validation.Parent;
			CusISFHeader header = docAddress.Parent as CusISFHeader;
			if (header != null && header.IsISF10Entry)
			{
				if (docAddress.E2_PassportID.IsEmpty)
				{
					docAddress.E2_PassportIDInfo.AddMessageError(ValidationConstants.Organisation.IDIsRequiredForPassport);
				}
				else
				{
					ZString errorMessage = PassportNumberValidator.Validate(docAddress.E2_PassportID);
					if (!errorMessage.IsEmpty)
					{
						docAddress.E2_PassportIDInfo.AddMessageError(errorMessage);
					}
				}
			}
		}

		void ValidatePassportCountryOfIssue(JobDocAddressValidation validation)
		{
			var docAddress = validation.Parent;
			CusISFHeader header = docAddress.Parent as CusISFHeader;
			if (header != null && header.IsISF10Entry)
			{
				if (docAddress.E2_PassportCountryOfIssue.IsEmpty)
				{
					docAddress.E2_PassportCountryOfIssueInfo.AddMessageError(ValidationConstants.Organisation.CountryOfIssueIsRequiredForPassport);
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(docAddress.E2_PassportCountryOfIssueInfo);
				}
			}
		}

		void ValidatePassportDateOfBirth(JobDocAddressValidation validation)
		{
			var docAddress = validation.Parent;
			CusISFHeader header = docAddress.Parent as CusISFHeader;
			if (header != null && header.IsISF10Entry)
			{
				if (docAddress.E2_PassportDateOfBirth.IsEmpty)
				{
					docAddress.E2_PassportDateOfBirthInfo.AddMessageError(ValidationConstants.Organisation.DateOfBirthIsRequiredForPassport);
				}
				else if (docAddress.E2_PassportDateOfBirth > ZDateTime.Today)
				{
					docAddress.E2_PassportDateOfBirthInfo.AddMessageError(ValidationConstants.Header.DateOfBirthIsInvalid);
				}
			}
		}

		void ValidateSocialSecurityNumber(JobDocAddressValidation validation)
		{
			var docAddress = validation.Parent as ISFDocAddress;
			CusISFHeader header = docAddress.Parent as CusISFHeader;
			if (header != null && header.IsISF10Entry)
			{
				if (docAddress.E2_SocialSecurityNumber.IsEmpty)
				{
					docAddress.E2_SocialSecurityNumberInfo.AddMessageError(ValidationConstants.Organisation.NumberIsRequiredForSocialSecurityNumber);
				}
				else
				{
					ZString errorMessage = SocialSecurityNumberValidator.Validate(docAddress.E2_SocialSecurityNumber);
					if (!errorMessage.IsEmpty)
					{
						docAddress.E2_SocialSecurityNumberInfo.AddMessageError(errorMessage);
					}
				}
			}
		}

		void ValidateSocialSecurityNumberDateOfBirth(JobDocAddressValidation validation)
		{
			var docAddress = validation.Parent as ISFDocAddress;
			CusISFHeader header = docAddress.Parent as CusISFHeader;
			if (header != null && header.IsISF10Entry)
			{
				if (docAddress.E2_SocialSecurityNumberDateOfBirth.IsEmpty)
				{
					docAddress.E2_SocialSecurityNumberDateOfBirthInfo.AddMessageError(ValidationConstants.Organisation.DateOfBirthIsRequiredForSocialSecurityNumber);
				}
				else if (docAddress.E2_SocialSecurityNumberDateOfBirth > ZDateTime.Today)
				{
					docAddress.E2_SocialSecurityNumberDateOfBirthInfo.AddMessageError(ValidationConstants.Header.DateOfBirthIsInvalid);
				}
			}
		}

		void ValidateContact(JobDocAddressValidation validation)
		{
			var docAddress = validation.Parent as ISFDocAddress;
			CusISFHeader header = docAddress.Parent as CusISFHeader;
			if (header != null && header.IsISF10Entry && (docAddress.IsSocialSecurityNumberGovRegNumType || docAddress.IsPassportIDGovRegNumType))
			{
				if (docAddress.E2_Contact.IsEmpty)
				{
					docAddress.E2_ContactInfo.AddMessageError(ValidationConstants.Organisation.ContactNameIsRequiredFor(docAddress.IsSocialSecurityNumberGovRegNumType ? "Social Security" : "Passport"));
				}
				else
				{
					AMSCharactersValidator.ValidateCharacters(docAddress.E2_ContactInfo);
				}
			}
		}

		void ConsigneeValidateGovRegNo(JobDocAddressValidation validation)
		{
			var docAddress = validation.Parent as ISFDocAddress;
			CusISFHeader header = docAddress.Parent as CusISFHeader;
			if (header != null && header.IsISF10Entry)
			{
				if (!docAddress.IsPassportIDGovRegNumType && !docAddress.IsSocialSecurityNumberGovRegNumType)
				{
					if (docAddress.E2_GovRegNum.IsEmpty)
					{
						if (docAddress.Lookups.GovRegNumTypes.ContainsCode(docAddress.E2_GovRegNumType))
						{
							docAddress.E2_GovRegNumInfo.AddMessageError(ValidationConstants.Header.ValueIsRequiredForEntity(docAddress.Lookups.GovRegNumTypes.GetDescriptionFromCode(docAddress.E2_GovRegNumType)));
						}
					}
					else
					{
						EntityIdentifierNumberValidation.Validate(docAddress.E2_GovRegNumInfo, docAddress.E2_GovRegNumType);
					}
				}
			}
		}

		void ConsigneeValidateGovRegNumType(JobDocAddressValidation validation)
		{
			var docAddress = validation.Parent;
			CusISFHeader header = docAddress.Parent as CusISFHeader;
			if (header != null && header.IsISF10Entry)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(docAddress.E2_GovRegNumTypeInfo);
				if (docAddress.IsPassportIDGovRegNumType && !ShipmentTypeList.IsPassportAllowed(header.BF_ShipmentType))
				{
					docAddress.E2_GovRegNumTypeInfo.AddMessageError(ValidationConstants.Header.PassportIsOnlyUsedFor03Or05Or06ShipmentType);
				}
			}
		}

		void DocAddressValidateGovRegNo(JobDocAddressValidation validation)
		{
			var docAddress = (ISFDocAddress)validation.Parent;
			if (!docAddress.IsSocialSecurityNumberGovRegNumType
				&& (docAddress.E2_AddressOverride
					|| (docAddress.E2_GovRegNumType != CodeTypeList.Codes.DUNS && docAddress.E2_GovRegNumType != CodeTypeList.Codes.DUNSPlus4)))
			{
				EntityIdentifierNumberValidation.Validate(docAddress.E2_GovRegNumInfo, docAddress.E2_GovRegNumType);
			}
		}

		void DocAddressValidateOrganisationPKForBoth(JobDocAddressValidation validation)
		{
			DocAddressValidateOrganisationPK(validation, ZString.Empty);
		}

		void DocAddressValidateOrganisationPKForISF5(JobDocAddressValidation validation)
		{
			DocAddressValidateOrganisationPK(validation, SubmissionTypeList.Codes.ISF5);
		}

		void DocAddressValidateOrganisationPKForISF10(JobDocAddressValidation validation)
		{
			DocAddressValidateOrganisationPK(validation, SubmissionTypeList.Codes.ISF10);
		}

		void DocAddressValidateOrganisationPK(JobDocAddressValidation validation, ZString entryType)
		{
			var parent = validation.Parent;
			CusISFHeader header = parent.Parent as CusISFHeader;
			if (header != null)
			{
				if ((entryType.IsEmpty || header.BF_EntryType == entryType) && IsValidForAddressType(header.BF_ActionReasonCode, parent.DocAddressType))
				{
					parent.OrganisationPKInfo.AddMessageError(ValidationConstants.Organisation.OrganisationPKRequired(parent.AddressCaption, header.BF_EntryType));
				}
			}
		}

		bool IsValidForAddressType(ZString actionReasonCode, DocAddressType addressType)
		{
			return !(addressType == DocAddressType.Consolidator || addressType == DocAddressType.ScheduledContainerStuffingLocation) || actionReasonCode != ActionReasonCodeList.Codes.FlexibleTiming && actionReasonCode != ActionReasonCodeList.Codes.FlexibleRangeAndFlexibleTiming;
		}

		void DocAddressDoNothingValidator(JobDocAddressValidation validation)
		{
		}

		void DocAddressValidateCompanyName(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			if (ShouldCheckAddressInformation(parent))
			{
				if (parent.E2_CompanyName.IsEmpty)
				{
					parent.E2_CompanyNameInfo.AddMessageError(ValidationConstants.Organisation.CompanyNameRequireWhenNoRegistration);
				}
				else
				{
					AMSCharactersValidator.ValidateCharacters(parent.E2_CompanyNameInfo);
				}
			}
		}

		bool ShouldCheckAddressInformation(JobDocAddress jobDocAddress)
		{
			return OtherPartiesCodeTypeList.IsDUNSType(jobDocAddress.E2_GovRegNumType) || jobDocAddress.E2_GovRegNum.IsEmpty;
		}

		void DocAddressValidateAddress1(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			if (ShouldCheckAddressInformation(parent))
			{
				if (parent.E2_Address1.IsEmpty)
				{
					parent.E2_Address1Info.AddMessageError(ValidationConstants.Organisation.AddressRequireWhenNoRegistration);
				}
				else
				{
					AMSCharactersValidator.ValidateCharacters(parent.E2_Address1Info);
				}
			}
		}

		void DocAddressValidateCity(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			if (ShouldCheckAddressInformation(parent))
			{
				if (parent.E2_City.IsEmpty)
				{
					parent.E2_CityInfo.AddMessageError(ValidationConstants.Organisation.CityRequireWhenNoRegistration);
				}
				else
				{
					new AddressValidation().CheckPostcodeViaCity(parent.E2_CityInfo, parent.E2_PostcodeInfo, parent.Country);
					AMSCharactersValidator.ValidateCharacters(parent.E2_CityInfo);
				}
			}
		}

		void DocAddressValidatePostCode(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			if (ShouldCheckAddressInformation(parent))
			{
				new AddressValidation().CheckCityViaPostcode(parent.E2_PostcodeInfo, parent.E2_CityInfo, parent.Country);
			}
		}

		void DocAddressValidateState(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			if (ShouldCheckAddressInformation(parent))
			{
				new AddressValidation().CheckState(parent.E2_StateInfo, parent.Country, parent.ValidationSection, parent.ValidationStatus);
			}
		}

		void DocAddressValidateCountry(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			if (parent.E2_RN_NKCountryCode.IsEmpty)
			{
				if (ShouldCheckAddressInformation(parent))
				{
					parent.E2_RN_NKCountryCodeInfo.AddMessageError(ValidationConstants.Organisation.CountryRequireWhenNoRegistration);
				}
			}
			else if (!parent.E2_AddressOverride)
			{
				var org = factory.Load<OrgHeader>(parent.OrganisationPKIncludesMiscOrg);
				if (org != null && org.UNLOCO == null)
				{
					parent.E2_RN_NKCountryCodeInfo.AddMessageError(ValidationConstants.Organisation.MissingUNLOCO);
				}
			}
		}

		void ValidateE2_OA_Address(JobDocAddressValidation validation)
		{
			var isfValidation = validation as ISFDocAddressValidation;
			var parent = validation.Parent;
			CusISFHeader header = parent.Parent as CusISFHeader;
			if (header != null)
			{
				header.Validation.ValidateAll();
			}
			isfValidation.ValidateE2_PassportDetails();
			isfValidation.ValidateE2_SocialSecurityNumberDetails();
		}

		void ValidateE2_AddressOverride(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			CusISFHeader header = parent.Parent as CusISFHeader;
			if (header != null)
			{
				header.Validation.ValidateAll();
			}
		}

		RegistrationNumberResult GetRegistrationNumberResult(JobDocAddress docAddress)
		{
			return new RegistrationNumberResult(docAddress.Factory, true,
				delegate
				{
					RegistrationNumber result = new RegistrationNumber() { NumberType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem };
					var cusCode = GetCusCode(docAddress, OrgCusCode.USACodeTypes.DataUniversalNumberingSystemPlus4, OrgCusCode.CodeTypes.DataUniversalNumberingSystem);
					if (cusCode != null)
					{
						result.Number = cusCode.OK_CustomsRegNo;
						result.NumberType = cusCode.OK_CodeType;
					}

					return result;
				});
		}

		RegistrationNumberResult GetRegistrationNumberResultForUSParty(JobDocAddress docAddress)
		{
			return new RegistrationNumberResult(docAddress.Factory, true,
				delegate
				{
					RegistrationNumber result = new RegistrationNumber() { NumberType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem };
					var cusCode = GetCusCode(docAddress, USCustomsCodesInOrderOfSignificance);
					if (cusCode != null)
					{
						if (cusCode.OK_CodeType == OrgCusCode.USACodeTypes.SocialSecurityNumber)
						{
							var contact = docAddress.Contact;
							result.Number = ISFDocAddress.GetSocialSecurityNumberDataFormat(cusCode.OK_CustomsRegNo, contact == null ? ZDateTime.Empty : contact.OC_Birthday);
						}
						else
						{
							result.Number = cusCode.OK_CustomsRegNo;
						}
						result.NumberType = cusCode.OK_CodeType;
					}
					return result;
				});
		}

		internal readonly ZString[] USCustomsCodesInOrderOfSignificance = new ZString[] {
			OrgCusCode.USACodeTypes.EmployerIdentificationNumber,
			OrgCusCode.USACodeTypes.CBPAssignedNumber,
			OrgCusCode.USACodeTypes.SocialSecurityNumber,
			OrgCusCode.USACodeTypes.DataUniversalNumberingSystemPlus4,
			OrgCusCode.CodeTypes.DataUniversalNumberingSystem,
			OrgCusCode.USACodeTypes.EncryptedConsigneeNumber
		};

		RegistrationNumberResult GetRegistrationNumberResultForConsignee(JobDocAddress docAddress)
		{
			return new RegistrationNumberResult(docAddress.Factory, true,
				delegate
				{
					var result = new RegistrationNumber() { NumberType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber };
					if (docAddress != null && !docAddress.IsDeleted)
					{
						var org = docAddress.Organisation;

						if (org != null)
						{
							var cusCode = GetCusCode(org, docAddress.Address, ConsigneeCodeTypeList.GetListInOrderOfImportance());
							if (cusCode == null)
							{
								var cusCodes = org.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.CodeTypes.PassportID);
								if (cusCodes.Length == 1)
								{
									cusCode = cusCodes[0];
								}
							}
							if (cusCode != null)
							{
								result.NumberType = cusCode.OK_CodeType;
								if (result.NumberType == OrgCusCode.CodeTypes.PassportID)
								{
									var contact = docAddress.Contact;
									result.Number = JobDocAddress.GetPassportDataStringFormat(cusCode.OK_CustomsRegNo, cusCode.OK_RN_NKCodeCountry, contact == null ? ZDateTime.Empty : contact.OC_Birthday);
								}
								else if (result.NumberType == OrgCusCode.USACodeTypes.SocialSecurityNumber)
								{
									var contact = docAddress.Contact;
									result.Number = ISFDocAddress.GetSocialSecurityNumberDataFormat(cusCode.OK_CustomsRegNo, contact == null ? ZDateTime.Empty : contact.OC_Birthday);
								}
								else
								{
									result.Number = cusCode.OK_CustomsRegNo;
								}
							}
						}
					}

					return result;
				});
		}

		OrgCusCode GetCusCode(JobDocAddress docAddress, params ZString[] codesInOrderOfSignificance)
		{
			OrgCusCode cusCode = null;
			if (codesInOrderOfSignificance != null && codesInOrderOfSignificance.Length > 0 && docAddress != null && !docAddress.IsDeleted)
			{
				cusCode = GetCusCode(docAddress.Organisation, docAddress.Address, codesInOrderOfSignificance);
			}
			return cusCode;
		}

		OrgCusCode GetCusCode(OrgHeader org, OrgAddress orgAddress, ZString[] codesInOrderOfSignificance)
		{
			OrgCusCode cusCode = null;
			foreach (var codeType in codesInOrderOfSignificance)
			{
				if (OrgCusCode.GetPremisesAddressIsAllowed(codeType, Core.Constants.CountryCodes.UnitedStates))
				{
					cusCode = orgAddress?.CustomsCodes.GetOrgCusCodeObjectForCodeTypeAndCountry(codeType, Core.Constants.CountryCodes.UnitedStates);
				}
				else
				{
					cusCode = org?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(codeType, Core.Constants.CountryCodes.UnitedStates);
				}
				if (cusCode != null)
				{
					break;
				}
			}
			return cusCode;
		}
	}
}
