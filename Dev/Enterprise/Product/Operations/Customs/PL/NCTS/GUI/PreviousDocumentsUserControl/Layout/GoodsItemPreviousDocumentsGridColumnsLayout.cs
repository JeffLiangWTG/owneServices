using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.NCTS.GUI;

sealed class GoodsItemPreviousDocumentsGridColumnsLayout : EU.NCTS.GUI.Phase5GoodsItemPreviousDocumentsGridColumnsLayout
{
	protected override IEnumerable<IGridColumnReference> GetColumns(EU.NCTS.GUI.Phase5GoodsItemPreviousDocumentsGridColumnsBag euGridColumnBag)
		=> base.GetColumns(euGridColumnBag).Select(column => column switch
		{
			_ when ReferenceEquals(column, euGridColumnBag.ItemNumberCalcEditColumn)
				=> GoodsItemPreviousDocumentsGridColumnsBag.Instance.ItemNumberNullableCalcEditColumn,

			_ => column,
		});
}
