namespace Enterprise.Freight.Agency.GUI
{
	partial class CountryLevelAllocations
	{
		private void InitializeComponent()
		{
			overAllocPercentLabel = new Enterprise.ZArchitecture.ZLabel();
			overallocationPercentBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			overallocatedTonnesBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			usedTonnesBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			tonnesBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			overrideOverallocationPercentBoundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			percentLabel = new Enterprise.ZArchitecture.ZLabel();
			tonnes_Label = new Enterprise.ZArchitecture.ZLabel();
			total_Label = new Enterprise.ZArchitecture.ZLabel();
			overallocatedVolumeBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			total_TEUBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			usedVolumeBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			volumeBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			volumeLabel = new Enterprise.ZArchitecture.ZLabel();
			allocationLabel = new Enterprise.ZArchitecture.ZLabel();
			overAllocationLabel = new Enterprise.ZArchitecture.ZLabel();
			usedLabel = new Enterprise.ZArchitecture.ZLabel();
			powerPointsLabel = new Enterprise.ZArchitecture.ZLabel();
			powerPointsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			usedPowerPointsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			overallocatedPowerPointsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			overallocatedTEUBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			usedTEUBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			overallocatedAreaBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			usedAreaBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			areaBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			area_Label = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.AgencyPrincipal);
			// 
			// overAllocPercentLabel
			// 
			overAllocPercentLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CountryLevelAllocations|05a88232-7949-44a1-8721-24647d5baca7", "Over Alloc.");
			overAllocPercentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 24, true);
			overAllocPercentLabel.Name = "overAllocPercentLabel";
			overAllocPercentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 23, true);
			overAllocPercentLabel.TabIndex = 23;
			// 
			// overallocationPercentBoundCalcEdit
			// 
			overallocationPercentBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(overallocationPercentBoundCalcEdit, "OverallocationPercent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).OverallocationPercent)));
			overallocationPercentBoundCalcEdit.DecimalPlaces = 0;
			overallocationPercentBoundCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(overallocationPercentBoundCalcEdit, false);
			overallocationPercentBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(526, 47, true);
			overallocationPercentBoundCalcEdit.Name = "overallocationPercentBoundCalcEdit";
			overallocationPercentBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			overallocationPercentBoundCalcEdit.TabIndex = 25;
			overallocationPercentBoundCalcEdit.Text = "0";
			overallocationPercentBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// overallocatedTonnesBoundCalcEdit
			// 
			overallocatedTonnesBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(overallocatedTonnesBoundCalcEdit, "OverallocatedTonnes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).OverallocatedTonnes)));
			overallocatedTonnesBoundCalcEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CountryLevelAllocations|007c5f45-4245-465e-a439-7b1e72d4a803", "Over. Tonnes", "Over Allocation Tonnes", "The upper limit on the weight in tonnes that may be booked at any point within this country.");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(overallocatedTonnesBoundCalcEdit, false);
			overallocatedTonnesBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 48, true);
			overallocatedTonnesBoundCalcEdit.Name = "overallocatedTonnesBoundCalcEdit";
			overallocatedTonnesBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			overallocatedTonnesBoundCalcEdit.TabIndex = 15;
			overallocatedTonnesBoundCalcEdit.Text = "0.000";
			overallocatedTonnesBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// usedTonnesBoundCalcEdit
			// 
			usedTonnesBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(usedTonnesBoundCalcEdit, "UsedTonnes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).UsedTonnes)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(usedTonnesBoundCalcEdit, false);
			usedTonnesBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 72, true);
			usedTonnesBoundCalcEdit.Name = "usedTonnesBoundCalcEdit";
			usedTonnesBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			usedTonnesBoundCalcEdit.TabIndex = 21;
			usedTonnesBoundCalcEdit.Text = "0.000";
			usedTonnesBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// tonnesBoundCalcEdit
			// 
			tonnesBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(tonnesBoundCalcEdit, "Tonnes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Tonnes)));
			tonnesBoundCalcEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CountryLevelAllocations|66ea3fb2-9620-488c-8002-28cf753c5036", "Tonnes", "Allocated Tonnes", "The total weight in tonnes you have been allocated for use at any point within this country.");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(tonnesBoundCalcEdit, false);
			tonnesBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 24, true);
			tonnesBoundCalcEdit.Name = "tonnesBoundCalcEdit";
			tonnesBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			tonnesBoundCalcEdit.TabIndex = 9;
			tonnesBoundCalcEdit.Text = "0";
			tonnesBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// overrideOverallocationPercentBoundCheckBox
			// 
			this.BindingSource.SetBindingMember(overrideOverallocationPercentBoundCheckBox, "OverrideOverallocationPercent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).OverrideOverallocationPercent)));
			overrideOverallocationPercentBoundCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(overrideOverallocationPercentBoundCheckBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(overrideOverallocationPercentBoundCheckBox, false);
			overrideOverallocationPercentBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(502, 47, true);
			overrideOverallocationPercentBoundCheckBox.Name = "overrideOverallocationPercentBoundCheckBox";
			overrideOverallocationPercentBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 24, true);
			overrideOverallocationPercentBoundCheckBox.TabIndex = 24;
			// 
			// percentLabel
			// 
			percentLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CountryLevelAllocations|c2e79865-d266-47a5-9f88-3d40f4aa0d79", "%");
			percentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(558, 47, true);
			percentLabel.Name = "percentLabel";
			percentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 20, true);
			percentLabel.TabIndex = 26;
			// 
			// tonnes_Label
			// 
			tonnes_Label.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CountryLevelAllocations|9bb43a52-7746-4777-8fbb-2264c20bde29", "Tonnes");
			tonnes_Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 0, true);
			tonnes_Label.Name = "tonnes_Label";
			tonnes_Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			tonnes_Label.TabIndex = 3;
			// 
			// total_Label
			// 
			total_Label.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CountryLevelAllocations|07f7bce5-6667-4507-a807-1c5fe3699ce6", "Total TEU");
			total_Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 0, true);
			total_Label.Name = "total_Label";
			total_Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			total_Label.TabIndex = 0;
			// 
			// overallocatedVolumeBoundCalcEdit
			// 
			overallocatedVolumeBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(overallocatedVolumeBoundCalcEdit, "OverallocatedVolume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).OverallocatedVolume)));
			overallocatedVolumeBoundCalcEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CountryLevelAllocations|72603279-bb0c-49cb-a9d4-2f55726f28bf", "Over Allocation Volume", "The upper limit on the volume in cubic meters that may be booked at any point within this country.\r\n\r\nContainerized shipments count towards the TEUs instead of volume.");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(overallocatedVolumeBoundCalcEdit, false);
			overallocatedVolumeBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 48, true);
			overallocatedVolumeBoundCalcEdit.Name = "overallocatedVolumeBoundCalcEdit";
			overallocatedVolumeBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			overallocatedVolumeBoundCalcEdit.TabIndex = 14;
			overallocatedVolumeBoundCalcEdit.Text = "0.000";
			overallocatedVolumeBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// total_TEUBoundCalcEdit
			// 
			total_TEUBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(total_TEUBoundCalcEdit, "TEU");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).TEU)));
			total_TEUBoundCalcEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CountryLevelAllocations|d69c3892-b23f-4201-a410-e89cf8be907e", "TEUs", "Total Allocated TEUs", "The total number of TEUs you have been allocated for use at any point within this country.");
			total_TEUBoundCalcEdit.DecimalPlaces = 0;
			total_TEUBoundCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(total_TEUBoundCalcEdit, false);
			total_TEUBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 24, true);
			total_TEUBoundCalcEdit.Name = "total_TEUBoundCalcEdit";
			total_TEUBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			total_TEUBoundCalcEdit.TabIndex = 6;
			total_TEUBoundCalcEdit.Text = "0";
			total_TEUBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// usedVolumeBoundCalcEdit
			// 
			usedVolumeBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(usedVolumeBoundCalcEdit, "UsedVolume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).UsedVolume)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(usedVolumeBoundCalcEdit, false);
			usedVolumeBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 72, true);
			usedVolumeBoundCalcEdit.Name = "usedVolumeBoundCalcEdit";
			usedVolumeBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			usedVolumeBoundCalcEdit.TabIndex = 20;
			usedVolumeBoundCalcEdit.Text = "0.000";
			usedVolumeBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// volumeBoundCalcEdit
			// 
			volumeBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(volumeBoundCalcEdit, "Volume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Volume)));
			volumeBoundCalcEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CountryLevelAllocations|eebf7ed4-d344-46d4-83f9-1b9556a27b61", "Volume", "Allocated Volume", "The total volume of space you have been allocated in cubic meters for use at any point within this country.\r\n\r\nContainerized shipments count towards the TEUs instead of volume.");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(volumeBoundCalcEdit, false);
			volumeBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 24, true);
			volumeBoundCalcEdit.Name = "volumeBoundCalcEdit";
			volumeBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			volumeBoundCalcEdit.TabIndex = 8;
			volumeBoundCalcEdit.Text = "0";
			volumeBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// volumeLabel
			// 
			volumeLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CountryLevelAllocations|dba0f665-40f4-4b44-8c2f-7615a958a84a", "Volume (M3)");
			volumeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 0, true);
			volumeLabel.Name = "volumeLabel";
			volumeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			volumeLabel.TabIndex = 2;
			// 
			// allocationLabel
			// 
			allocationLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CountryLevelAllocations|8a4d0e69-13b8-4bb7-9c25-11fddb3c98c2", "Allocation");
			allocationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			allocationLabel.Name = "allocationLabel";
			allocationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			allocationLabel.TabIndex = 5;
			allocationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// overAllocationLabel
			// 
			overAllocationLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CountryLevelAllocations|2b475703-b75c-4961-a4c2-fc507cef9cdf", "Over Allocation");
			overAllocationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 48, true);
			overAllocationLabel.Name = "overAllocationLabel";
			overAllocationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			overAllocationLabel.TabIndex = 11;
			overAllocationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// usedLabel
			// 
			usedLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CountryLevelAllocations|be758e8c-4b0b-4a87-adf8-d26772a9c5f8", "Used");
			usedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 72, true);
			usedLabel.Name = "usedLabel";
			usedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			usedLabel.TabIndex = 17;
			usedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// powerPointsLabel
			// 
			powerPointsLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CountryLevelAllocations|ea156d09-a549-4ef4-952c-60265e2c42d5", "Power Points");
			powerPointsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 0, true);
			powerPointsLabel.Name = "powerPointsLabel";
			powerPointsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			powerPointsLabel.TabIndex = 1;
			// 
			// powerPointsCalcEdit
			// 
			powerPointsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(powerPointsCalcEdit, "PowerPoints");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).PowerPoints)));
			powerPointsCalcEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CountryLevelAllocations|263c700f-f64e-443d-9354-70c078b4a865", "Power", "Power Points", "Allocated Power Points", "The total number of power points you have been allocated for use at any point within this country.");
			powerPointsCalcEdit.DecimalPlaces = 0;
			powerPointsCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(powerPointsCalcEdit, false);
			powerPointsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 24, true);
			powerPointsCalcEdit.Name = "powerPointsCalcEdit";
			powerPointsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			powerPointsCalcEdit.TabIndex = 7;
			powerPointsCalcEdit.Text = "0";
			powerPointsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// usedPowerPointsCalcEdit
			// 
			usedPowerPointsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(usedPowerPointsCalcEdit, "UsedPowerPoints");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).UsedPowerPoints)));
			usedPowerPointsCalcEdit.DecimalPlaces = 0;
			usedPowerPointsCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(usedPowerPointsCalcEdit, false);
			usedPowerPointsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 72, true);
			usedPowerPointsCalcEdit.Name = "usedPowerPointsCalcEdit";
			usedPowerPointsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			usedPowerPointsCalcEdit.TabIndex = 19;
			usedPowerPointsCalcEdit.Text = "0";
			usedPowerPointsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// overallocatedPowerPointsCalcEdit
			// 
			overallocatedPowerPointsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(overallocatedPowerPointsCalcEdit, "OverallocatedPowerPoints");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).OverallocatedPowerPoints)));
			overallocatedPowerPointsCalcEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CountryLevelAllocations|67e0c62e-2993-481a-928d-69c59ff67423", "Over Allocation Power Points", "The upper limit on the power points in tonnes that may be booked at any point within this country.");
			overallocatedPowerPointsCalcEdit.DecimalPlaces = 0;
			overallocatedPowerPointsCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(overallocatedPowerPointsCalcEdit, false);
			overallocatedPowerPointsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 48, true);
			overallocatedPowerPointsCalcEdit.Name = "overallocatedPowerPointsCalcEdit";
			overallocatedPowerPointsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			overallocatedPowerPointsCalcEdit.TabIndex = 13;
			overallocatedPowerPointsCalcEdit.Text = "0";
			overallocatedPowerPointsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// overallocatedTEUBoundCalcEdit
			// 
			overallocatedTEUBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(overallocatedTEUBoundCalcEdit, "OverallocatedTEU");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).OverallocatedTEU)));
			overallocatedTEUBoundCalcEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CountryLevelAllocations|30a66d73-8d8a-46a3-a38f-57dfe8a0c6b9", "Over Allocation TEUs", "The upper limit of the TEUs that may be booked at any point within this country.");
			overallocatedTEUBoundCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(overallocatedTEUBoundCalcEdit, false);
			overallocatedTEUBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 48, true);
			overallocatedTEUBoundCalcEdit.Name = "overallocatedTEUBoundCalcEdit";
			overallocatedTEUBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			overallocatedTEUBoundCalcEdit.TabIndex = 12;
			overallocatedTEUBoundCalcEdit.Text = "0.00";
			overallocatedTEUBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// usedTEUBoundCalcEdit
			// 
			usedTEUBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(usedTEUBoundCalcEdit, "UsedTEU");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).UsedTEU)));
			usedTEUBoundCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(usedTEUBoundCalcEdit, false);
			usedTEUBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 72, true);
			usedTEUBoundCalcEdit.Name = "usedTEUBoundCalcEdit";
			usedTEUBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			usedTEUBoundCalcEdit.TabIndex = 18;
			usedTEUBoundCalcEdit.Text = "0.00";
			usedTEUBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// overallocatedAreaBoundCalcEdit
			// 
			overallocatedAreaBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(overallocatedAreaBoundCalcEdit, "OverallocatedArea");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).OverallocatedArea)));
			overallocatedAreaBoundCalcEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CountryLevelAllocations|ed8a4f64-8671-4b45-80b6-ee01a853b88b", "Over. Area", "Over Allocation Area", "The upper limit on the floor space  in square meters that may be booked at any point within this country.\r\n\r\nOnly break-bulk and roll-on roll-off count towards area.");
			overallocatedAreaBoundCalcEdit.DecimalPlaces = 3;
			overallocatedAreaBoundCalcEdit.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(overallocatedAreaBoundCalcEdit, false);
			overallocatedAreaBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 48, true);
			overallocatedAreaBoundCalcEdit.Name = "overallocatedAreaBoundCalcEdit";
			overallocatedAreaBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			overallocatedAreaBoundCalcEdit.TabIndex = 16;
			overallocatedAreaBoundCalcEdit.Text = "0.000";
			overallocatedAreaBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// usedAreaBoundCalcEdit
			// 
			usedAreaBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(usedAreaBoundCalcEdit, "UsedArea");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).UsedArea)));
			usedAreaBoundCalcEdit.DecimalPlaces = 3;
			usedAreaBoundCalcEdit.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(usedAreaBoundCalcEdit, false);
			usedAreaBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 72, true);
			usedAreaBoundCalcEdit.Name = "usedAreaBoundCalcEdit";
			usedAreaBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			usedAreaBoundCalcEdit.TabIndex = 22;
			usedAreaBoundCalcEdit.Text = "0.000";
			usedAreaBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// areaBoundCalcEdit
			// 
			areaBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(areaBoundCalcEdit, "Area");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Area)));
			areaBoundCalcEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CountryLevelAllocations|5eded76d-ce32-4656-9c03-9dd99ef8716c", "Area", "Allocated Area", "The total floor space in square meters you have been allocated for use at any point within this country.\r\n\r\nOnly break-bulk and roll-on roll-off count towards area.");
			areaBoundCalcEdit.DecimalPlaces = 0;
			areaBoundCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(areaBoundCalcEdit, false);
			areaBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 24, true);
			areaBoundCalcEdit.Name = "areaBoundCalcEdit";
			areaBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			areaBoundCalcEdit.TabIndex = 10;
			areaBoundCalcEdit.Text = "0";
			areaBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// area_Label
			// 
			area_Label.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CountryLevelAllocations|f298122a-e8f0-4e56-a657-1c3371089d65", "Area (M2)");
			area_Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 0, true);
			area_Label.Name = "area_Label";
			area_Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			area_Label.TabIndex = 4;
			// 
			// CountryLevelAllocations
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(overallocatedAreaBoundCalcEdit);
			this.Controls.Add(usedAreaBoundCalcEdit);
			this.Controls.Add(areaBoundCalcEdit);
			this.Controls.Add(area_Label);
			this.Controls.Add(overAllocPercentLabel);
			this.Controls.Add(overallocationPercentBoundCalcEdit);
			this.Controls.Add(overallocatedTonnesBoundCalcEdit);
			this.Controls.Add(usedTonnesBoundCalcEdit);
			this.Controls.Add(tonnesBoundCalcEdit);
			this.Controls.Add(overrideOverallocationPercentBoundCheckBox);
			this.Controls.Add(percentLabel);
			this.Controls.Add(tonnes_Label);
			this.Controls.Add(usedLabel);
			this.Controls.Add(overAllocationLabel);
			this.Controls.Add(allocationLabel);
			this.Controls.Add(total_Label);
			this.Controls.Add(overallocatedTEUBoundCalcEdit);
			this.Controls.Add(usedTEUBoundCalcEdit);
			this.Controls.Add(overallocatedPowerPointsCalcEdit);
			this.Controls.Add(overallocatedVolumeBoundCalcEdit);
			this.Controls.Add(total_TEUBoundCalcEdit);
			this.Controls.Add(usedPowerPointsCalcEdit);
			this.Controls.Add(usedVolumeBoundCalcEdit);
			this.Controls.Add(powerPointsCalcEdit);
			this.Controls.Add(powerPointsLabel);
			this.Controls.Add(volumeBoundCalcEdit);
			this.Controls.Add(volumeLabel);
			this.Name = "CountryLevelAllocations";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(588, 100, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		Enterprise.ZArchitecture.ZLabel overAllocPercentLabel;
		Enterprise.ZArchitecture.ZCalcEdit overallocationPercentBoundCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit overallocatedTonnesBoundCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit usedTonnesBoundCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit tonnesBoundCalcEdit;
		Enterprise.ZArchitecture.GUI.ZCheckBox overrideOverallocationPercentBoundCheckBox;
		Enterprise.ZArchitecture.ZLabel percentLabel;
		Enterprise.ZArchitecture.ZLabel tonnes_Label;
		Enterprise.ZArchitecture.ZLabel total_Label;
		Enterprise.ZArchitecture.ZCalcEdit overallocatedVolumeBoundCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit total_TEUBoundCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit usedVolumeBoundCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit volumeBoundCalcEdit;
		Enterprise.ZArchitecture.ZLabel volumeLabel;
		Enterprise.ZArchitecture.ZLabel allocationLabel;
		Enterprise.ZArchitecture.ZLabel overAllocationLabel;
		Enterprise.ZArchitecture.ZLabel usedLabel;
		Enterprise.ZArchitecture.ZLabel powerPointsLabel;
		Enterprise.ZArchitecture.ZCalcEdit powerPointsCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit usedPowerPointsCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit overallocatedPowerPointsCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit overallocatedTEUBoundCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit usedTEUBoundCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit overallocatedAreaBoundCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit usedAreaBoundCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit areaBoundCalcEdit;
		Enterprise.ZArchitecture.ZLabel area_Label;
	}
}
