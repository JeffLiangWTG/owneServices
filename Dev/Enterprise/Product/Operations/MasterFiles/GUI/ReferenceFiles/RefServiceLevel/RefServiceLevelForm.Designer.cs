namespace Enterprise.MasterFiles.GUI
{
	public partial class RefServiceLevelForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Designer generated code

		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.RS_IsDoorToDoorBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RS_DescBoundTextBox = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.RS_CodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DDDPanel = new CargoWise.Windows.UI.KPanel();
			this.mainPanel = new CargoWise.Windows.UI.KPanel();
			this.transitDaysCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.transitHoursCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ArrivalTimeEdit = new Enterprise.ZArchitecture.GUI.ZTimeEdit();
			this.ServiceDeliveryDueTimeEdit = new Enterprise.ZArchitecture.GUI.ZTimeEdit();
			this.transitTimeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.deliverOnWeekendGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.MainTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zCalcEdit1 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.isSystemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.isDeliverOnSaturday = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.isDeliverOnSunday = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.isGatewayCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RS_DescBoundTextBox.SuspendLayout();
			this.ButtonsUserControl.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.DDDPanel.SuspendLayout();
			this.mainPanel.SuspendLayout();
			this.transitTimeGroupBox.SuspendLayout();
			this.deliverOnWeekendGroupBox.SuspendLayout();
			this.zDropEdit1.SuspendLayout();
			this.zStmNoteTabPage1.SuspendLayout();
			this.SuspendLayout();
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 161, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(593, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 2;
			//
			// MessageStatusBarPanel
			//
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(593);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefServiceLevel);
			//
			// RS_IsDoorToDoorBoundCheckEdit
			//
			this.RS_IsDoorToDoorBoundCheckEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RS_IsDoorToDoorBoundCheckEdit, "RS_IsDoorToDoor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefServiceLevel)(null)).RS_IsDoorToDoor)));
			this.RS_IsDoorToDoorBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RS_IsDoorToDoorBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RS_IsDoorToDoorBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(467, 37, true);
			this.RS_IsDoorToDoorBoundCheckEdit.Name = "RS_IsDoorToDoorBoundCheckEdit";
			this.RS_IsDoorToDoorBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.RS_IsDoorToDoorBoundCheckEdit.TabIndex = 6;
			//
			// isGatewayCheckBox
			//
			this.isGatewayCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.isGatewayCheckBox, "RS_IsGateway");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefServiceLevel)(null)).RS_IsGateway)));
			this.isGatewayCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.isGatewayCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isGatewayCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 37, true);
			this.isGatewayCheckBox.Name = "isGatewayCheckBox";
			this.isGatewayCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.isGatewayCheckBox.TabIndex = 5;
			//
			// RS_DescBoundTextBox
			//
			this.RS_DescBoundTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RS_DescBoundTextBox, "RS_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefServiceLevel)(null)).RS_Description)));
			this.RS_DescBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RS_DescBoundTextBox.GridCurrent = null;
			this.RS_DescBoundTextBox.GridMember = null;
			this.RS_DescBoundTextBox.IsMultiLine = false;
			this.RS_DescBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 37, true);
			this.RS_DescBoundTextBox.Name = "RS_DescBoundTextBox";
			this.RS_DescBoundTextBox.ReadOnly = false;
			this.RS_DescBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.RS_DescBoundTextBox.TabIndex = 3;
			//
			// RS_CodeBoundTextBox
			//
			this.RS_CodeBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.RS_CodeBoundTextBox, "RS_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefServiceLevel)(null)).RS_Code)));
			this.RS_CodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 10, true);
			this.RS_CodeBoundTextBox.Name = "RS_CodeBoundTextBox";
			this.RS_CodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.RS_CodeBoundTextBox.TabIndex = 0;
			//
			// ButtonsUserControl
			//
			this.ButtonsUserControl.AllowDrop = true;
			this.ButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 232, true);
			this.ButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.ButtonsUserControl.Name = "ButtonsUserControl";
			this.ButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.ButtonsUserControl.TabIndex = 1;

			//
			// IsActiveCheckBox
			//
			this.IsActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "RS_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefServiceLevel)(null)).RS_IsActive)));
			this.IsActiveCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(467, 10, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsActiveCheckBox.TabIndex = 2;
			//
			// MainTabControl
			//
			this.MainTabControl.Controls.Add(this.MainTabPage);
			this.MainTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.MainTabControl.Controls.Add(this.zLogsTabPage1);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(587, 235, true);
			this.MainTabControl.TabIndex = 0;
			//
			// MainTabPage
			//
			this.MainTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefServiceLevelForm|4fa020dc-ee3f-4048-b978-d3379bbc5499", "Service Level");
			this.MainTabPage.Controls.Add(this.mainPanel);
			this.MainTabPage.Controls.Add(this.DDDPanel);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(579, 170, true);
			this.MainTabPage.TabIndex = 0;
			//
			this.mainPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.mainPanel.Controls.Add(this.zCalcEdit1);
			this.mainPanel.Controls.Add(this.zDropEdit1);
			this.mainPanel.Controls.Add(this.RS_DescBoundTextBox);
			this.mainPanel.Controls.Add(this.isGatewayCheckBox);
			this.mainPanel.Controls.Add(this.RS_IsDoorToDoorBoundCheckEdit);
			this.mainPanel.Controls.Add(this.IsActiveCheckBox);
			this.mainPanel.Controls.Add(this.RS_CodeBoundTextBox);
			this.mainPanel.Controls.Add(this.isSystemCheckBox);
			this.mainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 25, true);
			this.mainPanel.Name = "mainPanel";
			this.mainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(565, 95, true);
			this.mainPanel.TabIndex = 0;
			//
			// zCalcEdit1
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit1, "RS_ServiceDeliveryPercentage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefServiceLevel)(null)).RS_ServiceDeliveryPercentage)));
			this.zCalcEdit1.DecimalPlaces = 2;
			this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(467, 67, true);
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			this.zCalcEdit1.TabIndex = 8;
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;

			//
			// zDropEdit1
			//
			this.zDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit1, "RS_ServiceDeliveryType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefServiceLevel)(null)).RS_ServiceDeliveryType)));
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 67, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.zDropEdit1.TabIndex = 7;
			//
			// isSystemCheckBox
			//
			this.isSystemCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.isSystemCheckBox, "RS_IsSystem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefServiceLevel)(null)).RS_IsSystem)));
			this.isSystemCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("42DCF8E7-534D-4825-8CCF-55823D30B83A", "Is System");
			this.isSystemCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.isSystemCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isSystemCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.isSystemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 10, true);
			this.isSystemCheckBox.Name = "isSystemCheckBox";
			this.isSystemCheckBox.ReadOnly = true;
			this.isSystemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 14, true);
			this.isSystemCheckBox.TabIndex = 1;
			//
			// DDDGroupBox
			//
			this.DDDPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom)));
			this.DDDPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 116, true);
			this.DDDPanel.Controls.Add(this.transitTimeGroupBox);
			this.DDDPanel.Controls.Add(this.ArrivalTimeEdit);
			this.DDDPanel.Controls.Add(this.ServiceDeliveryDueTimeEdit);
			this.DDDPanel.Controls.Add(this.deliverOnWeekendGroupBox);
			this.DDDPanel.Name = "DDDPanel";
			this.DDDPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 45, true);
			this.DDDPanel.TabIndex = 15;
			this.DDDPanel.TabStop = false;

			//
			// transitDaysCalcEdit
			//
			this.BindingSource.SetBindingMember(this.transitDaysCalcEdit, "DefaultTransitDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefServiceLevel)(null)).DefaultTransitDays)));
			this.transitDaysCalcEdit.DecimalPlaces = 2;
			this.transitDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 18, true);
			this.transitDaysCalcEdit.MaxValue = new decimal(new int[] { 999, 0, 0, 0 });
			this.transitDaysCalcEdit.Name = "transitDaysCalcEdit";
			this.transitDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 19, true);
			this.transitDaysCalcEdit.TabIndex = 6;
			this.transitDaysCalcEdit.Text = "0.00";
			this.transitDaysCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// transitHoursCalcEdit
			//
			this.BindingSource.SetBindingMember(this.transitHoursCalcEdit, "DefaultTransitHours");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefServiceLevel)(null)).DefaultTransitHours)));
			this.transitHoursCalcEdit.DecimalPlaces = 2;
			this.transitHoursCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 18, true);
			this.transitHoursCalcEdit.MaxValue = new decimal(new int[] { 23, 0, 0, 0 });
			this.transitHoursCalcEdit.Name = "transitHoursCalcEdit";
			this.transitHoursCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 19, true);
			this.transitHoursCalcEdit.TabIndex = 7;
			this.transitHoursCalcEdit.Text = "0.00";
			this.transitHoursCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// transitTimeGroupBox
			//
			this.transitTimeGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d2d415f4-9111-42c4-8b2c-e4e21202cdd1", "Default Transit Time");
			this.transitTimeGroupBox.Controls.Add(this.transitDaysCalcEdit);
			this.transitTimeGroupBox.Controls.Add(this.transitHoursCalcEdit);
			this.transitTimeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 13, true);
			this.transitTimeGroupBox.Name = "transitTimeGroupBox";
			this.transitTimeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 40, true);
			this.transitTimeGroupBox.TabIndex = 6;
			this.transitTimeGroupBox.TabStop = false;
			//
			// DefaultArrivalTime
			//
			this.BindingSource.SetBindingMember(this.ArrivalTimeEdit, "RS_DefaultArrivalTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefServiceLevel)(null)).RS_DefaultArrivalTime)));
			this.ArrivalTimeEdit.Name = "RS_DefaultArrivalTime";
			this.ArrivalTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 78, true);
			this.ArrivalTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 19, true);
			this.ArrivalTimeEdit.TabIndex = 9;
			//
			// DefaultDeliveryDueTime
			//
			this.BindingSource.SetBindingMember(this.ServiceDeliveryDueTimeEdit, "RS_DefaultDeliveryDueTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefServiceLevel)(null)).RS_DefaultDeliveryDueTime)));
			this.ServiceDeliveryDueTimeEdit.Name = "RS_DefaultDeliveryDueTime";
			this.ServiceDeliveryDueTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 30, true);
			this.ServiceDeliveryDueTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 19, true);
			this.ServiceDeliveryDueTimeEdit.TabIndex = 8;
			//
			// DeliverOnSaturday
			//
			this.isDeliverOnSaturday.AutoSize = true;
			this.BindingSource.SetBindingMember(this.isDeliverOnSaturday, "RS_DeliverOnSaturday");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefServiceLevel)(null)).RS_DeliverOnSaturday)));
			this.isDeliverOnSaturday.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b2b4a84b-a60d-4e8b-a11c-6a66ed6d7143", "Day 1");
			this.isDeliverOnSaturday.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.isDeliverOnSaturday.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isDeliverOnSaturday.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 18, true);
			this.isDeliverOnSaturday.Name = "isDeliverOnSaturday";
			this.isDeliverOnSaturday.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 19, true);
			this.isDeliverOnSaturday.TabIndex = 10;
			//
			// DeliverOnSaturday
			//
			this.isDeliverOnSunday.AutoSize = true;
			this.BindingSource.SetBindingMember(this.isDeliverOnSunday, "RS_DeliverOnSunday");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefServiceLevel)(null)).RS_DeliverOnSunday)));
			this.isDeliverOnSunday.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1276e2ff-58fe-4f99-9693-ec4ce4254092", "Day 2");
			this.isDeliverOnSunday.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.isDeliverOnSunday.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isDeliverOnSunday.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 18, true);
			this.isDeliverOnSunday.Name = "isDeliverOnSunday";
			this.isDeliverOnSunday.ReadOnly = true;
			this.isDeliverOnSunday.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 19, true);
			this.isDeliverOnSunday.TabIndex = 11;
			//
			// transitTimeGroupBox
			//
			this.deliverOnWeekendGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d5a7aca7-51e4-410f-961d-970441b0a365", "Deliver on Weekend");
			this.deliverOnWeekendGroupBox.Controls.Add(this.isDeliverOnSaturday);
			this.deliverOnWeekendGroupBox.Controls.Add(this.isDeliverOnSunday);
			this.deliverOnWeekendGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 60, true);
			this.deliverOnWeekendGroupBox.Name = "deliverOnWeekendGroupBox";
			this.deliverOnWeekendGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 40, true);
			this.deliverOnWeekendGroupBox.TabIndex = 10;
			this.deliverOnWeekendGroupBox.TabStop = false;
			//
			// zStmNoteTabPage1
			//
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(579, 104, true);
			this.zStmNoteTabPage1.TabIndex = 1;
			//
			// zLogsTabPage1
			//
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(579, 125, true);
			this.zLogsTabPage1.TabIndex = 2;
			//
			// RefServiceLevelForm
			//
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefServiceLevelForm|6f773a6c-048a-49fb-974a-8d0c5d29706b", "Service Level");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(593, 300, true);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.ButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefServiceLevel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(593, 300, true);
			this.Name = "RefServiceLevelForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RS_DescBoundTextBox.ResumeLayout(true);
			this.RS_DescBoundTextBox.PerformLayout();
			this.ButtonsUserControl.ResumeLayout(true);
			this.ButtonsUserControl.PerformLayout();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.DDDPanel.ResumeLayout(false);
			this.DDDPanel.PerformLayout();
			this.mainPanel.ResumeLayout(false);
			this.mainPanel.PerformLayout();
			this.transitTimeGroupBox.ResumeLayout(false);
			this.transitTimeGroupBox.PerformLayout();
			this.deliverOnWeekendGroupBox.ResumeLayout(false);
			this.deliverOnWeekendGroupBox.PerformLayout();
			this.zDropEdit1.ResumeLayout(true);
			this.zDropEdit1.PerformLayout();
			this.zStmNoteTabPage1.ResumeLayout(false);
			this.zStmNoteTabPage1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZCheckBox RS_IsDoorToDoorBoundCheckEdit;
		Enterprise.ZArchitecture.ZTranslatableTextControl RS_DescBoundTextBox;
		Enterprise.ZArchitecture.ZTextBox RS_CodeBoundTextBox;
		Enterprise.Core.Forms.ZPostingButtonsUserControl ButtonsUserControl;
		Enterprise.ZArchitecture.GUI.ZCheckBox IsActiveCheckBox;
		CargoWise.Windows.UI.KPanel DDDPanel;
		CargoWise.Windows.UI.KPanel mainPanel;
		Enterprise.ZArchitecture.GUI.ZGroupBox transitTimeGroupBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox deliverOnWeekendGroupBox;
		Enterprise.ZArchitecture.ZCalcEdit transitDaysCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit transitHoursCalcEdit;
		Enterprise.ZArchitecture.GUI.ZTimeEdit ArrivalTimeEdit;
		Enterprise.ZArchitecture.GUI.ZTimeEdit ServiceDeliveryDueTimeEdit;
		Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage MainTabPage;
		Enterprise.ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage1;
		Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		ZArchitecture.ZCalcEdit zCalcEdit1;
		Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit1;
		Enterprise.ZArchitecture.GUI.ZCheckBox isSystemCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox isDeliverOnSaturday;
		Enterprise.ZArchitecture.GUI.ZCheckBox isDeliverOnSunday;
		Enterprise.ZArchitecture.GUI.ZCheckBox isGatewayCheckBox;

	}
}
