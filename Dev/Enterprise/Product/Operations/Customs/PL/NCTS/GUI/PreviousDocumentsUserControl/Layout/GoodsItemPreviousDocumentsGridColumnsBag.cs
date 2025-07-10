using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.NCTS.GUI;

sealed class GoodsItemPreviousDocumentsGridColumnsBag
{
	public static GoodsItemPreviousDocumentsGridColumnsBag Instance => new();

	public IGridColumnReference ItemNumberNullableCalcEditColumn { get; }
		= new GridColumnReference<ZNullableCalcEditColumnStyleInfo>(NctsPreviousDocument.Schema.CSI_ItemNumber, 80);
}
