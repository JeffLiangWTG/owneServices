using System.Windows.Forms;
namespace Enterprise.TransportConsignment.GUI
{
	partial class SignatureForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		private new void InitializeComponent()
		{
			this.SignaturePictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SignaturePictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 112, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 0, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.Visible = false;
			// 
			// SignaturePictureBox
			// 
			this.SignaturePictureBox.Cursor = System.Windows.Forms.Cursors.Default;
			this.SignaturePictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SignaturePictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SignaturePictureBox.Name = "SignaturePictureBox";
			this.SignaturePictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 112, true);
			this.SignaturePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.SignaturePictureBox.TabIndex = 0;
			this.SignaturePictureBox.TabStop = false;
			this.SignaturePictureBox.Click += new System.EventHandler(this.SignaturePictureBox_Click);
			// 
			// SignatureForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.TransportConsignment.GUI.Res.GetData("SignatureForm|Caption", "Signature");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 112, true);
			this.Controls.Add(this.SignaturePictureBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "SignatureForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.SignaturePictureBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SignaturePictureBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZPictureBox SignaturePictureBox;
	}
}