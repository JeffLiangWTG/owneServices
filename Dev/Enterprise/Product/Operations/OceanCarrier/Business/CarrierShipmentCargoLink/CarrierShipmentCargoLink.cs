using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierShipmentCargoLink : AutoCarrierShipmentCargoLink
	{
		public CarrierShipmentCargoLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
