using CargoWise.Types;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	public interface ITranshipmentDetails
	{
		ZBool InternationalTranshipmentRequest { get; }
		ZBool DomesticTranshipmentRequest { get; }
		ZString ModeOfTransportForTransfer { get; }
		ZString ITRImportCraft { get; }
		ZString ITRImportMode { get; }
		ZDateTime ITRArrivalDate { get; }
		ZDateTime ITRDepartureDate { get; }
		ZString ITRVoyageFlight { get; }
		ZString PremiseCode { get; }
	}
}
