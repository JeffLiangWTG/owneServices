namespace Enterprise.Customs.US.GUI
{
	partial class PartImportClassificationForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.Customs.US.GUI.TariffColumnStyleInfo tariffColumnStyleInfo1 = new Enterprise.Customs.US.GUI.TariffColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo provTariffColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo additionalTariffColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo additionalTariffColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo additionalTariffColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo additionalTariffColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo additionalTariffColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.closeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.pivotGrid = new Enterprise.ZArchitecture.ZGrid();
			this.importClassificationUserControl = new Enterprise.Customs.US.GUI.ImportClassificationUserControl(pivotGrid);
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.bottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pivotGrid)).BeginInit();
			this.pivotGrid.SuspendLayout();
			this.importClassificationUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 508, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1217, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.CusClassPartPivot);
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.closeButton);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 472, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1217, 36, true);
			this.bottomPanel.TabIndex = 2;
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1137, 6, true);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.closeButton.TabIndex = 0;
			this.closeButton.Text = "&Close";
			this.closeButton.UseVisualStyleBackColor = true;
			// 
			// pivotGrid
			// 
			this.pivotGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.pivotGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CI_ChildListOrder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CI_UsageComment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CI_ChildType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CI_ChildTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CI_FormattedTariffNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CI_FormattedSupplementalTariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CD_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CD_WeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CD_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).SupFormattedAdditionalTariff1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).SupFormattedAdditionalTariff2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).SupFormattedAdditionalTariff3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).SupFormattedAdditionalTariff4)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).SupFormattedAdditionalTariff5)));
			this.pivotGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CI_ChildListOrder";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CI_UsageComment";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(103);
			zDropEditColumnStyleInfo1.ColumnName = "CI_ChildType";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("PartImportClassificationForm|6b0abd44-d184-4e12-9b69-1c2c90634060", "Type Desc.", "Type Description", "");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "CI_ChildTypeDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			tariffColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("PartImportClassificationForm|0a6e7d0f-90a2-4632-9905-080c3d2eb56c", "Tariff");
			tariffColumnStyleInfo1.ColumnName = "CI_FormattedTariffNum";
			tariffColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			provTariffColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("PartImportClassificationForm|68e51258-88e2-481a-bca3-a479e5d5877e", "Prov/Prog. Tariff", "Provision or Program Tariff", "");
			provTariffColumnStyleInfo1.ColumnName = "CI_FormattedSupplementalTariff";
			provTariffColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("PartImportClassificationForm|d82ca101-7476-4409-a6b6-00c3e2e3d4e6", "Gross Weight");
			zCalcEditColumnStyleInfo2.ColumnName = "CD_GrossWeight";
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Customs.US.GUI.Res.GetData("PartImportClassificationForm|4473fee3-dbe0-417f-8c96-b3d24d046117", "Weight");
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("PartImportClassificationForm|5d468cae-6b24-4285-80fc-c4ec45ae1e89", "UQ");
			zDropEditColumnStyleInfo2.ColumnName = "CD_WeightUQ";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.US.GUI.Res.GetData("PartImportClassificationForm|4473fee3-dbe0-417f-8c96-b3d24d046117", "Weight");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("PartImportClassificationForm|d1b12fa4-c4bd-4ca4-8c2c-120ff3dc76f6", "Net Weight");
			zCalcEditColumnStyleInfo3.ColumnName = "CD_NetWeight";
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Customs.US.GUI.Res.GetData("PartImportClassificationForm|4473fee3-dbe0-417f-8c96-b3d24d046117", "Weight");
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			additionalTariffColumnStyleInfo1.ColumnName = "SupFormattedAdditionalTariff1";
			additionalTariffColumnStyleInfo1.IsVisible = false;
			additionalTariffColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			additionalTariffColumnStyleInfo2.ColumnName = "SupFormattedAdditionalTariff2";
			additionalTariffColumnStyleInfo2.IsVisible = false;
			additionalTariffColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			additionalTariffColumnStyleInfo3.ColumnName = "SupFormattedAdditionalTariff3";
			additionalTariffColumnStyleInfo3.IsVisible = false;
			additionalTariffColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			additionalTariffColumnStyleInfo4.ColumnName = "SupFormattedAdditionalTariff4";
			additionalTariffColumnStyleInfo4.IsVisible = false;
			additionalTariffColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			additionalTariffColumnStyleInfo5.ColumnName = "SupFormattedAdditionalTariff5";
			additionalTariffColumnStyleInfo5.IsVisible = false;
			additionalTariffColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.pivotGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.pivotGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.pivotGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.pivotGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.pivotGrid.ColumnStyles.Add(tariffColumnStyleInfo1);
			this.pivotGrid.ColumnStyles.Add(provTariffColumnStyleInfo1);
			this.pivotGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.pivotGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.pivotGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.pivotGrid.ColumnStyles.Add(additionalTariffColumnStyleInfo1);
			this.pivotGrid.ColumnStyles.Add(additionalTariffColumnStyleInfo2);
			this.pivotGrid.ColumnStyles.Add(additionalTariffColumnStyleInfo3);
			this.pivotGrid.ColumnStyles.Add(additionalTariffColumnStyleInfo4);
			this.pivotGrid.ColumnStyles.Add(additionalTariffColumnStyleInfo5);
			this.pivotGrid.CopySelectedRowsAllowed = true;
			this.pivotGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pivotGrid.GridId = "31412c87-b5f2-47d0-a589-481e2da941f4";
			this.pivotGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.pivotGrid.LayoutKey = "pivotGrid";
			this.pivotGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.pivotGrid.Name = "pivotGrid";
			this.pivotGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1217, 122, true);
			this.pivotGrid.TabIndex = 0;
			// 
			// importClassificationUserControl
			// 
			this.importClassificationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.importClassificationUserControl, ".");
			this.importClassificationUserControl.CurrentPivot = null;
			this.importClassificationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.importClassificationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.importClassificationUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 0, true);
			this.importClassificationUserControl.Name = "importClassificationUserControl";
			this.importClassificationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1217, 346, true);
			this.importClassificationUserControl.TabIndex = 1;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.pivotGrid);
			this.splitContainer1.Panel1MinSize = 50;
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.importClassificationUserControl);
			this.splitContainer1.Panel2MinSize = 100;
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1217, 472, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(122);
			this.splitContainer1.TabIndex = 1;
			// 
			// PartImportClassificationForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.closeButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1217, 532, true);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.bottomPanel);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.CusClassPartPivot);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1232, 503, true);
			this.Name = "PartImportClassificationForm";
			this.Text = "PartImportClassificationForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.bottomPanel, 0);
			this.Controls.SetChildIndex(this.splitContainer1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.bottomPanel.ResumeLayout(false);
			this.bottomPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pivotGrid)).EndInit();
			this.pivotGrid.ResumeLayout(false);
			this.pivotGrid.PerformLayout();
			this.importClassificationUserControl.ResumeLayout(true);
			this.importClassificationUserControl.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel bottomPanel;
		private Enterprise.ZArchitecture.GUI.ZButton closeButton;
		internal ImportClassificationUserControl importClassificationUserControl;
		private Enterprise.ZArchitecture.ZGrid pivotGrid;
		private CargoWise.Windows.UI.KSplitContainer splitContainer1;

	}
}
