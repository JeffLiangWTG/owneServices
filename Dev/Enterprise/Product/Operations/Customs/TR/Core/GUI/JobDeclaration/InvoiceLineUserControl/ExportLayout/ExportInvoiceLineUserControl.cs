using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public partial class ExportInvoiceLineUserControl : EU.GUI.EUExportInvoiceLineUserControl
	{
		public ExportInvoiceLineUserControl()
		{
			InitializeComponent();
			SetControlBindingMember();
			ReorderTabPages();
			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);
			InvoiceLineUserControlHelper.InitializeContainerGridLayout(CusContainerInvoiceLineGrid);
		}

		protected override Type GetInvoiceLineVehicleUserControlType() => typeof(InvoiceLineVehicleUserControl);

		protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ExportInvoiceLineDetailsLayout();

		protected override ZBool DynamicLayoutApplied => ZBool.True;

		protected override ZString UniversalTariffType => Universal.Constants.TariffTypes.HarmonizedSystem;

		protected override ZString GetDataGroupingForUniversalTariff() => CurrentInvoiceLine?.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff) ?? ZString.Empty;

		protected override Type GetSupportingDocumentsUserControlType() => typeof(SupportingDocumentsUserControl);

		protected override Type GetAdditionalInfosUserControlType() => typeof(PlugIn.AdditionalInfosUserControl);

		protected override Customs.GUI.InvoiceLineChargesUserControl GetInvoiceLineChargesUserControl()
		{
			return new InvoiceLineChargesUserControl();
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				CustomsInvoiceLinesBoundGrid.ColumnStyles.AddRange(new ZGridColumnInfo[]
				{
					new ZMultiLineTextBoxColumnInfo
					{
						CaptionResourceString = Res.GetData("EB565A88-256B-4DCA-BF72-FC7F6723FFC7", "Turkish Description"),
						ColumnName = JobComInvoiceLine.Schema.JI_NDescription,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200)
					}
				});
			}

			CustomsInvoiceLinesBoundGrid.SetAllColumnsVisible(false);
			CustomsInvoiceLinesBoundGrid.SetColumnVisible(true, ColumnNamesInSortOrder);
			CustomsInvoiceLinesBoundGrid.ReOrderColumns(ColumnNamesInSortOrder);
		}

		string[] ColumnNamesInSortOrder
		{
			get
			{
				if (columnNamesInSortOrder == null)
				{
					var defaultcolumnList = new List<string>(base.GetDefaultColumnsForGrid());
					columnNamesInSortOrder = InvoiceLineUserControlHelper.ColumnNamesInSortOrder(defaultcolumnList);
				}
				return columnNamesInSortOrder;
			}
		}
		string[] columnNamesInSortOrder;

		void ReorderTabPages()
		{
			LineDetailTabControl.TabPages.Remove(LineDetailsTabPage);
			LineDetailTabControl.TabPages.Insert(LineDetailsTabPage, 0);

			LineDetailTabControl.TabPages.Remove(NewLineDetailsTabPage);
			LineDetailTabControl.TabPages.Insert(NewLineDetailsTabPage, 1);

			LineDetailTabControl.TabPages.Remove(ContainersTabPage);
			LineDetailTabControl.TabPages.Insert(ContainersTabPage, 2);

			LineDetailTabControl.TabPages.Remove(LineChargesTabPage);
			LineDetailTabControl.TabPages.Insert(LineChargesTabPage, 3);

			LineDetailTabControl.TabPages.Remove(InvoiceLineTaxTabPage);
			LineDetailTabControl.TabPages.Insert(InvoiceLineTaxTabPage, 4);

			LineDetailTabControl.TabPages.Remove(FiscalReferencesTabPage);
			LineDetailTabControl.TabPages.Insert(FiscalReferencesTabPage, 5);

			LineDetailTabControl.TabPages.Remove(AviationFuelTypeTabPage);
			LineDetailTabControl.TabPages.Insert(AviationFuelTypeTabPage, 6);

			LineDetailTabControl.TabPages.Remove(SupportingDocumentsTabPage);
			LineDetailTabControl.TabPages.Insert(SupportingDocumentsTabPage, 7);

			LineDetailTabControl.TabPages.Remove(AdditionalInfosTabPage);
			LineDetailTabControl.TabPages.Insert(AdditionalInfosTabPage, 8);

			LineDetailTabControl.TabPages.Remove(PreviousDocumentsTabPage);
			LineDetailTabControl.TabPages.Insert(PreviousDocumentsTabPage, 9);

			LineDetailTabControl.TabPages.Remove(PackagesPivotTabPage);
			LineDetailTabControl.TabPages.Insert(PackagesPivotTabPage, 10);

			LineDetailTabControl.TabPages.Remove(InvoiceLinePaymentTabPage);
			LineDetailTabControl.TabPages.Insert(InvoiceLinePaymentTabPage, 11);

			LineDetailTabControl.TabPages.Remove(TaxTabPage);
			LineDetailTabControl.TabPages.Insert(TaxTabPage, 12);
		}

		void SetControlBindingMember()
		{
			StatisticalValueLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_StatisticalValueUSD";
			StatisticalValueLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_CurrencyUSD";
		}
	}
}
