namespace Enterprise.Freight.LocalCartage.GUI
{
	partial class CartageLegControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.zDropEdit2 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zGuidFindBox1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.zCheckBox6 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zTextBox4 = new Enterprise.ZArchitecture.ZTextBox();
			this.zPanel7 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.LegDeliveryPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DeliveryAddressSelectionDropDown = new Enterprise.ZArchitecture.GUI.ZDropButtonOnly();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.CartageLegDeliveryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel7 = new Enterprise.ZArchitecture.ZLabel();
			this.zTimeEditEx4 = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.zDateEdit1 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zDateEdit2 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DeliveryETADateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LegWaitpointPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.WaitPointAddressSelectionDropDown = new Enterprise.ZArchitecture.GUI.ZDropButtonOnly();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.CartageLegWaitPointGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zDateEdit12 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zLabel10 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel11 = new Enterprise.ZArchitecture.ZLabel();
			this.zTimeEditEx5 = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.zDateEdit3 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zDateEdit4 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zPanel10 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PickupAddressSelectionDropDown = new Enterprise.ZArchitecture.GUI.ZDropButtonOnly();
			this.AddressGroupBoxCaption = new Enterprise.ZArchitecture.ZLabel();
			this.CartageLegPickupGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LegHasWaitPointCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zLabel14 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel15 = new Enterprise.ZArchitecture.ZLabel();
			this.zDateEdit5 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zDateEdit6 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zTimeEditEx6 = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.zDateEdit17 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zGuidFindBox2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zGuidFindBox4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.DriverCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.SignaturePictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.PostcodeDistanceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel9 = new Enterprise.ZArchitecture.ZLabel();
			this.DistanceButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zCalcDropEdit1 = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zPanel7.SuspendLayout();
			this.LegDeliveryPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DeliveryAddressSelectionDropDown)).BeginInit();
			this.CartageLegDeliveryGroupBox.SuspendLayout();
			this.LegWaitpointPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.WaitPointAddressSelectionDropDown)).BeginInit();
			this.CartageLegWaitPointGroupBox.SuspendLayout();
			this.zPanel10.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PickupAddressSelectionDropDown)).BeginInit();
			this.CartageLegPickupGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SignaturePictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.CommonCartageLeg);
			// 
			// zDropEdit2
			// 
			this.zDropEdit2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit2, "JU_AdditionalService");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).JU_AdditionalService)));
			this.zDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 1, true);
			this.zDropEdit2.Name = "zDropEdit2";
			this.zDropEdit2.ShowDescriptionBox = false;
			this.zDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.zDropEdit2.TabIndex = 1;
			// 
			// zGuidFindBox1
			// 
			this.zGuidFindBox1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox1, "JU_EY_RunSheet");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).JU_EY_RunSheet)));
			this.zGuidFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 1, true);
			this.zGuidFindBox1.Name = "zGuidFindBox1";
			this.zGuidFindBox1.PopupCaption = null;
			this.zGuidFindBox1.ShowDescriptionBox = false;
			this.zGuidFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.zGuidFindBox1.TabIndex = 0;
			// 
			// zTextBox2
			// 
			this.BindingSource.SetBindingMember(this.zTextBox2, "JU_LegNotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).JU_LegNotes)));
			this.zTextBox2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox2.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.LabelCaptionRenderProvider.SetLabelTop(this.zTextBox2, 0);
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(778, 1, true);
			this.zTextBox2.Multiline = true;
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 57, true);
			this.zTextBox2.TabIndex = 7;
			// 
			// zCheckBox6
			// 
			this.zCheckBox6.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zCheckBox6, "JU_IsEmptyContainer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).JU_IsEmptyContainer)));
			this.zCheckBox6.Cursor = System.Windows.Forms.Cursors.Default;
			this.zCheckBox6.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(513, 26, true);
			this.zCheckBox6.Name = "zCheckBox6";
			this.zCheckBox6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 17, true);
			this.zCheckBox6.TabIndex = 6;
			this.zCheckBox6.UseVisualStyleBackColor = true;
			// 
			// zTextBox4
			// 
			this.BindingSource.SetBindingMember(this.zTextBox4, "JU_DeliverySignedFor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).JU_DeliverySignedFor)));
			this.zTextBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 1, true);
			this.zTextBox4.Name = "zTextBox4";
			this.zTextBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 20, true);
			this.zTextBox4.TabIndex = 2;
			// 
			// zPanel7
			// 
			this.zPanel7.Controls.Add(this.LegDeliveryPanel);
			this.zPanel7.Controls.Add(this.LegWaitpointPanel);
			this.zPanel7.Controls.Add(this.zPanel10);
			this.zPanel7.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zPanel7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 64, true);
			this.zPanel7.Name = "zPanel7";
			this.zPanel7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 108, true);
			this.zPanel7.TabIndex = 90;
			// 
			// LegDeliveryPanel
			// 
			this.LegDeliveryPanel.Controls.Add(this.DeliveryAddressSelectionDropDown);
			this.LegDeliveryPanel.Controls.Add(this.zLabel3);
			this.LegDeliveryPanel.Controls.Add(this.CartageLegDeliveryGroupBox);
			this.LegDeliveryPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.LegDeliveryPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 0, true);
			this.LegDeliveryPanel.Name = "LegDeliveryPanel";
			this.LegDeliveryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 108, true);
			this.LegDeliveryPanel.TabIndex = 77;
			// 
			// DeliveryAddressSelectionDropDown
			// 
			this.DeliveryAddressSelectionDropDown.AutoSize = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DeliveryAddressSelectionDropDown, false);
			this.DeliveryAddressSelectionDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 0, true);
			this.DeliveryAddressSelectionDropDown.Name = "DeliveryAddressSelectionDropDown";
			this.DeliveryAddressSelectionDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 18, true);
			this.DeliveryAddressSelectionDropDown.TabIndex = 0;
			// 
			// zLabel3
			// 
			this.zLabel3.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zLabel3, "DeliverToDocAddress+AddressCaption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).DeliverToDocAddress.AddressCaption)));
			this.zLabel3.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zLabel3, false);
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 1, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.zLabel3.TabIndex = 2;
			// 
			// CartageLegDeliveryGroupBox
			// 
			this.CartageLegDeliveryGroupBox.Controls.Add(this.zLabel1);
			this.CartageLegDeliveryGroupBox.Controls.Add(this.zLabel7);
			this.CartageLegDeliveryGroupBox.Controls.Add(this.zTimeEditEx4);
			this.CartageLegDeliveryGroupBox.Controls.Add(this.zDateEdit1);
			this.CartageLegDeliveryGroupBox.Controls.Add(this.zDateEdit2);
			this.CartageLegDeliveryGroupBox.Controls.Add(this.DeliveryETADateEdit);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CartageLegDeliveryGroupBox, false);
			this.CartageLegDeliveryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 2, true);
			this.CartageLegDeliveryGroupBox.Name = "CartageLegDeliveryGroupBox";
			this.CartageLegDeliveryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 106, true);
			this.CartageLegDeliveryGroupBox.TabIndex = 1;
			this.CartageLegDeliveryGroupBox.TabStop = false;
			// 
			// zLabel1
			// 
			this.BindingSource.SetBindingMember(this.zLabel1, "DeliverToDocAddress+E2_CompanyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).DeliverToDocAddress.E2_CompanyName)));
			this.zLabel1.IsFontBold = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 20, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 13, true);
			this.zLabel1.TabIndex = 0;
			// 
			// zLabel7
			// 
			this.BindingSource.SetBindingMember(this.zLabel7, "DeliverToDocAddress+AddressSummaryWithCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).DeliverToDocAddress.AddressSummaryWithCode)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zLabel7, false);
			this.zLabel7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 37, true);
			this.zLabel7.Name = "zLabel7";
			this.zLabel7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 65, true);
			this.zLabel7.TabIndex = 1;
			this.zLabel7.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// zTimeEditEx4
			// 
			this.zTimeEditEx4.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zTimeEditEx4, "JU_CartageDeliveryDemurrage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).JU_CartageDeliveryDemurrage)));
			this.zTimeEditEx4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(261, 80, true);
			this.zTimeEditEx4.Name = "zTimeEditEx4";
			this.zTimeEditEx4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.zTimeEditEx4.TabIndex = 5;
			// 
			// zDateEdit1
			// 
			this.zDateEdit1.AllowDrop = true;
			this.zDateEdit1.AutoCompleteMonthThreshold = 1;
			this.zDateEdit1.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit1, "JU_DeliverTimeIn");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).JU_DeliverTimeIn)));
			this.zDateEdit1.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageLegControl|8c025932-bf52-4ce8-a995-7b341173e93e", "In", "Time In", "");
			this.zDateEdit1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 38, true);
			this.zDateEdit1.Name = "zDateEdit1";
			this.zDateEdit1.TabIndex = 3;
			// 
			// zDateEdit2
			// 
			this.zDateEdit2.AllowDrop = true;
			this.zDateEdit2.AutoCompleteMonthThreshold = 1;
			this.zDateEdit2.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit2, "JU_DeliverTimeOut");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).JU_DeliverTimeOut)));
			this.zDateEdit2.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageLegControl|ac9a1864-5dd1-4726-b8bb-3629bf949c7c", "Out", "Time Out", "");
			this.zDateEdit2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 59, true);
			this.zDateEdit2.Name = "zDateEdit2";
			this.zDateEdit2.TabIndex = 4;
			// 
			// DeliveryETADateEdit
			// 
			this.DeliveryETADateEdit.AllowDrop = true;
			this.DeliveryETADateEdit.AutoCompleteMonthThreshold = 1;
			this.DeliveryETADateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DeliveryETADateEdit, "QuickEstimatedDeliveryTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).QuickEstimatedDeliveryTime)));
			this.DeliveryETADateEdit.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("6d06c744-db41-48e2-b02b-902574b3d249", "Plan. Dlv.");
			this.DeliveryETADateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DeliveryETADateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 17, true);
			this.DeliveryETADateEdit.Name = "DeliveryETADateEdit";
			this.DeliveryETADateEdit.TabIndex = 2;
			// 
			// LegWaitpointPanel
			// 
			this.LegWaitpointPanel.Controls.Add(this.WaitPointAddressSelectionDropDown);
			this.LegWaitpointPanel.Controls.Add(this.zLabel2);
			this.LegWaitpointPanel.Controls.Add(this.CartageLegWaitPointGroupBox);
			this.LegWaitpointPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.LegWaitpointPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 0, true);
			this.LegWaitpointPanel.Name = "LegWaitpointPanel";
			this.LegWaitpointPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 108, true);
			this.LegWaitpointPanel.TabIndex = 76;
			this.LegWaitpointPanel.Visible = false;
			// 
			// WaitPointAddressSelectionDropDown
			// 
			this.WaitPointAddressSelectionDropDown.AutoSize = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.WaitPointAddressSelectionDropDown, false);
			this.WaitPointAddressSelectionDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 0, true);
			this.WaitPointAddressSelectionDropDown.Name = "WaitPointAddressSelectionDropDown";
			this.WaitPointAddressSelectionDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 18, true);
			this.WaitPointAddressSelectionDropDown.TabIndex = 0;
			// 
			// zLabel2
			// 
			this.zLabel2.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zLabel2, "WaitPointDocAddress+AddressCaption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).WaitPointDocAddress.AddressCaption)));
			this.zLabel2.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zLabel2, false);
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 1, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.zLabel2.TabIndex = 2;
			// 
			// CartageLegWaitPointGroupBox
			// 
			this.CartageLegWaitPointGroupBox.Controls.Add(this.zDateEdit12);
			this.CartageLegWaitPointGroupBox.Controls.Add(this.zLabel10);
			this.CartageLegWaitPointGroupBox.Controls.Add(this.zLabel11);
			this.CartageLegWaitPointGroupBox.Controls.Add(this.zTimeEditEx5);
			this.CartageLegWaitPointGroupBox.Controls.Add(this.zDateEdit3);
			this.CartageLegWaitPointGroupBox.Controls.Add(this.zDateEdit4);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CartageLegWaitPointGroupBox, false);
			this.CartageLegWaitPointGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 2, true);
			this.CartageLegWaitPointGroupBox.Name = "CartageLegWaitPointGroupBox";
			this.CartageLegWaitPointGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 106, true);
			this.CartageLegWaitPointGroupBox.TabIndex = 1;
			this.CartageLegWaitPointGroupBox.TabStop = false;
			// 
			// zDateEdit12
			// 
			this.zDateEdit12.AllowDrop = true;
			this.zDateEdit12.AutoCompleteMonthThreshold = 1;
			this.zDateEdit12.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit12, "QuickEstimatedDeliveryTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).QuickEstimatedDeliveryTime)));
			this.zDateEdit12.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("457fe4b4-cef9-4bfc-91db-2cf4bd966671", "Plan. Dlv.");
			this.zDateEdit12.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEdit12.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 17, true);
			this.zDateEdit12.Name = "zDateEdit12";
			this.zDateEdit12.TabIndex = 2;
			// 
			// zLabel10
			// 
			this.BindingSource.SetBindingMember(this.zLabel10, "WaitPointDocAddress+E2_CompanyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).WaitPointDocAddress.E2_CompanyName)));
			this.zLabel10.IsFontBold = true;
			this.zLabel10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 20, true);
			this.zLabel10.Name = "zLabel10";
			this.zLabel10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 13, true);
			this.zLabel10.TabIndex = 0;
			// 
			// zLabel11
			// 
			this.BindingSource.SetBindingMember(this.zLabel11, "WaitPointDocAddress+AddressSummaryWithCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).WaitPointDocAddress.AddressSummaryWithCode)));
			this.zLabel11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 37, true);
			this.zLabel11.Name = "zLabel11";
			this.zLabel11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 65, true);
			this.zLabel11.TabIndex = 1;
			this.zLabel11.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// zTimeEditEx5
			// 
			this.zTimeEditEx5.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zTimeEditEx5, "JU_CartageWaitPointDemurrage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).JU_CartageWaitPointDemurrage)));
			this.zTimeEditEx5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(261, 80, true);
			this.zTimeEditEx5.Name = "zTimeEditEx5";
			this.zTimeEditEx5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.zTimeEditEx5.TabIndex = 5;
			// 
			// zDateEdit3
			// 
			this.zDateEdit3.AllowDrop = true;
			this.zDateEdit3.AutoCompleteMonthThreshold = 1;
			this.zDateEdit3.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit3, "JU_WaitPointTimeIn");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).JU_WaitPointTimeIn)));
			this.zDateEdit3.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageLegControl|6bd3de58-6fab-4dc4-ae0b-8980dce424b9", "In", "Actual Wait Point Time In", "");
			this.zDateEdit3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 38, true);
			this.zDateEdit3.Name = "zDateEdit3";
			this.zDateEdit3.TabIndex = 3;
			// 
			// zDateEdit4
			// 
			this.zDateEdit4.AllowDrop = true;
			this.zDateEdit4.AutoCompleteMonthThreshold = 1;
			this.zDateEdit4.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit4, "JU_WaitPointTimeOut");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).JU_WaitPointTimeOut)));
			this.zDateEdit4.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageLegControl|04323147-b9ab-44f7-bd49-524143532f10", "Out", "Actual Wait Point Time Out", "");
			this.zDateEdit4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 59, true);
			this.zDateEdit4.Name = "zDateEdit4";
			this.zDateEdit4.TabIndex = 4;
			// 
			// zPanel10
			// 
			this.zPanel10.Controls.Add(this.PickupAddressSelectionDropDown);
			this.zPanel10.Controls.Add(this.AddressGroupBoxCaption);
			this.zPanel10.Controls.Add(this.CartageLegPickupGroupBox);
			this.zPanel10.Dock = System.Windows.Forms.DockStyle.Left;
			this.zPanel10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel10.Name = "zPanel10";
			this.zPanel10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 108, true);
			this.zPanel10.TabIndex = 75;
			// 
			// PickupAddressSelectionDropDown
			// 
			this.PickupAddressSelectionDropDown.AutoSize = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PickupAddressSelectionDropDown, false);
			this.PickupAddressSelectionDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 0, true);
			this.PickupAddressSelectionDropDown.Name = "PickupAddressSelectionDropDown";
			this.PickupAddressSelectionDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 18, true);
			this.PickupAddressSelectionDropDown.TabIndex = 0;
			// 
			// AddressGroupBoxCaption
			// 
			this.AddressGroupBoxCaption.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AddressGroupBoxCaption, "PickupFromDocAddress+AddressCaption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).PickupFromDocAddress.AddressCaption)));
			this.AddressGroupBoxCaption.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AddressGroupBoxCaption, false);
			this.AddressGroupBoxCaption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 1, true);
			this.AddressGroupBoxCaption.Name = "AddressGroupBoxCaption";
			this.AddressGroupBoxCaption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.AddressGroupBoxCaption.TabIndex = 1;
			// 
			// CartageLegPickupGroupBox
			// 
			this.CartageLegPickupGroupBox.Controls.Add(this.LegHasWaitPointCheckBox);
			this.CartageLegPickupGroupBox.Controls.Add(this.zLabel14);
			this.CartageLegPickupGroupBox.Controls.Add(this.zLabel15);
			this.CartageLegPickupGroupBox.Controls.Add(this.zDateEdit5);
			this.CartageLegPickupGroupBox.Controls.Add(this.zDateEdit6);
			this.CartageLegPickupGroupBox.Controls.Add(this.zTimeEditEx6);
			this.CartageLegPickupGroupBox.Controls.Add(this.zDateEdit17);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CartageLegPickupGroupBox, false);
			this.CartageLegPickupGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 2, true);
			this.CartageLegPickupGroupBox.Name = "CartageLegPickupGroupBox";
			this.CartageLegPickupGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(20, 3, 3, 20, true);
			this.CartageLegPickupGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 106, true);
			this.CartageLegPickupGroupBox.TabIndex = 2;
			this.CartageLegPickupGroupBox.TabStop = false;
			// 
			// LegHasWaitPointCheckBox
			// 
			this.BindingSource.SetBindingMember(this.LegHasWaitPointCheckBox, "HasWaitPoint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).HasWaitPoint)));
			this.LegHasWaitPointCheckBox.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("ContainerMovesControl|5f704ada-ad27-4c31-bace-acfe8cedad61", "Has Wait Point");
			this.LegHasWaitPointCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LegHasWaitPointCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 0, true);
			this.LegHasWaitPointCheckBox.Name = "LegHasWaitPointCheckBox";
			this.LegHasWaitPointCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 16, true);
			this.LegHasWaitPointCheckBox.TabIndex = 2;
			this.LegHasWaitPointCheckBox.UseVisualStyleBackColor = true;
			// 
			// zLabel14
			// 
			this.BindingSource.SetBindingMember(this.zLabel14, "PickupFromDocAddress+E2_CompanyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).PickupFromDocAddress.E2_CompanyName)));
			this.zLabel14.IsFontBold = true;
			this.zLabel14.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 20, true);
			this.zLabel14.Name = "zLabel14";
			this.zLabel14.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 13, true);
			this.zLabel14.TabIndex = 0;
			// 
			// zLabel15
			// 
			this.BindingSource.SetBindingMember(this.zLabel15, "PickupFromDocAddress+AddressSummaryWithCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).PickupFromDocAddress.AddressSummaryWithCode)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zLabel15, false);
			this.zLabel15.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 37, true);
			this.zLabel15.Name = "zLabel15";
			this.zLabel15.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 65, true);
			this.zLabel15.TabIndex = 1;
			this.zLabel15.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// zDateEdit5
			// 
			this.zDateEdit5.AllowDrop = true;
			this.zDateEdit5.AutoCompleteMonthThreshold = 1;
			this.zDateEdit5.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit5, "JU_PickupTimeIn");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).JU_PickupTimeIn)));
			this.zDateEdit5.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageLegControl|72a8e917-8343-4077-aae6-f008eb5b5394", "In", "Time In", "");
			this.zDateEdit5.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEdit5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 38, true);
			this.zDateEdit5.Name = "zDateEdit5";
			this.zDateEdit5.TabIndex = 4;
			// 
			// zDateEdit6
			// 
			this.zDateEdit6.AllowDrop = true;
			this.zDateEdit6.AutoCompleteMonthThreshold = 1;
			this.zDateEdit6.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit6, "JU_PickupTimeOut");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).JU_PickupTimeOut)));
			this.zDateEdit6.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageLegControl|c98719b3-9743-4350-8404-77b5ce68f3d7", "Out", "Time Out", "");
			this.zDateEdit6.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEdit6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 59, true);
			this.zDateEdit6.Name = "zDateEdit6";
			this.zDateEdit6.TabIndex = 5;
			// 
			// zTimeEditEx6
			// 
			this.zTimeEditEx6.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zTimeEditEx6, "JU_CartagePickupDemurrage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).JU_CartagePickupDemurrage)));
			this.zTimeEditEx6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(261, 80, true);
			this.zTimeEditEx6.Name = "zTimeEditEx6";
			this.zTimeEditEx6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.zTimeEditEx6.TabIndex = 6;
			// 
			// zDateEdit17
			// 
			this.zDateEdit17.AllowDrop = true;
			this.zDateEdit17.AutoCompleteMonthThreshold = 1;
			this.zDateEdit17.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit17, "QuickPlannedPickupTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).QuickPlannedPickupTime)));
			this.zDateEdit17.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("995e1c86-c7c4-4c56-9289-a9e26c542fb7", "Plan.Pic.");
			this.zDateEdit17.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEdit17.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 17, true);
			this.zDateEdit17.Name = "zDateEdit17";
			this.zDateEdit17.TabIndex = 3;
			// 
			// zGuidFindBox2
			// 
			this.zGuidFindBox2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox2, "QuickRQTruck");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).QuickRQTruck)));
			this.zGuidFindBox2.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageLegControl|fc2746b8-67c3-4b6c-b3c7-2961791afb29", "Vehicle", "Vehicle", "");
			this.zGuidFindBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 23, true);
			this.zGuidFindBox2.Name = "zGuidFindBox2";
			this.zGuidFindBox2.PopupCaption = null;
			this.zGuidFindBox2.ShowDescriptionBox = false;
			this.zGuidFindBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.zGuidFindBox2.TabIndex = 3;
			// 
			// zGuidFindBox4
			// 
			this.zGuidFindBox4.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox4, "QuickOHTransportCompany");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).QuickOHTransportCompany)));
			this.zGuidFindBox4.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageLegControl|7a5b0d05-4ddd-4992-8991-6a5f94b9625f", "Trans. Co.", "Transport Co.", "Transport Company", "");
			this.zGuidFindBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 23, true);
			this.zGuidFindBox4.Name = "zGuidFindBox4";
			this.zGuidFindBox4.PopupCaption = null;
			this.zGuidFindBox4.ShowDescriptionBox = false;
			this.zGuidFindBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.zGuidFindBox4.TabIndex = 5;
			// 
			// DriverCodeFindBox
			// 
			this.DriverCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DriverCodeFindBox, "QuickGSDriver");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).QuickGSDriver)));
			this.DriverCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 23, true);
			this.DriverCodeFindBox.Name = "DriverCodeFindBox";
			this.DriverCodeFindBox.ShowDescriptionBox = false;
			this.DriverCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.DriverCodeFindBox.TabIndex = 4;
			// 
			// SignaturePictureBox
			// 
			this.SignaturePictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.SignaturePictureBox.Cursor = System.Windows.Forms.Cursors.NoMove2D;
			this.SignaturePictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(606, 1, true);
			this.SignaturePictureBox.Name = "SignaturePictureBox";
			this.SignaturePictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 61, true);
			this.SignaturePictureBox.TabIndex = 91;
			this.SignaturePictureBox.TabStop = false;
			// 
			// PostcodeDistanceCalcEdit
			// 
			this.PostcodeDistanceCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PostcodeDistanceCalcEdit, "PostcodeDistance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).PostcodeDistance)));
			this.PostcodeDistanceCalcEdit.DecimalPlaces = 2;
			this.PostcodeDistanceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 44, true);
			this.PostcodeDistanceCalcEdit.Name = "PostcodeDistanceCalcEdit";
			this.PostcodeDistanceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.PostcodeDistanceCalcEdit.TabIndex = 8;
			this.PostcodeDistanceCalcEdit.Text = "0.000";
			this.PostcodeDistanceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel9
			// 
			this.zLabel9.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zLabel9, "JU_DistanceUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).JU_DistanceUnit)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zLabel9, false);
			this.zLabel9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 47, true);
			this.zLabel9.Name = "zLabel9";
			this.zLabel9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 13, true);
			this.zLabel9.TabIndex = 9;
			// 
			// DistanceButton
			// 
			this.DistanceButton.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageLegControl|d6f441c3-ace7-47c1-b8a5-2c6bb0717361", "Calculate Distance");
			this.DistanceButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 43, true);
			this.DistanceButton.Name = "DistanceButton";
			this.DistanceButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 23, true);
			this.DistanceButton.TabIndex = 11;
			this.DistanceButton.UseVisualStyleBackColor = true;
			// 
			// zCalcDropEdit1
			// 
			this.zCalcDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zCalcDropEdit1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).JU_Distance)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).JU_DistanceUnit)));
			this.zCalcDropEdit1.BindToAmount = "JU_Distance";
			this.zCalcDropEdit1.BindToUnit = "JU_DistanceUnit";
			this.zCalcDropEdit1.Decimals = 2;
			this.zCalcDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 44, true);
			this.zCalcDropEdit1.Name = "zCalcDropEdit1";
			this.zCalcDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 20, true);
			this.zCalcDropEdit1.TabIndex = 10;
			this.zCalcDropEdit1.UnitPreBoundMaxLength = 3;
			// 
			// CartageLegControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DistanceButton);
			this.Controls.Add(this.zCalcDropEdit1);
			this.Controls.Add(this.PostcodeDistanceCalcEdit);
			this.Controls.Add(this.zLabel9);
			this.Controls.Add(this.SignaturePictureBox);
			this.Controls.Add(this.DriverCodeFindBox);
			this.Controls.Add(this.zGuidFindBox4);
			this.Controls.Add(this.zGuidFindBox2);
			this.Controls.Add(this.zDropEdit2);
			this.Controls.Add(this.zGuidFindBox1);
			this.Controls.Add(this.zTextBox2);
			this.Controls.Add(this.zCheckBox6);
			this.Controls.Add(this.zTextBox4);
			this.Controls.Add(this.zPanel7);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 172, true);
			this.Name = "CartageLegControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 172, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zPanel7.ResumeLayout(false);
			this.LegDeliveryPanel.ResumeLayout(false);
			this.LegDeliveryPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DeliveryAddressSelectionDropDown)).EndInit();
			this.CartageLegDeliveryGroupBox.ResumeLayout(false);
			this.CartageLegDeliveryGroupBox.PerformLayout();
			this.LegWaitpointPanel.ResumeLayout(false);
			this.LegWaitpointPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.WaitPointAddressSelectionDropDown)).EndInit();
			this.CartageLegWaitPointGroupBox.ResumeLayout(false);
			this.CartageLegWaitPointGroupBox.PerformLayout();
			this.zPanel10.ResumeLayout(false);
			this.zPanel10.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PickupAddressSelectionDropDown)).EndInit();
			this.CartageLegPickupGroupBox.ResumeLayout(false);
			this.CartageLegPickupGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SignaturePictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit2;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox zGuidFindBox1;
		private Enterprise.ZArchitecture.ZTextBox zTextBox2;
		private Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBox6;
		private Enterprise.ZArchitecture.ZTextBox zTextBox4;
		private Enterprise.ZArchitecture.GUI.ZPanel zPanel7;
		private Enterprise.ZArchitecture.GUI.ZPanel LegDeliveryPanel;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox CartageLegDeliveryGroupBox;
		private Enterprise.ZArchitecture.ZLabel zLabel1;
		private Enterprise.ZArchitecture.ZLabel zLabel7;
		private Enterprise.ZArchitecture.GUI.ZTimeEditEx zTimeEditEx4;
		private Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit1;
		private Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit2;
		private Enterprise.ZArchitecture.GUI.ZDateEdit DeliveryETADateEdit;
		private Enterprise.ZArchitecture.GUI.ZPanel LegWaitpointPanel;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox CartageLegWaitPointGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit12;
		private Enterprise.ZArchitecture.ZLabel zLabel10;
		private Enterprise.ZArchitecture.ZLabel zLabel11;
		private Enterprise.ZArchitecture.GUI.ZTimeEditEx zTimeEditEx5;
		private Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit3;
		private Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit4;
		private Enterprise.ZArchitecture.GUI.ZPanel zPanel10;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox CartageLegPickupGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox LegHasWaitPointCheckBox;
		private Enterprise.ZArchitecture.ZLabel zLabel14;
		private Enterprise.ZArchitecture.ZLabel zLabel15;
		private Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit5;
		private Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit6;
		private Enterprise.ZArchitecture.GUI.ZTimeEditEx zTimeEditEx6;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit17;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox zGuidFindBox2;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox zGuidFindBox4;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox DriverCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZPictureBox SignaturePictureBox;
		internal Enterprise.ZArchitecture.GUI.ZDropButtonOnly PickupAddressSelectionDropDown;
		private Enterprise.ZArchitecture.ZLabel AddressGroupBoxCaption;
		internal Enterprise.ZArchitecture.GUI.ZDropButtonOnly DeliveryAddressSelectionDropDown;
		private Enterprise.ZArchitecture.ZLabel zLabel3;
		internal Enterprise.ZArchitecture.GUI.ZDropButtonOnly WaitPointAddressSelectionDropDown;
		private Enterprise.ZArchitecture.ZLabel zLabel2;
		private Enterprise.ZArchitecture.ZCalcEdit PostcodeDistanceCalcEdit;
		private Enterprise.ZArchitecture.ZLabel zLabel9;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit zCalcDropEdit1;
		private Enterprise.ZArchitecture.GUI.ZButton DistanceButton;
	}
}
