using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	public class OrgHeaderWrapper : IOrganisationSimple, IOrganisation, IPartyInformation
	{
		public OrgHeaderWrapper(OrgHeader organisation)
		{
			Organisation = organisation;
		}

		public static class Schema
		{
			public const int StreetAddressMaxLength = 70;
			public const int PostCodeMaxLength = 9;
		}

		public OrgHeaderWrapper(OrgHeader organisation, JobDocAddress orgAddressChosen)
		{
			Organisation = organisation;
			Address = orgAddressChosen;
		}
		public readonly OrgHeader Organisation;
		public readonly JobDocAddress Address;

		public static OrgHeaderWrapper New(OrgHeader organisation)
		{
			return organisation == null ? null : new OrgHeaderWrapper(organisation, null);
		}

		public static OrgHeaderWrapper New(OrgHeader organisation, JobDocAddress orgAddressChosen)
		{
			return organisation == null ? null : new OrgHeaderWrapper(organisation, orgAddressChosen);
		}

		public static ZString GetStreetAddressDetails(ZString address1, ZString address2)
		{
			address1 = address1.Trim();
			address2 = address2.Trim();
			var result = address2.IsEmpty ? address1 : (ZString)string.Join(" ", address1, address2);
			return result.Left(Schema.StreetAddressMaxLength);
		}

		#region IOrganisationSimple

		ZString IOrganisationSimple.Name => Name;

		//Should this be a new Org Config code? - Advice received from Carl Hagedorn (NZ Customs): Existing customs client codes will be migrated to TSW.
		ZString IOrganisationSimple.CustomsClientCode => Organisation?.GetCustomsClientCode() ?? ZString.Empty;

		ZString IOrganisationSimple.CustomsSupplierCode => Organisation?.GetSupplierCode() ?? ZString.Empty;

		IEnumerable<IContact> IOrganisationSimple.Contacts
		{
			get
			{
				foreach (OrgContact contact in Organisation.Contacts)
				{
					foreach (OrgContactAttribute contactAllocation in contact.Allocations)
					{
						if (contactAllocation.PC_Type == OrgConstants.ContactAllocationType.NZCustoms)
						{
							yield return OrgContactWrapper.New(contact);
						}
					}
				}
			}
		}

		#endregion

		#region IOrganisation

		ZString IOrganisation.City => City;

		ZString IOrganisation.CountryCode => CountryCode;

		ZString IOrganisation.CountryRegion => CountryRegion;

		ZString IOrganisation.Address => StreetAddress;

		ZString IOrganisation.PostCode => PostCode;

		ZString IOrganisation.ContactPerson => Organisation?.GetAllocatedNZCustomsContact()?.ContactName ?? ZString.Empty;

		IEnumerable<ICommunication> IOrganisation.Communications => Organisation?.GetCommunications() ?? Enumerable.Empty<ICommunication>();

		#endregion

		#region IPartyInformation

		ZString IPartyInformation.Name => Name;

		ZString IPartyInformation.City => City;

		ZString IPartyInformation.CountryCode => CountryCode;

		ZString IPartyInformation.CountryRegion => CountryRegion;

		ZString IPartyInformation.Address => StreetAddress;

		ZString IPartyInformation.PostCode => PostCode;

		ZString IPartyInformation.CustomsClientCode => Organisation?.GetCustomsClientCode() ?? ZString.Empty;

		IEnumerable<ICommunication> IPartyInformation.Communications => Organisation?.GetCommunications() ?? Enumerable.Empty<ICommunication>();

		#endregion

		#region Implementation

		ZString Name => Address?.CompanyName ?? Organisation?.OH_FullNameTruncated ?? ZString.Empty;

		ZString City => Address?.E2_City ?? Organisation?.MainAddress.OA_City ?? ZString.Empty;

		ZString CountryCode => Address?.E2_RN_NKCountryCode ?? Organisation?.MainAddress.OA_RL_NKRelatedPortCode.Left(2) ?? ZString.Empty;

		ZString CountryRegion => Address?.E2_State ?? Organisation?.MainAddress.OA_State ?? ZString.Empty;

		ZString StreetAddress
		{
			get
			{
				var result = ZString.Empty;
				if (Address != null)
				{
					result = GetStreetAddressDetails(Address.E2_Address1, Address.E2_Address2);
				}
				else if (Organisation?.MainAddress is OrgAddress mainAddress)
				{
					result = GetStreetAddressDetails(mainAddress.OA_Address1, mainAddress.OA_Address2);
				}

				return result;
			}
		}

		ZString PostCode
		{
			get
			{
				var result = Address?.E2_Postcode ?? Organisation?.MainAddress.OA_PostCode ?? ZString.Empty;
				if (result.Length > Schema.PostCodeMaxLength)
				{
					result = result.KeepAlphanumericCharacters();
				}

				return result.Left(Schema.PostCodeMaxLength);
			}
		}

		public bool IsEmpty => Organisation == null;

		#endregion

	}
}
