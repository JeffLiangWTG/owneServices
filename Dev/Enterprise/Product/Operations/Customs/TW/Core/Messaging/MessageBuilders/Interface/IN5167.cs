using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging
{
	public interface IN5167Declaration
	{
		ZString DeclarationOfficeID { get; }

		ZString FunctionalReferenceID { get; }

		IPartyDetails Agent { get; }

		IConsignment Consignment { get; }

		IGoodsShipment GoodsShipment { get; }

		IPartyDetails Importer { get; }
	}
}
