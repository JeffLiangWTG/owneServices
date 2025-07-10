using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC044CDepartureTransportMeansProvider : INCTSDepartureTransportMeans
{
	protected readonly CusTransportMeans transportMeans;
	public CC044CDepartureTransportMeansProvider(CusTransportMeans transportMeans)
	{
		this.transportMeans = Argument.NotNull(transportMeans, nameof(transportMeans));
	}

	public int SequenceNumeric => transportMeans.TPM_SequenceNumber;

	public int? TypeOfIdentification => StatusIsNew ? int.TryParse(transportMeans.TPM_TypeOfIdentification, out int value) ? value : null : null;

	public string Id => StatusIsNew ? transportMeans.TPM_IdentificationNumber : ZString.Empty;

	public string Nationality => StatusIsNew ? transportMeans.TPM_RN_NKTransportNationality : ZString.Empty;

	bool StatusIsNew => transportMeans.TPM_TransportState == EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
}
