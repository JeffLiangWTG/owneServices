using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.NL.GUI;

public partial class NLPreviousDocumentsUserControl : EU.GUI.PlugIn.PreviousDocumentsUserControl
{
	public NLPreviousDocumentsUserControl()
	{
		InitializeComponent();
		InitializeGridLayout();
	}

	protected override void InitializeGridLayoutCore()
	{
		base.InitializeGridLayoutCore();

		ZGridColumnInfo removeClassColumn = PreviousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_SubType);
		ZGridColumnInfo removeDateOfIssueColumn = PreviousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_DateOfIssue);
		PreviousDocumentsGrid.ColumnStyles.Remove(removeClassColumn);
		PreviousDocumentsGrid.ColumnStyles.Remove(removeDateOfIssueColumn);
	}
}
