using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class OrderEntryStagingUserControl
	{
		private ZGuidFindBox CrossDockFindBox;
		private ZGroupBox AllocationsGroupBox;
		private ZArchitecture.ZGrid AllocationsGrid;
		private ZGroupBox zGroupBox6;
		private ZCalcDropEdit zCalcDropEdit5;
		private ZCalcDropEdit zCalcDropEdit3;
		private ZGroupBox zGroupBox4;
		private ZCalcDropEdit zCalcDropEdit1;
		private ZCalcDropEdit zCalcDropEdit2;
		private ZCalcDropEdit zCalcDropEdit4;
		private ZArchitecture.ZLabel EntryStagingInstructionLabel;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo1 = new ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo2 = new ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AllocationsGroupBox = new ZGroupBox();
			this.AllocationsGrid = new ZArchitecture.ZGrid();
			this.zGroupBox6 = new ZGroupBox();
			this.EntryStagingInstructionLabel = new ZArchitecture.ZLabel();
			this.CrossDockFindBox = new ZGuidFindBox();
			this.zCalcDropEdit4 = new ZCalcDropEdit();
			this.zCalcDropEdit5 = new ZCalcDropEdit();
			this.zCalcDropEdit3 = new ZCalcDropEdit();
			this.zGroupBox4 = new ZGroupBox();
			this.zCalcDropEdit1 = new ZCalcDropEdit();
			this.zCalcDropEdit2 = new ZCalcDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AllocationsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AllocationsGrid)).BeginInit();
			this.zGroupBox6.SuspendLayout();
			this.zGroupBox4.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.WhsOrder);
			// 
			// AllocationsGroupBox
			// 
			this.AllocationsGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryStagingUserControl|c7b2acc7-6bce-46f1-93fa-59db467f2519", "Allocation");
			this.AllocationsGroupBox.Controls.Add(this.AllocationsGrid);
			this.AllocationsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AllocationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 208, true);
			this.AllocationsGroupBox.Name = "AllocationsGroupBox";
			this.AllocationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(673, 239, true);
			this.AllocationsGroupBox.TabIndex = 2;
			this.AllocationsGroupBox.TabStop = false;
			// 
			// AllocationsGrid
			// 
			this.AllocationsGrid.AllowCopyToNewRowMenuItem = false;
			this.AllocationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AllocationsGrid, "AllocatedReceipts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.WhsOrder)(null)).AllocatedReceipts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.WhsDocket)(((System.Collections.IList)(((Business.WhsOrder)(null)).AllocatedReceipts)).SyncRoot)).WD_DocketID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.WhsDocket)(((System.Collections.IList)(((Business.WhsOrder)(null)).AllocatedReceipts)).SyncRoot)).WD_ExternalReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.WhsDocket)(((System.Collections.IList)(((Business.WhsOrder)(null)).AllocatedReceipts)).SyncRoot)).WD_DocketStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Business.WhsDocket)(((System.Collections.IList)(((Business.WhsOrder)(null)).AllocatedReceipts)).SyncRoot)).WD_ArrivalDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Business.WhsDocket)(((System.Collections.IList)(((Business.WhsOrder)(null)).AllocatedReceipts)).SyncRoot)).WD_ETA)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.WhsDocket)(((System.Collections.IList)(((Business.WhsOrder)(null)).AllocatedReceipts)).SyncRoot)).SubTypeDesc)));
			this.AllocationsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "WD_DocketID";
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryStagingUserControl|2e4082e9-76e2-4ae4-93d5-4221d70b99c8", "Receipt Reference");
			zTextBoxColumnStyleInfo2.ColumnName = "WD_ExternalReference";
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryStagingUserControl|975b68a5-62da-4a03-9bf2-9abdd368d557", "Status");
			zTextBoxColumnStyleInfo3.ColumnName = "WD_DocketStatusDescription";
			zDateTimeOffsetEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateTimeOffsetEditColumnStyleInfo1.ColumnName = "WD_ArrivalDate";
			zDateTimeOffsetEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			zDateTimeOffsetEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateTimeOffsetEditColumnStyleInfo2.ColumnName = "WD_ETA";
			zDateTimeOffsetEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryStagingUserControl|bffb3160-637f-4b33-a27d-045b374e2ef9", "Sub Type");
			zTextBoxColumnStyleInfo4.ColumnName = "SubTypeDesc";
			this.AllocationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AllocationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AllocationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.AllocationsGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo1);
			this.AllocationsGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo2);
			this.AllocationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.AllocationsGrid.GridId = "56ece01d-483e-49e6-9a78-a8ff3c41630c";
			this.AllocationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AllocationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AllocationsGrid.LayoutKey = "AllocationsGrid";
			this.AllocationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AllocationsGrid.Name = "AllocationsGrid";
			this.AllocationsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.AllocationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 220, true);
			this.AllocationsGrid.TabIndex = 0;
			// 
			// zGroupBox6
			// 
			this.zGroupBox6.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryStagingUserControl|1f415ab2-345f-48a6-b7fc-6f9ee4fc1916", "Cross-Dock Area");
			this.zGroupBox6.Controls.Add(this.EntryStagingInstructionLabel);
			this.zGroupBox6.Controls.Add(this.CrossDockFindBox);
			this.zGroupBox6.Controls.Add(this.zCalcDropEdit4);
			this.zGroupBox6.Controls.Add(this.zCalcDropEdit5);
			this.zGroupBox6.Controls.Add(this.zCalcDropEdit3);
			this.zGroupBox6.Dock = System.Windows.Forms.DockStyle.Top;
			this.zGroupBox6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 80, true);
			this.zGroupBox6.Name = "zGroupBox6";
			this.zGroupBox6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(673, 128, true);
			this.zGroupBox6.TabIndex = 1;
			this.zGroupBox6.TabStop = false;
			// 
			// EntryStagingInstructionLabel
			// 
			this.EntryStagingInstructionLabel.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryStagingUserControl|8681b97c-c168-405c-8f6a-cb631e2aa6d7", "", "Sometimes the Cross-Dock Area Volumes (to the left) and Allocations (below) will not be the most current. To see the most up to date values, Save, Close and Reopen this Order.");
			this.EntryStagingInstructionLabel.ForeColor = System.Drawing.Color.SaddleBrown;
			this.EntryStagingInstructionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 44, true);
			this.EntryStagingInstructionLabel.Name = "EntryStagingInstructionLabel";
			this.EntryStagingInstructionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 66, true);
			this.EntryStagingInstructionLabel.TabIndex = 5;
			// 
			// CrossDockFindBox
			// 
			this.BindingSource.SetBindingMember(this.CrossDockFindBox, "WD_WL_CrossDock");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.WhsOrder)(null)).WD_WL_CrossDock)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.WhsOrder)(null)).Lookups.CrossDockLocations)));
			this.CrossDockFindBox.BindToList = "Lookups+CrossDockLocations";
			this.CrossDockFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 21, true);
			this.CrossDockFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.WhsConfigLocation;
			this.CrossDockFindBox.Name = "CrossDockFindBox";
			this.CrossDockFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 20, true);
			this.CrossDockFindBox.TabIndex = 1;
			// 
			// zCalcDropEdit4
			// 
			this.BindingSource.SetBindingMember(this.zCalcDropEdit4, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.WhsOrder)(null)).WD_CalcCrossDockAvailableVolumeIncludingCrossDockAllocations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.WhsOrder)(null)).CrossDockLocation.WLV_MaxCubicUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.WhsOrder)(null)).CrossDockLocation.Lookups.CubicUnits)));
			this.zCalcDropEdit4.BindToAmount = "WD_CalcCrossDockAvailableVolumeIncludingCrossDockAllocations";
			this.zCalcDropEdit4.BindToUnit = "CrossDockLocation+WLV_MaxCubicUnit";
			this.zCalcDropEdit4.BindToList = "CrossDockLocation+Lookups+CubicUnits";
			this.zCalcDropEdit4.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryStagingUserControl|9462b93e-e674-468b-a1c5-0005696699de", "Available Volume");
			this.zCalcDropEdit4.Decimals = 4;
			this.zCalcDropEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 93, true);
			this.zCalcDropEdit4.Name = "zCalcDropEdit4";
			this.zCalcDropEdit4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.zCalcDropEdit4.TabIndex = 4;
			this.zCalcDropEdit4.UnitPreBoundMaxLength = 3;
			// 
			// zCalcDropEdit5
			// 
			this.BindingSource.SetBindingMember(this.zCalcDropEdit5, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.WhsOrder)(null)).CrossDockLocation.WLV_MaxCubic)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.WhsOrder)(null)).CrossDockLocation.WLV_MaxCubicUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.WhsOrder)(null)).CrossDockLocation.Lookups.CubicUnits)));
			this.zCalcDropEdit5.BindToAmount = "CrossDockLocation+WLV_MaxCubic";
			this.zCalcDropEdit5.BindToUnit = "CrossDockLocation+WLV_MaxCubicUnit";
			this.zCalcDropEdit5.BindToList = "CrossDockLocation+Lookups+CubicUnits";
			this.zCalcDropEdit5.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryStagingUserControl|da9478e6-132c-416f-b57d-f35aad92b241", "Max Volume");
			this.zCalcDropEdit5.Decimals = 4;
			this.zCalcDropEdit5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 45, true);
			this.zCalcDropEdit5.Name = "zCalcDropEdit5";
			this.zCalcDropEdit5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.zCalcDropEdit5.TabIndex = 2;
			this.zCalcDropEdit5.UnitPreBoundMaxLength = 3;
			// 
			// zCalcDropEdit3
			// 
			this.BindingSource.SetBindingMember(this.zCalcDropEdit3, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.WhsOrder)(null)).WD_CalcCrossDockCurrentVolumeIncludingCrossDockAllocations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.WhsOrder)(null)).CrossDockLocation.WLV_MaxCubicUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.WhsOrder)(null)).CrossDockLocation.Lookups.CubicUnits)));
			this.zCalcDropEdit3.BindToAmount = "WD_CalcCrossDockCurrentVolumeIncludingCrossDockAllocations";
			this.zCalcDropEdit3.BindToUnit = "CrossDockLocation+WLV_MaxCubicUnit";
			this.zCalcDropEdit3.BindToList = "CrossDockLocation+Lookups+CubicUnits";
			this.zCalcDropEdit3.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryStagingUserControl|d65f6f32-2ba4-4db4-81cf-1f345a6b88d5", "Current Volume");
			this.zCalcDropEdit3.Decimals = 4;
			this.zCalcDropEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 69, true);
			this.zCalcDropEdit3.Name = "zCalcDropEdit3";
			this.zCalcDropEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.zCalcDropEdit3.TabIndex = 3;
			this.zCalcDropEdit3.UnitPreBoundMaxLength = 3;
			// 
			// zGroupBox4
			// 
			this.zGroupBox4.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryStagingUserControl|c62f89d1-a876-49bc-961c-8d5d025e77b6", "Order");
			this.zGroupBox4.Controls.Add(this.zCalcDropEdit1);
			this.zGroupBox4.Controls.Add(this.zCalcDropEdit2);
			this.zGroupBox4.Dock = System.Windows.Forms.DockStyle.Top;
			this.zGroupBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox4.Name = "zGroupBox4";
			this.zGroupBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(673, 80, true);
			this.zGroupBox4.TabIndex = 0;
			this.zGroupBox4.TabStop = false;
			// 
			// zCalcDropEdit1
			// 
			this.BindingSource.SetBindingMember(this.zCalcDropEdit1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.WhsOrder)(null)).WD_TotalCubic)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.WhsOrder)(null)).WD_TotalCubicUnit)));
			this.zCalcDropEdit1.BindToAmount = "WD_TotalCubic";
			this.zCalcDropEdit1.BindToUnit = "WD_TotalCubicUnit";
			this.zCalcDropEdit1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryStagingUserControl|e87e099d-b8f3-4513-94d1-22a89c49c25c", "Total Volume for this Order");
			this.zCalcDropEdit1.Decimals = 4;
			this.zCalcDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 45, true);
			this.zCalcDropEdit1.Name = "zCalcDropEdit1";
			this.zCalcDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.zCalcDropEdit1.TabIndex = 1;
			this.zCalcDropEdit1.UnitPreBoundMaxLength = 3;
			// 
			// zCalcDropEdit2
			// 
			this.BindingSource.SetBindingMember(this.zCalcDropEdit2, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.WhsOrder)(null)).WD_TotalWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.WhsOrder)(null)).WD_TotalWeightUnit)));
			this.zCalcDropEdit2.BindToAmount = "WD_TotalWeight";
			this.zCalcDropEdit2.BindToUnit = "WD_TotalWeightUnit";
			this.zCalcDropEdit2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryStagingUserControl|02155fd7-568a-45fe-ba61-58b7736fadbd", "Total Weight for this Order");
			this.zCalcDropEdit2.Decimals = 2;
			this.zCalcDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 21, true);
			this.zCalcDropEdit2.Name = "zCalcDropEdit2";
			this.zCalcDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.zCalcDropEdit2.TabIndex = 0;
			this.zCalcDropEdit2.UnitPreBoundMaxLength = 3;
			// 
			// OrderEntryStagingUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AllocationsGroupBox);
			this.Controls.Add(this.zGroupBox6);
			this.Controls.Add(this.zGroupBox4);
			this.Name = "OrderEntryStagingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(673, 447, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AllocationsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AllocationsGrid)).EndInit();
			this.zGroupBox6.ResumeLayout(false);
			this.zGroupBox4.ResumeLayout(false);
			this.ResumeLayout(false);
		}
	}
}
