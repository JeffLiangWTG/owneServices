using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	public static class FacilityMatchingHelper
	{
		public static IColumnIndexer GetFacility(Shipment xml, UniversalObjectFactory factory, IXmlImportLogger logger)
		{
			var (facilityAddress, facilityType, addressType) = TryGetFacilityDetailsFromUXML(xml);

			var address = new GateManagementOrganisationDataObjectReader(facilityAddress, logger, factory).GetMatched();
			if (address != null)
			{
				var facilityQuery = new ZQuery();
				facilityQuery.AddToFilter(WhsWarehouseSchema.WW_OA_WarehouseAddress, address.PK);
				facilityQuery.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, facilityType);
				facilityQuery.AddToFilter(WhsWarehouseSchema.WW_IsActive, true);
				facilityQuery.MaximumRows = 2;

				var facilities = factory.RowFactory.Load(WhsWarehouseSchema.Constants.TableName, facilityQuery);

				if (facilities.Length == 1)
				{
					return DataObjectReader.GetColumnIndexerFromRow(facilities.SingleOrDefault());
				}
				else if (facilities.Length > 1)
				{
					var multipleFacilitiesErrorMessage = Res.GetString("9f64a150-e03d-493d-a919-a99d87f021b4", "More than one {0} facility was found via the provided address with address type ({1}).", facilityType, addressType);
					throw new DataObjectReadFailureException(multipleFacilitiesErrorMessage);
				}
				else if (facilities.Length < 1)
				{
					var noFacilityMatchedErrorMessage = Res.GetString("daad5c2f-2bfb-4cf8-9f4c-b717e9362b27", "No {0} facility matched the provided address with address type ({1}).", facilityType, addressType);
					throw new DataObjectReadFailureException(noFacilityMatchedErrorMessage);
				}
			}

			throw new DataObjectReadFailureException(Res.GetString("5b278260-a474-4ae5-95f4-72ffe3799b54", "When matching for a facility via provided address, the address provided was not found."));
		}

		static (OrganizationAddress facilityAddress, string facilityType, string addressType) TryGetFacilityDetailsFromUXML(Shipment xml)
		{
			var address = xml.OrganizationAddressCollection.FirstOrDefault(a => a.AddressType.Value == nameof(DocAddressType.LocalCartageYard));

			if (address != null)
			{
				return (address, WarehouseTypes.Codes.ContainerYard, nameof(DocAddressType.LocalCartageYard));
			}

			address ??= xml.OrganizationAddressCollection.FirstOrDefault(a => a.AddressType.Value == nameof(DocAddressType.DepartureCFSAddress));
			if (address != null)
			{
				return (address, WarehouseTypes.Codes.Transit, nameof(DocAddressType.DepartureCFSAddress));
			}

			address ??= xml.OrganizationAddressCollection.FirstOrDefault(a => a.AddressType.Value == nameof(DocAddressType.ArrivalCFSAddress));

			if (address != null)
			{
				return (address, WarehouseTypes.Codes.Transit, nameof(DocAddressType.ArrivalCFSAddress));
			}

			var noValidAddressFound = Res.GetString("9125cf89-2fc7-46fc-b340-bd060d1f5ae6", "No valid address was found when matching for a facility.");
			throw new DataObjectReadFailureException(noValidAddressFound);
		}
	}
}
