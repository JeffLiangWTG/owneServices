
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.MasterFiles.Module
{
	partial class WorkflowFilterStripControl : ZDateRangeControl
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
			this.MilestoneTypeDropEdit = new Enterprise.ZArchitecture.GUI.Internal.ZFilterStripDropEdit();
			this.DatesToFilterDropEdit = new Enterprise.ZArchitecture.GUI.Internal.ZFilterStripDropEdit();
			this.eventReferenceCompareDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.eventReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PropertySearchDropEdit.SuspendLayout();
			this.FromDateEdit.SuspendLayout();
			this.ToDateEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MilestoneTypeDropEdit.SuspendLayout();
			this.DatesToFilterDropEdit.SuspendLayout();
			this.eventReferenceCompareDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MilestoneTypeDropEdit
			// 
			this.MilestoneTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MilestoneTypeDropEdit, "MilestoneEvent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.MilestoneTypeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MilestoneTypeDropEdit.EnableTimeRecording = false;
			this.MilestoneTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(214, 1, true);
			this.MilestoneTypeDropEdit.MaxItemsToShowInDropDown = 28;
			this.MilestoneTypeDropEdit.Name = "MilestoneTypeDropEdit";
			this.MilestoneTypeDropEdit.ShowDescriptionBox = false;
			this.MilestoneTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.MilestoneTypeDropEdit.TabIndex = 0;
			// 
			// DatesToFilterDropEdit
			// 
			this.DatesToFilterDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DatesToFilterDropEdit, "DatesToFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.DatesToFilterDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DatesToFilterDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(204, 24, true);
			this.DatesToFilterDropEdit.MaxItemsToShowInDropDown = 28;
			this.DatesToFilterDropEdit.Name = "DatesToFilterDropEdit";
			this.DatesToFilterDropEdit.ShowDescriptionBox = false;
			this.DatesToFilterDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.DatesToFilterDropEdit.TabIndex = 1;
			// 
			// eventReferenceCompareDropEdit
			// 
			this.eventReferenceCompareDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.eventReferenceCompareDropEdit, "EventReferenceComparisonOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.eventReferenceCompareDropEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("6601d2c5-af0b-45bb-a849-47cd0de17cb7", "Trg. Cond. Value", "Trigger Condition Value", "The value used in conjunction with the Trigger Condition to restrict whether the trigger fires when the relevant event is raised or field value changes.");
			this.eventReferenceCompareDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.eventReferenceCompareDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(204, 47, true);
			this.eventReferenceCompareDropEdit.Name = "eventReferenceCompareDropEdit";
			this.eventReferenceCompareDropEdit.PreBoundMaxLength = 7;
			this.eventReferenceCompareDropEdit.ShowDescriptionBox = false;
			this.eventReferenceCompareDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.eventReferenceCompareDropEdit.TabIndex = 5;
			// 
			// eventReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.eventReferenceTextBox, "EventReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.eventReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(294, 47, true);
			this.eventReferenceTextBox.Name = "eventReferenceTextBox";
			this.eventReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.eventReferenceTextBox.TabIndex = 6;
			// 
			// WorkflowFilterStripControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("WorkflowFilterStripControl|212281e3-7c68-49c7-bc54-5ba4d401dfd1", "From");
			this.Controls.Add(this.eventReferenceTextBox);
			this.Controls.Add(this.MilestoneTypeDropEdit);
			this.Controls.Add(this.eventReferenceCompareDropEdit);
			this.Controls.Add(this.DatesToFilterDropEdit);
			this.Name = "WorkflowFilterStripControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 68, true);
			this.Controls.SetChildIndex(this.DatesToFilterDropEdit, 0);
			this.Controls.SetChildIndex(this.eventReferenceCompareDropEdit, 0);
			this.Controls.SetChildIndex(this.MilestoneTypeDropEdit, 0);
			this.Controls.SetChildIndex(this.FromDateEdit, 0);
			this.Controls.SetChildIndex(this.ToDateEdit, 0);
			this.Controls.SetChildIndex(this.PropertySearchDropEdit, 0);
			this.Controls.SetChildIndex(this.eventReferenceTextBox, 0);
			this.PropertySearchDropEdit.ResumeLayout(true);
			this.PropertySearchDropEdit.PerformLayout();
			this.FromDateEdit.ResumeLayout(true);
			this.FromDateEdit.PerformLayout();
			this.ToDateEdit.ResumeLayout(true);
			this.ToDateEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MilestoneTypeDropEdit.ResumeLayout(true);
			this.MilestoneTypeDropEdit.PerformLayout();
			this.DatesToFilterDropEdit.ResumeLayout(true);
			this.DatesToFilterDropEdit.PerformLayout();
			this.eventReferenceCompareDropEdit.ResumeLayout(true);
			this.eventReferenceCompareDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		public ZFilterStripDropEdit MilestoneTypeDropEdit;
		public ZFilterStripDropEdit DatesToFilterDropEdit;
		public ZDropEdit eventReferenceCompareDropEdit;
		private Enterprise.ZArchitecture.ZTextBox eventReferenceTextBox;

		#endregion
	}
}
