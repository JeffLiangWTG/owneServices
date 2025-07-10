namespace Enterprise.MasterFiles.GUI
{
	public partial class CustomFieldsUserControl
	{

		#region Component Designer generated code

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.processTemplateCustomFieldsControl = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();

			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// SecurityPanel
			// 
			this.SecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// ProcessTemplateCustomFieldsControl
			// 
			this.processTemplateCustomFieldsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.processTemplateCustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.processTemplateCustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.processTemplateCustomFieldsControl.Name = "ProcessTemplateCustomFieldsControl";
			this.processTemplateCustomFieldsControl.NothingSetupMessageLabelText = Enterprise.MasterFiles.GUI.Res.GetString("MainDetailsUserControl|0447d646-9eec-4401-8990-7255e18be7e5", "To make use of this tab, please setup Organization custom fields in Workflow Manager.");
			this.processTemplateCustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 432, true);
			this.processTemplateCustomFieldsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.processTemplateCustomFieldsControl, ".");
			this.processTemplateCustomFieldsControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.processTemplateCustomFieldsControl.TabIndex = 0;
			this.processTemplateCustomFieldsControl.ResumeLayout(true);
			this.processTemplateCustomFieldsControl.PerformLayout();
			// 
			// CustomFieldsUserControl
			// 
			this.Controls.Add(this.processTemplateCustomFieldsControl);
			this.IsModifyDetailsCustomFields = true;
			this.Name = "CustomFieldsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 456, true);
			this.Controls.SetChildIndex(this.SecurityPanel, 0);
			this.Controls.SetChildIndex(this.processTemplateCustomFieldsControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.processTemplateCustomFieldsControl.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		#endregion

		System.ComponentModel.IContainer components;
		internal Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl processTemplateCustomFieldsControl;
	}
}
