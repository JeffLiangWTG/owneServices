namespace Enterprise.Freight.Agency.GUI
{
	partial class CreateBulkContainerDetentionControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.topPanel = new CargoWise.Windows.UI.KPanel();
			this.countryCode = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.findButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.childGrid = new Enterprise.ZArchitecture.ZGrid();
			clientFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			principalFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			directionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.topPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.childGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.BulkDetentionHeader);
			// 
			// topPanel
			// 
			this.topPanel.Controls.Add(this.countryCode);
			this.topPanel.Controls.Add(this.findButton);
			this.topPanel.Controls.Add(clientFindBox);
			this.topPanel.Controls.Add(principalFindBox);
			this.topPanel.Controls.Add(directionDropEdit);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.topPanel.Name = "topPanel";
			this.topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 104, true);
			this.topPanel.TabIndex = 0;
			// 
			// countryCode
			// 
			this.countryCode.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.countryCode, "CountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.BulkDetentionHeader)(null)).CountryCode)));
			this.countryCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 80, true);
			this.countryCode.Name = "countryCode";
			this.countryCode.PreBoundMaxLength = 2;
			this.countryCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 20, true);
			this.countryCode.TabIndex = 4;
			// 
			// findButton
			// 
			this.findButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CreateBulkContainerDetentionControl|f989e3ab-95c3-447a-9e92-e91fbf3c1494", "Find");
			this.findButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(422, 4, true);
			this.findButton.Name = "findButton";
			this.findButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.findButton.TabIndex = 3;
			this.findButton.UseVisualStyleBackColor = true;
			// 
			// clientFindBox
			// 
			clientFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(clientFindBox, "ClientPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.BulkDetentionHeader)(null)).ClientPK)));
			clientFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 56, true);
			clientFindBox.Name = "clientFindBox";
			clientFindBox.PreBoundMaxLength = 12;
			clientFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 20, true);
			clientFindBox.TabIndex = 2;
			// 
			// principalFindBox
			// 
			principalFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(principalFindBox, "PrincipalPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.BulkDetentionHeader)(null)).PrincipalPK)));
			principalFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 32, true);
			principalFindBox.Name = "principalFindBox";
			principalFindBox.PreBoundMaxLength = 12;
			principalFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 20, true);
			principalFindBox.TabIndex = 1;
			// 
			// directionDropEdit
			// 
			directionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(directionDropEdit, "DetentionType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.BulkDetentionHeader)(null)).DetentionType)));
			directionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 8, true);
			directionDropEdit.Name = "directionDropEdit";
			directionDropEdit.PreBoundMaxLength = 3;
			directionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 20, true);
			directionDropEdit.TabIndex = 0;
			// 
			// childGrid
			// 
			this.childGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.childGrid, "Children");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkDetentionHeader)(null)).Children)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.BulkDetentionChild)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkDetentionHeader)(null)).Children)).SyncRoot)).IsSelected)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.BulkDetentionChild)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkDetentionHeader)(null)).Children)).SyncRoot)).ContainerCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.BulkDetentionChild)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkDetentionHeader)(null)).Children)).SyncRoot)).ClientPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.BulkDetentionChild)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkDetentionHeader)(null)).Children)).SyncRoot)).PrincipalPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.BulkDetentionChild)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkDetentionHeader)(null)).Children)).SyncRoot)).DetentionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.BulkDetentionChild)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkDetentionHeader)(null)).Children)).SyncRoot)).CountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.BulkDetentionChild)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkDetentionHeader)(null)).Children)).SyncRoot)).JobNumber)));
			this.childGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "IsSelected";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "ContainerCount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ClientPK";
			zGuidFindBoxColumnStyleInfo2.ColumnName = "PrincipalPK";
			zTextBoxColumnStyleInfo1.ColumnName = "DetentionType";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CountryCode";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.ColumnName = "JobNumber";
			this.childGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.childGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.childGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.childGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.childGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.childGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.childGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.childGrid.GridId = "e33f534a-2452-4120-a251-d2097b66f5af";
			this.childGrid.CopySelectedRowsAllowed = true;
			this.childGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.childGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.childGrid.LayoutKey = "childGrid";
			this.childGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 104, true);
			this.childGrid.Name = "childGrid";
			this.childGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 324, true);
			this.childGrid.TabIndex = 1;
			this.childGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.childGrid_MouseDown);
			// 
			// CreateBulkContainerDetentionControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.childGrid);
			this.Controls.Add(this.topPanel);
			this.Name = "CreateBulkContainerDetentionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 428, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.topPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.childGrid)).EndInit();
			this.ResumeLayout(false);

		}

		Enterprise.ZArchitecture.GUI.ZButton findButton;
		Enterprise.ZArchitecture.ZGrid childGrid;
		ZArchitecture.GUI.ZCodeFindBox countryCode;
		CargoWise.Windows.UI.KPanel topPanel;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox clientFindBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox principalFindBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit directionDropEdit;
	}
}
