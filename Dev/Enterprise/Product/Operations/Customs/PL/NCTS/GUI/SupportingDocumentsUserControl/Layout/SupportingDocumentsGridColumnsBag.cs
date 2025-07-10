using Enterprise.Customs.PL.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.NCTS.GUI;

sealed class SupportingDocumentsGridColumnsBag
{
	public static SupportingDocumentsGridColumnsBag Instance => new();

	public IGridColumnReference ItemNumberNullableCalcEditColumn { get; }
		= new GridColumnReference<ZNullableCalcEditColumnStyleInfo>(NctsAdditionalInfo.Schema.CSI_ItemNumber, 80);
}
