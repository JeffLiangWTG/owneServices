using CargoWise.Types;
namespace Enterprise.Freight.Module
{
	partial class BulkScheduleCopyForm
	{
		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		new void InitializeComponent()
		{
			this.ActionButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RecurrencePatternGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MonthlyRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.WeeklyRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.DailyRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.MonthlyPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DayLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MonthsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OfEveryLabel = new Enterprise.ZArchitecture.ZLabel();
			this.NumOfMonthsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DayOfMonthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.WeeklyPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.WeekDaysCheckedList = new Enterprise.ZArchitecture.GUI.ZCheckedListBox();
			this.NumOfWeeksLabel = new Enterprise.ZArchitecture.ZLabel();
			this.NumOfWeeksCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DailyPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DaysLabel = new Enterprise.ZArchitecture.ZLabel();
			this.NumOfDaysCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RangeOfRecurrencePanel = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BulkCopyDateTo = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.BulkCopyDateFrom = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.Message = new Enterprise.ZArchitecture.ZLabel();
			this.RecurrencePatternPanel = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.firstETACommentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.firstETDCommentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FirstETACheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.FirstETDCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ConsolGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CopyRoutingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CopyShipmentsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zCalcDropEdit2 = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.zCalcDropEdit1 = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.zCheckBox2 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zCheckBox1 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RecurrencePatternGroupBox.SuspendLayout();
			this.MonthlyPanel.SuspendLayout();
			this.WeeklyPanel.SuspendLayout();
			this.DailyPanel.SuspendLayout();
			this.RangeOfRecurrencePanel.SuspendLayout();
			this.RecurrencePatternPanel.SuspendLayout();
			this.ConsolGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 478, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 14;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.BulkCopyCriteria);
			// 
			// ActionButton
			// 
			this.ActionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ActionButton.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|710faf2b-af59-4b36-a435-7eb9c79f0fa7", "OK");
			this.ActionButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 446, true);
			this.ActionButton.Name = "ActionButton";
			this.ActionButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 25, true);
			this.ActionButton.TabIndex = 3;
			this.ActionButton.Click += new System.EventHandler(this.ActionButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|0796651f-e695-449d-a0d0-df6d6b13b3c2", "&Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(440, 446, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 25, true);
			this.CloseButton.TabIndex = 4;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// RecurrencePatternGroupBox
			// 
			this.RecurrencePatternGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.RecurrencePatternGroupBox.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|6a5b55e3-5204-4ed9-8262-e2b5617fdec3", "Recurrence Pattern");
			this.RecurrencePatternGroupBox.Controls.Add(this.MonthlyRadioButton);
			this.RecurrencePatternGroupBox.Controls.Add(this.WeeklyRadioButton);
			this.RecurrencePatternGroupBox.Controls.Add(this.DailyRadioButton);
			this.RecurrencePatternGroupBox.Controls.Add(this.WeeklyPanel);
			this.RecurrencePatternGroupBox.Controls.Add(this.DailyPanel);
			this.RecurrencePatternGroupBox.Controls.Add(this.MonthlyPanel);
			this.RecurrencePatternGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 16, true);
			this.RecurrencePatternGroupBox.Name = "RecurrencePatternGroupBox";
			this.RecurrencePatternGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 106, true);
			this.RecurrencePatternGroupBox.TabIndex = 0;
			this.RecurrencePatternGroupBox.TabStop = false;
			// 
			// MonthlyRadioButton
			// 
			this.MonthlyRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.MonthlyRadioButton, "RepititionSelection.UseMonthlyPattern");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).RepititionSelection.UseMonthlyPattern)));
			this.MonthlyRadioButton.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|be75f5ec-bb8b-4ca8-818e-e6e2019c6cbc", "Monthly");
			this.MonthlyRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MonthlyRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 72, true);
			this.MonthlyRadioButton.Name = "MonthlyRadioButton";
			this.MonthlyRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 16, true);
			this.MonthlyRadioButton.TabIndex = 3;
			this.MonthlyRadioButton.CheckedChanged += new System.EventHandler(this.BulkCopyMonthlyPatternInfo_ValueChanged);
			// 
			// WeeklyRadioButton
			// 
			this.WeeklyRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.WeeklyRadioButton, "RepititionSelection.UseWeeklyPattern");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).RepititionSelection.UseWeeklyPattern)));
			this.WeeklyRadioButton.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|a8c593eb-e49c-4b88-b6c6-8d370053b88d", "Weekly");
			this.WeeklyRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.WeeklyRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 48, true);
			this.WeeklyRadioButton.Name = "WeeklyRadioButton";
			this.WeeklyRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 16, true);
			this.WeeklyRadioButton.TabIndex = 2;
			this.WeeklyRadioButton.CheckedChanged += new System.EventHandler(this.BulkCopyWeeklyPatternInfo_ValueChanged);
			// 
			// DailyRadioButton
			// 
			this.DailyRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.DailyRadioButton, "RepititionSelection.UseDailyPattern");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).RepititionSelection.UseDailyPattern)));
			this.DailyRadioButton.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|95e2636a-f184-461e-b9eb-754245158890", "Daily");
			this.DailyRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DailyRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 24, true);
			this.DailyRadioButton.Name = "DailyRadioButton";
			this.DailyRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 16, true);
			this.DailyRadioButton.TabIndex = 1;
			this.DailyRadioButton.CheckedChanged += new System.EventHandler(this.BulkCopyDailyPatternInfo_ValueChanged);
			// 
			// MonthlyPanel
			// 
			this.MonthlyPanel.Controls.Add(this.DayLabel);
			this.MonthlyPanel.Controls.Add(this.MonthsLabel);
			this.MonthlyPanel.Controls.Add(this.OfEveryLabel);
			this.MonthlyPanel.Controls.Add(this.NumOfMonthsCalcEdit);
			this.MonthlyPanel.Controls.Add(this.DayOfMonthCalcEdit);
			this.MonthlyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 16, true);
			this.MonthlyPanel.Name = "MonthlyPanel";
			this.MonthlyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 78, true);
			this.MonthlyPanel.TabIndex = 4;
			// 
			// DayLabel
			// 
			this.DayLabel.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|6fe07d96-dcf2-4441-9356-d3332f07e272", "Day");
			this.DayLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.DayLabel.Name = "DayLabel";
			this.DayLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 16, true);
			this.DayLabel.TabIndex = 5;
			// 
			// MonthsLabel
			// 
			this.MonthsLabel.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|e03719dd-7a53-4e88-b8fc-d3ecbdf90e73", "Month(s)");
			this.MonthsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 8, true);
			this.MonthsLabel.Name = "MonthsLabel";
			this.MonthsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 16, true);
			this.MonthsLabel.TabIndex = 9;
			// 
			// OfEveryLabel
			// 
			this.OfEveryLabel.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|fe5fcb06-8939-4f8b-ba68-070f66406c5c", "Of every");
			this.OfEveryLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 8, true);
			this.OfEveryLabel.Name = "OfEveryLabel";
			this.OfEveryLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 16, true);
			this.OfEveryLabel.TabIndex = 7;
			// 
			// NumOfMonthsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.NumOfMonthsCalcEdit, "RepititionSelection.RecurrenceInterval");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).RepititionSelection.RecurrenceInterval)));
			this.NumOfMonthsCalcEdit.DecimalPlaces = 2;
			this.NumOfMonthsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 8, true);
			this.NumOfMonthsCalcEdit.Name = "NumOfMonthsCalcEdit";
			this.NumOfMonthsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.NumOfMonthsCalcEdit.TabIndex = 8;
			this.NumOfMonthsCalcEdit.Text = "1";
			this.NumOfMonthsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DayOfMonthCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DayOfMonthCalcEdit, "RepititionSelection.DayOfTheMonth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).RepititionSelection.DayOfTheMonth)));
			this.DayOfMonthCalcEdit.DecimalPlaces = 2;
			this.DayOfMonthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 8, true);
			this.DayOfMonthCalcEdit.Name = "DayOfMonthCalcEdit";
			this.DayOfMonthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.DayOfMonthCalcEdit.TabIndex = 6;
			this.DayOfMonthCalcEdit.Text = "1";
			this.DayOfMonthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// WeeklyPanel
			// 
			this.WeeklyPanel.Controls.Add(this.WeekDaysCheckedList);
			this.WeeklyPanel.Controls.Add(this.NumOfWeeksLabel);
			this.WeeklyPanel.Controls.Add(this.NumOfWeeksCalcEdit);
			this.WeeklyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 16, true);
			this.WeeklyPanel.Name = "WeeklyPanel";
			this.WeeklyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 78, true);
			this.WeeklyPanel.TabIndex = 4;
			// 
			// WeekDaysCheckedList
			// 
			this.WeekDaysCheckedList.BackColor = System.Drawing.SystemColors.Control;
			this.WeekDaysCheckedList.BindingItems = null;
			this.BindingSource.SetBindingMember(this.WeekDaysCheckedList, "RepititionSelection.DayOfTheWeek");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.ZBoolDescriptionPairList)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).RepititionSelection.DayOfTheWeek)));
			this.WeekDaysCheckedList.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.WeekDaysCheckedList.CheckOnClick = true;
			this.WeekDaysCheckedList.ColumnWidth = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(88);
			this.WeekDaysCheckedList.Font = new System.Drawing.Font("Tahoma", 8F);
			this.WeekDaysCheckedList.ForeColor = System.Drawing.SystemColors.WindowText;
			this.WeekDaysCheckedList.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 40, true);
			this.WeekDaysCheckedList.MultiColumn = true;
			this.WeekDaysCheckedList.Name = "WeekDaysCheckedList";
			this.WeekDaysCheckedList.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 31, true);
			this.WeekDaysCheckedList.TabIndex = 8;
			this.WeekDaysCheckedList.ThreeDCheckBoxes = true;
			// 
			// NumOfWeeksLabel
			// 
			this.NumOfWeeksLabel.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|56db1f6a-48c8-45c9-a4a3-501f6d0c2584", "Week(s) on");
			this.NumOfWeeksLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 8, true);
			this.NumOfWeeksLabel.Name = "NumOfWeeksLabel";
			this.NumOfWeeksLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 16, true);
			this.NumOfWeeksLabel.TabIndex = 7;
			// 
			// NumOfWeeksCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.NumOfWeeksCalcEdit, "RepititionSelection.RecurrenceInterval");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).RepititionSelection.RecurrenceInterval)));
			this.NumOfWeeksCalcEdit.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|68fa44b1-9bd8-4d0a-bf47-ab5c1c3dbb50", "Recur every");
			this.NumOfWeeksCalcEdit.DecimalPlaces = 0;
			this.NumOfWeeksCalcEdit.Decimals = 0;
			this.NumOfWeeksCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 8, true);
			this.NumOfWeeksCalcEdit.Name = "NumOfWeeksCalcEdit";
			this.NumOfWeeksCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.NumOfWeeksCalcEdit.TabIndex = 6;
			this.NumOfWeeksCalcEdit.Text = "1";
			this.NumOfWeeksCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DailyPanel
			// 
			this.DailyPanel.Controls.Add(this.DaysLabel);
			this.DailyPanel.Controls.Add(this.NumOfDaysCalcEdit);
			this.DailyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 16, true);
			this.DailyPanel.Name = "DailyPanel";
			this.DailyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 78, true);
			this.DailyPanel.TabIndex = 4;
			// 
			// DaysLabel
			// 
			this.DaysLabel.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|863a9631-b4b1-47de-9bd8-f795bf8926e4", "Day(s)");
			this.DaysLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 8, true);
			this.DaysLabel.Name = "DaysLabel";
			this.DaysLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 16, true);
			this.DaysLabel.TabIndex = 7;
			// 
			// NumOfDaysCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.NumOfDaysCalcEdit, "RepititionSelection.RecurrenceInterval");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).RepititionSelection.RecurrenceInterval)));
			this.NumOfDaysCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NumOfDaysCalcEdit, false);
			this.NumOfDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 8, true);
			this.NumOfDaysCalcEdit.Name = "NumOfDaysCalcEdit";
			this.NumOfDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.NumOfDaysCalcEdit.TabIndex = 6;
			this.NumOfDaysCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RangeOfRecurrencePanel
			// 
			this.RangeOfRecurrencePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.RangeOfRecurrencePanel.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|14bad4f2-c225-46d8-9e4a-b683d03b3e5d", "Range Of Recurrence");
			this.RangeOfRecurrencePanel.Controls.Add(this.BulkCopyDateTo);
			this.RangeOfRecurrencePanel.Controls.Add(this.BulkCopyDateFrom);
			this.RangeOfRecurrencePanel.Controls.Add(this.Message);
			this.RangeOfRecurrencePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 232, true);
			this.RangeOfRecurrencePanel.Name = "RangeOfRecurrencePanel";
			this.RangeOfRecurrencePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 92, true);
			this.RangeOfRecurrencePanel.TabIndex = 2;
			this.RangeOfRecurrencePanel.TabStop = false;
			// 
			// BulkCopyDateTo
			// 
			this.BulkCopyDateTo.AutoCompleteMonthThreshold = 1;
			this.BulkCopyDateTo.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.BulkCopyDateTo, "RepititionSelection.ToDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).RepititionSelection.ToDate)));
			this.BulkCopyDateTo.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|c43bef96-a301-4e6d-8b8d-fd227d45ae32", "To");
			this.BulkCopyDateTo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 24, true);
			this.BulkCopyDateTo.Name = "BulkCopyDateTo";
			this.BulkCopyDateTo.TabIndex = 3;
			// 
			// BulkCopyDateFrom
			// 
			this.BulkCopyDateFrom.AutoCompleteMonthThreshold = 1;
			this.BulkCopyDateFrom.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.BulkCopyDateFrom, "RepititionSelection.FromDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).RepititionSelection.FromDate)));
			this.BulkCopyDateFrom.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|81b8aa3e-d180-432e-b974-20182e05906f", "From");
			this.BulkCopyDateFrom.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 24, true);
			this.BulkCopyDateFrom.Name = "BulkCopyDateFrom";
			this.BulkCopyDateFrom.TabIndex = 1;
			// 
			// Message
			// 
			this.Message.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|1e7e654b-1ed7-47f5-9906-73fea16661db", "Schedules can be created a maximum of 6 months in advance");
			this.Message.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 62, true);
			this.Message.Name = "Message";
			this.Message.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 23, true);
			this.Message.TabIndex = 4;
			// 
			// RecurrencePatternPanel
			// 
			this.RecurrencePatternPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.RecurrencePatternPanel.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|e24451b8-be82-43fc-af78-b313466e877b", "Recurrence Pattern");
			this.RecurrencePatternPanel.Controls.Add(this.firstETACommentLabel);
			this.RecurrencePatternPanel.Controls.Add(this.firstETDCommentLabel);
			this.RecurrencePatternPanel.Controls.Add(this.FirstETACheckBox);
			this.RecurrencePatternPanel.Controls.Add(this.FirstETDCheckBox);
			this.RecurrencePatternPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 128, true);
			this.RecurrencePatternPanel.Name = "RecurrencePatternPanel";
			this.RecurrencePatternPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 100, true);
			this.RecurrencePatternPanel.TabIndex = 1;
			this.RecurrencePatternPanel.TabStop = false;
			// 
			// firstETACommentLabel
			// 
			this.firstETACommentLabel.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|e74168aa-e885-4b8e-b61e-fc74d7e371b4", "(The selected recurrence criteria will be applied to the earliest \"Discharge Ports\" entry)");
			this.firstETACommentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 55, true);
			this.firstETACommentLabel.Name = "firstETACommentLabel";
			this.firstETACommentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 38, true);
			this.firstETACommentLabel.TabIndex = 3;
			// 
			// firstETDCommentLabel
			// 
			this.firstETDCommentLabel.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|11f28cb6-a537-45e1-a5d1-9a71dc22db81", "(The selected recurrence criteria will be applied to the earliest \"Load Ports\" entry)");
			this.firstETDCommentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 23, true);
			this.firstETDCommentLabel.Name = "firstETDCommentLabel";
			this.firstETDCommentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 24, true);
			this.firstETDCommentLabel.TabIndex = 1;
			// 
			// FirstETACheckBox
			// 
			this.BindingSource.SetBindingMember(this.FirstETACheckBox, "RepititionSelection.FromFirstETA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).RepititionSelection.FromFirstETA)));
			this.FirstETACheckBox.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|96fe9db7-bdf4-4df2-a53c-0e4c397fe3cd", "First ETA");
			this.FirstETACheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FirstETACheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 56, true);
			this.FirstETACheckBox.Name = "FirstETACheckBox";
			this.FirstETACheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.FirstETACheckBox.TabIndex = 2;
			// 
			// FirstETDCheckBox
			// 
			this.BindingSource.SetBindingMember(this.FirstETDCheckBox, "RepititionSelection.FromFirstETD");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).RepititionSelection.FromFirstETD)));
			this.FirstETDCheckBox.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|afe5b500-efe1-49a9-b267-5a5329f63648", "First ETD");
			this.FirstETDCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FirstETDCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 24, true);
			this.FirstETDCheckBox.Name = "FirstETDCheckBox";
			this.FirstETDCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.FirstETDCheckBox.TabIndex = 0;
			// 
			// ConsolGroupBox
			// 
			this.ConsolGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ConsolGroupBox.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|99339d0b-f68f-416f-82a8-0d7c183f9162", "Consolidation Details");
			this.ConsolGroupBox.Controls.Add(this.CopyRoutingCheckBox);
			this.ConsolGroupBox.Controls.Add(this.CopyShipmentsCheckBox);
			this.ConsolGroupBox.Controls.Add(this.zCalcDropEdit2);
			this.ConsolGroupBox.Controls.Add(this.zCalcDropEdit1);
			this.ConsolGroupBox.Controls.Add(this.zCheckBox2);
			this.ConsolGroupBox.Controls.Add(this.zCheckBox1);
			this.ConsolGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 330, true);
			this.ConsolGroupBox.Name = "ConsolGroupBox";
			this.ConsolGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 107, true);
			this.ConsolGroupBox.TabIndex = 15;
			this.ConsolGroupBox.TabStop = false;
			// 
			// CopyRoutingCheckBox
			// 
			this.CopyRoutingCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CopyRoutingCheckBox, "ConsolDetails.CopyRoutings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).ConsolDetails.CopyRoutings)));
			this.CopyRoutingCheckBox.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|81bbebbe-9655-4346-a876-054c4be56d89", "Copy Transports", "Copy All Transports", "");
			this.CopyRoutingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CopyRoutingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 74, true);
			this.CopyRoutingCheckBox.Name = "CopyRoutingCheckBox";
			this.CopyRoutingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 17, true);
			this.CopyRoutingCheckBox.TabIndex = 5;
			this.CopyRoutingCheckBox.UseVisualStyleBackColor = true;
			// 
			// CopyShipmentsCheckBox
			// 
			this.CopyShipmentsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CopyShipmentsCheckBox, "ConsolDetails.CopyShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).ConsolDetails.CopyShipments)));
			this.CopyShipmentsCheckBox.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|1e071239-5936-476d-b019-e9bdae0b1ab0", "Copy Shipments", "Copy All Shipments", "");
			this.CopyShipmentsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CopyShipmentsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 74, true);
			this.CopyShipmentsCheckBox.Name = "CopyShipmentsCheckBox";
			this.CopyShipmentsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 17, true);
			this.CopyShipmentsCheckBox.TabIndex = 6;
			this.CopyShipmentsCheckBox.UseVisualStyleBackColor = true;
			// 
			// zCalcDropEdit2
			// 
			this.BindingSource.SetBindingMember(this.zCalcDropEdit2, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).ConsolDetails.Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).ConsolDetails.VolumeUnit)));
			this.zCalcDropEdit2.BindToAmount = "ConsolDetails.Volume";
			this.zCalcDropEdit2.BindToUnit = "ConsolDetails.VolumeUnit";
			this.zCalcDropEdit2.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|da30c828-e7f9-4128-887f-c1664cf75d21", "Pre-Allocated Volume");
			this.zCalcDropEdit2.Decimals = 2;
			this.zCalcDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 49, true);
			this.zCalcDropEdit2.Name = "zCalcDropEdit2";
			this.zCalcDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.zCalcDropEdit2.TabIndex = 4;
			this.zCalcDropEdit2.UnitPreBoundMaxLength = 2;
			// 
			// zCalcDropEdit1
			// 
			this.BindingSource.SetBindingMember(this.zCalcDropEdit1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).ConsolDetails.Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).ConsolDetails.WeightUnit)));
			this.zCalcDropEdit1.BindToAmount = "ConsolDetails.Weight";
			this.zCalcDropEdit1.BindToUnit = "ConsolDetails.WeightUnit";
			this.zCalcDropEdit1.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|0c24fb1d-f448-45cd-ba2b-a2c25bc2dece", "Pre-Allocated Weight");
			this.zCalcDropEdit1.Decimals = 2;
			this.zCalcDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 24, true);
			this.zCalcDropEdit1.Name = "zCalcDropEdit1";
			this.zCalcDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.zCalcDropEdit1.TabIndex = 2;
			this.zCalcDropEdit1.UnitPreBoundMaxLength = 2;
			// 
			// zCheckBox2
			// 
			this.zCheckBox2.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zCheckBox2, "ConsolDetails.AllocateNeutralMaster");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).ConsolDetails.AllocateNeutralMaster)));
			this.zCheckBox2.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|28baa0ac-87ad-4861-ad31-aa1b044099eb", "Allocation Neutral Master Automatically");
			this.zCheckBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 49, true);
			this.zCheckBox2.Name = "zCheckBox2";
			this.zCheckBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 17, true);
			this.zCheckBox2.TabIndex = 1;
			this.zCheckBox2.UseVisualStyleBackColor = true;
			// 
			// zCheckBox1
			// 
			this.zCheckBox1.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zCheckBox1, "ConsolDetails.CreateConsol");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).ConsolDetails.CreateConsol)));
			this.zCheckBox1.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|33141166-e094-4b84-b35c-ad65b26324f4", "Create and Pre-Allocate Consolidation");
			this.zCheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 26, true);
			this.zCheckBox1.Name = "zCheckBox1";
			this.zCheckBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 17, true);
			this.zCheckBox1.TabIndex = 0;
			this.zCheckBox1.UseVisualStyleBackColor = true;
			// 
			// BulkScheduleCopyForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 502, true);
			this.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("BulkScheduleCopyForm|1bd619d7-d500-46d9-944b-38528bc7aa1d", "Bulk Schedule Copy");
			this.Controls.Add(this.ConsolGroupBox);
			this.Controls.Add(this.RecurrencePatternPanel);
			this.Controls.Add(this.RangeOfRecurrencePanel);
			this.Controls.Add(this.RecurrencePatternGroupBox);
			this.Controls.Add(this.ActionButton);
			this.Controls.Add(this.CloseButton);
			this.DataSourceType = typeof(Enterprise.Freight.Business.BulkCopyCriteria);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "BulkScheduleCopyForm";
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.ActionButton, 0);
			this.Controls.SetChildIndex(this.RecurrencePatternGroupBox, 0);
			this.Controls.SetChildIndex(this.RangeOfRecurrencePanel, 0);
			this.Controls.SetChildIndex(this.RecurrencePatternPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ConsolGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RecurrencePatternGroupBox.ResumeLayout(false);
			this.MonthlyPanel.ResumeLayout(false);
			this.MonthlyPanel.PerformLayout();
			this.WeeklyPanel.ResumeLayout(false);
			this.WeeklyPanel.PerformLayout();
			this.DailyPanel.ResumeLayout(false);
			this.DailyPanel.PerformLayout();
			this.RangeOfRecurrencePanel.ResumeLayout(false);
			this.RecurrencePatternPanel.ResumeLayout(false);
			this.ConsolGroupBox.ResumeLayout(false);
			this.ConsolGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		private Enterprise.ZArchitecture.GUI.ZButton ActionButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox RecurrencePatternGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox RangeOfRecurrencePanel;
		private Enterprise.ZArchitecture.ZLabel Message;
		private Enterprise.ZArchitecture.GUI.ZGroupBox RecurrencePatternPanel;
		private Enterprise.ZArchitecture.ZLabel firstETDCommentLabel;
		private Enterprise.ZArchitecture.GUI.ZPanel DailyPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel WeeklyPanel;
		private Enterprise.ZArchitecture.ZLabel NumOfWeeksLabel;
		private Enterprise.ZArchitecture.GUI.ZPanel MonthlyPanel;
		private Enterprise.ZArchitecture.GUI.ZRadioButton MonthlyRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton WeeklyRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton DailyRadioButton;
		private Enterprise.ZArchitecture.GUI.ZDateEdit BulkCopyDateFrom;
		private Enterprise.ZArchitecture.GUI.ZDateEdit BulkCopyDateTo;
		private Enterprise.ZArchitecture.GUI.ZCheckBox FirstETDCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox FirstETACheckBox;
		private Enterprise.ZArchitecture.ZCalcEdit NumOfDaysCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit NumOfWeeksCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit DayOfMonthCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit NumOfMonthsCalcEdit;
		private Enterprise.ZArchitecture.ZLabel OfEveryLabel;
		private Enterprise.ZArchitecture.ZLabel MonthsLabel;
		private Enterprise.ZArchitecture.ZLabel DayLabel;
		private Enterprise.ZArchitecture.ZLabel DaysLabel;
		private Enterprise.ZArchitecture.GUI.ZCheckedListBox WeekDaysCheckedList;
		private Enterprise.ZArchitecture.ZLabel firstETACommentLabel;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ConsolGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit zCalcDropEdit2;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit zCalcDropEdit1;
		private Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBox2;
		private Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBox1;
		private Enterprise.ZArchitecture.GUI.ZCheckBox CopyShipmentsCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox CopyRoutingCheckBox;
	}
}
