using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Matching;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public static class UniversalShipmentExtensions
	{
		public static OrganizationAddress FindBestCarrierMatch(this IEnumerable<OrganizationAddress> organizationAddresses, ZString? addressTypeToUseFirst = null)
		{
			OrganizationAddress address = null;
			if (organizationAddresses != null)
			{
				address = organizationAddresses.FirstOrDefault(GetAddressOrder(AddressTypeMatchHelper.GetCarrierAddressTypesInPreferredOrder(), addressTypeToUseFirst));
			}
			return address;
		}

		public static OrganizationAddress FindBestConsigneeMatch(this IEnumerable<OrganizationAddress> organizationAddresses, ZString? addressTypeToUseFirst = null)
		{
			OrganizationAddress address = null;
			if (organizationAddresses != null)
			{
				address = organizationAddresses.FirstOrDefault(GetAddressOrder(AddressTypeMatchHelper.GetConsigneeAddressTypesInPreferredOrder(), addressTypeToUseFirst));
			}
			return address;
		}

		public static OrganizationAddress FindBestConsignorMatch(this IEnumerable<OrganizationAddress> organizationAddresses, ZString? addressTypeToUseFirst = null)
		{
			OrganizationAddress address = null;
			if (organizationAddresses != null)
			{
				address = organizationAddresses.FirstOrDefault(GetAddressOrder(AddressTypeMatchHelper.GetConsignorAddressTypesInPreferredOrder(), addressTypeToUseFirst));
			}
			return address;
		}

		public static OrganizationAddress FindBestSupplierMatch(this IEnumerable<OrganizationAddress> organizationAddresses, ZString? addressTypeToUseFirst = null)
		{
			OrganizationAddress address = null;
			if (organizationAddresses != null)
			{
				address = organizationAddresses.FirstOrDefault(GetAddressOrder(AddressTypeMatchHelper.GetSupplierAddressTypesInPreferredOrder(), addressTypeToUseFirst));
			}
			return address;
		}

		public static OrganizationAddress FindBestImporterMatch(this IEnumerable<OrganizationAddress> organizationAddresses, ZString? addressTypeToUseFirst = null)
		{
			OrganizationAddress address = null;
			if (organizationAddresses != null)
			{
				address = organizationAddresses.FirstOrDefault(GetAddressOrder(AddressTypeMatchHelper.GetImporterAddressTypesInPreferredOrder(), addressTypeToUseFirst));
			}
			return address;
		}

		public static OrganizationAddress FindBestNotifyPartyMatch(this IEnumerable<OrganizationAddress> organizationAddresses, ZString? addressTypeToUseFirst = null)
		{
			OrganizationAddress address = null;
			if (organizationAddresses != null)
			{
				address = organizationAddresses.FirstOrDefault(GetAddressOrder(AddressTypeMatchHelper.GetNotifyPartyAddressTypesInPreferredOrder(), addressTypeToUseFirst));
			}
			return address;
		}

		public static OrganizationAddress[] AddOrgAddresses(this IOrganizationAddressCollectionParent shipmentData, IDataWritingManager manager, IDocAddresses docAddresses)
		{
			var result = new List<OrganizationAddress>();
			if (shipmentData != null && manager != null && docAddresses != null)
			{
				var supportedAddressTypes = docAddresses.SupportedAddressTypes;
				if (supportedAddressTypes.Count > 0)
				{
					var addresses = docAddresses.DocAddresses;
					if (addresses != null)
					{
						foreach (var supportedAddressType in supportedAddressTypes)
						{
							foreach (var docAddress in addresses.FindDocAddressesByType(supportedAddressType).OrderBy(x => x.E2_AddressSequence))
							{
								result.Add(shipmentData.AddOrgAddress(manager, docAddress));
							}
						}
					}
				}
			}
			return result.ToArray();
		}

		public static OrganizationAddress AddOrgAddress(this IOrganizationAddressCollectionParent shipmentData, IDataWritingManager manager, OrgHeader orgHeader, ZString addressType)
		{
			OrganizationAddress address = null;
			if (orgHeader != null)
			{
				address = shipmentData.AddOrgAddress(manager, orgHeader.MainAddress, addressType);
			}
			return address;
		}

		public static OrganizationAddress AddOrgAddress(this IOrganizationAddressCollectionParent shipmentData, IDataWritingManager manager, OrgAddress orgAddress, ZString addressType)
		{
			OrganizationAddress address = null;
			if (orgAddress != null)
			{
				var addressDataObject = OrganizationAddressHelper.GetAddressDataObject(orgAddress, manager, addressType);
				address = shipmentData.AddOrgAddress(addressDataObject);
			}
			return address;
		}

		public static OrganizationAddress AddOrgAddress(this IOrganizationAddressCollectionParent shipmentData, IDataWritingManager manager, OrgAddress orgAddress, ZString addressType, bool populateGeolocation, bool populateValidationStatus)
		{
			OrganizationAddress address = null;
			if (orgAddress != null)
			{
				var addressDataObject = OrganizationAddressHelper.GetAddressDataObject(
					address: orgAddress,
					writeManager: manager,
					addressType: addressType,
					contact: null,
					populateGeolocation: populateGeolocation,
					populateValidationStatus: populateValidationStatus);
				address = shipmentData.AddOrgAddress(addressDataObject);
			}
			return address;
		}

		public static OrganizationAddress AddOrgAddress(this IOrganizationAddressCollectionParent shipmentData, IDataWritingManager manager, OrgHeader orgHeader, DocAddressType addressType)
		{
			OrganizationAddress address = null;
			if (orgHeader != null)
			{
				address = shipmentData.AddOrgAddress(manager, orgHeader.MainAddress, addressType);
			}
			return address;
		}

		public static OrganizationAddress AddOrgAddress(this IOrganizationAddressCollectionParent shipmentData, IDataWritingManager manager, OrgAddress orgAddress, DocAddressType addressType)
		{
			OrganizationAddress address = null;
			if (orgAddress != null)
			{
				var addressDataObject = OrganizationAddressHelper.GetAddressDataObject(orgAddress, manager, addressType.ToString());
				address = shipmentData.AddOrgAddress(addressDataObject);
			}
			return address;
		}

		public static OrganizationAddress AddOrgAddress(this IOrganizationAddressCollectionParent shipmentData, IDataWritingManager manager, OrgContact contact, DocAddressType addressType)
		{
			OrganizationAddress address = null;
			if (contact != null)
			{
				address = shipmentData.AddOrgAddress(manager, contact.OrgAddress, addressType, contact);
			}
			return address;
		}

		public static OrganizationAddress AddOrgAddress(this IOrganizationAddressCollectionParent shipmentData, IDataWritingManager manager, OrgAddress orgAddress, DocAddressType addressType, OrgContact contact)
		{
			OrganizationAddress address = null;
			if (orgAddress != null)
			{
				address = shipmentData.AddOrgAddress(manager, orgAddress, addressType.ToString(), contact);
			}
			return address;
		}

		public static OrganizationAddress AddOrgAddress(this IOrganizationAddressCollectionParent shipmentData, IDataWritingManager manager, OrgAddress orgAddress, ZString addressType, OrgContact contact)
		{
			OrganizationAddress address = null;
			if (orgAddress != null)
			{
				var addressDataObject = OrganizationAddressHelper.GetAddressDataObject(orgAddress, manager, addressType, contact);
				address = shipmentData.AddOrgAddress(addressDataObject);
			}
			return address;
		}

		public static OrganizationAddress AddOrgAddress(this IOrganizationAddressCollectionParent shipmentData, IDataWritingManager manager, JobDocAddress docAddress)
		{
			var writer = new JobDocAddressDataObjectWriter(manager);
			var orgAddress = writer.GetDataObject(docAddress);
			shipmentData.AddOrgAddress(orgAddress);
			return orgAddress;
		}

		/// <summary>
		/// This method would output Doc Address Business Object to an Address Data Object with different address type, not from address type of Doc Address Business Object.
		/// </summary>
		public static OrganizationAddress AddOrgAddress(this IOrganizationAddressCollectionParent shipmentData, IDataWritingManager manager, JobDocAddress docAddress, DocAddressType addressType)
		{
			var orgAddress = shipmentData.AddOrgAddress(manager, docAddress);
			if (orgAddress != null)
			{
				orgAddress.AddressType = addressType.ToString();
			}

			return orgAddress;
		}

		static ZString[] GetAddressOrder(ZString[] addressTypes, ZString? addressTypeToUseFirst = null)
		{
			if (addressTypeToUseFirst.HasValue)
			{
				var value = addressTypeToUseFirst.Value;
				return new ZString[] { value }.Concat(addressTypes.Where(x => x != value)).ToArray();
			}
			else
			{
				return addressTypes;
			}
		}

		public static OrganizationAddress AddOrgAddress(this IOrganizationAddressCollectionParent shipmentData, OrganizationAddress address)
		{
			if (shipmentData != null && address != null)
			{
				shipmentData.SetOrganizationAddressCollection(() => shipmentData.OrganizationAddressCollection ?? new List<OrganizationAddress>());
				shipmentData.OrganizationAddressCollection?.Add(address);
			}
			return address;
		}
	}
}
