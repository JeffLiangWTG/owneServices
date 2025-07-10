namespace Enterprise.Customs.GUI
{
	public partial class DeliveryDetailsUserControl
	{
		void InitializeComponent()
		{
			this.LCLDatesOverrideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DurationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TruckWaitTimeEdit = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.LabourTimeEdit = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.LabourChargeBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.EquipmentNeededBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FCL_AvailableDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.FCL_StorageDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.RequiredByBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CartageAdvisedBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CartageCompletedBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EstimatedDeliveryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ChargeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WaitChargeBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LCLAvailableDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LCLStorageCommencesDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DeliveryRequiredFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EquipmentNeededBoundDropEdit.SuspendLayout();
			this.FCL_AvailableDateEdit.SuspendLayout();
			this.FCL_StorageDateEdit.SuspendLayout();
			this.RequiredByBoundDateEdit.SuspendLayout();
			this.CartageAdvisedBoundDateEdit.SuspendLayout();
			this.CartageCompletedBoundDateEdit.SuspendLayout();
			this.EstimatedDeliveryDateEdit.SuspendLayout();
			this.LCLAvailableDateEdit.SuspendLayout();
			this.LCLStorageCommencesDateEdit.SuspendLayout();
			this.DeliveryRequiredFromDateEdit.SuspendLayout();
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
			this.LCLDatesOverrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 26, true);
			this.LCLDatesOverrideCheckBox.Name = "LCLDatesOverrideCheckBox";
			this.LCLDatesOverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 17, true);
			this.LCLDatesOverrideCheckBox.TabIndex = 1;
			this.LCLDatesOverrideCheckBox.UseVisualStyleBackColor = true;
			// 
			// DurationLabel
			// 
			this.DurationLabel.AutoSize = true;
			this.DurationLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("8132c9c7-2b32-4141-9ee3-3ee45cd351cb", "Duration");
			this.DurationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.DurationLabel.IsFontBold = true;
			this.DurationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(194, 158, true);
			this.DurationLabel.Name = "DurationLabel";
			this.DurationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 13, true);
			this.DurationLabel.TabIndex = 12;
			// 
			// TruckWaitTimeEdit
			// 
			this.TruckWaitTimeEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TruckWaitTimeEdit, "JE_PickupOrDeliveryTruckWaitTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_PickupOrDeliveryTruckWaitTime)));
			this.TruckWaitTimeEdit.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TruckWaitTimeEdit, false);
			this.TruckWaitTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(197, 196, true);
			this.TruckWaitTimeEdit.Name = "TruckWaitTimeEdit";
			this.TruckWaitTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.TruckWaitTimeEdit.TabIndex = 16;
			// 
			// LabourTimeEdit
			// 
			this.LabourTimeEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.LabourTimeEdit, "JE_DeliveryOrPickupLabourTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_DeliveryOrPickupLabourTime)));
			this.LabourTimeEdit.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.LabourTimeEdit, false);
			this.LabourTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(197, 174, true);
			this.LabourTimeEdit.Name = "LabourTimeEdit";
			this.LabourTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.LabourTimeEdit.TabIndex = 14;
			// 
			// LabourChargeBoundCalcEdit
			// 
			this.LabourChargeBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.LabourChargeBoundCalcEdit, "JE_DeliveryOrPickupLabourCharge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_DeliveryOrPickupLabourCharge)));
			this.LabourChargeBoundCalcEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("d3a98229-0bd1-4590-bb60-a71eead62494", "Delivery Labor");
			this.LabourChargeBoundCalcEdit.DecimalPlaces = 2;
			this.LabourChargeBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 174, true);
			this.LabourChargeBoundCalcEdit.Name = "LabourChargeBoundCalcEdit";
			this.LabourChargeBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.LabourChargeBoundCalcEdit.TabIndex = 13;
			this.LabourChargeBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// EquipmentNeededBoundDropEdit
			// 
			this.EquipmentNeededBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EquipmentNeededBoundDropEdit, "JE_FCLDeliveryOrPickupEquipmentNeeded");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_FCLDeliveryOrPickupEquipmentNeeded)));
			this.EquipmentNeededBoundDropEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("f74db9d2-0c2c-44a3-8626-99090c0672dd", "Drop Mode");
			this.EquipmentNeededBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 2, true);
			this.EquipmentNeededBoundDropEdit.Name = "EquipmentNeededBoundDropEdit";
			this.EquipmentNeededBoundDropEdit.PreBoundMaxLength = 3;
			this.EquipmentNeededBoundDropEdit.ShouldResizeByMaxLength = true;
			this.EquipmentNeededBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 20, true);
			this.EquipmentNeededBoundDropEdit.TabIndex = 0;
			// 
			// FCL_AvailableDateEdit
			// 
			this.FCL_AvailableDateEdit.AllowDrop = true;
			this.FCL_AvailableDateEdit.AutoCompleteMonthThreshold = 1;
			this.FCL_AvailableDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FCL_AvailableDateEdit, "DocsAndCartage+JP_FCLAvailable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DocsAndCartage.JP_FCLAvailable)));
			this.FCL_AvailableDateEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("882add62-a241-4cf5-babf-a847471d6747", "Available", "Available Date", "");
			this.FCL_AvailableDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.FCL_AvailableDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 47, true);
			this.FCL_AvailableDateEdit.Name = "FCL_AvailableDateEdit";
			this.FCL_AvailableDateEdit.TabIndex = 2;
			// 
			// FCL_StorageDateEdit
			// 
			this.FCL_StorageDateEdit.AllowDrop = true;
			this.FCL_StorageDateEdit.AutoCompleteMonthThreshold = 1;
			this.FCL_StorageDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FCL_StorageDateEdit, "DocsAndCartage+JP_FCLStorageCommences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DocsAndCartage.JP_FCLStorageCommences)));
			this.FCL_StorageDateEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("b1cb1d32-b5bf-42b0-813b-75f62de543e6", "Storage", "Storage Date", "");
			this.FCL_StorageDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.FCL_StorageDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 47, true);
			this.FCL_StorageDateEdit.Name = "FCL_StorageDateEdit";
			this.FCL_StorageDateEdit.TabIndex = 3;
			// 
			// RequiredByBoundDateEdit
			// 
			this.RequiredByBoundDateEdit.AllowDrop = true;
			this.RequiredByBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.RequiredByBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.RequiredByBoundDateEdit, "JE_DeliveryOrPickupRequiredBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_DeliveryOrPickupRequiredBy)));
			this.RequiredByBoundDateEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("e89427ea-4c73-470a-b354-521da86f7852", "Required By", "Delivery Required By", "The date/time the goods are required to be delivered to the Consignee by. Generally this is the latest time the goods can be Delivered.");
			this.RequiredByBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.RequiredByBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 113, true);
			this.RequiredByBoundDateEdit.Name = "RequiredByBoundDateEdit";
			this.RequiredByBoundDateEdit.TabIndex = 8;
			// 
			// CartageAdvisedBoundDateEdit
			// 
			this.CartageAdvisedBoundDateEdit.AllowDrop = true;
			this.CartageAdvisedBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.CartageAdvisedBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CartageAdvisedBoundDateEdit, "JP_Calc_CartageAdvised");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JP_Calc_CartageAdvised)));
			this.CartageAdvisedBoundDateEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("01a29c52-3982-493c-8954-97e095ec4df2", "Trn. Booking Requested");
			this.CartageAdvisedBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.CartageAdvisedBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 135, true);
			this.CartageAdvisedBoundDateEdit.Name = "CartageAdvisedBoundDateEdit";
			this.CartageAdvisedBoundDateEdit.TabIndex = 10;
			// 
			// CartageCompletedBoundDateEdit
			// 
			this.CartageCompletedBoundDateEdit.AllowDrop = true;
			this.CartageCompletedBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.CartageCompletedBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CartageCompletedBoundDateEdit, "JE_CartageCompleted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_CartageCompleted)));
			this.CartageCompletedBoundDateEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("621395bd-21ec-4d56-b633-409afa58d892", "Actual Delivery", "The actual date/time the goods have been delivered to the Consignee.");
			this.CartageCompletedBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.CartageCompletedBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 113, true);
			this.CartageCompletedBoundDateEdit.Name = "CartageCompletedBoundDateEdit";
			this.CartageCompletedBoundDateEdit.TabIndex = 9;
			// 
			// EstimatedDeliveryDateEdit
			// 
			this.EstimatedDeliveryDateEdit.AllowDrop = true;
			this.EstimatedDeliveryDateEdit.AutoCompleteMonthThreshold = 1;
			this.EstimatedDeliveryDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EstimatedDeliveryDateEdit, "JE_EstimatedDeliveryOrPickup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_EstimatedDeliveryOrPickup)));
			this.EstimatedDeliveryDateEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("ae49efe2-eb8c-42ed-822d-c0610fd6660b", "Est. Delivery", "Estimated Delivery", "Estimated Delivery Date", "The date/time the goods are planned or booked to be picked up by the Local Transport company from the destination to be delivered to the Consignee. Generally this is the earliest date/time that the goods can be delivered.");
			this.EstimatedDeliveryDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.EstimatedDeliveryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 91, true);
			this.EstimatedDeliveryDateEdit.Name = "EstimatedDeliveryDateEdit";
			this.EstimatedDeliveryDateEdit.TabIndex = 7;
			// 
			// ChargeLabel
			// 
			this.ChargeLabel.AutoSize = true;
			this.ChargeLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("ec69845b-bad6-4bef-9459-fa32221ee6fc", "Charge");
			this.ChargeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ChargeLabel.IsFontBold = true;
			this.ChargeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 158, true);
			this.ChargeLabel.Name = "ChargeLabel";
			this.ChargeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 13, true);
			this.ChargeLabel.TabIndex = 11;
			// 
			// WaitChargeBoundCalcEdit
			// 
			this.WaitChargeBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.WaitChargeBoundCalcEdit, "JE_PickupOrDeliveryTruckWaitCharge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_PickupOrDeliveryTruckWaitCharge)));
			this.WaitChargeBoundCalcEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("c0a34ca0-2a00-4e95-a638-91a1721af591", "Truck Wait Time");
			this.WaitChargeBoundCalcEdit.DecimalPlaces = 2;
			this.WaitChargeBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 196, true);
			this.WaitChargeBoundCalcEdit.Name = "WaitChargeBoundCalcEdit";
			this.WaitChargeBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.WaitChargeBoundCalcEdit.TabIndex = 15;
			this.WaitChargeBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LCLAvailableDateEdit
			// 
			this.LCLAvailableDateEdit.AllowDrop = true;
			this.LCLAvailableDateEdit.AutoCompleteMonthThreshold = 1;
			this.LCLAvailableDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LCLAvailableDateEdit, "DocsAndCartage.JP_LCLAvailable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DocsAndCartage.JP_LCLAvailable)));
			this.LCLAvailableDateEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("40291fec-e448-46e3-86a9-095b2b149d0b", "CFS Available");
			this.LCLAvailableDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LCLAvailableDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 69, true);
			this.LCLAvailableDateEdit.Name = "LCLAvailableDateEdit";
			this.LCLAvailableDateEdit.TabIndex = 4;
			// 
			// LCLStorageCommencesDateEdit
			// 
			this.LCLStorageCommencesDateEdit.AllowDrop = true;
			this.LCLStorageCommencesDateEdit.AutoCompleteMonthThreshold = 1;
			this.LCLStorageCommencesDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LCLStorageCommencesDateEdit, "DocsAndCartage.JP_LCLStorageCommences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DocsAndCartage.JP_LCLStorageCommences)));
			this.LCLStorageCommencesDateEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("3837e633-5e94-4067-b753-765af6d6b48f", "CFS Storage Start");
			this.LCLStorageCommencesDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LCLStorageCommencesDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 69, true);
			this.LCLStorageCommencesDateEdit.Name = "LCLStorageCommencesDateEdit";
			this.LCLStorageCommencesDateEdit.TabIndex = 5;
			// 
			// DeliveryRequiredFromDateEdit
			// 
			this.DeliveryRequiredFromDateEdit.AllowDrop = true;
			this.DeliveryRequiredFromDateEdit.AutoCompleteMonthThreshold = 1;
			this.DeliveryRequiredFromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DeliveryRequiredFromDateEdit, "DocsAndCartage.JP_DeliveryRequiredFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DocsAndCartage.JP_DeliveryRequiredFrom)));
			this.DeliveryRequiredFromDateEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("dc1bacd3-04c2-4200-89a1-247654de378d", "Required From", "Delivery Required From", "");
			this.DeliveryRequiredFromDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DeliveryRequiredFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 91, true);
			this.DeliveryRequiredFromDateEdit.Name = "DeliveryRequiredFromDateEdit";
			this.DeliveryRequiredFromDateEdit.TabIndex = 6;
			// 
			// DeliveryDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DeliveryRequiredFromDateEdit);
			this.Controls.Add(this.LCLDatesOverrideCheckBox);
			this.Controls.Add(this.DurationLabel);
			this.Controls.Add(this.TruckWaitTimeEdit);
			this.Controls.Add(this.LabourTimeEdit);
			this.Controls.Add(this.LabourChargeBoundCalcEdit);
			this.Controls.Add(this.EquipmentNeededBoundDropEdit);
			this.Controls.Add(this.FCL_AvailableDateEdit);
			this.Controls.Add(this.FCL_StorageDateEdit);
			this.Controls.Add(this.RequiredByBoundDateEdit);
			this.Controls.Add(this.CartageAdvisedBoundDateEdit);
			this.Controls.Add(this.CartageCompletedBoundDateEdit);
			this.Controls.Add(this.EstimatedDeliveryDateEdit);
			this.Controls.Add(this.ChargeLabel);
			this.Controls.Add(this.WaitChargeBoundCalcEdit);
			this.Controls.Add(this.LCLAvailableDateEdit);
			this.Controls.Add(this.LCLStorageCommencesDateEdit);
			this.Name = "DeliveryDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(507, 225, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EquipmentNeededBoundDropEdit.ResumeLayout(true);
			this.EquipmentNeededBoundDropEdit.PerformLayout();
			this.FCL_AvailableDateEdit.ResumeLayout(true);
			this.FCL_AvailableDateEdit.PerformLayout();
			this.FCL_StorageDateEdit.ResumeLayout(true);
			this.FCL_StorageDateEdit.PerformLayout();
			this.RequiredByBoundDateEdit.ResumeLayout(true);
			this.RequiredByBoundDateEdit.PerformLayout();
			this.CartageAdvisedBoundDateEdit.ResumeLayout(true);
			this.CartageAdvisedBoundDateEdit.PerformLayout();
			this.CartageCompletedBoundDateEdit.ResumeLayout(true);
			this.CartageCompletedBoundDateEdit.PerformLayout();
			this.EstimatedDeliveryDateEdit.ResumeLayout(true);
			this.EstimatedDeliveryDateEdit.PerformLayout();
			this.LCLAvailableDateEdit.ResumeLayout(true);
			this.LCLAvailableDateEdit.PerformLayout();
			this.LCLStorageCommencesDateEdit.ResumeLayout(true);
			this.LCLStorageCommencesDateEdit.PerformLayout();
			this.DeliveryRequiredFromDateEdit.ResumeLayout(true);
			this.DeliveryRequiredFromDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		ZArchitecture.GUI.ZCheckBox LCLDatesOverrideCheckBox;
		ZArchitecture.ZLabel DurationLabel;
		ZArchitecture.GUI.ZTimeEditEx TruckWaitTimeEdit;
		ZArchitecture.GUI.ZTimeEditEx LabourTimeEdit;
		ZArchitecture.ZCalcEdit LabourChargeBoundCalcEdit;
		ZArchitecture.GUI.ZDropEdit EquipmentNeededBoundDropEdit;
		ZArchitecture.GUI.ZDateEdit FCL_AvailableDateEdit;
		ZArchitecture.GUI.ZDateEdit FCL_StorageDateEdit;
		ZArchitecture.GUI.ZDateEdit RequiredByBoundDateEdit;
		ZArchitecture.GUI.ZDateEdit CartageAdvisedBoundDateEdit;
		ZArchitecture.GUI.ZDateEdit CartageCompletedBoundDateEdit;
		ZArchitecture.GUI.ZDateEdit EstimatedDeliveryDateEdit;
		ZArchitecture.ZLabel ChargeLabel;
		ZArchitecture.ZCalcEdit WaitChargeBoundCalcEdit;
		ZArchitecture.GUI.ZDateEdit LCLAvailableDateEdit;
		ZArchitecture.GUI.ZDateEdit LCLStorageCommencesDateEdit;
		ZArchitecture.GUI.ZDateEdit DeliveryRequiredFromDateEdit;
	}
}
