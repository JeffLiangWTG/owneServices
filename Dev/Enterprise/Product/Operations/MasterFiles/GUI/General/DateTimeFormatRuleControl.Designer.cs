namespace Enterprise.MasterFiles.GUI.General
{
	partial class DateTimeFormatRuleControl
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
			this.formatDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// formatDropEdit
			// 
			this.formatDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 12, true);
			this.formatDropEdit.Name = "formatDropEdit";
			this.formatDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.formatDropEdit.TabIndex = 0;
			// 
			// DateTimeFormatRuleControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.formatDropEdit);
			this.Name = "DateTimeFormatRuleControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 47, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDropEdit formatDropEdit;
	}
}
