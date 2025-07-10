using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Freight.Business
{
	public class PackLocationForDocumentCollection
	{
		public PackLocationForDocumentCollection(CommonShipment shipment, BusinessObjectFactory factory)
		{
			this.Shipment = shipment;
			this.Factory = factory;
			packLocations = new List<PackLocationForDocument>();
			CreatePackLocationList();
		}

		public List<PackLocationForDocument> PackLocations
		{
			get { return packLocations; }
		}

		void CreatePackLocationList()
		{
			if (Shipment.OuterPackLines.Count > 0)
			{
				foreach (PackLine line in Shipment.OuterPackLines)
				{
					ZString packType = line.JL_F3_NKPackType;
					if (line.PackLocations.Count > 0)
					{
						foreach (PackLocation location in line.PackLocations)
						{
							packLocations.Add(new PackLocationForDocument(ShipmentNumber, Destination, ConsignorName, location.JQ_NoPackages, packType, PackWhsLocation(location)));
						}
					}
					ZInt locationDifference = TotalPacksOnPackLines - GetTotalPacksOnPackLocations(line.PackLocations);
					if (locationDifference > 0)
					{
						packLocations.Add(new PackLocationForDocument(ShipmentNumber, Destination, ConsignorName, locationDifference, packType, ShipmentWhsLocation));
					}
				}
			}

			ZInt packDifference = Shipment.JS_OuterPacks - TotalPacksOnPackLines;
			if (packDifference > 0)
			{
				packLocations.Add(new PackLocationForDocument(ShipmentNumber, Destination, ConsignorName, packDifference, Shipment.JS_F3_NKPackType, ShipmentWhsLocation));
			}
		}

		ZString PackWhsLocation(PackLocation location)
		{
			ZString result = location.JQ_WarehouseLocation;

			if (Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.Value)
			{
				result = GetWarehouseName(location.LocationWhsGuid) + " : " + location.LocationString;
			}

			return result;
		}

		ZString ShipmentWhsLocation
		{
			get
			{
				ZString result = Shipment.JS_WarehouseLocation;
				if (Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.Value)
				{
					result = GetWarehouseName(Shipment.LocationWhsGuid) + " : " + Shipment.LocationString;
				}

				return result;
			}
		}

		ZInt TotalPacksOnPackLines
		{
			get
			{
				ZInt total = 0;
				foreach (PackLine line in Shipment.OuterPackLines)
				{
					total += line.JL_PackageCount;
				}
				return total;
			}
		}

		ZInt GetTotalPacksOnPackLocations(PackLocationCollection locations)
		{
			ZInt total = 0;
			foreach (PackLocation location in locations)
			{
				total += location.JQ_NoPackages;
			}
			return total;
		}

		ZString GetWarehouseName(ZGuid whsGuid)
		{
			var warehouse = Factory.Load<IWhsWarehouse>(whsGuid);
			return warehouse?.WW_WarehouseCode ?? ZString.Empty;
		}

		ZString ShipmentNumber
		{
			get { return Shipment.JS_UniqueConsignRef; }
		}

		ZString Destination
		{
			get { return Shipment.Destination != null ? Shipment.Destination.Code : ZString.Empty; }
		}

		ZString ConsignorName
		{
			get { return Shipment.Consignor != null ? Shipment.Consignor.OH_FullNameTruncated : ZString.Empty; }
		}

		readonly BusinessObjectFactory Factory;
		readonly CommonShipment Shipment;
		readonly List<PackLocationForDocument> packLocations;
	}
}
