namespace Enterprise.Customs.GUI
{
	public partial class PickupDetailsUserControl
	{
		void InitializeComponent()
		{
			this.LCLDatesOverrideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DurationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.JE_PickupTruckWaitTimeEdit = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.JE_PickupLabourTimeEdit = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.JE_PickupLabourChargeBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JE_FCLPickupEquipmentNeededBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FCL_AvailableDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.FCL_StorageDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JE_PickupRequiredByBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JE_PickupCartageAdvisedBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JE_PickupCartageCompletedBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JE_EstimatedDeliveryOrPickupDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ChargeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.JE_DeliveryTruckWaitChargeBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JP_LCLAirStorageChargeBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JP_LCLAirStorageDaysOrHoursBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JP_LCLAvailableDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JP_LCLStorageCommencesDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PickupRequiredFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.JE_FCLPickupEquipmentNeededBoundDropEdit.SuspendLayout();
			this.FCL_AvailableDateEdit.SuspendLayout();
			this.FCL_StorageDateEdit.SuspendLayout();
			this.JE_PickupRequiredByBoundDateEdit.SuspendLayout();
			this.JE_PickupCartageAdvisedBoundDateEdit.SuspendLayout();
			this.JE_PickupCartageCompletedBoundDateEdit.SuspendLayout();
			this.JE_EstimatedDeliveryOrPickupDateEdit.SuspendLayout();
			this.JP_LCLAvailableDateEdit.SuspendLayout();
			this.JP_LCLStorageCommencesDateEdit.SuspendLayout();
			this.PickupRequiredFromDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// LCLDatesOverrideCheckBox
			// 
			this.LCLDatesOverrideCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.LCLDatesOverrideCheckBox, "DocsAndCartage+JP_LCLDatesOverrideConsol");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DocsAndCartage.JP_LCLDatesOverrideConsol)));
			this.LCLDatesOverrideCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LCLDatesOverrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 26, true);
			this.LCLDatesOverrideCheckBox.Name = "LCLDatesOverrideCheckBox";
			this.LCLDatesOverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 17, true);
			this.LCLDatesOverrideCheckBox.TabIndex = 3;
			this.LCLDatesOverrideCheckBox.UseVisualStyleBackColor = true;
			// 
			// DurationLabel
			// 
			this.DurationLabel.AutoSize = true;
			this.DurationLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("ae2632b0-4266-42d8-a1ba-f8acd4dc108f", "Duration");
			this.DurationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.DurationLabel.IsFontBold = true;
			this.DurationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 135, true);
			this.DurationLabel.Name = "DurationLabel";
			this.DurationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 13, true);
			this.DurationLabel.TabIndex = 12;
			// 
			// JE_PickupTruckWaitTimeEdit
			// 
			this.JE_PickupTruckWaitTimeEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JE_PickupTruckWaitTimeEdit, "JE_PickupOrDeliveryTruckWaitTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_PickupOrDeliveryTruckWaitTime)));
			this.JE_PickupTruckWaitTimeEdit.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JE_PickupTruckWaitTimeEdit, false);
			this.JE_PickupTruckWaitTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 173, true);
			this.JE_PickupTruckWaitTimeEdit.Name = "JE_PickupTruckWaitTimeEdit";
			this.JE_PickupTruckWaitTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.JE_PickupTruckWaitTimeEdit.TabIndex = 16;
			// 
			// JE_PickupLabourTimeEdit
			// 
			this.JE_PickupLabourTimeEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JE_PickupLabourTimeEdit, "JE_DeliveryOrPickupLabourTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_DeliveryOrPickupLabourTime)));
			this.JE_PickupLabourTimeEdit.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JE_PickupLabourTimeEdit, false);
			this.JE_PickupLabourTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 151, true);
			this.JE_PickupLabourTimeEdit.Name = "JE_PickupLabourTimeEdit";
			this.JE_PickupLabourTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.JE_PickupLabourTimeEdit.TabIndex = 14;
			// 
			// JE_PickupLabourChargeBoundCalcEdit
			// 
			this.JE_PickupLabourChargeBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JE_PickupLabourChargeBoundCalcEdit, "JE_DeliveryOrPickupLabourCharge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_DeliveryOrPickupLabourCharge)));
			this.JE_PickupLabourChargeBoundCalcEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("594307cd-8892-44b6-bf86-cb44264fe789", "Pickup Labor");
			this.JE_PickupLabourChargeBoundCalcEdit.DecimalPlaces = 2;
			this.JE_PickupLabourChargeBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 151, true);
			this.JE_PickupLabourChargeBoundCalcEdit.Name = "JE_PickupLabourChargeBoundCalcEdit";
			this.JE_PickupLabourChargeBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.JE_PickupLabourChargeBoundCalcEdit.TabIndex = 13;
			this.JE_PickupLabourChargeBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JE_FCLPickupEquipmentNeededBoundDropEdit
			// 
			this.JE_FCLPickupEquipmentNeededBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_FCLPickupEquipmentNeededBoundDropEdit, "JE_FCLDeliveryOrPickupEquipmentNeeded");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_FCLDeliveryOrPickupEquipmentNeeded)));
			this.JE_FCLPickupEquipmentNeededBoundDropEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("e0ef33d2-ab99-4ffc-a492-1ba3a59808eb", "Drop Mode");
			this.JE_FCLPickupEquipmentNeededBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 24, true);
			this.JE_FCLPickupEquipmentNeededBoundDropEdit.Name = "JE_FCLPickupEquipmentNeededBoundDropEdit";
			this.JE_FCLPickupEquipmentNeededBoundDropEdit.PreBoundMaxLength = 3;
			this.JE_FCLPickupEquipmentNeededBoundDropEdit.ShouldResizeByMaxLength = true;
			this.JE_FCLPickupEquipmentNeededBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.JE_FCLPickupEquipmentNeededBoundDropEdit.TabIndex = 2;
			// 
			// FCL_AvailableDateEdit
			// 
			this.FCL_AvailableDateEdit.AllowDrop = true;
			this.FCL_AvailableDateEdit.AutoCompleteMonthThreshold = 1;
			this.FCL_AvailableDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FCL_AvailableDateEdit, "DocsAndCartage+JP_FCLAvailable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DocsAndCartage.JP_FCLAvailable)));
			this.FCL_AvailableDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.FCL_AvailableDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 2, true);
			this.FCL_AvailableDateEdit.Name = "FCL_AvailableDateEdit";
			this.FCL_AvailableDateEdit.TabIndex = 0;
			// 
			// FCL_StorageDateEdit
			// 
			this.FCL_StorageDateEdit.AllowDrop = true;
			this.FCL_StorageDateEdit.AutoCompleteMonthThreshold = 1;
			this.FCL_StorageDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FCL_StorageDateEdit, "DocsAndCartage+JP_FCLStorageCommences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DocsAndCartage.JP_FCLStorageCommences)));
			this.FCL_StorageDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.FCL_StorageDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 2, true);
			this.FCL_StorageDateEdit.Name = "FCL_StorageDateEdit";
			this.FCL_StorageDateEdit.TabIndex = 1;
			// 
			// JE_PickupRequiredByBoundDateEdit
			// 
			this.JE_PickupRequiredByBoundDateEdit.AllowDrop = true;
			this.JE_PickupRequiredByBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JE_PickupRequiredByBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JE_PickupRequiredByBoundDateEdit, "JE_DeliveryOrPickupRequiredBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_DeliveryOrPickupRequiredBy)));
			this.JE_PickupRequiredByBoundDateEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("301e8eaf-574d-4ac7-987e-72ef0620174a", "Pickup Required By", "Pickup Required By", "The date/time the goods are required to be picked up from the origin by the carrier. Generally this is the latest time the goods can be picked up.");
			this.JE_PickupRequiredByBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JE_PickupRequiredByBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 90, true);
			this.JE_PickupRequiredByBoundDateEdit.Name = "JE_PickupRequiredByBoundDateEdit";
			this.JE_PickupRequiredByBoundDateEdit.TabIndex = 8;
			// 
			// JE_PickupCartageAdvisedBoundDateEdit
			// 
			this.JE_PickupCartageAdvisedBoundDateEdit.AllowDrop = true;
			this.JE_PickupCartageAdvisedBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JE_PickupCartageAdvisedBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JE_PickupCartageAdvisedBoundDateEdit, "JP_Calc_CartageAdvised");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JP_Calc_CartageAdvised)));
			this.JE_PickupCartageAdvisedBoundDateEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("05588efd-e779-4d7b-941a-37007320ed04", "Trn. Booking Requested");
			this.JE_PickupCartageAdvisedBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JE_PickupCartageAdvisedBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 112, true);
			this.JE_PickupCartageAdvisedBoundDateEdit.Name = "JE_PickupCartageAdvisedBoundDateEdit";
			this.JE_PickupCartageAdvisedBoundDateEdit.TabIndex = 10;
			// 
			// JE_PickupCartageCompletedBoundDateEdit
			// 
			this.JE_PickupCartageCompletedBoundDateEdit.AllowDrop = true;
			this.JE_PickupCartageCompletedBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JE_PickupCartageCompletedBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JE_PickupCartageCompletedBoundDateEdit, "JE_CartageCompleted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_CartageCompleted)));
			this.JE_PickupCartageCompletedBoundDateEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("bd2c5c84-08e2-469a-9e25-6feb13a103ce", "Goods Pickup", "Actual Pickup", "The actual date/time the goods have been picked up from the origin by the carrier.");
			this.JE_PickupCartageCompletedBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JE_PickupCartageCompletedBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 90, true);
			this.JE_PickupCartageCompletedBoundDateEdit.Name = "JE_PickupCartageCompletedBoundDateEdit";
			this.JE_PickupCartageCompletedBoundDateEdit.TabIndex = 9;
			// 
			// JE_EstimatedDeliveryOrPickupDateEdit
			// 
			this.JE_EstimatedDeliveryOrPickupDateEdit.AllowDrop = true;
			this.JE_EstimatedDeliveryOrPickupDateEdit.AutoCompleteMonthThreshold = 1;
			this.JE_EstimatedDeliveryOrPickupDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JE_EstimatedDeliveryOrPickupDateEdit, "JE_EstimatedDeliveryOrPickup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_EstimatedDeliveryOrPickup)));
			this.JE_EstimatedDeliveryOrPickupDateEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("001c34bf-3008-4e3f-a2d9-5911af42d59f", "Est. Pickup", "Estimated Pickup", "Estimated Pickup Date", "The planned or booked date/time at which the carrier is assigned to pick up the goods at the origin. Generally this is the earliest date/time that the goods can be picked up.");
			this.JE_EstimatedDeliveryOrPickupDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JE_EstimatedDeliveryOrPickupDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 68, true);
			this.JE_EstimatedDeliveryOrPickupDateEdit.Name = "JE_EstimatedDeliveryOrPickupDateEdit";
			this.JE_EstimatedDeliveryOrPickupDateEdit.TabIndex = 7;
			// 
			// ChargeLabel
			// 
			this.ChargeLabel.AutoSize = true;
			this.ChargeLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("2325ccba-27ca-40fd-b3fa-f93c03ae70a4", "Charge");
			this.ChargeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ChargeLabel.IsFontBold = true;
			this.ChargeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 135, true);
			this.ChargeLabel.Name = "ChargeLabel";
			this.ChargeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 13, true);
			this.ChargeLabel.TabIndex = 11;
			// 
			// JE_DeliveryTruckWaitChargeBoundCalcEdit
			// 
			this.JE_DeliveryTruckWaitChargeBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JE_DeliveryTruckWaitChargeBoundCalcEdit, "JE_PickupOrDeliveryTruckWaitCharge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_PickupOrDeliveryTruckWaitCharge)));
			this.JE_DeliveryTruckWaitChargeBoundCalcEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("dc9955c9-bac4-45db-a15b-259305c64ce5", "Truck Wait Time");
			this.JE_DeliveryTruckWaitChargeBoundCalcEdit.DecimalPlaces = 2;
			this.JE_DeliveryTruckWaitChargeBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 173, true);
			this.JE_DeliveryTruckWaitChargeBoundCalcEdit.Name = "JE_DeliveryTruckWaitChargeBoundCalcEdit";
			this.JE_DeliveryTruckWaitChargeBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.JE_DeliveryTruckWaitChargeBoundCalcEdit.TabIndex = 15;
			this.JE_DeliveryTruckWaitChargeBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JP_LCLAirStorageChargeBoundCalcEdit
			// 
			this.JP_LCLAirStorageChargeBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JP_LCLAirStorageChargeBoundCalcEdit, "DocsAndCartage+JP_LCLAirStorageCharge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DocsAndCartage.JP_LCLAirStorageCharge)));
			this.JP_LCLAirStorageChargeBoundCalcEdit.CaptionResourceString = null;
			this.JP_LCLAirStorageChargeBoundCalcEdit.DecimalPlaces = 2;
			this.JP_LCLAirStorageChargeBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 195, true);
			this.JP_LCLAirStorageChargeBoundCalcEdit.Name = "JP_LCLAirStorageChargeBoundCalcEdit";
			this.JP_LCLAirStorageChargeBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.JP_LCLAirStorageChargeBoundCalcEdit.TabIndex = 17;
			this.JP_LCLAirStorageChargeBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JP_LCLAirStorageDaysOrHoursBoundCalcEdit
			// 
			this.JP_LCLAirStorageDaysOrHoursBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JP_LCLAirStorageDaysOrHoursBoundCalcEdit, "DocsAndCartage+JP_LCLAirStorageDaysOrHours");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DocsAndCartage.JP_LCLAirStorageDaysOrHours)));
			this.JP_LCLAirStorageDaysOrHoursBoundCalcEdit.CaptionResourceString = null;
			this.JP_LCLAirStorageDaysOrHoursBoundCalcEdit.DecimalPlaces = 0;
			this.JP_LCLAirStorageDaysOrHoursBoundCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JP_LCLAirStorageDaysOrHoursBoundCalcEdit, false);
			this.JP_LCLAirStorageDaysOrHoursBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 195, true);
			this.JP_LCLAirStorageDaysOrHoursBoundCalcEdit.Name = "JP_LCLAirStorageDaysOrHoursBoundCalcEdit";
			this.JP_LCLAirStorageDaysOrHoursBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 20, true);
			this.JP_LCLAirStorageDaysOrHoursBoundCalcEdit.TabIndex = 18;
			this.JP_LCLAirStorageDaysOrHoursBoundCalcEdit.Text = "0";
			this.JP_LCLAirStorageDaysOrHoursBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JP_LCLAvailableDateEdit
			// 
			this.JP_LCLAvailableDateEdit.AllowDrop = true;
			this.JP_LCLAvailableDateEdit.AutoCompleteMonthThreshold = 1;
			this.JP_LCLAvailableDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JP_LCLAvailableDateEdit, "DocsAndCartage.JP_LCLAvailable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DocsAndCartage.JP_LCLAvailable)));
			this.JP_LCLAvailableDateEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("42d3e7a5-bbd3-49e0-9933-8931c039448b", "CFS Available");
			this.JP_LCLAvailableDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JP_LCLAvailableDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 46, true);
			this.JP_LCLAvailableDateEdit.Name = "JP_LCLAvailableDateEdit";
			this.JP_LCLAvailableDateEdit.TabIndex = 4;
			// 
			// JP_LCLStorageCommencesDateEdit
			// 
			this.JP_LCLStorageCommencesDateEdit.AllowDrop = true;
			this.JP_LCLStorageCommencesDateEdit.AutoCompleteMonthThreshold = 1;
			this.JP_LCLStorageCommencesDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JP_LCLStorageCommencesDateEdit, "DocsAndCartage.JP_LCLStorageCommences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DocsAndCartage.JP_LCLStorageCommences)));
			this.JP_LCLStorageCommencesDateEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("4feb8723-ae94-4fb2-9929-57897813dd64", "CFS Storage Start");
			this.JP_LCLStorageCommencesDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JP_LCLStorageCommencesDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 46, true);
			this.JP_LCLStorageCommencesDateEdit.Name = "JP_LCLStorageCommencesDateEdit";
			this.JP_LCLStorageCommencesDateEdit.TabIndex = 5;
			// 
			// PickupRequiredFromDateEdit
			// 
			this.PickupRequiredFromDateEdit.AllowDrop = true;
			this.PickupRequiredFromDateEdit.AutoCompleteMonthThreshold = 1;
			this.PickupRequiredFromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PickupRequiredFromDateEdit, "DocsAndCartage.JP_PickupRequiredFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DocsAndCartage.JP_PickupRequiredFrom)));
			this.PickupRequiredFromDateEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("bd8407ea-39e3-4b42-a3e4-b1088e4909e0", "Pickup Required From");
			this.PickupRequiredFromDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.PickupRequiredFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 68, true);
			this.PickupRequiredFromDateEdit.Name = "PickupRequiredFromDateEdit";
			this.PickupRequiredFromDateEdit.TabIndex = 6;
			// 
			// PickupDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PickupRequiredFromDateEdit);
			this.Controls.Add(this.LCLDatesOverrideCheckBox);
			this.Controls.Add(this.DurationLabel);
			this.Controls.Add(this.JE_PickupTruckWaitTimeEdit);
			this.Controls.Add(this.JE_PickupLabourTimeEdit);
			this.Controls.Add(this.JE_PickupLabourChargeBoundCalcEdit);
			this.Controls.Add(this.JE_FCLPickupEquipmentNeededBoundDropEdit);
			this.Controls.Add(this.FCL_AvailableDateEdit);
			this.Controls.Add(this.FCL_StorageDateEdit);
			this.Controls.Add(this.JE_PickupRequiredByBoundDateEdit);
			this.Controls.Add(this.JE_PickupCartageAdvisedBoundDateEdit);
			this.Controls.Add(this.JE_PickupCartageCompletedBoundDateEdit);
			this.Controls.Add(this.JE_EstimatedDeliveryOrPickupDateEdit);
			this.Controls.Add(this.ChargeLabel);
			this.Controls.Add(this.JE_DeliveryTruckWaitChargeBoundCalcEdit);
			this.Controls.Add(this.JP_LCLAirStorageChargeBoundCalcEdit);
			this.Controls.Add(this.JP_LCLAirStorageDaysOrHoursBoundCalcEdit);
			this.Controls.Add(this.JP_LCLAvailableDateEdit);
			this.Controls.Add(this.JP_LCLStorageCommencesDateEdit);
			this.Name = "PickupDetailsUserControl";
			this.ShouldSerializeTabPageMethods = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 232, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.JE_FCLPickupEquipmentNeededBoundDropEdit.ResumeLayout(true);
			this.JE_FCLPickupEquipmentNeededBoundDropEdit.PerformLayout();
			this.FCL_AvailableDateEdit.ResumeLayout(true);
			this.FCL_AvailableDateEdit.PerformLayout();
			this.FCL_StorageDateEdit.ResumeLayout(true);
			this.FCL_StorageDateEdit.PerformLayout();
			this.JE_PickupRequiredByBoundDateEdit.ResumeLayout(true);
			this.JE_PickupRequiredByBoundDateEdit.PerformLayout();
			this.JE_PickupCartageAdvisedBoundDateEdit.ResumeLayout(true);
			this.JE_PickupCartageAdvisedBoundDateEdit.PerformLayout();
			this.JE_PickupCartageCompletedBoundDateEdit.ResumeLayout(true);
			this.JE_PickupCartageCompletedBoundDateEdit.PerformLayout();
			this.JE_EstimatedDeliveryOrPickupDateEdit.ResumeLayout(true);
			this.JE_EstimatedDeliveryOrPickupDateEdit.PerformLayout();
			this.JP_LCLAvailableDateEdit.ResumeLayout(true);
			this.JP_LCLAvailableDateEdit.PerformLayout();
			this.JP_LCLStorageCommencesDateEdit.ResumeLayout(true);
			this.JP_LCLStorageCommencesDateEdit.PerformLayout();
			this.PickupRequiredFromDateEdit.ResumeLayout(true);
			this.PickupRequiredFromDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		ZArchitecture.GUI.ZCheckBox LCLDatesOverrideCheckBox;
		ZArchitecture.ZLabel DurationLabel;
		ZArchitecture.GUI.ZTimeEditEx JE_PickupTruckWaitTimeEdit;
		ZArchitecture.GUI.ZTimeEditEx JE_PickupLabourTimeEdit;
		ZArchitecture.ZCalcEdit JE_PickupLabourChargeBoundCalcEdit;
		ZArchitecture.GUI.ZDropEdit JE_FCLPickupEquipmentNeededBoundDropEdit;
		ZArchitecture.GUI.ZDateEdit FCL_AvailableDateEdit;
		ZArchitecture.GUI.ZDateEdit FCL_StorageDateEdit;
		ZArchitecture.GUI.ZDateEdit JE_PickupRequiredByBoundDateEdit;
		ZArchitecture.GUI.ZDateEdit JE_PickupCartageAdvisedBoundDateEdit;
		ZArchitecture.GUI.ZDateEdit JE_PickupCartageCompletedBoundDateEdit;
		ZArchitecture.GUI.ZDateEdit JE_EstimatedDeliveryOrPickupDateEdit;
		ZArchitecture.ZLabel ChargeLabel;
		ZArchitecture.ZCalcEdit JE_DeliveryTruckWaitChargeBoundCalcEdit;
		ZArchitecture.ZCalcEdit JP_LCLAirStorageChargeBoundCalcEdit;
		ZArchitecture.ZCalcEdit JP_LCLAirStorageDaysOrHoursBoundCalcEdit;
		ZArchitecture.GUI.ZDateEdit JP_LCLAvailableDateEdit;
		ZArchitecture.GUI.ZDateEdit JP_LCLStorageCommencesDateEdit;
		ZArchitecture.GUI.ZDateEdit PickupRequiredFromDateEdit;
	}
}
