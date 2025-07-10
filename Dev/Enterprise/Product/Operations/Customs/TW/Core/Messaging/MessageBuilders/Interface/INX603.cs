using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging
{
	public interface INX603Declaration : INXDeclaration
	{
		ZString ID { get; }

		ZString TypeCode { get; }

		IDeclarationAdditionalDocument AdditionalDocument { get; }

		IDeclarationAgent Agent { get; }

		ITransportMeans BorderTransportMeans { get; }

		ICurrencyExchange CurrencyExchange { get; }

		IGoodsShipment GoodsShipment { get; }

		IPartyDetails Importer { get; }
	}
}
