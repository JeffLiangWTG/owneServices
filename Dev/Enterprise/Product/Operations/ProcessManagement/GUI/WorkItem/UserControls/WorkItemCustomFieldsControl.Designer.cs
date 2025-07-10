using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	partial class WorkItemCustomFieldsControl : ZUserControl
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
			this.CustomFieldsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.WorkItemProcessTemplateCustomFieldsControl = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			CustomFieldsGroupBox.SuspendLayout();
			WorkItemProcessTemplateCustomFieldsControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ProcessManagement.Business.WorkItem);
			// 
			// CustomFieldsGroupBox
			// 
			this.CustomFieldsGroupBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("6c886693-059c-46ed-8bf1-8a002aed967b", "Custom Fields");
			this.CustomFieldsGroupBox.Controls.Add(this.WorkItemProcessTemplateCustomFieldsControl);
			this.CustomFieldsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomFieldsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustomFieldsGroupBox.Name = "CustomFieldsGroupBox";
			this.CustomFieldsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 296, true);
			this.CustomFieldsGroupBox.TabIndex = 0;
			this.CustomFieldsGroupBox.TabStop = false;
			// 
			// WorkItemProcessTemplateCustomFieldsControl
			// 
			this.WorkItemProcessTemplateCustomFieldsControl.AllowDrop = true;
			this.WorkItemProcessTemplateCustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WorkItemProcessTemplateCustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.WorkItemProcessTemplateCustomFieldsControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 3, 3, true);
			this.WorkItemProcessTemplateCustomFieldsControl.Name = "WorkItemProcessTemplateCustomFieldsControl";
			this.WorkItemProcessTemplateCustomFieldsControl.NothingSetupMessageLabelText = "";
			this.WorkItemProcessTemplateCustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(342, 280, true);
			this.WorkItemProcessTemplateCustomFieldsControl.TabIndex = 0;
			// 
			// WorkItemCustomFieldsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomFieldsGroupBox);
			this.Name = "WorkItemCustomFieldsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 296, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			CustomFieldsGroupBox.ResumeLayout(false);
			CustomFieldsGroupBox.PerformLayout();
			WorkItemProcessTemplateCustomFieldsControl.ResumeLayout(false);
			WorkItemProcessTemplateCustomFieldsControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox CustomFieldsGroupBox;
		private ZArchitecture.GUI.ProcessTemplateCustomFieldsControl WorkItemProcessTemplateCustomFieldsControl;
	}
}
