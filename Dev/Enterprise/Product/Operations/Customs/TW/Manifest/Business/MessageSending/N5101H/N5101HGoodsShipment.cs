using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class N5101HGoodsShipment : IN5101HGoodsShipment
	{
		public N5101HGoodsShipment(IN5101HConsignment consignment, ZString entryOfficeId)
		{
			Consignment = consignment;
			EntryOfficeId = entryOfficeId;
		}

		public IN5101HConsignment Consignment { get; private set; }

		public ZString EntryOfficeId { get; private set; }
	}
}
