using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.GUI.Testing;

sealed class SupportingDocumentsGridColumnsLayoutTest
	: EU.NCTS.GUI.Testing.SupportingDocumentsGridColumnLayoutTest<SupportingDocumentsGridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns
	{
		get => base.ExpectedColumns.Select(x => x.Item1 switch
		{
			NctsAdditionalInfo.Schema.CSI_ItemNumber
				=> (NctsAdditionalInfo.Schema.CSI_ItemNumber, typeof(ZNullableCalcEditColumnStyleInfo), 80),

			_ => x,
		}).ToArray();
	}

	protected override Type GridBoundEntityType => typeof(NctsSupportingDocument);
}
