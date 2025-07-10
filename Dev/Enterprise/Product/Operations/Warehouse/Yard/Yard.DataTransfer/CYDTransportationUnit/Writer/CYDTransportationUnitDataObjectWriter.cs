using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Yard.Business;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal;

public class CYDTransportationUnitDataObjectWriter(IDataWritingManager writeManager) : TopLevelDataObjectWriter<CYDTransportationUnit, Shipment>(writeManager)
{
	protected override ZString GetEDIMessageSubType() => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

	protected override DataContextType GetTopLevelDataContextType() => DataContextType.CYDTransportationUnit;

	protected override void PopulateDataObject(CYDTransportationUnit transportationUnit, Shipment shipment)
	{
		PopulateVehicleRegistrationNumber(transportationUnit, shipment);
		PopulateOrganizationAddressCollection(transportationUnit, shipment);
		PopulateDeliveryCollection(transportationUnit, shipment);
		PopulatePickupCollection(transportationUnit, shipment);
	}

	void PopulateOrganizationAddressCollection(CYDTransportationUnit transportationUnit, Shipment shipment)
	{
		var yardOrganizationAddress = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.LocalCartageYard)).GetDataObject(transportationUnit.Yard.WarehouseAddress);
		shipment.SetOrganizationAddressCollection(() => ProcessCollection(transportationUnit.DocAddresses, new JobDocAddressDataObjectWriter(writeManager)).AddSafe(yardOrganizationAddress));
	}

	void PopulateDeliveryCollection(CYDTransportationUnit transportationUnit, Shipment shipment)
	{
		if (transportationUnit.Deliveries.Count > 0)
		{
			shipment.SetSubShipmentCollection(() => []);

			foreach (var delivery in transportationUnit.Deliveries)
			{
				shipment.SubShipmentCollection.AddRange(ProcessCollection(delivery, new CYDDeliveryDataObjectWriter(writeManager)));
			}
		}
	}

	void PopulatePickupCollection(CYDTransportationUnit transportationUnit, Shipment shipment)
	{
		if (transportationUnit.Pickups.Count > 0)
		{
			shipment.SetSubShipmentCollection(() => []);

			foreach (var pickup in transportationUnit.Pickups)
			{
				shipment.SubShipmentCollection.AddRange(ProcessCollection(pickup, new CYDPickupDataObjectWriter(writeManager)));
			}
		}
	}

	static void PopulateVehicleRegistrationNumber(CYDTransportationUnit transportationUnit, Shipment shipment)
	{
		var vehicleNumber = transportationUnit.YTU_TransportationReference;
		if (string.IsNullOrWhiteSpace(vehicleNumber))
		{
			throw new DataObjectValidationException(Res.GetString("17fb585c-6304-4375-b10d-2ffca75a8f9a", "Vehicle Registration Number is null or empty."));
		}

		shipment.SetPreCarriageShipmentCollection(() =>
		[
			new Shipment
			{
				VehicleRun = new VehicleRun
				{
					Vehicle = new Vehicle
					{
						Registration = new Registration
						{
							Number = vehicleNumber
						}
					}
				}
			}
		]);
	}
}
