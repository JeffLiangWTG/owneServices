using CargoWise.Windows.UI;

namespace Enterprise.MasterData.GUI
{
	partial class UXMLMatchedListUserControl
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
			this.controlTableLayout = new CargoWise.Windows.UI.KTableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// controlTableLayout
			// 
			this.controlTableLayout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.controlTableLayout.ColumnCount = 1;
			this.controlTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.controlTableLayout.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.controlTableLayout.Name = "controlTableLayout";
			this.controlTableLayout.RowCount = 1;
			this.controlTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.controlTableLayout.TabIndex = 0;
			this.controlTableLayout.AutoSize = true;
			// 
			// UXMLMatchedListUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.BackColor = System.Drawing.Color.White;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.controlTableLayout);
			this.Name = "UXMLMatchedListUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 267, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private KTableLayoutPanel controlTableLayout;
	}
}
