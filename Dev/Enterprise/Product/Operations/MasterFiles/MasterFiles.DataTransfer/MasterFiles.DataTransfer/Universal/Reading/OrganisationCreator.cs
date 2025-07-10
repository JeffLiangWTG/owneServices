using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
#if DEBUG
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
#endif
	public static class OrganisationCreator
	{
		/// <summary>
		/// Creates a new Organisation with a MainAddress containing addressData. Will return null if the addressData is not valid (missing Address Line 1 and 2).
		/// Note this method will need updating if the DB contraint rules for OrgHeader/OrgAddress change. Or change it to use validation and check for errors
		/// (will work with any DB changes but is slower).
		/// </summary>
		public static OrgHeader GetNewOrgHeader(OrganizationAddress addressData, UniversalObjectFactory factory, IXmlImportLogger logger)
		{
			Argument.NotNull(addressData, "addressData");
			return GetNewOrgHeaderCore(new OrganizationAddressFormatted(addressData), factory, logger);
		}

		internal static OrgHeader GetNewOrgHeaderForTest(OrganizationAddressFormatted formattedAddress, UniversalObjectFactory factory, IXmlImportLogger logger)
		{
			return GetNewOrgHeaderCore(formattedAddress, factory, logger, allowOrgCodeOverride: true);
		}

		static OrgHeader GetNewOrgHeaderCore(OrganizationAddressFormatted formattedAddress, UniversalObjectFactory factory, IXmlImportLogger logger, bool allowOrgCodeOverride = false)
		{
			Argument.NotNull(factory, "factory");
			Argument.NotNull(logger, "logger");

			OrgHeader result = null;
			bool addressLine1Missing = formattedAddress.Address1.GetValueOrDefault().IsEmpty;
			bool addressLine2Missing = formattedAddress.Address2.GetValueOrDefault().IsEmpty;

			if (!addressLine1Missing || !addressLine2Missing)
			{
				var portCode = formattedAddress.Port.GetUNLOCOAsUpperCase(factory.BOFactory);
				var baseCountryCode = formattedAddress.Country.GetCodeAsUpperCase();
				if (baseCountryCode.IsEmpty && portCode.Length == 5)
				{
					baseCountryCode = portCode.SubstringSafe(0, 2);
				}

				if (portCode.IsEmpty)
				{
					portCode = baseCountryCode;
				}

				result = factory.New<OrgHeader>();
				result.OH_FullName = formattedAddress.CompanyName.GetValueOrDefault();
				result.OH_RL_NKClosestPort = portCode;

				CreateMainAddress(formattedAddress, result);
				CreateOrgContact(formattedAddress, result);
				CreateOrgCusCodes(formattedAddress, result, baseCountryCode);

				if ((bool)RawDataRegistry.Instance.CanUserEditOrganisationCode.Value || allowOrgCodeOverride)
				{
					var codeFromXml = formattedAddress.OrganizationCode.GetValueOrDefault();
					if (!codeFromXml.IsEmpty)
					{
						result.OH_Code = formattedAddress.OrganizationCode.GetValueOrDefault();
					}
				}

				if (result.OH_Code.IsEmpty)
				{
					logger.Log(LogType.Error, Res.GetString("bdbedaf1-711b-499d-a9e0-91dc63d37964",
						"<OrganizationCode> is missing and could not be generated - Organization could not be created for <OrganizationAddress> with <AddressType> [{0}].", formattedAddress.AddressType));

					result.Delete();
					result = null;
				}
			}
			else
			{
				if (addressLine1Missing)
				{
					logger.Log(LogType.Error, Res.GetString("b9868fbd-6017-492c-a5d8-637c40b84807",
						"<Address1> is missing - Organization could not be created for <OrganizationAddress> with <AddressType> [{0}].", formattedAddress.AddressType));
				}
				if (addressLine2Missing)
				{
					logger.Log(LogType.Error, Res.GetString("3d049235-2ec0-4c58-9a96-294e55d0e26c",
						"<Address2> is missing - Organization could not be created for <OrganizationAddress> with <AddressType> [{0}].", formattedAddress.AddressType));
				}
			}

			return result;
		}

		#region CreateMainAddress

		static void CreateMainAddress(OrganizationAddressFormatted formattedAddress, OrgHeader org)
		{
			var orgAddress = org.MainAddress;

			if (!formattedAddress.Address1.GetValueOrDefault().IsEmpty)
			{
				orgAddress.OA_Address1 = formattedAddress.Address1.GetValueOrDefault();
				orgAddress.OA_Address2 = formattedAddress.Address2.GetValueOrDefault();
			}
			else // we only have line 2, set it to line 1.
			{
				orgAddress.OA_Address1 = formattedAddress.Address2.GetValueOrDefault();
			}

			orgAddress.OA_Code = formattedAddress.AddressShortCode.GetValueOrDefault();
			orgAddress.OA_City = formattedAddress.City.GetValueOrDefault();
			orgAddress.OA_PostCode = formattedAddress.Postcode.GetValueOrDefault();
			orgAddress.OA_RL_NKRelatedPortCode = org.OH_RL_NKClosestPort;
			orgAddress.OA_State = formattedAddress.State.GetValueOrDefault();
			orgAddress.OA_Email = formattedAddress.Email.GetValueOrDefault();
			orgAddress.OA_Fax = formattedAddress.Fax.GetValueOrDefault();
			orgAddress.OA_Mobile = formattedAddress.Mobile.GetValueOrDefault();
			orgAddress.OA_Phone = formattedAddress.Phone.GetValueOrDefault();
		}

		#endregion

		#region CreateOrgContact

		static void CreateOrgContact(OrganizationAddressFormatted formattedAddress, OrgHeader org)
		{
			var contactName = formattedAddress.Contact.GetValueOrDefault();
			if (!contactName.IsEmpty)
			{
				var orgContact = org.Contacts.AddNew();
				orgContact.OC_ContactName = contactName;
				orgContact.OC_Email = formattedAddress.Email.GetValueOrDefault();
				orgContact.OC_Fax = formattedAddress.Fax.GetValueOrDefault();
				orgContact.OC_Mobile = formattedAddress.Mobile.GetValueOrDefault();
				orgContact.OC_Phone = formattedAddress.Phone.GetValueOrDefault();
			}
		}

		#endregion

		#region CreateOrgCusCodes

		static void CreateOrgCusCodes(OrganizationAddressFormatted formattedAddress, OrgHeader org, ZString baseCountryCode)
		{
			if (formattedAddress.RegistrationNumberCollection != null)
			{
				foreach (var registrationNumberData in formattedAddress.RegistrationNumberCollection)
				{
					var countryCode = registrationNumberData.CountryOfIssue.GetCodeAsUpperCase();
					var typeCode = registrationNumberData.Type.GetCodeAsUpperCase();
					var registrationNumber = registrationNumberData.Value.GetValueOrDefault();

					AddOrgCusCodeIfValidAndNotDuplicate(countryCode, typeCode, registrationNumber, org);
				}
			}

			AddOrgCusCodeIfValidAndNotDuplicate(baseCountryCode, OrgCusCode.CodeTypes.UniversalNettingCode, formattedAddress.UniversalNettingCode.GetValueOrDefault(), org);
			AddOrgCusCodeIfValidAndNotDuplicate(baseCountryCode, OrgCusCode.CodeTypes.UniversalOfficeCode, formattedAddress.UniversalOfficeCode.GetValueOrDefault(), org);

			if (formattedAddress.GovRegNumType != null && formattedAddress.GovRegNumType.Code.HasValue)
			{
				AddOrgCusCodeIfValidAndNotDuplicate(baseCountryCode, formattedAddress.GovRegNumType.GetCodeAsUpperCase(), formattedAddress.GovRegNum.GetValueOrDefault(), org);
			}
		}

		static void AddOrgCusCodeIfValidAndNotDuplicate(ZString countryCode, ZString typeCode, ZString registrationNumber, OrgHeader orgHeader)
		{
			if (!countryCode.IsEmpty && !typeCode.IsEmpty && !registrationNumber.IsEmpty)
			{
				foreach (OrgCusCode customsCode in orgHeader.CustomsCodes)
				{
					if (customsCode.OK_CodeType == typeCode && customsCode.OK_RN_NKCodeCountry == countryCode)
					{
						return;
					}
				}

				var orgCusCodeMatch = orgHeader.CustomsCodes.AddNew();
				orgCusCodeMatch.OK_CodeType = typeCode;
				orgCusCodeMatch.OK_CustomsRegNo = registrationNumber;
				orgCusCodeMatch.OK_RN_NKCodeCountry = countryCode;
				if (orgCusCodeMatch.PremisesAddressIsAllowed)
				{
					orgCusCodeMatch.OK_OA_PremisesAddress = orgHeader.MainAddress.PK;
				}
			}
		}

		#endregion
	}
}
