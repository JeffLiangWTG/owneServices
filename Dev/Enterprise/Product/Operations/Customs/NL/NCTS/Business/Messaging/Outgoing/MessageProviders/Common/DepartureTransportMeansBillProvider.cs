using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class DepartureTransportMeansBillProvider : INCTSDepartureTransportMeans
{
	readonly DepartureCusTransportMeans departureTransportMeans;
	readonly string transportMode;

	public DepartureTransportMeansBillProvider(DepartureCusTransportMeans departureTransportMeans, string transportMode, int sequence)
	{
		this.departureTransportMeans = Argument.NotNull(departureTransportMeans, nameof(departureTransportMeans));
		this.transportMode = transportMode;
		SequenceNumeric = sequence;
	}

	public int SequenceNumeric { get; }

	public int? TypeOfIdentification => transportMode switch
	{
		ModeOfTransportList.Codes._1_SeaTransport => 10,
		ModeOfTransportList.Codes._2_RailTransport => departureTransportMeans.TPM_SequenceNumber == 1 ? 20 : 21,
		ModeOfTransportList.Codes._3_RoadTransport => departureTransportMeans.TPM_SequenceNumber == 1 ? 30 : 31,
		ModeOfTransportList.Codes._4_AirTransport => 40,
		ModeOfTransportList.Codes._8_InlandWaterwayTransport => 81,
		ModeOfTransportList.Codes._9_OwnPropulsion => int.TryParse(departureTransportMeans.TPM_TypeOfIdentification, out var type) ? type : null,
		_ => null,
	};

	public string Id => departureTransportMeans.TPM_IdentificationNumber;

	public string Nationality => departureTransportMeans.TPM_RN_NKTransportNationality;
}
