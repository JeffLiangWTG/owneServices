namespace Enterprise.Freight.LocalCartage.GUI
{
	partial class SignatureForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		public new void InitializeComponent()
		{
			this.signaturePictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.signaturePictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 112, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 0, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.Visible = false;
			// 
			// signaturePictureBox
			// 
			this.signaturePictureBox.Cursor = System.Windows.Forms.Cursors.Default;
			this.signaturePictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.signaturePictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.signaturePictureBox.Name = "signaturePictureBox";
			this.signaturePictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 112, true);
			this.signaturePictureBox.TabIndex = 0;
			this.signaturePictureBox.TabStop = false;
			// 
			// SignatureForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 112, true);
			this.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("SignatureForm|f5972ac9-a68e-4040-81df-ce8a3c8a754a", "Signature");
			this.Controls.Add(this.signaturePictureBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "SignatureForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.signaturePictureBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.signaturePictureBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPictureBox signaturePictureBox;
	}
}
