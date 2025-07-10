namespace Enterprise.Freight.Agency.GUI
{
	partial class PortManifestPortControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.portManifestGrid = new Enterprise.ZArchitecture.ZGrid();
			this.portManifestGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.portManifestGrid)).BeginInit();
			this.portManifestGrid.SuspendLayout();
			this.portManifestGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.PortManifestPortCollection);
			// 
			// portManifestGrid
			// 
			this.portManifestGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.portManifestGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.PortManifestPort)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortManifestPort)(null)).Port)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.PortManifestPort)(null)).PrincipalPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortManifestPort)(null)).SenderID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.PortManifestPort)(null)).Enabled)));
			this.portManifestGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("a96cf462-b57a-4e52-9547-b76f1850c5cd", "Port");
			zDropEditColumnStyleInfo1.ColumnName = "Port";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "PrincipalPK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "SenderID";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "Enabled";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.portManifestGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.portManifestGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.portManifestGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.portManifestGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.portManifestGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.portManifestGrid.GridId = "6ba51904-8fb2-49e2-869d-7c7e2deb21e9";
			this.portManifestGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.portManifestGrid.LayoutKey = "portManifest";
			this.portManifestGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.portManifestGrid.Name = "portManifestGrid";
			this.portManifestGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 399, true);
			this.portManifestGrid.TabIndex = 0;
			// 
			// portManifestGroupBox
			// 
			this.portManifestGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("73c40c4c-98af-4ab2-8fb1-02ba3b924ca7", "Load & Discharge Manifest");
			this.portManifestGroupBox.Controls.Add(this.portManifestGrid);
			this.portManifestGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.portManifestGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.portManifestGroupBox.Name = "portManifestGroupBox";
			this.portManifestGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
			this.portManifestGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 424, true);
			this.portManifestGroupBox.TabIndex = 0;
			this.portManifestGroupBox.TabStop = false;
			// 
			// portManifestPortControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.portManifestGroupBox);
			this.Name = "portManifestPortControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 430, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.portManifestGrid)).EndInit();
			this.portManifestGrid.ResumeLayout(false);
			this.portManifestGrid.PerformLayout();
			this.portManifestGroupBox.ResumeLayout(false);
			this.portManifestGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		Enterprise.ZArchitecture.ZGrid portManifestGrid;
		Enterprise.ZArchitecture.GUI.ZGroupBox portManifestGroupBox;
	}
}
