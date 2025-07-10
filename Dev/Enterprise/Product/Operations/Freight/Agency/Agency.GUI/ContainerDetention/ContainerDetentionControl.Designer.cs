namespace Enterprise.Freight.Agency.GUI
{
	partial class ContainerDetentionControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.findButton = new Enterprise.ZArchitecture.GUI.ZButton();
			topPanel = new CargoWise.Windows.UI.KPanel();
			zGuidFindBox1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			nc_DetentionTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			localClientFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			containersGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			topPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(containersGrid.InnerGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.ContainerDetention);
			// 
			// topPanel
			// 
			topPanel.Controls.Add(this.findButton);
			topPanel.Controls.Add(zGuidFindBox1);
			topPanel.Controls.Add(nc_DetentionTypeDropEdit);
			topPanel.Controls.Add(localClientFindBox);
			topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			topPanel.Name = "topPanel";
			topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(530, 88, true);
			topPanel.TabIndex = 0;
			// 
			// findButton
			// 
			this.findButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ContainerDetentionControl|28f8f5f3-2b2f-456e-95d8-68cd8368a6d0", "Find");
			this.findButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 8, true);
			this.findButton.Name = "findButton";
			this.findButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.findButton.TabIndex = 3;
			this.findButton.UseVisualStyleBackColor = true;
			// 
			// zGuidFindBox1
			// 
			this.BindingSource.SetBindingMember(zGuidFindBox1, "NC_OH_Principal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.ContainerDetention)(null)).NC_OH_Principal)));
			zGuidFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 32, true);
			zGuidFindBox1.Name = "zGuidFindBox1";
			zGuidFindBox1.PreBoundMaxLength = 12;
			zGuidFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			zGuidFindBox1.TabIndex = 1;
			// 
			// nc_DetentionTypeDropEdit
			// 
			this.BindingSource.SetBindingMember(nc_DetentionTypeDropEdit, "NC_DetentionType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.ContainerDetention)(null)).NC_DetentionType)));
			nc_DetentionTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 8, true);
			nc_DetentionTypeDropEdit.Name = "nc_DetentionTypeDropEdit";
			nc_DetentionTypeDropEdit.PreBoundMaxLength = 3;
			nc_DetentionTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			nc_DetentionTypeDropEdit.TabIndex = 0;
			// 
			// localClientFindBox
			// 
			this.BindingSource.SetBindingMember(localClientFindBox, "NC_OH_Client");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.ContainerDetention)(null)).NC_OH_Client)));
			localClientFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 56, true);
			localClientFindBox.Name = "localClientFindBox";
			localClientFindBox.PreBoundMaxLength = 12;
			localClientFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			localClientFindBox.TabIndex = 2;
			// 
			// containersGrid
			// 
			this.BindingSource.SetBindingMember(containersGrid, "Movements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.ContainerDetention)(null)).Movements)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.ContainerDetention)(null)).Lookups.Movements)));
			containersGrid.BindToFindBoxList = "Lookups.Movements";
			zDateEditColumnStyleInfo1.ColumnName = "E9_MovementDate";
			zTextBoxColumnStyleInfo1.ColumnName = "Stock+R6_ContainerNum";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "Stock+R6_RC";
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.ColumnName = "Voyage+JV_RV_NKVessel";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.ColumnName = "Voyage+JV_VoyageFlight";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo4.ColumnName = "RelatedInfo+BillsOfLading";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "E9_DetentionDays";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo5.ColumnName = "E9_MovementType";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zDateEditColumnStyleInfo2.ColumnName = "RelatedInfo+AvailabilityDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.IsVisible = false;
			zDateEditColumnStyleInfo3.ColumnName = "RelatedInfo+ReturnByDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "RelatedInfo+ImportDetentionFreeDays";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "RelatedInfo+ExportDetentionFreeDays";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo6.ColumnName = "RelatedInfo+Consignor";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo7.ColumnName = "RelatedInfo+Consignee";
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo8.ColumnName = "RelatedInfo+LocalClient";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo9.ColumnName = "RelatedInfo+Principal";
			zTextBoxColumnStyleInfo9.IsVisible = false;
			containersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			containersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			containersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			containersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			containersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			containersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			containersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			containersGrid.GridId = "f33fdbfe-6326-41b7-ba40-74d6311a3ab4";
			containersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// 
			// 
			containersGrid.InnerGrid.AllowNavigation = false;
			containersGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			containersGrid.InnerGrid.CaptionVisible = false;
			containersGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			containersGrid.InnerGrid.LayoutKey = "containersGrid";
			containersGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			containersGrid.InnerGrid.Name = "Grid";
			containersGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			containersGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 150, true);
			containersGrid.InnerGrid.TabIndex = 0;
			containersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 88, true);
			containersGrid.Name = "containersGrid";
			containersGrid.ReadOnly = false;
			containersGrid.ShowEditButton = false;
			containersGrid.ShowNewButton = false;
			containersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(530, 188, true);
			containersGrid.TabIndex = 1;
			containersGrid.Attaching += new Enterprise.ZArchitecture.GUI.ModuleButtonGridOperationCancelEventHandler(this.containersGrid_Attaching);
			containersGrid.Detaching += new Enterprise.ZArchitecture.GUI.ModuleButtonGridOperationCancelEventHandler(this.containersGrid_Detaching);
			// 
			// ContainerDetentionControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(containersGrid);
			this.Controls.Add(topPanel);
			this.Name = "ContainerDetentionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(530, 276, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			topPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(containersGrid.InnerGrid)).EndInit();
			this.ResumeLayout(false);

		}

		Enterprise.ZArchitecture.GUI.ZButton findButton;
		CargoWise.Windows.UI.KPanel topPanel;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox zGuidFindBox1;
		Enterprise.ZArchitecture.GUI.ZDropEdit nc_DetentionTypeDropEdit;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox localClientFindBox;
		Enterprise.ZArchitecture.GUI.ZModuleButtonGrid containersGrid;
	}
}
