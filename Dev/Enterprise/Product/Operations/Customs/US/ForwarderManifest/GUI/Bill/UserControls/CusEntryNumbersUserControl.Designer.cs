using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	partial class CusEntryNumbersUserControl
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
			this.MoreNumbersButton = new ZButton();
			this.MoreNumbersTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MoreNumbersButton.SuspendLayout();
			this.MoreNumbersTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MoreNumbersButton
			// 
			this.MoreNumbersButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(262, 0, true);
			this.MoreNumbersButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 21, true);
			this.MoreNumbersButton.TabIndex = 1;
			
			this.MoreNumbersButton.Text = "...";
			this.MoreNumbersButton.Click += new EventHandler(this.MoreNumbersButton_Click);
			// 
			// MoreNumbersTextBox
			// 
			this.MoreNumbersTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.MoreNumbersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MoreNumbersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.MoreNumbersTextBox.TabIndex = 0;
			//
			// CusEntryNumbersUserControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MoreNumbersButton);
			this.Controls.Add(this.MoreNumbersTextBox);
			this.Name = "CusEntryNumbersUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MoreNumbersButton.ResumeLayout(true);
			this.MoreNumbersButton.PerformLayout();
			this.MoreNumbersTextBox.ResumeLayout(true);
			this.MoreNumbersTextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZArchitecture.ZTextBox MoreNumbersTextBox;
		internal ZButton MoreNumbersButton;
	}
}
