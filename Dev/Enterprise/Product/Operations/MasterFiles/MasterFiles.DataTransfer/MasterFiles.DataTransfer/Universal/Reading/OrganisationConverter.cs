using System.Collections;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	class OrganisationConverter
	{
		internal OrganisationConverter(OrganizationAddress addressData)
		{
			addressDataReaderObject = new OrganizationAddressFormatted(Argument.NotNull(addressData, "OrganizationAddress addressData"));
		}

		internal OrganisationConverter(OrganizationAddressFormatted addressDataReaderObject)
		{
			this.addressDataReaderObject = Argument.NotNull(addressDataReaderObject, "OrganizationAddressReaderObject addressDataReaderObject");
		}

		readonly OrganizationAddressFormatted addressDataReaderObject;

		internal OrgHeaderForMatching GetMatchingData(BusinessObjectFactory factory)
		{
			var portCode = addressDataReaderObject.Port.GetUNLOCOAsUpperCase(factory);
			var baseCountryCode = addressDataReaderObject.Country.GetCodeAsUpperCase();
			if (baseCountryCode.IsEmpty && portCode.Length == 5)
			{
				baseCountryCode = portCode.SubstringSafe(0, 2);
			}

			if (portCode.IsEmpty)
			{
				portCode = baseCountryCode;
			}

			var orgHeaderMatch = new OrgHeaderForMatching(new BusinessObjectFactory());
			orgHeaderMatch.OH_FullName = addressDataReaderObject.CompanyName.GetValueOrDefault();
			orgHeaderMatch.OH_RL_NKClosestPort = portCode;
			orgHeaderMatch.OH_Code = addressDataReaderObject.OrganizationCode.GetValueOrDefault();

			var orgAddressMatch = new OrgAddressForMatching();
			orgHeaderMatch.Addresses.Add(orgAddressMatch);
			orgAddressMatch.OA_Address1 = addressDataReaderObject.Address1.GetValueOrDefault();
			orgAddressMatch.OA_Address2 = addressDataReaderObject.Address2.GetValueOrDefault();
			orgAddressMatch.OA_Code = addressDataReaderObject.AddressShortCode.GetValueOrDefault();
			orgAddressMatch.OA_City = addressDataReaderObject.City.GetValueOrDefault();
			orgAddressMatch.OA_PostCode = addressDataReaderObject.Postcode.GetValueOrDefault();
			orgAddressMatch.OA_RL_NKRelatedPortCode = portCode;
			orgAddressMatch.OA_State = addressDataReaderObject.State.GetValueOrDefault();
			orgAddressMatch.OA_Email = addressDataReaderObject.Email.GetValueOrDefault();
			orgAddressMatch.OA_Fax = addressDataReaderObject.Fax.GetValueOrDefault();
			orgAddressMatch.OA_Mobile = addressDataReaderObject.Mobile.GetValueOrDefault();
			orgAddressMatch.OA_Phone = addressDataReaderObject.Phone.GetValueOrDefault();
			orgAddressMatch.OA_AdditionalAddressInformation = addressDataReaderObject.AdditionalAddressInformation.GetValueOrDefault();
			orgAddressMatch.Contact = addressDataReaderObject.Contact.GetValueOrDefault();
			orgAddressMatch.CountryCode = addressDataReaderObject.Country?.Code ?? string.Empty;

			if (addressDataReaderObject.RegistrationNumberCollection != null)
			{
				foreach (var registrationNumberData in addressDataReaderObject.RegistrationNumberCollection)
				{
					var countryCode = registrationNumberData.CountryOfIssue != null ? registrationNumberData.CountryOfIssue.Code.GetValueOrDefault() : ZString.Empty;
					var typeCode = registrationNumberData.Type != null ? registrationNumberData.Type.Code.GetValueOrDefault() : ZString.Empty;
					var registrationNumber = registrationNumberData.Value.GetValueOrDefault();

					AddOrgCusCodeMatchIfValidAndNotDuplicate(countryCode, typeCode, registrationNumber, orgHeaderMatch.CustomsCodes);
				}
			}

			AddOrgCusCodeMatchIfValidAndNotDuplicate(baseCountryCode, OrgCusCode.CodeTypes.UniversalNettingCode, addressDataReaderObject.UniversalNettingCode.GetValueOrDefault(), orgHeaderMatch.CustomsCodes);
			AddOrgCusCodeMatchIfValidAndNotDuplicate(baseCountryCode, OrgCusCode.CodeTypes.UniversalOfficeCode, addressDataReaderObject.UniversalOfficeCode.GetValueOrDefault(), orgHeaderMatch.CustomsCodes);

			if (addressDataReaderObject.GovRegNumType != null && addressDataReaderObject.GovRegNumType.Code.HasValue)
			{
				AddOrgCusCodeMatchIfValidAndNotDuplicate(baseCountryCode, addressDataReaderObject.GovRegNumType.Code.Value, addressDataReaderObject.GovRegNum.GetValueOrDefault(), orgHeaderMatch.CustomsCodes);
			}

			return orgHeaderMatch;
		}

		static void AddOrgCusCodeMatchIfValidAndNotDuplicate(ZString countryCode, ZString typeCode, ZString registrationNumber, IList customsCodes)
		{
			if (OrgCusCodeIsValid(countryCode, typeCode, registrationNumber))
			{
				foreach (OrgCusCodeForMatching customsCode in customsCodes)
				{
					if (customsCode.OK_CodeType == typeCode && customsCode.OK_RN_NKCodeCountry == countryCode)
					{
						return;
					}
				}

				var orgCusCodeMatch = new OrgCusCodeForMatching();
				customsCodes.Add(orgCusCodeMatch);
				orgCusCodeMatch.OK_CodeType = typeCode;
				orgCusCodeMatch.OK_CustomsRegNo = registrationNumber;
				orgCusCodeMatch.OK_RN_NKCodeCountry = countryCode;
			}
		}

		static bool OrgCusCodeIsValid(ZString countryCode, ZString typeCode, ZString registrationNumber)
		{
			if (typeCode == OrgCusCode.CodeTypes.ContainerChainCommunityCode)
			{
				return !registrationNumber.IsEmpty;
			}

			return !countryCode.IsEmpty && !typeCode.IsEmpty && !registrationNumber.IsEmpty;
		}
	}
}
