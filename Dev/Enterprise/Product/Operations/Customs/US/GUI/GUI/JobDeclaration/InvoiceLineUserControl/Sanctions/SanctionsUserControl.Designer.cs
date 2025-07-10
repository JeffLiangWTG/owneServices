namespace Enterprise.Customs.US.GUI
{
	partial class SanctionsUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.DisclaimSanctionsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CopyNMFSDataToSanctionsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FishingInformationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FishingInformationGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MiningInformationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MiningInformationGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FishingInformationGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FishingInformationGrid)).BeginInit();
			this.FishingInformationGrid.SuspendLayout();
			this.MiningInformationGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MiningInformationGrid)).BeginInit();
			this.MiningInformationGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.JobComInvoiceLine);
			// 
			// DisclaimSanctionsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.DisclaimSanctionsCheckBox, "US_DisclaimSanctions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).US_DisclaimSanctions)));
			this.DisclaimSanctionsCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.DisclaimSanctionsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 12, true);
			this.DisclaimSanctionsCheckBox.Name = "DisclaimSanctionsCheckBox";
			this.DisclaimSanctionsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 19, true);
			this.DisclaimSanctionsCheckBox.TabIndex = 0;
			this.DisclaimSanctionsCheckBox.Text = "Disclaim Sanctions:";
			this.DisclaimSanctionsCheckBox.UseVisualStyleBackColor = true;
			// 
			// CopyNMFSDataToSanctionsButton
			// 
			this.CopyNMFSDataToSanctionsButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.CopyNMFSDataToSanctionsButton.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("E65C637B-8F33-4584-95C5-51049C6D7F07", "Copy NMFS data to Sanctions");
			this.CopyNMFSDataToSanctionsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 8, true);
			this.CopyNMFSDataToSanctionsButton.Name = "CopyNMFSDataToSanctionsButton";
			this.CopyNMFSDataToSanctionsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 23, true);
			this.CopyNMFSDataToSanctionsButton.TabIndex = 1;
			this.CopyNMFSDataToSanctionsButton.ToolTipCaption = null;
			this.CopyNMFSDataToSanctionsButton.UseVisualStyleBackColor = true;
			this.CopyNMFSDataToSanctionsButton.Click += new System.EventHandler(this.CopyNMFSDataToSanctions_Click);
			// 
			// FishingInformationGroupBox
			// 
			this.FishingInformationGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("786C290A-D730-4758-AD17-D6DF8C877657", "Fishing Information");
			this.FishingInformationGroupBox.Controls.Add(this.FishingInformationGrid);
			this.FishingInformationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 37, true);
			this.FishingInformationGroupBox.Name = "FishingInformationGroupBox";
			this.FishingInformationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 180, true);
			this.FishingInformationGroupBox.TabIndex = 2;
			this.FishingInformationGroupBox.TabStop = false;
			// 
			// FishingInformationGrid
			// 
			this.FishingInformationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.FishingInformationGrid, "FishingInformations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).FishingInformations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FishingInformation)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).FishingInformations)).SyncRoot)).US_MethodOfHarvest)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FishingInformation)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).FishingInformations)).SyncRoot)).US_VesselName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FishingInformation)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).FishingInformations)).SyncRoot)).US_VesselCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FishingInformation)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).FishingInformations)).SyncRoot)).US_HarvestedCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FishingInformation)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).FishingInformations)).SyncRoot)).US_VesselIMO)));
			this.FishingInformationGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "US_MethodOfHarvest";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "US_VesselName";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "US_VesselCountry";
			zCodeFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo3.ColumnName = "US_HarvestedCountry";
			zCodeFindBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo1.ColumnName = "US_VesselIMO";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.FishingInformationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.FishingInformationGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.FishingInformationGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.FishingInformationGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.FishingInformationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FishingInformationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FishingInformationGrid.GridId = "13cd9e01-7e44-4b59-8cd5-650ab4b056c5";
			this.FishingInformationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FishingInformationGrid.LayoutKey = "FishingInformationGrid";
			this.FishingInformationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.FishingInformationGrid.Name = "FishingInformationGrid";
			this.FishingInformationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 161, true);
			this.FishingInformationGrid.TabIndex = 0;
			// 
			// MiningInformationGroupBox
			// 
			this.MiningInformationGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("A7CD3111-7CB2-406E-9E38-A667E576E5AA", "Mining Information");
			this.MiningInformationGroupBox.Controls.Add(this.MiningInformationGrid);
			this.MiningInformationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 37, true);
			this.MiningInformationGroupBox.Name = "MiningInformationGroupBox";
			this.MiningInformationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 180, true);
			this.MiningInformationGroupBox.TabIndex = 2;
			this.MiningInformationGroupBox.TabStop = false;
			// 
			// MiningInformationGrid
			// 
			this.MiningInformationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MiningInformationGrid, "MiningInformations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).MiningInformations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.MiningInformation)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).MiningInformations)).SyncRoot)).CountryOfMining)));
			this.MiningInformationGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo4.ColumnName = "CountryOfMining";
			zCodeFindBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.MiningInformationGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
			this.MiningInformationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MiningInformationGrid.GridId = "D13CCC65-4C25-4C88-BB4E-98C158E7CBF7";
			this.MiningInformationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MiningInformationGrid.LayoutKey = "MiningInformationGrid";
			this.MiningInformationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MiningInformationGrid.Name = "MiningInformationGrid";
			this.MiningInformationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 161, true);
			this.MiningInformationGrid.TabIndex = 0;
			// 
			// SanctionsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DisclaimSanctionsCheckBox);
			this.Controls.Add(this.CopyNMFSDataToSanctionsButton);
			this.Controls.Add(this.FishingInformationGroupBox);
			this.Controls.Add(this.MiningInformationGroupBox);
			this.Name = "SanctionsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 267, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FishingInformationGroupBox.ResumeLayout(false);
			this.FishingInformationGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FishingInformationGrid)).EndInit();
			this.FishingInformationGrid.ResumeLayout(false);
			this.FishingInformationGrid.PerformLayout();
			this.MiningInformationGroupBox.ResumeLayout(false);
			this.MiningInformationGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MiningInformationGrid)).EndInit();
			this.MiningInformationGrid.ResumeLayout(false);
			this.MiningInformationGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZButton CopyNMFSDataToSanctionsButton;
		ZArchitecture.GUI.ZCheckBox DisclaimSanctionsCheckBox;
		ZArchitecture.GUI.ZGroupBox FishingInformationGroupBox;
		ZArchitecture.GUI.ZGroupBox MiningInformationGroupBox;
		ZArchitecture.ZGrid FishingInformationGrid;
		ZArchitecture.ZGrid MiningInformationGrid;
	}
}
