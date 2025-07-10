using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	public class AdaptersHelper
	{
		public AdaptersHelper()
		{
		}

		#region Find Customs Codes

		public OrgAddress FindOrganisationAddressByCusCode(string codeType, string code, BusinessObjectFactory factory)
		{
			OrgAddress result = null;
			var codesRef = new OrgCusCode.Loader(factory).Load(Core.Constants.CountryCodes.UnitedStates, codeType, code);

			if (codesRef.Length >= 1)
			{
				result = codesRef[0].PremisesAddress;
			}

			return result;
		}

		public OrgHeader FindOrganisationByCusCode(string codeType, string code, BusinessObjectFactory factory)
		{
			OrgHeader result = null;
			var codesRef = new OrgCusCode.Loader(factory).Load(Core.Constants.CountryCodes.UnitedStates, codeType, code);

			if (codesRef.Length >= 1)
			{
				result = codesRef[0].Header;
			}

			return result;
		}

		#endregion

		public void SetOrgDetailsToBusinessObjectOrganization(object item, IValueObjectImportContext context, ZPropertyInfo info)
		{
			ZGuid orgOrAddressPK = ZGuid.Empty;

			if (item is Xsd.Organisation)
			{
				orgOrAddressPK = context.FindOrCreateTempOrganisationPK((Xsd.Organisation)item, null, OrganisationTypes.None);
			}
			else if (item is string)
			{
				ZString customsNumber = item as string;
				ZString cusCodeType = ZString.Empty;

				if (Enterprise.Customs.US.Business.CBPAssignedNumberValidator.IsValidCBPAssignedNumber(customsNumber))
				{
					cusCodeType = OrgCusCode.USACodeTypes.CBPAssignedNumber;
				}
				else if (Enterprise.Customs.US.Business.EmployerIdentificationNumberValidator.IsValidEIN(customsNumber))
				{
					cusCodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
				}
				else if (Enterprise.Customs.US.Business.SocialSecurityNumberValidator.IsValidSSN(customsNumber))
				{
					cusCodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
				}

				if (!cusCodeType.IsEmpty)
				{
					OrgHeader org = FindOrganisationByCusCode(cusCodeType, customsNumber, context.Factory);
					orgOrAddressPK = org != null ? org.PK : ZGuid.Empty;
				}
			}
			else
			{
				Xsd.Organisation consigneeOrganisation = null;

				var lineUltimateConsigneeItem = item as Xsd.USInvoiceLineUltimateConsignee;
				if (lineUltimateConsigneeItem != null)
				{
					consigneeOrganisation = lineUltimateConsigneeItem.Item as Xsd.Organisation;
				}
				else
				{
					var declUltimateConsigneeItem = item as Xsd.USDeclarationOrganisationsUltimateConsignee;
					if (declUltimateConsigneeItem != null)
					{
						consigneeOrganisation = declUltimateConsigneeItem.Item as Xsd.Organisation;
					}
				}

				if (consigneeOrganisation != null)
				{
					orgOrAddressPK = GetOrgAddressPK(consigneeOrganisation, context);
				}
			}

			info.Value = orgOrAddressPK;
		}

		#region Set Organisation or Org Address Details

		ZGuid GetOrgAddressPK(Xsd.Organisation organisation, IValueObjectImportContext context)
		{
			var orgAddress = ZGuid.Empty;
			var org = context.FindOrganisation(organisation, null, OrganisationTypes.None);
			if (org != null)
			{
				var selectedAddress = GetAddressWithSequence1(organisation.OrganisationDetails.Addresses);
				if (selectedAddress != null)
				{
					foreach (OrgAddress address in org.Addresses)
					{
						if (address.OA_Address1 == selectedAddress.AddressLine1 && address.OA_Address2 == selectedAddress.AddressLine2)
						{
							orgAddress = address.PK;
						}
					}
				}
			}

			return orgAddress;
		}

		public void SetOrgAddressDetails(Xsd.Organisation organisation, IValueObjectImportContext context, ZPropertyInfo addressInfo)
		{
			var orgAddressPK = GetOrgAddressPK(organisation, context);
			addressInfo.Value = orgAddressPK;
		}

		public void FindOrCreateOrgFromMID(object item, IValueObjectImportContext context, ZPropertyInfo info)
		{
			FindOrCreateOrgFromRegistrationTypeIfNeeded(item, context, info, OrgCusCode.USACodeTypes.ManufacturerID, CreateNewManufacturerAndGetAddress);
		}

		public void FindOrCreateOrgFromRegistrationTypeIfNeeded(object item, IValueObjectImportContext context, ZPropertyInfo info, ZString registrationType, Func<IValueObjectImportContext, ZString, OrgAddress> createNewOrganisationAndGetAddress)
		{
			OrgAddress orgAddress = null;

			if (item is Xsd.Organisation)
			{
				var xsdOrg = (Xsd.Organisation)item;
				var org = context.FindOrCreateTempOrganisation(xsdOrg, null, OrganisationTypes.None) as OrgHeader;
				if (org != null)
				{
					var selectedAddress = GetAddressWithSequence1(xsdOrg.OrganisationDetails.Addresses);

					if (selectedAddress != null)
					{
						foreach (OrgAddress address in org.Addresses)
						{
							if (address.OA_Address1 == selectedAddress.AddressLine1 && address.OA_Address2 == selectedAddress.AddressLine2)
							{
								orgAddress = address;

								if (address.CustomsCodes.GetCustomsRegNo(registrationType, Core.Constants.CountryCodes.UnitedStates).IsEmpty)
								{
									var orgCusCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(registrationType, Core.Constants.CountryCodes.UnitedStates);
									if (orgCusCode != null)
									{
										orgCusCode.OK_OA_PremisesAddress = address.PK;
									}
								}
								break;
							}
						}
					}
				}
			}
			else if (item is string)
			{
				ZString registrationCode = item as string;
				if (registrationCode.Length > USMIDQuery.Schema.US_MIDMaxLength)
				{
					registrationCode = registrationCode.Left(USMIDQuery.Schema.US_MIDMaxLength);
					context.Notify(new InfoNotification(string.Format("MID exceeds {0} characters in length and therefore will be truncated.", USMIDQuery.Schema.US_MIDMaxLength)));
				}

				orgAddress = FindOrganisationAddressByCusCode(registrationType, registrationCode, context.Factory);

				if (orgAddress == null && createNewOrganisationAndGetAddress != null)
				{
					orgAddress = createNewOrganisationAndGetAddress(context, registrationCode);
				}
			}

			if (orgAddress != null)
			{
				info.Value = orgAddress.PK;
			}
		}

		OrgAddress CreateNewManufacturerAndGetAddress(IValueObjectImportContext context, ZString registrationCode)
		{
			var result = MIDOrganisationCreator.CreateMIDOrganizationIfNecessary(registrationCode, context.Factory) as OrgAddress;
			if (result == null && !registrationCode.IsLettersAndNumbersOnlyOrEmpty)
			{
				context.AddWarning(ZString.Format("There are invalid characters in MID {0}, only alphanumeric characters are allowed.", registrationCode));
			}

			return result;
		}

		public void SetAddressSequence(Xsd.OrgAddressCollection xmlAddresses, ZString address1, ZString address2)
		{
			int start = 2;
			foreach (Xsd.OrgAddress address in xmlAddresses)
			{
				if (address.AddressLine1 == address1 && address.AddressLine2 == address2)
				{
					address.Sequence = 1;
				}
				else
				{
					address.Sequence = start;
					start++;
				}
			}
		}

		public Xsd.OrgAddress GetAddressWithSequence1(Xsd.OrgAddressCollection addresses)
		{
			Xsd.OrgAddress result = null;
			foreach (Xsd.OrgAddress address in addresses)
			{
				if (address.Sequence == 1)
				{
					return address;
				}
			}
			return result;
		}

		MIDOrganisation MIDOrganisationCreator => fMIDOrganisationCreator ?? (fMIDOrganisationCreator = new MIDOrganisation());
		MIDOrganisation fMIDOrganisationCreator;

		#endregion
	}
}
