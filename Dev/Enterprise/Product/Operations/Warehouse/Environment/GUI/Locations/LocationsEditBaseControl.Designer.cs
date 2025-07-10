namespace Enterprise.Warehouse.Environment.GUI
{
	partial class LocationsEditBaseControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Dispose

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnhookEvents();

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void UnhookEvents()
		{
			var row = Row;
			if (row != null)
			{
				row.SortPickPathMethodInfo.ValueChanged -= SortPickPathMethodInfo_ValueChanged; 
				row.SortPutawayPathMethodInfo.ValueChanged -= SortPutawayPathMethodInfo_ValueChanged;
				row.SortCycleCountMethodInfo.ValueChanged -= SortCycleCountMethodInfo_ValueChanged;
				row.NotificationManager.Pop();
			}
		}

		#endregion

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo13 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            this.SelectedUpdateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.GenerateCheckDigitCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.PalletStackHeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.PalletFloorSpacesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.MaxDimensionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.MaxWidthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.PutawayAreaGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.LocationTypeGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.SortByPathGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.PutawayPathSortMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.UpdatePathSequenceOrderButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.CycleCountSortMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.PickPathSortMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.PickingAreaGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.MaxCubicCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.MaxWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.PickMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.SelectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.SelectedUpdateButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.MaxHeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.MaxDepthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.MaximumTouchCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.MaxQuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.GridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.LocationGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SelectedUpdateGroupBox.SuspendLayout();
            this.MaxDimensionDropEdit.SuspendLayout();
            this.PutawayAreaGuidFindBox.SuspendLayout();
            this.LocationTypeGuidFindBox.SuspendLayout();
            this.SortByPathGroupBox.SuspendLayout();
            this.PutawayPathSortMethodDropEdit.SuspendLayout();
            this.CycleCountSortMethodDropEdit.SuspendLayout();
            this.PickPathSortMethodDropEdit.SuspendLayout();
            this.PickingAreaGuidFindBox.SuspendLayout();
            this.MaxCubicCalcDropEdit.SuspendLayout();
            this.MaxWeightCalcDropEdit.SuspendLayout();
            this.PickMethodDropEdit.SuspendLayout();
            this.StatusDropEdit.SuspendLayout();
            this.GridPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LocationGrid)).BeginInit();
            this.LocationGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Environment.Business.WhsRow);
            // 
            // SelectedUpdateGroupBox
            // 
            this.SelectedUpdateGroupBox.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("LocationsEditBaseControl|e9ae9753-1300-4cca-9479-8475fb3d5f1f", "Selected Update");
            this.SelectedUpdateGroupBox.Controls.Add(this.GenerateCheckDigitCheckBox);
            this.SelectedUpdateGroupBox.Controls.Add(this.PalletStackHeightCalcEdit);
            this.SelectedUpdateGroupBox.Controls.Add(this.PalletFloorSpacesCalcEdit);
            this.SelectedUpdateGroupBox.Controls.Add(this.MaxDimensionDropEdit);
            this.SelectedUpdateGroupBox.Controls.Add(this.MaxWidthCalcEdit);
            this.SelectedUpdateGroupBox.Controls.Add(this.PutawayAreaGuidFindBox);
            this.SelectedUpdateGroupBox.Controls.Add(this.LocationTypeGuidFindBox);
            this.SelectedUpdateGroupBox.Controls.Add(this.SortByPathGroupBox);
            this.SelectedUpdateGroupBox.Controls.Add(this.PickingAreaGuidFindBox);
            this.SelectedUpdateGroupBox.Controls.Add(this.MaxCubicCalcDropEdit);
            this.SelectedUpdateGroupBox.Controls.Add(this.MaxWeightCalcDropEdit);
            this.SelectedUpdateGroupBox.Controls.Add(this.PickMethodDropEdit);
            this.SelectedUpdateGroupBox.Controls.Add(this.StatusDropEdit);
            this.SelectedUpdateGroupBox.Controls.Add(this.SelectAllButton);
            this.SelectedUpdateGroupBox.Controls.Add(this.SelectedUpdateButton);
            this.SelectedUpdateGroupBox.Controls.Add(this.MaxHeightCalcEdit);
            this.SelectedUpdateGroupBox.Controls.Add(this.MaxDepthCalcEdit);
            this.SelectedUpdateGroupBox.Controls.Add(this.MaximumTouchCountCalcEdit);
            this.SelectedUpdateGroupBox.Controls.Add(this.MaxQuantityCalcEdit);
            this.SelectedUpdateGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.SelectedUpdateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.SelectedUpdateGroupBox.Name = "SelectedUpdateGroupBox";
            this.SelectedUpdateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 166, true);
            this.SelectedUpdateGroupBox.TabIndex = 5;
            this.SelectedUpdateGroupBox.TabStop = false;
            // 
            // GenerateCheckDigitCheckBox
            // 
            this.BindingSource.SetBindingMember(this.GenerateCheckDigitCheckBox, "GenerateCheckDigit");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).GenerateCheckDigit)));
            this.GenerateCheckDigitCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 93, true);
            this.GenerateCheckDigitCheckBox.Name = "GenerateCheckDigitCheckBox";
            this.GenerateCheckDigitCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 24, true);
            this.GenerateCheckDigitCheckBox.TabIndex = 17;
            this.GenerateCheckDigitCheckBox.UseVisualStyleBackColor = true;
            this.GenerateCheckDigitCheckBox.BackColor = System.Drawing.Color.Transparent;
            // 
            // PalletStackHeightCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.PalletStackHeightCalcEdit, "PalletStackHeight");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).PalletStackHeight)));
            this.PalletStackHeightCalcEdit.DecimalPlaces = 3;
            this.PalletStackHeightCalcEdit.Decimals = 3;
            this.PalletStackHeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(514, 140, true);
            this.PalletStackHeightCalcEdit.Name = "PalletStackHeightCalcEdit";
            this.PalletStackHeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(38, 23, true);
            this.PalletStackHeightCalcEdit.TabIndex = 16;
            this.PalletStackHeightCalcEdit.Text = "0.000";
            this.PalletStackHeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.PalletStackHeightCalcEdit.TrackDisposedAccess = true;
            this.PalletStackHeightCalcEdit.Visible = false;
            // 
            // PalletFloorSpacesCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.PalletFloorSpacesCalcEdit, "PalletFloorSpaces");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).PalletFloorSpaces)));
            this.PalletFloorSpacesCalcEdit.DecimalPlaces = 3;
            this.PalletFloorSpacesCalcEdit.Decimals = 3;
            this.PalletFloorSpacesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 140, true);
            this.PalletFloorSpacesCalcEdit.Name = "PalletFloorSpacesCalcEdit";
            this.PalletFloorSpacesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(38, 23, true);
            this.PalletFloorSpacesCalcEdit.TabIndex = 15;
            this.PalletFloorSpacesCalcEdit.Text = "0.000";
            this.PalletFloorSpacesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.PalletFloorSpacesCalcEdit.TrackDisposedAccess = true;
            this.PalletFloorSpacesCalcEdit.Visible = false;
            // 
            // MaxDimensionDropEdit
            // 
            this.MaxDimensionDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.MaxDimensionDropEdit, "MaxDimensionUnit");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).MaxDimensionUnit)));
            this.MaxDimensionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 117, true);
            this.MaxDimensionDropEdit.Name = "MaxDimensionDropEdit";
            this.MaxDimensionDropEdit.ShowDescriptionBox = false;
            this.MaxDimensionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
            this.MaxDimensionDropEdit.TabIndex = 11;
            // 
            // MaxWidthCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.MaxWidthCalcEdit, "MaxWidth");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).MaxWidth)));
            this.MaxWidthCalcEdit.DecimalPlaces = 3;
            this.MaxWidthCalcEdit.Decimals = 3;
            this.MaxWidthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 70, true);
            this.MaxWidthCalcEdit.Name = "MaxWidthCalcEdit";
            this.MaxWidthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 23, true);
            this.MaxWidthCalcEdit.TabIndex = 9;
            this.MaxWidthCalcEdit.Text = "0.000";
            this.MaxWidthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.MaxWidthCalcEdit.TrackDisposedAccess = true;
            this.MaxWidthCalcEdit.Visible = false;
            // 
            // PutawayAreaGuidFindBox
            // 
            this.PutawayAreaGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PutawayAreaGuidFindBox, "PutawayArea_MassUpdate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).PutawayArea_MassUpdate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Lookups.PutawayAreas)));
            this.PutawayAreaGuidFindBox.BindToList = "Lookups+PutawayAreas";
            this.PutawayAreaGuidFindBox.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("ba6528ed-b25e-4e71-a90e-9efff2b69bbc", "Putaway Area");
            this.PutawayAreaGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 94, true);
            this.PutawayAreaGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.WhsConfigArea;
            this.PutawayAreaGuidFindBox.Name = "PutawayAreaGuidFindBox";
            this.PutawayAreaGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.PutawayAreaGuidFindBox.ParentType = null;
            this.PutawayAreaGuidFindBox.PreBoundMaxLength = 16;
            this.PutawayAreaGuidFindBox.ShowDescriptionBox = false;
            this.PutawayAreaGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 23, true);
            this.PutawayAreaGuidFindBox.TabIndex = 4;
            // 
            // LocationTypeGuidFindBox
            // 
            this.LocationTypeGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.LocationTypeGuidFindBox, "LocationType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).LocationType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Lookups.LocationTypes)));
            this.LocationTypeGuidFindBox.BindToList = "Lookups+LocationTypes";
            this.LocationTypeGuidFindBox.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("LocationsEditBaseControl|69d44a66-f109-41d1-aa4a-51865b79013c", "Type");
            this.LocationTypeGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 22, true);
            this.LocationTypeGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.WhsConfigLocationType;
            this.LocationTypeGuidFindBox.Name = "LocationTypeGuidFindBox";
            this.LocationTypeGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.LocationTypeGuidFindBox.ParentType = null;
            this.LocationTypeGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 23, true);
            this.LocationTypeGuidFindBox.TabIndex = 7;
            // 
            // SortByPathGroupBox
            // 
            this.SortByPathGroupBox.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("2098F050-2F7E-4172-9C4C-5F2BDEA4943C", "Path Sequence Sorting");
            this.SortByPathGroupBox.Controls.Add(this.PutawayPathSortMethodDropEdit);
            this.SortByPathGroupBox.Controls.Add(this.UpdatePathSequenceOrderButton);
            this.SortByPathGroupBox.Controls.Add(this.CycleCountSortMethodDropEdit);
            this.SortByPathGroupBox.Controls.Add(this.PickPathSortMethodDropEdit);
            this.SortByPathGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(666, 16, true);
            this.SortByPathGroupBox.Name = "SortByPathGroupBox";
            this.SortByPathGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(311, 144, true);
            this.SortByPathGroupBox.TabIndex = 14;
            this.SortByPathGroupBox.TabStop = false;
            // 
            // PutawayPathSortMethodDropEdit
            // 
            this.PutawayPathSortMethodDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PutawayPathSortMethodDropEdit, "SortPutawayPathMethod");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).SortPutawayPathMethod)));
            this.PutawayPathSortMethodDropEdit.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("LocationsEditBaseControl|CD2424E3-A39D-462A-B10F-466A07CE5D45", "Putaway Path Sequence");
            this.PutawayPathSortMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 46, true);
            this.PutawayPathSortMethodDropEdit.Name = "PutawayPathSortMethodDropEdit";
            this.PutawayPathSortMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 23, true);
            this.PutawayPathSortMethodDropEdit.TabIndex = 15;
            // 
            // UpdatePathSequenceOrderButton
            // 
            this.UpdatePathSequenceOrderButton.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("LocationsEditBaseControl|431CB427-568D-4D0B-A6A0-40EAFDB805AE", "Update Path Sequence");
            this.UpdatePathSequenceOrderButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 115, true);
            this.UpdatePathSequenceOrderButton.Name = "UpdatePathSequenceOrderButton";
            this.UpdatePathSequenceOrderButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 23, true);
            this.UpdatePathSequenceOrderButton.TabIndex = 17;
            this.UpdatePathSequenceOrderButton.ToolTipCaption = null;
            this.UpdatePathSequenceOrderButton.Click += new System.EventHandler(this.UpdatePathSequenceOrder_Click);
            // 
            // CycleCountSortMethodDropEdit
            // 
            this.CycleCountSortMethodDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CycleCountSortMethodDropEdit, "SortCycleCountMethod");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).SortCycleCountMethod)));
            this.CycleCountSortMethodDropEdit.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("LocationsEditBaseControl|512E64F5-431C-472C-B46B-1867BB69ED73", "Cycle Count Sequence");
            this.CycleCountSortMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 70, true);
            this.CycleCountSortMethodDropEdit.Name = "CycleCountSortMethodDropEdit";
            this.CycleCountSortMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 23, true);
            this.CycleCountSortMethodDropEdit.TabIndex = 16;
            // 
            // PickPathSortMethodDropEdit
            // 
            this.PickPathSortMethodDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PickPathSortMethodDropEdit, "SortPickPathMethod");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).SortPickPathMethod)));
            this.PickPathSortMethodDropEdit.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("LocationsEditBaseControl|9BC07D6C-E501-4E20-8B9C-2A7AF27B6888", "Pick Path Sequence");
            this.PickPathSortMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 22, true);
            this.PickPathSortMethodDropEdit.Name = "PickPathSortMethodDropEdit";
            this.PickPathSortMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 23, true);
            this.PickPathSortMethodDropEdit.TabIndex = 14;
            // 
            // PickingAreaGuidFindBox
            // 
            this.PickingAreaGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PickingAreaGuidFindBox, "PickingArea_MassUpdate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).PickingArea_MassUpdate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Lookups.PickingAreas)));
            this.PickingAreaGuidFindBox.BindToList = "Lookups+PickingAreas";
            this.PickingAreaGuidFindBox.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("LocationsEditBaseControl|16FB9779-DBC7-4C97-AE9A-F9D3B72D73CF", "Pick Area");
            this.PickingAreaGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 70, true);
            this.PickingAreaGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.WhsConfigArea;
            this.PickingAreaGuidFindBox.Name = "PickingAreaGuidFindBox";
            this.PickingAreaGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.PickingAreaGuidFindBox.ParentType = null;
            this.PickingAreaGuidFindBox.PreBoundMaxLength = 16;
            this.PickingAreaGuidFindBox.ShowDescriptionBox = false;
            this.PickingAreaGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 23, true);
            this.PickingAreaGuidFindBox.TabIndex = 3;
            // 
            // MaxCubicCalcDropEdit
            // 
            this.MaxCubicCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.MaxCubicCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).MaxCubic)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).MaxCubicUnit)));
            this.MaxCubicCalcDropEdit.BindToAmount = "MaxCubic";
            this.MaxCubicCalcDropEdit.BindToUnit = "MaxCubicUnit";
            this.MaxCubicCalcDropEdit.Decimals = 3;
            this.MaxCubicCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 46, true);
            this.MaxCubicCalcDropEdit.Name = "MaxCubicCalcDropEdit";
            this.MaxCubicCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 23, true);
            this.MaxCubicCalcDropEdit.TabIndex = 2;
            // 
            // MaxWeightCalcDropEdit
            // 
            this.MaxWeightCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.MaxWeightCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).MaxWeight)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).MaxWeightUnit)));
            this.MaxWeightCalcDropEdit.BindToAmount = "MaxWeight";
            this.MaxWeightCalcDropEdit.BindToUnit = "MaxWeightUnit";
            this.MaxWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 22, true);
            this.MaxWeightCalcDropEdit.Name = "MaxWeightCalcDropEdit";
            this.MaxWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 23, true);
            this.MaxWeightCalcDropEdit.TabIndex = 1;
            // 
            // PickMethodDropEdit
            // 
            this.PickMethodDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PickMethodDropEdit, "PickMethod");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).PickMethod)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).PickMethods)));
            this.PickMethodDropEdit.BindToList = "PickMethods";
            this.PickMethodDropEdit.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("LocationsEditBaseControl|b75589de-e992-4894-9524-c1f161ba1572", "Pick Method");
            this.PickMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 140, true);
            this.PickMethodDropEdit.Name = "PickMethodDropEdit";
            this.PickMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 23, true);
            this.PickMethodDropEdit.TabIndex = 6;
            // 
            // StatusDropEdit
            // 
            this.StatusDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.StatusDropEdit, "LocationStatus");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).LocationStatus)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).LocationStatuses)));
            this.StatusDropEdit.BindToList = "LocationStatuses";
            this.StatusDropEdit.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("LocationsEditBaseControl|9bb81020-f462-4271-9d80-7e9760bb7eb6", "Status");
            this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 116, true);
            this.StatusDropEdit.Name = "StatusDropEdit";
            this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 23, true);
            this.StatusDropEdit.TabIndex = 5;
            // 
            // SelectAllButton
            // 
            this.SelectAllButton.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("LocationsEditBaseControl|cceddd69-7009-45bc-a789-bcdadffa54fd", "Select All");
            this.SelectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(558, 22, true);
            this.SelectAllButton.Name = "SelectAllButton";
            this.SelectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
            this.SelectAllButton.TabIndex = 12;
            this.SelectAllButton.ToolTipCaption = null;
            this.SelectAllButton.Click += new System.EventHandler(this.SelectAllButton_Click);
            // 
            // SelectedUpdateButton
            // 
            this.SelectedUpdateButton.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("LocationsEditBaseControl|348264f7-4063-4080-8c05-323637b829cc", "Update");
            this.SelectedUpdateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(558, 49, true);
            this.SelectedUpdateButton.Name = "SelectedUpdateButton";
            this.SelectedUpdateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
            this.SelectedUpdateButton.TabIndex = 13;
            this.SelectedUpdateButton.ToolTipCaption = null;
            this.SelectedUpdateButton.Click += new System.EventHandler(this.SelectedUpdateButton_Click);
            // 
            // MaxHeightCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.MaxHeightCalcEdit, "MaxHeight");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).MaxHeight)));
            this.MaxHeightCalcEdit.DecimalPlaces = 3;
            this.MaxHeightCalcEdit.Decimals = 3;
            this.MaxHeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 94, true);
            this.MaxHeightCalcEdit.Name = "MaxHeightCalcEdit";
            this.MaxHeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 23, true);
            this.MaxHeightCalcEdit.TabIndex = 10;
            this.MaxHeightCalcEdit.Text = "0.000";
            this.MaxHeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.MaxHeightCalcEdit.TrackDisposedAccess = true;
            this.MaxHeightCalcEdit.Visible = false;
            // 
            // MaxDepthCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.MaxDepthCalcEdit, "MaxDepth");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).MaxDepth)));
            this.MaxDepthCalcEdit.DecimalPlaces = 3;
            this.MaxDepthCalcEdit.Decimals = 3;
            this.MaxDepthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 46, true);
            this.MaxDepthCalcEdit.Name = "MaxDepthCalcEdit";
            this.MaxDepthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 23, true);
            this.MaxDepthCalcEdit.TabIndex = 8;
            this.MaxDepthCalcEdit.Text = "0.000";
            this.MaxDepthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.MaxDepthCalcEdit.TrackDisposedAccess = true;
            this.MaxDepthCalcEdit.Visible = false;
            // 
            // MaximumTouchCountCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.MaximumTouchCountCalcEdit, "MaximumTouchCount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).MaximumTouchCount)));
            this.MaximumTouchCountCalcEdit.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("ae23a657-8587-40c4-8746-de0b8c869dc8", "Max Touch Count");
            this.MaximumTouchCountCalcEdit.DecimalPlaces = 0;
            this.MaximumTouchCountCalcEdit.Decimals = 0;
            this.MaximumTouchCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 70, true);
            this.MaximumTouchCountCalcEdit.Name = "MaximumTouchCountCalcEdit";
            this.MaximumTouchCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 23, true);
            this.MaximumTouchCountCalcEdit.TabIndex = 9;
            this.MaximumTouchCountCalcEdit.Text = "0";
            this.MaximumTouchCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.MaximumTouchCountCalcEdit.TrackDisposedAccess = true;
            // 
            // MaxQuantityCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.MaxQuantityCalcEdit, "MaxQuantity");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).MaxQuantity)));
            this.MaxQuantityCalcEdit.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("LocationsEditBaseControl|4ECBA806-577D-4DBF-9009-5678E9347E95", "Max Quantity");
            this.MaxQuantityCalcEdit.DecimalPlaces = 0;
            this.MaxQuantityCalcEdit.Decimals = 0;
            this.MaxQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 46, true);
            this.MaxQuantityCalcEdit.Name = "MaxQuantityCalcEdit";
            this.MaxQuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 23, true);
            this.MaxQuantityCalcEdit.TabIndex = 8;
            this.MaxQuantityCalcEdit.Text = "0";
            this.MaxQuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.MaxQuantityCalcEdit.TrackDisposedAccess = true;
            // 
            // GridPanel
            // 
            this.GridPanel.Controls.Add(this.LocationGrid);
            this.GridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 166, true);
            this.GridPanel.Name = "GridPanel";
            this.GridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 282, true);
            this.GridPanel.TabIndex = 6;
            // 
            // LocationGrid
            // 
            this.LocationGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.LocationGrid, "Locations");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).RowName)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).FormattedColumn)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).FormattedLevel)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).FormattedTray)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).FormattedCheckDigit)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).WLV_WA_PickingArea)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).Lookups.PickingAreaList)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).WLV_WA_PutawayArea)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).Lookups.PutawayAreaList)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).WLV_LocationStatus)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).LocationStatuses)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).WLV_WLT_LocationType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).WLV_PickMethod)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).PickMethods)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).WLV_MaxWeight)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).WLV_MaxWeightUnit)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).WLV_MaxCubic)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).WLV_MaxCubicUnit)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).WLV_MaxDepth)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).WLV_MaxWidth)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).WLV_MaxHeight)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).WLV_MaxDimensionUnit)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).WLV_MaxQuantity)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).WLV_PalletFloorSpaces)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).WLV_PalletStackHeight)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).WLV_PickPathSequence)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).WLV_PutawayPathSequence)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).WLV_MaximumPickCountBeforeAutomatedStocktake)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).WLV_FinalisedPickCount)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).WLV_LastInventoryChangeDateForBinding)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).WLV_CycleCountPathSequence)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).WLV_CycleCountLastPerformedForBinding)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).RowLocationSequence)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).WLV_TransitDischargeLRC)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).WLV_RS_NKTransitServiceLevel)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsLocation)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Locations)).SyncRoot)).Lookups.TransitServiceLevels)));
            this.LocationGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("LocationsEditBaseControl|37f21b08-e0ab-4144-a7ed-ebc84a90e767", "Row");
            zTextBoxColumnStyleInfo1.ColumnName = "RowName";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("LocationsEditBaseControl|3cf67e2a-829e-4c0a-8dfe-031bc546ad32", "Col");
            zTextBoxColumnStyleInfo2.ColumnName = "FormattedColumn";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.IsReadOnly = true;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("LocationsEditBaseControl|ab249ca9-d877-4dfa-87d5-53f1b18f0166", "Level");
            zTextBoxColumnStyleInfo3.ColumnName = "FormattedLevel";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.IsReadOnly = true;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("LocationsEditBaseControl|095e81dc-df41-4754-ba40-acf3d321de14", "Tray");
            zTextBoxColumnStyleInfo4.ColumnName = "FormattedTray";
            zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo4.IsReadOnly = true;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo5.ColumnName = "FormattedCheckDigit";
            zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
            zGuidDropEditColumnStyleInfo1.BindToList = "Lookups.PickingAreaList";
            zGuidDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zGuidDropEditColumnStyleInfo1.ColumnName = "WLV_WA_PickingArea";
            zGuidDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zGuidDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
            zGuidDropEditColumnStyleInfo1.ToolTip = "The Putaway Area this location is assigned to";
            zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zGuidDropEditColumnStyleInfo2.BindToList = "Lookups.PutawayAreaList";
            zGuidDropEditColumnStyleInfo2.ColumnName = "WLV_WA_PutawayArea";
            zGuidDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zGuidDropEditColumnStyleInfo2.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
            zGuidDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo1.BindToList = "LocationStatuses";
            zDropEditColumnStyleInfo1.ColumnName = "WLV_LocationStatus";
            zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zGuidFindBoxColumnStyleInfo1.ColumnName = "WLV_WLT_LocationType";
            zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo2.BindToList = "PickMethods";
            zDropEditColumnStyleInfo2.ColumnName = "WLV_PickMethod";
            zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.ColumnName = "WLV_MaxWeight";
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.GroupName = Enterprise.Warehouse.Environment.GUI.Res.GetData("e853608f-a01b-4e6f-88c5-c930f428c4ad", "Max Weight");
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo3.ColumnName = "WLV_MaxWeightUnit";
            zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo3.GroupName = Enterprise.Warehouse.Environment.GUI.Res.GetData("e853608f-a01b-4e6f-88c5-c930f428c4ad", "Max Weight");
            zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.ColumnName = "WLV_MaxCubic";
            zCalcEditColumnStyleInfo2.Decimals = 3;
            zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo2.GroupName = Enterprise.Warehouse.Environment.GUI.Res.GetData("e4810887-eeb8-40e6-88e1-324597d3c0d0", "Max Cubic");
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo4.ColumnName = "WLV_MaxCubicUnit";
            zDropEditColumnStyleInfo4.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo4.GroupName = Enterprise.Warehouse.Environment.GUI.Res.GetData("e4810887-eeb8-40e6-88e1-324597d3c0d0", "Max Cubic");
            zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo3.ColumnName = "WLV_MaxDepth";
            zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo3.GroupName = Enterprise.Warehouse.Environment.GUI.Res.GetData("6b431502-bb76-4d4f-9b9e-9859c63f4365", "Max Dimensions");
            zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo4.ColumnName = "WLV_MaxWidth";
            zCalcEditColumnStyleInfo4.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo4.GroupName = Enterprise.Warehouse.Environment.GUI.Res.GetData("6b431502-bb76-4d4f-9b9e-9859c63f4365", "Max Dimensions");
            zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo5.ColumnName = "WLV_MaxHeight";
            zCalcEditColumnStyleInfo5.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo5.GroupName = Enterprise.Warehouse.Environment.GUI.Res.GetData("6b431502-bb76-4d4f-9b9e-9859c63f4365", "Max Dimensions");
            zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo5.ColumnName = "WLV_MaxDimensionUnit";
            zDropEditColumnStyleInfo5.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo5.GroupName = Enterprise.Warehouse.Environment.GUI.Res.GetData("6b431502-bb76-4d4f-9b9e-9859c63f4365", "Max Dimensions");
            zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo6.ColumnName = "WLV_MaxQuantity";
            zCalcEditColumnStyleInfo6.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo7.ColumnName = "WLV_PalletFloorSpaces";
            zCalcEditColumnStyleInfo7.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo7.GroupName = Enterprise.Warehouse.Environment.GUI.Res.GetData("f122a2a1-694b-44d0-939e-5051209c9f02", "Pallet Spaces");
            zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
            zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo8.ColumnName = "WLV_PalletStackHeight";
            zCalcEditColumnStyleInfo8.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo8.GroupName = Enterprise.Warehouse.Environment.GUI.Res.GetData("f122a2a1-694b-44d0-939e-5051209c9f02", "Pallet Spaces");
            zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
            zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo9.ColumnName = "WLV_PickPathSequence";
            zCalcEditColumnStyleInfo9.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(119);
            zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo10.ColumnName = "WLV_PutawayPathSequence";
            zCalcEditColumnStyleInfo10.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(119);
            zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo11.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("9d68dd4b-47dd-4209-99f2-6ba7b47bbc9a", "Maximum Touch Count");
            zCalcEditColumnStyleInfo11.ColumnName = "WLV_MaximumPickCountBeforeAutomatedStocktake";
            zCalcEditColumnStyleInfo11.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo12.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo12.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("f5727678-9092-4475-94d2-5158224e0b5c", "Current Touch Count");
            zCalcEditColumnStyleInfo12.ColumnName = "WLV_FinalisedPickCount";
            zCalcEditColumnStyleInfo12.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo12.IsReadOnly = true;
            zCalcEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(109);
            zDateEditColumnStyleInfo1.ColumnName = "WLV_LastInventoryChangeDateForBinding";
            zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo1.IsReadOnly = true;
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zCalcEditColumnStyleInfo13.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo13.ColumnName = "WLV_CycleCountPathSequence";
            zCalcEditColumnStyleInfo13.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
            zDateEditColumnStyleInfo2.ColumnName = "WLV_CycleCountLastPerformedForBinding";
            zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo2.IsReadOnly = true;
            zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
            zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("f6c674e8-dcb9-4327-b582-dba958f0597b", "Row Loc. Seq");
            zTextBoxColumnStyleInfo6.ColumnName = "RowLocationSequence";
            zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo6.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCodeFindBoxColumnStyleInfo1.ColumnName = "WLV_TransitDischargeLRC";
            zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo6.BindToList = "Lookups.TransitServiceLevels";
            zDropEditColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo6.ColumnName = "WLV_RS_NKTransitServiceLevel";
            zDropEditColumnStyleInfo6.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.LocationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.LocationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.LocationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.LocationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.LocationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.LocationGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
            this.LocationGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
            this.LocationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.LocationGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
            this.LocationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
            this.LocationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.LocationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
            this.LocationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.LocationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
            this.LocationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
            this.LocationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
            this.LocationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
            this.LocationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
            this.LocationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
            this.LocationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
            this.LocationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
            this.LocationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
            this.LocationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
            this.LocationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
            this.LocationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
            this.LocationGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.LocationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo13);
            this.LocationGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
            this.LocationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.LocationGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
            this.LocationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
            this.LocationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LocationGrid.GridId = "e4259276-4b69-4224-b56c-3dfdce780bcd";
            this.LocationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.LocationGrid.LayoutKey = "LocationGrid";
            this.LocationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.LocationGrid.Name = "LocationGrid";
            this.LocationGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
            this.LocationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 282, true);
            this.LocationGrid.TabIndex = 7;
            // 
            // LocationsEditBaseControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.GridPanel);
            this.Controls.Add(this.SelectedUpdateGroupBox);
            this.Name = "LocationsEditBaseControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 448, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.SelectedUpdateGroupBox.ResumeLayout(false);
            this.SelectedUpdateGroupBox.PerformLayout();
            this.MaxDimensionDropEdit.ResumeLayout(true);
            this.MaxDimensionDropEdit.PerformLayout();
            this.PutawayAreaGuidFindBox.ResumeLayout(true);
            this.PutawayAreaGuidFindBox.PerformLayout();
            this.LocationTypeGuidFindBox.ResumeLayout(true);
            this.LocationTypeGuidFindBox.PerformLayout();
            this.SortByPathGroupBox.ResumeLayout(false);
            this.SortByPathGroupBox.PerformLayout();
            this.PutawayPathSortMethodDropEdit.ResumeLayout(true);
            this.PutawayPathSortMethodDropEdit.PerformLayout();
            this.CycleCountSortMethodDropEdit.ResumeLayout(true);
            this.CycleCountSortMethodDropEdit.PerformLayout();
            this.PickPathSortMethodDropEdit.ResumeLayout(true);
            this.PickPathSortMethodDropEdit.PerformLayout();
            this.PickingAreaGuidFindBox.ResumeLayout(true);
            this.PickingAreaGuidFindBox.PerformLayout();
            this.MaxCubicCalcDropEdit.ResumeLayout(true);
            this.MaxCubicCalcDropEdit.PerformLayout();
            this.MaxWeightCalcDropEdit.ResumeLayout(true);
            this.MaxWeightCalcDropEdit.PerformLayout();
            this.PickMethodDropEdit.ResumeLayout(true);
            this.PickMethodDropEdit.PerformLayout();
            this.StatusDropEdit.ResumeLayout(true);
            this.StatusDropEdit.PerformLayout();
            this.GridPanel.ResumeLayout(false);
            this.GridPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LocationGrid)).EndInit();
            this.LocationGrid.ResumeLayout(false);
            this.LocationGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		public Enterprise.ZArchitecture.GUI.ZGroupBox SelectedUpdateGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit MaxCubicCalcDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit MaxWeightCalcDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit PickMethodDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit StatusDropEdit;
		public Enterprise.ZArchitecture.GUI.ZButton SelectAllButton;
		public Enterprise.ZArchitecture.GUI.ZButton SelectedUpdateButton;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox PickingAreaGuidFindBox;
		private ZArchitecture.ZCalcEdit MaximumTouchCountCalcEdit;
		private ZArchitecture.ZCalcEdit MaxQuantityCalcEdit;
		public ZArchitecture.GUI.ZGroupBox SortByPathGroupBox;
		private ZArchitecture.GUI.ZPanel GridPanel;
		public ZArchitecture.ZGrid LocationGrid;
		private ZArchitecture.GUI.ZGuidFindBox LocationTypeGuidFindBox;
		private ZArchitecture.GUI.ZGuidFindBox PutawayAreaGuidFindBox;
		public ZArchitecture.GUI.ZButton UpdatePathSequenceOrderButton;
		protected ZArchitecture.GUI.ZDropEdit CycleCountSortMethodDropEdit;
		protected ZArchitecture.GUI.ZDropEdit PickPathSortMethodDropEdit;
		protected ZArchitecture.GUI.ZDropEdit PutawayPathSortMethodDropEdit;
		private ZArchitecture.ZCalcEdit MaxHeightCalcEdit;
		private ZArchitecture.ZCalcEdit MaxDepthCalcEdit;
		protected ZArchitecture.GUI.ZDropEdit MaxDimensionDropEdit;
		private ZArchitecture.ZCalcEdit MaxWidthCalcEdit;
		private ZArchitecture.ZCalcEdit PalletStackHeightCalcEdit;
		private ZArchitecture.ZCalcEdit PalletFloorSpacesCalcEdit;
		private ZArchitecture.GUI.ZCheckBox GenerateCheckDigitCheckBox;
	}
}
