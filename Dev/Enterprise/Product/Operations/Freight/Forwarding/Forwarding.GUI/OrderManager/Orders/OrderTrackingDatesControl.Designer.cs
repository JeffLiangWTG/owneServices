using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class OrderTrackingDatesControl : ZUserControl
	{
		protected ZDateEdit JD_A_DEPBoundDateEdit;
		ZArchitecture.ZLabel ActualLabel;
		ZArchitecture.ZLabel EstimatedLabel;
		ZDateEdit JD_E_RCVBoundDateEdit;
		ZDateEdit JD_E_PUPBoundDateEdit;
		ZDateEdit JD_E_UNPBoundDateEdit;
		ZDateEdit JD_E_CLRBoundDateEdit;
		ZDateEdit JD_E_CCCBoundDateEdit;
		ZDateEdit JD_E_ARVBoundDateEdit;
		ZDateEdit JD_E_DEPBoundDateEdit;
		ZDateEdit JD_E_ISTBoundDateEdit;
		ZDateEdit JD_E_EXWBoundDateEdit;
		ZDateEdit JD_A_RCVBoundDateEdit;
		ZDateEdit JD_A_PUPBoundDateEdit;
		ZDateEdit JD_A_UNPBoundDateEdit;
		ZDateEdit JD_A_CLRBoundDateEdit;
		ZDateEdit JD_A_CCCBoundDateEdit;
		ZDateEdit JD_A_ARVBoundDateEdit;
		ZDateEdit JD_A_ISTBoundDateEdit;
		ZDateEdit JD_A_EXWBoundDateEdit;

		void InitializeComponent()
		{
			this.JD_A_RCVBoundDateEdit = new ZDateEdit();
			this.JD_E_RCVBoundDateEdit = new ZDateEdit();
			this.JD_A_PUPBoundDateEdit = new ZDateEdit();
			this.JD_E_PUPBoundDateEdit = new ZDateEdit();
			this.JD_A_UNPBoundDateEdit = new ZDateEdit();
			this.JD_E_UNPBoundDateEdit = new ZDateEdit();
			this.JD_A_CLRBoundDateEdit = new ZDateEdit();
			this.JD_E_CLRBoundDateEdit = new ZDateEdit();
			this.JD_A_CCCBoundDateEdit = new ZDateEdit();
			this.JD_E_CCCBoundDateEdit = new ZDateEdit();
			this.JD_A_ARVBoundDateEdit = new ZDateEdit();
			this.JD_E_ARVBoundDateEdit = new ZDateEdit();
			this.JD_A_DEPBoundDateEdit = new ZDateEdit();
			this.JD_E_DEPBoundDateEdit = new ZDateEdit();
			this.JD_A_ISTBoundDateEdit = new ZDateEdit();
			this.JD_E_ISTBoundDateEdit = new ZDateEdit();
			this.JD_A_EXWBoundDateEdit = new ZDateEdit();
			this.ActualLabel = new ZArchitecture.ZLabel();
			this.EstimatedLabel = new ZArchitecture.ZLabel();
			this.JD_E_EXWBoundDateEdit = new ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.OrderDetailsBulkUpdateBusinessObject);
			// 
			// JD_A_RCVBoundDateEdit
			// 
			this.JD_A_RCVBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JD_A_RCVBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_A_RCVBoundDateEdit, "JD_A_RCV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrderDetailsBulkUpdateBusinessObject)(null)).JD_A_RCV)));
			this.JD_A_RCVBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JD_A_RCVBoundDateEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JD_A_RCVBoundDateEdit, false);
			this.JD_A_RCVBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 60, true);
			this.JD_A_RCVBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_A_RCVBoundDateEdit.Name = "JD_A_RCVBoundDateEdit";
			this.JD_A_RCVBoundDateEdit.TabIndex = 42;
			// 
			// JD_E_RCVBoundDateEdit
			// 
			this.JD_E_RCVBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JD_E_RCVBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_E_RCVBoundDateEdit, "JD_E_RCV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrderDetailsBulkUpdateBusinessObject)(null)).JD_E_RCV)));
			this.JD_E_RCVBoundDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderTrackingDatesControl|cfa2a281-7158-44a4-8cde-d2b73bbcd189", "Origin Receival");
			this.JD_E_RCVBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 60, true);
			this.JD_E_RCVBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_E_RCVBoundDateEdit.Name = "JD_E_RCVBoundDateEdit";
			this.JD_E_RCVBoundDateEdit.TabIndex = 40;
			// 
			// JD_A_PUPBoundDateEdit
			// 
			this.JD_A_PUPBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JD_A_PUPBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_A_PUPBoundDateEdit, "JD_A_PUP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrderDetailsBulkUpdateBusinessObject)(null)).JD_A_PUP)));
			this.JD_A_PUPBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JD_A_PUPBoundDateEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JD_A_PUPBoundDateEdit, false);
			this.JD_A_PUPBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 192, true);
			this.JD_A_PUPBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_A_PUPBoundDateEdit.Name = "JD_A_PUPBoundDateEdit";
			this.JD_A_PUPBoundDateEdit.TabIndex = 57;
			// 
			// JD_E_PUPBoundDateEdit
			// 
			this.JD_E_PUPBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JD_E_PUPBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_E_PUPBoundDateEdit, "JD_E_PUP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrderDetailsBulkUpdateBusinessObject)(null)).JD_E_PUP)));
			this.JD_E_PUPBoundDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderTrackingDatesControl|c8f08b91-7bb2-45aa-a45a-6d8a8fb3c383", "Estimated Pickup");
			this.JD_E_PUPBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 192, true);
			this.JD_E_PUPBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_E_PUPBoundDateEdit.Name = "JD_E_PUPBoundDateEdit";
			this.JD_E_PUPBoundDateEdit.TabIndex = 56;
			// 
			// JD_A_UNPBoundDateEdit
			// 
			this.JD_A_UNPBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JD_A_UNPBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_A_UNPBoundDateEdit, "JD_A_UNP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrderDetailsBulkUpdateBusinessObject)(null)).JD_A_UNP)));
			this.JD_A_UNPBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JD_A_UNPBoundDateEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JD_A_UNPBoundDateEdit, false);
			this.JD_A_UNPBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 170, true);
			this.JD_A_UNPBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_A_UNPBoundDateEdit.Name = "JD_A_UNPBoundDateEdit";
			this.JD_A_UNPBoundDateEdit.TabIndex = 55;
			// 
			// JD_E_UNPBoundDateEdit
			// 
			this.JD_E_UNPBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JD_E_UNPBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_E_UNPBoundDateEdit, "JD_E_UNP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrderDetailsBulkUpdateBusinessObject)(null)).JD_E_UNP)));
			this.JD_E_UNPBoundDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderTrackingDatesControl|d3f09b38-5283-4084-923b-0fda1743136f", "Unpacked");
			this.JD_E_UNPBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 170, true);
			this.JD_E_UNPBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_E_UNPBoundDateEdit.Name = "JD_E_UNPBoundDateEdit";
			this.JD_E_UNPBoundDateEdit.TabIndex = 53;
			// 
			// JD_A_CLRBoundDateEdit
			// 
			this.JD_A_CLRBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JD_A_CLRBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_A_CLRBoundDateEdit, "JD_A_CLR");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrderDetailsBulkUpdateBusinessObject)(null)).JD_A_CLR)));
			this.JD_A_CLRBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JD_A_CLRBoundDateEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JD_A_CLRBoundDateEdit, false);
			this.JD_A_CLRBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 148, true);
			this.JD_A_CLRBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_A_CLRBoundDateEdit.Name = "JD_A_CLRBoundDateEdit";
			this.JD_A_CLRBoundDateEdit.TabIndex = 52;
			// 
			// JD_E_CLRBoundDateEdit
			// 
			this.JD_E_CLRBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JD_E_CLRBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_E_CLRBoundDateEdit, "JD_E_CLR");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrderDetailsBulkUpdateBusinessObject)(null)).JD_E_CLR)));
			this.JD_E_CLRBoundDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderTrackingDatesControl|7bc0dd1d-afb7-48d6-b28c-a12066c99b0f", "CC Finalized");
			this.JD_E_CLRBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 148, true);
			this.JD_E_CLRBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_E_CLRBoundDateEdit.Name = "JD_E_CLRBoundDateEdit";
			this.JD_E_CLRBoundDateEdit.TabIndex = 51;
			// 
			// JD_A_CCCBoundDateEdit
			// 
			this.JD_A_CCCBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JD_A_CCCBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_A_CCCBoundDateEdit, "JD_A_CCC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrderDetailsBulkUpdateBusinessObject)(null)).JD_A_CCC)));
			this.JD_A_CCCBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JD_A_CCCBoundDateEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JD_A_CCCBoundDateEdit, false);
			this.JD_A_CCCBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 126, true);
			this.JD_A_CCCBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_A_CCCBoundDateEdit.Name = "JD_A_CCCBoundDateEdit";
			this.JD_A_CCCBoundDateEdit.TabIndex = 50;
			// 
			// JD_E_CCCBoundDateEdit
			// 
			this.JD_E_CCCBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JD_E_CCCBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_E_CCCBoundDateEdit, "JD_E_CCC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrderDetailsBulkUpdateBusinessObject)(null)).JD_E_CCC)));
			this.JD_E_CCCBoundDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderTrackingDatesControl|8437d58b-45cb-48f8-b1ec-78c4acde6e8b", "CC Commenced");
			this.JD_E_CCCBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 126, true);
			this.JD_E_CCCBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_E_CCCBoundDateEdit.Name = "JD_E_CCCBoundDateEdit";
			this.JD_E_CCCBoundDateEdit.TabIndex = 48;
			// 
			// JD_A_ARVBoundDateEdit
			// 
			this.JD_A_ARVBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JD_A_ARVBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_A_ARVBoundDateEdit, "JD_A_ARV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrderDetailsBulkUpdateBusinessObject)(null)).JD_A_ARV)));
			this.JD_A_ARVBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JD_A_ARVBoundDateEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JD_A_ARVBoundDateEdit, false);
			this.JD_A_ARVBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 104, true);
			this.JD_A_ARVBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_A_ARVBoundDateEdit.Name = "JD_A_ARVBoundDateEdit";
			this.JD_A_ARVBoundDateEdit.TabIndex = 47;
			// 
			// JD_E_ARVBoundDateEdit
			// 
			this.JD_E_ARVBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JD_E_ARVBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_E_ARVBoundDateEdit, "JD_E_ARV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrderDetailsBulkUpdateBusinessObject)(null)).JD_E_ARV)));
			this.JD_E_ARVBoundDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderTrackingDatesControl|61d5c617-d921-4cd8-b5bf-bacd9a2df7f6", "Arrival");
			this.JD_E_ARVBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 104, true);
			this.JD_E_ARVBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_E_ARVBoundDateEdit.Name = "JD_E_ARVBoundDateEdit";
			this.JD_E_ARVBoundDateEdit.TabIndex = 45;
			// 
			// JD_A_DEPBoundDateEdit
			// 
			this.JD_A_DEPBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JD_A_DEPBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_A_DEPBoundDateEdit, "JD_A_DEP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrderDetailsBulkUpdateBusinessObject)(null)).JD_A_DEP)));
			this.JD_A_DEPBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JD_A_DEPBoundDateEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JD_A_DEPBoundDateEdit, false);
			this.JD_A_DEPBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 82, true);
			this.JD_A_DEPBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_A_DEPBoundDateEdit.Name = "JD_A_DEPBoundDateEdit";
			this.JD_A_DEPBoundDateEdit.TabIndex = 44;
			// 
			// JD_E_DEPBoundDateEdit
			// 
			this.JD_E_DEPBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JD_E_DEPBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_E_DEPBoundDateEdit, "JD_E_DEP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrderDetailsBulkUpdateBusinessObject)(null)).JD_E_DEP)));
			this.JD_E_DEPBoundDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderTrackingDatesControl|23f6f330-7fdf-4d10-bf99-4a09fc254911", "Departure");
			this.JD_E_DEPBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 82, true);
			this.JD_E_DEPBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_E_DEPBoundDateEdit.Name = "JD_E_DEPBoundDateEdit";
			this.JD_E_DEPBoundDateEdit.TabIndex = 43;
			// 
			// JD_A_ISTBoundDateEdit
			// 
			this.JD_A_ISTBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JD_A_ISTBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_A_ISTBoundDateEdit, "JD_A_IST");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrderDetailsBulkUpdateBusinessObject)(null)).JD_A_IST)));
			this.JD_A_ISTBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JD_A_ISTBoundDateEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JD_A_ISTBoundDateEdit, false);
			this.JD_A_ISTBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 38, true);
			this.JD_A_ISTBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_A_ISTBoundDateEdit.Name = "JD_A_ISTBoundDateEdit";
			this.JD_A_ISTBoundDateEdit.TabIndex = 39;
			// 
			// JD_E_ISTBoundDateEdit
			// 
			this.JD_E_ISTBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JD_E_ISTBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_E_ISTBoundDateEdit, "JD_E_IST");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrderDetailsBulkUpdateBusinessObject)(null)).JD_E_IST)));
			this.JD_E_ISTBoundDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderTrackingDatesControl|9728743a-ace2-4d5c-bca1-2bffbc9a17f6", "Delivered");
			this.JD_E_ISTBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 38, true);
			this.JD_E_ISTBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_E_ISTBoundDateEdit.Name = "JD_E_ISTBoundDateEdit";
			this.JD_E_ISTBoundDateEdit.TabIndex = 38;
			// 
			// JD_A_EXWBoundDateEdit
			// 
			this.JD_A_EXWBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JD_A_EXWBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_A_EXWBoundDateEdit, "JD_A_EXW");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrderDetailsBulkUpdateBusinessObject)(null)).JD_A_EXW)));
			this.JD_A_EXWBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JD_A_EXWBoundDateEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JD_A_EXWBoundDateEdit, false);
			this.JD_A_EXWBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 16, true);
			this.JD_A_EXWBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_A_EXWBoundDateEdit.Name = "JD_A_EXWBoundDateEdit";
			this.JD_A_EXWBoundDateEdit.TabIndex = 35;
			// 
			// ActualLabel
			// 
			this.ActualLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderTrackingDatesControl|24643f9a-c980-4011-ad5a-39f9ea5858b0", "Actual");
			this.ActualLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 0, true);
			this.ActualLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.ActualLabel.Name = "ActualLabel";
			this.ActualLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 15, true);
			this.ActualLabel.TabIndex = 36;
			this.ActualLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// EstimatedLabel
			// 
			this.EstimatedLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderTrackingDatesControl|357ebc46-4341-4e8d-a335-0e6419e70bb0", "Estimated");
			this.EstimatedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 0, true);
			this.EstimatedLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.EstimatedLabel.Name = "EstimatedLabel";
			this.EstimatedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 15, true);
			this.EstimatedLabel.TabIndex = 33;
			// 
			// JD_E_EXWBoundDateEdit
			// 
			this.JD_E_EXWBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JD_E_EXWBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_E_EXWBoundDateEdit, "JD_E_EXW");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrderDetailsBulkUpdateBusinessObject)(null)).JD_E_EXW)));
			this.JD_E_EXWBoundDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderTrackingDatesControl|301d93ef-d1cd-4f1f-9e15-e42c9a892640", "Ex. Factory");
			this.JD_E_EXWBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 16, true);
			this.JD_E_EXWBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.JD_E_EXWBoundDateEdit.Name = "JD_E_EXWBoundDateEdit";
			this.JD_E_EXWBoundDateEdit.TabIndex = 34;
			// 
			// OrderTrackingDatesControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.JD_A_RCVBoundDateEdit);
			this.Controls.Add(this.JD_E_RCVBoundDateEdit);
			this.Controls.Add(this.JD_A_PUPBoundDateEdit);
			this.Controls.Add(this.JD_E_PUPBoundDateEdit);
			this.Controls.Add(this.JD_A_UNPBoundDateEdit);
			this.Controls.Add(this.JD_E_UNPBoundDateEdit);
			this.Controls.Add(this.JD_A_CLRBoundDateEdit);
			this.Controls.Add(this.JD_E_CLRBoundDateEdit);
			this.Controls.Add(this.JD_A_CCCBoundDateEdit);
			this.Controls.Add(this.JD_E_CCCBoundDateEdit);
			this.Controls.Add(this.JD_A_ARVBoundDateEdit);
			this.Controls.Add(this.JD_E_ARVBoundDateEdit);
			this.Controls.Add(this.JD_A_DEPBoundDateEdit);
			this.Controls.Add(this.JD_E_DEPBoundDateEdit);
			this.Controls.Add(this.JD_A_ISTBoundDateEdit);
			this.Controls.Add(this.JD_E_ISTBoundDateEdit);
			this.Controls.Add(this.JD_A_EXWBoundDateEdit);
			this.Controls.Add(this.ActualLabel);
			this.Controls.Add(this.EstimatedLabel);
			this.Controls.Add(this.JD_E_EXWBoundDateEdit);
			this.Name = "OrderTrackingDatesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 302, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
