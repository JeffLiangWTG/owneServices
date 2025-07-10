namespace Enterprise.Freight.Agency.GUI
{
	public partial class ShipmentAllocationUsageControl
	{
		private void InitializeComponent()
		{
			allocated_teuBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			available_TEUBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			required_TEUBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			available_TonnesBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			allocated_TonnesBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			required_TonnesBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			allocated_VolumeBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			required_VolumeBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			available_VolumeBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			refreshButton = new Enterprise.ZArchitecture.GUI.ZButton();
			allocationMethodBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			required_PowerPointsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			allocated_PowerPointsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			available_PowerPointsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			availableLabel = new Enterprise.ZArchitecture.ZLabel();
			teuLabel = new Enterprise.ZArchitecture.ZLabel();
			powerPointsLabel = new Enterprise.ZArchitecture.ZLabel();
			weightLabel = new Enterprise.ZArchitecture.ZLabel();
			volumeLabel = new Enterprise.ZArchitecture.ZLabel();
			allocatedLabel = new Enterprise.ZArchitecture.ZLabel();
			requiredLabel = new Enterprise.ZArchitecture.ZLabel();
			methodLabel = new Enterprise.ZArchitecture.ZLabel();
			available_AreaBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			required_AreaBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			allocated_AreaBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			areaLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.AllocationCalcWrapper);
			// 
			// allocated_teuBoundCalcEdit
			// 
			allocated_teuBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(allocated_teuBoundCalcEdit, "Allocated_TEU");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationCalcWrapper)(null)).Allocated_TEU)));
			allocated_teuBoundCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(allocated_teuBoundCalcEdit, false);
			allocated_teuBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 20, true);
			allocated_teuBoundCalcEdit.Name = "allocated_teuBoundCalcEdit";
			allocated_teuBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			allocated_teuBoundCalcEdit.TabIndex = 6;
			allocated_teuBoundCalcEdit.Text = "0.00";
			allocated_teuBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// available_TEUBoundCalcEdit
			// 
			available_TEUBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(available_TEUBoundCalcEdit, "Available_TEU");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationCalcWrapper)(null)).Available_TEU)));
			available_TEUBoundCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(available_TEUBoundCalcEdit, false);
			available_TEUBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 44, true);
			available_TEUBoundCalcEdit.Name = "available_TEUBoundCalcEdit";
			available_TEUBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			available_TEUBoundCalcEdit.TabIndex = 12;
			available_TEUBoundCalcEdit.Text = "0.00";
			available_TEUBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// required_TEUBoundCalcEdit
			// 
			required_TEUBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(required_TEUBoundCalcEdit, "Required_TEU");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationCalcWrapper)(null)).Required_TEU)));
			required_TEUBoundCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(required_TEUBoundCalcEdit, false);
			required_TEUBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 68, true);
			required_TEUBoundCalcEdit.Name = "required_TEUBoundCalcEdit";
			required_TEUBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			required_TEUBoundCalcEdit.TabIndex = 18;
			required_TEUBoundCalcEdit.Text = "0.00";
			required_TEUBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// available_TonnesBoundCalcEdit
			// 
			available_TonnesBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(available_TonnesBoundCalcEdit, "Available_Tonnes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationCalcWrapper)(null)).Available_Tonnes)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(available_TonnesBoundCalcEdit, false);
			available_TonnesBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 44, true);
			available_TonnesBoundCalcEdit.Name = "available_TonnesBoundCalcEdit";
			available_TonnesBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			available_TonnesBoundCalcEdit.TabIndex = 14;
			available_TonnesBoundCalcEdit.Text = "0.000";
			available_TonnesBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// allocated_TonnesBoundCalcEdit
			// 
			allocated_TonnesBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(allocated_TonnesBoundCalcEdit, "Allocated_Tonnes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationCalcWrapper)(null)).Allocated_Tonnes)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(allocated_TonnesBoundCalcEdit, false);
			allocated_TonnesBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 20, true);
			allocated_TonnesBoundCalcEdit.Name = "allocated_TonnesBoundCalcEdit";
			allocated_TonnesBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			allocated_TonnesBoundCalcEdit.TabIndex = 8;
			allocated_TonnesBoundCalcEdit.Text = "0.000";
			allocated_TonnesBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// required_TonnesBoundCalcEdit
			// 
			required_TonnesBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(required_TonnesBoundCalcEdit, "Required_Tonnes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationCalcWrapper)(null)).Required_Tonnes)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(required_TonnesBoundCalcEdit, false);
			required_TonnesBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 68, true);
			required_TonnesBoundCalcEdit.Name = "required_TonnesBoundCalcEdit";
			required_TonnesBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			required_TonnesBoundCalcEdit.TabIndex = 20;
			required_TonnesBoundCalcEdit.Text = "0.000";
			required_TonnesBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// allocated_VolumeBoundCalcEdit
			// 
			allocated_VolumeBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(allocated_VolumeBoundCalcEdit, "Allocated_Volume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationCalcWrapper)(null)).Allocated_Volume)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(allocated_VolumeBoundCalcEdit, false);
			allocated_VolumeBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 20, true);
			allocated_VolumeBoundCalcEdit.Name = "allocated_VolumeBoundCalcEdit";
			allocated_VolumeBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			allocated_VolumeBoundCalcEdit.TabIndex = 9;
			allocated_VolumeBoundCalcEdit.Text = "0.000";
			allocated_VolumeBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// required_VolumeBoundCalcEdit
			// 
			required_VolumeBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(required_VolumeBoundCalcEdit, "Required_Volume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationCalcWrapper)(null)).Required_Volume)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(required_VolumeBoundCalcEdit, false);
			required_VolumeBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 68, true);
			required_VolumeBoundCalcEdit.Name = "required_VolumeBoundCalcEdit";
			required_VolumeBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			required_VolumeBoundCalcEdit.TabIndex = 21;
			required_VolumeBoundCalcEdit.Text = "0.000";
			required_VolumeBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// available_VolumeBoundCalcEdit
			// 
			available_VolumeBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(available_VolumeBoundCalcEdit, "Available_Volume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationCalcWrapper)(null)).Available_Volume)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(available_VolumeBoundCalcEdit, false);
			available_VolumeBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 44, true);
			available_VolumeBoundCalcEdit.Name = "available_VolumeBoundCalcEdit";
			available_VolumeBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			available_VolumeBoundCalcEdit.TabIndex = 15;
			available_VolumeBoundCalcEdit.Text = "0.000";
			available_VolumeBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// refreshButton
			// 
			refreshButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ShipmentAllocationUsageControl|67015fde-6251-46a2-bf4f-07f511065051", "Refresh");
			refreshButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 64, true);
			refreshButton.Name = "refreshButton";
			refreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			refreshButton.TabIndex = 25;
			refreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
			// 
			// allocationMethodBoundTextBox
			// 
			allocationMethodBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(allocationMethodBoundTextBox, "AllocationMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AllocationCalcWrapper)(null)).AllocationMethod)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(allocationMethodBoundTextBox, false);
			allocationMethodBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(482, 20, true);
			allocationMethodBoundTextBox.Name = "allocationMethodBoundTextBox";
			allocationMethodBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			allocationMethodBoundTextBox.TabIndex = 24;
			// 
			// required_PowerPointsCalcEdit
			// 
			required_PowerPointsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(required_PowerPointsCalcEdit, "Required_PowerPoints");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationCalcWrapper)(null)).Required_PowerPoints)));
			required_PowerPointsCalcEdit.DecimalPlaces = 0;
			required_PowerPointsCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(required_PowerPointsCalcEdit, false);
			required_PowerPointsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 68, true);
			required_PowerPointsCalcEdit.Name = "required_PowerPointsCalcEdit";
			required_PowerPointsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			required_PowerPointsCalcEdit.TabIndex = 19;
			required_PowerPointsCalcEdit.Text = "0";
			required_PowerPointsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// allocated_PowerPointsCalcEdit
			// 
			allocated_PowerPointsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(allocated_PowerPointsCalcEdit, "Allocated_PowerPoints");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationCalcWrapper)(null)).Allocated_PowerPoints)));
			allocated_PowerPointsCalcEdit.DecimalPlaces = 0;
			allocated_PowerPointsCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(allocated_PowerPointsCalcEdit, false);
			allocated_PowerPointsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 20, true);
			allocated_PowerPointsCalcEdit.Name = "allocated_PowerPointsCalcEdit";
			allocated_PowerPointsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			allocated_PowerPointsCalcEdit.TabIndex = 7;
			allocated_PowerPointsCalcEdit.Text = "0";
			allocated_PowerPointsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// available_PowerPointsCalcEdit
			// 
			available_PowerPointsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(available_PowerPointsCalcEdit, "Available_PowerPoints");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationCalcWrapper)(null)).Available_PowerPoints)));
			available_PowerPointsCalcEdit.DecimalPlaces = 0;
			available_PowerPointsCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(available_PowerPointsCalcEdit, false);
			available_PowerPointsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 44, true);
			available_PowerPointsCalcEdit.Name = "available_PowerPointsCalcEdit";
			available_PowerPointsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			available_PowerPointsCalcEdit.TabIndex = 13;
			available_PowerPointsCalcEdit.Text = "0";
			available_PowerPointsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// availableLabel
			// 
			availableLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ShipmentAllocationUsageControl|258ffe37-3aca-41a9-a9bf-22024e0a7075", "Available:");
			availableLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 44, true);
			availableLabel.Name = "availableLabel";
			availableLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			availableLabel.TabIndex = 11;
			// 
			// teuLabel
			// 
			teuLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ShipmentAllocationUsageControl|99af24ce-44c2-4b23-80d5-a25d0bde6532", "TEUs");
			teuLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 2, true);
			teuLabel.Name = "teuLabel";
			teuLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 16, true);
			teuLabel.TabIndex = 0;
			teuLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// powerPointsLabel
			// 
			powerPointsLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ShipmentAllocationUsageControl|e2b3e80d-6339-4b16-8f22-1e474fdd4aa6", "Power");
			powerPointsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 2, true);
			powerPointsLabel.Name = "powerPointsLabel";
			powerPointsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 16, true);
			powerPointsLabel.TabIndex = 1;
			powerPointsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// weightLabel
			// 
			weightLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ShipmentAllocationUsageControl|759c4e98-cd64-4cce-87a3-1f94d3ae69d8", "Tonnes");
			weightLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 2, true);
			weightLabel.Name = "weightLabel";
			weightLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 16, true);
			weightLabel.TabIndex = 2;
			weightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// volumeLabel
			// 
			volumeLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ShipmentAllocationUsageControl|00cbb449-6724-4de7-ae54-6dd1695463f9", "Volume (M3)");
			volumeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(310, 2, true);
			volumeLabel.Name = "volumeLabel";
			volumeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 16, true);
			volumeLabel.TabIndex = 3;
			volumeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// allocatedLabel
			// 
			allocatedLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ShipmentAllocationUsageControl|d89cc24f-295d-4e07-ac2c-d62fff303922", "Over Alloc.:");
			allocatedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 20, true);
			allocatedLabel.Name = "allocatedLabel";
			allocatedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			allocatedLabel.TabIndex = 5;
			// 
			// requiredLabel
			// 
			requiredLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ShipmentAllocationUsageControl|251e3892-bc72-4401-a05b-3724bc49250c", "Required:");
			requiredLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 68, true);
			requiredLabel.Name = "requiredLabel";
			requiredLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			requiredLabel.TabIndex = 17;
			// 
			// methodLabel
			// 
			methodLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ShipmentAllocationUsageControl|cfbcf0c4-700a-4977-bd37-f94411711641", "Allocation Method");
			methodLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(472, 2, true);
			methodLabel.Name = "methodLabel";
			methodLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 16, true);
			methodLabel.TabIndex = 23;
			// 
			// available_AreaBoundCalcEdit
			// 
			available_AreaBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(available_AreaBoundCalcEdit, "Available_Area");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationCalcWrapper)(null)).Available_Area)));
			available_AreaBoundCalcEdit.DecimalPlaces = 3;
			available_AreaBoundCalcEdit.Decimals = 3;
			available_AreaBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(394, 44, true);
			available_AreaBoundCalcEdit.Name = "available_AreaBoundCalcEdit";
			available_AreaBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			available_AreaBoundCalcEdit.TabIndex = 16;
			available_AreaBoundCalcEdit.Text = "0.000";
			available_AreaBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// required_AreaBoundCalcEdit
			// 
			required_AreaBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(required_AreaBoundCalcEdit, "Required_Area");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationCalcWrapper)(null)).Required_Area)));
			required_AreaBoundCalcEdit.DecimalPlaces = 3;
			required_AreaBoundCalcEdit.Decimals = 3;
			required_AreaBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(394, 68, true);
			required_AreaBoundCalcEdit.Name = "required_AreaBoundCalcEdit";
			required_AreaBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			required_AreaBoundCalcEdit.TabIndex = 22;
			required_AreaBoundCalcEdit.Text = "0.000";
			required_AreaBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// allocated_AreaBoundCalcEdit
			// 
			allocated_AreaBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(allocated_AreaBoundCalcEdit, "Allocated_Area");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AllocationCalcWrapper)(null)).Allocated_Area)));
			allocated_AreaBoundCalcEdit.DecimalPlaces = 3;
			allocated_AreaBoundCalcEdit.Decimals = 3;
			allocated_AreaBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(394, 20, true);
			allocated_AreaBoundCalcEdit.Name = "allocated_AreaBoundCalcEdit";
			allocated_AreaBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			allocated_AreaBoundCalcEdit.TabIndex = 10;
			allocated_AreaBoundCalcEdit.Text = "0.000";
			allocated_AreaBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// areaLabel
			// 
			areaLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ShipmentAllocationUsageControl|4c42baf6-0032-414a-afd1-8dfeddaa2cbf", "Area (M2)");
			areaLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(392, 2, true);
			areaLabel.Name = "areaLabel";
			areaLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 16, true);
			areaLabel.TabIndex = 4;
			areaLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ShipmentAllocationUsageControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(availableLabel);
			this.Controls.Add(teuLabel);
			this.Controls.Add(powerPointsLabel);
			this.Controls.Add(weightLabel);
			this.Controls.Add(areaLabel);
			this.Controls.Add(volumeLabel);
			this.Controls.Add(allocatedLabel);
			this.Controls.Add(requiredLabel);
			this.Controls.Add(methodLabel);
			this.Controls.Add(refreshButton);
			this.Controls.Add(allocated_teuBoundCalcEdit);
			this.Controls.Add(available_TEUBoundCalcEdit);
			this.Controls.Add(required_TEUBoundCalcEdit);
			this.Controls.Add(available_PowerPointsCalcEdit);
			this.Controls.Add(allocated_PowerPointsCalcEdit);
			this.Controls.Add(available_TonnesBoundCalcEdit);
			this.Controls.Add(required_PowerPointsCalcEdit);
			this.Controls.Add(allocated_TonnesBoundCalcEdit);
			this.Controls.Add(allocated_AreaBoundCalcEdit);
			this.Controls.Add(required_TonnesBoundCalcEdit);
			this.Controls.Add(required_AreaBoundCalcEdit);
			this.Controls.Add(allocated_VolumeBoundCalcEdit);
			this.Controls.Add(available_AreaBoundCalcEdit);
			this.Controls.Add(required_VolumeBoundCalcEdit);
			this.Controls.Add(available_VolumeBoundCalcEdit);
			this.Controls.Add(allocationMethodBoundTextBox);
			this.Name = "ShipmentAllocationUsageControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(571, 93, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		Enterprise.ZArchitecture.ZCalcEdit allocated_teuBoundCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit available_TEUBoundCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit required_TEUBoundCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit available_TonnesBoundCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit allocated_TonnesBoundCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit required_TonnesBoundCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit allocated_VolumeBoundCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit required_VolumeBoundCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit available_VolumeBoundCalcEdit;
		Enterprise.ZArchitecture.GUI.ZButton refreshButton;
		Enterprise.ZArchitecture.ZTextBox allocationMethodBoundTextBox;
		Enterprise.ZArchitecture.ZCalcEdit required_PowerPointsCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit allocated_PowerPointsCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit available_PowerPointsCalcEdit;
		Enterprise.ZArchitecture.ZLabel availableLabel;
		Enterprise.ZArchitecture.ZLabel teuLabel;
		Enterprise.ZArchitecture.ZLabel powerPointsLabel;
		Enterprise.ZArchitecture.ZLabel weightLabel;
		Enterprise.ZArchitecture.ZLabel volumeLabel;
		Enterprise.ZArchitecture.ZLabel allocatedLabel;
		Enterprise.ZArchitecture.ZLabel requiredLabel;
		Enterprise.ZArchitecture.ZLabel methodLabel;
		Enterprise.ZArchitecture.ZCalcEdit available_AreaBoundCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit required_AreaBoundCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit allocated_AreaBoundCalcEdit;
		Enterprise.ZArchitecture.ZLabel areaLabel;
	}
}
