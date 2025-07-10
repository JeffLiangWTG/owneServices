using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging
{
	public interface IBriefCusDeclaration
	{
		ZDate AcceptanceDateTime { get; }

		ZString FunctionCode { get; }

		ZString ID { get; }

		ZString TypeCode { get; }

		IPartyDetails Agent { get; }

		ITransportMeans BorderTransportMeans { get; }

		IBCDConsignment Consignment { get; }

		ZString RepresentativePersonName { get; }

		IPartyDetails ExpressCarrier { get; }

		IPartyDetails OnBoardCourier { get; }
	}
}
