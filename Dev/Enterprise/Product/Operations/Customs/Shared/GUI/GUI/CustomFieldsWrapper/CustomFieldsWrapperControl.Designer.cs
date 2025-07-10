using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.GUI
{
	partial class CustomFieldsWrapperControl
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
			this.ProcessTemplateCustomFieldsControl = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ProcessTemplateCustomFieldsControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(BusinessObject);
			// 
			// CustomFieldsControl
			// 
			this.ProcessTemplateCustomFieldsControl.AllowDrop = true;
			this.ProcessTemplateCustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProcessTemplateCustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProcessTemplateCustomFieldsControl.Name = "ProcessTemplateCustomFieldsControl";
			this.ProcessTemplateCustomFieldsControl.NothingSetupMessageLabelText = Res.GetString("63CED5D5-8A24-41E2-AC8E-CF637467EABD", "To make use of this tab, please setup custom fields in Workflow Manager");
			this.ProcessTemplateCustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(746, 135, true);
			this.ProcessTemplateCustomFieldsControl.TabIndex = 1;
			// 
			// AirCargoCustomFieldsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ProcessTemplateCustomFieldsControl);
			this.Name = "CustomFieldsWrapperControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(746, 135, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ProcessTemplateCustomFieldsControl.ResumeLayout(true);
			this.ProcessTemplateCustomFieldsControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.GUI.ProcessTemplateCustomFieldsControl ProcessTemplateCustomFieldsControl;

		#endregion
	}
}
