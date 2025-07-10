using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.TW.Messaging
{
	[CodeAlive("MessageBuilder will be created later")]
	public interface INX301_AXDeclaration : INXDeclaration
	{
		ZString ID { get; }

		IDeclarationAdditionalDocument AdditionalDocument { get; }

		IDeclarationAgent Agent { get; }

		ITransportMeans BorderTransportMeans { get; }

		IConsignment Consignment { get; }

		ICurrencyExchange CurrencyExchange { get; }

		IGoodsShipment GoodsShipment { get; }

		IPartyDetails Importer { get; }
	}
}
