using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public partial class ImportInvoiceLineUserControl : EU.GUI.EUImportInvoiceLineUserControl
	{
		public ImportInvoiceLineUserControl()
		{
			InitializeComponent();
			SetControlBindingMember();
			ReorderTabPages();
			Captions();
			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
			InvoiceLineUserControlHelper.InitializeContainerGridLayout(CusContainerInvoiceLineGrid);

			var columnSeparator0 = this.FindSingle<Control>("ColumnSeparator0");
			this.FindSingle<Control>("FormattedProcedureCodeFindBox").AllowOverlap(columnSeparator0);
			this.FindSingle<Control>("AdditionalSupplementaryCodesUserControl").AllowOverlap(columnSeparator0);
			this.FindSingle<Control>("SupplementaryCode1AndGDMUserControl").AllowOverlap(columnSeparator0);
			this.FindSingle<Control>("CommercialPaymentCodeDropEdit").AllowOverlap(columnSeparator0);
			this.FindSingle<Control>("PartNoCodeFindBox").AllowOverlap(columnSeparator0);
			this.FindSingle<Control>("BrandNameTextBox").AllowOverlap(columnSeparator0);
			this.FindSingle<Control>("UnicodeDescriptionLongTextControl").AllowOverlap(columnSeparator0);
			this.FindSingle<Control>("DescriptionLongTextControl").AllowOverlap(columnSeparator0);
			this.FindSingle<Control>("CountryOfOriginDropEdit").AllowOverlap(columnSeparator0);
			var invoiceLineDetailsUserControl = this.FindSingle<Control>("InvoiceLineDetailsUserControl");
			invoiceLineDetailsUserControl.FindSingle<Control>("PreferenceCodeDropEdit").AllowOverlap(columnSeparator0);

			var columnSeparator1 = this.FindSingle<Control>("ColumnSeparator1");
			this.FindSingle<Control>("InwardProcessingLicenseLineNumberTextBox").AllowOverlap(columnSeparator1);
			this.FindSingle<Control>("UsedGoodsCodeDropEdit").AllowOverlap(columnSeparator1);
			this.FindSingle<Control>("ReturnToOriginCheckBox").AllowOverlap(columnSeparator1);
			this.FindSingle<Control>("SecondaryTreatedProductCheckBox").AllowOverlap(columnSeparator1);
			this.FindSingle<Control>("ReturningGoodsReasonCodeDropEdit").AllowOverlap(columnSeparator1);
			this.FindSingle<Control>("EntryExitPurposeCodeDropEdit").AllowOverlap(columnSeparator1);
			this.FindSingle<Control>("ManufacturerAddressControl").AllowOverlap(columnSeparator1);
		}

		void Captions()
		{
			JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("ImportInvoiceLineUserControl|A0899459-B656-421E-B093-5C3D09AF570E", "Deferred VAT");
		}

		protected override Type GetInvoiceLineVehicleUserControlType() => typeof(InvoiceLineVehicleUserControl);

		protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ImportInvoiceLineDetailsLayout();

		protected override ZBool DynamicLayoutApplied => ZBool.True;

		protected override ZString UniversalTariffType => Universal.Constants.TariffTypes.HarmonizedSystem;

		protected override ZString GetDataGroupingForUniversalTariff() => CurrentInvoiceLine?.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff) ?? ZString.Empty;

		protected override Type GetSupportingDocumentsUserControlType() => typeof(SupportingDocumentsUserControl);

		protected override Type GetAdditionalInfosUserControlType() => typeof(PlugIn.AdditionalInfosUserControl);

		protected override ResourceStringData GetAdditionalInfosTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration) => EU.GUI.CaptionProvider.AdditionalInfo_44;

		protected override Customs.GUI.InvoiceLineChargesUserControl GetInvoiceLineChargesUserControl()
		{
			return new InvoiceLineChargesUserControl();
		}

		protected Type GetInvoiceLineTaxUserControlType() => typeof(InvoiceLineTaxUserControl);

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				CustomsInvoiceLinesBoundGrid.ColumnStyles.AddRange(new ZGridColumnInfo[]
				{
					new ZMultiLineTextBoxColumnInfo
					{
						CaptionResourceString = Res.GetData("7F703561-C9AB-4E93-9851-73849AA01870", "Turkish Description"),
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
			LineDetailTabControl.TabPages.Remove(NewLineDetailsTabPage);
			LineDetailTabControl.TabPages.Insert(NewLineDetailsTabPage, 0);

			LineDetailTabControl.TabPages.Remove(LineChargesTabPage);
			LineDetailTabControl.TabPages.Insert(LineChargesTabPage, 1);

			LineDetailTabControl.TabPages.Remove(InvoiceLineTaxTabPage);
			LineDetailTabControl.TabPages.Insert(InvoiceLineTaxTabPage, 2);

			LineDetailTabControl.TabPages.Remove(SupportingDocumentsTabPage);
			LineDetailTabControl.TabPages.Insert(SupportingDocumentsTabPage, 3);

			LineDetailTabControl.TabPages.Remove(AdditionalInfosTabPage);
			LineDetailTabControl.TabPages.Insert(AdditionalInfosTabPage, 4);

			LineDetailTabControl.TabPages.Remove(PreviousDocumentsTabPage);
			LineDetailTabControl.TabPages.Insert(PreviousDocumentsTabPage, 5);

			LineDetailTabControl.TabPages.Remove(PackagesPivotTabPage);
			LineDetailTabControl.TabPages.Insert(PackagesPivotTabPage, 6);

			LineDetailTabControl.TabPages.Remove(InvoiceLinePaymentTabPage);
			LineDetailTabControl.TabPages.Insert(InvoiceLinePaymentTabPage, 9);
		}

		void SetControlBindingMember()
		{
			StatisticalValueLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_StatisticalValueUSD";
			StatisticalValueLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_CurrencyUSD";
		}
	}
}
