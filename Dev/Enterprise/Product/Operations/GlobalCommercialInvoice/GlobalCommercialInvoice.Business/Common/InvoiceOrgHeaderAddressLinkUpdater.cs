using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.GlobalCommercialInvoice.Business
{
	class InvoiceOrgHeaderAddressLinkUpdater
	{
		readonly Dictionary<InvoiceOrgHeaderIndex, InvoiceOrgHeaderAddressLink> orgHeaderAddressLinkDictionary;

		public InvoiceOrgHeaderAddressLinkUpdater(Dictionary<InvoiceOrgHeaderIndex, InvoiceOrgHeaderAddressLink> orgHeaderAddressLinkDictionary)
		{
			this.orgHeaderAddressLinkDictionary = orgHeaderAddressLinkDictionary;
		}

		/// <summary>
		/// This will update GIH_OA_*Address_ZAddress to reflect the panel address when the organization is changed in the grid.
		/// </summary>
		internal void SetOrgHeader(InvoiceOrgHeaderIndex orgIndex, ZGuid orgHeaderPK)
		{
			var link = orgHeaderAddressLinkDictionary[orgIndex];
			var oldOrgHeaderPK = link.HeaderPKGetter();
			link.HeaderPKSetter(orgHeaderPK);

			// Set the default address only if the organization has changed.
			if (orgHeaderPK != oldOrgHeaderPK)
			{
				link.Address.SetOrgWithoutSettingDefaultAddress(orgHeaderPK);
				// This is necessary to force the panel address to update its organization
				// when a value in the grid changes even if it is empty.
				TryGetOrganizationDefaultAddressPK(link.Address, out var defaultAddressPK);
				link.AddressPKSetter(defaultAddressPK);
			}
		}

		/// <summary>
		/// This will update the organization's values and address in the grid when the panel values are changed.
		/// </summary>
		internal void SetOrgAddress(InvoiceOrgHeaderIndex orgIndex, ZGuid orgAddressPK)
		{
			var link = orgHeaderAddressLinkDictionary[orgIndex];
			link.AddressPKSetter(orgAddressPK);

			// Set the default address only if the organization has changed.
			if (link.Address.OrgPK != link.HeaderPKGetter())
			{
				link.HeaderPKSetter(link.Address.OrgPK);

				if (TryGetOrganizationDefaultAddressPK(link.Address, out var defaultAddressPK))
				{
					link.AddressPKSetter(defaultAddressPK);
				}
			}
		}

		/// <summary>
		/// Tries to get the default address PK for the organization.
		/// If there is only one address, it will be returned.
		/// If there are multiple addresses, the first one with the default capability will be returned.
		/// Otherwise, an empty ZGuid will be returned.
		/// </summary>
		bool TryGetOrganizationDefaultAddressPK(ZAddress orgAddress, out ZGuid defaultAddressPK)
		{
			defaultAddressPK = ZGuid.Empty;

			var orgAddressList = orgAddress.OrgAddress_List.List;
			if (orgAddressList.Count == 1)
			{
				defaultAddressPK = (ZGuid)orgAddressList[0].PK;
			}

			if (orgAddressList.Count > 1)
			{
				var defaultItem = orgAddressList.Cast<ZAddressItem>().FirstOrDefault((x) => x.Capabilities.Any((y) => y.IsDefault));
				defaultAddressPK = defaultItem?.PK ?? ZGuid.Empty;
			}

			return defaultAddressPK.IsValid;
		}
	}
}
