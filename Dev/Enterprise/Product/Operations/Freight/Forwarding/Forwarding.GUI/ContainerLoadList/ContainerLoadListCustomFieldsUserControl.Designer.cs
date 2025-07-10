
namespace Enterprise.Freight.Forwarding.GUI
{
	partial class ContainerLoadListCustomFieldsUserControl
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
			this.CustomFieldsUserControl = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CustomFieldsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// CustomFieldsUserControl
			// 
			this.CustomFieldsUserControl.AllowDrop = true;
			this.CustomFieldsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 22, true);
			this.CustomFieldsUserControl.Name = "CustomFieldsUserControl";
			this.CustomFieldsUserControl.NothingSetupMessageLabelText = Enterprise.Freight.Forwarding.GUI.Res.GetString("15044052-AA6D-4D0D-B5F6-E943734C52A1", "To make use of custom fields, please set up Container Load List custom fields in Workflow Manager.");
			this.CustomFieldsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 120, true);
			this.CustomFieldsUserControl.TabIndex = 1;
			// 
			// ContainerLoadListCustomFieldsUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.CustomFieldsUserControl);
			this.Name = "ContainerLoadListCustomFieldsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 130, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CustomFieldsUserControl.ResumeLayout(true);
			this.CustomFieldsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ProcessTemplateCustomFieldsControl CustomFieldsUserControl;
	}
}
