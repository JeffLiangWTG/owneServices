using System;
using System.Collections.Generic;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.PL.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(ExportInvoiceLinePreviousDocumentGridColumnLayout))]
sealed class ExportInvoiceLinePreviousDocumentGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<ExportInvoiceLinePreviousDocumentGridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		(PreviousDocument.Schema.CSI_Code, typeof(ZDropEditColumnStyleInfo), 80),
		(PreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 120),
		(PreviousDocument.Schema.CSI_LineNo, typeof(ZCalcEditColumnStyleInfo), 80),
		(PreviousDocument.Schema.CSI_UnitOfQuantity, typeof(ZDropEditColumnStyleInfo), 80),
		(PreviousDocument.Schema.CSI_Quantity, typeof(ZCalcEditColumnStyleInfo), 80),
		(PreviousDocument.Schema.CSI_PackType, typeof(ZDropEditColumnStyleInfo), 80),
		(PreviousDocument.Schema.CSI_PackQty, typeof(ZCalcEditColumnStyleInfo), 80),
		(PreviousDocument.Schema.CSI_ReferenceNumber2, typeof(ZTextBoxColumnStyleInfo), 80),
	};

	protected override Type GridBoundEntityType => typeof(PreviousDocument);
}
