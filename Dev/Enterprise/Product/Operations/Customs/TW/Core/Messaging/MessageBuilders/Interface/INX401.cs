using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging
{
	public interface INX401Declaration : INXDeclaration
	{
		ZDate AcceptanceDateTime { get; }

		ZString ID { get; }

		ZString TypeCode { get; }

		IDeclarationAdditionalDocument AdditionalDocument { get; }

		IDeclarationAdditionalInformation AdditionalInformation { get; }

		IDeclarationAgent Agent { get; }

		ITransportMeans BorderTransportMeans { get; }

		ICurrencyExchange CurrencyExchange { get; }

		IGoodsShipment GoodsShipment { get; }

		IPartyDetails Importer { get; }

		IPackaging Packaging { get; }
	}
}
