using Enterprise.ZArchitecture.GUI;
namespace Enterprise.ProcessManagement.GUI
{
	partial class ProjectInformationControl : ZUserControl
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
			this.InformationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ProjectDetailsPanel = new CargoWise.Windows.UI.Layout.RowLayoutPanel();
			this.PriorityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ProjectModuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ProjectSubTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ProjectTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.CustomFieldsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ProjectProcessTemplateCustomFieldsControl = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			InformationGroupBox.SuspendLayout();
			ProjectDetailsPanel.SuspendLayout();
			PriorityDropEdit.SuspendLayout();
			ProjectModuleDropEdit.SuspendLayout();
			ProjectSubTypeDropEdit.SuspendLayout();
			ProjectTypeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			splitContainer1.SuspendLayout();
			CustomFieldsGroupBox.SuspendLayout();
			ProjectProcessTemplateCustomFieldsControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ProcessManagement.Business.Project);
			// 
			// InformationGroupBox
			// 
			this.InformationGroupBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("1F6E683B-C397-4702-99F5-78D3C172E785", "Project Information");
			this.InformationGroupBox.Controls.Add(this.ProjectDetailsPanel);
			this.InformationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InformationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InformationGroupBox.Name = "InformationGroupBox";
			this.InformationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 173, true);
			this.InformationGroupBox.TabIndex = 0;
			this.InformationGroupBox.TabStop = false;
			// 
			// ProjectDetailsPanel
			// 
			this.ProjectDetailsPanel.Alignment = System.Windows.Forms.VisualStyles.VerticalAlignment.Center;
			this.ProjectDetailsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ProjectDetailsPanel.Controls.Add(this.PriorityDropEdit);
			this.ProjectDetailsPanel.Controls.Add(this.ProjectModuleDropEdit);
			this.ProjectDetailsPanel.Controls.Add(this.ProjectSubTypeDropEdit);
			this.ProjectDetailsPanel.Controls.Add(this.ProjectTypeDropEdit);
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.ProjectDetailsPanel, true);
			this.ProjectDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 16, true);
			this.ProjectDetailsPanel.Name = "ProjectDetailsPanel";
			this.ProjectDetailsPanel.RowHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(25);
			this.ProjectDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 111, true);
			this.ProjectDetailsPanel.TabIndex = 1;
			// 
			// PriorityDropEdit
			// 
			this.PriorityDropEdit.AllowDrop = true;
			this.PriorityDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PriorityDropEdit, "WKP_Priority");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ProcessManagement.Business.Project)(null)).WKP_Priority)));
			this.PriorityDropEdit.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("2DC27496-CB9F-49AE-9D3C-6D8FEA1C22A8", "Priority");
			this.PriorityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 77, true);
			this.PriorityDropEdit.Name = "PriorityDropEdit";
			this.PriorityDropEdit.PreBoundMaxLength = 3;
			this.ProjectDetailsPanel.SetRow(this.PriorityDropEdit, 3);
			this.PriorityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 20, true);
			this.PriorityDropEdit.TabIndex = 4;
			// 
			// ProjectModuleDropEdit
			// 
			this.ProjectModuleDropEdit.AllowDrop = true;
			this.ProjectModuleDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ProjectModuleDropEdit, "WKP_Module");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ProcessManagement.Business.Project)(null)).WKP_Module)));
			this.ProjectModuleDropEdit.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("EDB37487-F520-47DE-843B-EFEA21514C53", "Project Module");
			this.ProjectModuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 52, true);
			this.ProjectModuleDropEdit.Name = "ProjectModuleDropEdit";
			this.ProjectModuleDropEdit.PreBoundMaxLength = 3;
			this.ProjectDetailsPanel.SetRow(this.ProjectModuleDropEdit, 2);
			this.ProjectModuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 20, true);
			this.ProjectModuleDropEdit.TabIndex = 3;
			// 
			// ProjectSubTypeDropEdit
			// 
			this.ProjectSubTypeDropEdit.AllowDrop = true;
			this.ProjectSubTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ProjectSubTypeDropEdit, "WKP_SubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ProcessManagement.Business.Project)(null)).WKP_SubType)));
			this.ProjectSubTypeDropEdit.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("F012869E-836B-4352-8BB0-1A350E2F9CF7", "Project Sub Type");
			this.ProjectSubTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 27, true);
			this.ProjectSubTypeDropEdit.Name = "ProjectSubTypeDropEdit";
			this.ProjectSubTypeDropEdit.PreBoundMaxLength = 3;
			this.ProjectDetailsPanel.SetRow(this.ProjectSubTypeDropEdit, 1);
			this.ProjectSubTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 20, true);
			this.ProjectSubTypeDropEdit.TabIndex = 2;
			// 
			// ProjectTypeDropEdit
			// 
			this.ProjectTypeDropEdit.AllowDrop = true;
			this.ProjectTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ProjectTypeDropEdit, "WKP_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ProcessManagement.Business.Project)(null)).WKP_Type)));
			this.ProjectTypeDropEdit.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("377A998C-DD7D-4CB6-8573-413A47A43EFA", "Product Type");
			this.ProjectTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 2, true);
			this.ProjectTypeDropEdit.Name = "ProjectTypeDropEdit";
			this.ProjectTypeDropEdit.PreBoundMaxLength = 3;
			this.ProjectDetailsPanel.SetRow(this.ProjectTypeDropEdit, 0);
			this.ProjectTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 20, true);
			this.ProjectTypeDropEdit.TabIndex = 1;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.InformationGroupBox);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.CustomFieldsGroupBox);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 307, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(173);
			this.splitContainer1.TabIndex = 1;
			// 
			// CustomFieldsGroupBox
			// 
			this.CustomFieldsGroupBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("6c886693-059c-46ed-8bf1-8a002aed967b", "Custom Fields");
			this.CustomFieldsGroupBox.Controls.Add(this.ProjectProcessTemplateCustomFieldsControl);
			this.CustomFieldsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomFieldsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustomFieldsGroupBox.Name = "CustomFieldsGroupBox";
			this.CustomFieldsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 131, true);
			this.CustomFieldsGroupBox.TabIndex = 1;
			this.CustomFieldsGroupBox.TabStop = false;
			// 
			// ProjectProcessTemplateCustomFieldsControl
			// 
			this.ProjectProcessTemplateCustomFieldsControl.AllowDrop = true;
			this.ProjectProcessTemplateCustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProjectProcessTemplateCustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.ProjectProcessTemplateCustomFieldsControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 3, 3, true);
			this.ProjectProcessTemplateCustomFieldsControl.Name = "ProjectProcessTemplateCustomFieldsControl";
			this.ProjectProcessTemplateCustomFieldsControl.NothingSetupMessageLabelText = "";
			this.ProjectProcessTemplateCustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 115, true);
			this.ProjectProcessTemplateCustomFieldsControl.TabIndex = 0;
			// 
			// ProjectInformationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer1);
			this.Name = "ProjectInformationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 307, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			InformationGroupBox.ResumeLayout(false);
			InformationGroupBox.PerformLayout();
			ProjectDetailsPanel.ResumeLayout(false);
			ProjectDetailsPanel.PerformLayout();
			PriorityDropEdit.ResumeLayout(false);
			PriorityDropEdit.PerformLayout();
			ProjectModuleDropEdit.ResumeLayout(false);
			ProjectModuleDropEdit.PerformLayout();
			ProjectSubTypeDropEdit.ResumeLayout(false);
			ProjectSubTypeDropEdit.PerformLayout();
			ProjectTypeDropEdit.ResumeLayout(false);
			ProjectTypeDropEdit.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			splitContainer1.ResumeLayout(false);
			splitContainer1.PerformLayout();
			CustomFieldsGroupBox.ResumeLayout(false);
			CustomFieldsGroupBox.PerformLayout();
			ProjectProcessTemplateCustomFieldsControl.ResumeLayout(false);
			ProjectProcessTemplateCustomFieldsControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox InformationGroupBox;
		private ZArchitecture.GUI.ZDropEdit ProjectTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit PriorityDropEdit;
		private ZArchitecture.GUI.ZDropEdit ProjectSubTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit ProjectModuleDropEdit;
		private CargoWise.Windows.UI.Layout.RowLayoutPanel ProjectDetailsPanel;
		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		private ZGroupBox CustomFieldsGroupBox;
		private ProcessTemplateCustomFieldsControl ProjectProcessTemplateCustomFieldsControl;
	}
}