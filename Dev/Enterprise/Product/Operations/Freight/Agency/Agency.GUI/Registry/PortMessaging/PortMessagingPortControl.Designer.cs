namespace Enterprise.Freight.Agency.GUI
{
	partial class PortMessagingPortControl
	{
		private void InitializeComponent()
		{
      Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
      Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
      this.portGrid = new Enterprise.ZArchitecture.ZGrid();
      portMessagingPortsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.portGrid)).BeginInit();
      this.portGrid.SuspendLayout();
      portMessagingPortsGroupBox.SuspendLayout();
      this.portMessagingPortsGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // BindingSource
      // 
      this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.PortMessagingPortCollection);
      // 
      // portGrid
      // 
      this.portGrid.AllowNavigation = false;
      this.BindingSource.SetBindingMember(this.portGrid, ".");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.PortMessagingPort)(null)))));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortMessagingPort)(null)).Port)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.PortMessagingPort)(null)).PrincipalPK)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortMessagingPort)(null)).SenderID)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.PortMessagingPort)(null)).Enabled)));
      this.portGrid.CaptionVisible = false;
      zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ed9df1ea-f7b9-4f5f-8fc9-d95b78c8d126", "Port");
      zDropEditColumnStyleInfo1.ColumnName = "Port";
      zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
      zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("8952ac73-c05a-49c3-a7d0-0da16076d8ac", "Principal");
      zGuidFindBoxColumnStyleInfo1.ColumnName = "PrincipalPK";
      zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
      zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("aa76e318-1a07-44d3-8d76-cd874b01cb8e", "Sender Id");
      zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
      zTextBoxColumnStyleInfo1.ColumnName = "SenderID";
      zTextBoxColumnStyleInfo1.IsMandatory = true;
      zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
      zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("dae32cf3-1bbd-429e-ad07-09fe090d7012", "Enabled");
      zCheckBoxColumnStyleInfo1.ColumnName = "Enabled";
      zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
      this.portGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
      this.portGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
      this.portGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
      this.portGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
      this.portGrid.Dock = System.Windows.Forms.DockStyle.Fill;
      this.portGrid.GridId = "e4e768e9-ead4-4977-a99f-00ef95d2531c";
      this.portGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
      this.portGrid.LayoutKey = "portGrid";
      this.portGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
      this.portGrid.Name = "portGrid";
      this.portGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 399, true);
      this.portGrid.TabIndex = 2;
      // 
      // portMessagingPortsGroupBox
      // 
      portMessagingPortsGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("PortMessagingPortControl|9dd369b6-1d84-4820-8e65-778901017420", "Port Authorities");
      portMessagingPortsGroupBox.Controls.Add(this.portGrid);
      portMessagingPortsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
      portMessagingPortsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
      portMessagingPortsGroupBox.Name = "portMessagingPortsGroupBox";
      portMessagingPortsGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
      portMessagingPortsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 424, true);
      portMessagingPortsGroupBox.TabIndex = 4;
      portMessagingPortsGroupBox.TabStop = false;
      // 
      // PortMessagingPortControl
      // 
      this.CaptionRenderingEnabled = true;
      this.Controls.Add(portMessagingPortsGroupBox);
      this.Name = "PortMessagingPortControl";
      this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
      this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 430, true);
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.portGrid)).EndInit();
      this.portGrid.ResumeLayout(false);
      this.portGrid.PerformLayout();
      portMessagingPortsGroupBox.ResumeLayout(false);
      this.portMessagingPortsGroupBox.ResumeLayout(false);
      this.portMessagingPortsGroupBox.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		private ZArchitecture.ZGrid portGrid;
		Enterprise.ZArchitecture.GUI.ZGroupBox portMessagingPortsGroupBox;
	}
}
