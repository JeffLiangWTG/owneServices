using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	public class OrgAddressWrapper : IOrganisation
	{
		OrgAddressWrapper(OrgAddress orgAddress)
		{
			this.orgAddress = Argument.NotNull(orgAddress, "orgAddress cannot be null");
		}
		readonly OrgAddress orgAddress;

		public static OrgAddressWrapper New(OrgAddress orgAddress)
		{
			return orgAddress == null ? null : new OrgAddressWrapper(orgAddress);
		}

		ZString IOrganisation.City => orgAddress.OA_City;

		ZString IOrganisation.CountryCode => !orgAddress.OA_RN_NKCountryCode.IsEmpty ? orgAddress.OA_RN_NKCountryCode : orgAddress.OA_RL_NKRelatedPortCode.Left(2);

		ZString IOrganisation.CountryRegion => orgAddress.OA_State;

		ZString IOrganisation.Address => GetStreetAddressDetails(orgAddress.OA_Address1, orgAddress.OA_Address2);

		ZString IOrganisation.PostCode => PostCode;

		ZString IOrganisation.ContactPerson => ZString.Empty;

		IEnumerable<ICommunication> IOrganisation.Communications => Enumerable.Empty<ICommunication>();

		ZString IOrganisationSimple.Name => orgAddress.Header?.OH_FullNameTruncated ?? ZString.Empty;

		ZString IOrganisationSimple.CustomsClientCode => ZString.Empty;

		ZString IOrganisationSimple.CustomsSupplierCode => ZString.Empty;

		IEnumerable<IContact> IOrganisationSimple.Contacts => Enumerable.Empty<IContact>();

		public static ZString GetStreetAddressDetails(ZString address1, ZString address2)
		{
			address1 = address1.Trim();
			address2 = address2.Trim();
			var result = address2.IsEmpty ? address1 : (ZString)string.Join(" ", address1, address2);
			return result.Left(OrgHeaderWrapper.Schema.StreetAddressMaxLength);
		}

		ZString PostCode
		{
			get
			{
				var result = orgAddress.OA_PostCode;
				if (result.Length > OrgHeaderWrapper.Schema.PostCodeMaxLength)
				{
					result = result.KeepAlphanumericCharacters();
				}

				return result.Left(OrgHeaderWrapper.Schema.PostCodeMaxLength);
			}
		}
	}
}
