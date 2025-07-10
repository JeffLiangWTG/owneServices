using System;
using System.Collections.Generic;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public partial class ImportInvoiceLineUserControl : EU.GUI.EUImportInvoiceLineUserControl
{
	public ImportInvoiceLineUserControl()
	{
		InitializeComponent();

		CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
	}

	public new JobComInvoiceLine CurrentInvoiceLine => (JobComInvoiceLine)base.CurrentInvoiceLine;

	protected override Type GetAdditionalInfosUserControlType() => typeof(PlugIn.AdditionalInfosUserControlWithGrid);

	protected override Type GetPreviousDocumentsUserControlType() => typeof(PlugIn.LayoutPreviousDocumentsUserControl);

	protected override Type GetSupportingDocumentsUserControlType() => typeof(PlugIn.LayoutSupportingDocumentsUserControl);

	protected override Type GetOrganizationsUserControlType() => typeof(EU.GUI.PlugIn.InvoiceLineOrganizationsUserControl);

	protected override Type GetValuationIndicatorsUserControlType() => typeof(EU.GUI.InvoiceLineValuationIndicatorDropEditsUserControl);

	protected override CargoWiseOne.ResourceStrings.ResourceStringData GetAdditionalInfosTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration) => EU.GUI.CaptionProvider.AdditionalInfo_44;

	protected override void InitializeGridLayoutCore()
	{
		base.InitializeGridLayoutCore();
		var valuationCodeColumnStyle = CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_ValuationCode);
		valuationCodeColumnStyle.IsMandatory = true;
	}

	protected override void AddColumnsToGrid()
	{
		base.AddColumnsToGrid();

		CustomsInvoiceLinesBoundGrid.ColumnStyles.AddRange(new ZGridColumnInfo[]
		{
			new ZTextBoxColumnStyleInfo(JobComInvoiceLine.Schema.JI_NDescription, ControlDpiScalingHelper.ScaleToCurrentDpiX(200)),
			new ZDateEditColumnStyleInfo(JobComInvoiceLine.Schema.JI_DateForDutyOverride, ControlDpiScalingHelper.ScaleToCurrentDpiX(80)),
			new ZDateEditColumnStyleInfo(JobComInvoiceLine.Schema.JI_ValuationDateOverride, ControlDpiScalingHelper.ScaleToCurrentDpiX(80)),
		});
	}

	protected override string[] GetDefaultColumnsForGrid()
	{
		var result = new List<string>(base.GetDefaultColumnsForGrid());
		foreach (var columnName in result)
		{
			if (columnName == JobComInvoiceLine.Schema.JI_Description)
			{
				var index = result.IndexOf(columnName);
				result.Insert(index, JobComInvoiceLine.Schema.JI_NDescription);
				break;
			}
		}

		result.Add(JobComInvoiceLine.Schema.JI_DateForDutyOverride);
		result.Add(JobComInvoiceLine.Schema.JI_ValuationDateOverride);
		result.Add(JobComInvoiceLine.Schema.JI_ValuationCode);

		return result.ToArray();
	}

	protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ImportInvoiceLineDetailsLayout();

	protected override ZBool DynamicLayoutApplied => ZBool.True;

	protected override ZBool HasDifferentPanelLayout => ZBool.True;
}
