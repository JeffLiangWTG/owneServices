namespace Enterprise.TransportConsignment.GUI
{
	partial class DtbBookingConsignmentCustomFieldsControl
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
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.processTemplateCustomFieldsControl1 = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.processTemplateCustomFieldsControl1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.TransportConsignment.Business.DtbBookingConsignment);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.TransportConsignment.GUI.Res.GetData("DtbConsignmentCustomFieldsControl|6b119722-85c7-42ef-8e61-e3d2d135dcef", "Workflow Custom Fields");
			this.zGroupBox1.Controls.Add(this.processTemplateCustomFieldsControl1);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(726, 382, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// processTemplateCustomFieldsControl1
			// 
			this.processTemplateCustomFieldsControl1.AllowDrop = true;
			this.processTemplateCustomFieldsControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.processTemplateCustomFieldsControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.processTemplateCustomFieldsControl1.Name = "processTemplateCustomFieldsControl1";
			this.processTemplateCustomFieldsControl1.NothingSetupMessageLabelText = "To make use of this tab, please setup custom fields in Workflow Manager.";
			this.processTemplateCustomFieldsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 363, true);
			this.processTemplateCustomFieldsControl1.TabIndex = 0;
			// 
			// DtbConsignmentCustomFieldsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGroupBox1);
			this.Name = "DtbConsignmentCustomFieldsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(726, 382, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.processTemplateCustomFieldsControl1.ResumeLayout(true);
			this.processTemplateCustomFieldsControl1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		protected ZArchitecture.GUI.ProcessTemplateCustomFieldsControl processTemplateCustomFieldsControl1;
	}
}
