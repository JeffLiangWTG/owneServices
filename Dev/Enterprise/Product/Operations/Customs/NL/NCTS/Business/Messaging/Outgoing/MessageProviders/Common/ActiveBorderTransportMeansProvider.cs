using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class ActiveBorderTransportMeansProvider : IActiveBorderTransportMeans
{
	readonly NctsCommonMovementHeader depHeader;

	public ActiveBorderTransportMeansProvider(NctsCommonMovementHeader depHeader)
	{
		this.depHeader = Argument.NotNull(depHeader, nameof(depHeader));
	}

	public int SequenceNumeric => 1;

	public string CustomsOfficeAtBorderReferenceNumber => depHeader.BM_CustomsOfficeAtBorder;

	public int? TypeOfIdentification => int.TryParse(depHeader.BM_ActiveBorderIdentificationType, out int result) ? result : null;

	public string Id => depHeader.BM_TOLCarrierID;

	public string Nationality => depHeader.BM_RN_NKTOLCarrierNationality;

	public string ConveyanceReferenceNumber => depHeader.BM_ConveyanceNumber;
}
