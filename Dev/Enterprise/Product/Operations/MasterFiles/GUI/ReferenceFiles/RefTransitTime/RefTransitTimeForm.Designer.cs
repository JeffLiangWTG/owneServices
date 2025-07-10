using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.GUI
{
	partial class RefTransitTimeForm
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.postingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.modeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.serviceLevelDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.transitDaysCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.transitHoursCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.orginZoneSelectionControl = new Enterprise.MasterFiles.GUI.ZoneSelectionControl();
			this.destinationZoneSelectionControl = new Enterprise.MasterFiles.GUI.ZoneSelectionControl();
			this.originZoneGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.destinationZoneGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.transitTimeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.panel1 = new CargoWise.Windows.UI.KPanel();
			this.mainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.transitTimeTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.refTransitTimeDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.refTransitTimeDetailsGrid)).BeginInit();
			this.refTransitTimeDetailsGrid.SuspendLayout();
			this.postingButtonsUserControl.SuspendLayout();
			this.modeDropEdit.SuspendLayout();
			this.serviceLevelDropEdit.SuspendLayout();
			this.orginZoneSelectionControl.SuspendLayout();
			this.destinationZoneSelectionControl.SuspendLayout();
			this.originZoneGroupBox.SuspendLayout();
			this.destinationZoneGroupBox.SuspendLayout();
			this.transitTimeGroupBox.SuspendLayout();
			this.mainTabControl.SuspendLayout();
			this.transitTimeTabPage.SuspendLayout();
			this.panel1.SuspendLayout();

			this.zStmNoteTabPage1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 272, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefTransitTime);
			// 
			// postingButtonsUserControl
			// 
			this.postingButtonsUserControl.AllowDrop = true;
			this.postingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.postingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 441, true);
			this.postingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtonsUserControl.Name = "postingButtonsUserControl";
			this.postingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtonsUserControl.TabIndex = 1;
			// 
			// modeDropEdit
			// 
			this.modeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.modeDropEdit, "RTT_Mode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefTransitTime)(null)).RTT_Mode)));
			this.modeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 15, true);
			this.modeDropEdit.Name = "modeDropEdit";
			this.modeDropEdit.PreBoundMaxLength = 3;
			this.modeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 15, true);
			this.modeDropEdit.TabIndex = 3;
			// 
			// serviceLevelDropEdit
			// 
			this.serviceLevelDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.serviceLevelDropEdit, "RTT_RS_NKServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefTransitTime)(null)).RTT_RS_NKServiceLevel)));
			this.serviceLevelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 40, true);
			this.serviceLevelDropEdit.Name = "serviceLevelDropEdit";
			this.serviceLevelDropEdit.PreBoundMaxLength = 3;
			this.serviceLevelDropEdit.ShowDescriptionBox = false;
			this.serviceLevelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 19, true);
			this.serviceLevelDropEdit.TabIndex = 2;
			// 
			// transitDaysCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.transitDaysCalcEdit, "TransitDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefTransitTime)(null)).TransitDays)));
			this.transitDaysCalcEdit.DecimalPlaces = 2;
			this.transitDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 18, true);
			this.transitDaysCalcEdit.MaxValue = new decimal(new int[] {
			999,
			0,
			0,
			0});
			this.transitDaysCalcEdit.Name = "transitDaysCalcEdit";
			this.transitDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 19, true);
			this.transitDaysCalcEdit.TabIndex = 4;
			this.transitDaysCalcEdit.Text = "0.00";
			this.transitDaysCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// transitHoursCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.transitHoursCalcEdit, "TransitHours");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefTransitTime)(null)).TransitHours)));
			this.transitHoursCalcEdit.DecimalPlaces = 2;
			this.transitHoursCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 18, true);
			this.transitHoursCalcEdit.MaxValue = new decimal(new int[] {
			23,
			0,
			0,
			0});
			this.transitHoursCalcEdit.Name = "transitHoursCalcEdit";
			this.transitHoursCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 19, true);
			this.transitHoursCalcEdit.TabIndex = 5;
			this.transitHoursCalcEdit.Text = "0.00";
			this.transitHoursCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// orginZoneSelectionControl
			// 
			this.orginZoneSelectionControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.orginZoneSelectionControl, "OriginZone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.ZoneSelection)(((Enterprise.MasterFiles.Business.RefTransitTime)(null)).OriginZone)));
			this.orginZoneSelectionControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 18, true);
			this.orginZoneSelectionControl.Name = "orginZoneSelectionControl";
			this.orginZoneSelectionControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 93, true);
			this.orginZoneSelectionControl.TabIndex = 11;
			// 
			// destinationZoneSelectionControl
			// 
			this.destinationZoneSelectionControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.destinationZoneSelectionControl, "DestinationZone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.ZoneSelection)(((Enterprise.MasterFiles.Business.RefTransitTime)(null)).DestinationZone)));
			this.destinationZoneSelectionControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 18, true);
			this.destinationZoneSelectionControl.Name = "destinationZoneSelectionControl";
			this.destinationZoneSelectionControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 93, true);
			this.destinationZoneSelectionControl.TabIndex = 12;
			// 
			// originZoneGroupBox
			// 
			this.originZoneGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("512409a2-660e-4bd9-9be1-996fa28f8a22", "Origin Zone");
			this.originZoneGroupBox.Controls.Add(this.orginZoneSelectionControl);
			this.originZoneGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 73, true);
			this.originZoneGroupBox.Name = "originZoneGroupBox";
			this.originZoneGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 116, true);
			this.originZoneGroupBox.TabIndex = 13;
			this.originZoneGroupBox.TabStop = false;
			// 
			// destinationZoneGroupBox
			// 
			this.destinationZoneGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("087c4e01-91cb-40ce-b230-45ad21c53db7", "Destination Zone");
			this.destinationZoneGroupBox.Controls.Add(this.destinationZoneSelectionControl);
			this.destinationZoneGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(283, 73, true);
			this.destinationZoneGroupBox.Name = "destinationZoneGroupBox";
			this.destinationZoneGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 116, true);
			this.destinationZoneGroupBox.TabIndex = 14;
			this.destinationZoneGroupBox.TabStop = false;
			// 
			// transitTimeGroupBox
			// 
			this.transitTimeGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("67e7acc4-803b-4339-b6dc-d60557b220b3", "Transit Time");
			this.transitTimeGroupBox.Controls.Add(this.transitDaysCalcEdit);
			this.transitTimeGroupBox.Controls.Add(this.transitHoursCalcEdit);
			this.transitTimeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(283, 13, true);
			this.transitTimeGroupBox.Name = "transitTimeGroupBox";
			this.transitTimeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 52, true);
			this.transitTimeGroupBox.TabIndex = 15;
			this.transitTimeGroupBox.TabStop = false;

			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.panel1.Controls.Add(this.refTransitTimeDetailsGrid);
			this.panel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 200, true);
			this.panel1.Name = "panel1";
			this.panel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(540, 200, true);
			this.panel1.TabIndex = 8;

			// 
			// RefTransitTimeDetails
			// 
			this.refTransitTimeDetailsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.refTransitTimeDetailsGrid, "RefTransitTimeDetails");
			this.refTransitTimeDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;

			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTransitTime)(null)).RefTransitTimeDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefTransitTimeDetail)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTransitTime)(null)).RefTransitTimeDetails)).SyncRoot)).DayOfWeek)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.MasterFiles.Business.RefTransitTimeDetail)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTransitTime)(null)).RefTransitTimeDetails)).SyncRoot)).TransitDays)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.MasterFiles.Business.RefTransitTimeDetail)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTransitTime)(null)).RefTransitTimeDetails)).SyncRoot)).TransitHours)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.RefTransitTimeDetail)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTransitTime)(null)).RefTransitTimeDetails)).SyncRoot)).RTD_ArrivalTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.MasterFiles.Business.RefTransitTimeDetail)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTransitTime)(null)).RefTransitTimeDetails)).SyncRoot)).RTD_EffectiveDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.MasterFiles.Business.RefTransitTimeDetail)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTransitTime)(null)).RefTransitTimeDetails)).SyncRoot)).RTD_EndDate)));
			this.refTransitTimeDetailsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d2d415f4-9111-42c4-8b2c-e4d21202cdd1", "Day Of The Week");
			zDropEditColumnStyleInfo1.ColumnName = "DayOfWeek";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3e867f18-5a9c-4c74-90a1-86c6e2261db5", "Transit Time (Days)");
			zTextBoxColumnStyleInfo2.ColumnName = "TransitDays";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3e867f11-5a9c-4c74-90a1-86c6ea661d22", "Transit Time (Hours)");
			zTextBoxColumnStyleInfo3.ColumnName = "TransitHours";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("bbe1e1f8-1de8-4fc0-bf94-17d13e5e4219", "Arrival Time");
			zDateEditColumnStyleInfo1.ColumnName = "RTD_ArrivalTime";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Time;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateTimeOffsetEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("24292208-82b6-4262-ad1f-6433090e1a82", "Effective Date");
			zDateTimeOffsetEditColumnStyleInfo1.ColumnName = "RTD_EffectiveDate";
			zDateTimeOffsetEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Custom;
			zDateTimeOffsetEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateTimeOffsetEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ffe1e1f8-1de8-4fc0-bf94-17d13e5e4217", "End Date");
			zDateTimeOffsetEditColumnStyleInfo2.ColumnName = "RTD_EndDate";
			zDateTimeOffsetEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Custom;
			zDateTimeOffsetEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.refTransitTimeDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.refTransitTimeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.refTransitTimeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.refTransitTimeDetailsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.refTransitTimeDetailsGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo1);
			this.refTransitTimeDetailsGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo2);
			this.refTransitTimeDetailsGrid.GridId = "dd3110d8-0dbc-466a-8beb-621a9db0cb0a";
			this.refTransitTimeDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.refTransitTimeDetailsGrid.LayoutKey = "RefTransitTimeDetailsGrid";
			this.refTransitTimeDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 200, true);
			this.refTransitTimeDetailsGrid.Name = "RefTransitTimeDetailsGrid";
			this.refTransitTimeDetailsGrid.RowHeadersVisible = false;
			this.refTransitTimeDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(503, 130, true);
			this.refTransitTimeDetailsGrid.TabIndex = 16;
			// 
			// mainTabControl
			// 
			this.mainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.mainTabControl.Controls.Add(this.transitTimeTabPage);
			this.mainTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.mainTabControl.Controls.Add(this.zLogsTabPage1);
			this.mainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 2, true);
			this.mainTabControl.Name = "mainTabControl";
			this.mainTabControl.SelectedIndex = 0;
			this.mainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(570, 426, true);
			this.mainTabControl.TabIndex = 16;
			// 
			// transitTimeTabPage
			// 
			this.transitTimeTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f1ede1cd-ceff-47e3-83b5-e7314de41192", "Transit Time");
			this.transitTimeTabPage.Controls.Add(this.modeDropEdit);
			this.transitTimeTabPage.Controls.Add(this.serviceLevelDropEdit);
			this.transitTimeTabPage.Controls.Add(this.transitTimeGroupBox);
			this.transitTimeTabPage.Controls.Add(this.originZoneGroupBox);
			this.transitTimeTabPage.Controls.Add(this.destinationZoneGroupBox);
			this.transitTimeTabPage.Controls.Add(this.panel1);
			this.transitTimeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.transitTimeTabPage.Name = "transitTimeTabPage";
			this.transitTimeTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.transitTimeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(563, 403, true);
			this.transitTimeTabPage.TabIndex = 0;
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(870, 448, true);
			this.zStmNoteTabPage1.TabIndex = 2;
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(870, 468, true);
			this.zLogsTabPage1.TabIndex = 3;
			// 
			// RefTransitTimeForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e0a85e3c-b5f5-4945-aa48-e337856b8cea", "Zone Delivery SLA");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 496, true);
			this.Controls.Add(this.mainTabControl);
			this.Controls.Add(this.postingButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefTransitTime);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Name = "RefTransitTimeForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.postingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.mainTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.refTransitTimeDetailsGrid)).EndInit();
			this.refTransitTimeDetailsGrid.ResumeLayout(false);
			this.postingButtonsUserControl.ResumeLayout(true);
			this.postingButtonsUserControl.PerformLayout();
			this.modeDropEdit.ResumeLayout(true);
			this.modeDropEdit.PerformLayout();
			this.serviceLevelDropEdit.ResumeLayout(true);
			this.serviceLevelDropEdit.PerformLayout();
			this.orginZoneSelectionControl.ResumeLayout(true);
			this.orginZoneSelectionControl.PerformLayout();
			this.destinationZoneSelectionControl.ResumeLayout(true);
			this.destinationZoneSelectionControl.PerformLayout();
			this.originZoneGroupBox.ResumeLayout(false);
			this.originZoneGroupBox.PerformLayout();
			this.destinationZoneGroupBox.ResumeLayout(false);
			this.destinationZoneGroupBox.PerformLayout();
			this.transitTimeGroupBox.ResumeLayout(false);
			this.transitTimeGroupBox.PerformLayout();
			this.mainTabControl.ResumeLayout(false);
			this.mainTabControl.PerformLayout();
			this.transitTimeTabPage.ResumeLayout(false);
			this.transitTimeTabPage.PerformLayout();
			this.zStmNoteTabPage1.ResumeLayout(false);
			this.zStmNoteTabPage1.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZPostingButtonsUserControl postingButtonsUserControl;
		private ZArchitecture.GUI.ZDropEdit modeDropEdit;
		private ZArchitecture.GUI.ZDropEdit serviceLevelDropEdit;
		private ZArchitecture.ZCalcEdit transitDaysCalcEdit;
		private ZArchitecture.ZCalcEdit transitHoursCalcEdit;
		private ZoneSelectionControl orginZoneSelectionControl;
		private ZoneSelectionControl destinationZoneSelectionControl;
		private ZArchitecture.GUI.ZGroupBox originZoneGroupBox;
		private ZArchitecture.GUI.ZGroupBox destinationZoneGroupBox;
		private ZArchitecture.GUI.ZGroupBox transitTimeGroupBox;
		private ZArchitecture.ZGrid refTransitTimeDetailsGrid;
		private ZArchitecture.GUI.ZTemplateTabControl mainTabControl;
		private ZArchitecture.GUI.ZTabPage transitTimeTabPage;
		private ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage1;
		private ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		private CargoWise.Windows.UI.KPanel panel1;
	}
}
