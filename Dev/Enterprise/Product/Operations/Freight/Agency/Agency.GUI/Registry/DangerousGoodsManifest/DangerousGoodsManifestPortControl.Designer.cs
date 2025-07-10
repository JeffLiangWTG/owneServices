namespace Enterprise.Freight.Agency.GUI
{
	partial class DangerousGoodsManifestPortControl
	{
		private void InitializeComponent()
		{
      Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
      Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
      this.dangerousGoodsManifestGrid = new Enterprise.ZArchitecture.ZGrid();
      this.dangerousGoodsManifestGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.dangerousGoodsManifestGrid)).BeginInit();
      this.dangerousGoodsManifestGrid.SuspendLayout();
      this.dangerousGoodsManifestGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // BindingSource
      // 
      this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.DangerousGoodsManifestPortCollection);
      // 
      // dangerousGoodsManifestGrid
      // 
      this.dangerousGoodsManifestGrid.AllowNavigation = false;
      this.BindingSource.SetBindingMember(this.dangerousGoodsManifestGrid, ".");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.DangerousGoodsManifestPort)(null)))));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.DangerousGoodsManifestPort)(null)).Port)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.DangerousGoodsManifestPort)(null)).PrincipalPK)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.DangerousGoodsManifestPort)(null)).SenderID)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.DangerousGoodsManifestPort)(null)).Enabled)));
      this.dangerousGoodsManifestGrid.CaptionVisible = false;
      zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("3e6165c8-b287-4e77-908b-fb328d915123", "Port");
      zDropEditColumnStyleInfo1.ColumnName = "Port";
      zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
      zGuidFindBoxColumnStyleInfo1.ColumnName = "PrincipalPK";
      zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
      zTextBoxColumnStyleInfo1.ColumnName = "SenderID";
      zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
      zCheckBoxColumnStyleInfo1.ColumnName = "Enabled";
      zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
      this.dangerousGoodsManifestGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
      this.dangerousGoodsManifestGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
      this.dangerousGoodsManifestGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
      this.dangerousGoodsManifestGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
      this.dangerousGoodsManifestGrid.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dangerousGoodsManifestGrid.GridId = "6ba51904-8fb2-49e2-869d-7c7e2deb21e9";
      this.dangerousGoodsManifestGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
      this.dangerousGoodsManifestGrid.LayoutKey = "DangerousGoodsManifest";
      this.dangerousGoodsManifestGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
      this.dangerousGoodsManifestGrid.Name = "dangerousGoodsManifestGrid";
      this.dangerousGoodsManifestGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 399, true);
      this.dangerousGoodsManifestGrid.TabIndex = 0;
      // 
      // dangerousGoodsManifestGroupBox
      // 
      this.dangerousGoodsManifestGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("7af3109c-1bc7-4333-aa86-8d4e19643faa", "Dangerous Goods Manifest");
      this.dangerousGoodsManifestGroupBox.Controls.Add(this.dangerousGoodsManifestGrid);
      this.dangerousGoodsManifestGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dangerousGoodsManifestGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
      this.dangerousGoodsManifestGroupBox.Name = "dangerousGoodsManifestGroupBox";
      this.dangerousGoodsManifestGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
      this.dangerousGoodsManifestGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 424, true);
      this.dangerousGoodsManifestGroupBox.TabIndex = 0;
      this.dangerousGoodsManifestGroupBox.TabStop = false;
      // 
      // DangerousGoodsManifestPortControl
      // 
      this.CaptionRenderingEnabled = true;
      this.Controls.Add(this.dangerousGoodsManifestGroupBox);
      this.Name = "DangerousGoodsManifestPortControl";
      this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
      this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 430, true);
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.dangerousGoodsManifestGrid)).EndInit();
      this.dangerousGoodsManifestGrid.ResumeLayout(false);
      this.dangerousGoodsManifestGrid.PerformLayout();
      this.dangerousGoodsManifestGroupBox.ResumeLayout(false);
      this.dangerousGoodsManifestGroupBox.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		Enterprise.ZArchitecture.ZGrid dangerousGoodsManifestGrid;
		Enterprise.ZArchitecture.GUI.ZGroupBox dangerousGoodsManifestGroupBox;
	}
}
