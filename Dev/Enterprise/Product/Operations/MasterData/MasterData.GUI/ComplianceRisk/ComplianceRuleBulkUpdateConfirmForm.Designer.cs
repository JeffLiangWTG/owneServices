namespace Enterprise.MasterData.GUI
{
	partial class ComplianceRuleBulkUpdateConfirmForm
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
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ComplianceRuleGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ComplianceRuleGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CountryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CountryGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BulkUpdateTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.ButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ButtonOK = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ComplianceRuleGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ComplianceRuleGrid)).BeginInit();
			this.ComplianceRuleGrid.SuspendLayout();
			this.CountryGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CountryGrid)).BeginInit();
			this.CountryGrid.SuspendLayout();
			this.BulkUpdateTableLayoutPanel.SuspendLayout();
			this.ButtonsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 321, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 16, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ComplianceRisk.Business.ComplianceRuleWrapper);
			// 
			// ComplianceRuleGroupBox
			// 
			this.ComplianceRuleGroupBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("c772f64a-b0d3-4ba7-9740-09f76b3f7cf4", "Compliance Rules");
			this.ComplianceRuleGroupBox.Controls.Add(this.ComplianceRuleGrid);
			this.ComplianceRuleGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComplianceRuleGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.ComplianceRuleGroupBox.Name = "ComplianceRuleGroupBox";
			this.ComplianceRuleGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(595, 141, true);
			this.ComplianceRuleGroupBox.TabIndex = 0;
			this.ComplianceRuleGroupBox.TabStop = false;
			// 
			// ComplianceRuleGrid
			// 
			this.ComplianceRuleGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ComplianceRuleGrid, "Rules");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRuleWrapper)(null)).Rules)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceRuleCandidate)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRuleWrapper)(null)).Rules)).SyncRoot)).Origin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceRuleCandidate)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRuleWrapper)(null)).Rules)).SyncRoot)).Destination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceRuleCandidate)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRuleWrapper)(null)).Rules)).SyncRoot)).HarmonizedCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceRuleCandidate)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRuleWrapper)(null)).Rules)).SyncRoot)).RiskStatus)));
			this.ComplianceRuleGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("ff5f818b-1b46-4fba-a13a-56e5c2c87c6a", "Origin");
			zTextBoxColumnStyleInfo1.ColumnName = "Origin";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("891f4a86-1338-4007-b17e-afe733515605", "Destination");
			zTextBoxColumnStyleInfo2.ColumnName = "Destination";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("5e943c5c-3035-4a66-8735-5066cb8a2e2b", "Harmonized Code");
			zTextBoxColumnStyleInfo3.ColumnName = "HarmonizedCode";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("8264AABB-D292-43CF-B35E-ADC99E186ECA", "Risk Status");
			zTextBoxColumnStyleInfo6.ColumnName = "RiskStatus";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ComplianceRuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ComplianceRuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ComplianceRuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ComplianceRuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ComplianceRuleGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComplianceRuleGrid.GridId = "f0bd4c45-4f81-403d-b950-e3dd582e23b6";
			this.ComplianceRuleGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ComplianceRuleGrid.LayoutKey = "ComplianceRuleGrid";
			this.ComplianceRuleGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.ComplianceRuleGrid.Name = "ComplianceRuleGrid";
			this.ComplianceRuleGrid.ReadOnly = true;
			this.ComplianceRuleGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 124, true);
			this.ComplianceRuleGrid.TabIndex = 0;
			// 
			// CountryGroupBox
			// 
			this.CountryGroupBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("3f9ff8ad-8732-44ba-9e1a-e22fb3dde2b7", "Countries/Regions");
			this.CountryGroupBox.Controls.Add(this.CountryGrid);
			this.CountryGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CountryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 148, true);
			this.CountryGroupBox.Name = "CountryGroupBox";
			this.CountryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(595, 141, true);
			this.CountryGroupBox.TabIndex = 0;
			this.CountryGroupBox.TabStop = false;
			// 
			// CountryGrid
			// 
			this.CountryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CountryGrid, "Countries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRuleWrapper)(null)).Countries)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceRuleTargetCountry)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRuleWrapper)(null)).Countries)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceRuleTargetCountry)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRuleWrapper)(null)).Countries)).SyncRoot)).Name)));
			this.CountryGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("5237d431-5e5c-43d4-98d0-37a6c1a6f025", "Code");
			zTextBoxColumnStyleInfo4.ColumnName = "Code";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("ee4404e3-dc14-4bea-be50-8735aadf3ab1", "Name");
			zTextBoxColumnStyleInfo5.ColumnName = "Name";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.CountryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.CountryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.CountryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CountryGrid.GridId = "51aa25d1-5bdb-423b-b112-486434707262";
			this.CountryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CountryGrid.LayoutKey = "CountryGrid";
			this.CountryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.CountryGrid.Name = "CountryGrid";
			this.CountryGrid.ReadOnly = true;
			this.CountryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 124, true);
			this.CountryGrid.TabIndex = 0;
			// 
			// BulkUpdateTableLayoutPanel
			// 
			this.BulkUpdateTableLayoutPanel.ColumnCount = 1;
			this.BulkUpdateTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.BulkUpdateTableLayoutPanel.Controls.Add(this.ButtonsPanel, 0, 2);
			this.BulkUpdateTableLayoutPanel.Controls.Add(this.ComplianceRuleGroupBox, 0, 0);
			this.BulkUpdateTableLayoutPanel.Controls.Add(this.CountryGroupBox, 0, 1);
			this.BulkUpdateTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BulkUpdateTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BulkUpdateTableLayoutPanel.Name = "BulkUpdateTableLayoutPanel";
			this.BulkUpdateTableLayoutPanel.RowCount = 3;
			this.BulkUpdateTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.BulkUpdateTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.BulkUpdateTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(28)));
			this.BulkUpdateTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 321, true);
			this.BulkUpdateTableLayoutPanel.TabIndex = 1;
			// 
			// ButtonsPanel
			// 
			this.ButtonsPanel.Controls.Add(this.ButtonOK);
			this.ButtonsPanel.Controls.Add(this.ButtonCancel);
			this.ButtonsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 294, true);
			this.ButtonsPanel.Name = "ButtonsPanel";
			this.ButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(595, 23, true);
			this.ButtonsPanel.TabIndex = 2;
			// 
			// ButtonOK
			// 
			this.ButtonOK.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("234a8c63-0ea0-496e-9162-eed796c04e4b", "OK");
			this.ButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ButtonOK.IsCaptionOverridden = false;
			this.ButtonOK.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(443, 1, true);
			this.ButtonOK.Name = "ButtonOK";
			this.ButtonOK.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 22, true);
			this.ButtonOK.TabIndex = 0;
			this.ButtonOK.ToolTipCaption = null;
			this.ButtonOK.UseVisualStyleBackColor = true;
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("3e0809e0-357f-48fc-82cb-4a0ba9d80ee8", "Cancel");
			this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.IsCaptionOverridden = false;
			this.ButtonCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 1, true);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 22, true);
			this.ButtonCancel.TabIndex = 1;
			this.ButtonCancel.ToolTipCaption = null;
			this.ButtonCancel.UseVisualStyleBackColor = true;
			// 
			// ComplianceRuleBulkUpdateConfirmForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("37327598-3bcf-4a64-b342-6dbd080bbdfe", "Copy Compliance Rule Summary");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 337, true);
			this.Controls.Add(this.BulkUpdateTableLayoutPanel);
			this.DataSourceType = typeof(Enterprise.ComplianceRisk.Business.ComplianceRuleWrapper);
			this.Name = "ComplianceRuleBulkUpdateConfirmForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BulkUpdateTableLayoutPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ComplianceRuleGroupBox.ResumeLayout(false);
			this.ComplianceRuleGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ComplianceRuleGrid)).EndInit();
			this.ComplianceRuleGrid.ResumeLayout(false);
			this.ComplianceRuleGrid.PerformLayout();
			this.CountryGroupBox.ResumeLayout(false);
			this.CountryGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CountryGrid)).EndInit();
			this.CountryGrid.ResumeLayout(false);
			this.CountryGrid.PerformLayout();
			this.BulkUpdateTableLayoutPanel.ResumeLayout(false);
			this.BulkUpdateTableLayoutPanel.PerformLayout();
			this.ButtonsPanel.ResumeLayout(false);
			this.ButtonsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox CountryGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ComplianceRuleGroupBox;
		internal Enterprise.ZArchitecture.ZGrid CountryGrid;
		internal Enterprise.ZArchitecture.ZGrid ComplianceRuleGrid;
		private CargoWise.Windows.UI.KTableLayoutPanel BulkUpdateTableLayoutPanel;
		private ZArchitecture.GUI.ZPanel ButtonsPanel;
		private ZArchitecture.GUI.ZButton ButtonOK;
		private ZArchitecture.GUI.ZButton ButtonCancel;
	}
}
