namespace Enterprise.MasterFiles.GUI
{
	public partial class CurrentQueueUserControl
	{
		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.QueueNameDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QueueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReasonLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AssignToLabel = new Enterprise.ZArchitecture.ZLabel();
			this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TaskAssignedToCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.SubStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SubStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.IActiveProcessQueue);
			// 
			// QueueNameDropEdit
			// 
			this.BindingSource.SetBindingMember(this.QueueNameDropEdit, "QueueName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.IActiveProcessQueue)(null)).QueueName)));
			this.QueueNameDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CurrentQueueUserControl|ba208644-10c7-4e7b-8ab5-3b6f681ee8e6", "Queue");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QueueNameDropEdit, false);
			this.QueueNameDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 8, true);
			this.QueueNameDropEdit.Name = "QueueNameDropEdit";
			this.QueueNameDropEdit.PreBoundMaxLength = 3;
			this.QueueNameDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.QueueNameDropEdit.TabIndex = 0;
			// 
			// QueueLabel
			// 
			this.BindingSource.SetBindingMember(this.QueueLabel, "QueueNameCaption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.IActiveProcessQueue)(null)).QueueNameCaption)));
			this.QueueLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CurrentQueueUserControl|9ac24edb-8ebf-461b-bb86-6361a33235fb", "Queue");
			this.QueueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.QueueLabel.Name = "QueueLabel";
			this.QueueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 16, true);
			this.QueueLabel.TabIndex = 1;
			// 
			// ReasonTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReasonTextBox, "Reason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.IActiveProcessQueue)(null)).Reason)));
			this.ReasonTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CurrentQueueUserControl|6f3e0916-f4ad-4ce8-a519-1f850ebe9a87", "Reason");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ReasonTextBox, false);
			this.ReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 32, true);
			this.ReasonTextBox.Multiline = true;
			this.ReasonTextBox.Name = "ReasonTextBox";
			this.ReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 48, true);
			this.ReasonTextBox.TabIndex = 4;
			// 
			// ReasonLabel
			// 
			this.BindingSource.SetBindingMember(this.ReasonLabel, "ReasonCaption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.IActiveProcessQueue)(null)).ReasonCaption)));
			this.ReasonLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CurrentQueueUserControl|89d7e211-d17d-4cab-a5ac-ab83d301d798", "Reason");
			this.ReasonLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 32, true);
			this.ReasonLabel.Name = "ReasonLabel";
			this.ReasonLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 16, true);
			this.ReasonLabel.TabIndex = 0;
			// 
			// AssignToLabel
			// 
			this.BindingSource.SetBindingMember(this.AssignToLabel, "AssignedToCaption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.IActiveProcessQueue)(null)).AssignedToCaption)));
			this.AssignToLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CurrentQueueUserControl|618a9024-28f8-442e-905b-7c0c836f8caf", "Assign To");
			this.AssignToLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 8, true);
			this.AssignToLabel.Name = "AssignToLabel";
			this.AssignToLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 16, true);
			this.AssignToLabel.TabIndex = 1;
			// 
			// StatusDropEdit
			// 
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.IActiveProcessQueue)(null)).Status)));
			this.StatusDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CurrentQueueUserControl|3aeafa2f-9a39-4fb4-985a-381d1902f18a", "Status");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.StatusDropEdit, false);
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 32, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.PreBoundMaxLength = 3;
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.StatusDropEdit.TabIndex = 1;
			// 
			// StatusLabel
			// 
			this.BindingSource.SetBindingMember(this.StatusLabel, "StatusCaption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.IActiveProcessQueue)(null)).StatusCaption)));
			this.StatusLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CurrentQueueUserControl|1e8c023b-178f-4ebc-9bb6-a8394c206034", "Status");
			this.StatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 32, true);
			this.StatusLabel.Name = "StatusLabel";
			this.StatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 16, true);
			this.StatusLabel.TabIndex = 1;
			// 
			// TaskAssignedToCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.TaskAssignedToCodeFindBox, "AssignedTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.IActiveProcessQueue)(null)).AssignedTo)));
			this.TaskAssignedToCodeFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CurrentQueueUserControl|12a85a0b-d011-4863-9f1d-6f0e1cc693cc", "Assign To");
			this.TaskAssignedToCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 8, true);
			this.TaskAssignedToCodeFindBox.Name = "TaskAssignedToCodeFindBox";
			this.TaskAssignedToCodeFindBox.PreBoundMaxLength = 3;
			this.TaskAssignedToCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.TaskAssignedToCodeFindBox.TabIndex = 3;
			// 
			// SubStatusDropEdit
			// 
			this.BindingSource.SetBindingMember(this.SubStatusDropEdit, "SubStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.IActiveProcessQueue)(null)).SubStatus)));
			this.SubStatusDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CurrentQueueUserControl|c4cc5110-8f2b-4016-b3b1-8e2644e80a6c", "Sub Status");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SubStatusDropEdit, false);
			this.SubStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 56, true);
			this.SubStatusDropEdit.Name = "SubStatusDropEdit";
			this.SubStatusDropEdit.PreBoundMaxLength = 3;
			this.SubStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.SubStatusDropEdit.TabIndex = 2;
			// 
			// SubStatusLabel
			// 
			this.BindingSource.SetBindingMember(this.SubStatusLabel, "SubStatusCaption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.IActiveProcessQueue)(null)).SubStatusCaption)));
			this.SubStatusLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CurrentQueueUserControl|7a954946-92ee-4d71-b17c-a21f4259b851", "Sub Status");
			this.SubStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 56, true);
			this.SubStatusLabel.Name = "SubStatusLabel";
			this.SubStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 16, true);
			this.SubStatusLabel.TabIndex = 4;
			// 
			// CurrentQueueUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SubStatusDropEdit);
			this.Controls.Add(this.SubStatusLabel);
			this.Controls.Add(this.TaskAssignedToCodeFindBox);
			this.Controls.Add(this.QueueLabel);
			this.Controls.Add(this.QueueNameDropEdit);
			this.Controls.Add(this.StatusDropEdit);
			this.Controls.Add(this.StatusLabel);
			this.Controls.Add(this.ReasonLabel);
			this.Controls.Add(this.ReasonTextBox);
			this.Controls.Add(this.AssignToLabel);
			this.Name = "CurrentQueueUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 88, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		private Enterprise.ZArchitecture.ZLabel QueueLabel;
		private Enterprise.ZArchitecture.GUI.ZDropEdit QueueNameDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox ReasonTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit StatusDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox TaskAssignedToCodeFindBox;
		internal Enterprise.ZArchitecture.ZLabel ReasonLabel;
		internal Enterprise.ZArchitecture.ZLabel AssignToLabel;
		private Enterprise.ZArchitecture.ZLabel StatusLabel;
		private Enterprise.ZArchitecture.GUI.ZDropEdit SubStatusDropEdit;
		private Enterprise.ZArchitecture.ZLabel SubStatusLabel;
	}
}
