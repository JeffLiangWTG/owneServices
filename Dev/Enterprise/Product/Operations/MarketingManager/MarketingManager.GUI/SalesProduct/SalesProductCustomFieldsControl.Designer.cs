namespace Enterprise.MarketingManager.GUI
{
	partial class SalesProductCustomFieldsControl
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
			this.salesProductCustomFieldsInnerControl = new Enterprise.MarketingManager.GUI.SalesProductCustomFieldsInnerControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.salesProductCustomFieldsInnerControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CargoWise.EntityFramework.IDynamicBusinessObject);
			// 
			// salesProductCustomFieldsInnerControl
			// 
			this.salesProductCustomFieldsInnerControl.AllowDrop = true;
			this.salesProductCustomFieldsInnerControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.salesProductCustomFieldsInnerControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.salesProductCustomFieldsInnerControl.Name = "salesProductCustomFieldsInnerControl";
			this.salesProductCustomFieldsInnerControl.NothingSetupMessageLabelText = "";
			this.salesProductCustomFieldsInnerControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 100, true);
			this.salesProductCustomFieldsInnerControl.TabIndex = 0;
			// 
			// SalesProductCustomFieldsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.salesProductCustomFieldsInnerControl);
			this.Name = "SalesProductCustomFieldsControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.salesProductCustomFieldsInnerControl.ResumeLayout(true);
			this.salesProductCustomFieldsInnerControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private SalesProductCustomFieldsInnerControl salesProductCustomFieldsInnerControl;
	}
}
