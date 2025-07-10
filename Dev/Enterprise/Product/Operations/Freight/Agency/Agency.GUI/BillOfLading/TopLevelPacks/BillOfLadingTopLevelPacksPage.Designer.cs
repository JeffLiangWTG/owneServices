namespace Enterprise.Freight.Agency.GUI
{
	partial class BillOfLadingTopLevelPacksPage
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
			this.topLevelPacksDetailsControl = new Enterprise.Freight.Agency.GUI.TopLevelPacksDetailsControl();
			this.topLevelPacksControl1 = new Enterprise.Freight.Agency.GUI.TopLevelPacksControl();
			this.OuterSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.AdditionalDetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ExportProcessTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.topLevelPacksExportProcessControl = new Enterprise.Freight.Agency.GUI.TopLevelPacksExportProcessControl();
			this.ImportProcessTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.topLevelPacksImportProcessControl = new Enterprise.Freight.Agency.GUI.TopLevelPacksImportProcessControl();
			this.topLevelPacksTotalsControl = new Enterprise.Freight.Agency.GUI.TopLevelPacksTotalsControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OuterSplitContainer)).BeginInit();
			this.OuterSplitContainer.Panel1.SuspendLayout();
			this.OuterSplitContainer.Panel2.SuspendLayout();
			this.OuterSplitContainer.SuspendLayout();
			this.AdditionalDetailsTabControl.SuspendLayout();
			this.ExportProcessTabPage.SuspendLayout();
			this.ImportProcessTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.AgencyShipment);
			// 
			// topLevelPacksDetailsControl
			// 
			this.topLevelPacksDetailsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.topLevelPacksDetailsControl, "TopLevelPacks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Agency.Business.AgencyShipmentContainersView)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).TopLevelPacks)));
			this.topLevelPacksDetailsControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.topLevelPacksDetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.topLevelPacksDetailsControl.Name = "topLevelPacksDetailsControl";
			this.topLevelPacksDetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 244, true);
			this.topLevelPacksDetailsControl.TabIndex = 0;
			// 
			// topLevelPacksControl1
			// 
			this.topLevelPacksControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.topLevelPacksControl1, ".");
			this.topLevelPacksControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.topLevelPacksControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.topLevelPacksControl1.Name = "topLevelPacksControl1";
			this.topLevelPacksControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 130, true);
			this.topLevelPacksControl1.TabIndex = 1;
			// 
			// OuterSplitContainer
			// 
			this.OuterSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OuterSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OuterSplitContainer.Name = "OuterSplitContainer";
			this.OuterSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// OuterSplitContainer.Panel1
			// 
			this.OuterSplitContainer.Panel1.Controls.Add(this.topLevelPacksControl1);
			// 
			// OuterSplitContainer.Panel2
			// 
			this.OuterSplitContainer.Panel2.Controls.Add(this.AdditionalDetailsTabControl);
			this.OuterSplitContainer.Panel2.Controls.Add(this.topLevelPacksDetailsControl);
			this.OuterSplitContainer.Panel2.Controls.Add(this.topLevelPacksTotalsControl);
			this.OuterSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 434, true);
			this.OuterSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			this.OuterSplitContainer.TabIndex = 2;
			// 
			// AdditionalDetailsTabControl
			// 
			this.AdditionalDetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.AdditionalDetailsTabControl.Controls.Add(this.ExportProcessTabPage);
			this.AdditionalDetailsTabControl.Controls.Add(this.ImportProcessTabPage);
			this.AdditionalDetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(528, 0, true);
			this.AdditionalDetailsTabControl.Name = "AdditionalDetailsTabControl";
			this.AdditionalDetailsTabControl.SelectedIndex = 0;
			this.AdditionalDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 244, true);
			this.AdditionalDetailsTabControl.TabIndex = 1;
			// 
			// ExportProcessTabPage
			// 
			this.ExportProcessTabPage.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ee90e36b-214c-446a-b278-ded9b31c7b97", "Export Process");
			this.ExportProcessTabPage.Controls.Add(this.topLevelPacksExportProcessControl);
			this.ExportProcessTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ExportProcessTabPage.Name = "ExportProcessTabPage";
			this.ExportProcessTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ExportProcessTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 217, true);
			this.ExportProcessTabPage.TabIndex = 0;
			// 
			// topLevelPacksExportProcessControl
			// 
			this.topLevelPacksExportProcessControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.topLevelPacksExportProcessControl, "TopLevelPacks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Agency.Business.AgencyShipmentContainersView)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).TopLevelPacks)));
			this.topLevelPacksExportProcessControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.topLevelPacksExportProcessControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.topLevelPacksExportProcessControl.Name = "topLevelPacksExportProcessControl";
			this.topLevelPacksExportProcessControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 211, true);
			this.topLevelPacksExportProcessControl.TabIndex = 0;
			// 
			// ImportProcessTabPage
			// 
			this.ImportProcessTabPage.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("5ccf8d85-6efb-4547-9cc5-0dbfcbf44f62", "Import Process");
			this.ImportProcessTabPage.Controls.Add(this.topLevelPacksImportProcessControl);
			this.ImportProcessTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ImportProcessTabPage.Name = "ImportProcessTabPage";
			this.ImportProcessTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ImportProcessTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 217, true);
			this.ImportProcessTabPage.TabIndex = 1;
			// 
			// topLevelPacksImportProcessControl
			// 
			this.topLevelPacksImportProcessControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.topLevelPacksImportProcessControl, "TopLevelPacks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Agency.Business.AgencyShipmentContainersView)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).TopLevelPacks)));
			this.topLevelPacksImportProcessControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.topLevelPacksImportProcessControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.topLevelPacksImportProcessControl.Name = "topLevelPacksImportProcessControl";
			this.topLevelPacksImportProcessControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 211, true);
			this.topLevelPacksImportProcessControl.TabIndex = 0;
			// 
			// topLevelPacksTotalsControl
			// 
			this.topLevelPacksTotalsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.topLevelPacksTotalsControl, ".");
			this.topLevelPacksTotalsControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.topLevelPacksTotalsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 244, true);
			this.topLevelPacksTotalsControl.Name = "topLevelPacksTotalsControl";
			this.topLevelPacksTotalsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 56, true);
			this.topLevelPacksTotalsControl.TabIndex = 2;
			// 
			// BillOfLadingTopLevelPacksPage
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.OuterSplitContainer);
			this.Name = "BillOfLadingTopLevelPacksPage";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 434, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OuterSplitContainer.Panel1.ResumeLayout(false);
			this.OuterSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.OuterSplitContainer)).EndInit();
			this.OuterSplitContainer.ResumeLayout(false);
			this.AdditionalDetailsTabControl.ResumeLayout(false);
			this.ExportProcessTabPage.ResumeLayout(false);
			this.ImportProcessTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private TopLevelPacksDetailsControl topLevelPacksDetailsControl;
		private TopLevelPacksControl topLevelPacksControl1;
		private CargoWise.Windows.UI.KSplitContainer OuterSplitContainer;
		private ZArchitecture.GUI.ZTabControl AdditionalDetailsTabControl;
		private ZArchitecture.GUI.ZTabPage ExportProcessTabPage;
		private ZArchitecture.GUI.ZTabPage ImportProcessTabPage;
		private TopLevelPacksExportProcessControl topLevelPacksExportProcessControl;
		private TopLevelPacksImportProcessControl topLevelPacksImportProcessControl;
		private TopLevelPacksTotalsControl topLevelPacksTotalsControl;
	}
}
