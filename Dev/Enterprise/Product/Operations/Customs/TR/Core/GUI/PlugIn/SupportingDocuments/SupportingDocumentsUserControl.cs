using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.GUI
{
	public partial class SupportingDocumentsUserControl : EU.GUI.PlugIn.SupportingDocumentsUserControl
	{
		public SupportingDocumentsUserControl()
		{
			InitializeComponent();
			InitializeGridLayout();
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			RemoveColumnsExcept(reorderedColumnsSequence.ToList());
			SupportingDocumentsGrid.ReOrderColumns(reorderedColumnsSequence);
		}

		protected override BaseCustomsEntryUserControl GetSupportingDocumentsFieldsControl() => new SupportingDocumentsFieldsControl();

		string[] reorderedColumnsSequence => CachedValueHelper.GetValue(ref reorderedColumnsSequenceCached, () => new string[]
		{
			SupportingDocument.Schema.CSI_Code,
			CSI_CodeDescription,
			SupportingDocument.Schema.CSI_ReferenceNumber,
			SupportingDocument.Schema.CSI_Status,
			SupportingDocument.Schema.CSI_DateOfIssue,
			SupportingDocument.Schema.CSI_DateOfExpiry
		});
		CachedValue<string[]> reorderedColumnsSequenceCached;

		const string CSI_CodeDescription = "CSI_CodeDescription";
	}
}
