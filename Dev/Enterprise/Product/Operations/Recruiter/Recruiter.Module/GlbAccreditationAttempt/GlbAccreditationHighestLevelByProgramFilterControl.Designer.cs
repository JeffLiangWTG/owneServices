namespace Enterprise.Recruiter.Module
{
	partial class GlbAccreditationHighestLevelByProgramFilterControl
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
			this.accreditationGroupGuidFindBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.isCompletedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.accreditationGroupGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruiter.Module.GlbAccreditationHighestLevelByProgramFilter);
			// 
			// accreditationGroupGuidFindBox
			// 
			this.accreditationGroupGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.accreditationGroupGuidFindBox, "AccreditationGroupDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Recruiter.Module.GlbAccreditationHighestLevelByProgramFilter)(null)).AccreditationGroupDescription)));
			this.accreditationGroupGuidFindBox.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("d8a9bed4-be18-45ed-9b9b-d60b397b3bf9", "Program");
			this.accreditationGroupGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 3, true);
			this.accreditationGroupGuidFindBox.Name = "accreditationGroupGuidFindBox";
			this.accreditationGroupGuidFindBox.ShouldResizeByMaxLength = true;
			this.accreditationGroupGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.accreditationGroupGuidFindBox.TabIndex = 0;
			// 
			// isCompletedCheckBox
			// 
			this.isCompletedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.isCompletedCheckBox, "IsCompleted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Recruiter.Module.GlbAccreditationHighestLevelByProgramFilter)(null)).IsCompleted)));
			this.isCompletedCheckBox.CaptionResourceString = Enterprise.Recruiter.Module.Res.GetData("80a18250-68b2-49a8-a8c3-2f22329e6c1d", "Completed Only");
			this.isCompletedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isCompletedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 29, true);
			this.isCompletedCheckBox.Name = "isCompletedCheckBox";
			this.isCompletedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.isCompletedCheckBox.TabIndex = 1;
			this.isCompletedCheckBox.UseVisualStyleBackColor = true;
			// 
			// GlbAccreditationHighestLevelByProgramFilterControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.accreditationGroupGuidFindBox);
			this.Controls.Add(this.isCompletedCheckBox);
			this.Name = "GlbAccreditationHighestLevelByProgramFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 46, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.accreditationGroupGuidFindBox.ResumeLayout(true);
			this.accreditationGroupGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDropEdit accreditationGroupGuidFindBox;
		private ZArchitecture.GUI.ZCheckBox isCompletedCheckBox;
	}
}
