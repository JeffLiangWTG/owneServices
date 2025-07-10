using CargoWise.Types;
using Enterprise.Customs.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public abstract class CusSCAHouseForwardingShipmentCustomsStatusProvider : ForwardingShipmentCustomsStatusProvider, Integration.Customs.AU.ICusSCAHouseForwardingShipmentCustomsStatusProvider
	{
		protected CusSCAHouseForwardingShipmentCustomsStatusProvider(ForwardingShipment shipment)
			: base(shipment)
		{
			shipment.Factory.AddFetchHint(typeof(BaseCusSCAHouse), CusSCAHouseSchema.CA_JS, shipment.PK);
		}

		protected abstract ZString[] ApplicationCodes { get; }

		protected BaseCusSCAHouse House
		{
			get { return new BaseCusSCAHouse.Loader(Shipment.Factory).LoadFromShipmentAndApplicationCode(Shipment.PK, ApplicationCodes); }
		}

		public override ZString CustomsCargoStatus()
		{
			BaseCusSCAHouse house = this.House;
			return house != null ? house.CA_ShipmentStatus : ZString.Empty;
		}

		public override ZString CustomsMessageStatus()
		{
			BaseCusSCAHouse house = this.House;
			return house != null ? house.CA_MessageStatus : ZString.Empty;
		}
	}
}

