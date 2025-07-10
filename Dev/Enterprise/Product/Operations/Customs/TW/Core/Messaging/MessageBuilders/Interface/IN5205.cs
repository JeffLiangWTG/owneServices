using System.Collections.Generic;

namespace Enterprise.Customs.TW.Messaging
{
	public interface IN5205Declaration : IBriefCusDeclaration
	{
		IPartyDetails Exporter { get; }

		IEnumerable<IBCDGoodsShipment> GoodsShipments { get; }
	}
}
