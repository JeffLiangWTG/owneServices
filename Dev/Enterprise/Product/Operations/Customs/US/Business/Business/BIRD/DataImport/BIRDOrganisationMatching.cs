using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	static class BIRDOrganisationMatching
	{
		public static OrgAddress GetOrganisationAddress(BusinessObjectFactory factory, OrgMatchedCustomsRegNoType customsNoType, ZString customsNumber, ZString organisationType, INotifications notifications)
		{
			var result = GetRelatedOrganisationRecord(factory, customsNoType, customsNumber, organisationType, notifications);
			return result is OrgAddress address ? address : (result is OrgHeader header ? header.MainAddress : null);
		}

		public static ZGuid GetCustomsRecordOrMainAddressOfOrganisation(BusinessObjectFactory factory, OrgMatchedCustomsRegNoType customsNoType, ZString customsNumber, ZString organisationType, INotifications notifications)
		{
			var addressPK = ZGuid.Empty;
			var orgHeader = GetOrganisation(factory, customsNoType, customsNumber, organisationType, notifications);
			if (orgHeader != null)
			{
				if (customsNoType == OrgMatchedCustomsRegNoType.EIN)
				{
					var customsAddress = orgHeader.AddressesActive.FirstOrDefault(x => x.IsCustomsAddress);
					addressPK = customsAddress != null ? customsAddress.PK : orgHeader.MainAddress.PK;
				}
				else
				{
					addressPK = orgHeader.MainAddress.PK;
				}
			}

			return addressPK;
		}

		public static OrgHeader GetOrganisation(BusinessObjectFactory factory, OrgMatchedCustomsRegNoType customsNoType, ZString customsNumber, ZString organisationType, INotifications notifications)
		{
			var result = GetRelatedOrganisationRecord(factory, customsNoType, customsNumber, organisationType, notifications);
			return result is OrgHeader header ? header : (result is OrgAddress address ? address.Header : null);
		}

		public static OrgHeader GetOrganisation(BusinessObjectFactory factory, ZString customsNoType, ZString customsNumber, ZString organisationType, INotifications notifications)
		{
			var result = GetRelatedOrganisationRecord(factory, customsNoType, customsNumber, organisationType, notifications);
			return result is OrgHeader header ? header : (result is OrgAddress address ? address.Header : null);
		}

		public static OrgAddress GetOrganisationAddress(BusinessObjectFactory factory, OrgMatchedCustomsRegNoType customsNoType, ZString customsNumber)
		{
			var result = GetRelatedOrganisationRecord(factory, customsNoType, customsNumber, ZString.Empty, null);
			return result is OrgAddress address ? address : (result is OrgHeader header ? header.MainAddress : null);
		}

		public static OrgHeader GetOrganisation(BusinessObjectFactory factory, OrgMatchedCustomsRegNoType customsNoType, ZString customsNumber)
		{
			return GetOrganisation(factory, customsNoType, customsNumber, ZString.Empty, null);
		}

		static BusinessObject GetRelatedOrganisationRecord(BusinessObjectFactory factory, OrgMatchedCustomsRegNoType customsNoType, ZString customsNumber, ZString organisationType, INotifications notifications)
		{
			BusinessObject result = null;

			if (!customsNumber.IsEmpty)
			{
				ZString codeType = GetCodeTypeForNumber(customsNoType, customsNumber);
				if (codeType.IsEmpty)
				{
					if (notifications != null)
					{
						notifications.AddWarning("Unknown format for EIN/SSN/CBP number/MID/FEI, " + customsNumber + " and system couldn't set a value for " + organisationType);
					}
				}
				else
				{
					result = GetOrganizationOrAddress(factory, codeType, customsNumber, organisationType, notifications);
				}
			}

			return result;
		}

		static BusinessObject GetRelatedOrganisationRecord(BusinessObjectFactory factory, ZString codeType, ZString customsNumber, ZString organisationType, INotifications notifications)
		{
			BusinessObject result = null;

			if (!customsNumber.IsEmpty)
			{
				BusinessObject[] results = GetLinkedObjectsForThisCustomsRegNo(factory, codeType, customsNumber);

				if (results.Length == 1)
				{
					result = results[0];
				}
				else if (results.Length > 1)
				{
					result = results.FirstOrDefault();
					if (result != null && notifications != null)
					{
						string warning = organisationType + ": There is more than one organization found with this number, " + customsNumber + ". System will match first active organization, please review data.";
						notifications.AddWarning(warning);
					}
				}

				if (result == null && notifications != null)
				{
					string warning = results.Length == 0 ?
							GetNoOrganizationMatchMsg(organisationType, customsNumber)
							: organisationType + ": There is more than one organization found with this number, " + customsNumber + ". You should select the right organization for the field. For future importing, please consider merging the organizations.";

					notifications.AddWarning(warning);
				}
			}

			return result;
		}

		internal static string GetNoOrganizationMatchMsg(ZString organisationType, ZString customsNumber)
		{
			return string.Format("{0}: There is no organization found with this number, {1}. You should create an organization record with this number and set the organization for the field.",
				organisationType, customsNumber);
		}

		static BusinessObject[] GetLinkedObjectsForThisCustomsRegNo(BusinessObjectFactory factory, ZString codeType, ZString customsNumber)
		{
			var codes = new OrgCusCode.Loader(factory).Load(Core.Constants.CountryCodes.UnitedStates, codeType, customsNumber);
			var premisesAddressIsAllowed = (codes.FirstOrDefault()?.PremisesAddressIsAllowed ?? false);
			var result = new List<BusinessObject>();

			foreach (OrgCusCode code in codes)
			{
				var linkedObject = premisesAddressIsAllowed ?
					(code.PremisesAddress is OrgAddress premisesAddress && premisesAddress.OA_IsActive && premisesAddress.Header.OH_IsActive ? premisesAddress : null) :
					(code.Header is OrgHeader header && header.OH_IsActive ? (BusinessObject)header : null);

				if (linkedObject != null)
				{
					result.Add(linkedObject);
				}

				if (result.Count > 1)
				{
					break;
				}
			}

			return result.ToArray();
		}

		public static ZString GetCodeTypeForNumber(OrgMatchedCustomsRegNoType customsNoType, ZString customsNumber)
		{
			ZString result = ZString.Empty;

			if (customsNoType == OrgMatchedCustomsRegNoType.ECN || customsNoType == OrgMatchedCustomsRegNoType.EIN)
			{
				if (Enterprise.Customs.US.Business.CBPAssignedNumberValidator.IsValidCBPAssignedNumber(customsNumber))
				{
					result = OrgCusCode.USACodeTypes.CBPAssignedNumber;
				}
				else if (Enterprise.Customs.US.Business.EmployerIdentificationNumberValidator.IsValidEIN(customsNumber))
				{
					result = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
				}
				else if (Enterprise.Customs.US.Business.SocialSecurityNumberValidator.IsValidSSN(customsNumber))
				{
					result = OrgCusCode.USACodeTypes.SocialSecurityNumber;
				}
			}
			else if (customsNoType == OrgMatchedCustomsRegNoType.MID)
			{
				result = OrgCusCode.USACodeTypes.ManufacturerID;
			}
			else if (customsNoType == OrgMatchedCustomsRegNoType.FEI)
			{
				result = OrgCusCode.CodeTypes.FDAEstablishmentIdentifier;
			}

			return result;
		}

		public static ZString GetCustomsNumberTypeEntityIdentifierQualifier(ZString entityIdentifierQualifier)
		{
			var result = ZString.Empty;
			switch (entityIdentifierQualifier)
			{
				case EntityIdentifierQualifierList.Codes.EmployerIdentificationNumber:
					result = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
					break;
				case EntityIdentifierQualifierList.Codes.CBPAssignedNumber:
					result = OrgCusCode.USACodeTypes.CBPAssignedNumber;
					break;
				case EntityIdentifierQualifierList.Codes.SocialSecurityNumber:
					result = OrgCusCode.USACodeTypes.SocialSecurityNumber;
					break;
				case EntityIdentifierQualifierList.Codes.CBPEncryptedConsigneeID:
					result = OrgCusCode.USACodeTypes.EncryptedConsigneeNumber;
					break;
				case EntityIdentifierQualifierList.Codes.FIRMS:
					result = OrgCusCode.USACodeTypes.FIRMSCode;
					break;
				case "MID":
					result = OrgCusCode.USACodeTypes.ManufacturerID;
					break;
			}

			return result;
		}

		static OrgAddress FindMatchedOrgAddress(Dictionary<string, OrgAddress> organizationDictionary, BusinessObjectFactory factory, ZString organizationCode, ZString companyName, ZString address1, ZString tableName, INotifications notifications)
		{
			OrgAddress orgAddress = null;

			if (!companyName.IsEmpty && !address1.IsEmpty)
			{
				var key = GetCompanyAddressKey(companyName, address1);
				if (!organizationDictionary.TryGetValue(key, out orgAddress))
				{
					var hasMultipleOrgMatched = false;
					var organizationQuery = new ZDBOnlyQuery(typeof(OrgHeader));
					organizationQuery.AddToFilter(OrgHeaderSchema.OH_FullName, companyName);
					organizationQuery.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
					var addressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH);
					addressSubQuery.AddToFilter(OrgAddressSchema.OA_Address1, address1);
					addressSubQuery.AddToFilter(OrgAddressSchema.OA_IsActive, true);
					organizationQuery.AddSubQuery(addressSubQuery, JoinCondition.And);
					var organizations = factory.Load<OrgHeader>(organizationQuery);
					if (organizations != null && organizations.Length > 0)
					{
						hasMultipleOrgMatched = organizations.Length > 1;
						orgAddress = organizations.FirstOrDefault().Addresses.OfType<OrgAddress>().FirstOrDefault(x => x.OA_Address1.EqualsIgnoringCase(address1));
					}

					if (orgAddress == null)
					{
						var addressQuery = new ZQuery(OrgAddressSchema.OA_CompanyNameOverride, companyName);
						addressQuery.AddToFilter(OrgAddressSchema.OA_IsActive, true);
						addressQuery.AddToFilter(OrgAddressSchema.OA_Address1, address1);
						var orgAddresses = factory.Load<OrgAddress>(addressQuery);
						if (orgAddresses != null && orgAddresses.Length > 0)
						{
							hasMultipleOrgMatched = orgAddresses.Length > 1;
							orgAddress = orgAddresses.FirstOrDefault(x => x.OA_Address1.EqualsIgnoringCase(address1));
						}
					}

					if (hasMultipleOrgMatched)
					{
						notifications.AddWarning(ZString.Format(organizationCode + ": There is more than one organization address found with company name [{0}] and address [{1}] when import {2} data. System will match first active organization address, please review data.", companyName, address1, tableName));
					}
					else if (orgAddress != null)
					{
						organizationDictionary.Add(key, orgAddress);
					}
				}
			}

			return orgAddress;
		}

		static string GetCompanyAddressKey(string companyName, string address1) => $"ORG|{companyName}|{address1}";

		static OrgAddress FindMatchedOrgAddressOrCreateOrganization(Dictionary<string, OrgAddress> organizationDictionary, BusinessObjectFactory factory, ZString organizationCode, ZString customsNoType, ZString customsNumber, ZString companyName, ZString address1, ZString address2, ZString countryCode, ZString city, ZString postCode, ZString tableName, INotifications notifications)
		{
			var orgAddress = FindMatchedOrgAddress(organizationDictionary, factory, organizationCode, companyName, address1, tableName, notifications);
			if (orgAddress == null)
			{
				if (customsNoType == OrgCusCode.USACodeTypes.ManufacturerID)
				{
					if (!customsNumber.IsEmpty)
					{
						orgAddress = OrganisationCreator.CreateManufacturerAndSendNameAddressQueryMessage(factory, customsNumber);
						if (orgAddress != null)
						{
							notifications.AddWarning(organizationCode + ": " + ZString.Format(OrganisationCreator.ManufacturerCreated, customsNumber));
							organizationDictionary.Add(GetCustomsTypeAndNumberKey(customsNoType, customsNumber), orgAddress);
						}
						else if (!customsNumber.IsLettersAndNumbersOnlyOrEmpty)
						{
							notifications.AddWarning(organizationCode + ": " + ZString.Format(OrganisationCreator.NoManufacturerCreatedAsMIDInvalid, customsNumber));
						}
					}
				}
				else
				{
					var warningMessage = ZString.Format(": There is no matched address found for company name [{0}] and address1 [{1}] when import {2} data. System has created an organization record with this company and address1 automatically.", companyName, address1, tableName);

					if (companyName.IsEmpty || address1.IsEmpty)
					{
						var customsNumberSuffix = customsNumber.IsEmpty ? string.Empty : ":" + customsNumber;
						companyName = $"AUTO CREATED FROM {customsNoType}{customsNumberSuffix}";
						address1 = $"AUTO CREATED ADDRESS FROM {customsNoType}{customsNumberSuffix}";
						address2 = "AUTO CREATED ADDRES 2";
						city = "AUTO CREATED CITY";
						warningMessage = ZString.Format(": There is no organization matching this {0}, {1} when import {2} data. System has created an organization record named [{3}] automatically.", customsNoType, customsNumber, tableName, companyName);
					}

					var key = GetCompanyAddressKey(companyName, address1);
					if (!organizationDictionary.TryGetValue(key, out orgAddress))
					{
						var createdOrgHeader = CreateOrganisation(factory, companyName, address1, address2, countryCode, city, postCode);
						orgAddress = createdOrgHeader.MainAddress;
						organizationDictionary.Add(key, orgAddress);
						if (!customsNoType.IsEmpty && !customsNumber.IsEmpty)
						{
							var customsCode = createdOrgHeader.CustomsCodes.AddNew(customsNoType, customsNumber);
							organizationDictionary.Add(GetCustomsTypeAndNumberKey(customsNoType, customsNumber), orgAddress);
							if (customsCode.PremisesAddressIsAllowed)
							{
								customsCode.OK_OA_PremisesAddress = orgAddress.PK;
							}
						}
					}
					notifications.AddWarning(organizationCode + warningMessage);
				}
			}

			return orgAddress;
		}

		public static OrgAddress FindMatchedOrgAddress(BusinessObjectFactory factory, ZString organizationCode, ZString customsNoType, ZString customsNumber, ZString companyName, ZString address1, ZString address2, ZString countryCode, ZString city, ZString postCode, ZString tableName, INotifications notifications)
		{
			var organizationDictionary = factory.GetCachedValue("USBIRDOrganizationDictionary", () => new Dictionary<string, OrgAddress>());
			var key = GetCustomsTypeAndNumberKey(customsNoType, customsNumber);
			if (!organizationDictionary.TryGetValue(key, out var result))
			{
				var organizationOrAddress = GetOrganizationOrAddress(factory, customsNoType, customsNumber, organizationCode, notifications);
				if (organizationOrAddress != null)
				{
					if (organizationOrAddress is OrgAddress orgAddress)
					{
						result = orgAddress;
					}
					else if (organizationOrAddress is OrgHeader organization)
					{
						if (organizationCode.EqualsIgnoringCase("Consignee") || organizationCode.EqualsIgnoringCase("Sold To Party"))
						{
							var customsAddress = organization.AddressesActive.FirstOrDefault(x => x.IsCustomsAddress);
							result = customsAddress ?? organization.MainAddress;
						}
						else
						{
							result = organization.MainAddress;
						}
					}
				}

				if (result == null)
				{
					result = FindMatchedOrgAddressOrCreateOrganization(organizationDictionary, factory, organizationCode, customsNoType, customsNumber, companyName, address1, address2, countryCode, city, postCode, tableName, notifications);
				}
				else
				{
					organizationDictionary.Add(key, result);
				}
			}
			return result;
		}

		static string GetCustomsTypeAndNumberKey(string customsNoType, string customsNumber) => $"CUS|{customsNoType}|{customsNumber}";

		static BusinessObject GetOrganizationOrAddress(BusinessObjectFactory factory, ZString customsNoType, ZString customsNumber, ZString organizationCode, INotifications notifications)
		{
			var possibleCodeTypesForEIN = new List<ZString> { OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber };
			var isValidType = possibleCodeTypesForEIN.Remove(customsNoType);
			var result = GetRelatedOrganisationRecord(factory, customsNoType, customsNumber, organizationCode, isValidType ? null : notifications);
			if (result == null && isValidType)
			{
				result = GetRelatedOrganisationRecord(factory, possibleCodeTypesForEIN[0], customsNumber, organizationCode, null);
				if (result == null)
				{
					result = GetRelatedOrganisationRecord(factory, possibleCodeTypesForEIN[1], customsNumber, organizationCode, notifications);
				}
			}

			return result;
		}

		static OrgHeader CreateOrganisation(BusinessObjectFactory factory, ZString name, ZString address1, ZString address2, ZString countryCode, ZString city, ZString postCode)
		{
			var unloco = !countryCode.IsEmpty ? countryCode : name;
			var orgHeader = OrganisationCreator.CreateOrganisation(factory, name, address1, address2, city, ZString.Empty, postCode, unloco.Trim().Left(2), true, true);
			orgHeader.MainAddress.OA_RN_NKCountryCode = countryCode.Left(orgHeader.MainAddress.OA_RN_NKCountryCodeInfo.MaxLength);
			orgHeader.MainAddress.OA_City = city.Left(orgHeader.MainAddress.OA_CityInfo.MaxLength);
			return orgHeader;
		}
	}
}
