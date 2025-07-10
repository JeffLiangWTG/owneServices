using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class PlaceOfUnloadingProvider : IPlace
{
	readonly NctsCommonMovementHeader moveHeader;

	public PlaceOfUnloadingProvider(NctsCommonMovementHeader moveHeader)
	{
		this.moveHeader = Argument.NotNull(moveHeader, nameof(moveHeader));
	}

	public string UnLocode => CodeIs2OrLessCharacters ? null : moveHeader.BM_ForeignDestPortKCode;

	public string Country => CodeIs2OrLessCharacters ? moveHeader.BM_ForeignDestPortKCode : null;

	public string Location => CodeIs2OrLessCharacters ? moveHeader.BM_PlaceOfUnloading : null;

	bool CodeIs2OrLessCharacters => moveHeader.BM_ForeignDestPortKCode.Length <= 2;
}
