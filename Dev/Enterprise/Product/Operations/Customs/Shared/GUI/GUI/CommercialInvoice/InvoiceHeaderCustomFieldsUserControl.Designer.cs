using Enterprise.Freight.Forwarding.GUI;
namespace Enterprise.Customs.GUI
{
	partial class InvoiceHeaderCustomFieldsUserControl
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
			components = new System.ComponentModel.Container();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.InvCustomFieldsDisplayControl = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
			this.InvCustomFieldsDisplayControl.SuspendLayout();
			// 
			// InvCustomFieldsDisplayControl
			//
			this.CaptionRenderingEnabled = true;
			this.InvCustomFieldsDisplayControl.AllowDrop = true;
			this.InvCustomFieldsDisplayControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvCustomFieldsDisplayControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.InvCustomFieldsDisplayControl.Name = "InvCustomFieldsDisplayControl";
			this.InvCustomFieldsDisplayControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 341, true);
			this.InvCustomFieldsDisplayControl.TabIndex = 0;
			this.Controls.Add(this.InvCustomFieldsDisplayControl);
			this.InvCustomFieldsDisplayControl.ResumeLayout(true);
			this.InvCustomFieldsDisplayControl.PerformLayout();
		}

		#endregion

		public Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl InvCustomFieldsDisplayControl;
	}
}
