namespace Enterprise.Freight.Agency.GUI
{
	partial class PortAuthoritySettingsControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			portAuthoritiesGrid = new Enterprise.ZArchitecture.ZGrid();
			portAuthoritiesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(portAuthoritiesGrid)).BeginInit();
			portAuthoritiesGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.PortAuthoritySettings);
			// 
			// portAuthoritiesGrid
			// 
			portAuthoritiesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(portAuthoritiesGrid, "Settings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.PortAuthoritySettings)(null)).Settings)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortAuthoritySetting)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.PortAuthoritySettings)(null)).Settings)).SyncRoot)).Port)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortAuthoritySetting)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.PortAuthoritySettings)(null)).Settings)).SyncRoot)).Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.PortAuthoritySetting)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.PortAuthoritySettings)(null)).Settings)).SyncRoot)).PrincipalPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortAuthoritySetting)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.PortAuthoritySettings)(null)).Settings)).SyncRoot)).SenderID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortAuthoritySetting)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.PortAuthoritySettings)(null)).Settings)).SyncRoot)).Version)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortAuthoritySetting)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.PortAuthoritySettings)(null)).Settings)).SyncRoot)).Email)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortAuthoritySetting)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.PortAuthoritySettings)(null)).Settings)).SyncRoot)).RecipientID)));
			portAuthoritiesGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "PrincipalPK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "Port";
			zDropEditColumnStyleInfo2.IsMandatory = true;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "Status";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "SenderID";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.ColumnName = "Version";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo3.ColumnName = "Email";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zTextBoxColumnStyleInfo4.ColumnName = "RecipientID";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			portAuthoritiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			portAuthoritiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			portAuthoritiesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			portAuthoritiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			portAuthoritiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			portAuthoritiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			portAuthoritiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			portAuthoritiesGrid.GridId = "b5ddbdd6-6d5a-4bc3-810b-d3078e20527a";
			portAuthoritiesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			portAuthoritiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			portAuthoritiesGrid.LayoutKey = "PortAuthorities";
			portAuthoritiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			portAuthoritiesGrid.Name = "portAuthoritiesGrid";
			portAuthoritiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 399, true);
			portAuthoritiesGrid.TabIndex = 2;
			// 
			// portAuthoritiesGroupBox
			// 
			portAuthoritiesGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("PortAuthoritySettingsControl|7b592412-4690-455d-8aed-0a918a9f6418", "Port Authorities");
			portAuthoritiesGroupBox.Controls.Add(portAuthoritiesGrid);
			portAuthoritiesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			portAuthoritiesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			portAuthoritiesGroupBox.Name = "portAuthoritiesGroupBox";
			portAuthoritiesGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
			portAuthoritiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 424, true);
			portAuthoritiesGroupBox.TabIndex = 4;
			portAuthoritiesGroupBox.TabStop = false;
			// 
			// PortAuthoritySettingsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(portAuthoritiesGroupBox);
			this.Name = "PortAuthoritySettingsControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 430, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(portAuthoritiesGrid)).EndInit();
			portAuthoritiesGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		Enterprise.ZArchitecture.ZGrid portAuthoritiesGrid;
		Enterprise.ZArchitecture.GUI.ZGroupBox portAuthoritiesGroupBox;
	}
}
