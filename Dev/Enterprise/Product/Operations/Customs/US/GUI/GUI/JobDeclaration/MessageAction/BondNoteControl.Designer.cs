namespace Enterprise.Customs.US.GUI
{
	partial class BondNoteControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BondNoteControl));
			this.eBondStatementTextBox = new CargoWise.Windows.UI.KTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// eBondStatementTextBox
			// 
			this.eBondStatementTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(51, 0, true);
			this.eBondStatementTextBox.Multiline = true;
			this.eBondStatementTextBox.Name = "eBondStatementTextBox";
			this.eBondStatementTextBox.ReadOnly = true;
			this.eBondStatementTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.eBondStatementTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(535, 48, true);
			this.eBondStatementTextBox.TabIndex = 0;
			this.eBondStatementTextBox.Text = resources.GetString("eBondStatementTextBox.Text");
			// 
			// BondNoteControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.eBondStatementTextBox);
			this.Name = "BondNoteControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 50, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal CargoWise.Windows.UI.KTextBox eBondStatementTextBox;
	}
}
