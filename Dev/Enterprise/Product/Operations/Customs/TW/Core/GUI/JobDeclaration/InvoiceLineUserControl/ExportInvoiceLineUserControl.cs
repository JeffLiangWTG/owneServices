using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class ExportInvoiceLineUserControl : BaseInvoiceLineUserControl
	{
		public ExportInvoiceLineUserControl()
		{
			InitializeComponent();

			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);
			var procedureColumnInfo = CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Procedure);
			procedureColumnInfo.CaptionResourceString = this.LineDetailsUserControl.JI_ProcedureDropEdit.CaptionResourceString;
			procedureColumnInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);

			PendingApportionmentLabel.AllowOverlap(CurrentInvoiceQuantityGroupPanel);
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			var isVisibleJI_BondedGoodsCode = JobDeclaration.JE_MessageType != JobMessageTypeList.Codes.MiscellaneousCustoms;
			CustomsInvoiceLinesBoundGrid.SetColumnVisible(isVisibleJI_BondedGoodsCode, JobComInvoiceLine.Schema.JI_BondedGoodsCode);
			CustomsInvoiceLinesBoundGrid.SetAvailability(isVisibleJI_BondedGoodsCode, JobComInvoiceLine.Schema.JI_BondedGoodsCode);
		}

		protected override LineDetailsUserControl GetLineDetailsUserControl() => new ExportLineDetailsUserControl();

		protected override void ResetCustomsInvoiceLinesBoundGridColumns()
		{
			base.ResetCustomsInvoiceLinesBoundGridColumns();

			#region Foreign Manufacturer

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZGuidFindBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("F30AF08C-0AFF-477E-8F05-8A7D3BB0D082", "Manuf.", "Manufacturer", ""),
				ColumnName = JobComInvoiceLine.Schema.ManufacturerDocAddressOrgPK,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				BindToList = "Lookups.ManufacturerList"
			});

			#endregion

			#region Licensing - Certificate of Origin

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("BCD65475-295E-408F-99B1-37ED0873F9D1", "Permit Unit Price"),
				ColumnName = JobComInvoiceLine.Schema.JI_PermitUnitPrice,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				BindToDecimalPlaces = null,
				ShowEmptyStringForEmptyValue = false
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				CaptionResourceString = Res.GetData("221D539A-3412-47AC-B1CE-B139F73778F1", "Origin Criteria"),
				ColumnName = JobComInvoiceLine.Schema.JI_OriginCriteria,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				CaptionResourceString = Res.GetData("BDCEAB1A-41FB-40D1-B00D-609F5D490511", "Preferential Treatment Criteria"),
				ColumnName = JobComInvoiceLine.Schema.JI_PTCriteria,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200),
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				CaptionResourceString = Res.GetData("F864687E-38F7-441D-839D-9A8A13A5FF51", "Other PT Criteria"),
				ColumnName = JobComInvoiceLine.Schema.JI_PTCriteria2,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				CaptionResourceString = Res.GetData("42F05E0D-82A2-4130-AF51-7E81D9193478", "Mfr. Rel", "Mfr. Relationship", "Manufacturer Relationship"),
				ColumnName = JobComInvoiceLine.Schema.JI_ManufacturerRelationship,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("04F3E3AA-3914-413C-AADD-8AB7D9918024", "Tariff Printing"),
				ColumnName = JobComInvoiceLine.Schema.JI_TariffPrintLength,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				CaptionResourceString = Res.GetData("B728D308-B003-4541-9DB5-B22341427F68", "Import Country's Tariff"),
				ColumnName = JobComInvoiceLine.Schema.JI_IMPTariff,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180),
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZMultiLineTextBoxColumnInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				CaptionResourceString = Res.GetData("F77545F2-10BE-4DB5-9287-E5064FCD7ADA", "Shipping Marks"),
				ColumnName = JobComInvoiceLine.Schema.NX101ShippingMarks,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
			});

			#endregion
		}
	}
}
