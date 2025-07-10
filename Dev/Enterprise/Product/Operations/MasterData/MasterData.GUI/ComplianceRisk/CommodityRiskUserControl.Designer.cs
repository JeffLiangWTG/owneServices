using Enterprise.ZArchitecture;

namespace Enterprise.MasterData.GUI
{
	partial class CommodityRiskUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfoOriginGoods = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo zTariffColumnStyleInfoHarmonizedCode = new Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo();
			Enterprise.ComplianceRisk.GUI.CommodityRiskStatusColumnStyleInfo zCCD_RiskStatusDescription = new Enterprise.ComplianceRisk.GUI.CommodityRiskStatusColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfoJobNumber = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfoNomenclatureAlerts = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfoTariffAlerts = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zCommoditySourceColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ComplianceRisk.GUI.LegalBooksColumnStyleInfo zLegalBookLink = new Enterprise.ComplianceRisk.GUI.LegalBooksColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfoGoodsDescription = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.CommodityRiskGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CommodityRiskGrid)).BeginInit();
			this.CommodityRiskGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ComplianceRisk.Business.ComplianceRiskStatus);
			// 
			// CommodityRiskGrid
			// 
			this.CommodityRiskGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CommodityRiskGrid, "CommodityDetailCollectionView");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceCommodityDetailFilteredCollectionView)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceCommodityDetail)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskStatus)(null)).CommodityDetailCollection)).SyncRoot)).CCD_RiskStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceCommodityDetail)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskStatus)(null)).CommodityDetailCollection)).SyncRoot)).ImportAlertsForExportJobDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceCommodityDetail)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskStatus)(null)).CommodityDetailCollection)).SyncRoot)).CCD_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceCommodityDetail)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskStatus)(null)).CommodityDetailCollection)).SyncRoot)).CCD_RN_NKOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceCommodityDetail)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskStatus)(null)).CommodityDetailCollection)).SyncRoot)).CCD_HarmonizedCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceCommodityDetail)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskStatus)(null)).CommodityDetailCollection)).SyncRoot)).Source)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceCommodityDetail)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskStatus)(null)).CommodityDetailCollection)).SyncRoot)).LegalBookLink)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceCommodityDetail)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskStatus)(null)).CommodityDetailCollection)).SyncRoot)).CommoditySource)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.ComplianceRisk.Business.ComplianceCommodityDetail)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskStatus)(null)).CommodityDetailCollection)).SyncRoot)).CCD_SystemCreateTimeUtc)));
			this.CommodityRiskGrid.CaptionVisible = false;
			zMultiLineTextBoxColumnInfoGoodsDescription.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("306F3E38-D218-4CDD-B62D-1C1B0232DC8A", "Goods Description");
			zMultiLineTextBoxColumnInfoGoodsDescription.ColumnName = "CCD_Description";
			zMultiLineTextBoxColumnInfoGoodsDescription.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiLineTextBoxColumnInfoGoodsDescription.MinimumEditControlWidth = 200;
			zMultiLineTextBoxColumnInfoGoodsDescription.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCodeFindBoxColumnStyleInfoOriginGoods.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("7F3CD848-ACD8-417F-B3C2-7681E74B4FB3", "Origin Of Goods");
			zCodeFindBoxColumnStyleInfoOriginGoods.ColumnName = "CCD_RN_NKOrigin";
			zCodeFindBoxColumnStyleInfoOriginGoods.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTariffColumnStyleInfoHarmonizedCode.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("56E9DF0C-DDF3-4E47-9DC2-B5515E70E7AB", "Harmonized Code");
			zTariffColumnStyleInfoHarmonizedCode.ColumnName = "CCD_HarmonizedCode";
			zTariffColumnStyleInfoHarmonizedCode.TariffType = "HSN";
			zTariffColumnStyleInfoHarmonizedCode.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfoJobNumber.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("D50BB76A-45A8-4460-B8B9-21D17ABD485E", "Job Number");
			zTextBoxColumnStyleInfoJobNumber.ColumnName = "Source";
			zTextBoxColumnStyleInfoJobNumber.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfoNomenclatureAlerts.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("D26F9692-AB3B-4FF5-8064-ED30B32105DD", "Nomenclature Alerts");
			zTextBoxColumnStyleInfoNomenclatureAlerts.ColumnName = "NomenclatureCondition";
			zTextBoxColumnStyleInfoNomenclatureAlerts.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfoTariffAlerts.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("C2682C9D-F9C3-4AC0-B402-651C50A2F401", "Tariff Alerts");
			zTextBoxColumnStyleInfoTariffAlerts.ColumnName = "SpecificCondition";
			zTextBoxColumnStyleInfoTariffAlerts.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145);
			// 
			// zCCD_RiskStatusDescription
			//
			zCCD_RiskStatusDescription.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("F77025BA-25B8-460F-83E5-183E524CD218", "Risk Status");
			zCCD_RiskStatusDescription.ColumnName = "CCD_RiskStatusDescription";
			zCCD_RiskStatusDescription.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCCD_RiskStatusDescription.ShowHorizontalScrollBar = true;
			zCCD_RiskStatusDescription.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			// 
			// zImportAlertsForExportJobDescription
			//
			zImportAlertsForExportJobDescription.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("B68803F4-08EC-4A84-A74C-7681BE412E07", "Import Risk");
			zImportAlertsForExportJobDescription.ColumnName = "ImportAlertsForExportJobDescription";
			zImportAlertsForExportJobDescription.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zImportAlertsForExportJobDescription.IsReadOnly = true;
			// 
			// ZLegalBookLink
			// 
			zLegalBookLink.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("D50BB76A-45A8-4460-B8B9-21D17ABD485G", "Compliance Alerts");
			zLegalBookLink.ColumnName = "LegalBookLink";
			zLegalBookLink.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			// 
			// zCommoditySourceColumnStyleInfo
			//
			zCommoditySourceColumnStyleInfo.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("C2DB041C-FF91-45D4-BC08-CD20201FB41A", "Commodity Source");
			zCommoditySourceColumnStyleInfo.ColumnName = "CommoditySource";
			zCommoditySourceColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			//
			// zDateEditColumnStyleInfo
			//
			zDateEditColumnStyleInfo.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("1BD3CC52-765E-4CCD-A667-DCA128B4D31B", "Date Added (UTC)");
			zDateEditColumnStyleInfo.ColumnName = "CCD_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			zDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDateEditColumnStyleInfo.IsReadOnly = true;
			zDateEditColumnStyleInfo.IsVisible = false;
			// 
			// CommodityRiskGrid
			// 
			this.CommodityRiskGrid.ColumnStyles.Add(zCCD_RiskStatusDescription);
			this.CommodityRiskGrid.ColumnStyles.Add(zTextBoxColumnStyleInfoNomenclatureAlerts);
			this.CommodityRiskGrid.ColumnStyles.Add(zTextBoxColumnStyleInfoTariffAlerts);
			this.CommodityRiskGrid.ColumnStyles.Add(zTariffColumnStyleInfoHarmonizedCode);
			this.CommodityRiskGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfoGoodsDescription);
			this.CommodityRiskGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfoOriginGoods);
			this.CommodityRiskGrid.ColumnStyles.Add(zLegalBookLink);
			this.CommodityRiskGrid.ColumnStyles.Add(zCommoditySourceColumnStyleInfo);
			this.CommodityRiskGrid.ColumnStyles.Add(zTextBoxColumnStyleInfoJobNumber);
			this.CommodityRiskGrid.ColumnStyles.Add(zDateEditColumnStyleInfo);
			this.CommodityRiskGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommodityRiskGrid.GridId = "04663478-5e10-4a9b-a7d8-648a8d8e0102";
			this.CommodityRiskGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CommodityRiskGrid.LayoutKey = "CommodityRiskGrid";
			this.CommodityRiskGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CommodityRiskGrid.Name = "CommodityRiskGrid";
			this.CommodityRiskGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 487, true);
			this.CommodityRiskGrid.TabIndex = 0;
			this.CommodityRiskGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.CommodityRiskGrid_MouseDown);
			this.CommodityRiskGrid.TabStop = false;
			// 
			// CommodityRiskUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CommodityRiskGrid);
			this.Name = "CommodityRiskUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 487, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CommodityRiskGrid)).EndInit();
			this.CommodityRiskGrid.ResumeLayout(false);
			this.CommodityRiskGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zImportAlertsForExportJobDescription = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();

		#endregion

		protected ZGrid CommodityRiskGrid;
		protected Enterprise.ComplianceRisk.GUI.LegalBooksColumnStyleInfo zLegalBookLink;
	}
}
