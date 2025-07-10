using System;
using System.Collections.Generic;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public partial class ExportInvoiceLineUserControl : EU.GUI.EUExportInvoiceLineUserControl
{
	public ExportInvoiceLineUserControl()
	{
		InitializeComponent();

		CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);
	}

	protected override Type GetAdditionalInfosUserControlType() => typeof(PlugIn.AdditionalInfosUserControlWithGrid);

	protected override Type GetPreviousDocumentsUserControlType() => typeof(PlugIn.LayoutPreviousDocumentsUserControl);

	protected override Type GetSupportingDocumentsUserControlType() => typeof(PlugIn.LayoutSupportingDocumentsUserControl);

	protected override void AddColumnsToGrid()
	{
		base.AddColumnsToGrid();

		CustomsInvoiceLinesBoundGrid.ColumnStyles.AddRange(new ZGridColumnInfo[]
		{
			new ZTextBoxColumnStyleInfo(JobComInvoiceLine.Schema.JI_NDescription,  ControlDpiScalingHelper.ScaleToCurrentDpiX(200))
			{
				IsVisible = false
			},
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

		return result.ToArray();
	}

	protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ExportInvoiceLineDetailsLayout();

	protected override ZBool DynamicLayoutApplied => ZBool.True;

	protected override ZBool HasDifferentPanelLayout => ZBool.True;

	protected override Type GetInvoiceLineAuthorisationsUserControlType() => typeof(InvoiceLineAuthorisationsUserControl);
}
