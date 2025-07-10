using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.NCTS.GUI;

sealed class SupportingDocumentsGridColumnLayout : EU.NCTS.GUI.SupportingDocumentsGridColumnLayout
{
	protected override IEnumerable<IGridColumnReference> GetColumns(EU.NCTS.GUI.SupportingDocumentsGridColumnsBag euGridColumnBag)
		=> base.GetColumns(euGridColumnBag).Select(column => column switch
		{
			_ when ReferenceEquals(column, euGridColumnBag.ItemNumberCalcEditColumn)
				=> SupportingDocumentsGridColumnsBag.Instance.ItemNumberNullableCalcEditColumn,

			_ => column,
		});
}
