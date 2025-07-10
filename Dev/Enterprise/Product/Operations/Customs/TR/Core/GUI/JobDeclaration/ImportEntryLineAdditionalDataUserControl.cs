using System;
using System.Collections.Immutable;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.TR.GUI
{
	public partial class ImportEntryLineAdditionalDataUserControl : EU.GUI.EntryLineAdditionalDataUserControl
	{
		public ImportEntryLineAdditionalDataUserControl()
		{
			InitializeComponent();
			SetUpEntryLineSupportingDocumentsGridColumns();
			SetCaptions();
		}

		protected override Type GetDutyAndTaxDetailsUserControlType() => typeof(EntryLineTaxAndFeeUserControl);

		void SetCaptions()
		{
			EntryLineSupportingDocumentsGrid.SetColumnCaption(SupportingDocument.Schema.CSI_Code, Enterprise.Customs.TR.GUI.Res.GetData("44C9786D-88F4-413C-9772-8E6AE0B73CB0", "Code").Caption);
		}

		void SetUpEntryLineSupportingDocumentsGridColumns()
		{
			EntryLineSupportingDocumentsGrid.ColumnStyles.AddRange(new ZGridColumnInfo[]
			{
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = CSI_StatusDescription,
					CaptionResourceString = Res.GetData("B4F91DE1-C476-4C87-BC47-F8F60D852FC0", "Availability"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsCustomColumn = false
				}
			});

			EntryLineSupportingDocumentsGrid.SetAllColumnsVisible(false);
			EntryLineSupportingDocumentsGrid.SetColumnVisible(true, entryInstructionsGridColumns.ToArray());
			EntryLineSupportingDocumentsGrid.ReOrderColumns(entryInstructionsGridColumns.ToArray());
		}

		readonly ImmutableArray<string> entryInstructionsGridColumns = new string[]
		{
			SupportingDocument.Schema.CSI_Code,
			CSI_CodeDescription,
			CSI_StatusDescription,
			SupportingDocument.Schema.CSI_ReferenceNumber,
			SupportingDocument.Schema.CSI_DateOfIssue,
			SupportingDocument.Schema.CSI_DateOfExpiry,
		}.ToImmutableArray();

		const string CSI_CodeDescription = "CSI_CodeDescription";
		const string CSI_StatusDescription = "CSI_StatusDescription";
	}
}

