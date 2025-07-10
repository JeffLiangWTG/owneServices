using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging
{
	public interface INX301Declaration : INXDeclaration
	{
		ZString ID { get; }

		IDeclarationAdditionalDocument AdditionalDocument { get; }

		IDeclarationAgent Agent { get; }

		ITransportMeans BorderTransportMeans { get; }

		ICurrencyExchange CurrencyExchange { get; }

		IGoodsShipment GoodsShipment { get; }

		IPartyDetails Importer { get; }
	}
}
