using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalOrganizationAddress = Enterprise.UniversalDataBuss.DataObjects.Universal.OrganizationAddress;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	internal static class JobSupplierBookingDataObjectHelper
	{
		internal static UniversalOrganizationAddress FindByDocAddressType(UniversalShipment dataObject, DocAddressType docAddressType)
		{
			return dataObject?.OrganizationAddressCollection?.FirstOrDefault(address => address.AddressType.GetValueOrDefault() == docAddressType.ToString());
		}

		internal static OrgAddress FindByDocAddressTypeAndGetMatched(UniversalShipment dataObject, DocAddressType docAddressType, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			var foundUniversalAddress = FindByDocAddressType(dataObject, docAddressType);
			if (foundUniversalAddress == null)
			{
				return null;
			}

			return new OrganisationDataObjectReader(foundUniversalAddress, logger, factory).GetMatched();
		}

		internal static void TryPopulateOrganisations(IDataWritingManager writeManager, UniversalShipment dataObject, DocAddressType addressType, OrgAddress address)
		{
			if (address != null)
			{
				dataObject.OrganizationAddressCollection?.Add(new OrganizationDataObjectWriter(writeManager, addressType.ToString()).GetDataObject(address));
			}
		}
	}
}
