namespace Enterprise.GPS.Module
{
	partial class GPSSupporterDetailsControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.GPSGroup = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zPanel4 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MessagesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.FilterPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ClearButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FindButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FilterActivityTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FilterToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ActivityTimeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FilterFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GPSGroup.SuspendLayout();
			this.zPanel4.SuspendLayout();
			this.zPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).BeginInit();
			this.MessagesGrid.SuspendLayout();
			this.FilterPanel.SuspendLayout();
			this.FilterActivityTypeDropEdit.SuspendLayout();
			this.FilterToDateEdit.SuspendLayout();
			this.FilterFromDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.GPS.Business.GPSSupporter);
			// 
			// GPSGroup
			// 
			this.GPSGroup.CaptionResourceString = Enterprise.GPS.Module.Res.GetData("62ba193f-1a8b-415e-9f87-b968c3836b3c", "GPS");
			this.GPSGroup.Controls.Add(this.zPanel4);
			this.GPSGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GPSGroup.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GPSGroup.Name = "GPSGroup";
			this.GPSGroup.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 600, true);
			this.GPSGroup.TabIndex = 0;
			this.GPSGroup.TabStop = false;
			// 
			// zPanel4
			// 
			this.zPanel4.Controls.Add(this.zPanel1);
			this.zPanel4.Controls.Add(this.FilterPanel);
			this.zPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel4.Name = "zPanel4";
			this.zPanel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 600, true);
			this.zPanel4.TabIndex = 0;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.MessagesGrid);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 86, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 514, true);
			this.zPanel1.TabIndex = 7;
			// 
			// MessagesGrid
			// 
			this.MessagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MessagesGrid, "Activities");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.GPS.Business.GPSSupporter)(null)).Activities)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GPS.Business.GPSSupporterActivity)(((System.Collections.IList)(((Enterprise.GPS.Business.GPSSupporter)(null)).Activities)).SyncRoot)).EN_ActivityType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GPS.Business.GPSSupporterActivity)(((System.Collections.IList)(((Enterprise.GPS.Business.GPSSupporter)(null)).Activities)).SyncRoot)).EN_EventType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.GPS.Business.GPSSupporterActivity)(((System.Collections.IList)(((Enterprise.GPS.Business.GPSSupporter)(null)).Activities)).SyncRoot)).EN_ActivityTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GPS.Business.GPSSupporterActivity)(((System.Collections.IList)(((Enterprise.GPS.Business.GPSSupporter)(null)).Activities)).SyncRoot)).EN_ActivityInformation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.GPS.Business.GPSSupporterActivity)(((System.Collections.IList)(((Enterprise.GPS.Business.GPSSupporter)(null)).Activities)).SyncRoot)).EN_Speed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GPS.Business.GPSSupporterActivity)(((System.Collections.IList)(((Enterprise.GPS.Business.GPSSupporter)(null)).Activities)).SyncRoot)).EN_SpeedKmhMph)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.GPS.Business.GPSSupporterActivity)(((System.Collections.IList)(((Enterprise.GPS.Business.GPSSupporter)(null)).Activities)).SyncRoot)).EN_Latitude)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.GPS.Business.GPSSupporterActivity)(((System.Collections.IList)(((Enterprise.GPS.Business.GPSSupporter)(null)).Activities)).SyncRoot)).EN_Longitude)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.GPS.Business.GPSSupporterActivity)(((System.Collections.IList)(((Enterprise.GPS.Business.GPSSupporter)(null)).Activities)).SyncRoot)).EN_Heading)));
			this.MessagesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "EN_ActivityType";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zTextBoxColumnStyleInfo2.ColumnName = "EN_EventType";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zDateEditColumnStyleInfo1.ColumnName = "EN_ActivityTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.ColumnName = "EN_ActivityInformation";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(320);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "EN_Speed";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zTextBoxColumnStyleInfo4.ColumnName = "EN_SpeedKmhMph";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "EN_Latitude";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "EN_Longitude";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "EN_Heading";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MessagesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MessagesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.MessagesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.MessagesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.MessagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesGrid.GridId = "b38e6df2-4160-4f6a-937d-29324e1a2818";
			this.MessagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessagesGrid.LayoutKey = "zGrid1";
			this.MessagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagesGrid.Name = "MessagesGrid";
			this.MessagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 514, true);
			this.MessagesGrid.TabIndex = 0;
			// 
			// FilterPanel
			// 
			this.FilterPanel.Controls.Add(this.ClearButton);
			this.FilterPanel.Controls.Add(this.FindButton);
			this.FilterPanel.Controls.Add(this.FilterActivityTypeDropEdit);
			this.FilterPanel.Controls.Add(this.FilterToDateEdit);
			this.FilterPanel.Controls.Add(this.ActivityTimeLabel);
			this.FilterPanel.Controls.Add(this.FilterFromDateEdit);
			this.FilterPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.FilterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FilterPanel.Name = "FilterPanel";
			this.FilterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 86, true);
			this.FilterPanel.TabIndex = 8;
			// 
			// ClearButton
			// 
			this.ClearButton.CaptionResourceString = Enterprise.GPS.Module.Res.GetData("GPSSupporterDetailsControl|1cd34704-f1d8-4304-a30b-54615f529f99", "Clear");
			this.ClearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(557, 47, true);
			this.ClearButton.Name = "ClearButton";
			this.ClearButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ClearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 23, true);
			this.ClearButton.TabIndex = 15;
			this.ClearButton.ToolTipCaption = null;
			this.ClearButton.UseVisualStyleBackColor = true;
			// 
			// FindButton
			// 
			this.FindButton.CaptionResourceString = Enterprise.GPS.Module.Res.GetData("GPSSupporterDetailsControl|dd54d6ca-c4f5-4439-b03c-c208263d3eb2", "Find");
			this.FindButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(482, 47, true);
			this.FindButton.Name = "FindButton";
			this.FindButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.FindButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 23, true);
			this.FindButton.TabIndex = 14;
			this.FindButton.ToolTipCaption = null;
			this.FindButton.UseVisualStyleBackColor = true;
			// 
			// FilterActivityTypeDropEdit
			// 
			this.FilterActivityTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FilterActivityTypeDropEdit, "ActivityFilterProvider.FilterActivityType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.GPS.Business.GPSSupporter)(null)).ActivityFilterProvider.FilterActivityType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.GPS.Business.GPSSupporter)(null)).ActivityFilterProvider.ActivityTypes)));
			this.FilterActivityTypeDropEdit.BindToList = "ActivityFilterProvider.ActivityTypes";
			this.FilterActivityTypeDropEdit.CaptionResourceString = Enterprise.GPS.Module.Res.GetData("GPSSupporterDetailsControl|a31ed1fb-796e-4128-aee5-b4e5192729fb", "Activity Type");
			this.FilterActivityTypeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FilterActivityTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 50, true);
			this.FilterActivityTypeDropEdit.Name = "FilterActivityTypeDropEdit";
			this.FilterActivityTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 18, true);
			this.FilterActivityTypeDropEdit.TabIndex = 13;
			// 
			// FilterToDateEdit
			// 
			this.FilterToDateEdit.AllowDrop = true;
			this.FilterToDateEdit.AutoCompleteMonthThreshold = 1;
			this.FilterToDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FilterToDateEdit, "ActivityFilterProvider.ActivityFilterDateTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.GPS.Business.GPSSupporter)(null)).ActivityFilterProvider.ActivityFilterDateTo)));
			this.FilterToDateEdit.CaptionResourceString = Enterprise.GPS.Module.Res.GetData("GPSSupporterDetailsControl|9f706ca3-66b0-4ada-aca2-833861a34c82", "To");
			this.FilterToDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.FilterToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(315, 15, true);
			this.FilterToDateEdit.Name = "FilterToDateEdit";
			this.FilterToDateEdit.TabIndex = 9;
			// 
			// ActivityTimeLabel
			// 
			this.ActivityTimeLabel.AutoSize = true;
			this.ActivityTimeLabel.CaptionResourceString = Enterprise.GPS.Module.Res.GetData("GPSSupporterDetailsControl|855e2b64-b0cf-4f06-a5fe-45ed3bc2e1b3", "Activity Time:");
			this.ActivityTimeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ActivityTimeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 18, true);
			this.ActivityTimeLabel.Name = "ActivityTimeLabel";
			this.ActivityTimeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 14, true);
			this.ActivityTimeLabel.TabIndex = 5;
			// 
			// FilterFromDateEdit
			// 
			this.FilterFromDateEdit.AllowDrop = true;
			this.FilterFromDateEdit.AutoCompleteMonthThreshold = 1;
			this.FilterFromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FilterFromDateEdit, "ActivityFilterProvider.ActivityFilterDateFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.GPS.Business.GPSSupporter)(null)).ActivityFilterProvider.ActivityFilterDateFrom)));
			this.FilterFromDateEdit.CaptionResourceString = Enterprise.GPS.Module.Res.GetData("GPSSupporterDetailsControl|25fcfc9c-e48e-4bed-a7cf-569b846a14e8", "From");
			this.FilterFromDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.FilterFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 15, true);
			this.FilterFromDateEdit.Name = "FilterFromDateEdit";
			this.FilterFromDateEdit.TabIndex = 7;
			// 
			// GPSSupporterDetailsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GPSGroup);
			this.Name = "GPSSupporterDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 600, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GPSGroup.ResumeLayout(false);
			this.GPSGroup.PerformLayout();
			this.zPanel4.ResumeLayout(false);
			this.zPanel4.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).EndInit();
			this.MessagesGrid.ResumeLayout(false);
			this.MessagesGrid.PerformLayout();
			this.FilterPanel.ResumeLayout(false);
			this.FilterPanel.PerformLayout();
			this.FilterActivityTypeDropEdit.ResumeLayout(true);
			this.FilterActivityTypeDropEdit.PerformLayout();
			this.FilterToDateEdit.ResumeLayout(true);
			this.FilterToDateEdit.PerformLayout();
			this.FilterFromDateEdit.ResumeLayout(true);
			this.FilterFromDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox GPSGroup;
		private Enterprise.ZArchitecture.GUI.ZPanel zPanel4;
		private Enterprise.ZArchitecture.GUI.ZPanel zPanel1;
		private Enterprise.ZArchitecture.ZGrid MessagesGrid;
		private Enterprise.ZArchitecture.GUI.ZPanel FilterPanel;
		private Enterprise.ZArchitecture.GUI.ZDateEdit FilterToDateEdit;
		private Enterprise.ZArchitecture.ZLabel ActivityTimeLabel;
		private Enterprise.ZArchitecture.GUI.ZDateEdit FilterFromDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit FilterActivityTypeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZButton ClearButton;
		private Enterprise.ZArchitecture.GUI.ZButton FindButton;
	}
}
