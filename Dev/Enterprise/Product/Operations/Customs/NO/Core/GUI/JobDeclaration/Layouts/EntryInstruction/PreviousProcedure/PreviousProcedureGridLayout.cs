using System;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI;

sealed class PreviousProcedureGridLayout : IGridColumnLayoutProvider
{
	IGridColumnLayout IGridColumnLayoutProvider.Layout => previousProcedureGridColumnLayout.Value;
	readonly Lazy<IGridColumnLayout> previousProcedureGridColumnLayout = new (CreatePreviousProcedureGridColumnLayout);

	static IGridColumnLayout CreatePreviousProcedureGridColumnLayout()
	{
		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn<ZDropEditColumnStyleInfo>(PreviousDocument.Schema.CSI_Code, 50);
		builder.AddColumn<ZTextBoxColumnStyleInfo>(PreviousDocument.Schema.CSI_ReferenceNumber, 150);
		builder.AddColumn<ZTextBoxColumnStyleInfo>(PreviousDocument.Schema.CSI_ReferenceNumber2, 150);
		builder.AddColumn<ZCalcEditColumnStyleInfo>(PreviousDocument.Schema.CSI_LineNo, 70);
		builder.AddColumn<ZCalcEditColumnStyleInfo>(PreviousDocument.Schema.CSI_Quantity, 70);
		builder.AddColumn<ZTextBoxColumnStyleInfo>(PreviousDocument.Schema.CSI_Procedure, 100, x => x.IsReadOnly = true);
		return builder.Build();
	}
}
