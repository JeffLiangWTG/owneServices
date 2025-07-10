namespace Enterprise.Freight.Agency.GUI
{
	partial class PortAuthorityPortControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			portAuthoritiesGrid = new Enterprise.ZArchitecture.ZGrid();
			portAuthoritiesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			productionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			productionEmailBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			productionIdBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			testGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			testingEmailBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			testingIdBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			versionBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			portBoundFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(portAuthoritiesGrid)).BeginInit();
			portAuthoritiesGroupBox.SuspendLayout();
			bottomPanel.SuspendLayout();
			productionGroupBox.SuspendLayout();
			testGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.PortAuthorityPortCollection);
			// 
			// portAuthoritiesGrid
			// 
			portAuthoritiesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(portAuthoritiesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.PortAuthorityPort)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortAuthorityPort)(null)).Port)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortAuthorityPort)(null)).Version)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortAuthorityPort)(null)).ProductionEmail)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortAuthorityPort)(null)).ProductionID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortAuthorityPort)(null)).TestingEmail)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortAuthorityPort)(null)).TestingID)));
			portAuthoritiesGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Port";
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "Version";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zTextBoxColumnStyleInfo1.ColumnName = "ProductionEmail";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "ProductionID";
			zTextBoxColumnStyleInfo3.ColumnName = "TestingEmail";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "TestingID";
			portAuthoritiesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			portAuthoritiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			portAuthoritiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			portAuthoritiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			portAuthoritiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			portAuthoritiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			portAuthoritiesGrid.GridId = "6ba51904-8fb2-49e2-869d-7c7e2deb21e9";
			portAuthoritiesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			portAuthoritiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			portAuthoritiesGrid.LayoutKey = "PortAuthorities";
			portAuthoritiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			portAuthoritiesGrid.Name = "portAuthoritiesGrid";
			portAuthoritiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 204, true);
			portAuthoritiesGrid.TabIndex = 0;
			// 
			// portAuthoritiesGroupBox
			// 
			portAuthoritiesGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("PortAuthorityPortControl|157c70ca-e21a-44ad-a667-0e662b0bb408", "Port Authorities");
			portAuthoritiesGroupBox.Controls.Add(portAuthoritiesGrid);
			portAuthoritiesGroupBox.Controls.Add(bottomPanel);
			portAuthoritiesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			portAuthoritiesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			portAuthoritiesGroupBox.Name = "portAuthoritiesGroupBox";
			portAuthoritiesGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
			portAuthoritiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 424, true);
			portAuthoritiesGroupBox.TabIndex = 0;
			portAuthoritiesGroupBox.TabStop = false;
			// 
			// bottomPanel
			// 
			bottomPanel.Controls.Add(productionGroupBox);
			bottomPanel.Controls.Add(testGroupBox);
			bottomPanel.Controls.Add(versionBoundDropEdit);
			bottomPanel.Controls.Add(portBoundFindBox);
			bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 223, true);
			bottomPanel.Name = "bottomPanel";
			bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 195, true);
			bottomPanel.TabIndex = 1;
			// 
			// productionGroupBox
			// 
			productionGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("PortAuthorityPortControl|914635e3-c72f-4268-84f6-eb06bc35706e", "Production Details");
			productionGroupBox.Controls.Add(productionEmailBoundTextBox);
			productionGroupBox.Controls.Add(productionIdBoundTextBox);
			productionGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			productionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 61, true);
			productionGroupBox.Name = "productionGroupBox";
			productionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 67, true);
			productionGroupBox.TabIndex = 4;
			productionGroupBox.TabStop = false;
			// 
			// productionEmailBoundTextBox
			// 
			productionEmailBoundTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(productionEmailBoundTextBox, "ProductionEmail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortAuthorityPort)(null)).ProductionEmail)));
			productionEmailBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			productionEmailBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 40, true);
			productionEmailBoundTextBox.Name = "productionEmailBoundTextBox";
			productionEmailBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			productionEmailBoundTextBox.TabIndex = 3;
			// 
			// productionIdBoundTextBox
			// 
			productionIdBoundTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(productionIdBoundTextBox, "ProductionID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortAuthorityPort)(null)).ProductionID)));
			productionIdBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 16, true);
			productionIdBoundTextBox.Name = "productionIdBoundTextBox";
			productionIdBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			productionIdBoundTextBox.TabIndex = 1;
			// 
			// testGroupBox
			// 
			testGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("PortAuthorityPortControl|1d04a37a-2614-43f0-bc04-1f42e5b1841a", "Testing Details");
			testGroupBox.Controls.Add(testingEmailBoundTextBox);
			testGroupBox.Controls.Add(testingIdBoundTextBox);
			testGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			testGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 128, true);
			testGroupBox.Name = "testGroupBox";
			testGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 67, true);
			testGroupBox.TabIndex = 5;
			testGroupBox.TabStop = false;
			// 
			// testingEmailBoundTextBox
			// 
			testingEmailBoundTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(testingEmailBoundTextBox, "TestingEmail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortAuthorityPort)(null)).TestingEmail)));
			testingEmailBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			testingEmailBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 40, true);
			testingEmailBoundTextBox.Name = "testingEmailBoundTextBox";
			testingEmailBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			testingEmailBoundTextBox.TabIndex = 3;
			// 
			// testingIdBoundTextBox
			// 
			testingIdBoundTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(testingIdBoundTextBox, "TestingID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortAuthorityPort)(null)).TestingID)));
			testingIdBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 16, true);
			testingIdBoundTextBox.Name = "testingIdBoundTextBox";
			testingIdBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			testingIdBoundTextBox.TabIndex = 1;
			// 
			// versionBoundDropEdit
			// 
			versionBoundDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(versionBoundDropEdit, "Version");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.PortAuthorityPort)(null)).Version)));
			versionBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 32, true);
			versionBoundDropEdit.Name = "versionBoundDropEdit";
			versionBoundDropEdit.PreBoundMaxLength = 3;
			versionBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			versionBoundDropEdit.TabIndex = 3;
			// 
			// portBoundFindBox
			// 
			portBoundFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(portBoundFindBox, "Port");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.PortAuthorityPort)(null)).Port)));
			portBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 5, true);
			portBoundFindBox.Name = "portBoundFindBox";
			portBoundFindBox.PreBoundMaxLength = 5;
			portBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			portBoundFindBox.TabIndex = 1;
			// 
			// PortAuthorityPortControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(portAuthoritiesGroupBox);
			this.Name = "PortAuthorityPortControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 430, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(portAuthoritiesGrid)).EndInit();
			portAuthoritiesGroupBox.ResumeLayout(false);
			bottomPanel.ResumeLayout(false);
			productionGroupBox.ResumeLayout(false);
			productionGroupBox.PerformLayout();
			testGroupBox.ResumeLayout(false);
			testGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		Enterprise.ZArchitecture.ZGrid portAuthoritiesGrid;
		Enterprise.ZArchitecture.GUI.ZGroupBox portAuthoritiesGroupBox;
		Enterprise.ZArchitecture.GUI.ZPanel bottomPanel;
		Enterprise.ZArchitecture.GUI.ZGroupBox productionGroupBox;
		Enterprise.ZArchitecture.ZTextBox productionEmailBoundTextBox;
		Enterprise.ZArchitecture.ZTextBox productionIdBoundTextBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox testGroupBox;
		Enterprise.ZArchitecture.ZTextBox testingEmailBoundTextBox;
		Enterprise.ZArchitecture.ZTextBox testingIdBoundTextBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit versionBoundDropEdit;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox portBoundFindBox;
	}
}
