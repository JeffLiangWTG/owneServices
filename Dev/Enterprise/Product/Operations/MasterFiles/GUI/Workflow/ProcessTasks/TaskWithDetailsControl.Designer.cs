namespace Enterprise.MasterFiles.GUI
{
	public partial class TaskWithDetailsControl
	{
		#region Component Designer Generated Code

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.NotesTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.ActualDurTimeEdit = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.ActualStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateTimeOffsetEdit();
			this.ScheduleDateEdit = new Enterprise.ZArchitecture.GUI.ZDateTimeOffsetEdit();
			this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GroupFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SequenceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.IDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StaffFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TaskTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TasksSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.TasksControl = new Enterprise.MasterFiles.GUI.TasksControl();
			this.DetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.SimpleViewTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SimpleNotesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SimpleNotesRichTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.SimpleScheduleGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IsInterruptableCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SimpleEstVariationCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SimpleWorkflowGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.SimpleHighEstTimeEditEx = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.SimpleEstTimeEditEx = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.SimpleActDurTimeEditEx = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.SimpleDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SimpleStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SimpleCapabilityGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SimpleTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SimpleDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SimpleGroupGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SimpleStaffCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.AdvancedViewTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ScheduleGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ProcessHeaderDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.ReminderCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.HighEstDurTimeEditEx = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.LastEstTimeToCompleteTimeEditEx = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.CompletedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateTimeOffsetEdit();
			this.EstimateVariationFactorCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.EstTimeEditEx = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CapabilityFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.NotesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ExtraResourcesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ExtraResourcesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.NotesTextBox.SuspendLayout();
			this.ActualStartDateEdit.SuspendLayout();
			this.ScheduleDateEdit.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			this.GroupFindBox.SuspendLayout();
			this.StaffFindBox.SuspendLayout();
			this.TaskTypeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TasksSplitContainer)).BeginInit();
			this.TasksSplitContainer.Panel1.SuspendLayout();
			this.TasksSplitContainer.Panel2.SuspendLayout();
			this.TasksSplitContainer.SuspendLayout();
			this.TasksControl.SuspendLayout();
			this.DetailsTabControl.SuspendLayout();
			this.SimpleViewTabPage.SuspendLayout();
			this.SimpleNotesGroupBox.SuspendLayout();
			this.SimpleNotesRichTextBox.SuspendLayout();
			this.SimpleScheduleGroupBox.SuspendLayout();
			this.SimpleWorkflowGuidDropEdit.SuspendLayout();
			this.SimpleDetailsGroupBox.SuspendLayout();
			this.SimpleStatusDropEdit.SuspendLayout();
			this.SimpleCapabilityGuidFindBox.SuspendLayout();
			this.SimpleTypeDropEdit.SuspendLayout();
			this.SimpleGroupGuidFindBox.SuspendLayout();
			this.SimpleStaffCodeFindBox.SuspendLayout();
			this.AdvancedViewTabPage.SuspendLayout();
			this.ScheduleGroupBox.SuspendLayout();
			this.ProcessHeaderDropEdit.SuspendLayout();
			this.CompletedDateEdit.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.CapabilityFindBox.SuspendLayout();
			this.NotesGroupBox.SuspendLayout();
			this.ExtraResourcesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ExtraResourcesGrid)).BeginInit();
			this.ExtraResourcesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ProcessTask);
			// 
			// NotesTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotesTextBox, "P9_Notes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_Notes)));
			this.NotesTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NotesTextBox, false);
			this.NotesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.NotesTextBox.MaxLength = 10000000;
			this.NotesTextBox.Name = "NotesTextBox";
			this.NotesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(447, 87, true);
			this.NotesTextBox.TabIndex = 0;
			// 
			// ActualDurTimeEdit
			// 
			this.ActualDurTimeEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ActualDurTimeEdit, "P9_ActualDuration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_ActualDuration)));
			this.ActualDurTimeEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TaskWithDetailsControl|0bae2aa6-2949-4d9a-bd39-0f1ad09b402f", "Actual Duration");
			this.ActualDurTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 59, true);
			this.ActualDurTimeEdit.Name = "ActualDurTimeEdit";
			this.ActualDurTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.ActualDurTimeEdit.TabIndex = 4;
			// 
			// ActualStartDateEdit
			// 
			this.ActualStartDateEdit.AllowDrop = true;
			this.ActualStartDateEdit.AutoCompleteMonthThreshold = 1;
			this.ActualStartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ActualStartDateEdit, "P9_ActualDateForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_ActualDateForBinding)));
			this.ActualStartDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f160d418-8d2f-4e9d-b087-6a4a74db6ab9", "Actual Start");
			this.ActualStartDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ActualStartDateEdit.HasTimeZoneFindBox = true;
			this.ActualStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 108, true);
			this.ActualStartDateEdit.Name = "ActualStartDateEdit";
			this.ActualStartDateEdit.TabIndex = 7;
			// 
			// ScheduleDateEdit
			// 
			this.ScheduleDateEdit.AllowDrop = true;
			this.ScheduleDateEdit.AutoCompleteMonthThreshold = 1;
			this.ScheduleDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ScheduleDateEdit, "P9_ScheduledDateForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_ScheduledDateForBinding)));
			this.ScheduleDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ScheduleDateEdit.HasTimeZoneFindBox = true;
			this.ScheduleDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 11, true);
			this.ScheduleDateEdit.Name = "ScheduleDateEdit";
			this.ScheduleDateEdit.TabIndex = 0;
			// 
			// StatusDropEdit
			// 
			this.StatusDropEdit.AllowDrop = true;
			this.StatusDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "P9_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_Status)));
			this.StatusDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TaskWithDetailsControl|5fa931be-104d-495f-80ee-a6c8916b325b", "Status");
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 82, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.PreBoundMaxLength = 3;
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 20, true);
			this.StatusDropEdit.TabIndex = 4;
			// 
			// GroupFindBox
			// 
			this.GroupFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GroupFindBox, "P9_GG_AssignedGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_GG_AssignedGroup)));
			this.GroupFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TaskWithDetailsControl|2b94b79d-62e8-4dc1-8235-53e517df7fa9", "Group", "Group assigned to this task");
			this.GroupFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 132, true);
			this.GroupFindBox.Name = "GroupFindBox";
			this.GroupFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.GroupFindBox.TabIndex = 6;
			// 
			// SequenceCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SequenceCalcEdit, "P9_Sequence");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_Sequence)));
			this.SequenceCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TaskWithDetailsControl|f790f0f7-ed3b-43ae-8793-f80f0b354176", "Sequence Number");
			this.SequenceCalcEdit.DecimalPlaces = 0;
			this.SequenceCalcEdit.Decimals = 0;
			this.SequenceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(347, 11, true);
			this.SequenceCalcEdit.Name = "SequenceCalcEdit";
			this.SequenceCalcEdit.ShowGroupSeparators = false;
			this.SequenceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.SequenceCalcEdit.TabIndex = 1;
			this.SequenceCalcEdit.Text = "0";
			this.SequenceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// IDTextBox
			// 
			this.BindingSource.SetBindingMember(this.IDTextBox, "P9_TaskID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_TaskID)));
			this.IDTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TaskWithDetailsControl|a00e1117-1192-4556-a700-d15e36d49347", "ID");
			this.IDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 11, true);
			this.IDTextBox.Name = "IDTextBox";
			this.IDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.IDTextBox.TabIndex = 0;
			// 
			// StaffFindBox
			// 
			this.StaffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StaffFindBox, "P9_GS_NKAssignedStaffMember");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_GS_NKAssignedStaffMember)));
			this.StaffFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TaskWithDetailsControl|76cb4dfa-eec6-4274-9219-ecff35369314", "Staff", "Staff assigned to this task");
			this.StaffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 108, true);
			this.StaffFindBox.Name = "StaffFindBox";
			this.StaffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.StaffFindBox.TabIndex = 5;
			// 
			// TaskTypeDropEdit
			// 
			this.TaskTypeDropEdit.AllowDrop = true;
			this.TaskTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TaskTypeDropEdit, "P9_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_Type)));
			this.TaskTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TaskWithDetailsControl|f81aa35d-a6ea-48aa-a561-b8337a7b2812", "Type");
			this.TaskTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 58, true);
			this.TaskTypeDropEdit.Name = "TaskTypeDropEdit";
			this.TaskTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 20, true);
			this.TaskTypeDropEdit.TabIndex = 3;
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "P9_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_Description)));
			this.DescriptionTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TaskWithDetailsControl|4e9f8091-d25b-4082-a4b7-8929061406f7", "Desc.", "Task Description");
			this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 34, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 20, true);
			this.DescriptionTextBox.TabIndex = 2;
			// 
			// TasksSplitContainer
			// 
			this.TasksSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TasksSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TasksSplitContainer.Name = "TasksSplitContainer";
			this.TasksSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// TasksSplitContainer.Panel1
			// 
			this.TasksSplitContainer.Panel1.Controls.Add(this.TasksControl);
			this.TasksSplitContainer.Panel1MinSize = 100;
			// 
			// TasksSplitContainer.Panel2
			// 
			this.TasksSplitContainer.Panel2.Controls.Add(this.DetailsTabControl);
			this.TasksSplitContainer.Panel2MinSize = 160;
			this.TasksSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(845, 515, true);
			this.TasksSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(182);
			this.TasksSplitContainer.TabIndex = 5;
			// 
			// TasksControl
			// 
			this.TasksControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TasksControl, ".");
			this.TasksControl.BindTo = "Tasks";
			this.TasksControl.CreateTasksFromTemplateLinkVisible = true;
			this.TasksControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TasksControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TasksControl.Name = "TasksControl";
			this.TasksControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(845, 182, true);
			this.TasksControl.TabIndex = 0;
			// 
			// DetailsTabControl
			// 
			this.DetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.DetailsTabControl.Controls.Add(this.SimpleViewTabPage);
			this.DetailsTabControl.Controls.Add(this.AdvancedViewTabPage);
			this.DetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsTabControl.Name = "DetailsTabControl";
			this.DetailsTabControl.SelectedIndex = 0;
			this.DetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(845, 329, true);
			this.DetailsTabControl.TabIndex = 5;
			this.DetailsTabControl.SelectedIndexChanged += new System.EventHandler(this.DetailsTabControl_SelectedIndexChanged);
			// 
			// SimpleViewTabPage
			// 
			this.SimpleViewTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.SimpleViewTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7118c35e-9303-4502-b18d-8e4a81ccecc0", "Simple View");
			this.SimpleViewTabPage.Controls.Add(this.SimpleNotesGroupBox);
			this.SimpleViewTabPage.Controls.Add(this.SimpleScheduleGroupBox);
			this.SimpleViewTabPage.Controls.Add(this.SimpleDetailsGroupBox);
			this.SimpleViewTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SimpleViewTabPage.Name = "SimpleViewTabPage";
			this.SimpleViewTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SimpleViewTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(837, 302, true);
			this.SimpleViewTabPage.TabIndex = 0;
			// 
			// SimpleNotesGroupBox
			// 
			this.SimpleNotesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.SimpleNotesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1bd90e0d-5889-4225-97e3-671eee085de8", "Notes");
			this.SimpleNotesGroupBox.Controls.Add(this.SimpleNotesRichTextBox);
			this.SimpleNotesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 171, true);
			this.SimpleNotesGroupBox.Name = "SimpleNotesGroupBox";
			this.SimpleNotesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 128, true);
			this.SimpleNotesGroupBox.TabIndex = 16;
			this.SimpleNotesGroupBox.TabStop = false;
			// 
			// SimpleNotesRichTextBox
			// 
			this.BindingSource.SetBindingMember(this.SimpleNotesRichTextBox, "P9_Notes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_Notes)));
			this.SimpleNotesRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SimpleNotesRichTextBox, false);
			this.SimpleNotesRichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SimpleNotesRichTextBox.MaxLength = 10000000;
			this.SimpleNotesRichTextBox.Name = "SimpleNotesRichTextBox";
			this.SimpleNotesRichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 109, true);
			this.SimpleNotesRichTextBox.TabIndex = 1;
			// 
			// SimpleScheduleGroupBox
			// 
			this.SimpleScheduleGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.SimpleScheduleGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("603b8144-ead6-4034-b843-12d9dde703ad", "Schedule");
			this.SimpleScheduleGroupBox.Controls.Add(this.IsInterruptableCheckBox);
			this.SimpleScheduleGroupBox.Controls.Add(this.SimpleEstVariationCalcEdit);
			this.SimpleScheduleGroupBox.Controls.Add(this.SimpleWorkflowGuidDropEdit);
			this.SimpleScheduleGroupBox.Controls.Add(this.SimpleHighEstTimeEditEx);
			this.SimpleScheduleGroupBox.Controls.Add(this.SimpleEstTimeEditEx);
			this.SimpleScheduleGroupBox.Controls.Add(this.SimpleActDurTimeEditEx);
			this.SimpleScheduleGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(484, 1, true);
			this.SimpleScheduleGroupBox.Name = "SimpleScheduleGroupBox";
			this.SimpleScheduleGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 164, true);
			this.SimpleScheduleGroupBox.TabIndex = 15;
			this.SimpleScheduleGroupBox.TabStop = false;
			// 
			// IsInterruptableCheckBox
			// 
			this.IsInterruptableCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsInterruptableCheckBox, "P9_IsInterruptable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_IsInterruptable)));
			this.IsInterruptableCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsInterruptableCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 114, true);
			this.IsInterruptableCheckBox.Name = "IsInterruptableCheckBox";
			this.IsInterruptableCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 17, true);
			this.IsInterruptableCheckBox.TabIndex = 4;
			this.IsInterruptableCheckBox.UseVisualStyleBackColor = true;
			// 
			// SimpleEstVariationCalcEdit
			// 
			this.SimpleEstVariationCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.SimpleEstVariationCalcEdit, "P9_EstimateVariationFactor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_EstimateVariationFactor)));
			this.SimpleEstVariationCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0d5c9754-34dd-4e39-8565-9b371d496453", "Est. Variation Factor");
			this.SimpleEstVariationCalcEdit.DecimalPlaces = 1;
			this.SimpleEstVariationCalcEdit.Decimals = 1;
			this.SimpleEstVariationCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 39, true);
			this.SimpleEstVariationCalcEdit.Name = "SimpleEstVariationCalcEdit";
			this.SimpleEstVariationCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.SimpleEstVariationCalcEdit.TabIndex = 1;
			this.SimpleEstVariationCalcEdit.Text = "0.0";
			this.SimpleEstVariationCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SimpleWorkflowGuidDropEdit
			// 
			this.SimpleWorkflowGuidDropEdit.AllowDrop = true;
			this.SimpleWorkflowGuidDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SimpleWorkflowGuidDropEdit, "P9_FH_ProcessHeader");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_FH_ProcessHeader)));
			this.SimpleWorkflowGuidDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SimpleWorkflowGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 137, true);
			this.SimpleWorkflowGuidDropEdit.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 0, true);
			this.SimpleWorkflowGuidDropEdit.Name = "SimpleWorkflowGuidDropEdit";
			this.SimpleWorkflowGuidDropEdit.PreBoundMaxLength = 28;
			this.SimpleWorkflowGuidDropEdit.ShowDescriptionBox = false;
			this.SimpleWorkflowGuidDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.SimpleWorkflowGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.SimpleWorkflowGuidDropEdit.TabIndex = 5;
			// 
			// SimpleHighEstTimeEditEx
			// 
			this.SimpleHighEstTimeEditEx.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.SimpleHighEstTimeEditEx, "HighEstimatedDuration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).HighEstimatedDuration)));
			this.SimpleHighEstTimeEditEx.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("45307278-2e5d-44fc-be8c-a7af79b1a24b", "High Est. Duration");
			this.SimpleHighEstTimeEditEx.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 64, true);
			this.SimpleHighEstTimeEditEx.Name = "SimpleHighEstTimeEditEx";
			this.SimpleHighEstTimeEditEx.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.SimpleHighEstTimeEditEx.TabIndex = 2;
			// 
			// SimpleEstTimeEditEx
			// 
			this.SimpleEstTimeEditEx.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.SimpleEstTimeEditEx, "P9_EstDuration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_EstDuration)));
			this.SimpleEstTimeEditEx.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("42155ce0-b8da-4c38-96ae-a725823b7367", "Low Est. Duration");
			this.SimpleEstTimeEditEx.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 15, true);
			this.SimpleEstTimeEditEx.Name = "SimpleEstTimeEditEx";
			this.SimpleEstTimeEditEx.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.SimpleEstTimeEditEx.TabIndex = 0;
			// 
			// SimpleActDurTimeEditEx
			// 
			this.SimpleActDurTimeEditEx.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.SimpleActDurTimeEditEx, "P9_ActualDuration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_ActualDuration)));
			this.SimpleActDurTimeEditEx.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 88, true);
			this.SimpleActDurTimeEditEx.Name = "SimpleActDurTimeEditEx";
			this.SimpleActDurTimeEditEx.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.SimpleActDurTimeEditEx.TabIndex = 3;
			// 
			// SimpleDetailsGroupBox
			// 
			this.SimpleDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5b63a9b4-ecd6-4e06-b93d-0d752b498a50", "Details");
			this.SimpleDetailsGroupBox.Controls.Add(this.SimpleStatusDropEdit);
			this.SimpleDetailsGroupBox.Controls.Add(this.SimpleCapabilityGuidFindBox);
			this.SimpleDetailsGroupBox.Controls.Add(this.SimpleTypeDropEdit);
			this.SimpleDetailsGroupBox.Controls.Add(this.SimpleDescriptionTextBox);
			this.SimpleDetailsGroupBox.Controls.Add(this.SimpleGroupGuidFindBox);
			this.SimpleDetailsGroupBox.Controls.Add(this.SimpleStaffCodeFindBox);
			this.SimpleDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.SimpleDetailsGroupBox.Name = "SimpleDetailsGroupBox";
			this.SimpleDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 164, true);
			this.SimpleDetailsGroupBox.TabIndex = 14;
			this.SimpleDetailsGroupBox.TabStop = false;
			// 
			// SimpleStatusDropEdit
			// 
			this.SimpleStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SimpleStatusDropEdit, "P9_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_Status)));
			this.SimpleStatusDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("70e3a82f-30e1-4bb7-ad89-611cdb07a0b9", "Status");
			this.SimpleStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 62, true);
			this.SimpleStatusDropEdit.Name = "SimpleStatusDropEdit";
			this.SimpleStatusDropEdit.PreBoundMaxLength = 3;
			this.SimpleStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 20, true);
			this.SimpleStatusDropEdit.TabIndex = 3;
			// 
			// SimpleCapabilityGuidFindBox
			// 
			this.SimpleCapabilityGuidFindBox.AllowDrop = true;
			this.SimpleCapabilityGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SimpleCapabilityGuidFindBox, "P9_G4_RequiredCapability");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_G4_RequiredCapability)));
			this.SimpleCapabilityGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 135, true);
			this.SimpleCapabilityGuidFindBox.Name = "SimpleCapabilityGuidFindBox";
			this.SimpleCapabilityGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 20, true);
			this.SimpleCapabilityGuidFindBox.TabIndex = 6;
			// 
			// SimpleTypeDropEdit
			// 
			this.SimpleTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SimpleTypeDropEdit, "P9_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_Type)));
			this.SimpleTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ae257e11-9b16-4a06-9c7d-c63ec230568a", "Type");
			this.SimpleTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 38, true);
			this.SimpleTypeDropEdit.Name = "SimpleTypeDropEdit";
			this.SimpleTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 20, true);
			this.SimpleTypeDropEdit.TabIndex = 2;
			// 
			// SimpleDescriptionTextBox
			// 
			this.SimpleDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SimpleDescriptionTextBox, "P9_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_Description)));
			this.SimpleDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SimpleDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 14, true);
			this.SimpleDescriptionTextBox.Name = "SimpleDescriptionTextBox";
			this.SimpleDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
			this.SimpleDescriptionTextBox.TabIndex = 1;
			// 
			// SimpleGroupGuidFindBox
			// 
			this.SimpleGroupGuidFindBox.AllowDrop = true;
			this.SimpleGroupGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SimpleGroupGuidFindBox, "P9_GG_AssignedGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_GG_AssignedGroup)));
			this.SimpleGroupGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("12c9bf1d-f24f-4f9c-a375-bb69a82f9da5", "Group");
			this.SimpleGroupGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 111, true);
			this.SimpleGroupGuidFindBox.Name = "SimpleGroupGuidFindBox";
			this.SimpleGroupGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 20, true);
			this.SimpleGroupGuidFindBox.TabIndex = 5;
			// 
			// SimpleStaffCodeFindBox
			// 
			this.SimpleStaffCodeFindBox.AllowDrop = true;
			this.SimpleStaffCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SimpleStaffCodeFindBox, "P9_GS_NKAssignedStaffMember");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_GS_NKAssignedStaffMember)));
			this.SimpleStaffCodeFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e29116d7-f125-4bfb-973d-1dfbd45b35ba", "Staff");
			this.SimpleStaffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 87, true);
			this.SimpleStaffCodeFindBox.Name = "SimpleStaffCodeFindBox";
			this.SimpleStaffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 20, true);
			this.SimpleStaffCodeFindBox.TabIndex = 4;
			// 
			// AdvancedViewTabPage
			// 
			this.AdvancedViewTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.AdvancedViewTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ab12d56c-0655-4f6d-b0d8-9a4075c46ed3", "Advanced View");
			this.AdvancedViewTabPage.Controls.Add(this.ScheduleGroupBox);
			this.AdvancedViewTabPage.Controls.Add(this.DetailsGroupBox);
			this.AdvancedViewTabPage.Controls.Add(this.NotesGroupBox);
			this.AdvancedViewTabPage.Controls.Add(this.ExtraResourcesGroupBox);
			this.AdvancedViewTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AdvancedViewTabPage.Name = "AdvancedViewTabPage";
			this.AdvancedViewTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AdvancedViewTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(837, 302, true);
			this.AdvancedViewTabPage.TabIndex = 1;
			// 
			// ScheduleGroupBox
			// 
			this.ScheduleGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ScheduleGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TaskWithDetailsControl|f203d5c3-ca1a-4edf-b3e7-4e7c120bc560", "Schedule");
			this.ScheduleGroupBox.Controls.Add(this.ProcessHeaderDropEdit);
			this.ScheduleGroupBox.Controls.Add(this.ReminderCheckBox);
			this.ScheduleGroupBox.Controls.Add(this.HighEstDurTimeEditEx);
			this.ScheduleGroupBox.Controls.Add(this.LastEstTimeToCompleteTimeEditEx);
			this.ScheduleGroupBox.Controls.Add(this.CompletedDateEdit);
			this.ScheduleGroupBox.Controls.Add(this.EstimateVariationFactorCalcEdit);
			this.ScheduleGroupBox.Controls.Add(this.EstTimeEditEx);
			this.ScheduleGroupBox.Controls.Add(this.ScheduleDateEdit);
			this.ScheduleGroupBox.Controls.Add(this.ActualStartDateEdit);
			this.ScheduleGroupBox.Controls.Add(this.ActualDurTimeEdit);
			this.ScheduleGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 3, true);
			this.ScheduleGroupBox.Name = "ScheduleGroupBox";
			this.ScheduleGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 184, true);
			this.ScheduleGroupBox.TabIndex = 1;
			this.ScheduleGroupBox.TabStop = false;
			// 
			// ProcessHeaderDropEdit
			// 
			this.ProcessHeaderDropEdit.AllowDrop = true;
			this.ProcessHeaderDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ProcessHeaderDropEdit, "P9_FH_ProcessHeader");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_FH_ProcessHeader)));
			this.ProcessHeaderDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ProcessHeaderDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 158, true);
			this.ProcessHeaderDropEdit.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 0, true);
			this.ProcessHeaderDropEdit.Name = "ProcessHeaderDropEdit";
			this.ProcessHeaderDropEdit.PreBoundMaxLength = 41;
			this.ProcessHeaderDropEdit.ShowDescriptionBox = false;
			this.ProcessHeaderDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.ProcessHeaderDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			this.ProcessHeaderDropEdit.TabIndex = 9;
			// 
			// ReminderCheckBox
			// 
			this.ReminderCheckBox.AutoSize = true;
			this.ReminderCheckBox.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.ReminderCheckBox, "P9_IsCalendarItem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_IsCalendarItem)));
			this.ReminderCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TaskWithDetailsControl|90807e6d-dcb2-4ba6-95f8-c12598f3f999", "Reminder");
			this.ReminderCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ReminderCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(341, 13, true);
			this.ReminderCheckBox.Name = "ReminderCheckBox";
			this.ReminderCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ReminderCheckBox.TabIndex = 1;
			this.ReminderCheckBox.UseCompatibleTextRendering = true;
			this.ReminderCheckBox.UseVisualStyleBackColor = false;
			// 
			// HighEstDurTimeEditEx
			// 
			this.HighEstDurTimeEditEx.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.HighEstDurTimeEditEx, "HighEstimatedDuration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).HighEstimatedDuration)));
			this.HighEstDurTimeEditEx.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 85, true);
			this.HighEstDurTimeEditEx.Name = "HighEstDurTimeEditEx";
			this.HighEstDurTimeEditEx.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.HighEstDurTimeEditEx.TabIndex = 6;
			// 
			// LastEstTimeToCompleteTimeEditEx
			// 
			this.LastEstTimeToCompleteTimeEditEx.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.LastEstTimeToCompleteTimeEditEx, "P9_EstimatedTimeToComplete");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_EstimatedTimeToComplete)));
			this.LastEstTimeToCompleteTimeEditEx.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(374, 35, true);
			this.LastEstTimeToCompleteTimeEditEx.Name = "LastEstTimeToCompleteTimeEditEx";
			this.LastEstTimeToCompleteTimeEditEx.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.LastEstTimeToCompleteTimeEditEx.TabIndex = 3;
			// 
			// CompletedDateEdit
			// 
			this.CompletedDateEdit.AllowDrop = true;
			this.CompletedDateEdit.AutoCompleteMonthThreshold = 1;
			this.CompletedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CompletedDateEdit, "P9_CompletedTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_CompletedTime)));
			this.CompletedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.CompletedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 133, true);
			this.CompletedDateEdit.Name = "CompletedDateEdit";
			this.CompletedDateEdit.TabIndex = 8;
			// 
			// EstimateVariationFactorCalcEdit
			// 
			this.EstimateVariationFactorCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.EstimateVariationFactorCalcEdit, "P9_EstimateVariationFactor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_EstimateVariationFactor)));
			this.EstimateVariationFactorCalcEdit.DecimalPlaces = 1;
			this.EstimateVariationFactorCalcEdit.Decimals = 1;
			this.EstimateVariationFactorCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(374, 59, true);
			this.EstimateVariationFactorCalcEdit.Name = "EstimateVariationFactorCalcEdit";
			this.EstimateVariationFactorCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.EstimateVariationFactorCalcEdit.TabIndex = 5;
			this.EstimateVariationFactorCalcEdit.Text = "0.0";
			this.EstimateVariationFactorCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// EstTimeEditEx
			// 
			this.EstTimeEditEx.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.EstTimeEditEx, "P9_EstDuration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_EstDuration)));
			this.EstTimeEditEx.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 35, true);
			this.EstTimeEditEx.Name = "EstTimeEditEx";
			this.EstTimeEditEx.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.EstTimeEditEx.TabIndex = 2;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TaskWithDetailsControl|b3904e97-d6e5-465b-9ddb-7152a4f5b698", "Details");
			this.DetailsGroupBox.Controls.Add(this.CapabilityFindBox);
			this.DetailsGroupBox.Controls.Add(this.DescriptionTextBox);
			this.DetailsGroupBox.Controls.Add(this.StaffFindBox);
			this.DetailsGroupBox.Controls.Add(this.GroupFindBox);
			this.DetailsGroupBox.Controls.Add(this.TaskTypeDropEdit);
			this.DetailsGroupBox.Controls.Add(this.IDTextBox);
			this.DetailsGroupBox.Controls.Add(this.SequenceCalcEdit);
			this.DetailsGroupBox.Controls.Add(this.StatusDropEdit);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 184, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			// 
			// CapabilityFindBox
			// 
			this.CapabilityFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CapabilityFindBox, "P9_G4_RequiredCapability");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_G4_RequiredCapability)));
			this.CapabilityFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f505a168-1465-4098-9072-6e09c3ec4611", "Capability", "Capability required for this task");
			this.CapabilityFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 158, true);
			this.CapabilityFindBox.Name = "CapabilityFindBox";
			this.CapabilityFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.CapabilityFindBox.TabIndex = 7;
			// 
			// NotesGroupBox
			// 
			this.NotesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.NotesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TaskWithDetailsControl|21a23e76-efea-4bf1-9194-c30d3cdf6a97", "Notes");
			this.NotesGroupBox.Controls.Add(this.NotesTextBox);
			this.NotesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 193, true);
			this.NotesGroupBox.Name = "NotesGroupBox";
			this.NotesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(453, 106, true);
			this.NotesGroupBox.TabIndex = 2;
			this.NotesGroupBox.TabStop = false;
			// 
			// ExtraResourcesGroupBox
			// 
			this.ExtraResourcesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ExtraResourcesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TaskWithDetailsControl|0f2d61fc-78de-4c8a-86b2-c7855acf0362", "Extra Resources");
			this.ExtraResourcesGroupBox.Controls.Add(this.ExtraResourcesGrid);
			this.ExtraResourcesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(461, 193, true);
			this.ExtraResourcesGroupBox.Name = "ExtraResourcesGroupBox";
			this.ExtraResourcesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 103, true);
			this.ExtraResourcesGroupBox.TabIndex = 3;
			this.ExtraResourcesGroupBox.TabStop = false;
			// 
			// ExtraResourcesGrid
			// 
			this.ExtraResourcesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ExtraResourcesGrid, "ExtraResources");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).ExtraResources)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTaskExtraResource)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).ExtraResources)).SyncRoot)).PE_GS_NKStaffOrResource)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTaskExtraResource)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).ExtraResources)).SyncRoot)).StaffOrResource.GS_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTaskExtraResource)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).ExtraResources)).SyncRoot)).PE_ResourceComment)));
			this.ExtraResourcesGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "PE_GS_NKStaffOrResource";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "StaffOrResource+GS_FullName";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo2.ColumnName = "PE_ResourceComment";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			this.ExtraResourcesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ExtraResourcesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ExtraResourcesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ExtraResourcesGrid.CopySelectedRowsAllowed = true;
			this.ExtraResourcesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExtraResourcesGrid.GridId = "d9f9bdfb-b065-4912-a9a3-087564bd3fe5";
			this.ExtraResourcesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ExtraResourcesGrid.LayoutKey = "ExtraResourcesGrid";
			this.ExtraResourcesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ExtraResourcesGrid.Name = "ExtraResourcesGrid";
			this.ExtraResourcesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(364, 84, true);
			this.ExtraResourcesGrid.TabIndex = 0;
			// 
			// TaskWithDetailsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TasksSplitContainer);
			this.Name = "TaskWithDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(845, 515, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.NotesTextBox.ResumeLayout(true);
			this.NotesTextBox.PerformLayout();
			this.ActualStartDateEdit.ResumeLayout(true);
			this.ActualStartDateEdit.PerformLayout();
			this.ScheduleDateEdit.ResumeLayout(true);
			this.ScheduleDateEdit.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.GroupFindBox.ResumeLayout(true);
			this.GroupFindBox.PerformLayout();
			this.StaffFindBox.ResumeLayout(true);
			this.StaffFindBox.PerformLayout();
			this.TaskTypeDropEdit.ResumeLayout(true);
			this.TaskTypeDropEdit.PerformLayout();
			this.TasksSplitContainer.Panel1.ResumeLayout(false);
			this.TasksSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TasksSplitContainer)).EndInit();
			this.TasksSplitContainer.ResumeLayout(false);
			this.TasksSplitContainer.PerformLayout();
			this.TasksControl.ResumeLayout(true);
			this.TasksControl.PerformLayout();
			this.DetailsTabControl.ResumeLayout(false);
			this.DetailsTabControl.PerformLayout();
			this.SimpleViewTabPage.ResumeLayout(false);
			this.SimpleViewTabPage.PerformLayout();
			this.SimpleNotesGroupBox.ResumeLayout(false);
			this.SimpleNotesGroupBox.PerformLayout();
			this.SimpleNotesRichTextBox.ResumeLayout(true);
			this.SimpleNotesRichTextBox.PerformLayout();
			this.SimpleScheduleGroupBox.ResumeLayout(false);
			this.SimpleScheduleGroupBox.PerformLayout();
			this.SimpleWorkflowGuidDropEdit.ResumeLayout(true);
			this.SimpleWorkflowGuidDropEdit.PerformLayout();
			this.SimpleDetailsGroupBox.ResumeLayout(false);
			this.SimpleDetailsGroupBox.PerformLayout();
			this.SimpleStatusDropEdit.ResumeLayout(true);
			this.SimpleStatusDropEdit.PerformLayout();
			this.SimpleCapabilityGuidFindBox.ResumeLayout(true);
			this.SimpleCapabilityGuidFindBox.PerformLayout();
			this.SimpleTypeDropEdit.ResumeLayout(true);
			this.SimpleTypeDropEdit.PerformLayout();
			this.SimpleGroupGuidFindBox.ResumeLayout(true);
			this.SimpleGroupGuidFindBox.PerformLayout();
			this.SimpleStaffCodeFindBox.ResumeLayout(true);
			this.SimpleStaffCodeFindBox.PerformLayout();
			this.AdvancedViewTabPage.ResumeLayout(false);
			this.AdvancedViewTabPage.PerformLayout();
			this.ScheduleGroupBox.ResumeLayout(false);
			this.ScheduleGroupBox.PerformLayout();
			this.ProcessHeaderDropEdit.ResumeLayout(true);
			this.ProcessHeaderDropEdit.PerformLayout();
			this.CompletedDateEdit.ResumeLayout(true);
			this.CompletedDateEdit.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.CapabilityFindBox.ResumeLayout(true);
			this.CapabilityFindBox.PerformLayout();
			this.NotesGroupBox.ResumeLayout(false);
			this.NotesGroupBox.PerformLayout();
			this.ExtraResourcesGroupBox.ResumeLayout(false);
			this.ExtraResourcesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ExtraResourcesGrid)).EndInit();
			this.ExtraResourcesGrid.ResumeLayout(false);
			this.ExtraResourcesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZTimeEditEx ActualDurTimeEdit;
		private Enterprise.ZArchitecture.GUI.ZDateTimeOffsetEdit ActualStartDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateTimeOffsetEdit ScheduleDateEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit StatusDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox GroupFindBox;
		internal Enterprise.ZArchitecture.ZCalcEdit SequenceCalcEdit;
		internal Enterprise.ZArchitecture.ZTextBox IDTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox StaffFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit TaskTypeDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox DescriptionTextBox;
		private CargoWise.Windows.UI.KSplitContainer TasksSplitContainer;
		internal Enterprise.ZArchitecture.GUI.ZRichTextBox NotesTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ScheduleGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox NotesGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ExtraResourcesGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox ReminderCheckBox;
		private Enterprise.ZArchitecture.GUI.ZTimeEditEx EstTimeEditEx;
		internal Enterprise.ZArchitecture.ZCalcEdit EstimateVariationFactorCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZDateTimeOffsetEdit CompletedDateEdit;
		private Enterprise.ZArchitecture.GUI.ZTimeEditEx LastEstTimeToCompleteTimeEditEx;
		private Enterprise.ZArchitecture.GUI.ZTimeEditEx HighEstDurTimeEditEx;
		internal Enterprise.ZArchitecture.GUI.ZGuidDropEdit ProcessHeaderDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox CapabilityFindBox;
		internal Enterprise.ZArchitecture.GUI.ZTabControl DetailsTabControl;
		internal Enterprise.ZArchitecture.GUI.ZTabPage SimpleViewTabPage;
		internal Enterprise.ZArchitecture.GUI.ZTabPage AdvancedViewTabPage;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox SimpleNotesGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZRichTextBox SimpleNotesRichTextBox;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox SimpleScheduleGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZGuidDropEdit SimpleWorkflowGuidDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZTimeEditEx SimpleHighEstTimeEditEx;
		internal Enterprise.ZArchitecture.GUI.ZTimeEditEx SimpleEstTimeEditEx;
		internal Enterprise.ZArchitecture.GUI.ZTimeEditEx SimpleActDurTimeEditEx;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox SimpleDetailsGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit SimpleStatusDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox SimpleCapabilityGuidFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit SimpleTypeDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox SimpleDescriptionTextBox;
		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox SimpleGroupGuidFindBox;
		internal Enterprise.ZArchitecture.ZCalcEdit SimpleEstVariationCalcEdit;
		internal TasksControl TasksControl;
		internal Enterprise.ZArchitecture.ZGrid ExtraResourcesGrid;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsInterruptableCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox SimpleStaffCodeFindBox;
	}
}
