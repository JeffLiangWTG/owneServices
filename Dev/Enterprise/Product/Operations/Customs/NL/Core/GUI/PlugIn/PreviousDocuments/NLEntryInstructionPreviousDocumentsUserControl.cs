using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.NL.GUI;

public partial class NLEntryInstructionPreviousDocumentsUserControl : EU.GUI.PlugIn.PreviousDocumentsUserControl
{
	public NLEntryInstructionPreviousDocumentsUserControl()
	{
		InitializeComponent();
		InitializeGridLayout();
	}

	protected override void InitializeGridLayoutCore()
	{
		base.InitializeGridLayoutCore();

		ZGridColumnInfo removeClassColumn = PreviousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_SubType);
		ZGridColumnInfo removeDateOfIssueColumn = PreviousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_DateOfIssue);
		ZGridColumnInfo lineNoColumn = PreviousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_LineNo);
		PreviousDocumentsGrid.ColumnStyles.Remove(removeClassColumn);
		PreviousDocumentsGrid.ColumnStyles.Remove(removeDateOfIssueColumn);
		PreviousDocumentsGrid.ColumnStyles.Remove(lineNoColumn);
	}
}
