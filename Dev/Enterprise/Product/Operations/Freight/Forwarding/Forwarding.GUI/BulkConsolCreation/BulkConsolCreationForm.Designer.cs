namespace Enterprise.Freight.Forwarding.GUI
{
	partial class BulkConsolCreationForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.MultipleFlightsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RecurrencePatternGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DailyPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DailyFromLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DailyFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DailyDaysLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DailyRecurEveryLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DailyRecurEveryCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.WeeklyPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.WeeklyDaysCheckedListBox = new Enterprise.ZArchitecture.GUI.ZCheckedListBox();
			this.WeeklyRecurEveryLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WeeklyRecurEveryCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.WeeklyWeeksLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MonthlyPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MonthlyFromLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MonthlyFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.MonthlyMonthsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MonthlyRecurEveryLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MonthlyRecurEveryCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DailyRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.MonthlyRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.WeeklyRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.RangeOfRecurrenceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RangeToLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RangeFromLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RangeToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.RangeFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SelectButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ControlButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.createConsolUserControl = new CreateConsolUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RecurrencePatternGroupBox.SuspendLayout();
			this.DailyPanel.SuspendLayout();
			this.DailyFromDateEdit.SuspendLayout();
			this.WeeklyPanel.SuspendLayout();
			this.MonthlyPanel.SuspendLayout();
			this.MonthlyFromDateEdit.SuspendLayout();
			this.RangeOfRecurrenceGroupBox.SuspendLayout();
			this.RangeToDateEdit.SuspendLayout();
			this.RangeFromDateEdit.SuspendLayout();
			this.ControlButtonsPanel.SuspendLayout();
			this.createConsolUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 597, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(577, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.MultiDaysSelection);
			// 
			// MultipleFlightsLabel
			// 
			this.MultipleFlightsLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BulkConsolCreationForm|4bea66e6-b95b-42fd-b302-2a70e0eb0c8e", "There are multiple flights satisfying your criteria, please select one or multiple flights to import");
			this.MultipleFlightsLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.MultipleFlightsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.MultipleFlightsLabel.IsFontBold = true;
			this.MultipleFlightsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MultipleFlightsLabel.Name = "MultipleFlightsLabel";
			this.MultipleFlightsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(577, 30, true);
			this.MultipleFlightsLabel.TabIndex = 5;
			this.MultipleFlightsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// RecurrencePatternGroupBox
			// 
			this.RecurrencePatternGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BulkConsolCreationForm|A92B2B55-B263-41CB-8B82-42FF7D0EE1B5", "Recurrence Pattern");
			this.RecurrencePatternGroupBox.Controls.Add(this.DailyPanel);
			this.RecurrencePatternGroupBox.Controls.Add(this.WeeklyPanel);
			this.RecurrencePatternGroupBox.Controls.Add(this.MonthlyPanel);
			this.RecurrencePatternGroupBox.Controls.Add(this.DailyRadioButton);
			this.RecurrencePatternGroupBox.Controls.Add(this.MonthlyRadioButton);
			this.RecurrencePatternGroupBox.Controls.Add(this.WeeklyRadioButton);
			this.RecurrencePatternGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.RecurrencePatternGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 30, true);
			this.RecurrencePatternGroupBox.Name = "RecurrencePatternGroupBox";
			this.RecurrencePatternGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(577, 130, true);
			this.RecurrencePatternGroupBox.TabIndex = 1;
			this.RecurrencePatternGroupBox.TabStop = false;
			// 
			// DailyPanel
			// 
			this.DailyPanel.Controls.Add(this.DailyFromLabel);
			this.DailyPanel.Controls.Add(this.DailyFromDateEdit);
			this.DailyPanel.Controls.Add(this.DailyDaysLabel);
			this.DailyPanel.Controls.Add(this.DailyRecurEveryLabel);
			this.DailyPanel.Controls.Add(this.DailyRecurEveryCalcEdit);
			this.DailyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 19, true);
			this.DailyPanel.Name = "DailyPanel";
			this.DailyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 80, true);
			this.DailyPanel.TabIndex = 3;
			// 
			// DailyFromLabel
			// 
			this.DailyFromLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BulkConsolCreationForm|A2C42BF6-41BF-416F-A3B3-29754B9061CC", "From:");
			this.DailyFromLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DailyFromLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 25, true);
			this.DailyFromLabel.Name = "DailyFromLabel";
			this.DailyFromLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 15, true);
			this.DailyFromLabel.TabIndex = 1;
			// 
			// DailyFromDateEdit
			// 
			this.DailyFromDateEdit.AllowDrop = true;
			this.DailyFromDateEdit.AutoCompleteMonthThreshold = 1;
			this.DailyFromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DailyFromDateEdit, "DailyFromDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).DailyFromDate)));
			this.DailyFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 23, true);
			this.DailyFromDateEdit.Name = "DailyFromDateEdit";
			this.DailyFromDateEdit.TabIndex = 0;
			// 
			// DailyDaysLabel
			// 
			this.DailyDaysLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BulkConsolCreationForm|23220FDC-E597-41A4-B3D6-7E1B3EEF3D0A", "days");
			this.DailyDaysLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DailyDaysLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 59, true);
			this.DailyDaysLabel.Name = "DailyDaysLabel";
			this.DailyDaysLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 15, true);
			this.DailyDaysLabel.TabIndex = 4;
			// 
			// DailyRecurEveryLabel
			// 
			this.DailyRecurEveryLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BulkConsolCreationForm|AE030516-6FA4-4082-98C0-71170E859954", "Recur every");
			this.DailyRecurEveryLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DailyRecurEveryLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 59, true);
			this.DailyRecurEveryLabel.Name = "DailyRecurEveryLabel";
			this.DailyRecurEveryLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 15, true);
			this.DailyRecurEveryLabel.TabIndex = 2;
			// 
			// DailyRecurEveryCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DailyRecurEveryCalcEdit, "DailyRecurEvery");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).DailyRecurEvery)));
			this.DailyRecurEveryCalcEdit.DecimalPlaces = 2;
			this.DailyRecurEveryCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 57, true);
			this.DailyRecurEveryCalcEdit.Name = "DailyRecurEveryCalcEdit";
			this.DailyRecurEveryCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 17, true);
			this.DailyRecurEveryCalcEdit.TabIndex = 3;
			this.DailyRecurEveryCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// WeeklyPanel
			// 
			this.WeeklyPanel.Controls.Add(this.WeeklyDaysCheckedListBox);
			this.WeeklyPanel.Controls.Add(this.WeeklyRecurEveryLabel);
			this.WeeklyPanel.Controls.Add(this.WeeklyRecurEveryCalcEdit);
			this.WeeklyPanel.Controls.Add(this.WeeklyWeeksLabel);
			this.WeeklyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 19, true);
			this.WeeklyPanel.Name = "WeeklyPanel";
			this.WeeklyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 121, true);
			this.WeeklyPanel.TabIndex = 4;
			// 
			// WeeklyDaysCheckedListBox
			// 
			this.WeeklyDaysCheckedListBox.BindingItems = null;
			this.BindingSource.SetBindingMember(this.WeeklyDaysCheckedListBox, "WeeklyDaysCheckedList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.ZBoolDescriptionPairList)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).WeeklyDaysCheckedList)));
			this.WeeklyDaysCheckedListBox.FormattingEnabled = true;
			this.WeeklyDaysCheckedListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 3, true);
			this.WeeklyDaysCheckedListBox.MultiColumn = true;
			this.WeeklyDaysCheckedListBox.Name = "WeeklyDaysCheckedListBox";
			this.WeeklyDaysCheckedListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 71, true);
			this.WeeklyDaysCheckedListBox.TabIndex = 9;
			// 
			// WeeklyRecurEveryLabel
			// 
			this.WeeklyRecurEveryLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BulkConsolCreationForm|E94E0866-B9BF-49CC-AC30-AAB6C1CA2FD6", "Recur every");
			this.WeeklyRecurEveryLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.WeeklyRecurEveryLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 71, true);
			this.WeeklyRecurEveryLabel.Name = "WeeklyRecurEveryLabel";
			this.WeeklyRecurEveryLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 15, true);
			this.WeeklyRecurEveryLabel.TabIndex = 8;
			// 
			// WeeklyRecurEveryCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.WeeklyRecurEveryCalcEdit, "WeeklyRecurEvery");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).WeeklyRecurEvery)));
			this.WeeklyRecurEveryCalcEdit.DecimalPlaces = 2;
			this.WeeklyRecurEveryCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 69, true);
			this.WeeklyRecurEveryCalcEdit.Name = "WeeklyRecurEveryCalcEdit";
			this.WeeklyRecurEveryCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 17, true);
			this.WeeklyRecurEveryCalcEdit.TabIndex = 11;
			this.WeeklyRecurEveryCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// WeeklyWeeksLabel
			// 
			this.WeeklyWeeksLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BulkConsolCreationForm|E35DAB84-77BA-4C16-BD71-E80F7CD5355C", "weeks");
			this.WeeklyWeeksLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.WeeklyWeeksLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 71, true);
			this.WeeklyWeeksLabel.Name = "WeeklyWeeksLabel";
			this.WeeklyWeeksLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 15, true);
			this.WeeklyWeeksLabel.TabIndex = 10;
			// 
			// MonthlyPanel
			// 
			this.MonthlyPanel.Controls.Add(this.MonthlyFromLabel);
			this.MonthlyPanel.Controls.Add(this.MonthlyFromDateEdit);
			this.MonthlyPanel.Controls.Add(this.MonthlyMonthsLabel);
			this.MonthlyPanel.Controls.Add(this.MonthlyRecurEveryLabel);
			this.MonthlyPanel.Controls.Add(this.MonthlyRecurEveryCalcEdit);
			this.MonthlyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 19, true);
			this.MonthlyPanel.Name = "MonthlyPanel";
			this.MonthlyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 121, true);
			this.MonthlyPanel.TabIndex = 5;
			// 
			// MonthlyFromLabel
			// 
			this.MonthlyFromLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BulkConsolCreationForm|469ECD28-49A1-4E86-B12A-09C4EFA5F79B", "From:");
			this.MonthlyFromLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MonthlyFromLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(50, 29, true);
			this.MonthlyFromLabel.Name = "MonthlyFromLabel";
			this.MonthlyFromLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 15, true);
			this.MonthlyFromLabel.TabIndex = 3;
			// 
			// MonthlyFromDateEdit
			// 
			this.MonthlyFromDateEdit.AllowDrop = true;
			this.MonthlyFromDateEdit.AutoCompleteMonthThreshold = 1;
			this.MonthlyFromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.MonthlyFromDateEdit, "MonthlyFromDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).MonthlyFromDate)));
			this.MonthlyFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 27, true);
			this.MonthlyFromDateEdit.Name = "MonthlyFromDateEdit";
			this.MonthlyFromDateEdit.TabIndex = 2;
			// 
			// MonthlyMonthsLabel
			// 
			this.MonthlyMonthsLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BulkConsolCreationForm|9CE8A09D-61FD-4DA4-8246-69178919972C", "months");
			this.MonthlyMonthsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MonthlyMonthsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 59, true);
			this.MonthlyMonthsLabel.Name = "MonthlyMonthsLabel";
			this.MonthlyMonthsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 15, true);
			this.MonthlyMonthsLabel.TabIndex = 7;
			// 
			// MonthlyRecurEveryLabel
			// 
			this.MonthlyRecurEveryLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BulkConsolCreationForm|1E899282-064B-4457-A5BC-5ED9D331CEBF", "Recur every");
			this.MonthlyRecurEveryLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MonthlyRecurEveryLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(50, 59, true);
			this.MonthlyRecurEveryLabel.Name = "MonthlyRecurEveryLabel";
			this.MonthlyRecurEveryLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 15, true);
			this.MonthlyRecurEveryLabel.TabIndex = 5;
			// 
			// MonthlyRecurEveryCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MonthlyRecurEveryCalcEdit, "MonthlyRecurEvery");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).MonthlyRecurEvery)));
			this.MonthlyRecurEveryCalcEdit.DecimalPlaces = 2;
			this.MonthlyRecurEveryCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 57, true);
			this.MonthlyRecurEveryCalcEdit.Name = "MonthlyRecurEveryCalcEdit";
			this.MonthlyRecurEveryCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 17, true);
			this.MonthlyRecurEveryCalcEdit.TabIndex = 6;
			this.MonthlyRecurEveryCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DailyRadioButton
			// 
			this.DailyRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.DailyRadioButton, "UseDailyPattern");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).UseDailyPattern)));
			this.DailyRadioButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BulkConsolCreationForm|A8053CE5-1FBC-4F68-94A7-E96DBFDFB0D8", "Daily");
			this.DailyRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DailyRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 21, true);
			this.DailyRadioButton.Name = "DailyRadioButton";
			this.DailyRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 30, true);
			this.DailyRadioButton.TabIndex = 0;
			this.DailyRadioButton.TabStop = true;
			this.DailyRadioButton.UseVisualStyleBackColor = true;
			this.DailyRadioButton.CheckedChanged += new System.EventHandler(this.DailyRadioButton_CheckedChanged);
			// 
			// MonthlyRadioButton
			// 
			this.MonthlyRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.MonthlyRadioButton, "UseMonthlyPattern");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).UseMonthlyPattern)));
			this.MonthlyRadioButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BulkConsolCreationForm|E20C4B75-66EE-4C6A-8EAC-1345102D54D4", "Monthly");
			this.MonthlyRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MonthlyRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 92, true);
			this.MonthlyRadioButton.Name = "MonthlyRadioButton";
			this.MonthlyRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 30, true);
			this.MonthlyRadioButton.TabIndex = 2;
			this.MonthlyRadioButton.TabStop = true;
			this.MonthlyRadioButton.UseVisualStyleBackColor = true;
			this.MonthlyRadioButton.CheckedChanged += new System.EventHandler(this.MonthlyRadioButton_CheckedChanged);
			// 
			// WeeklyRadioButton
			// 
			this.WeeklyRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.WeeklyRadioButton, "UseWeeklyPattern");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).UseWeeklyPattern)));
			this.WeeklyRadioButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BulkConsolCreationForm|BC9BE742-E96E-46B9-AE78-62ACC9A82EB0", "Weekly");
			this.WeeklyRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.WeeklyRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 57, true);
			this.WeeklyRadioButton.Name = "WeeklyRadioButton";
			this.WeeklyRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 30, true);
			this.WeeklyRadioButton.TabIndex = 1;
			this.WeeklyRadioButton.TabStop = true;
			this.WeeklyRadioButton.UseVisualStyleBackColor = true;
			this.WeeklyRadioButton.CheckedChanged += new System.EventHandler(this.WeeklyRadioButton_CheckedChanged);
			// 
			// RangeOfRecurrenceGroupBox
			// 
			this.RangeOfRecurrenceGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BulkConsolCreationForm|FB32D166-4AE5-412D-B5B8-B84786C49621", "Range Of Recurrence");
			this.RangeOfRecurrenceGroupBox.Controls.Add(this.RangeToLabel);
			this.RangeOfRecurrenceGroupBox.Controls.Add(this.RangeFromLabel);
			this.RangeOfRecurrenceGroupBox.Controls.Add(this.RangeToDateEdit);
			this.RangeOfRecurrenceGroupBox.Controls.Add(this.RangeFromDateEdit);
			this.RangeOfRecurrenceGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.RangeOfRecurrenceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 160, true);
			this.RangeOfRecurrenceGroupBox.Name = "RangeOfRecurrenceGroupBox";
			this.RangeOfRecurrenceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(577, 45, true);
			this.RangeOfRecurrenceGroupBox.TabIndex = 2;
			this.RangeOfRecurrenceGroupBox.TabStop = false;
			// 
			// RangeToLabel
			// 
			this.RangeToLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BulkConsolCreationForm|74F57616-7674-4731-900F-9B98FC190E8D", "To");
			this.RangeToLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RangeToLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 21, true);
			this.RangeToLabel.Name = "RangeToLabel";
			this.RangeToLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 17, true);
			this.RangeToLabel.TabIndex = 3;
			// 
			// RangeFromLabel
			// 
			this.RangeFromLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BulkConsolCreationForm|D8F2FE1C-DBA6-4BF2-875E-08AE6C88A2BC", "From");
			this.RangeFromLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RangeFromLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 21, true);
			this.RangeFromLabel.Name = "RangeFromLabel";
			this.RangeFromLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 17, true);
			this.RangeFromLabel.TabIndex = 2;
			// 
			// RangeToDateEdit
			// 
			this.RangeToDateEdit.AllowDrop = true;
			this.RangeToDateEdit.AutoCompleteMonthThreshold = 1;
			this.RangeToDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.RangeToDateEdit, "RangeToDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).RangeToDate)));
			this.RangeToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(211, 21, true);
			this.RangeToDateEdit.Name = "RangeToDateEdit";
			this.RangeToDateEdit.TabIndex = 1;
			// 
			// RangeFromDateEdit
			// 
			this.RangeFromDateEdit.AllowDrop = true;
			this.RangeFromDateEdit.AutoCompleteMonthThreshold = 1;
			this.RangeFromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.RangeFromDateEdit, "RangeFromDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).RangeFromDate)));
			this.RangeFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 21, true);
			this.RangeFromDateEdit.Name = "RangeFromDateEdit";
			this.RangeFromDateEdit.TabIndex = 0;
			// 
			// SelectButton
			// 
			this.SelectButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BulkConsolCreationForm|95568768-9D80-417F-B39B-7EAAE69BF34A", "Select");
			this.SelectButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(402, 7, true);
			this.SelectButton.Name = "SelectButton";
			this.SelectButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SelectButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SelectButton.TabIndex = 0;
			this.SelectButton.ToolTipCaption = null;
			this.SelectButton.UseVisualStyleBackColor = true;
			this.SelectButton.Click += new System.EventHandler(this.SelectButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BulkConsolCreationForm|ABDA69C3-4311-4ECF-A8DF-1A265EE609D1", "Cancel");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 7, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 1;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// ControlButtonsPanel
			// 
			this.ControlButtonsPanel.Controls.Add(this.SelectButton);
			this.ControlButtonsPanel.Controls.Add(this.CloseButton);
			this.ControlButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ControlButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 566, true);
			this.ControlButtonsPanel.Name = "ControlButtonsPanel";
			this.ControlButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(577, 34, true);
			this.ControlButtonsPanel.TabIndex = 4;
			//
			// createConsolUserControl
			//
			this.createConsolUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 215, true);
			this.createConsolUserControl.Name = "createConsolsTabControl";
			this.createConsolUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(577, 290, true);
			this.createConsolUserControl.TabIndex = 3;
			// 
			// BulkConsolCreationForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BulkConsolCreationForm|7E311FAA-A206-4419-B572-3664C090715E", "Select Multiple Schedules");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(577, 561, true);
			this.Controls.Add(this.ControlButtonsPanel);
			this.Controls.Add(this.RangeOfRecurrenceGroupBox);
			this.Controls.Add(this.RecurrencePatternGroupBox);
			this.Controls.Add(this.MultipleFlightsLabel);
			this.Controls.Add(this.createConsolUserControl);
			this.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.MultiDaysSelection);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "BulkConsolCreationForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.MultipleFlightsLabel, 0);
			this.Controls.SetChildIndex(this.RecurrencePatternGroupBox, 0);
			this.Controls.SetChildIndex(this.RangeOfRecurrenceGroupBox, 0);
			this.Controls.SetChildIndex(this.ControlButtonsPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RecurrencePatternGroupBox.ResumeLayout(false);
			this.RecurrencePatternGroupBox.PerformLayout();
			this.DailyPanel.ResumeLayout(false);
			this.DailyPanel.PerformLayout();
			this.DailyFromDateEdit.ResumeLayout(true);
			this.DailyFromDateEdit.PerformLayout();
			this.WeeklyPanel.ResumeLayout(false);
			this.WeeklyPanel.PerformLayout();
			this.MonthlyPanel.ResumeLayout(false);
			this.MonthlyPanel.PerformLayout();
			this.MonthlyFromDateEdit.ResumeLayout(true);
			this.MonthlyFromDateEdit.PerformLayout();
			this.RangeOfRecurrenceGroupBox.ResumeLayout(false);
			this.RangeOfRecurrenceGroupBox.PerformLayout();
			this.RangeToDateEdit.ResumeLayout(true);
			this.RangeToDateEdit.PerformLayout();
			this.RangeFromDateEdit.ResumeLayout(true);
			this.RangeFromDateEdit.PerformLayout();
			this.ControlButtonsPanel.ResumeLayout(false);
			this.ControlButtonsPanel.PerformLayout();
			this.createConsolUserControl.ResumeLayout(false);
			this.createConsolUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel MultipleFlightsLabel;
		private ZArchitecture.GUI.ZGroupBox RecurrencePatternGroupBox;
		private ZArchitecture.GUI.ZPanel MonthlyPanel;
		private ZArchitecture.ZLabel MonthlyMonthsLabel;
		private ZArchitecture.ZCalcEdit MonthlyRecurEveryCalcEdit;
		private ZArchitecture.ZLabel MonthlyRecurEveryLabel;
		private ZArchitecture.ZLabel MonthlyFromLabel;
		private ZArchitecture.GUI.ZDateEdit MonthlyFromDateEdit;
		private ZArchitecture.GUI.ZPanel DailyPanel;
		private ZArchitecture.GUI.ZPanel WeeklyPanel;
		private ZArchitecture.ZLabel WeeklyWeeksLabel;
		private ZArchitecture.ZCalcEdit WeeklyRecurEveryCalcEdit;
		private ZArchitecture.ZLabel WeeklyRecurEveryLabel;
		private ZArchitecture.ZLabel DailyDaysLabel;
		private ZArchitecture.ZLabel DailyFromLabel;
		private ZArchitecture.ZCalcEdit DailyRecurEveryCalcEdit;
		private ZArchitecture.GUI.ZDateEdit DailyFromDateEdit;
		private ZArchitecture.ZLabel DailyRecurEveryLabel;
		private ZArchitecture.GUI.ZRadioButton MonthlyRadioButton;
		private ZArchitecture.GUI.ZRadioButton WeeklyRadioButton;
		private ZArchitecture.GUI.ZRadioButton DailyRadioButton;
		private ZArchitecture.GUI.ZGroupBox RangeOfRecurrenceGroupBox;
		private ZArchitecture.ZLabel RangeToLabel;
		private ZArchitecture.ZLabel RangeFromLabel;
		private ZArchitecture.GUI.ZDateEdit RangeToDateEdit;
		private ZArchitecture.GUI.ZDateEdit RangeFromDateEdit;
		private ZArchitecture.GUI.ZButton SelectButton;
		private ZArchitecture.GUI.ZButton CloseButton;
		private ZArchitecture.GUI.ZPanel ControlButtonsPanel;
		private ZArchitecture.GUI.ZCheckedListBox WeeklyDaysCheckedListBox;
		internal CreateConsolUserControl createConsolUserControl;
	}
}
