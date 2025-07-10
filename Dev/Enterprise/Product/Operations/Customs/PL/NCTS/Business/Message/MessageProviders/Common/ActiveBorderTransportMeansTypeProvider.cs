using CargoWise.Types;

namespace Enterprise.Customs.PL.NCTS.Business;

public class ActiveBorderTransportMeansTypeProvider : BorderTransportMeansTypeProviderBase
{
	public ActiveBorderTransportMeansTypeProvider(int sequenceNumber, NctsDepartureMovementHeader movementHeader)
		: base(sequenceNumber, movementHeader)
	{
	}

	protected override ZString CustomsOfficeAtBorderReferenceNumberCore => movementHeader.BM_CustomsOfficeAtBorder;

	protected override ZString TypeOfIdentificationCore => movementHeader.BM_ActiveBorderIdentificationType;

	protected override ZString IdentificationNumberCore => movementHeader.BM_TOLCarrierID;

	protected override ZString NationalityCore => movementHeader.BM_RN_NKTOLCarrierNationality;

	protected override ZString ConveyanceReferenceNumberCore => movementHeader.BM_ConveyanceNumber;
}
