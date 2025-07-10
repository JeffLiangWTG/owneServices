using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;

namespace Enterprise.Customs.PL.NCTS.Business;

public abstract class BorderTransportMeansTypeProviderBase : IActiveBorderTransportMeansType
{
	protected BorderTransportMeansTypeProviderBase(int sequenceNumber, NctsDepartureMovementHeader movementHeader)
	{
		this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
		SequenceNumber = sequenceNumber.ToString();
	}

	protected readonly NctsDepartureMovementHeader movementHeader;

	public string ConveyanceReferenceNumber => ConveyanceReferenceNumberCore;

	public string CustomsOfficeAtBorderReferenceNumber => CustomsOfficeAtBorderReferenceNumberCore;

	public string IdentificationNumber => IdentificationNumberCore;

	public string Nationality => NationalityCore;

	public string SequenceNumber { get; }

	public string TypeOfIdentification => TypeOfIdentificationCore;

	protected abstract ZString ConveyanceReferenceNumberCore { get; }

	protected abstract ZString CustomsOfficeAtBorderReferenceNumberCore { get; }

	protected abstract ZString IdentificationNumberCore { get; }

	protected abstract ZString NationalityCore { get; }

	protected abstract ZString TypeOfIdentificationCore { get; }
}
