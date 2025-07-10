using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class ISFDocAddressRequirementProviderTest : TestCaseWithFactory
	{
		public void TestGetFirmsCode()
		{
			var org = Factory.New<OrgHeader>();
			var address1 = org.Addresses.AddNew();
			address1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "ADDRESS1", Core.Constants.CountryCodes.UnitedStates);
			var address2 = org.Addresses.AddNew();
			address2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "ADDRESS2", Core.Constants.CountryCodes.UnitedStates);
			var header = Factory.New<CusISFHeader>();
			var docAddress = header.MainShipToParty;
			docAddress.OrganisationPK = org.PK;
			docAddress.E2_OA_Address = address1.PK;
			docAddress.E2_AddressOverride = true;
			AssertEquals("FIRMS is taken from address 1", "ADDRESS1", docAddress.E2_GovRegNum);
			docAddress.E2_AddressOverride = false;
			docAddress.E2_OA_Address = address2.PK;
			docAddress.E2_AddressOverride = true;
			AssertEquals("FIRMS is taken from address 1", "ADDRESS2", docAddress.E2_GovRegNum);
		}

		public void TestValidateAMSCharactersWhenOverrideAddress()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			var docAddress = header.BuyingParty;
			var message = @"Buying Party: Contact Name : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.";
			docAddress.E2_AddressOverride = true;
			docAddress.E2_GovRegNumType = OtherUSPartiesCodeTypeList.Codes.SocialSecurity;
			docAddress.E2_GovRegNum = "1234";
			docAddress.E2_Contact = "Î  Ï";
			AssertHasMessageErrorContaining(docAddress.E2_ContactInfo, message);
			docAddress.E2_GovRegNum = ZString.Empty;
			docAddress.E2_GovRegNumType = OtherUSPartiesCodeTypeList.Codes.DUNSPlus4;
			docAddress.E2_Contact = ZString.Empty;
			docAddress.E2_Contact = "Î  Ï";
			AssertHasMessageErrorContaining(docAddress.E2_ContactInfo, message);
			docAddress.E2_CompanyName = "Î  Ï";
			var message2 = @"Buying Party: Company Name : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.";
			AssertHasMessageErrorContaining(docAddress.E2_CompanyNameInfo, message2);
			docAddress.E2_Address1 = "Î  Ï";
			AssertHasMessageErrorContaining(docAddress.E2_Address1Info, @"Buying Party: Address Line 1 : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.");
			docAddress.E2_City = "Î  Ï";
			AssertHasMessageErrorContaining(docAddress.E2_CityInfo, @"Buying Party: City : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.");
			docAddress.E2_AddressOverride = false;
			docAddress.E2_GovRegNumType = OtherUSPartiesCodeTypeList.Codes.SocialSecurity;
			docAddress.E2_GovRegNum = "1234";
			docAddress.E2_Contact = "Î  Ï";
			AssertNoMessageErrorContaining(docAddress.E2_ContactInfo, message2);
		}

		public void TestMakingSureCNIIsTheLeastPreferableForGovRegNo()
		{
			AssertEquals("ECN is at the end of the list", ConsigneeCodeTypeList.GetListInOrderOfImportance()[ConsigneeCodeTypeList.GetListInOrderOfImportance().Length - 1], OrgCusCode.USACodeTypes.EncryptedConsigneeNumber);
			var provider = new ISFDocAddressRequirementProvider(Factory);
			AssertEquals("ECN is at the end of the list", provider.USCustomsCodesInOrderOfSignificance[provider.USCustomsCodesInOrderOfSignificance.Length - 1], OrgCusCode.USACodeTypes.EncryptedConsigneeNumber);
		}

		public void TestGetRegistrationNumberResultForShipToParty()
		{
			var header = Factory.New<CusISFHeader>();
			var shipToParty = Factory.New<OrgHeader>();
			header.MainShipToParty.OrganisationPK = shipToParty.PK;
			AssertEquals("", header.MainShipToParty.ISFRequirement.GetRegistrationNumberResult(header.MainShipToParty).RegistrationNumber);
			var cusCode1 = shipToParty.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "OH1234", Core.Constants.CountryCodes.UnitedStates);
			cusCode1.OK_OA_PremisesAddress = ZGuid.Empty;
			var address2 = shipToParty.Addresses.AddNew();
			address2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "OA5678", Core.Constants.CountryCodes.UnitedStates);
			header.MainShipToParty.E2_OA_Address = address2.PK;
			AssertEquals("OA5678", header.MainShipToParty.ISFRequirement.GetRegistrationNumberResult(header.MainShipToParty).RegistrationNumber);
		}

		public void TestShipToPartyDocAddressValidateGovRegNo()
		{
			var header = Factory.New<CusISFHeader>();
			var shipToParty = Factory.New<OrgHeader>();
			AssertEquals("", header.MainShipToParty.ISFRequirement.GetRegistrationNumberResult(header.MainShipToParty).RegistrationNumber);
			var cusCode = shipToParty.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "OH1234", Core.Constants.CountryCodes.UnitedStates);
			cusCode.OK_OA_PremisesAddress = shipToParty.MainAddress.PK;
			var docAddress = header.DocAddresses.FindByDocAddressType(DocAddressType.ShipToParty);
			header.MainShipToParty.OrganisationPK = shipToParty.PK;
			AssertNoWarningContaining(docAddress.E2_GovRegNumInfo, "DUNS number is no longer supported by CBP");
			docAddress.E2_AddressOverride = true;
			docAddress.Validation.ValidateE2_GovRegNum();
			AssertHasWarningContaining(docAddress.E2_GovRegNumInfo, "DUNS number is no longer supported by CBP");
		}

		public void TestCreateNewISFDocAddressRequirementDefaultAddressTypeIsNoDefault()
		{
			var header = Factory.New<CusISFHeader>();
			IDocAddresses docAddresses = header;
			AssertEquals(AddressType.NoDefault, header.MainShipToParty.DefaultAddressType);
			AssertEquals(AddressType.NoDefault, docAddresses.GetDocAddressRequirement(DocAddressType.BuyingParty).DefaultAddressType);
			AssertEquals(AddressType.NoDefault, docAddresses.GetDocAddressRequirement(DocAddressType.SellingParty).DefaultAddressType);
			AssertEquals(AddressType.NoDefault, docAddresses.GetDocAddressRequirement(DocAddressType.Consolidator).DefaultAddressType);
			AssertEquals(AddressType.NoDefault, docAddresses.GetDocAddressRequirement(DocAddressType.ScheduledContainerStuffingLocation).DefaultAddressType);
		}

		public void TestGetRegistrationNumberResult()
		{
			var header = Factory.New<CusISFHeader>();
			var consolidator = Factory.New<OrgHeader>();
			header.Consolidator.OrganisationPK = consolidator.PK;
			AssertEquals("", header.Consolidator.ISFRequirement.GetRegistrationNumberResult(header.Consolidator).RegistrationNumber);
			var cusCode1 = consolidator.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "OH1234", Core.Constants.CountryCodes.UnitedStates);
			cusCode1.OK_OA_PremisesAddress = ZGuid.Empty;
			var address2 = consolidator.Addresses.AddNew();
			address2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "OA5678", Core.Constants.CountryCodes.UnitedStates);
			header.Consolidator.E2_OA_Address = address2.PK;
			AssertEquals("OA5678", header.Consolidator.ISFRequirement.GetRegistrationNumberResult(header.Consolidator).RegistrationNumber);
		}

		public void TestGetRegistrationNumberResultForUSParty()
		{
			var header = Factory.New<CusISFHeader>();
			var buyingParty = Factory.New<OrgHeader>();
			header.BuyingParty.OrganisationPK = buyingParty.PK;
			AssertEquals("", header.BuyingParty.ISFRequirement.GetRegistrationNumberResult(header.BuyingParty).RegistrationNumber);
			var cusCode1 = buyingParty.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "OH1234", Core.Constants.CountryCodes.UnitedStates);
			cusCode1.OK_OA_PremisesAddress = ZGuid.Empty;
			var address2 = buyingParty.Addresses.AddNew();
			address2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "OA5678", Core.Constants.CountryCodes.UnitedStates);
			header.BuyingParty.E2_OA_Address = address2.PK;
			AssertEquals("OA5678", header.BuyingParty.ISFRequirement.GetRegistrationNumberResult(header.BuyingParty).RegistrationNumber);
		}

		public void TestValidationForConsolidator()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			header.BF_ActionReasonCode = ActionReasonCodeList.Codes.CompliantTransaction;
			var docAddress = header.Consolidator;
			string message = ValidationConstants.Organisation.OrganisationPKRequired(docAddress.AddressCaption, SubmissionTypeList.Codes.ISF10);
			docAddress.OrganisationPK = org.PK;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			docAddress.OrganisationPK = ZGuid.Empty;
			AssertHasMessageError(docAddress.OrganisationPKInfo, message);
			header.BF_ActionReasonCode = ActionReasonCodeList.Codes.FlexibleRange;
			docAddress.OrganisationPK = org.PK;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			docAddress.OrganisationPK = ZGuid.Empty;
			AssertHasMessageError(docAddress.OrganisationPKInfo, message);
			header.BF_ActionReasonCode = ActionReasonCodeList.Codes.FlexibleTiming;
			docAddress.OrganisationPK = org.PK;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			docAddress.OrganisationPK = ZGuid.Empty;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			header.BF_ActionReasonCode = ActionReasonCodeList.Codes.FlexibleRangeAndFlexibleTiming;
			docAddress.OrganisationPK = org.PK;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			docAddress.OrganisationPK = ZGuid.Empty;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			header.BF_EntryType = SubmissionTypeList.Codes.ISF5;
			header.BF_ActionReasonCode = ActionReasonCodeList.Codes.CompliantTransaction;
			docAddress.OrganisationPK = org.PK;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			docAddress.OrganisationPK = ZGuid.Empty;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			header.BF_ActionReasonCode = ActionReasonCodeList.Codes.FlexibleRange;
			docAddress.OrganisationPK = org.PK;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			docAddress.OrganisationPK = ZGuid.Empty;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			header.BF_ActionReasonCode = ActionReasonCodeList.Codes.FlexibleTiming;
			docAddress.OrganisationPK = org.PK;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			docAddress.OrganisationPK = ZGuid.Empty;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			header.BF_ActionReasonCode = ActionReasonCodeList.Codes.FlexibleRangeAndFlexibleTiming;
			docAddress.OrganisationPK = org.PK;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			docAddress.OrganisationPK = ZGuid.Empty;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			AssertNoMessageError(docAddress.OrganisationPKInfo, ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
			docAddress.OrganisationPK = OrgHeader.UnmatchedOrganisationPK;
			AssertHasMessageError(docAddress.OrganisationPKInfo, ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
		}

		public void TestValidationForConsignee()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			header.BF_ShipmentType = ZString.Empty;
			IDocAddresses iDocAddress = header;
			var addressRequirement = iDocAddress.GetDocAddressRequirement(DocAddressType.ConsigneeAddress);
			ISFDocAddress docAddress = header.DocAddresses.CreateWithRequirement(addressRequirement) as ISFDocAddress;
			docAddress.E2_AddressOverride = true;
			docAddress.E2_RN_NKCountryCode = ZString.Empty;
			docAddress.E2_CompanyName = "A";
			AssertNoNotifications(docAddress.E2_CompanyNameInfo);
			docAddress.E2_CompanyName = ZString.Empty;
			AssertNoNotifications(docAddress.E2_CompanyNameInfo);
			docAddress.E2_Address1 = "A";
			AssertNoNotifications(docAddress.E2_Address1Info);
			docAddress.E2_Address1 = ZString.Empty;
			AssertNoNotifications(docAddress.E2_Address1Info);
			docAddress.E2_Address2 = "A";
			AssertNoNotifications(docAddress.E2_Address2Info);
			docAddress.E2_Address2 = ZString.Empty;
			AssertNoNotifications(docAddress.E2_Address2Info);
			docAddress.E2_City = "A";
			AssertNoNotifications(docAddress.E2_CityInfo);
			docAddress.E2_City = ZString.Empty;
			AssertNoNotifications(docAddress.E2_CityInfo);
			docAddress.E2_Contact = "A";
			AssertNoNotifications(docAddress.E2_ContactInfo);
			docAddress.E2_Contact = ZString.Empty;
			AssertNoNotifications(docAddress.E2_ContactInfo);
			docAddress.E2_Email = "A@A.COM";
			AssertNoNotifications(docAddress.E2_EmailInfo);
			docAddress.E2_Email = ZString.Empty;
			AssertNoNotifications(docAddress.E2_EmailInfo);
			docAddress.E2_Fax = "A";
			AssertNoNotifications(docAddress.E2_FaxInfo);
			docAddress.E2_Fax = ZString.Empty;
			AssertNoNotifications(docAddress.E2_FaxInfo);
			docAddress.E2_IsResidential = ZBool.True;
			AssertNoNotifications(docAddress.E2_IsResidentialInfo);
			docAddress.E2_IsResidential = ZBool.False;
			AssertNoNotifications(docAddress.E2_IsResidentialInfo);
			docAddress.E2_Mobile = "A";
			AssertNoNotifications(docAddress.E2_MobileInfo);
			docAddress.E2_Mobile = ZString.Empty;
			AssertNoNotifications(docAddress.E2_MobileInfo);
			docAddress.E2_PassportCountryOfIssue = "A";
			AssertNoNotifications(docAddress.E2_PassportCountryOfIssueInfo);
			docAddress.E2_PassportCountryOfIssue = ZString.Empty;
			AssertNoNotifications(docAddress.E2_PassportCountryOfIssueInfo);
			docAddress.E2_PassportDateOfBirth = ZDate.BrettsBirthday;
			AssertNoNotifications(docAddress.E2_PassportDateOfBirthInfo);
			docAddress.E2_PassportDateOfBirth = ZDate.Empty;
			AssertNoNotifications(docAddress.E2_PassportDateOfBirthInfo);
			docAddress.E2_PassportID = "A";
			AssertNoNotifications(docAddress.E2_PassportIDInfo);
			docAddress.E2_PassportID = ZString.Empty;
			AssertNoNotifications(docAddress.E2_PassportIDInfo);
			docAddress.E2_Phone = "A";
			AssertNoNotifications(docAddress.E2_PhoneInfo);
			docAddress.E2_Phone = ZString.Empty;
			AssertNoNotifications(docAddress.E2_PhoneInfo);
			docAddress.E2_Postcode = "A";
			AssertNoNotifications(docAddress.E2_PostcodeInfo);
			docAddress.E2_Postcode = ZString.Empty;
			AssertNoNotifications(docAddress.E2_PostcodeInfo);
			docAddress.E2_RN_NKCountryCode = "AU";
			AssertNoNotifications(docAddress.E2_RN_NKCountryCodeInfo);
			docAddress.E2_RN_NKCountryCode = ZString.Empty;
			AssertNoNotifications(docAddress.E2_RN_NKCountryCodeInfo);
			docAddress.E2_State = "A";
			AssertNoErrors(docAddress.E2_StateInfo);
			docAddress.E2_State = ZString.Empty;
			AssertNoNotifications(docAddress.E2_StateInfo);
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			AssertEquals(Factory.GetCachedValue<ConsigneeCodeTypeList>(), docAddress.Lookups.GovRegNumTypes);
			docAddress.E2_GovRegNumType = "ZZ";
			AssertNoMessageErrorContaining(docAddress.E2_GovRegNumTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(docAddress.E2_GovRegNumTypeInfo, ListValidation.InvalidCodeMessageError);
			docAddress.E2_GovRegNumType = ZString.Empty;
			AssertHasMessageErrorContaining(docAddress.E2_GovRegNumTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(docAddress.E2_GovRegNumTypeInfo, ListValidation.InvalidCodeMessageError);
			header.BF_EntryType = SubmissionTypeList.Codes.ISF5;
			docAddress.E2_GovRegNumType = "ZZ";
			AssertNoMessageErrorContaining(docAddress.E2_GovRegNumTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(docAddress.E2_GovRegNumTypeInfo, ListValidation.InvalidCodeMessageError);
			docAddress.E2_GovRegNumType = ZString.Empty;
			AssertNoMessageErrorContaining(docAddress.E2_GovRegNumTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(docAddress.E2_GovRegNumTypeInfo, ListValidation.InvalidCodeMessageError);
			ShipmentTypeList shipmentTypeList = new ShipmentTypeList();
			ZString[] shipmentTypesThatSupportPassport = new ZString[] { ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects, ShipmentTypeList.Codes.DiplomaticShipment, ShipmentTypeList.Codes.Carnet };
			shipmentTypeList.RemoveCode(ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects);
			shipmentTypeList.RemoveCode(ShipmentTypeList.Codes.DiplomaticShipment);
			shipmentTypeList.RemoveCode(ShipmentTypeList.Codes.Carnet);
			foreach (ICodeDescription codeType in Factory.GetCachedValue<ConsigneeCodeTypeList>())
			{
				header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
				docAddress.E2_GovRegNumType = codeType.Code;
				AssertNoMessageErrorContaining(docAddress.E2_GovRegNumTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(docAddress.E2_GovRegNumTypeInfo, ListValidation.InvalidCodeMessageError);
				if (OrgCusCode.CodeTypes.PassportID == codeType.Code)
				{
					docAddress.E2_GovRegNum = "A";
					docAddress.E2_GovRegNum = ZString.Empty; // cause changes
					AssertNoNotifications(docAddress.E2_GovRegNumInfo);
					foreach (ICodeDescription pair in shipmentTypeList)
					{
						header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
						header.BF_ShipmentType = pair.Code;
						docAddress.E2_GovRegNumType = ZString.Empty;
						docAddress.E2_GovRegNumType = codeType.Code;
						AssertHasMessageError(docAddress.E2_GovRegNumTypeInfo, ValidationConstants.Header.PassportIsOnlyUsedFor03Or05Or06ShipmentType);
						header.BF_EntryType = SubmissionTypeList.Codes.ISF5;
						header.BF_ShipmentType = pair.Code;
						docAddress.E2_GovRegNumType = ZString.Empty;
						docAddress.E2_GovRegNumType = codeType.Code;
						AssertNoMessageError(docAddress.E2_GovRegNumTypeInfo, ValidationConstants.Header.PassportIsOnlyUsedFor03Or05Or06ShipmentType);
					}

					header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
					foreach (ZString shipmentType in shipmentTypesThatSupportPassport)
					{
						header.BF_ShipmentType = shipmentType;
						docAddress.E2_GovRegNumType = ZString.Empty;
						docAddress.E2_GovRegNumType = codeType.Code;
						AssertNoMessageError(docAddress.E2_GovRegNumTypeInfo, ValidationConstants.Header.PassportIsOnlyUsedFor03Or05Or06ShipmentType);
					}
				}
				else if (codeType.Code == OrgCusCode.USACodeTypes.SocialSecurityNumber)
				{
					header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
					docAddress.E2_GovRegNum = ZString.Empty;
					AssertNoMessageErrors(docAddress.E2_GovRegNumInfo);
					header.BF_EntryType = SubmissionTypeList.Codes.ISF5;
					docAddress.E2_GovRegNum = ZString.Empty;
					AssertNoMessageErrors(docAddress.E2_GovRegNumInfo);
				}
				else
				{
					string messsageError = ValidationConstants.Header.ValueIsRequiredForEntity(codeType.Description);
					header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
					docAddress.E2_GovRegNum = "ZDS";
					AssertNoMessageError(docAddress.E2_GovRegNumInfo, messsageError);
					docAddress.E2_GovRegNum = ZString.Empty;
					AssertHasMessageError(docAddress.E2_GovRegNumInfo, messsageError);
					header.BF_EntryType = SubmissionTypeList.Codes.ISF5;
					docAddress.E2_GovRegNum = "ZDS";
					AssertNoMessageError(docAddress.E2_GovRegNumInfo, messsageError);
					docAddress.E2_GovRegNum = ZString.Empty;
					AssertNoMessageError(docAddress.E2_GovRegNumInfo, messsageError);
				}
			}

			Dictionary<string, string> validRegs = new Dictionary<string, string>();
			validRegs.Add(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-3456789XY");
			validRegs.Add(OrgCusCode.USACodeTypes.CBPAssignedNumber, "078888-12345");
			validRegs.Add(OrgCusCode.USACodeTypes.EncryptedConsigneeNumber, "-L8E9QX17S-P");
			foreach (KeyValuePair<string, string> pair in validRegs)
			{
				header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
				docAddress.E2_GovRegNumType = pair.Key;
				docAddress.E2_GovRegNum = "ZZ#";
				AssertHasMessageErrors(docAddress.E2_GovRegNumInfo);
				docAddress.E2_GovRegNum = pair.Value;
				AssertNoMessageErrors(docAddress.E2_GovRegNumInfo);
				header.BF_EntryType = SubmissionTypeList.Codes.ISF5;
				docAddress.E2_GovRegNumType = pair.Key;
				docAddress.E2_GovRegNum = "ZZ#";
				AssertNoMessageErrors(docAddress.E2_GovRegNumInfo);
				docAddress.E2_GovRegNum = pair.Value;
				AssertNoMessageErrors(docAddress.E2_GovRegNumInfo);
			}

			docAddress.E2_AddressOverride = false;
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			docAddress.OrganisationPK = org.PK;
			AssertHasMessageError(docAddress.OrganisationPKInfo, ValidationConstants.Organisation.OrganizationRequiresEIN_CBN_ECN_SSN_PAS);
			docAddress.OrganisationPK = ZGuid.Empty;
			AssertNoMessageError(docAddress.OrganisationPKInfo, ValidationConstants.Organisation.OrganizationRequiresEIN_CBN_ECN_SSN_PAS);
			header.BF_EntryType = SubmissionTypeList.Codes.ISF5;
			docAddress.OrganisationPK = org.PK;
			AssertNoMessageError(docAddress.OrganisationPKInfo, ValidationConstants.Organisation.OrganizationRequiresEIN_CBN_ECN_SSN_PAS);
			var cusCode = org.CustomsCodes.AddNew();
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			foreach (KeyValuePair<string, string> pair in validRegs)
			{
				cusCode.OK_CodeType = pair.Key;
				cusCode.OK_CustomsRegNo = pair.Value;
				docAddress.OrganisationPK = org.PK;
				AssertNoMessageError(docAddress.OrganisationPKInfo, ValidationConstants.Organisation.OrganizationRequiresEIN_CBN_ECN_SSN_PAS);
				docAddress.E2_AddressOverride = true;
				AssertEquals(pair.Key, docAddress.E2_GovRegNumType);
				AssertEquals(pair.Value, docAddress.E2_GovRegNum);
			}

			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.PassportID;
			cusCode.OK_CustomsRegNo = "KD32342";
			docAddress.OrganisationPK = org.PK;
			docAddress.E2_AddressOverride = true;
			AssertEquals(OrgCusCode.CodeTypes.PassportID, docAddress.E2_GovRegNumType);
			AssertEquals("KD32342  " + Core.Constants.CountryCodes.Australia, docAddress.E2_GovRegNum);
			AssertEquals("KD32342", docAddress.E2_PassportID);
			AssertEquals(Core.Constants.CountryCodes.Australia, docAddress.E2_PassportCountryOfIssue);
			AssertEquals(ZDate.Empty, docAddress.E2_PassportDateOfBirth);
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			docAddress.E2_PassportID = ZString.Empty;
			AssertHasMessageError(docAddress.E2_PassportIDInfo, ValidationConstants.Organisation.IDIsRequiredForPassport);
			docAddress.E2_PassportID = "#@12";
			AssertNoMessageError(docAddress.E2_PassportIDInfo, ValidationConstants.Organisation.IDIsRequiredForPassport);
			AssertHasMessageErrors(docAddress.E2_PassportIDInfo);
			docAddress.E2_PassportID = "KD32342";
			AssertNoMessageError(docAddress.E2_PassportIDInfo, ValidationConstants.Organisation.IDIsRequiredForPassport);
			AssertNoMessageErrors(docAddress.E2_PassportIDInfo);
			docAddress.E2_PassportCountryOfIssue = ZString.Empty;
			AssertHasMessageError(docAddress.E2_PassportCountryOfIssueInfo, ValidationConstants.Organisation.CountryOfIssueIsRequiredForPassport);
			docAddress.E2_PassportCountryOfIssue = "#@";
			AssertNoMessageError(docAddress.E2_PassportCountryOfIssueInfo, ValidationConstants.Organisation.CountryOfIssueIsRequiredForPassport);
			AssertHasMessageErrorContaining(docAddress.E2_PassportCountryOfIssueInfo, ListValidation.InvalidCodeMessageError);
			docAddress.E2_PassportCountryOfIssue = Core.Constants.CountryCodes.Australia;
			AssertNoMessageError(docAddress.E2_PassportCountryOfIssueInfo, ValidationConstants.Organisation.CountryOfIssueIsRequiredForPassport);
			AssertNoMessageErrorContaining(docAddress.E2_PassportCountryOfIssueInfo, ListValidation.InvalidCodeMessageError);
			docAddress.E2_PassportDateOfBirth = ZDate.Empty;
			AssertHasMessageError(docAddress.E2_PassportDateOfBirthInfo, ValidationConstants.Organisation.DateOfBirthIsRequiredForPassport);
			AssertNoMessageError(docAddress.E2_PassportDateOfBirthInfo, ValidationConstants.Header.DateOfBirthIsInvalid);
			docAddress.E2_PassportDateOfBirth = ZDate.Today.AddDays(1);
			AssertNoMessageError(docAddress.E2_PassportDateOfBirthInfo, ValidationConstants.Organisation.DateOfBirthIsRequiredForPassport);
			AssertHasMessageError(docAddress.E2_PassportDateOfBirthInfo, ValidationConstants.Header.DateOfBirthIsInvalid);
			docAddress.E2_PassportDateOfBirth = ZDate.Today;
			AssertNoMessageError(docAddress.E2_PassportDateOfBirthInfo, ValidationConstants.Organisation.DateOfBirthIsRequiredForPassport);
			AssertNoMessageError(docAddress.E2_PassportDateOfBirthInfo, ValidationConstants.Header.DateOfBirthIsInvalid);
			header.BF_EntryType = SubmissionTypeList.Codes.ISF5;
			docAddress.E2_PassportID = ZString.Empty;
			AssertNoMessageError(docAddress.E2_PassportIDInfo, ValidationConstants.Organisation.IDIsRequiredForPassport);
			AssertNoMessageErrors(docAddress.E2_PassportIDInfo);
			docAddress.E2_PassportID = "#@12";
			AssertNoMessageError(docAddress.E2_PassportIDInfo, ValidationConstants.Organisation.IDIsRequiredForPassport);
			AssertNoMessageErrors(docAddress.E2_PassportIDInfo);
			docAddress.E2_PassportCountryOfIssue = ZString.Empty;
			AssertNoMessageError(docAddress.E2_PassportCountryOfIssueInfo, ValidationConstants.Organisation.CountryOfIssueIsRequiredForPassport);
			AssertNoMessageErrorContaining(docAddress.E2_PassportCountryOfIssueInfo, ListValidation.InvalidCodeMessageError);
			docAddress.E2_PassportCountryOfIssue = "#@";
			AssertNoMessageError(docAddress.E2_PassportCountryOfIssueInfo, ValidationConstants.Organisation.CountryOfIssueIsRequiredForPassport);
			AssertNoMessageErrorContaining(docAddress.E2_PassportCountryOfIssueInfo, ListValidation.InvalidCodeMessageError);
			docAddress.E2_PassportDateOfBirth = ZDate.Empty;
			AssertNoMessageError(docAddress.E2_PassportDateOfBirthInfo, ValidationConstants.Organisation.DateOfBirthIsRequiredForPassport);
			AssertNoMessageError(docAddress.E2_PassportDateOfBirthInfo, ValidationConstants.Header.DateOfBirthIsInvalid);
			docAddress.E2_PassportDateOfBirth = ZDate.Today.AddDays(1);
			AssertNoMessageError(docAddress.E2_PassportDateOfBirthInfo, ValidationConstants.Organisation.DateOfBirthIsRequiredForPassport);
			AssertNoMessageError(docAddress.E2_PassportDateOfBirthInfo, ValidationConstants.Header.DateOfBirthIsInvalid);
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			cusCode.OK_CustomsRegNo = "123-12-1234";
			docAddress.OrganisationPK = org.PK;
			docAddress.E2_AddressOverride = true;
			AssertEquals(OrgCusCode.USACodeTypes.SocialSecurityNumber, docAddress.E2_GovRegNumType);
			AssertEquals("123-12-1234", docAddress.E2_GovRegNum);
			AssertEquals("123-12-1234", docAddress.E2_SocialSecurityNumber);
			AssertEquals(ZDate.Empty, docAddress.E2_SocialSecurityNumberDateOfBirth);
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			docAddress.E2_SocialSecurityNumberDateOfBirth = ZDate.BrettsBirthday;
			docAddress.E2_SocialSecurityNumber = ZString.Empty;
			AssertHasMessageError(docAddress.E2_SocialSecurityNumberInfo, ValidationConstants.Organisation.NumberIsRequiredForSocialSecurityNumber);
			docAddress.E2_SocialSecurityNumber = "#@12";
			AssertNoMessageError(docAddress.E2_SocialSecurityNumberInfo, ValidationConstants.Organisation.NumberIsRequiredForSocialSecurityNumber);
			AssertHasMessageErrors(docAddress.E2_SocialSecurityNumberInfo);
			docAddress.E2_SocialSecurityNumber = "987-56-7896";
			AssertNoMessageError(docAddress.E2_SocialSecurityNumberInfo, ValidationConstants.Organisation.NumberIsRequiredForSocialSecurityNumber);
			AssertNoMessageErrors(docAddress.E2_SocialSecurityNumberInfo);
			docAddress.E2_SocialSecurityNumberDateOfBirth = ZDate.Empty;
			AssertHasMessageError(docAddress.E2_SocialSecurityNumberDateOfBirthInfo, ValidationConstants.Organisation.DateOfBirthIsRequiredForSocialSecurityNumber);
			AssertNoMessageError(docAddress.E2_SocialSecurityNumberDateOfBirthInfo, ValidationConstants.Header.DateOfBirthIsInvalid);
			docAddress.E2_SocialSecurityNumberDateOfBirth = ZDate.Today.AddDays(1);
			AssertNoMessageError(docAddress.E2_SocialSecurityNumberDateOfBirthInfo, ValidationConstants.Organisation.DateOfBirthIsRequiredForSocialSecurityNumber);
			AssertHasMessageError(docAddress.E2_SocialSecurityNumberDateOfBirthInfo, ValidationConstants.Header.DateOfBirthIsInvalid);
			docAddress.E2_SocialSecurityNumberDateOfBirth = ZDate.Today;
			AssertNoMessageError(docAddress.E2_SocialSecurityNumberDateOfBirthInfo, ValidationConstants.Organisation.DateOfBirthIsRequiredForSocialSecurityNumber);
			AssertNoMessageError(docAddress.E2_SocialSecurityNumberDateOfBirthInfo, ValidationConstants.Header.DateOfBirthIsInvalid);
			header.BF_EntryType = SubmissionTypeList.Codes.ISF5;
			docAddress.E2_SocialSecurityNumber = ZString.Empty;
			AssertNoMessageError(docAddress.E2_SocialSecurityNumberInfo, ValidationConstants.Organisation.NumberIsRequiredForSocialSecurityNumber);
			AssertNoMessageErrors(docAddress.E2_SocialSecurityNumberInfo);
			docAddress.E2_SocialSecurityNumber = "#@12";
			AssertNoMessageError(docAddress.E2_SocialSecurityNumberInfo, ValidationConstants.Organisation.NumberIsRequiredForSocialSecurityNumber);
			AssertNoMessageErrors(docAddress.E2_SocialSecurityNumberInfo);
			docAddress.E2_SocialSecurityNumberDateOfBirth = ZDate.Empty;
			AssertNoMessageError(docAddress.E2_SocialSecurityNumberDateOfBirthInfo, ValidationConstants.Organisation.DateOfBirthIsRequiredForSocialSecurityNumber);
			AssertNoMessageError(docAddress.E2_SocialSecurityNumberDateOfBirthInfo, ValidationConstants.Header.DateOfBirthIsInvalid);
			docAddress.E2_SocialSecurityNumberDateOfBirth = ZDate.Today.AddDays(1);
			AssertNoMessageError(docAddress.E2_SocialSecurityNumberDateOfBirthInfo, ValidationConstants.Organisation.DateOfBirthIsRequiredForSocialSecurityNumber);
			AssertNoMessageError(docAddress.E2_SocialSecurityNumberDateOfBirthInfo, ValidationConstants.Header.DateOfBirthIsInvalid);
		}

		public void TestValidationForScheduledContainerStuffingLocation()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			header.BF_ActionReasonCode = ActionReasonCodeList.Codes.CompliantTransaction;
			var docAddress = header.StuffingLocation;
			string message = ValidationConstants.Organisation.OrganisationPKRequired(docAddress.AddressCaption, SubmissionTypeList.Codes.ISF10);
			docAddress.OrganisationPK = org.PK;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			docAddress.OrganisationPK = ZGuid.Empty;
			AssertHasMessageError(docAddress.OrganisationPKInfo, message);
			header.BF_ActionReasonCode = ActionReasonCodeList.Codes.FlexibleRange;
			docAddress.OrganisationPK = org.PK;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			docAddress.OrganisationPK = ZGuid.Empty;
			AssertHasMessageError(docAddress.OrganisationPKInfo, message);
			header.BF_ActionReasonCode = ActionReasonCodeList.Codes.FlexibleTiming;
			docAddress.OrganisationPK = org.PK;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			docAddress.OrganisationPK = ZGuid.Empty;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			header.BF_ActionReasonCode = ActionReasonCodeList.Codes.FlexibleRangeAndFlexibleTiming;
			docAddress.OrganisationPK = org.PK;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			docAddress.OrganisationPK = ZGuid.Empty;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			header.BF_EntryType = SubmissionTypeList.Codes.ISF5;
			header.BF_ActionReasonCode = ActionReasonCodeList.Codes.CompliantTransaction;
			docAddress.OrganisationPK = org.PK;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			docAddress.OrganisationPK = ZGuid.Empty;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			header.BF_ActionReasonCode = ActionReasonCodeList.Codes.FlexibleRange;
			docAddress.OrganisationPK = org.PK;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			docAddress.OrganisationPK = ZGuid.Empty;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			header.BF_ActionReasonCode = ActionReasonCodeList.Codes.FlexibleTiming;
			docAddress.OrganisationPK = org.PK;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			docAddress.OrganisationPK = ZGuid.Empty;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			header.BF_ActionReasonCode = ActionReasonCodeList.Codes.FlexibleRangeAndFlexibleTiming;
			docAddress.OrganisationPK = org.PK;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			docAddress.OrganisationPK = ZGuid.Empty;
			AssertNoMessageError(docAddress.OrganisationPKInfo, message);
			AssertNoMessageError(docAddress.OrganisationPKInfo, ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
			docAddress.OrganisationPK = OrgHeader.UnmatchedOrganisationPK;
			AssertHasMessageError(docAddress.OrganisationPKInfo, ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
		}

		public void TestDocAddressValidateCountry()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.MainAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var org2 = Factory.New<OrgHeader>();
			var header = Factory.New<CusISFHeader>();
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			var supportedAddressTypes = new List<DocAddressType>(((IDocAddresses)header).SupportedAddressTypes);
			supportedAddressTypes.Remove(DocAddressType.ConsigneeAddress);
			foreach (DocAddressType addressType in supportedAddressTypes)
			{
				var docAddress = header.DocAddresses.FindOrCreateWithDocAddressType(addressType);
				docAddress.E2_OA_Address = org1.MainAddress.PK;
				docAddress.E2_AddressOverride = true;
				docAddress.E2_GovRegNum = ZString.Empty;
				docAddress.E2_RN_NKCountryCode = "Z!";
				AssertNoMessageError(addressType.ToString(), docAddress.E2_RN_NKCountryCodeInfo, ValidationConstants.Organisation.CountryRequireWhenNoRegistration);
				AssertHasErrorContaining(docAddress.E2_RN_NKCountryCodeInfo, "Country/Region Code");
				docAddress.E2_RN_NKCountryCode = ZString.Empty;
				AssertHasMessageError(docAddress.E2_RN_NKCountryCodeInfo, ValidationConstants.Organisation.CountryRequireWhenNoRegistration);
				docAddress.E2_RN_NKCountryCode = "MX";
				AssertNoErrorContaining(docAddress.E2_RN_NKCountryCodeInfo, "Country/Region Code");
				docAddress.E2_AddressOverride = false;
				docAddress.E2_OA_Address = org2.MainAddress.PK;
				AssertHasMessageError(docAddress.E2_RN_NKCountryCodeInfo, ValidationConstants.Organisation.MissingUNLOCO);
				docAddress.E2_AddressOverride = true;
				var org = Factory.Load<OrgHeader>(docAddress.OrganisationPKIncludesMiscOrg);
				org.OH_RL_NKClosestPort = "";
				docAddress.E2_RN_NKCountryCode = "ZZ";
				AssertNoMessageError(docAddress.E2_RN_NKCountryCodeInfo, ValidationConstants.Organisation.MissingUNLOCO);
				docAddress.E2_AddressOverride = false;
				docAddress.E2_OA_Address = org1.MainAddress.PK;
				AssertNoMessageError(docAddress.E2_RN_NKCountryCodeInfo, ValidationConstants.Organisation.MissingUNLOCO);
			}
		}

		public void TestBuyingPartyAndSellingParty()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			var buyingPartyAddress = header.BuyingParty;
			var sellingPartyAddress = header.SellingParty;
			AssertEquals(Factory.GetCachedValue<OtherUSPartiesCodeTypeList>(), buyingPartyAddress.Lookups.GovRegNumTypes);
			AssertEquals(Factory.GetCachedValue<OtherUSPartiesCodeTypeList>(), sellingPartyAddress.Lookups.GovRegNumTypes);
			OrgHeader org = Factory.New<OrgHeader>();
			OrgCusCode cusCode = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "AAEU");
			org.OH_RL_NKClosestPort = "AUSYD";
			string[] list = new string[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber, OrgCusCode.USACodeTypes.EncryptedConsigneeNumber, OrgCusCode.USACodeTypes.DataUniversalNumberingSystemPlus4, OrgCusCode.CodeTypes.DataUniversalNumberingSystem };
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			buyingPartyAddress.OrganisationPK = ZGuid.Empty;
			AssertEquals(ZString.Empty, buyingPartyAddress.E2_GovRegNum);
			buyingPartyAddress.OrganisationPK = org.PK;
			var cusCodeForGobRegNum = buyingPartyAddress.Address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.SocialSecurityNumber, "AAEU", Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("AAEU       ", buyingPartyAddress.E2_GovRegNum);
			AssertNoMessageError(buyingPartyAddress.OrganisationPKInfo, ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
			buyingPartyAddress.OrganisationPK = OrgHeader.UnmatchedOrganisationPK;
			AssertHasMessageError(buyingPartyAddress.OrganisationPKInfo, ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
			cusCode.OK_CustomsRegNo = "123-45-6789";
			cusCodeForGobRegNum.OK_CustomsRegNo = "123-45-6789";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "TEST CONTACT";
			contact.OC_Birthday = ZDateTime.Today;
			buyingPartyAddress.OrganisationPK = org.PK;
			buyingPartyAddress.E2_Contact = "TEST CONTACT";
			AssertNotNull(buyingPartyAddress.Contact);
			AssertNoMessageErrors(buyingPartyAddress.OrganisationPKInfo);
			cusCode.OK_CustomsRegNo = "AAEU";
			cusCodeForGobRegNum.OK_CustomsRegNo = "AAEU";
			sellingPartyAddress.OrganisationPK = ZGuid.Empty;
			AssertEquals(ZString.Empty, sellingPartyAddress.E2_GovRegNum);
			sellingPartyAddress.OrganisationPK = org.PK;
			AssertEquals("AAEU       ", sellingPartyAddress.E2_GovRegNum);
			AssertNoMessageError(sellingPartyAddress.OrganisationPKInfo, ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
			sellingPartyAddress.OrganisationPK = OrgHeader.UnmatchedOrganisationPK;
			AssertHasMessageError(sellingPartyAddress.OrganisationPKInfo, ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
			foreach (string codeType in list)
			{
				cusCode.OK_CodeType = codeType;
				cusCodeForGobRegNum.OK_CodeType = codeType;
				buyingPartyAddress.OrganisationPK = ZGuid.Empty;
				AssertEquals(ZString.Empty, buyingPartyAddress.E2_GovRegNum);
				buyingPartyAddress.OrganisationPK = org.PK;
				AssertEquals("AAEU", buyingPartyAddress.E2_GovRegNum);
				AssertNoMessageError(buyingPartyAddress.OrganisationPKInfo, ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
				buyingPartyAddress.OrganisationPK = OrgHeader.UnmatchedOrganisationPK;
				AssertHasMessageError(buyingPartyAddress.OrganisationPKInfo, ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
				sellingPartyAddress.OrganisationPK = ZGuid.Empty;
				AssertEquals(ZString.Empty, sellingPartyAddress.E2_GovRegNum);
				sellingPartyAddress.OrganisationPK = org.PK;
				AssertEquals("AAEU", sellingPartyAddress.E2_GovRegNum);
				AssertNoMessageError(sellingPartyAddress.OrganisationPKInfo, ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
				sellingPartyAddress.OrganisationPK = OrgHeader.UnmatchedOrganisationPK;
				AssertHasMessageError(sellingPartyAddress.OrganisationPKInfo, ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
			}
		}
	}
}
