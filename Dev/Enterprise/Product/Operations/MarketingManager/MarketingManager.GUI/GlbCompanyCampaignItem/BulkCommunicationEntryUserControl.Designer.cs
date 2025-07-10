using Enterprise.ZArchitecture.Modules;
namespace Enterprise.MarketingManager.GUI
{
	partial class BulkCommunicationEntryUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SummaryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.StaffCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SubjectTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CategoryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DatesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DurationTimeEdit = new Enterprise.ZArchitecture.GUI.ZTimeEdit();
			this.SendReminderCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ScheduleDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ActualDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.OverallDispositionLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SummaryGroupBox.SuspendLayout();
			this.DatesGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.BulkCommunication);
			// 
			// SummaryGroupBox
			// 
			this.SummaryGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("2560421b-cb6e-4381-9521-6d61949e1f9d", "Summary");
			this.SummaryGroupBox.Controls.Add(this.OverallDispositionLabel);
			this.SummaryGroupBox.Controls.Add(this.StaffCodeFindBox);
			this.SummaryGroupBox.Controls.Add(this.StatusDropEdit);
			this.SummaryGroupBox.Controls.Add(this.SubjectTextBox);
			this.SummaryGroupBox.Controls.Add(this.TypeDropEdit);
			this.SummaryGroupBox.Controls.Add(this.CategoryDropEdit);
			this.SummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 1, true);
			this.SummaryGroupBox.Name = "SummaryGroupBox";
			this.SummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(439, 160, true);
			this.SummaryGroupBox.TabIndex = 1;
			this.SummaryGroupBox.TabStop = false;
			// 
			// OverallDispositionLabel
			// 
			this.OverallDispositionLabel.BackColor = System.Drawing.Color.Red;
			this.BindingSource.SetBindingMember(this.OverallDispositionLabel, "OverallDispositionDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).OverallDispositionDescription)));
			this.OverallDispositionLabel.ForeColor = System.Drawing.Color.White;
			this.OverallDispositionLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OverallDispositionLabel, false);
			this.OverallDispositionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(347, 99, true);
			this.OverallDispositionLabel.Name = "OverallDispositionLabel";
			this.OverallDispositionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 20, true);
			this.OverallDispositionLabel.TabIndex = 3;
			this.OverallDispositionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// StaffCodeFindBox
			// 
			this.StaffCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StaffCodeFindBox, "StaffCoordinator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).StaffCoordinator)));
			this.StaffCodeFindBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("5d9e7ea0-da64-40eb-9c73-e4d39bc98002", "Staff Coordinator");
			this.StaffCodeFindBox.ModuleID = ModuleIDs.GlbStaff;
			this.StaffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 125, true);
			this.StaffCodeFindBox.Name = "StaffCodeFindBox";
			this.StaffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 20, true);
			this.StaffCodeFindBox.TabIndex = 4;
			// 
			// StatusDropEdit
			// 
			this.StatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).StatusDescription)));
			this.StatusDropEdit.BindToForDescription = "StatusDescription";
			this.StatusDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("c4fd8785-da39-42d8-8c7c-80bb6ed8fd4e", "Status");
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 99, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 20, true);
			this.StatusDropEdit.TabIndex = 3;
			// 
			// SubjectTextBox
			// 
			this.BindingSource.SetBindingMember(this.SubjectTextBox, "Summary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).Summary)));
			this.SubjectTextBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ad31f92e-9cd6-4af3-bda4-77bae27ee3ae", "Subject");
			this.SubjectTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SubjectTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 73, true);
			this.SubjectTextBox.Name = "SubjectTextBox";
			this.SubjectTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.SubjectTextBox.TabIndex = 2;
			// 
			// TypeDropEdit
			// 
			this.TypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TypeDropEdit, "TypeOfCall");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).TypeOfCall)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).TypeOfCallDescription)));
			this.TypeDropEdit.BindToForDescription = "TypeOfCallDescription";
			this.TypeDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("61b6991c-d997-46d3-bca3-e9ee7546e416", "Method");
			this.TypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 21, true);
			this.TypeDropEdit.Name = "TypeDropEdit";
			this.TypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 20, true);
			this.TypeDropEdit.TabIndex = 0;
			// 
			// CategoryDropEdit
			// 
			this.CategoryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CategoryDropEdit, "Category");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).Category)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).CategoryDescription)));
			this.CategoryDropEdit.BindToForDescription = "CategoryDescription";
			this.CategoryDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("55b9aa31-d773-44de-9ad0-abd3eaa4878d", "Purpose");
			this.CategoryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 47, true);
			this.CategoryDropEdit.Name = "CategoryDropEdit";
			this.CategoryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 20, true);
			this.CategoryDropEdit.TabIndex = 1;
			// 
			// DatesGroupBox
			// 
			this.DatesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DatesGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("e6637f39-7658-4c05-bdda-e920522c88fa", "Dates");
			this.DatesGroupBox.Controls.Add(this.DurationTimeEdit);
			this.DatesGroupBox.Controls.Add(this.SendReminderCheckBox);
			this.DatesGroupBox.Controls.Add(this.ScheduleDateEdit);
			this.DatesGroupBox.Controls.Add(this.ActualDateEdit);
			this.DatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(449, 1, true);
			this.DatesGroupBox.Name = "DatesGroupBox";
			this.DatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(473, 85, true);
			this.DatesGroupBox.TabIndex = 2;
			this.DatesGroupBox.TabStop = false;
			// 
			// DurationTimeEdit
			// 
			this.DurationTimeEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DurationTimeEdit, "Duration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).Duration)));
			this.DurationTimeEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("8d7f64c9-1848-4185-9fb9-046ac9bebb0a", "Duration");
			this.DurationTimeEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DurationTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 20, true);
			this.DurationTimeEdit.Name = "DurationTimeEdit";
			this.DurationTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			this.DurationTimeEdit.TabIndex = 2;
			// 
			// SendReminderCheckBox
			// 
			this.SendReminderCheckBox.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.SendReminderCheckBox, "ShouldSendInvitation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).ShouldSendInvitation)));
			this.SendReminderCheckBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("2b2efeef-e99b-4119-aed3-b6edcfd889d2", "Send reminder");
			this.SendReminderCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SendReminderCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(361, 18, true);
			this.SendReminderCheckBox.Name = "SendReminderCheckBox";
			this.SendReminderCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.SendReminderCheckBox.TabIndex = 3;
			this.SendReminderCheckBox.UseVisualStyleBackColor = false;
			// 
			// ScheduleDateEdit
			// 
			this.ScheduleDateEdit.AllowDrop = true;
			this.ScheduleDateEdit.AutoCompleteMonthThreshold = 1;
			this.ScheduleDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ScheduleDateEdit, "NextCallLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).NextCallLocal)));
			this.ScheduleDateEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("150cbacf-b1da-46a6-8736-f8a250bc8717", "Scheduled Date");
			this.ScheduleDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ScheduleDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 20, true);
			this.ScheduleDateEdit.Name = "ScheduleDateEdit";
			this.ScheduleDateEdit.TabIndex = 0;
			// 
			// ActualDateEdit
			// 
			this.ActualDateEdit.AllowDrop = true;
			this.ActualDateEdit.AutoCompleteMonthThreshold = 1;
			this.ActualDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ActualDateEdit, "CallDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.BulkCommunication)(null)).CallDate)));
			this.ActualDateEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("dfa789d3-e010-4d5d-9d37-ef3dde0fab86", "Actual Date");
			this.ActualDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ActualDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 50, true);
			this.ActualDateEdit.Name = "ActualDateEdit";
			this.ActualDateEdit.TabIndex = 1;
			// 
			// BulkCommunicationEntryUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.DatesGroupBox);
			this.Controls.Add(this.SummaryGroupBox);
			this.Name = "BulkCommunicationEntryUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(927, 159, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SummaryGroupBox.ResumeLayout(false);
			this.SummaryGroupBox.PerformLayout();
			this.DatesGroupBox.ResumeLayout(false);
			this.DatesGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox SummaryGroupBox;
		private ZArchitecture.GUI.ZGroupBox DatesGroupBox;
		private ZArchitecture.GUI.ZTimeEdit DurationTimeEdit;
		private ZArchitecture.GUI.ZCheckBox SendReminderCheckBox;
		private ZArchitecture.GUI.ZDateEdit ScheduleDateEdit;
		private ZArchitecture.GUI.ZDateEdit ActualDateEdit;
		private ZArchitecture.GUI.ZCodeFindBox StaffCodeFindBox;
		private ZArchitecture.GUI.ZDropEdit StatusDropEdit;
		private ZArchitecture.ZTextBox SubjectTextBox;
		private ZArchitecture.GUI.ZDropEdit CategoryDropEdit;
		private ZArchitecture.GUI.ZDropEdit TypeDropEdit;
		private ZArchitecture.ZLabel OverallDispositionLabel;
	}
}
