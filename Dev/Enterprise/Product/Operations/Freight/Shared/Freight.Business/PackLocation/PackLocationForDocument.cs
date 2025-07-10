using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class PackLocationForDocument : NonPersistentBusinessObject, IObsoleteValidation
	{
		public PackLocationForDocument(ZString shipNumber, ZString destination, ZString consignorName, ZInt packCount, ZString packType, ZString whsLocation)
			: base()
		{
			shipmentNumber = shipNumber;
			this.destination = destination;
			this.consignorName = consignorName;
			this.packCount = packCount;
			this.packType = packType;
			this.whsLocation = whsLocation;
		}

		public ZString ShipmentNumber
		{
			get { return shipmentNumber; }
		}

		public ZString ConsignorName
		{
			get { return consignorName; }
		}

		public ZString Destination
		{
			get { return destination; }
		}

		public ZInt PackCount
		{
			get { return packCount; }
		}

		public ZString PackType
		{
			get { return packType; }
		}

		public ZString WhsLocation
		{
			get { return whsLocation; }
		}

		readonly ZString shipmentNumber;
		readonly ZString destination;
		readonly ZString consignorName;
		readonly ZString packType;
		readonly ZInt packCount;
		readonly ZString whsLocation;
	}
}
