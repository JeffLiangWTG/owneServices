using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(PreviousProcedureGridLayout))]
sealed class PreviousProcedureGridLayoutTest : GridColumnLayoutProviderAbstractTest<PreviousProcedureGridLayout>
{
	public void TestCSI_Procedure_ReadOnly()
	{
		IGridColumnLayoutProvider layoutProvider = CreateGridColumnLayoutProvider();
		var layout = layoutProvider.Layout;
		var columnInfo = layout.Columns.SingleOrDefault(c => c.ColumnName == PreviousDocument.Schema.CSI_Procedure);
		AssertNotNull("CSI_Procedure Column", columnInfo);
		AssertEquals("ReadOnly", true, columnInfo.IsReadOnly);
	}

	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		(PreviousDocument.Schema.CSI_Code, typeof(ZDropEditColumnStyleInfo), 50),
		(PreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 150),
		(PreviousDocument.Schema.CSI_ReferenceNumber2, typeof(ZTextBoxColumnStyleInfo), 150),
		(PreviousDocument.Schema.CSI_LineNo, typeof(ZCalcEditColumnStyleInfo), 70),
		(PreviousDocument.Schema.CSI_Quantity, typeof(ZCalcEditColumnStyleInfo), 70),
		(PreviousDocument.Schema.CSI_Procedure, typeof(ZTextBoxColumnStyleInfo), 100),
	};

	protected override Type GridBoundEntityType => typeof(PreviousDocument);
}
