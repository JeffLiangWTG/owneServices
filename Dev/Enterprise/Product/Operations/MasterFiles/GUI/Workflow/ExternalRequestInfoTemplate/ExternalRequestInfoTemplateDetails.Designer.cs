namespace Enterprise.MasterFiles.GUI
{
	partial class ExternalRequestInfoTemplateDetails
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
            this.detailsGroup = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.RIT_CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.RIT_DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.RIT_JobTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.RIT_IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.InternalNotesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.RIT_TemplateRichTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.detailsGroup.SuspendLayout();
            this.RIT_JobTypeDropEdit.SuspendLayout();
            this.InternalNotesGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ExternalRequestInfoTemplate);
            // 
            // detailsGroup
            // 
            this.detailsGroup.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6aff6e23-5623-41f2-b418-971c1dedbf75", "Details");
            this.detailsGroup.Controls.Add(this.RIT_CodeTextBox);
            this.detailsGroup.Controls.Add(this.RIT_DescriptionTextBox);
            this.detailsGroup.Controls.Add(this.RIT_JobTypeDropEdit);
            this.detailsGroup.Controls.Add(this.RIT_IsActiveCheckBox);
            this.detailsGroup.Dock = System.Windows.Forms.DockStyle.Top;
            this.detailsGroup.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.detailsGroup.Name = "detailsGroup";
            this.detailsGroup.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(575, 156, true);
            this.detailsGroup.TabIndex = 0;
            this.detailsGroup.TabStop = false;
            // 
            // RIT_CodeTextBox
            // 
            this.BindingSource.SetBindingMember(this.RIT_CodeTextBox, "RIT_Code");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ExternalRequestInfoTemplate)(null)).RIT_Code)));
            this.RIT_CodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c6f93891-ff5f-4bd2-89ad-ea3587752f59", "Code");
            this.RIT_CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 10, true);
            this.RIT_CodeTextBox.Name = "RIT_CodeTextBox";
            this.RIT_CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 15, true);
            this.RIT_CodeTextBox.TabIndex = 1;
            // 
            // RIT_DescriptionTextBox
            // 
            this.BindingSource.SetBindingMember(this.RIT_DescriptionTextBox, "RIT_Description");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ExternalRequestInfoTemplate)(null)).RIT_Description)));
            this.RIT_DescriptionTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a70dc859-5563-47cb-aa43-66081c6d7ca2", "Description");
            this.RIT_DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 50, true);
            this.RIT_DescriptionTextBox.Name = "RIT_DescriptionTextBox";
            this.RIT_DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 18, true);
            this.RIT_DescriptionTextBox.TabIndex = 2;
            // 
            // RIT_JobTypeDropEdit
            // 
            this.RIT_JobTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.RIT_JobTypeDropEdit, "RIT_JobType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.ExternalRequestInfoTemplate)(null)).RIT_JobType)));
            this.RIT_JobTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("91154ca0-c1b8-4446-9a0e-16010e4403ff", "Job Type");
            this.RIT_JobTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 90, true);
            this.RIT_JobTypeDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
            this.RIT_JobTypeDropEdit.Name = "RIT_JobTypeDropEdit";
            this.RIT_JobTypeDropEdit.PreBoundMaxLength = 3;
            this.RIT_JobTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 15, true);
            this.RIT_JobTypeDropEdit.TabIndex = 3;
            // 
            // RIT_IsActiveCheckBox
            // 
            this.RIT_IsActiveCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.RIT_IsActiveCheckBox, "RIT_IsActive");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ExternalRequestInfoTemplate)(null)).RIT_IsActive)));
            this.RIT_IsActiveCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7c30ed5f-f8d6-4c1d-914a-9637b16f2e3e", "Is Active");
            this.RIT_IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 130, true);
            this.RIT_IsActiveCheckBox.Name = "RIT_IsActiveCheckBox";
            this.RIT_IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 14, true);
            this.RIT_IsActiveCheckBox.TabIndex = 4;
            // 
            // InternalNotesGroupBox
            // 
            this.InternalNotesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c23332aa-ba95-4b0a-937a-b650868def61", "Template");
            this.InternalNotesGroupBox.Controls.Add(this.RIT_TemplateRichTextBox);
            this.InternalNotesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InternalNotesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 156, true);
            this.InternalNotesGroupBox.Name = "InternalNotesGroupBox";
            this.InternalNotesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(575, 297, true);
            this.InternalNotesGroupBox.TabIndex = 1;
            this.InternalNotesGroupBox.TabStop = false;
            // 
            // RIT_TemplateRichTextBox
            // 
            this.BindingSource.SetBindingMember(this.RIT_TemplateRichTextBox, "RIT_Template");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.MasterFiles.Business.ExternalRequestInfoTemplate)(null)).RIT_Template)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RIT_TemplateRichTextBox, false);
			this.RIT_TemplateRichTextBox.PopupFormCaption = Enterprise.MasterFiles.GUI.Res.GetData("ec5bcbf0-5d66-443c-9e88-2cc92711aa78", "Template");
			this.RIT_TemplateRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RIT_TemplateRichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
            this.RIT_TemplateRichTextBox.Name = "RIT_TemplateRichTextBox";
            this.RIT_TemplateRichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 15, true);
            this.RIT_TemplateRichTextBox.TabIndex = 0;
            // 
            // ExternalRequestInfoTemplateDetails
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.InternalNotesGroupBox);
            this.Controls.Add(this.detailsGroup);
            this.Name = "ExternalRequestInfoTemplateDetails";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(575, 453, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.detailsGroup.ResumeLayout(false);
            this.detailsGroup.PerformLayout();
            this.RIT_JobTypeDropEdit.ResumeLayout(true);
            this.RIT_JobTypeDropEdit.PerformLayout();
            this.InternalNotesGroupBox.ResumeLayout(false);
            this.InternalNotesGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		private ZArchitecture.ZTextBox RIT_CodeTextBox;
		private ZArchitecture.ZTextBox RIT_DescriptionTextBox;
		private ZArchitecture.GUI.ZDropEdit RIT_JobTypeDropEdit;
		private ZArchitecture.GUI.ZCheckBox RIT_IsActiveCheckBox;
		private ZArchitecture.GUI.ZGroupBox detailsGroup;
		private Enterprise.ZArchitecture.GUI.ZRichTextBox RIT_TemplateRichTextBox;
		private ZArchitecture.GUI.ZGroupBox InternalNotesGroupBox;

		#endregion
	}
}
