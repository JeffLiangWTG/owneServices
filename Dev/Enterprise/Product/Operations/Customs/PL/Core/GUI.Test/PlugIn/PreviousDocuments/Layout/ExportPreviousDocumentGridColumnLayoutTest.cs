using System;
using System.Collections.Generic;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.PL.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class ExportPreviousDocumentGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<ExportPreviousDocumentGridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		(PreviousDocument.Schema.CSI_Code, typeof(ZDropEditColumnStyleInfo), 80),
		(PreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 120),
	};

	protected override Type GridBoundEntityType => typeof(PreviousDocument);
}
