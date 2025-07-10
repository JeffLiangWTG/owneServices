using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Types;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.OrgPatternMatching;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.MasterFiles.DataTransfer.OrgMatching
{
	public static class OrganizationAddressTransformHelper
	{
		public static DeduplicationOrgHeader GetDeduplicationOrgHeader(OrganizationAddress organizationAddress, BusinessObjectFactory factory)
		{
			Argument.NotNull(organizationAddress, "OrganizationAddress organizationAddress");

			return GetDeduplicationOrgHeader(new OrganizationAddressFormatted(organizationAddress), factory);
		}

		public static DeduplicationOrgHeader GetDeduplicationOrgHeader(OrganizationAddressFormatted formattedAddress, BusinessObjectFactory factory)
		{
			Argument.NotNull(formattedAddress, "OrganizationAddressFormatted formattedAddress");

			var converter = new OrganisationConverter(formattedAddress);
			var organizationMatchingData = converter.GetMatchingData(factory);
			return GetDeduplicationOrgHeader(organizationMatchingData);
		}

		public static DeduplicationOrgHeader GetDeduplicationOrgHeader(IOrgHeaderForMatching orgHeaderForMatching)
		{
			Argument.NotNull(orgHeaderForMatching, "IOrgHeaderForMatching orgHeaderForMatching");

			var dedupOrgHeader = new DeduplicationOrgHeader(GetStringFromNullable(orgHeaderForMatching.OH_FullName))
			{
				OH_PK = Guid.NewGuid(),
				OH_Code = GetStringFromNullable(orgHeaderForMatching.OH_Code),
				CountryCode = orgHeaderForMatching.OH_RL_NKClosestPort.SubstringSafe(0, 2)
			};

			if (orgHeaderForMatching.Addresses.Any())
			{
				var address = orgHeaderForMatching.Addresses.First();
				SetAddressesInfo(dedupOrgHeader, address);
				SetContactsInfo(dedupOrgHeader, address);
			}

			SetCusCodesInfo(dedupOrgHeader, orgHeaderForMatching.CustomsCodes);

			return dedupOrgHeader;
		}

		static void SetAddressesInfo(DeduplicationOrgHeader dedupOrgHeader, IMatchingAddress address)
		{
			var mainAddress = new DeduplicationOrgAddress
			{
				OA_PK = Guid.NewGuid(),
				OA_OH = dedupOrgHeader.OH_PK,
				OA_RL_NKRelatedPortCode = GetStringFromNullable(address.OA_RL_NKRelatedPortCode),
				OA_RN_NKCountryCode = GetStringFromNullable(address.CountryCode),
				OA_AdditionalAddressInformation = GetStringFromNullable(address.OA_AdditionalAddressInformation),
				OA_Address1 = GetStringFromNullable(address.OA_Address1),
				OA_Address2 = GetStringFromNullable(address.OA_Address2),
				OA_City = GetStringFromNullable(address.OA_City),
				OA_Code = GetStringFromNullable(address.OA_Code),
				OA_PostCode = GetStringFromNullable(address.OA_PostCode),
				OA_State = GetStringFromNullable(address.OA_State),
				OA_Email = GetStringFromNullable(address.OA_Email),
				OA_Fax = GetStringFromNullable(address.OA_Fax),
				OA_Phone = GetStringFromNullable(address.OA_Phone),
				OA_Mobile = string.Empty,
				OA_ValidationStatus = "NTC"
			};
			dedupOrgHeader.OrgAddresses = new[] { mainAddress };
		}

		static void SetCusCodesInfo(DeduplicationOrgHeader dedupOrgHeader, List<IOrgCusCodeForMatching> customsCodes)
		{
			dedupOrgHeader.CusCodes = new List<IOrgCusCode>();
			foreach (IOrgCusCodeForMatching customsCode in customsCodes)
			{
				dedupOrgHeader.CusCodes.Add(new DeduplicationOrgCusCode()
				{
					OK_PK = Guid.NewGuid(),
					OK_CodeType = customsCode.OK_CodeType,
					OK_CustomsRegNo = customsCode.OK_CustomsRegNo,
					OK_RN_NKCodeCountry = customsCode.OK_RN_NKCodeCountry,
					OK_OH = dedupOrgHeader.PK,
				});
			}
		}

		static void SetContactsInfo(DeduplicationOrgHeader dedupOrgHeader, IMatchingAddress address)
		{
			if (!string.IsNullOrEmpty(address.Contact))
			{
				var contact = new DeduplicationOrgContact(GetStringFromNullable(address.Contact))
				{
					OC_PK = Guid.NewGuid(),
					OC_Email = GetStringFromNullable(address.OA_Email),
					OC_Fax = GetStringFromNullable(address.OA_Fax),
					OC_Mobile = GetStringFromNullable(address.OA_Mobile),
					OC_Phone = GetStringFromNullable(address.OA_Phone),
					OC_OH = dedupOrgHeader.OH_PK,
					OC_OtherPhone = string.Empty,
					OC_HomePhone = string.Empty,
				};

				dedupOrgHeader.OrgContacts = new[] { contact };
			}
		}

		static string GetStringFromNullable(ZString? value)
		{
			return value ?? string.Empty;
		}
	}
}
