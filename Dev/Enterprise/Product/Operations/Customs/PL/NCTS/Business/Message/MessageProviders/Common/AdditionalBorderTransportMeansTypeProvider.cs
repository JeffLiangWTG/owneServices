using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class AdditionalBorderTransportMeansTypeProvider : BorderTransportMeansTypeProviderBase
{
	public AdditionalBorderTransportMeansTypeProvider(int sequenceNumber, DepartureCusTransportMeans transportMeans, NctsDepartureMovementHeader movementHeader)
		: base(sequenceNumber, movementHeader)
	{
		this.transportMeans = Argument.NotNull(transportMeans, nameof(transportMeans));
	}
	readonly DepartureCusTransportMeans transportMeans;

	protected override ZString CustomsOfficeAtBorderReferenceNumberCore => transportMeans.TPM_CustomsOffice;

	protected override ZString TypeOfIdentificationCore => transportMeans.TPM_TypeOfIdentification;

	protected override ZString IdentificationNumberCore => transportMeans.TPM_IdentificationNumber;

	protected override ZString NationalityCore => transportMeans.TPM_RN_NKTransportNationality;

	protected override ZString ConveyanceReferenceNumberCore => transportMeans.TPM_ReferenceNumber;
}
