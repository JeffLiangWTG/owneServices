namespace Enterprise.Freight.Agency.GUI
{
	partial class AllocationAdjustmentDisplayTable
	{
		private void InitializeComponent()
		{
			gpTEULabel = new Enterprise.ZArchitecture.ZLabel();
			volumeLabel = new Enterprise.ZArchitecture.ZLabel();
			tonnesLabel = new Enterprise.ZArchitecture.ZLabel();
			powerPointsLabel = new Enterprise.ZArchitecture.ZLabel();
			allocatedLabel = new Enterprise.ZArchitecture.ZLabel();
			allocatedTEUCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			allocatedVolumeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			allocatedTonnesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			overAllocationTEUCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			overAllocationVolumeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			overAllocationTonnesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			oldOverAllocatedLabel = new Enterprise.ZArchitecture.ZLabel();
			totalRequiredLabel = new Enterprise.ZArchitecture.ZLabel();
			requiredTEUCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			requiredVolumeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			requiredTonnesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			newOverAllocatedLabel = new Enterprise.ZArchitecture.ZLabel();
			newOverAllocationTEUCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			newOverAllocationVolumeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			newOverAllocationTonnesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			allocatedPowerPointsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			overAllocationPowerPointsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			newOverAllocationPowerPointsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			requiredPowerPointsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			allocationsTable = new CargoWise.Windows.UI.KTableLayoutPanel();
			areaLabel = new Enterprise.ZArchitecture.ZLabel();
			allocatedAreaCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			overAllocationAreaCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			newOverAllocationAreaCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			requiredAreaCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			allocationsTable.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails);
			// 
			// gpTEULabel
			// 
			gpTEULabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("AllocationAdjustmentDisplayTable|d9b99087-adf9-423f-bb3d-03e59a9313f5", "TEUs");
			gpTEULabel.Dock = System.Windows.Forms.DockStyle.Fill;
			gpTEULabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 0, true);
			gpTEULabel.Name = "gpTEULabel";
			gpTEULabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			gpTEULabel.TabIndex = 0;
			// 
			// volumeLabel
			// 
			volumeLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("AllocationAdjustmentDisplayTable|5b55f3f1-a906-47f8-ab79-583b7893fc19", "Volume (M3)");
			volumeLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			volumeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 0, true);
			volumeLabel.Name = "volumeLabel";
			volumeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			volumeLabel.TabIndex = 2;
			// 
			// tonnesLabel
			// 
			tonnesLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("AllocationAdjustmentDisplayTable|c24c2623-ae3e-4d44-91b3-aa28f1248cc4", "Tonnes");
			tonnesLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			tonnesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(336, 0, true);
			tonnesLabel.Name = "tonnesLabel";
			tonnesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			tonnesLabel.TabIndex = 3;
			// 
			// powerPointsLabel
			// 
			powerPointsLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("AllocationAdjustmentDisplayTable|703d282b-07e0-4869-bf0a-7aa9de019f74", "Power");
			powerPointsLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			powerPointsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 0, true);
			powerPointsLabel.Name = "powerPointsLabel";
			powerPointsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			powerPointsLabel.TabIndex = 1;
			// 
			// allocatedLabel
			// 
			allocatedLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("AllocationAdjustmentDisplayTable|ba0e3efa-b94c-4279-98ed-c1667b836fd4", "Allocated");
			allocatedLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			allocatedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			allocatedLabel.Name = "allocatedLabel";
			allocatedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 26, true);
			allocatedLabel.TabIndex = 5;
			// 
			// allocatedTEUCalcEdit
			// 
			allocatedTEUCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(allocatedTEUCalcEdit, "Allocated_TEU");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails)(null)).Allocated_TEU)));
			allocatedTEUCalcEdit.DecimalPlaces = 0;
			allocatedTEUCalcEdit.Decimals = 0;
			allocatedTEUCalcEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(allocatedTEUCalcEdit, false);
			allocatedTEUCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 23, true);
			allocatedTEUCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 7, 3, true);
			allocatedTEUCalcEdit.Name = "allocatedTEUCalcEdit";
			allocatedTEUCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			allocatedTEUCalcEdit.TabIndex = 6;
			allocatedTEUCalcEdit.Text = "0";
			allocatedTEUCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// allocatedVolumeCalcEdit
			// 
			allocatedVolumeCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(allocatedVolumeCalcEdit, "Allocated_Volume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails)(null)).Allocated_Volume)));
			allocatedVolumeCalcEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(allocatedVolumeCalcEdit, false);
			allocatedVolumeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 23, true);
			allocatedVolumeCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 7, 3, true);
			allocatedVolumeCalcEdit.Name = "allocatedVolumeCalcEdit";
			allocatedVolumeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			allocatedVolumeCalcEdit.TabIndex = 8;
			allocatedVolumeCalcEdit.Text = "0";
			allocatedVolumeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// allocatedTonnesCalcEdit
			// 
			allocatedTonnesCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(allocatedTonnesCalcEdit, "Allocated_Tonnes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails)(null)).Allocated_Tonnes)));
			allocatedTonnesCalcEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(allocatedTonnesCalcEdit, false);
			allocatedTonnesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(336, 23, true);
			allocatedTonnesCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 7, 3, true);
			allocatedTonnesCalcEdit.Name = "allocatedTonnesCalcEdit";
			allocatedTonnesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			allocatedTonnesCalcEdit.TabIndex = 9;
			allocatedTonnesCalcEdit.Text = "0";
			allocatedTonnesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// overAllocationTEUCalcEdit
			// 
			overAllocationTEUCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(overAllocationTEUCalcEdit, "OldOverAllocation_TEU");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails)(null)).OldOverAllocation_TEU)));
			overAllocationTEUCalcEdit.DecimalPlaces = 2;
			overAllocationTEUCalcEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(overAllocationTEUCalcEdit, false);
			overAllocationTEUCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 49, true);
			overAllocationTEUCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 7, 3, true);
			overAllocationTEUCalcEdit.Name = "overAllocationTEUCalcEdit";
			overAllocationTEUCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			overAllocationTEUCalcEdit.TabIndex = 12;
			overAllocationTEUCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// overAllocationVolumeCalcEdit
			// 
			overAllocationVolumeCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(overAllocationVolumeCalcEdit, "OldOverAllocation_Volume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails)(null)).OldOverAllocation_Volume)));
			overAllocationVolumeCalcEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(overAllocationVolumeCalcEdit, false);
			overAllocationVolumeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 49, true);
			overAllocationVolumeCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 7, 3, true);
			overAllocationVolumeCalcEdit.Name = "overAllocationVolumeCalcEdit";
			overAllocationVolumeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			overAllocationVolumeCalcEdit.TabIndex = 14;
			overAllocationVolumeCalcEdit.Text = "0.000";
			overAllocationVolumeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// overAllocationTonnesCalcEdit
			// 
			overAllocationTonnesCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(overAllocationTonnesCalcEdit, "OldOverAllocation_Tonnes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails)(null)).OldOverAllocation_Tonnes)));
			overAllocationTonnesCalcEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(overAllocationTonnesCalcEdit, false);
			overAllocationTonnesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(336, 49, true);
			overAllocationTonnesCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 7, 3, true);
			overAllocationTonnesCalcEdit.Name = "overAllocationTonnesCalcEdit";
			overAllocationTonnesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			overAllocationTonnesCalcEdit.TabIndex = 15;
			overAllocationTonnesCalcEdit.Text = "0.000";
			overAllocationTonnesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// oldOverAllocatedLabel
			// 
			this.BindingSource.SetBindingMember(oldOverAllocatedLabel, "OldPercentLabelText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails)(null)).OldPercentLabelText)));
			oldOverAllocatedLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			oldOverAllocatedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 46, true);
			oldOverAllocatedLabel.Name = "oldOverAllocatedLabel";
			oldOverAllocatedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 26, true);
			oldOverAllocatedLabel.TabIndex = 11;
			// 
			// totalRequiredLabel
			// 
			totalRequiredLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("AllocationAdjustmentDisplayTable|ff6d8947-f8d3-461e-a824-03ddbacaf4b5", "Total Required");
			totalRequiredLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			totalRequiredLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 98, true);
			totalRequiredLabel.Name = "totalRequiredLabel";
			totalRequiredLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 29, true);
			totalRequiredLabel.TabIndex = 23;
			// 
			// requiredTEUCalcEdit
			// 
			requiredTEUCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(requiredTEUCalcEdit, "TotalRequired_TEU");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails)(null)).TotalRequired_TEU)));
			requiredTEUCalcEdit.DecimalPlaces = 2;
			requiredTEUCalcEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(requiredTEUCalcEdit, false);
			requiredTEUCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 101, true);
			requiredTEUCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 7, 3, true);
			requiredTEUCalcEdit.Name = "requiredTEUCalcEdit";
			requiredTEUCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			requiredTEUCalcEdit.TabIndex = 24;
			requiredTEUCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// requiredVolumeCalcEdit
			// 
			requiredVolumeCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(requiredVolumeCalcEdit, "TotalRequired_Volume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails)(null)).TotalRequired_Volume)));
			requiredVolumeCalcEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(requiredVolumeCalcEdit, false);
			requiredVolumeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 101, true);
			requiredVolumeCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 7, 3, true);
			requiredVolumeCalcEdit.Name = "requiredVolumeCalcEdit";
			requiredVolumeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			requiredVolumeCalcEdit.TabIndex = 26;
			requiredVolumeCalcEdit.Text = "0.000";
			requiredVolumeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// requiredTonnesCalcEdit
			// 
			requiredTonnesCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(requiredTonnesCalcEdit, "TotalRequired_Tonnes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails)(null)).TotalRequired_Tonnes)));
			requiredTonnesCalcEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(requiredTonnesCalcEdit, false);
			requiredTonnesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(336, 101, true);
			requiredTonnesCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 7, 3, true);
			requiredTonnesCalcEdit.Name = "requiredTonnesCalcEdit";
			requiredTonnesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			requiredTonnesCalcEdit.TabIndex = 27;
			requiredTonnesCalcEdit.Text = "0.000";
			requiredTonnesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// newOverAllocatedLabel
			// 
			this.BindingSource.SetBindingMember(newOverAllocatedLabel, "NewPercentLabelText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails)(null)).NewPercentLabelText)));
			newOverAllocatedLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			newOverAllocatedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 72, true);
			newOverAllocatedLabel.Name = "newOverAllocatedLabel";
			newOverAllocatedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 26, true);
			newOverAllocatedLabel.TabIndex = 17;
			// 
			// newOverAllocationTEUCalcEdit
			// 
			newOverAllocationTEUCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(newOverAllocationTEUCalcEdit, "NewOverAllocation_TEU");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails)(null)).NewOverAllocation_TEU)));
			newOverAllocationTEUCalcEdit.DecimalPlaces = 2;
			newOverAllocationTEUCalcEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(newOverAllocationTEUCalcEdit, false);
			newOverAllocationTEUCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 75, true);
			newOverAllocationTEUCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 7, 3, true);
			newOverAllocationTEUCalcEdit.Name = "newOverAllocationTEUCalcEdit";
			newOverAllocationTEUCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			newOverAllocationTEUCalcEdit.TabIndex = 18;
			newOverAllocationTEUCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// newOverAllocationVolumeCalcEdit
			// 
			newOverAllocationVolumeCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(newOverAllocationVolumeCalcEdit, "NewOverAllocation_Volume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails)(null)).NewOverAllocation_Volume)));
			newOverAllocationVolumeCalcEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(newOverAllocationVolumeCalcEdit, false);
			newOverAllocationVolumeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 75, true);
			newOverAllocationVolumeCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 7, 3, true);
			newOverAllocationVolumeCalcEdit.Name = "newOverAllocationVolumeCalcEdit";
			newOverAllocationVolumeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			newOverAllocationVolumeCalcEdit.TabIndex = 20;
			newOverAllocationVolumeCalcEdit.Text = "0.000";
			newOverAllocationVolumeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// newOverAllocationTonnesCalcEdit
			// 
			newOverAllocationTonnesCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(newOverAllocationTonnesCalcEdit, "NewOverAllocation_Tonnes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails)(null)).NewOverAllocation_Tonnes)));
			newOverAllocationTonnesCalcEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(newOverAllocationTonnesCalcEdit, false);
			newOverAllocationTonnesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(336, 75, true);
			newOverAllocationTonnesCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 7, 3, true);
			newOverAllocationTonnesCalcEdit.Name = "newOverAllocationTonnesCalcEdit";
			newOverAllocationTonnesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			newOverAllocationTonnesCalcEdit.TabIndex = 21;
			newOverAllocationTonnesCalcEdit.Text = "0.000";
			newOverAllocationTonnesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// allocatedPowerPointsCalcEdit
			// 
			allocatedPowerPointsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(allocatedPowerPointsCalcEdit, "Allocated_PowerPoints");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails)(null)).Allocated_PowerPoints)));
			allocatedPowerPointsCalcEdit.DecimalPlaces = 0;
			allocatedPowerPointsCalcEdit.Decimals = 0;
			allocatedPowerPointsCalcEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(allocatedPowerPointsCalcEdit, false);
			allocatedPowerPointsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 23, true);
			allocatedPowerPointsCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 7, 3, true);
			allocatedPowerPointsCalcEdit.Name = "allocatedPowerPointsCalcEdit";
			allocatedPowerPointsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			allocatedPowerPointsCalcEdit.TabIndex = 7;
			allocatedPowerPointsCalcEdit.Text = "0";
			allocatedPowerPointsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// overAllocationPowerPointsCalcEdit
			// 
			overAllocationPowerPointsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(overAllocationPowerPointsCalcEdit, "OldOverAllocation_PowerPoints");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails)(null)).OldOverAllocation_PowerPoints)));
			overAllocationPowerPointsCalcEdit.DecimalPlaces = 0;
			overAllocationPowerPointsCalcEdit.Decimals = 0;
			overAllocationPowerPointsCalcEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(overAllocationPowerPointsCalcEdit, false);
			overAllocationPowerPointsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 49, true);
			overAllocationPowerPointsCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 7, 3, true);
			overAllocationPowerPointsCalcEdit.Name = "overAllocationPowerPointsCalcEdit";
			overAllocationPowerPointsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			overAllocationPowerPointsCalcEdit.TabIndex = 13;
			overAllocationPowerPointsCalcEdit.Text = "0";
			overAllocationPowerPointsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// newOverAllocationPowerPointsCalcEdit
			// 
			newOverAllocationPowerPointsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(newOverAllocationPowerPointsCalcEdit, "NewOverAllocation_PowerPoints");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails)(null)).NewOverAllocation_PowerPoints)));
			newOverAllocationPowerPointsCalcEdit.DecimalPlaces = 0;
			newOverAllocationPowerPointsCalcEdit.Decimals = 0;
			newOverAllocationPowerPointsCalcEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(newOverAllocationPowerPointsCalcEdit, false);
			newOverAllocationPowerPointsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 75, true);
			newOverAllocationPowerPointsCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 7, 3, true);
			newOverAllocationPowerPointsCalcEdit.Name = "newOverAllocationPowerPointsCalcEdit";
			newOverAllocationPowerPointsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			newOverAllocationPowerPointsCalcEdit.TabIndex = 19;
			newOverAllocationPowerPointsCalcEdit.Text = "0";
			newOverAllocationPowerPointsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// requiredPowerPointsCalcEdit
			// 
			requiredPowerPointsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(requiredPowerPointsCalcEdit, "TotalRequired_PowerPoints");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails)(null)).TotalRequired_PowerPoints)));
			requiredPowerPointsCalcEdit.DecimalPlaces = 0;
			requiredPowerPointsCalcEdit.Decimals = 0;
			requiredPowerPointsCalcEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(requiredPowerPointsCalcEdit, false);
			requiredPowerPointsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 101, true);
			requiredPowerPointsCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 7, 3, true);
			requiredPowerPointsCalcEdit.Name = "requiredPowerPointsCalcEdit";
			requiredPowerPointsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			requiredPowerPointsCalcEdit.TabIndex = 25;
			requiredPowerPointsCalcEdit.Text = "0";
			requiredPowerPointsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// allocationsTable
			// 
			allocationsTable.Anchor = System.Windows.Forms.AnchorStyles.None;
			allocationsTable.ColumnCount = 6;
			allocationsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
			allocationsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			allocationsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			allocationsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			allocationsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			allocationsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			allocationsTable.Controls.Add(gpTEULabel, 1, 0);
			allocationsTable.Controls.Add(allocatedLabel, 0, 1);
			allocationsTable.Controls.Add(allocatedTEUCalcEdit, 1, 1);
			allocationsTable.Controls.Add(overAllocationTEUCalcEdit, 1, 2);
			allocationsTable.Controls.Add(oldOverAllocatedLabel, 0, 2);
			allocationsTable.Controls.Add(totalRequiredLabel, 0, 4);
			allocationsTable.Controls.Add(requiredTEUCalcEdit, 1, 4);
			allocationsTable.Controls.Add(newOverAllocatedLabel, 0, 3);
			allocationsTable.Controls.Add(newOverAllocationTEUCalcEdit, 1, 3);
			allocationsTable.Controls.Add(allocatedPowerPointsCalcEdit, 2, 1);
			allocationsTable.Controls.Add(overAllocationPowerPointsCalcEdit, 2, 2);
			allocationsTable.Controls.Add(newOverAllocationPowerPointsCalcEdit, 2, 3);
			allocationsTable.Controls.Add(requiredPowerPointsCalcEdit, 2, 4);
			allocationsTable.Controls.Add(allocatedVolumeCalcEdit, 3, 1);
			allocationsTable.Controls.Add(overAllocationVolumeCalcEdit, 3, 2);
			allocationsTable.Controls.Add(newOverAllocationVolumeCalcEdit, 3, 3);
			allocationsTable.Controls.Add(requiredVolumeCalcEdit, 3, 4);
			allocationsTable.Controls.Add(requiredTonnesCalcEdit, 4, 4);
			allocationsTable.Controls.Add(newOverAllocationTonnesCalcEdit, 4, 3);
			allocationsTable.Controls.Add(overAllocationTonnesCalcEdit, 4, 2);
			allocationsTable.Controls.Add(allocatedTonnesCalcEdit, 4, 1);
			allocationsTable.Controls.Add(powerPointsLabel, 2, 0);
			allocationsTable.Controls.Add(volumeLabel, 3, 0);
			allocationsTable.Controls.Add(tonnesLabel, 4, 0);
			allocationsTable.Controls.Add(areaLabel, 5, 0);
			allocationsTable.Controls.Add(allocatedAreaCalcEdit, 5, 1);
			allocationsTable.Controls.Add(overAllocationAreaCalcEdit, 5, 2);
			allocationsTable.Controls.Add(newOverAllocationAreaCalcEdit, 5, 3);
			allocationsTable.Controls.Add(requiredAreaCalcEdit, 5, 4);
			allocationsTable.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			allocationsTable.Name = "allocationsTable";
			allocationsTable.RowCount = 5;
			allocationsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(20)));
			allocationsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
			allocationsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
			allocationsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
			allocationsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
			allocationsTable.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 127, true);
			allocationsTable.TabIndex = 0;
			// 
			// areaLabel
			// 
			areaLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("AllocationAdjustmentDisplayTable|276bb2e4-8e4c-45af-b8b3-85c8c10dc192", "Area (M2)");
			areaLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			areaLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(417, 0, true);
			areaLabel.Name = "areaLabel";
			areaLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 20, true);
			areaLabel.TabIndex = 4;
			// 
			// allocatedAreaCalcEdit
			// 
			allocatedAreaCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(allocatedAreaCalcEdit, "Allocated_Area");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails)(null)).Allocated_Area)));
			allocatedAreaCalcEdit.DecimalPlaces = 0;
			allocatedAreaCalcEdit.Decimals = 0;
			allocatedAreaCalcEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			allocatedAreaCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(417, 23, true);
			allocatedAreaCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 7, 3, true);
			allocatedAreaCalcEdit.Name = "allocatedAreaCalcEdit";
			allocatedAreaCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			allocatedAreaCalcEdit.TabIndex = 10;
			allocatedAreaCalcEdit.Text = "0";
			allocatedAreaCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// overAllocationAreaCalcEdit
			// 
			overAllocationAreaCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(overAllocationAreaCalcEdit, "OldOverAllocation_Area");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails)(null)).OldOverAllocation_Area)));
			overAllocationAreaCalcEdit.DecimalPlaces = 3;
			overAllocationAreaCalcEdit.Decimals = 3;
			overAllocationAreaCalcEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			overAllocationAreaCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(417, 49, true);
			overAllocationAreaCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 7, 3, true);
			overAllocationAreaCalcEdit.Name = "overAllocationAreaCalcEdit";
			overAllocationAreaCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			overAllocationAreaCalcEdit.TabIndex = 16;
			overAllocationAreaCalcEdit.Text = "0.000";
			overAllocationAreaCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// newOverAllocationAreaCalcEdit
			// 
			newOverAllocationAreaCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(newOverAllocationAreaCalcEdit, "NewOverAllocation_Area");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails)(null)).NewOverAllocation_Area)));
			newOverAllocationAreaCalcEdit.DecimalPlaces = 3;
			newOverAllocationAreaCalcEdit.Decimals = 3;
			newOverAllocationAreaCalcEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			newOverAllocationAreaCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(417, 75, true);
			newOverAllocationAreaCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 7, 3, true);
			newOverAllocationAreaCalcEdit.Name = "newOverAllocationAreaCalcEdit";
			newOverAllocationAreaCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			newOverAllocationAreaCalcEdit.TabIndex = 22;
			newOverAllocationAreaCalcEdit.Text = "0.000";
			newOverAllocationAreaCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// requiredAreaCalcEdit
			// 
			requiredAreaCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(requiredAreaCalcEdit, "TotalRequired_Area");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails)(null)).TotalRequired_Area)));
			requiredAreaCalcEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("AllocationAdjustmentDisplayTable|4c11a0b2-5ace-4db7-a153-93bb71df0dd7", "Total Required Area", "The total floor space in square meters from all shipments confirmed against this allocation.\r\n\r\nOnly break-bulk and roll-on roll-off count towards area.");
			requiredAreaCalcEdit.DecimalPlaces = 3;
			requiredAreaCalcEdit.Decimals = 3;
			requiredAreaCalcEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			requiredAreaCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(417, 101, true);
			requiredAreaCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 7, 3, true);
			requiredAreaCalcEdit.Name = "requiredAreaCalcEdit";
			requiredAreaCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			requiredAreaCalcEdit.TabIndex = 28;
			requiredAreaCalcEdit.Text = "0.000";
			requiredAreaCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AllocationAdjustmentDisplayTable
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(allocationsTable);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 127, true);
			this.Name = "AllocationAdjustmentDisplayTable";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 127, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			allocationsTable.ResumeLayout(false);
			allocationsTable.PerformLayout();
			this.ResumeLayout(false);

		}

		Enterprise.ZArchitecture.ZLabel gpTEULabel;
		Enterprise.ZArchitecture.ZLabel volumeLabel;
		Enterprise.ZArchitecture.ZLabel tonnesLabel;
		Enterprise.ZArchitecture.ZLabel powerPointsLabel;
		Enterprise.ZArchitecture.ZLabel allocatedLabel;
		Enterprise.ZArchitecture.ZCalcEdit allocatedTEUCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit allocatedVolumeCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit allocatedTonnesCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit overAllocationTEUCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit overAllocationVolumeCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit overAllocationTonnesCalcEdit;
		Enterprise.ZArchitecture.ZLabel oldOverAllocatedLabel;
		Enterprise.ZArchitecture.ZLabel totalRequiredLabel;
		Enterprise.ZArchitecture.ZCalcEdit requiredTEUCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit requiredVolumeCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit requiredTonnesCalcEdit;
		Enterprise.ZArchitecture.ZLabel newOverAllocatedLabel;
		Enterprise.ZArchitecture.ZCalcEdit newOverAllocationTEUCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit newOverAllocationVolumeCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit newOverAllocationTonnesCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit allocatedPowerPointsCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit overAllocationPowerPointsCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit newOverAllocationPowerPointsCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit requiredPowerPointsCalcEdit;
		CargoWise.Windows.UI.KTableLayoutPanel allocationsTable;
		Enterprise.ZArchitecture.ZLabel areaLabel;
		Enterprise.ZArchitecture.ZCalcEdit allocatedAreaCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit overAllocationAreaCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit newOverAllocationAreaCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit requiredAreaCalcEdit;
	}
}
